Imports System.DirectoryServices.ActiveDirectory
Imports System.Globalization
Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar
Imports System.Xml
Imports GPXTrailAnalyzer.My.Resources
Imports TrackVideoExporter
Imports TrackVideoExporter.TrackConverter


Public Class GpxFileManager
    'obsahuje seznam souborů typu gpxRecord a funkce na jejich vytvoření a zpracování
    Dim _dogInfo As DogInfo 'informace o psovi, pro kterého se zpracovávají soubory
    Public Property DogInfo As DogInfo 'informace o psovi, pro kterého se zpracovávají soubory
        Get
            Return _dogInfo
        End Get
        Set(value As DogInfo)
            _dogInfo = value
        End Set
    End Property

    Public Property NumberOfDogs As Integer 'počet psů

    Public dateFrom As Date
    Public dateTo As Date
    Public Property ForceProcess As Boolean = False ' 'pokud je True, zpracuje všechny soubory, i ty, které už byly zpracovány (přepíše popis a další věci)
    Private ReadOnly maxAge As TimeSpan
    Private ReadOnly prependDateToName As Boolean
    Private ReadOnly trimGPS_Noise As Boolean
    Private ReadOnly mergeDecisions As System.Collections.Specialized.StringCollection

    Private mergeNoAsk As Boolean = False 'if yes merge runner and dog trails without asking
    Private mergeCancel As Boolean = False 'don't merge 

    Public Property GpxRecords As New List(Of GPXRecord)
    Public Property TotalDistance As Double = 0 'celková vzdálenost všech zpracovaných trailů
    Public Event WarningOccurred(message As String, _color As Color)

    Public Sub New()
        'gpxRemoteDirectory = My.Settings.Directory
        maxAge = New TimeSpan(My.Settings.maxAge, 0, 0)
        prependDateToName = My.Settings.PrependDateToName
        trimGPS_Noise = My.Settings.TrimGPSnoise
    End Sub

    Public Async Function Main() As Task(Of Boolean)
        Try
            Dim allFiles As New List(Of GPXRecord)  ' všechny soubory v pracovním adresáři
            Dim localFiles As List(Of GPXRecord) = GetdAndProcessGPXFiles(False) 'seznam starých souborů, které už byly zpracovány
            allFiles.AddRange(localFiles) 'přidá staré soubory

            Dim remoteFiles As List(Of GPXRecord) = GetdAndProcessGPXFiles(True)
            If remoteFiles.Count > 0 AndAlso Me.NumberOfDogs > 1 Then
                Dim response = mboxQEx($"Found {remoteFiles.Count} new GPX files in remote directory:" & vbCrLf & $"{DogInfo.RemoteDirectory}." & vbCrLf & $"Do you really want to import them for the dog {DogInfo.Name}?")
                If response = DialogResult.No Then
                    RaiseEvent WarningOccurred("Import of new GPX files was cancelled by user.", Color.Red)
                    Return False 'vrátí false, pokud uživatel zrušil import
                End If
            End If

            'pokud jsou nějaké nové soubory, tak je spojí do jednoho listu
            Dim gpxFilesMerged As New List(Of GPXRecord)
            If remoteFiles.Count > 0 Then
                RaiseEvent WarningOccurred($"Found {remoteFiles.Count} new GPX files.", Color.DarkGreen)
                gpxFilesMerged = MergeGpxFiles(remoteFiles)
            End If
            allFiles.AddRange(gpxFilesMerged) 'přidá nové soubory

            If allFiles.Count = 0 Then
                RaiseEvent WarningOccurred("No GPX files found!", Color.Red)
                Return False 'vrátí false, pokud nejsou žádné soubory v zadaném intervalu
            End If

            Dim gpxFilesSortedAndFiltered As List(Of GPXRecord) = GetGPXFilesWithinInterval(allFiles)
            If gpxFilesSortedAndFiltered.Count = 0 Then
                RaiseEvent WarningOccurred("No GPX files found within the specified date interval.", Color.Red)
                Return False 'vrátí false, pokud nejsou žádné soubory v zadaném intervalu
            End If

            Me.TotalDistance = 0 'resetuje celkovou vzdálenost

            ' Zpracování každého GPX souboru
            For Each _gpxRecord As GPXRecord In gpxFilesSortedAndFiltered
                Try
                    If Not _gpxRecord.IsAlreadyProcessed Then 'možno přeskočit, už to proběhlo...
                        _gpxRecord.RenamewptNode(My.Resources.Resource1.artickle) 'renaming wpt to "artickle"
                        If prependDateToName Then _gpxRecord.PrependDateToFilename()
                        If trimGPS_Noise Then _gpxRecord.TrimGPSnoise(12) 'ořízne nevýznamné konce a začátky trailů, když se stojí na místě.
                    End If
                    Me.TotalDistance += _gpxRecord.TrailDistance
                    _gpxRecord.TotalDistance = Me.TotalDistance 'tohle není vlastnost recordu, záleží na zvoleném období
                    _gpxRecord.Description = _gpxRecord.BuildSummaryDescription() 'vytvoří popis, pokud není, nebo doplní věk trasy do popisu
                    'když už byl file v minulosti zpracován, tak se dál nemusí pokračovat, dialog by byl zbytečný
                    If _gpxRecord.LocalisedReports Is Nothing OrElse _gpxRecord.LocalisedReports.Count = 0 Then _gpxRecord.IsAlreadyProcessed = False
                    If Not _gpxRecord.IsAlreadyProcessed Then
                        _gpxRecord.Description = Await _gpxRecord.BuildLocalisedDescriptionAsync(_gpxRecord.Description) 'async kvůli počasí!
                    End If
                    _gpxRecord.TrailAge = _gpxRecord.GetAge
                    If _gpxRecord.IsAlreadyProcessed = False Then 'možno přeskočit, už to proběhlo...
                        _gpxRecord.WriteDescription() 'zapíše agregovaný popis do tracku Runner
                        _gpxRecord.WriteLocalizedReports() 'zapíše popis do DogTracku
                        _gpxRecord.IsAlreadyProcessed = True 'už byl soubor zpracován
                        _gpxRecord.Save()
                    End If


                    'a nakonec
                    _gpxRecord.SetCreatedModifiedDate()

                Catch ex As Exception
                    MessageBox.Show($"Reading or processing of the file {_gpxRecord.Reader.FileName} failed.")
                End Try

            Next _gpxRecord

            Me.GpxRecords = gpxFilesSortedAndFiltered
            If Me.GpxRecords IsNot Nothing And GpxRecords.Count > 0 Then
                Return True
            Else
                Return False
            End If
        Catch ex As Exception 'tohle aby to nezamrzlo, když se něco nepovede
            RaiseEvent WarningOccurred("Something went wrong", Color.Red)
            Return False

        End Try
    End Function


    Private Function GetdAndProcessGPXFiles(remoteFiles As Boolean) As List(Of GPXRecord)
        Dim GPXFRecords As New List(Of GPXRecord)

        If remoteFiles Then
            Dim downloadedPath = Path.Combine(Application.StartupPath, "AppData", "downloaded.json")
            'If Not File.Exists(downloadedPath) Then
            '    File.Copy(dowloadedDefault, downloadedPath)
            'End If
            Dim tracker As New FileTracker(downloadedPath)
                Dim Files As List(Of String) = Directory.GetFiles(DogInfo.RemoteDirectory, "*.gpx").ToList()
                Dim i As Integer = 0
                For Each remoteFilePath In Files

                'tracker.MarkAsProcessed(remoteFilePath) 'toto připraveno pro tiché zpracování Originals
                'Continue For '
                Try
                        If tracker.IsNewOrChanged(remoteFilePath) Then 'new files!!!
                            Debug.WriteLine("Zpracovávám: " & Path.GetFileName(remoteFilePath))
                            ' Pokud je soubor nový nebo změněný, zpracujeme ho
                            'zkopíruje soubor do lokálních adresářů
                            Dim localOriginalsFilePath As String = Path.Combine(DogInfo.OriginalsDirectory, Path.GetFileName(remoteFilePath))
                            If File.Exists(localOriginalsFilePath) Then
                                RaiseEvent WarningOccurred($"File  {Path.GetFileName(localOriginalsFilePath)} already exists in localOriginals directory!", Color.Red)
                            Else
                                IO.File.Copy(remoteFilePath, localOriginalsFilePath, False)
                            End If

                            Dim localProcessedFilePath As String = Path.Combine(DogInfo.ProcessedDirectory, Path.GetFileName(remoteFilePath))

                            i += 1
                            tracker.MarkAsProcessed(remoteFilePath)
                            Try
                                'načte z Originals:
                                Dim _reader As New GpxReader(localOriginalsFilePath)
                                'Ukládat bude do Processed!
                                _reader.FilePath = localProcessedFilePath
                                Dim _gpxRecord As New GPXRecord(_reader, Me.ForceProcess, DogInfo.Name)
                                _gpxRecord.SplitSegmentsIntoTracks() 'rozdělí trk s více segmenty na jednotlivé trk
                                _gpxRecord.IsAlreadyProcessed = False 'nastaví, že soubor ještě nebyl zpracován
                            _gpxRecord.CreateTracks() 'seřadí trk podle času
                            GPXFRecords.Add(_gpxRecord)
                        Catch ex As Exception
                            'pokud dojde k chybě při čtení souboru, vypíše se varování a pokračuje se na další soubor
                            RaiseEvent WarningOccurred($"Error reading file {Path.GetFileName(remoteFilePath)}: {ex.Message}", Color.Red)
                        End Try

                    Else 'zpracované soubory
                        Debug.WriteLine("Skipping: " & Path.GetFileName(remoteFilePath))
                    End If
                Catch ex As Exception

                End Try
            Next
            RaiseEvent WarningOccurred($"Found {i} new files.", Color.OrangeRed)

        Else 'local files
            Dim Files As List(Of String) = Directory.GetFiles(DogInfo.ProcessedDirectory, "*.gpx").ToList()
            For Each filePath In Files
                Try
                    Dim _reader As New GpxReader(filePath)
                    Dim _gpxRecord As New GPXRecord(_reader, Me.ForceProcess, DogInfo.Name)
                    _gpxRecord.CreateTracks() 'seřadí trk podle času
                    GPXFRecords.Add(_gpxRecord)
                Catch ex As Exception
                    'pokud dojde k chybě při čtení souboru, vypíše se varování a pokračuje se na další soubor
                    RaiseEvent WarningOccurred($"Error reading file {Path.GetFileName(filePath)}: {ex.Message}", Color.Red)
                End Try
            Next

        End If
        ' Seřazení podle data
        GPXFRecords.Sort(Function(x, y) x.TrailStart.Time.CompareTo(y.TrailStart.Time))
        Return GPXFRecords
    End Function


    Public Function GetGPXFilesWithinInterval(_records As List(Of GPXRecord)) As List(Of GPXRecord)
        Dim gpxFilesWithinInterval As New List(Of GPXRecord)

        For Each _gpxRecord In _records 'Directory.GetFiles(GpxLocalDirectory, "*.gpx")
            Try

                If _gpxRecord.TrailStart.Time.Date >= dateFrom.Date And _gpxRecord.TrailStart.Time.Date <= dateTo.Date Then
                    AddHandler _gpxRecord.WarningOccurred, AddressOf WriteRTBWarning
                    gpxFilesWithinInterval.Add(_gpxRecord)
                End If

            Catch ex As ArgumentException
                RaiseEvent WarningOccurred(ex.Message, Color.Red)
                Debug.WriteLine(ex.ToString())
            Catch ex As XmlException
                RaiseEvent WarningOccurred(ex.Message, Color.Red)
                Debug.WriteLine(ex.ToString())
            Catch ex As Exception
                RaiseEvent WarningOccurred(ex.Message, Color.Red)
                Debug.WriteLine(ex.ToString())

            End Try
        Next

        ' Seřazení podle data
        gpxFilesWithinInterval.Sort(Function(x, y) x.TrailStart.Time.CompareTo(y.TrailStart.Time))
        If gpxFilesWithinInterval.Count = 0 Then
            RaiseEvent WarningOccurred("No GPX files found within the specified date interval.", Color.Red)
        Else
            RaiseEvent WarningOccurred($"Found {gpxFilesWithinInterval.Count} GPX files within the specified date interval.", Color.DarkGreen)
        End If
        Return gpxFilesWithinInterval
    End Function


    Public Sub WriteRTBWarning(_message As String, _color As Color)
        RaiseEvent WarningOccurred(_message, _color)
    End Sub



    Public Function MergeGpxFiles(_gpxRecords As List(Of GPXRecord)) As List(Of GPXRecord)
        Dim gpxFilesMerged As New List(Of GPXRecord)
        If _gpxRecords.Count = 0 Then Return gpxFilesMerged 'ošetření prázdného listu

        Dim usedIndexes As New List(Of Integer) ' Seznam indexů, které už byly použity pro spojení

        For i As Integer = 0 To _gpxRecords.Count - 1
            If usedIndexes.Contains(i) Then Continue For ' Přeskočíme již spojené prvky, ty jsou smazány, do listu se tedy nepřidávají

            '_gpxRecords(i).CreateAndSortTracks()
            gpxFilesMerged.Add(_gpxRecords(i)) ' Přidáme aktuální prvek do merged listu
            If _gpxRecords(i).IsAlreadyProcessed Then Continue For  'možno přeskočit, už to proběhlo...

            Dim lastAddedIndex As Integer = gpxFilesMerged.Count - 1 ' Index posledního přidaného prvku

            Dim iTypes As New List(Of TrackType)
            Dim TraiRunnerStart As DateTime
            Dim DogStart As DateTime
            For k = 0 To _gpxRecords(i).Tracks.Count - 1
                Dim kType = _gpxRecords(i).Tracks(k).TrackType
                iTypes.Add(kType)
                Select Case kType
                    Case TrackType.RunnerTrail
                        TraiRunnerStart = _gpxRecords(i).Tracks(k).StartTrackGeoPoint.Time
                    Case TrackType.DogTrack
                        DogStart = _gpxRecords(i).Tracks(k).StartTrackGeoPoint.Time
                End Select
            Next k

            ' Vnitřní cyklus se pokouší spojit POSLEDNĚ PŘIDANÝ prvek s NÁSLEDUJÍCÍMI
            Dim j As Integer = i + 1
            While j < _gpxRecords.Count
                Dim timeDiff As TimeSpan = _gpxRecords(j).TrailStart.Time - _gpxRecords(lastAddedIndex).TrailStart.Time

                If timeDiff > maxAge Then
                    ' Další záznam je už moc starý, nemá cenu pokračovat
                    Exit While
                End If

                If Not usedIndexes.Contains(j) Then
                    '#TODO
                    'pokud jsou oba záznamy stejného typu dogtrack či RunnerTrail tak nespojovat!
                    Dim jTypes As New List(Of TrackType)
                    For k = 0 To _gpxRecords(j).Tracks.Count - 1
                        Dim kType = _gpxRecords(j).Tracks(k).TrackType
                        jTypes.Add(kType)
                        Select Case kType
                            Case TrackType.RunnerTrail
                                TraiRunnerStart = _gpxRecords(j).Tracks(k).StartTrackGeoPoint.Time
                            Case TrackType.DogTrack
                                DogStart = _gpxRecords(j).Tracks(k).StartTrackGeoPoint.Time
                        End Select
                    Next k
                    'TODO  když jtypes bude obsahovat dogTrack a i types RunnerTrail, tak zkontolovat, že pes je mladší!!! 
                    Dim haveCommonItem As Boolean = jTypes.Any(Function(x) iTypes.Contains(x))
                    If Not haveCommonItem Then
                        If DogStart <= TraiRunnerStart Then Continue For 'pes nemůže startovat dřív než Runner
                        If TryMerge(_gpxRecords(j), gpxFilesMerged(lastAddedIndex)) Then
                            usedIndexes.Add(j)
                            ' lastAddedIndex zůstává stejný, protože mergujeme do stejného objektu
                            gpxFilesMerged(lastAddedIndex).CreateTracks() 'seřadí trk podle času!!
                        End If
                    End If
                End If

                j += 1
            End While

        Next i

        Return gpxFilesMerged
    End Function

    Private Function TryMerge(file_i As GPXRecord, file_prev As GPXRecord) As Boolean
        'vrací true pokud došlo ke vnoření souboru file_i do file_prev nebo pokud byl soubor smazán jako duplicitní
        'najdi všechny sousední soubory, které se liší o méně než MaxAge
        ' Základní kontrola, zda rozdíl dat splňuje podmínku na max stáří
        'kontrola duplicit: když je rozdíl menší než jedna sekunda, je to nejspíš stejný track
        If (file_i.TrailStart.Time - file_prev.TrailStart.Time < New TimeSpan(0, 0, 1)) Then
            Dim question As String = $"Tracks in files 
{file_i.Reader.FileName} 
and 
{file_prev.Reader.FileName} 
have same start time. 
I suspect it's a duplication. 

Should we delete the {file_i.Reader.FileName} file?

Be carefull with this!!!!!"
            Dim result As DialogResult = MessageBox.Show(question, "Delete duplicate file?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
            If result = DialogResult.Yes Then
                IO.File.Delete(file_i.Reader.FilePath)
                RaiseEvent WarningOccurred($"File {file_i.Reader.FileName} was deleted because it is duplicate", Color.DarkOrange)
                Return True
                'ElseIf result = DialogResult.No Then
                '    SaveMergeDecision(file_prev, file_i, result.ToString)
            End If
        End If

        ' Zeptej se uživatele, zda chce soubory spojit
        Dim mergeFiles As DialogResult
        If Not mergeNoAsk Then mergeFiles = DialogMergeFiles(file_prev, file_i)
        ' Pokud uživatel souhlasí, spoj soubory, jinak přidej
        If mergeNoAsk OrElse (mergeFiles = DialogResult.Yes) Then
            If file_prev.MergeDogToMe(file_i) Then
                Return True
            End If
        End If

        'End If
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
        Dim lblDotaz As New System.Windows.Forms.Label()
        lblDotaz.Text = My.Resources.Resource1.lblMergeTwoToOneQ '"Chcete spojit tyto dva soubory do jednoho?"
        lblDotaz.AutoSize = True
        lblDotaz.Location = New Point(10, 10)
        lblDotaz.ForeColor = Color.Maroon
        dialog.Controls.Add(lblDotaz)

        ' Popis (volitelný, můžeš ho upravit podle potřeby)
        Dim lblPopis As New System.Windows.Forms.Label()
        lblPopis.Text = My.Resources.Resource1.lblMergingYouGet ' "Spojením získáte jeden soubor gpx obsahující trasu kladeče i psa." ' Příklad popisu
        lblPopis.AutoSize = True
        lblPopis.Location = New Point(10, lblDotaz.Bottom + 5)
        dialog.Controls.Add(lblPopis)

        ' Popisky se jmény souborů
        Dim lblSoubor1 As New System.Windows.Forms.Label()
        lblSoubor1.Text = $"{My.Resources.Resource1.lblIsThisRunnerQ}: '{Path.GetFileName(runner.Reader.FilePath)}'"
        lblSoubor1.AutoSize = True
        lblSoubor1.Location = New Point(10, lblPopis.Bottom + 10)
        lblSoubor1.ForeColor = Color.Maroon
        dialog.Controls.Add(lblSoubor1)

        Dim lblSoubor2 As New System.Windows.Forms.Label()
        lblSoubor2.Text = $"{My.Resources.Resource1.lblIsThisTrackOfTheDog}: '{Path.GetFileName(dog.Reader.FilePath)}' ?"
        lblSoubor2.AutoSize = True
        lblSoubor2.Location = New Point(10, lblSoubor1.Bottom + 5)
        lblSoubor2.ForeColor = Color.Maroon
        dialog.Controls.Add(lblSoubor2)

        '' Zaškrtávací políčko pro zapamatování rozhodnutí
        'Dim chbRemembDecision As New System.Windows.Forms.CheckBox
        'chbRemembDecision.Text = My.Resources.Resource1.chbRemembDecisQ '"Zapamatovat rozhodnutí 'Ne' pro tuto dvojici"
        'chbRemembDecision.Location = New Point(10, lblSoubor2.Bottom + 10)
        'chbRemembDecision.AutoSize = True
        'chbRemembDecision.Checked = True
        'dialog.Controls.Add(chbRemembDecision)

        ' Tlačítka
        Dim btnAno As New System.Windows.Forms.Button()
        btnAno.Text = "Yes"
        btnAno.DialogResult = DialogResult.Yes ' Nastavení výsledku dialogu
        btnAno.Location = New Point(10, lblSoubor2.Bottom + 15)
        btnAno.AutoSize = True
        dialog.Controls.Add(btnAno)

        Dim btnNe As New System.Windows.Forms.Button()
        btnNe.Text = "No"
        btnNe.DialogResult = DialogResult.No
        btnNe.Location = New Point(btnAno.Right + 10, lblSoubor2.Bottom + 15)
        btnNe.AutoSize = True
        dialog.Controls.Add(btnNe)

        ' Zaškrtávací políčko pro automatické spojování
        Dim rbNoAsk As New System.Windows.Forms.RadioButton()
        rbNoAsk.Text = My.Resources.Resource1.rbDontAskMergeQ '"U dalších dvojic se už neptat a rovnou spojit (opatrně!)"
        rbNoAsk.Location = New Point(10, btnNe.Bottom + 15)
        rbNoAsk.AutoSize = True
        dialog.Controls.Add(rbNoAsk)

        ' Zaškrtávací políčko pro zrušení spojování
        Dim rbCancel As New System.Windows.Forms.RadioButton()
        rbCancel.Text = My.Resources.Resource1.rbDontAskDontMerge ' "U dalších dvojic se už neptat a nic nespojovat"
        rbCancel.Location = New Point(10, rbNoAsk.Bottom + 7)
        rbCancel.AutoSize = True
        dialog.Controls.Add(rbCancel)

        ' Zaškrtávací políčko pro zrušení spojování
        Dim rbAsk As New System.Windows.Forms.RadioButton()
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

        '' Uložení stavu zaškrtávacího políčka  do nastavení aplikace
        'If result = DialogResult.No AndAlso chbRemembDecision.Checked Then
        '    SaveMergeDecision(runner, dog, result.ToString)
        'End If

        mergeNoAsk = rbNoAsk.Checked ' Uložení stavu pro použití v hlavní funkci
        mergeCancel = rbCancel.Checked
        Return result
    End Function


End Class


Public Class GPXRecord

    Public Event WarningOccurred(_message As String, _color As Color)

    Public Property Tracks As New List(Of TrackAsTrkNode)
    Public Property DogName As String

    Public ReadOnly Property WptNodes As TrackAsTrkPts
        Get
            If Me.Reader Is Nothing Then
                Throw New InvalidOperationException("Reader nebyl nastaven.")
            End If
            Dim _wptNodes As XmlNodeList = Me.Reader.SelectNodes("wpt")
            Return New TrackAsTrkPts(TrackType.Artickle, _wptNodes) 'vrací seznam všech wpt, které jsou v souboru
        End Get
    End Property


    Public ReadOnly Property RunnerStart As TrackGeoPoint
        Get
            Return Me.Tracks.FirstOrDefault(Function(t) t.TrackType = TrackType.RunnerTrail)?.StartTrackGeoPoint
        End Get
    End Property


    Public ReadOnly Property DogStart As TrackGeoPoint
        Get
            Return Me.Tracks.FirstOrDefault(Function(t) t.TrackType = TrackType.DogTrack)?.StartTrackGeoPoint
        End Get
    End Property

    Public ReadOnly Property DogFinish As TrackGeoPoint
        Get

            Return Me.Tracks.LastOrDefault(Function(t) t.TrackType = TrackType.DogTrack)?.EndTrackGeoPoint
        End Get
    End Property
    Dim _TrackStats As New TrackStats
    Public ReadOnly Property TrackStats As TrackConverter.TrackStats
        Get
            Return _TrackStats
        End Get
    End Property

    Public ReadOnly Property TrailDistance As Double
        Get
            If Me.Tracks.Count = 0 Then Return Nothing
            For Each track As TrackAsTrkNode In Me.Tracks
                If track.TrackType = TrackType.RunnerTrail Then
                    Return track.TrackStats.DistanceKm
                ElseIf track.TrackType = TrackType.DogTrack Then
                    Return track.TrackStats.DistanceKm
                Else

                End If
            Next track

            Return Nothing 'pokud nenajde žádný RunnerTrail, vrátí Nothing
        End Get
    End Property

    Dim _dogDeviation As Double = -1.0F
    Public ReadOnly Property dogDeviation As Double
        Get
            If _dogDeviation >= 0 Then 'cache
                Return _dogDeviation
            End If
            If Me.Tracks.Count < 2 Then Return -1.0F 'pokud je jen jeden track, tak není co porovnávat
            For Each track As TrackAsTrkNode In Me.Tracks
                If track.TrackType = TrackType.DogTrack Then
                    Return track.TrackStats.Deviation
                End If
            Next track
            Return -1.0F 'pokud nenajde žádný dogTrail 
        End Get
    End Property

    Public ReadOnly Property TrailStart As TrackGeoPoint
        Get
            If Me.Tracks?.Count = 0 Then
                Return Nothing
            Else 'vrací  vrací start prvního tracku
                Return Me.Tracks(0)?.StartTrackGeoPoint
            End If
        End Get
    End Property



    Public ReadOnly Property FileName As String
        Get
            Return Me.Reader.FileName
        End Get
    End Property

    'Public Property TrkNodes As XmlNodeList
    Public Property TrailAge As TimeSpan = TimeSpan.Zero
    Public Property TotalDistance As Double = CDbl(0) 'celková vzdálenost všech zpracovaných trailů, záleží na zvoleném období
    Public Property Description As String
    'Public Property DescriptionParts As List(Of (Text As String, Color As Color, FontStyle As FontStyle))

    Public ReadOnly Property Link As String
        Get
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
        End Get
    End Property

    Dim _DogSpeed As Double = -1.0F
    Public ReadOnly Property DogSpeed As Double
        Get
            If _DogSpeed >= 0 Then 'cache
                Return _DogSpeed
            End If

            Dim dogTrackDistance As Double = -1
            For Each track As TrackAsTrkNode In Me.Tracks
                If track.TrackType = TrackType.DogTrack Then
                    Return track.TrackStats.SpeedKmh
                End If
            Next track

            Return -1.0F
        End Get
    End Property

    Public Property Reader As GpxReader

    Dim _IsAlreadyProcessed As Boolean = False 'zda už byl soubor zpracován, pokud ano, tak se nezpracovává znovu
    Public Property IsAlreadyProcessed As Boolean
        Get
            Return _IsAlreadyProcessed
        End Get
        Set(value As Boolean)
            _IsAlreadyProcessed = value
            Dim metadataNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "metadata", Me.Reader.rootNode)
            If metadataNode Is Nothing Then
                metadataNode = Me.Reader.CreateElement("metadata")
                Me.Reader.rootNode.PrependChild(metadataNode)
            End If
            Dim extensionsNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "extensions", metadataNode)
            If value Then
                If extensionsNode Is Nothing Then
                    extensionsNode = Me.Reader.CreateElement("extensions")
                    metadataNode.PrependChild(extensionsNode)
                End If
                Dim processedNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "processed", extensionsNode)
                If processedNode Is Nothing Then
                    Me.Reader.CreateAndAddElement(extensionsNode, GpxReader.K9_PREFIX & ":" & "processed", DateTime.Now, False,,, GpxReader.K9_NAMESPACE_URI)
                Else
                    processedNode.InnerText = DateTime.Now
                End If
            Else 'smaže zápis!!
                If extensionsNode Is Nothing Then
                    'OK
                Else
                    Dim ProcessedNodes As XmlNodeList = Me.Reader.SelectChildNodes(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "extensions", Me.Reader.rootNode)

                    Me.Reader.DeleteElements(ProcessedNodes, GpxReader.K9_PREFIX & ":" & "processed")
                End If

            End If
        End Set
    End Property

    '' 🔧 Lokálně nastav labely 
    'Private goalLabel As String = "📍"
    'Private trailLabel As String = "👣"
    'Private pperformanceLabel As String = "🐕"
    Public LocalisedReports As New Dictionary(Of String, TrailReport)
    'Private Property WindDirection As Double = 0.0 ' Směr větru v stupních
    Public Property WeatherData As (_temperature As Double?, _windSpeed As Double?, _windDirection As Double?, _precipitation As Double?, _relHumidity As Double?, _cloudCover As Double?)

    'Private ReadOnly Property gpxDirectory As String
    'Private ReadOnly Property BackupDirectory As String

    Private Const NBSP As String = ChrW(160)

    Public ReadOnly Property TrkNodes As XmlNodeList
        Get
            If Me.Reader Is Nothing Then
                Throw New InvalidOperationException("Reader nebyl nastaven.")
            End If
            Return Me.Reader.SelectNodes("trk")
        End Get
    End Property



    Public Sub New(_reader As GpxReader, forceProcess As Boolean, _dogname As String)
        'gpxDirectory = 
        Me.Reader = _reader
        Me.DogName = _dogname
        If forceProcess Then
            _IsAlreadyProcessed = False
        Else
            _IsAlreadyProcessed = IsProcessed()
        End If


        Me.LocalisedReports = ReadLocalisedReport()

    End Sub

    Friend Sub SetCreatedModifiedDate()
        'change of attributes
        ' Setting the file creation date
        IO.File.SetCreationTime(Me.Reader.FilePath, Me.TrailStart.Time)
        ' Setting the last modified file date
        IO.File.SetLastWriteTime(Me.Reader.FilePath, Me.TrailStart.Time)
    End Sub

    Public Sub WriteRTBWarning(_message As String, _color As Color)
        RaiseEvent WarningOccurred(_message, _color)
    End Sub


    Public Function GetAgeFromTime() As TimeSpan
        Dim ageFromTime As TimeSpan = TimeSpan.Zero
        If Me.DogStart Is Nothing Or Me.RunnerStart Is Nothing Then
            Return Nothing
        End If
        Try
            If Me.DogStart.Time <> Date.MinValue AndAlso Me.RunnerStart.Time <> Date.MinValue Then
                ageFromTime = Me.DogStart.Time - Me.RunnerStart.Time
            End If
        Catch ex As Exception
            Return Nothing
        End Try
        Return ageFromTime
    End Function

    Public Function GetAge() As TimeSpan
        ' Vrací dvojici: (Age, IsNotInComments)
        Dim ageFromTime As TimeSpan = GetAgeFromTime()
        Dim ageFromComments As TimeSpan = TimeSpan.Zero


        Dim ageIsNotInComments As Boolean = (ageFromComments = TimeSpan.Zero)
        If ageFromTime.TotalMinutes > 0 Then
            Return ageFromTime
        Else
            If Not String.IsNullOrWhiteSpace(Me.Description) Then ageFromComments = GetAgeFromComments(Me.Description)
            If ageFromComments.TotalMinutes > 0 Then
                Return ageFromComments
            Else
                Debug.WriteLine($"Age of the trail { Me.Reader.FileName} wasn't found!")
                Return TimeSpan.Zero
            End If
        End If
        Return Nothing
    End Function


    Public Function IsProcessed() As Boolean
        Try
            Dim metadataNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "metadata", Me.Reader.rootNode)
            If metadataNode Is Nothing Then Return False
            Dim extensionsNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "extensions", metadataNode)
            If extensionsNode Is Nothing Then Return False ' <extensions> vůbec neexistuje
            Dim processedNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "processed", extensionsNode)
            If processedNode Is Nothing Then Return False ' neexistuje záznam
            Return True
        Catch ex As Exception
            Return False
        End Try

    End Function

    ' Function to set the <desc> description from the first <trk> node in the GPX file
    Public Sub WriteDescription()

        If Not String.IsNullOrWhiteSpace(Me.Description) Then
            ' Find the first <trk> node and its <desc> subnode
            'Dim trkNodes As XmlNodeList = Me.Reader.SelectNodes("trk")
            Dim RunnerTrailTrk As XmlNode = Nothing ' Inicializace proměnné pro <trk> s <type>RunnerTrail</type>
            ' Najdeme <trk> s <type>RunnerTrail</type>
            For Each trkNode As XmlNode In Me.TrkNodes
                Dim extensions As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "extensions", trkNode)

                Dim typeNodes As XmlNodeList = Me.Reader.SelectChildNodes(GpxReader.K9_PREFIX & ":" & "TrackType", extensions)
                For Each typeNode As XmlNode In typeNodes
                    ' Zkontrolujeme, zda <type> obsahuje "RunnerTrail"
                    ' Pokud ano, uložíme tento <trk> do RunnerTrailTrk
                    ' a ukončíme cyklus
                    If typeNode IsNot Nothing AndAlso typeNode.InnerText.Trim().Equals(TrackType.RunnerTrail.ToString, StringComparison.OrdinalIgnoreCase) Then
                        RunnerTrailTrk = trkNode
                        GoTo FoundRunnerTrailTrk
                    End If
                Next
            Next

FoundRunnerTrailTrk:
            Dim descNodeRunner As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "desc", RunnerTrailTrk)
            ' Pokud uzel <desc> neexistuje, vytvoříme jej a přidáme do <trk>
            If descNodeRunner Is Nothing Then
                descNodeRunner = Me.Reader.CreateElement("desc")
                If RunnerTrailTrk IsNot Nothing Then
                    ' Vytvoříme nový uzel <desc>
                    ' Přidání <desc> jako prvního potomka v uzlu <trk>
                    If RunnerTrailTrk.HasChildNodes Then
                        ' Vloží <desc> před první existující poduzel
                        RunnerTrailTrk.InsertBefore(descNodeRunner, RunnerTrailTrk.FirstChild)
                    Else
                        ' Pokud <trk> nemá žádné poduzly, použijeme AppendChild
                        RunnerTrailTrk.AppendChild(descNodeRunner)
                    End If
                End If
            End If


            ' Vytvoříme nový CDATA uzel
            Dim cdata As XmlCDataSection = Me.Reader.xmlDoc.CreateCDataSection(Me.Description)
            descNodeRunner.RemoveAll()
            ' Přidáme do descNode
            descNodeRunner.AppendChild(cdata)

        End If
    End Sub


    Public Function GetAgeFromComments(inputText As String) As TimeSpan
        ' Upravený regulární výraz pro nalezení čísla, které může být i desetinné
        Dim regex As New Regex("\d+[.,]?\d*\s*(h(odin(y|a))?|hod|min(ut)?)(?=\W|$)", RegexOptions.IgnoreCase)
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

    Public Function GetLengthFromComments(inputText As String) As Single
        ' Upravený regulární výraz pro nalezení čísla, které může být i desetinné
        Dim regex As New Regex("\b\d+[.,]?\d*\s*(m(metr(y|ů))?|km|kilometrů?)\b", RegexOptions.IgnoreCase)
        Dim match As Match = regex.Match(inputText)

        If match.Success Then
            Dim nalezenaDelka As String = match.Value
            ' Převede desetinnou tečku nebo čárku na standardní tečku pro parsování
            Dim delkaString As String = Regex.Match(nalezenaDelka, "\d+[.,]?\d*").Value.Replace(",", ".")
            Dim delkaCislo As Double = Double.Parse(delkaString, CultureInfo.InvariantCulture)




            If nalezenaDelka.Contains("km") Then

                Return delkaCislo ' in km
            ElseIf nalezenaDelka.Contains("m"c) Then

                Return delkaCislo / 1000
            End If
        End If

        ' Pokud nebyla nalezena, vrátí 0
        Return 0
    End Function




    Public Function OdstranDataACasZNazvuSouboru(fileName As String) As String
        ' Regex pro data v různých formátech (nyní s ne-zachytávajícími skupinami pro lepší chování)
        Dim Separator As String = "\s*(?:\.|_|-|,|\._)\s*" ' Pomlčka, podtržítko nebo tečka
        Dim isoSeparator As String = "\s*(?:[-/_]|\.)\s*" ' Více separátorů
        'Separator = "\._"
        ' Definice regulárního výrazu s pojmenovanými skupinami
        Dim pattern As String =
        $"(?<eu>(?<day>[0-2]\d|3[01]){Separator}(?<month>0[1-9]|1[0-2]){Separator}(?<year>\d{{4}}(?!\w)))|" &
        $"(?<us>(?<month>0[1-9]|1[0-2]){Separator}(?<day>[0-2]\d|3[01]){Separator}(?<year>\d{{4}}(?!\w)))|" &
        $"(?<iso>(?<year>\d{{4}}){isoSeparator}(?<month>0[1-9]|1[0-2]){isoSeparator}(?<day>[0-2]\d|3[01](?!\w)))"

        ' Regex pro čas v různých formátech
        Dim timeRegex As String = "(?:0?[0-9]|1[0-9]|2[0-3])[:\.](?:[0-5][0-9])(?:[:\.](?:[0-5][0-9]))?"

        ' Kombinace obou regexů a nahrazení prázdným řetězcem
        Dim result As String = Regex.Replace(fileName, "(" & pattern & "|" & timeRegex & ")", " ").Trim()

        ' Odstranění vícenásobných mezer
        result = Regex.Replace(result, "\s+", " ")

        Return result
    End Function



    Public Function GetRemoveDateFromName() As (DateTime, String)

        Dim Separator As String = "\s*(?:\.|_|-|,|\._)\s*" ' Pomlčka, podtržítko nebo tečka
        Dim isoSeparator As String = "\s*(?:[-/_]|\.)\s*" ' Více separátorů
        'Separator = "\._"
        ' Definice regulárního výrazu s pojmenovanými skupinami
        Dim datePattern1 As String =
        $"(?<eu>(?<day>[0-2]\d|3[01]){Separator}(?<month>0[1-9]|1[0-2]){Separator}(?<year>\d{{4}}))|" &
        $"(?<us>(?<month>0[1-9]|1[0-2]){Separator}(?<day>[0-2]\d|3[01]){Separator}(?<year>\d{{4}}))|" &
        $"(?<iso>(?<year>\d{{4}}){isoSeparator}(?<month>0[1-9]|1[0-2]){isoSeparator}(?<day>[0-2]\d|3[01]))"

        Dim datePattern2 As String = $"(?<eu2>(\d+){Separator}(\d+){Separator}(\d+))"
        ' Regex pro čas v různých formátech
        Dim timeRegex As String = "(?:0?[0-9]|1[0-9]|2[0-3])[:\.](?:[0-5][0-9])(?:[:\.](?:[0-5][0-9]))?"
        Dim myRegex As New Regex(datePattern1 & "|" & datePattern2 & "|" & timeRegex)
        Dim fileName As String = Me.Reader.FileName

        Dim match As Match = myRegex.Match(fileName)
        If match.Success Then
            Dim dateTimeFromFileName As Date = New DateTime

            Try
                Dim _year As Integer = Integer.Parse(match.Groups("year").Value)
                Dim _month As Integer = Integer.Parse(match.Groups("month").Value)
                Dim _day As Integer = Integer.Parse(match.Groups("day").Value)

                If (match.Groups("eu").Success Or match.Groups("eu2").Success) And CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.StartsWith("d") Then
                    ' Evropský formát
                    dateTimeFromFileName = New DateTime(_year, _month, _day)
                ElseIf match.Groups("us").Success And CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.StartsWith("M") Then
                    ' Americký formát
                    dateTimeFromFileName = New DateTime(_year, _month, _day)
                ElseIf match.Groups("iso").Success Then
                    ' ISO formát (YYYY-MM-DD)
                    dateTimeFromFileName = New DateTime(_year, _month, _day)
                Else
                    dateTimeFromFileName = New DateTime(_year, _day, _month)
                End If

                ' Odstranění nalezeného datového řetězce z názvu souboru
                Dim modifiedFileName As String = myRegex.Replace(fileName, "").Trim()
                ' Kombinace obou regexů a nahrazení prázdným řetězcem

                'odstraní znaky na začátku a konci
                Dim charsToTrim As Char() = {"_", "-", ".", " "}
                modifiedFileName = modifiedFileName.Replace(".gpx", "")
                modifiedFileName = modifiedFileName.TrimStart(charsToTrim).TrimEnd(charsToTrim)
                modifiedFileName = modifiedFileName & ".gpx" ' Přidání přípony .gpx zpět, pokud je potřeba"
                ' Vrácení data i upraveného názvu souboru
                Return (dateTimeFromFileName, modifiedFileName)
            Catch ex As Exception
                Debug.WriteLine($"{fileName} - Error in date format")
                Return (Nothing, fileName)
            End Try
        Else
            Debug.WriteLine($"{fileName} - Date not found")
            Return (Nothing, fileName)
        End If

    End Function


    Private Function ExtractOriginalText(original As String, foundNorm As String) As String
        If String.IsNullOrEmpty(foundNorm) Then Return ""
        Dim normOriginal = RemoveDiacritics(original).ToLowerInvariant()
        Dim index = normOriginal.IndexOf(foundNorm.ToLowerInvariant())
        If index >= 0 Then
            Return original.Substring(index, foundNorm.Length).Trim()
        End If
        Return foundNorm
    End Function


    Private Function RemoveDiacritics(text As String) As String
        Dim normalized As String = text.Normalize(System.Text.NormalizationForm.FormD)
        Dim sb As New System.Text.StringBuilder()
        For Each c As Char In normalized
            If Globalization.CharUnicodeInfo.GetUnicodeCategory(c) <> Globalization.UnicodeCategory.NonSpacingMark Then
                sb.Append(c)
            End If
        Next
        Return sb.ToString().Normalize(System.Text.NormalizationForm.FormC)
    End Function


    ' Funkce, která jen rozdělí text na části a doplní stáří trailu
    Public Function ExtractDescriptionParts(originalDescription As String) As TrailReport
        Dim goalPart As String = ""
        Dim trailPart As String = ""
        Dim dogPart As String = ""
        ' 1️⃣ Odstraníme HTML tagy
        Dim cleanDescription As String = System.Text.RegularExpressions.Regex.Replace(originalDescription, "<.*?>", "").Trim()

        ' 2️⃣ Najdeme části pomocí regexu
        Dim g = TrailReport.goalLabel
        Dim t = TrailReport.trailLabel
        Dim p = TrailReport.performanceLabel
        Dim pattern As String = $"(?:(?:(?<goal>{g}|g:)\s*(?<goalText>.*?))(?=({t}|t:|{p}|p:|🌡|$)))?" &
                            $"(?:(?:(?<trail>{t}|t:)\s*(?<trailText>.*?))(?=({g}|g:|{p}|p:|🌡|$)))?" &
                            $"(?:(?:(?<dog>{p}|p:)\s*(?<dogText>.*?))(?=({g}|g:|{t}|t:|🌡|$)))?"

        Dim regex As New Regex(pattern, RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        Dim match As Match = regex.Match(cleanDescription)

        If match.Success Then
            goalPart = match.Groups("goalText").Value.Trim()
            trailPart = match.Groups("trailText").Value.Trim()
            dogPart = match.Groups("dogText").Value.Trim()
        End If

        ' 3️⃣ Pokud žádná část nebyla nalezena, použij celý text jako trailPart
        If String.IsNullOrWhiteSpace(goalPart) AndAlso String.IsNullOrWhiteSpace(trailPart) AndAlso String.IsNullOrWhiteSpace(dogPart) Then
            trailPart = cleanDescription
        End If


        ' 🕰 Trail part – doplníme čas a délku pokud nejsou
        Dim ageFromTime As TimeSpan = GetAgeFromTime()
        If trailPart <> "" Then
            Dim ageFromComments As TimeSpan = GetAgeFromComments(trailPart)
            If ageFromComments = TimeSpan.Zero Then
                ' Odebereme případný starý čas z trailPart (např. "1.2 h něco")
                'trailPart = Regex.Replace(trailPart, "^[0-9\.,]+\s*h\s*", "", RegexOptions.IgnoreCase).Trim()
                trailPart = Regex.Replace(trailPart, "\d+[.,]?\d*\s*(h(odin(y|a))?|hod|min(ut)?)(?=\W|$)", "", RegexOptions.IgnoreCase).Trim()
                trailPart = trailPart.Replace(My.Resources.Resource1.outAge.ToLower & ":", "") ' odstranění age:
                trailPart = trailPart.Replace("⏳:", "") ' odstranění 🕒:
                trailPart = "⏳:" & NBSP & ageFromTime.TotalHours.ToString("F1") & NBSP & "h, " & trailPart
            End If
            Dim LengthfromComments As Single = GetLengthFromComments(trailPart)
            If LengthfromComments = 0 Then
                ' Odebereme případnou starou délku z trailPart (např. "1.2 km něco")
                trailPart = Regex.Replace(trailPart, "^[0-9\.,]+\s*(km|m)(?=\W|$)", "", RegexOptions.IgnoreCase).Trim()
                trailPart = trailPart.Replace(My.Resources.Resource1.outLength.ToLower & ":", "") ' odstranění vícenásobných mezer
                trailPart = trailPart.Replace("📏:", "") '
                trailPart = "📏:" & NBSP & Me.TrailDistance.ToString("F1") & NBSP & "km, " & trailPart
            End If

        Else
            If Me.TrailDistance > 0 Then
                trailPart = "📏:" & NBSP & Me.TrailDistance.ToString("F1") & NBSP & "km"
            End If
            If ageFromTime.TotalHours > 0 Then
                trailPart &= "  ⏳:" & NBSP & ageFromTime.TotalHours.ToString("F1") & NBSP & "h"
            End If

        End If

        Return New TrailReport(Me.DogName, goalPart, trailPart, dogPart, (Nothing, Nothing, Nothing, Nothing, Nothing, Nothing))
    End Function


    Private Async Function BuildDescription(desc As TrailReport) As Task(Of String)
        Dim goalPart As String = desc.Goal.Text
        Dim trailPart As String = desc.Trail.Text
        Dim performancePart As String = desc.Performance.Text
        Dim crlf As String = "<br>"

        ' 🌧🌦☀ Počasí
        'Wheather() 'získá počasí
        WeatherData = Await Wheather()
        Dim strWeather As String = $"🌡{CDbl(WeatherData._temperature).ToString("0.#")}{NBSP}°C  💨{NBSP}{CDbl(WeatherData._windSpeed).ToString("0.#")}{NBSP}m/s {WindDirectionToText(WeatherData._windDirection)} 💧{WeatherData._relHumidity}{NBSP}%   💧{WeatherData._precipitation}{NBSP}mm/h ⛅{WeatherData._cloudCover}{NBSP}%"
        If WeatherData._temperature Is Nothing Then 'pokud se nenačetla data o počasí
            strWeather = ""
        End If
        desc.weather.Text = strWeather


        Dim sb As New System.Text.StringBuilder()

        If goalPart <> "" Then sb.Append(TrailReport.goalLabel & " " & goalPart & crlf)
        sb.Append(TrailReport.trailLabel & " " & trailPart & crlf)
        If performancePart <> "" Then sb.Append(TrailReport.performanceLabel & " " & performancePart & crlf)

        sb.Append(strWeather)

        Return sb.ToString().Trim()
    End Function


    '' Funkce pro sestavení popisu ze všech <trk> uzlů
    Public Function BuildSummaryDescription() As String

        'Dim crossDescs As New List(Of String)
        Dim goalDesc As String = ""

        Dim SummaryDescription As String = ""

        For Each track In Me.Tracks
            Dim trkNode As XmlNode = track.TrkNode
            Dim extensionsNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "extensions", trkNode)
            Select Case track.TrackType 'tohle kvůli rozdělení pro další editaci v extractDescriptionParts
                Case TrackType.RunnerTrail
                    Dim descNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "desc", trkNode)
                    If descNode IsNot Nothing Then
                        Dim trailDesc As String = descNode.InnerText.Trim()
                        If Not (trailDesc.Contains(TrailReport.trailLabel) _
                                Or trailDesc.Contains("t:", StringComparison.InvariantCultureIgnoreCase) _
                                Or trailDesc.Contains("g:", StringComparison.InvariantCultureIgnoreCase) _
                                 Or trailDesc.Contains("p:", StringComparison.InvariantCultureIgnoreCase)) Then
                            SummaryDescription &= TrailReport.trailLabel & trailDesc & " "
                        Else
                            SummaryDescription &= trailDesc & " "
                        End If
                    End If
                Case TrackType.DogTrack
                    Dim descNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "desc", trkNode)
                    If descNode IsNot Nothing Then
                        Dim dogDesc As String = descNode.InnerText.Trim()

                        If Not (dogDesc.Contains(TrailReport.performanceLabel) Or dogDesc.Contains("p:", StringComparison.InvariantCultureIgnoreCase)) Then
                            SummaryDescription &= TrailReport.performanceLabel & dogDesc & " "
                        Else
                            SummaryDescription &= dogDesc & " "
                        End If
                    End If
                Case Else
                    ' Pro ostatní typy tracků, které nejsou RunnerTrail nebo DogTrack, můžeme přidat další logiku
                    Dim descNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "desc", trkNode)
                    If descNode IsNot Nothing Then
                        Dim crossDesc As String = descNode.InnerText.Trim()
                        SummaryDescription &= crossDesc & " "
                    End If
            End Select
            'Dim descNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "desc", trkNode)

            'SummaryDescription &= descNode?.InnerText.Trim() & " " ' Získání textu z <desc> uzlu, pokud existuje
        Next


        Return SummaryDescription.ToString().Trim()


    End Function

    Public Async Function BuildLocalisedDescriptionAsync(summaryDescription As String) As Task(Of String)
        Dim lang As String
        Dim newDescription As String = ""
        If Me.LocalisedReports Is Nothing OrElse Me.LocalisedReports.Count = 0 Then
            Dim desc = ExtractDescriptionParts(summaryDescription)
            lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant() 'todo
            Me.LocalisedReports.Add(lang, desc)
            newDescription = Await BuildDescription(desc)
        End If

        Dim firstLocalisedReport As KeyValuePair(Of String, TrailReport) = Me.LocalisedReports.FirstOrDefault()

        Dim keys = LocalisedReports.Keys.ToList()
        Dim totalCount = keys.Count

        For i = 0 To 3 'dočasně přeskakuji!!
            Dim frm As frmEditComments
            Dim report As TrailReport
            Dim result As DialogResult
            If i < totalCount Then ' Existující položky
                lang = keys(i)
                report = LocalisedReports(lang)
                Debug.WriteLine($"{i + 1}. {lang} – {report.Goal.Text}")

                frm = New frmEditComments With {.TrailDescription = report,
                                                    .GpxFileName = Me.Reader.FileName,
                                                    .Language = lang
                                              }
                result = frm.ShowDialog()
                newDescription = Await BuildDescription(frm.TrailDescription)
                If lang <> frm.Language Then ' aktualizace jazyka
                    LocalisedReports.Remove(lang) ' odstranění starého jazyka
                    lang = frm.Language ' aktualizace jazyka
                    Debug.WriteLine($"Jazyk změněn na: {lang}")
                    frm.TrailDescription.WeatherData = Me.WeatherData
                    Me.LocalisedReports.Add(lang, frm.TrailDescription) ' přidání nového jazyka
                Else
                    frm.TrailDescription.WeatherData = Me.WeatherData ' aktualizace počasí
                    Me.LocalisedReports(lang) = frm.TrailDescription ' aktualizace existujícího reportu
                End If

                Select Case result
                    Case DialogResult.Retry ' třeba tlačítko "Znovu" nebo "Pokračovat"
                        ' smyčka poběží dál
                    Case Else
                        Exit For
                End Select
            Else 'přidáme nový jazyk!!!!
                report = New TrailReport(firstLocalisedReport.Value.DogName.Text, firstLocalisedReport.Value.Goal.Text,
                                        firstLocalisedReport.Value.Trail.Text,
                                        firstLocalisedReport.Value.Performance.Text,
                                         (Nothing, Nothing, Nothing, Nothing, Nothing, Nothing))
                ' Nová položka – prázdná, připravená k vyplnění
                Debug.WriteLine($"{i + 1}. [Nový záznam]")

                frm = New frmEditComments With {.TrailDescription = report,
                                                    .GpxFileName = Me.Reader.FileName,
                                                    .Language = Nothing
                                              }

                result = frm.ShowDialog()
                lang = frm.Language
                If Not LocalisedReports.ContainsKey(lang) Then
                    frm.TrailDescription.WeatherData = Me.WeatherData
                    LocalisedReports.Add(lang, frm.TrailDescription)
                    newDescription = Await BuildDescription(frm.TrailDescription)
                End If
                Select Case result
                    Case DialogResult.Retry ' třeba tlačítko "Znovu" nebo "Pokračovat"
                        ' smyčka poběží dál
                    Case Else
                        Exit For
                End Select
            End If
        Next i

        Return newDescription.ToString().Trim()

    End Function


    '' Funkce pro sloučení názvů souborů z dvou GPX záznamů
    Private Function MergeFileNames(record1 As GPXRecord, record2 As GPXRecord) As String

        ' Extrakce jmen 
        Dim name1 As String = Path.GetFileNameWithoutExtension(record1.Reader.FilePath)
        Dim name2 As String = Path.GetFileNameWithoutExtension(record2.Reader.FilePath)
        Dim names1 As New List(Of String)(name1.Split({"_", "."}, StringSplitOptions.RemoveEmptyEntries))
        Dim names2 As New List(Of String)(name2.Split({"_", "."}, StringSplitOptions.RemoveEmptyEntries))

        ' Odstranění čísel
        names1.RemoveAll(Function(s) Regex.IsMatch(s, "^\d+$")) ' Odstraní prvky, které obsahují pouze čísla
        names2.RemoveAll(Function(s) Regex.IsMatch(s, "^\d+$")) ' Odstraní prvky, které obsahují pouze čísla

        'odstraní čísla ze všech prvků
        names1 = names1.Select(Function(s) Regex.Replace(s, "[\d+]", "")).ToList()
        names2 = names2.Select(Function(s) Regex.Replace(s, "[\d+]", "")).ToList()

        'odstraní podtržítka, tečky, pomlčky
        names1 = names1.Select(Function(s) Regex.Replace(s, "[-._]", "")).ToList()
        names2 = names2.Select(Function(s) Regex.Replace(s, "[-._]", "")).ToList()

        '' Odstranění gpx
        'names1.RemoveAll(Function(s) Regex.IsMatch(s, "gpx")) ' 
        'names2.RemoveAll(Function(s) Regex.IsMatch(s, "gpx")) ' 

        names1.RemoveAll(Function(s) Regex.IsMatch(s, "T")) ' 
        names2.RemoveAll(Function(s) Regex.IsMatch(s, "T")) '

        ' Odstranění prázdných prvků
        names1.RemoveAll(Function(s) String.IsNullOrWhiteSpace(s)) ' 
        names2.RemoveAll(Function(s) String.IsNullOrWhiteSpace(s))

        ' Odstranění duplicitních jmen
        Dim commonnames As New List(Of String)(names1.Intersect(names2))
        names1.RemoveAll(Function(s) commonnames.Contains(s))
        'names2.RemoveAll(Function(s) spolecnanames.Contains(s))

        'Sestavení finálních jmen
        Dim finalnames As New List(Of String)
        finalnames.AddRange(names1)
        finalnames.AddRange(names2)

        ' Sestavení nového názvu
        Dim newName As String = $"{String.Join("_", finalnames)}_merged.gpx".Trim

        Return newName
    End Function

    '' Funkce pro sloučení dvou GPX záznamů (např. trasy Runner a psa)
    Public Function MergeDogToMe(dog As GPXRecord) As Boolean

        Dim newName = MergeFileNames(Me, dog)
        'do souboru Me vloží kompletní uzel  <trk> vyjmutý ze souboru dog
        Try
            ' Najdi první uzel <trk>
            Dim dogTrkNodes As XmlNodeList = dog.Reader.SelectNodes("trk")

            ' Předpokládáme, že "dog.Reader" je XmlDocument prvního souboru
            ' a "Me.Reader" je XmlDocument cílového souboru



            For Each dogTrkNode In dogTrkNodes
                Dim importedNode As XmlNode = Me.Reader.ImportNode(dogTrkNode, True) ' Důležité: Import uzlu!
                Dim meGpxNode As XmlNode = Me.Reader.SelectSingleNode("gpx")
                meGpxNode.AppendChild(importedNode) ' Přidání na konec <gpx>
            Next

            'spojené trasy se uloží do souboru kladeče
            'když je nové jméno stejné jako jméno kladeče nepřejmenovává se
            If Me.Reader.FileName = newName OrElse RenameFile(newName) Then
                'Me.Save()
                IO.File.Delete(dog.Reader.FilePath) 'pes se smaže pokud existuje (neměl by)
                RaiseEvent WarningOccurred($"Tracks in files {Me.Reader.FileName} and {dog.Reader.FileName} were successfully merged in file {Me.Reader.FileName} {vbCrLf}File {dog.Reader.FileName}  was deleted.{vbCrLf}", Color.DarkGreen)
            End If


            Return True
        Catch ex As Exception
            RaiseEvent WarningOccurred($"Merging tracks of the me  {Me.Reader.FileName} and the dog {dog.Reader.FileName} failed!" & vbCrLf & ex.Message, Color.Red)
            Return False
        End Try

    End Function

    Public Sub SplitSegmentsIntoTracks()
        'Dim trkNodes As XmlNodeList = Me.Reader.SelectNodes("trk")

        For Each trkNode As XmlNode In Me.TrkNodes
            Dim trkSegNodes As XmlNodeList = Me.Reader.SelectChildNodes(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "trkseg", trkNode)

            If trkSegNodes.Count > 1 Then
                For i As Integer = 1 To trkSegNodes.Count - 1
                    Dim trkSeg As XmlNode = trkSegNodes(i)
                    Dim segDesc As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "desc", trkSeg)
                    Dim newTrk As XmlNode = Me.Reader.CreateElement("trk")
                    If segDesc IsNot Nothing Then
                        trkSeg.RemoveChild(segDesc) 'případná desc přemístí ze segmentu do trk
                        newTrk.AppendChild(segDesc)
                    End If

                    trkNode.RemoveChild(trkSeg)
                    newTrk.AppendChild(trkSeg)

                    trkNode.ParentNode.InsertAfter(newTrk, trkNode)
                    RaiseEvent WarningOccurred($"Track {trkNode.Name} in file {Me.Reader.FileName}, which had two segments, has been split into two tracks.", Color.DarkGreen)
                    Dim newFileName As String = Path.GetFileNameWithoutExtension(Me.Reader.FileName) & "_split" & ".gpx"
                    If RenameFile(newFileName) Then
                    Else
                        RaiseEvent WarningOccurred($"Failed to rename file {Me.Reader.FileName} to {newFileName}.", Color.Red)
                    End If
                    RaiseEvent WarningOccurred($"Track {trkNode.Name} in file {Me.Reader.FileName}, which had two segments, has been split into two tracks.", Color.DarkGreen)
                Next
            End If
        Next


    End Sub

    Public Sub CreateTracks()
        Me.Tracks.Clear() ' Vyčistit seznam tracků
        'Dim trkNodes As XmlNodeList = Me.Reader.SelectNodes("trk")
        Dim parentNode As XmlNode = Me.TrkNodes(0)?.ParentNode
        If parentNode Is Nothing Then Exit Sub

        'projde všechny trkNode:
        For i As Integer = 0 To Me.TrkNodes.Count - 1
            Dim trkNode As XmlNode = Me.TrkNodes(i)
            'najde první <trkseg> v něm první <trkpt> a v něm načte <time>
            Dim trkseg As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "trkseg", trkNode)
            Dim trkpt As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "trkpt", trkseg)
            Dim timeNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "time", trkpt)
            Dim extensionsNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "extensions", trkNode)

            ' Zjisti  typ tracku:
            Dim trkTypeText As String = "Unknown"
            Dim trkTypeEnum As TrackType = TrackType.Unknown
            If extensionsNode IsNot Nothing Then
                trkTypeText = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "TrackType", extensionsNode)?.InnerText
            End If


            If trkTypeText Is Nothing Then

            ElseIf Not [Enum].TryParse(trkTypeText.Trim(), ignoreCase:=True, result:=trkTypeEnum) Then
                trkTypeEnum = TrackType.Unknown ' pokud není typ, nastavíme na Unknown
            End If
            'pokud některý track je uknown musí se znovu zpracovat
            If trkTypeEnum = TrackType.Unknown Then _IsAlreadyProcessed = False

            Dim _TrackAsTrkNode As New TrackAsTrkNode(trkNode, trkTypeEnum)

            Me.Tracks.Add(_TrackAsTrkNode)
        Next

        ' Seřadit podle času
        Me.Tracks.Sort(Function(a, b) Nullable.Compare(a.StartTrackGeoPoint.Time, b.StartTrackGeoPoint.Time))

        ' Odebrat staré <trk>
        For Each trk In TrkNodes
            parentNode.RemoveChild(trk)
        Next

        ' --- Doplnění <type> ---

        Using dlg As New frmCrossTrailSelector(Me.Tracks, Me.FileName)
            'pokud nejsou vybrány typy trků, nebo pokud už nebyl soubor zpracován, tak se ptá
            If Not dlg.ValidateTrailTypes Or (Not Me.IsAlreadyProcessed) Then

                If Me.Tracks.Count = 1 Then
                    ' Zde volání  funkce, která vrátí typ trků

                    dlg.ShowDialog()
                    Dim trk = Me.Tracks(0).TrkNode
                    Dim type = Me.Tracks(0).TrackType
                    AddTypeToTrk(trk, type)

                ElseIf Me.tracks.Count = 2 Then 'když jsou jen dva trky, tak se předpokládá, že první je RunnerTrail a druhý DogTrack
                    AddTypeToTrk(Me.Tracks(0).TrkNode, TrackType.RunnerTrail)
                    Me.Tracks(0).TrackType = TrackType.RunnerTrail ' aktualizace typu
                    AddTypeToTrk(Me.Tracks(1).TrkNode, TrackType.DogTrack)
                    Me.Tracks(1).TrackType = TrackType.DogTrack ' aktualizace typu
                ElseIf Me.tracks.Count > 2 Then
                    Try
                        Me.Tracks(Me.Tracks.Count - 1).TrackType = TrackType.DogTrack ' aktualizace typu (poslední je dog)
                        ' Zde volání nějaké funkce, která vrátí typ trků
                        dlg.ShowDialog()

                        For i As Integer = 0 To Me.Tracks.Count - 1
                            Dim trk = Me.Tracks(i).TrkNode
                            Dim type = Me.Tracks(i).TrackType
                            AddTypeToTrk(trk, type)
                        Next
                    Catch ex As Exception
                        RaiseEvent WarningOccurred($"me.tracks in file {Me.Reader.FileName} were not identified properly.", Color.Red)
                    End Try
                End If
            End If
        End Using

        ' Přidat zpět ve správném pořadí
        For Each t In Me.Tracks
            parentNode.AppendChild(t.TrkNode)
        Next
        CalculateTrackStats(Me.Tracks) 'vypočte statistiky
        RaiseEvent WarningOccurred($"Tracks in file {Me.Reader.FileName} were sorted and typed.", Color.DarkGreen)
    End Sub

    Private Sub CalculateTrackStats(_tracks As List(Of TrackAsTrkNode))
        Dim runnerTrail As TrackAsTrkNode = Nothing
        For Each track As TrackAsTrkNode In _tracks
            If track.TrackType = TrackType.RunnerTrail Then
                runnerTrail = track
            End If
        Next track
        Dim conv As New TrackConverter()
        For Each track As TrackAsTrkNode In _tracks
            If track.TrackType = TrackType.DogTrack Then
                If runnerTrail IsNot Nothing Then
                    track.TrackStats = conv.CalculateTrackStats(track.TrkNode, runnerTrail.TrkNode)
                Else
                    track.TrackStats = conv.CalculateTrackStats(track.TrkNode)
                End If
            Else
                track.TrackStats = conv.CalculateTrackStats(track.TrkNode)
            End If
        Next track

    End Sub



    Public Function AddTypeToTrk(trkNode As XmlNode, _trackType As TrackType) As Boolean
        ' Zkontroluj, jestli už <type> existuje
        Dim trackTypeText As String = _trackType.ToString
        Dim extensionsNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "extensions", trkNode)

        'Dim existingTypes As XmlNodeList = Me.Reader.SelectAllChildNodes(Me.Reader.K9TrailsAnalyzer_PREFIX & "TrackType", extensionsNode)
        If extensionsNode Is Nothing Then
            extensionsNode = Me.Reader.CreateElement("extensions")
            trkNode.PrependChild(extensionsNode)
        End If
        Dim trackTypeNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "TrackType", extensionsNode)
        If trackTypeNode Is Nothing Then
            Me.Reader.CreateAndAddElement(extensionsNode, GpxReader.K9_PREFIX & ":" & "TrackType", _trackType.ToString, False, ,, GpxReader.K9_NAMESPACE_URI)
        Else
            trackTypeNode.InnerText = _trackType.ToString
        End If

        Return True
    End Function

    Private Function GetTrkType(trkNode As XmlNode) As TrackType
        ' Zkontroluj, jestli už <type> existuje
        Dim existingTypes As XmlNodeList = Me.Reader.SelectAllChildNodes(GpxReader.K9_PREFIX & "TrackType", trkNode)
        Dim isTheSameType As Boolean = False
        For Each existingtype As XmlNode In existingTypes
            Dim rawText As String = existingtype.InnerText.Trim()
            Dim parsedType As TrackType

            If [Enum].TryParse(Of TrackType)(rawText, True, parsedType) Then ' True = case-insensitive
                Select Case parsedType
                    Case TrackType.RunnerTrail, TrackType.DogTrack, TrackType.CrossTrail, TrackType.Unknown
                        Return parsedType
                    Case Else
                        ' záznam jiných aplikací nemazat
                End Select
            Else
                ' Pokud GPX obsahuje neznámý typ, nevracíme nic
                Return rawText
            End If

        Next

        'když nic nenajde, vrátí Unknown
        Return Nothing
    End Function

    Private Function AskUserWhichTrackIsWhich(trkList As List(Of TrackAsTrkNode), filename As String) As Boolean
        Using dlg As New frmCrossTrailSelector(trkList, filename)
            If dlg.ShowDialog = DialogResult.OK Then
                Return True
            End If
        End Using
        Return False
    End Function



    Public Sub TrimGPSnoise(minDistance As Integer)
        'clip the start and end of both <trk>, i.e., the Runner and the dog, which was recorded after (or before) the end of the trail. Useful when the GPS doesn't turn off right away.
        ' Získání všech uzlů <trk>
        Dim trackNodes = Me.Reader.SelectNodes("trk")
        For Each trkNode As XmlNode In trackNodes
            ' Získání všech <trkseg> uvnitř <trk>
            Dim trackSegments = Me.Reader.SelectChildNodes(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "trkseg", trkNode)
            For Each trksegNode As XmlNode In trackSegments
                ' Získání všech <trkpt> uvnitř <trkseg>
                Dim trackPoints = Me.Reader.SelectChildNodes(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "trkpt", trksegNode)
                ' Převod XmlNodeList na seznam pro snadnou manipulaci
                Dim points = trackPoints.Cast(Of XmlNode).ToList()
                Dim startCluster = Cluster(points, minDistance)
                ' Odeber body z clusteru
                If startCluster.Count > 5 Then
                    For i = 0 To startCluster.Count - 2 'poslední ponechá, neb je nahrazen centroidem
                        Dim point = startCluster.Item(i)
                        trksegNode.RemoveChild(point)
                        points.Remove(point)
                    Next
                End If

                Dim reversedPoints = points.AsEnumerable().Reverse().ToList()
                Dim endCluster = Cluster(reversedPoints, minDistance)

                ' Odeber body z endCluster
                If endCluster.Count > 5 Then
                    For i = 0 To endCluster.Count - 2 ' poslední ponecháme
                        Dim point = endCluster.Item(i)
                        trksegNode.RemoveChild(point)
                    Next
                End If
            Next trksegNode
        Next trkNode
        'Me.Save()
    End Sub

    Private Function Cluster(points As List(Of XmlNode), minDistance As Double) As List(Of XmlNode)
        Dim cluster_ As New List(Of XmlNode)
        Dim centroidLat, centroidLon As Double

        Dim isCluster As Boolean = True
        Dim conv As New TrackConverter
        For i As Integer = 0 To points.Count - 1
            'Dim lat = Double.Parse(points(i).Attributes("lat").Value, CultureInfo.InvariantCulture)
            'Dim lon = Double.Parse(points(i).Attributes("lon").Value, CultureInfo.InvariantCulture)

            Dim lat As Double = Convert.ToDouble(points(i).Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
            Dim lon As Double = Convert.ToDouble(points(i).Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)


            If cluster_.Count = 0 Then
                ' Inicializace clusteru
                cluster_.Add(points(i))
                centroidLat = lat
                centroidLon = lon

                Continue For
            End If
            ' Výpočet vzdálenosti od centroidu
            Dim currentDistance = conv.HaversineDistance(centroidLat, centroidLon, lat, lon, "m")
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

    'Public Sub SmoothTrail()
    '    'Vyhladí track pomocí Beziér splinů.
    '    ' Získání všech uzlů <trk>
    '    Dim trackNodes = Me.Reader.SelectNodes("trk")
    '    For Each trkNode As XmlNode In trackNodes
    '        ' Získání všech <trkseg> uvnitř <trk>
    '        Dim trackSegments = Me.Reader.SelectChildNodes("trkseg", trkNode)
    '        For Each trksegNode As XmlNode In trackSegments
    '            ' Získání všech <trkpt> uvnitř <trkseg>
    '            Dim trackPoints = Me.Reader.SelectChildNodes("trkpt", trksegNode)
    '            ' Převod XmlNodeList na seznam pro snadnou manipulaci
    '            Dim points = trackPoints.Cast(Of XmlNode).ToList()
    '            Dim smoothedPoints As New List(Of TrackPoint)

    '            ' Načíst body s přesností
    '            For Each trkpt As XmlNode In trackPoints
    '                Dim lat As Double = Double.Parse(trkpt.Attributes("lat").Value, CultureInfo.InvariantCulture)
    '                Dim lon As Double = Double.Parse(trkpt.Attributes("lon").Value, CultureInfo.InvariantCulture)
    '                Dim accuracy As Double = 5.0 ' Výchozí přesnost

    '                Dim accNode As XmlNode = Me.Reader.SelectSingleChildNode("extensions/gpxtpx:TrackPointExtension/opentracks:accuracy_horizontal", trkpt)
    '                If accNode IsNot Nothing Then
    '                    accuracy = Double.Parse(accNode.InnerText, CultureInfo.InvariantCulture)
    '                End If

    '                smoothedPoints.Add(New TrackPoint(lat, lon, accuracy))
    '            Next

    '            ' Použít Bézierovy křivky
    '            Dim bezierTrack As List(Of TrackPoint) = ApplyBezierSmoothing(smoothedPoints)

    '            ' Aktualizace GPX
    '            Dim i As Integer = 0
    '            For Each trkpt As XmlNode In trackPoints
    '                If i <bezierTrack.Count Then
    '                    trkpt.Attributes("lat").Value = bezierTrack(i).Latitude.ToString(CultureInfo.InvariantCulture)
    '                    trkpt.Attributes("lon").Value = bezierTrack(i).Longitude.ToString(CultureInfo.InvariantCulture)
    '                    i += 1
    '                End If
    '            Next



    '        Next trksegNode
    '    Next trkNode



    '    ' Uložit výstupní soubor

    '    Me.Reader.Save("d:\OneDrive - České vysoké učení technické v Praze\Dokumenty\Visual Studio 2022\K9-Trails-Analyzer\GPXTrailCalc\bin\Debug\smmosed.gpx")

    'End Sub


    ' Bézierovy křivky s více body
    'Function ApplyBezierSmoothing(points As List(Of TrackPoint)) As List(Of TrackPoint)
    '    Dim smoothed As New List(Of TrackPoint)

    '    ' Přidáme první bod, aby trasa nezačínala později
    '    smoothed.Add(points(0))

    '    For i As Integer = 1 To points.Count - 2
    '        Dim p0 As TrackPoint = points(i - 1)
    '        Dim p1 As TrackPoint = points(i)
    '        Dim p2 As TrackPoint = points(i + 1)

    '        ' Určíme váhu podle přesnosti (čím vyšší přesnost, tím méně se mění)
    '        Dim weight As Double = 1.0 / Math.Max(1, p1.Accuracy) ' Ochrana proti dělení nulou

    '        ' Vytvoříme řídicí bod pro Bézierovu křivku
    '        Dim controlLat As Double = (p0.Latitude + p2.Latitude) / 2 + (p1.Latitude - (p0.Latitude + p2.Latitude) / 2) * weight
    '        Dim controlLon As Double = (p0.Longitude + p2.Longitude) / 2 + (p1.Longitude - (p0.Longitude + p2.Longitude) / 2) * weight

    '        ' Přidáme původní bod
    '        smoothed.Add(p1)

    '        ' Přidáme více bodů pro lepší interpolaci
    '        Dim steps As Integer = 5 ' Počet bodů mezi uzly pro hladší průběh
    '        For j As Integer = 1 To steps
    '            Dim t As Double = j / (steps + 1)
    '            Dim midLat As Double = (1 - t) * (1 - t) * p1.Latitude + 2 * (1 - t) * t * controlLat + t * t * p2.Latitude
    '            Dim midLon As Double = (1 - t) * (1 - t) * p1.Longitude + 2 * (1 - t) * t * controlLon + t * t * p2.Longitude
    '            smoothed.Add(New TrackPoint(midLat, midLon, p1.Accuracy))
    '        Next
    '    Next

    '    ' Přidáme poslední bod, aby trasa nekončila předčasně
    '    smoothed.Add(points(points.Count - 1))

    '    Return smoothed
    'End Function


    'Public Function Backup() As Boolean
    '    ' Vytvoření kompletní cílové cesty
    '    'Dim backupFilePath As String = Path.Combine(BackupDirectory, Me.Reader.FileName)

    '    If Not IO.File.Exists(backupFilePath) Then
    '        ' Kopírování souboru
    '        Try
    '            IO.File.Copy(Me.Reader.FilePath, backupFilePath, False)
    '            Return True
    '        Catch ex As Exception
    '            ' Zpracování jakýchkoli neočekávaných chyb
    '            Debug.WriteLine($"Chyba při kopírování souboru {Reader.FileName}: {ex.Message}")
    '            Return False
    '        End Try
    '    Else
    '        ' Soubor již existuje, přeskočíme
    '        Debug.WriteLine($"Soubor {Reader.FileName} již existuje, přeskočeno.")
    '        Return False
    '    End If
    'End Function


    Public Sub RenamewptNode(newname As String)
        ' traverses all <wpt> nodes in the GPX file and overwrites the value of <name> nodes to "-předmět":
        ' Find all <wpt> nodes using the namespace

        Dim wptNodes = Me.Reader.SelectNodes("wpt")
        ' Go through each <wpt> node
        For Each wptNode As XmlNode In wptNodes
            ' Najdi uzel <name> uvnitř <wpt> s použitím namespace
            Dim nameNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "name", wptNode)
            If nameNode IsNot Nothing AndAlso nameNode.InnerText <> newname Then
                ' Přepiš hodnotu <name> na newname
                nameNode.InnerText = newname
            End If
        Next wptNode
    End Sub



    Public Sub PrependDateToFilename()

        'Dim fileExtension As String = Path.GetExtension(Reader.FilePath)
        'Dim fileNameOhneExt As String = Path.GetFileNameWithoutExtension(Reader.FilePath)
        Dim newFileName As String = Reader.FileName

        Try
            ' Smaže datum v názvu souboru (to kvůli převodu na iso formát):
            Dim result As (DateTime?, String) = GetRemoveDateFromName()
            Dim modifiedFileName As String = result.Item2
            newFileName = $"{TrailStart.Time:yyyy-MM-dd} {modifiedFileName}"
        Catch ex As Exception
            Debug.WriteLine(ex.ToString())
            'ponechá původní jméno, ale přidá datum
            newFileName = $"{TrailStart:yyyy-MM-dd} {Reader.FileName}"
        End Try

        If Me.Reader.FileName <> newFileName Then RenameFile(newFileName)

    End Sub

    ' Funkce pro přejmenování souboru
    Public Function RenameFile(newFileName As String) As Boolean
        Dim extension As String = Path.GetExtension(Me.Reader.FilePath)
        Dim gpxDirectory As String = Directory.GetParent(Me.Reader.FilePath).ToString
        Dim newFilePath As String = Path.Combine(gpxDirectory, newFileName)


        Try
            'neptá se přejmenuje automaticky
            Dim romanNumeralIndex As Integer = 1
            While IO.File.Exists(newFilePath)
                'Dim nameWithoutExtension As String = Path.GetFileNameWithoutExtension(newFilePath)
                Dim romanNumeral As String = ToRoman(romanNumeralIndex)
                Dim newFileNameWithoutExtension As String = Path.GetFileNameWithoutExtension(newFileName)
                newFileName = $"{newFileNameWithoutExtension}_{romanNumeral}{extension}"
                romanNumeralIndex += 1
                newFilePath = Path.Combine(gpxDirectory, newFileName)
            End While

            'IO.File.Move(Reader.FilePath, newFilePath)

            Debug.WriteLine($"Renamed file to {newFileName}.{Environment.NewLine}")
            RaiseEvent WarningOccurred($"File {Reader.FileName} was renamed to {newFileName}.{Environment.NewLine}", Color.DarkGreen)
            Reader.FilePath = newFilePath

            Return True
        Catch ex As Exception
            Debug.WriteLine(ex.ToString())
            RaiseEvent WarningOccurred($"Error renaming file: {ex.Message}{Environment.NewLine}", Color.Red)

            Return False
        End Try
    End Function

    ' Funkce pro převod čísla na římské číslice (převzato z internetu/Stack Overflow - upraveno)
    Private Function ToRoman(number As Integer) As String
        If number < 1 OrElse number > 3999 Then
            Throw New ArgumentOutOfRangeException("Value must be between 1 and 3999.")
        End If

        Dim values As Integer() = {1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1}
        Dim numerals As String() = {"M", "CM", "D", "CD", "C", "XC", "L", "XL", "x", "ix", "v", "iv", "i"}
        Dim result As New System.Text.StringBuilder()

        For i As Integer = 0 To values.Length - 1
            While number >= values(i)
                number -= values(i)
                result.Append(numerals(i))
            End While
        Next

        Return result.ToString()
    End Function


    Public Function Save() As Boolean
        Me.Reader.Save()
        RaiseEvent WarningOccurred($"File {Me.Reader.FileName} has been saved successfully.", Color.DarkGreen)
        Return True
    End Function

    ' ☀🌦🌧  Počasí
    Private Async Function Wheather() As Task(Of (_temperature As Double, _windSpeed As Double, _windDirection As Double, _precipitation As Double, _relHumidity As Double, _cloudCover As Double))
        Dim client As New HttpClient()
        Dim datum As String = $"{TrailStart.Time:yyyy-MM-dd}"
        Dim url As String = $"https://api.open-meteo.com/v1/forecast?latitude={TrailStart.Location.Lat.ToString(CultureInfo.InvariantCulture)}&longitude={TrailStart.Location.Lon.ToString(CultureInfo.InvariantCulture)}&start_date={datum}&end_date={datum}&hourly=temperature_2m,wind_speed_10m,soil_temperature_0cm,wind_direction_10m,relative_humidity_2m,cloud_cover,precipitation&wind_speed_unit=ms"

        If TrailStart.Time < Today.AddDays(-6) Then
            'po šesti dnech jsou k dispozici historická data z archivu
            url = $"https://archive-api.open-meteo.com/v1/archive?latitude={TrailStart.Location.Lat.ToString(CultureInfo.InvariantCulture)}&longitude={TrailStart.Location.Lon.ToString(CultureInfo.InvariantCulture)}&start_date={datum}&end_date={datum}&hourly=temperature_2m,wind_speed_10m,soil_temperature_0_to_7cm,wind_direction_10m,relative_humidity_2m,cloud_cover,precipitation&wind_speed_unit=ms"
        End If

        Try
            Dim response As HttpResponseMessage = Await client.GetAsync(url)
            If Not response.IsSuccessStatusCode Then
                RaiseEvent WarningOccurred($"Failed to fetch weather data: {response.ReasonPhrase}", Color.Red)
                Return (Nothing, Nothing, Nothing, Nothing, Nothing, Nothing)
            End If
            Dim content As String = Await response.Content.ReadAsStringAsync()
            Dim json As JsonDocument = JsonDocument.Parse(content)


            ' Získej kořenový element
            Dim root = json.RootElement

            ' Najdi pole časů
            Dim times = root.GetProperty("hourly").GetProperty("time")

            Dim localTime As DateTime = Me.TrailStart.Time ' ten načtený čas
            Dim utcTime As DateTime = TrailStart.Time.ToUniversalTime()
            Dim hledanyCasUTC As String = $"{utcTime:yyyy-MM-ddThh:00}"

            ' Teď projdeme všechny časy a najdeme index, kde čas == hledaný čas
            Dim index As Integer = -1
            For i As Integer = 0 To times.GetArrayLength() - 1
                If times(i).GetString() = hledanyCasUTC Then
                    index = i
                    Exit For
                End If
            Next

            If index = -1 Then
                Debug.WriteLine("Čas nenalezen")
            Else
                ' Najdi další pole
                Dim temps = root.GetProperty("hourly").GetProperty("temperature_2m")
                Dim windSpeeds = root.GetProperty("hourly").GetProperty("wind_speed_10m")
                Dim windDirs = root.GetProperty("hourly").GetProperty("wind_direction_10m")
                Dim rains = root.GetProperty("hourly").GetProperty("precipitation")
                Dim relHumidities = root.GetProperty("hourly").GetProperty("relative_humidity_2m")
                Dim cloud_covers = root.GetProperty("hourly").GetProperty("cloud_cover")
                Try


                    ' Vytáhni hodnoty
                    Dim temperature = temps(index).GetDouble()
                    Dim windSpeed = windSpeeds(index).GetDouble()
                    Dim windDir = windDirs(index).GetDouble()
                    Dim precipitation = rains(index).GetDouble
                    Dim relHumidity = relHumidities(index).GetDouble
                    Dim cloude_cover = cloud_covers(index).GetDouble

                    Debug.Write("Pro čas " & hledanyCasUTC & ": ")
                    Debug.Write("Teplota: " & temperature.ToString())
                    Debug.Write(" Oblačnost:  " & cloude_cover.ToString())
                    Debug.Write(" Srážky (mm/h):  " & precipitation.ToString())
                    Debug.Write(" Vítr (m/s): " & windSpeed.ToString())
                    Debug.WriteLine(" Vítr: " & windDirectionToText(windDir))
                    Return (temperature, windSpeed, windDir, precipitation, relHumidity, cloude_cover)
                Catch ex As Exception
                    Return (Nothing, Nothing, Nothing, Nothing, Nothing, Nothing)
                End Try
            End If


        Catch ex As Exception
            RaiseEvent WarningOccurred($"Failed to fetch weather data: {ex.ToString}", Color.Red)
            Return (Nothing, Nothing, Nothing, Nothing, Nothing, Nothing)
        End Try




        Return (Nothing, Nothing, Nothing, Nothing, Nothing, Nothing)
    End Function

    Function WindDirectionToText(direction As Double?) As String
        Dim windDir = {
    "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE",
    "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW"
            }
        If direction IsNot Nothing Then
            Dim index As Integer = CInt((direction + 11.25) \ 22.5) Mod 16
            Return windDir(index)
        Else
            Return ""
        End If
        ' Každý díl má 22.5°

    End Function

    ''' <summary>
    ''' Writes localized reports into the GPX file.
    ''' Adds <K9TrailsAnalyzer:report> elements to the <extensions> section of the GPX file.
    ''' Each report contains localized descriptions of the dog's name, goal, trail, performance, and weather.
    ''' </summary>
    ''' <param name="LocalisedReports"></param>
    ''' <returns></returns>
    Public Function WriteLocalizedReports() As Boolean
        'přidá do <extensions> <K9TrailsAnalyzer:report>
        Dim metadataNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "metadata", Me.Reader.rootNode)
        If metadataNode Is Nothing Then
            metadataNode = Me.Reader.CreateElement("metadata")
            Me.Reader.rootNode.PrependChild(metadataNode)
        End If

        Dim extensionsNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "extensions", metadataNode)
        If extensionsNode Is Nothing Then
            extensionsNode = Me.Reader.CreateElement("extensions")
            metadataNode.PrependChild(extensionsNode)
        End If
        Dim keys As String() = LocalisedReports.Keys.ToArray()
        For Each key In keys
            Dim localizedReport As TrailReport = Me.LocalisedReports(key)
            Dim reportNode As XmlNode = Me.Reader.CreateAndAddElement(extensionsNode, GpxReader.K9_PREFIX & ":" & "report", "", True, "lang", key, GpxReader.K9_NAMESPACE_URI)
            'přidá do <report> <dogName>, <goal>, <trail>, <performance>, <weather>
            Me.Reader.CreateAndAddElement(reportNode, GpxReader.K9_PREFIX & ":" & "dogName", localizedReport.DogName.Text, True,,, GpxReader.K9_NAMESPACE_URI)
            Me.Reader.CreateAndAddElement(reportNode, GpxReader.K9_PREFIX & ":" & "goal", localizedReport.Goal.Text, True,,, GpxReader.K9_NAMESPACE_URI)
            Me.Reader.CreateAndAddElement(reportNode, GpxReader.K9_PREFIX & ":" & "trail", localizedReport.Trail.Text, True,,, GpxReader.K9_NAMESPACE_URI)
            Me.Reader.CreateAndAddElement(reportNode, GpxReader.K9_PREFIX & ":" & "performance", localizedReport.Performance.Text, True,,, GpxReader.K9_NAMESPACE_URI)
            Dim weatherNode As XmlNode = Me.Reader.CreateAndAddElement(reportNode, GpxReader.K9_PREFIX & ":" & "weather", localizedReport.weather.Text, True,,, GpxReader.K9_NAMESPACE_URI)
            Me.WriteWeatherDataToXml(reportNode, weatherNode, localizedReport.WeatherData)
        Next
        Return True
    End Function

    Public Function ReadLocalisedReport() As Dictionary(Of String, TrailReport)
        Dim reports As New Dictionary(Of String, TrailReport)()
        Try
            Dim metadataNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "metadata", Me.Reader.rootNode)
            If metadataNode Is Nothing Then Return reports '< If metadata> vůbec neexistuje
            Dim extensionsNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "extensions", metadataNode)
            If extensionsNode Is Nothing Then Return reports ' <extensions> vůbec neexistuje

            ' Získání všech <K9TrailsAnalyzer:report> uzlů
            Dim reportNodes As XmlNodeList = Me.Reader.SelectAllChildNodes(GpxReader.K9_PREFIX & ":" & "report", extensionsNode)
            If reportNodes Is Nothing OrElse reportNodes.Count = 0 Then Return reports ' žádné reporty
            ' Pro každý report uzel vytvoříme TrailReport objekt
            For Each reportNode As XmlNode In reportNodes
                Dim lang As String = reportNode.Attributes("lang")?.Value
                If String.IsNullOrEmpty(lang) Then Continue For ' pokud není jazyk, přeskočíme
                Dim dogNameNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "dogName", reportNode)
                Dim goalNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "goal", reportNode)
                Dim trailNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "trail", reportNode)
                Dim performanceNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "performance", reportNode)
                Dim weatherNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "weather", reportNode)
                'Dim weatherDataNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "weatherdata", reportNode)
                Dim weatherData As (temperature As Double?,
                                                  windSpeed As Double?,
                                                  windDirection As Double?,
                                                  precipitation As Double?,
                                                  relHumidity As Double?,
                                                  cloudCover As Double?)
                weatherData = ReadWeatherDataFromXml(weatherNode)
                Me.WeatherData = weatherData
                Dim localizedReport As New TrailReport(If(dogNameNode?.InnerText, ""),
                   If(goalNode?.InnerText, ""),
                    If(trailNode?.InnerText, ""),
                    If(performanceNode?.InnerText, ""),
                   weatherData,
                    If(weatherNode?.InnerText, ""))

                If Not reports.ContainsKey(lang) Then reports.Add(lang, localizedReport)
            Next reportNode

            Return reports
        Catch ex As Exception
            Return reports
        End Try

    End Function

    ' Uloží tuple do XML elementu pomocí CreateAndAddElement
    Private Sub WriteWeatherDataToXml(parentNode As XmlElement, weatherNode As XmlNode,
                                  weatherData As (temperature As Double?,
                                                  windSpeed As Double?,
                                                  windDirection As Double?,
                                                  precipitation As Double?,
                                                  relHumidity As Double?,
                                                  cloudCover As Double?))

        '' 1️⃣ Vytvoříme prázdný element <weatherdata> v tvém namespace
        'Dim weatherNode As XmlNode = Me.Reader.CreateAndAddElement(parentNode,
        '                                                GpxReader.K9_PREFIX & ":weatherdata",
        '                                                "",
        '                                                True,
        '                                                "",
        '                                                "",
        '                                                GpxReader.K9_NAMESPACE_URI)

        ' 2️⃣ Funkce pro zápis atributu
        Dim setAttr = Sub(name As String, value As Double?)
                          If value.HasValue Then
                              CType(weatherNode, XmlElement).SetAttribute(name, value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))
                          Else
                              CType(weatherNode, XmlElement).SetAttribute(name, "")
                          End If
                      End Sub

        ' 3️⃣ Nastavení atributů
        setAttr("temperature", weatherData.temperature)
        setAttr("windSpeed", weatherData.windSpeed)
        setAttr("windDirection", weatherData.windDirection)
        setAttr("precipitation", weatherData.precipitation)
        setAttr("relHumidity", weatherData.relHumidity)
        setAttr("cloudCover", weatherData.cloudCover)
    End Sub

    ' Načte tuple z XML elementu
    Private Function ReadWeatherDataFromXml(weatherDataNode As XmlNode) As (Double?, Double?, Double?, Double?, Double?, Double?)
        Dim parseNullable = Function(text As String) As Double?
                                If String.IsNullOrWhiteSpace(text) Then Return Nothing
                                Dim result As Double
                                If Double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, result) Then
                                    Return result
                                Else
                                    Return Nothing
                                End If
                            End Function

        Dim t = parseNullable(weatherDataNode?.Attributes("temperature")?.Value)
        Dim ws = parseNullable(weatherDataNode?.Attributes("windSpeed")?.Value)
        Dim wd = parseNullable(weatherDataNode?.Attributes("windDirection")?.Value)
        Dim p = parseNullable(weatherDataNode?.Attributes("precipitation")?.Value)
        Dim rh = parseNullable(weatherDataNode?.Attributes("relHumidity")?.Value)
        Dim cc = parseNullable(weatherDataNode?.Attributes("cloudCover")?.Value)

        Return (t, ws, wd, p, rh, cc)
    End Function



End Class


Module TrackDisplayLogic
    Public Function ResolveLabel(tt As TrackType) As String
        Select Case tt
            Case TrackType.RunnerTrail : Return My.Resources.Resource1.RunnerTrail
            Case TrackType.DogTrack : Return My.Resources.Resource1.dogTrack
            Case TrackType.CrossTrail : Return My.Resources.Resource1.CrossingTrail
            Case TrackType.Artickle : Return My.Resources.Resource1.artickle
            Case Else : Return My.Resources.Resource1.txtUnknown
        End Select
    End Function
End Module


Public Class GpxReader
    Public xmlDoc As XmlDocument
    Public namespaceManager As XmlNamespaceManager
    Public Property FilePath As String


    Public ReadOnly Property FileName As String
        Get
            Return Path.GetFileName(FilePath)
        End Get
    End Property

    'Public Property Nodes As XmlNodeList
    Public Property rootNode As XmlNode
    Public Property namespaceUri As String

    ' Konstanty pro jmenné prostory
    Public GPX_DEFAULT_PREFIX As String
    Private Const GPX_NAMESPACE_URI As String = "http://www.topografix.com/GPX/1/1" ' Standardní GPX namespace
    Public Const K9_PREFIX As String = "k9"
    Public Const K9_NAMESPACE_URI As String = "https://github.com/mwrnckx/K9-Trails-Analyzer/gpxextenze/1.0"
    Public Const K9Creator As String = "K9TrailsAnalyzer"

    ' Konstruktor načte XML dokument a nastaví XmlNamespaceManager
    ' Konstruktor načte XML dokument, sjednotí namespace na GPX 1.1 a nastaví XmlNamespaceManager
    Public Sub New(_filePath As String)
        FilePath = _filePath
        Try
            ' 1. Načteme celý soubor do textového řetězce
            Dim fileContent As String = System.IO.File.ReadAllText(_filePath)

            ' 2. Provedeme textovou náhradu starého namespace za nový
            ' Tím zajistíme, že všechny soubory budou interně brány jako GPX 1.1
            fileContent = fileContent.Replace("http://www.topografix.com/GPX/1/0", GPX_NAMESPACE_URI)

            ' 3. Načteme upravený textový řetězec do XmlDocument
            xmlDoc = New XmlDocument()
            xmlDoc.LoadXml(fileContent)


            ' ----------------------------------------------------------------------------------
            ' ZBYTEK VAŠEHO KÓDU ZŮSTÁVÁ STEJNÝ
            ' Nyní už bude pracovat se sjednoceným XML, kde je vždy (pokud byl přítomen) GPX 1.1
            ' ----------------------------------------------------------------------------------

            ' Zjištění namespace, pokud je definován
            rootNode = xmlDoc.DocumentElement
            namespaceUri = rootNode.NamespaceURI

            ' ' Přidání deklarace jmenného prostoru pro aplikaci
            AddNamespaceDeclaration(K9_PREFIX, K9_NAMESPACE_URI)
            AddCreator(K9Creator)

            ' Inicializace XmlNamespaceManager s dynamicky zjištěným namespace
            namespaceManager = New XmlNamespaceManager(xmlDoc.NameTable)

            If Not String.IsNullOrEmpty(namespaceUri) Then
                namespaceManager.AddNamespace("gpx", namespaceUri) ' Použijeme lokální prefix "gpx"
                GPX_DEFAULT_PREFIX = "gpx"
                ' Přidání dalších běžných jmenných prostorů pro rozšíření
                namespaceManager.AddNamespace("opentracks", "http://opentracksapp.com/xmlschemas/v1")
                namespaceManager.AddNamespace("gpxtpx", "http://www.garmin.com/xmlschemas/TrackPointExtension/v2")
                namespaceManager.AddNamespace("gpxtrkx", "http://www.garmin.com/xmlschemas/TrackStatsExtension/v1")
xmlns:          namespaceManager.AddNamespace("locus", "https://www.locusmap.app")
                namespaceManager.AddNamespace(K9_PREFIX, K9_NAMESPACE_URI)
            Else
                ' Soubor nemá výchozí namespace
                GPX_DEFAULT_PREFIX = ""
            End If

        Catch ex As FileNotFoundException
            Throw New ArgumentException($"File '{Me.FileName}' has not been found.", ex)
        Catch ex As XmlException
            Throw New XmlException($"Error in XML '{Me.FileName}': {ex.Message}", ex)
        Catch ex As Exception
            Throw New Exception($"Error loading file '{Me.FileName}': {ex.Message}", ex)
        End Try
    End Sub

    ' Předpokládáme, že Me.GpxDocument je váš XmlDocument objekt
    ' a je v něm načten GPX soubor.

    Public Sub AddNamespaceDeclaration(prefix As String, uri As String)
        Dim gpxElement As XmlElement = Me.xmlDoc.DocumentElement

        If gpxElement IsNot Nothing AndAlso gpxElement.Name.ToLower() = "gpx" Then
            ' Přidáme atribut jmenného prostoru k elementu <gpx>
            ' Pokud atribut již existuje, SetAttribute ho aktualizuje.
            gpxElement.SetAttribute("xmlns:" & prefix, uri)
            Debug.WriteLine($"Deklarace jmenného prostoru xmlns:{prefix}='{uri}' přidána/aktualizována na elementu <gpx>.")
        Else
            Debug.WriteLine("Element <gpx> nebyl nalezen nebo je dokument prázdný.")
        End If
    End Sub

    Public Sub AddCreator(creator As String)
        Dim gpxElement As XmlElement = Me.xmlDoc.DocumentElement

        If gpxElement IsNot Nothing AndAlso gpxElement.Name.ToLower() = "gpx" Then
            ' Přidáme atribut jmenného prostoru k elementu <gpx>
            ' Pokud atribut již existuje, SetAttribute ho aktualizuje.
            gpxElement.SetAttribute("creator", creator)
        Else
            Debug.WriteLine("Element <gpx> nebyl nalezen nebo je dokument prázdný.")
        End If
    End Sub

    'Metoda pro získání jednoho uzlu na základě XPath
    Public Function SelectSingleChildNode(prefixedchildname As String, Node As XmlNode) As XmlNode
        If Node IsNot Nothing Then
            Return Node.SelectSingleNode(prefixedchildname, namespaceManager)
        Else Return Nothing
        End If
    End Function

    ' Metoda pro získání seznamu uzlů na základě XPath
    Public Function SelectNodes(nodeName As String) As XmlNodeList
        Dim Nodes = xmlDoc.SelectNodes("//" & GPX_DEFAULT_PREFIX & ":" & nodeName, namespaceManager)
        Return Nodes
    End Function

    ' Metoda pro výběr jednoho uzlu na základě názvu
    Public Function SelectSingleNode(nodename As String) As XmlNode
        Try
            Return xmlDoc.SelectSingleNode("//" & GPX_DEFAULT_PREFIX & ":" & nodename, namespaceManager)
        Catch ex As Exception
            Return Nothing
        End Try

    End Function

    ' Metoda pro výběr poduzlů z uzlu Node
    Public Function SelectChildNodes(XpathchildName As String, node As XmlNode) As XmlNodeList
        Return node.SelectNodes(XpathchildName, namespaceManager)
    End Function

    ' Metoda pro rekurentní výběr všech poduzlů z uzlu Node
    Public Function SelectAllChildNodes(XpathChildName As String, node As XmlNode) As XmlNodeList
        Return node.SelectNodes(".//" & XpathChildName, namespaceManager)
    End Function

    Public Function CreateElement(nodename As String, Optional _namespaceUri As String = Nothing) As XmlNode
        If _namespaceUri IsNot Nothing Then
            ' Pokud je zadán jmenný prostor, použijeme ho
            Return xmlDoc.CreateElement(nodename, _namespaceUri)
        End If
        Return xmlDoc.CreateElement(nodename, xmlDoc.DocumentElement.NamespaceURI)
    End Function

    Public Function CreateAndAddElement(parentNode As XmlElement,
                                XpathchildNodeName As String,
                                value As String,
                                insertAfter As Boolean,
                                Optional attName As String = "",
                                Optional attValue As String = "",
                                Optional namespaceUriToUse As String = GPX_NAMESPACE_URI
                                ) As XmlNode



        Dim childNodes As XmlNodeList = Me.SelectAllChildNodes(XpathchildNodeName, parentNode)

        ' Kontrola duplicity
        For Each node As XmlNode In childNodes
            If (node.Attributes(attName)?.Value = attValue) Then ' zkontroluje zda node s atributem attvalue už neexistuje:
                node.InnerText = value ' nastaví text na nový
                Return node ' nalezen existující uzel, končíme
            End If
        Next

        ' Pokud jsme žádný nenalezli, tak ho přidáme
        Dim insertedNode As XmlNode = Nothing
        Dim childNode As XmlElement = CreateElement(XpathchildNodeName, namespaceUriToUse)
        childNode.InnerText = value
        If attValue <> "" Then childNode.SetAttribute(attName, attValue)
        Debug.WriteLine($"Přidávám nový uzel {XpathchildNodeName} s atributem {attName}={attValue} a textem '{value}'.")

        If childNodes.Count = 0 OrElse insertAfter Then
            insertedNode = parentNode.AppendChild(childNode)
        Else
            insertedNode = parentNode.InsertBefore(childNode, childNodes(0))
        End If

        Return insertedNode
    End Function


    Public Sub DeleteElements(XpathNodelist As XmlNodeList,
                              Optional namespaceUriToUse As String = GPX_NAMESPACE_URI) ' 
        ' Odstraní zadaný uzel z dokumentu
        If XpathNodelist Is Nothing OrElse XpathNodelist.Count = 0 Then
            Debug.WriteLine("No nodes to delete.")
            Return
        End If
        For Each node As XmlNode In XpathNodelist
            If node IsNot Nothing AndAlso node.ParentNode IsNot Nothing Then
                node.ParentNode.RemoveChild(node)
            End If
        Next
    End Sub


    Public Function ImportNode(node As XmlNode, deepClone As Boolean)
        Return xmlDoc.ImportNode(node, deepClone)
    End Function


    Public Sub Save()
        xmlDoc.Save(FilePath)
    End Sub
    Public Sub Save(_FilePath As String)
        xmlDoc.Save(_FilePath)
    End Sub
End Class


Public Class FileTracker

    Private downloadedFiles As Dictionary(Of String, FileRecord)
    Private ReadOnly savePath As String

    Public Sub New(storageFile As String)
        savePath = storageFile
        If File.Exists(savePath) Then
            Try
                Dim json = File.ReadAllText(savePath, Encoding.UTF8)
                downloadedFiles = JsonSerializer.Deserialize(Of Dictionary(Of String, FileRecord))(json)
            Catch ex As Exception 'kdyby byl poškozený soubor
                downloadedFiles = New Dictionary(Of String, FileRecord)()
            End Try

        Else
            downloadedFiles = New Dictionary(Of String, FileRecord)()
        End If
    End Sub

    ' Struktura pro metadata
    Public Class FileRecord
        Public Property LastWriteTime As DateTime
        Public Property Length As Long
    End Class

    ' Vrátí True, pokud je soubor nový nebo změněný
    Public Function IsNewOrChanged(filePath As String) As Boolean
        Dim fi As New FileInfo(filePath)
        Dim fileName = fi.Name

        If Not downloadedFiles.ContainsKey(fileName) Then
            ' Nový soubor
            Return True
        End If

        Dim rec = downloadedFiles(fileName)
        If rec.LastWriteTime <> fi.LastWriteTimeUtc OrElse rec.Length <> fi.Length Then
            ' Soubor změněn
            Return True
        End If

        ' Stejný jako dříve
        Return False
    End Function

    ' Uloží info o souboru jako zpracovaném
    Public Sub MarkAsProcessed(filePath As String)
        Dim fi As New FileInfo(filePath)
        Dim fileName = fi.Name
        Dim rec As New FileRecord With {
            .LastWriteTime = fi.LastWriteTimeUtc,
            .Length = fi.Length
        }

        downloadedFiles(fileName) = rec
        Save()
    End Sub

    ' Uloží JSON
    Private Sub Save()
        Dim options As New JsonSerializerOptions With {.WriteIndented = True,
                 .Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping ' aby se diakritika neescapeovala
        }
        Dim json = JsonSerializer.Serialize(downloadedFiles, options)
        File.WriteAllText(savePath, json, Encoding.UTF8)
    End Sub

End Class

