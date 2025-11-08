Imports System.DirectoryServices.ActiveDirectory
Imports System.Drawing.Text
Imports System.Globalization
Imports System.IO
Imports System.Net.Http
Imports System.Reflection.Emit
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.Encodings.Web
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports System.Windows.Forms.Design
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar
Imports System.Xml
Imports GPXTrailAnalyzer.GPXRecord
Imports GPXTrailAnalyzer.My.Resources
Imports TrackVideoExporter
Imports TrackVideoExporter.TrackConverter



Public Class GpxFileManager
    'obsahuje seznam souborů typu gpxRecord a funkce na jejich vytvoření a zpracování

    Dim _CategoryInfo As CategoryInfo 'informace o psovi, pro kterého se zpracovávají soubory
    ''' <summary>
    ''' Informace o psovi, pro kterého se zpracovávají soubory.
    ''' </summary>
    ''' <returns>Vrací objekt CategoryInfo s informacemi o psovi: Id, Name, RemoteDirectory, ProcessedDirectory atd.</returns>
    Public Property CategoryInfo As CategoryInfo 'informace o psovi, pro kterého se zpracovávají soubory
        Get
            Return _CategoryInfo
        End Get
        Set(value As CategoryInfo)
            _CategoryInfo = value
        End Set
    End Property

    Public Property NumberOfCategories As Integer 'počet psů

    Public dateFrom As Date
    Public dateTo As Date
    Public Property ForceProcess As Boolean = False ' 'pokud je True, zpracuje všechny soubory, i ty, které už byly zpracovány (přepíše popis a další věci)
    Private ReadOnly maxAgeForMerge As TimeSpan
    Private ReadOnly prependDateToName As Boolean
    'Private ReadOnly trimGPS_Noise As Boolean
    Private ReadOnly mergeDecisions As System.Collections.Specialized.StringCollection

    Private mergeNoAsk As Boolean = False 'if yes merge runner and dog trails without asking
    Private mergeCancel As Boolean = False 'don't merge 

    Public Property GpxRecords As New List(Of GPXRecord)
    Public Event WarningOccurred(message As String, _color As Color)

    ''' <summary>
    ''' vrací součin stáří a délky tj. difficulty index, index obtížnosti jako funkci času
    ''' </summary>
    Dim _DiffIndexes As New List(Of (time As DateTime, DiffIndex As Double))
    Public ReadOnly Property DiffIndexes As List(Of (time As DateTime, DiffIndex As Double))
        Get
            If _DiffIndexes.Count > 0 Then Return _DiffIndexes

            For i = 0 To Me.GpxRecords.Count - 1
                Dim diffIndex As Double = Me.GpxRecords(i).TrailStats.DogDistance * Me.GpxRecords(i).TrailStats.TrailAge.TotalHours
                If diffIndex > 0 Then 'když chybí age, nepřidává se
                    Me._DiffIndexes.Add((Me.GpxRecords(i).TrailStart.Time, diffIndex))
                End If

            Next i
            Return _DiffIndexes
        End Get
    End Property

    ''' <summary>
    ''' vrací kumulativní součet indexu obtížnosti
    ''' </summary>
    Dim _TotalDiffIndexes As New List(Of (time As DateTime, DiffIndex As Double))
    Public ReadOnly Property TotalDiffIndexes As List(Of (time As DateTime, DiffIndex As Double))
        Get
            If _TotalDiffIndexes.Count > 0 Then Return _TotalDiffIndexes
            Me._TotalDiffIndexes.Add(Me.DiffIndexes(0))
            For i = 1 To Me.DiffIndexes.Count - 1
                Me._TotalDiffIndexes.Add((Me.DiffIndexes(i).time, Me._TotalDiffIndexes.Last.DiffIndex + Me.DiffIndexes(i).DiffIndex))
            Next i
            Return _TotalDiffIndexes
        End Get
    End Property

    ''' <summary>
    ''' vrací seznam délek jednotlivých stop
    ''' </summary>
    Dim _DistancesKm As New List(Of (time As DateTime, Distance As Double))
    Public ReadOnly Property DistancesKm As List(Of (time As DateTime, DistanceKm As Double))
        Get
            If _DistancesKm.Count > 0 Then Return _DistancesKm

            For i = 0 To Me.GpxRecords.Count - 1
                Dim Distance As Double = Me.GpxRecords(i).TrailDistance
                If Distance > 0 Then 'když chybí, nepřidává se
                    Me._DistancesKm.Add((Me.GpxRecords(i).TrailStart.Time, Distance / 1000))
                End If

            Next i
            Return _DistancesKm
        End Get
    End Property

    ''' <summary>
    ''' vrací kumulativní součet načuchaných vzdáleností
    ''' </summary>
    Dim _TotalDistancesKm As New List(Of (time As DateTime, totalDistanceKm As Double))
    Public ReadOnly Property TotalDistancesKm As List(Of (time As DateTime, totalDistanceKm As Double))
        Get
            If _TotalDistancesKm.Count > 0 Then Return _TotalDistancesKm
            Me._TotalDistancesKm.Add((Me.DistancesKm(0)))
            For i = 1 To Me.DistancesKm.Count - 1
                If Me.DistancesKm(i).DistanceKm > 0 Then
                    Me._TotalDistancesKm.Add((Me.DistancesKm(i).time, Me._TotalDistancesKm.Last.totalDistanceKm + Me.DistancesKm(i).DistanceKm))
                End If
            Next i
            Return _TotalDistancesKm
        End Get
    End Property

    ''' <summary>
    ''' vrací seznam stáří jednotlivých stop v hodinách
    ''' </summary>
    Dim _Ages As New List(Of (time As DateTime, Age As Double))
    Public ReadOnly Property Ages As List(Of (time As DateTime, Age As Double))
        Get
            If _Ages.Count > 0 Then Return _Ages

            For i = 0 To Me.GpxRecords.Count - 1
                Dim Age As Double = Me.GpxRecords(i).TrailStats.TrailAge.TotalHours
                If Age > 0 Then 'když chybí, nepřidává se
                    Me._Ages.Add((Me.GpxRecords(i).TrailStart.Time, Age))
                End If
            Next i
            Return _Ages
        End Get
    End Property

    Dim _Speeds As New List(Of (time As DateTime, Speed As Double))
    Public ReadOnly Property Speeds As List(Of (time As DateTime, Speed As Double))
        Get
            If _Speeds.Count > 0 Then Return _Speeds

            For i = 0 To Me.GpxRecords.Count - 1
                Dim Speed As Double = Me.GpxRecords(i).TrailStats.DogNetSpeed
                If Speed > 0 Then 'když chybí, nepřidává se
                    Me._Speeds.Add((Me.GpxRecords(i).TrailStart.Time, Speed))
                End If
            Next i
            Return _Speeds
        End Get
    End Property

    Dim _Deviations As New List(Of (time As DateTime, Deviation As Double))
    Public ReadOnly Property Deviations As List(Of (time As DateTime, Deviation As Double))
        Get
            If _Deviations.Count > 0 Then Return _Deviations

            For i = 0 To Me.GpxRecords.Count - 1
                Dim Deviation As Double = Me.GpxRecords(i).TrailStats.AverDeviation
                If Deviation > 0 Then 'když chybí, nepřidává se
                    Me._Deviations.Add((Me.GpxRecords(i).TrailStart.Time, Deviation))
                End If

            Next i
            Return _Deviations
        End Get
    End Property





    Public Sub New()
        'gpxRemoteDirectory = My.Settings.Directory
        maxAgeForMerge = New TimeSpan(My.Settings.maxAge, 0, 0)
        prependDateToName = My.Settings.PrependDateToName
        'trimGPS_Noise = My.Settings.TrimGPSnoise
    End Sub

    Public Async Function Main() As Task(Of Boolean)

        Try
            Debug.WriteLine("Starting processing GPX files...")
            Dim allFiles As New List(Of GPXRecord)  ' všechny soubory v pracovním adresáři
            Dim localFiles As List(Of GPXRecord) = GetdAndProcessGPXFiles(False) 'seznam starých souborů, které už byly zpracovány
            allFiles.AddRange(localFiles) 'přidá staré soubory

            Dim remoteFiles As List(Of GPXRecord) = GetdAndProcessGPXFiles(True)
            If remoteFiles.Count > 0 AndAlso Me.NumberOfCategories > 1 Then
                Dim response = mboxQEx($"Found {remoteFiles.Count} new GPX files in remote directory:" & vbCrLf & $"{CategoryInfo.RemoteDirectory}." & vbCrLf & $"Do you really want to import them for the dog {CategoryInfo.Name}?")
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


            ' Zpracování každého GPX souboru
            For Each _gpxRecord As GPXRecord In gpxFilesSortedAndFiltered
                Dim readData = _gpxRecord.ReadSavedDataFromXML(_gpxRecord.LocalisedReports)
                If Not readData.isStatsLoaded Then
                    Dim isTrackStatsCalculated = _gpxRecord.CalculateTrackStats(_gpxRecord.Tracks, _gpxRecord.WptNodes, _gpxRecord.TrailStats)
                    If isTrackStatsCalculated Then
                        _gpxRecord.WriteTrailStatsToXml(_gpxRecord.TrailStats)
                        _gpxRecord.IsSaved = False
                    End If

                End If
                If Not readData.isScoringLoaded Then
                    _gpxRecord.TrailStats.PointsInMTCompetition = _gpxRecord.CalculateCompetitionScore(_gpxRecord.TrailStats)
                    _gpxRecord.IsSaved = False
                End If
                If Not readData.isWeatherLoaded Then
                    _gpxRecord.WeatherData = Await _gpxRecord.Weather()
                    _gpxRecord.IsSaved = False
                End If
                'vytvořit lokalizované reporty a popisy, pokud ještě neexistují 
                If Not readData.isLocalisedReportLoaded OrElse _gpxRecord.LocalisedReports Is Nothing OrElse _gpxRecord.LocalisedReports.Count = 0 Then
                    _gpxRecord.Description = _gpxRecord.BuildSummaryDescription() 'vytvoří popis, pokud není, nebo doplní věk trasy do popisu
                    _gpxRecord.Description = _gpxRecord.BuildLocalisedDescription(_gpxRecord.Description) 'async kvůli počasí!
                    _gpxRecord.BuildLocalisedPerformancePoints()
                    _gpxRecord.WriteDescription() 'zapíše agregovaný popis do tracku Runner
                    _gpxRecord.WriteLocalizedReports() 'zapíše popis do DogTracku
                    _gpxRecord.IsSaved = False

                ElseIf (Not readData.isScoringLoaded Or Not readData.isWeatherLoaded) Then 'pokud jsou načtené lokalizované reporty, ale chybí některé údaje, doplní je
                    If Not readData.isScoringLoaded Then
                        _gpxRecord.BuildLocalisedPerformancePoints() 'todo: výhledově sjednotit s _gpxRecord.strWeather, viz dále
                    End If
                    If Not readData.isWeatherLoaded Then
                        For Each locReport In _gpxRecord.LocalisedReports
                            locReport.Value.WeatherText = _gpxRecord.strWeather()
                            locReport.Value.WeatherData = _gpxRecord.WeatherData
                        Next
                    End If
                    _gpxRecord.WriteLocalizedReports() 'zapíše popis do DogTracku
                    _gpxRecord.IsSaved = False
                End If

                Try
                    If Not _gpxRecord.IsAlreadyProcessed Then 'možno přeskočit, už to proběhlo...
                        _gpxRecord.RenamewptNode(My.Resources.Resource1.article) 'renaming wpt to "article"
                        '_gpxRecord.TrailStats = _gpxRecord.CalculateTrackStats(_gpxRecord.Tracks, _gpxRecord.WptNodes)
                    End If


                    If _gpxRecord.IsSaved = False Then
                        _gpxRecord.Save() 'obsahuje i modified date
                    Else
                        _gpxRecord.SetCreatedModifiedDate()
                    End If


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
            RaiseEvent WarningOccurred("Something went wrong" & vbCrLf & ex.ToString, Color.Red)
            Return False

        End Try

    End Function


    Private Function GetdAndProcessGPXFiles(remoteFiles As Boolean) As List(Of GPXRecord)
        Dim GPXFRecords As New List(Of GPXRecord)

        If remoteFiles Then
            'Dim downloadedPath = Path.Combine(Application.StartupPath, "AppData", "downloaded.json")
            Dim downloadedPath = Path.Combine(Application.StartupPath, CategoryInfo.LocalBaseDirectory, "downloaded.json")
            Dim tracker As New FileTracker(downloadedPath)
            Dim Files As List(Of String) = Directory.GetFiles(CategoryInfo.RemoteDirectory, "*.gpx").ToList()
            Dim i As Integer = 0
            For Each remoteFilePath In Files

                Try
                    If tracker.IsNewOrChanged(remoteFilePath) Then 'new files!!!
                        Debug.WriteLine("Zpracovávám: " & Path.GetFileName(remoteFilePath))
                        ' If the file is new or modified, we process it
                        'copies the file to the local directories
                        Dim localOriginalsFilePath As String = Path.Combine(CategoryInfo.OriginalsDirectory, Path.GetFileName(remoteFilePath))
                        If File.Exists(localOriginalsFilePath) Then
                            RaiseEvent WarningOccurred($"File  {Path.GetFileName(localOriginalsFilePath)} already exists in localOriginals directory!", Color.Red)
                        Else
                            IO.File.Copy(remoteFilePath, localOriginalsFilePath, False)
                        End If
                        i += 1
                        tracker.MarkAsDownloadeded(remoteFilePath)
                        Try
                            'načte z Originals:
                            Dim _reader As New GpxReader(localOriginalsFilePath)
                            Dim _gpxRecord As New GPXRecord(_reader, Me.ForceProcess, CategoryInfo)
                            Dim fileNameWithDate As String = _gpxRecord.PrependDateToFilename(Path.GetFileName(remoteFilePath))
                            Dim localProcessedFilePath As String = Path.Combine(CategoryInfo.ProcessedDirectory, fileNameWithDate)

                            If File.Exists(localProcessedFilePath) Then
                                Dim dialogResult = mboxQEx($"File {fileNameWithDate} already exists in localProcessed directory!" & vbCrLf & "It's probably a duplicate trail. Do you still want to add it?")
                                If dialogResult = DialogResult.No Then
                                    RaiseEvent WarningOccurred($"File {fileNameWithDate} was skipped because it is duplicate", Color.DarkOrange)
                                    Continue For 'přejde na další soubor
                                End If
                            End If

                            _reader.FilePath = localProcessedFilePath

                            _gpxRecord.SplitSegmentsIntoTracks() 'rozdělí trk s více segmenty na jednotlivé trk
                            _gpxRecord.IsAlreadyProcessed = False 'nastaví, že soubor ještě nebyl zpracován
                            '_gpxRecord.CreateTracks() 'seřadí trk podle času
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
            Dim Files As List(Of String) = Directory.GetFiles(CategoryInfo.ProcessedDirectory, "*.gpx").ToList()
            For Each filePath In Files
                Try
                    Dim _reader As New GpxReader(filePath)
                    Dim _gpxRecord As New GPXRecord(_reader, Me.ForceProcess, CategoryInfo)
                    '_gpxRecord.CreateTracks() 'seřadí trk podle času
                    GPXFRecords.Add(_gpxRecord)
                    'Dim test = _gpxRecord.TrailStats.WeightedDistanceAlongTrailPerCent
                Catch ex As Exception
                    'pokud dojde k chybě při čtení souboru, vypíše se varování a pokračuje se na další soubor
                    RaiseEvent WarningOccurred($"Error reading file {Path.GetFileName(filePath)}: {ex.Message}", Color.Red)
                End Try
            Next

        End If
        ' Seřazení podle data -  volá i formulář pro určení typu trasy!!!! Je to důležité pro další zpracování (merge!)
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

    ''' <summary>
    ''' Merges pairs of GPX files where one contains only the runner's track and the other only the dog's track.
    ''' Ensures that a resulting record does not contain both tracks before merging.
    ''' </summary>
    ''' <param name="_gpxRecords">List of GPXRecord objects to be merged.</param>
    ''' <returns>List of merged GPXRecord objects.</returns>
    Public Function MergeGpxFiles(_gpxRecords As List(Of GPXRecord)) As List(Of GPXRecord)

        Dim gpxFilesMerged As New List(Of GPXRecord)
        If _gpxRecords.Count = 0 Then Return gpxFilesMerged

        Dim usedIndexes As New HashSet(Of Integer)
        For i = 0 To _gpxRecords.Count - 1
            If usedIndexes.Contains(i) Then Continue For
            ' Ensure the record does not already contain both tracks (runner and dog)
            Dim iHasOnlyRunner = _gpxRecords(i).Tracks.Any(Function(t) t.TrackType = TrackType.RunnerTrail) _
                AndAlso Not _gpxRecords(i).Tracks.Any(Function(t) t.TrackType = TrackType.DogTrack)
            Dim iHasOnlyDog = _gpxRecords(i).Tracks.Any(Function(t) t.TrackType = TrackType.DogTrack) _
                AndAlso Not _gpxRecords(i).Tracks.Any(Function(t) t.TrackType = TrackType.RunnerTrail)

            If Not (iHasOnlyRunner Or iHasOnlyDog) Then
                ' If the record already contains both tracks, add it as is
                gpxFilesMerged.Add(_gpxRecords(i))
                usedIndexes.Add(i)
                Continue For
            End If

            Dim merged As Boolean = False
            For j = 0 To _gpxRecords.Count - 1
                If i = j OrElse usedIndexes.Contains(j) Then Continue For

                Dim jHasRunner = _gpxRecords(j).Tracks.All(Function(t) t.TrackType = TrackType.RunnerTrail)
                Dim jHasDog = _gpxRecords(j).Tracks.All(Function(t) t.TrackType = TrackType.DogTrack)

                ' One must be runner, the other must be dog
                If (iHasOnlyRunner And jHasDog) Or (iHasOnlyDog And jHasRunner) Then
                    Dim runnerIdx = If(iHasOnlyRunner, i, j)
                    Dim dogIdx = If(iHasOnlyDog, i, j)
                    Dim runner = _gpxRecords(runnerIdx)
                    Dim dog = _gpxRecords(dogIdx)

                    Dim timeDiff = dog.TrailStart.Time - runner.TrailStart.Time
                    ' Merge only if the time difference is positive and within the allowed maxAgeForMerge
                    If timeDiff.TotalSeconds > 0 AndAlso timeDiff <= maxAgeForMerge Then
                        If TryMerge(dog, runner) Then
                            gpxFilesMerged.Add(runner)
                            usedIndexes.Add(runnerIdx)
                            usedIndexes.Add(dogIdx)
                            runner.CreateTracks() 'nutné přepočítat tracky, protože jinak proprties vrací staré hodnoty, kde je jen runnerTrail
                            merged = True
                            Exit For
                        End If
                    End If
                End If
            Next

            ' If not merged, add the record as is
            If Not merged AndAlso Not usedIndexes.Contains(i) Then
                gpxFilesMerged.Add(_gpxRecords(i))
                usedIndexes.Add(i)
            End If
        Next

        Return gpxFilesMerged
    End Function


    Private Function TryMerge(dog As GPXRecord, runner As GPXRecord) As Boolean
        'returns true if file_i was nested in file_prev or if the file was deleted as a duplicate
        'find all adjacent files that differ by less than MaxAge
        ' Basic check to see if the difference in data meets the maxAge condition
        'check for duplicates: if the difference is less than one second, it's probably the same track
        If (dog.TrailStart.Time - runner.TrailStart.Time < New TimeSpan(0, 0, 1)) Then
            Dim question As String = $"Tracks in files 
{dog.Reader.FileName} 
and 
{runner.Reader.FileName} 
have same start time. 
I suspect it's a duplication. 

Should we delete the {dog.FileName} file?

Be carefull with this!!!!!"
            Dim result As DialogResult = MessageBox.Show(question, "Delete duplicate file?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
            If result = DialogResult.Yes Then
                IO.File.Delete(dog.Reader.FilePath)
                RaiseEvent WarningOccurred($"File {dog.Reader.FileName} was deleted because it is duplicate", Color.DarkOrange)
                Return True
                'ElseIf result = DialogResult.No Then
                '    SaveMergeDecision(file_prev, file_i, result.ToString)
            End If
        End If

        ' Zeptej se uživatele, zda chce soubory spojit
        Dim mergeFiles As DialogResult
        If Not mergeNoAsk Then mergeFiles = DialogMergeFiles(runner, dog)
        ' Pokud uživatel souhlasí, spoj soubory, jinak přidej
        If mergeNoAsk OrElse (mergeFiles = DialogResult.Yes) Then
            If runner.MergeDogToMe(dog) Then
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


        mergeNoAsk = rbNoAsk.Checked ' Uložení stavu pro použití v hlavní funkci
        mergeCancel = rbCancel.Checked
        Return result
    End Function


End Class


Public Class GPXRecord

    Public Event WarningOccurred(_message As String, _color As Color)
    ''' <summary>
    ''' track list in gpxfile
    ''' </summary>
    Private _tracks As List(Of TrackAsTrkNode)
    Public ReadOnly Property Tracks As List(Of TrackAsTrkNode)
        Get
            If _tracks IsNot Nothing Then Return _tracks
            _tracks = Me.CreateTracks()
            Return _tracks 'vrací seznam všech tracků, které jsou v souboru
        End Get
    End Property

    'Public Property DogName As String
    Public Property ActiveCategoryInfo As CategoryInfo

    Dim _wptNodes As TrackAsTrkPts
    Public ReadOnly Property WptNodes As TrackAsTrkPts
        Get
            If _wptNodes IsNot Nothing Then Return _wptNodes
            If Me.Reader Is Nothing Then
                Throw New InvalidOperationException("Reader nebyl nastaven.")
            End If
            Dim wptNodeList As XmlNodeList = Me.Reader.SelectNodes("wpt") 'když není žádný, vrátí prázdný list
            _wptNodes = New TrackAsTrkPts(TrackType.article, wptNodeList)
            Return _wptNodes 'vrací seznam všech wpt, které jsou v souboru 
        End Get
    End Property

    Private _runnerStart As TrackGeoPoint
    Public ReadOnly Property RunnerStart As TrackGeoPoint
        Get
            If _runnerStart IsNot Nothing Then Return _runnerStart 'cache
            _runnerStart = Me.Tracks.FirstOrDefault(Function(t) t.TrackType = TrackType.RunnerTrail)?.StartTrackGeoPoint
            Return _runnerStart
        End Get
    End Property

    Private _dogStart As TrackGeoPoint
    Public ReadOnly Property DogStart As TrackGeoPoint
        Get
            If _dogStart IsNot Nothing Then Return _dogStart 'cache
            _dogStart = Me.Tracks.FirstOrDefault(Function(t) t.TrackType = TrackType.DogTrack)?.StartTrackGeoPoint
            Return _dogStart
        End Get
    End Property

    Private _dogFinish As TrackGeoPoint
    Public ReadOnly Property DogFinish As TrackGeoPoint
        Get
            If _dogFinish IsNot Nothing Then Return _dogFinish 'cache
            _dogFinish = Me.Tracks.LastOrDefault(Function(t) t.TrackType = TrackType.DogTrack)?.EndTrackGeoPoint
            Return _dogFinish
        End Get
    End Property

    Dim _TrailStats As New TrailStats
    Public Property TrailStats As TrailStats
        Get
            ' Vraci _TrailStats, pokud je inicializovany.
            ' Pokud neni inicializovany, vrati novy, prazdny objekt.
            If _TrailStats Is Nothing Then
                _TrailStats = New TrailStats()
            End If
            Return _TrailStats
        End Get
        Set(value As TrailStats)
            _TrailStats = value
        End Set
    End Property


    ''' <summary>
    ''' returns WeightedDistanceAlongTrail
    ''' </summary>
    Dim _trailDistance As Double = 0
    Public ReadOnly Property TrailDistance As Double
        Get
            If _trailDistance > 0 Then 'cache
                Return _trailDistance
            End If
            If Me.Tracks.Count = 0 Then Return 0F
            If Me.TrailStats.WeightedDistanceAlongTrail > 0 Then
                _trailDistance = Me.TrailStats.WeightedDistanceAlongTrail
                Return _trailDistance
            ElseIf Me.TrailStats.DogDistance > 0 Then
                _trailDistance = Me.TrailStats.DogDistance
                Return _trailDistance
            ElseIf Me.TrailStats.RunnerDistance > 0 Then
                _trailDistance = Me.TrailStats.RunnerDistance
                Return _trailDistance
            End If
            Return 0F 'pokud nenajde žádný Track, vrátí 0
        End Get
    End Property



    'todo: přidat logiku - buď start kladeče, pokud není start psa
    Public ReadOnly Property TrailStart As TrackGeoPoint
        Get
            If Me.Tracks?.Count = 0 Then
                Return Nothing
            Else 'vrací start prvního tracku
                Return Me.Tracks(0)?.StartTrackGeoPoint
            End If
        End Get
    End Property



    ''' <returns>Returns file name with extension</returns>
    Public ReadOnly Property FileName As String
        Get
            Return Me.Reader.FileName
        End Get
    End Property

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

    Public Property IsSaved As Boolean = True


    Public LocalisedReports As New Dictionary(Of String, TrailReport)

    Public Property WeatherData As WeatherData

    Private Const NBSP As String = ChrW(160)

    Public ReadOnly Property TrkNodes As XmlNodeList
        Get
            If Me.Reader Is Nothing Then
                Throw New InvalidOperationException("Reader nebyl nastaven.")
            End If
            Return Me.Reader.SelectNodes("trk")
        End Get
    End Property


    Public Sub New(_reader As GpxReader, forceProcess As Boolean, activeCategoryInfo As CategoryInfo)
        'gpxDirectory = 
        Me.Reader = _reader
        'Me.DogName = activeCategoryInfo.Name
        Me.ActiveCategoryInfo = activeCategoryInfo
        If forceProcess Then
            _IsAlreadyProcessed = False
        Else
            _IsAlreadyProcessed = IsProcessed()
        End If
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
            Return TimeSpan.Zero
        End If
        Try
            If Me.DogStart.Time <> Date.MinValue AndAlso Me.RunnerStart.Time <> Date.MinValue Then
                ageFromTime = Me.DogStart.Time - Me.RunnerStart.Time
            End If
        Catch ex As Exception
            Return TimeSpan.Zero
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
        Return TimeSpan.Zero
    End Function

    Private Function GetWeightedAge(window As Integer) As Single
        Dim lengthFromComments As Single = 0.0F
        If Not String.IsNullOrWhiteSpace(Me.Description) Then lengthFromComments = GetLengthFromComments(Me.Description)
        Return lengthFromComments
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


    ''' <summary>
    ''' Extracts a date from the file name and removes it from the name.
    ''' </summary>
    ''' <returns>Returns a tuple: (found date, modified file name without the date).
        ' If no date is found, returns (Nothing, original file name).</returns>
    Public Function GetRemoveDateFromName(fileName As String) As (fileDate As DateTime, fileNameWithoutDate As String)
        Dim extension = Path.GetExtension(fileName)
        fileName = Path.GetFileNameWithoutExtension(fileName) 'odstraní příponu

        Dim Separator As String = "\s*(?:\.|_|-|,|\._)\s*" ' Dash, underscore, dot, comma, or ._
        Dim isoSeparator As String = "\s*(?:[-/_]|\.)\s*"   ' Multiple separators for ISO format

        Dim pattern As String =
$"(?<time>(?:0?[0-9]|1[0-9]|2[0-3])[-:\.](?:[0-5][0-9])(?:[-:\.](?:[0-5][0-9]))?)" &
"|" &
$"(?<eu>(?<day>[0-2]\d|3[01]){Separator}(?<month>0[1-9]|1[0-2])\s*(?:\.|_|-|,|\._)\s*(?<year>\d{{4}}))" &
"|" &
$"(?<us>(?<month>0[1-9]|1[0-2]){Separator}(?<day>[0-2]\d|3[01]){Separator}(?<year>\d{{4}}))" &
"|" &
$"(?<iso>(?<year>\d{{4}}){isoSeparator}(?<month>0[1-9]|1[0-2]){isoSeparator}(?<day>[0-2]\d|3[01]))" &
"|" &
$"(?<eu2>(\d+){Separator}(\d+){Separator}(\d+))"

        Dim matches = Regex.Matches(fileName, pattern)
        Dim foundDate As String = Nothing
        Dim foundTime As String = Nothing

        For Each m As Match In matches
            If m.Groups("time").Success Then
                foundTime = m.Groups("time").Value
            ElseIf m.Groups("eu").Success OrElse m.Groups("us").Success OrElse m.Groups("iso").Success OrElse m.Groups("eu2").Success Then
                foundDate = m.Value
            End If
        Next

        ' Odstraníme datum a čas z názvu souboru
        Dim cleanedName As String = fileName
        If Not String.IsNullOrEmpty(foundDate) Then cleanedName = cleanedName.Replace(foundDate, "")
        If Not String.IsNullOrEmpty(foundTime) Then cleanedName = cleanedName.Replace(foundTime, "")

        ' Vyčistíme případné zbytky oddělovačů, např. "__" nebo "--", "..","  "
        cleanedName = Regex.Replace(cleanedName, "[-_. ]{2,}", "_")
        'mezery nahradí podtržítkem
        cleanedName = Regex.Replace(cleanedName, "[ ]", "_")
        cleanedName = cleanedName.Trim("_"c, "-"c, " "c, "."c)
        Debug.WriteLine($"Datum: {foundDate}")
        Debug.WriteLine($"Čas:   {foundTime}")
        Debug.WriteLine($"Bez datumu a času: {cleanedName}")







        '' Regex patterns for different date formats with named groups
        'Dim datePattern1 As String =
        '    $"(?<eu>(?<day>[0-2]\d|3[01]){Separator}(?<month>0[1-9]|1[0-2]){Separator}(?<year>\d{{4}}))|" &
        '    $"(?<us>(?<month>0[1-9]|1[0-2]){Separator}(?<day>[0-2]\d|3[01]){Separator}(?<year>\d{{4}}))|" &
        '    $"(?<iso>(?<year>\d{{4}}){isoSeparator}(?<month>0[1-9]|1[0-2]){isoSeparator}(?<day>[0-2]\d|3[01]))"

        'Dim datePattern2 As String = $"(?<eu2>(\d+){Separator}(\d+){Separator}(\d+))" ' Fallback for any three numbers separated
        '' Regex for time in various formats (not used for date extraction, but for removal)
        'Dim timeRegex As String = "(?:0?[0-9]|1[0-9]|2[0-3])[-:\.](?:[0-5][0-9])(?:[-:\.](?:[0-5][0-9]))?"

        '' Combine all regex patterns
        'Dim myRegex As New Regex(datePattern1 & "|" & datePattern2 & "|" & timeRegex)

        'Dim match As Match = myRegex.Match(fileName)
        'If match.Success Then
        Try
            '        ' Try to extract year, month, day from named groups
            '        Dim _year, _month, _day As Integer
            '        'If match.Groups("year") IsNot Nothing Then
            '        '    _year = Integer.Parse(match.Groups("year").Value)
            '        'End If
            '        If False = match.Groups("year").Success AndAlso match.Groups("eu2").Success Then
            '            _year = Integer.Parse(match.Groups("year").Value)
            '            _month = Integer.Parse(match.Groups("month").Value)
            '            _day = Integer.Parse(match.Groups("day").Value)
            '        End If


            Dim dateTimeFromFileName As New DateTime
            '' Determine the date format based on matched group and current culture
            'If (match.Groups("eu").Success Or match.Groups("eu2").Success) And CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.StartsWith("d") Then
            '    ' European format: day-month-year
            '    dateTimeFromFileName = New DateTime(_year, _month, _day)
            'ElseIf match.Groups("us").Success And CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.StartsWith("M") Then
            '    ' US format: month-day-year
            '    dateTimeFromFileName = New DateTime(_year, _month, _day)
            'ElseIf match.Groups("iso").Success Then
            '    ' ISO format: year-month-day
            '    dateTimeFromFileName = New DateTime(_year, _month, _day)
            'Else
            '    ' Fallback: year-day-month (may be incorrect if parsing fails)
            '    dateTimeFromFileName = New DateTime(_year, _day, _month)
            'End If

            ' Remove the found date/time string from the file name
            Dim fileNameWithoutDate As String = cleanedName 'myRegex.Replace(fileName, "").Trim()

            ' --- Převod na DateTime ---
            Dim parsedDate As Date? = Nothing
            Dim parsedTime As TimeSpan? = Nothing

            ' 1️⃣ Zkusíme převést datum (různé možné formáty)
            Dim possibleDateFormats As String() = {
            "yyyy-MM-dd", "dd-MM-yyyy", "MM-dd-yyyy",
            "yyyy.MM.dd", "dd.MM.yyyy", "MM.dd.yyyy",
            "yyyy_MM_dd", "dd_MM_yyyy", "MM_dd_yyyy",
            "yyyy/MM/dd", "dd/MM/yyyy", "MM/dd/yyyy"
        }

            If Not String.IsNullOrEmpty(foundDate) Then
                For Each fmt In possibleDateFormats
                    Dim tmp As Date
                    If Date.TryParseExact(foundDate, fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, tmp) Then
                        parsedDate = tmp
                        Exit For
                    End If
                Next

                ' fallback pro tříčíselný "eu2" formát (např. 025-10-06)
                If Not parsedDate.HasValue Then
                    Dim parts = Regex.Split(foundDate, "[^0-9]+")
                    If parts.Length = 3 Then
                        Dim y, m, d As Integer
                        If parts(0).Length = 4 Then
                            ' ISO (2025-10-06)
                            y = CInt(parts(0)) : m = CInt(parts(1)) : d = CInt(parts(2))
                        Else
                            ' zkusíme interpretovat první číslo jako rok (např. 025 = 2025)
                            y = If(parts(0).Length = 3, 2000 + CInt(parts(0)), CInt(parts(2)))
                            m = CInt(parts(1))
                            d = CInt(parts(2))
                        End If
                        Try
                            parsedDate = New Date(y, m, d)
                        Catch
                            ' pokud nesedí (např. 31.11.)
                        End Try
                    End If
                End If
            End If

            ' 2️⃣ Zkusíme převést čas
            If Not String.IsNullOrEmpty(foundTime) Then
                Dim t As TimeSpan
                Dim normalizedTime = foundTime.Replace("-", ":").Replace(".", ":")
                If TimeSpan.TryParse(normalizedTime, t) Then
                    parsedTime = t
                End If
            End If

            ' 3️⃣ Poskládáme DateTime
            If parsedDate.HasValue Then
                Dim result As DateTime = parsedDate.Value
                If parsedTime.HasValue Then result = result.Add(parsedTime.Value)
                Debug.WriteLine($"Spojené DateTime: {result}")
                dateTimeFromFileName = result
            Else
                Debug.WriteLine("Nepodařilo se sestavit platný DateTime.")
            End If




            ' Remove unwanted characters from the start and end
            'Dim charsToTrim As Char() = {"_", "-", ".", " "}
            'fileNameWithoutDate = fileNameWithoutDate.Replace(".gpx", "")
            'fileNameWithoutDate = fileNameWithoutDate.TrimStart(charsToTrim).TrimEnd(charsToTrim)
            fileNameWithoutDate = fileNameWithoutDate & extension ' Add .gpx extension back

            ' Return the found date and the modified file name
            Return (dateTimeFromFileName, fileNameWithoutDate)
        Catch ex As Exception
            Debug.WriteLine($"{fileName} - Error in date format")
            Return (Nothing, fileName)
        End Try
        'Else
        '    Debug.WriteLine($"{fileName} - Date not found")
        '    Return (Nothing, fileName)
        'End If
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
                trailPart = trailPart.Replace("⌛:", "") ' odstranění 🕒:
                trailPart = "⌛" & NBSP & ageFromTime.TotalHours.ToString("F1") & NBSP & "h, " & trailPart
            End If
            Dim LengthfromComments As Single = GetLengthFromComments(trailPart)
            If LengthfromComments = 0 Then
                ' Odebereme případnou starou délku z trailPart (např. "1.2 km něco")
                trailPart = Regex.Replace(trailPart, "^[0-9\.,]+\s*(km|m)(?=\W|$)", "", RegexOptions.IgnoreCase).Trim()
                trailPart = trailPart.Replace(My.Resources.Resource1.outLength.ToLower & ":", "") ' odstranění vícenásobných mezer
                trailPart = trailPart.Replace("📏:", "") '
                trailPart = (Me.TrailDistance / 1000.0).ToString("F1") & NBSP & "km, " & trailPart
            End If

        Else
            If Me.TrailDistance > 0 Then
                trailPart = (Me.TrailDistance / 1000.0).ToString("F1") & NBSP & "km"
            End If
            If ageFromTime.TotalHours > 0 Then
                trailPart &= "  ⌛" & NBSP & ageFromTime.TotalHours.ToString("F1") & NBSP & "h"
            End If

        End If

        Return New TrailReport("", Me.ActiveCategoryInfo.Name, goalPart, trailPart, dogPart, "", Nothing)
    End Function


    Private Function BuildDescription(_trailReport As TrailReport) As String
        Dim goalPart As String = _trailReport.Goal.Text
        Dim trailPart As String = _trailReport.Trail.Text
        Dim performancePart As String = _trailReport.Performance.Text
        Dim crlf As String = "<br>"


        _trailReport.WeatherText = strWeather()


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
        Next


        Return SummaryDescription.ToString().Trim()


    End Function

    Public Function BuildLocalisedDescription(summaryDescription As String) As String
        Dim lang As String
        Dim newDescription As String = ""
        If Me.LocalisedReports Is Nothing OrElse Me.LocalisedReports.Count = 0 Then
            Dim _localisedReport = ExtractDescriptionParts(summaryDescription)
            _localisedReport.WeatherData = Me.WeatherData

            lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant()
            Me.LocalisedReports.Add(lang, _localisedReport)
            newDescription = BuildDescription(_localisedReport)
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

                frm = New frmEditComments With {.Category = Me.ActiveCategoryInfo.Name,
                                                .TrailDescription = report,
                                                .GpxFileName = Me.Reader.FileName,
                                                .Language = lang
                                              }

                result = frm.ShowDialog()
                newDescription = BuildDescription(frm.TrailDescription)
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
                report = New TrailReport("", firstLocalisedReport.Value.Category.Text, firstLocalisedReport.Value.Goal.Text,
                                        firstLocalisedReport.Value.Trail.Text,
                                        firstLocalisedReport.Value.Performance.Text,
                                        "",
                                         Nothing)
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
                    newDescription = BuildDescription(frm.TrailDescription)
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

    Public Sub BuildLocalisedPerformancePoints()
        For Each kvp As KeyValuePair(Of String, TrailReport) In Me.LocalisedReports
            Dim txtTotal = Localizer.GetString("Total", kvp.Key)
            Dim txtpoints = Localizer.GetString("points", kvp.Key) 'todo dodělat lokalizaci
            Dim txtof = Localizer.GetString("ofPoints", kvp.Key)
            Dim txtFinalFind = Localizer.GetString("FinalFind", kvp.Key)
            Dim txtSpeed = Localizer.GetString("Speed", kvp.Key)
            Dim txtAccuracy = Localizer.GetString("Accuracy", kvp.Key)
            Dim txtDogReading = Localizer.GetString("DogReading", kvp.Key)
            Dim pointsTotal As Integer = Me.TrailStats.PointsInMTCompetition.RunnerFoundPoints + Me.TrailStats.PointsInMTCompetition.DogSpeedPoints + Me.TrailStats.PointsInMTCompetition.DogAccuracyPoints + Me.TrailStats.PointsInMTCompetition.DogReadingPoints
            Dim performancePoints As String = $"🏆{txtTotal}: {pointsTotal} {txtpoints}{vbCrLf}
        ❤{txtFinalFind}: {Me.TrailStats.PointsInMTCompetition.RunnerFoundPoints} {txtpoints} ({txtof} {Me.ActiveCategoryInfo.PointsForFindMax}),{vbCrLf}
        🚀{txtSpeed}: {Me.TrailStats.PointsInMTCompetition.DogSpeedPoints} {txtpoints} ({Me.ActiveCategoryInfo.PointsPerKmhGrossSpeed} per km/h),{vbCrLf}
        🎯{txtAccuracy}: {Me.TrailStats.PointsInMTCompetition.DogAccuracyPoints} {txtpoints} ({txtof} {Me.ActiveCategoryInfo.PointsForAccuracyMax}),{vbCrLf}
        👁{txtDogReading}: {Me.TrailStats.PointsInMTCompetition.DogReadingPoints} {txtpoints} ({txtof} {Me.ActiveCategoryInfo.PointsForDogReadingMax})."

            kvp.Value.PerformancePointsText = performancePoints

        Next
    End Sub

    Public Function strWeather() As String
        ' 🌧🌦☀ Počasí
        If WeatherData Is Nothing Then 'pokud se nenačetla data o počasí
            Return ""
        End If
        Return $"🌡{CDbl(WeatherData.temperature).ToString("0.#")}{NBSP}°C,  🌬{CDbl(WeatherData.windSpeed).ToString("0.#")}{NBSP}m/s {WindDirectionToText(WeatherData.windDirection)}, 💧{WeatherData.relHumidity}{NBSP}%,   ☔​{WeatherData.precipitation}{NBSP}mm/h, 🌥{WeatherData.cloudCover}{NBSP}%"

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
        newName = PrependDateToFilename(newName)
        'do souboru Me vloží kompletní uzel  <trk> vyjmutý ze souboru dog
        Try
            ' Najdi všechny trk v přidávaném souboru
            Dim dogTrkNodes As XmlNodeList = dog.Reader.SelectNodes("trk")

            ' Předpokládáme, že "dog.Reader" je XmlDocument prvního souboru
            ' a "Me.Reader" je XmlDocument cílového souboru



            For Each dogTrkNode In dogTrkNodes
                Dim importedNode As XmlNode = Me.Reader.ImportNode(dogTrkNode, True) ' Důležité: Import uzlu!
                Dim meGpxNode As XmlNode = Me.Reader.SelectSingleNode("gpx") 'vybere root (gpx)
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

    Public Function CreateTracks() As List(Of TrackAsTrkNode)
        'Dim trkNodes As XmlNodeList = Me.Reader.SelectNodes("trk")
        Dim parentNode As XmlNode = Me.TrkNodes(0)?.ParentNode
        If parentNode Is Nothing Then Return New List(Of TrackAsTrkNode)
        Me._tracks = New List(Of TrackAsTrkNode)
        Dim types(Me.TrkNodes.Count - 1) As TrackType
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
                'unknown
            ElseIf Not [Enum].TryParse(trkTypeText.Trim(), ignoreCase:=True, result:=trkTypeEnum) Then
                trkTypeEnum = TrackType.Unknown ' pokud není typ, nastavíme na Unknown
            End If
            'pokud některý track je uknown musí se znovu zpracovat
            'If trkTypeEnum = TrackType.Unknown Then _IsAlreadyProcessed = False
            types(i) = trkTypeEnum
            Dim _TrackAsTrkNode As New TrackAsTrkNode(trkNode, trkTypeEnum)

            Me._tracks.Add(_TrackAsTrkNode)
        Next

        ' Seřadit podle času
        Me._tracks.Sort(Function(a, b) Nullable.Compare(a.StartTrackGeoPoint.Time, b.StartTrackGeoPoint.Time))

        ' Odebrat staré <trk>
        For Each trk In TrkNodes
            parentNode.RemoveChild(trk)
        Next

        ' --- Doplnění <type> ---

        ' Kód v místě, kde rozhoduješ, jestli máš form ukázat:
        If ValidateTypes(types, False) And Me.IsAlreadyProcessed Then
            ' Všechny typy jsou platné a soubor již byl zpracován, nemusíš ukazovat formulář
            ' pokračuj dál — formulář není potřeba
        Else
            ' musíme nechat uživatele domluvit typy -> otevři formulář
            _IsAlreadyProcessed = False 'nutné kvůli tomu aby se soubor uložil
            Dim suggestedTypes = ComputeSuggestedTypes(_tracks)

            ' Pokud projde validací navržených typů, použij je a formulář neukazuj:
            If ValidateTypes(suggestedTypes, False) And suggestedTypes.Count = 2 Then
                ' Aplikuj navržené typy do modelu (trkList) a pokračuj bez formuláře
                For i = 0 To Me._tracks.Count - 1
                    Me._tracks(i).TrackType = suggestedTypes(i)
                    Dim trk = Me._tracks(i).TrkNode
                    Dim type = Me._tracks(i).TrackType
                    AddTypeToTrk(trk, type)
                Next

                ' pokračuj dál — formulář není potřeba
            Else
                ' musíme nechat uživatele domluvit typy -> otevři formulář
                Using f As New frmCrossTrailSelector(_tracks, FileName)
                    f.ShowDialog()
                    ' předpoklad: formulář změnil trkList při potvrzení
                    For i As Integer = 0 To Me._tracks.Count - 1
                        Dim trk = Me._tracks(i).TrkNode
                        Dim type = Me._tracks(i).TrackType
                        AddTypeToTrk(trk, type)
                    Next

                End Using
            End If

        End If



        ' Přidat zpět ve správném pořadí
        For Each track In Me._tracks
            parentNode.AppendChild(track.TrkNode)
        Next
        Return Me._tracks
        RaiseEvent WarningOccurred($"Tracks in file {Me.Reader.FileName} were sorted and typed.", Color.DarkGreen)
    End Function

    ' Vrátí pole TrackType, které jsou "navržené" podle pravidel (pouze pro Unknown měníme)
    Public Function ComputeSuggestedTypes(trkList As List(Of TrackAsTrkNode)) As TrackType()
        Dim n = trkList.Count
        Dim result(n - 1) As TrackType

        ' nejprve zkopíruj původní stavy
        For i = 0 To n - 1
            result(i) = trkList(i).TrackType
        Next

        ' pravidla, která používáš v Load:
        Select Case n
            Case 1
                If result(0) = TrackType.Unknown Then result(0) = TrackType.DogTrack
            Case 2
                If result(0) = TrackType.Unknown Then result(0) = TrackType.RunnerTrail
                If result(1) = TrackType.Unknown Then result(1) = TrackType.DogTrack
            Case Else
                If n > 0 AndAlso result(n - 1) = TrackType.Unknown Then result(n - 1) = TrackType.DogTrack
        End Select

        Return result
    End Function

    ' Validuje pole typů (možno bez UI). Pokud showMessages = True, použije mboxEx pro chybová hlášení.
    Public Function ValidateTypes(types() As TrackType, Optional showMessages As Boolean = True) As Boolean
        Dim runnerCount As Integer = 0
        Dim dogCount As Integer = 0

        For i = 0 To types.Length - 1
            Dim t = types(i)
            If t = TrackType.Unknown Then
                If showMessages Then mboxEx("For each track you have to choose its type!")
                Return False
            ElseIf t = TrackType.RunnerTrail Then
                runnerCount += 1
            ElseIf t = TrackType.DogTrack Then
                dogCount += 1
            End If
        Next

        If runnerCount > 1 Then
            If showMessages Then mboxEx("There can be only one Runner trail.")
            Return False
        End If
        If dogCount > 1 Then
            If showMessages Then mboxEx("There can be only one dog track.")
            Return False
        End If

        Return True
    End Function




    ' Enum pro reprezentaci stavu (pohyb / stání).
    Private Enum MovementState
        Moving
        Stopped
    End Enum

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="_tracks"></param>
    ''' <param name="checkPoints"></param>
    ''' <returns>TrackStatsStructure</returns>
    Public Function CalculateTrackStats(_tracks As List(Of TrackAsTrkNode), checkPoints As TrackAsTrkPts, ByRef stats As TrailStats) As Boolean
        Dim runnerTrkNode As XmlNode = Nothing
        Dim dogTrkNode As XmlNode = Nothing
        For Each track As TrackAsTrkNode In _tracks
            If track.TrackType = TrackType.RunnerTrail Then
                runnerTrkNode = track.TrkNode
            ElseIf track.TrackType = TrackType.DogTrack Then
                dogTrkNode = track.TrkNode
            End If
        Next track

        ' --- KROK 1: Příprava všech dat ---
        Dim preparedData = PrepareTrackData(dogTrkNode, runnerTrkNode)
        If preparedData.DogGeoPoints Is Nothing OrElse preparedData.DogGeoPoints.Count < 2 Then
            With stats
                .RunnerDistance = preparedData.RunnerTotalDistance
                .TrailAge = GetAge()
            End With
            Return False ' Není co analyzovat
        End If

        ' --- KROK 2: Analýza pohybu a sledování stopy ---
        Dim movementAnalysis = AnalyzeDogMovement(preparedData.DogGeoPoints, preparedData.dogXY, preparedData.RunnerXY, preparedData.RunnerGeoPoints)
        Dim checkPointsEvals As List(Of CheckpointData) '(distanceAlongTrail As Double, deviationFromTrail As Double, dogGrossSpeed As Double)) = Nothing
        Dim maxDistance As Double = 0.0
        ' --- KROK 3: Vyhodnocení checkpointů ---
        'GrossSpeed: The biggest scoring benefit is a checkpoint where the dog is as far away as possible while staying as close to the route as possible.
        Dim _dogGrossSpeed As Double = 0.0
        If runnerTrkNode IsNot Nothing Then

            checkPointsEvals = EvaluateCheckPoints(checkPoints, preparedData.DogGeoPoints, preparedData.RunnerGeoPoints, preparedData.RunnerXY, preparedData.Lat0, preparedData.Lon0, preparedData.RunnerTotalDistance)
            Dim max_index As Integer = -1

            Dim maxWeight As Double = 0


            For i = 0 To checkPointsEvals.Count - 1
                Dim cp = checkPointsEvals(i)
                'weight is the distance of the checkPoint from the path of the runner x the relative effective length along the path 
                Dim _weight = Weight(cp.deviationFromTrail) * (cp.distanceAlongTrail / (preparedData.RunnerTotalDistance))
                If _weight > maxWeight Then
                    maxWeight = _weight
                    maxDistance = cp.distanceAlongTrail
                    max_index = i
                    _dogGrossSpeed = cp.dogGrossSpeedkmh
                End If
            Next
        End If


        ' --- KROK 4: Sestavení předběžných statistik a výpočet finálního skóre ---
        With stats
            .DogDistance = preparedData.dogTotalDistance
            .RunnerDistance = preparedData.RunnerTotalDistance
            .WeightedDistanceAlongTrailPerCent = movementAnalysis.weightedDistance / preparedData.RunnerTotalDistance * 100.0 'jako procento z délky kladečovy stopy
            .WeightedDistanceAlongTrail = movementAnalysis.weightedDistance
            .WeightedTimePerCent = movementAnalysis.weightedTime / (movementAnalysis.movingTime + movementAnalysis.stoppedTime) * 100 'jako procento z celkového času
            .TrailAge = GetAge()
            .DogMovingTime = movementAnalysis.movingTime
            .DogStoppedTime = movementAnalysis.stoppedTime
            .DogTotalTime = movementAnalysis.movingTime + movementAnalysis.stoppedTime
            .DogGrossSpeed = _dogGrossSpeed 'The biggest scoring benefit is a checkpoint where the dog is as far away as possible while staying as close to the route as possible.
            .AverDeviation = If(movementAnalysis.movingTime.TotalSeconds > 0, movementAnalysis.averageDeviation, -1.0)
            .MaxDeviationGeoPoints = movementAnalysis.maxDeviationGeoPoints
            .CheckpointsEval = checkPointsEvals
            .RunnerFound = If(Weight(movementAnalysis.MinDistanceToRunnerEnd) >= 0.75, True, False) ' up to distance of 15 meters full weight (gps accuracy?) after that zero
            .MaxTeamDistance = maxDistance
        End With

        If movementAnalysis.movingTime.TotalHours > 0 Then
            stats.DogNetSpeed = (preparedData.dogTotalDistance / 1000.0) / movementAnalysis.movingTime.TotalHours
        End If

        Return True

    End Function

    Private Function PrepareTrackData(dogTrkNode As XmlNode, runnerTrkNode As XmlNode) _
    As (DogGeoPoints As List(Of TrackGeoPoint), dogXY As List(Of (X As Double, Y As Double)), RunnerGeoPoints As List(Of TrackGeoPoint), RunnerXY As List(Of (X As Double, Y As Double)), Lat0 As Double, Lon0 As Double, RunnerTotalDistance As Double, dogTotalDistance As Double)

        ' all the code  applies:
        ' 1. Convert XmlNode to List(Of TrackGeoPoint) for both the dog and the runner.
        ' 2. Selecting a reference point (lat0, lon0).
        ' 3. Converting the runner's track from Lat/Lon to XY.
        ' 4. Calculate the total length of the track of the runner (totalRunnerDistanceKm).
        Dim conv As New TrackConverter()
        Dim dogTrkAsGeoPoints As TrackAsGeoPoints
        Dim dogGeoPoints As List(Of TrackGeoPoint) = Nothing
        If dogTrkNode IsNot Nothing Then
            dogTrkAsGeoPoints = conv.ConvertTrackTrkPtsToGeoPoints(conv.ConvertTrackAsTrkNodeToTrkPts(New TrackAsTrkNode(dogTrkNode, trackType:=TrackType.DogTrack)))
            dogGeoPoints = dogTrkAsGeoPoints.TrackGeoPoints 'trasa psa/psovoda
        End If
        Dim runnerTrkAsGeoPoints As TrackAsGeoPoints
        Dim runnerGeoPoints As List(Of TrackGeoPoint) = Nothing
        ' Transfer the runner to XY
        Dim runnerXY As New List(Of (X As Double, Y As Double))
        Dim dogXY As New List(Of (X As Double, Y As Double))
        Dim lat0 As Double
        Dim lon0 As Double

        Dim runnerTotalDistance As Double = 0.0F 'celková dráha kladeče (aka runner) počítána ze záznamu jeho trasy (runnerGeoPoints)
        Dim dogTotalDistance As Double = 0.0F 'celková dráha psa počítána ze záznamu jeho trasy (dogGeoPoints)

        If runnerTrkNode IsNot Nothing Then
            runnerTrkAsGeoPoints = conv.ConvertTrackTrkPtsToGeoPoints(conv.ConvertTrackAsTrkNodeToTrkPts(New TrackAsTrkNode(runnerTrkNode, trackType:=TrackType.Unknown)))
            runnerGeoPoints = runnerTrkAsGeoPoints.TrackGeoPoints
            If runnerGeoPoints.Count > 0 Then
                ' We will convert the runner to XY
                lat0 = runnerGeoPoints(0).Location.Lat
                lon0 = runnerGeoPoints(0).Location.Lon
                For i As Integer = 0 To runnerGeoPoints.Count - 2
                    Dim point1 As TrackGeoPoint = runnerGeoPoints(i) 'první bod segmentu trasy psa/psovoda
                    Dim point2 As TrackGeoPoint = runnerGeoPoints(i + 1) 'druhý bod segmentu trasy psa/psovoda

                    Dim lat As Double = point1.Location.Lat
                    Dim lon As Double = point1.Location.Lon
                    Dim X, Y As Double
                    conv.LatLonToXY(lat, lon, lat0, lon0, X, Y)
                    runnerXY.Add((X, Y))
                    Dim distance As Double = conv.HaversineDistance(point1.Location.Lat, point1.Location.Lon, point2.Location.Lat, point2.Location.Lon, "m")
                    runnerTotalDistance += distance  'tohle je situace, když čas chybí, nutno započítat!
                Next
                'add the last point of the runner (final runners position):
                Dim lastLat As Double = runnerGeoPoints.Last.Location.Lat
                Dim lastLon As Double = runnerGeoPoints.Last.Location.Lon
                Dim lastX, lastY As Double
                conv.LatLonToXY(lastLat, lastLon, lat0, lon0, lastX, lastY)
                runnerXY.Add((lastX, lastY))
            End If
        ElseIf dogGeoPoints IsNot Nothing AndAlso dogGeoPoints.Count > 0 Then
            ' Backup solution if we don't have a runner's track, we will use the dog's first point as a reference
            lat0 = dogGeoPoints(0).Location.Lat
            lon0 = dogGeoPoints(0).Location.Lon
        Else
            ' A case where even the dog has no points, this is where the feature should probably end
            Return (Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing)
        End If

        If dogGeoPoints IsNot Nothing AndAlso dogGeoPoints.Count > 0 Then
            ' We will convert the dog to XY
            For i As Integer = 0 To dogGeoPoints.Count - 2
                Dim point1 As TrackGeoPoint = dogGeoPoints(i) 'první bod segmentu trasy psa/psovoda
                Dim point2 As TrackGeoPoint = dogGeoPoints(i + 1) 'druhý bod segmentu trasy psa/psovoda

                Dim lat As Double = point1.Location.Lat
                Dim lon As Double = point1.Location.Lon
                Dim X, Y As Double
                conv.LatLonToXY(lat, lon, lat0, lon0, X, Y)
                dogXY.Add((X, Y))
                Dim distance As Double = conv.HaversineDistance(point1.Location.Lat, point1.Location.Lon, point2.Location.Lat, point2.Location.Lon, "m")
                dogTotalDistance += distance 'tohle je situace, když čas chybí, nutno započítat!
            Next
            'add the last point of the dog (final dogs position):
            Dim lastLat As Double = dogGeoPoints.Last.Location.Lat
            Dim lastLon As Double = dogGeoPoints.Last.Location.Lon
            Dim lastX, lastY As Double
            conv.LatLonToXY(lastLat, lastLon, lat0, lon0, lastX, lastY)
            dogXY.Add((lastX, lastY))
        End If

        ' The function returns all prepared data structures at once
        Return (dogGeoPoints, dogXY, runnerGeoPoints, runnerXY, lat0, lon0, runnerTotalDistance, dogTotalDistance)
    End Function


    '''<summary>
    ''' Analyzes the dog's track, distinguishes between moving and standing, and compares it with the trajectory of the runner.
    '''</summary>
    ''' <param name="dogGeoPoints">Lists GPS points of the dog's track.</param>
    ''' <param name="runnerXY">List of XY coordinates of the runner's track.</param>
    ''' <param name="lat0">Latitude reference for conversion.</param>
    ''' <param name="lon0">Longitude reference for conversion.</param>
    ''' <returns>
    ''' Tuple containing key motion analysis metrics:
    ''' - movingTime: Total time the dog was in the 'Moving' state (speed > 1.0 km/h or above 0.36 km/h if already moving).
    ''' - stoppedTime: Total time the dog was in the 'Stopped' state (speed < 0.36 km/h or below 1.0 km/h if already stopped).
    ''' - weightedDistance: The distance along the runner's (paver's) track that the dog covered. This distance is counted *only* when the dog is moving and is weighted by its proximity (deviation) to the track.
    ''' - weightedTime: Total time the dog spent in close proximity to the runner's track (regardless of whether the dog was moving or stopped). This time is weighted by the proximity (deviation), reflecting the overall fidelity to the track.
    ''' - weightedDeviation: The sum of (deviation from runner's track * segment time) in [m*s]. A measure of the dog's average deviation weighted by time, indicating tracing accuracy during movement.
    ''' - MinDistanceToRunnerEnd: The minimum distance (in meters) the dog came to the very last point of the runner's track (the goal/end point).
    ''' </returns>
    Private Function AnalyzeDogMovement(dogGeoPoints As List(Of TrackGeoPoint), dogXY As List(Of (X As Double, Y As Double)), runnerXY As List(Of (X As Double, Y As Double)), runnerGeoPoints As List(Of TrackGeoPoint)) _
As (movingTime As TimeSpan, stoppedTime As TimeSpan, weightedDistance As Double, weightedTime As TimeSpan, averageDeviation As Double, maxDeviationGeoPoints As TrackAsGeoPoints, MinDistanceToRunnerEnd As Double)
        Dim conv As New TrackConverter()
        ' --- Setting constants ---
        Const MOVING_SPEED_THRESHOLD_MS As Double = 0.277 ' 1.0 km/h
        Const STOPPED_SPEED_THRESHOLD_MS As Double = 0.1 ' 0.36 km/h
        Const MAX_SEGMENT_JUMP As Integer = 5 ' Maximum allowed "jump" of segments on the paver route

        ' --- Initialization of accumulation variables ---
        Dim movingTime As TimeSpan = TimeSpan.Zero
        Dim stoppedTime As TimeSpan = TimeSpan.Zero
        Dim weightedTime As TimeSpan = TimeSpan.Zero ' Původní init
        Dim totalTime As TimeSpan = TimeSpan.Zero
        Dim weightedDistance As Double = 0.0
        Dim averageDeviation As Double  ' average by time 
        Dim maxDeviation As Double
        Dim maxDeviationGeoPoints As TrackAsGeoPoints
        Dim maxDevDogIndex, maxDevRunnerIndex As Integer
        Dim deviationXTime As Double = 0.0 '[metr krát sekunda]
        Dim minDeviationFromRunnerEnd As Double = Double.MaxValue

        ' --- Initialization of state variables for the loop ---
        Dim currentState As MovementState = MovementState.Stopped ' Default state
        Dim lastCreditedRunnerSegmentIndex As Integer = -1
        Dim currentRunnerSearchIndex As Integer = 0 ' Search optimization
        ' Get the end point of the cluster path to calculate the minimum distance
        Dim runnerEndPoint As (X As Double, Y As Double)? = If(runnerXY.Count > 0, runnerXY.Last(), Nothing)
        Dim nearistDogPointIndex As Integer
        Dim nearistDogPointXY As (X As Double, Y As Double)

        ' --- Main loop for analyzing dog track segments ---
        For i As Integer = 0 To dogGeoPoints.Count - 2
            Dim point1 As TrackGeoPoint = dogGeoPoints(i)
            Dim point2 As TrackGeoPoint = dogGeoPoints(i + 1)

            ' Transfer the current point of the dog to XY
            Dim timeDiff As TimeSpan = point2.Time - point1.Time
            ' Skip invalid segments (no time or negative time)
            If timeDiff.TotalSeconds <= 0 Then Continue For
            Dim distance As Double = Math.Sqrt((dogXY(i).X - dogXY(i + 1).X) ^ 2 + (dogXY(i).Y - dogXY(i + 1).Y) ^ 2) 'conv.HaversineDistance(point1.Location.Lat, point1.Location.Lon, point2.Location.Lat, point2.Location.Lon, "m")
            Dim speedMs As Double = distance / timeDiff.TotalSeconds

            ' --- 1. State machine logic (Hysteresis) ---
            If i = 0 Then
                ' For the first segment, determine the initial state
                currentState = If(speedMs > MOVING_SPEED_THRESHOLD_MS, MovementState.Moving, MovementState.Stopped)
            Else
                ' For other segments, we change the state only when the CURRENT boundary is crossed
                If currentState = MovementState.Moving AndAlso speedMs < STOPPED_SPEED_THRESHOLD_MS Then
                    currentState = MovementState.Stopped
                ElseIf currentState = MovementState.Stopped AndAlso speedMs > MOVING_SPEED_THRESHOLD_MS Then
                    currentState = MovementState.Moving
                End If
            End If

            ' *** Proměnné pro váženou analýzu deklarujeme mimo stavový blok, aby byly dostupné ***
            Dim deviation As Double = 0.0
            Dim weight As Double = 0.0
            Dim projection As (ClosestSegmentIndex As Integer, Deviation As Double) = Nothing

            ' *** Tracking analysis (provádí se PRO VŠECHNY SEGMENTY) ***
            If runnerXY.Count > 1 Then

                ' Find the nearest point on the path of the runner (přesunuto z bloku Moving)
                projection = FindClosestProjectionOnTrack(dogXY(i + 1), runnerXY, currentRunnerSearchIndex)
                deviation = projection.Deviation
                If maxDeviation < deviation Then
                    maxDeviation = deviation
                    maxDevDogIndex = i + 1
                    maxDevRunnerIndex = projection.ClosestSegmentIndex + 1
                End If
                currentRunnerSearchIndex = projection.ClosestSegmentIndex ' Update the start for the next search

                ' Calculate the weight by deviation (přesunuto z bloku Moving)
                weight = GPXRecord.Weight(deviation)

                ' *** NOVÉ: Započtení váženého času (provedeno BEZ OHLEDU na stav pohybu) ***
                If weight > 0 Then
                    ' timeDiff.TotalSeconds * _weight dá čas v sekundách, převedeme na TimeSpan
                    weightedTime = weightedTime.Add(timeDiff * weight)
                End If
                ' Add the time-weighted deviation (přesunuto sem z Tracking analysis bloku)
                deviationXTime += deviation * timeDiff.TotalSeconds

            End If

            ' --- 3. Accumulation of statistics by state (Původní blok) ---
            If currentState = MovementState.Moving Then
                movingTime = movingTime.Add(timeDiff)

                ' --- 4. Weighted Metrics (provádí se POUZE když se pes pohybuje a máme trasu kladeče) ---
                If runnerXY.Count > 1 Then

                    ' Add the weighted distance along the path of the runner (původní kód)
                    If weight > 0 AndAlso projection.ClosestSegmentIndex > lastCreditedRunnerSegmentIndex Then
                        Dim segmentJump = projection.ClosestSegmentIndex - lastCreditedRunnerSegmentIndex

                        ' Case 1: The dog follows the track smoothly or shortens slightly
                        If segmentJump <= MAX_SEGMENT_JUMP Then
                            For k As Integer = lastCreditedRunnerSegmentIndex + 1 To projection.ClosestSegmentIndex
                                Dim p1 = runnerXY(k)
                                Dim p2 = runnerXY(k + 1)
                                Dim runnerSegmentLength As Double = Math.Sqrt((p1.X - p2.X) ^ 2 + (p1.Y - p2.Y) ^ 2)
                                weightedDistance += runnerSegmentLength * weight
                            Next
                            ' Case 2: The dog skipped a large segment, we only count the segment where it got "caught" again
                        Else
                            Dim p1 = runnerXY(projection.ClosestSegmentIndex)
                            Dim p2 = runnerXY(projection.ClosestSegmentIndex + 1)
                            Dim runnerSegmentLength As Double = Math.Sqrt((p1.X - p2.X) ^ 2 + (p1.Y - p2.Y) ^ 2)
                            weightedDistance += runnerSegmentLength * weight
                        End If

                        lastCreditedRunnerSegmentIndex = projection.ClosestSegmentIndex
                    End If
                End If
            Else ' currentState = MovementState.Stopped
                stoppedTime = stoppedTime.Add(timeDiff)
            End If

        Next i 'cyklus přes segmenty trasy psa
        If runnerXY.Count > 1 Then
            averageDeviation = deviationXTime / (movingTime + stoppedTime).TotalSeconds
            Dim maxDeviationPointList As New List(Of TrackGeoPoint)
            maxDeviationPointList.Add(dogGeoPoints(maxDevDogIndex)) 'Nejprve bod psa pak runner, neměnit pořadí!
            maxDeviationPointList.Add(runnerGeoPoints(maxDevRunnerIndex))
            maxDeviationGeoPoints = New TrackAsGeoPoints(TrackType.Unknown, maxDeviationPointList)
        End If
        ' Update the minimum distance to the end of the runner's path
        Dim index As Integer = 0
        If runnerEndPoint IsNot Nothing Then
            For Each dogPointXY In dogXY
                Dim distToEnd As Double = Math.Sqrt((dogPointXY.X - runnerEndPoint.Value.X) ^ 2 + (dogPointXY.Y - runnerEndPoint.Value.Y) ^ 2)
                If distToEnd < minDeviationFromRunnerEnd Then
                    minDeviationFromRunnerEnd = distToEnd
                    nearistDogPointIndex = index
                    nearistDogPointXY = dogPointXY
                End If
                index += 1
            Next dogPointXY
        End If



        ' --- Returning results ---
        Return (movingTime, stoppedTime, weightedDistance, weightedTime, averageDeviation, maxDeviationGeoPoints, minDeviationFromRunnerEnd)

    End Function

    '''<summary>
    ''' Analyzes the dog's track, distinguishes between moving and standing, and compares it with the trajectory of the runner.
    '''</summary>
    ''' <param name="dogGeoPoints">Lists GPS points of the dog's track.</param>
    ''' <param name="runnerXY">List of XY coordinates of the runner's track.</param>
    ''' <param name="lat0">Latitude reference for conversion.</param>
    ''' <param name="lon0">Longitude reference for conversion.</param>
    ''' <returns>Tuple containing key motion analysis metrics.</returns>
    Private Function AnalyzeDogMovement_old(dogGeoPoints As List(Of TrackGeoPoint), dogXY As List(Of (X As Double, Y As Double)), runnerXY As List(Of (X As Double, Y As Double)), lat0 As Double, lon0 As Double) _
    As (movingTime As TimeSpan, stoppedTime As TimeSpan, weightedDistance As Double, weightedTime As TimeSpan, weightedDeviation As Double, MinDistanceToRunnerEnd As Double)
        Dim conv As New TrackConverter()
        ' --- Setting constants ---
        Const MOVING_SPEED_THRESHOLD_MS As Double = 0.277 ' 1.0 km/h
        Const STOPPED_SPEED_THRESHOLD_MS As Double = 0.1 ' 0.36 km/h
        Const MAX_SEGMENT_JUMP As Integer = 5 ' Maximum allowed "jump" of segments on the paver route

        ' --- Initialization of accumulation variables ---
        Dim movingTime As TimeSpan = TimeSpan.Zero
        Dim stoppedTime As TimeSpan = TimeSpan.Zero
        Dim weightedTime As TimeSpan = TimeSpan.Zero
        Dim weightedDistance As Double = 0.0
        Dim weightedDeviation As Double = 0.0 ' Sum (deviation * segment time) -> [m*s]
        Dim minDeviationFromRunnerEnd As Double = Double.MaxValue

        ' --- Initialization of state variables for the loop ---
        Dim currentState As MovementState = MovementState.Stopped ' Default state
        Dim lastCreditedRunnerSegmentIndex As Integer = -1
        Dim currentRunnerSearchIndex As Integer = 0 ' Search optimization
        ' Get the end point of the cluster path to calculate the minimum distance
        Dim runnerEndPoint As (X As Double, Y As Double)? = If(runnerXY.Count > 0, runnerXY.Last(), Nothing)
        Dim nearistDogPointIndex As Integer
        Dim nearistDogPointXY As (X As Double, Y As Double)
        ' --- Main loop for analyzing dog track segments ---
        For i As Integer = 0 To dogGeoPoints.Count - 2
            Dim point1 As TrackGeoPoint = dogGeoPoints(i)
            Dim point2 As TrackGeoPoint = dogGeoPoints(i + 1)


            ' Transfer the current point of the dog to XY

            Dim timeDiff As TimeSpan = point2.Time - point1.Time
            ' Skip invalid segments (no time or negative time)
            If timeDiff.TotalSeconds <= 0 Then Continue For
            Dim distanceMeters As Double = Math.Sqrt((dogXY(i).X - dogXY(i + 1).X) ^ 2 + (dogXY(i).Y - dogXY(i + 1).Y) ^ 2) 'conv.HaversineDistance(point1.Location.Lat, point1.Location.Lon, point2.Location.Lat, point2.Location.Lon, "m")
            Dim speedMs As Double = distanceMeters / timeDiff.TotalSeconds

            ' --- 1. State machine logic (Hysteresis) ---
            If i = 0 Then
                ' For the first segment, determine the initial state
                currentState = If(speedMs > MOVING_SPEED_THRESHOLD_MS, MovementState.Moving, MovementState.Stopped)
            Else
                ' For other segments, we change the state only when the CURRENT boundary is crossed
                If currentState = MovementState.Moving AndAlso speedMs < STOPPED_SPEED_THRESHOLD_MS Then
                    currentState = MovementState.Stopped
                ElseIf currentState = MovementState.Stopped AndAlso speedMs > MOVING_SPEED_THRESHOLD_MS Then
                    currentState = MovementState.Moving
                End If
            End If

            ' --- 2. Accumulation of statistics by state ---
            If currentState = MovementState.Moving Then
                movingTime = movingTime.Add(timeDiff)

                ' --- 3. Tracking analysis (only when the dog is moving and we have the runner's route) ---
                If runnerXY.Count > 1 Then

                    ' Find the nearest point on the path of the runner (call to the helper function)
                    Dim projection = FindClosestProjectionOnTrack(dogXY(i + 1), runnerXY, currentRunnerSearchIndex)
                    Dim deviation As Double = projection.DeviationMeters
                    currentRunnerSearchIndex = projection.ClosestSegmentIndex ' Update the start for the next search

                    ' Add the time-weighted deviation
                    weightedDeviation += deviation * timeDiff.TotalSeconds


                    ' Calculate the weight by deviation (nonlinear decrease)
                    Dim _weight As Double = Weight(deviation)


                    ' Add the weighted distance along the path of the runner
                    If _weight > 0 AndAlso projection.ClosestSegmentIndex > lastCreditedRunnerSegmentIndex Then
                        Dim segmentJump = projection.ClosestSegmentIndex - lastCreditedRunnerSegmentIndex

                        ' Case 1: The dog follows the track smoothly or shortens slightly
                        If segmentJump <= MAX_SEGMENT_JUMP Then
                            For k As Integer = lastCreditedRunnerSegmentIndex + 1 To projection.ClosestSegmentIndex
                                Dim p1 = runnerXY(k)
                                Dim p2 = runnerXY(k + 1)
                                Dim runnerSegmentLength As Double = Math.Sqrt((p1.X - p2.X) ^ 2 + (p1.Y - p2.Y) ^ 2)
                                weightedDistance += runnerSegmentLength * _weight
                            Next
                            ' Case 2: The dog skipped a large segment, we only count the segment where it got "caught" again
                        Else
                            Dim p1 = runnerXY(projection.ClosestSegmentIndex)
                            Dim p2 = runnerXY(projection.ClosestSegmentIndex + 1)
                            Dim runnerSegmentLength As Double = Math.Sqrt((p1.X - p2.X) ^ 2 + (p1.Y - p2.Y) ^ 2)
                            weightedDistance += runnerSegmentLength * _weight
                        End If

                        lastCreditedRunnerSegmentIndex = projection.ClosestSegmentIndex
                    End If


                End If
            Else ' currentState = MovementState.Stopped
                stoppedTime = stoppedTime.Add(timeDiff)
            End If

        Next i

        ' Update the minimum distance to the end of the runner's path
        Dim index As Integer = 0
        If runnerEndPoint IsNot Nothing Then
            For Each dogPointXY In dogXY
                Dim distToEnd As Double = Math.Sqrt((dogPointXY.X - runnerEndPoint.Value.X) ^ 2 + (dogPointXY.Y - runnerEndPoint.Value.Y) ^ 2)
                If distToEnd < minDeviationFromRunnerEnd Then
                    minDeviationFromRunnerEnd = distToEnd
                    nearistDogPointIndex = index
                    nearistDogPointXY = dogPointXY
                End If
                index += 1
            Next dogPointXY
        End If


        ' --- Returning results ---
        Return (movingTime, stoppedTime, weightedDistance, weightedTime, weightedDeviation, minDeviationFromRunnerEnd)

    End Function


    ''' <param name="dogPointXY"></param>
    ''' <param name="runnerTrackXY"></param>
    ''' <param name="searchStartIndex"></param>
    ''' <returns></returns>
    Private Function FindClosestProjectionOnTrack(dogPointXY As (X As Double, Y As Double), runnerTrackXY As List(Of (X As Double, Y As Double)), searchStartIndex As Integer) _
    As (ClosestSegmentIndex As Integer, DeviationMeters As Double)

        ' The input is one point of the dog and the route of the runner.
        ' Here will be the part with the "floating window" that looks for the nearest segment on the path of the runner.
        ' It will calculate the projection and return the index of the nearest segment and the distance in meters.

        ' ... implementace ...
        Dim windowSize As Int16 = runnerTrackXY.Count / 2 'nezkoumáme celou trasu, ale jen polovinu
        Dim minDeviation As Double = Double.MaxValue
        For j = Math.Max(0, searchStartIndex - windowSize) To Math.Min(runnerTrackXY.Count - 2, searchStartIndex + windowSize)

            Dim dx As Double = runnerTrackXY(j + 1).X - runnerTrackXY(j).X
            Dim dy As Double = runnerTrackXY(j + 1).Y - runnerTrackXY(j).Y
            Dim t As Double = ((dogPointXY.X - runnerTrackXY(j).X) * dx + (dogPointXY.Y - runnerTrackXY(j).Y) * dy) / (dx * dx + dy * dy)
            t = Math.Max(0, Math.Min(1, t))
            Dim projX As Double = runnerTrackXY(j).X + t * dx
            Dim projY As Double = runnerTrackXY(j).Y + t * dy
            Dim deviation As Double = Math.Sqrt((dogPointXY.X - projX) ^ 2 + (dogPointXY.Y - projY) ^ 2)
            If deviation < minDeviation Then
                minDeviation = deviation
                searchStartIndex = j ' posuneme okno dopředu
            End If
        Next j
        Dim closestSegmentIndex As Integer = searchStartIndex
        Return (closestSegmentIndex, minDeviation)
    End Function

    ''' <summary>
    ''' Evaluates a list of waypoints against the runner's track.
    ''' It calculates each waypoint's deviation from the track and its projected distance from the start of the track.
    ''' Automatically includes the dog's last known position as the final waypoint to evaluate.
    ''' </summary>
    ''' <param name="wayPoints">A collection of user-defined waypoints (e.g., found checkpoints).</param>
    ''' <param name="dogGeoPoints">The dog's track, used to get the final position.</param>
    ''' <param name="runnerGeoPoints">The runner's track with original Lat/Lon coordinates, used for accurate distance calculation along the trail.</param>
    ''' <param name="runnerXY">The runner's track converted to XY coordinates, used for fast geometric projections.</param>
    ''' <param name="lat0">Reference latitude for coordinate conversion.</param>
    ''' <param name="lon0">Reference longitude for coordinate conversion.</param>
    ''' <returns>A list of tuples, where each tuple contains the distance from the start of the trail in kilometers, the waypoint's deviation from the trail in meters and gross speed of the team as distancealongTrail to the checkpoint/time from start to the checkPoint.</returns>
    Private Function EvaluateCheckPoints(checkPoints As TrackAsTrkPts, dogGeoPoints As List(Of TrackGeoPoint), runnerGeoPoints As List(Of TrackGeoPoint), runnerXY As List(Of (X As Double, Y As Double)), lat0 As Double, lon0 As Double, totalRunnerDistanceKm As Double) _
    As List(Of CheckpointData) '(distanceAlongTrail As Double, deviationFromTrail As Double, dogGrossSpeedkmh As Double))
        Dim conv As New TrackConverter()
        Dim results As New List(Of CheckpointData) '(distanceAlongTrail As Double, deviationFromTrail As Double, dogGrossSpeedkmh As Double))
        Dim converter As New TrackConverter
        ' If there's no runner track to compare against, we can't evaluate anything.
        If runnerXY.Count < 2 OrElse runnerGeoPoints.Count < 2 Then
            Return results
        End If

        ' --- Step 1: Prepare a consolidated list of all points to evaluate ---
        Dim pointsToEvaluate As New List(Of TrackGeoPoint)

        ' Add user-defined checkpoints, if they exist.
        If checkPoints.TrackPoints.Count > 0 Then
            ' This assumes a converter class exists, as per your original code.
            Dim checkpointsAsListOfGeoPoints As List(Of TrackGeoPoint) = converter.ConvertTrackTrkPtsToGeoPoints(checkPoints).TrackGeoPoints
            checkpointsAsListOfGeoPoints.Sort(Function(a, b) Nullable.Compare(a.Time, b.Time))
            If checkpointsAsListOfGeoPoints IsNot Nothing Then
                pointsToEvaluate.AddRange(checkpointsAsListOfGeoPoints)
            End If
        End If

        ' Add the dog's last position as a critical checkpoint to check for "finding" the runner.
        If dogGeoPoints?.Count > 0 Then
            pointsToEvaluate.Add(dogGeoPoints.Last())
        End If

        ' If there are no points to check, exit early.
        If pointsToEvaluate.Count = 0 Then
            Return results
        End If

        ' --- Step 2: Process each point in the list ---
        For Each _Point As TrackGeoPoint In pointsToEvaluate
            ' Convert the checkpoint's location to XY coordinates.
            Dim checkPointX, checkPointY As Double
            conv.LatLonToXY(_Point.Location.Lat, _Point.Location.Lon, lat0, lon0, checkPointX, checkPointY)

            Dim minDeviation As Double = Double.MaxValue
            Dim closestSegmentIndex As Integer = -1
            Dim finalProjectionPoint As (X As Double, Y As Double) = (0, 0)

            ' --- Find the closest point on the runner's XY track (projection) ---
            For j As Integer = 0 To runnerXY.Count - 2
                Dim p1 = runnerXY(j)
                Dim p2 = runnerXY(j + 1)
                Dim dx As Double = p2.X - p1.X
                Dim dy As Double = p2.Y - p1.Y

                ' If the segment has zero length, skip it.
                If dx = 0 AndAlso dy = 0 Then Continue For

                ' Calculate the projection of the checkpoint onto the line defined by the segment.
                Dim t As Double = ((checkPointX - p1.X) * dx + (checkPointY - p1.Y) * dy) / (dx * dx + dy * dy)
                ' Clamp the projection to the segment itself (t between 0 and 1).
                t = Math.Max(0, Math.Min(1, t))

                Dim projectionX As Double = p1.X + t * dx
                Dim projectionY As Double = p1.Y + t * dy

                ' Calculate the deviation of the checkpoint to its projection on the segment.
                Dim deviation As Double = Math.Sqrt((checkPointX - projectionX) ^ 2 + (checkPointY - projectionY) ^ 2)

                If deviation < minDeviation Then
                    minDeviation = deviation
                    closestSegmentIndex = j
                    finalProjectionPoint = (projectionX, projectionY)
                End If
            Next j

            ' --- If a closest point was found, calculate the distance from the start along the trail ---
            If closestSegmentIndex > -1 Then

                ' Sum the lengths of all full segments before the closest one.
                Dim distanceAlongTrailMeters As Double = 0.0
                For k As Integer = 0 To closestSegmentIndex - 1
                    Dim runnerPoint1 = runnerXY(k)
                    Dim runnerPoint2 = runnerXY(k + 1)
                    distanceAlongTrailMeters += Math.Sqrt((runnerPoint1.X - runnerPoint2.X) ^ 2 + (runnerPoint1.Y - runnerPoint2.Y) ^ 2) ' in meters 
                Next k


                ' Add the partial length of the last segment (from its start to the projection point).
                ' This is calculated in the XY plane, which is accurate enough for a single segment.
                Dim startOfClosestSegment = runnerXY(closestSegmentIndex)
                Dim partialSegmentLength As Double = Math.Sqrt((finalProjectionPoint.X - startOfClosestSegment.X) ^ 2 + (finalProjectionPoint.Y - startOfClosestSegment.Y) ^ 2)
                distanceAlongTrailMeters += partialSegmentLength
                ' dog's time from start to checkpoint (time from start to checkpoint.time)
                ' Add the final calculated values to the results list.
                Dim checkPointTime As DateTime = _Point.Time
                Dim dogStartTime As DateTime = dogGeoPoints.First.Time
                Dim timeDiffHours As Double = (checkPointTime - dogStartTime).TotalHours
                ' Calculate gross speed (distanceAlongTrail / total time) in km/h
                Dim dogGrossSpeedkmh As Double = If(timeDiffHours > 0, distanceAlongTrailMeters / 1000.0 / timeDiffHours, 0.0) ' in km/h
                results.Add(New CheckpointData With {
                            .distanceAlongTrail = distanceAlongTrailMeters,
                            .deviationFromTrail = minDeviation,
                            .dogGrossSpeedkmh = dogGrossSpeedkmh})
            End If
        Next

        Return results
    End Function

    ''' <summary>
    ''' Calculates the final Competition score based on the analyzed track statistics.
    ''' </summary>
    ''' <param name="stats">The complete TrackStatsStructure object containing all performance metrics.</param>
    ''' <param name="recalculate">A boolean indicating whether to force a recalculation, ignoring existing scores even if they are present.</param>
    ''' <returns>A tuple containing the individual score components and handler/dog names.</returns>
    Public Function CalculateCompetitionScore(stats As TrailStats) As ScoringData

        ' --- 1. Scoring Configuration ---
        ' Define all scoring parameters based on the active category configuration.

        ' A) Points for major objectives
        Dim POINTS_FOR_FIND As Integer = ActiveCategoryInfo.PointsForFindMax               ' Max points for successfully finding the runner.
        ' B) Checkpoint reading points (Divided by 2 as typically two checkpoints are evaluated).
        Dim POINTS_FOR_DOG_READING_MAX_CHECKPOINT As Integer = ActiveCategoryInfo.PointsForDogReadingMax / 2
        ' C) Bonus points for each km/h of gross speed.
        Dim POINTS_PER_KMH_GROSS_SPEED As Double = ActiveCategoryInfo.PointsPerKmhGrossSpeed
        ' D) Maximum points for dog accuracy (trail following).
        Dim POINTS_FOR_DOG_ACCURACY_MAX As Integer = ActiveCategoryInfo.PointsForAccuracyMax

        ' --- 2. Initial Validity Check ---
        If stats.PointsInMTCompetition Is Nothing Then
            stats.PointsInMTCompetition = New ScoringData
        End If
        If stats.DogTotalTime.TotalSeconds <= 0 Then
            ' Return zero points if no time was recorded (no activity to score).
            Return New ScoringData With {.RunnerFoundPoints = 0,
                .DogSpeedPoints = 0,
                .DogAccuracyPoints = 0,
                .DogReadingPoints = 0,
                .dogName = stats.PointsInMTCompetition.dogName,
                .handlerName = stats.PointsInMTCompetition.handlerName}
        End If

        ' --- 3. Initialize Score Accumulators ---
        Dim RunnerFoundPoints As Integer = 0
        Dim DogSpeedPoints As Integer = 0
        Dim DogAccuracyPoints As Integer = 0
        Dim DogReadingPoints As Integer = 0


        ' --------------------------------------------------------------------------------
        ' --- STEP 1: Calculate Runner Found Points (Primary Objective) ---
        ' --------------------------------------------------------------------------------


        If stats.RunnerFound Then
            ' SUCCESS PATH: Full points for the find.
            RunnerFoundPoints = POINTS_FOR_FIND
        Else
            ' FAILURE PATH: Zero points for no find.
            RunnerFoundPoints = 0
        End If


        ' --------------------------------------------------------------------------------
        ' --- STEP 2: Calculate Dog Speed Points (Bonus for Overall Efficiency) ---
        ' --------------------------------------------------------------------------------


        If stats.DogGrossSpeed > 0 Then
            ' Calculate a distance weight: rewards speed based on how far into the trail the team reached.
            Dim distanceWeight As Double = stats.MaxTeamDistance / stats.RunnerDistance

            ' Score = GrossSpeed [km/h] * PointsPerKmh * DistanceWeight (Rounded)
            DogSpeedPoints = CInt(Math.Round(stats.DogGrossSpeed * POINTS_PER_KMH_GROSS_SPEED * distanceWeight))
        End If


        ' --------------------------------------------------------------------------------
        ' --- STEP 3: Calculate Dog Accuracy Points (Trail Following) ---
        ' --------------------------------------------------------------------------------


        ' Check for invalid or non-numeric percentage values before calculating.
        If Double.IsNaN(stats.WeightedDistanceAlongTrailPerCent) OrElse
           Double.IsInfinity(stats.WeightedDistanceAlongTrailPerCent) OrElse
           stats.WeightedDistanceAlongTrailPerCent < 0 OrElse
           stats.WeightedTimePerCent < 0 Then

            DogAccuracyPoints = 0 ' Assign zero if data is invalid/missing.
        Else
            ' Accuracy score is usually a 50/50 blend of time-based and distance-based accuracy.

            ' 50% of max score from distance accuracy: (Percentage / 100) * (MaxPoints / 2)
            DogAccuracyPoints = CInt(stats.WeightedDistanceAlongTrailPerCent / 100 * (POINTS_FOR_DOG_ACCURACY_MAX / 2))

            ' Add 50% of max score from time accuracy: (Percentage / 100) * (MaxPoints / 2)
            DogAccuracyPoints += CInt(stats.WeightedTimePerCent / 100 * (POINTS_FOR_DOG_ACCURACY_MAX / 2))
        End If


        ' --------------------------------------------------------------------------------
        ' --- STEP 4: Calculate Dog Reading Points (Checkpoints) ---
        ' --------------------------------------------------------------------------------

        If stats.CheckpointsEval IsNot Nothing AndAlso stats.CheckpointsEval.Count >= 2 Then
            ' Note: The last item in CheckpointsEval is the final dog position, NOT a scored checkpoint.
            ' We generally evaluate the last two actual checkpoints.

            ' Determine the index range for the last two potential checkpoints.
            Dim lastActualCheckpointIndex As Integer = stats.CheckpointsEval.Count - 2
            Dim startIndex As Integer = Math.Max(lastActualCheckpointIndex - 1, 0) ' Start at the second-to-last or the first point (index 0)
            Dim endIndex As Integer = lastActualCheckpointIndex ' The last actual checkpoint index

            For i As Integer = startIndex To endIndex
                Dim checkPointEval = stats.CheckpointsEval(i)
                Dim Deviation As Double = checkPointEval.deviationFromTrail

                ' Calculate a weight (0.0 to 1.0) based on deviation from the trail.
                Dim weightFactor As Double = Weight(Deviation) ' Assumes Weight(double) function exists.

                ' Score contribution for this checkpoint:
                ' (Distance up to CP / Total Runner Distance) * WeightFactor * MaxPointsPerCheckpoint
                DogReadingPoints += CInt((checkPointEval.distanceAlongTrail / stats.RunnerDistance) * weightFactor * POINTS_FOR_DOG_READING_MAX_CHECKPOINT)
            Next

        End If


        ' --------------------------------------------------------------------------------
        ' --- Final Step: Return all calculated scores ---
        ' --------------------------------------------------------------------------------

        Return New ScoringData With {
        .RunnerFoundPoints = RunnerFoundPoints,
        .DogSpeedPoints = DogSpeedPoints,
        .DogAccuracyPoints = DogAccuracyPoints,
        .DogReadingPoints = DogReadingPoints,
        .dogName = stats.PointsInMTCompetition.dogName,
        .handlerName = stats.PointsInMTCompetition.handlerName}

    End Function

    ''' <summary>
    ''' Calculates the weight based on the deviation in meters.
    ''' The weight is 1.0 for a deviation of 0 m and decreases to 0.5 at a deviation of 30 m, then decreases asymptotically to 0.
    '''</summary>
    ''' <param name="deviation"></param>
    ''' <returns></returns>
    Private Shared Function Weight(deviation As Double) As Double
        Dim p As Double = 4.0 '  exponent determining the steepness of the weight drop
        Dim k As Double = 20.0 '  characteristic distance in meters, at which the weight drops to 0.5 for p=4

        Dim _weight As Double
        If deviation <= 0 Then
            _weight = 1.0
        Else
            _weight = 1.0 / (1.0 + Math.Pow(deviation / k, p)) 'up to k meters the weight drops from 1 to 0.5 then goes to limit 0
        End If

        Return _weight
    End Function





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
        Return 'zablokováno
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
                'poslední bod v clusteru se nahradí centroidem
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



    Public Function PrependDateToFilename(newFileName As String) As String

        'Dim fileExtension As String = Path.GetExtension(Reader.FilePath)
        'Dim fileNameOhneExt As String = Path.GetFileNameWithoutExtension(Reader.FilePath)
        'Dim newFileName As String = Reader.FileName

        Try
            ' Smaže datum v názvu souboru (to kvůli převodu na iso formát):
            Dim result As (DateTime?, String) = GetRemoveDateFromName(Me.FileName)
            Dim modifiedFileName As String = result.Item2
            newFileName = $"{Me.TrailStart.Time:yyyy-MM-dd}_{modifiedFileName}"
        Catch ex As Exception
            Debug.WriteLine(ex.ToString())
            'ponechá původní jméno, ale přidá datum
            newFileName = $"{TrailStart:yyyy-MM-dd}_{Me.Reader.FileName}"
        End Try
        Return newFileName
        'If Me.Reader.FileName <> newFileName Then RenameFile(newFileName)

    End Function

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
        SetCreatedModifiedDate() 'znovu nastaví datum na původní
        RaiseEvent WarningOccurred($"File {Me.Reader.FileName} has been saved successfully.", Color.DarkGreen)
        Return True
    End Function

    ' ☀🌦🌧  Počasí
    Public Async Function Weather() As Task(Of WeatherData)
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
                Return Nothing
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
                    Dim cloud_cover = cloud_covers(index).GetDouble

                    Debug.Write("Pro čas " & hledanyCasUTC & ": ")
                    Debug.Write("Teplota: " & temperature.ToString())
                    Debug.Write(" Oblačnost:  " & cloud_cover.ToString())
                    Debug.Write(" Srážky (mm/h):  " & precipitation.ToString())
                    Debug.Write(" Vítr (m/s): " & windSpeed.ToString())
                    Debug.WriteLine(" Vítr: " & WindDirectionToText(windDir))
                    Return New WeatherData With {
                        .temperature = temperature,
                        .windSpeed = windSpeed,
                        .windDirection = windDir,
                        .precipitation = precipitation,
                        .relHumidity = relHumidity,
                        .cloudCover = cloud_cover}
                Catch ex As Exception
                    Return Nothing
                End Try
            End If


        Catch ex As Exception
            RaiseEvent WarningOccurred($"Failed to fetch weather data: {ex.ToString}", Color.Red)
            Return Nothing
        End Try

        Return Nothing
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
            Me.Reader.CreateAndAddElement(reportNode, GpxReader.K9_PREFIX & ":" & "dogName", localizedReport.Category.Text, True,,, GpxReader.K9_NAMESPACE_URI)
            Me.Reader.CreateAndAddElement(reportNode, GpxReader.K9_PREFIX & ":" & "goal", localizedReport.Goal.Text, True,,, GpxReader.K9_NAMESPACE_URI)
            Me.Reader.CreateAndAddElement(reportNode, GpxReader.K9_PREFIX & ":" & "trail", localizedReport.Trail.Text, True,,, GpxReader.K9_NAMESPACE_URI)
            Me.Reader.CreateAndAddElement(reportNode, GpxReader.K9_PREFIX & ":" & "performance", localizedReport.Performance.Text, True,,, GpxReader.K9_NAMESPACE_URI)
            Dim weatherNode As XmlNode = Me.Reader.CreateAndAddElement(reportNode, GpxReader.K9_PREFIX & ":" & "weather", localizedReport.Weather.Text, True,,, GpxReader.K9_NAMESPACE_URI)
            WriteWeatherDataToXml(weatherNode, localizedReport.WeatherData)
            Dim ScoringNode As XmlNode = Me.Reader.CreateAndAddElement(reportNode, GpxReader.K9_PREFIX & ":" & "scoring", localizedReport.PerformancePoints.Text, True,,, GpxReader.K9_NAMESPACE_URI)
            WriteScoringDataToXml(ScoringNode, Me.TrailStats.PointsInMTCompetition)
        Next

        Return True
    End Function

    Public Function ReadSavedDataFromXML(ByRef reports As Dictionary(Of String, TrailReport)) As (isStatsLoaded As Boolean, isScoringLoaded As Boolean, isWeatherLoaded As Boolean, isLocalisedReportLoaded As Boolean)
        'Dim reports As New Dictionary(Of String, TrailReport)()
        Try
            Dim metadataNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "metadata", Me.Reader.rootNode)
            If metadataNode Is Nothing Then Return (False, False, False, False) '< If metadata> vůbec neexistuje
            Dim extensionsNode As XmlNode = Me.Reader.SelectSingleChildNode(Me.Reader.GPX_DEFAULT_PREFIX & ":" & "extensions", metadataNode)
            If extensionsNode Is Nothing Then Return (False, False, False, False) ' <extensions> vůbec neexistuje

            Dim loadedStats As TrailStats = New TrailStats
            Dim loadedScoring As ScoringData = Nothing
            ' Find the TrailStats element
            Dim statsNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "trailStats", extensionsNode)
            Dim isStatsLoaded As Boolean = ReadTrailStatsFromXml(statsNode, loadedStats)
            Dim isScoringLoaded As Boolean = False 'je lokalizováno proto dále v cyklu
            Dim isWeatherLoaded As Boolean = False 'je lokalizováno proto dále v cyklu
            Dim isLocalisedReportLoaded As Boolean = False

            ' Získání všech <K9TrailsAnalyzer:report> uzlů
            Dim reportNodes As XmlNodeList = Me.Reader.SelectAllChildNodes(GpxReader.K9_PREFIX & ":" & "report", extensionsNode)
            If reportNodes Is Nothing OrElse reportNodes.Count = 0 Then Return (False, False, False, False) ' žádné reporty

            ' Pro každý report uzel vytvoříme TrailReport objekt
            For Each reportNode As XmlNode In reportNodes
                Dim lang As String = reportNode.Attributes("lang")?.Value
                If String.IsNullOrEmpty(lang) Then Continue For ' pokud není jazyk, přeskočíme

                Dim weatherNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "weather", reportNode)
                isWeatherLoaded = ReadWeatherDataFromXml(weatherNode, Me.WeatherData)

                Dim scoringNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "scoring", reportNode)
                isScoringLoaded = ReadScoringDataFromXml(scoringNode, loadedScoring)

                Dim localizedReport As New TrailReport With {
                    .WeatherData = Me.WeatherData}

                isLocalisedReportLoaded = ReadLocalisedReportFromXml(reportNode, localizedReport)
                If Not reports.ContainsKey(lang) Then reports.Add(lang, localizedReport)
            Next reportNode
            If isScoringLoaded Then
                loadedStats.PointsInMTCompetition = loadedScoring
            End If
            Me.TrailStats = loadedStats ' Nastaví kompletní objekt přes Set sekci property


            Return (isStatsLoaded, isScoringLoaded, isWeatherLoaded, isLocalisedReportLoaded) 'Me.TrailStats = loadedStats ' Nastaví kompletní objekt přes Set sekci property

        Catch ex As Exception
            Return (False, False, False, False)
        End Try
        Return (False, False, False, False)

    End Function

    ' Uloží tuple do XML elementu pomocí CreateAndAddElement
    Private Shared Sub WriteWeatherDataToXml(weatherNode As XmlNode,
                                  _weatherData As WeatherData)

        If _weatherData Is Nothing Then
            Debug.WriteLine("WeatherData je nothing, proč???")
            Return 'pokud se je nepodařilo načíst...
        End If

        ' 2️⃣ Funkce pro zápis atributu
        Dim setAttr = Sub(name As String, value As Double?)
                          If value.HasValue Then
                              CType(weatherNode, XmlElement).SetAttribute(name, value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))
                          Else
                              CType(weatherNode, XmlElement).SetAttribute(name, "")
                          End If
                      End Sub

        ' 3️⃣ Nastavení atributů
        setAttr("temperature", _weatherData.temperature)
        setAttr("windSpeed", _weatherData.windSpeed)
        setAttr("windDirection", _weatherData.windDirection)
        setAttr("precipitation", _weatherData.precipitation)
        setAttr("relHumidity", _weatherData.relHumidity)
        setAttr("cloudCover", _weatherData.cloudCover)
    End Sub

    ' Načte tuple z XML elementu
    Private Shared Function ReadWeatherDataFromXml(weatherDataNode As XmlNode, ByRef _weatherData As WeatherData) As Boolean
        If weatherDataNode Is Nothing Then Return False
        If weatherDataNode.Attributes.Count = 0 Then Return False

        _weatherData = New WeatherData With {
            .temperature = ParseDouble(weatherDataNode, "temperature"),
            .windSpeed = ParseDouble(weatherDataNode, "windSpeed"),
            .windDirection = ParseDouble(weatherDataNode, "windDirection"),
            .precipitation = ParseDouble(weatherDataNode, "precipitation"),
            .relHumidity = ParseDouble(weatherDataNode, "relHumidity"),
            .cloudCover = ParseDouble(weatherDataNode, "cloudCover")}
        If weatherDataNode.InnerText = "" Then Return False
        Return True
    End Function

    Private Function ReadLocalisedReportFromXml(reportNode As XmlNode, ByRef _localizedReport As TrailReport) As Boolean
        If reportNode Is Nothing Then Return False
        ' Načte lokalizovaný report z XML uzlu
        Dim dogNameNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "dogName", reportNode)
        Dim goalNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "goal", reportNode)
        Dim trailNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "trail", reportNode)
        Dim performanceNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "performance", reportNode)

        If dogNameNode Is Nothing OrElse goalNode Is Nothing OrElse trailNode Is Nothing OrElse performanceNode Is Nothing Then Return False

        With _localizedReport
            .CategoryText = If(dogNameNode?.InnerText, "")
            .GoalText = If(goalNode?.InnerText, "")
            .TrailText = If(trailNode?.InnerText, "")
            .PerformanceText = If(performanceNode?.InnerText, "")
        End With
        Return True

    End Function
    ' Uloží tuple do XML elementu pomocí CreateAndAddElement
    Private Shared Sub WriteScoringDataToXml(scoringNode As XmlNode,
                                  scoringData As ScoringData)


        ' 2️⃣ Funkce pro zápis atributu
        Dim setAttr = Sub(name As String, value As Double?)
                          If value.HasValue Then
                              CType(scoringNode, XmlElement).SetAttribute(name, value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))
                          Else
                              CType(scoringNode, XmlElement).SetAttribute(name, "")
                          End If
                      End Sub

        ' 3️⃣ Nastavení atributů
        setAttr("runnerFoundPoints", scoringData.RunnerFoundPoints)
        setAttr("speedPoints", scoringData.DogSpeedPoints)
        setAttr("accuracyPoints", scoringData.DogAccuracyPoints)
        setAttr("dogReadingPoints", scoringData.DogReadingPoints)
        CType(scoringNode, XmlElement).SetAttribute("dogName", scoringData.dogName)
        CType(scoringNode, XmlElement).SetAttribute("handlerName", scoringData.handlerName)
    End Sub


    Private Sub SetAttributeDouble(node As XmlElement, attributeName As String, attributeValue As Double, format As String)
        node.SetAttribute(attributeName, attributeValue.ToString(format, System.Globalization.CultureInfo.InvariantCulture))
    End Sub

    Private Sub SetAttributeTimeSpan(node As XmlElement, attributeName As String, attributeValue As TimeSpan, format As String)
        ' Ukládáme čas jako celkový počet sekund (double) pro snadné načtení.
        node.SetAttribute(attributeName, attributeValue.TotalSeconds.ToString(format, System.Globalization.CultureInfo.InvariantCulture))
    End Sub
    Private Sub SetAttributeBoolean(node As XmlElement, attributeName As String, attributeValue As Boolean)
        node.SetAttribute(attributeName, If(attributeValue, "True", "False"))
    End Sub

    Public Sub WriteTrailStatsToXml(statsData As TrailStats)
        'přidá do <extensions> 
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
        Dim trailStatsNode As XmlNode = Me.Reader.CreateAndAddElement(extensionsNode, GpxReader.K9_PREFIX & ":" & "trailStats", "", True,,, GpxReader.K9_NAMESPACE_URI)

        ' 3. Nastavení atributů pro Double/procenta do <TrailStats>
        SetAttributeDouble(trailStatsNode, "dogDistance", statsData.DogDistance, "0")
        SetAttributeDouble(trailStatsNode, "runnerDistance", statsData.RunnerDistance, "0")
        SetAttributeDouble(trailStatsNode, "weightedDistanceAlongTrail", statsData.WeightedDistanceAlongTrail, "0")
        SetAttributeDouble(trailStatsNode, "weightedDistanceAlongTrailPerCent", statsData.WeightedDistanceAlongTrailPerCent, "0")
        SetAttributeDouble(trailStatsNode, "weightedTimePerCent", statsData.WeightedTimePerCent, "0")
        SetAttributeDouble(trailStatsNode, "dogNetSpeed", statsData.DogNetSpeed, "0.0")
        SetAttributeDouble(trailStatsNode, "dogGrossSpeed", statsData.DogGrossSpeed, "0.0")
        SetAttributeDouble(trailStatsNode, "deviation", statsData.AverDeviation, "0.0")
        SetAttributeDouble(trailStatsNode, "maxTeamDistance", statsData.MaxTeamDistance, "0")

        ' 4. Nastavení atributů pro TimeSpan
        SetAttributeTimeSpan(trailStatsNode, "trailAge", statsData.TrailAge, "0")
        SetAttributeTimeSpan(trailStatsNode, "totalTime", statsData.DogTotalTime, "0")
        SetAttributeTimeSpan(trailStatsNode, "movingTime", statsData.DogMovingTime, "0")
        SetAttributeTimeSpan(trailStatsNode, "stoppedTime", statsData.DogStoppedTime, "0")

        ' 5. Nastavení atributů pro Boolean
        SetAttributeBoolean(trailStatsNode, "runnerFound", statsData.RunnerFound)

        ' --------------------------------------------------------------------------------
        ' 6. ZÁPIS CHECKPOINTŮ (CheckpointsEval)

        If statsData.CheckpointsEval IsNot Nothing AndAlso statsData.CheckpointsEval.Count > 0 Then

            ' Vytvoření kontejnerového nodu pro všechny checkpointy: <Checkpoints>
            Dim checkpointsContainerNode As XmlElement = Me.Reader.CreateAndAddElement(trailStatsNode, GpxReader.K9_PREFIX & ":" & "checkpoints", "", True,,, GpxReader.K9_NAMESPACE_URI)

            Dim i As Integer = 0
            ' Iterace přes všechny vyhodnocené checkpointy
            For Each cpEval In statsData.CheckpointsEval

                ' Vytvoření nodu pro jeden checkpoint: <Checkpoint>
                Dim cpNode As XmlElement = Me.Reader.CreateAndAddElement(checkpointsContainerNode, GpxReader.K9_PREFIX & ":" & $"checkpointNo{i}", "", True,,, GpxReader.K9_NAMESPACE_URI)
                i += 1

                ' Zápis hodnot jako atributy:
                ' U anonymních tuple (jak byly v původní definici) musíte přistupovat přes Item1, Item2, Item3.
                ' Pokud používáte explicitní strukturu (viz poznámka 1), použijte názvy property.
                SetAttributeDouble(cpNode, "distanceAlongTrail", cpEval.distanceAlongTrail, "0")
                SetAttributeDouble(cpNode, "deviationFromTrail", cpEval.deviationFromTrail, "0.0")
                SetAttributeDouble(cpNode, "dogGrossSpeed", cpEval.dogGrossSpeedkmh, "0.0")

                ' Přidání <Checkpoint> do <Checkpoints>
                checkpointsContainerNode.AppendChild(cpNode)
            Next
        End If
        ' --------------------------------------------------------------------------------
        ' 7. ZÁPIS BODŮ S MAXIMÁLNÍ DEVIACÍ (MaxDeviationGeoPoints)
        ' --------------------------------------------------------------------------------
        If statsData.MaxDeviationGeoPoints IsNot Nothing AndAlso statsData.MaxDeviationGeoPoints.TrackGeoPoints IsNot Nothing AndAlso statsData.MaxDeviationGeoPoints.TrackGeoPoints.Count > 0 Then

            ' Vytvoření kontejnerového nodu pro body s maximální deviací: <MaxDeviationGeoPoints>
            Dim maxDevContainerNode As XmlElement = Me.Reader.CreateAndAddElement(trailStatsNode, GpxReader.K9_PREFIX & ":" & "maxDeviationPoints", "", True,,, GpxReader.K9_NAMESPACE_URI)

            '  body s maximální deviací
            Dim dogGeoPoint As TrackGeoPoint = statsData.MaxDeviationGeoPoints.TrackGeoPoints(0)
            ' Vytvoření nodu pro jeden bod: <Point>
            Dim DogpointNode As XmlElement = Me.Reader.CreateAndAddElement(maxDevContainerNode, GpxReader.K9_PREFIX & ":" & $"dogpt", "", True,,, GpxReader.K9_NAMESPACE_URI)
            ' Zápis hodnot jako atributy:
            SetAttributeDouble(DogpointNode, "lat", dogGeoPoint.Location.Lat, "0.0000000")
            SetAttributeDouble(DogpointNode, "lon", dogGeoPoint.Location.Lon, "0.0000000")

            '  runner s maximální deviací
            Dim runnerGeoPoint As TrackGeoPoint = statsData.MaxDeviationGeoPoints.TrackGeoPoints(1)
            ' Vytvoření nodu pro jeden bod: <Point>
            Dim runnerpointNode As XmlElement = Me.Reader.CreateAndAddElement(maxDevContainerNode, GpxReader.K9_PREFIX & ":" & $"runnerpt", "", True,,, GpxReader.K9_NAMESPACE_URI)
            ' Zápis hodnot jako atributy:
            SetAttributeDouble(runnerpointNode, "lat", runnerGeoPoint.Location.Lat, "0.0000000")
            SetAttributeDouble(runnerpointNode, "lon", runnerGeoPoint.Location.Lon, "0.0000000")

        End If

    End Sub

    ''' <summary>
    ''' Reads competition scoring data from the provided XML node.
    ''' </summary>
    Public Shared Function ReadScoringDataFromXml(scoringNode As XmlNode, ByRef _scoringData As ScoringData) As Boolean
        If scoringNode Is Nothing Then Return False
        If scoringNode.Attributes.Count = 0 Then Return False

        _scoringData = New ScoringData With {
        .RunnerFoundPoints = ParseInteger(scoringNode, "runnerFoundPoints"),
        .DogSpeedPoints = ParseInteger(scoringNode, "speedPoints"),
        .DogAccuracyPoints = ParseInteger(scoringNode, "accuracyPoints"),
        .DogReadingPoints = ParseInteger(scoringNode, "dogReadingPoints")
    }

        ' Dog and Handler names (String attributes)
        _scoringData.dogName = scoringNode?.Attributes("dogName")?.Value
        _scoringData.handlerName = scoringNode?.Attributes("handlerName")?.Value

        If _scoringData.dogName Is Nothing Then _scoringData.dogName = ""
        If _scoringData.handlerName Is Nothing Then _scoringData.handlerName = ""
        With _scoringData
            If .RunnerFoundPoints + .DogSpeedPoints + .DogAccuracyPoints + .DogReadingPoints < 1 Then
                'žádné body
                Return False
            End If
        End With
        Return True
    End Function

    ''' <summary>
    ''' Reads detailed trail statistics and checkpoint data from the parent XML node.
    ''' </summary>
    ''' <returns>True if the TrailStats node was successfully found and parsed; False otherwise.</returns>
    Public Function ReadTrailStatsFromXml(statsNode As XmlElement, ByRef stats As TrailStats) As Boolean

        If statsNode Is Nothing Then
            ' Nenašli jsme uzel, načtení bylo neúspěšné. 
            stats = New TrailStats() ' Vratte prázdný objekt
            Return False
            'Else
            '    statsNode.RemoveAll() 'todo: pro testování, smazat!!!!
            '    Return False
        End If

        ' Find the maxDeviationPoints container
        Dim maxDevContainerNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "maxDeviationPoints", statsNode)
        If maxDevContainerNode Is Nothing Then
            stats = New TrailStats() ' Vratte prázdný objekt
            Return False 'je třeba znovu vypočítat
        End If

        stats = New TrailStats With {
              .DogDistance = ParseDouble(statsNode, "dogDistance"),
        .RunnerDistance = ParseDouble(statsNode, "runnerDistance"),
        .WeightedDistanceAlongTrail = ParseDouble(statsNode, "weightedDistanceAlongTrail"),
        .WeightedDistanceAlongTrailPerCent = ParseDouble(statsNode, "weightedDistanceAlongTrailPerCent"),
        .WeightedTimePerCent = ParseDouble(statsNode, "weightedTimePerCent"),
        .DogNetSpeed = ParseDouble(statsNode, "dogNetSpeed"),
        .DogGrossSpeed = ParseDouble(statsNode, "dogGrossSpeed"),
        .AverDeviation = ParseDouble(statsNode, "deviation"),
        .MaxTeamDistance = ParseDouble(statsNode, "maxTeamDistance"),' --- Read TimeSpan attributes (stored as seconds) ---
        .TrailAge = ParseTimeSpan(statsNode, "trailAge"),
        .DogTotalTime = ParseTimeSpan(statsNode, "totalTime"),
        .DogMovingTime = ParseTimeSpan(statsNode, "movingTime"),
        .DogStoppedTime = ParseTimeSpan(statsNode, "stoppedTime"),' --- Read Boolean attribute ---
        .RunnerFound = ParseBoolean(statsNode, "runnerFound")
    }

        ' --- Read CheckpointsEval (List of Tuples) ---
        stats.CheckpointsEval = New List(Of CheckpointData) ' (distanceAlongTrail As Double, deviationFromTrail As Double, dogGrossSpeed As Double))()

        ' Find the Checkpoints container
        'statsNode.SelectSingleNode("checkpoints")
        Dim checkpointsContainer As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "checkpoints", statsNode)
        If checkpointsContainer IsNot Nothing Then
            ' Iterate over all Checkpoint elements
            Dim i As Integer = 0

            For Each cpNode As XmlNode In checkpointsContainer.ChildNodes

                Dim distAlong As Double = ParseDouble(cpNode, "distanceAlongTrail")
                Dim devFrom As Double = ParseDouble(cpNode, "deviationFromTrail")
                Dim grossSpeed As Double = ParseDouble(cpNode, "dogGrossSpeed")

                ' Add the parsed tuple to the list
                stats.CheckpointsEval.Add(New CheckpointData With {
                                          .distanceAlongTrail = distAlong,
                                          .deviationFromTrail = devFrom,
                                          .dogGrossSpeedkmh = grossSpeed})
            Next
        End If




        '  body s maximální deviací
        Dim dogGeoPointNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "dogpt", maxDevContainerNode)
        Dim lat As Double = ParseDouble(dogGeoPointNode, "lat")
        Dim lon As Double = ParseDouble(dogGeoPointNode, "lon")
        Dim dogGeoPoint As New TrackGeoPoint With {
                    .Location = New Coordinates With {.Lat = lat, .Lon = lon},
                    .Time = Nothing
                }

        Dim runnerGeoPointNode As XmlNode = Me.Reader.SelectSingleChildNode(GpxReader.K9_PREFIX & ":" & "runnerpt", maxDevContainerNode)
        lat = ParseDouble(dogGeoPointNode, "lat")
            lon = ParseDouble(dogGeoPointNode, "lon")
            Dim runnergeopoint As New TrackGeoPoint With {
                    .Location = New Coordinates With {.Lat = lat, .Lon = lon},
                    .Time = Nothing
                }
            Dim trackGeopoints As New List(Of TrackGeoPoint) From {
                dogGeoPoint,
                runnergeopoint
            }
        If runnergeopoint.Location.Lat = dogGeoPoint.Location.Lat And runnergeopoint.Location.Lon = dogGeoPoint.Location.Lon Then
            Return False 'dočasně, omylem se ukládaly stejné hodnoty todo: smazat!
        End If
        stats.MaxDeviationGeoPoints = New TrackAsGeoPoints(TrackType.Unknown, trackGeopoints)


        If stats.RunnerDistance + stats.DogDistance < 1 Then Return False
        Return True
    End Function

    ' Helper pro bezpečné parsování Double z atributu.
    Private Shared Function ParseDouble(node As XmlNode, attributeName As String) As Double
        Dim attrValue As String = node?.Attributes(attributeName)?.Value
        If String.IsNullOrWhiteSpace(attrValue) Then Return 0.0

        Dim result As Double
        ' Použití InvariantCulture pro parsování čísel s tečkou.
        If Double.TryParse(attrValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, result) Then
            Return result
        Else
            Return 0.0
        End If
    End Function

    ' Helper pro bezpečné parsování Integer z atributu.
    Private Shared Function ParseInteger(node As XmlNode, attributeName As String) As Integer
        Dim attrValue As String = node?.Attributes(attributeName)?.Value
        If String.IsNullOrWhiteSpace(attrValue) Then Return 0

        Dim result As Integer
        If Integer.TryParse(attrValue, System.Globalization.CultureInfo.InvariantCulture, result) Then
            Return result
        Else
            Return 0
        End If
    End Function

    ' Helper pro bezpečné parsování TimeSpan (předpokládá, že je uložen jako celkový počet sekund Double).
    Private Shared Function ParseTimeSpan(node As XmlNode, attributeName As String) As TimeSpan
        Dim seconds As Double = ParseDouble(node, attributeName)
        Return TimeSpan.FromSeconds(seconds)
    End Function

    ' Helper pro bezpečné parsování Boolean (předpokládá, že je uložen jako "True" nebo "False").
    Private Shared Function ParseBoolean(node As XmlNode, attributeName As String) As Boolean
        Dim attrValue As String = node?.Attributes(attributeName)?.Value
        Return Boolean.TryParse(attrValue, Nothing) ' TryParse vrátí False při selhání
    End Function

End Class




Module TrackDisplayLogic
    Public Function ResolveLabel(tt As TrackType) As String
        Select Case tt
            Case TrackType.RunnerTrail : Return My.Resources.Resource1.RunnerTrail
            Case TrackType.DogTrack : Return My.Resources.Resource1.dogTrack
            Case TrackType.CrossTrail : Return My.Resources.Resource1.CrossingTrail
            Case TrackType.Article : Return My.Resources.Resource1.article
            Case Else : Return My.Resources.Resource1.txtUnknown
        End Select
    End Function
End Module


Public Class GpxReader
    Public xmlDoc As XmlDocument
    Public namespaceManager As XmlNamespaceManager
    Public Property FilePath As String


    ''' <returns>Return file name with extension</returns>
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
    Public Sub MarkAsDownloadeded(filePath As String)
        Dim fi As New FileInfo(filePath)
        Dim fileName = fi.Name
        Dim rec As New FileRecord With {
            .LastWriteTime = fi.LastWriteTimeUtc,
            .Length = fi.Length
        }

        downloadedFiles(fileName) = rec
        DownloadedSave()
    End Sub

    ' Uloží JSON
    Private Sub DownloadedSave()
        Dim options As New JsonSerializerOptions With {.WriteIndented = True,
                 .Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping ' aby se diakritika neescapeovala
        }
        Dim json = JsonSerializer.Serialize(downloadedFiles, options)
        File.WriteAllText(savePath, json, Encoding.UTF8)
    End Sub

End Class

