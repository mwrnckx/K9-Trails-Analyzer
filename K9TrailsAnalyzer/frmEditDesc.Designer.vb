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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEditComments))
        lblInfo = New Label()
        lblDog = New Label()
        lblTrail = New Label()
        lblGoal = New Label()
        btnOK = New Button()
        rtbGoal = New RichTextBox()
        rtbTrail = New RichTextBox()
        rtbDog = New RichTextBox()
        rtbGoalEng = New RichTextBox()
        Label1 = New Label()
        rtbTrailEng = New RichTextBox()
        Label2 = New Label()
        rtbDogEng = New RichTextBox()
        Label3 = New Label()
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
        lblDog.Location = New Point(38, 556)
        lblDog.Name = "lblDog"
        lblDog.Size = New Size(75, 28)
        lblDog.TabIndex = 2
        lblDog.Text = "lblDog"
        ' 
        ' lblTrail
        ' 
        lblTrail.AutoSize = True
        lblTrail.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        lblTrail.Location = New Point(34, 416)
        lblTrail.Name = "lblTrail"
        lblTrail.Size = New Size(77, 28)
        lblTrail.TabIndex = 3
        lblTrail.Text = "lblTrail"
        ' 
        ' lblGoal
        ' 
        lblGoal.AutoSize = True
        lblGoal.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        lblGoal.Location = New Point(34, 276)
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
        btnOK.Location = New Point(1041, 714)
        btnOK.Name = "btnOK"
        btnOK.Size = New Size(134, 60)
        btnOK.TabIndex = 1
        btnOK.Text = "OK"
        btnOK.UseVisualStyleBackColor = False
        ' 
        ' rtbGoal
        ' 
        rtbGoal.EnableAutoDragDrop = True
        rtbGoal.Location = New Point(244, 258)
        rtbGoal.Name = "rtbGoal"
        rtbGoal.ScrollBars = RichTextBoxScrollBars.Vertical
        rtbGoal.Size = New Size(931, 64)
        rtbGoal.TabIndex = 7
        rtbGoal.Text = ""
        ' 
        ' rtbTrail
        ' 
        rtbTrail.EnableAutoDragDrop = True
        rtbTrail.Location = New Point(244, 398)
        rtbTrail.Name = "rtbTrail"
        rtbTrail.ScrollBars = RichTextBoxScrollBars.Vertical
        rtbTrail.Size = New Size(931, 64)
        rtbTrail.TabIndex = 8
        rtbTrail.Text = ""
        ' 
        ' rtbDog
        ' 
        rtbDog.EnableAutoDragDrop = True
        rtbDog.Location = New Point(244, 538)
        rtbDog.Name = "rtbDog"
        rtbDog.ScrollBars = RichTextBoxScrollBars.Vertical
        rtbDog.Size = New Size(931, 64)
        rtbDog.TabIndex = 9
        rtbDog.Text = ""
        ' 
        ' rtbGoalEng
        ' 
        rtbGoalEng.EnableAutoDragDrop = True
        rtbGoalEng.Location = New Point(244, 328)
        rtbGoalEng.Name = "rtbGoalEng"
        rtbGoalEng.ScrollBars = RichTextBoxScrollBars.Vertical
        rtbGoalEng.Size = New Size(931, 64)
        rtbGoalEng.TabIndex = 11
        rtbGoalEng.Text = ""
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        Label1.Location = New Point(34, 346)
        Label1.Name = "Label1"
        Label1.Size = New Size(60, 28)
        Label1.TabIndex = 10
        Label1.Text = "Goal:"
        ' 
        ' rtbTrailEng
        ' 
        rtbTrailEng.EnableAutoDragDrop = True
        rtbTrailEng.Location = New Point(244, 468)
        rtbTrailEng.Name = "rtbTrailEng"
        rtbTrailEng.ScrollBars = RichTextBoxScrollBars.Vertical
        rtbTrailEng.Size = New Size(931, 64)
        rtbTrailEng.TabIndex = 13
        rtbTrailEng.Text = ""
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        Label2.Location = New Point(34, 486)
        Label2.Name = "Label2"
        Label2.Size = New Size(58, 28)
        Label2.TabIndex = 12
        Label2.Text = "Trail:"
        ' 
        ' rtbDogEng
        ' 
        rtbDogEng.EnableAutoDragDrop = True
        rtbDogEng.Location = New Point(244, 608)
        rtbDogEng.Name = "rtbDogEng"
        rtbDogEng.ScrollBars = RichTextBoxScrollBars.Vertical
        rtbDogEng.Size = New Size(931, 64)
        rtbDogEng.TabIndex = 15
        rtbDogEng.Text = ""
        ' 
        ' Label3
        ' 
        Label3.AutoSize = True
        Label3.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        Label3.Location = New Point(38, 626)
        Label3.Name = "Label3"
        Label3.Size = New Size(56, 28)
        Label3.TabIndex = 14
        Label3.Text = "Dog:"
        ' 
        ' frmEditComments
        ' 
        AutoScaleDimensions = New SizeF(14F, 32F)
        AutoScaleMode = AutoScaleMode.Font
        AutoSize = True
        BackColor = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        ClientSize = New Size(1199, 1039)
        Controls.Add(rtbDogEng)
        Controls.Add(Label3)
        Controls.Add(rtbTrailEng)
        Controls.Add(Label2)
        Controls.Add(rtbGoalEng)
        Controls.Add(Label1)
        Controls.Add(rtbDog)
        Controls.Add(rtbTrail)
        Controls.Add(rtbGoal)
        Controls.Add(btnOK)
        Controls.Add(lblGoal)
        Controls.Add(lblTrail)
        Controls.Add(lblDog)
        Controls.Add(lblInfo)
        Font = New Font("Cascadia Mono", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(238))
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
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
    Friend WithEvents rtbGoalEng As RichTextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents rtbTrailEng As RichTextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents rtbDogEng As RichTextBox
    Friend WithEvents Label3 As Label
End Class
