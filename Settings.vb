Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace PUBGMESP
	Friend Class Settings
		Public Shared PlayerESP As Boolean = True
		Public Shared PlayerBone As Boolean = False
		Public Shared PlayerBox As Boolean = False
		Public Shared Player3dBox As Boolean = True
		Public Shared PlayerLines As Boolean = True
		Public Shared PlayerHealth As Boolean = False
		Public Shared ItemESP As Boolean = False
		Public Shared VehicleESP As Boolean = False
		Public Shared ShowMenu As Boolean = True

		' aimbot
		Public Shared aimEnabled As Boolean = True
		Public Shared bDrawFow As Boolean = True
		Public Shared bSmooth As Integer = 11
		Public Shared bFovInt As Integer = 2
		Public Shared bPredict As Integer = 1
		Public Shared bYAxis As Integer = 2
		Public Shared bAimKeyINT As Integer = 2
		Public Shared aimkeys() As String = { "CAPSLOCK", "LBUTTON", "RBUTTON", "LSHIFT", "V", "E", "Q" }
		Public Shared bAimKeys() As System.Windows.Forms.Keys = { System.Windows.Forms.Keys.CapsLock, System.Windows.Forms.Keys.LButton, System.Windows.Forms.Keys.RButton, System.Windows.Forms.Keys.LShiftKey, System.Windows.Forms.Keys.V, System.Windows.Forms.Keys.E, System.Windows.Forms.Keys.Q }
		Public Shared bFovArray() As Single = { 60F, 90F, 120F, 160F, 300F, 300F }
	End Class
End Namespace
