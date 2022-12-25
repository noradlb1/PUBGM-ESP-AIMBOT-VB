Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Linq
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Windows.Forms
Imports static PUBGMESP.SigScanSharp

Namespace PUBGMESP


	Public Class SigScanSharp
		Public Class BypaPH
			Implements IDisposable

			<DllImport("BypaPH.dll", EntryPoint := "CreateInstance")> _
			Shared Function BypaPH_ctor(ByVal pID As UInteger) As IntPtr
			End Function
			<DllImport("BypaPH.dll", EntryPoint := "DeleteInstance")> _
			Shared Sub BypaPH_dtor(ByVal pInstance As IntPtr)
			End Sub
'INSTANT VB TODO TASK: Assignments within expressions are not supported in VB:
'ORIGINAL LINE: static extern bool BypaPH_RWVM(IntPtr pInstance, UIntPtr BaseAddress, [Out] byte[] Buffer, UIntPtr BufferSize, out ulong NumberOfBytesReadOrWritten, bool read = True, bool unsafeRequest = False);
			<DllImport("BypaPH.dll", EntryPoint := "RWVM")> _
			Shared Function BypaPH_RWVM(ByVal pInstance As IntPtr, ByVal BaseAddress As UIntPtr, <Out()> ByVal Buffer() As Byte, ByVal BufferSize As UIntPtr, ByRef NumberOfBytesReadOrWritten As ULong, Optional ByVal read As Boolean = True, Optional ByVal unsafeRequest As Boolean = False) As Boolean
			End Function
			Private pInstance As IntPtr
			Public Sub New(ByVal pID As Integer)
				pInstance = BypaPH_ctor(CUInt(pID))
			End Sub
			Public Function ReadProcessMem(ByVal BaseAddress As ULong, <System.Runtime.InteropServices.Out()> ByRef Buffer() As Byte, ByVal BufferSize As Integer, <System.Runtime.InteropServices.Out()> ByRef NumberOfBytesRead As ULong) As Boolean
				Dim buf(BufferSize - 1) As Byte
				Dim b As Boolean = BypaPH_RWVM(pInstance, New UIntPtr(BaseAddress), buf, New UIntPtr(CUInt(buf.Length)), NumberOfBytesRead)

				Buffer = buf
				Return b
			End Function
			Public Function WriteProcessMem(ByVal BaseAddress As ULong, ByVal Buffer() As Byte, <System.Runtime.InteropServices.Out()> ByRef NumberOfBytesWittin As ULong) As Boolean
				Return BypaPH_RWVM(pInstance, New UIntPtr(BaseAddress), Buffer, New UIntPtr(CUInt(Buffer.Length)), NumberOfBytesWittin, False)
			End Function
			Public Sub Dispose() Implements IDisposable.Dispose
				BypaPH_dtor(pInstance)
			End Sub
		End Class
		Private Property g_hProcess() As IntPtr
		Private Property g_arrModuleBuffer() As Byte()
		Private Property g_lpModuleBase() As Long

		Private MBI As Win32.MEMORY_BASIC_INFORMATION

		Private Shared question As Byte = AscW("?"c)

		Private ReadOnly Property g_dictStringPatterns() As Dictionary(Of String, String)

		Public Sub New(ByVal hProc As IntPtr)
			g_hProcess = hProc
			g_dictStringPatterns = New Dictionary(Of String, String)()
		End Sub

		Public Function SelectModule(ByVal targetModule As ProcessModule) As Boolean
			g_lpModuleBase = CLng(Math.Truncate(targetModule.BaseAddress))
			g_arrModuleBuffer = New Byte(targetModule.ModuleMemorySize - 1){}

			g_dictStringPatterns.Clear()

			Return Win32.ReadProcessMemory(g_hProcess, g_lpModuleBase, g_arrModuleBuffer, targetModule.ModuleMemorySize)
		End Function

		Public Sub AddPattern(ByVal szPatternName As String, ByVal szPattern As String)
			g_dictStringPatterns.Add(szPatternName, szPattern)
		End Sub

		Private Function PatternCheck(ByVal nOffset As Integer, ByVal arrPattern() As Byte) As Boolean
			For i As Integer = 0 To arrPattern.Length - 1
				If arrPattern(i) = question Then
					Continue For
				End If

				If arrPattern(i) <> Me.g_arrModuleBuffer(nOffset + i) Then
					Return False
				End If
			Next i

			Return True
		End Function

		Private Function PatternCheck(ByVal nOffset As Integer, ByVal arrPattern() As Byte, ByVal source() As Byte) As Boolean
			If nOffset + arrPattern.Length > source.Length Then
				Return False
			End If
			For i As Integer = 0 To arrPattern.Length - 1
				If arrPattern(i) = question Then
					Continue For
				End If

				If arrPattern(i) <> source(nOffset + i) Then
					Return False
				End If
			Next i

			Return True
		End Function

		Public Function FindPattern(ByVal szPattern As String, <System.Runtime.InteropServices.Out()> ByRef lTime As Long) As Long
			If g_arrModuleBuffer Is Nothing OrElse g_lpModuleBase = 0 Then
				Throw New Exception("Selected module is null")
			End If

			Dim stopwatch As Stopwatch = System.Diagnostics.Stopwatch.StartNew()

			Dim arrPattern() As Byte = ParsePatternString(szPattern)

			For nModuleIndex As Integer = 0 To g_arrModuleBuffer.Length - 1
				If Me.g_arrModuleBuffer(nModuleIndex) <> arrPattern(0) Then
					Continue For
				End If

				If PatternCheck(nModuleIndex, arrPattern) Then
					lTime = stopwatch.ElapsedMilliseconds
					Return g_lpModuleBase + CLng(nModuleIndex)
				End If
			Next nModuleIndex

			lTime = stopwatch.ElapsedMilliseconds
			Return 0
		End Function

		Public Function FindPatternsAllRegion(ByVal szPattern As String, Optional ByVal iStartAddress As Long = 0, Optional ByVal iEndAddress As Long = &H7FFFFFFF) As Long()
			Dim arrPattern() As Byte = ParsePatternString(szPattern)
			Dim iAddress As Long = iStartAddress
			Dim ptrBytesRead As Integer = 0
			Dim bBuffer() As Byte
			Dim matchAddvs As New List(Of Long)()
			Do
				Dim iRead As Integer = Win32.VirtualQueryEx(g_hProcess, New IntPtr(iAddress), MBI, CUInt(Marshal.SizeOf(MBI)))
				If (iRead > 0) AndAlso (CLng(Math.Truncate(MBI.RegionSize)) > 0) Then
					'bBuffer = new byte[(long)MBI.RegionSize];
					bBuffer = Mem.ReadMemory(CLng(Math.Truncate(MBI.BaseAddress)), CInt(Math.Truncate(MBI.RegionSize)))
					'Win32.ReadProcessMemory(g_hProcess, (long)MBI.BaseAddress, bBuffer, bBuffer.Length, ptrBytesRead);
					For i As Integer = 0 To bBuffer.Length - 1
						If bBuffer(i) <> arrPattern(0) Then
							Continue For
						End If
						If PatternCheck(i, arrPattern, bBuffer) Then
							matchAddvs.Add(CLng(iAddress + i))
							i += arrPattern.Length
						End If
					Next i
				End If
				iAddress = CLng(MBI.BaseAddress.ToInt64() + MBI.RegionSize.ToInt64())
			Loop While iAddress <= iEndAddress
			Return matchAddvs.ToArray()
		End Function

		Public Function FindPatterns(<System.Runtime.InteropServices.Out()> ByRef lTime As Long) As Dictionary(Of String, Long)
			If g_arrModuleBuffer Is Nothing OrElse g_lpModuleBase = 0 Then
				Throw New Exception("Selected module is null")
			End If

			Dim stopwatch As Stopwatch = System.Diagnostics.Stopwatch.StartNew()

			Dim arrBytePatterns(g_dictStringPatterns.Count - 1)() As Byte
			Dim arrResult(g_dictStringPatterns.Count - 1) As Long

			' PARSE PATTERNS
			For nIndex As Integer = 0 To g_dictStringPatterns.Count - 1
				arrBytePatterns(nIndex) = ParsePatternString(g_dictStringPatterns.ElementAt(nIndex).Value)
			Next nIndex

			' SCAN FOR PATTERNS
			For nModuleIndex As Integer = 0 To g_arrModuleBuffer.Length - 1
				For nPatternIndex As Integer = 0 To arrBytePatterns.Length - 1
					If arrResult(nPatternIndex) <> 0 Then
						Continue For
					End If

					If Me.PatternCheck(nModuleIndex, arrBytePatterns(nPatternIndex)) Then
						arrResult(nPatternIndex) = g_lpModuleBase + CLng(nModuleIndex)
					End If
				Next nPatternIndex
			Next nModuleIndex

			Dim dictResultFormatted As New Dictionary(Of String, Long)()

			' FORMAT PATTERNS
			For nPatternIndex As Integer = 0 To arrBytePatterns.Length - 1
				dictResultFormatted(g_dictStringPatterns.ElementAt(nPatternIndex).Key) = arrResult(nPatternIndex)
			Next nPatternIndex

			lTime = stopwatch.ElapsedMilliseconds
			Return dictResultFormatted
		End Function

		Private Function ParsePatternString(ByVal szPattern As String) As Byte()
			Dim patternbytes As New List(Of Byte)()

			For Each szByte In szPattern.Split(" "c)
				patternbytes.Add(szByte Is "?If(", AscW("?"c), Convert.ToByte(szByte, 16)))
			Next szByte

			Return patternbytes.ToArray()
		End Function

		Private NotInheritable Class Win32

			Private Sub New()
			End Sub
'INSTANT VB TODO TASK: Assignments within expressions are not supported in VB:
'ORIGINAL LINE: public static extern bool ReadProcessMemory(IntPtr hProcess, long lpBaseAddress, byte[] lpBuffer, int dwSize, int lpNumberOfBytesRead = 0);
			<DllImport("kernel32.dll")> _
			Public Shared Function ReadProcessMemory(ByVal hProcess As IntPtr, ByVal lpBaseAddress As Long, ByVal lpBuffer() As Byte, ByVal dwSize As Integer, Optional ByVal lpNumberOfBytesRead As Integer = 0) As Boolean
			End Function

			<DllImport("kernel32.dll")> _
			Friend Shared Function VirtualQueryEx(ByVal hProcess As IntPtr, ByVal lpAddress As IntPtr, ByRef lpBuffer As Win32.MEMORY_BASIC_INFORMATION, ByVal dwLength As UInteger) As Int32
			End Function

			<StructLayout(LayoutKind.Sequential)> _
			Public Structure MEMORY_BASIC_INFORMATION
				Public BaseAddress As IntPtr
				Public AllocationBase As IntPtr
				Public AllocationProtect As UInteger
				Public RegionSize As IntPtr
				Public State As UInteger
				Public Protect As UInteger
				Public Type As UInteger
			End Structure
		End Class
	End Class

	Public Class Mem
		Public Shared m_iNumberOfBytesRead As Integer = 0
		Public Shared m_iNumberOfBytesWritten As Integer = 0
		Public Shared m_Process As Process
		Public Shared m_pProcessHandle As IntPtr = IntPtr.Zero
		Public Shared BaseAddress As Int64
		Private Const PROCESS_VM_OPERATION As Integer = 8
		Private Const PROCESS_VM_READ As Integer = 16
		Private Const PROCESS_VM_WRITE As Integer = 32

		Public Shared Sub Initialize(ByVal ProcessName As String)
			If CUInt(Process.GetProcessesByName(ProcessName).Length) > 0UI Then
				Mem.m_Process = Process.GetProcessesByName(ProcessName)(0)
				Mem.BaseAddress = Process.GetProcessesByName(ProcessName)(0).MainModule.BaseAddress.ToInt64()
			Else
				Dim num As Integer = CInt(MessageBox.Show("Emulator should start first!!!"))
				Environment.Exit(1)
			End If
			Mem.m_pProcessHandle = Mem.OpenProcess(&H1F0FFF, False, Mem.m_Process.Id)
		End Sub
		Public Shared ByPH As BypaPH
		Public Shared Sub Initialize(ByVal ptr As IntPtr)
			If ptr <> IntPtr.Zero Then
				ByPH = New BypaPH(CInt(Math.Truncate(ptr)))
				Mem.m_pProcessHandle = Mem.OpenProcess(&H1F0FFF, False, CInt(Math.Truncate(ptr)))
			End If
		End Sub

		Public Shared Function GetModuleAdress(ByVal ModuleName As String) As Int64
			Dim num As Int64
			Try
				For Each [module] As ProcessModule In CType(Mem.m_Process.Modules, ReadOnlyCollectionBase)
					If Not ModuleName.Contains(".dll") Then
						ModuleName = ModuleName.Insert(ModuleName.Length, ".dll")
					End If
					If ModuleName = [module].ModuleName Then
						num = CLng(Math.Truncate([module].BaseAddress))
						GoTo label_13
					End If
				Next [module]
			Catch
			End Try
			num = -1
		label_13:
			Return num
		End Function

		Public Shared Function ReadString(ByVal address As Int64, ByVal _Size As Integer) As String
			Return Encoding.Default.GetString(ReadMemory(address, _Size))
		End Function


		Public Shared Function ReadMemory(Of T As Structure)(ByVal Adress As Int64) As T
			Dim num As ULong
			Dim numArray(Marshal.SizeOf(GetType(T)) - 1) As Byte
			ByPH.ReadProcessMem(CULng(Adress), numArray, numArray.Length, num)
			'Mem.ReadProcessMemory((int)Mem.m_pProcessHandle, Adress, numArray, numArray.Length, ref Mem.m_iNumberOfBytesRead);
			Return Mem.ByteArrayToStructure(Of T)(numArray)
		End Function

		Public Shared Function ReadMemory(ByVal address As Int64, ByVal _Size As Integer) As Byte()
			Dim num As ULong
			Dim numArray(_Size - 1) As Byte
			ByPH.ReadProcessMem(CULng(address), numArray, numArray.Length, num)
			'Mem.ReadProcessMemory((int)Mem.m_pProcessHandle, address, numArray, _Size, ref Mem.m_iNumberOfBytesRead);
			Return numArray
		End Function

		Public Shared Function ReadMatrix(Of T As Structure)(ByVal Adress As Int64, ByVal MatrixSize As Integer) As Single()
			Dim numArray((Marshal.SizeOf(GetType(T)) * MatrixSize) - 1) As Byte
			Dim num As ULong
			ByPH.ReadProcessMem(CULng(Adress), numArray, numArray.Length, num)
			Return Mem.ConvertToFloatArray(numArray)
		End Function

		Public Shared Sub WriteMemory(Of T As Structure)(ByVal Adress As Int64, ByVal Value As Object)
			Dim byteArray() As Byte = Mem.StructureToByteArray(Value)
			Dim num As ULong
			ByPH.WriteProcessMem(CULng(Adress), byteArray, num)
			'Mem.WriteProcessMemory((int)Mem.m_pProcessHandle, Adress, byteArray, byteArray.Length, out Mem.m_iNumberOfBytesWritten);
		End Sub

		Public Shared Sub WriteMemory(ByVal Adress As Int64, ByVal byteArray() As Byte)
			Dim num As ULong
			ByPH.WriteProcessMem(CULng(Adress), byteArray, num)
		End Sub

		Public Shared Function PatternToBytes(ByVal pattern As String, Optional ByVal offset As Integer = 0) As Byte()
			Dim patternArr = pattern.Split(" "c)
			Dim result As New List(Of Byte)()
			For i As Integer = offset To patternArr.Length - 1
				If patternArr(i) = "?" Then
					Continue For
				End If
				result.Add(Convert.ToByte(patternArr(i), 16))
			Next i
			Return result.ToArray()
		End Function

		Public Shared Function ConvertToFloatArray(ByVal bytes() As Byte) As Single()
			If CUInt(bytes.Length Mod 4) > 0UI Then
				Throw New ArgumentException()
			End If
			Dim numArray((bytes.Length \ 4) - 1) As Single
			For index As Integer = 0 To numArray.Length - 1
				numArray(index) = BitConverter.ToSingle(bytes, index * 4)
			Next index
			Return numArray
		End Function

		Private Shared Function ByteArrayToStructure(Of T As Structure)(ByVal bytes() As Byte) As T
			Dim gcHandle As GCHandle = GCHandle.Alloc(DirectCast(bytes, Object), GCHandleType.Pinned)
			Try
				Return DirectCast(Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), GetType(T)), T)
			Finally
				gcHandle.Free()
			End Try
		End Function

		Public Shared Function AllocateMemory(ByVal size As UInteger) As IntPtr
			Return VirtualAllocEx(Mem.m_pProcessHandle, IntPtr.Zero, size, AllocationType.Commit, MemoryProtection.ReadWrite)
		End Function

		Private Shared Function StructureToByteArray(ByVal obj As Object) As Byte()
			Dim length As Integer = Marshal.SizeOf(obj)
			Dim destination(length - 1) As Byte
			Dim num As IntPtr = Marshal.AllocHGlobal(length)
			Marshal.StructureToPtr(obj, num, True)
			Marshal.Copy(num, destination, 0, length)
			Marshal.FreeHGlobal(num)
			Return destination
		End Function

		<DllImport("kernel32.dll")> _
		Private Shared Function OpenProcess(ByVal dwDesiredAccess As Integer, ByVal bInheritHandle As Boolean, ByVal dwProcessId As Integer) As IntPtr
		End Function


		<DllImport("kernel32.dll", SetLastError := True, ExactSpelling := True)> _
		Shared Function VirtualAllocEx(ByVal hProcess As IntPtr, ByVal lpAddress As IntPtr, ByVal dwSize As UInteger, ByVal flAllocationType As AllocationType, ByVal flProtect As MemoryProtection) As IntPtr
		End Function

		<Flags> _
		Public Enum AllocationType
			Commit = &H1000
			Reserve = &H2000
			Decommit = &H4000
			Release = &H8000
			Reset = &H80000
			Physical = &H400000
			TopDown = &H100000
			WriteWatch = &H200000
			LargePages = &H20000000
		End Enum

		<Flags> _
		Public Enum MemoryProtection
			Execute = &H10
			ExecuteRead = &H20
			ExecuteReadWrite = &H40
			ExecuteWriteCopy = &H80
			NoAccess = &H1
			[ReadOnly] = &H2
			ReadWrite = &H4
			WriteCopy = &H8
			GuardModifierflag = &H100
			NoCacheModifierflag = &H200
			WriteCombineModifierflag = &H400
		End Enum
	End Class

	Public Class GameMemSearch
		Private sgn As SigScanSharp
		Public Sub New(ByVal sgn As SigScanSharp)
			Me.sgn = sgn
		End Sub

		Public Function ViewWorldSearchCandidates(Optional ByVal startAddv As Long = &H26000000, Optional ByVal endAddv As Long = &H30000000) As Long()
			Dim tmpViewWorlds() As Long = sgn.FindPatternsAllRegion(Patterns.viewWorldSearch, startAddv, endAddv)
			Dim viewWorlds(tmpViewWorlds.Length - 1) As Long
			For i As Integer = 0 To viewWorlds.Length - 1
				viewWorlds(i) = tmpViewWorlds(i) - 32
			Next i
			Return viewWorlds
		End Function

		Public Function GetViewWorld(ByVal cands() As Long) As Long
			Dim tmp As Long
			Dim t1, t2, t3, t4 As Single
			For i As Integer = 0 To cands.Length - 1
				tmp = Mem.ReadMemory(Of Integer)(Mem.ReadMemory(Of Integer)(cands(i)) + 32) + 512
				t1 = Mem.ReadMemory(Of Single)(tmp + 56)
				t2 = Mem.ReadMemory(Of Single)(tmp + 40)
				t3 = Mem.ReadMemory(Of Single)(tmp + 24)
				t4 = Mem.ReadMemory(Of Single)(tmp + 8)
				If t1 >= 3 AndAlso t2 = 0 AndAlso t3 = 0 AndAlso t4 = 0 Then
					Return cands(i)
				End If
			Next i
			Return -1
		End Function
	End Class

	Public NotInheritable Class GameData

		Private Sub New()
		End Sub

		''' <summary>
		''' Tell if a entity is player
		''' </summary>
		''' <param name="str"></param>
		''' <returns></returns>
		Public Shared Function IsPlayer(ByVal str As String) As Boolean
			If str.Contains("BP_PlayerPawn") Then
				Return True
			End If
			Return False
		End Function


		''' <summary>
		''' Get Box Item From Item Code
		''' </summary>
		''' <param name="code"></param>
		''' <returns></returns>
		Public Shared Function GetBoxItemType(ByVal code As Long) As Item
			If code = 601001 Then
				Return Item.EnegyDrink
			End If
			If code = 601002 Then
				Return Item.Epinephrine
			End If
			If code = 601003 Then
				Return Item.PainKiller
			End If
			If code = 601005 Then
				Return Item.AidKit
			End If
			If code = 501006 Then
				Return Item.BagLv3
			End If
			If code = 503003 Then
				Return Item.ArmorLv3
			End If
			If code = 502003 Then
				Return Item.HelmetLv3
			End If
			If code = 103003 Then
				Return Item.AWM
			End If
			If code = 101003 Then
				Return Item.SCARL
			End If
			If code = 103001 Then
				Return Item.Kar98
			End If
			If code = 101008 Then
				Return Item.M762
			End If
			If code = 105002 Then
				Return Item.DP28
			End If
			If code = 101005 Then
				Return Item.Groza
			End If
			If code = 101001 Then
				Return Item.AKM
			End If
			If code = 101006 Then
				Return Item.AUG
			End If
			If code = 101007 Then
				Return Item.QBZ
			End If
			If code = 105001 Then
				Return Item.M249
			End If
			If code = 101004 Then
				Return Item.M4A1
			End If
			If code = 306001 Then
				Return Item.AmmoMagnum
			End If
			If code = 302001 Then
				Return Item.Ammo762
			End If
			If code = 303001 Then
				Return Item.Ammo556
			End If
			If code = 203004 Then
				Return Item.Scope4x
			End If
			If code = 203015 Then
				Return Item.Scope6x
			End If
			If code = 203005 Then
				Return Item.Scope8x
			End If
			If code = 201011 Then
				Return Item.RifleSilenter
			End If
			If code = 204013 Then
				Return Item.RifleMagazine
			End If
			If code = 403990 OrElse code = 403187 Then
				Return Item.GhillieSuit
			End If
			Return Item.Useless
		End Function


		''' <summary>
		''' Tell if an item is box
		''' </summary>
		''' <param name="str"></param>
		''' <returns></returns>
		Public Shared Function IsBox(ByVal str As String) As Boolean
			If str.Contains("PlayerDeadInventoryBox") OrElse str.Contains("PickUpListWrapperActor") OrElse str.Contains("AirDrop") Then
				Return True
			End If
			Return False
		End Function

		''' <summary>
		''' Get Grenade Type
		''' </summary>
		''' <param name="str"></param>
		''' <returns></returns>
		Public Shared Function GetGrenadeType(ByVal str As String) As Grenade
			If str.Contains("BP_Grenade_Smoke_C") Then
				Return Grenade.Smoke
			End If
			If str.Contains("BP_Grenade_Burn_C") Then
				Return Grenade.Burn
			End If
			If str.Contains("BP_Grenade_tun_C") Then
				Return Grenade.Flash
			End If
			If str.Contains("BP_Grenade_Shoulei_C") Then
				Return Grenade.Explode
			End If
			Return Grenade.Unknown
		End Function

		''' <summary>
		''' Get Vehicle Type
		''' </summary>
		''' <param name="str"></param>
		''' <returns></returns>
		Public Shared Function GetVehicleType(ByVal str As String) As Vehicle
			If str.Contains("BRDM") Then
				Return Vehicle.BRDM
			End If
			If str.Contains("Scooter") Then
				Return Vehicle.Scooter
			End If
			If str.Contains("Motorcycle") Then
				Return Vehicle.Motorcycle
			End If
			If str.Contains("MotorcycleCart") Then
				Return Vehicle.MotorcycleCart
			End If
			If str.Contains("Snowmobile") Then
				Return Vehicle.Snowmobile
			End If
			If str.Contains("Tuk") Then
				Return Vehicle.Tuk
			End If
			If str.Contains("Buggy") Then
				Return Vehicle.Buggy
			End If
			If str.Contains("open") Then
				Return Vehicle.Sports
			End If
			If str.Contains("close") Then
				Return Vehicle.Sports
			End If
			If str.Contains("Dacia") Then
				Return Vehicle.Dacia
			End If
			If str.Contains("Rony") Then
				Return Vehicle.Rony
			End If
			If str.Contains("UAZ") Then
				Return Vehicle.UAZ
			End If
			If str.Contains("MiniBus") Then
				Return Vehicle.MiniBus
			End If
			If str.Contains("PG117") Then
				Return Vehicle.PG117
			End If
			If str.Contains("AquaRail") Then
				Return Vehicle.AquaRail
			End If
			If str.Contains("BP_AirDropPlane_C") Then
				Return Vehicle.BP_AirDropPlane_C
			End If
			'if (str.Contains("PickUp"))
			'{
			'    if (str.Contains("PickUp_BP"))
			'    {
			'        if (str != "PickUpListWrapperActor")
			'            return Vehicle.PickUp;
			'    }
			'}
			Return Vehicle.Unknown
		End Function

		''' <summary>
		''' Get Item's Type
		''' </summary>
		''' <param name="str"></param>
		''' <returns></returns>
		Public Shared Function GetItemType(ByVal str As String) As Item
			'if (!str.Contains("Pickup") || !str.Contains("PickUp"))
			'    return Item.Useless;
			If str.Contains("Grenade_Shoulei_Weapon_Wra") Then
				Return Item.Grenade
			End If
			If str.Contains("MZJ_4X") Then
				Return Item.Scope4x
			End If
			If str.Contains("MZJ_6X") Then
				Return Item.Scope6x
			End If
			If str.Contains("MZJ_8X") Then
				Return Item.Scope8x
			End If
			If str.Contains("DJ_Large_EQ") Then
				Return Item.RifleMagazine
			End If
			If str.Contains("QK_Sniper_Suppressor") Then
				Return Item.SniperSilenter
			End If
			If str.Contains("QK_Large_Suppressor") Then
				Return Item.RifleSilenter
			End If
			'if (str.Contains("Ammo_556mm"))
			'    return Item.Ammo556;
			'if (str.Contains("Ammo_762mm"))
			'    return Item.Ammo762;
			'if (str.Contains("Ammo_300Magnum"))
			'    return Item.AmmoMagnum;
			If str.Contains("Helmet_Lv3") Then
				Return Item.HelmetLv3
			End If
			If str.Contains("Armor_Lv3") Then
				Return Item.ArmorLv3
			End If
			If str.Contains("Bag_Lv3") Then
				Return Item.BagLv3
			End If
			If str.Contains("Helmet_Lv2") Then
				Return Item.HelmetLv2
			End If
			If str.Contains("Armor_Lv2") Then
				Return Item.ArmorLv2
			End If
			If str.Contains("Bag_Lv2") Then
				Return Item.BagLv2
			End If
			If str.Contains("Firstaid") Then
				Return Item.AidKit
			End If
			If str.Contains("Injection") Then
				Return Item.Epinephrine
			End If
			If str.Contains("Pills") Then
				Return Item.PainKiller
			End If
			If str.Contains("Drink") Then
				Return Item.EnegyDrink
			End If
			If Not str.Contains("Wrapper") Then
				Return Item.Useless
			End If
			If str.Contains("Pistol_Flaregun") Then
				Return Item.FlareGun
			End If
			If str.Contains("AWM") Then
				Return Item.AWM
			End If
			If str.Contains("Kar98k") Then
				Return Item.Kar98
			End If
			If str.Contains("Mk14") Then
				Return Item.MK14
			End If
			If str.Contains("DP28") Then
				Return Item.DP28
			End If
			If str.Contains("SKS") Then
				Return Item.SKS
			End If
			If str.Contains("Groza") Then
				Return Item.Groza
			End If
			If str.Contains("M762") Then
				Return Item.M762
			End If
			If str.Contains("AKM") Then
				Return Item.AKM
			End If
			If str.Contains("M249") Then
				Return Item.M249
			End If
			If str.Contains("M24") Then
				Return Item.M24
			End If
			If str.Contains("AUG") Then
				Return Item.AUG
			End If
			If str.Contains("QBZ") Then
				Return Item.QBZ
			End If
			If str.Contains("M416") Then
				Return Item.M4A1
			End If
			If str.Contains("SCAR") Then
				Return Item.SCARL
			End If

			Return Item.Useless
		End Function

		''' <summary>
		''' Get Entity's Type
		''' </summary>
		''' <param name="gNames"></param>
		''' <param name="id"></param>
		''' <returns></returns>
		Public Shared Function GetEntityType(ByVal gNames As Long, ByVal id As Long) As String
			Dim result As String = ""
			Dim gname As Long = Mem.ReadMemory(Of Integer)(gNames)
			If id > 0 AndAlso id < 2000000 Then
				Dim page As Long = id \ 16384
				Dim index As Long = id Mod 16384
				Dim secPartAddv As Long = Mem.ReadMemory(Of Integer)(gname + page * 4)
				If secPartAddv > 0 Then
					Dim nameAddv As Long = Mem.ReadMemory(Of Integer)(secPartAddv + index * 4)
					If nameAddv > 0 Then
						result = Mem.ReadString(nameAddv + 8, 32)
					End If
				End If
			End If
			Return result
		End Function


	End Class

	Friend Module Utility
		<System.Runtime.CompilerServices.Extension> _
		Public Function GetDescription(ByVal value As System.Enum) As String
			Dim field As FieldInfo = value.GetType().GetField(value.ToString())

			Dim attribute As DescriptionAttribute = TryCast(System.Attribute.GetCustomAttribute(field, GetType(DescriptionAttribute)), DescriptionAttribute)

			Return If(attribute Is Nothing, value.ToString(), attribute.Description)
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Function CheckFeasible(ByVal vec As Vector2) As Boolean
			If vec.X > 1 AndAlso vec.Y > 1 Then
				Return True
			End If
			Return False
		End Function
	End Module
End Namespace

