
Imports System.Globalization
Imports System.Runtime.InteropServices
Imports System.Runtime.InteropServices.ComTypes
Imports System.Threading
Imports System.Windows.Forms.DataVisualization.Charting

Partial Class DistanceChart
    Inherits System.Windows.Forms.Form


    ' Vlastnosti pro data
    Private X_Data As DateTime()
    Private X_DataString As String()
    Private Y_Data As Double()
    Private yAxisLabel As String
    Private startDate As Date
    Private endDate As Date
    Private isIntercept As Boolean 'typ proložené pøímky
    Private chartType As SeriesChartType 'Typ grafu


    ' Konstruktor, který pøijme data
    Public Sub New(_X_data As DateTime(), _Y_data As Double(), yAxisLabel As String, _startDate As Date, _endDate As Date, _meText As String, _isIntercept As Boolean, _chartType As SeriesChartType, _CultureInfo As CultureInfo)
        Me.X_Data = _X_data
        Me.Y_Data = _Y_data
        Me.yAxisLabel = yAxisLabel
        Me.startDate = _startDate
        Me.endDate = _endDate
        Me.Text = _meText
        Me.isIntercept = _isIntercept
        Me.chartType = _chartType
        Thread.CurrentThread.CurrentCulture = _CultureInfo
        InitializeComponent()

    End Sub
    ' Konstruktor, který pøijme data
    Public Sub New(_X_data As String(), _Y_data As Double(), yAxisLabel As String, _startDate As Date, _endDate As Date, _meText As String, _isIntercept As Boolean, _chartType As SeriesChartType, _CultureInfo As CultureInfo)
        Me.X_DataString = _X_data
        Me.Y_Data = _Y_data
        Me.yAxisLabel = yAxisLabel
        Me.startDate = _startDate
        Me.endDate = _endDate
        Me.Text = _meText
        Me.isIntercept = _isIntercept
        Me.chartType = _chartType
        Thread.CurrentThread.CurrentCulture = _CultureInfo
        InitializeComponent()

    End Sub



    ' Metoda pro výpoèet smìrnice pøímky procházející bodem [X_Data.First().ToOADate(), 0]
    Private Function CalculateLinearRegression(_X_Data() As Date, _Y_data() As Double, _IsIntercept As Boolean) As Tuple(Of Double, Double)
        Dim n As Integer = _X_Data.Length
        If n = 0 Then Return Tuple.Create(0.0, 0.0) ' Ošetøení prázdných dat

        Dim firstX As Double = _X_Data(0).ToOADate() ' První X hodnota (pro posun)
        Dim sumX As Double = 0
        Dim sumY As Double = 0
        Dim sumXY As Double = 0
        Dim sumX2 As Double = 0

        Dim slope As Double
        Dim intercept As Double

        If isIntercept Then ' Standardní lineární regrese (bez posunu)
            For i As Integer = 0 To n - 1
                Dim x As Double = _X_Data(i).ToOADate()
                Dim y As Double = _Y_data(i)
                sumX += x
                sumY += y
                sumXY += x * y
                sumX2 += x * x
            Next

            slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX)
            intercept = (sumY - slope * sumX) / n

        Else ' Lineární regrese s posunutým poèátkem
            For i As Integer = 0 To n - 1
                Dim x As Double = _X_Data(i).ToOADate() - firstX ' Posun X hodnot
                Dim y As Double = _Y_data(i)
                sumX += x
                sumY += y
                sumXY += x * y
                sumX2 += x * x
            Next

            If sumX2 = 0 Then ' Ošetøení dìlení nulou (všechny X hodnoty stejné)
                Return Tuple.Create(0.0, 0.0) ' Nebo vyhoï výjimku, podle potøeby
            End If
            slope = sumXY / sumX2
            intercept = -slope * firstX ' Výpoèet interceptu v pùvodních souøadnicích
        End If

        Return Tuple.Create(slope, intercept)
    End Function


    Private Sub DistanceChart_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' Nastavení rozsahu osy X na základì data
        ' Získání rozmìrù obrazovky
        Dim screenBounds As Rectangle = Screen.PrimaryScreen.Bounds
        Me.Size = New Size(screenBounds.Width / 2, screenBounds.Height / 2)
        Me.chart1.ChartAreas(0).AxisX.IsStartedFromZero = False
        ' Formátování popiskù osy X (ŠIKMÉ POPISKY)
        chart1.ChartAreas(0).AxisX.LabelStyle.IsStaggered = True
        chart1.ChartAreas(0).AxisX.LabelStyle.Angle = -45 ' Nastavení úhlu



        ' Nastavení vlastností pro osu Y
        Me.chart1.ChartAreas(0).AxisY.Title = yAxisLabel

        ' Pokud chceme zobrazit møížku
        chart1.ChartAreas(0).AxisX.MajorGrid.Enabled = True
        chart1.ChartAreas(0).AxisY.MajorGrid.Enabled = True

        'Styl møížky
        chart1.ChartAreas(0).AxisX.MajorGrid.LineColor = Color.LightGray
        chart1.ChartAreas(0).AxisY.MajorGrid.LineColor = Color.LightGray
        chart1.ChartAreas(0).AxisX.MajorGrid.LineWidth = 1
        chart1.ChartAreas(0).AxisY.MajorGrid.LineWidth = 1
        chart1.ChartAreas(0).AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash 'Teèkovaná èára
        chart1.ChartAreas(0).AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash

        Dim series1 As New Series() With {
            .Name = "Series1",
            .ChartType = Me.chartType}

        ' Pøidání dat do série
        If Me.chartType = SeriesChartType.Point Then
            chart1.ChartAreas(0).AxisX.LabelStyle.Format = "MMMM yy"
            With series1
                .MarkerSize = 10 ' Nastaví velikost bodù na 10 pixelù
                .MarkerStyle = MarkerStyle.Circle
                .MarkerColor = Color.Chocolate
                .XValueType = ChartValueType.DateTime
            End With
            Me.chart1.ChartAreas(0).AxisX.Minimum = startDate.ToOADate()
            Me.chart1.ChartAreas(0).AxisX.Maximum = endDate.ToOADate()
            For i As Integer = 0 To Y_Data.Length - 1
                series1.Points.AddXY(X_Data(i), Y_Data(i))
            Next
            'series1.Points.AddXY("Test1", 10)
            'series1.Points.AddXY("Test2", 20)


            ' Výpoèet lineární regrese
            Dim regression = CalculateLinearRegression(X_Data, Y_Data, isIntercept)
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
                Dim xStart As Double = X_Data.First().ToOADate()
                Dim xEnd As Double = X_Data.Last().ToOADate()
                Dim yStart As Double = slope * xStart + intercept
                Dim yEnd As Double = slope * xEnd + intercept

                regressionSeries.Points.AddXY(DateTime.FromOADate(xStart), yStart)
                regressionSeries.Points.AddXY(DateTime.FromOADate(xEnd), yEnd)

                ' Pøidání regresní série do grafu
                chart1.Series.Add(regressionSeries)
            Catch ex As Exception
                Debug.WriteLine("Nepodaøilo se proložit pøímku")
            End Try


        ElseIf Me.chartType = SeriesChartType.Column Then


            series1.Color = Color.Chocolate
        series1.IsValueShownAsLabel = True
            series1.LabelFormat = "N1"
            series1.XValueType = ChartValueType.String
            series1.IsXValueIndexed = True
            series1.XValueType = ChartValueType.String



            For i As Integer = 0 To Y_Data.Length - 1
                series1.Points.AddXY(X_DataString(i), Y_Data(i))
            Next

        End If


        chart1.Series.Clear()
        ' Pøidání série do grafu
        'chart.Series.Add(series1)
        chart1.Series.Add(series1)
        Debug.WriteLine($"Poèet bodù: {series1.Points.Count}")
        Debug.WriteLine($"ChartAreas: {chart1.ChartAreas.Count}, Series: {chart1.Series.Count}")
        Debug.WriteLine($"Nakonec: chart.Series.Count={chart1.Series.Count}, Body={series1.Points.Count}")


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

