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
        Me.lblSpecify = New System.Windows.Forms.Label()
        Me.ListBox1 = New System.Windows.Forms.ListBox()
        Me.btnOK = New System.Windows.Forms.Button()
        Me.lblFileName = New System.Windows.Forms.Label()
        Me.txtIndexes = New System.Windows.Forms.TextBox()
        Me.SuspendLayout()
        '
        'label
        '
        Me.label.Location = New System.Drawing.Point(0, 0)
        Me.label.Name = "label"
        Me.label.Size = New System.Drawing.Size(100, 23)
        Me.label.TabIndex = 0
        '
        'lblSpecify
        '
        Me.lblSpecify.AutoSize = True
        Me.lblSpecify.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblSpecify.Location = New System.Drawing.Point(40, 65)
        Me.lblSpecify.Name = "lblSpecify"
        Me.lblSpecify.Size = New System.Drawing.Size(1063, 32)
        Me.lblSpecify.TabIndex = 0
        Me.lblSpecify.Text = "Specify the indexes of the tracks, which are Cross - Tracks (separated by a comma" &
    "):"
        '
        'ListBox1
        '
        Me.ListBox1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.ListBox1.FormattingEnabled = True
        Me.ListBox1.ItemHeight = 29
        Me.ListBox1.Location = New System.Drawing.Point(46, 136)
        Me.ListBox1.Name = "ListBox1"
        Me.ListBox1.Size = New System.Drawing.Size(1036, 178)
        Me.ListBox1.TabIndex = 1
        '
        'btnOK
        '
        Me.btnOK.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.btnOK.Location = New System.Drawing.Point(46, 396)
        Me.btnOK.Name = "btnOK"
        Me.btnOK.Size = New System.Drawing.Size(205, 58)
        Me.btnOK.TabIndex = 2
        Me.btnOK.Text = "OK"
        Me.btnOK.UseVisualStyleBackColor = True
        '
        'lblFileName
        '
        Me.lblFileName.AutoSize = True
        Me.lblFileName.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.lblFileName.Location = New System.Drawing.Point(49, 3)
        Me.lblFileName.Name = "lblFileName"
        Me.lblFileName.Size = New System.Drawing.Size(143, 32)
        Me.lblFileName.TabIndex = 3
        Me.lblFileName.Text = "File Name"
        '
        'txtIndexes
        '
        Me.txtIndexes.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.txtIndexes.Location = New System.Drawing.Point(46, 332)
        Me.txtIndexes.Name = "txtIndexes"
        Me.txtIndexes.Size = New System.Drawing.Size(255, 35)
        Me.txtIndexes.TabIndex = 4
        '
        'CrossTrailSelector
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1156, 466)
        Me.Controls.Add(Me.txtIndexes)
        Me.Controls.Add(Me.lblFileName)
        Me.Controls.Add(Me.btnOK)
        Me.Controls.Add(Me.ListBox1)
        Me.Controls.Add(Me.lblSpecify)
        Me.Name = "CrossTrailSelector"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents label As Label
    Friend WithEvents lblSpecify As Label
    Friend WithEvents ListBox1 As ListBox
    Friend WithEvents btnOK As Button
    Friend WithEvents lblFileName As Label
    Friend WithEvents txtIndexes As TextBox
End Class
