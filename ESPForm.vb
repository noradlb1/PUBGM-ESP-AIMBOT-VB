Imports System
Imports System.Text
Imports GameOverlay.Drawing
Imports GameOverlay.Windows
Imports SharpDX
Imports Point = GameOverlay.Drawing.Point
Imports Color = GameOverlay.Drawing.Color
Imports Rectangle = GameOverlay.Drawing.Rectangle
Imports RawVector2 = SharpDX.Mathematics.Interop.RawVector2
Imports ShpVector3 = SharpDX.Vector3
Imports ShpVector2 = SharpDX.Vector2
Namespace PUBGMESP
	Public Interface IESPForm
		Sub Initialize()
		Sub Update()
	End Interface

	Public Class ESPForm
		Implements IESPForm

		Public ReadOnly _window As OverlayWindow
		Private ReadOnly _graphics As Graphics
		Private ReadOnly _ueSearch As GameMemSearch

		Private _font As Font
		Private _infoFont As Font
		Private _bigfont As Font
		Private _black As SolidBrush
		Private _red As SolidBrush
		Private _green As SolidBrush
		Private _blue As SolidBrush
		Private _orange As SolidBrush
		Private _purple As SolidBrush
		Private _yellow As SolidBrush
		Private _white As SolidBrush
		Private _transparent As SolidBrush
		Private _txtBrush As SolidBrush
		Private _randomBrush() As SolidBrush
		Private _boxBrush As SolidBrush


		Private _data As DisplayData
		Private playerCount As Integer

		' offset
		Private actorOffset, boneOffset, tmpOffset As Integer


		Public Sub New(ByVal rect As RECT, ByVal ueSearch As GameMemSearch)
			Me._ueSearch = ueSearch

			_window = New OverlayWindow(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top) With {.IsTopmost = True, .IsVisible = True}

			AddHandler _window.SizeChanged, AddressOf _window_SizeChanged

			_graphics = New Graphics With {.MeasureFPS = True, .Height = _window.Height, .PerPrimitiveAntiAliasing = True, .TextAntiAliasing = True, .UseMultiThreadedFactories = False, .VSync = True, .Width = _window.Width, .WindowHandle = IntPtr.Zero}

			' offset
			actorOffset = 320
			boneOffset = 1408
			tmpOffset = 776
		End Sub

		Protected Overrides Sub Finalize()
			_graphics.Dispose()
			_window.Dispose()
		End Sub

		Public Sub Initialize() Implements IESPForm.Initialize
			_window.CreateWindow()

			_graphics.WindowHandle = _window.Handle
			_graphics.Setup()

			_font = _graphics.CreateFont("Microsoft YaHei", 10)
			_infoFont = _graphics.CreateFont("Microsoft YaHei", 12)
			_bigfont = _graphics.CreateFont("Microsoft YaHei", 18, True)

			_black = _graphics.CreateSolidBrush(0, 0, 0)
			_red = _graphics.CreateSolidBrush(255, 99, 71)
			_green = _graphics.CreateSolidBrush(Color.Green)
			_blue = _graphics.CreateSolidBrush(135, 206, 250)
			_orange = _graphics.CreateSolidBrush(255, 97, 0)
			_purple = _graphics.CreateSolidBrush(255, 105, 180)
			_yellow = _graphics.CreateSolidBrush(255, 255, 0)
			_white = _graphics.CreateSolidBrush(255, 255, 255)
			_transparent = _graphics.CreateSolidBrush(0, 0, 0, 0)
			_randomBrush = New SolidBrush() { _orange,_red,_green,_blue,_yellow,_white,_purple }
			_txtBrush = _graphics.CreateSolidBrush(0, 0, 0, 0.5F)
		End Sub

		Public Sub UpdateData(ByVal data As DisplayData)
			_data = data
		End Sub

		Public Sub Update() Implements IESPForm.Update
			Dim gfx = _graphics
			gfx.BeginScene()
			gfx.ClearScene(_transparent)
			' Draw FPS
			'gfx.DrawTextWithBackground(_font, _red, _black, 10, 10, "FPS: " + gfx.FPS);
			' Draw Menu
			If Settings.ShowMenu Then
				'gfx.FillRectangle(_menuBrush, 10f, _window.Height / 2 - 75, 180, _window.Height / 2 + 165);
				DrawShadowText(gfx, _font, _green, New Point(20F, _window.Height \ 2 - 65), "  [ AM7 PUBG ] ")
				DrawShadowText(gfx, _font, _green, New Point(20F, _window.Height \ 2 - 50), "┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈")
				DrawShadowText(gfx, _font, _green, New Point(20F, _window.Height \ 2 - 35), "ESP Menu")
				If Settings.PlayerESP Then
					DrawShadowText(gfx, _font, _red, New Point(20F, _window.Height \ 2 - 20), "Player ESP    (Num1) :  " & Settings.PlayerESP.ToString())
				Else
					DrawShadowText(gfx, _font, _green, New Point(20F, _window.Height \ 2 - 20), "Player ESP    (Num1) :  " & Settings.PlayerESP.ToString())
				End If
				If Settings.PlayerBox Then
					DrawShadowText(gfx, _font, _red, New Point(20F, _window.Height \ 2 - 5), "Player Box    (Num2) :  " & Settings.PlayerBox.ToString())
				Else
					DrawShadowText(gfx, _font, _green, New Point(20F, _window.Height \ 2 - 5), "Player Box    (Num2) :  " & Settings.PlayerBox.ToString())
				End If
				If Settings.PlayerBone Then
					DrawShadowText(gfx, _font, _red, New Point(20F, _window.Height \ 2 + 10), "Player Bone   (Num3) :  " & Settings.PlayerBone.ToString())
				Else
					DrawShadowText(gfx, _font, _green, New Point(20F, _window.Height \ 2 + 10), "Player Bone   (Num3) :  " & Settings.PlayerBone.ToString())
				End If
				If Settings.PlayerLines Then
					DrawShadowText(gfx, _font, _red, New Point(20F, _window.Height \ 2 + 25), "Player Line   (Num4) :  " & Settings.PlayerLines.ToString())
				Else
					DrawShadowText(gfx, _font, _green, New Point(20F, _window.Height \ 2 + 25), "Player Line   (Num4) :  " & Settings.PlayerLines.ToString())
				End If
				If Settings.PlayerHealth Then
					DrawShadowText(gfx, _font, _red, New Point(20F, _window.Height \ 2 + 40), "Player Health (Num5) :  " & Settings.PlayerHealth.ToString())
				Else
					DrawShadowText(gfx, _font, _green, New Point(20F, _window.Height \ 2 + 40), "Player Health (Num5) :  " & Settings.PlayerHealth.ToString())
				End If
				If Settings.ItemESP Then
					DrawShadowText(gfx, _font, _red, New Point(20F, _window.Height \ 2 + 55), "Item ESP      (Num6) :  " & Settings.ItemESP.ToString())
				Else
					DrawShadowText(gfx, _font, _green, New Point(20F, _window.Height \ 2 + 55), "Item ESP      (Num6) :  " & Settings.ItemESP.ToString())
				End If
				If Settings.VehicleESP Then
					DrawShadowText(gfx, _font, _red, New Point(20F, _window.Height \ 2 + 70), "Vehicle ESP   (Num7) :  " & Settings.VehicleESP.ToString())
				Else
					DrawShadowText(gfx, _font, _green, New Point(20F, _window.Height \ 2 + 70), "Vehicle ESP   (Num7) :  " & Settings.VehicleESP.ToString())
				End If
				If Settings.Player3dBox Then
					DrawShadowText(gfx, _font, _red, New Point(20F, _window.Height \ 2 + 85), "Player 3D Box    (Num8) :  " & Settings.Player3dBox.ToString())
				Else
					DrawShadowText(gfx, _font, _green, New Point(20F, _window.Height \ 2 + 85), "Player 3D Box   (Num8) :  " & Settings.Player3dBox.ToString())
				End If
				DrawShadowText(gfx, _font, _green, New Point(20F, _window.Height \ 2 + 100), "┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈")
			End If
			If _data.Players.Length > 0 Then
				playerCount = _data.Players.Length
				DrawShadowText(gfx, _bigfont, _red, New Point(_window.Width \ 2 - 40F, 40F), "Enemy near  :  " & playerCount)
			End If
			' Read View Matrix
			Dim viewMatrixAddr As Long = Mem.ReadMemory(Of Integer)(Mem.ReadMemory(Of Integer)(_data.ViewMatrixBase) + 32) + 512
			Dim viewMatrix As D3DMatrix = Algorithms.ReadViewMatrix(viewMatrixAddr)
			' Draw Player ESP
			If Settings.PlayerESP Then
				For Each player In _data.Players

					'if (player.Health <= 0) continue;
					If Algorithms.WorldToScreenPlayer(viewMatrix, player.Position, ShpVector3 playerScreen, Integer distance, _window.Width, _window.Height) Then
						' Too Far not render
						If distance > 500 Then
							Continue For
						End If
						Dim x As Single = playerScreen.X
						Dim y As Single = playerScreen.Y
						Dim h As Single = playerScreen.Z
						'monstermc
						Dim w As Single = playerScreen.Z / 2

						Try
							_boxBrush = _randomBrush(player.TeamID Mod 7)
						Catch e1 As IndexOutOfRangeException
							_boxBrush = _green
						End Try
						'DrawShadowText(gfx,_font, _green, new Point((x - playerScreen.Z / 4) - 3, y - 15), player.Pose.ToString());

						' Adjust Box
						If player.Pose = 1114636288 Then
							'monstermc
							y = playerScreen.Y + playerScreen.Z / 5
							'monstermc
							h -= playerScreen.Z / 5
						End If
						If player.Pose = 1112014848 OrElse player.Status = 7 Then
							'monstermc
							y = playerScreen.Y + playerScreen.Z / 4
							'monstermc
							h -= playerScreen.Z / 4
						End If

						' Draw Info
						Dim sb As New StringBuilder("[")
						If player.IsRobot Then
							sb.Append("[Bot] ")
						End If
						sb.Append(player.Name)
						DrawShadowText(gfx, _infoFont, _boxBrush, New Point(x + w / 2, y - 5), sb.ToString())
						' Draw Distance
						sb = New StringBuilder("[")
						sb.Append(distance).Append("M]")
						DrawShadowText(gfx, _font, _boxBrush, New Point(x + w / 2, y + 7), sb.ToString())

						If Settings.PlayerBox Then
							' Draw Box
							'monstermc
							gfx.DrawRectangle(_boxBrush, Rectangle.Create(x - playerScreen.Z / 4 - 3, y - 5, w + 3, h + 5), 1)
						End If
						If Settings.Player3dBox Then
							Draw3DBox(viewMatrix, player, playerScreen, _window.Width, _window.Height, 180.0F)
							' Draw Box
							'gfx.DrawRectangle(_boxBrush, Rectangle.Create(x - playerScreen.Z / 4 - 3, y - 5, w + 3, h + 5), 1);
						End If
						If Settings.PlayerBone Then
							' Draw Bone
							Dim tmpAddv As Long = Mem.ReadMemory(Of Integer)(player.Address + tmpOffset)
							Dim bodyAddv As Long = tmpAddv + actorOffset
							Dim boneAddv As Long = Mem.ReadMemory(Of Integer)(tmpAddv + boneOffset) + 48
							DrawPlayerBone(bodyAddv, boneAddv, w, viewMatrix, _window.Width, _window.Height)
						End If
						If Settings.PlayerHealth Then
							' Draw Health
							'monstermc
							DrawPlayerBlood((x - playerScreen.Z / 4) - 8, y - 5, h + 5, 3, player.Health)
						End If
						If Settings.PlayerLines Then
							' Draw Line
							gfx.DrawLine(_white, New Line(_window.Width \ 2, 0, x, y), 2)
						End If
					End If
				Next player
			End If
			' Draw Item ESP
			If Settings.ItemESP Then
				For Each item In _data.Items
					If Algorithms.WorldToScreenItem(viewMatrix, item.Position, Vector2 itemScreen, Integer distance, _window.Width, _window.Height) Then
						' Too Far not render
						If distance > 100 Then
							Continue For
						End If
						' Draw Item
						Dim disStr As String = String.Format("[{0}m]", distance)
						DrawShadowText(gfx, _font, _yellow, New Point(itemScreen.X, itemScreen.Y), item.Name)
						DrawShadowText(gfx, _font, _yellow, New Point(itemScreen.X, itemScreen.Y + 10), disStr)
					End If
				Next item
				For Each box In _data.Boxes
					If Algorithms.WorldToScreenItem(viewMatrix, box.Position, Vector2 itemScreen, Integer distance, _window.Width, _window.Height) Then
						' Too Far not render
						If distance > 100 Then
							Continue For
						End If
						DrawShadowText(gfx, _font, _yellow, New Point(itemScreen.X, itemScreen.Y), "Lootbox [" & distance.ToString() & "M]")
					End If
				Next box
			End If
			' Draw Vehicle ESP
			If Settings.VehicleESP Then
				For Each car In _data.Vehicles
					If Algorithms.WorldToScreenItem(viewMatrix, car.Position, Vector2 carScreen, Integer distance, _window.Width, _window.Height) Then
						' Too Far not render
						If distance > 300 Then
							Continue For
						End If
						Dim disStr As String = String.Format("[{0}m]", distance)
						' Draw Car
						DrawShadowText(gfx, _font, _blue, New Point(carScreen.X, carScreen.Y), car.Name)
						DrawShadowText(gfx, _font, _blue, New Point(carScreen.X, carScreen.Y + 10), disStr)
					End If
				Next car
			End If
			' Grenade alert
			For Each gre In _data.Grenades
				If Algorithms.WorldToScreenItem(viewMatrix, gre.Position, Vector2 greScreen, Integer distance, _window.Width, _window.Height) Then
					DrawShadowText(gfx, _font, 15, _red, New Point(greScreen.X, greScreen.Y), String.Format("!!! {0} !!! [{1}M]", gre.Type.GetDescription(), distance))
				End If
			Next gre
			gfx.EndScene()
		End Sub
		Private Sub Draw3DBox(ByVal viewMatrix As D3DMatrix, ByVal player As PlayerData, ByVal playersc As ShpVector3, ByVal winWidth As Integer, ByVal winHeight As Integer, Optional ByVal hei As Single = 180.0F)
			Dim num As Single = 70.0F
			Dim num2 As Single = 60.0F
			Dim num3 As Single = 50.0F
			Dim num4 As Single = 85.0F
			hei = 180.0F
			Dim vector As New ShpVector3(num3, -num2 / 2.0F, 0F)
			Dim vector2 As New ShpVector3(num3, num2 / 2.0F, 0F)
			Dim vector3 As New ShpVector3(num3 - num, num2 / 2.0F, 0F)
			Dim vector4 As New ShpVector3(num3 - num, -num2 / 2.0F, 0F)

			'monstermc
			Dim matrix As Matrix = Matrix.RotationZ((6.28318548F * (player.Position.Y) / 180F / 2F))
			Dim vector5 As New ShpVector3(player.Position.X, player.Position.Y, player.Position.Z - num4)
			vector = ShpVector3.TransformCoordinate(vector, matrix) + vector5
			vector2 = ShpVector3.TransformCoordinate(vector2, matrix) + vector5
			vector3 = ShpVector3.TransformCoordinate(vector3, matrix) + vector5
			vector4 = ShpVector3.TransformCoordinate(vector4, matrix) + vector5
			Dim vector6 As ShpVector2

			If Not Algorithms.WorldToScreen3DBox(viewMatrix, vector, vector6, winWidth, winHeight) Then
				Return
			End If
			Dim vector7 As ShpVector2
			If Not Algorithms.WorldToScreen3DBox(viewMatrix, vector2, vector7, winWidth, winHeight) Then
				Return
			End If
			Dim vector8 As ShpVector2
			If Not Algorithms.WorldToScreen3DBox(viewMatrix, vector3, vector8, winWidth, winHeight) Then
				Return
			End If
			Dim vector9 As ShpVector2
			If Not Algorithms.WorldToScreen3DBox(viewMatrix, vector4, vector9, winWidth, winHeight) Then
				Return
			End If

			Dim array() As RawVector2 = { vector6, vector7, vector8, vector9, vector6 }
			DrawLines(array, _boxBrush)
			vector.Z += hei

			Dim arg_240_0 As Boolean = Algorithms.WorldToScreen3DBox(viewMatrix, vector, vector6, winWidth, winHeight)
			vector2.Z += hei
			Dim flag As Boolean = Algorithms.WorldToScreen3DBox(viewMatrix, vector2, vector7, winWidth, winHeight)
			vector3.Z += hei
			Dim flag2 As Boolean = Algorithms.WorldToScreen3DBox(viewMatrix, vector3, vector8, winWidth, winHeight)
			vector4.Z += hei
			Dim flag3 As Boolean = Algorithms.WorldToScreen3DBox(viewMatrix, vector4, vector9, winWidth, winHeight)
			If (Not arg_240_0) OrElse (Not flag) OrElse (Not flag2) OrElse (Not flag3) Then
				Return
			End If
			Dim array2() As RawVector2 = { vector6, vector7, vector8, vector9, vector6 }
			DrawLines(array2, _boxBrush)
			DrawLine(New RawVector2(array(0).X, array(0).Y), New RawVector2(array2(0).X, array2(0).Y), _boxBrush)
			DrawLine(New RawVector2(array(1).X, array(1).Y), New RawVector2(array2(1).X, array2(1).Y), _boxBrush)
			DrawLine(New RawVector2(array(2).X, array(2).Y), New RawVector2(array2(2).X, array2(2).Y), _boxBrush)
			DrawLine(New RawVector2(array(3).X, array(3).Y), New RawVector2(array2(3).X, array2(3).Y), _boxBrush)
		End Sub
		Public Sub DrawLines(ByVal point0() As RawVector2, ByVal gxx As IBrush)
			If point0.Length < 2 Then
				Return
			End If
			For i As Integer = 0 To point0.Length - 2
				DrawLine(point0(i), point0(i + 1), _boxBrush)
			Next i
		End Sub
		Public Sub DrawLine(ByVal a As RawVector2, ByVal b As RawVector2, ByVal gcolr As IBrush)
			_graphics.DrawLine(_boxBrush, a.X, a.Y, b.X, b.Y, 1F)
		End Sub
		Private Sub DrawPlayerBone(ByVal bodyAddv As Long, ByVal boneAddv As Long, ByVal w As Single, ByVal viewMatrix As D3DMatrix, ByVal winWidth As Integer, ByVal winHeight As Integer)

			Dim sightX As Single = winWidth \ 2, sightY As Single = winHeight \ 2

			Dim headPos As ShpVector3 = Algorithms.GetBoneWorldPosition(bodyAddv, boneAddv + 5 * 48)
			headPos.Z += 7
			Dim head As ShpVector2
			Algorithms.WorldToScreenBone(viewMatrix, headPos, head, Integer distance, winWidth, winHeight)
			Dim neck As ShpVector2 = head
			Dim chest As ShpVector2
			Dim chestPos As ShpVector3 = Algorithms.GetBoneWorldPosition(bodyAddv, boneAddv + 4 * 48)
			Algorithms.WorldToScreenBone(viewMatrix, chestPos, chest, distance, winWidth, winHeight)
			Dim pelvis As ShpVector2
			Dim pelvisPos As ShpVector3 = Algorithms.GetBoneWorldPosition(bodyAddv, boneAddv + 1 * 48)
			Algorithms.WorldToScreenBone(viewMatrix, pelvisPos, pelvis, distance, winWidth, winHeight)
			Dim lSholder As ShpVector2
			Dim lSholderPos As ShpVector3 = Algorithms.GetBoneWorldPosition(bodyAddv, boneAddv + 11 * 48)
			Algorithms.WorldToScreenBone(viewMatrix, lSholderPos, lSholder, distance, winWidth, winHeight)
			Dim rSholder As ShpVector2
			Dim rSholderPos As ShpVector3 = Algorithms.GetBoneWorldPosition(bodyAddv, boneAddv + 32 * 48)
			Algorithms.WorldToScreenBone(viewMatrix, rSholderPos, rSholder, distance, winWidth, winHeight)
			Dim lElbow As ShpVector2
			Dim lElbowPos As ShpVector3 = Algorithms.GetBoneWorldPosition(bodyAddv, boneAddv + 12 * 48)
			Algorithms.WorldToScreenBone(viewMatrix, lElbowPos, lElbow, distance, winWidth, winHeight)
			Dim rElbow As ShpVector2
			Dim rElbowPos As ShpVector3 = Algorithms.GetBoneWorldPosition(bodyAddv, boneAddv + 33 * 48)
			Algorithms.WorldToScreenBone(viewMatrix, rElbowPos, rElbow, distance, winWidth, winHeight)
			Dim lWrist As ShpVector2
			Dim lWristPos As ShpVector3 = Algorithms.GetBoneWorldPosition(bodyAddv, boneAddv + 63 * 48)
			Algorithms.WorldToScreenBone(viewMatrix, lWristPos, lWrist, distance, winWidth, winHeight)
			Dim rWrist As ShpVector2
			Dim rWristPos As ShpVector3 = Algorithms.GetBoneWorldPosition(bodyAddv, boneAddv + 62 * 48)
			Algorithms.WorldToScreenBone(viewMatrix, rWristPos, rWrist, distance, winWidth, winHeight)
			Dim lThigh As ShpVector2
			Dim lThighPos As ShpVector3 = Algorithms.GetBoneWorldPosition(bodyAddv, boneAddv + 52 * 48)
			Algorithms.WorldToScreenBone(viewMatrix, lThighPos, lThigh, distance, winWidth, winHeight)
			Dim rThigh As ShpVector2
			Dim rThighPos As ShpVector3 = Algorithms.GetBoneWorldPosition(bodyAddv, boneAddv + 56 * 48)
			Algorithms.WorldToScreenBone(viewMatrix, rThighPos, rThigh, distance, winWidth, winHeight)
			Dim lKnee As ShpVector2
			Dim lKneePos As ShpVector3 = Algorithms.GetBoneWorldPosition(bodyAddv, boneAddv + 53 * 48)
			Algorithms.WorldToScreenBone(viewMatrix, lKneePos, lKnee, distance, winWidth, winHeight)
			Dim rKnee As ShpVector2
			Dim rKneePos As ShpVector3 = Algorithms.GetBoneWorldPosition(bodyAddv, boneAddv + 57 * 48)
			Algorithms.WorldToScreenBone(viewMatrix, rKneePos, rKnee, distance, winWidth, winHeight)
			Dim lAnkle As ShpVector2
			Dim lAnklePos As ShpVector3 = Algorithms.GetBoneWorldPosition(bodyAddv, boneAddv + 54 * 48)
			Algorithms.WorldToScreenBone(viewMatrix, lAnklePos, lAnkle, distance, winWidth, winHeight)
			Dim rAnkle As ShpVector2
			Dim rAnklePos As ShpVector3 = Algorithms.GetBoneWorldPosition(bodyAddv, boneAddv + 58 * 48)
			Algorithms.WorldToScreenBone(viewMatrix, rAnklePos, rAnkle, distance, winWidth, winHeight)

			If head IsNot Nothing AndAlso chest IsNot Nothing AndAlso pelvis IsNot Nothing AndAlso lSholder IsNot Nothing AndAlso rSholder IsNot Nothing AndAlso lElbow IsNot Nothing AndAlso rElbow IsNot Nothing AndAlso lWrist IsNot Nothing AndAlso rWrist IsNot Nothing AndAlso lThigh IsNot Nothing AndAlso rThigh IsNot Nothing AndAlso lKnee IsNot Nothing AndAlso rKnee IsNot Nothing AndAlso lAnkle IsNot Nothing AndAlso rAnkle IsNot Nothing Then

				_graphics.DrawCircle(_white, New Circle(head.X, head.Y, w / 6), 1)
				_graphics.DrawLine(_white, New Line(neck.X, neck.Y, chest.X, chest.Y), 1)
				_graphics.DrawLine(_white, New Line(chest.X, chest.Y, pelvis.X, pelvis.Y), 1)

				_graphics.DrawLine(_white, New Line(chest.X, chest.Y, lSholder.X, lSholder.Y), 1)
				_graphics.DrawLine(_white, New Line(chest.X, chest.Y, rSholder.X, rSholder.Y), 1)

				_graphics.DrawLine(_white, New Line(lSholder.X, lSholder.Y, lElbow.X, lElbow.Y), 1)
				_graphics.DrawLine(_white, New Line(rSholder.X, rSholder.Y, rElbow.X, rElbow.Y), 1)

				_graphics.DrawLine(_white, New Line(lElbow.X, lElbow.Y, lWrist.X, lWrist.Y), 1)
				_graphics.DrawLine(_white, New Line(rElbow.X, rElbow.Y, rWrist.X, rWrist.Y), 1)

				_graphics.DrawLine(_white, New Line(pelvis.X, pelvis.Y, lThigh.X, lThigh.Y), 1)
				_graphics.DrawLine(_white, New Line(pelvis.X, pelvis.Y, rThigh.X, rThigh.Y), 1)

				_graphics.DrawLine(_white, New Line(lThigh.X, lThigh.Y, lKnee.X, lKnee.Y), 1)
				_graphics.DrawLine(_white, New Line(rThigh.X, rThigh.Y, rKnee.X, rKnee.Y), 1)

				_graphics.DrawLine(_white, New Line(lKnee.X, lKnee.Y, lAnkle.X, lAnkle.Y), 1)
				_graphics.DrawLine(_white, New Line(rKnee.X, rKnee.Y, rAnkle.X, rAnkle.Y), 1)

			End If
		End Sub


		Private Sub DrawShadowText(ByVal gfx As Graphics, ByVal font As Font, ByVal brush As IBrush, ByVal pt As Point, ByVal txt As String)
			Dim bpPt = New Point(pt.X - 1, pt.Y + 1)
			'var bpPt2 = new Point(pt.X + 1, pt.Y - 1);
			gfx.DrawText(font, _txtBrush, bpPt, txt)
			'gfx.DrawText(font, _txtBrush, bpPt2, txt);
			gfx.DrawText(font, brush, pt, txt)

		End Sub
		Private Sub DrawShadowText(ByVal gfx As Graphics, ByVal font As Font, ByVal fontSize As Single, ByVal brush As IBrush, ByVal pt As Point, ByVal txt As String)
			Dim bpPt = New Point()
			bpPt.X = pt.X - 1
			bpPt.Y = pt.Y + 1
			gfx.DrawText(font, fontSize, _txtBrush, bpPt, txt)
			gfx.DrawText(font, fontSize, brush, pt, txt)
		End Sub

		Private Sub DrawPlayerBlood(ByVal x As Single, ByVal y As Single, ByVal h As Single, ByVal w As Single, ByVal fBlood As Single)
			If fBlood > 70.0 Then
				'FillRGB(x, y, 5, h, TextBlack);
				'FillRGB(x, y, 5, h * fBlood / 100.0, TextGreen);
				_graphics.FillRectangle(_black, Rectangle.Create(x, y, w, h))
				_graphics.FillRectangle(_green, Rectangle.Create(x, y, w, h * fBlood / 100))
			End If
			If fBlood > 30.0 AndAlso fBlood <= 70.0 Then
				'FillRGB(x, y, 5, h, TextBlack);
				'FillRGB(x, y, 5, h * fBlood / 100.0, TextYellow);
				_graphics.FillRectangle(_black, Rectangle.Create(x, y, w, h))
				_graphics.FillRectangle(_yellow, Rectangle.Create(x, y, w, h * fBlood / 100))
			End If
			If fBlood > 0.0 AndAlso fBlood <= 30.0 Then
				'FillRGB(x, y, 5, h, TextBlack);
				'FillRGB(x, y, 5, h * fBlood / 100.0, TextRed);
				_graphics.FillRectangle(_black, Rectangle.Create(x, y, w, h))
				_graphics.FillRectangle(_red, Rectangle.Create(x, y, w, h * fBlood / 100))
			End If
		End Sub

		Private Sub _window_SizeChanged(ByVal sender As Object, ByVal e As OverlaySizeEventArgs)
			If _graphics Is Nothing Then
				Return
			End If

			If _graphics.IsInitialized Then
				_graphics.Resize(e.Width, e.Height)
			Else
				_graphics.Width = e.Width
				_graphics.Height = e.Height
			End If
		End Sub
	End Class
End Namespace
