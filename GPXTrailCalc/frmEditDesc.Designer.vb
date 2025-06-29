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
        Me.lblInfo = New System.Windows.Forms.Label()
        Me.txtGoal = New System.Windows.Forms.TextBox()
        Me.lblDog = New System.Windows.Forms.Label()
        Me.lblTrail = New System.Windows.Forms.Label()
        Me.lblGoal = New System.Windows.Forms.Label()
        Me.txtDog = New System.Windows.Forms.TextBox()
        Me.txtTrail = New System.Windows.Forms.TextBox()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lblInfo
        '
        Me.lblInfo.AutoSize = True
        Me.lblInfo.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblInfo.Location = New System.Drawing.Point(34, 51)
        Me.lblInfo.Name = "lblInfo"
        Me.lblInfo.Size = New System.Drawing.Size(74, 28)
        Me.lblInfo.TabIndex = 0
        Me.lblInfo.Text = "lblInfo"
        '
        'txtGoal
        '
        Me.txtGoal.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.txtGoal.Location = New System.Drawing.Point(159, 155)
        Me.txtGoal.Name = "txtGoal"
        Me.txtGoal.Size = New System.Drawing.Size(1016, 34)
        Me.txtGoal.TabIndex = 1
        Me.txtGoal.Text = "txtGoal"
        '
        'lblDog
        '
        Me.lblDog.AutoSize = True
        Me.lblDog.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblDog.Location = New System.Drawing.Point(34, 252)
        Me.lblDog.Name = "lblDog"
        Me.lblDog.Size = New System.Drawing.Size(75, 28)
        Me.lblDog.TabIndex = 2
        Me.lblDog.Text = "lblDog"
        '
        'lblTrail
        '
        Me.lblTrail.AutoSize = True
        Me.lblTrail.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblTrail.Location = New System.Drawing.Point(34, 202)
        Me.lblTrail.Name = "lblTrail"
        Me.lblTrail.Size = New System.Drawing.Size(77, 28)
        Me.lblTrail.TabIndex = 3
        Me.lblTrail.Text = "lblTrail"
        '
        'lblGoal
        '
        Me.lblGoal.AutoSize = True
        Me.lblGoal.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblGoal.Location = New System.Drawing.Point(34, 155)
        Me.lblGoal.Name = "lblGoal"
        Me.lblGoal.Size = New System.Drawing.Size(79, 28)
        Me.lblGoal.TabIndex = 4
        Me.lblGoal.Text = "lblGoal"
        '
        'txtDog
        '
        Me.txtDog.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.txtDog.Location = New System.Drawing.Point(159, 252)
        Me.txtDog.Name = "txtDog"
        Me.txtDog.Size = New System.Drawing.Size(1016, 34)
        Me.txtDog.TabIndex = 5
        Me.txtDog.Text = "txtDog"
        '
        'txtTrail
        '
        Me.txtTrail.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.txtTrail.Location = New System.Drawing.Point(159, 202)
        Me.txtTrail.Name = "txtTrail"
        Me.txtTrail.Size = New System.Drawing.Size(1016, 34)
        Me.txtTrail.TabIndex = 6
        Me.txtTrail.Text = "txtTrail"
        '
        'btnOK
        '
        Me.btnOK.BackColor = System.Drawing.Color.DarkGoldenrod
        Me.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnOK.Font = New System.Drawing.Font("Segoe UI", 10.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnOK.Location = New System.Drawing.Point(1041, 293)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(134, 60)
        Me.btnOK.TabIndex = 1
        Me.btnOK.Text = "OK"
        Me.btnOK.UseVisualStyleBackColor = False
        '
        'frmEditComments
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(14.0!, 32.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(237, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(213, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(1199, 360)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.txtTrail)
        Me.Controls.Add(Me.txtDog)
        Me.Controls.Add(Me.lblGoal)
        Me.Controls.Add(Me.lblTrail)
        Me.Controls.Add(Me.lblDog)
        Me.Controls.Add(Me.txtGoal)
        Me.Controls.Add(Me.lblInfo)
        Me.Font = New System.Drawing.Font("Cascadia Mono", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.Margin = New System.Windows.Forms.Padding(5)
        Me.Name = "frmEditComments"
        Me.Text = "Edit comments"
        Me.ResumeLayout(False)
        Me.PerformLayout()

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
