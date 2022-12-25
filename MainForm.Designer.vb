Namespace PUBGMESP
	Partial Public Class MainForm
		''' <summary>
		''' 必需的设计器变量。
		''' </summary>
		Private components As System.ComponentModel.IContainer = Nothing

		''' <summary>
		''' 清理所有正在使用的资源。
		''' </summary>
		''' <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		Protected Overrides Sub Dispose(ByVal disposing As Boolean)
			If disposing AndAlso (components IsNot Nothing) Then
				components.Dispose()
			End If
			MyBase.Dispose(disposing)
		End Sub

		#Region "Windows 窗体设计器生成的代码"

		''' <summary>
		''' 设计器支持所需的方法 - 不要修改
		''' 使用代码编辑器修改此方法的内容。
		''' </summary>
		Private Sub InitializeComponent()
			Me.components = New System.ComponentModel.Container()
			Me.Btn_Activate = New System.Windows.Forms.Button()
			Me.LoopTimer = New System.Windows.Forms.Timer(Me.components)
			Me.UpdateTimer = New System.Windows.Forms.Timer(Me.components)
			Me.SuspendLayout()
			' 
			' Btn_Activate
			' 
			Me.Btn_Activate.Dock = System.Windows.Forms.DockStyle.Fill
			Me.Btn_Activate.Font = New System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, (CByte(134)))
			Me.Btn_Activate.Location = New System.Drawing.Point(0, 0)
			Me.Btn_Activate.Name = "Btn_Activate"
			Me.Btn_Activate.Size = New System.Drawing.Size(419, 47)
			Me.Btn_Activate.TabIndex = 0
			Me.Btn_Activate.Text = "Inject"
			Me.Btn_Activate.UseVisualStyleBackColor = True
'			Me.Btn_Activate.Click += New System.EventHandler(Me.Btn_Activate_Click)
			' 
			' LoopTimer
			' 
			Me.LoopTimer.Interval = 500
'			Me.LoopTimer.Tick += New System.EventHandler(Me.Loop_Tick)
			' 
			' UpdateTimer
			' 
'			Me.UpdateTimer.Tick += New System.EventHandler(Me.Update_Tick)
			' 
			' MainForm
			' 
			Me.AutoScaleDimensions = New System.Drawing.SizeF(8F, 15F)
			Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
			Me.BackColor = System.Drawing.SystemColors.Control
			Me.ClientSize = New System.Drawing.Size(419, 47)
			Me.Controls.Add(Me.Btn_Activate)
			Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
			Me.Name = "MainForm"
			Me.Text = "PUBGM HACK - [ AM7 ]"
'			Me.FormClosing += New System.Windows.Forms.FormClosingEventHandler(Me.MainForm_FormClosing)
'			Me.Load += New System.EventHandler(Me.MainForm_Load)
			Me.ResumeLayout(False)

		End Sub

		#End Region
		Private WithEvents Btn_Activate As System.Windows.Forms.Button
		Private WithEvents LoopTimer As System.Windows.Forms.Timer
		Private WithEvents UpdateTimer As System.Windows.Forms.Timer
	End Class
End Namespace

