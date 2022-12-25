Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports SharpDX
Imports ShpVector3 = SharpDX.Vector3
Imports ShpVector2 = SharpDX.Vector2
Namespace PUBGMESP
	Friend Class Algorithms
		''' <summary>
		''' Check if enemy is inside fov circle
		''' </summary>
		''' <param name="xc"></param>
		''' <param name="yc"></param>
		''' <param name="r"></param>
		''' <param name="x"></param>
		''' <param name="y"></param>
		''' <returns></returns>
		Public Shared Function isInside(ByVal circle_x As Single, ByVal circle_y As Single, ByVal rad As Single, ByVal x As Single, ByVal y As Single) As Boolean
			' Compare radius of circle with distance  
			' of its center from given point 
			If (x - circle_x) * (x - circle_x) + (y - circle_y) * (y - circle_y) <= rad * rad Then
				Return True
			Else
				Return False
			End If
		End Function

		''' <summary>
		''' Get Distance Between Enemy And Player
		''' </summary>
		''' <param name="x1"></param>
		''' <param name="y1"></param>
		''' <param name="x2"></param>
		''' <param name="y2"></param>
		''' <returns></returns>
		Public Shared Function GetDistance(ByVal x1 As Double, ByVal y1 As Double, ByVal x2 As Double, ByVal y2 As Double) As Double
			Return Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2))
		End Function

		''' <summary>
		''' Read View Matrix
		''' </summary>
		''' <param name="vAddv"></param>
		''' <returns></returns>
		public static D3DMatrix ReadViewMatrixFunction(vAddv As Long) Mem.ReadMemory(Of D3DMatrix)(vAddv)

		''' <summary>
		''' Read FTTransform
		''' </summary>
		''' <param name="vAddv"></param>
		''' <returns></returns>
		public static FTTransform ReadFTTransformFunction(vAddv As Long) Mem.ReadMemory(Of FTTransform)(vAddv)

		public static FTTransform2 ReadFTransform2Function(vAddv As Long) Mem.ReadMemory(Of FTTransform2)(vAddv)

		''' <summary>
		''' Get Bone's world position
		''' </summary>
		''' <param name="actorAddv"></param>
		''' <param name="boneAddv"></param>
		''' <returns></returns>
		public static ShpVector3 GetBoneWorldPosition(Long actorAddv, Long boneAddv)

			Dim bone = ReadFTransform2(boneAddv)
			Dim actor = ReadFTransform2(actorAddv)
			Dim boneMatrix = ToMatrixWithScale(bone.Translation, bone.Scale3D, bone.Rotation)
			Dim componentToWorldMatrix = ToMatrixWithScale(actor.Translation, actor.Scale3D, actor.Rotation)
			Dim newMatrix = MatrixMultiplication(boneMatrix, componentToWorldMatrix)
			Dim bonePos As New ShpVector3()
			bonePos.X = newMatrix._41
			bonePos.Y = newMatrix._42
			bonePos.Z = newMatrix._43
			Return bonePos

		''' <summary>
		''' To Matrix With Scale
		''' </summary>
		''' <param name="translation"></param>
		''' <param name="scale"></param>
		''' <param name="rot"></param>
		''' <returns></returns>
		private static D3DMatrix ToMatrixWithScale(Vector3 translation, Vector3 scale, Vector4 rot)
			Dim m As D3DMatrix = New D3DMatrix With {._41 = translation.X, ._42 = translation.Y, ._43 = translation.Z}

			Dim x2 As Single = rot.X + rot.X
			Dim y2 As Single = rot.Y + rot.Y
			Dim z2 As Single = rot.Z + rot.Z

			Dim xx2 As Single = rot.X * x2
			Dim yy2 As Single = rot.Y * y2
			Dim zz2 As Single = rot.Z * z2
			m._11 = (1.0F - (yy2 + zz2)) * scale.X
			m._22 = (1.0F - (xx2 + zz2)) * scale.Y
			m._33 = (1.0F - (xx2 + yy2)) * scale.Z

			Dim yz2 As Single = rot.Y * z2
			Dim wx2 As Single = rot.W * x2
			m._32 = (yz2 - wx2) * scale.Z
			m._23 = (yz2 + wx2) * scale.Y

			Dim xy2 As Single = rot.X * y2
			Dim wz2 As Single = rot.W * z2
			m._21 = (xy2 - wz2) * scale.Y
			m._12 = (xy2 + wz2) * scale.X

			Dim xz2 As Single = rot.X * z2
			Dim wy2 As Single = rot.W * y2
			m._31 = (xz2 + wy2) * scale.Z
			m._13 = (xz2 - wy2) * scale.X

			m._14 = 0.0F
			m._24 = 0.0F
			m._34 = 0.0F
			m._44 = 1.0F

			Return m

		''' <summary>
		''' D3DMatrix Mutiplication
		''' </summary>
		''' <param name="pM1"></param>
		''' <param name="pM2"></param>
		''' <returns></returns>
		public static D3DMatrix MatrixMultiplication(D3DMatrix pM1, D3DMatrix pM2)
			Dim pOut As D3DMatrix = New D3DMatrix With {._11 = pM1._11 * pM2._11 + pM1._12 * pM2._21 + pM1._13 * pM2._31 + pM1._14 * pM2._41, ._12 = pM1._11 * pM2._12 + pM1._12 * pM2._22 + pM1._13 * pM2._32 + pM1._14 * pM2._42, ._13 = pM1._11 * pM2._13 + pM1._12 * pM2._23 + pM1._13 * pM2._33 + pM1._14 * pM2._43, ._14 = pM1._11 * pM2._14 + pM1._12 * pM2._24 + pM1._13 * pM2._34 + pM1._14 * pM2._44, ._21 = pM1._21 * pM2._11 + pM1._22 * pM2._21 + pM1._23 * pM2._31 + pM1._24 * pM2._41, ._22 = pM1._21 * pM2._12 + pM1._22 * pM2._22 + pM1._23 * pM2._32 + pM1._24 * pM2._42, ._23 = pM1._21 * pM2._13 + pM1._22 * pM2._23 + pM1._23 * pM2._33 + pM1._24 * pM2._43, ._24 = pM1._21 * pM2._14 + pM1._22 * pM2._24 + pM1._23 * pM2._34 + pM1._24 * pM2._44, ._31 = pM1._31 * pM2._11 + pM1._32 * pM2._21 + pM1._33 * pM2._31 + pM1._34 * pM2._41, ._32 = pM1._31 * pM2._12 + pM1._32 * pM2._22 + pM1._33 * pM2._32 + pM1._34 * pM2._42, ._33 = pM1._31 * pM2._13 + pM1._32 * pM2._23 + pM1._33 * pM2._33 + pM1._34 * pM2._43, ._34 = pM1._31 * pM2._14 + pM1._32 * pM2._24 + pM1._33 * pM2._34 + pM1._34 * pM2._44, ._41 = pM1._41 * pM2._11 + pM1._42 * pM2._21 + pM1._43 * pM2._31 + pM1._44 * pM2._41, ._42 = pM1._41 * pM2._12 + pM1._42 * pM2._22 + pM1._43 * pM2._32 + pM1._44 * pM2._42, ._43 = pM1._41 * pM2._13 + pM1._42 * pM2._23 + pM1._43 * pM2._33 + pM1._44 * pM2._43, ._44 = pM1._41 * pM2._14 + pM1._42 * pM2._24 + pM1._43 * pM2._34 + pM1._44 * pM2._44}

			Return pOut

		''' <summary>
		''' Player's World To Screen Function
		''' </summary>
		''' <param name="vAddr"></param>
		''' <param name="pos"></param>
		''' <param name="screen"></param>
		''' <param name="windowWidth"></param>
		''' <param name="windowHeight"></param>
		''' <returns></returns>
		public static Boolean WorldToScreenPlayer(D3DMatrix viewMatrix, ShpVector3 pos, ShpVector3 screen, Integer distance, Integer windowWidth, Integer windowHeight)
			screen = New ShpVector3()
			'ScreenW = (GameViewMatrix._14 * _Enemy_Point.x) + (GameViewMatrix._24* _Enemy_Point.y) + (GameViewMatrix._34 * _Enemy_Point.z + GameViewMatrix._44);
			Dim screenW As Single = (viewMatrix._14 * pos.X) + (viewMatrix._24 * pos.Y) + (viewMatrix._34 * pos.Z + viewMatrix._44)
			distance = CInt(Math.Truncate(screenW / 100))
			If screenW < 0.0001F Then
				Return False
			End If

			' float ScreenY = (GameViewMatrix._12 * _Enemy_Point.x) + (GameViewMatrix._22 * _Enemy_Point.y) + (GameViewMatrix._32 * (_Enemy_Point.z + 85) + GameViewMatrix._42);
			Dim screenY As Single = (viewMatrix._12 * pos.X) + (viewMatrix._22 * pos.Y) + (viewMatrix._32 * (pos.Z + 85) + viewMatrix._42)
			' float ScreenX = (GameViewMatrix._11 * _Enemy_Point.x) + (GameViewMatrix._21 * _Enemy_Point.y) + (GameViewMatrix._31 * _Enemy_Point.z + GameViewMatrix._41);
			Dim screenX As Single = (viewMatrix._11 * pos.X) + (viewMatrix._21 * pos.Y) + (viewMatrix._31 * pos.Z + viewMatrix._41)
'monstermc
			screen.Y = (windowHeight / 2) - (windowHeight / 2) * screenY / screenW
'monstermc
			screen.X = (windowWidth / 2) + (windowWidth / 2) * screenX / screenW
			' float y1 = (pDxm->s_height / 2) - (GameViewMatrix._12*_Enemy_Point.x + GameViewMatrix._22 * _Enemy_Point.y + GameViewMatrix._32 *(_Enemy_Point.z - 95) + GameViewMatrix._42) *(pDxm->s_height / 2) / ScreenW;
'monstermc
			Dim y1 As Single = (windowHeight / 2) - (viewMatrix._12 * pos.X + viewMatrix._22 * pos.Y + viewMatrix._32 * (pos.Z - 95) + viewMatrix._42) * (windowHeight / 2) / screenW
			screen.Z = y1 - screen.Y
			Return True

		''' <summary>
		''' Bone's World to Screen Function
		''' </summary>
		''' <param name="viewMatrix"></param>
		''' <param name="pos"></param>
		''' <param name="screen"></param>
		''' <param name="windowWidth"></param>
		''' <param name="windowHeight"></param>
		''' <returns></returns>
		Public Static Boolean WorldToScreenBoneFunction(viewMatrix As D3DMatrix, pos As ShpVector3, ByRef screen As ShpVector2, ByRef distance As Integer, windowWidth As Integer, windowHeight As Integer) WorldToScreenItem(viewMatrix, pos, screen, distance, windowWidth, windowHeight)

		''' Item's World To Screen Function
		''' </summary>
		''' <param name="viewMatrix"></param>
		''' <param name="pos"></param>
		''' <param name="screen"></param>
		''' <param name="distance"></param>
		''' <param name="windowWidth"></param>
		''' <param name="windowHeight"></param>
		''' <returns></returns>
		Public Static Boolean WorldToScreenItem(D3DMatrix viewMatrix, ShpVector3 pos, ShpVector2 screen, Integer distance, Integer windowWidth, Integer windowHeight)
			screen = New ShpVector2()
			Dim screenW As Single = (viewMatrix._14 * pos.X) + (viewMatrix._24 * pos.Y) + (viewMatrix._34 * pos.Z + viewMatrix._44)
			distance = CInt(Math.Truncate(screenW / 100))
			If screenW < 0.0001F Then
		Return False
		End If
			screenW = 1 / screenW
			Dim sightX As Single = (windowWidth \ 2)
		Dim sightY As Single = (windowHeight \ 2)
			screen.X = sightX + (viewMatrix._11 * pos.X + viewMatrix._21 * pos.Y + viewMatrix._31 * pos.Z + viewMatrix._41) * screenW * sightX
			screen.Y = sightY - (viewMatrix._12 * pos.X + viewMatrix._22 * pos.Y + viewMatrix._32 * pos.Z + viewMatrix._42) * screenW * sightY
			Return (Not Single.IsNaN(screen.X)) AndAlso Not Single.IsNaN(screen.Y)


		Public Static Boolean WorldToScreen3DBox(D3DMatrix viewMatrix, ShpVector3 position, ShpVector2 res, Integer sw, Integer sh)
			res.X = 0F
			res.Y = 0F
			Dim matrix As D3DMatrix = viewMatrix
		Dim num As Double = CDbl(position.X * matrix._14 + position.Y * matrix._24 + position.Z * matrix._34 + matrix._44)
		If num < 0.100000001490116 Then
		Return False
		End If
		Dim num2 As Double = CDbl(position.X * matrix._11 + position.Y * matrix._21 + position.Z * matrix._31 + matrix._41)
		Dim num3 As Double = CDbl(position.X * matrix._12 + position.Y * matrix._22 + position.Z * matrix._32 + matrix._42)
			num2 /= num
			num3 /= num
'monstermc
			Dim num4 As Single = CSng(sw / 2)
		'monstermc
		Dim num5 As Single = CSng(sh / 2)
			res.X = CSng(CDbl(num4) * num2 + (num2 + CDbl(num4)))
			res.Y = CSng(-CSng(CDbl(num5) * num3) + (num3 + CDbl(num5)))
			Return (Not Single.IsNaN(res.X)) AndAlso Not Single.IsNaN(res.Y)


	End Class
End Namespace
