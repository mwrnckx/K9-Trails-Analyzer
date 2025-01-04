Imports System.Diagnostics.Eventing
Imports System.Globalization
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Windows.Forms.LinkLabel
Imports System.Xml
Imports GPXTrailAnalyzer.GPXDistanceCalculator
Imports GPXTrailAnalyzer.My.Resources
Public Class GpxFileManager
    'obsahuje seznam souborů typu gpxRecord a funkce na jejich vytvoření a zpracování
    Public ReadOnly Property gpxDirectory As String
    Private ReadOnly Property BackupDirectory As String
    Public dateFrom As Date
    Public dateTo As Date
    Private ReadOnly maxAge As TimeSpan
    Private ReadOnly prependDateToName As Boolean
    Private ReadOnly trimGPS_Noise As Boolean
    Private ReadOnly mergeDecisions As System.Collections.Specialized.StringCollection

    Private mergeNoAsk As Boolean = False 'if yes merge runner and dog trails without asking
    Private mergeCancel As Boolean = False 'don't merge 

    Public Property GpxRecords As New List(Of GPXRecord)

    Public Event WarningOccurred(message As String)

    Public Sub New()
        gpxDirectory = My.Settings.Directory
        BackupDirectory = My.Settings.BackupDirectory
        If Not Directory.Exists(BackupDirectory) Then
            Directory.CreateDirectory(BackupDirectory)
        End If
        maxAge = New TimeSpan(My.Settings.maxAge, 0, 0)
        prependDateToName = My.Settings.PrependDateToName
        trimGPS_Noise = My.Settings.TrimGPSnoise
        mergeDecisions = My.Settings.MergeDecisions

    End Sub

    Public Function Main() As Boolean
        Dim _gpxFilesSortedAndFiltered As List(Of GPXRecord) = ReadGPXFilesWithinInterval()
        Dim _gpxFilesMerged As List(Of GPXRecord) = MergeLayerAndDog(_gpxFilesSortedAndFiltered)

        Dim totalDist As Double = 0

        For Each _gpxRecord As GPXRecord In _gpxFilesMerged
            Try

                _gpxRecord.RenamewptNode(My.Resources.Resource1.article) 'renaming wpt to "artickle"
                If prependDateToName Then _gpxRecord.PrependDateToFilename()
                If trimGPS_Noise Then _gpxRecord.TrimGPSnoise(12)
                _gpxRecord.SplitTrackIntoTwo() 'in gpx files, splits a track with two segments into two separate tracks
                _gpxRecord.Description = _gpxRecord.GetDescription() 'musí být první - slouží k výpočtu age
                If trimGPS_Noise Then _gpxRecord.TrimGPSnoise(12) 'ořízne nevýznamné konce a začátky trailů, když se stojí na místě.
                _gpxRecord.Distance = _gpxRecord.CalculateFirstSegmentDistance()
                totalDist += _gpxRecord.Distance
                _gpxRecord.TotalDistance = totalDist
                _gpxRecord.DogStart = _gpxRecord.GetDogStart
                _gpxRecord.DogFinish = _gpxRecord.GetDogFinish
                _gpxRecord.TrailAge = _gpxRecord.CalculateAge
                _gpxRecord.DogSpeed = _gpxRecord.CalculateSpeed
                _gpxRecord.Link = _gpxRecord.Getlink

                _gpxRecord.Reader.Save() 'hlavně kvůli desc
                'a nakonec
                _gpxRecord.SetCreatedModifiedDate()
            Catch ex As Exception
                MessageBox.Show($"Reading or processing of the file {_gpxRecord.Reader.FileName} failed.")
            End Try
        Next _gpxRecord
        GpxRecords = _gpxFilesMerged
        If GpxRecords IsNot Nothing Then
            Return True
        Else
            Return False
        End If

    End Function



    Public Function ReadGPXFilesWithinInterval() As List(Of GPXRecord)
        Dim gpxFilesWithinInterval As New List(Of GPXRecord)
        ' Načteme všechny GPX soubory
        Dim gpxFilesAllPath As List(Of String) = Directory.GetFiles(gpxDirectory, "*.gpx").ToList()

        Try
            For Each gpxFilePath In gpxFilesAllPath
                'Tady najde layerStart 
                Dim _reader As New GpxReader(gpxFilePath) With {
                    .FileName = Path.GetFileName(gpxFilePath)
                    }

                Dim _layerStart As DateTime = GetLayerStart(gpxFilePath, _reader)
                If _layerStart >= dateFrom And _layerStart <= dateTo Then
                    Dim _gpxRecord As New GPXRecord With {
                        .Reader = _reader,
                        .LayerStart = _layerStart
                    }
                    _gpxRecord.Backup()
                    gpxFilesWithinInterval.Add(_gpxRecord)

                End If
            Next
            Debug.WriteLine($"Soubory gpx byly úspěšně zálohovány do: {BackupDirectory }")
            RaiseEvent WarningOccurred($"{vbCrLf}{Resource1.logBackupOfFiles}   {BackupDirectory }{vbCrLf}")
        Catch ex As Exception
            Debug.WriteLine($"Chyba při zálohování souborů: {ex.Message}")
        End Try
        ' Seřazení podle data
        gpxFilesWithinInterval.Sort(Function(x, y) x.LayerStart.CompareTo(y.LayerStart))

        Return gpxFilesWithinInterval
    End Function

    ' Function to read the time from the first <time> node in the GPX file
    ' If <time> node doesnt exist tries to read date from file name and creates <time> node
    Private Function GetLayerStart(filePath As String, reader As GpxReader) As DateTime
        Dim layerStart As DateTime

        ' Načtení jednoho uzlu <time>
        Dim LayerStartTimeNode As XmlNode = reader.SelectSingleNode("time")

        Dim RecordedDateFromFileName As DateTime
        Dim filename As String = Path.GetFileNameWithoutExtension(filePath)
        If Regex.IsMatch(filename, "^\d{4}-\d{2}-\d{2}") Then
            ' Extrahování data z názvu souboru
            Dim dateMatch As Match = Regex.Match(filename, "^\d{4}-\d{2}-\d{2}")
            If dateMatch.Success Then
                ' Převedení nalezeného řetězce na DateTime
                RecordedDateFromFileName = DateTime.ParseExact(dateMatch.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture)
            End If
        End If

        ' Check if the <time> node exists and has a valid value
        If LayerStartTimeNode IsNot Nothing AndAlso DateTime.TryParse(LayerStartTimeNode.InnerText, layerStart) Then

            'keeps value from file
        ElseIf RecordedDateFromFileName <> Date.MinValue Then
            'pokusí se odečíst datum z názvu souboru a vytvořit uzel <time>
            ' Převedení nalezeného řetězce na DateTime
            layerStart = RecordedDateFromFileName
            AddTimeNodeToFirstTrkpt(reader, RecordedDateFromFileName.ToString("yyyy-MM-dd" & "T" & "hh:mm:ss" & "Z"))
            RaiseEvent WarningOccurred($" <time> node with Date from file name created: {RecordedDateFromFileName.ToString("yyyy-MM-dd")}" & $"in file: {filename}")
        Else
            ' If the node doesn't exist or isn't a valid date, return the default DateTime value
            RaiseEvent WarningOccurred($"GPX file: {filename} contains no date!")
        End If

        Return layerStart
    End Function

    Sub AddTimeNodeToFirstTrkpt(gpxReader As GpxReader, timeValue As String)

        ' Vyhledání prvního uzlu <trkpt>
        Dim firstTrkptNode As XmlNode = gpxReader.SelectSingleNode("trkpt")
        Dim save As Boolean = False

        If firstTrkptNode IsNot Nothing Then
            gpxReader.CreateElement(firstTrkptNode, "time", timeValue)
            save = True
            Debug.WriteLine("Časový uzel byl úspěšně přidán.")
        Else
            Debug.WriteLine("Uzel <trkpt> nebyl nalezen.")
        End If

        gpxReader.Save()

    End Sub





    Public Function PrependDateToFilename(_gpxRecord As GPXRecord)

        Dim _gpxFilePath As String = _gpxRecord.Reader.filePath
        Dim _gpxFileName As String = _gpxRecord.Reader.FileName
        Dim fileExtension As String = Path.GetExtension(_gpxFilePath)
        Dim _layerStart As DateTime = _gpxRecord.LayerStart

        Dim newFileName As String = _gpxFileName
        Dim newFilePath As String = _gpxFilePath 'pokud se nezmění zůstane původní hodnota


        Dim dateTimeFromFileName As DateTime
        Try
            'Pokusí se najít datum v názvu souboru:
            ' Regex s pojmenovanými skupinami pro celé formáty i jednotlivé části data
            Dim pattern As String = "(?<format1>T(?<year1>\d{4})-(?<month1>\d{2})-(?<day1>\d{2})-(?<hour1>\d{2})-(?<minute1>\d{2}))|" &
                                "(?<format2>(?<year2>\d{4})-(?<month2>\d{2})-(?<day2>\d{2})_(?<hour2>\d{2})-(?<minute2>\d{2}))|" &
                                "(?<format3>(?<day3>\d{1,2})\._(?<month3>\d{2})\._(?<year3>\d{4})_(?<hour3>\d{1,2})_(?<minute3>\d{2})_(?<second3>\d{2}))|" &
                                "(?<format4>(?<year4>\d{4})-(?<month4>\d{2})-(?<day4>\d{2}))"
            Dim myRegex As New Regex(pattern)

            Dim match As Match = myRegex.Match(_gpxFileName)
            If match.Success Then

                Dim formattedDate As String = ""
                ' Rozpoznání formátu podle shody celé pojmenované skupiny formátu
                If match.Groups("format1").Success Then
                    ' Formát TYYYY-MM-DD-hh-mm
                    Dim year As Integer = Integer.Parse(match.Groups("year1").Value)
                    Dim month As Integer = Integer.Parse(match.Groups("month1").Value)
                    Dim day As Integer = Integer.Parse(match.Groups("day1").Value)
                    Dim hour As Integer = Integer.Parse(match.Groups("hour1").Value)
                    Dim minute As Integer = Integer.Parse(match.Groups("minute1").Value)
                    dateTimeFromFileName = New DateTime(year, month, day, hour, minute, 0)
                    formattedDate = match.Groups("format1").Value

                ElseIf match.Groups("format2").Success Then
                    ' Formát YYYY-MM-DD_hh-mm
                    Dim year As Integer = Integer.Parse(match.Groups("year2").Value)
                    Dim month As Integer = Integer.Parse(match.Groups("month2").Value)
                    Dim day As Integer = Integer.Parse(match.Groups("day2").Value)
                    Dim hour As Integer = Integer.Parse(match.Groups("hour2").Value)
                    Dim minute As Integer = Integer.Parse(match.Groups("minute2").Value)
                    dateTimeFromFileName = New DateTime(year, month, day, hour, minute, 0)
                    formattedDate = match.Groups("format2").Value
                ElseIf match.Groups("format3").Success Then
                    ' Formát D._MM._YYYY_h_mm_ss
                    Dim day As Integer = Integer.Parse(match.Groups("day3").Value.PadLeft(2, "0"c))
                    Dim month As Integer = Integer.Parse(match.Groups("month3").Value.PadLeft(2, "0"c))
                    Dim year As Integer = Integer.Parse(match.Groups("year3").Value)
                    Dim hour As Integer = Integer.Parse(match.Groups("hour3").Value)
                    Dim minute As Integer = Integer.Parse(match.Groups("minute3").Value)
                    Dim second As Integer = Integer.Parse(match.Groups("second3").Value)
                    formattedDate = match.Groups("format3").Value
                    dateTimeFromFileName = New DateTime(year, month, day, hour, minute, second)
                ElseIf match.Groups("format4").Success Then
                    ' Formát YYYY-MM-DD
                    Dim year As Integer = Integer.Parse(match.Groups("year4").Value)
                    Dim month As Integer = Integer.Parse(match.Groups("month4").Value)
                    Dim day As Integer = Integer.Parse(match.Groups("day4").Value)
                    dateTimeFromFileName = New DateTime(year, month, day)
                End If

                ' Výstup formátu data ve tvaru YYYY-MM-DD
                ' Debug.writeline("Převedené datum: " & dateTimeFromFileName.ToString("yyyy-MM-dd"))
                ' Odstranění původního datového vzoru z řetězce
                Dim modifiedFileName As String = myRegex.Replace(_gpxFileName, "")

                ' Přidání přeformátovaného data na začátek modifikovaného řetězce
                newFileName = $"{dateTimeFromFileName.ToString("yyyy-MM-dd")}{modifiedFileName}"
                '  Debug.writeline("Přeformátované file name: " & newFileName)

                If Not String.IsNullOrWhiteSpace(newFileName) AndAlso Not newFileName.TrimEnd = _gpxFileName.TrimEnd Then

                    newFilePath = Path.Combine(gpxDirectory, newFileName & ".gpx")

                    If IO.File.Exists(newFilePath) Then
                        ' Handle existing files
                        Dim userInput As String = InputBox($"File {newFileName} already exists. Enter a new name:", newFileName)
                        If Not String.IsNullOrWhiteSpace(userInput) Then
                            newFilePath = Path.Combine(gpxDirectory, userInput & fileExtension)
                            IO.File.Move(_gpxFilePath, newFilePath)
                            RaiseEvent WarningOccurred($"Renamed file: {Path.GetFileName(_gpxFilePath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                            Debug.WriteLine($"Renamed file: {Path.GetFileName(_gpxFilePath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")

                        Else
                            RaiseEvent WarningOccurred($"New name for {newFilePath} was not provided.{Environment.NewLine}")

                        End If

                    Else
                        IO.File.Move(_gpxFilePath, newFilePath)
                        _gpxFilePath = newFilePath
                        Debug.WriteLine($"Renamed file: {Path.GetFileName(_gpxFilePath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                        RaiseEvent WarningOccurred($"Renamed file: {Path.GetFileName(_gpxFilePath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                    End If
                    _gpxFilePath = newFilePath
                End If

            Else
                Debug.WriteLine("Žádné datum v požadovaném formátu nebylo nalezeno.")
                newFileName = $"{_layerStart.Date.ToString("yyyy-MM-dd")}{_gpxFileName}{fileExtension}"
                newFilePath = Path.Combine(gpxDirectory, newFileName)

                If IO.File.Exists(newFilePath) Then
                    ' Handle existing files
                    Dim userInput As String = InputBox($"File {newFileName} already exists. Enter a new name:", newFileName)
                    If Not String.IsNullOrWhiteSpace(userInput) Then
                        newFilePath = Path.Combine(gpxDirectory, userInput & fileExtension)
                        IO.File.Move(_gpxFilePath, newFilePath)
                        RaiseEvent WarningOccurred($"Renamed file: {Path.GetFileName(_gpxFilePath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                        Debug.WriteLine($"Renamed file: {Path.GetFileName(_gpxFilePath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")

                    Else
                        RaiseEvent WarningOccurred($"New name for {newFilePath} was not provided.{Environment.NewLine}")

                    End If
                Else
                    IO.File.Move(_gpxFilePath, newFilePath)
                    _gpxFilePath = newFilePath
                    Debug.WriteLine($"Renamed file: {Path.GetFileName(_gpxFilePath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                    RaiseEvent WarningOccurred($"Renamed file: {Path.GetFileName(_gpxFilePath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                End If
            End If

        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
        End Try
        _gpxRecord.Reader.filePath = newFilePath
        _gpxRecord.Reader.FileName = newFileName

        Return _gpxRecord

    End Function

    Public Function MergeLayerAndDog(_gpxRecords As List(Of GPXRecord)) As List(Of GPXRecord)
        'vytváříme list gpx souborů seřazených podle LayerStart, první vložíme,
        'další prohlídneme, zda se neliší o méně než maxAge - ty je pak možné spojit,
        'protože se nejspíš jedná o záznam stopy kladeč a trasy psa, jen je každá zvlášť

        Dim gpxFilesMerged As New List(Of GPXRecord) From {_gpxRecords(0)}
        Dim lastAddedIndex As Integer = 0

        For i = 1 To _gpxRecords.Count - 1
            If Not TryMerge(_gpxRecords(i), _gpxRecords(lastAddedIndex)) Then
                ' Přidání souboru do seznamu, pokud ke spojení nedošlo
                gpxFilesMerged.Add(_gpxRecords(i))
                lastAddedIndex = i
            End If
        Next i
        Return gpxFilesMerged
    End Function

    Private Function TryMerge(soubor_i As GPXRecord, soubor_prev As GPXRecord) As Boolean

        'najdi všechny sousední soubory, které se liší o méně než MaxAge
        ' Základní kontrola, zda rozdíl dat splňuje podmínku na max stáří

        Dim mergeDecision As String = LoadMergeDecision(soubor_prev, soubor_i)
        If (soubor_i.LayerStart - soubor_prev.LayerStart < maxAge) AndAlso
        (Not mergeCancel) AndAlso (Not mergeDecision = System.Windows.Forms.DialogResult.No.ToString) Then

            ' Zjisti, zda oba soubory obsahují pouze jeden uzel <trkseg>
            Dim trksegNodes_i As XmlNodeList = soubor_i.Reader.SelectNodes("trkseg")
            Dim trksegNodes_prev As XmlNodeList = soubor_prev.Reader.SelectNodes("trkseg")
            If trksegNodes_i.Count = 1 AndAlso trksegNodes_prev.Count = 1 Then
                ' Zeptej se uživatele, zda chce soubory spojit
                Dim mergeFiles As DialogResult
                If Not mergeNoAsk Then mergeFiles = DialogMergeFiles(soubor_prev, soubor_i)
                ' Pokud uživatel souhlasí, spoj soubory, jinak přidej
                If mergeNoAsk OrElse (mergeFiles = DialogResult.Yes) Then
                    If MergeTwoGpxFiles(soubor_prev, soubor_i) Then
                        Return True
                    End If
                End If
            End If
        End If
        Return False 'ke spojení souborů nedošlo
    End Function

    Private Function DialogMergeFiles(runner As GPXRecord, dog As GPXRecord) As DialogResult
        ' Vytvoření instance formuláře (dialogu)
        Dim dialog As New Form()
        dialog.Text = My.Resources.Resource1.lblMergeGPXtracksQ ' "Spojit GPX trasy?"
        dialog.FormBorderStyle = FormBorderStyle.FixedDialog ' Zamezení změně velikosti
        dialog.StartPosition = FormStartPosition.CenterParent ' Vycentrování na rodičovské okno
        dialog.MinimizeBox = False
        dialog.MaximizeBox = False
        dialog.BackColor = Color.FromArgb(237, 240, 213)
        dialog.Font = New Font("Cascadia Code Semibold", 10, FontStyle.Bold)
        dialog.ForeColor = Color.DarkGreen ' Nastavit barvu


        ' Popisek s dotazem
        Dim lblDotaz As New Label()
        lblDotaz.Text = My.Resources.Resource1.lblMergeTwoToOneQ '"Chcete spojit tyto dva soubory do jednoho?"
        lblDotaz.AutoSize = True
        lblDotaz.Location = New Point(10, 10)
        lblDotaz.ForeColor = Color.Maroon
        dialog.Controls.Add(lblDotaz)

        ' Popis (volitelný, můžeš ho upravit podle potřeby)
        Dim lblPopis As New Label()
        lblPopis.Text = My.Resources.Resource1.lblMergingYouGet ' "Spojením získáte jeden soubor gpx obsahující trasu kladeče i psa." ' Příklad popisu
        lblPopis.AutoSize = True
        lblPopis.Location = New Point(10, lblDotaz.Bottom + 5)
        dialog.Controls.Add(lblPopis)

        ' Popisky se jmény souborů
        Dim lblSoubor1 As New Label()
        lblSoubor1.Text = $"{My.Resources.Resource1.lblIsThisLayerQ}: '{Path.GetFileName(runner.Reader.filePath)}' ?"
        lblSoubor1.AutoSize = True
        lblSoubor1.Location = New Point(10, lblPopis.Bottom + 10)
        lblSoubor1.ForeColor = Color.Maroon
        dialog.Controls.Add(lblSoubor1)

        Dim lblSoubor2 As New Label()
        lblSoubor2.Text = $"{My.Resources.Resource1.lblIsThisTrackOfTheDog}: '{Path.GetFileName(dog.Reader.filePath)}' ?"
        lblSoubor2.AutoSize = True
        lblSoubor2.Location = New Point(10, lblSoubor1.Bottom + 5)
        lblSoubor2.ForeColor = Color.Maroon
        dialog.Controls.Add(lblSoubor2)

        ' Zaškrtávací políčko pro zapamatování rozhodnutí
        Dim chbRemembDecision As New CheckBox
        chbRemembDecision.Text = My.Resources.Resource1.chbRemembDecisQ '"Zapamatovat rozhodnutí 'Ne' pro tuto dvojici"
        chbRemembDecision.Location = New Point(10, lblSoubor2.Bottom + 10)
        chbRemembDecision.AutoSize = True
        chbRemembDecision.Checked = True
        dialog.Controls.Add(chbRemembDecision)

        ' Tlačítka
        Dim btnAno As New Button()
        btnAno.Text = "Yes"
        btnAno.DialogResult = DialogResult.Yes ' Nastavení výsledku dialogu
        btnAno.Location = New Point(10, chbRemembDecision.Bottom + 15)
        btnAno.AutoSize = True
        dialog.Controls.Add(btnAno)

        Dim btnNe As New Button()
        btnNe.Text = "No"
        btnNe.DialogResult = DialogResult.No
        btnNe.Location = New Point(btnAno.Right + 10, chbRemembDecision.Bottom + 15)
        btnNe.AutoSize = True
        dialog.Controls.Add(btnNe)


        ' Zaškrtávací políčko pro automatické spojování
        Dim rbNoAsk As New RadioButton()
        rbNoAsk.Text = My.Resources.Resource1.rbDontAskMergeQ '"U dalších dvojic se už neptat a rovnou spojit (opatrně!)"
        rbNoAsk.Location = New Point(10, btnNe.Bottom + 15)
        rbNoAsk.AutoSize = True
        dialog.Controls.Add(rbNoAsk)

        ' Zaškrtávací políčko pro zrušení spojování
        Dim rbCancel As New RadioButton()
        rbCancel.Text = My.Resources.Resource1.rbDontAskDontMerge ' "U dalších dvojic se už neptat a nic nespojovat"
        rbCancel.Location = New Point(10, rbNoAsk.Bottom + 7)
        rbCancel.AutoSize = True
        dialog.Controls.Add(rbCancel)

        ' Zaškrtávací políčko pro zrušení spojování
        Dim rbAsk As New RadioButton()
        rbAsk.Text = My.Resources.Resource1.rbAskAgein '"Příště se zase ptát"
        rbAsk.Location = New Point(10, rbCancel.Bottom + 7)
        rbAsk.AutoSize = True
        rbAsk.Checked = True
        dialog.Controls.Add(rbAsk)


        ' Nastavení velikosti dialogu (automaticky podle obsahu)
        dialog.AutoSize = True
        dialog.AutoSizeMode = AutoSizeMode.GrowAndShrink

        ' Zobrazení dialogu modálně a uložení výsledku
        Dim result As DialogResult = dialog.ShowDialog()

        ' Uložení stavu zaškrtávacího políčka do veřejné proměnné nebo do nastavení aplikace
        If result = DialogResult.No AndAlso chbRemembDecision.Checked Then
            SaveMergeDecision(runner, dog, result.ToString)
        End If

        mergeNoAsk = rbNoAsk.Checked ' Uložení stavu pro použití v hlavní funkci
        mergeCancel = rbCancel.Checked
        Return result
    End Function



    Private Sub SaveMergeDecision(runner As GPXRecord, dog As GPXRecord, result As String)
        Dim key As String = $"{runner.Reader.filePath}|{dog.Reader.filePath}" ' Vytvoření klíče
        Dim settings As System.Collections.Specialized.StringCollection = My.Settings.MergeDecisions
        ' Inicializace kolekce, pokud je Nothing
        If settings Is Nothing Then
            settings = New System.Collections.Specialized.StringCollection()
            My.Settings.MergeDecisions = settings ' Důležité: Uložení inicializované kolekce zpět do nastavení!
        End If
        ' Odstranění starého rozhodnutí, pokud existuje

        Dim existingIndex As Integer = -1
        For i As Integer = 0 To settings.Count - 1
            If settings(i).StartsWith($"{runner.Reader.filePath}|{dog.Reader.filePath}|") Then
                existingIndex = i
                Exit For
            End If
        Next
        If existingIndex > -1 Then settings.RemoveAt(existingIndex)
        settings.Add($"{key}|{result}") ' Uložení nového rozhodnutí
        My.Settings.Save() ' Uložení změn do konfiguračního souboru

    End Sub

    ' Načtení rozhodnutí
    Private Function LoadMergeDecision(runner As GPXRecord, dog As GPXRecord) As String
        Dim key As String = $"{runner.Reader.filePath}|{dog.Reader.filePath}"
        Dim settings As System.Collections.Specialized.StringCollection = My.Settings.MergeDecisions
        If Not settings Is Nothing Then
            For Each item As String In settings
                If item.StartsWith($"{key}|") Then
                    Return item.Split("|")(2) ' Vrácení rozhodnutí ("Yes", "No", "NoAsk", "Cancel")
                End If
            Next
        End If
        Return "" ' Vrácení prázdného řetězce, pokud rozhodnutí neexistuje
    End Function

    Private Function MergeTwoGpxFiles(layer As GPXRecord, dog As GPXRecord) As Boolean
        'do souboru layer vloží kompletní uzel  <trk> vyjmutý ze souboru dog
        Try
            ' Najdi první uzel <trk>
            Dim layertrkNode As XmlNode = layer.Reader.SelectSingleNode("trk")
            Dim dogtrkNode As XmlNode = dog.Reader.SelectSingleNode("trk")
            If layertrkNode IsNot Nothing AndAlso dogtrkNode IsNot Nothing Then
                Dim importedNode As XmlNode = layer.Reader.ImportNode(dogtrkNode, True) ' Důležité: Import uzlu!
                Dim layerGpxNode As XmlNode = layer.Reader.SelectSingleNode("gpx")
                layerGpxNode.AppendChild(importedNode) ' Přidání na konec <gpx>

                'spojené trasy se uloží do souboru kladeče
                layer.Reader.Save()
                IO.File.Delete(dog.Reader.filePath)
                RaiseEvent WarningOccurred($"Tracks in files {Path.GetFileName(layer.Reader.filePath)} and {Path.GetFileName(dog.Reader.filePath)} were successfully merged in file {Path.GetFileName(layer.Reader.filePath)} {vbCrLf}File {Path.GetFileName(dog.Reader.filePath)}  was deleted.{vbCrLf}")

            End If
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    Sub RenamewptNode(_gpxRecord As GPXRecord, newname As String)
        ' traverses all <wpt> nodes in the GPX file and overwrites the value of <name> nodes to "-předmět":
        ' Find all <wpt> nodes using the namespace

        _gpxRecord.Reader.Nodes = _gpxRecord.Reader.SelectNodes("wpt")
        ' Go through each <wpt> node
        For Each wptNode As XmlNode In _gpxRecord.Reader.Nodes
            ' Najdi uzel <name> uvnitř <wpt> s použitím namespace
            Dim nameNode As XmlNode = _gpxRecord.Reader.SelectSingleChildNode("name", wptNode)
            If nameNode IsNot Nothing AndAlso nameNode.InnerText <> newname Then
                ' Přepiš hodnotu <name> na newname
                nameNode.InnerText = newname
            End If
        Next wptNode
    End Sub










    'nepoužito
    Public Function FilterByDate(GpxRecords As List(Of GPXRecord), startDate As DateTime, endDate As DateTime) As List(Of GPXRecord)
        Return GpxRecords.Where(Function(r) r.LayerStart >= startDate AndAlso r.LayerStart <= endDate).ToList()
    End Function



End Class

Public Class GPXRecord

    Public Event WarningOccurred(message As String)

    Public Sub New()
        gpxDirectory = My.Settings.Directory
        BackupDirectory = My.Settings.BackupDirectory
        If Not Directory.Exists(BackupDirectory) Then
            Directory.CreateDirectory(BackupDirectory)
        End If
    End Sub

    'Public Property FilePath As String
    'Public Property FileName As String
    Public Property LayerStart As DateTime
    Public Property DogStart As DateTime
    Public Property DogFinish As DateTime
    Public Property TrailAge As TimeSpan
    Public Property Distance As Double
    Public Property TotalDistance As Double
    Public Property Description As String
    Public Property Link As String
    Public Property DogSpeed As Double
    Public Property Reader As GpxReader

    Private ReadOnly Property gpxDirectory As String
    Private ReadOnly Property BackupDirectory As String

    Friend Sub SetCreatedModifiedDate()
        'change of attributes
        ' Setting the file creation date
        IO.File.SetCreationTime(Me.Reader.filePath, Me.LayerStart)
        ' Setting the last modified file date
        IO.File.SetLastWriteTime(Me.Reader.filePath, Me.LayerStart)
    End Sub

    ' Function to read the <link> description from the first <trk> node in the GPX file
    Friend Function Getlink()
        ' Načtení více uzlů, např. <trkseg>
        Dim linkNodes As XmlNodeList = Me.Reader.SelectNodes("link")

        For Each linkNode As XmlNode In linkNodes
            ' Zpracování každého uzlu <link>
            If linkNode IsNot Nothing AndAlso linkNode.Attributes("href") IsNot Nothing Then
                Dim linkHref As String = linkNode.Attributes("href").Value
                If linkHref.Contains("youtu") Then
                    Return linkHref
                End If
            End If
        Next
        Return Nothing
    End Function

    Public Function CalculateAge() As TimeSpan
        Dim ageFromTime As TimeSpan
        Dim ageFromComments As TimeSpan

        If Me.DogStart <> Date.MinValue AndAlso Me.LayerStart <> Date.MinValue Then
            Try
                ageFromTime = Me.DogStart - Me.LayerStart
            Catch ex As Exception
            End Try
        End If

        If Not String.IsNullOrWhiteSpace(Me.Description) Then ageFromComments = FindTheAgeinComments(Me.Description)

        'Add age to comments
        If ageFromComments = TimeSpan.Zero And Not ageFromTime = TimeSpan.Zero Then

            Dim newDescription As String
            If Me.Description Is Nothing Then
                newDescription = "Trail: " & ageFromTime.TotalHours.ToString("F1") & " hod"
            Else ' Najde řetězec "Trail:" a nahradí ho řetězcem "Trail:" & AgeFromTime
                Dim searchText As String = "trail" ' Hledaný text (malými písmeny)
                Dim index As Integer = Me.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase)

                If index >= 0 Then ' Pokud byl text nalezen
                    ' Získání textu před a za nalezeným slovem
                    Dim prefix As String = Me.Description.Substring(0, index)
                    Dim suffix As String = Me.Description.Substring(index + searchText.Length)

                    ' Odstranění případných mezer za slovem "trail"
                    suffix = suffix.TrimStart()
                    ' Sestavení nového popisu
                    newDescription = $"{prefix}Trail: {ageFromTime.TotalHours.ToString("F1")} h {suffix}"
                Else
                    ' když tam Trail není vytvoří ho a doplní do desc
                    newDescription = "Trail: " & ageFromTime.TotalHours.ToString("F1") & " h " & Me.Description
                End If

            End If

            If Not String.IsNullOrWhiteSpace(newDescription) Then
                WriteDescription(newDescription)
            End If
        End If

        If Not ageFromTime = TimeSpan.Zero Then
            Return ageFromTime
        ElseIf Not ageFromComments = TimeSpan.Zero Then
            Return ageFromComments
        Else Return TimeSpan.Zero
        End If
        Return TimeSpan.Zero
    End Function

    ' Function to set the <desc> description from the first <trk> node in the GPX file
    Public Sub WriteDescription(newDescription As String)
        ' Find the first <trk> node and its <desc> subnode
        Dim trkNode As XmlNode = Me.Reader.SelectSingleNode("trk")
        Dim descNode As XmlNode = Me.Reader.SelectSingleChildNode("desc", trkNode)
        ' Pokud uzel <desc> neexistuje, vytvoříme jej a přidáme do <trk>
        If descNode Is Nothing Then
            ' Najdeme první uzel <trk>
            'Dim trkNode As XmlNode = xmlDoc.SelectSingleNode("/gpx:gpx/gpx:trk[1]", namespaceManager)
            descNode = Me.Reader.CreateElement("desc")
            If trkNode IsNot Nothing Then
                ' Vytvoříme nový uzel <desc>
                ' Přidání <desc> jako prvního potomka v uzlu <trk>
                If trkNode.HasChildNodes Then
                    ' Vloží <desc> před první existující poduzel
                    trkNode.InsertBefore(descNode, trkNode.FirstChild)
                Else
                    ' Pokud <trk> nemá žádné poduzly, použijeme AppendChild
                    trkNode.AppendChild(descNode)
                End If
                ' Nastavíme hodnotu pro <desc> (můžete ji změnit podle potřeby)
                descNode.InnerText = newDescription
                ' Přidáme nový uzel <desc> do uzlu <trk>
                'trkNode.AppendChild(descNode)
            End If
        Else
            descNode.InnerText = newDescription
        End If
        Me.Description = newDescription
    End Sub

    Public Function CalculateSpeed() As Double 'km/h
        If Not Me.DogStart = DateTime.MinValue AndAlso Not Me.DogFinish = DateTime.MinValue Then
            If (Me.DogFinish - Me.DogStart).TotalHours > 0 Then
                Return Me.Distance / (Me.DogFinish - Me.DogStart).TotalHours
            End If
        End If
        Return Nothing
    End Function


    Public Function FindTheAgeinComments(inputText As String) As TimeSpan
        ' Upravený regulární výraz pro nalezení čísla, které může být i desetinné
        Dim regex As New Regex("\b\d+[.,]?\d*\s*(h(odin(y|a))?|hod|min(ut)?)\b", RegexOptions.IgnoreCase)
        Dim match As Match = regex.Match(inputText)

        If match.Success Then
            Dim nalezenyCas As String = match.Value
            ' Převede desetinnou tečku nebo čárku na standardní tečku pro parsování
            Dim casString As String = Regex.Match(nalezenyCas, "\d+[.,]?\d*").Value.Replace(",", ".")
            Dim casCislo As Double = Double.Parse(casString, CultureInfo.InvariantCulture)



            Dim casTimeSpan As TimeSpan
            If nalezenyCas.Contains("h") Then
                casTimeSpan = TimeSpan.FromHours(casCislo)
                Return casTimeSpan
            ElseIf nalezenyCas.Contains("min") Then
                casTimeSpan = TimeSpan.FromMinutes(casCislo)
                Return casTimeSpan
            End If
        End If

        ' Pokud nebyl čas nalezen, vrátí Nothing
        Return TimeSpan.Zero
    End Function


    Public Function GetDogStart() As Date
        Dim trksegNodes As XmlNodeList = Me.Reader.SelectNodes("trkseg")
        Dim dogStart As DateTime
        If trksegNodes.Count > 1 Then
            Dim dogtimeNodes As XmlNodeList = Me.Reader.SelectAllChildNodes("time", trksegNodes(1)) '.SelectNodes("gpx:trkpt/gpx:time", namespaceManager)
            Dim DogstartTimeNode As XmlNode = dogtimeNodes(0)
            If Not DogstartTimeNode Is Nothing Then DateTime.TryParse(DogstartTimeNode.InnerText, dogStart)
        End If
        Return dogStart
    End Function

    Public Function GetDogFinish() As Date
        Dim trksegNodes As XmlNodeList = Me.Reader.SelectNodes("trkseg")
        Dim dogFinish As DateTime

        If trksegNodes.Count > 1 Then
            Dim dogtimeNodes As XmlNodeList = Me.Reader.SelectAllChildNodes("time", trksegNodes(1)) '.SelectNodes("gpx:trkpt/gpx:time", namespaceManager)
            Dim DogFinishTimeNode As XmlNode = dogtimeNodes(dogtimeNodes.Count - 1)
            If Not DogFinishTimeNode Is Nothing Then DateTime.TryParse(DogFinishTimeNode.InnerText, dogFinish)
        End If
        Return dogFinish
    End Function

    ' Function to read and calculate the length of only the first segment from the GPX file
    Public Function CalculateFirstSegmentDistance() As Double
        Dim totalLengthOfFirst_trkseg As Double = 0.0
        Dim lat1, lon1, lat2, lon2 As Double
        Dim firstPoint As Boolean = True

        ' Select the first track segment (<trkseg>) using the namespace
        Dim trknode As XmlNode = Me.Reader.SelectSingleNode("trk")
        Dim firstSegment As XmlNode = Me.Reader.SelectSingleChildNode("trkseg", trknode)

        ' If the segment exists, calculate its length
        If firstSegment IsNot Nothing Then
            ' Select all track points in the first segment (<trkpt>)
            Dim trackPoints As XmlNodeList = Me.Reader.SelectChildNodes("trkpt", firstSegment)

            ' Calculate the distance between each point in the first segment
            For Each point As XmlNode In trackPoints
                Try
                    ' Check if attributes exist and load them as Double
                    If point.Attributes("lat") IsNot Nothing AndAlso point.Attributes("lon") IsNot Nothing Then
                        Dim lat As Double = Convert.ToDouble(point.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
                        Dim lon As Double = Convert.ToDouble(point.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)

                        If firstPoint Then
                            ' Initialize the first point
                            lat1 = lat
                            lon1 = lon
                            firstPoint = False
                        Else
                            ' Calculate the distance between the previous and current point
                            lat2 = lat
                            lon2 = lon
                            totalLengthOfFirst_trkseg += HaversineDistance(lat1, lon1, lat2, lon2, "km")

                            ' Move the current point into lat1, lon1 for the next iteration
                            lat1 = lat2
                            lon1 = lon2
                        End If
                    End If
                Catch ex As Exception
                    ' Adding a more detailed exception message
                    Debug.WriteLine("Error: " & ex.Message)
                    ' TODO: Replace direct access to Form1 with a better method for separating logic
                    RaiseEvent WarningOccurred("Error processing point: " & ex.Message & Environment.NewLine)
                End Try
            Next
        Else
            ' TODO: Replace direct access to Form1 with a better method for separating logic
            RaiseEvent WarningOccurred("No tracks found in GPX file: " & Me.Reader.FileName & Environment.NewLine)
        End If

        Return totalLengthOfFirst_trkseg ' Result in kilometers
    End Function

    Public Function GetDescription() As String

        ' Find the first <trk> node and its <desc> subnode
        ' Vyhledání uzlu <trk> v rámci hlavního namespace
        Dim trkNode As XmlNode = Me.Reader.SelectSingleNode("trk")
        If trkNode IsNot Nothing Then

            Dim descNode As XmlNode = Me.Reader.SelectSingleChildNode("desc", trkNode)


            If descNode IsNot Nothing Then
                Return descNode.InnerText
            Else
                Return Nothing '"The <desc> node was not found."
            End If
        Else
            Return Nothing
        End If

    End Function

    ' in gpx files, splits a track with two segments into two separate tracks
    Public Sub SplitTrackIntoTwo()
        ' Najdi první uzel <trk>
        Dim trkNode As XmlNode = Me.Reader.SelectSingleNode("trk")

        If trkNode IsNot Nothing Then
            ' Najdi všechny <trkseg> uvnitř <trk>
            Dim trkSegNodes As XmlNodeList = Me.Reader.SelectChildNodes("trkseg", trkNode)

            If trkSegNodes.Count > 1 Then
                ' Vytvoř nový uzel <trk>
                Dim newTrkNode As XmlNode = Me.Reader.CreateElement("trk")

                ' Přesuň druhý <trkseg> do nového <trk>
                Dim secondTrkSeg As XmlNode = trkSegNodes(1)
                trkNode.RemoveChild(secondTrkSeg)
                newTrkNode.AppendChild(secondTrkSeg)

                ' Přidej nový <trk> do dokumentu hned po prvním
                trkNode.ParentNode.InsertAfter(newTrkNode, trkNode)
                Me.Reader.Save()
                RaiseEvent WarningOccurred($"Track in file { Me.Reader.FileName} was successfully split.")
            End If
        End If
    End Sub

    ' Function to convert degrees to radians
    Private Function DegToRad(degrees As Double) As Double
        Const PI As Double = 3.14159265358979
        Return degrees * PI / 180
    End Function

    ' Function to calculate the distance in km between two GPS points using the Haversine formula
    Private Function HaversineDistance(lat1 As Double, lon1 As Double, lat2 As Double, lon2 As Double, units As String) As Double
        Dim dLat As Double = DegToRad(lat2 - lat1)
        Dim dLon As Double = DegToRad(lon2 - lon1)
        ' Constants for converting degrees to radians and Earth's radius
        Const EARTH_RADIUS As Double = 6371 ' Earth's radius in kilometers

        Dim a As Double = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(DegToRad(lat1)) * Math.Cos(DegToRad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
        Dim c As Double = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a))

        If units = "km" Then
            Return EARTH_RADIUS * c ' Result in kilometers
        ElseIf units = "m" Then
            Return EARTH_RADIUS * c * 1000 'result in metres
        Else
            Return EARTH_RADIUS * c ' Result in kilometers
        End If
    End Function

    Public Sub TrimGPSnoise(minDistance As Integer)
        'clip the start and end of both <trk>, i.e., the layer and the dog, which was recorded after (or before) the end of the trail. Useful when the GPS doesn't turn off right away.
        ' Získání všech uzlů <trk>
        Dim trackNodes = Me.Reader.SelectNodes("trk")

        For Each trkNode As XmlNode In trackNodes
            ' Získání všech <trkseg> uvnitř <trk>
            Dim trackSegments = Me.Reader.SelectChildNodes("trkseg", trkNode)

            For Each trksegNode As XmlNode In trackSegments
                ' Získání všech <trkpt> uvnitř <trkseg>
                Dim trackPoints = Me.Reader.SelectChildNodes("trkpt", trksegNode)

                ' Převod XmlNodeList na seznam pro snadnou manipulaci
                Dim points = trackPoints.Cast(Of XmlNode).ToList()

                Dim startCluster = Cluster(points, Me.Reader, minDistance)

                ' Odeber body z clusteru
                If startCluster.Count > 5 Then
                    For i = 0 To startCluster.Count - 2 'poslední ponechá, neb je nahrazen centroidem
                        Dim point = startCluster.Item(i)
                        trksegNode.RemoveChild(point)
                        points.Remove(point)
                    Next
                    Me.Reader.Save()
                End If

                Dim reversedPoints = points.AsEnumerable().Reverse().ToList()

                Dim endCluster = Cluster(reversedPoints, Me.Reader, minDistance)

                ' Odeber body z endCluster
                If endCluster.Count > 5 Then
                    For i = 0 To endCluster.Count - 2 ' poslední ponecháme
                        Dim point = endCluster.Item(i)
                        trksegNode.RemoveChild(point)
                    Next
                    Me.Reader.Save()
                End If
            Next trksegNode
        Next trkNode
    End Sub

    Private Function Cluster(points As List(Of XmlNode), gpxReader As GpxReader, minDistance As Double) As List(Of XmlNode)
        Dim cluster_ As New List(Of XmlNode)
        Dim centroidLat, centroidLon As Double

        Dim isCluster As Boolean = True

        For i As Integer = 0 To points.Count - 1
            Dim lat = Double.Parse(points(i).Attributes("lat").Value, CultureInfo.InvariantCulture)
            Dim lon = Double.Parse(points(i).Attributes("lon").Value, CultureInfo.InvariantCulture)


            If cluster_.Count = 0 Then
                ' Inicializace clusteru
                cluster_.Add(points(i))
                centroidLat = lat
                centroidLon = lon

                Continue For
            End If
            ' Výpočet vzdálenosti od centroidu
            Dim currentDistance = HaversineDistance(centroidLat, centroidLon, lat, lon, "m")
            Debug.WriteLine($"   {i}  {centroidLat} {centroidLon} {lat} {lon} {currentDistance}")

            ' Rozhodnutí o ukončení clusteru 

            If currentDistance > minDistance Then ' Pokud je vzdálenost větší ukonči cluster
                isCluster = False
            End If

            If Not isCluster Then
                'poslední bod v clusteru je nahrazen centroidem
                If cluster_.Count > 5 Then
                    'Poslední point v klastru se nahradí centroidem,
                    'přitom uzel time zůstává beze změny - tím se zpřesní
                    'výpočet stáří a rychlosti psa
                    cluster_.Last.Attributes("lat").Value = centroidLat.ToString("G", NumberFormatInfo.InvariantInfo)
                    cluster_.Last.Attributes("lon").Value = centroidLon.ToString("G", NumberFormatInfo.InvariantInfo)
                End If

                Exit For
            End If

            cluster_.Add(points(i))

            ' Aktualizace centroidu
            centroidLat = Math.Round((centroidLat * cluster_.Count + lat) / (cluster_.Count + 1), 8)
            centroidLon = Math.Round((centroidLon * cluster_.Count + lon) / (cluster_.Count + 1), 8)
        Next
        Return cluster_
    End Function


    Public Function Backup() As Boolean
        ' Vytvoření kompletní cílové cesty
        Dim backupFilePath As String = Path.Combine(BackupDirectory, Me.Reader.FileName)

        If Not IO.File.Exists(backupFilePath) Then
            ' Kopírování souboru
            Try
                IO.File.Copy(Me.Reader.filePath, backupFilePath, False)
                Return True
            Catch ex As Exception
                ' Zpracování jakýchkoli neočekávaných chyb
                Debug.WriteLine($"Chyba při kopírování souboru {Reader.FileName}: {ex.Message}")
                Return False
            End Try
        Else
            ' Soubor již existuje, přeskočíme
            Debug.WriteLine($"Soubor {Reader.FileName} již existuje, přeskočeno.")
            Return True
        End If
    End Function

    Public Sub RenamewptNode(newname As String)
        ' traverses all <wpt> nodes in the GPX file and overwrites the value of <name> nodes to "-předmět":
        ' Find all <wpt> nodes using the namespace

        Me.Reader.Nodes = Me.Reader.SelectNodes("wpt")
        ' Go through each <wpt> node
        For Each wptNode As XmlNode In Me.Reader.Nodes
            ' Najdi uzel <name> uvnitř <wpt> s použitím namespace
            Dim nameNode As XmlNode = Me.Reader.SelectSingleChildNode("name", wptNode)
            If nameNode IsNot Nothing AndAlso nameNode.InnerText <> newname Then
                ' Přepiš hodnotu <name> na newname
                nameNode.InnerText = newname
            End If
        Next wptNode
    End Sub

    Public Function PrependDateToFilename()

        Dim fileExtension As String = Path.GetExtension(Reader.filePath)
        Dim fileNameOhneExt As String = IO.Path.GetFileNameWithoutExtension(Reader.filePath)
        Dim newFileName As String = Reader.FileName
        Dim newFilePath As String = Reader.filePath 'pokud se nezmění zůstane původní hodnota


        Dim dateTimeFromFileName As DateTime
        Try
            'Pokusí se najít datum v názvu souboru:
            ' Regex s pojmenovanými skupinami pro celé formáty i jednotlivé části data
            Dim pattern As String = "(?<format1>T(?<year1>\d{4})-(?<month1>\d{2})-(?<day1>\d{2})-(?<hour1>\d{2})-(?<minute1>\d{2}))|" &
                            "(?<format2>(?<year2>\d{4})-(?<month2>\d{2})-(?<day2>\d{2})_(?<hour2>\d{2})-(?<minute2>\d{2}))|" &
                            "(?<format3>(?<day3>\d{1,2})\._(?<month3>\d{2})\._(?<year3>\d{4})_(?<hour3>\d{1,2})_(?<minute3>\d{2})_(?<second3>\d{2}))|" &
                            "(?<format4>(?<year4>\d{4})-(?<month4>\d{2})-(?<day4>\d{2}))"
            Dim myRegex As New Regex(pattern)

            Dim match As Match = myRegex.Match(fileNameOhneExt)
            If match.Success Then

                Dim formattedDate As String = ""
                ' Rozpoznání formátu podle shody celé pojmenované skupiny formátu
                If match.Groups("format1").Success Then
                    ' Formát TYYYY-MM-DD-hh-mm
                    Dim year As Integer = Integer.Parse(match.Groups("year1").Value)
                    Dim month As Integer = Integer.Parse(match.Groups("month1").Value)
                    Dim day As Integer = Integer.Parse(match.Groups("day1").Value)
                    Dim hour As Integer = Integer.Parse(match.Groups("hour1").Value)
                    Dim minute As Integer = Integer.Parse(match.Groups("minute1").Value)
                    dateTimeFromFileName = New DateTime(year, month, day, hour, minute, 0)
                    formattedDate = match.Groups("format1").Value

                ElseIf match.Groups("format2").Success Then
                    ' Formát YYYY-MM-DD_hh-mm
                    Dim year As Integer = Integer.Parse(match.Groups("year2").Value)
                    Dim month As Integer = Integer.Parse(match.Groups("month2").Value)
                    Dim day As Integer = Integer.Parse(match.Groups("day2").Value)
                    Dim hour As Integer = Integer.Parse(match.Groups("hour2").Value)
                    Dim minute As Integer = Integer.Parse(match.Groups("minute2").Value)
                    dateTimeFromFileName = New DateTime(year, month, day, hour, minute, 0)
                    formattedDate = match.Groups("format2").Value
                ElseIf match.Groups("format3").Success Then
                    ' Formát D._MM._YYYY_h_mm_ss
                    Dim day As Integer = Integer.Parse(match.Groups("day3").Value.PadLeft(2, "0"c))
                    Dim month As Integer = Integer.Parse(match.Groups("month3").Value.PadLeft(2, "0"c))
                    Dim year As Integer = Integer.Parse(match.Groups("year3").Value)
                    Dim hour As Integer = Integer.Parse(match.Groups("hour3").Value)
                    Dim minute As Integer = Integer.Parse(match.Groups("minute3").Value)
                    Dim second As Integer = Integer.Parse(match.Groups("second3").Value)
                    formattedDate = match.Groups("format3").Value
                    dateTimeFromFileName = New DateTime(year, month, day, hour, minute, second)
                ElseIf match.Groups("format4").Success Then
                    ' Formát YYYY-MM-DD
                    Dim year As Integer = Integer.Parse(match.Groups("year4").Value)
                    Dim month As Integer = Integer.Parse(match.Groups("month4").Value)
                    Dim day As Integer = Integer.Parse(match.Groups("day4").Value)
                    dateTimeFromFileName = New DateTime(year, month, day)
                End If

                ' Výstup formátu data ve tvaru YYYY-MM-DD
                ' Debug.writeline("Převedené datum: " & dateTimeFromFileName.ToString("yyyy-MM-dd"))
                ' Odstranění původního datového vzoru z řetězce
                Dim modifiedFileName As String = myRegex.Replace(fileNameOhneExt, "")

                ' Přidání přeformátovaného data na začátek modifikovaného řetězce
                newFileName = $"{dateTimeFromFileName.ToString("yyyy-MM-dd")}{modifiedFileName}"
                '  Debug.writeline("Přeformátované file name: " & newFileName)

                If Not String.IsNullOrWhiteSpace(newFileName) AndAlso Not newFileName.TrimEnd = fileNameOhneExt.TrimEnd Then

                    newFilePath = Path.Combine(gpxDirectory, newFileName & ".gpx")

                    If IO.File.Exists(newFilePath) Then
                        ' Handle existing files
                        Dim userInput As String = InputBox($"File {newFileName} already exists. Enter a new name:", newFileName)
                        If Not String.IsNullOrWhiteSpace(userInput) Then
                            newFilePath = Path.Combine(gpxDirectory, userInput & fileExtension)
                            IO.File.Move(Reader.filePath, newFilePath)
                            RaiseEvent WarningOccurred($"Renamed file: {Path.GetFileName(Reader.filePath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                            Debug.WriteLine($"Renamed file: {Path.GetFileName(Reader.filePath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                        Else
                            RaiseEvent WarningOccurred($"New name for {newFilePath} was not provided.{Environment.NewLine}")

                        End If


                    Else
                        IO.File.Move(Reader.filePath, newFilePath)
                        Reader.filePath = newFilePath
                        Debug.WriteLine($"Renamed file: {Path.GetFileName(Reader.filePath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                        RaiseEvent WarningOccurred($"Renamed file: {Path.GetFileName(Reader.filePath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                    End If
                    Reader.filePath = newFilePath
                End If

            Else
                Debug.WriteLine("Žádné datum v požadovaném formátu nebylo nalezeno.")
                newFileName = $"{LayerStart.Date.ToString("yyyy-MM-dd")}{Reader.FileName}"
                newFilePath = Path.Combine(gpxDirectory, newFileName)

                If IO.File.Exists(newFilePath) Then
                    ' Handle existing files
                    Dim userInput As String = InputBox($"File {newFileName} already exists. Enter a new name:", newFileName)
                    If Not String.IsNullOrWhiteSpace(userInput) Then
                        newFilePath = Path.Combine(gpxDirectory, userInput & fileExtension)
                        IO.File.Move(Reader.filePath, newFilePath)
                        RaiseEvent WarningOccurred($"Renamed file: {Path.GetFileName(Reader.filePath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                        Debug.WriteLine($"Renamed file: {Path.GetFileName(Reader.filePath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")

                    Else
                        RaiseEvent WarningOccurred($"New name for {newFilePath} was not provided.{Environment.NewLine}")

                    End If
                Else
                    IO.File.Move(Reader.filePath, newFilePath)
                    Reader.filePath = newFilePath
                    Debug.WriteLine($"Renamed file: {Path.GetFileName(Reader.filePath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                    RaiseEvent WarningOccurred($"Renamed file: {Path.GetFileName(Reader.filePath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                End If
            End If

        Catch ex As Exception
            Debug.WriteLine(ex.ToString)
        End Try
        Me.Reader.filePath = newFilePath
        Me.Reader.FileName = newFileName

        Return newFilePath

    End Function

End Class

Public Class GpxReader
    Private xmlDoc As XmlDocument
    Private namespaceManager As XmlNamespaceManager
    Public Property filePath As String
    Private namespacePrefix As String
    Public Property FileName As String

    Public Property Nodes As XmlNodeList



    ' Konstruktor načte XML dokument a nastaví XmlNamespaceManager
    Public Sub New(_filePath As String)
        xmlDoc = New XmlDocument()
        xmlDoc.Load(_filePath)
        filePath = _filePath

        ' Zjištění namespace, pokud je definován
        Dim rootNode As XmlNode = xmlDoc.DocumentElement
        Dim namespaceUri As String = rootNode.NamespaceURI

        ' Inicializace XmlNamespaceManager s dynamicky zjištěným namespace
        namespaceManager = New XmlNamespaceManager(xmlDoc.NameTable)
        If Not String.IsNullOrEmpty(namespaceUri) Then
            namespaceManager.AddNamespace("gpx", namespaceUri) ' Použijeme lokální prefix "gpx"
            namespacePrefix = "gpx:"
        Else
            namespaceManager.AddNamespace("", namespaceUri) ' Použijeme lokální prefix "gpx"
            namespacePrefix = ""
        End If
    End Sub

    'Metoda pro získání jednoho uzlu na základě XPath
    Public Function SelectSingleChildNode(childname As String, Node As XmlNode) As XmlNode
        If Node IsNot Nothing Then
            Return Node.SelectSingleNode(namespacePrefix & childname, namespaceManager)
        Else Return Nothing
        End If


    End Function



    ' Metoda pro získání seznamu uzlů na základě XPath
    Public Function SelectNodes(nodeName As String) As XmlNodeList

        Nodes = xmlDoc.SelectNodes("//" & namespacePrefix & nodeName, namespaceManager)

        Return Nodes
    End Function

    ' Metoda pro výběr jednoho uzlu na základě názvu
    Public Function SelectSingleNode(nodename As String) As XmlNode
        Return xmlDoc.SelectSingleNode("//" & namespacePrefix & nodename, namespaceManager)

    End Function

    ' Metoda pro výběr poduzlů z uzlu Node
    Public Function SelectChildNodes(childName As String, node As XmlNode) As XmlNodeList
        Return node.SelectNodes(namespacePrefix & childName, namespaceManager)
    End Function

    ' Metoda pro rekurentní výběr všech poduzlů z uzlu Node
    Public Function SelectAllChildNodes(childName As String, node As XmlNode) As XmlNodeList
        Return node.SelectNodes(".//" & namespacePrefix & childName, namespaceManager)
    End Function

    Public Function CreateElement(nodename As String) As XmlNode
        Return xmlDoc.CreateElement(nodename, "http://www.topografix.com/GPX/1/1")
    End Function

    Public Function CreateElement(parentNode As XmlElement, childNodeName As String, value As String) As XmlElement
        Dim childNode As XmlElement = CreateElement(childNodeName)
        childNode.InnerText = value
        ' Přidání uzlu <childNodeName> do prvního <parentNode>
        parentNode.AppendChild(childNode)

        Return parentNode

    End Function

    Public Function ImportNode(node As XmlNode, deepClone As Boolean)
        Return xmlDoc.ImportNode(node, deepClone)
    End Function

    Public Sub Save()
        xmlDoc.Save(_filePath)
    End Sub
End Class

