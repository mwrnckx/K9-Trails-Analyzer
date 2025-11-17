<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmEditCategoryPoints
    Inherits System.Windows.Forms.Form

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmEditCategoryPoints))
        lblFind = New Label()
        lblSpeed = New Label()
        lblAcc = New Label()
        lblRead = New Label()
        lblPick = New Label()
        numFind = New NumericUpDown()
        numSpeed = New NumericUpDown()
        numAcc = New NumericUpDown()
        numRead = New NumericUpDown()
        numPick = New NumericUpDown()
        btnOK = New Button()
        btnCancel = New Button()
        CType(numFind, ComponentModel.ISupportInitialize).BeginInit()
        CType(numSpeed, ComponentModel.ISupportInitialize).BeginInit()
        CType(numAcc, ComponentModel.ISupportInitialize).BeginInit()
        CType(numRead, ComponentModel.ISupportInitialize).BeginInit()
        CType(numPick, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' lblFind
        ' 
        resources.ApplyResources(lblFind, "lblFind")
        lblFind.Name = "lblFind"
        ' 
        ' lblSpeed
        ' 
        resources.ApplyResources(lblSpeed, "lblSpeed")
        lblSpeed.Name = "lblSpeed"
        ' 
        ' lblAcc
        ' 
        resources.ApplyResources(lblAcc, "lblAcc")
        lblAcc.Name = "lblAcc"
        ' 
        ' lblRead
        ' 
        resources.ApplyResources(lblRead, "lblRead")
        lblRead.Name = "lblRead"
        ' 
        ' lblPick
        ' 
        resources.ApplyResources(lblPick, "lblPick")
        lblPick.Name = "lblPick"
        ' 
        ' numFind
        ' 
        resources.ApplyResources(numFind, "numFind")
        numFind.BackColor = SystemColors.Window
        numFind.Increment = New Decimal(New Integer() {10, 0, 0, 0})
        numFind.Name = "numFind"
        ' 
        ' numSpeed
        ' 
        resources.ApplyResources(numSpeed, "numSpeed")
        numSpeed.BackColor = SystemColors.Window
        numSpeed.Increment = New Decimal(New Integer() {5, 0, 0, 0})
        numSpeed.Maximum = New Decimal(New Integer() {50, 0, 0, 0})
        numSpeed.Name = "numSpeed"
        ' 
        ' numAcc
        ' 
        resources.ApplyResources(numAcc, "numAcc")
        numAcc.BackColor = SystemColors.Window
        numAcc.Increment = New Decimal(New Integer() {10, 0, 0, 0})
        numAcc.Name = "numAcc"
        ' 
        ' numRead
        ' 
        resources.ApplyResources(numRead, "numRead")
        numRead.BackColor = SystemColors.Window
        numRead.Increment = New Decimal(New Integer() {10, 0, 0, 0})
        numRead.Name = "numRead"
        ' 
        ' numPick
        ' 
        resources.ApplyResources(numPick, "numPick")
        numPick.BackColor = SystemColors.Window
        numPick.Increment = New Decimal(New Integer() {10, 0, 0, 0})
        numPick.Name = "numPick"
        ' 
        ' btnOK
        ' 
        resources.ApplyResources(btnOK, "btnOK")
        btnOK.BackColor = Color.Goldenrod
        btnOK.DialogResult = DialogResult.OK
        btnOK.FlatAppearance.BorderSize = 0
        btnOK.Name = "btnOK"
        btnOK.UseVisualStyleBackColor = False
        ' 
        ' btnCancel
        ' 
        resources.ApplyResources(btnCancel, "btnCancel")
        btnCancel.BackColor = Color.Goldenrod
        btnCancel.DialogResult = DialogResult.Cancel
        btnCancel.FlatAppearance.BorderSize = 0
        btnCancel.Name = "btnCancel"
        btnCancel.UseVisualStyleBackColor = False
        ' 
        ' frmEditCategoryPoints
        ' 
        resources.ApplyResources(Me, "$this")
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.LightYellow
        Controls.Add(btnCancel)
        Controls.Add(btnOK)
        Controls.Add(numPick)
        Controls.Add(numRead)
        Controls.Add(numAcc)
        Controls.Add(numSpeed)
        Controls.Add(numFind)
        Controls.Add(lblPick)
        Controls.Add(lblRead)
        Controls.Add(lblAcc)
        Controls.Add(lblSpeed)
        Controls.Add(lblFind)
        FormBorderStyle = FormBorderStyle.FixedDialog
        MaximizeBox = False
        MinimizeBox = False
        Name = "frmEditCategoryPoints"
        CType(numFind, ComponentModel.ISupportInitialize).EndInit()
        CType(numSpeed, ComponentModel.ISupportInitialize).EndInit()
        CType(numAcc, ComponentModel.ISupportInitialize).EndInit()
        CType(numRead, ComponentModel.ISupportInitialize).EndInit()
        CType(numPick, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents lblFind As Label
    Friend WithEvents lblSpeed As Label
    Friend WithEvents lblAcc As Label
    Friend WithEvents lblRead As Label
    Friend WithEvents lblPick As Label

    Friend WithEvents numFind As NumericUpDown
    Friend WithEvents numSpeed As NumericUpDown
    Friend WithEvents numAcc As NumericUpDown
    Friend WithEvents numRead As NumericUpDown
    Friend WithEvents numPick As NumericUpDown

    Friend WithEvents btnOK As Button
    Friend WithEvents btnCancel As Button

End Class
