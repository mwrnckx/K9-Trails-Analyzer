Imports System.Globalization
Imports System.Runtime.InteropServices.ComTypes
Imports System.Threading
Imports System.Windows.Forms.DataVisualization.Charting


<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmChart
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
        Dim ChartArea2 As ChartArea = New ChartArea()
        Dim Title2 As Title = New Title()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmChart))
        chart1 = New Chart()
        MenuStrip1 = New MenuStrip()
        SaveAsToolStripMenuItem = New ToolStripMenuItem()
        CType(chart1, ComponentModel.ISupportInitialize).BeginInit()
        MenuStrip1.SuspendLayout()
        SuspendLayout()
        ' 
        ' chart1
        ' 
        chart1.BackColor = color.lightyellow
        ChartArea2.AxisX.LabelStyle.Font = New Font("Cascadia Code SemiBold", 12F)
        ChartArea2.AxisX.TitleFont = New Font("Cascadia Code SemiBold", 12F)
        ChartArea2.AxisY.LabelStyle.Font = New Font("Cascadia Code SemiBold", 12F)
        ChartArea2.AxisY.Minimum = 0R
        ChartArea2.AxisY.TitleFont = New Font("Cascadia Code SemiBold", 12F)
        ChartArea2.BackColor = color.lightyellow
        ChartArea2.Name = "ChartArea1"
        chart1.ChartAreas.Add(ChartArea2)
        chart1.Dock = DockStyle.Fill
        chart1.Location = New Point(0, 24)
        chart1.Margin = New Padding(2, 3, 2, 3)
        chart1.Name = "chart1"
        chart1.Size = New Size(700, 454)
        chart1.TabIndex = 0
        Title2.Font = New Font("Cascadia Code SemiBold", 14.0F)
        Title2.Name = "Title1"
        chart1.Titles.Add(Title2)
        ' 
        ' MenuStrip1
        ' 
        MenuStrip1.BackColor = color.lightyellow
        MenuStrip1.ImageScalingSize = New Size(24, 24)
        MenuStrip1.Items.AddRange(New ToolStripItem() {SaveAsToolStripMenuItem})
        MenuStrip1.Location = New Point(0, 0)
        MenuStrip1.Name = "MenuStrip1"
        MenuStrip1.Padding = New Padding(5, 2, 0, 2)
        MenuStrip1.Size = New Size(700, 24)
        MenuStrip1.TabIndex = 1
        MenuStrip1.Text = "MenuStrip1"
        ' 
        ' SaveAsToolStripMenuItem
        ' 
        SaveAsToolStripMenuItem.Font = New Font("Cascadia Code Semibold", 9.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        SaveAsToolStripMenuItem.Name = "SaveAsToolStripMenuItem"
        SaveAsToolStripMenuItem.Size = New Size(58, 20)
        SaveAsToolStripMenuItem.Text = "Save as"
        ' 
        ' frmChart
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        AutoSize = True
        ClientSize = New Size(700, 478)
        Controls.Add(chart1)
        Controls.Add(MenuStrip1)
        Icon = CType(resources.GetObject("$this.Icon"), Icon)
        MainMenuStrip = MenuStrip1
        Margin = New Padding(2, 3, 2, 3)
        Name = "frmChart"
        CType(chart1, ComponentModel.ISupportInitialize).EndInit()
        MenuStrip1.ResumeLayout(False)
        MenuStrip1.PerformLayout()
        ResumeLayout(False)
        PerformLayout()

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

