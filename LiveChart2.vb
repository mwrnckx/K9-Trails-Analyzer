


'Imports System.Globalization
'Imports System.IO
'Imports System.Threading
'Imports LiveChartsCore
'Imports LiveChartsCore.Defaults
'Imports LiveChartsCore.Measure
'Imports LiveChartsCore.SkiaSharpView
'Imports LiveChartsCore.SkiaSharpView.Painting
'Imports LiveChartsCore.SkiaSharpView.WinForms
'Imports SkiaSharp

'Public Class LiveChart2
'    Inherits Form

'    Private X_Data As DateTime()
'    Private Y_Data As Double()
'    Private yAxisLabel As String
'    Private isIntercept As Boolean
'    Private chartType As String ' "Column" nebo "Point"
'    Private WithEvents cartesianChart As CartesianChart
'    Private StartDate As DateTime
'    Private EndDate As DateTime

'    ' Konstruktor
'    Public Sub New(_X_data As DateTime(), _Y_data As Double(), yAxisLabel As String, _startDate As Date, _endDate As Date, _meText As String, _isIntercept As Boolean, _chartType As String, _CultureInfo As CultureInfo)
'        Me.X_Data = _X_data
'        Me.Y_Data = _Y_data
'        Me.yAxisLabel = yAxisLabel
'        Me.Text = _meText
'        Me.isIntercept = _isIntercept
'        Me.chartType = _chartType
'        Me.StartDate = _startDate
'        Me.EndDate = _endDate


'        Thread.CurrentThread.CurrentCulture = _CultureInfo

'        InitializeComponent()
'    End Sub

'    Private Sub InitializeComponent()
'        Me.cartesianChart = New CartesianChart()
'        Me.cartesianChart.Dock = DockStyle.Fill
'        Me.Controls.Add(Me.cartesianChart)

'        Dim menuStrip As New MenuStrip()
'        Dim saveItem As New ToolStripMenuItem("Save as")
'        AddHandler saveItem.Click, AddressOf SaveChart
'        menuStrip.Items.Add(saveItem)
'        Me.MainMenuStrip = menuStrip
'        Me.Controls.Add(menuStrip)

'        Me.Size = New Size(900, 600)
'    End Sub

'    Protected Overrides Sub OnLoad(e As EventArgs)
'        MyBase.OnLoad(e)

'        ' Převod dat na DateTimePoint
'        Dim points = New List(Of DateTimePoint)
'        For i = 0 To X_Data.Length - 1
'            points.Add(New DateTimePoint(X_Data(i), Y_Data(i)))
'        Next

'        ' Vytvoření série
'        Dim lineSeries = New LineSeries(Of DateTimePoint) With {
'            .Values = points,
'            .GeometrySize = 8,
'            .Stroke = New SolidColorPaint(SKColors.Chocolate, 2),
'            .Fill = Nothing
'        }

'        ' Nastavení os
'        Dim xAxis = New Axis With {
'            .Name = "Datum",
'            .LabelsRotation = -45,
'            .Labeler = Function(value) DateTime.FromOADate(value).ToString("MMM yy"),
'            .MinLimit = startDate.ToOADate(),
'            .MaxLimit = endDate.ToOADate()
'        }

'        Dim yAxis = New Axis With {
'            .Name = yAxisLabel,
'            .MinLimit = 0
'        }

'        ' Vytvoření a přidání CartesianChart
'        cartesianChart = New CartesianChart() With {
'            .Series = {lineSeries},
'            .XAxes = {xAxis},
'            .YAxes = {yAxis},
'            .Dock = DockStyle.Fill
'        }

'        ' Přidání grafu do okna
'        Me.Controls.Add(cartesianChart)



'        '' Popisky osy X
'        'cartesianChart.XAxes = {
'        '    New Axis With {
'        '        .LabelsRotation = -45,
'        '        .Name = "Datum",
'        '        .Labeler = Function(value) New DateTime(CLng(value)).ToString("MMM yy")
'        '    }
'        '}

'        '' Osa Y
'        'cartesianChart.YAxes = {
'        '    New Axis With {
'        '        .Name = yAxisLabel
'        '    }
'        '}

'        'Dim seriesList As New List(Of ISeries)

'        'If chartType = "Column" Then
'        '    seriesList.Add(New ColumnSeries(Of Double) With {
'        '        .Values = Y_Data,
'        '        .Name = "Data",
'        '        .DataLabelsPaint = New SolidColorPaint(SKColors.Black),
'        '        .DataLabelsPosition = DataLabelsPosition.Top,
'        '        .DataLabelsFormatter = Function(point) point.Coordinate.PrimaryValue.ToString("N1"),
'        '        .Fill = New SolidColorPaint(SKColors.Chocolate)
'        '    })
'        'ElseIf chartType = "Point" Then
'        '    seriesList.Add(New ScatterSeries(Of Double) With {
'        '        .Values = Y_Data,
'        '        .Name = "Data",
'        '        .GeometrySize = 10,
'        '        .Fill = New SolidColorPaint(SKColors.Chocolate)
'        '    })

'        '    ' Trendová čára (lineární regrese)
'        '    Dim regressionValues = CalculateRegressionValues()
'        '    seriesList.Add(New LineSeries(Of Double) With {
'        '        .Values = regressionValues,
'        '        .Name = "Trend",
'        '        .Stroke = New SolidColorPaint(SKColors.Red, 2)
'        '    })
'        'End If

'        'cartesianChart.Series = seriesList
'    End Sub

'    Private Function CalculateRegressionValues() As List(Of Double)
'        ' Pro jednoduchost: X jsou indexy 0,1,2,... 
'        Dim n = Y_Data.Length
'        Dim xMean = Enumerable.Range(0, n).Average()
'        Dim yMean = Y_Data.Average()
'        Dim numerator = 0.0
'        Dim denominator = 0.0

'        For i = 0 To n - 1
'            numerator += (i - xMean) * (Y_Data(i) - yMean)
'            denominator += (i - xMean) ^ 2
'        Next

'        Dim slope = numerator / denominator
'        Dim intercept = yMean - slope * xMean

'        ' Vypočteme hodnoty trendu pro všechny indexy
'        Dim regressionValues As New List(Of Double)
'        For i = 0 To n - 1
'            regressionValues.Add(slope * i + intercept)
'        Next
'        Return regressionValues
'    End Function

'    Private Sub SaveChart(sender As Object, e As EventArgs)
'        Using dialog As New SaveFileDialog()
'            dialog.Filter = "PNG (*.png)|*.png"
'            dialog.FileName = Me.Text.Replace("/", "_")
'            If dialog.ShowDialog() = DialogResult.OK Then
'                ' Vytvoř bitmapu ve velikosti grafu
'                Using bmp As New Bitmap(cartesianChart.Width, cartesianChart.Height)
'                    ' Nech kontrolku se vykreslit do bitmapy
'                    cartesianChart.DrawToBitmap(bmp, New Rectangle(0, 0, bmp.Width, bmp.Height))
'                    ' Ulož do PNG
'                    bmp.Save(dialog.FileName, Imaging.ImageFormat.Png)
'                End Using
'            End If
'        End Using
'    End Sub

'End Class


