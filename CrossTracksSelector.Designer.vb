<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CrossTrailSelector
    Inherits System.Windows.Forms.Form

    'Uživatelský ovládací prvek UserControl přepisuje metodu Dispose, aby vyčistil seznam součástí.
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
        Me.label = New System.Windows.Forms.Label()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.txtInfo = New System.Windows.Forms.TextBox()
        Me.chkListTracks = New System.Windows.Forms.CheckedListBox()
        Me.SuspendLayout()
        '
        'label
        '
        Me.label.Location = New System.Drawing.Point(0, 0)
        Me.label.Name = "label"
        Me.label.Size = New System.Drawing.Size(100, 23)
        Me.label.TabIndex = 0
        '
        'btnOK
        '
        Me.btnOK.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnOK.Location = New System.Drawing.Point(868, 486)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(205, 58)
        Me.btnOK.TabIndex = 2
        Me.btnOK.Text = "OK"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'txtInfo
        '
        Me.txtInfo.Font = New System.Drawing.Font("Cascadia Mono", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.txtInfo.Location = New System.Drawing.Point(12, 32)
        Me.txtInfo.Multiline = True
        Me.txtInfo.Name = "txtInfo"
        Me.txtInfo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtInfo.Size = New System.Drawing.Size(1108, 313)
        Me.txtInfo.TabIndex = 6
        '
        'chkListTracks
        '
        Me.chkListTracks.FormattingEnabled = True
        Me.chkListTracks.Location = New System.Drawing.Point(12, 351)
        Me.chkListTracks.Name = "chkListTracks"
        Me.chkListTracks.Size = New System.Drawing.Size(1102, 119)
        Me.chkListTracks.TabIndex = 8
        '
        'CrossTrailSelector
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1126, 556)
        Me.Controls.Add(Me.chkListTracks)
        Me.Controls.Add(Me.txtInfo)
        Me.Controls.Add(Me.btnOK)
        Me.Name = "CrossTrailSelector"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents label As Label
    Friend WithEvents btnOK As Button
    Friend WithEvents txtInfo As TextBox
    Friend WithEvents chkListTracks As CheckedListBox
End Class
