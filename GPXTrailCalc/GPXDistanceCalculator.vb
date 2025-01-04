'Imports System.Globalization
'Imports System.IO
'Imports System.Net.WebRequestMethods
'Imports System.Text.RegularExpressions
'Imports System.Xml
'Imports GPXTrailAnalyzer.My.Resources
'Imports System.Linq
'Imports System.Runtime.CompilerServices.RuntimeHelpers
'Imports Microsoft.VisualBasic.Logging
''Imports System.Windows.Media

'Public Class GPXDistanceCalculator

'    ' Constants for converting degrees to radians and Earth's radius
'    Private Const PI As Double = 3.14159265358979
'    Private Const EARTH_RADIUS As Double = 6371 ' Earth's radius in kilometers
'    Private gpxFiles As New List(Of String)
'    Private gpxReaders As New List(Of GpxReader)
'    Public distances As New List(Of Double)
'    Private dateFrom As DateTime
'    Private dateTo As DateTime
'    Public layerStart, dogStart, dogFinish As New List(Of DateTime)
'    Public age As New List(Of TimeSpan)
'    Private descriptions As New List(Of String)
'    Public totalDistances As New List(Of Double)
'    Private link As New List(Of String)
'    Public speed As New List(Of Double)
'    Private save As Boolean = False
'    Private maxAge As TimeSpan
'    Private mergeNoAsk As Boolean = False 'if yes merge runner and dog trails without asking
'    Private mergeCancel As Boolean = False 'don't merge 

'    Dim totalDistance As Double

'    Private _gpxFilesCount As Integer = 0
'    Public Property GpxFilesCount As Integer
'        Get
'            Return _gpxFilesCount
'        End Get
'        Set(value As Integer)
'            _gpxFilesCount = value
'        End Set
'    End Property



'    Private _directoryPath As String



'    Public Property DirectoryPath() As String
'        Get
'            Return _directoryPath
'        End Get
'        Set(value As String)
'            If Not String.IsNullOrWhiteSpace(value) AndAlso Directory.Exists(value) Then
'                _directoryPath = value
'            Else
'                Throw New ArgumentException("Zadaná cesta adresáře není platná.")
'            End If
'        End Set
'    End Property




'    ' Function to convert degrees to radians
'    Private Function DegToRad(degrees As Double) As Double
'        Return degrees * PI / 180
'    End Function

'    ' Function to calculate the distance in km between two GPS points using the Haversine formula
'    Private Function HaversineDistance(lat1 As Double, lon1 As Double, lat2 As Double, lon2 As Double, units As String) As Double
'        Dim dLat As Double = DegToRad(lat2 - lat1)
'        Dim dLon As Double = DegToRad(lon2 - lon1)

'        Dim a As Double = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(DegToRad(lat1)) * Math.Cos(DegToRad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
'        Dim c As Double = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a))

'        If units = "km" Then
'            Return EARTH_RADIUS * c ' Result in kilometers
'        ElseIf units = "m" Then
'            Return EARTH_RADIUS * c * 1000 'result in metres
'        Else
'            Return EARTH_RADIUS * c ' Result in kilometers
'        End If
'    End Function

'    ' Function to read the time from the first <time> node in the GPX file
'    ' If <time> node doesnt exist tries to read date from file name and creates <time> node
'    Private Function GetLayerStart(filePath As String, reader As GpxReader) As DateTime
'        Dim layerStart As DateTime

'        ' Načtení jednoho uzlu <time>
'        Dim LayerStartTimeNode As XmlNode = reader.SelectSingleNode("time")


'        Dim RecordedDateFromFileName As DateTime
'        Dim filename As String = Path.GetFileNameWithoutExtension(filePath)
'        If Regex.IsMatch(filename, "^\d{4}-\d{2}-\d{2}") Then
'            ' Extrahování data z názvu souboru
'            Dim dateMatch As Match = Regex.Match(filename, "^\d{4}-\d{2}-\d{2}")
'            If dateMatch.Success Then
'                ' Převedení nalezeného řetězce na DateTime
'                RecordedDateFromFileName = DateTime.ParseExact(dateMatch.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture)
'            End If
'        End If



'        ' Check if the <time> node exists and has a valid value
'        If LayerStartTimeNode IsNot Nothing AndAlso DateTime.TryParse(LayerStartTimeNode.InnerText, layerStart) Then

'            'keeps value from file
'        ElseIf RecordedDateFromFileName <> Date.MinValue Then

'            'pokusí se odečíst datum z názvu souboru a vytvořit uzel <time>
'            ' Převedení nalezeného řetězce na DateTime
'            layerStart = RecordedDateFromFileName
'            AddTimeNodeToFirstTrkpt(reader, RecordedDateFromFileName.ToString("yyyy-MM-dd" & "T" & "hh:mm:ss" & "Z"))
'            Form1.txtWarnings.AppendText($" <time> node with Date from file name created: {RecordedDateFromFileName.ToString("yyyy-MM-dd")}" & $"in file: {filename}")


'        Else
'            ' If the node doesn't exist or isn't a valid date, return the default DateTime value


'        End If

'        Return layerStart

'    End Function

'    Private Function CalculateAge(i As Integer) As TimeSpan
'        Dim ageFromTime As TimeSpan
'        Dim ageFromComments As TimeSpan

'        If dogStart(i) <> Date.MinValue AndAlso layerStart(i) <> Date.MinValue Then
'            Try
'                ageFromTime = dogStart(i) - layerStart(i)
'            Catch ex As Exception
'            End Try
'        End If



'        If Not String.IsNullOrWhiteSpace(descriptions(i)) Then ageFromComments = FindTheAgeinComments(descriptions(i))

'        'Add age to comments
'        If ageFromComments = TimeSpan.Zero And Not ageFromTime = TimeSpan.Zero Then
'            Dim newDescription As String
'            If descriptions(i) Is Nothing Then
'                newDescription = "Trail: " & ageFromTime.TotalHours.ToString("F1") & " hod"

'                ' Najde řetězec "Trail:" a nahradí ho řetězcem "Trail:" & AgeFromTime
'            ElseIf descriptions(i).Contains("Trail:") Then
'                newDescription = descriptions(i).Replace($"Trail:", "Trail: " & ageFromTime.TotalHours.ToString("F1") & " hod")
'                ' když tam Trai není vytvoří ho a doplní do desc
'            Else
'                newDescription = "Trail: " & ageFromTime.TotalHours.ToString("F1") & " hod" & descriptions(i)
'            End If

'            If Not String.IsNullOrWhiteSpace(newDescription) Then
'                save = SetDescription(i, newDescription)
'            End If
'            descriptions(i) = newDescription
'        Else

'        End If




'        If Not ageFromTime = TimeSpan.Zero Then
'            Return ageFromTime
'        ElseIf Not ageFromComments = TimeSpan.Zero Then
'            Return ageFromComments
'        Else Return TimeSpan.Zero
'        End If
'        Return TimeSpan.Zero
'    End Function

'    Private Function CalculateSpeed(i As Integer) As Double 'km/h

'        If Not dogStart(i) = DateTime.MinValue AndAlso Not dogFinish(i) = DateTime.MinValue Then
'            If (dogFinish(i) - dogStart(i)).TotalHours > 0 Then
'                Return distances(i) / (dogFinish(i) - dogStart(i)).TotalHours
'            End If
'        End If
'        Return Nothing


'    End Function




'    Public Function FindTheAgeinComments(inputText As String) As TimeSpan
'        ' Upravený regulární výraz pro nalezení čísla, které může být i desetinné
'        Dim regex As New Regex("\b\d+[.,]?\d*\s*(h(odin(y|a))?|hod|min(ut)?)\b", RegexOptions.IgnoreCase)
'        Dim match As Match = regex.Match(inputText)

'        If match.Success Then
'            Dim nalezenyCas As String = match.Value
'            ' Převede desetinnou tečku nebo čárku na standardní tečku pro parsování
'            Dim casString As String = Regex.Match(nalezenyCas, "\d+[.,]?\d*").Value.Replace(",", ".")
'            Dim casCislo As Double = Double.Parse(casString, CultureInfo.InvariantCulture)



'            Dim casTimeSpan As TimeSpan
'            If nalezenyCas.Contains("h") Then
'                casTimeSpan = TimeSpan.FromHours(casCislo)
'                Return casTimeSpan
'            ElseIf nalezenyCas.Contains("min") Then
'                casTimeSpan = TimeSpan.FromMinutes(casCislo)
'                Return casTimeSpan
'            End If
'        End If

'        ' Pokud nebyl čas nalezen, vrátí Nothing
'        Return TimeSpan.Zero
'    End Function



'    Sub AddTimeNodeToFirstTrkpt(gpxReader As GpxReader, timeValue As String)

'        ' Vyhledání prvního uzlu <trkpt>
'        Dim firstTrkptNode As XmlNode = gpxReader.SelectSingleNode("trkpt")
'        Dim save As Boolean = False

'        If firstTrkptNode IsNot Nothing Then
'            gpxReader.CreateElement(firstTrkptNode, "time", timeValue)
'            save = True
'            Debug.WriteLine("Časový uzel byl úspěšně přidán.")
'        Else
'            Debug.WriteLine("Uzel <trkpt> nebyl nalezen.")
'        End If

'        gpxReader.Save()

'    End Sub
'    ' Function to read the <link> description from the first <trk> node in the GPX file
'    Private Function Getlink(i As Integer)
'        ' Načtení více uzlů, např. <trkseg>
'        Dim linkNodes As XmlNodeList = gpxReaders(i).SelectNodes("link")

'        For Each linkNode As XmlNode In linkNodes
'            ' Zpracování každého uzlu <link>

'            If linkNode IsNot Nothing AndAlso linkNode.Attributes("href") IsNot Nothing Then
'                Dim linkHref As String = linkNode.Attributes("href").Value
'                If linkHref.Contains("youtu") Then
'                    Return linkHref
'                End If
'            End If
'        Next
'        Return Nothing
'    End Function



'    ' Function to set the <desc> description from the first <trk> node in the GPX file
'    Private Function SetDescription(i As Integer, newDescription As String) As Boolean
'        ' Find the first <trk> node and its <desc> subnode
'        Dim trkNode As XmlNode = gpxReaders(i).SelectSingleNode("trk")
'        Dim descNode As XmlNode = gpxReaders(i).SelectSingleChildNode("desc", trkNode)
'        ' Pokud uzel <desc> neexistuje, vytvoříme jej a přidáme do <trk>
'        If descNode Is Nothing Then
'            ' Najdeme první uzel <trk>
'            'Dim trkNode As XmlNode = xmlDoc.SelectSingleNode("/gpx:gpx/gpx:trk[1]", namespaceManager)
'            descNode = gpxReaders(i).CreateElement("desc")
'            If trkNode IsNot Nothing Then
'                ' Vytvoříme nový uzel <desc>
'                ' Přidání <desc> jako prvního potomka v uzlu <trk>
'                If trkNode.HasChildNodes Then
'                    ' Vloží <desc> před první existující poduzel
'                    trkNode.InsertBefore(descNode, trkNode.FirstChild)
'                Else
'                    ' Pokud <trk> nemá žádné poduzly, použijeme AppendChild
'                    trkNode.AppendChild(descNode)
'                End If
'                ' Nastavíme hodnotu pro <desc> (můžete ji změnit podle potřeby)
'                descNode.InnerText = newDescription
'                ' Přidáme nový uzel <desc> do uzlu <trk>
'                'trkNode.AppendChild(descNode)
'            End If
'        Else
'            descNode.InnerText = newDescription
'        End If
'        Return True

'    End Function

'    ' Function to read and calculate the length of only the first segment from the GPX file
'    Private Function CalculateFirstSegmentDistance(i As Integer) As Double
'        Dim totalLengthOfFirst_trkseg As Double = 0.0
'        Dim lat1, lon1, lat2, lon2 As Double
'        Dim firstPoint As Boolean = True




'        ' Select the first track segment (<trkseg>) using the namespace
'        Dim trknode As XmlNode = gpxReaders(i).SelectSingleNode("trk")
'        Dim firstSegment As XmlNode = gpxReaders(i).SelectSingleChildNode("trkseg", trknode)

'        ' If the segment exists, calculate its length
'        If firstSegment IsNot Nothing Then
'            ' Select all track points in the first segment (<trkpt>)
'            Dim trackPoints As XmlNodeList = gpxReaders(i).SelectChildNodes("trkpt", firstSegment)

'            ' Calculate the distance between each point in the first segment
'            For Each point As XmlNode In trackPoints
'                Try
'                    ' Check if attributes exist and load them as Double
'                    If point.Attributes("lat") IsNot Nothing AndAlso point.Attributes("lon") IsNot Nothing Then
'                        Dim lat As Double = Convert.ToDouble(point.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
'                        Dim lon As Double = Convert.ToDouble(point.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)

'                        If firstPoint Then
'                            ' Initialize the first point
'                            lat1 = lat
'                            lon1 = lon
'                            firstPoint = False
'                        Else
'                            ' Calculate the distance between the previous and current point
'                            lat2 = lat
'                            lon2 = lon
'                            totalLengthOfFirst_trkseg += HaversineDistance(lat1, lon1, lat2, lon2, "km")

'                            ' Move the current point into lat1, lon1 for the next iteration
'                            lat1 = lat2
'                            lon1 = lon2
'                        End If
'                    End If
'                Catch ex As Exception
'                    ' Adding a more detailed exception message
'                    Debug.WriteLine("Error: " & ex.Message)
'                    ' TODO: Replace direct access to Form1 with a better method for separating logic
'                    Form1.txtWarnings.AppendText("Error processing point: " & ex.Message & Environment.NewLine)
'                End Try
'            Next
'        Else
'            ' TODO: Replace direct access to Form1 with a better method for separating logic
'            Form1.txtWarnings.AppendText("No tracks found in GPX file: " & gpxFiles(i) & Environment.NewLine)
'        End If

'        Return totalLengthOfFirst_trkseg ' Result in kilometers
'    End Function

'    Public Function ReadAndProcessData(startDate As DateTime, endDate As DateTime) As Boolean
'        DirectoryPath = My.Settings.Directory
'        maxAge = New TimeSpan(My.Settings.maxAge, 0, 0)
'        Dim PrependDatetoFileName As Boolean = My.Settings.PrependDateToName
'        dateFrom = startDate
'        dateTo = endDate

'        Form1.rtbOutput.Clear()
'        Form1.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Underline Or FontStyle.Bold) ' Nastavit font
'        Form1.rtbOutput.SelectionColor = Color.DarkGreen ' Nastavit barvu

'        Dim manySpaces As String = "                                                 "
'        Form1.rtbOutput.AppendText((My.Resources.Resource1.outgpxFileName & manySpaces).Substring(0, 33))
'        Form1.rtbOutput.AppendText((My.Resources.Resource1.X_AxisLabel & manySpaces).Substring(0, 14))
'        Form1.rtbOutput.AppendText((My.Resources.Resource1.outLength & manySpaces).Substring(0, 12))
'        Form1.rtbOutput.AppendText((My.Resources.Resource1.outAge & manySpaces).Substring(0, 8))
'        Form1.rtbOutput.AppendText((My.Resources.Resource1.outSpeed & manySpaces).Substring(0, 20))
'        Form1.rtbOutput.AppendText(My.Resources.Resource1.outDescription)
'        Form1.rtbOutput.AppendText(vbCrLf)



'        If Not ClearAllLists() Then
'            Return False
'        End If

'        If Not gpxFiles Is Nothing Then gpxFiles.Clear()
'        gpxFiles = GetGpxFiles(DirectoryPath)

'        If gpxFiles Is Nothing OrElse gpxFiles.Count = 0 Then
'            MessageBox.Show(My.Resources.Resource1.mBoxNo_gpx_file_was_found)
'            Return False
'        End If



'        If Not ClearAllLists() Then
'            Return False
'        End If



'        For i = 0 To gpxFiles.Count - 1
'            Try
'                Dim gpxfilePath As String = gpxFiles(i)
'                Dim reader As New GpxReader(gpxFiles(i)) 'musí se načíst znovu kvůli files.sort
'                gpxReaders.Add(reader)

'                ' Start calculation using the values
'                RenamewptNode(i, My.Resources.Resource1.article) 'renaming wpt to "artickle"
'                layerStart.Add(GetLayerStart(gpxFiles(i), gpxReaders(i)))
'                SplitTrackIntoTwo(i) 'in gpx files, splits a track with two segments into two separate tracks
'                'descriptions.Add(GetDescription(i)) 'musí být první - slouží k výpočtu age
'                If My.Settings.TrimGPSnoise Then TrimGPSnoise(12, gpxReaders(i)) 'ořízne nevýznamné konce a začátky trailů, když se stojí na místě.
'                distances.Add(CalculateFirstSegmentDistance(i))
'                If i = 0 Then totalDistances.Add(distances(i)) Else totalDistances.Add(totalDistances(i - 1) + distances(i))
'                dogStart.Add(GetDogStart(i))
'                dogFinish.Add(GetDogFinish(i))
'                age.Add(CalculateAge(i))
'                speed.Add(CalculateSpeed(i))
'                link.Add(Getlink(i))

'                gpxReaders(i).Save() 'hlavně kvůli desc
'                'a nakonec
'                SetCreatedModifiedDate(i)

'                ' Display results
'                Dim fileShortName As String = (Path.GetFileNameWithoutExtension(gpxFiles(i)) & manySpaces).Substring(0, 30)

'                ' Nastavení fontu a barvy textu
'                Form1.rtbOutput.SelectionStart = Form1.rtbOutput.Text.Length ' Pozice na konec textu
'                Form1.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font

'                Form1.rtbOutput.SelectionColor = Color.Maroon ' Nastavit barvu
'                Form1.rtbOutput.AppendText(fileShortName & "   ")

'                Form1.rtbOutput.SelectionColor = Color.DarkGreen ' Nastavit barvu
'                Form1.rtbOutput.AppendText(layerStart(i).Date.ToShortDateString & "    ")
'                Form1.rtbOutput.AppendText(distances(i).ToString("F2") & " km" & "     ")
'                If age(i).TotalHours > 0 Then
'                    Form1.rtbOutput.AppendText(age(i).TotalHours.ToString("F1") & " h" & "   ")
'                Else
'                    Form1.rtbOutput.AppendText("         ")
'                End If
'                If speed(i) > 0 Then
'                    Form1.rtbOutput.AppendText(speed(i).ToString("F1") & " km/h" & "   ")
'                Else
'                    Form1.rtbOutput.AppendText("           ")
'                End If
'                If Not descriptions(i) = Nothing Then
'                    Form1.rtbOutput.AppendText(descriptions(i))
'                End If

'                If Not link(i) = Nothing Then

'                    Form1.rtbOutput.AppendText("    Video: ")
'                    Form1.rtbOutput.SelectionColor = Color.Blue ' Nastavit barvu
'                    Form1.rtbOutput.AppendText(link(i))

'                End If

'                Form1.rtbOutput.AppendText(vbCrLf)

'                ' Posunutí kurzoru na konec textu
'                Form1.rtbOutput.SelectionStart = Form1.rtbOutput.Text.Length

'                ' Skrolování na aktuální pozici kurzoru
'                Form1.rtbOutput.ScrollToCaret()
'            Catch ex As Exception
'                MessageBox.Show(My.Resources.Resource1.mBoxDataRetrievalFailed & vbCrLf & "File: " & Path.GetFileNameWithoutExtension(gpxFiles(i)) & vbCrLf & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
'            End Try
'        Next i


'        totalDistance = totalDistances(gpxFiles.Count - 1)
'        Dim AgeAsDouble As List(Of Double) = age.Select(Function(ts) ts.TotalMinutes).ToList()

'        ' Nastavení fontu a barvy textu
'        Form1.rtbOutput.SelectionStart = Form1.rtbOutput.Text.Length ' Pozice na konec textu
'        Form1.rtbOutput.SelectionFont = New Font("Calibri", 10) ' Nastavit font
'        Form1.rtbOutput.SelectionColor = Color.Maroon ' Nastavit barvu
'        Form1.rtbOutput.AppendText(vbCrLf & My.Resources.Resource1.outProcessed_period_from & startDate.ToShortDateString & My.Resources.Resource1.outDo & endDate.ToShortDateString &
'                vbCrLf & My.Resources.Resource1.outAll_gpx_files_from_directory & DirectoryPath & vbCrLf & vbCrLf)

'        Dim manydots As String = "...................................................................."
'        Dim labelLength As Integer = 40

'        Form1.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font
'        Form1.rtbOutput.SelectionColor = Color.Maroon
'        Form1.rtbOutput.AppendText((My.Resources.Resource1.outTotalNumberOfGPXFiles & manydots).Substring(0, labelLength))

'        Form1.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
'        Form1.rtbOutput.SelectionColor = Color.Firebrick
'        Form1.rtbOutput.AppendText(distances.Count & vbCrLf)



'        Form1.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font
'        Form1.rtbOutput.SelectionColor = Color.Maroon
'        Form1.rtbOutput.AppendText((My.Resources.Resource1.outTotalLength & manydots).Substring(0, labelLength))

'        Form1.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
'        Form1.rtbOutput.SelectionColor = Color.Firebrick
'        Form1.rtbOutput.AppendText(totalDistance.ToString("F2") & " km" & vbCrLf)
'        Form1.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font
'        Form1.rtbOutput.SelectionColor = Color.Maroon
'        Form1.rtbOutput.AppendText((My.Resources.Resource1.outAverageDistance & manydots).Substring(0, labelLength))
'        Form1.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
'        Form1.rtbOutput.SelectionColor = Color.Firebrick
'        Form1.rtbOutput.AppendText((1000 * AverageOf(distances)).ToString("F0") & " m" & vbCrLf)
'        Form1.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font
'        Form1.rtbOutput.SelectionColor = Color.Maroon
'        Form1.rtbOutput.AppendText((My.Resources.Resource1.outAverageAge & manydots).Substring(0, labelLength))
'        Form1.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
'        Form1.rtbOutput.SelectionColor = Color.Firebrick
'        Form1.rtbOutput.AppendText(AverageOf(AgeAsDouble).ToString("F0") & " min" & vbCrLf)
'        Form1.rtbOutput.SelectionFont = New Font("Cascadia Code", 10) ' Nastavit font
'        Form1.rtbOutput.SelectionColor = Color.Maroon
'        Form1.rtbOutput.AppendText((My.Resources.Resource1.outAverageSpeed & manydots).Substring(0, labelLength))
'        Form1.rtbOutput.SelectionFont = New Font("Cascadia Code Semibold", 10, FontStyle.Bold) ' Nastavit font
'        Form1.rtbOutput.SelectionColor = Color.Firebrick
'        Form1.rtbOutput.AppendText(AverageOf(speed).ToString("F2") & " km/h")

'        ' Posunutí kurzoru na konec textu
'        Form1.rtbOutput.SelectionStart = Form1.rtbOutput.Text.Length

'        ' Skrolování na aktuální pozici kurzoru
'        Form1.rtbOutput.ScrollToCaret()
'        Return True
'    End Function

'    Private Function ClearAllLists() As Boolean
'        Try
'            gpxReaders.Clear()
'            layerStart.Clear()
'            dogStart.Clear()
'            dogFinish.Clear()
'            distances.Clear()
'            totalDistances.Clear()
'            age.Clear()
'            speed.Clear()
'            descriptions.Clear()
'            link.Clear()
'            Return True
'        Catch ex As Exception
'            MessageBox.Show("Something went wrong")
'            Return False
'        End Try

'    End Function

'    Private Sub AppendHyperlink(rtb As RichTextBox, url As String, displayText As String)
'        ' Načíst aktuální RTF obsah
'        Dim currentRtf As String = rtb.Rtf

'        ' Klikací odkaz v RTF formátu
'        Dim hyperlinkRtf As String = "{\field{\*\fldinst{HYPERLINK """ & url & """}}{\fldrslt{" & displayText & "}}}"

'        ' Vložení odkazu před poslední uzavírací složenou závorku
'        Dim newRtf As String = currentRtf.Insert(currentRtf.LastIndexOf("}"), hyperlinkRtf)

'        ' Aktualizace obsahu RichTextBox
'        rtb.Rtf = newRtf
'    End Sub


'    Private Function AverageOf(y As List(Of Double)) As Double
'        Dim suma As Double = 0
'        Dim n As Integer = 0
'        For Each number In y
'            If number > 0 Then
'                suma += number
'                n += 1
'            End If
'        Next
'        If n > 0 Then Return suma / n Else Return 0

'    End Function

'    Private Function GetDogStart(i As Integer) As Date

'        Dim trksegNodes As XmlNodeList = gpxReaders(i).SelectNodes("trkseg")
'        Dim dogStart As DateTime



'        If trksegNodes.Count > 1 Then

'            Dim dogtimeNodes As XmlNodeList = gpxReaders(i).SelectAllChildNodes("time", trksegNodes(1)) '.SelectNodes("gpx:trkpt/gpx:time", namespaceManager)

'            Dim DogstartTimeNode As XmlNode = dogtimeNodes(0)
'            If Not DogstartTimeNode Is Nothing Then DateTime.TryParse(DogstartTimeNode.InnerText, dogStart)

'        End If
'        Return dogStart

'    End Function
'    Private Function GetDogFinish(i As Integer) As Date

'        Dim trksegNodes As XmlNodeList = gpxReaders(i).SelectNodes("trkseg")
'        Dim dogFinish As DateTime



'        If trksegNodes.Count > 1 Then

'            Dim dogtimeNodes As XmlNodeList = gpxReaders(i).SelectAllChildNodes("time", trksegNodes(1)) '.SelectNodes("gpx:trkpt/gpx:time", namespaceManager)

'            Dim DogFinishTimeNode As XmlNode = dogtimeNodes(dogtimeNodes.Count - 1)
'            If Not DogFinishTimeNode Is Nothing Then DateTime.TryParse(DogFinishTimeNode.InnerText, dogFinish)
'        End If
'        Return dogFinish

'    End Function

'    Private Sub BackupGpxFiles(gpxFiles As List(Of String))
'        Dim backupDirectory As String
'        backupDirectory = My.Settings.BackupDirectory

'        Try
'            ' Zajisti, že cílový adresář existuje
'            If Not Directory.Exists(backupDirectory) Then
'                Directory.CreateDirectory(backupDirectory)
'            End If
'            Dim backupFilePath As String
'            For Each sourcefilePath In gpxFiles
'                ' Získání názvu souboru z cesty
'                Dim fileName As String = Path.GetFileName(sourcefilePath)

'                ' Vytvoření kompletní cílové cesty
'                backupFilePath = Path.Combine(backupDirectory, fileName)

'                If Not IO.File.Exists(backupFilePath) Then
'                    ' Kopírování souboru
'                    Try
'                        IO.File.Copy(sourcefilePath, backupFilePath, False)
'                    Catch ex As Exception
'                        ' Zpracování jakýchkoli neočekávaných chyb
'                        Debug.WriteLine($"Chyba při kopírování souboru {fileName}: {ex.Message}")
'                    End Try
'                Else
'                    ' Soubor již existuje, přeskočíme
'                    Debug.WriteLine($"Soubor {fileName} již existuje, přeskočeno.")
'                End If

'            Next
'            Debug.WriteLine($"Soubory gpx byly úspěšně zálohovány do: {backupDirectory }")
'            Form1.txtWarnings.AppendText($"{vbCrLf}{Resource1.logBackupOfFiles}   {backupDirectory }{vbCrLf}")
'        Catch ex As Exception
'            Debug.WriteLine($"Chyba při zálohování souborů: {ex.Message}")
'        End Try

'    End Sub

'    Structure SouborSDatem
'        Dim NazevSouboru As String
'        Dim Datum As DateTime
'        Dim reader As GpxReader
'    End Structure

'    Private Function MergeTwoGpxFiles(layer As SouborSDatem, dog As SouborSDatem) As Boolean
'        'do souboru layer vloží kompletní uzel  <trk> vyjmutý ze souboru dog
'        Try


'            ' Najdi první uzel <trk>
'            Dim layertrkNode As XmlNode = layer.reader.SelectSingleNode("trk")
'            Dim dogtrkNode As XmlNode = dog.reader.SelectSingleNode("trk")
'            If layertrkNode IsNot Nothing AndAlso dogtrkNode IsNot Nothing Then
'                Dim importedNode As XmlNode = layer.reader.ImportNode(dogtrkNode, True) ' Důležité: Import uzlu!
'                Dim layerGpxNode As XmlNode = layer.reader.SelectSingleNode("gpx")
'                layerGpxNode.AppendChild(importedNode) ' Přidání na konec <gpx>

'                'spojené trasy se uloží do souboru kladeče
'                layer.reader.Save()
'                IO.File.Delete(dog.NazevSouboru)
'                Form1.txtWarnings.AppendText($"Tracks in files {Path.GetFileName(layer.NazevSouboru)} and {Path.GetFileName(dog.NazevSouboru)} were successfully merged in file {Path.GetFileName(layer.NazevSouboru)} {vbCrLf}File {Path.GetFileName(dog.NazevSouboru)}  was deleted.{vbCrLf}")

'            End If
'            Return True
'        Catch ex As Exception
'            Return False
'        End Try

'    End Function
'    ' Get a list of all GPX files in the specified directory and filters them according to the specified condition, i.e. the specified time interval
'    Private Function GetGpxFiles(directorypath As String) As List(Of String)
'        Try

'            ' Načteme všechny GPX soubory
'            Dim gpxFilesAll As List(Of String) = Directory.GetFiles(directorypath, "*.gpx").ToList()
'            BackupGpxFiles(gpxFilesAll)
'            Dim gpxFilesWithinInterval As New List(Of String)
'            ' Filtrujeme soubory podle podmínky
'            For i As Integer = 0 To gpxFilesAll.Count - 1

'                Dim reader As New GpxReader(gpxFilesAll(i))
'                Dim _layerStart As DateTime = GetLayerStart(gpxFilesAll(i), reader)
'                If _layerStart >= dateFrom And _layerStart <= dateTo Then
'                    gpxFilesWithinInterval.Add(gpxFilesAll(i))
'                    layerStart.Add(_layerStart)
'                    gpxReaders.Add(reader)
'                End If
'            Next

'            If My.Settings.PrependDateToName Then
'                For i = 0 To gpxFilesWithinInterval.Count - 1
'                    'modifies the name to start with the date
'                    PrependDateToFilename(i)
'                Next i
'            End If

'            ' následuje setřídění podle data a pospojování tracků psa a kladeče
'            Dim SouborySDaty As New List(Of SouborSDatem)

'            For i As Integer = 0 To gpxFilesWithinInterval.Count - 1
'                SouborySDaty.Add(New SouborSDatem With {.NazevSouboru = gpxFilesWithinInterval(i), .Datum = layerStart(i), .reader = gpxReaders(i)})
'            Next


'            SouborySDaty.Sort(Function(x, y) x.Datum.CompareTo(y.Datum)) ' Seřazení podle data
'            'vytváříme list gpx souborů seřazených podle LayerStart, první vložíme,
'            'další prohlídneme, zda se neliší o méně než maxAge - ty je pak možné spojit,
'            'protože se nejspíš jedná o záznam stopy kladeč a trasy psa, jen je každá zvlášť

'            Dim gpxFilesSortedByDate As New List(Of String) From {SouborySDaty(0).NazevSouboru}
'            Dim lastAddedIndex As Integer = 0

'            For i = 1 To SouborySDaty.Count - 1
'                If Not TryMerge(SouborySDaty(i), SouborySDaty(lastAddedIndex)) Then
'                    ' Přidání souboru do seznamu, pokud ke spojení nedošlo
'                    gpxFilesSortedByDate.Add(SouborySDaty(i).NazevSouboru)
'                    lastAddedIndex = i
'                End If
'            Next i

'            ' Nyní máš v gpxFilesSortedByDate seřazené názvy souborů podle data v LayerStart
'            ' Původní seznam gpxFilesWithinInterval zůstane nezměněn.


'            Return gpxFilesSortedByDate
'        Catch ex As Exception
'            ' Adding a more detailed exception message
'            Debug.WriteLine("Error: " & ex.Message)
'            Return Nothing
'        End Try


'    End Function

'    Private Function TryMerge(soubor_i As SouborSDatem, soubor_prev As SouborSDatem) As Boolean

'        'najdi všechny sousední soubory, které se liší o méně než MaxAge
'        ' Základní kontrola, zda rozdíl dat splňuje podmínku na max stáří


'        If (soubor_i.Datum - soubor_prev.Datum < maxAge) AndAlso
'        (Not mergeCancel) AndAlso (Not LoadMergeDecision(soubor_prev, soubor_i) = "No") Then

'            ' Zjisti, zda oba soubory obsahují pouze jeden uzel <trkseg>
'            Dim trksegNodes_i As XmlNodeList = soubor_i.reader.SelectNodes("trkseg")
'            Dim trksegNodes_prev As XmlNodeList = soubor_prev.reader.SelectNodes("trkseg")
'            If trksegNodes_i.Count = 1 AndAlso trksegNodes_prev.Count = 1 Then
'                ' Zeptej se uživatele, zda chce soubory spojit
'                Dim mergeFiles As DialogResult
'                If Not mergeNoAsk Then mergeFiles = DialogMergeFiles(soubor_prev, soubor_i)
'                ' Pokud uživatel souhlasí, spoj soubory, jinak přidej
'                If mergeNoAsk OrElse (mergeFiles = DialogResult.Yes) Then
'                    If MergeTwoGpxFiles(soubor_prev, soubor_i) Then
'                        Return True
'                    End If
'                End If
'            End If
'        End If
'        Return False 'ke spojení souborů nedošlo
'    End Function



'    Private Function DialogMergeFiles(runner As SouborSDatem, dog As SouborSDatem) As DialogResult
'        ' Vytvoření instance formuláře (dialogu)
'        Dim dialog As New Form()
'        dialog.Text = My.Resources.Resource1.lblMergeGPXtracksQ ' "Spojit GPX trasy?"
'        dialog.FormBorderStyle = FormBorderStyle.FixedDialog ' Zamezení změně velikosti
'        dialog.StartPosition = FormStartPosition.CenterParent ' Vycentrování na rodičovské okno
'        dialog.MinimizeBox = False
'        dialog.MaximizeBox = False
'        dialog.BackColor = Color.FromArgb(237, 240, 213)
'        dialog.Font = New Font("Cascadia Code Semibold", 10, FontStyle.Bold)
'        dialog.ForeColor = Color.DarkGreen ' Nastavit barvu


'        ' Popisek s dotazem
'        Dim lblDotaz As New Label()
'        lblDotaz.Text = My.Resources.Resource1.lblMergeTwoToOneQ '"Chcete spojit tyto dva soubory do jednoho?"
'        lblDotaz.AutoSize = True
'        lblDotaz.Location = New Point(10, 10)
'        lblDotaz.ForeColor = Color.Maroon
'        dialog.Controls.Add(lblDotaz)

'        ' Popis (volitelný, můžeš ho upravit podle potřeby)
'        Dim lblPopis As New Label()
'        lblPopis.Text = My.Resources.Resource1.lblMergingYouGet ' "Spojením získáte jeden soubor gpx obsahující trasu kladeče i psa." ' Příklad popisu
'        lblPopis.AutoSize = True
'        lblPopis.Location = New Point(10, lblDotaz.Bottom + 5)
'        dialog.Controls.Add(lblPopis)

'        ' Popisky se jmény souborů
'        Dim lblSoubor1 As New Label()
'        lblSoubor1.Text = $"{My.Resources.Resource1.lblIsThisLayerQ}: '{Path.GetFileName(runner.NazevSouboru)}' ?"
'        lblSoubor1.AutoSize = True
'        lblSoubor1.Location = New Point(10, lblPopis.Bottom + 10)
'        lblSoubor1.ForeColor = Color.Maroon
'        dialog.Controls.Add(lblSoubor1)

'        Dim lblSoubor2 As New Label()
'        lblSoubor2.Text = $"{My.Resources.Resource1.lblIsThisTrackOfTheDog}: '{Path.GetFileName(dog.NazevSouboru)}' ?"
'        lblSoubor2.AutoSize = True
'        lblSoubor2.Location = New Point(10, lblSoubor1.Bottom + 5)
'        lblSoubor2.ForeColor = Color.Maroon
'        dialog.Controls.Add(lblSoubor2)

'        ' Zaškrtávací políčko pro zapamatování rozhodnutí
'        Dim chbRemembDecision As New CheckBox
'        chbRemembDecision.Text = My.Resources.Resource1.chbRemembDecisQ '"Zapamatovat rozhodnutí 'Ne' pro tuto dvojici"
'        chbRemembDecision.Location = New Point(10, lblSoubor2.Bottom + 10)
'        chbRemembDecision.AutoSize = True
'        chbRemembDecision.Checked = True
'        dialog.Controls.Add(chbRemembDecision)

'        ' Tlačítka
'        Dim btnAno As New Button()
'        btnAno.Text = "Yes"
'        btnAno.DialogResult = DialogResult.Yes ' Nastavení výsledku dialogu
'        btnAno.Location = New Point(10, chbRemembDecision.Bottom + 15)
'        btnAno.AutoSize = True
'        dialog.Controls.Add(btnAno)

'        Dim btnNe As New Button()
'        btnNe.Text = "No"
'        btnNe.DialogResult = DialogResult.No
'        btnNe.Location = New Point(btnAno.Right + 10, chbRemembDecision.Bottom + 15)
'        btnNe.AutoSize = True
'        dialog.Controls.Add(btnNe)


'        ' Zaškrtávací políčko pro automatické spojování
'        Dim rbNoAsk As New RadioButton()
'        rbNoAsk.Text = My.Resources.Resource1.rbDontAskMergeQ '"U dalších dvojic se už neptat a rovnou spojit (opatrně!)"
'        rbNoAsk.Location = New Point(10, btnNe.Bottom + 15)
'        rbNoAsk.AutoSize = True
'        dialog.Controls.Add(rbNoAsk)

'        ' Zaškrtávací políčko pro zrušení spojování
'        Dim rbCancel As New RadioButton()
'        rbCancel.Text = My.Resources.Resource1.rbDontAskDontMerge ' "U dalších dvojic se už neptat a nic nespojovat"
'        rbCancel.Location = New Point(10, rbNoAsk.Bottom + 7)
'        rbCancel.AutoSize = True
'        dialog.Controls.Add(rbCancel)

'        ' Zaškrtávací políčko pro zrušení spojování
'        Dim rbAsk As New RadioButton()
'        rbAsk.Text = My.Resources.Resource1.rbAskAgein '"Příště se zase ptát"
'        rbAsk.Location = New Point(10, rbCancel.Bottom + 7)
'        rbAsk.AutoSize = True
'        rbAsk.Checked = True
'        dialog.Controls.Add(rbAsk)


'        ' Nastavení velikosti dialogu (automaticky podle obsahu)
'        dialog.AutoSize = True
'        dialog.AutoSizeMode = AutoSizeMode.GrowAndShrink

'        ' Zobrazení dialogu modálně a uložení výsledku
'        Dim result As DialogResult = dialog.ShowDialog()

'        ' Uložení stavu zaškrtávacího políčka do veřejné proměnné nebo do nastavení aplikace
'        If result = DialogResult.No AndAlso chbRemembDecision.Checked Then
'            SaveMergeDecision(runner, dog, DialogResult.No)
'        End If

'        mergeNoAsk = rbNoAsk.Checked ' Uložení stavu pro použití v hlavní funkci
'        mergeCancel = rbCancel.Checked
'        Return result
'    End Function


'    Private Sub SaveMergeDecision(runner As SouborSDatem, dog As SouborSDatem, result As String)
'        Dim key As String = $"{runner.NazevSouboru}|{dog.NazevSouboru}" ' Vytvoření klíče
'        Dim settings As System.Collections.Specialized.StringCollection = My.Settings.MergeDecisions
'        ' Inicializace kolekce, pokud je Nothing
'        If settings Is Nothing Then
'            settings = New System.Collections.Specialized.StringCollection()
'            My.Settings.MergeDecisions = settings ' Důležité: Uložení inicializované kolekce zpět do nastavení!
'        End If
'        ' Odstranění starého rozhodnutí, pokud existuje

'        Dim existingIndex As Integer = -1
'        For i As Integer = 0 To settings.Count - 1
'            If settings(i).StartsWith($"{runner.NazevSouboru}|{dog.NazevSouboru}|") Then
'                existingIndex = i
'                Exit For
'            End If
'        Next
'        If existingIndex > -1 Then settings.RemoveAt(existingIndex)
'        settings.Add($"{key}|{result}") ' Uložení nového rozhodnutí
'        My.Settings.Save() ' Uložení změn do konfiguračního souboru

'    End Sub

'    ' Načtení rozhodnutí
'    Private Function LoadMergeDecision(runner As SouborSDatem, dog As SouborSDatem) As String
'        Dim key As String = $"{runner.NazevSouboru}|{dog.NazevSouboru}"
'        Dim settings As System.Collections.Specialized.StringCollection = My.Settings.MergeDecisions
'        If Not settings Is Nothing Then
'            For Each item As String In settings
'                If item.StartsWith($"{key}|") Then
'                    Return item.Split("|")(2) ' Vrácení rozhodnutí ("Yes", "No", "NoAsk", "Cancel")
'                End If
'            Next
'        End If
'        Return "" ' Vrácení prázdného řetězce, pokud rozhodnutí neexistuje
'    End Function

'    Public Sub PrependDateToFilename(i As Integer)


'        Dim fileName As String = Path.GetFileNameWithoutExtension(gpxFiles(i))
'        Dim fileExtension As String = Path.GetExtension(gpxFiles(i))
'        Dim _layerStart As DateTime = GetLayerStart(gpxFiles(i), gpxReaders(i))

'        Dim newFileName As String
'        Dim newFilePath As String


'        Dim dateTimeFromFileName As DateTime
'        Try

'            ' Regex s pojmenovanými skupinami pro celé formáty i jednotlivé části data
'            Dim pattern As String = "(?<format1>T(?<year1>\d{4})-(?<month1>\d{2})-(?<day1>\d{2})-(?<hour1>\d{2})-(?<minute1>\d{2}))|" &
'                        "(?<format2>(?<year2>\d{4})-(?<month2>\d{2})-(?<day2>\d{2})_(?<hour2>\d{2})-(?<minute2>\d{2}))|" &
'                        "(?<format3>(?<day3>\d{1,2})\._(?<month3>\d{2})\._(?<year3>\d{4})_(?<hour3>\d{1,2})_(?<minute3>\d{2})_(?<second3>\d{2}))|" &
'                        "(?<format4>(?<year4>\d{4})-(?<month4>\d{2})-(?<day4>\d{2}))"
'            Dim myRegex As New Regex(pattern)

'            Dim match As Match = myRegex.Match(fileName)
'            If match.Success Then

'                Dim formattedDate As String = ""
'                ' Rozpoznání formátu podle shody celé pojmenované skupiny formátu
'                If match.Groups("format1").Success Then
'                    ' Formát TYYYY-MM-DD-hh-mm
'                    Dim year As Integer = Integer.Parse(match.Groups("year1").Value)
'                    Dim month As Integer = Integer.Parse(match.Groups("month1").Value)
'                    Dim day As Integer = Integer.Parse(match.Groups("day1").Value)
'                    Dim hour As Integer = Integer.Parse(match.Groups("hour1").Value)
'                    Dim minute As Integer = Integer.Parse(match.Groups("minute1").Value)
'                    dateTimeFromFileName = New DateTime(year, month, day, hour, minute, 0)
'                    formattedDate = match.Groups("format1").Value

'                ElseIf match.Groups("format2").Success Then
'                    ' Formát YYYY-MM-DD_hh-mm
'                    Dim year As Integer = Integer.Parse(match.Groups("year2").Value)
'                    Dim month As Integer = Integer.Parse(match.Groups("month2").Value)
'                    Dim day As Integer = Integer.Parse(match.Groups("day2").Value)
'                    Dim hour As Integer = Integer.Parse(match.Groups("hour2").Value)
'                    Dim minute As Integer = Integer.Parse(match.Groups("minute2").Value)
'                    dateTimeFromFileName = New DateTime(year, month, day, hour, minute, 0)
'                    formattedDate = match.Groups("format2").Value
'                ElseIf match.Groups("format3").Success Then
'                    ' Formát D._MM._YYYY_h_mm_ss
'                    Dim day As Integer = Integer.Parse(match.Groups("day3").Value.PadLeft(2, "0"c))
'                    Dim month As Integer = Integer.Parse(match.Groups("month3").Value.PadLeft(2, "0"c))
'                    Dim year As Integer = Integer.Parse(match.Groups("year3").Value)
'                    Dim hour As Integer = Integer.Parse(match.Groups("hour3").Value)
'                    Dim minute As Integer = Integer.Parse(match.Groups("minute3").Value)
'                    Dim second As Integer = Integer.Parse(match.Groups("second3").Value)
'                    formattedDate = match.Groups("format3").Value
'                    dateTimeFromFileName = New DateTime(year, month, day, hour, minute, second)
'                ElseIf match.Groups("format4").Success Then
'                    ' Formát YYYY-MM-DD
'                    Dim year As Integer = Integer.Parse(match.Groups("year4").Value)
'                    Dim month As Integer = Integer.Parse(match.Groups("month4").Value)
'                    Dim day As Integer = Integer.Parse(match.Groups("day4").Value)
'                    dateTimeFromFileName = New DateTime(year, month, day)
'                End If

'                ' Výstup formátu data ve tvaru YYYY-MM-DD
'                ' Debug.writeline("Převedené datum: " & dateTimeFromFileName.ToString("yyyy-MM-dd"))
'                ' Odstranění původního datového vzoru z řetězce
'                Dim modifiedFileName As String = myRegex.Replace(fileName, "")

'                ' Přidání přeformátovaného data na začátek modifikovaného řetězce
'                newFileName = $"{dateTimeFromFileName.ToString("yyyy-MM-dd")}{modifiedFileName}"
'                '  Debug.writeline("Přeformátované file name: " & newFileName)

'                If Not String.IsNullOrWhiteSpace(newFileName) AndAlso Not newFileName.TrimEnd = fileName.TrimEnd Then

'                    newFilePath = Path.Combine(DirectoryPath, newFileName & ".gpx")

'                    If IO.File.Exists(newFilePath) Then
'                        ' Handle existing files
'                        Dim userInput As String = InputBox($"File {newFileName} already exists. Enter a new name:", newFileName)
'                        If Not String.IsNullOrWhiteSpace(userInput) Then
'                            newFilePath = Path.Combine(DirectoryPath, userInput & fileExtension)
'                            IO.File.Move(gpxFiles(i), newFilePath)
'                            Form1.txtWarnings.AppendText($"Renamed file: {Path.GetFileName(gpxFiles(i))} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
'                            Debug.WriteLine($"Renamed file: {Path.GetFileName(gpxFiles(i))} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")

'                        Else
'                            Form1.txtWarnings.AppendText($"New name for {newFilePath} was not provided.{Environment.NewLine}")

'                        End If

'                    Else
'                        IO.File.Move(gpxFiles(i), newFilePath)
'                        gpxFiles(i) = newFilePath
'                        Debug.WriteLine($"Renamed file: {Path.GetFileName(gpxFiles(i))} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
'                        Form1.txtWarnings.AppendText($"Renamed file: {Path.GetFileName(gpxFiles(i))} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
'                    End If

'                    gpxFiles(i) = newFilePath
'                End If


'            Else
'                Debug.WriteLine("Žádné datum v požadovaném formátu nebylo nalezeno.")
'                newFileName = $"{_layerStart.Date.ToString("yyyy-MM-dd")}{fileName}{fileExtension}"
'                newFilePath = Path.Combine(DirectoryPath, newFileName)

'                If IO.File.Exists(newFilePath) Then
'                    ' Handle existing files
'                    Dim userInput As String = InputBox($"File {newFileName} already exists. Enter a new name:", newFileName)
'                    If Not String.IsNullOrWhiteSpace(userInput) Then
'                        newFilePath = Path.Combine(DirectoryPath, userInput & fileExtension)
'                        IO.File.Move(gpxFiles(i), newFilePath)
'                        Form1.txtWarnings.AppendText($"Renamed file: {Path.GetFileName(gpxFiles(i))} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
'                        Debug.WriteLine($"Renamed file: {Path.GetFileName(gpxFiles(i))} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")

'                    Else
'                        Form1.txtWarnings.AppendText($"New name for {newFilePath} was not provided.{Environment.NewLine}")

'                    End If

'                Else
'                    IO.File.Move(gpxFiles(i), newFilePath)
'                    gpxFiles(i) = newFilePath
'                    Debug.WriteLine($"Renamed file: {Path.GetFileName(gpxFiles(i))} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
'                    Form1.txtWarnings.AppendText($"Renamed file: {Path.GetFileName(gpxFiles(i))} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
'                End If

'            End If

'        Catch ex As Exception
'            Debug.WriteLine(ex.ToString)
'        End Try
'    End Sub


'    Sub SetCreatedModifiedDate(i)
'        'change of attributes
'        ' Setting the file creation date
'        IO.File.SetCreationTime(gpxFiles(i), layerStart(i))
'        ' Setting the last modified file date
'        IO.File.SetLastWriteTime(gpxFiles(i), layerStart(i))
'    End Sub

'    Public Sub WriteCSVfile(csvFilePath As String)
'        Try

'            ' Create the CSV file and write headers
'            Using writer As New StreamWriter(csvFilePath, False, System.Text.Encoding.UTF8)
'                writer.WriteLine("File Name;Date;Age/h;Length/km;speed;Total Length;Description;Video")

'                For i As Integer = 0 To distances.Count - 1
'                    Dim fileName As String = Path.GetFileNameWithoutExtension(gpxFiles(i))

'                    Dim _age As String = ""
'                    If age(i) > TimeSpan.Zero Then
'                        _age = age(i).TotalHours.ToString("F1")
'                    End If

'                    ' Write each row in the CSV file
'                    writer.Write($"{fileName};")
'                    writer.Write($"{layerStart(i).ToString("yyyy-MM-dd")};")
'                    writer.Write($"{_age};")
'                    writer.Write($"{distances(i):F2};")
'                    If Not speed(i) = 0 Then writer.Write($"{speed(i):F2};") Else writer.Write(";")
'                    writer.Write($"{totalDistances(i):F2};")
'                    writer.Write($"{descriptions(i)};")
'                    If Not link(i) Is Nothing Then
'                        writer.WriteLine($"=HYPERTEXTOVÝ.ODKAZ(""{link(i)}"")")
'                    End If
'                    writer.WriteLine()

'                Next

'                ' Write the total distance at the end of the CSV file
'                writer.WriteLine($"Total;;; {totalDistances(distances.Count - 1):F2}")
'            End Using


'            Form1.txtWarnings.AppendText($"{vbCrLf}CSV file created: {csvFilePath}.{Environment.NewLine}")
'        Catch ex As Exception
'            Form1.txtWarnings.AppendText($"{My.Resources.Resource1.mBoxErrorCreatingCSV}: {ex.Message}{Environment.NewLine}")
'            MessageBox.Show($"Error creating CSV file: {ex.Message}")
'        End Try
'    End Sub




'    ' in gpx files, splits a track with two segments into two separate tracks
'    Sub SplitTrackIntoTwo(i As Integer)
'        ' Najdi první uzel <trk>
'        Dim trkNode As XmlNode = gpxReaders(i).SelectSingleNode("trk")

'        If trkNode IsNot Nothing Then
'            ' Najdi všechny <trkseg> uvnitř <trk>
'            Dim trkSegNodes As XmlNodeList = gpxReaders(i).SelectChildNodes("trkseg", trkNode)

'            If trkSegNodes.Count > 1 Then
'                ' Vytvoř nový uzel <trk>
'                Dim newTrkNode As XmlNode = gpxReaders(i).CreateElement("trk")

'                ' Přesuň druhý <trkseg> do nového <trk>
'                Dim secondTrkSeg As XmlNode = trkSegNodes(1)
'                trkNode.RemoveChild(secondTrkSeg)
'                newTrkNode.AppendChild(secondTrkSeg)

'                ' Přidej nový <trk> do dokumentu hned po prvním
'                trkNode.ParentNode.InsertAfter(newTrkNode, trkNode)
'                gpxReaders(i).Save()
'                Form1.txtWarnings.AppendText($"Track in file {gpxFiles(i)} was successfully split.")
'            End If
'        End If
'    End Sub


'    Sub RenamewptNode(i As Integer, newname As String)
'        ' traverses all <wpt> nodes in the GPX file and overwrites the value of <name> nodes to "-předmět":
'        ' Find all <wpt> nodes using the namespace
'        gpxReaders(i).Nodes = gpxReaders(i).SelectNodes("wpt")
'        ' Go through each <wpt> node
'        For Each wptNode As XmlNode In gpxReaders(i).Nodes
'            ' Najdi uzel <name> uvnitř <wpt> s použitím namespace
'            Dim nameNode As XmlNode = gpxReaders(i).SelectSingleChildNode("name", wptNode)

'            If nameNode IsNot Nothing AndAlso nameNode.InnerText <> newname Then
'                ' Přepiš hodnotu <name> na newname
'                nameNode.InnerText = newname
'            End If
'        Next
'    End Sub


'    Sub TrimGPSnoise(minDistance As Integer, gpxReader As GpxReader)
'        'clip the start and end of both <trk>, i.e., the layer and the dog, which was recorded after (or before) the end of the trail. Useful when the GPS doesn't turn off right away.
'        ' Získání všech uzlů <trk>
'        Dim trackNodes = gpxReader.SelectNodes("trk")

'        For Each trkNode As XmlNode In trackNodes
'            ' Získání všech <trkseg> uvnitř <trk>
'            Dim trackSegments = gpxReader.SelectChildNodes("trkseg", trkNode)

'            For Each trksegNode As XmlNode In trackSegments
'                ' Získání všech <trkpt> uvnitř <trkseg>
'                Dim trackPoints = gpxReader.SelectChildNodes("trkpt", trksegNode)

'                ' Převod XmlNodeList na seznam pro snadnou manipulaci
'                Dim points = trackPoints.Cast(Of XmlNode).ToList()

'                Dim startCluster = Cluster(points, gpxReader, minDistance)

'                ' Odeber body z clusteru
'                If startCluster.Count > 5 Then
'                    For i = 0 To startCluster.Count - 2 'poslední ponechá, neb je nahrazen centroidem
'                        Dim point = startCluster.Item(i)
'                        trksegNode.RemoveChild(point)
'                        points.Remove(point)
'                    Next
'                    gpxReader.Save()
'                End If

'                Dim reversedPoints = points.AsEnumerable().Reverse().ToList()

'                Dim endCluster = Cluster(reversedPoints, gpxReader, minDistance)

'                ' Odeber body z endCluster
'                If endCluster.Count > 5 Then
'                    For i = 0 To endCluster.Count - 2 ' poslední ponecháme
'                        Dim point = endCluster.Item(i)
'                        trksegNode.RemoveChild(point)
'                    Next
'                    gpxReader.Save()
'                End If
'            Next
'        Next


'    End Sub

'    Private Function Cluster(points As List(Of XmlNode), gpxReader As GpxReader, minDistance As Double) As List(Of XmlNode)
'        Dim cluster_ As New List(Of XmlNode)
'        Dim centroidLat, centroidLon As Double

'        Dim isCluster As Boolean = True

'        For i As Integer = 0 To points.Count - 1
'            Dim lat = Double.Parse(points(i).Attributes("lat").Value, CultureInfo.InvariantCulture)
'            Dim lon = Double.Parse(points(i).Attributes("lon").Value, CultureInfo.InvariantCulture)


'            If cluster_.Count = 0 Then
'                ' Inicializace clusteru
'                cluster_.Add(points(i))
'                centroidLat = lat
'                centroidLon = lon

'                Continue For
'            End If
'            ' Výpočet vzdálenosti od centroidu
'            Dim currentDistance = HaversineDistance(centroidLat, centroidLon, lat, lon, "m")
'            Debug.WriteLine($"   {i}  {centroidLat} {centroidLon} {lat} {lon} {currentDistance}")

'            ' Rozhodnutí o ukončení clusteru 

'            If currentDistance > minDistance Then ' Pokud je vzdálenost větší ukonči cluster
'                isCluster = False
'            End If

'            If Not isCluster Then
'                'poslední bod v clusteru je nahrazen centroidem
'                If cluster_.Count > 5 Then
'                    'Poslední point v klastru se nahradí centroidem,
'                    'přitom uzel time zůstává beze změny - tím se zpřesní
'                    'výpočet stáří a rychlosti psa
'                    cluster_.Last.Attributes("lat").Value = centroidLat.ToString("G", NumberFormatInfo.InvariantInfo)
'                    cluster_.Last.Attributes("lon").Value = centroidLon.ToString("G", NumberFormatInfo.InvariantInfo)
'                End If

'                Exit For
'            End If

'            cluster_.Add(points(i))


'            ' Aktualizace centroidu
'            centroidLat = Math.Round((centroidLat * cluster_.Count + lat) / (cluster_.Count + 1), 8)
'            centroidLon = Math.Round((centroidLon * cluster_.Count + lon) / (cluster_.Count + 1), 8)


'        Next
'        Return cluster_
'    End Function

'    ' Výpočet směru pohybu mezi dvěma body
'    Function HaversineDirection(lat1 As Double, lon1 As Double, lat2 As Double, lon2 As Double) As Double
'        Dim dLon = (lon2 - lon1) * Math.PI / 180.0
'        Dim y = Math.Sin(dLon) * Math.Cos(lat2 * Math.PI / 180.0)
'        Dim x = Math.Cos(lat1 * Math.PI / 180.0) * Math.Sin(lat2 * Math.PI / 180.0) -
'            Math.Sin(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) * Math.Cos(dLon)
'        Dim bearing = Math.Atan2(y, x) * 180.0 / Math.PI
'        Return (bearing + 360) Mod 360 ' Úhel v rozsahu 0–360°
'    End Function



'End Class

