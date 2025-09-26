Imports System.ComponentModel
Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Text
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Threading
Imports System.Windows.Forms.DataVisualization.Charting
Imports GPXTrailAnalyzer.My.Resources
Imports Microsoft.VisualBasic.Logging
Imports TrackVideoExporter.TrackVideoExporter



Public Class Form1
    Private currentCulture As CultureInfo = Thread.CurrentThread.CurrentCulture
    Private GPXFilesManager As GpxFileManager
    Private ReadOnly DogsFilePath As String = Path.Combine(Application.StartupPath, "AppData", "dogs.json")
    Private ReadOnly ConfigPath As String = Path.Combine(Application.StartupPath, "AppData", "config.json")
    'Private ReadOnly DogsRootPath As String = Path.Combine(Application.StartupPath, "Dogs")

    Private DogsList As List(Of DogInfo)
    Private ActiveDogId As String = String.Empty
    Private ReadOnly Property ActiveDog As DogInfo
        Get
            Return DogsList.FirstOrDefault(Function(d) d.Id = ActiveDogId)
        End Get
    End Property

    Private Async Sub btnReadGpxFiles_Click(sender As Object, e As EventArgs) Handles btnReadGpxFiles.Click

        Enabled = False

        Dim gpxDir = ActiveDog.RemoteDirectory 'My.Settings.Directory
        If String.IsNullOrWhiteSpace(gpxDir) OrElse Not Directory.Exists(gpxDir) Then
            ' Cesta není nastavená nebo složka neexistuje → použij výchozí složku Samples vedle exe
            Dim defaultDir = Path.Combine(Application.StartupPath, "Samples")

            If Directory.Exists(defaultDir) Then
                gpxDir = defaultDir
                ActiveDog.RemoteDirectory = gpxDir
                'My.Settings.Save()
            Else
                ' Můžeš nabídnout dialog, nebo nastavit nějaké jiné výchozí chování
                mnuSelect_directory_gpx_files_Click(btnReadGpxFiles, New EventArgs)

            End If
        End If

        CreateGpxFileManager() 'smaže vše ve staré instanci a vytvoří novou

        rtbWarnings.Visible = True
        'zavři případné grafy
        CloseGrafs()

        'období, které se má zpracovat
        GPXFilesManager.dateFrom = dtpStartDate.Value.Date
        GPXFilesManager.dateTo = dtpEndDate.Value.Date.AddDays(1) 'aby to bylo až do půlnoci

        GPXFilesManager.DogInfo = ActiveDog
        GPXFilesManager.NumberOfDogs = DogsList.Count



        Dim waitForm As New frmPleaseWait("I'm reading GPX files, please stand by...")
        waitForm.Show()
        waitForm.Refresh()


        Try
            If Await GPXFilesManager.Main Then
                Enabled = True
                waitForm.Close()

                WriteRTBOutput(GPXFilesManager)
                'Me.GPXFilesManager = GPXFilesManager
                btnCharts.Visible = True
            Else
                Enabled = True
                waitForm.Close()
                MessageBox.Show(Resource1.mBoxDataRetrievalFailed)

                WriteRTBOutput(GPXFilesManager)
                btnCharts.Visible = False
                Return
            End If
        Catch ex As Exception
            MessageBox.Show(Resource1.mBoxDataRetrievalFailed)
            Return
        End Try

        ' Naplnění ListView s daty
        FillListViewWithGpxRecords()
        Enabled = True
        Me.AcceptButton = Me.btnCharts

    End Sub


    Private Sub FillListViewWithGpxRecords()
        ' Vymažeme předchozí položky
        lvGpxFiles.Items.Clear()

        ' Pro každý záznam přidáme řádek do ListView
        For i As Integer = GPXFilesManager.GpxRecords.Count - 1 To 0 Step -1
            Dim record As GPXRecord = GPXFilesManager.GpxRecords(i)
            Dim item As New ListViewItem(record.FileName) ' první sloupec
            If record.DogStart Is Nothing OrElse record.DogStart.Time = Date.MinValue Then 'Pokud není datum začátku trasy psa, nejde označit
                item.ForeColor = Color.Gray
                item.Font = New Font(lvGpxFiles.Font, FontStyle.Italic)
                item.ToolTipText = "This gpx record doesn't contain dog's track, video cannot be created from it."
            End If
            item.SubItems.Add(record.TrailStart.Time.ToString("yyyy-MM-dd HH:mm")) ' např. datum
            item.SubItems.Add($"{record.TrailDistance:F2} km") ' délka trasy
            item.SubItems.Add($"{record.TrailAge.TotalHours:F1} h") ' věk trasy v hodinách
            item.SubItems.Add($"{record.Tracks.Count}") ' počet tras
            item.Tag = record ' pro pozdější použití (např. vytvoření videa)

            lvGpxFiles.Items.Add(item)
        Next

    End Sub





    Private Sub CreateGpxFileManager()
        ' Zrušení odběru událostí u staré instance (pokud existuje)
        If GPXFilesManager IsNot Nothing Then
            RemoveHandler GPXFilesManager.WarningOccurred, AddressOf WriteRTBWarning
            For Each record In GPXFilesManager.GpxRecords
                RemoveHandler record.WarningOccurred, AddressOf GPXFilesManager.WriteRTBWarning
            Next
            GPXFilesManager = Nothing ' Uvolnění staré instance
        End If

        ' Vytvoření nové instance
        GPXFilesManager = New GpxFileManager()
        GPXFilesManager.ForceProcess = Me.mnuProcessProcessed.Checked
        AddHandler GPXFilesManager.WarningOccurred, AddressOf WriteRTBWarning

    End Sub

    Private Sub WriteRTBWarning(message As String, _color As Color)
        Me.rtbWarnings.SelectionStart = Me.rtbOutput.Text.Length ' Pozice na konec textu
        Me.rtbWarnings.SelectionColor = _color ' Nastavit barvu
        Me.rtbWarnings.AppendText(message & vbCrLf) ' Přidání odřádkování
        Me.rtbWarnings.ScrollToCaret() ' Pozice na konec textu
    End Sub

    Private Sub WriteRTBOutput(_gpxFilesManager As GpxFileManager)
        Dim _gpxRecords As List(Of GPXRecord) = _gpxFilesManager.GpxRecords

        Me.rtbOutput.Clear()
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Underline Or FontStyle.Bold) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.DarkGreen ' Nastavit barvu

        Dim manySpaces As String = "                                                 "
        Me.rtbOutput.AppendText(("    " & My.Resources.Resource1.outgpxFileName & manySpaces).Substring(0, 35))
        Me.rtbOutput.AppendText((My.Resources.Resource1.X_AxisLabel & manySpaces).Substring(0, 12))
        Me.rtbOutput.AppendText((My.Resources.Resource1.outLength & manySpaces).Substring(0, 12))
        Me.rtbOutput.AppendText((My.Resources.Resource1.outAge & manySpaces).Substring(0, 8))
        Me.rtbOutput.AppendText((My.Resources.Resource1.outSpeed & manySpaces).Substring(0, 11))
        Me.rtbOutput.AppendText((My.Resources.Resource1.Y_AxisLabelDeviation & manySpaces).Substring(0, 12))
        Me.rtbOutput.AppendText(My.Resources.Resource1.outDescription)
        Me.rtbOutput.AppendText(vbCrLf)

        ' Display results
        Dim i As Integer = 0
        For Each _gpxRecord As GPXRecord In _gpxRecords
            Try
                Dim fileShortName As String = (IO.Path.GetFileNameWithoutExtension(_gpxRecord.Reader.FilePath) & manySpaces).Substring(0, 30)
                i += 1
                ' Nastavení fontu a barvy textu
                Me.rtbOutput.SelectionStart = Me.rtbOutput.Text.Length ' Pozice na konec textu
                Me.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font

                Me.rtbOutput.SelectionColor = Color.Maroon ' Nastavit barvu
                Me.rtbOutput.AppendText($"{i.ToString("D3")} {fileShortName} ")

                Me.rtbOutput.SelectionColor = Color.DarkGreen ' Nastavit barvu
                Me.rtbOutput.AppendText(_gpxRecord.TrailStart.Time.ToString("dd.MM.yy") & "    ")
                Me.rtbOutput.AppendText(_gpxRecord.TrailDistance.ToString("F2") & " km" & "     ")
                If _gpxRecord.TrailAge.TotalHours > 0 Then
                    Me.rtbOutput.AppendText(_gpxRecord.TrailAge.TotalHours.ToString("F1") & " h" & "   ")
                Else
                    Me.rtbOutput.AppendText("        ")
                End If
                Dim dogspeed As Double = _gpxRecord.DogSpeed
                If _gpxRecord.DogSpeed > 0 Then
                    Me.rtbOutput.AppendText(_gpxRecord.DogSpeed.ToString("F1") & " km/h" & "   ")
                Else
                    Me.rtbOutput.AppendText("           ")
                End If
                If _gpxRecord.dogDeviation > 0 Then
                    Me.rtbOutput.AppendText(_gpxRecord.dogDeviation.ToString("F1") & " m" & "   ")
                Else
                    Me.rtbOutput.AppendText("        ")
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
                MessageBox.Show(My.Resources.Resource1.mBoxDataRetrievalFailed & vbCrLf & "File: " & IO.Path.GetFileNameWithoutExtension(_gpxRecord.Reader.FilePath) & vbCrLf & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        Next _gpxRecord


        'Dim AgeAsDouble As List(Of Double) = age.Select(Function(ts) ts.TotalMinutes).ToList()

        ' Nastavení fontu a barvy textu
        Me.rtbOutput.SelectionStart = Me.rtbOutput.Text.Length ' Pozice na konec textu
        Me.rtbOutput.SelectionFont = New Font("Calibri", 10) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Maroon ' Nastavit barvu
        Me.rtbOutput.AppendText(vbCrLf & My.Resources.Resource1.outProcessed_period_from & _gpxFilesManager.dateFrom.ToShortDateString & My.Resources.Resource1.outDo & _gpxFilesManager.dateTo.ToShortDateString &
                vbCrLf & My.Resources.Resource1.outAll_gpx_files_from_directory & ActiveDog.RemoteDirectory & vbCrLf & vbCrLf)

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
        Dim totalDistance As Double = Me.GPXFilesManager.TotalDistances.Last.totalDistance '_gpxFilesManager.TotalDistance
        Me.rtbOutput.AppendText(totalDistance.ToString("F1") & " km" & vbCrLf)
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Maroon
        Me.rtbOutput.AppendText((My.Resources.Resource1.outAverageDistance & manydots).Substring(0, labelLength))
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Firebrick
        Dim averageDistance As Double = GetAverage(Of Double)(_gpxRecords, Function(r) r.TrailDistance)
        Me.rtbOutput.AppendText((1000 * averageDistance).ToString("F0") & " m" & vbCrLf)
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Maroon
        Me.rtbOutput.AppendText((My.Resources.Resource1.outAverageAge & manydots).Substring(0, labelLength))
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Firebrick
        Dim averageTrailAge As Double = GetAverage(Of TimeSpan?)(_gpxRecords, Function(r) r.TrailAge)
        Me.rtbOutput.AppendText(averageTrailAge.ToString("F2") & " h " & vbCrLf)
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Maroon
        Me.rtbOutput.AppendText((My.Resources.Resource1.outAverageSpeed & manydots).Substring(0, labelLength))
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Firebrick
        Dim averageDogSpeed As Double = GetAverage(_gpxRecords, Function(r) r.DogSpeed, ignoreZeros:=True)
        Me.rtbOutput.AppendText(averageDogSpeed.ToString("F1") & " km/h")

        ' Posunutí kurzoru na konec textu
        Me.rtbOutput.SelectionStart = Me.rtbOutput.Text.Length

        ' Skrolování na aktuální pozici kurzoru
        Me.rtbOutput.ScrollToCaret()
    End Sub


    Public Function GetAverage(Of T)(gpxRecords As List(Of GPXRecord),
                                  selector As Func(Of GPXRecord, T),
                                  Optional ignoreZeros As Boolean = False) As Double

        If gpxRecords Is Nothing OrElse Not gpxRecords.Any() Then
            Debug.WriteLine("List GpxRecords je Nothing nebo prázdný. Nelze vypočítat průměr.")
            Return 0
        End If

        ' Vyber platné hodnoty a převedeme je na Double
        Dim values As IEnumerable(Of Double) = gpxRecords _
        .Select(Function(r) selector(r)) _
        .Where(Function(v) v IsNot Nothing) _
        .Select(Function(v)
                    If TypeOf v Is TimeSpan Then
                        ' Pokud je typ TimeSpan, bezpečně ho převeď na TimeSpan
                        Return CType(Convert.ChangeType(v, GetType(TimeSpan)), TimeSpan).TotalHours
                    Else
                        ' Pro ostatní typy, které Convert.ToDouble umí
                        Return Convert.ToDouble(v)
                    End If
                End Function)

        If ignoreZeros Then
            values = values.Where(Function(v) v <> 0)
        End If

        If Not values.Any() Then
            Debug.WriteLine("Nejsou žádné hodnoty k průměrování.")
            Return 0
        End If

        Return values.Average()
    End Function



    Private Sub SaveCSVFile(sender As Object, e As EventArgs)
        If GPXFilesManager.GpxRecords.Count < 1 Then
            MessageBox.Show(My.Resources.Resource1.mBoxMissingData)
            Return
        End If

        Dim FileName As String = "GPX_File_Data_" & Today.ToString("yyyy-MM-dd") 'Path.Combine(directoryPath, "GPX_File_Data_" & Today.ToString("yyyy-MM-dd") & ".csv")

        Using dialog As New SaveFileDialog()
            dialog.Filter = "Soubory csv|*.csv"
            dialog.CheckFileExists = True 'když existuje zeptá se 
            dialog.AddExtension = True
            dialog.InitialDirectory = My.Settings.VideoDirectory
            dialog.Title = "Save as CSV"
            dialog.FileName = FileName

            If dialog.ShowDialog() = DialogResult.OK Then

                Debug.WriteLine($"Selected file: {dialog.FileName}")
                Dim csvFilePath As String = dialog.FileName
                Try
                    Me.WriteCSVfile(csvFilePath)
                Catch ex As Exception
                    MessageBox.Show($"{My.Resources.Resource1.mBoxErrorCreatingCSV}: {csvFilePath} " & ex.Message & vbCrLf, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using



    End Sub

    Private Sub SaveRtfFile(sender As Object, e As EventArgs) Handles mnuExportAs.Click
        If GPXFilesManager.GpxRecords.Count < 1 Then
            MessageBox.Show(My.Resources.Resource1.mBoxMissingData)
            Return
        End If

        Dim FileName As String = "GPX_File_Data_" & Today.ToString("yyyy-MM-dd") 'Path.Combine(directoryPath, "GPX_File_Data_" & Today.ToString("yyyy-MM-dd") & ".csv")

        Using dialog As New SaveFileDialog()
            dialog.Filter = "Rich Text Format (*.rtf)|*.rtf|Text (*.txt)|*.txt|Comma-separated values (*.csv)|*.csv"
            'dialog.CheckFileExists = True 'když existuje zeptá se 
            dialog.AddExtension = True
            dialog.InitialDirectory = My.Settings.VideoDirectory
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
                            Me.WriteCSVfile(dialog.FileName)
                    End Select


                Catch ex As Exception
                    MessageBox.Show($"{My.Resources.Resource1.mBoxErrorCreatingCSV}: {dialog.FileName} " & ex.Message & vbCrLf, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using
    End Sub

    Public Sub WriteCSVfile(csvFilePath As String)
        Try

            ' Create the CSV file and write headers
            Using writer As New IO.StreamWriter(csvFilePath, False, System.Text.Encoding.UTF8)
                writer.WriteLine("File Name;Date;Age/h;Length/km;speed;Description;Video")

                For Each _gpxRecord As GPXRecord In GPXFilesManager.GpxRecords
                    With _gpxRecord
                        Dim fileName As String = IO.Path.GetFileNameWithoutExtension(.Reader.FilePath)

                        Dim _age As String = ""
                        If .TrailAge > TimeSpan.Zero Then
                            _age = .TrailAge.TotalHours.ToString("F1")
                        End If

                        ' Write each row in the CSV file
                        writer.Write($"{fileName};")
                        writer.Write($"{ .TrailStart.Time.ToString("yyyy-MM-dd")};")
                        writer.Write($"{_age};")
                        writer.Write($"{ .TrailDistance:F2};")
                        If Not .DogSpeed = 0 Then writer.Write($"{ .DogSpeed:F2};") Else writer.Write(";")
                        writer.Write($"{ .Description};")
                        If Not .Link Is Nothing Then
                            writer.WriteLine($"=HYPERTEXTOVÝ.ODKAZ(""{ .Link}"")")
                        End If
                    End With
                    writer.WriteLine()

                Next

                ' Write the total distance at the end of the CSV file
                writer.WriteLine($"Total;;; { GPXFilesManager.TotalDistances.Last.totalDistance:F2}")
            End Using


            WriteRTBWarning($"{vbCrLf}CSV file created: {csvFilePath}.{Environment.NewLine}", Color.DarkGreen)
        Catch ex As Exception
            Me.WriteRTBWarning($"{My.Resources.Resource1.mBoxErrorCreatingCSV}: {ex.Message}{Environment.NewLine}", Color.DarkGreen)
            MessageBox.Show($"Error creating CSV file: {ex.Message}")
        End Try
    End Sub


    Private Charts As New List(Of frmChart)
    Private  Sub btnChartsClick(sender As Object, e As EventArgs) Handles btnCharts.Click
        'zruší předchozí grafy
        CloseGrafs()

        Dim gpxRecords = GPXFilesManager.GpxRecords
        If gpxRecords.Count < 2 Then
            MessageBox.Show("First you need to read the data from the gpx files!")
            Return
        End If

        'what to display

        Dim chart1 As frmChart

        ' Získání dat pro graf rychlosti

        chart1 = New frmChart(ActiveDog.Name, GPXFilesManager.Speeds, Resource1.Y_AxisLabelSpeed, GPXFilesManager.dateFrom, GPXFilesManager.dateTo, Resource1.Y_AxisLabelSpeed, True, SeriesChartType.Point, Me.currentCulture)
        chart1.Show()
        Charts.Add(chart1)



        ' Získání dat pro graf stáří trasy
        chart1 = New frmChart(ActiveDog.Name, GPXFilesManager.Ages, Resource1.Y_AxisLabelAge, GPXFilesManager.dateFrom, GPXFilesManager.dateTo, Resource1.Y_AxisLabelAge, True, SeriesChartType.Point, Me.currentCulture)
        chart1.Show()
        Charts.Add(chart1)




        'Difficulty indexes
        chart1 = New frmChart(ActiveDog.Name, GPXFilesManager.DiffIndexes, "Trail Difficulty Index (h·km)", GPXFilesManager.dateFrom, GPXFilesManager.dateTo, "Trail Difficulty Index (h·km)", True, SeriesChartType.Point, Me.currentCulture)
        chart1.Show()
        Charts.Add(chart1)

        'Total Difficulty indexes

        chart1 = New frmChart(ActiveDog.Name, GPXFilesManager.TotalDiffIndexes, "Total Trail Difficulty Index (h·km)", GPXFilesManager.dateFrom, GPXFilesManager.dateTo, "Total Trail Difficulty Index (h·km)", True, SeriesChartType.Point, Me.currentCulture)
        chart1.Show()
        Charts.Add(chart1)


        'Deviations
        chart1 = New frmChart(ActiveDog.Name, GPXFilesManager.Deviations, Resource1.Y_AxisLabelDeviation, GPXFilesManager.dateFrom, GPXFilesManager.dateTo, Resource1.Y_AxisLabelDeviation, True, SeriesChartType.Point, Me.currentCulture)
        chart1.Show()
        Charts.Add(chart1)



        'Distances
        chart1 = New frmChart(ActiveDog.Name, GPXFilesManager.Distances, Resource1.Y_AxisLabelLength, GPXFilesManager.dateFrom, GPXFilesManager.dateTo, Resource1.Y_AxisLabelLength, True, SeriesChartType.Point, Me.currentCulture)
        chart1.Show()
        Charts.Add(chart1)


        'TotDistance
        chart1 = New frmChart(ActiveDog.Name, Me.GPXFilesManager.TotalDistances, Resource1.Y_AxisLabelTotalLength, GPXFilesManager.dateFrom, GPXFilesManager.dateTo, Resource1.Y_AxisLabelTotalLength, True, SeriesChartType.Point, Me.currentCulture)
        chart1.Show()
        Charts.Add(chart1)




        ' Vygenerujeme seznam všech měsíců v daném období
        Dim allMonths = Enumerable.Range(0, 12 * (GPXFilesManager.dateTo.Year - GPXFilesManager.dateFrom.Year) + (GPXFilesManager.dateTo.Month - GPXFilesManager.dateFrom.Month) + 1).
                    Select(Function(offset) GPXFilesManager.dateFrom.AddMonths(offset)).
                    Select(Function(d) New DateTime(d.Year, d.Month, 1))

        ' Použijeme Left Join pro zahrnutí všech měsíců, i těch bez dat a použijeme trailStart
        Dim monthlySumsWithEmpty = From month In allMonths
                                   Group Join ms In (From record In gpxRecords
                                                     Group record By Month = New DateTime(record.TrailStart.Time.Year, record.TrailStart.Time.Month, 1) Into grp = Group
                                                     Select New With {Month, .TotalDistance = grp.Sum(Function(r) r.TrailDistance)}) On month Equals ms.Month Into gj = Group From subMs In gj.DefaultIfEmpty(New With {month, .TotalDistance = 0.0})
                                   Select subMs

        ' Převedeme na pole pro graf
        Dim monthlyXAxisDataWithEmpty = monthlySumsWithEmpty.Select(Function(ms) ms.Month.ToString("MMMM yy", currentCulture)).ToArray
        Dim monthlyYAxisDataWithEmpty = monthlySumsWithEmpty.Select(Function(ms) ms.TotalDistance).ToArray

        For Each s In monthlyXAxisDataWithEmpty
            Debug.WriteLine($"X:  {s}")
        Next
        For Each y In monthlyYAxisDataWithEmpty
            Debug.WriteLine($"Y: {y}")
        Next

        Dim monthlyYAxisLabel = Resource1.Y_AxisLabelMonthly  'My.Resources.Resource1.Y_AxisLabelLength ' Nebo jiný popisek pro osu Y
        Dim monthlyGrafText = monthlyYAxisLabel ' Např. "Měsíční vzdálenost"
        Dim MonthlyChart1 = New frmChart(ActiveDog.Name, monthlyXAxisDataWithEmpty, monthlyYAxisDataWithEmpty, monthlyYAxisLabel, GPXFilesManager.dateFrom, GPXFilesManager.dateTo, monthlyGrafText, True, SeriesChartType.Column, currentCulture) ' Použijeme sloupcový graf (Column)
        MonthlyChart1.Show()
        Charts.Add(MonthlyChart1)


        ' Nastavení AcceptButton pro formulář, aby se při stisku Enter spustil btnReadGpxFiles_Click
        Me.AcceptButton = Me.btnReadGpxFiles

    End Sub


    Public Sub CloseGrafs()
        ' Zavření grafů
        For Each grf In Charts
            grf.Close()
        Next grf

        ' Vyprázdnění seznamu
        Charts.Clear()
    End Sub


    Public Sub ChangeLanguage(sender As Object, e As EventArgs) Handles mnuCzech.Click, mnuGerman.Click, mnuRussian.Click, mnuUkrainian.Click, mnuPolish.Click, mnuEnglish.Click
        SuspendLayout()
        Dim cultureName As String = sender.Tag
        Thread.CurrentThread.CurrentUICulture = New CultureInfo(cultureName)
        'Thread.CurrentThread.CurrentCulture = New CultureInfo(cultureName)

        currentCulture = Thread.CurrentThread.CurrentUICulture

        'Dim resources = New ComponentResourceManager([GetType]())
        Dim resources = New ComponentResourceManager(Me.GetType) ' nebo jiný konkrétní formulář

        resources.ApplyResources(Me, "$this")
        Dim stack As New Stack(Of Control)(Controls.Cast(Of Control)())

        ' Procházení všech ovládacích prvků a aplikace zdrojů, je třeba pro překlad textů:
        While stack.Count > 0
            Dim ctrl = stack.Pop()
            resources.ApplyResources(ctrl, ctrl.Name)
            If TypeOf ctrl Is ComboBox Then
                Dim cmb = DirectCast(ctrl, ComboBox)
                Dim selIndex As Integer = cmb.SelectedIndex
                cmb.Items.Clear()

                Dim i As Integer = 0
                Do
                    Dim key As String = If(i = 0, $"{cmb.Name}.Items", $"{cmb.Name}.Items{i}")
                    Dim item As String = resources.GetString(key)
                    If item Is Nothing Then Exit Do
                    cmb.Items.Add(item)
                    i += 1
                Loop
                cmb.SelectedIndex = selIndex 'last 365 days

            End If

            For Each child As Control In ctrl.Controls
                stack.Push(child)
            Next
        End While

        'lokalizace ListView:
        ' 
        resources.ApplyResources(clmFileName, "clmFileName")

        resources.ApplyResources(clmDate, "clmDate")

        resources.ApplyResources(clmLength, "clmLength")

        resources.ApplyResources(clmAge, "clmAge")

        resources.ApplyResources(clmTrkCount, "clmTrkCount")


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
        'mnuSelectBackupDirectory.ToolTipText = Resource1.Tooltip_mnuBackupDirectory
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



    Private Sub mnuSelect_directory_gpx_files_Click(sender As Object, e As EventArgs) Handles mnuSelect_directory_gpx_files.Click, mnuSelectADirectoryToSaveVideo.Click
        Dim folderDialog As New FolderBrowserDialog

        If sender Is mnuSelect_directory_gpx_files Or sender Is btnReadGpxFiles Then
            If ActiveDog.RemoteDirectory = "" Then
                ActiveDog.RemoteDirectory = Directory.GetParent(Application.StartupPath).ToString
            End If
            folderDialog.SelectedPath = ActiveDog.RemoteDirectory
            folderDialog.Description = $"Select a folder from where to load gpx files for dog {ActiveDog.Name}"
            folderDialog.UseDescriptionForTitle = True
        ElseIf sender Is mnuSelectADirectoryToSaveVideo Or sender Is btnCreateVideos Then
            folderDialog.ShowNewFolderButton = True
            folderDialog.Description = "Selecting the folder to save the video!"
            folderDialog.UseDescriptionForTitle = True
            If My.Settings.VideoDirectory = "" Then
                folderDialog.SelectedPath = Directory.GetParent(Application.StartupPath).ToString
            Else
                folderDialog.SelectedPath = My.Settings.VideoDirectory
            End If

        Else
            Return ' Pokud není žádná z očekávaných položek menu, ukonči metodu
        End If

        If folderDialog.ShowDialog = DialogResult.OK Then

            If sender Is mnuSelect_directory_gpx_files Or sender Is btnReadGpxFiles Then
                ActiveDog.RemoteDirectory = folderDialog.SelectedPath
                mbox($"The gpx files for the dog {ActiveDog.Name} will be imported from the {ActiveDog.RemoteDirectory} folder.")
                SaveDogs()
            ElseIf sender Is mnuSelectADirectoryToSaveVideo Or sender Is btnCreateVideos Then
                My.Settings.VideoDirectory = folderDialog.SelectedPath
                My.Settings.Save()
            Else
                Return ' Pokud není žádná z očekávaných položek menu, ukonči metodu
            End If

        End If


        StatusLabel1.Text = $"GPX files downloaded from: {ZkratCestu(ActiveDog.RemoteDirectory, 130)}" & vbCrLf & $"Video exported to: {ZkratCestu(My.Settings.VideoDirectory, 130)}"

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
        message = My.Resources.Resource1.Tooltip_mnuMergingTracks '"Set the maximum time difference (i.e. age of trails in hours) to identify related GPX tracks for automatic merging i.e. tracks of a Runner (runner) and the dog. A value of 0 disables automatic merging."
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

    Private Sub FactoryResetToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles mnuFactoryReset.Click
        If MessageBox.Show("Are you sure you want to clear all your settings?", " ",
        MessageBoxButtons.YesNo) = DialogResult.Yes Then My.Settings.Reset()
    End Sub

    Private Sub dtpStartDate_ValueChanged(sender As Object, e As EventArgs) Handles dtpStartDate.ValueChanged, dtpEndDate.ValueChanged
        CreateGpxFileManager() 'smaže vše ve staré instanci a vytvoří novou
        'zavři případné grafy
        CloseGrafs()
    End Sub

    'Private Sub mnuDogName_Click(sender As Object, e As EventArgs) Handles mnuDogName.Click
    '    My.Settings.DogName = InputBox("Set name of the dog:", Application.ProductName, My.Settings.DogName)
    '    My.Settings.Save()
    'End Sub

    ' ----- načtení a uložení dogs.json -----
    Private Sub LoadDogs()
        If File.Exists(DogsFilePath) Then
            Dim json = File.ReadAllText(DogsFilePath, Encoding.UTF8)
            Dim opts = New JsonSerializerOptions With {
                .PropertyNameCaseInsensitive = True
            }
            DogsList = JsonSerializer.Deserialize(Of List(Of DogInfo))(json, opts)
        Else
            DogsList = New List(Of DogInfo)()
        End If
    End Sub

    Private Sub SaveDogs()
        Dim options As New JsonSerializerOptions With {
            .WriteIndented = True,
            .Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping ' aby se diakritika neescapeovala
        }
        Directory.CreateDirectory(Path.GetDirectoryName(DogsFilePath))
        File.WriteAllText(DogsFilePath, JsonSerializer.Serialize(DogsList, options), Encoding.UTF8)
    End Sub

    ' ----- načtení a uložení config.json (aktivní pes) -----
    Private Sub LoadConfig()
        If File.Exists(ConfigPath) Then
            Dim json = File.ReadAllText(ConfigPath, Encoding.UTF8)
            Dim cfg = JsonSerializer.Deserialize(Of AppConfig)(json)
            If cfg IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(cfg.ActiveDogId) Then
                ActiveDogId = cfg.ActiveDogId
            End If
        End If
    End Sub

    Private Sub SaveConfig()
        Dim options As New JsonSerializerOptions With {
            .WriteIndented = True,
            .Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        }
        Dim cfg As New AppConfig With {.ActiveDogId = ActiveDogId}
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(cfg, options), Encoding.UTF8)
    End Sub

    ' ----- naplnění ToolStripComboBoxu objekty DogInfo -----
    Private Sub PopulateDogsToolStrip()
        mnucbActiveDog.ComboBox.Items.Clear()

        For Each d As DogInfo In DogsList
            mnucbActiveDog.ComboBox.Items.Add(d) ' zobrazí se Name díky ToString()
        Next

        ' pokud je nastaven ActiveDogId, vyber ho; jinak vyber první
        Dim selectedIndex As Integer = -1
        If Not String.IsNullOrEmpty(ActiveDogId) Then
            For i As Integer = 0 To mnucbActiveDog.ComboBox.Items.Count - 1
                Dim item As DogInfo = CType(mnucbActiveDog.ComboBox.Items(i), DogInfo)
                If item.Id = ActiveDogId Then
                    selectedIndex = i
                    Exit For
                End If
            Next
        End If

        If selectedIndex = -1 AndAlso mnucbActiveDog.ComboBox.Items.Count > 0 Then selectedIndex = 0

        If selectedIndex >= 0 Then
            mnucbActiveDog.ComboBox.SelectedIndex = selectedIndex
            ' zajistí, že ActiveDogId drží hodnotu
            Dim sel = TryCast(mnucbActiveDog.ComboBox.SelectedItem, DogInfo)
            If sel IsNot Nothing Then
                ActiveDogId = sel.Id
                'lblActiveDog.Text = $"Aktivní pes: {sel.Name} ({sel.Id})"
            End If
        Else
            'lblActiveDog.Text = "Aktivní pes: (není)"
        End If
    End Sub

    ' ----- handler při změně výběru v ToolStripComboBoxu -----
    Private Sub mnucbActiveDog_SelectedIndexChanged(sender As Object, e As EventArgs) Handles mnucbActiveDog.SelectedIndexChanged
        Dim sel = TryCast(mnucbActiveDog.ComboBox.SelectedItem, DogInfo)
        If sel Is Nothing Then Return
        SetScrollState(1, sel.Id) ' nastavíme scroll state pro aktivního psa
        ActiveDogId = sel.Id
        My.Settings.ActiveDog = sel.Id ' uložíme jméno psa do nastavení
        'lblActiveDog.Text = $"Aktivní pes: {sel.Name} ({sel.Id})"
        My.Settings.Save()
        ' uložíme config (aby se volba pamatovala)
        SaveConfig()

        ' tady můžeš volat další inicializace pro aktivního psa
        ' e.g. RefreshOriginalsFolderForActiveDog()
    End Sub

    ' ----- příklad: přidání nového psa (mnuAddDog click) -----
    Private Sub mnuAddDog_Click(sender As Object, e As EventArgs) Handles mnuAddDog.Click
        Dim dogName = InputBox("Enter the name of the new dog:", "New dog")
        If String.IsNullOrWhiteSpace(dogName) Then
            MessageBox.Show("The name of the dog was not given.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' vytvoříme nové ID (trojciferné)
        Dim newId As String = If(DogsList.Count = 0, "001", (DogsList.Select(Function(d) Integer.Parse(d.Id)).Max() + 1).ToString("D3"))
        ' vytvoříme složky
        'Dim dogLocalBasePath = Path.Combine(DogsRootPath, newId)

        Dim dogRemotePath = ActiveDog.RemoteDirectory 'Path.Combine(My.Settings.Directory, dogName)
        Dim folderDialog As New FolderBrowserDialog()
        folderDialog.SelectedPath = dogRemotePath
        folderDialog.Description = $"Selecting the folder from where to load gpx files for the dog {dogName}."
        folderDialog.UseDescriptionForTitle = True
        If False = (folderDialog.ShowDialog() = DialogResult.OK) Then
            ' pokud uživatel zrušil výběr, skončíme
            MessageBox.Show("The dog was not added.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        ElseIf Directory.Exists(folderDialog.SelectedPath) Then
            dogRemotePath = folderDialog.SelectedPath
        Else
            MessageBox.Show("The selected folder does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        ' přidáme do seznamu a uložíme
        DogsList.Add(New DogInfo With {.Id = newId,
                     .Name = dogName,
                     .RemoteDirectory = dogRemotePath})

        SaveDogs()



        ' refresh UI: vložíme nového psa do comboboxu a vybereme ho
        PopulateDogsToolStrip()
        ' vybrat nového psa:
        For i As Integer = 0 To mnucbActiveDog.ComboBox.Items.Count - 1
            Dim d As DogInfo = CType(mnucbActiveDog.ComboBox.Items(i), DogInfo)
            If d.Id = newId Then mnucbActiveDog.ComboBox.SelectedIndex = i : Exit For
        Next

        MessageBox.Show($"Pes '{dogName}' přidán s ID {newId}.", "Hotovo", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub



    Private Sub mnuSetFFmpegPath_Click(sender As Object, e As EventArgs) Handles mnuSetFFmpegPath.Click

        Dim FFmpegPath As String = FindAnSaveFfmpegPath()
        Dim ofd As New OpenFileDialog()
        ofd.InitialDirectory = My.Settings.FfmpegPath
        ofd.Title = "Find ffmpeg.exe"
        ofd.Filter = "ffmpeg.exe|ffmpeg.exe"
        If ofd.ShowDialog() = DialogResult.OK Then
            My.Settings.FfmpegPath = ofd.FileName
            My.Settings.Save()
        End If
    End Sub


    Private Sub mnuExit_Click(sender As Object, e As EventArgs) Handles mnuExit.Click
        Me.Close() ' Zavře aplikaci
    End Sub

    Private Sub cmbTimeInterval_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbTimeInterval.SelectedIndexChanged

        Dim today = Date.Today
        Select Case cmbTimeInterval.SelectedIndex
            Case 0
                dtpStartDate.Value = today.AddDays(-6)
                dtpEndDate.Value = today
            Case 1
                dtpStartDate.Value = today.AddDays(-30)
                dtpEndDate.Value = today
            Case 2
                dtpStartDate.Value = today.AddDays(-364)
                dtpEndDate.Value = today
            Case 3
                dtpStartDate.Value = New Date(today.Year, 1, 1) ' Začátek roku
                dtpEndDate.Value = today
            Case 4
                dtpStartDate.Value = New Date(today.Year - 1, 1, 1) ' Začátek minulého roku
                dtpEndDate.Value = New Date(today.Year - 1, 12, 31) ' konec minulého roku
            Case Else
                ' Necháme hodnoty, jak jsou
        End Select
    End Sub

    Private Async Sub btnCreateVideos_Click(sender As Object, e As EventArgs) Handles btnCreateVideos.Click

        If lvGpxFiles.CheckedItems.Count > 2 Then
            If mboxq($"Are you sure you want to create videos from {lvGpxFiles.CheckedItems.Count} gpx records now? It can take a long time and there's no chance to stop it 🤣!", "Are you sure?") = DialogResult.No Then
                Return
            End If
        End If

        ' Zkontroluj, zda je nastavená složka pro videa
        Dim videoDir = My.Settings.VideoDirectory
        If String.IsNullOrWhiteSpace(videoDir) OrElse Not Directory.Exists(videoDir) Then
            ' Cesta není nastavená nebo složka neexistuje 
            mnuSelect_directory_gpx_files_Click(btnCreateVideos, New EventArgs)
        End If

        Dim selectedFiles As New List(Of GPXRecord)
        For Each item As ListViewItem In lvGpxFiles.CheckedItems
            ' Předpoklad: Tag obsahuje plnou cestu k souboru
            Dim _gpxRecord As GPXRecord = TryCast(item.Tag, GPXRecord) 'není to zbytečný?
            If Not _gpxRecord Is Nothing Then
                selectedFiles.Add(_gpxRecord)
            End If
        Next

        ' Můžeš je teď předat funkci pro export videa

        If selectedFiles.Count = 0 Then
            mboxEx("First, select the footage from which to create the video!")
            Return
        End If


        For Each record In selectedFiles
            ' Pro test: vypiš vybrané cesty
            Debug.WriteLine(record)
            Try
                record.Description = Await record.BuildLocalisedDescriptionAsync(record.Description) 'async kvůli počasí!
                record.WriteDescription() 'zapíše agregovaný popis do tracku Runner
                record.WriteLocalizedReports() 'zapíše popis do DogTracku
                record.IsAlreadyProcessed = True 'už byl soubor zpracován
                record.Save()
            Catch ex As Exception

            End Try

            Try
                Await CreateVideoFromGPXRecord(record)
            Catch ex As Exception
                mboxEx($"Creating a video from a file {record.FileName} failed." & vbCrLf & $"Message: {ex}")
            End Try
        Next
    End Sub

    Public Async Function CreateVideoFromGPXRecord(_gpxRecord As GPXRecord) As Task(Of Boolean)

        ' Create a video from the dog track and save it in the video directory
        ' Zjisti název souboru bez přípony
        Dim gpxName = System.IO.Path.GetFileNameWithoutExtension(_gpxRecord.FileName)
        ' Sestav cestu k novému adresáři

        Dim directory As New IO.DirectoryInfo(System.IO.Path.Combine(My.Settings.VideoDirectory, gpxName))
        ' Pokud adresář neexistuje, vytvoř ho
        If Not directory.Exists Then directory.Create()
        Dim FFmpegPath As String = FindAnSaveFfmpegPath()
        Dim videoCreator As New VideoExportManager(FFmpegPath, directory, _gpxRecord.WeatherData._windDirection, _gpxRecord.WeatherData._windSpeed)
        AddHandler videoCreator.WarningOccurred, AddressOf WriteRTBWarning

        Dim waitForm As New frmPleaseWait("I'm making an overlay video, please stand by...")
        waitForm.Show()

        ' Spustíme na pozadí, aby nezamrzlo UI
        Await Task.Run(Async Function()
                           ' Spustíme tvůj dlouhý proces
                           Dim success = Await videoCreator.CreateVideoFromTrkNodes(_gpxRecord.Tracks, _gpxRecord.WptNodes, _gpxRecord.LocalisedReports)

                           ' Po dokončení se vrať na UI thread a proveď akce
                           waitForm.Invoke(Sub()
                                               waitForm.Close()

                                               If success Then
                                                   Dim videopath As String = IO.Path.Combine(directory.FullName, "overlay.webm")
                                                   Dim bgPNGPath As String = IO.Path.Combine(directory.FullName, "TrailsOnMap.png")
                                                   Dim form As New frmVideoDone(videopath, bgPNGPath)
                                                   form.ShowDialog()
                                                   form.Dispose()
                                               Else
                                                   MessageBox.Show("Video creation failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                               End If
                                           End Sub)
                       End Function)

        Return False
    End Function



    Private Sub lvGpxFiles_ItemChecked(sender As Object, e As ItemCheckedEventArgs) Handles lvGpxFiles.ItemChecked
        Dim _gpxRecord As GPXRecord = TryCast(e.Item.Tag, GPXRecord)
        If _gpxRecord.DogStart Is Nothing OrElse _gpxRecord.DogStart.Time = Date.MinValue Then 'Pokud není datum začátku trasy psa, nejde označit

            If e.Item.Checked Then
                mboxEx("This gpx footage does not contain the dog's movement, there is no point in creating a video from it")
                e.Item.Checked = False
            End If
            e.Item.Selected = False

        End If
    End Sub

    Private Sub lvGpxFiles_ItemSelectionChanged(sender As Object, e As ListViewItemSelectionChangedEventArgs) Handles lvGpxFiles.ItemSelectionChanged
        e.Item.Selected = False 'aby se to nepletlo s checked
    End Sub

    Private Sub TabControl1_Selecting(sender As Object, e As TabControlCancelEventArgs) Handles TabControl1.Selecting
        If e.TabPage Is TabVideoExport AndAlso lvGpxFiles.Items.Count = 0 Then
            mboxEx("This tab is not ready yet." & vbCrLf & "First you need to load the gpx files - click on the salmon button!")
            e.Cancel = True
        End If
    End Sub

    Private Sub mnuAbout_Click(sender As Object, e As EventArgs) Handles mnuAbout.Click
        Try
            Process.Start(New ProcessStartInfo With {
                .FileName = "https://github.com/mwrnckx/K9-Trails-Analyzer",
                .UseShellExecute = True
            })
        Catch ex As Exception
            MessageBox.Show("Unable to open link: " & ex.Message)
        End Try
    End Sub

    Private Sub mnuCheckUpdates_Click(sender As Object, e As EventArgs) Handles mnuCheckForUpdates1.Click
        Try
            Dim url As String = "https://api.github.com/repos/mwrnckx/K9-Trails-Analyzer/releases/latest"
            Dim client As New Net.WebClient()
            client.Headers.Add("User-Agent", "K9TrailsAnalyzer")
            Dim content As String = client.DownloadString(url)

            Dim json As JsonDocument = JsonDocument.Parse(content)
            Dim root = json.RootElement
            Dim latestTag = root.GetProperty("tag_name").GetString()

            Dim currentVersion '= New Version(Application.ProductVersion)
            currentVersion = GetType(Form1).Assembly.GetName.Version
            Dim latestVersion = New Version(latestTag.TrimStart("v"c))


            If latestVersion > currentVersion Then
                If mboxq($"A new version is available: {latestVersion.ToString()}" & vbCrLf & "Should I try to download it now?", MessageBoxDefaultButton.Button1) = DialogResult.Yes Then
                    ' Získat pole assets
                    Dim assets = root.GetProperty("assets")
                    Dim downloadUrl As String
                    If assets.GetArrayLength() > 0 Then
                        Dim firstAsset = assets(0)
                        downloadUrl = firstAsset.GetProperty("browser_download_url").GetString()
                        ' Otevře URL v defaultním browseru
                        Process.Start(New ProcessStartInfo(downloadUrl) With {.UseShellExecute = True})
                        Debug.WriteLine("Download URL: " & downloadUrl)
                    Else
                        Debug.WriteLine("The download failed.")
                    End If
                End If
            Else
                MessageBox.Show("You're using the latest version.", "Updates", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            MessageBox.Show("The availability of the new version could not be ascertained." & vbCrLf & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Form1_ResizeEnd(sender As Object, e As EventArgs) Handles Me.ResizeEnd
        My.Settings.WindowSize = Me.Size
    End Sub

    Private Sub DeleteCurrentDogToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles mnuDeleteCurrentDog.Click
        ' Najdi psa podle ID
        Dim dogId As String = ActiveDogId
        Dim dogToRemove = DogsList.FirstOrDefault(Function(d) d.Id = dogId)
        If dogToRemove Is Nothing Then
            MessageBox.Show("Dog not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If
        If DogsList.Count = 1 Then
            MessageBox.Show("You cannot delete the last dog. Please add another one first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        ' Potvrzení od uživatele
        Dim result = MessageBox.Show(
        $"You really want to delete the dog '{dogToRemove.Name}'? (Local gpx files will be preserved.)",
        "Confirmation of deletion",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Warning)

        If result <> DialogResult.Yes Then Return

        ' Odebrání psa ze seznamu
        DogsList.Remove(dogToRemove)

        ' Ulož seznam zpět do JSON
        SaveDogs()

        ' Pokud byl aktivní, přepnout na jiného
        If ActiveDogId = dogId Then
            ActiveDogId = If(DogsList.Count > 0, DogsList(0).Id, String.Empty)
        End If

        ' Obnov UI (combo box apod.)
        PopulateDogsToolStrip()

        MessageBox.Show($"The dog '{dogToRemove.Name}' has been deleted.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)


    End Sub

    Private Sub mnuRenameCurrentDog_Click(sender As Object, e As EventArgs) Handles mnuRenameCurrentDog.Click
        Dim selectedDog As DogInfo = CType(mnucbActiveDog.ComboBox.SelectedItem, DogInfo)
        If selectedDog IsNot Nothing Then
            Dim newName As String = InputBox("Enter the dog's new name:", "Rename the dog", selectedDog.Name)
            If Not String.IsNullOrWhiteSpace(newName) AndAlso newName <> selectedDog.Name Then
                If DogsList.Any(Function(d) d.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)) Then
                    MessageBox.Show("A dog with this name already exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Else
                    selectedDog.Name = newName
                    ' případně uložit do JSON
                    SaveDogs()
                    PopulateDogsToolStrip()
                End If
            End If
        End If

    End Sub

End Class



' --- model psa ---
Public Class DogInfo
    <JsonPropertyName("id")>
    Public Property Id As String

    <JsonPropertyName("name")>
    Public Property Name As String

    <JsonPropertyName("remotedirectory")>
    Dim _remoteDirectory As String
    Public Property RemoteDirectory As String
        Get
            If (String.IsNullOrWhiteSpace(_remoteDirectory)) Then
                ' pokud není nastaveno, použije se výchozí cesta

                _remoteDirectory = Path.Combine(Application.StartupPath, "Samples")
                End If
            Return _remoteDirectory
        End Get
        Set(value As String)
            _remoteDirectory = value
        End Set
    End Property



    <JsonIgnore>
    Public ReadOnly Property LocalBaseDirectory As String
        Get
            Dim _directory = Path.Combine(Application.StartupPath, "Dogs", Id)
            Directory.CreateDirectory(_directory)
            Return _directory
        End Get
    End Property

    <JsonIgnore>
    Public ReadOnly Property ProcessedDirectory As String
        Get
            Dim _directory = Path.Combine(LocalBaseDirectory, "Processed")
            Directory.CreateDirectory(_directory)
            Return _directory
        End Get
    End Property

    <JsonIgnore>
    Public ReadOnly Property OriginalsDirectory As String
        Get
            Dim _directory = Path.Combine(LocalBaseDirectory, "Originals")
            Directory.CreateDirectory(_directory)
            Return _directory
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return Name & " (" & Id & ")"
    End Function
End Class


' --- jednoduchá konfigurace pro aktivního psa ---
Public Class AppConfig
    <JsonPropertyName("activeDogId")>
    Public Property ActiveDogId As String
End Class