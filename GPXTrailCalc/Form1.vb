Imports System.ComponentModel
Imports System.Globalization
Imports System.Reflection
Imports System.Runtime.InteropServices.ComTypes
Imports System.Threading
Imports System.Windows.Forms.DataVisualization.Charting
Imports GPXTrailAnalyzer.My.Resources
Imports System.Linq

Public Class Form1

    Private gpxCalculator As GPXDistanceCalculator
    Private currentCulture As CultureInfo = Thread.CurrentThread.CurrentCulture
    Dim GPXFilesManager As New GpxFileManager()

    Private Sub btnReadGpxFiles_Click(sender As Object, e As EventArgs) Handles btnReadGpxFiles.Click

        txtWarnings.Visible = True
        'zavři případné grafy
        CloseGrafs()
        Dim _GPXFilesManager As New GpxFileManager()
        'interval který se má zpracovat
        _GPXFilesManager.dateFrom = dtpStartDate.Value
        _GPXFilesManager.dateTo = dtpEndDate.Value
        Try
            If _GPXFilesManager.Main() Then
                Me.WriteRTBOutput(_GPXFilesManager)
                Me.GPXFilesManager = _GPXFilesManager
                btnChartDistances.Visible = True
            Else
                MessageBox.Show(My.Resources.Resource1.mBoxDataRetrievalFailed)
            End If
        Catch ex As Exception
            MessageBox.Show(My.Resources.Resource1.mBoxDataRetrievalFailed)
        End Try

        'Try
        '    'send directoryPath to gpxCalculator
        '    If gpxCalculator.ReadAndProcessData(dtpStartDate.Value, dtpEndDate.Value) Then
        '        btnChartDistances.Visible = True
        '    Else
        '        MessageBox.Show(My.Resources.Resource1.mBoxDataRetrievalFailed)
        '    End If
        'Catch ex As Exception
        '    MessageBox.Show(My.Resources.Resource1.mBoxDataRetrievalFailed)
        'End Try
    End Sub

    Private Sub WriteRTBOutput(_gpxFilesManager As GpxFileManager)
        Dim _gpxRecords As List(Of GPXRecord) = _gpxFilesManager.GpxRecords

        Me.rtbOutput.Clear()
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Underline Or FontStyle.Bold) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.DarkGreen ' Nastavit barvu

        Dim manySpaces As String = "                                                 "
        Me.rtbOutput.AppendText((My.Resources.Resource1.outgpxFileName & manySpaces).Substring(0, 33))
        Me.rtbOutput.AppendText((My.Resources.Resource1.X_AxisLabel & manySpaces).Substring(0, 14))
        Me.rtbOutput.AppendText((My.Resources.Resource1.outLength & manySpaces).Substring(0, 12))
        Me.rtbOutput.AppendText((My.Resources.Resource1.outAge & manySpaces).Substring(0, 8))
        Me.rtbOutput.AppendText((My.Resources.Resource1.outSpeed & manySpaces).Substring(0, 20))
        Me.rtbOutput.AppendText(My.Resources.Resource1.outDescription)
        Me.rtbOutput.AppendText(vbCrLf)

        ' Display results
        For Each _gpxRecord As GPXRecord In _gpxRecords
            Try
                Dim fileShortName As String = (IO.Path.GetFileNameWithoutExtension(_gpxRecord.Reader.filePath) & manySpaces).Substring(0, 30)

                ' Nastavení fontu a barvy textu
                Me.rtbOutput.SelectionStart = Me.rtbOutput.Text.Length ' Pozice na konec textu
                Me.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font

                Me.rtbOutput.SelectionColor = Color.Maroon ' Nastavit barvu
                Me.rtbOutput.AppendText(fileShortName & "   ")

                Me.rtbOutput.SelectionColor = Color.DarkGreen ' Nastavit barvu
                Me.rtbOutput.AppendText(_gpxRecord.LayerStart.Date.ToShortDateString & "    ")
                Me.rtbOutput.AppendText(_gpxRecord.Distance.ToString("F2") & " km" & "     ")
                If _gpxRecord.TrailAge.TotalHours > 0 Then
                    Me.rtbOutput.AppendText(_gpxRecord.TrailAge.TotalHours.ToString("F1") & " h" & "   ")
                Else
                    Me.rtbOutput.AppendText("         ")
                End If
                If _gpxRecord.DogSpeed > 0 Then
                    Me.rtbOutput.AppendText(_gpxRecord.DogSpeed.ToString("F1") & " km/h" & "   ")
                Else
                    Me.rtbOutput.AppendText("           ")
                End If
                If Not _gpxRecord.Description = Nothing Then
                    Me.rtbOutput.AppendText(_gpxRecord.Description)
                End If

                If Not _gpxRecord.Link = Nothing Then

                    Me.rtbOutput.AppendText("    Video: ")
                    Me.rtbOutput.SelectionColor = Color.Blue ' Nastavit barvu
                    Me.rtbOutput.AppendText(_gpxRecord.Link)

                End If

                Me.rtbOutput.AppendText(vbCrLf)

                ' Posunutí kurzoru na konec textu
                Me.rtbOutput.SelectionStart = Me.rtbOutput.Text.Length

                ' Skrolování na aktuální pozici kurzoru
                Me.rtbOutput.ScrollToCaret()
            Catch ex As Exception
                MessageBox.Show(My.Resources.Resource1.mBoxDataRetrievalFailed & vbCrLf & "File: " & IO.Path.GetFileNameWithoutExtension(_gpxRecord.Reader.filePath) & vbCrLf & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        Next _gpxRecord


        Dim totalDistance As Double = _gpxRecords(_gpxRecords.Count - 1).TotalDistance
        'Dim AgeAsDouble As List(Of Double) = age.Select(Function(ts) ts.TotalMinutes).ToList()

        ' Nastavení fontu a barvy textu
        Me.rtbOutput.SelectionStart = Me.rtbOutput.Text.Length ' Pozice na konec textu
        Me.rtbOutput.SelectionFont = New Font("Calibri", 10) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Maroon ' Nastavit barvu
        Me.rtbOutput.AppendText(vbCrLf & My.Resources.Resource1.outProcessed_period_from & _gpxFilesManager.dateFrom.ToShortDateString & My.Resources.Resource1.outDo & _gpxFilesManager.dateTo.ToShortDateString &
                vbCrLf & My.Resources.Resource1.outAll_gpx_files_from_directory & _gpxFilesManager.gpxDirectory & vbCrLf & vbCrLf)

        Dim manydots As String = "...................................................................."
        Dim labelLength As Integer = 40

        Me.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Maroon
        Me.rtbOutput.AppendText((My.Resources.Resource1.outTotalNumberOfGPXFiles & manydots).Substring(0, labelLength))

        Me.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Firebrick
        Me.rtbOutput.AppendText(_gpxRecords.Count & vbCrLf)



        Me.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Maroon
        Me.rtbOutput.AppendText((My.Resources.Resource1.outTotalLength & manydots).Substring(0, labelLength))

        Me.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Firebrick
        Me.rtbOutput.AppendText(totalDistance.ToString("F2") & " km" & vbCrLf)
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Maroon
        Me.rtbOutput.AppendText((My.Resources.Resource1.outAverageDistance & manydots).Substring(0, labelLength))
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Firebrick
        Dim averageDistance As Double = GetAverage(Of Double)(_gpxRecords, Function(r) r.Distance)
        Me.rtbOutput.AppendText((1000 * averageDistance).ToString("F0") & " m" & vbCrLf)
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Maroon
        Me.rtbOutput.AppendText((My.Resources.Resource1.outAverageAge & manydots).Substring(0, labelLength))
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Firebrick
        Dim averageTrailAge As Double = GetAverage(Of Double)(_gpxRecords, Function(r) r.TrailAge.TotalHours)
        Me.rtbOutput.AppendText(averageTrailAge.ToString("F1") & " h " & vbCrLf)
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Maroon
        Me.rtbOutput.AppendText((My.Resources.Resource1.outAverageSpeed & manydots).Substring(0, labelLength))
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Firebrick
        Dim averageDogSpeed As Double = GetAverage(Of Double)(_gpxRecords, Function(r) r.DogSpeed)
        Me.rtbOutput.AppendText(averageDogSpeed.ToString("F2") & " km/h")

        ' Posunutí kurzoru na konec textu
        Me.rtbOutput.SelectionStart = Me.rtbOutput.Text.Length

        ' Skrolování na aktuální pozici kurzoru
        Me.rtbOutput.ScrollToCaret()
    End Sub

    Public Function GetAverage(Of T)(gpxRecords As List(Of GPXRecord), selector As Func(Of GPXRecord, T)) As Double
        If gpxRecords IsNot Nothing AndAlso gpxRecords.Any() Then
            ' Ošetření pro typy Integer a Long a Double (můžeš rozšířit pro další typy)
            If GetType(T) = GetType(Integer) OrElse GetType(T) = GetType(Long) Then
                Return gpxRecords.Select(Function(r) Convert.ToDouble(selector(r))).Average()
            ElseIf GetType(T) = GetType(Double) Then
                Return gpxRecords.Select(Function(r) Convert.ToDouble(selector(r))).Average()
            Else
                Throw New ArgumentException("Typ T musí být numerický (Integer, Long, Double).")
            End If
        Else
            Console.WriteLine("List GpxRecords je Nothing nebo prázdný. Nelze vypočítat průměr.")
            Return 0
        End If
    End Function

    Private Function AverageOf(y As List(Of Double)) As Double
        Dim suma As Double = 0
        Dim n As Integer = 0
        For Each number In y
            If number > 0 Then
                suma += number
                n += 1
            End If
        Next
        If n > 0 Then Return suma / n Else Return 0

    End Function


    Private Sub SaveCSVFile(sender As Object, e As EventArgs)
        If gpxCalculator.distances.Count < 1 Then
            MessageBox.Show(My.Resources.Resource1.mBoxMissingData)
            Return
        End If

        Dim FileName As String = "GPX_File_Data_" & Today.ToString("yyyy-MM-dd") 'Path.Combine(directoryPath, "GPX_File_Data_" & Today.ToString("yyyy-MM-dd") & ".csv")

        Using dialog As New SaveFileDialog()
            dialog.Filter = "Soubory csv|*.csv"
            dialog.CheckFileExists = True 'když existuje zeptá se 
            dialog.AddExtension = True
            dialog.InitialDirectory = My.Settings.Directory
            dialog.Title = "Save as CSV"
            dialog.FileName = FileName

            If dialog.ShowDialog() = DialogResult.OK Then

                Debug.WriteLine($"Selected file: {dialog.FileName}")
                Dim csvFilePath As String = dialog.FileName
                Try
                    gpxCalculator.WriteCSVfile(csvFilePath)
                Catch ex As Exception
                    MessageBox.Show($"{My.Resources.Resource1.mBoxErrorCreatingCSV}: {csvFilePath} " & ex.Message & vbCrLf, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using



    End Sub

    Private Sub SaveRtfFile(sender As Object, e As EventArgs) Handles mnuExportAs.Click
        If gpxCalculator.distances.Count < 1 Then
            MessageBox.Show(My.Resources.Resource1.mBoxMissingData)
            Return
        End If

        Dim FileName As String = "GPX_File_Data_" & Today.ToString("yyyy-MM-dd") 'Path.Combine(directoryPath, "GPX_File_Data_" & Today.ToString("yyyy-MM-dd") & ".csv")

        Using dialog As New SaveFileDialog()
            dialog.Filter = "Rich Text Format (*.rtf)|*.rtf|Text (*.txt)|*.txt|Comma-separated values (*.csv)|*.csv"
            'dialog.CheckFileExists = True 'když existuje zeptá se 
            dialog.AddExtension = True
            dialog.InitialDirectory = My.Settings.Directory
            dialog.Title = "Save as"
            dialog.FileName = FileName

            ' Načti obsah RichTextBoxu jako RTF text
            Dim rtfText As String = rtbOutput.Rtf

            If dialog.ShowDialog() = DialogResult.OK Then


                Debug.WriteLine($"Selected file: {dialog.FileName}")
                'Ulož upravený RTF text zpět do souboru



                Try
                    Select Case dialog.FilterIndex
                        Case 1
                            rtbOutput.SaveFile(dialog.FileName, RichTextBoxStreamType.RichText)

                        Case 2
                            rtbOutput.SaveFile(dialog.FileName, RichTextBoxStreamType.PlainText)
                        Case 3
                            gpxCalculator.WriteCSVfile(dialog.FileName)
                    End Select


                Catch ex As Exception
                    MessageBox.Show($"{My.Resources.Resource1.mBoxErrorCreatingCSV}: {dialog.FileName} " & ex.Message & vbCrLf, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using



    End Sub

    Private distanceCharts As New List(Of DistanceChart)
    Private Sub btnOpenChart(sender As Object, e As EventArgs) Handles btnChartDistances.Click

        If gpxCalculator.distances Is Nothing Then
            MessageBox.Show("First you need to read the data from the gpx files!")
            Return
        End If

        'what to display
        Dim yAxisData() As Double
        Dim yAxisLabel As String
        Dim xAxisData As Date()
        Dim GrafText As String = Resource1.Y_AxisLabelSpeed

        Dim distanceChart1 As New DistanceChart
        Dim gpxrecords As List(Of GPXRecord) = GPXFilesManager.GpxRecords
        ''Speed
        ''    ' Načtení y-hodnot a filtrování hodnot, kde je y nulové
        'If GPXFilesManager.GpxRecords IsNot Nothing AndAlso GPXFilesManager.GpxRecords.Any() Then
        '    ' Použití LINQ pro filtrování a projekci
        '    Dim filteredData = GPXFilesManager.GpxRecords.
        'Where(Function(record) record.DogSpeed <> 0). ' Filtrování podle speed <> 0
        'Select(Function(record) New With {.X = record.LayerStart, .Y = record.DogSpeed}) ' Projekce do anonymního typu

        '    ' Kontrola, zda po filtrování zbyly nějaké data
        '    If filteredData.Any() Then
        '        xAxisData = filteredData.Select(Function(item) item.X).ToArray()
        '        yAxisData = filteredData.Select(Function(item) item.Y).ToArray()
        '    Else
        '        Console.WriteLine("Po filtrování nezůstala žádná data. Graf nebude zobrazen.")
        '        xAxisData = New DateTime() {} 'Prázdné pole pro zamezení chyb
        '        yAxisData = New Double() {} 'Prázdné pole pro zamezení chyb
        '    End If
        'Else
        '    Console.WriteLine("List gpxRecords je Nothing nebo prázdný. Graf nebude zobrazen.")
        '    xAxisData = New DateTime() {} 'Prázdné pole pro zamezení chyb
        '    yAxisData = New Double() {} 'Prázdné pole pro zamezení chyb
        'End If

        ' Získání dat pro graf rychlosti
        Dim speedData As Tuple(Of DateTime(), Double()) = GetGraphData(Of Double)(gpxrecords, "DogSpeed")
        xAxisData = speedData.Item1
        yAxisData = speedData.Item2
        yAxisLabel = My.Resources.Resource1.Y_AxisLabelSpeed
        GrafText = Resource1.Y_AxisLabelSpeed
        distanceChart1 = New DistanceChart(xAxisData, yAxisData, yAxisLabel, dtpStartDate.Value, dtpEndDate.Value, currentCulture)
        distanceChart1.Display(GrafText)
        distanceCharts.Add(distanceChart1)



        ' Získání dat pro graf věku trasy
        Dim trailAgeData As Tuple(Of DateTime(), TimeSpan()) = GetGraphData(Of TimeSpan)(gpxrecords, "TrailAge")
        Dim xAxisTrailAge As DateTime() = trailAgeData.Item1
        Dim yAxisTrailAge As TimeSpan() = trailAgeData.Item2
        yAxisLabel = My.Resources.Resource1.Y_AxisLabelAge
        GrafText = Resource1.Y_AxisLabelAge
        distanceChart1 = New DistanceChart(xAxisData, yAxisData, yAxisLabel, dtpStartDate.Value, dtpEndDate.Value, currentCulture)
        distanceChart1.Display(GrafText)
        distanceCharts.Add(distanceChart1)



        '    'Age
        '    ' Filtrování y-hodnot (TotalHours) a x-hodnot (časové značky) pro body, kde TotalHours není nulová
        '    yAxisData = gpxCalculator.age.
        'Where(Function(ts, index) ts.TotalHours <> 0). ' Podmínka pro filtrování TotalHours == 0
        'Select(Function(ts) ts.TotalHours).
        'ToArray()
        '    ' Filtrování x-hodnot (časové značky) podle stejných indexů jako yAxisData
        '    xAxisData = gpxCalculator.layerStart.
        'Where(Function(ts, index) gpxCalculator.age(index).TotalHours <> 0).
        'Select(Function(ts) ts).
        'ToArray()
        '    yAxisLabel = My.Resources.Resource1.Y_AxisLabelAge
        '    GrafText = Resource1.Y_AxisLabelAge
        '    distanceChart1 = New DistanceChart(xAxisData, yAxisData, yAxisLabel, dtpStartDate.Value, dtpEndDate.Value, currentCulture)
        '    distanceChart1.Display(GrafText)
        '    distanceCharts.Add(distanceChart1)



        'Distances
        ' Získání dat pro graf vzdálenosti
        Dim distanceData As Tuple(Of DateTime(), Double()) = GetGraphData(Of Double)(gpxrecords, "Distance")
        xAxisData = distanceData.Item1
        yAxisData = distanceData.Item2
        yAxisLabel = My.Resources.Resource1.Y_AxisLabelLength
        GrafText = Resource1.Y_AxisLabelLength
        distanceChart1 = New DistanceChart(xAxisData, yAxisData, yAxisLabel, dtpStartDate.Value, dtpEndDate.Value, currentCulture)
        distanceChart1.Display(GrafText)
        distanceCharts.Add(distanceChart1)

        'TotDistance
        Dim totTistanceData As Tuple(Of DateTime(), Double()) = GetGraphData(Of Double)(gpxrecords, "TotalDistance")
        xAxisData = distanceData.Item1
        yAxisData = distanceData.Item2
        yAxisLabel = My.Resources.Resource1.Y_AxisLabelTotalLength
        GrafText = Resource1.Y_AxisLabelTotalLength
        distanceChart1 = New DistanceChart(xAxisData, yAxisData, yAxisLabel, dtpStartDate.Value, dtpEndDate.Value, currentCulture)
        distanceChart1.Display(GrafText)
        distanceCharts.Add(distanceChart1)


        'Me.BringToFront()

    End Sub

    Public Function GetGraphData(Of T)(gpxRecords As List(Of GPXRecord), propertyName As String) As Tuple(Of DateTime(), T())
        If gpxRecords IsNot Nothing AndAlso gpxRecords.Any() Then
            Dim propertyInfo As PropertyInfo = GetType(GPXRecord).GetProperty(propertyName)

            If propertyInfo IsNot Nothing Then
                Dim filteredData = gpxRecords.
                Where(Function(record)
                          Dim propertyValue = propertyInfo.GetValue(record)
                          If propertyValue IsNot Nothing Then
                              If GetType(T) = GetType(TimeSpan) Then
                                  Return DirectCast(propertyValue, TimeSpan).TotalHours <> 0 ' Přímé porovnání TotalHours s 0
                              ElseIf GetType(T) = GetType(Double) Then
                                  Return CDbl(propertyValue) <> 0
                              ElseIf GetType(T) = GetType(Integer) Then
                                  Return CInt(propertyValue) <> 0
                              ElseIf GetType(T) = GetType(Long) Then
                                  Return CLng(propertyValue) <> 0
                              ElseIf GetType(T) = GetType(Single) Then
                                  Return CSng(propertyValue) <> 0
                              Else
                                  Throw New ArgumentException($"Typ T musí být numerický (Double, Integer, Long, Single).")
                              End If
                          Else
                              Return False ' Ošetření pro null hodnoty
                          End If
                      End Function).
                Select(Function(record) New With {.X = record.LayerStart, .Y = DirectCast(propertyInfo.GetValue(record), T)})

                If filteredData.Any() Then
                    Return New Tuple(Of DateTime(), T())(filteredData.Select(Function(item) item.X).ToArray(), filteredData.Select(Function(item) item.Y).ToArray())
                Else
                    Console.WriteLine($"Po filtrování pro vlastnost '{propertyName}' nezůstala žádná data. Graf nebude zobrazen.")
                    Return New Tuple(Of DateTime(), T())(New DateTime() {}, New T() {}) ' Prázdná pole
                End If
            Else
                Throw New ArgumentException($"Vlastnost '{propertyName}' neexistuje ve třídě GPXRecord.")
            End If
        Else
            Console.WriteLine("List gpxRecords je Nothing nebo prázdný. Graf nebude zobrazen.")
            Return New Tuple(Of DateTime(), T())(New DateTime() {}, New T() {}) ' Prázdná pole
        End If
    End Function

    Public Sub CloseGrafs()
        ' Zavření grafů
        For Each grf In distanceCharts
            grf.CloseChart()
        Next grf

        ' Vyprázdnění seznamu
        distanceCharts.Clear()
    End Sub





    Public Sub ChangeLanguage(sender As Object, e As EventArgs) Handles mnuCzech.Click, mnuGerman.Click, mnuRussian.Click, mnuUkrainian.Click, mnuPolish.Click, mnuEnglish.Click
        SuspendLayout()
        Dim cultureName As String = sender.Tag
        Thread.CurrentThread.CurrentUICulture = New CultureInfo(cultureName)
        'Thread.CurrentThread.CurrentCulture = New CultureInfo(cultureName)

        currentCulture = Thread.CurrentThread.CurrentUICulture

        Dim resources = New ComponentResourceManager([GetType]())
        resources.ApplyResources(Me, "$this")
        For Each ctrl As Control In Controls
            resources.ApplyResources(ctrl, ctrl.Name)
        Next

        ' Lokalizace položek MenuStrip

        LocalizeMenuItems(MenuStrip1.Items, resources)

        SetTooltips()

        ReadHelp()

        ResumeLayout()
    End Sub

    Private toolTipLabel As Label

    Private Sub ShowLabelToolTip(item As ToolStripMenuItem, toolTipText As String)
        If toolTipLabel IsNot Nothing Then toolTipLabel.Dispose()
        toolTipLabel = New Label With {
        .Text = toolTipText,
        .BackColor = Color.LightYellow,
        .AutoSize = True,
        .MaximumSize = New Size(300, 0), ' Maximální šířka
        .BorderStyle = BorderStyle.FixedSingle
    }
        Controls.Add(toolTipLabel)

        Dim itemBounds = item.GetCurrentParent.Bounds


        toolTipLabel.Location = PointToClient(New Point(Cursor.Position.X + 20, Cursor.Position.Y + 30))
        toolTipLabel.BringToFront()
    End Sub

    Private Sub HideLabelToolTip()
        If toolTipLabel IsNot Nothing Then
            toolTipLabel.Dispose()
            toolTipLabel = Nothing
        End If
    End Sub


    Private Sub LocalizeMenuItems(items As ToolStripItemCollection, resources As ComponentResourceManager)
        For Each item As ToolStripItem In items
            ' Zkus lokalizovat text aktuální položky
            resources.ApplyResources(item, item.Name)

            ' Pokud má položka podmenu, projdi i jeho položky
            If TypeOf item Is ToolStripMenuItem Then
                Dim menuItem As ToolStripMenuItem = DirectCast(item, ToolStripMenuItem)
                If menuItem.DropDownItems.Count > 0 Then
                    LocalizeMenuItems(menuItem.DropDownItems, resources)
                End If
            End If
        Next
        Dim currentCulture As String = System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName
        Dim menuIcon As Image = Nothing
        Select Case currentCulture
            Case "cs-CZ", "cs"
                menuIcon = My.Resources.czech_flag
            Case "en-GB", "en", "en-US"
                menuIcon = My.Resources.en_flag
                mnuEnglish.Image = resizeImage(My.Resources.en_flag, Nothing, 18)
            Case "de-DE", "de"
                menuIcon = My.Resources.De_Flag
                mnuGerman.Image = resizeImage(My.Resources.De_Flag, Nothing, 18)
            Case "pl-PL", "pl"
                menuIcon = My.Resources.pl_flag
                mnuPolish.Image = resizeImage(My.Resources.pl_flag, Nothing, 18)
            Case "ru-RU", "ru"
                menuIcon = My.Resources.ru_flag
                mnuRussian.Image = resizeImage(My.Resources.ru_flag, Nothing, 18)
            Case "uk"
                menuIcon = My.Resources.uk_flag
                mnuUkrainian.Image = resizeImage(My.Resources.uk_flag, Nothing, 18)
            Case Else
                ' Výchozí obrázek (např. angličtina)
                menuIcon = My.Resources.en_flag
        End Select

        ' Nastavení obrázku na ToolStripMenuItem
        mnuLanguage.Image = resizeImage(menuIcon, Nothing, 18)
        mnuCzech.Image = resizeImage(My.Resources.czech_flag, Nothing, 18)
    End Sub

    Private Function resizeImage(menuIcon As Image, width As Integer, height As Integer) As Image

        If width = Nothing Then width = menuIcon.Width * height / menuIcon.Height
        Dim resizedImage As New Bitmap(width, height)
        Using g As Graphics = Graphics.FromImage(resizedImage)
            g.DrawImage(menuIcon, 0, 0, width, height)
        End Using
        Return resizedImage
    End Function




    Private Sub SetTooltips()



        ' Nastavení ToolTip pro jednotlivé ovládací prvky

        mnuSelect_directory_gpx_files.ToolTipText = Resource1.Tooltip_mnuDirectory
        mnuSelectBackupDirectory.ToolTipText = Resource1.Tooltip_mnuBackupDirectory
        mnuExportAs.ToolTipText = Resource1.Tooltip_ExportAs
        mnuPrependDateToFileName.ToolTipText = Resource1.Tooltip_mnuPrependDate
        'mnuTrimGPSNoise.ToolTipText = Resource1.Tooltip_mnuTrim

        AddHandler mnuTrimGPSNoise.MouseEnter, Sub() ShowLabelToolTip(mnuTrimGPSNoise, Resource1.Tooltip_mnuTrim)
        AddHandler mnuTrimGPSNoise.MouseLeave, Sub() HideLabelToolTip()
        AddHandler mnuMergingTracks.MouseEnter, Sub() ShowLabelToolTip(mnuMergingTracks, Resource1.Tooltip_mnuMergingTracks)
        AddHandler mnuMergingTracks.MouseLeave, Sub() HideLabelToolTip()


        ' Nastavení ToolTip pro jednotlivé ovládací prvky

        'ToolTip1.SetToolTip(btnChartDistances, Resource1.Tooltip_dtpStart)
        ToolTip1.SetToolTip(dtpStartDate, Resource1.Tooltip_dtpStart)
        ToolTip1.SetToolTip(dtpEndDate, Resource1.Tooltip_dtpEnd)


        ' Přidej další ovládací prvky, jak je potřeba

        ' Nastavení formátu dtp podle aktuální kultury
        dtpStartDate.CustomFormat = $"'{My.Resources.Resource1.lblFrom}'  {Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern}"
        dtpEndDate.CustomFormat = $"'{My.Resources.Resource1.lblTo}'   {Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern}"

    End Sub


    Private Sub chbTrimGpxFile(sender As Object, e As EventArgs) Handles mnuTrimGPSNoise.CheckedChanged
        My.Settings.TrimGPSnoise = mnuTrimGPSNoise.Checked
    End Sub


    Private Sub chbDateToName_CheckedChanged(sender As Object, e As EventArgs) Handles mnuPrependDateToFileName.CheckedChanged
        My.Settings.PrependDateToName = mnuPrependDateToFileName.Checked
    End Sub



    Private Sub mnuSelect_directory_gpx_files_Click(sender As Object, e As EventArgs) Handles mnuSelect_directory_gpx_files.Click, mnuSelectBackupDirectory.Click
        Dim folderDialog As New FolderBrowserDialog


        If sender Is mnuSelect_directory_gpx_files Then
            folderDialog.SelectedPath = My.Settings.Directory
        ElseIf sender Is mnuSelectBackupDirectory Then
            folderDialog.ShowNewFolderButton = True
            folderDialog.SelectedPath = My.Settings.BackupDirectory
        End If



        If folderDialog.ShowDialog() = DialogResult.OK Then

            If sender Is mnuSelect_directory_gpx_files Then
                My.Settings.Directory = folderDialog.SelectedPath
            ElseIf sender Is mnuSelectBackupDirectory Then
                My.Settings.BackupDirectory = folderDialog.SelectedPath
            End If

        End If

        My.Settings.Save()
        StatusLabel1.Text = $"Directory: {ZkratCestu(My.Settings.Directory, 130)}" & vbCrLf & $"Backup Directory: {ZkratCestu(My.Settings.BackupDirectory, 130)}"

    End Sub


    Private Function ZkratCestu(cesta As String, maxDelka As Integer) As String
        ' Pokud je cesta krátká, není třeba ji upravovat
        If cesta.Length <= maxDelka Or maxDelka < 9 Then
            Return cesta
        End If

        ' Počet znaků, které ponecháme na začátku a na konci
        Dim pocetZnakuNaKazdeStrane As Integer = (maxDelka - 7) \ 2

        ' Vytvoříme zkrácenou cestu
        Dim zacatek As String = cesta.Substring(0, pocetZnakuNaKazdeStrane)
        Dim konec As String = cesta.Substring(cesta.Length - pocetZnakuNaKazdeStrane)

        Return zacatek & "  ...  " & konec
    End Function

    Private Sub mnuMergingTracks_Click(sender As Object, e As EventArgs) Handles mnuMergingTracks.Click
        Dim message, title, defaultValue As String
        Dim myValue As Object
        ' Set prompt.
        message = My.Resources.Resource1.Tooltip_mnuMergingTracks '"Set the maximum time difference (i.e. age of trails in hours) to identify related GPX tracks for automatic merging i.e. tracks of a trail-layer (runner) and the dog. A value of 0 disables automatic merging."
        ' Set title.
        title = My.Resources.Resource1.mBoxMergingTracksText
        defaultValue = My.Settings.maxAge   ' Set default value.

        ' Display message, title, and default value.
        myValue = InputBox(message, title, defaultValue)
        ' If user has clicked Cancel, set myValue to 0
        If myValue Is "" Then myValue = 0
        My.Settings.maxAge = myValue
        My.Settings.Save()

    End Sub

    Private Sub FactoryResetToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FactoryResetToolStripMenuItem.Click
        If MessageBox.Show("Are you sure you want to clear all your settings?", " ",
        MessageBoxButtons.YesNo) = DialogResult.Yes Then My.Settings.Reset()

    End Sub
End Class


