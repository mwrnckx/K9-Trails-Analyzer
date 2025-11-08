Imports System.ComponentModel
Imports System.Data.Common
Imports System.Diagnostics.Metrics
Imports System.DirectoryServices.ActiveDirectory
Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices.JavaScript.JSType
Imports System.Text
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports System.Threading
Imports System.Windows.Forms.DataVisualization.Charting
Imports GPXTrailAnalyzer.My.Resources
Imports Microsoft.VisualBasic.Logging
Imports TrackVideoExporter
Imports TrackVideoExporter.TrackVideoExporter
Imports Windows.Win32.System





Partial Public Class Form1
    Private currentCulture As CultureInfo = Thread.CurrentThread.CurrentCulture
    Private GPXFilesManager As GpxFileManager
    Private ReadOnly CategoriesInfoPath As String = Path.Combine(Application.StartupPath, "AppData", "categoriesInfo.json")
    'Private ReadOnly ConfigPath As String = Path.Combine(Application.StartupPath, "AppData", "config.json")
    Private sortColumnName As String = String.Empty 'třídění dgvCompetition
    Private sortDirection As SortOrder = SortOrder.None
    Private CategoriesInfo As List(Of CategoryInfo)
    Private ActiveCategoryId As String = String.Empty
    Private ReadOnly Property ActiveCategoryInfo As CategoryInfo
        Get
            Return CategoriesInfo.FirstOrDefault(Function(d) d.Id = ActiveCategoryId)
        End Get
    End Property

    Private displayList As List(Of TrailStatsDisplay) 'datasource dgvCompetition

    Private Async Sub btnReadGpxFiles_Click(sender As Object, e As EventArgs) Handles btnReadGpxFiles.Click

        Enabled = False

        Dim gpxDir = ActiveCategoryInfo.RemoteDirectory 'My.Settings.Directory
        If String.IsNullOrWhiteSpace(gpxDir) OrElse Not Directory.Exists(gpxDir) Then
            ' Cesta není nastavená nebo složka neexistuje → použij výchozí složku Samples vedle exe
            Dim defaultDir = Path.Combine(Application.StartupPath, "Samples")

            If Directory.Exists(defaultDir) Then
                gpxDir = defaultDir
                ActiveCategoryInfo.RemoteDirectory = gpxDir
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

        GPXFilesManager.CategoryInfo = ActiveCategoryInfo
        GPXFilesManager.NumberOfCategories = CategoriesInfo.Count



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

    Private Sub ClearDgvCompetition()
        If displayList IsNot Nothing Then displayList.Clear()
        dgvCompetition.DataSource = Nothing ' Odpojte stávající zdroj
        dgvCompetition.Columns.Clear()     ' Vyčistěte sloupce, pokud byly automaticky generovány
    End Sub



    Private Sub FillDgvCompetition()
        ' Vytvořte seznam pro zobrazení a naplňte ho
        ClearDgvCompetition() 'nejprve vyčistíme
        displayList = New List(Of TrailStatsDisplay)()
        For i As Integer = GPXFilesManager.GpxRecords.Count - 1 To 0 Step -1
            Dim record As GPXRecord = GPXFilesManager.GpxRecords(i)
            Dim stats As TrackVideoExporter.TrailStats = record.TrailStats
            displayList.Add(New TrailStatsDisplay With {
                .OriginalRecord = record,
                .GPXFilename = IO.Path.GetFileNameWithoutExtension(GPXFilesManager.GpxRecords(i).Reader.FilePath),'.Substring(11),
                .DogName = stats.PointsInMTCompetition.dogName,
                .HandlerName = stats.PointsInMTCompetition.handlerName,
                 .RunnerDistance = stats.RunnerDistance,
                 .TotalTime = stats.DogTotalTime.Minutes,
                .DogGrossSpeedKmh = stats.DogGrossSpeed,
                .DogDistance = stats.DogDistance,
                .Deviation = stats.AverDeviation,
                .MaxTeamDistance = stats.MaxTeamDistance,
                .WeightedDistanceAlongTrail = stats.WeightedDistanceAlongTrail,
                .WeightedDistanceAlongTrailPerCent = stats.WeightedDistanceAlongTrailPerCent / 100,'je v % převedeno zpět na desetinné číslo
                .WeightedTimePerCent = stats.WeightedTimePerCent / 100, 'je v % převedeno zpět na desetinné číslo
                .StartTime = record.TrailStart.Time,
                .TrailAge = stats.TrailAge.TotalMinutes,
                .RunnerFoundPoints = stats.PointsInMTCompetition.RunnerFoundPoints,
                .DogSpeedPoints = stats.PointsInMTCompetition.DogSpeedPoints,
                .DogAccuracyPoints = stats.PointsInMTCompetition.DogAccuracyPoints,
                .dogReadingPoints = stats.PointsInMTCompetition.DogReadingPoints
                 })

            Dim displayItem = displayList.Last()
            'displayItem.TotalPoints = displayItem.RunnerFoundPoints +
            '                   displayItem.DogSpeedPoints +
            '                   displayItem.DogAccuracyPoints +
            '                   displayItem.HandlerCheckPoints
            ' Nyní přidejte data pro první dva checkpointy z CheckpointsEval
            Dim firstIndex As Integer = 0
            Dim secondIndex As Integer = 0
            If stats.CheckpointsEval IsNot Nothing Then
                firstIndex = stats.CheckpointsEval.Count - 2  ' První checkpoint
                secondIndex = stats.CheckpointsEval.Count - 1 ' Druhý checkpoint

                ' Použij podmínku:
                If secondIndex >= 0 Then
                    With displayItem
                        .SecondCheckpointEvalDeviationFromTrail = stats.CheckpointsEval(secondIndex).deviationFromTrail
                        .SecondCheckpointEvalDistance = stats.CheckpointsEval(secondIndex).distanceAlongTrail
                        .SecondCheckpointEvaldogGrossSpeed = stats.CheckpointsEval(secondIndex).dogGrossSpeedkmh
                    End With
                End If
                If firstIndex >= 0 Then
                    With displayItem
                        .FirstCheckpointEvalDeviationFromTrail = stats.CheckpointsEval(firstIndex).deviationFromTrail
                        .FirstCheckpointEvalDistance = stats.CheckpointsEval(firstIndex).distanceAlongTrail
                        .FirstCheckpointEvaldogGrossSpeed = stats.CheckpointsEval(firstIndex).dogGrossSpeedkmh
                    End With
                End If
            End If
        Next i

        'displayList.Sort(Function(a, b) b.TotalPoints.CompareTo(a.TotalPoints)) ' Seřadit sestupně 

        ''vypíše pořadí v soutěži:
        'Dim ranking As Integer = 1
        'For Each item In displayList
        '    item.Ranking = ranking.ToString & "."
        '    ranking += 1
        'Next

        For Each item In Me.displayList
            ' Přihlášení k události PropertyChanged každého objektu
            AddHandler item.PropertyChanged, AddressOf Me.TrailItem_PropertyChanged
        Next

        UpdateRankingAndDisplay(False) ' Aktualizuje řazení a pořadí

        dgvCompetition.Columns.Clear()
        Me.bsCompetitions.DataSource = Me.displayList 'bindingSource kvůli řazení sloupců
        Me.dgvCompetition.DataSource = Me.bsCompetitions
        'zformátovat dgvCompetition:
        Me.FormatDgvCompetition()
    End Sub


    Private Sub TrailItem_PropertyChanged(sender As Object, e As PropertyChangedEventArgs)

        ' Zkontrolujte, zda se změnily vypočítané TotalPoints.
        If e.PropertyName = NameOf(TrailStatsDisplay.TotalPoints) Then
            ' TotalPoints se změnily, musíme znovu seřadit celý seznam a aktualizovat pořadí
            Me.UpdateRankingAndDisplay(True)
        End If

        Dim changedItem As TrailStatsDisplay = TryCast(sender, TrailStatsDisplay)

        If changedItem IsNot Nothing AndAlso changedItem.OriginalRecord IsNot Nothing Then

            ' 1. Získat KOPII celé nadřazené struktury TrailStats.
            Dim currentStats As TrackVideoExporter.TrailStats = changedItem.OriginalRecord.TrailStats

            ' 2. Získat KOPII vnitřní struktury PointsInMTCompetition, kterou budeme měnit.
            Dim pointsStruct As TrackVideoExporter.ScoringData = currentStats.PointsInMTCompetition

            Select Case e.PropertyName
                Case NameOf(TrailStatsDisplay.RunnerFoundPoints)
                    pointsStruct.RunnerFoundPoints = changedItem.RunnerFoundPoints

                Case NameOf(TrailStatsDisplay.DogSpeedPoints)
                    pointsStruct.DogSpeedPoints = changedItem.DogSpeedPoints

                Case NameOf(TrailStatsDisplay.DogAccuracyPoints)
                    pointsStruct.DogAccuracyPoints = changedItem.DogAccuracyPoints

                Case NameOf(TrailStatsDisplay.dogReadingPoints)
                    pointsStruct.DogReadingPoints = changedItem.dogReadingPoints

                Case NameOf(TrailStatsDisplay.DogName)
                    pointsStruct.dogName = changedItem.DogName

                Case NameOf(TrailStatsDisplay.HandlerName)
                    pointsStruct.handlerName = changedItem.HandlerName
                Case Else
                    Return 'není třeba nic zapisovat, proto return
            End Select

            ' 3. Po modifikaci PŘIŘADIT upravenou strukturu PointsInMTCompetition ZPĚT do KOPIE TrailStats.
            currentStats.PointsInMTCompetition = pointsStruct

            ' 4. KLÍČOVÝ KROK: PŘIŘADIT celou upravenou strukturu TrailStats ZPĚT do OriginalRecord.
            ' Tím se změna propíše do skutečného místa v paměti.
            changedItem.OriginalRecord.TrailStats = currentStats

            changedItem.OriginalRecord.BuildLocalisedPerformancePoints()
            changedItem.OriginalRecord.WriteLocalizedReports()
            changedItem.OriginalRecord.Save()
        End If
    End Sub

    Private Sub UpdateRankingAndDisplay(Optional resetBindings As Boolean = False)
        ' 1. Seřadit displayList (sestupně podle TotalPoints)
        ' Funkce CompareTo pro Integer je nejpřímější. Pro sestupné řazení se vymění a a b
        Me.displayList.Sort(Function(a, b) b.TotalPoints.CompareTo(a.TotalPoints))

        ' 2. Aktualizovat vlastnost Ranking
        Dim ranking As Integer = 1
        For Each item In Me.displayList
            ' Důležité: Nastavte Ranking pomocí vlastnosti, kde máte implementovaný INotifyPropertyChanged
            ' i pro Ranking. Pokud Ranking nemá INotifyPropertyChanged, musíte ho přidat.
            ' Předpokládáme, že jej nyní přidáme do TrailStatsDisplay (viz Krok 2).
            item.Ranking = ranking.ToString() & "."
            ranking += 1
        Next

        ' 3. Vynutit aktualizaci DataGridView (klíčové)
        ' Protože jsme změnili pořadí v podkladovém listu, musíme BindingSource
        ' upozornit, že se všechna data změnila.
        If resetBindings Then Me.bsCompetitions.ResetBindings(False)

    End Sub

    Private Sub FormatDgvCompetition()

        ' Krok 3: Projít sloupce, lokalizovat, vynutit zalomení a nastavit Autosize
        For Each column As DataGridViewColumn In Me.dgvCompetition.Columns

            ' Příklad získání lokalizovaného textu TODO:!!!!!!
            Dim propertyName As String = column.DataPropertyName
            Dim resourceKey As String = "Header_" & propertyName
            Dim localizedText As String = My.Resources.ResourceManager.GetString(resourceKey, My.Resources.Culture)

            ' ZAJIŠTĚNÍ JEDNOTNÉ FORMÁTU HLAVIČKY:
            If Not String.IsNullOrEmpty(localizedText) Then
                ' 1. Aplikovat zalomení: "Successful Find Points" -> "Runner" & vbCrLf & "Found" & vbCrLf & "Points"
                column.HeaderText = FormatHeader(localizedText)
            Else
                ' Fallback, pokud není lokalizovaný text (např. název Property)
                column.HeaderText = FormatHeader(column.HeaderText)
            End If

            ' 2. Vynucení nízké šířky pro správný Autosize
            ' Tímto zajistíte, že se sloupec přizpůsobí pouze nejdelšímu slovu.
            column.Width = 10
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells

            column.DefaultCellStyle.Font = New Font("Cascadia Code", 12.0F, FontStyle.Regular, GraphicsUnit.Point, CByte(238))
            column.HeaderCell.Style.ForeColor = Color.Black
        Next
        Me.dgvCompetition.Columns("GPXFilename").HeaderText = ActiveCategoryInfo.Name
        Me.dgvCompetition.Columns("GPXFilename").HeaderCell.Style.Font = New Font("Cascadia Code", 16.0F, FontStyle.Bold, GraphicsUnit.Point, CByte(238))
        ' Krok 4: Vynutit automatické přizpůsobení šířky po dokončení
        Me.dgvCompetition.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells)


        Dim columnsIntegerData As String() = {
    "DogDistance",
    "RunnerDistance",
    "WeightedDistanceAlongTrail",
       "Deviation",
       "maxTeamDistance",
        "TrailAge",
      "FirstCheckpointEvalDistance",
          "SecondCheckpointEvalDistance",
     "SecondCheckpointEvalDeviationFromTrail"
}

        For Each columnName As String In columnsIntegerData
            ' Zkontrolujte, jestli sloupec existuje
            If Me.dgvCompetition.Columns.Contains(columnName) Then
                Dim column As DataGridViewColumn = Me.dgvCompetition.Columns(columnName)

                ' Nastavte formát na jedno desetinné místo ("N1")
                ' N1 = Číslo s oddělovači tisíců a 1 desetinným místem (podle aktuální regionální kultury)
                ' F1 = Pevný počet desetinných míst, 1 desetinné místo
                column.DefaultCellStyle.Format = "N0"

            End If
        Next
        Dim columnsFloatData As String() = {
    "DogGrossSpeedKmh",
       "FirstCheckpointEvalDeviationFromTrail",
     "FirstCheckpointEvaldogGrossSpeed",
     "SecondCheckpointEvaldogGrossSpeed",
      "TotalTime"
}
        For Each columnName As String In columnsFloatData
            ' Zkontrolujte, jestli sloupec existuje
            If Me.dgvCompetition.Columns.Contains(columnName) Then
                Dim column As DataGridViewColumn = Me.dgvCompetition.Columns(columnName)
                ' Nastavte formát na jedno desetinné místo ("N1")
                ' N1 = Číslo s oddělovači tisíců a 1 desetinným místem (podle aktuální regionální kultury)
                ' F1 = Pevný počet desetinných míst, 1 desetinné místo
                column.DefaultCellStyle.Format = "F1"
            End If
        Next

        If Me.dgvCompetition.Columns.Contains("WeightedDistanceAlongTrailPerCent") Then
            Dim column As DataGridViewColumn = Me.dgvCompetition.Columns("WeightedDistanceAlongTrailPerCent")
            ' P2 = Procenta na 2 desetinná místa (např. 12,34 %)
            column.DefaultCellStyle.Format = "P0"
        End If

        If Me.dgvCompetition.Columns.Contains("WeightedTimePerCent") Then
            Dim column As DataGridViewColumn = Me.dgvCompetition.Columns("WeightedTimePerCent")
            ' P2 = Procenta na 2 desetinná místa (např. 12,34 %)
            column.DefaultCellStyle.Format = "P0"
        End If

        If Me.dgvCompetition.Columns.Contains("StartTime") Then
            Dim column As DataGridViewColumn = Me.dgvCompetition.Columns("StartTime")
            column.DefaultCellStyle.Format = "HH:mm"
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        End If

        Dim columnsPointsData As String() = {
    "TotalPoints",
    "RunnerFoundPoints",
   "DogSpeedPoints",
        "DogAccuracyPoints",
        "DogReadingPoints",
        "Ranking"
    }
        For Each columnName As String In columnsPointsData
            ' Zkontrolujte, jestli sloupec existuje
            If Me.dgvCompetition.Columns.Contains(columnName) Then
                Dim column As DataGridViewColumn = Me.dgvCompetition.Columns(columnName)
                ' Nastavte formát na jedno desetinné místo ("N1")
                ' N1 = Číslo s oddělovači tisíců a 1 desetinným místem (podle aktuální regionální kultury)
                ' F1 = Pevný počet desetinných míst, 1 desetinné místo
                column.DefaultCellStyle.Format = "F0"
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
                column.DefaultCellStyle.BackColor = Color.DarkSeaGreen
                'column.HeaderCell.Style.BackColor = Color.DarkSeaGreen
            End If
        Next
        Me.dgvCompetition.Columns("TotalPoints").DefaultCellStyle.Font = New Font(Me.dgvCompetition.Columns("TotalPoints").DefaultCellStyle.Font, FontStyle.Bold)

        ' Ranking je pouze pro čtení
        If Me.dgvCompetition.Columns.Contains(NameOf(TrailStatsDisplay.Ranking)) Then
            Me.dgvCompetition.Columns(NameOf(TrailStatsDisplay.Ranking)).ReadOnly = True
        End If

    End Sub
    ' Funkce nahradí mezery v textu záhlaví za zalomení řádku (vbCrLf)
    Private Function FormatHeader(ByVal headerText As String) As String
        ' Nahradí všechny mezery v textu za zalomení řádku
        Return headerText.Replace(" ", vbCrLf)
    End Function

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
            item.SubItems.Add($"{record.TrailDistance / 1000.0:F2} km") ' délka trasy
            item.SubItems.Add($"{record.TrailStats.TrailAge.TotalHours:F1} h") ' věk trasy v hodinách
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
            GPXFilesManager.GpxRecords.Clear()
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
        'Me.rtbOutput.AppendText((My.Resources.Resource1.X_AxisLabel & manySpaces).Substring(0, 12))
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
                'Me.rtbOutput.AppendText(_gpxRecord.TrailStart.Time.ToString("dd.MM.yy") & "    ")
                Me.rtbOutput.AppendText((_gpxRecord.TrailDistance / 1000.0).ToString("F2") & " km" & "     ")
                If _gpxRecord.TrailStats.TrailAge.TotalHours > 0 Then
                    Me.rtbOutput.AppendText(_gpxRecord.TrailStats.TrailAge.TotalHours.ToString("F1") & " h" & "   ")
                Else
                    Me.rtbOutput.AppendText("        ")
                End If
                'Dim dogspeed As Double = _gpxRecord.DogSpeed
                If _gpxRecord.TrailStats.DogNetSpeed > 0 Then
                    Me.rtbOutput.AppendText(_gpxRecord.TrailStats.DogNetSpeed.ToString("F1") & " km/h" & "   ")
                Else
                    Me.rtbOutput.AppendText("           ")
                End If
                If _gpxRecord.TrailStats.AverDeviation > 0 Then
                    Me.rtbOutput.AppendText(_gpxRecord.TrailStats.AverDeviation.ToString("F1") & " m" & "   ")
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
                MessageBox.Show(My.Resources.Resource1.mBoxDataRetrievalFailed & vbCrLf & "File: " & IO.Path.GetFileNameWithoutExtension(_gpxRecord.Reader.FilePath) & vbCrLf & ex.Message & Environment.NewLine &
                              $"(StackTrace):" & Environment.NewLine &
                              ex.StackTrace, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        Next _gpxRecord


        'Dim AgeAsDouble As List(Of Double) = age.Select(Function(ts) ts.TotalMinutes).ToList()

        ' Nastavení fontu a barvy textu
        Me.rtbOutput.SelectionStart = Me.rtbOutput.Text.Length ' Pozice na konec textu
        Me.rtbOutput.SelectionFont = New Font("Calibri", 10) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Maroon ' Nastavit barvu
        Me.rtbOutput.AppendText(vbCrLf & My.Resources.Resource1.outProcessed_period_from & _gpxFilesManager.dateFrom.ToShortDateString & My.Resources.Resource1.outDo & _gpxFilesManager.dateTo.ToShortDateString &
                vbCrLf & My.Resources.Resource1.outAll_gpx_files_from_directory & ActiveCategoryInfo.RemoteDirectory & vbCrLf & vbCrLf)

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


        Me.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Firebrick
        Dim totalDistanceKm As Double = Me.GPXFilesManager.TotalDistancesKm.Last.totalDistanceKm '_gpxFilesManager.TotalDistance
        Me.rtbOutput.AppendText((totalDistanceKm).ToString("F1") & " km" & vbCrLf)


        Me.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Maroon
        Me.rtbOutput.AppendText((My.Resources.Resource1.outAverageDistance & manydots).Substring(0, labelLength))
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Firebrick
        Dim averageDistance As Double = GetAverage(Of Double)(_gpxRecords, Function(r) r.TrailStats.RunnerDistance)
        Me.rtbOutput.AppendText((averageDistance).ToString("F0") & " m" & vbCrLf)
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Maroon
        Me.rtbOutput.AppendText((My.Resources.Resource1.outAverageAge & manydots).Substring(0, labelLength))
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Firebrick
        Dim averageTrailAge As Double = GetAverage(Of TimeSpan?)(_gpxRecords, Function(r) r.TrailStats.TrailAge)
        Me.rtbOutput.AppendText(averageTrailAge.ToString("F2") & " h " & vbCrLf)
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Maroon
        Me.rtbOutput.AppendText((My.Resources.Resource1.outAverageSpeed & manydots).Substring(0, labelLength))
        Me.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
        Me.rtbOutput.SelectionColor = Color.Firebrick
        Dim averageDogSpeed As Double = GetAverage(_gpxRecords, Function(r) r.TrailStats.DogNetSpeed, ignoreZeros:=True)
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
                        If .TrailStats.TrailAge > TimeSpan.Zero Then
                            _age = .TrailStats.TrailAge.TotalHours.ToString("F1")
                        End If

                        ' Write each row in the CSV file
                        writer.Write($"{fileName};")
                        writer.Write($"{ .TrailStart.Time.ToString("yyyy-MM-dd")};")
                        writer.Write($"{_age};")
                        writer.Write($"{ .TrailDistance:F2};")
                        If Not .TrailStats.DogNetSpeed = 0 Then writer.Write($"{ .TrailStats.DogNetSpeed:F2};") Else writer.Write(";")
                        writer.Write($"{ .Description};")
                        If Not .Link Is Nothing Then
                            writer.WriteLine($"=HYPERTEXTOVÝ.ODKAZ(""{ .Link}"")")
                        End If
                    End With
                    writer.WriteLine()

                Next

                ' Write the total distance at the end of the CSV file
                writer.WriteLine($"Total;;; { GPXFilesManager.TotalDistancesKm.Last.totalDistanceKm:F2}")
            End Using


            WriteRTBWarning($"{vbCrLf}CSV file created: {csvFilePath}.{Environment.NewLine}", Color.DarkGreen)
        Catch ex As Exception
            Me.WriteRTBWarning($"{My.Resources.Resource1.mBoxErrorCreatingCSV}: {ex.Message}{Environment.NewLine}", Color.DarkGreen)
            MessageBox.Show($"Error creating CSV file: {ex.Message}")
        End Try
    End Sub


    Private Charts As New List(Of frmChart)
    Private Sub btnChartsClick(sender As Object, e As EventArgs) Handles btnCharts.Click
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

        chart1 = New frmChart(ActiveCategoryInfo.Name, GPXFilesManager.Speeds, Resource1.Y_AxisLabelSpeed, GPXFilesManager.dateFrom, GPXFilesManager.dateTo, Resource1.Y_AxisLabelSpeed, True, SeriesChartType.Point, Me.currentCulture)
        chart1.Show()
        Charts.Add(chart1)



        ' Získání dat pro graf stáří trasy
        chart1 = New frmChart(ActiveCategoryInfo.Name, GPXFilesManager.Ages, Resource1.Y_AxisLabelAge, GPXFilesManager.dateFrom, GPXFilesManager.dateTo, Resource1.Y_AxisLabelAge, True, SeriesChartType.Point, Me.currentCulture)
        chart1.Show()
        Charts.Add(chart1)




        'Difficulty indexes
        chart1 = New frmChart(ActiveCategoryInfo.Name, GPXFilesManager.DiffIndexes, "Trail Difficulty Index (h·km)", GPXFilesManager.dateFrom, GPXFilesManager.dateTo, "Trail Difficulty Index (h·km)", True, SeriesChartType.Point, Me.currentCulture)
        chart1.Show()
        Charts.Add(chart1)

        'Total Difficulty indexes

        chart1 = New frmChart(ActiveCategoryInfo.Name, GPXFilesManager.TotalDiffIndexes, "Total Trail Difficulty Index (h·km)", GPXFilesManager.dateFrom, GPXFilesManager.dateTo, "Total Trail Difficulty Index (h·km)", True, SeriesChartType.Point, Me.currentCulture)
        chart1.Show()
        Charts.Add(chart1)


        'Deviations
        chart1 = New frmChart(ActiveCategoryInfo.Name, GPXFilesManager.Deviations, Resource1.Y_AxisLabelDeviation, GPXFilesManager.dateFrom, GPXFilesManager.dateTo, Resource1.Y_AxisLabelDeviation, True, SeriesChartType.Point, Me.currentCulture)
        chart1.Show()
        Charts.Add(chart1)



        'Distances
        chart1 = New frmChart(ActiveCategoryInfo.Name, GPXFilesManager.DistancesKm, Resource1.Y_AxisLabelLength, GPXFilesManager.dateFrom, GPXFilesManager.dateTo, Resource1.Y_AxisLabelLength, True, SeriesChartType.Point, Me.currentCulture)
        chart1.Show()
        Charts.Add(chart1)


        'TotDistance
        chart1 = New frmChart(ActiveCategoryInfo.Name, Me.GPXFilesManager.TotalDistancesKm, Resource1.Y_AxisLabelTotalLength, GPXFilesManager.dateFrom, GPXFilesManager.dateTo, Resource1.Y_AxisLabelTotalLength, True, SeriesChartType.Point, Me.currentCulture)
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
                                                     Select New With {Month, .TotalDistanceKm = grp.Sum(Function(r) r.TrailDistance / 1000)}) On month Equals ms.Month Into gj = Group From subMs In gj.DefaultIfEmpty(New With {month, .TotalDistanceKm = 0.0})
                                   Select subMs

        ' Převedeme na pole pro graf
        Dim monthlyXAxisDataWithEmpty = monthlySumsWithEmpty.Select(Function(ms) ms.Month.ToString("MMMM yy", currentCulture)).ToArray
        Dim monthlyYAxisDataWithEmpty = monthlySumsWithEmpty.Select(Function(ms) ms.TotalDistanceKm).ToArray

        For Each s In monthlyXAxisDataWithEmpty
            Debug.WriteLine($"X:  {s}")
        Next
        For Each y In monthlyYAxisDataWithEmpty
            Debug.WriteLine($"Y: {y}")
        Next

        Dim monthlyYAxisLabel = Resource1.Y_AxisLabelMonthly  'My.Resources.Resource1.Y_AxisLabelLength ' Nebo jiný popisek pro osu Y
        Dim monthlyGrafText = monthlyYAxisLabel ' Např. "Měsíční vzdálenost"
        Dim MonthlyChart1 = New frmChart(ActiveCategoryInfo.Name, monthlyXAxisDataWithEmpty, monthlyYAxisDataWithEmpty, monthlyYAxisLabel, GPXFilesManager.dateFrom, GPXFilesManager.dateTo, monthlyGrafText, True, SeriesChartType.Column, currentCulture) ' Použijeme sloupcový graf (Column)
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

        ' Použijte plně kvalifikovaný název:
        Dim point As Point = PointToClient(New Point(System.Windows.Forms.Cursor.Position.X + 20, System.Windows.Forms.Cursor.Position.Y + 30))
        toolTipLabel.Location = point
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
        Dim menuIcon As Image = Me.mnuEnglish.Image
        Select Case currentCulture
            Case "cs-CZ", "cs"
                menuIcon = mnuCzech.Image
            Case "en-GB", "en", "en-US"
                menuIcon = Me.mnuEnglish.Image
            Case "de-DE", "de"
                menuIcon = Me.mnuGerman.Image
                'mnuGerman.Image = resizeImage(My.Resources.De_Flag, Nothing, 18)
            Case "pl-PL", "pl"
                menuIcon = Me.mnuPolish.Image
                'mnuPolish.Image = resizeImage(My.Resources.pl_flag, Nothing, 18)
            Case "ru-RU", "ru"
                menuIcon = Me.mnuRussian.Image
                'mnuRussian.Image = resizeImage(My.Resources.ru_flag, Nothing, 18)
            Case "uk"
                menuIcon = Me.mnuUkrainian.Image
                'mnuUkrainian.Image = resizeImage(My.Resources.uk_flag, Nothing, 18)
            Case Else
                ' Výchozí obrázek (např. angličtina)
                menuIcon = Me.mnuEnglish.Image
        End Select

        If menuIcon Is Nothing Then
            menuIcon = Me.mnuEnglish.Image ' Zajistí, že nebude Nothing
        End If
        ' Nastavení obrázku na ToolStripMenuItem

        mnuLanguage.Image = menuIcon
        Me.mnuPointInCompetition.Tag = Me.mnuPointInCompetition.Text

        'mnuCzech.Image = resizeImage(My.Resources.czech_flag, Nothing, 18)
    End Sub

    Private Function resizeImage(menuIcon As Image, width As Integer, height As Integer) As Image
        If menuIcon Is Nothing Then Return Nothing
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
            If ActiveCategoryInfo.RemoteDirectory = "" Then
                ActiveCategoryInfo.RemoteDirectory = Directory.GetParent(Application.StartupPath).ToString
            End If
            folderDialog.SelectedPath = ActiveCategoryInfo.RemoteDirectory
            folderDialog.Description = $"Select a folder from where to load gpx files for dog {ActiveCategoryInfo.Name}"
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
                ActiveCategoryInfo.RemoteDirectory = folderDialog.SelectedPath
                mbox($"The gpx files for the dog {ActiveCategoryInfo.Name} will be imported from the {ActiveCategoryInfo.RemoteDirectory} folder.")
                SaveUnifiedConfig()
                'SaveCategoriesInfo()
            ElseIf sender Is mnuSelectADirectoryToSaveVideo Or sender Is btnCreateVideos Then
                My.Settings.VideoDirectory = folderDialog.SelectedPath
                My.Settings.Save()
            Else
                Return ' Pokud není žádná z očekávaných položek menu, ukonči metodu
            End If

        End If


        StatusLabel1.Text = $"GPX files downloaded from: {ZkratCestu(ActiveCategoryInfo.RemoteDirectory, 130)}" & vbCrLf & $"Video exported to: {ZkratCestu(My.Settings.VideoDirectory, 130)}"

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

    Private unifiedConfig As UnifiedConfig ' Uložte si instanci této třídy na úrovni třídy/modulu

    Private Sub LoadUnifiedConfig()
        Dim UnifiedConfigPath As String = CategoriesInfoPath ' Definujte si novou cestu

        If File.Exists(UnifiedConfigPath) Then
            Dim json = File.ReadAllText(UnifiedConfigPath, Encoding.UTF8)
            Dim opts = New JsonSerializerOptions With {
            .PropertyNameCaseInsensitive = True ' Nastavení můžete ponechat
        }

            ' 1. Deserializace do kontejnerové třídy
            unifiedConfig = JsonSerializer.Deserialize(Of UnifiedConfig)(json, opts)
        Else
            ' 2. Pokud soubor neexistuje, vytvořte novou instanci s výchozími hodnotami
            unifiedConfig = New UnifiedConfig()
        End If

        ' 3. Přiřazení hodnot do vašich stávajících proměnných (pro snadnou integraci)
        CategoriesInfo = unifiedConfig.CategoriesInfo

        If unifiedConfig.activeDoguration IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(unifiedConfig.activeDoguration.ActiveDogId) Then
            ActiveCategoryId = unifiedConfig.activeDoguration.ActiveDogId
        Else
            ' Zajistěte, že je activeDoguration alespoň inicializovaný, pokud se v souboru nenašel
            unifiedConfig.activeDoguration = New activeDog()
        End If
    End Sub
    'Private Sub LoadCategories()
    '    If File.Exists(CategoriesInfoPath) Then
    '        Dim json = File.ReadAllText(CategoriesInfoPath, Encoding.UTF8)
    '        Dim opts = New JsonSerializerOptions With {
    '            .PropertyNameCaseInsensitive = True
    '        }
    '        CategoriesInfo = JsonSerializer.Deserialize(Of List(Of CategoryInfo))(json, opts)
    '    Else
    '        CategoriesInfo = New List(Of CategoryInfo)()
    '    End If
    'End Sub


    Private Sub SaveUnifiedConfig()
        Dim UnifiedConfigPath As String = CategoriesInfoPath ' Stejná cesta jako při načítání

        ' 1. Aktualizace dat v kontejneru
        ' Předpokládáme, že vaše CategoriesInfo je už aktualizováno
        unifiedConfig.CategoriesInfo = CategoriesInfo

        ' Aktualizace activeDog, ve kterém se nachází ActiveDogId
        unifiedConfig.activeDoguration.ActiveDogId = ActiveCategoryId


        Dim options As New JsonSerializerOptions With {
        .WriteIndented = True,
        .Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    }

        ' 2. Serializace celé kontejnerové třídy
        Directory.CreateDirectory(Path.GetDirectoryName(UnifiedConfigPath))
        File.WriteAllText(UnifiedConfigPath, JsonSerializer.Serialize(unifiedConfig, options), Encoding.UTF8)
    End Sub

    'Private Sub SaveCategoriesInfo()
    '    Dim options As New JsonSerializerOptions With {
    '        .WriteIndented = True,
    '        .Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping ' aby se diakritika neescapeovala
    '    }
    '    Directory.CreateDirectory(Path.GetDirectoryName(CategoriesInfoPath))
    '    File.WriteAllText(CategoriesInfoPath, JsonSerializer.Serialize(CategoriesInfo, options), Encoding.UTF8)
    'End Sub

    ' ----- načtení a uložení config.json (aktivní pes) -----
    'Private Sub LoadConfig()
    '    If File.Exists(ConfigPath) Then
    '        Dim json = File.ReadAllText(ConfigPath, Encoding.UTF8)
    '        Dim cfg = JsonSerializer.Deserialize(Of activeDog)(json)
    '        If cfg IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(cfg.ActiveDogId) Then
    '            ActiveDogId = cfg.ActiveDogId
    '        End If
    '    End If
    'End Sub

    'Private Sub SaveConfig()
    '    Dim options As New JsonSerializerOptions With {
    '        .WriteIndented = True,
    '        .Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    '    }
    '    Dim cfg As New activeDog With {.ActiveDogId = ActiveDogId}
    '    File.WriteAllText(ConfigPath, JsonSerializer.Serialize(cfg, options), Encoding.UTF8)
    'End Sub

    ' ----- naplnění ToolStripComboBoxu objekty CategoryInfo -----
    Private Sub PopulateCategoriesToolStrip()
        mnucbActiveCategory.ComboBox.Items.Clear()

        For Each d As CategoryInfo In CategoriesInfo
            mnucbActiveCategory.ComboBox.Items.Add(d) ' zobrazí se Name díky ToString()
        Next

        ' pokud je nastaven ActiveDogId, vyber ho; jinak vyber první
        Dim selectedIndex As Integer = -1
        If Not String.IsNullOrEmpty(ActiveCategoryId) Then
            For i As Integer = 0 To mnucbActiveCategory.ComboBox.Items.Count - 1
                Dim item As CategoryInfo = CType(mnucbActiveCategory.ComboBox.Items(i), CategoryInfo)
                If item.Id = ActiveCategoryId Then
                    selectedIndex = i
                    Exit For
                End If
            Next
        End If

        If selectedIndex = -1 AndAlso mnucbActiveCategory.ComboBox.Items.Count > 0 Then selectedIndex = 0

        If selectedIndex >= 0 Then
            mnucbActiveCategory.ComboBox.SelectedIndex = selectedIndex
            ' zajistí, že ActiveDogId drží hodnotu
            Dim sel = TryCast(mnucbActiveCategory.ComboBox.SelectedItem, CategoryInfo)
            If sel IsNot Nothing Then
                ActiveCategoryId = sel.Id
                'lblActiveDog.Text = $"Aktivní pes: {sel.Name} ({sel.Id})"
            End If
        Else
            'lblActiveDog.Text = "Aktivní pes: (není)"
        End If
    End Sub

    ' ----- handler při změně výběru v ToolStripComboBoxu -----
    Private Sub mnucbActiveCategory_SelectedIndexChanged(sender As Object, e As EventArgs) Handles mnucbActiveCategory.SelectedIndexChanged
        Dim selected = TryCast(mnucbActiveCategory.ComboBox.SelectedItem, CategoryInfo)
        If selected Is Nothing Then Return
        SetScrollState(1, selected.Id) ' nastavíme scroll state pro aktivního psa
        ActiveCategoryId = selected.Id
        'My.Settings.ActiveDog = sel.Id ' uložíme jméno psa do nastavení
        ''lblActiveDog.Text = $"Aktivní pes: {sel.Name} ({sel.Id})"
        'My.Settings.Save()
        ' uložíme config (aby se volba pamatovala)
        SaveUnifiedConfig()
        'SaveConfig()
        ClearDgvCompetition()
        lvGpxFiles.Items.Clear()
        ' tady můžeš volat další inicializace pro aktivního psa
        ' e.g. RefreshOriginalsFolderForActiveDog()
    End Sub

    ' ----- příklad: přidání nového psa (mnuAddNewCategory click) -----
    Private Sub mnuAddNewCategory_Click(sender As Object, e As EventArgs) Handles mnuAddNewCategory.Click
        Dim categoryName = InputBox("Enter the name of the new Category:", "New Category")
        If String.IsNullOrWhiteSpace(categoryName) Then
            MessageBox.Show("The name of the category was not given.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' vytvoříme nové ID (trojciferné)
        Dim newId As String = If(CategoriesInfo.Count = 0, "001", (CategoriesInfo.Select(Function(d) Integer.Parse(d.Id)).Max() + 1).ToString("D3"))
        ' vytvoříme složky

        Dim categoryRemotePath = ActiveCategoryInfo.RemoteDirectory 'Path.Combine(My.Settings.Directory, dogName)
        Dim folderDialog As New FolderBrowserDialog()
        folderDialog.SelectedPath = categoryRemotePath
        folderDialog.Description = $"Selecting the folder from where to load gpx files for the category {categoryName}."
        folderDialog.UseDescriptionForTitle = True
        If False = (folderDialog.ShowDialog() = DialogResult.OK) Then
            ' pokud uživatel zrušil výběr, skončíme
            MessageBox.Show("The category was not added.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        ElseIf Directory.Exists(folderDialog.SelectedPath) Then
            categoryRemotePath = folderDialog.SelectedPath
        Else
            MessageBox.Show("The selected folder does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If

        ' přidáme do seznamu a uložíme
        CategoriesInfo.Add(New CategoryInfo With {.Id = newId,
                     .Name = categoryName,
                     .RemoteDirectory = categoryRemotePath})
        SaveUnifiedConfig()
        'SaveCategoriesInfo()



        ' refresh UI: vložíme nového psa do comboboxu a vybereme ho
        PopulateCategoriesToolStrip()
        ' vybrat nového psa:
        For i As Integer = 0 To mnucbActiveCategory.ComboBox.Items.Count - 1
            Dim d As CategoryInfo = CType(mnucbActiveCategory.ComboBox.Items(i), CategoryInfo)
            If d.Id = newId Then mnucbActiveCategory.ComboBox.SelectedIndex = i : Exit For
        Next

        MessageBox.Show($"Category '{categoryName}' added with ID {newId}.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)
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

    Private Sub TabControl1_DrawItem(sender As Object, e As DrawItemEventArgs) Handles TabControl1.DrawItem
        Dim tabControl = DirectCast(sender, TabControl)
        Dim tabPage = tabControl.TabPages(e.Index)
        Dim rect = tabControl.GetTabRect(e.Index)
        Dim brush As Brush

        Select Case e.Index
            Case 0
                brush = New SolidBrush(Color.Goldenrod) ' Zvolte požadovanou barvu
            Case 1
                brush = New SolidBrush(Color.DarkSeaGreen)'DarkSeaGreen)'zelená
            Case 2
                brush = New SolidBrush(Color.Salmon) ' Zvolte požadovanou barvu
                ' Zvolte požadovanou barvu
            Case Else
                brush = New SolidBrush(SystemColors.Control)
        End Select

        e.Graphics.FillRectangle(brush, rect)
        TextRenderer.DrawText(e.Graphics, tabPage.Text, tabControl.Font, rect, tabPage.ForeColor)
        brush.Dispose()
    End Sub

    Private Async Sub btnCreateVideos_Click(sender As Object, e As EventArgs) Handles btnCreateVideos.Click

        If lvGpxFiles.CheckedItems.Count > 2 Then
            If mboxq($"Are you sure you want to create videos from {lvGpxFiles.CheckedItems.Count} gpx records now? It can take a long time and there's no chance to stop it 🤣!") = DialogResult.No Then
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
                record.Description = record.BuildLocalisedDescription(record.Description) 'async kvůli počasí!
                record.WriteDescription() 'zapíše agregovaný popis do tracku Runner
                record.BuildLocalisedPerformancePoints()
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
        Dim videoCreator As New VideoExportManager(FFmpegPath, directory, _gpxRecord.WeatherData.windDirection, _gpxRecord.WeatherData.windSpeed)
        AddHandler videoCreator.WarningOccurred, AddressOf WriteRTBWarning

        Dim waitForm As New frmPleaseWait("I'm making an overlay video, please stand by...")
        waitForm.Show()

        ' Spustíme na pozadí, aby nezamrzlo UI
        Await Task.Run(Async Function()
                           ' Spustíme tvůj dlouhý proces
                           Dim success = Await videoCreator.CreateVideoFromTrkNodes(_gpxRecord.Tracks, _gpxRecord.TrailStats.MaxDeviationGeoPoints, _gpxRecord.WptNodes, _gpxRecord.LocalisedReports)

                           ' Po dokončení se vrať na UI thread a proveď akce
                           waitForm.Invoke(Sub()
                                               waitForm.Close()

                                               If success Then
                                                   Dim videopath As String = IO.Path.Combine(directory.FullName, "overlay.webm")
                                                   Dim bgPNGPath As String = IO.Path.Combine(directory.FullName, "TracksOnMap.png")
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
        If (e.TabPage Is TabVideoExport Or e.TabPage Is TabCompetition) AndAlso lvGpxFiles.Items.Count = 0 Then
            mboxEx("This tab is not ready yet." & vbCrLf & "First you need to load the gpx files - click on the salmon button!")
            e.Cancel = True
        ElseIf e.TabPage Is TabCompetition AndAlso dgvCompetition.RowCount <= 0 Then
            Me.FillDgvCompetition()  'naplní datagridView s přehledem tras a jejich statistikami pro závody

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
            'Dim url As String = "https://api.github.com/repos/mwrnckx/K9-Trails-Analyzer/releases/latest"
            'Dim client As New Net.WebClient()
            'client.Headers.Add("User-Agent", "K9TrailsAnalyzer")
            'Dim content As String = client.DownloadString(url)

            Dim url As String = "https://api.github.com/repos/mwrnckx/K9-Trails-Analyzer/releases/latest"

            Using client As New Net.Http.HttpClient()
                client.DefaultRequestHeaders.Add("User-Agent", "K9TrailsAnalyzer")

                ' Synchronní verze
                Dim response As String = client.GetStringAsync(url).Result

                ' Pokračujte se zpracováním response...


                Dim json As JsonDocument = JsonDocument.Parse(response)
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
            End Using
        Catch ex As Exception
            MessageBox.Show("The availability of the new version could not be ascertained." & vbCrLf & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub Form1_ResizeEnd(sender As Object, e As EventArgs) Handles Me.ResizeEnd
        My.Settings.WindowSize = Me.Size
    End Sub

    Private Sub DeleteCurrentDogToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles mnuDeleteCurrentCategory.Click
        ' Najdi psa podle ID
        Dim dogId As String = ActiveCategoryId
        Dim dogToRemove = CategoriesInfo.FirstOrDefault(Function(d) d.Id = dogId)
        If dogToRemove Is Nothing Then
            MessageBox.Show("Dog not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return
        End If
        If CategoriesInfo.Count = 1 Then
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
        CategoriesInfo.Remove(dogToRemove)

        ' Ulož seznam zpět do JSON
        SaveUnifiedConfig()
        'SaveCategoriesInfo()

        ' Pokud byl aktivní, přepnout na jiného
        If ActiveCategoryId = dogId Then
            ActiveCategoryId = If(CategoriesInfo.Count > 0, CategoriesInfo(0).Id, String.Empty)
        End If

        ' Obnov UI (combo box apod.)
        PopulateCategoriesToolStrip()

        MessageBox.Show($"The dog '{dogToRemove.Name}' has been deleted.", "Done", MessageBoxButtons.OK, MessageBoxIcon.Information)


    End Sub

    Private Sub mnuRenameCurrentCategory_Click(sender As Object, e As EventArgs) Handles mnuRenameCurrentCategory.Click


        'Dim selectedDog As CategoryInfo = CType(mnucbActiveCategory.ComboBox.SelectedItem, CategoryInfo)
        If ActiveCategoryInfo IsNot Nothing Then
            Dim newName As String = InputBox("Enter the dog's new name:", "Rename the dog", ActiveCategoryInfo.Name)
            If Not String.IsNullOrWhiteSpace(newName) AndAlso newName <> ActiveCategoryInfo.Name Then
                If CategoriesInfo.Any(Function(d) d.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)) Then
                    MessageBox.Show("A dog with this name already exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Else
                    ActiveCategoryInfo.Name = newName
                    ' případně uložit do JSON

                    SaveUnifiedConfig()
                    'SaveCategoriesInfo()
                    PopulateCategoriesToolStrip()
                End If
            End If
        End If

    End Sub


    Private Sub dgvCompetition_ColumnHeaderMouseClick(sender As Object, e As DataGridViewCellMouseEventArgs) Handles dgvCompetition.ColumnHeaderMouseClick
        Dim columnName As String = dgvCompetition.Columns(e.ColumnIndex).DataPropertyName

        ' Zjištění směru seřazení
        If sortColumnName = columnName Then
            ' Přepnutí směru, pokud se kliklo na stejný sloupec
            sortDirection = IIf(sortDirection = SortOrder.Ascending, SortOrder.Descending, SortOrder.Ascending)
        Else
            ' První kliknutí na nový sloupec: seřaď vzestupně
            sortDirection = SortOrder.Ascending
            sortColumnName = columnName
        End If

        ' Kontrola, že je v DisplayList něco k seřazení
        If Me.displayList IsNot Nothing AndAlso Me.displayList.Count > 0 Then
            If sortDirection = SortOrder.Ascending Then
                ' Vzestupné seřazení (Ascending)
                Me.displayList = Me.displayList.OrderBy(Function(x) GetPropertyValue(x, columnName)).ToList()
            Else
                ' Sestupné seřazení (Descending)
                Me.displayList = Me.displayList.OrderByDescending(Function(x) GetPropertyValue(x, columnName)).ToList()
            End If

            ' Znovu přiřaď seřazený list jako zdroj dat
            Me.dgvCompetition.DataSource = Nothing ' Odpojit starý zdroj
            Me.bsCompetitions.DataSource = Me.displayList 'bindingSource kvůli řazení sloupců
            Me.dgvCompetition.DataSource = Me.bsCompetitions
            'znovu zformátovat dgvCompetition:
            Me.FormatDgvCompetition()
        End If

        ' Volitelné: Zobrazení šipky pro indikaci seřazení
        SetSortGlyph(e.ColumnIndex, sortDirection)
    End Sub

    ' --- Pomocné funkce pro dynamické získání hodnoty vlastnosti (Reflection) ---
    ' Tato funkce je klíčová pro Linq seřazení, protože nevíme typ T v List(Of T)
    Private Function GetPropertyValue(ByVal obj As Object, ByVal propertyName As String) As Object
        Return obj.GetType().GetProperty(propertyName).GetValue(obj, Nothing)
    End Function

    ' --- Pomocná funkce pro zobrazení šipky (volitelné) ---
    Private Sub SetSortGlyph(ByVal columnIndex As Integer, ByVal direction As SortOrder)
        For Each col As DataGridViewColumn In dgvCompetition.Columns
            col.HeaderCell.SortGlyphDirection = SortOrder.None
        Next

        dgvCompetition.Columns(columnIndex).HeaderCell.SortGlyphDirection = direction
    End Sub

    Private Sub dgvCompetition_CellEndEdit(sender As Object, e As DataGridViewCellEventArgs) Handles dgvCompetition.CellEndEdit
        '// Krok 1: Zjistěte, zda je editovaný sloupec ten, který má spustit přepočet
        '// Např. sloupec s indexem 0 se změní
        If (e.ColumnIndex = 0) Then
            ' Pokud ano, pokračujte k přepočtu
            '// Krok 2: Proveďte přepočet a aktualizaci
            'RecalculateRow(e.RowIndex)
        End If
    End Sub

    Private Sub mnuPointsForFind_LostFocus(sender As Object, e As EventArgs) Handles mnuPointInCompetition.DropDownClosed
        'Todo když se změní hodnoty bodů v menu, přepočítat body ve všech záznamech
        'ideálně až se změní všechny body a zavře se menu tedy asi při kliknutí mimo menu, snad je toto správná událost
        'For Each record As TrailStatsDisplay In displayList?
    End Sub
End Class

''' <summary>
''' structure for returning calculation results.
''' </summary>
''' 


Public Class TrailStatsDisplay
    Implements INotifyPropertyChanged ' Implementace rozhraní
    ' Interní reference na původní data
    Private _originalRecord As GPXRecord

    Friend Property OriginalRecord As GPXRecord
        Get
            Return _originalRecord
        End Get
        Set(value As GPXRecord)
            _originalRecord = value
        End Set
    End Property
    ' Událost INotifyPropertyChanged
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    ' Metoda pro spuštění události PropertyChanged
    Protected Sub OnPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub

    ' Privátní proměnné pro zálohované pole (backing fields)
    Private _runnerFoundPoints As Integer
    Private _dogSpeedPoints As Integer
    Private _dogAccuracyPoints As Integer
    Private _dogReadingPoints As Integer
    Private _totalPoints As Integer
    Private _DogName As String ' Přidáme i DogName, protože to je editovatelný sloupec
    Private _HandlerName As String
    Private _ranking As String

    <DisplayName("File")>
    Public Property GPXFilename As String ' 

    <DisplayName("Start")>
    Public Property StartTime As Date ' start time of the trail (from the runner's track)


    <DisplayName("Dog")>
    Public Property DogName As String
        Get
            Return _DogName
        End Get
        Set(value As String)
            If _DogName <> value Then
                _DogName = value
                OnPropertyChanged(NameOf(DogName))
            End If
        End Set
    End Property


    <DisplayName("Handler")>
    Public Property HandlerName As String
        Get
            Return _HandlerName
        End Get
        Set(value As String)
            If _HandlerName <> value Then
                _HandlerName = value
                OnPropertyChanged(NameOf(HandlerName))
            End If
        End Set
    End Property


    ' 2. VLASTNOSTi, KTERÁ SE PŘEPOČÍTÁVÁJÍ:
    <DisplayName("Ranking")>
    Public Property Ranking As String ' 
        Get
            Return _ranking
        End Get
        Set(value As String)
            If _ranking <> value Then
                _ranking = value
                OnPropertyChanged(NameOf(Ranking))
            End If
        End Set
    End Property

    <DisplayName("Total Points")>
    Public ReadOnly Property TotalPoints As Integer
        Get
            Return _totalPoints
        End Get
    End Property

    ' 1. VLASTNOSTI, KTERÉ MŮŽE UŽIVATEL EDITOVAT:
    ' U těchto vlastností přidáme logiku INotifyPropertyChanged a automatický přepočet

    <DisplayName("Successful Find Points")>
    Public Property RunnerFoundPoints As Integer
        Get
            Return _runnerFoundPoints
        End Get
        Set(value As Integer)
            If _runnerFoundPoints <> value Then
                _runnerFoundPoints = value
                OnPropertyChanged(NameOf(RunnerFoundPoints))

                ' Zde voláme metodu pro přepočet, která zajistí,
                ' že se aktualizuje i TotalPoints
                CalculateTotalPoints()
            End If
        End Set
    End Property



    <DisplayName("Dog Speed Points")>
    Public Property DogSpeedPoints As Integer
        Get
            Return _dogSpeedPoints
        End Get
        Set(value As Integer)
            If _dogSpeedPoints <> value Then
                _dogSpeedPoints = value
                OnPropertyChanged(NameOf(DogSpeedPoints))
                CalculateTotalPoints()
            End If
        End Set
    End Property

    <DisplayName("Dog Accuracy Points")>
    Public Property DogAccuracyPoints As Integer
        Get
            Return _dogAccuracyPoints
        End Get
        Set(value As Integer)
            If _dogAccuracyPoints <> value Then
                _dogAccuracyPoints = value
                OnPropertyChanged(NameOf(DogAccuracyPoints))
                CalculateTotalPoints()
            End If
        End Set
    End Property

    <DisplayName("Dog Reading Points")>
    Public Property dogReadingPoints As Integer
        Get
            Return _dogReadingPoints
        End Get
        Set(value As Integer)
            If _dogReadingPoints <> value Then
                _dogReadingPoints = value
                OnPropertyChanged(NameOf(dogReadingPoints))
                CalculateTotalPoints()
            End If
        End Set
    End Property

    '3. Ostatní vlastnosti (GPXFilename, StartTime, DogName atd.) mohou zůstat jako Auto-Implemented Properties,
    ' protože jejich hodnota by se neměla po startu měnit, nebo je nemusíte notifikovat.
    ' 

    <DisplayName("Dog Speed km/h")>
    Public Property DogGrossSpeedKmh As Double 'gross speed calculated from the last checkpoint or the dog's last point if the dog is close to the track

    <DisplayName("Max. Team Distance")>
    Public Property MaxTeamDistance As Double 'where on the trail the team reached (measured to the last checkpointu)

    <DisplayName("Average Deviation")>
    Public Property Deviation As Double ' average deviation of the entire dog's route from the runner's track weighted by time

    <DisplayName("Trail Distance")>
    Public Property RunnerDistance As Double ' Distance actually traveled by the runner (measured from the runner's route)

    <DisplayName("Total Time min")>
    Public Property TotalTime As Double ' total time of the dog's route

    <DisplayName("Trail Age min")>
    Public Property TrailAge As Double ' age of the trail 

    <DisplayName("Weighted Time")>
    Public Property WeightedTimePerCent As Double ' Distance traveled by the dog as measured from the runners's route with weighting by deviation

    <DisplayName("Weighted Distance Along Trail")>
    Public Property WeightedDistanceAlongTrailPerCent As Double ' Distance traveled by the dog as measured from the runners's route with weighting by deviation

    <DisplayName("Weighted Distance Along Trail")>
    Public Property WeightedDistanceAlongTrail As Double ' Distance traveled by the dog as measured from the runners's route with weighting by deviation

    <DisplayName("Dog Distance")>
    Public Property DogDistance As Double ' Distance actually traveled by the dog (measured from the dog's route)


    <DisplayName("1th Checkpoint Distance")>
    Public Property FirstCheckpointEvalDistance As Double = 0

    <DisplayName("1th Checkpoint Deviation")>
    Public Property FirstCheckpointEvalDeviationFromTrail As Double = 0
    <DisplayName("1th Checkpoint dogSpeed km/h")>
    Public Property FirstCheckpointEvaldogGrossSpeed As Double = 0 ' evaluation of checkpoints: distance from start along the runner's route and distance from the route in meters
    <DisplayName("Last Checkpoint Distance")>
    Public Property SecondCheckpointEvalDistance As Double = 0
    <DisplayName("Last Checkpoint Deviation")>
    Public Property SecondCheckpointEvalDeviationFromTrail As Double = 0
    <DisplayName("Last Checkpoint dogSpeed km/h")>
    Public Property SecondCheckpointEvaldogGrossSpeed As Double = 0

    <DisplayName("Runner name")>
    Public Property RunnerName As String '


    ' 3. METODA PRO PŘEPOČET

    Public Sub CalculateTotalPoints()
        Dim newTotal As Integer = Me.RunnerFoundPoints + Me.DogSpeedPoints + Me.DogAccuracyPoints + Me.dogReadingPoints

        ' Nastavíme novou hodnotu, ale pouze pokud se liší, abychom zamezili zbytečným notifikacím
        If _totalPoints <> newTotal Then
            _totalPoints = newTotal

            ' KLÍČOVÝ KROK: Oznámíme, že se TotalPoints změnilo
            OnPropertyChanged(NameOf(TotalPoints))
        End If
    End Sub


End Class





' --- model psa ---
''' <summary>
''' Information about the selected dog/Competition category
''' </summary>
''' <!--
''' id: unique ID of the dog (three digits, e.g. "001")
''' name: name of the dog (e.g. "Rex")
''' remoteDirectory: path to the directory where the gpx files for this dog are stored (e.g. "D:\GPXFiles\Rex")
''' -->
Public Class CategoryInfo
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

                _remoteDirectory = Path.Combine(Application.StartupPath, "Samples", Me.Id)
            End If
            Return _remoteDirectory
        End Get
        Set(value As String)
            _remoteDirectory = value
        End Set
    End Property

    ' Konfigurační body pro disciplíny (uživatelsky nastavitelné)
    <JsonPropertyName("PointsForFindMax")>
    Public Property PointsForFindMax As Integer = 100

    <JsonPropertyName("PointsPerKmhGrossSpeed")>
    Public Property PointsPerKmhGrossSpeed As Double = 20

    <JsonPropertyName("PointsForAccuracyMax")>
    Public Property PointsForAccuracyMax As Integer = 100

    <JsonPropertyName("PointsForDogReadingMax")>
    Public Property PointsForDogReadingMax As Integer = 100


    <JsonIgnore>
    Public ReadOnly Property LocalBaseDirectory As String
        Get
            Dim _directory = Path.Combine(Application.StartupPath, "Categories", Id)
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
Public Class activeDog
    <JsonPropertyName("activeDogId")>
    Public Property ActiveDogId As String
End Class


Public Class UnifiedConfig
    <JsonPropertyName("categoriesInfo")>
    Public Property CategoriesInfo As List(Of CategoryInfo)

    <JsonPropertyName("activeDog")>
    Public Property activeDoguration As activeDog

    ' Volitelně můžete přidat konstruktor pro inicializaci seznamu
    Public Sub New()
        CategoriesInfo = New List(Of CategoryInfo)()
        activeDoguration = New activeDog() ' Zajistí, že activeDog není Nothing
    End Sub
End Class