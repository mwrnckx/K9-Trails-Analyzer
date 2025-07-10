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
        txtGoal = New TextBox()
        lblDog = New Label()
        lblTrail = New Label()
        lblGoal = New Label()
        txtDog = New TextBox()
        txtTrail = New TextBox()
        btnOK = New Button()
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
        ' txtGoal
        ' 
        txtGoal.AllowDrop = True
        txtGoal.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        txtGoal.Location = New Point(159, 155)
        txtGoal.Name = "txtGoal"
        txtGoal.Size = New Size(1016, 34)
        txtGoal.TabIndex = 1
        txtGoal.Text = "txtGoal"
        ' 
        ' lblDog
        ' 
        lblDog.AutoSize = True
        lblDog.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        lblDog.Location = New Point(34, 252)
        lblDog.Name = "lblDog"
        lblDog.Size = New Size(75, 28)
        lblDog.TabIndex = 2
        lblDog.Text = "lblDog"
        ' 
        ' lblTrail
        ' 
        lblTrail.AutoSize = True
        lblTrail.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        lblTrail.Location = New Point(34, 202)
        lblTrail.Name = "lblTrail"
        lblTrail.Size = New Size(77, 28)
        lblTrail.TabIndex = 3
        lblTrail.Text = "lblTrail"
        ' 
        ' lblGoal
        ' 
        lblGoal.AutoSize = True
        lblGoal.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        lblGoal.Location = New Point(34, 155)
        lblGoal.Name = "lblGoal"
        lblGoal.Size = New Size(79, 28)
        lblGoal.TabIndex = 4
        lblGoal.Text = "lblGoal"
        ' 
        ' txtDog
        ' 
        txtDog.AllowDrop = True
        txtDog.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        txtDog.Location = New Point(159, 252)
        txtDog.Name = "txtDog"
        txtDog.Size = New Size(1016, 34)
        txtDog.TabIndex = 5
        txtDog.Text = "txtDog"
        ' 
        ' txtTrail
        ' 
        txtTrail.AllowDrop = True
        txtTrail.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        txtTrail.Location = New Point(159, 202)
        txtTrail.Name = "txtTrail"
        txtTrail.Size = New Size(1016, 34)
        txtTrail.TabIndex = 6
        txtTrail.Text = "txtTrail"
        ' 
        ' btnOK
        ' 
        btnOK.BackColor = Color.DarkGoldenrod
        btnOK.FlatStyle = FlatStyle.Flat
        btnOK.Font = New Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        btnOK.Location = New Point(1041, 293)
        btnOK.Name = "btnOK"
        btnOK.Size = New Size(134, 60)
        btnOK.TabIndex = 1
        btnOK.Text = "OK"
        btnOK.UseVisualStyleBackColor = False
        ' 
        ' frmEditComments
        ' 
        AutoScaleDimensions = New SizeF(14F, 32F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        ClientSize = New Size(1199, 360)
        Controls.Add(btnOK)
        Controls.Add(txtTrail)
        Controls.Add(txtDog)
        Controls.Add(lblGoal)
        Controls.Add(lblTrail)
        Controls.Add(lblDog)
        Controls.Add(txtGoal)
        Controls.Add(lblInfo)
        Font = New Font("Cascadia Mono", 12F, FontStyle.Regular, GraphicsUnit.Point, CByte(238))
        Margin = New Padding(5)
        Name = "frmEditComments"
        Text = "Edit comments"
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents lblInfo As Label
    Friend WithEvents txtGoal As TextBox
    Friend WithEvents lblDog As Label
    Friend WithEvents lblTrail As Label
    Friend WithEvents lblGoal As Label
    Friend WithEvents txtDog As TextBox
    Friend WithEvents txtTrail As TextBox
    Friend WithEvents btnOK As Button
End Class
