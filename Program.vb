Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Windows.Forms

Namespace PUBGMESP
	Friend NotInheritable Class Program

		Private Sub New()
		End Sub

		''' <summary>
		''' 应用程序的主入口点。
		''' </summary>
		<STAThread> _
		Shared Sub Main()
			Application.EnableVisualStyles()
			Application.SetCompatibleTextRenderingDefault(False)
			Application.Run(New MainForm())
		End Sub
	End Class
End Namespace
