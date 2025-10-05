<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmCrossTrailSelector
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmCrossTrailSelector))
        Dim DataGridViewCellStyle1 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As DataGridViewCellStyle = New DataGridViewCellStyle()
        label = New Label()
        btnOK = New Button()
        txtInfo = New TextBox()
        dgvTracks = New DataGridView()
        nameColumn = New DataGridViewTextBoxColumn()
        startColumn = New DataGridViewTextBoxColumn()
        descColumn = New DataGridViewTextBoxColumn()
        typeColumn = New DataGridViewComboBoxColumn()
        CType(dgvTracks, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' label
        ' 
        resources.ApplyResources(label, "label")
        label.Name = "label"
        ' 
        ' btnOK
        ' 
        resources.ApplyResources(btnOK, "btnOK")
        btnOK.BackColor = Color.Salmon
        btnOK.Name = "btnOK"
        btnOK.UseVisualStyleBackColor = False
        ' 
        ' txtInfo
        ' 
        txtInfo.BackColor = Color.Beige
        resources.ApplyResources(txtInfo, "txtInfo")
        txtInfo.Name = "txtInfo"
        ' 
        ' dgvTracks
        ' 
        dgvTracks.AllowUserToAddRows = False
        dgvTracks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dgvTracks.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
        dgvTracks.BackgroundColor = Color.Beige
        dgvTracks.BorderStyle = BorderStyle.None
        dgvTracks.CellBorderStyle = DataGridViewCellBorderStyle.Single
        DataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle1.BackColor = Color.Beige
        DataGridViewCellStyle1.Font = New Font("Cascadia Code", 14.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        DataGridViewCellStyle1.ForeColor = Color.DarkSalmon
        DataGridViewCellStyle1.SelectionBackColor = SystemColors.GradientActiveCaption
        DataGridViewCellStyle1.SelectionForeColor = SystemColors.ControlText
        DataGridViewCellStyle1.WrapMode = DataGridViewTriState.True
        dgvTracks.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle1
        dgvTracks.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        dgvTracks.Columns.AddRange(New DataGridViewColumn() {nameColumn, startColumn, descColumn, typeColumn})
        DataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle2.BackColor = Color.Beige
        DataGridViewCellStyle2.Font = New Font("Cascadia Code", 12.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(238))
        DataGridViewCellStyle2.ForeColor = Color.Maroon
        DataGridViewCellStyle2.SelectionBackColor = SystemColors.GradientActiveCaption
        DataGridViewCellStyle2.SelectionForeColor = SystemColors.ControlText
        DataGridViewCellStyle2.WrapMode = DataGridViewTriState.False
        dgvTracks.DefaultCellStyle = DataGridViewCellStyle2
        resources.ApplyResources(dgvTracks, "dgvTracks")
        dgvTracks.Name = "dgvTracks"
        dgvTracks.RowHeadersVisible = False
        ' 
        ' nameColumn
        ' 
        resources.ApplyResources(nameColumn, "nameColumn")
        nameColumn.Name = "nameColumn"
        nameColumn.ReadOnly = True
        ' 
        ' startColumn
        ' 
        resources.ApplyResources(startColumn, "startColumn")
        startColumn.Name = "startColumn"
        startColumn.ReadOnly = True
        ' 
        ' descColumn
        ' 
        resources.ApplyResources(descColumn, "descColumn")
        descColumn.Name = "descColumn"
        descColumn.ReadOnly = True
        ' 
        ' typeColumn
        ' 
        typeColumn.DataPropertyName = "TrackType"
        resources.ApplyResources(typeColumn, "typeColumn")
        typeColumn.Name = "typeColumn"
        typeColumn.DefaultCellStyle.BackColor = Color.Beige
        typeColumn.DefaultCellStyle.ForeColor = Color.Maroon


        ' 
        ' frmCrossTrailSelector
        ' 
        resources.ApplyResources(Me, "$this")
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Beige
        Controls.Add(txtInfo)
        Controls.Add(btnOK)
        Controls.Add(dgvTracks)
        Name = "frmCrossTrailSelector"
        CType(dgvTracks, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents label As Label
    Friend WithEvents btnOK As Button
    Friend WithEvents txtInfo As TextBox
    Friend WithEvents dgvTracks As DataGridView
    Friend WithEvents nameColumn As DataGridViewTextBoxColumn
    Friend WithEvents startColumn As DataGridViewTextBoxColumn
    Friend WithEvents descColumn As DataGridViewTextBoxColumn
    Friend WithEvents typeColumn As DataGridViewComboBoxColumn
End Class
