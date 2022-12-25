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
Imports System.Windows.Forms

Namespace PUBGMESP
	Public Interface IAimbotForm
		Sub Initialize()
		Sub Update()
	End Interface
	Public Class AimTarget
		Public Screen2D As ShpVector2
		Public CrosshairDistance As Single
		Public uniqueID As Integer
	End Class
	Public Class AimbotForm
		Implements IAimbotForm

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
		Private BestTargetUniqID As Integer = -1

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

		Public Sub Initialize() Implements IAimbotForm.Initialize
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

		Public Sub Update() Implements IAimbotForm.Update
			Dim gfx = _graphics
			gfx.BeginScene()
			gfx.ClearScene(_transparent)

			If Settings.ShowMenu Then

				DrawShadowText(gfx, _font, _green, New Point(20F, _window.Height \ 2 + 115), "Aimbot Menu")
				If Settings.aimEnabled Then
					DrawShadowText(gfx, _font, _red, New Point(20F, _window.Height \ 2 + 130), "Aimbot ON    (F5) :  " & Settings.aimEnabled.ToString())
				Else
					DrawShadowText(gfx, _font, _green, New Point(20F, _window.Height \ 2 + 130), "Aimbot OF    (F5) :  " & Settings.aimEnabled.ToString())
				End If
				If Settings.bDrawFow Then
					DrawShadowText(gfx, _font, _red, New Point(20F, _window.Height \ 2 + 145), "FOV SHOW    (F6) :  " & Settings.bDrawFow.ToString())
				Else
					DrawShadowText(gfx, _font, _green, New Point(20F, _window.Height \ 2 + 145), "FOV HIDE    (F6) :  " & Settings.bDrawFow.ToString())
				End If

				DrawShadowText(gfx, _font, _green, New Point(20F, _window.Height \ 2 + 160), "┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈")
			End If

			' Read View Matrix
			Dim viewMatrixAddr As Long = Mem.ReadMemory(Of Integer)(Mem.ReadMemory(Of Integer)(_data.ViewMatrixBase) + 32) + 512
			Dim viewMatrix As D3DMatrix = Algorithms.ReadViewMatrix(viewMatrixAddr)

			Dim AimTargets = New AimTarget(_data.Players.Length - 1){}
			Dim fClosestDist As Single = -1
			' Draw Player ESP
			If Settings.PlayerESP Then
				For i As Integer = 0 To _data.Players.Length - 1

					Dim player = _data.Players(i)
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

						Dim ScreenCenterX As Integer = _window.Width \ 2, ScreenCenterY As Integer = _window.Height \ 2

						If Settings.aimEnabled Then
							Dim tmpAddv As Long = Mem.ReadMemory(Of Integer)(player.Address + tmpOffset)
							Dim bodyAddv As Long = tmpAddv + actorOffset
							Dim boneAddv As Long = Mem.ReadMemory(Of Integer)(tmpAddv + boneOffset) + 48
							Dim headPos As ShpVector3 = Algorithms.GetBoneWorldPosition(bodyAddv, boneAddv + 5 * 48)

							headPos.Z += 7

							Dim clampPos = headPos - player.Position
							Dim w2sHead As Boolean = Algorithms.WorldToScreen3DBox(viewMatrix, New ShpVector3(headPos.X, headPos.Y - (Settings.bPredict * 2), headPos.Z - (Settings.bYAxis * 8)), ShpVector2 HeadPosition, _window.Width, _window.Height)

							AimTargets(i) = New AimTarget()
							AimTargets(i).Screen2D = HeadPosition
							AimTargets(i).uniqueID = player.TeamID
							AimTargets(i).CrosshairDistance = ShpVector2.Distance(HeadPosition, New ShpVector2(ScreenCenterX, ScreenCenterY))

							If BestTargetUniqID = -1 Then
								If Algorithms.isInside(ScreenCenterX, ScreenCenterY, Settings.bFovArray(Settings.bFovInt), AimTargets(i).Screen2D.X, AimTargets(i).Screen2D.Y) Then
									fClosestDist = AimTargets(i).CrosshairDistance
									BestTargetUniqID = AimTargets(i).uniqueID
								End If
							End If
							If MainForm.GetAsyncKeyState(Settings.bAimKeys(Settings.bAimKeyINT)) Then
								If BestTargetUniqID <> -1 Then
									Dim best = FindAimTargetByUniqueID(AimTargets, BestTargetUniqID)

									If best IsNot Nothing Then
											Dim roundPos = New ShpVector2(CSng(Math.Round(best.Screen2D.X)), CSng(Math.Round(best.Screen2D.Y)))
											AimAtPosV2(roundPos.X, roundPos.Y, _window.Width, _window.Height, False)

									End If
								End If
							Else
								BestTargetUniqID = -1
							End If
						End If
						If Settings.bDrawFow Then
							gfx.DrawCircle(_red, ScreenCenterX, ScreenCenterY, Settings.bFovArray(Settings.bFovInt), 2)
						End If
					End If
				Next i

				gfx.EndScene()
			End If
		End Sub

		Private Shared Function FindAimTargetByUniqueID(ByVal array() As AimTarget, ByVal uniqueID As Integer) As AimTarget
			Dim entityList = array
			For i As Integer = 0 To entityList.Length - 1
				Dim current = entityList(i)
				If current Is Nothing Then
					Continue For
				End If

				If current.uniqueID = uniqueID Then
					Return current
				End If
			Next i
			Return Nothing
		End Function
		'uc port
		Private Sub AimAtPosV2(ByVal x As Single, ByVal y As Single, ByVal Width As Integer, ByVal Height As Integer, ByVal smooth As Boolean)
			Dim ScreenCenterX As Integer = Width \ 2, ScreenCenterY As Integer = Height \ 2

			Dim AimSpeed As Single = CSng(Settings.bSmooth) + 1F
			Dim TargetX As Single = 0
			Dim TargetY As Single = 0

			'X Axis
			If x <> 0 Then
				If x > ScreenCenterX Then
					TargetX = -(ScreenCenterX - x)
					TargetX /= AimSpeed
					If TargetX + ScreenCenterX > ScreenCenterX * 2 Then
						TargetX = 0
					End If
				End If

				If x < ScreenCenterX Then
					TargetX = x - ScreenCenterX
					TargetX /= AimSpeed
					If TargetX + ScreenCenterX < 0 Then
						TargetX = 0
					End If
				End If
			End If

			'Y Axis

			If y <> 0 Then
				If y > ScreenCenterY Then
					TargetY = -(ScreenCenterY - y)
					TargetY /= AimSpeed
					If TargetY + ScreenCenterY > ScreenCenterY * 2 Then
						TargetY = 0
					End If
				End If

				If y < ScreenCenterY Then
					TargetY = y - ScreenCenterY
					TargetY /= AimSpeed
					If TargetY + ScreenCenterY < 0 Then
						TargetY = 0
					End If
				End If
			End If

			If Not smooth Then
				MainForm.mouse_event(1, CInt(Math.Truncate(TargetX)), CInt(Math.Truncate(TargetY)), 0, UIntPtr.Zero)
				Return
			End If

			TargetX /= 10
			TargetY /= 10

			If Math.Abs(TargetX) < 1 Then
				If TargetX > 0 Then
					TargetX = 1
				End If
				If TargetX < 0 Then
					TargetX = -1
				End If
			End If
			If Math.Abs(TargetY) < 1 Then
				If TargetY > 0 Then
					TargetY = 1
				End If
				If TargetY < 0 Then
					TargetY = -1
				End If
			End If
			MainForm.mouse_event(1, CInt(Math.Truncate(TargetX)), CInt(Math.Truncate(TargetY)), 0, UIntPtr.Zero)
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
	End Class

End Namespace
