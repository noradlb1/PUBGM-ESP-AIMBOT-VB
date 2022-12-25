Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
'Imports static PUBGMESP.SigScanSharp
Imports ShpVector3 = SharpDX.Vector3
Imports ShpVector2 = SharpDX.Vector2
Namespace PUBGMESP
	Partial Public Class MainForm
		Inherits Form

#Region "Modules"
		Private sigScan As SigScanSharp
		Private ueSearch As GameMemSearch
		Private espForm As ESPForm
		Private aimbotForm As AimbotForm
#End Region

#Region "Variables"
		Private Const WINDOW_NAME As String = "腾讯手游助手【极速傲引擎】"
		Private Const WINDOW_NAME_G As String = "Gameloop【Turbo AOW Motoru】"
		Private hwnd As IntPtr = IntPtr.Zero
		Private rect As RECT
		Private uWorld As Long
		Private uWorlds As Long
		Private uLevel As Long
		Private gNames As Long
		Private viewWorld As Long
		Private gameInstance As Long
		Private playerController As Long
		Private playerCarry As Long
		Private myTeamID As Integer
		Private uMyself As Long
		Private myWorld As Long
		Private uCamera As Long
		Private uCursor As Long
		Private uMyObject As Long
		Private myObjectPos As Vector3
		Private entityEntry As Long
		Private entityCount As Long
#End Region

		Public Sub New()
			InitializeComponent()
		End Sub

		Private Sub MainForm_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
		End Sub

		Private Sub Btn_Activate_Click(ByVal sender As Object, ByVal e As EventArgs) Handles Btn_Activate.Click
			' Enable Debug Privilige
			EnableDebugPriv()
			' Get Window Handle
			hwnd = FindWindow("TXGuiFoundation", "Gameloop【Turbo AOW Engine】")
			Console.WriteLine(hwnd)
			If hwnd = IntPtr.Zero Then
				hwnd = FindWindow("TXGuiFoundation", WINDOW_NAME_G)
				If hwnd = IntPtr.Zero Then
					MessageBox.Show("Please Open Emulator First!!!")
					Return
				End If
			End If
			hwnd = FindWindowEx(hwnd, 0, "AEngineRenderWindowClass", "AEngineRenderWindow")

			' Find true aow_exe process
			Dim aowHandle = FindTrueAOWHandle()

			' Initialize Memory
			Mem.Initialize(aowHandle)
			If Mem.m_pProcessHandle = IntPtr.Zero Then
				MessageBox.Show("Error", "Cannot initialize simulator memory, please restart simulator then retry")
				Return
			Else
				' Initialize SigScan
				sigScan = New SigScanSharp(Mem.m_pProcessHandle)
			End If

			' Find UWorld Offset
			ueSearch = New GameMemSearch(sigScan)
			Dim cands = ueSearch.ViewWorldSearchCandidates()
			viewWorld = ueSearch.GetViewWorld(cands)
			uWorld = viewWorld - 4217216
			gNames = viewWorld - 1638204
			If uWorld > 0 Then
				' Start Drawing ESP
				LoopTimer.Enabled = True
				UpdateTimer.Enabled = True
				GetWindowRect(hwnd, rect)
				espForm = New ESPForm(rect, ueSearch)
				aimbotForm = New AimbotForm(rect, ueSearch)
				CType(New Thread(AddressOf ESPThread), Thread).Start()
				CType(New Thread(AddressOf AimbotThread), Thread).Start()
				CType(New Thread(AddressOf InfoThread), Thread).Start()
				Btn_Activate.Enabled = False
				Btn_Activate.Text = "Injected"
			Else
				MessageBox.Show("Unable to initialize, please check if simulator and game is running")
			End If
		End Sub

		Private Sub InfoThread()
			' offset
			Dim controllerOffset, posOffset, healthOffset, nameOffset, teamIDOffset, poseOffset, statusOffset As Integer
			controllerOffset = 96
			posOffset = 336
			healthOffset = 1912
			nameOffset = 1512
			teamIDOffset = 1552
			statusOffset = 868
			poseOffset = 288
			Do
				' Read Basic Offset
				uWorlds = Mem.ReadMemory(Of Integer)(uWorld)
				uLevel = Mem.ReadMemory(Of Integer)(uWorlds + 32)
				gameInstance = Mem.ReadMemory(Of Integer)(uWorlds + 36)
				playerController = Mem.ReadMemory(Of Integer)(gameInstance + controllerOffset)
				playerCarry = Mem.ReadMemory(Of Integer)(playerController + 32)
				uMyObject = Mem.ReadMemory(Of Integer)(playerCarry + 788)
				'uMyself = Mem.ReadMemory<int>(uLevel + 124);
				'uMyself = Mem.ReadMemory<int>(uMyself + 36);
				'uMyself = Mem.ReadMemory<int>(uMyself + 312);
				'uCamera = Mem.ReadMemory<int>(playerCarry + 804) + 832;
				'uCursor = playerCarry + 732;
				'myWorld = Mem.ReadMemory<int>(uMyObject + 312);
				'myObjectPos = Mem.ReadMemory<Vector3>(myWorld + posOffset);
				entityEntry = Mem.ReadMemory(Of Integer)(uLevel + 112)
				entityCount = Mem.ReadMemory(Of Integer)(uLevel + 116)
				' Initilize Display Data
				Dim data As New DisplayData(viewWorld, uMyObject)
				Dim playerList As New List(Of PlayerData)()
				Dim itemList As New List(Of ItemData)()
				Dim vehicleList As New List(Of VehicleData)()
				Dim boxList As New List(Of BoxData)()
				Dim grenadeList As New List(Of GrenadeData)()
				For i As Integer = 0 To entityCount - 1
					Dim entityAddv As Long = Mem.ReadMemory(Of Integer)(entityEntry + i * 4)
					Dim entityStruct As Long = Mem.ReadMemory(Of Integer)(entityAddv + 16)
					Dim entityType As String = GameData.GetEntityType(gNames, entityStruct)
					If Settings.PlayerESP Then
						' if entity is player
						If GameData.IsPlayer(entityType) Then
							'Console.WriteLine(entityType);
							Dim playerWorld As Long = Mem.ReadMemory(Of Integer)(entityAddv + 312)
							' read player info
							' dead player continue
							Dim status As Integer = Mem.ReadMemory(Of Integer)(playerWorld + statusOffset)

							If status = 6 Then
								Continue For
							End If
							' my team player continue
							'int isTeam = Mem.ReadMemory<int>(Mem.ReadMemory<int>(Mem.ReadMemory<int>(entityAddv + 724 + 4)) + 20);
							'if (isTeam > 0)
							'    continue;
							Mem.WriteMemory(Of Integer)(Mem.ReadMemory(Of Integer)(uMyObject + 2656) + 352, 300000)

							'INSTANT VB NOTE: The variable name was renamed since Visual Basic does not handle local variables named the same as class members well:
							Dim name_Renamed As String = Encoding.Unicode.GetString(Mem.ReadMemory(Mem.ReadMemory(Of Integer)(entityAddv + nameOffset), 32))
							name_Renamed = name_Renamed.Substring(0, name_Renamed.IndexOf(ControlChars.NullChar))
							Dim playerData As PlayerData = New PlayerData With {.Type = entityType, .Address = entityAddv, .Position = Mem.ReadMemory(Of ShpVector3)(playerWorld + posOffset), .Status = status, .Pose = Mem.ReadMemory(Of Integer)(playerWorld + poseOffset), .IsRobot = If(Mem.ReadMemory(Of Integer)(entityAddv + 692) = 0, True, False), .Health = Mem.ReadMemory(Of Single)(entityAddv + healthOffset), .Name = name_Renamed, .TeamID = Mem.ReadMemory(Of Integer)(entityAddv + teamIDOffset)}
							If playerData.Address = uMyObject OrElse playerData.Address = uMyself Then
								myTeamID = playerData.TeamID
								Continue For
							End If
							If playerData.TeamID = myTeamID Then
								Continue For
							End If
							'Console.WriteLine(entityType);
							playerList.Add(playerData)
							Continue For
						End If
					End If
					If Settings.ItemESP Then
						' check if this entity is item
						Dim item As Item = GameData.GetItemType(entityType)
						If item <> PUBGMESP.Item.Useless Then
							' Read Item Info
							Dim itemData As ItemData = New ItemData With {.Name = item.GetDescription(), .Position = Mem.ReadMemory(Of ShpVector3)(Mem.ReadMemory(Of Integer)(entityAddv + 312) + posOffset), .Type = item}
							itemList.Add(itemData)
						End If
						' check if this entity is box
						If GameData.IsBox(entityType) Then
							' Read Box Info
							Dim boxEntity As Long = Mem.ReadMemory(Of Integer)(entityAddv + 312)
							Dim boxData As New BoxData()
							boxData.Position = Mem.ReadMemory(Of ShpVector3)(boxEntity + posOffset)
							boxList.Add(boxData)
							Continue For
						End If
					End If
					If Settings.VehicleESP Then
						Dim vehicle As Vehicle = GameData.GetVehicleType(entityType)
						If vehicle <> PUBGMESP.Vehicle.Unknown Then
							' Read Vehicle Info
							Dim vehicleData As VehicleData = New VehicleData With {.Position = Mem.ReadMemory(Of ShpVector3)(Mem.ReadMemory(Of Integer)(entityAddv + 312) + posOffset), .Type = vehicle, .Name = vehicle.GetDescription()}
							vehicleList.Add(vehicleData)
							Continue For
						End If
					End If
					' check if the entity is a grenade
					Dim grenade As Grenade = GameData.GetGrenadeType(entityType)
					If grenade <> PUBGMESP.Grenade.Unknown Then
						Dim grenadeEntity As Long = Mem.ReadMemory(Of Integer)(entityAddv + 312)
						Dim greData As GrenadeData = New GrenadeData With {.Type = grenade, .Position = Mem.ReadMemory(Of ShpVector3)(grenadeEntity + posOffset)}
						grenadeList.Add(greData)
					End If
				Next i
				data.Players = playerList.ToArray()
				data.Items = itemList.ToArray()
				data.Vehicles = vehicleList.ToArray()
				data.Boxes = boxList.ToArray()
				data.Grenades = grenadeList.ToArray()
				espForm.UpdateData(data)
				aimbotForm.UpdateData(data)
				Thread.Sleep(10)
			Loop
		End Sub

		Private Sub ESPThread()
			espForm.Initialize()
			Do
				espForm.Update()
				'Thread.Sleep(10);
			Loop
		End Sub
		Private Sub AimbotThread()
			aimbotForm.Initialize()
			Do
				aimbotForm.Update()
				'Thread.Sleep(10);
			Loop
		End Sub
		Private Function FindTrueAOWHandle() As IntPtr
			Dim aowHandle As IntPtr = IntPtr.Zero
			Dim maxThread As UInteger = 0
			'INSTANT VB NOTE: The variable handle was renamed since Visual Basic does not handle local variables named the same as class members well:
			Dim handle_Renamed As IntPtr = CreateToolhelp32Snapshot(&H2, 0)
			If CInt(Math.Truncate(CDbl(handle_Renamed))) > 0 Then
				Dim pe32 As New ProcessEntry32()
				pe32.dwSize = CUInt(Marshal.SizeOf(pe32))
				Dim bMore As Integer = Process32First(handle_Renamed, pe32)
				Do While bMore = 1
					Dim temp As IntPtr = Marshal.AllocHGlobal(CInt(pe32.dwSize))
					Marshal.StructureToPtr(pe32, temp, True)
					Dim pe As ProcessEntry32 = DirectCast(Marshal.PtrToStructure(temp, GetType(ProcessEntry32)), ProcessEntry32)
					Marshal.FreeHGlobal(temp)
					If pe.szExeFile.Contains("aow_exe.exe") AndAlso pe.cntThreads > maxThread Then
						maxThread = pe.cntThreads
						aowHandle = CType(pe.th32ProcessID, IntPtr)
					End If

					bMore = Process32Next(handle_Renamed, pe32)
				Loop
				CloseHandle(handle_Renamed)
			End If
			Return aowHandle
		End Function


		Private Function EnableDebugPriv() As Boolean
			Dim hToken As IntPtr = IntPtr.Zero
			If Not OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_ADJUST_PRIVILEGES Or TOKEN_QUERY, hToken) Then
				Return False
			End If
			Dim luid As New LUID()
			If Not LookupPrivilegeValue(Nothing, "SeDebugPrivilege", luid) Then
				CloseHandle(hToken)
				Return False
			End If
			Dim tp As New TOKEN_PRIVILEGES()
			tp.PrivilegeCount = 1
			tp.Privileges = New LUID_AND_ATTRIBUTES(0) {}
			tp.Privileges(0).Luid = luid
			tp.Privileges(0).Attributes = SE_PRIVILEGE_ENABLED
			If Not AdjustTokenPrivileges(hToken, False, tp, 0, IntPtr.Zero, IntPtr.Zero) Then
				Return False
			End If
			CloseHandle(hToken)
			Return True
		End Function



#Region "WIN32 API"
		<DllImport("user32.dll", SetLastError:=True)>
		Private Shared Function GetWindow(ByVal hWnd As IntPtr, ByVal uCmd As UInteger) As IntPtr
		End Function

		<DllImport("user32.dll")>
		Private Shared Function GetWindowRect(ByVal hWnd As IntPtr, ByRef lpRect As RECT) As <MarshalAs(UnmanagedType.Bool)> Boolean
		End Function

		<DllImport("user32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
		Private Shared Function FindWindow(ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr
		End Function

		<DllImport("user32.dll", EntryPoint:="FindWindowEx", SetLastError:=True)>
		Private Shared Function FindWindowEx(ByVal hwndParent As IntPtr, ByVal hwndChildAfter As UInteger, ByVal lpszClass As String, ByVal lpszWindow As String) As IntPtr
		End Function

		<DllImport("User32.dll")>
		Public Shared Function GetAsyncKeyState(ByVal vKey As Keys) As Boolean
		End Function

		Private Const TOKEN_ADJUST_PRIVILEGES As Integer = &H20
		Private Const TOKEN_QUERY As Integer = &H8
		Private Const SE_PRIVILEGE_ENABLED As Integer = &H2

		<DllImport("advapi32", SetLastError:=True), SuppressUnmanagedCodeSecurityAttribute>
		Private Shared Function OpenProcessToken(ByVal ProcessHandle As IntPtr, ByVal DesiredAccess As Integer, ByRef TokenHandle As IntPtr) As Boolean
		End Function

		<DllImport("kernel32", SetLastError:=True), SuppressUnmanagedCodeSecurityAttribute>
		Private Shared Function CloseHandle(ByVal handle As IntPtr) As Boolean
		End Function

		<StructLayout(LayoutKind.Sequential)>
		Private Structure LUID
			Public LowPart As UInt32
			Public HighPart As Int32
		End Structure

		<StructLayout(LayoutKind.Sequential, Pack:=4)>
		Private Structure LUID_AND_ATTRIBUTES
			Public Luid As LUID
			Public Attributes As UInt32
		End Structure

		<DllImport("advapi32.dll", SetLastError:=True)>
		Private Shared Function LookupPrivilegeValue(ByVal lpSystemName As String, ByVal lpName As String, ByRef lpLuid As LUID) As Boolean
		End Function

		Private Structure TOKEN_PRIVILEGES
			Public PrivilegeCount As Integer
			<MarshalAs(UnmanagedType.ByValArray)>
			Public Privileges() As LUID_AND_ATTRIBUTES
		End Structure
		' Use this signature if you want the previous state information returned
		<DllImport("advapi32.dll", SetLastError:=True)>
		Private Shared Function AdjustTokenPrivileges(ByVal TokenHandle As IntPtr, <MarshalAs(UnmanagedType.Bool)> ByVal DisableAllPrivileges As Boolean, ByRef NewState As TOKEN_PRIVILEGES, ByVal BufferLengthInBytes As UInt32, ByVal prev As IntPtr, ByVal relen As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
		End Function

		<DllImport("KERNEL32.DLL ")>
		Public Shared Function CreateToolhelp32Snapshot(ByVal flags As UInteger, ByVal processid As UInteger) As IntPtr
		End Function
		<DllImport("KERNEL32.DLL ")>
		Public Shared Function Process32First(ByVal handle As IntPtr, ByRef pe As ProcessEntry32) As Integer
		End Function
		<DllImport("KERNEL32.DLL ")>
		Public Shared Function Process32Next(ByVal handle As IntPtr, ByRef pe As ProcessEntry32) As Integer
		End Function

		<StructLayout(LayoutKind.Sequential)>
		Public Structure ProcessEntry32
			Public dwSize As UInteger
			Public cntUsage As UInteger
			Public th32ProcessID As UInteger
			Public th32DefaultHeapID As IntPtr
			Public th32ModuleID As UInteger
			Public cntThreads As UInteger
			Public th32ParentProcessID As UInteger
			Public pcPriClassBase As Integer
			Public dwFlags As UInteger

			<MarshalAs(UnmanagedType.ByValTStr, SizeConst:=260)>
			Public szExeFile As String
		End Structure
		<DllImport("user32.dll", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall)>
		Public Shared Sub mouse_event(ByVal dwFlags As Long, ByVal dx As Long, ByVal dy As Long, ByVal cButtons As Long, ByVal dwExtraInfo As Long)
		End Sub
#End Region

		Private Sub Loop_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles LoopTimer.Tick
			GetWindowRect(hwnd, rect)
			If espForm IsNot Nothing Then
				espForm._window.FitToWindow(hwnd, True)
			End If
			If aimbotForm IsNot Nothing Then
				aimbotForm._window.FitToWindow(hwnd, True)
			End If
		End Sub

		Private Sub Update_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles UpdateTimer.Tick
			If GetAsyncKeyState(Keys.End) Then
				System.Environment.Exit(-1)
			End If
			If GetAsyncKeyState(Keys.End) Then
				Me.Close()
			End If
			If GetAsyncKeyState(Keys.Home) Then
				Settings.ShowMenu = Not Settings.ShowMenu
			End If
			If GetAsyncKeyState(Keys.NumPad1) Then
				Settings.PlayerESP = Not Settings.PlayerESP
			End If
			If GetAsyncKeyState(Keys.NumPad2) Then
				Settings.PlayerBox = Not Settings.PlayerBox
			End If
			If GetAsyncKeyState(Keys.NumPad3) Then
				Settings.PlayerBone = Not Settings.PlayerBone
			End If
			If GetAsyncKeyState(Keys.NumPad4) Then
				Settings.PlayerLines = Not Settings.PlayerLines
			End If
			If GetAsyncKeyState(Keys.NumPad5) Then
				Settings.PlayerHealth = Not Settings.PlayerHealth
			End If
			If GetAsyncKeyState(Keys.NumPad6) Then
				Settings.ItemESP = Not Settings.ItemESP
			End If
			If GetAsyncKeyState(Keys.NumPad7) Then
				Settings.VehicleESP = Not Settings.VehicleESP
			End If
			If GetAsyncKeyState(Keys.NumPad8) Then
				Settings.Player3dBox = Not Settings.Player3dBox
			End If
			If GetAsyncKeyState(Keys.F5) Then
				Settings.aimEnabled = Not Settings.aimEnabled
			End If
			If GetAsyncKeyState(Keys.F6) Then
				Settings.bDrawFow = Not Settings.bDrawFow
			End If
		End Sub
		<DllImport("user32.dll")>
		Public Shared Sub mouse_event(ByVal dwFlags As UInteger, ByVal dx As Integer, ByVal dy As Integer, ByVal dwData As UInteger, ByVal dwExtraInfo As UIntPtr)
		End Sub

		Private Sub MainForm_FormClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs) Handles MyBase.FormClosing
			' Stop ESP
			System.Environment.Exit(-1)
		End Sub
	End Class
End Namespace
