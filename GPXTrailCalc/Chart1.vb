
Imports System.Globalization
Imports System.Runtime.InteropServices
Imports System.Runtime.InteropServices.ComTypes
Imports System.Threading
Imports System.Windows.Forms.DataVisualization.Charting

Partial Class DistanceChart
    Inherits System.Windows.Forms.Form


    ' Vlastnosti pro data
    Private layerStart As DateTime()
    Private Y_Data As Double()
    Private yAxisLabel As String
    Private startDate As Date
    Private endDate As Date
    Private chartForm As Form ' Uloží referenci na formuláø

    ' Konstruktor, který pøijme data
    Public Sub New(layerStart As DateTime(), _Y_data As Double(), yAxisLabel As String, _startDate As Date, _endDate As Date, _CultureInfo As CultureInfo)
        Me.layerStart = layerStart
        Y_Data = _Y_data
        Me.yAxisLabel = yAxisLabel
        startDate = _startDate
        endDate = _endDate
        Thread.CurrentThread.CurrentCulture = _CultureInfo
        InitializeComponent()

    End Sub

    Public Sub New()

    End Sub

    ' Metoda pro výpoèet smìrnice a posunu pøímky metodou nejmenších ètvercù
    Private Function CalculateLinearRegression() As Tuple(Of Double, Double)
        Dim n As Integer = layerStart.Length
        Dim sumX As Double = 0
        Dim sumY As Double = 0
        Dim sumXY As Double = 0
        Dim sumX2 As Double = 0

        For i As Integer = 0 To n - 1
            Dim x As Double = layerStart(i).ToOADate() ' Pøevedení data na èíselný formát
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

    Public Sub Display(text As String)
        Me.Text = text
        Me.Show()
    End Sub

    Private Sub DistanceChart_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' Nastavení rozsahu osy X na základì data
        ' Získání rozmìrù obrazovky
        Dim screenBounds As Rectangle = Screen.PrimaryScreen.Bounds
        Me.Size = New Size(screenBounds.Width / 2, screenBounds.Height / 2)

        Me.chart1.ChartAreas(0).AxisX.Minimum = startDate.ToOADate()
        Me.chart1.ChartAreas(0).AxisX.Maximum = endDate.ToOADate()

        ' Nastavení vlastností pro osu Y
        Me.chart1.ChartAreas(0).AxisY.Title = yAxisLabel

        Dim series1 As New Series() With {
            .Name = "Data Points",
            .ChartType = SeriesChartType.Point,
            .MarkerSize = 10, ' Nastaví velikost bodù na 10 pixelù
            .MarkerStyle = MarkerStyle.Circle,
            .MarkerColor = Color.Chocolate,
            .XValueType = ChartValueType.DateTime
        }

        ' Pøidání dat do série
        For i As Integer = 0 To Y_Data.Length - 1
            'series1.Points.AddXY(layerStart(i), TotalDistances(i))
            series1.Points.AddXY(layerStart(i), Y_Data(i))
        Next

        ' Pøidání série do grafu
        'chart.Series.Add(series1)
        chart1.Series.Add(series1)

        ' Výpoèet lineární regrese
        Dim regression = CalculateLinearRegression()
        Dim slope = regression.Item1
        Dim intercept = regression.Item2

        ' Vytvoøení nové série pro proloženou pøímku
        Dim regressionSeries As New Series() With {
            .Name = "Trend Line",
            .ChartType = SeriesChartType.Line,
            .XValueType = ChartValueType.DateTime,
            .Color = System.Drawing.Color.Red,
            .BorderWidth = 2
        }
        Try


            ' Pøidání dvou bodù do série, které reprezentují pøímku
            Dim xStart As Double = layerStart.First().ToOADate()
            Dim xEnd As Double = layerStart.Last().ToOADate()
            Dim yStart As Double = slope * xStart + intercept
            Dim yEnd As Double = slope * xEnd + intercept

            regressionSeries.Points.AddXY(DateTime.FromOADate(xStart), yStart)
            regressionSeries.Points.AddXY(DateTime.FromOADate(xEnd), yEnd)

            ' Pøidání regresní série do grafu
            chart1.Series.Add(regressionSeries)
        Catch ex As Exception
            Debug.WriteLine("Nepodaøilo se proložit pøímku")
        End Try

    End Sub

    Sub CloseChart()
        If chartForm IsNot Nothing Then
            chartForm.Close()
            chartForm = Nothing ' Uvolní referenci
        End If
    End Sub


    Private Sub SaveAs(sender As Object, e As EventArgs) Handles SaveAsToolStripMenuItem.Click
        Using dialog As New SaveFileDialog()
            dialog.Filter = "PNG (*.png)|*.png|JPEG (*.jpeg)|*.jpeg"
            'dialog.CheckFileExists = True 'když existuje zeptá se 
            dialog.AddExtension = True
            dialog.InitialDirectory = My.Settings.Directory
            dialog.Title = "Save as"
            dialog.FileName = Me.Text.Replace("/", " per ")



            If dialog.ShowDialog() = DialogResult.OK Then


                Debug.WriteLine($"Selected file: {dialog.FileName}")
                'Ulož upravený RTF text zpìt do souboru


                Dim format As ChartImageFormat
                Try
                    Select Case dialog.FilterIndex
                        Case 1
                            format = ChartImageFormat.Png
                        Case 2
                            format = ChartImageFormat.Jpeg
                    End Select
                    Me.chart1.SaveImage(dialog.FileName, format)

                Catch ex As Exception
                    MessageBox.Show($"{My.Resources.Resource1.mBoxErrorCreatingCSV}: {dialog.FileName} " & ex.Message & vbCrLf, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub
End Class

