Imports System.Diagnostics.Eventing
Imports System.Globalization
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Xml
Imports GPXTrailAnalyzer.My.Resources
Public Class GpxFileManager
    'obsahuje seznam souborů typu gpxRecord a funkce na jejich vytvoření a zpracování
    Private ReadOnly Property gpxDirectory As String
    Private ReadOnly Property BackupDirectory As String
    Public dateFrom As DateTime?
    Public dateTo As DateTime?
    Private ReadOnly maxAge As TimeSpan
    Private ReadOnly prependDateToName As Boolean
    Private ReadOnly trimGPSNoise As Boolean
    Private ReadOnly mergeDecisions As System.Collections.Specialized.StringCollection

    Private mergeNoAsk As Boolean = False 'if yes merge runner and dog trails without asking
    Private mergeCancel As Boolean = False 'don't merge 

    Private Property AllGpxRecords As New List(Of GPXRecord)

    Public Sub New()
        gpxDirectory = My.Settings.Directory
        BackupDirectory = My.Settings.BackupDirectory
        If Not Directory.Exists(BackupDirectory) Then
            Directory.CreateDirectory(BackupDirectory)
        End If
        maxAge = New TimeSpan(My.Settings.maxAge, 0, 0)
        prependDateToName = My.Settings.PrependDateToName
        trimGPSNoise = My.Settings.TrimGPSnoise
        mergeDecisions = My.Settings.MergeDecisions
    End Sub

    Public Function ReadGPXFilesWithinInterval() As List(Of GPXRecord)
        Dim gpxFilesWithinInterval As New List(Of GPXRecord)
        ' Načteme všechny GPX soubory
        Dim gpxFilesAllPath As List(Of String) = Directory.GetFiles(gpxDirectory, "*.gpx").ToList()

        BackupGpxFiles(gpxFilesAllPath)

        For Each gpxFilePath In gpxFilesAllPath
            'Tady najde layerStart 
            Dim _reader As New GpxReader(gpxFilePath)
            Dim _layerStart As DateTime = GetLayerStart(gpxFilePath, _reader)
            If _layerStart >= dateFrom And _layerStart <= dateTo Then
                Dim _gpxRecord As New GPXRecord With {
                    .Reader = _reader,
                    .LayerStart = _layerStart,
                    .FilePath = gpxFilePath,
                    .FileName = Path.GetFileName(gpxFilePath)
                }
                gpxFilesWithinInterval.Add(_gpxRecord)

            End If
        Next
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
            Form1.txtWarnings.AppendText($" <time> node with Date from file name created: {RecordedDateFromFileName.ToString("yyyy-MM-dd")}" & $"in file: {filename}")
        Else
            ' If the node doesn't exist or isn't a valid date, return the default DateTime value
            Form1.txtWarnings.AppendText($"GPX file: {filename} contains no date!")
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

        gpxReader.Save(save)

    End Sub



    Private Sub BackupGpxFiles(gpxFiles As List(Of String))

        Try
            ' Zajisti, že cílový adresář existuje

            Dim backupFilePath As String
            For Each sourcefilePath In gpxFiles
                ' Získání názvu souboru z cesty
                Dim fileName As String = Path.GetFileName(sourcefilePath)

                ' Vytvoření kompletní cílové cesty
                backupFilePath = Path.Combine(BackupDirectory, fileName)

                If Not IO.File.Exists(backupFilePath) Then
                    ' Kopírování souboru
                    Try
                        IO.File.Copy(sourcefilePath, backupFilePath, False)
                    Catch ex As Exception
                        ' Zpracování jakýchkoli neočekávaných chyb
                        Debug.WriteLine($"Chyba při kopírování souboru {fileName}: {ex.Message}")
                    End Try
                Else
                    ' Soubor již existuje, přeskočíme
                    Debug.WriteLine($"Soubor {fileName} již existuje, přeskočeno.")
                End If

            Next
            Debug.WriteLine($"Soubory gpx byly úspěšně zálohovány do: {BackupDirectory }")
            Form1.txtWarnings.AppendText($"{vbCrLf}{Resource1.logBackupOfFiles}   {BackupDirectory }{vbCrLf}")
        Catch ex As Exception
            Debug.WriteLine($"Chyba při zálohování souborů: {ex.Message}")
        End Try
    End Sub

    Public Function PrependDateToFilenames(_gpxRecords As List(Of GPXRecord))
        If Not prependDateToName Then Return _gpxRecords

        For Each _gpxRecord In _gpxRecords
            Dim _gpxRecordPath As String = _gpxRecord.FilePath
            Dim fileName As String = Path.GetFileNameWithoutExtension(_gpxRecordPath)
            Dim fileExtension As String = Path.GetExtension(_gpxRecordPath)
            Dim _layerStart As DateTime = GetLayerStart(_gpxRecordPath, _gpxRecord.Reader)

            Dim newFileName As String
            Dim newFilePath As String


            Dim dateTimeFromFileName As DateTime
            Try

                ' Regex s pojmenovanými skupinami pro celé formáty i jednotlivé části data
                Dim pattern As String = "(?<format1>T(?<year1>\d{4})-(?<month1>\d{2})-(?<day1>\d{2})-(?<hour1>\d{2})-(?<minute1>\d{2}))|" &
                                "(?<format2>(?<year2>\d{4})-(?<month2>\d{2})-(?<day2>\d{2})_(?<hour2>\d{2})-(?<minute2>\d{2}))|" &
                                "(?<format3>(?<day3>\d{1,2})\._(?<month3>\d{2})\._(?<year3>\d{4})_(?<hour3>\d{1,2})_(?<minute3>\d{2})_(?<second3>\d{2}))|" &
                                "(?<format4>(?<year4>\d{4})-(?<month4>\d{2})-(?<day4>\d{2}))"
                Dim myRegex As New Regex(pattern)

                Dim match As Match = myRegex.Match(fileName)
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
                    Dim modifiedFileName As String = myRegex.Replace(fileName, "")

                    ' Přidání přeformátovaného data na začátek modifikovaného řetězce
                    newFileName = $"{dateTimeFromFileName.ToString("yyyy-MM-dd")}{modifiedFileName}"
                    '  Debug.writeline("Přeformátované file name: " & newFileName)

                    If Not String.IsNullOrWhiteSpace(newFileName) AndAlso Not newFileName.TrimEnd = fileName.TrimEnd Then

                        newFilePath = Path.Combine(gpxDirectory, newFileName & ".gpx")

                        If IO.File.Exists(newFilePath) Then
                            ' Handle existing files
                            Dim userInput As String = InputBox($"File {newFileName} already exists. Enter a new name:", newFileName)
                            If Not String.IsNullOrWhiteSpace(userInput) Then
                                newFilePath = Path.Combine(gpxDirectory, userInput & fileExtension)
                                IO.File.Move(_gpxRecordPath, newFilePath)
                                Form1.txtWarnings.AppendText($"Renamed file: {Path.GetFileName(_gpxRecordPath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                                Debug.WriteLine($"Renamed file: {Path.GetFileName(_gpxRecordPath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")

                            Else
                                Form1.txtWarnings.AppendText($"New name for {newFilePath} was not provided.{Environment.NewLine}")

                            End If

                        Else
                            IO.File.Move(_gpxRecordPath, newFilePath)
                            _gpxRecordPath = newFilePath
                            Debug.WriteLine($"Renamed file: {Path.GetFileName(_gpxRecordPath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                            Form1.txtWarnings.AppendText($"Renamed file: {Path.GetFileName(_gpxRecordPath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                        End If

                        _gpxRecordPath = newFilePath
                    End If


                Else
                    Debug.WriteLine("Žádné datum v požadovaném formátu nebylo nalezeno.")
                    newFileName = $"{_layerStart.Date.ToString("yyyy-MM-dd")}{fileName}{fileExtension}"
                    newFilePath = Path.Combine(gpxDirectory, newFileName)

                    If IO.File.Exists(newFilePath) Then
                        ' Handle existing files
                        Dim userInput As String = InputBox($"File {newFileName} already exists. Enter a new name:", newFileName)
                        If Not String.IsNullOrWhiteSpace(userInput) Then
                            newFilePath = Path.Combine(gpxDirectory, userInput & fileExtension)
                            IO.File.Move(_gpxRecordPath, newFilePath)
                            Form1.txtWarnings.AppendText($"Renamed file: {Path.GetFileName(_gpxRecordPath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                            Debug.WriteLine($"Renamed file: {Path.GetFileName(_gpxRecordPath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")

                        Else
                            Form1.txtWarnings.AppendText($"New name for {newFilePath} was not provided.{Environment.NewLine}")

                        End If

                    Else
                        IO.File.Move(_gpxRecordPath, newFilePath)
                        _gpxRecordPath = newFilePath
                        Debug.WriteLine($"Renamed file: {Path.GetFileName(_gpxRecordPath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                        Form1.txtWarnings.AppendText($"Renamed file: {Path.GetFileName(_gpxRecordPath)} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                    End If

                End If

            Catch ex As Exception
                Debug.WriteLine(ex.ToString)
            End Try
        Next

    End Function




    'nepoužito
    Public Function FilterByDate(GpxRecords As List(Of GPXRecord), startDate As DateTime, endDate As DateTime) As List(Of GPXRecord)
        Return GpxRecords.Where(Function(r) r.LayerStart >= startDate AndAlso r.LayerStart <= endDate).ToList()
    End Function



End Class

Public Class GPXRecord
    Public Property FilePath As String
    Public Property FileName As String
    Public Property LayerStart As DateTime
    Public Property DogStart As DateTime
    Public Property DogFinish As DateTime
    Public Property TrailAge As TimeSpan
    Public Property Distance As Double
    Public Property Desctription As String
    Public Property Link As String
    Public Property DogSpeed As String
    Public Property Reader As GpxReader
    Public Function Backup(backupDirectory As String) As Boolean


        ' Vytvoření kompletní cílové cesty
        Dim backupFilePath As String = Path.Combine(backupDirectory, Me.FileName)

        If Not IO.File.Exists(backupFilePath) Then
            ' Kopírování souboru
            Try
                IO.File.Copy(Me.FilePath, backupFilePath, False)
                Return True
            Catch ex As Exception
                ' Zpracování jakýchkoli neočekávaných chyb
                Debug.WriteLine($"Chyba při kopírování souboru {FileName}: {ex.Message}")
                Return False
            End Try
        Else
            ' Soubor již existuje, přeskočíme
            Debug.WriteLine($"Soubor {FileName} již existuje, přeskočeno.")
            Return True
        End If
    End Function

End Class

Public Class GpxReader
    Private xmlDoc As XmlDocument
    Private namespaceManager As XmlNamespaceManager
    Private filePath As String
    Private namespacePrefix As String

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

    Public Sub Save(save As Boolean)
        If save Then xmlDoc.Save(filePath)
    End Sub
End Class

