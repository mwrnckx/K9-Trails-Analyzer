Imports System.Globalization
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Xml
Imports GPXTrailAnalyzer.My.Resources
Imports System.Text.Json
Imports GPXTrailAnalyzer.OverlayVideoExport
Imports System.Net.Http


Public Class GpxFileManager
    'obsahuje seznam souborů typu gpxRecord a funkce na jejich vytvoření a zpracování
    Public ReadOnly Property gpxDirectory As String
    Private ReadOnly Property BackupDirectory As String
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

    Public Event WarningOccurred(message As String, _color As Color)

    Public Sub New()
        gpxDirectory = My.Settings.Directory
        BackupDirectory = My.Settings.BackupDirectory
        If Not Directory.Exists(BackupDirectory) Then
            Try
                Directory.CreateDirectory(BackupDirectory)
            Catch ex As Exception

            End Try

        End If
        maxAge = New TimeSpan(My.Settings.maxAge, 0, 0)
        prependDateToName = My.Settings.PrependDateToName
        trimGPS_Noise = My.Settings.TrimGPSnoise
        'mergeDecisions = My.Settings.MergeDecisions

    End Sub

    Public Async Function Main() As Task(Of Boolean)
        Dim _gpxFilesSortedAndFiltered As List(Of GPXRecord) = GetGPXFilesWithinInterval()
        Dim _gpxFilesMerged As List(Of GPXRecord) = MergeGpxFiles(_gpxFilesSortedAndFiltered)

        Dim totalDist As Double = 0


        For Each _gpxRecord As GPXRecord In _gpxFilesMerged
            Try

                If Me.ForceProcess Or Not _gpxRecord.IsAlreadyProcessed Then 'možno přeskočit, už to proběhlo...
                    _gpxRecord.RenamewptNode(My.Resources.Resource1.article) 'renaming wpt to "artickle"
                    If prependDateToName Then _gpxRecord.PrependDateToFilename()
                    If trimGPS_Noise Then _gpxRecord.TrimGPSnoise(12) 'ořízne nevýznamné konce a začátky trailů, když se stojí na místě.
                End If
                _gpxRecord.Distance = _gpxRecord.CalculateLayerTrailDistance()
                totalDist += _gpxRecord.Distance
                _gpxRecord.TotalDistance = totalDist
                _gpxRecord.Description = Await _gpxRecord.BuildSummaryDescription(Me.ForceProcess) 'vytvoří popis, pokud není, nebo doplní věk trasy do popisu
                _gpxRecord.TrailAge = _gpxRecord.GetAge

                If Me.ForceProcess Or Not _gpxRecord.IsAlreadyProcessed Then 'možno přeskočit, už to proběhlo...
                    _gpxRecord.WriteDescription() 'zapíše agregovaný popis do tracku TrailLayer
                End If
                _gpxRecord.DogSpeed = _gpxRecord.CalculateSpeed
                _gpxRecord.Link = _gpxRecord.Getlink
                _gpxRecord.WriteProcessed()
                If Me.ForceProcess Or Not _gpxRecord.IsAlreadyProcessed Then
                    _gpxRecord.Save() 'hlavně kvůli desc
                End If

                'a nakonec
                _gpxRecord.SetCreatedModifiedDate()

                If Me.ForceProcess Or Not _gpxRecord.IsAlreadyProcessed Then 'možno přeskočit, už to proběhlo...
                    If My.Settings.AskForVideo AndAlso _gpxRecord.DogStart <> Date.MinValue Then
                        If MessageBox.Show($"Should a video of the dog's movement be created from the file: {_gpxRecord.Reader.FileName}?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) = DialogResult.Yes Then
                            Try
                                Await _gpxRecord.CreateVideoFromDogTrack()
                            Catch ex As Exception
                                MessageBox.Show($"Creating a video from a file {_gpxRecord.Reader.FileName} failed." & vbCrLf & $"Message: {ex}")
                            End Try

                        End If
                    End If
                End If
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





    Public Function GetGPXFilesWithinInterval() As List(Of GPXRecord)
        Dim gpxFilesWithinInterval As New List(Of GPXRecord)
        ' Načteme všechny GPX soubory
        Dim gpxFilesAllPath As List(Of String) = Directory.GetFiles(gpxDirectory, "*.gpx").ToList()
        Dim backup As Boolean = False
        Try
            For Each gpxFilePath In gpxFilesAllPath
                Try
                    'Tady najde layerStart 
                    Dim _reader As New GpxReader(gpxFilePath)

                    Dim _gpxRecord As New GPXRecord(_reader, Me.ForceProcess)

                    If Me.ForceProcess Or Not _gpxRecord.IsAlreadyProcessed Then 'možno přeskočit, už to proběhlo...
                        If (_gpxRecord.IsAlreadyProcessed) Then
                            'pokud je forceProcess, tak se zpracuje i již zpracovaný soubor
                            RaiseEvent WarningOccurred($"File {_gpxRecord.Reader.FileName} has already been processed, but will be processed again.", Color.DarkOrange)
                        End If
                        _gpxRecord.SplitSegmentsIntoTracks() 'rozdělí trk s více segmenty na jednotlivé trk
                        _gpxRecord.SortTracksByTime() 'seřadí trk podle stáří od nejstaršího (layer) po nejmladší (dog)
                    End If
                    _gpxRecord.RefreshLayerDogStartFinish() ' načte časy startů

                    If _gpxRecord.LayerStart >= dateFrom And _gpxRecord.LayerStart <= dateTo Then

                        AddHandler _gpxRecord.WarningOccurred, AddressOf WriteRTBWarning
                        Dim _backup As Boolean = _gpxRecord.Backup()
                        'kvůli výpisu, pokud se žádný soubor nezazálohuje, výpis se nedělá:
                        If Not backup Then backup = _backup
                        gpxFilesWithinInterval.Add(_gpxRecord)

                    End If
                    '                   
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
            If backup Then
                Debug.WriteLine($"Soubory gpx byly úspěšně zálohovány do: {BackupDirectory }")
                RaiseEvent WarningOccurred($"{vbCrLf}{Resource1.logBackupOfFiles}   {BackupDirectory }{vbCrLf}", Color.DarkGreen)
            End If

        Catch ex As Exception
            Debug.WriteLine($"Chyba při zálohování souborů: {ex.Message}")
        End Try
        ' Seřazení podle data
        gpxFilesWithinInterval.Sort(Function(x, y) x.LayerStart.CompareTo(y.LayerStart))

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
            gpxFilesMerged.Add(_gpxRecords(i)) ' Přidáme aktuální prvek do merged listu
            If Not Me.ForceProcess And _gpxRecords(i).IsAlreadyProcessed Then Continue For  'možno přeskočit, už to proběhlo...

            Dim lastAddedIndex As Integer = gpxFilesMerged.Count - 1 ' Index posledního přidaného prvku

            ' Vnitřní cyklus se pokouší spojit POSLEDNĚ PŘIDANÝ prvek s NÁSLEDUJÍCÍMI
            Dim j As Integer = i + 1
            While j < _gpxRecords.Count
                Dim timeDiff As TimeSpan = _gpxRecords(j).LayerStart - _gpxRecords(lastAddedIndex).LayerStart

                If timeDiff > maxAge Then
                    ' Další záznam je už moc starý, nemá cenu pokračovat
                    Exit While
                End If

                If Not usedIndexes.Contains(j) Then
                    If TryMerge(_gpxRecords(j), gpxFilesMerged(lastAddedIndex)) Then
                        usedIndexes.Add(j)
                        ' lastAddedIndex zůstává stejný, protože mergujeme do stejného objektu
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

        'Dim mergeDecision As String = LoadMergeDecision(file_prev, file_i)
        'If Not mergeCancel AndAlso (Not mergeDecision = System.Windows.Forms.DialogResult.No.ToString) Then

        'kontrola duplicit: když je rozdíl menší než jedna sekunda, je to nejspíš stejný track
        If (file_i.LayerStart - file_prev.LayerStart < New TimeSpan(0, 0, 1)) Then
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
        lblSoubor1.Text = $"{My.Resources.Resource1.lblIsThisLayerQ}: '{Path.GetFileName(runner.Reader.FilePath)}'"
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


Public Class TrackTypes
    Public Const Dog As String = "Dog"
    Public Const TrailLayer As String = "TrailLayer"
    Public Const CrossTrack As String = "CrossTrack"
End Class



Public Class GPXRecord

    Public Event WarningOccurred(_message As String, _color As Color)

    Private _LayerStart As Date
    Public Property LayerStart As DateTime
        Get
            Return _LayerStart
        End Get
        Set
            _LayerStart = Value
        End Set
    End Property

    Private _DogStart As Date
    Public Property DogStart As DateTime
        Get
            Return _DogStart
        End Get
        Set
            _DogStart = Value
        End Set
    End Property

    Public Property trailStart As TrackGeoPoint

    Private _DogFinish As Date
    Public Property DogFinish As DateTime
        Get
            Return _DogFinish
        End Get
        Set
            _DogFinish = Value
        End Set
    End Property

    Public Property TrailAge As TimeSpan
    Public Property Distance As Double
    Public Property TotalDistance As Double
    Public Property Description As String
    Private DescriptionParts As List(Of (Text As String, Color As Color, FontStyle As FontStyle))
    Private DescriptionPartsEng As List(Of (Text As String, Color As Color, FontStyle As FontStyle))
    Public Property Link As String
    Public Property DogSpeed As Double
    Public Property Reader As GpxReader
    Public Property IsAlreadyProcessed As Boolean
    'Private Property WindDirection As Double = 0.0 ' Směr větru v stupních
    Dim WeatherData As (_temperature As Double, _windSpeed As Double, _windDirection As Double, _precipitation As Double, _relHumidity As Double, _cloudCover As Double)

    Private ReadOnly Property gpxDirectory As String
    Private ReadOnly Property BackupDirectory As String

    Public Sub New(_reader As GpxReader, forceProcess As Boolean)
        gpxDirectory = My.Settings.Directory
        BackupDirectory = My.Settings.BackupDirectory

        If Not Directory.Exists(BackupDirectory) Then
            Directory.CreateDirectory(BackupDirectory)
        End If
        Me.Reader = _reader
        IsAlreadyProcessed = IsProcessed()

    End Sub

    Friend Sub SetCreatedModifiedDate()
        'change of attributes
        ' Setting the file creation date
        IO.File.SetCreationTime(Me.Reader.FilePath, Me.LayerStart)
        ' Setting the last modified file date
        IO.File.SetLastWriteTime(Me.Reader.FilePath, Me.LayerStart)
    End Sub

    Public Sub WriteRTBWarning(_message As String, _color As Color)
        RaiseEvent WarningOccurred(_message, _color)
    End Sub

    Public Async Function CreateVideoFromDogTrack() As Task(Of Boolean)
        'Dim layerNodes, dogNodes As XmlNodeList
        Dim allTracks As New List(Of TrackAsTrkPts)
        For Each trkNode As XmlNode In Me.Reader.SelectNodes("trk")
            Dim TrackAsTrkptsList As XmlNodeList = Me.Reader.SelectAllChildNodes("trkpt", trkNode)

            Dim isMoving As Boolean = False 'defaultně pro ostatní trasy
            Dim trackColor As Color = Color.Green ' Default color for other tracks
            Dim label As String = Me.GetTrkType(trkNode).label
            Dim trkType As String = Me.GetTrkType(trkNode).typ
            If trkType.Trim().ToLower() = TrackTypes.Dog.Trim().ToLower() Then
                isMoving = True
                trackColor = Color.Red
            ElseIf trkType.Trim().ToLower() = TrackTypes.TrailLayer.Trim().ToLower() Then
                trackColor = Color.Blue
            End If
            TrackAsTrkptsList = Me.Reader.SelectAllChildNodes("trkpt", trkNode)
            allTracks.Add(New TrackAsTrkPts With {
                .Label = label,
                .Color = trackColor,
                .IsMoving = isMoving,
                .TrackPoints = TrackAsTrkptsList
                            })
        Next trkNode

        ' Create a video from the dog track and save it in the video directory
        ' Zjisti název souboru bez přípony
        Dim gpxName = System.IO.Path.GetFileNameWithoutExtension(Me.Reader.FilePath)
        ' Sestav cestu k novému adresáři
        If My.Settings.VideoDirectory = "" Then My.Settings.VideoDirectory = My.Settings.Directory
        Dim directory As New IO.DirectoryInfo(System.IO.Path.Combine(My.Settings.VideoDirectory, gpxName))
        ' Pokud adresář neexistuje, vytvoř ho
        If Not directory.Exists Then directory.Create()

        Dim _videoCreator As New OverlayVideoCreator(directory, WeatherData._windDirection, WeatherData._windSpeed)
        AddHandler _videoCreator.WarningOccurred, AddressOf WriteRTBWarning

        Dim waitForm As New frmPleaseWait()
        waitForm.Show()

        ' Spustíme na pozadí, aby nezamrzlo UI
        Await Task.Run(Async Function()
                           ' Spustíme tvůj dlouhý proces
                           Dim success = Await _videoCreator.CreateVideoFromTrkPts(allTracks, DescriptionParts, DescriptionPartsEng)

                           ' Po dokončení se vrať na UI thread a proveď akce
                           waitForm.Invoke(Sub()
                                               waitForm.Close()

                                               If success Then
                                                   Dim videopath As String = IO.Path.Combine(directory.FullName, "overlay.mov")
                                                   Dim bgPNGPath As String = IO.Path.Combine(directory.FullName, "background.png")
                                                   Dim form As New frmVideoDone(videopath, bgPNGPath)
                                                   form.ShowDialog()
                                                   form.Dispose()
                                               Else
                                                   MessageBox.Show("Vytvoření videa selhalo!", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                               End If
                                           End Sub)
                       End Function)

        '' Vytvoř video z trk bodů
        'If Await _videoCreator.CreateVideoFromTrkPts(allTracks, DescriptionParts) Then
        '    Dim videopath As String = IO.Path.Combine(directory.FullName, "overlay.mov")
        '    Dim bgPNGPath As String = IO.Path.Combine(directory.FullName, "background.png")
        '    Dim form As New frmVideoDone(videopath, bgPNGPath)
        '    form.ShowDialog()
        '    form.Dispose()
        '    Return True
        'End If

        Return False
    End Function

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

    Public Function GetAgeFromTime() As TimeSpan
        Dim ageFromTime As TimeSpan
        If Me.DogStart <> Date.MinValue AndAlso Me.LayerStart <> Date.MinValue Then
            Try
                ageFromTime = Me.DogStart - Me.LayerStart
            Catch ex As Exception
                ageFromTime = TimeSpan.Zero
            End Try
        End If
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

    End Function

    Public Sub WriteProcessed()
        Dim extensionsNode As XmlNode = Me.Reader.SelectSingleChildNode("extensions", Me.Reader.rootNode)
        If extensionsNode Is Nothing Then
            extensionsNode = Me.Reader.CreateElement("extensions")
            Me.Reader.rootNode.PrependChild(extensionsNode)
        End If

        Me.Reader.CreateAndAddElement(extensionsNode, "K9-Trails-Analyzer-processed", True, False)

    End Sub

    Public Function IsProcessed() As Boolean
        Dim extensionsNode As XmlNode = Me.Reader.SelectSingleChildNode("extensions", Me.Reader.rootNode)
        If extensionsNode Is Nothing Then Return False ' <extensions> vůbec neexistuje

        Dim processedNode As XmlNode = Me.Reader.SelectSingleChildNode("K9-Trails-Analyzer-processed", extensionsNode)
        If processedNode Is Nothing Then Return False ' neexistuje záznam


        Return (processedNode.InnerText.Trim().ToLower().Contains("true"))
    End Function

    ' Function to set the <desc> description from the first <trk> node in the GPX file
    Public Sub WriteDescription()

        If Not String.IsNullOrWhiteSpace(Me.Description) Then
            ' Find the first <trk> node and its <desc> subnode
            Dim trkNodes As XmlNodeList = Me.Reader.SelectNodes("trk")
            Dim trailLayerTrk As XmlNode = Nothing ' Inicializace proměnné pro <trk> s <type>TrailLayer</type>
            ' Najdeme <trk> s <type>TrailLayer</type>
            For Each trkNode As XmlNode In trkNodes
                Dim typeNodes As XmlNodeList = Me.Reader.SelectChildNodes("type", trkNode)
                For Each typeNode As XmlNode In typeNodes
                    ' Zkontrolujeme, zda <type> obsahuje "TrailLayer"
                    ' Pokud ano, uložíme tento <trk> do trailLayerTrk
                    ' a ukončíme cyklus
                    If typeNode IsNot Nothing AndAlso typeNode.InnerText.Trim().Equals(TrackTypes.TrailLayer, StringComparison.OrdinalIgnoreCase) Then
                        trailLayerTrk = trkNode
                        GoTo FoundTrailLayerTrk
                    End If
                Next
            Next

FoundTrailLayerTrk:
            Dim descNodeLayer As XmlNode = Me.Reader.SelectSingleChildNode("desc", trailLayerTrk)
            ' Pokud uzel <desc> neexistuje, vytvoříme jej a přidáme do <trk>
            If descNodeLayer Is Nothing Then
                descNodeLayer = Me.Reader.CreateElement("desc")
                If trailLayerTrk IsNot Nothing Then
                    ' Vytvoříme nový uzel <desc>
                    ' Přidání <desc> jako prvního potomka v uzlu <trk>
                    If trailLayerTrk.HasChildNodes Then
                        ' Vloží <desc> před první existující poduzel
                        trailLayerTrk.InsertBefore(descNodeLayer, trailLayerTrk.FirstChild)
                    Else
                        ' Pokud <trk> nemá žádné poduzly, použijeme AppendChild
                        trailLayerTrk.AppendChild(descNodeLayer)
                    End If
                End If
            End If


            ' Vytvoříme nový CDATA uzel
            Dim cdata As XmlCDataSection = Me.Reader.xmlDoc.CreateCDataSection(Me.Description)
            descNodeLayer.RemoveAll()
            ' Přidáme do descNode
            descNodeLayer.AppendChild(cdata)

        End If
    End Sub

    Public Function CalculateSpeed() As Double 'km/h
        If Not Me.DogStart = DateTime.MinValue AndAlso Not Me.DogFinish = DateTime.MinValue Then
            If (Me.DogFinish - Me.DogStart).TotalHours > 0 Then
                Return Me.Distance / (Me.DogFinish - Me.DogStart).TotalHours
            End If
        End If
        Return Nothing
    End Function


    Public Function GetAgeFromComments(inputText As String) As TimeSpan
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


    ' Function to read the time from the first <time> node in the GPX file
    ' If <time> node doesnt exist tries to read date from file name and creates <time> node
    Private _isGettingLayerStart As Boolean = False
    Public Sub RefreshLayerDogStartFinish()

        If _isGettingLayerStart Then
            Debug.WriteLine("Ochrana: GetLayerStart již běží.")
            Return
        End If
        _isGettingLayerStart = True

        Try
            Dim trkNodes As XmlNodeList = Me.Reader.SelectNodes("trk")
            Dim timeNode As XmlNode = Nothing
            Dim trkptNodes As XmlNodeList
            Dim startTimeNode As XmlNode = Nothing
            Dim finishTimeNode As XmlNode = Nothing
            Dim starttrkptNode As XmlNode = Nothing
            Dim finishtrkptNode As XmlNode = Nothing

            For Each trkNode As XmlNode In trkNodes


                ' Najdi první <trkseg>
                Dim trksegNode As XmlNode = Me.Reader.SelectSingleChildNode("trkseg", trkNode)
                If trksegNode Is Nothing Then Continue For

                trkptNodes = Me.Reader.SelectChildNodes("trkpt", trksegNode)
                starttrkptNode = trkptNodes(0) ' první trkpt v trkseg
                finishtrkptNode = trkptNodes(trkptNodes.Count - 1) ' poslední trkpt v trkseg
                If starttrkptNode Is Nothing Then Continue For

                startTimeNode = Me.Reader.SelectSingleChildNode("time", starttrkptNode)
                finishTimeNode = Me.Reader.SelectSingleChildNode("time", finishtrkptNode)

                If startTimeNode IsNot Nothing Then
                    ' Zjisti  typ tracku:
                    Dim typeNode As XmlNode = Me.Reader.SelectSingleChildNode("type", trkNode)
                    If typeNode?.InnerText.Trim().ToLower().Contains(TrackTypes.TrailLayer.Trim().ToLower()) Then
                        If startTimeNode Is Nothing OrElse Not DateTime.TryParse(startTimeNode.InnerText, LayerStart) Then Debug.WriteLine("Uzel <time> chybí nebo má neplatný formát.")
                    ElseIf typeNode?.InnerText.Trim().ToLower().Contains(TrackTypes.Dog.Trim().ToLower()) Then
                        If startTimeNode Is Nothing OrElse Not DateTime.TryParse(startTimeNode.InnerText, DogStart) Then Debug.WriteLine("Uzel <time> chybí nebo má neplatný formát.")
                        If finishTimeNode Is Nothing OrElse Not DateTime.TryParse(finishTimeNode.InnerText, DogFinish) Then Debug.WriteLine("Uzel <time> chybí nebo má neplatný formát.")
                    End If

                    Dim longitude As Double = Convert.ToDouble(starttrkptNode.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)
                    Dim Latitude As Double = Convert.ToDouble(starttrkptNode.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
                    Dim datum As DateTime = LayerStart
                    ' Pokud DogStart není nastaven, použijeme LayerStart
                    If DogStart <> DateTime.MinValue Then datum = DogStart
                    Dim loc As New Coordinates With {
                        .Lat = Latitude,
                        .Lon = longitude
                    }
                    trailStart = New TrackGeoPoint With {
                        .Location = loc,
                        .Time = datum
                    }

                End If
            Next


        Finally
            _isGettingLayerStart = False
        End Try
    End Sub




    ' Function to read and calculate the length of only the first segment from the GPX file
    Public Function CalculateLayerTrailDistance() As Double
        Dim totalLengthOfFirst_trkseg As Double = 0.0
        Dim lat1, lon1, lat2, lon2 As Double
        Dim firstPoint As Boolean = True

        ' Select the first track segment (<trkseg>) using the namespace
        Dim trkNodes As XmlNodeList = Me.Reader.SelectNodes("trk")
        Dim timeNode As XmlNode = Nothing

        For Each trkNode As XmlNode In trkNodes
            ' Zkontroluj, jestli tento <trk> má <desc>CrossTrail</desc>



            ' Zjisti  typ tracku:
            Dim typeNode As XmlNode = Me.Reader.SelectSingleChildNode("type", trkNode)
            If typeNode.InnerText.Trim().ToLower().Contains(TrackTypes.TrailLayer.Trim().ToLower()) Then

                Dim firstSegment As XmlNode = Me.Reader.SelectSingleChildNode("trkseg", trkNode)
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
                            RaiseEvent WarningOccurred("Error processing point: " & ex.Message & Environment.NewLine, Color.Red)
                        End Try
                    Next
                Else
                    RaiseEvent WarningOccurred("No tracks found in GPX file: " & Me.Reader.FileName & Environment.NewLine, Color.Red)
                End If
                Exit For ' ukončit - počítá se jen trailLayer
            End If
        Next
        Return totalLengthOfFirst_trkseg ' Result in kilometers
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
    Public Function ExtractDescriptionParts(originalDescription As String, ByRef goalPart As String, ByRef trailPart As String, ByRef dogPart As String) As Boolean
        ' 1️⃣ Odstraníme HTML tagy
        Dim cleanDescription As String = System.Text.RegularExpressions.Regex.Replace(originalDescription, "<.*?>", "").Trim()

        ' 2️⃣ Najdeme části pomocí regexu
        Dim pattern As String = "(?:(?:(?<goal>🎯|📍|g:)\s*(?<goalText>.*?))(?=(👣|t:|🐕|d:|🌡|$)))?" &
                            "(?:(?:(?<trail>👣|t:)\s*(?<trailText>.*?))(?=(📍|g:|🐕|d:|🌡|$)))?" &
                            "(?:(?:(?<dog>🐕|d:)\s*(?<dogText>.*?))(?=(📍|g:|👣|t:|🌡|$)))?"

        Dim regex As New Regex(pattern, RegexOptions.Singleline Or RegexOptions.IgnoreCase)
        Dim match As Match = regex.Match(cleanDescription)

        goalPart = ""
        trailPart = ""
        dogPart = ""

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
                trailPart = Regex.Replace(trailPart, "^[0-9\.,]+\s*h\s*", "", RegexOptions.IgnoreCase).Trim()
                trailPart = trailPart.Replace(My.Resources.Resource1.outAge.ToLower & ":", "") ' odstranění vícenásobných mezer
                trailPart = My.Resources.Resource1.outAge.ToLower & ": " & ageFromTime.TotalHours.ToString("F1") & " h, " & trailPart
            End If
            Dim LengthfromComments As Single = GetLengthFromComments(trailPart)
            If LengthfromComments = 0 Then
                ' Odebereme případnou starou délku z trailPart (např. "1.2 km něco")
                trailPart = Regex.Replace(trailPart, "^[0-9\.,]+\s*(km|m)\s*", "", RegexOptions.IgnoreCase).Trim()
                trailPart = trailPart.Replace(My.Resources.Resource1.outLength.ToLower & ":", "") ' odstranění vícenásobných mezer
                trailPart = My.Resources.Resource1.outLength.ToLower & ": " & Me.Distance.ToString("F1") & " km, " & trailPart
            End If

        Else
            If Me.Distance > 0 Then
                trailPart = My.Resources.Resource1.outLength.ToLower & ": " & Me.Distance.ToString("F1") & " km"
            End If
            trailPart &= ", " & My.Resources.Resource1.outAge.ToLower & ": " & ageFromTime.TotalHours.ToString("F1") & " h"

        End If
        Return True ' Vrátíme True, pokud se podařilo rozdělit popis
    End Function


    Private Async Function BuildDescription(goalPart As String, trailPart As String, dogPart As String, goalPartEng As String, trailPartEng As String, dogPartEng As String) As Task(Of String)
        Dim crlf As String = "<br>"
        Dim styleGreenBold As String = "<span style='color:darkgreen; font-weight:bold;'>"
        Dim styleBlueBold As String = "<span style='color:blue; font-weight:bold;'>"
        Dim styleRedBold As String = "<span style='color:red; font-weight:bold;'>"
        Dim styleMaroonBold As String = "<span style='color:maroon; font-weight:bold;'>"
        Dim styleend As String = "</span>"

        ' 🔧 Lokálně nastav labely 
        Dim goalLabel As String = "📍" 'My.Resources.Resource1.txtGoalLabel 'cíl
        Dim trailLabel As String = "👣" 'My.Resources.Resource1.txtTrailLabel '"Trail:"
        Dim dogLabel As String = "🐕" 'My.Resources.Resource1.txtDogLabel '"Pes:"

        ' 🌧🌦☀ Počasí
        'Wheather() 'získá počasí
        WeatherData = Await Wheather()
        Dim strWeather As String = $"🌡{WeatherData._temperature.ToString("0.#")} °C  💨 {WeatherData._windSpeed.ToString("0.#")} m/s {windDirectionToText(WeatherData._windDirection)} 💧{WeatherData._relHumidity} %   💧{WeatherData._precipitation} mm/h ⛅{WeatherData._cloudCover} %"


        ' 📦 Sestavíme nový popis pro video
        Me.DescriptionParts = New List(Of (Text As String, Color As Color, FontStyle As FontStyle)) From {
        ("🐩  " & My.Settings.DogName, Color.Maroon, FontStyle.Bold),
        (My.Resources.Resource1.txtGoalLabel & " " & goalPart, Color.DarkGreen, FontStyle.Regular),
        (trailLabel & " " & trailPart, Color.Blue, FontStyle.Regular),
        (dogLabel & " " & dogPart, Color.Red, FontStyle.Regular),
        (strWeather, Color.Maroon, FontStyle.Regular)}

        ' 📦 Sestavíme nový popis pro video
        Me.DescriptionPartsEng = New List(Of (Text As String, Color As Color, FontStyle As FontStyle)) From {
        ("🐩  " & My.Settings.DogName, Color.Maroon, FontStyle.Bold),
        (goalLabel & " " & goalPartEng, Color.DarkGreen, FontStyle.Regular),
        (trailLabel & " " & trailPartEng, Color.Blue, FontStyle.Regular),
        (dogLabel & " " & dogPartEng, Color.Red, FontStyle.Regular),
        (strWeather, Color.Maroon, FontStyle.Regular)}


        Dim sb As New System.Text.StringBuilder()
        If goalPart <> "" Then sb.Append(styleGreenBold & goalLabel & " " & goalPart & styleend & crlf)
        sb.Append(styleBlueBold & trailLabel & " " & trailPart & styleend & crlf)
        If dogPart <> "" Then sb.Append(styleRedBold & dogLabel & " " & dogPart & styleend & crlf)

        If WeatherData._temperature.ToString = "100" Then Return sb.ToString().Trim()

        sb.Append(styleMaroonBold & strWeather & styleend)

        Return sb.ToString().Trim()
    End Function


    '' Funkce pro sestavení popisu ze všech <trk> uzlů
    Public Async Function BuildSummaryDescription(Process As Boolean) As Task(Of String)
        Dim trailDesc As String = ""
        Dim dogDesc As String = ""
        Dim crossDescs As New List(Of String)
        Dim goalDesc As String = ""

        Dim SummaryDescription As String = ""

        For Each trkNode As XmlNode In Me.Reader.SelectNodes("trk")
            Dim typeNode As XmlNode = Me.Reader.SelectSingleChildNode("type", trkNode)
            Dim descNode As XmlNode = Me.Reader.SelectSingleChildNode("desc", trkNode)

            SummaryDescription &= descNode?.InnerText.Trim() & " " ' Získání textu z <desc> uzlu, pokud existuje
        Next

        'když už byl file v minulosti zpracován, tak se dál nemusí pokračovat, dialog by byl zbytečný
        If Process Or Not Me.IsAlreadyProcessed Then

            Dim goalPart As String = "", trailPart As String = "", dogPart As String = ""
            ExtractDescriptionParts(SummaryDescription, goalPart, trailPart, dogPart)
            ' Otevřeš okno k editaci:
            Dim frm As New frmEditComments With {
                           .GoalPart = goalPart,
                           .TrailPart = trailPart,
                           .DogPart = dogPart,
                           .GpxFileName = Me.Reader.FileName
                            }
            Dim newDescription As String = ""
            If frm.ShowDialog() = DialogResult.OK Then
                newDescription = Await BuildDescription(frm.GoalPart, frm.TrailPart, frm.DogPart, frm.GoalPartEng, frm.TrailPartEng, frm.DogPartEng)
                ' ... tady nový popis použiješ
            End If
            Return newDescription.ToString().Trim()
        Else
            Return SummaryDescription.ToString().Trim()
        End If

    End Function


    '' Funkce pro sloučení názvů souborů z dvou GPX záznamů
    Private Function MergeFileNames(record1 As GPXRecord, record2 As GPXRecord) As String

        ' Extrakce jmen 
        Dim names1 As New List(Of String)(record1.Reader.FileName.Split({"_", "."}, StringSplitOptions.RemoveEmptyEntries))
        Dim names2 As New List(Of String)(record2.Reader.FileName.Split({"_", "."}, StringSplitOptions.RemoveEmptyEntries))

        ' Odstranění čísel
        names1.RemoveAll(Function(s) Regex.IsMatch(s, "^\d+$")) ' Odstraní prvky, které obsahují pouze čísla
        names2.RemoveAll(Function(s) Regex.IsMatch(s, "^\d+$")) ' Odstraní prvky, které obsahují pouze čísla

        'odstraní čísla ze všech prvků
        names1 = names1.Select(Function(s) Regex.Replace(s, "[\d+]", "")).ToList()
        names2 = names2.Select(Function(s) Regex.Replace(s, "[\d+]", "")).ToList()

        'odstraní podtržítka, tečky, pomlčky
        names1 = names1.Select(Function(s) Regex.Replace(s, "[-._]", "")).ToList()
        names2 = names2.Select(Function(s) Regex.Replace(s, "[-._]", "")).ToList()

        ' Odstranění gpx
        names1.RemoveAll(Function(s) Regex.IsMatch(s, "gpx")) ' 
        names2.RemoveAll(Function(s) Regex.IsMatch(s, "gpx")) ' 

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
        Dim newName As String = $"{String.Join("_", finalnames)}.gpx"

        Return newName
    End Function

    '' Funkce pro sloučení dvou GPX záznamů (např. trasy layer a psa)
    Public Function MergeDogToMe(dog As GPXRecord) As Boolean

        Dim newName = MergeFileNames(Me, dog)
        'do souboru Me vloží kompletní uzel  <trk> vyjmutý ze souboru dog
        Try
            ' Najdi první uzel <trk>
            'Dim meTrkNode As XmlNode = Me.Reader.SelectSingleNode("trk")
            Dim dogTrkNodes As XmlNodeList = dog.Reader.SelectNodes("trk")

            For Each dogTrkNode In dogTrkNodes
                Dim importedNode As XmlNode = Me.Reader.ImportNode(dogTrkNode, True) ' Důležité: Import uzlu!
                Dim meGpxNode As XmlNode = Me.Reader.SelectSingleNode("gpx")
                meGpxNode.AppendChild(importedNode) ' Přidání na konec <gpx>
            Next
            Me.SortTracksByTime() 'seřadí trk podle stáří od nejstaršího (layer?) po nejmladší (dog)
            Me.RefreshLayerDogStartFinish() ' načte časy startů

            'spojené trasy se uloží do souboru kladeče
            'když je nové jméno stejné jako jméno kladeče nepřejmenovává se
            If Me.Reader.FileName = newName OrElse RenameFile(newName) Then
                Me.Save()
                IO.File.Delete(dog.Reader.FilePath) 'pes se smaže
                RaiseEvent WarningOccurred($"Tracks in files {Me.Reader.FileName} and {dog.Reader.FileName} were successfully merged in file {Me.Reader.FileName} {vbCrLf}File {dog.Reader.FileName}  was deleted.{vbCrLf}", Color.DarkGreen)
            End If


            Return True
        Catch ex As Exception
            RaiseEvent WarningOccurred($"Merging tracks of the me  {Me.Reader.FileName} and the dog {dog.Reader.FileName} failed!" & vbCrLf & ex.Message, Color.Red)
            Return False
        End Try

    End Function

    Public Sub SplitSegmentsIntoTracks()
        Dim trkNodes As XmlNodeList = Me.Reader.SelectNodes("trk")

        For Each trkNode As XmlNode In trkNodes
            Dim trkSegNodes As XmlNodeList = Me.Reader.SelectChildNodes("trkseg", trkNode)

            If trkSegNodes.Count > 1 Then
                For i As Integer = 1 To trkSegNodes.Count - 1
                    Dim trkSeg As XmlNode = trkSegNodes(i)
                    Dim segDesc As XmlNode = Me.Reader.SelectSingleChildNode("desc", trkSeg)
                    Dim newTrk As XmlNode = Me.Reader.CreateElement("trk")
                    If segDesc IsNot Nothing Then
                        trkSeg.RemoveChild(segDesc) 'případná desc přemístí ze segmentu do trk
                        newTrk.AppendChild(segDesc)
                    End If

                    trkNode.RemoveChild(trkSeg)
                    newTrk.AppendChild(trkSeg)

                    trkNode.ParentNode.InsertAfter(newTrk, trkNode)
                Next
            End If
        Next

        'Me.Save()
        RaiseEvent WarningOccurred($"Tracks in file {Me.Reader.FileName} were split.", Color.DarkGreen)
    End Sub

    Public Sub SortTracksByTime()
        Dim trkNodes As XmlNodeList = Me.Reader.SelectNodes("trk")
        Dim parentNode As XmlNode = trkNodes(0)?.ParentNode
        If parentNode Is Nothing Then Exit Sub

        ' Seznam tuple (trkNode, čas)
        Dim trkList As New List(Of Tuple(Of XmlNode, DateTime, String))()


        For i As Integer = 0 To trkNodes.Count - 1
            Dim trkNode As XmlNode = trkNodes(i)
            'najde první <trkseg> v něm první <trkpt> a v něm načte <time>
            Dim trkseg As XmlNode = Me.Reader.SelectSingleChildNode("trkseg", trkNode)
            Dim trkpt As XmlNode = Me.Reader.SelectSingleChildNode("trkpt", trkseg)
            Dim timeNode As XmlNode = Me.Reader.SelectSingleChildNode("time", trkpt)

            Dim dt As DateTime = DateTime.MinValue
            If timeNode IsNot Nothing Then
                DateTime.TryParse(timeNode.InnerText, dt)
            End If
            ' Zjisti  typ tracku:
            Dim trkType As String = Me.Reader.SelectSingleChildNode("type", trkNode)?.InnerText
            If trkType Is Nothing Then
                trkType = "?" ' pokud není typ, nastavíme na Unknown
            End If
            trkList.Add(Tuple.Create(trkNode, dt, trkType))

        Next

        ' Seřadit podle času
        trkList.Sort(Function(a, b) a.Item2.CompareTo(b.Item2))

        ' Odebrat staré <trk>
        For Each trk In trkNodes
            parentNode.RemoveChild(trk)
        Next

        ' --- Doplnění <type> ---
        If trkList.Count = 1 Then
            AddTypeToTrk(trkList(0).Item1, TrackTypes.TrailLayer)
            trkList(0) = Tuple.Create(trkList(0).Item1, trkList(0).Item2, TrackTypes.TrailLayer) ' aktualizace typu
        ElseIf trkList.Count = 2 Then
            AddTypeToTrk(trkList(0).Item1, TrackTypes.TrailLayer)
            trkList(0) = Tuple.Create(trkList(0).Item1, trkList(0).Item2, TrackTypes.TrailLayer) ' aktualizace typu
            AddTypeToTrk(trkList(1).Item1, TrackTypes.Dog)
            trkList(1) = Tuple.Create(trkList(1).Item1, trkList(1).Item2, TrackTypes.Dog) ' aktualizace typu
        ElseIf trkList.Count > 2 Then
            trkList(trkNodes.Count - 1) = Tuple.Create(trkList(trkNodes.Count - 1).Item1, trkList(trkNodes.Count - 1).Item2, TrackTypes.Dog) ' aktualizace typu (poslední je dog)

            ' Zde volání nějaké funkce, která vrátí indexy CrossTrail trků
            Dim crossTrailIndices As New List(Of Integer)
            crossTrailIndices = AskUserWhichTracksAreCrossTrail(trkList)
            Dim layerFound As Boolean = False
            Dim dogFound As Boolean = False
            For i As Integer = 0 To trkList.Count - 1
                Dim trk = trkList(i).Item1
                If crossTrailIndices.Contains(i) Then
                    AddTypeToTrk(trk, TrackTypes.CrossTrack)
                ElseIf Not layerFound Then
                    AddTypeToTrk(trk, TrackTypes.TrailLayer)
                    layerFound = True
                ElseIf Not dogFound Then
                    AddTypeToTrk(trk, TrackTypes.Dog)
                    dogFound = True
                Else
                    RaiseEvent WarningOccurred($"Tracks in file {Me.Reader.FileName} were not identified properly.", Color.Red)
                End If
            Next
        End If

        ' Přidat zpět ve správném pořadí
        For Each t In trkList
            parentNode.AppendChild(t.Item1)
        Next

        'Me.Save()
        RaiseEvent WarningOccurred($"Tracks in file {Me.Reader.FileName} were sorted and typed.", Color.DarkGreen)
    End Sub

    Private Function AddTypeToTrk(trkNode As XmlNode, trackTypeValue As String) As Boolean
        ' Zkontroluj, jestli už <type> existuje
        Dim existingTypes As XmlNodeList = Me.Reader.SelectAllChildNodes("type", trkNode)
        Dim isTheSameType As Boolean = False
        For Each existingtype As XmlNode In existingTypes
            Select Case existingtype.InnerText.Trim().ToLower()
                Case trackTypeValue.Trim().ToLower()
                    ' pokud už existuje
                    isTheSameType = True
                    trkNode.RemoveChild(existingtype)
                Case TrackTypes.TrailLayer.Trim().ToLower(), TrackTypes.Dog.Trim().ToLower(), TrackTypes.CrossTrack.Trim().ToLower()
                    ' všechny starší zápisy smazat
                    trkNode.RemoveChild(existingtype)
                Case Else
                    ' záznam jiných aplikací nemazat
            End Select
        Next

        Me.Reader.CreateAndAddElement(trkNode, "type", trackTypeValue, False)
        Return isTheSameType
    End Function

    Private Function GetTrkType(trkNode As XmlNode) As (typ As String, label As String)
        ' Zkontroluj, jestli už <type> existuje
        Dim existingTypes As XmlNodeList = Me.Reader.SelectAllChildNodes("type", trkNode)
        Dim isTheSameType As Boolean = False
        For Each existingtype As XmlNode In existingTypes
            Select Case existingtype.InnerText.Trim().ToLower()
                Case TrackTypes.TrailLayer.Trim().ToLower()
                    ' vrátí první nalezený typ
                    Dim _typ As String = existingtype.InnerText.Trim().ToLower()
                    Dim _label As String = My.Resources.Resource1.txtTrailLayer
                    Return (_typ, _label)
                Case TrackTypes.Dog.Trim().ToLower()
                    Dim _typ As String = existingtype.InnerText.Trim().ToLower()
                    Dim _label As String = My.Resources.Resource1.txtDogLabel
                    Return (_typ, _label)
                Case TrackTypes.CrossTrack.Trim().ToLower()
                    Dim _typ As String = existingtype.InnerText.Trim().ToLower()
                    Dim _label As String = My.Resources.Resource1.txtCrossTrack
                    Return (_typ, _label)
                Case Else
                    ' záznam jiných aplikací nemazat
            End Select
        Next

        'když nic nenajde, vrátí Unknown
        Return Nothing
    End Function

    Private Function AskUserWhichTracksAreCrossTrail(trkList As List(Of Tuple(Of XmlNode, DateTime, String))) As List(Of Integer)
        ' Připrav popis pro každý trk – můžeš doplnit třeba čas
        Dim descriptionsList As New List(Of String)()
        Dim trackTypesList As New List(Of String)
        For i As Integer = 1 To trkList.Count
            descriptionsList.Add($"Track No. {i} Start: {trkList(i - 1).Item2.ToLocalTime} Type: {trkList(i - 1)?.Item3.ToLower}? ")
            trackTypesList.Add(trkList(i - 1)?.Item3.ToLower)
        Next

        Using dlg As New CrossTrailSelector(descriptionsList, trackTypesList, Me.Reader.FileName)
            If dlg.ShowDialog = DialogResult.OK Then
                Return dlg.CrossTrailIndices
            End If
        End Using

        Return New List(Of Integer)() ' pokud user zavře bez výběru
    End Function


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


    Public Function Backup() As Boolean
        ' Vytvoření kompletní cílové cesty
        Dim backupFilePath As String = Path.Combine(BackupDirectory, Me.Reader.FileName)

        If Not IO.File.Exists(backupFilePath) Then
            ' Kopírování souboru
            Try
                IO.File.Copy(Me.Reader.FilePath, backupFilePath, False)
                Return True
            Catch ex As Exception
                ' Zpracování jakýchkoli neočekávaných chyb
                Debug.WriteLine($"Chyba při kopírování souboru {Reader.FileName}: {ex.Message}")
                Return False
            End Try
        Else
            ' Soubor již existuje, přeskočíme
            Debug.WriteLine($"Soubor {Reader.FileName} již existuje, přeskočeno.")
            Return False
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



    Public Sub PrependDateToFilename()
        Dim newNameWithDate As String = GenerateNewFileNameWithDate()
        If Me.Reader.FileName <> newNameWithDate Then RenameFile(newNameWithDate)
    End Sub


    ' Funkce pro vytvoření nového jména souboru
    Public Function GenerateNewFileNameWithDate() As String
        'Dim fileExtension As String = Path.GetExtension(Reader.FilePath)
        'Dim fileNameOhneExt As String = Path.GetFileNameWithoutExtension(Reader.FilePath)
        Dim newFileName As String = Reader.FileName

        Try
            ' Smaže datum v názvu souboru (to kvůli převodu na iso formát):
            Dim result As (DateTime?, String) = GetRemoveDateFromName()
            Dim modifiedFileName As String = result.Item2
            newFileName = $"{LayerStart:yyyy-MM-dd} {modifiedFileName}"
        Catch ex As Exception
            Debug.WriteLine(ex.ToString())
            'ponechá původní jméno, ale přidá datum
            newFileName = $"{LayerStart:yyyy-MM-dd} {Reader.FileName}"
        End Try

        Return newFileName
    End Function

    ' Funkce pro přejmenování souboru
    Public Function RenameFile(newFileName As String) As Boolean
        Dim newFilePath As String = Path.Combine(gpxDirectory, newFileName)
        Dim extension As String = Path.GetExtension(newFileName)

        Try
            'neptá se přejmenuje automaticky
            Dim romanNumeralIndex As Integer = 1
            While IO.File.Exists(newFilePath)
                Dim nameWithoutExtension As String = Path.GetFileNameWithoutExtension(newFilePath)
                Dim romanNumeral As String = ToRoman(romanNumeralIndex)
                newFileName = $"{nameWithoutExtension}_{romanNumeral}{extension}"
                romanNumeralIndex += 1
                newFilePath = Path.Combine(gpxDirectory, newFileName)
            End While

            IO.File.Move(Reader.FilePath, newFilePath)

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
        Dim datum As String = $"{trailStart.Time:yyyy-MM-dd}"
        Dim url As String = $"https://api.open-meteo.com/v1/forecast?latitude={trailStart.Location.Lat.ToString(CultureInfo.InvariantCulture)}&longitude={trailStart.Location.Lon.ToString(CultureInfo.InvariantCulture)}&start_date={datum}&end_date={datum}&hourly=temperature_2m,wind_speed_10m,soil_temperature_0cm,wind_direction_10m,relative_humidity_2m,cloud_cover,precipitation&wind_speed_unit=ms"

        If trailStart.Time < Today.AddDays(-6) Then
            'po šesti dnech jsou k dispozici historická data z archivu
            url = $"https://archive-api.open-meteo.com/v1/archive?latitude={trailStart.Location.Lat.ToString(CultureInfo.InvariantCulture)}&longitude={trailStart.Location.Lon.ToString(CultureInfo.InvariantCulture)}&start_date={datum}&end_date={datum}&hourly=temperature_2m,wind_speed_10m,soil_temperature_0_to_7cm,wind_direction_10m,relative_humidity_2m,cloud_cover,precipitation&wind_speed_unit=ms"
        End If


        Dim response As HttpResponseMessage = Await client.GetAsync(url)
        Dim content As String = Await response.Content.ReadAsStringAsync()
        If Not response.IsSuccessStatusCode Then
            RaiseEvent WarningOccurred($"Failed to fetch weather data: {response.ReasonPhrase}", Color.Red)
            Return (100, 0, 0, 0, 0, 0)
        End If
        Dim json As JsonDocument = JsonDocument.Parse(content)

        ' Získej kořenový element
        Dim root = json.RootElement

        ' Najdi pole časů
        Dim times = root.GetProperty("hourly").GetProperty("time")

        Dim localTime As DateTime = LayerStart ' ten načtený čas
        Dim utcTime As DateTime = trailStart.Time.ToUniversalTime()
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
                Return (100, 0, 0, 0, 0, 0)
            End Try
        End If

        Return (100, 0, 0, 0, 0, 0)
    End Function

    'Function WindDirectionToText(smer As Double) As String
    '    Dim strany = {"N", "NE", "E", "SE", "S", "SW", "W", "NW"}
    '    ' Každý díl má 22.5°
    '    Dim index As Integer = CInt((smer + 22.5) \ 45) Mod 8
    '    Return strany(index)
    'End Function
    Function windDirectionToText(direction As Double) As String
        Dim windDir = {
    "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE",
    "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW"
            }
        ' Každý díl má 22.5°
        Dim index As Integer = CInt((direction + 11.25) \ 22.5) Mod 16
        Return windDir(index)
    End Function


End Class



Public Class GpxReader
    Public xmlDoc As XmlDocument
    Public namespaceManager As XmlNamespaceManager
    Public Property FilePath As String
    Private namespacePrefix As String

    Public ReadOnly Property FileName As String
        Get
            Return Path.GetFileName(FilePath)
        End Get
    End Property

    Public Property Nodes As XmlNodeList
    Public Property rootNode As XmlNode
    Public Property namespaceUri As String


    ' Konstruktor načte XML dokument a nastaví XmlNamespaceManager
    Public Sub New(_filePath As String)
        Try
            xmlDoc = New XmlDocument()
            xmlDoc.Load(_filePath)
            FilePath = _filePath

            ' Zjištění namespace, pokud je definován
            rootNode = xmlDoc.DocumentElement
            namespaceUri = rootNode.NamespaceURI
            ' Inicializace XmlNamespaceManager s dynamicky zjištěným namespace
            namespaceManager = New XmlNamespaceManager(xmlDoc.NameTable)


            If Not String.IsNullOrEmpty(namespaceUri) Then
                namespaceManager.AddNamespace("gpx", namespaceUri) ' Použijeme lokální prefix "gpx"
                namespacePrefix = "gpx:"
                namespaceManager.AddNamespace("opentracks", "http://opentracksapp.com/xmlschemas/v1")
                namespaceManager.AddNamespace("gpxtpx", "http://www.garmin.com/xmlschemas/TrackPointExtension/v2")
                namespaceManager.AddNamespace("gpxtrkx", "http://www.garmin.com/xmlschemas/TrackStatsExtension/v1")
            Else
                namespaceManager.AddNamespace("", namespaceUri) ' nepoužijeme lokální prefix "gpx"
                namespacePrefix = ""
            End If
        Catch ex As FileNotFoundException
            ' Soubor nebyl nalezen
            Throw New ArgumentException($"File '{FileName}' has not been found.", ex) ' Vytvořit novou výjimku s kontextem
        Catch ex As XmlException
            ' Chyba v XML formátu
            Throw New XmlException($"Error in XML '{FileName}': {ex.Message}", ex) ' Vytvořit novou výjimku s kontextem
        Catch ex As Exception
            ' Obecná chyba
            Throw New Exception($"Error loading file '{FileName}': {ex.Message}", ex) ' Vytvořit novou výjimku s kontextem
        End Try
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
        Try
            Return xmlDoc.SelectSingleNode("//" & namespacePrefix & nodename, namespaceManager)
        Catch ex As Exception
            Return Nothing
        End Try

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
        Return xmlDoc.CreateElement(nodename, xmlDoc.DocumentElement.NamespaceURI)
    End Function

    Public Sub CreateAndAddElement(parentNode As XmlElement, childNodeName As String, value As String, insertAfter As Boolean)
        Dim childNode As XmlElement = CreateElement(childNodeName)
        childNode.InnerText = value

        Dim childNodes As XmlNodeList = Me.SelectAllChildNodes(childNodeName, parentNode)
        ' Kontrola, zda existuje alespoň jeden uzel <childnodename>
        If childNodes.Count = 0 Then
            ' Uzel <childnodename> neexistuje, můžeme ho vytvořit a vložit
            ' Pokud parent nemá žádné podřízené uzly, jednoduše ho přidáme jako první 
            parentNode.InsertBefore(childNode, parentNode.FirstChild)

        Else
            'kontrola duplicity:
            For Each node As XmlNode In childNodes
                'zkontroluje zda node s hodnotou 'value' už neexistuje:
                If node.InnerText.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0 Then
                    Debug.WriteLine($"Uzel {childNodeName} s textem {value} již existuje, nepřidávám nový.")
                Else
                    Debug.WriteLine($"Uzel {childNodeName} s jiným textem již existuje, přidávám nový.")
                    If insertAfter Then
                        parentNode.AppendChild(childNode)
                    Else
                        'vložíme nový uzel PŘED prvního podřízeného
                        parentNode.InsertBefore(childNode, childNodes(0))
                    End If
                End If
            Next
        End If

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

