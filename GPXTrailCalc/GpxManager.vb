Imports System.Diagnostics.Eventing
Imports System.Globalization
Imports System.IO
Imports System.Net.WebRequestMethods
Imports System.Runtime.CompilerServices.RuntimeHelpers
Imports System.Text.RegularExpressions
Imports System.Windows.Controls
Imports System.Windows.Forms.LinkLabel
Imports System.Xml
Imports GPXTrailAnalyzer.My.Resources
Imports Microsoft.VisualBasic.Logging
Imports System.Windows.Forms

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
                If trimGPS_Noise Then _gpxRecord.TrimGPSnoise(12) 'ořízne nevýznamné konce a začátky trailů, když se stojí na místě.
                _gpxRecord.SplitTrackIntoTwo() 'in gpx files, splits a track with two segments into two separate tracks
                _gpxRecord.Description = _gpxRecord.GetDescription() 'musí být první - slouží k výpočtu age
                'If trimGPS_Noise Then _gpxRecord.SmoothTrail()
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
        Dim backup As Boolean = False
        Try
            For Each gpxFilePath In gpxFilesAllPath
                Try
                    'Tady najde layerStart 
                    Dim _reader As New GpxReader(gpxFilePath)
                    Dim trksegNodes As XmlNodeList = _reader.SelectNodes("trkseg")
                    Dim trkNodes As XmlNodeList = _reader.SelectNodes("trk")

                    ' Zjisti, zda  soubor obsahuje max. dva  uzly <trkseg> nebo <trk>
                    If trksegNodes.Count <= 2 AndAlso trkNodes.Count <= 2 Then
                        Dim _gpxRecord As New GPXRecord With {.Reader = _reader}

                        If _gpxRecord.LayerStart >= dateFrom And _gpxRecord.LayerStart <= dateTo Then

                            AddHandler _gpxRecord.WarningOccurred, AddressOf _writeRTBWarning
                            Dim _backup As Boolean = _gpxRecord.Backup()
                            'kvůli výpisu, pokud se žádný soubor nezazálohuje, výpis se nedělá:
                            If Not backup Then backup = _backup
                            gpxFilesWithinInterval.Add(_gpxRecord)

                        End If
                    Else 'more than 2 segmets ot tracks
                        RaiseEvent WarningOccurred($"Trail record {Path.GetFileName(gpxFilePath)}
is invalid, there are more than two tracks recorded! Not included in the stats.", Color.Red)
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


    Public Sub _writeRTBWarning(_message As String, _color As Color)
        RaiseEvent WarningOccurred(_message, _color)
    End Sub


    'Public Function MergeLayerAndDogOld(_gpxRecords As List(Of GPXRecord)) As List(Of GPXRecord)
    '    'vytváříme list gpx souborů gpxFilesMerged, první vložíme,
    '    'orvní náslehující prohlídneme, zda se neliší o méně než maxAge - ty je pak možné spojit,
    '    'protože se nejspíš jedná o záznam stopy kladeč a trasy psa, jen je každá zvlášť
    '    'pokud se první následující liší o více než maxAge nebo se ke spojení nedojde z jiného důvodu
    '    ''(např. z rozhodnutí uživatele), tak se gpx přidá do listu, pokud ke spojení dojde tak se přidává,
    '    'ale smaže se.

    '    Dim gpxFilesMerged As New List(Of GPXRecord) From {_gpxRecords(0)}
    '    Dim lastAddedIndex As Integer = 0
    '    If _gpxRecords.Count = 0 Then Return gpxFilesMerged 'ošetření prázdného listu


    '    For i = 1 To _gpxRecords.Count - 1
    '        If Not TryMerge(_gpxRecords(i), _gpxRecords(lastAddedIndex)) Then
    '            ' Přidání souboru do seznamu, pokud ke spojení nedošlo
    '            gpxFilesMerged.Add(_gpxRecords(i))
    '            lastAddedIndex = i
    '        End If
    '    Next i
    '    Return gpxFilesMerged
    'End Function

    Public Function MergeLayerAndDog(_gpxRecords As List(Of GPXRecord)) As List(Of GPXRecord)
        Dim gpxFilesMerged As New List(Of GPXRecord)
        If _gpxRecords.Count = 0 Then Return gpxFilesMerged 'ošetření prázdného listu

        Dim usedIndexes As New List(Of Integer) ' Seznam indexů, které už byly použity pro spojení

        For i As Integer = 0 To _gpxRecords.Count - 1
            If usedIndexes.Contains(i) Then Continue For ' Přeskočíme již spojené prvky

            gpxFilesMerged.Add(_gpxRecords(i)) ' Přidáme aktuální prvek do merged listu
            Dim lastAddedIndex As Integer = gpxFilesMerged.Count - 1 ' Index posledního přidaného prvku

            ' Vnitřní cyklus se pokouší spojit POSLEDNĚ PŘIDANÝ prvek s NÁSLEDUJÍCÍMI
            Dim j As Integer = i + 1
            While j < _gpxRecords.Count
                If Not usedIndexes.Contains(j) AndAlso (_gpxRecords(j).LayerStart - _gpxRecords(lastAddedIndex).LayerStart <= maxAge) Then
                    If TryMerge(_gpxRecords(j), gpxFilesMerged(lastAddedIndex)) Then
                        ' Spojení proběhlo úspěšně. Aktualizujeme spojený prvek v gpxFilesMerged.
                        'gpxFilesMerged(lastAddedIndex) = _gpxRecords(j)
                        ' Přidáme index j do seznamu použitých indexů
                        usedIndexes.Add(j)
                        ' Ukončíme vnitřní cyklus, protože jsme našli spárování
                        Exit While
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

        Dim mergeDecision As String = LoadMergeDecision(file_prev, file_i)
        If Not mergeCancel AndAlso (Not mergeDecision = System.Windows.Forms.DialogResult.No.ToString) Then

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
                ElseIf result = DialogResult.No Then
                    SaveMergeDecision(file_prev, file_i, result.ToString)
                End If
            End If
            ' Zjisti, zda oba soubory obsahují pouze jeden uzel <trkseg>
            Dim trksegNodes_i As XmlNodeList = file_i.Reader.SelectNodes("trkseg")
            Dim trksegNodes_prev As XmlNodeList = file_prev.Reader.SelectNodes("trkseg")
            If trksegNodes_i.Count = 1 AndAlso trksegNodes_prev.Count = 1 Then
                ' Zeptej se uživatele, zda chce soubory spojit
                Dim mergeFiles As DialogResult
                If Not mergeNoAsk Then mergeFiles = DialogMergeFiles(file_prev, file_i)
                ' Pokud uživatel souhlasí, spoj soubory, jinak přidej
                If mergeNoAsk OrElse (mergeFiles = DialogResult.Yes) Then
                    If file_prev.MergeDogToMe(file_i) Then
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
        lblSoubor1.Text = $"{My.Resources.Resource1.lblIsThisLayerQ}: '{Path.GetFileName(runner.Reader.FilePath)}' ?"
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

        ' Zaškrtávací políčko pro zapamatování rozhodnutí
        Dim chbRemembDecision As New System.Windows.Forms.CheckBox
        chbRemembDecision.Text = My.Resources.Resource1.chbRemembDecisQ '"Zapamatovat rozhodnutí 'Ne' pro tuto dvojici"
        chbRemembDecision.Location = New Point(10, lblSoubor2.Bottom + 10)
        chbRemembDecision.AutoSize = True
        chbRemembDecision.Checked = True
        dialog.Controls.Add(chbRemembDecision)

        ' Tlačítka
        Dim btnAno As New System.Windows.Forms.Button()
        btnAno.Text = "Yes"
        btnAno.DialogResult = DialogResult.Yes ' Nastavení výsledku dialogu
        btnAno.Location = New Point(10, chbRemembDecision.Bottom + 15)
        btnAno.AutoSize = True
        dialog.Controls.Add(btnAno)

        Dim btnNe As New System.Windows.Forms.Button()
        btnNe.Text = "No"
        btnNe.DialogResult = DialogResult.No
        btnNe.Location = New Point(btnAno.Right + 10, chbRemembDecision.Bottom + 15)
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

        ' Uložení stavu zaškrtávacího políčka  do nastavení aplikace
        If result = DialogResult.No AndAlso chbRemembDecision.Checked Then
            SaveMergeDecision(runner, dog, result.ToString)
        End If

        mergeNoAsk = rbNoAsk.Checked ' Uložení stavu pro použití v hlavní funkci
        mergeCancel = rbCancel.Checked
        Return result
    End Function



    Private Sub SaveMergeDecision(runner As GPXRecord, dog As GPXRecord, result As String)
        Dim key As String = $"{runner.Reader.FilePath}|{dog.Reader.FilePath}" ' Vytvoření klíče
        Dim settings As System.Collections.Specialized.StringCollection = My.Settings.MergeDecisions
        ' Inicializace kolekce, pokud je Nothing
        If settings Is Nothing Then
            settings = New System.Collections.Specialized.StringCollection()
            My.Settings.MergeDecisions = settings ' Důležité: Uložení inicializované kolekce zpět do nastavení!
        End If
        ' Odstranění starého rozhodnutí, pokud existuje

        Dim existingIndex As Integer = -1
        For i As Integer = 0 To settings.Count - 1
            If settings(i).StartsWith($"{runner.Reader.FilePath}|{dog.Reader.FilePath}|") Then
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
        Dim key As String = $"{runner.Reader.FilePath}|{dog.Reader.FilePath}"
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




    Private Sub TestKombinaceNazvu()
        'Dim nazev1 As String = "Fany_runner_2024-12-14_10-08.gpx"
        'Dim nazev2 As String = "Peggy_dog_24-12-14_11-49.gpx"

        'Try
        '    Dim novyNazev As String = ZkombinujNazvySouboru(nazev1, nazev2)
        '    MessageBox.Show($"Nový název: {novyNazev}") ' Zobrazí: 2024_12_14_Fany_runner_Peggy_dog.gpx

        '    nazev1 = "Karel_2024.12.15.txt"
        '    nazev2 = "Pepa_24.12.15.txt"
        '    novyNazev = ZkombinujNazvySouboru(nazev1, nazev2)
        '    MessageBox.Show($"Nový název: {novyNazev}") ' Zobrazí: 2024_12_15_Karel_Pepa.txt

        '    nazev1 = "Karel_2024.12.15_beh.txt"
        '    nazev2 = "Karel_2024.12.15_chůze.txt"
        '    novyNazev = ZkombinujNazvySouboru(nazev1, nazev2)
        '    MessageBox.Show($"Nový název: {novyNazev}") ' Zobrazí: 2024_12_15_beh_chůze.txt

        '    nazev1 = "2025-01-05T15_20+01.gpx"
        '    nazev2 = "2025-01-05_T07_36+01.gpx"
        '    novyNazev = ZkombinujNazvySouboru(nazev1, nazev2)
        '    MessageBox.Show($"Nový název: {novyNazev}") ' Zobrazí: 2024_12_15_beh_chůze.txt

        'Catch ex As Exception
        '    MessageBox.Show($"Neočekávaná chyba: {ex.Message}")
        'End Try

    End Sub


End Class

' Struktura pro trackpoint
Public Class TrackPoint
    Public Property Latitude As Double
    Public Property Longitude As Double
    Public Property Accuracy As Double
    Public Property Time As Double
    Public Property Elevation As Double

    Public Sub New(lat As Double, lon As Double, acc As Double, tim As Double, ele As Double)
        Latitude = lat
        Longitude = lon
        Accuracy = acc
        Time = tim
        Elevation = ele
    End Sub
    Public Sub New(lat As Double, lon As Double, acc As Double)
        Latitude = lat
        Longitude = lon
        Accuracy = acc

    End Sub
End Class


Public Class TrackTypes
    Public Const Dog As String = "Dog"
    Public Const TrailLayer As String = "TrailLayer"
End Class




Public Class GPXRecord
    Private _LayerStart As Date
    Public Event WarningOccurred(_message As String, _color As Color)

    Public Sub New()
        gpxDirectory = My.Settings.Directory
        BackupDirectory = My.Settings.BackupDirectory
        If Not Directory.Exists(BackupDirectory) Then
            Directory.CreateDirectory(BackupDirectory)
        End If
    End Sub

    Public Property LayerStart As DateTime
        Get
            If _LayerStart = Date.MinValue Then
                _LayerStart = GetLayerStart()
            End If
            Return _LayerStart
        End Get
        Set
            _LayerStart = Value
        End Set
    End Property

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
        IO.File.SetCreationTime(Me.Reader.FilePath, Me.LayerStart)
        ' Setting the last modified file date
        IO.File.SetLastWriteTime(Me.Reader.FilePath, Me.LayerStart)
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
                ageFromTime = TimeSpan.Zero
            End Try
        End If


        If Not String.IsNullOrWhiteSpace(Me.Description) Then ageFromComments = FindTheAgeinComments(Me.Description)


        'Write age to comments
        If ageFromComments = TimeSpan.Zero And Not ageFromTime.TotalMinutes <= 0 Then
            WriteDescription(ageFromTime)
        End If

        If ageFromTime.TotalMinutes > 0 Then
            Return ageFromTime
        ElseIf ageFromComments.TotalMinutes > 0 Then
            Return ageFromComments
        Else
            Debug.WriteLine($"Age of the trail { Me.Reader.FileName} wasn't found!")
            Return TimeSpan.Zero
        End If

    End Function

    ' Function to set the <desc> description from the first <trk> node in the GPX file
    Public Sub WriteDescription(agefromTime As TimeSpan)
        Dim newDescription As String
        If Me.Description Is Nothing Then
            newDescription = "Trail: " & agefromTime.TotalHours.ToString("F1") & " h"
        Else ' Najde řetězec "Trail:" a nahradí ho řetězcem "Trail:" & AgeFromTime
            Dim searchText As String = "trail" ' Hledaný text (malými písmeny)
            Dim index As Integer = Me.Description.IndexOf(searchText, StringComparison.OrdinalIgnoreCase)

            If index >= 0 Then ' Pokud byl text nalezen
                ' Získání textu před a za nalezeným slovem
                Dim prefix As String = Me.Description.Substring(0, index)
                Dim suffix As String = Me.Description.Substring(index + searchText.Length)

                ' Odstranění případných mezer a dvojtečky za slovem "trail"
                suffix = suffix.TrimStart(" ", "", ":")
                ' Sestavení nového popisu
                newDescription = $"{prefix}Trail: {agefromTime.TotalHours.ToString("F1")} h {suffix}"
            Else
                ' když tam Trail není vytvoří ho a doplní do desc
                newDescription = "Trail: " & agefromTime.TotalHours.ToString("F1") & " h " & Me.Description
            End If

        End If

        If Not String.IsNullOrWhiteSpace(newDescription) Then
            ' Find the first <trk> node and its <desc> subnode
            Dim trkNodes As XmlNodeList = Me.Reader.SelectNodes("trk")
            Dim descNodeLayer As XmlNode = Me.Reader.SelectSingleChildNode("desc", trkNodes(0))
            ' Pokud uzel <desc> neexistuje, vytvoříme jej a přidáme do <trk>
            If descNodeLayer Is Nothing Then
                descNodeLayer = Me.Reader.CreateElement("desc")
                If trkNodes(0) IsNot Nothing Then
                    ' Vytvoříme nový uzel <desc>
                    ' Přidání <desc> jako prvního potomka v uzlu <trk>
                    If trkNodes(0).HasChildNodes Then
                        ' Vloží <desc> před první existující poduzel
                        trkNodes(0).InsertBefore(descNodeLayer, trkNodes(0).FirstChild)
                    Else
                        ' Pokud <trk> nemá žádné poduzly, použijeme AppendChild
                        trkNodes(0).AppendChild(descNodeLayer)
                    End If
                End If
            End If
            'najdeme poznámku u psa
            Dim descNodeDog As XmlNode = Me.Reader.SelectSingleChildNode("desc", trkNodes(1))
            Dim DogsDesc As String = ""
            If descNodeDog IsNot Nothing Then
                'najde text poznámky u psa
                DogsDesc = descNodeDog.InnerText
                'node  descNodeDog smažu jinak se načítá opakovaně
                descNodeDog.ParentNode.RemoveChild(descNodeDog)
            End If

            ' Nastavíme hodnotu pro <desc> spojením s poznámkou u psa
            newDescription = newDescription & " " & DogsDesc
            descNodeLayer.InnerText = newDescription

            Me.Description = newDescription
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
                modifiedFileName = modifiedFileName.TrimStart(charsToTrim).TrimEnd(charsToTrim)

                ' Vrácení data i upraveného názvu souboru
                Return (dateTimeFromFileName, modifiedFileName)
            Catch ex As Exception
                Console.WriteLine($"{fileName} - Error in date format")
                Return (Nothing, fileName)
            End Try
        Else
            Console.WriteLine($"{fileName} - Date not found")
            Return (Nothing, fileName)
        End If

    End Function


    ' Function to read the time from the first <time> node in the GPX file
    ' If <time> node doesnt exist tries to read date from file name and creates <time> node
    Private _isGettingLayerStart As Boolean = False
    Public Function GetLayerStart() As DateTime

        If _isGettingLayerStart Then
            Debug.WriteLine("Ochrana: GetLayerStart již běží.")
            Return LayerStart
        End If
        _isGettingLayerStart = True

        Try
            'Načtení času z prvního  uzlu <trkpt>
            Dim timeNode As XmlNode = Nothing
            Dim trkptNode As XmlNode = Me.Reader.SelectSingleNode("trkpt")
            If trkptNode IsNot Nothing Then
                timeNode = Me.Reader.SelectSingleChildNode("time", trkptNode)
            End If

            Dim LayerStartTimeNode As XmlNode = timeNode ' Me.Reader.SelectSingleNode("time")
            If LayerStartTimeNode Is Nothing OrElse Not DateTime.TryParse(LayerStartTimeNode.InnerText, _LayerStart) Then
                Debug.WriteLine("Uzel <time> chybí nebo má neplatný formát.")
                ' ... logika vytvoření nového <time> uzlu ...

                ' if the <time> node doesn't exists or has an  invalid value:
                'pokusí se odečíst datum z názvu souboru a vytvořit uzel <time>
                ' Převedení nalezeného řetězce na DateTime
                Dim RecordedDateFromFileName As DateTime = Me.GetRemoveDateFromName.Item1
                If RecordedDateFromFileName <> Date.MinValue Then
                    LayerStart = RecordedDateFromFileName
                    AddTimeNodeToFirstTrkpt(RecordedDateFromFileName.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")
    )
                    RaiseEvent WarningOccurred($" <time> node with Date from file name created: {RecordedDateFromFileName.ToString("yyyy-MM-dd")}" & $"in file: {Reader.FileName}", Color.DarkGreen)
                Else
                    ' If the node doesn't exist or isn't a valid date, return the default DateTime value
                    RaiseEvent WarningOccurred($"GPX file: {Me.Reader.FileName} contains no date!", Color.Red)
                End If
            End If

        Finally
            _isGettingLayerStart = False
        End Try
        Return LayerStart
    End Function


    Public Function GetDogStart() As Date
        Dim trksegNodes As XmlNodeList = Me.Reader.SelectNodes("trkseg")
        Dim dogStart As DateTime
        If trksegNodes.Count > 1 Then
            Dim dogtimeNodes As XmlNodeList = Me.Reader.SelectAllChildNodes("time", trksegNodes(1)) '.SelectNodes("gpx:trkpt/gpx:time", namespaceManager)
            Dim DogstartTimeNode As XmlNode = dogtimeNodes(0)
            If Not DogstartTimeNode Is Nothing Then DateTime.TryParse(DogstartTimeNode.InnerText, dogStart)
        End If
        Me.DogStart = dogStart
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
        Me.DogFinish = dogFinish
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
                    RaiseEvent WarningOccurred("Error processing point: " & ex.Message & Environment.NewLine, Color.Red)
                End Try
            Next
        Else
            RaiseEvent WarningOccurred("No tracks found in GPX file: " & Me.Reader.FileName & Environment.NewLine, Color.Red)
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

    Private Function ZkombinujNazvySouboru(record1 As GPXRecord, record2 As GPXRecord) As String
        '' odstranění data (hledáme různé formáty)
        'Dim datum1 As Date? = record1.GetRemoveDateFromName.Item1
        'Dim datum2 As Date? = record2.GetRemoveDateFromName.Item1

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


    Public Function MergeDogToMe(dog As GPXRecord) As Boolean

        Dim newName = ZkombinujNazvySouboru(Me, dog)
        'do souboru me vloží kompletní uzel  <trk> vyjmutý ze souboru dog
        Try
            ' Najdi první uzel <trk>
            Dim meTrkNode As XmlNode = Me.Reader.SelectSingleNode("trk")
            'přidá node typu:
            Dim newTypeNode_layer As XmlNode = Me.Reader.CreateElement(meTrkNode, "type", TrackTypes.TrailLayer, False)
            Dim dogTrkNode As XmlNode = dog.Reader.SelectSingleNode("trk")
            'přidá node typu:
            Dim newTypeNode_dog As XmlNode = dog.Reader.CreateElement(dogTrkNode, "type", TrackTypes.Dog, False)

            If meTrkNode IsNot Nothing AndAlso dogTrkNode IsNot Nothing Then
                Dim importedNode As XmlNode = Me.Reader.ImportNode(dogTrkNode, True) ' Důležité: Import uzlu!
                Dim meGpxNode As XmlNode = Me.Reader.SelectSingleNode("gpx")
                meGpxNode.AppendChild(importedNode) ' Přidání na konec <gpx>

                'spojené trasy se uloží do souboru kladeče
                'když je nové jméno stejné jako jméno kladeč nepřejmenovává se
                If Me.Reader.FileName = newName OrElse RenameFile(newName) Then
                    Me.Reader.Save()
                    IO.File.Delete(dog.Reader.FilePath)
                    RaiseEvent WarningOccurred($"Tracks in files {Me.Reader.FileName} and {dog.Reader.FileName} were successfully merged in file {Me.Reader.FileName} {vbCrLf}File {dog.Reader.FileName}  was deleted.{vbCrLf}", Color.DarkGreen)
                End If

            End If
            Return True
        Catch ex As Exception
            RaiseEvent WarningOccurred($"Merging tracks of the me  {Me.Reader.FileName} and the dog {dog.Reader.FileName} failed!" & vbCrLf & ex.Message, Color.Red)
            Return False
        End Try

    End Function




    ' in gpx files, splits a track with two segments into two separate tracks
    Public Sub SplitTrackIntoTwo()
        ' Najdi první uzel <trk>
        Dim trkNode As XmlNode = Me.Reader.SelectSingleNode("trk")


        If trkNode IsNot Nothing Then
            ' Najdi všechny <trkseg> uvnitř <trk>
            Dim trkSegNodes As XmlNodeList = Me.Reader.SelectChildNodes("trkseg", trkNode)

            If trkSegNodes.Count > 1 Then
                Dim newTypeNode_trailLayer As XmlNode = Me.Reader.CreateElement(trkNode, "type", TrackTypes.TrailLayer, False)

                ' Vytvoř nový uzel <trk>
                Dim newTrkNode As XmlNode = Me.Reader.CreateElement("trk")
                Dim newTypeNode_dog As XmlNode = Me.Reader.CreateElement(newTrkNode, "type", TrackTypes.Dog, False)
                ' Přesuň druhý <trkseg> do nového <trk>
                Dim secondTrkSeg As XmlNode = trkSegNodes(1)
                trkNode.RemoveChild(secondTrkSeg)
                newTrkNode.AppendChild(secondTrkSeg)

                ' Přidej nový <trk> do dokumentu hned po prvním
                trkNode.ParentNode.InsertAfter(newTrkNode, trkNode)
                Me.Reader.Save()
                RaiseEvent WarningOccurred($"Track in file { Me.Reader.FileName} was successfully split.", Color.DarkGreen)
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
        Me.Reader.Save()
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
    '                If i < bezierTrack.Count Then
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

    Sub AddTimeNodeToFirstTrkpt(timeValue As String)

        ' Vyhledání prvního uzlu <trkpt>
        Dim firstTrkptNode As XmlNode = Me.Reader.SelectSingleNode("trkpt")
        Dim save As Boolean = False

        If firstTrkptNode IsNot Nothing Then
            Me.Reader.CreateElement(firstTrkptNode, "time", timeValue, True)
            save = True
            Debug.WriteLine("Časový uzel byl úspěšně přidán.")
        Else
            Debug.WriteLine("Uzel <trkpt> nebyl nalezen.")
        End If

        Me.Reader.Save()

    End Sub

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
            'Using saveFileDialog As New SaveFileDialog()
            '    saveFileDialog.InitialDirectory = gpxDirectory ' Počáteční adresář
            '    saveFileDialog.FileName = newFileName ' Navrhované jméno souboru
            '    saveFileDialog.Filter = "GPX Files (*.gpx)|*.gpx|All Files (*.*)|*.*" ' Filtry souborů (volitelné)
            '    saveFileDialog.Title = "Save GPX File As" ' Titulek dialogu
            '    saveFileDialog.OverwritePrompt = True ' Zeptat se na přepsání existujícího souboru

            '    If saveFileDialog.ShowDialog() = DialogResult.OK Then
            '        newFileName = saveFileDialog.FileName ' Vrátí vybranou cestu k souboru
            '    Else
            '        newFileName = Nothing ' Uživatel zrušil dialog
            '    End If
            'End Using
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

End Class



Public Class GpxReader
    Private xmlDoc As XmlDocument
    Private namespaceManager As XmlNamespaceManager
    Public Property FilePath As String
    Private namespacePrefix As String

    Public ReadOnly Property FileName As String
        Get
            Return Path.GetFileName(FilePath)
        End Get
    End Property

    Public Property Nodes As XmlNodeList


    ' Konstruktor načte XML dokument a nastaví XmlNamespaceManager
    Public Sub New(_filePath As String)
        Try
            xmlDoc = New XmlDocument()
            xmlDoc.Load(_filePath)
            FilePath = _filePath

            ' Zjištění namespace, pokud je definován
            Dim rootNode As XmlNode = xmlDoc.DocumentElement
            Dim namespaceUri As String = rootNode.NamespaceURI
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
        Return xmlDoc.CreateElement(nodename, "http://www.topografix.com/GPX/1/1")
    End Function

    Public Function CreateElement(parentNode As XmlElement, childNodeName As String, value As String, insertAfter As Boolean) As XmlElement
        Dim childNode As XmlElement = CreateElement(childNodeName)
        childNode.InnerText = value

        Dim childNodes As XmlNodeList = Me.SelectAllChildNodes(childNodeName, parentNode)
        ' Kontrola, zda existuje alespoň jeden uzel <childnodename>
        If childNodes.Count = 0 Then
            ' Uzel <childnodename> neexistuje, můžeme ho vytvořit a vložit
            ' Pokud parent nemá žádné podřízené uzly, jednoduše ho přidáme jako poslední (AppendChild)
            parentNode.AppendChild(childNode)
        Else
            'kontrola duplicity:
            For Each node As XmlNode In childNodes
                'zkontroluje zda node s hodnotou 'value' už neexistuje:
                If node.InnerText.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0 Then
                    Console.WriteLine($"Uzel {childNodeName} s textem {value} již existuje, nepřidávám nový.")
                Else
                    Console.WriteLine($"Uzel {childNodeName} s jiným textem již existuje, přidávám nový.")
                    If insertAfter Then
                        parentNode.AppendChild(childNode)
                    Else
                        'vložíme nový uzel PŘED prvního podřízeného
                        parentNode.InsertBefore(childNode, childNodes(0))
                    End If
                End If
            Next
        End If

        Return parentNode

    End Function


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

