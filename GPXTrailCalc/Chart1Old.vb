﻿Imports System.Globalization
Imports System.Threading
Imports System.Windows.Forms.DataVisualization.Charting

Public Class DistanceChart
    Inherits Form
    ' Vlastnosti pro data
    Private layerStart As DateTime()
    Private Y_Data As Double()
    Private yAxisLabel As String
    Private startDate As Date
    Private endDate As Date
    Private chartForm As Form ' Uloží referenci na formulář

    ' Konstruktor, který přijme data
    Public Sub New(layerStart As DateTime(), _Y_data As Double(), yAxisLabel As String, _startDate As Date, _endDate As Date, _CultureInfo As CultureInfo)
        Me.layerStart = layerStart
        Y_Data = _Y_data
        Me.yAxisLabel = yAxisLabel
        startDate = _startDate
        endDate = _endDate
        Thread.CurrentThread.CurrentCulture = _CultureInfo

    End Sub

    Public Sub New()

    End Sub

    ' Metoda pro výpočet směrnice a posunu přímky metodou nejmenších čtverců
    Private Function CalculateLinearRegression() As Tuple(Of Double, Double)
        Dim n As Integer = layerStart.Length
        Dim sumX As Double = 0
        Dim sumY As Double = 0
        Dim sumXY As Double = 0
        Dim sumX2 As Double = 0

        For i As Integer = 0 To n - 1
            Dim x As Double = layerStart(i).ToOADate() ' Převedení data na číselný formát
            Dim y As Double = Y_Data(i)
            sumX += x
            sumY += y
            sumXY += x * y
            sumX2 += x * x
        Next

        Dim slope As Double = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX)
        Dim intercept As Double = (sumY - slope * sumX) / n

        Return Tuple.Create(slope, intercept)
    End Function

    ' Metoda pro vytvoření a zobrazení grafu
    Dim chart As New Chart()
    Public Sub Display(text As String, position As Integer)

        ' Vytvoření nového formuláře pro zobrazení grafu
        chartForm = New Form()

        chartForm.Text = text
        'chartForm.Size = New Size(1000, 600)
        chartForm.Icon = My.Resources.track2

        ' Získání rozměrů obrazovky
        Dim screenBounds As Rectangle = Screen.PrimaryScreen.Bounds
        chartForm.Size = New Size(screenBounds.Width / 2, screenBounds.Height / 2)

        ' Vytvoření a nastavení komponenty Chart
        'Dim chart As New Chart()
        Chart.Dock = DockStyle.Fill
        chartForm.Controls.Add(chart)

        ' Nastavení oblasti grafu
        Dim chartArea As New ChartArea()
        chartArea.AxisX.Title = My.Resources.Resource1.X_AxisLabel
        chartArea.AxisX.TitleFont = New Font("Arial", 14, FontStyle.Bold) ' Nastavení většího a tučného písma
        'chartArea.AxisX.LabelStyle.Format = "MM.yy"
        'Date label according the currentCulture:
        chartArea.AxisX.LabelStyle.Format = "MMMM yy"
        'chartArea.AxisX.LabelStyle.Format = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.YearMonthPattern
        'chartArea.AxisX.LabelStyle.Format = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern

        chartArea.AxisX.LabelStyle.Font = New Font("Arial", 14, FontStyle.Bold)
        ' Nastavení rozsahu osy X na základě data
        chartArea.AxisX.Minimum = startDate.ToOADate()
        chartArea.AxisX.Maximum = endDate.ToOADate()

        ' Nastavení vlastností pro osu Y
        chartArea.AxisY.Title = yAxisLabel
        chartArea.AxisY.TitleFont = New Font("Arial", 14, FontStyle.Bold) ' Nastavení většího a tučného písma
        chartArea.AxisY.LabelStyle.Font = New Font("Arial", 14, FontStyle.Bold)
        chartArea.AxisY.Minimum = 0
        chartArea.BackColor = Color.FromArgb(237, 240, 213)

        chart.ChartAreas.Add(chartArea)
        chart.BackColor = Color.FromArgb(237, 240, 213)


        Dim series1 As New Series() With {
            .Name = "Data Points",
            .ChartType = SeriesChartType.Point,
            .MarkerSize = 10, ' Nastaví velikost bodů na 10 pixelů
            .MarkerStyle = MarkerStyle.Circle,
            .MarkerColor = Color.Chocolate,
            .XValueType = ChartValueType.DateTime
        }

        ' Přidání dat do série
        For i As Integer = 0 To Y_Data.Length - 1
            'series1.Points.AddXY(layerStart(i), TotalDistances(i))
            series1.Points.AddXY(layerStart(i), Y_Data(i))
        Next

        ' Přidání série do grafu
        'chart.Series.Add(series1)
        chart.Series.Add(series1)

        ' Výpočet lineární regrese
        Dim regression = CalculateLinearRegression()
        Dim slope = regression.Item1
        Dim intercept = regression.Item2

        ' Vytvoření nové série pro proloženou přímku
        Dim regressionSeries As New Series() With {
            .Name = "Trend Line",
            .ChartType = SeriesChartType.Line,
            .XValueType = ChartValueType.DateTime,
            .Color = System.Drawing.Color.Red,
            .BorderWidth = 2
        }
        Try


            ' Přidání dvou bodů do série, které reprezentují přímku
            Dim xStart As Double = layerStart.First().ToOADate()
            Dim xEnd As Double = layerStart.Last().ToOADate()
            Dim yStart As Double = slope * xStart + intercept
            Dim yEnd As Double = slope * xEnd + intercept

            regressionSeries.Points.AddXY(DateTime.FromOADate(xStart), yStart)
            regressionSeries.Points.AddXY(DateTime.FromOADate(xEnd), yEnd)

            ' Přidání regresní série do grafu
            chart.Series.Add(regressionSeries)
        Catch ex As Exception
            Debug.WriteLine("Nepodařilo se proložit přímku")
        End Try

        ' Zobrazení formuláře

        chartForm.Show()

        chartForm.BringToFront()

    End Sub

    Sub CloseChart()
        If chartForm IsNot Nothing Then
            chartForm.Close()
            chartForm = Nothing ' Uvolní referenci
        End If
    End Sub

    Sub SaveAs(imgname As String)
        Me.chart.SaveImage(imgname, format:=ChartImageFormat.Png)
    End Sub

End Class
