<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmEditComments
    Inherits System.Windows.Forms.Form

    'Formulář přepisuje metodu Dispose, aby vyčistil seznam součástí.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Vyžadováno Návrhářem Windows Form
    Private components As System.ComponentModel.IContainer

    'POZNÁMKA: Následující procedura je vyžadována Návrhářem Windows Form
    'Může být upraveno pomocí Návrháře Windows Form.  
    'Neupravovat pomocí editoru kódu
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        lblInfo = New Label()
        lblDog = New Label()
        lblTrail = New Label()
        lblGoal = New Label()
        btnOK = New Button()
        rtbGoal = New RichTextBox()
        rtbTrail = New RichTextBox()
        rtbDog = New RichTextBox()
        SuspendLayout()
        ' 
        ' lblInfo
        ' 
        lblInfo.AutoSize = True
        lblInfo.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        lblInfo.Location = New Point(34, 51)
        lblInfo.Name = "lblInfo"
        lblInfo.Size = New Size(74, 28)
        lblInfo.TabIndex = 0
        lblInfo.Text = "lblInfo"
        ' 
        ' lblDog
        ' 
        lblDog.AutoSize = True
        lblDog.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        lblDog.Location = New Point(34, 361)
        lblDog.Name = "lblDog"
        lblDog.Size = New Size(75, 28)
        lblDog.TabIndex = 2
        lblDog.Text = "lblDog"
        ' 
        ' lblTrail
        ' 
        lblTrail.AutoSize = True
        lblTrail.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        lblTrail.Location = New Point(34, 311)
        lblTrail.Name = "lblTrail"
        lblTrail.Size = New Size(77, 28)
        lblTrail.TabIndex = 3
        lblTrail.Text = "lblTrail"
        ' 
        ' lblGoal
        ' 
        lblGoal.AutoSize = True
        lblGoal.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        lblGoal.Location = New Point(34, 264)
        lblGoal.Name = "lblGoal"
        lblGoal.Size = New Size(79, 28)
        lblGoal.TabIndex = 4
        lblGoal.Text = "lblGoal"
        ' 
        ' btnOK
        ' 
        btnOK.BackColor = Color.DarkGoldenrod
        btnOK.FlatStyle = FlatStyle.Flat
        btnOK.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        btnOK.Location = New Point(1041, 451)
        btnOK.Name = "btnOK"
        btnOK.Size = New Size(134, 60)
        btnOK.TabIndex = 1
        btnOK.Text = "OK"
        btnOK.UseVisualStyleBackColor = False
        ' 
        ' rtbGoal
        ' 
        rtbGoal.EnableAutoDragDrop = True
        rtbGoal.Location = New Point(115, 258)
        rtbGoal.Name = "rtbGoal"
        rtbGoal.ScrollBars = RichTextBoxScrollBars.None
        rtbGoal.Size = New Size(1060, 45)
        rtbGoal.TabIndex = 7
        rtbGoal.Text = ""
        ' 
        ' rtbTrail
        ' 
        rtbTrail.EnableAutoDragDrop = True
        rtbTrail.Location = New Point(115, 311)
        rtbTrail.Name = "rtbTrail"
        rtbTrail.ScrollBars = RichTextBoxScrollBars.None
        rtbTrail.Size = New Size(1060, 45)
        rtbTrail.TabIndex = 8
        rtbTrail.Text = ""
        ' 
        ' rtbDog
        ' 
        rtbDog.EnableAutoDragDrop = True
        rtbDog.Location = New Point(115, 361)
        rtbDog.Name = "rtbDog"
        rtbDog.ScrollBars = RichTextBoxScrollBars.None
        rtbDog.Size = New Size(1060, 45)
        rtbDog.TabIndex = 9
        rtbDog.Text = ""
        ' 
        ' frmEditComments
        ' 
        AutoScaleDimensions = New SizeF(14F, 32F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        ClientSize = New Size(1199, 532)
        Controls.Add(rtbDog)
        Controls.Add(rtbTrail)
        Controls.Add(rtbGoal)
        Controls.Add(btnOK)
        Controls.Add(lblGoal)
        Controls.Add(lblTrail)
        Controls.Add(lblDog)
        Controls.Add(lblInfo)
        Font = New Font("Cascadia Mono", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(238))
        Margin = New Padding(5)
        Name = "frmEditComments"
        Text = "Edit comments"
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents lblInfo As Label
    Friend WithEvents lblDog As Label
    Friend WithEvents lblTrail As Label
    Friend WithEvents lblGoal As Label
    Friend WithEvents btnOK As Button
    Friend WithEvents rtbGoal As RichTextBox
    Friend WithEvents rtbTrail As RichTextBox
    Friend WithEvents rtbDog As RichTextBox
End Class
