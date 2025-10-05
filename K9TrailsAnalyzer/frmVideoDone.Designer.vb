<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmVideoDone
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmVideoDone))
        lblInfo = New Label()
        btnOpenFolder = New Button()
        btnPlayVideo = New Button()
        btnClose = New Button()
        SuspendLayout()
        ' 
        ' lblInfo
        ' 
        lblInfo.AutoEllipsis = True
        lblInfo.AutoSize = True
        lblInfo.BackColor = Color.Transparent
        lblInfo.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        lblInfo.Location = New Point(41, 52)
        lblInfo.Name = "lblInfo"
        lblInfo.Size = New Size(74, 28)
        lblInfo.TabIndex = 0
        lblInfo.Text = "lblInfo"
        ' 
        ' btnOpenFolder
        ' 
        btnOpenFolder.AutoSizeMode = AutoSizeMode.GrowAndShrink
        btnOpenFolder.BackColor = Color.Goldenrod
        btnOpenFolder.FlatStyle = FlatStyle.Flat
        btnOpenFolder.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        btnOpenFolder.Location = New Point(193, 334)
        btnOpenFolder.Name = "btnOpenFolder"
        btnOpenFolder.Size = New Size(201, 58)
        btnOpenFolder.TabIndex = 2
        btnOpenFolder.Text = "Open folder "
        btnOpenFolder.UseVisualStyleBackColor = False
        ' 
        ' btnPlayVideo
        ' 
        btnPlayVideo.AutoSizeMode = AutoSizeMode.GrowAndShrink
        btnPlayVideo.BackColor = Color.Goldenrod
        btnPlayVideo.FlatStyle = FlatStyle.Flat
        btnPlayVideo.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        btnPlayVideo.Location = New Point(44, 334)
        btnPlayVideo.Name = "btnPlayVideo"
        btnPlayVideo.Size = New Size(123, 58)
        btnPlayVideo.TabIndex = 1
        btnPlayVideo.Text = "Play "
        btnPlayVideo.UseVisualStyleBackColor = False
        ' 
        ' btnClose
        ' 
        btnClose.AutoSizeMode = AutoSizeMode.GrowAndShrink
        btnClose.BackColor = Color.Goldenrod
        btnClose.FlatStyle = FlatStyle.Flat
        btnClose.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        btnClose.Location = New Point(585, 334)
        btnClose.Name = "btnClose"
        btnClose.Size = New Size(134, 58)
        btnClose.TabIndex = 3
        btnClose.Text = "Close"
        btnClose.UseVisualStyleBackColor = False
        ' 
        ' frmVideoDone
        ' 
        AutoScaleDimensions = New SizeF(12.0F, 28.0F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = Color.Beige
        BackgroundImageLayout = ImageLayout.Zoom
        ClientSize = New Size(731, 404)
        Controls.Add(btnClose)
        Controls.Add(btnPlayVideo)
        Controls.Add(btnOpenFolder)
        Controls.Add(lblInfo)
        Font = New Font("Segoe UI", 10.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        Margin = New Padding(4)
        Name = "frmVideoDone"
        Text = "Overlay video"
        ResumeLayout(False)
        PerformLayout()

    End Sub

    Friend WithEvents lblInfo As Label
    Friend WithEvents btnOpenFolder As Button
    Friend WithEvents btnPlayVideo As Button
    Friend WithEvents btnClose As Button
End Class
