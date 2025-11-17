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
        lblPerformance = New Label()
        lblTrail = New Label()
        lblGoal = New Label()
        btnOK = New Button()
        btnAnotherLang = New Button()
        lblLanguage = New Label()
        ComboBox1 = New ComboBox()
        txtGoal = New TextBox()
        txtPerformance = New TextBox()
        txtTrail = New TextBox()
        lblDogName = New Label()
        lblHandlerName = New Label()
        txtDogName = New TextBox()
        txtHandlerName = New TextBox()
        SuspendLayout()
        ' 
        ' lblInfo
        ' 
        lblInfo.BackColor = Color.LightYellow
        resources.ApplyResources(lblInfo, "lblInfo")
        lblInfo.Name = "lblInfo"
        ' 
        ' lblPerformance
        ' 
        resources.ApplyResources(lblPerformance, "lblPerformance")
        lblPerformance.Name = "lblPerformance"
        ' 
        ' lblTrail
        ' 
        resources.ApplyResources(lblTrail, "lblTrail")
        lblTrail.Name = "lblTrail"
        ' 
        ' lblGoal
        ' 
        resources.ApplyResources(lblGoal, "lblGoal")
        lblGoal.Name = "lblGoal"
        ' 
        ' btnOK
        ' 
        resources.ApplyResources(btnOK, "btnOK")
        btnOK.BackColor = Color.Goldenrod
        btnOK.Name = "btnOK"
        btnOK.UseVisualStyleBackColor = False
        ' 
        ' btnAnotherLang
        ' 
        resources.ApplyResources(btnAnotherLang, "btnAnotherLang")
        btnAnotherLang.BackColor = Color.Salmon
        btnAnotherLang.Name = "btnAnotherLang"
        btnAnotherLang.UseVisualStyleBackColor = False
        ' 
        ' lblLanguage
        ' 
        resources.ApplyResources(lblLanguage, "lblLanguage")
        lblLanguage.Name = "lblLanguage"
        ' 
        ' ComboBox1
        ' 
        ComboBox1.FormattingEnabled = True
        resources.ApplyResources(ComboBox1, "ComboBox1")
        ComboBox1.Name = "ComboBox1"
        ' 
        ' txtGoal
        ' 
        txtGoal.AllowDrop = True
        resources.ApplyResources(txtGoal, "txtGoal")
        txtGoal.Name = "txtGoal"
        ' 
        ' txtPerformance
        ' 
        txtPerformance.AllowDrop = True
        resources.ApplyResources(txtPerformance, "txtPerformance")
        txtPerformance.Name = "txtPerformance"
        ' 
        ' txtTrail
        ' 
        txtTrail.AllowDrop = True
        resources.ApplyResources(txtTrail, "txtTrail")
        txtTrail.Name = "txtTrail"
        ' 
        ' lblDogName
        ' 
        resources.ApplyResources(lblDogName, "lblDogName")
        lblDogName.Name = "lblDogName"
        ' 
        ' lblHandlerName
        ' 
        resources.ApplyResources(lblHandlerName, "lblHandlerName")
        lblHandlerName.Name = "lblHandlerName"
        ' 
        ' txtDogName
        ' 
        resources.ApplyResources(txtDogName, "txtDogName")
        txtDogName.Name = "txtDogName"
        ' 
        ' txtHandlerName
        ' 
        resources.ApplyResources(txtHandlerName, "txtHandlerName")
        txtHandlerName.Name = "txtHandlerName"
        ' 
        ' frmEditComments
        ' 
        resources.ApplyResources(Me, "$this")
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.LightYellow
        Controls.Add(txtHandlerName)
        Controls.Add(txtDogName)
        Controls.Add(lblHandlerName)
        Controls.Add(lblDogName)
        Controls.Add(txtTrail)
        Controls.Add(txtPerformance)
        Controls.Add(txtGoal)
        Controls.Add(ComboBox1)
        Controls.Add(btnAnotherLang)
        Controls.Add(btnOK)
        Controls.Add(lblGoal)
        Controls.Add(lblTrail)
        Controls.Add(lblLanguage)
        Controls.Add(lblPerformance)
        Controls.Add(lblInfo)
        Name = "frmEditComments"
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents lblInfo As Label
    Friend WithEvents lblPerformance As Label
    Friend WithEvents lblTrail As Label
    Friend WithEvents lblGoal As Label
    Friend WithEvents btnOK As Button
    Friend WithEvents btnAnotherLang As Button
    Friend WithEvents lblLanguage As Label
    Friend WithEvents ComboBox1 As ComboBox
    Friend WithEvents txtGoal As TextBox
    Friend WithEvents txtPerformance As TextBox
    Friend WithEvents txtTrail As TextBox
    Friend WithEvents lblDogName As Label
    Friend WithEvents lblHandlerName As Label
    Friend WithEvents txtDogName As TextBox
    Friend WithEvents txtHandlerName As TextBox
End Class
