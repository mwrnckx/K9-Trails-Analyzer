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
        rtbGoal = New RichTextBox()
        rtbTrail = New RichTextBox()
        rtbPerformance = New RichTextBox()
        btnAnotherLang = New Button()
        lblLanguage = New Label()
        ComboBox1 = New ComboBox()
        SuspendLayout()
        ' 
        ' lblInfo
        ' 
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
        btnOK.BackColor = Color.DarkGoldenrod
        btnOK.Name = "btnOK"
        btnOK.UseVisualStyleBackColor = False
        ' 
        ' rtbGoal
        ' 
        rtbGoal.EnableAutoDragDrop = True
        resources.ApplyResources(rtbGoal, "rtbGoal")
        rtbGoal.Name = "rtbGoal"
        ' 
        ' rtbTrail
        ' 
        rtbTrail.EnableAutoDragDrop = True
        resources.ApplyResources(rtbTrail, "rtbTrail")
        rtbTrail.Name = "rtbTrail"
        ' 
        ' rtbPerformance
        ' 
        rtbPerformance.EnableAutoDragDrop = True
        resources.ApplyResources(rtbPerformance, "rtbPerformance")
        rtbPerformance.Name = "rtbPerformance"
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
        ' frmEditComments
        ' 
        resources.ApplyResources(Me, "$this")
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        Controls.Add(ComboBox1)
        Controls.Add(btnAnotherLang)
        Controls.Add(rtbPerformance)
        Controls.Add(rtbTrail)
        Controls.Add(rtbGoal)
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
    Friend WithEvents rtbGoal As RichTextBox
    Friend WithEvents rtbTrail As RichTextBox
    Friend WithEvents rtbPerformance As RichTextBox
    Friend WithEvents btnAnotherLang As Button
    Friend WithEvents lblLanguage As Label
    Friend WithEvents ComboBox1 As ComboBox
End Class
