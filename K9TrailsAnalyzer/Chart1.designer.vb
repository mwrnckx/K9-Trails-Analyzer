Imports System.Globalization
Imports System.Runtime.InteropServices.ComTypes
Imports System.Threading
Imports System.Windows.Forms.DataVisualization.Charting


<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class DistanceChart
    Inherits Form

    ' Designer-generated code for initializing components
    Private components As System.ComponentModel.IContainer

    ' Dispose method to clean up resources
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    ' Required by the Windows Form Designer
    Private Sub InitializeComponent()
        Dim ChartArea1 As System.Windows.Forms.DataVisualization.Charting.ChartArea = New System.Windows.Forms.DataVisualization.Charting.ChartArea()
        Dim title1 As Title = New Title(My.Settings.DogName)
        Me.chart1 = New System.Windows.Forms.DataVisualization.Charting.Chart()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.SaveAsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        CType(Me.chart1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.MenuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'chart1
        '
        Me.chart1.BackColor = System.Drawing.Color.FromArgb(CType(CType(237, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(213, Byte), Integer))
        ChartArea1.AxisX.LabelStyle.Font = New System.Drawing.Font("Cascadia Code Semibold", 12.0!)

        ChartArea1.AxisX.Title = "" 'Global.GPXTrailAnalyzer.My.Resources.Resource1.X_AxisLabel
        ChartArea1.AxisX.TitleFont = New System.Drawing.Font("Cascadia Code Semibold", 12.0!)
        ChartArea1.AxisY.LabelStyle.Font = New System.Drawing.Font("Cascadia Code Semibold", 12.0!)
        ChartArea1.AxisY.Minimum = 0R
        ChartArea1.AxisY.TitleFont = New System.Drawing.Font("Cascadia Code Semibold", 12.0!)
        ChartArea1.BackColor = System.Drawing.Color.FromArgb(CType(CType(237, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(213, Byte), Integer))
        ChartArea1.Name = "ChartArea1"
        Me.chart1.ChartAreas.Add(ChartArea1)
        title1.Font = New System.Drawing.Font("Cascadia Code Semibold", 12.0!)
        Me.chart1.Titles.Add(title1)
        Me.chart1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.chart1.Location = New System.Drawing.Point(0, 33)
        Me.chart1.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.chart1.Name = "chart1"
        Me.chart1.Size = New System.Drawing.Size(900, 529)
        Me.chart1.TabIndex = 0
        '
        'MenuStrip1
        '
        Me.MenuStrip1.BackColor = System.Drawing.Color.FromArgb(CType(CType(237, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(213, Byte), Integer))
        Me.MenuStrip1.GripMargin = New System.Windows.Forms.Padding(2, 2, 0, 2)
        Me.MenuStrip1.ImageScalingSize = New System.Drawing.Size(24, 24)
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SaveAsToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(900, 33)
        Me.MenuStrip1.TabIndex = 1
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'SaveAsToolStripMenuItem
        '
        Me.SaveAsToolStripMenuItem.Font = New System.Drawing.Font("Segoe UI Semibold", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(238, Byte))
        Me.SaveAsToolStripMenuItem.Name = "SaveAsToolStripMenuItem"
        Me.SaveAsToolStripMenuItem.Size = New System.Drawing.Size(88, 29)
        Me.SaveAsToolStripMenuItem.Text = "Save as"
        '
        'DistanceChart
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.AutoSize = True
        Me.ClientSize = New System.Drawing.Size(900, 562)
        Me.Controls.Add(Me.chart1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.Icon = Global.GPXTrailAnalyzer.My.Resources.Resources.track2
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.Name = "DistanceChart"
        CType(Me.chart1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

    ' Declare the Chart control
    Friend WithEvents chart1 As System.Windows.Forms.DataVisualization.Charting.Chart
    Friend WithEvents chartArea1 As ChartArea
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents SaveAsToolStripMenuItem As ToolStripMenuItem
End Class

