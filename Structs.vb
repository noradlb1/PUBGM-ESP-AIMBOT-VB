Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Threading.Tasks

Namespace PUBGMESP

	<StructLayout(LayoutKind.Sequential)> _
	Public Structure RECT
		Public left As Integer
		Public top As Integer
		Public right As Integer
		Public bottom As Integer
	End Structure

	<StructLayout(LayoutKind.Explicit)> _
	Friend Structure FTTransform
		<FieldOffset(&H0)> _
		Public Rotation As Vector4
		<FieldOffset(&H10)> _
		Public Translation As Vector3
		<FieldOffset(&H1C)> _
		Public Scale3D As Vector3
	End Structure

	<StructLayout(LayoutKind.Explicit)> _
	Friend Structure FTTransform2
		<FieldOffset(&H0)> _
		Public Rotation As Vector4
		<FieldOffset(&H10)> _
		Public Translation As Vector3
		<FieldOffset(&H20)> _
		Public Scale3D As Vector3
	End Structure

	<StructLayout(LayoutKind.Sequential)> _
	Friend Structure D3DMatrix
		Public _11, _12, _13, _14 As Single
		Public _21, _22, _23, _24 As Single
		Public _31, _32, _33, _34 As Single
		Public _41, _42, _43, _44 As Single
	End Structure

	<StructLayout(LayoutKind.Sequential)> _
	Public Structure Vector2
		Public X As Single
		Public Y As Single
	End Structure

	<StructLayout(LayoutKind.Sequential)> _
	Public Structure Vector3
		Public X As Single
		Public Y As Single
		Public Z As Single
	End Structure

	<StructLayout(LayoutKind.Sequential)> _
	Public Structure Vector4

		Public X As Single
		Public Y As Single
		Public Z As Single
		Public W As Single

		Public Sub New(ByVal x As Single, ByVal y As Single, ByVal z As Single, ByVal w As Single)
			Me.New()
			Me.W = w
			Me.X = x
			Me.Y = y
			Me.Z = z
		End Sub
	End Structure
End Namespace
