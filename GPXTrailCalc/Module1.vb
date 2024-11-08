﻿Imports System.Xml
Imports System.IO
Imports System.Globalization
Imports System.Text.RegularExpressions ' Added for working with Match type
Imports System.Windows.Forms.DataVisualization.Charting
Imports System.Collections.Generic
Imports System.Runtime.InteropServices.ComTypes
Imports System.DirectoryServices.ActiveDirectory

Public Class GPXDistanceCalculator

    ' Constants for converting degrees to radians and Earth's radius
    Private Const PI As Double = 3.14159265358979
    Private Const EARTH_RADIUS As Double = 6371 ' Earth's radius in kilometers
    Private gpxFiles As New List(Of String)
    Public distances As New List(Of Double)
    Private dateFrom As DateTime
    Private dateTo As DateTime
    Public layerStart, dogStart, dogFinish As New List(Of DateTime)
    Public age As New List(Of TimeSpan)
    Private descriptions As New List(Of String)
    Public totalDistances As New List(Of Double)
    Private link As New List(Of String)
    Public speed As New List(Of Double)


    Dim totalDistance As Double

    Private _gpxFilesCount As Integer = 0
    Public Property GpxFilesCount As Integer
        Get
            Return _gpxFilesCount
        End Get
        Set(value As Integer)
            _gpxFilesCount = value
        End Set
    End Property



    Private _directoryPath As String
    Public Property DirectoryPath() As String
        Get
            Return _directoryPath
        End Get
        Set(value As String)
            If Not String.IsNullOrWhiteSpace(value) AndAlso Directory.Exists(value) Then
                _directoryPath = value
            Else
                Throw New ArgumentException("Zadaná cesta adresáře není platná.")
            End If
        End Set
    End Property




    ' Function to convert degrees to radians
    Private Function DegToRad(degrees As Double) As Double
        Return degrees * PI / 180
    End Function

    ' Function to calculate the distance between two GPS points using the Haversine formula
    Private Function HaversineDistance(lat1 As Double, lon1 As Double, lat2 As Double, lon2 As Double) As Double
        Dim dLat As Double = DegToRad(lat2 - lat1)
        Dim dLon As Double = DegToRad(lon2 - lon1)

        Dim a As Double = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(DegToRad(lat1)) * Math.Cos(DegToRad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
        Dim c As Double = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a))

        Return EARTH_RADIUS * c ' Result in kilometers
    End Function

    ' Function to read the time from the first <time> node in the GPX file
    ' If <time> node doesnt exist tries to read date from file name and creates <time> node
    Private Function GetLayerStart(filePath As String) As DateTime
        Dim layerStart As DateTime
        Dim xmlDoc As New XmlDocument()

        Try
            xmlDoc.Load(filePath)
        Catch ex As Exception
            ' Adding a more detailed exception message
            Debug.WriteLine("Error: " & ex.Message)
            ' TODO: Replace direct access to Form1 with a better method for separating logic
            Form1.txtWarnings.AppendText($"File {filePath} could not be read: " & ex.Message & Environment.NewLine)
        End Try

        Dim namespaceManager As New XmlNamespaceManager(xmlDoc.NameTable)
        namespaceManager.AddNamespace("gpx", "http://www.topografix.com/GPX/1/1") ' GPX namespace URI
        Dim trksegNodes As XmlNodeList = xmlDoc.SelectNodes("//gpx:trkseg", namespaceManager)


        Dim LayerStartTimeNode As XmlNode = xmlDoc.SelectSingleNode("//gpx:time", namespaceManager)


        'If trksegNodes.Count > 1 Then

        '    Dim dogtimeNodes As XmlNodeList = trksegNodes(1).SelectNodes("gpx:trkpt/gpx:time", namespaceManager)

        '    Dim DogStartTimeNode As XmlNode = dogtimeNodes(0)
        '    DateTime.TryParse(DogStartTimeNode.InnerText, _dogStart)

        '    Dim DogFinishTimeNode As XmlNode = dogtimeNodes(dogtimeNodes.Count - 1)
        '    DateTime.TryParse(DogFinishTimeNode.InnerText, dogFinish(i))
        'End If


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
            AddTimeNodeToFirstTrkpt(filePath, RecordedDateFromFileName.ToString("yyyy-MM-dd" & "T" & "hh:mm:ss" & "Z"))
            Form1.txtWarnings.AppendText($" <time> node with Date from file name created: {RecordedDateFromFileName.ToString("yyyy-MM-dd")}" & $"in file: {filename}")


        Else
            ' If the node doesn't exist or isn't a valid date, return the default DateTime value


        End If

        Return layerStart

    End Function

    Private Function CalculateAge(i As Integer, ByRef xmlDoc As XmlDocument) As TimeSpan
        Dim ageFromTime As TimeSpan
        Dim ageFromComments As TimeSpan

        If dogStart(i) <> Date.MinValue AndAlso layerStart(i) <> Date.MinValue Then
            Try
                ageFromTime = dogStart(i) - layerStart(i)
            Catch ex As Exception
            End Try
        End If



        If Not String.IsNullOrWhiteSpace(descriptions(i)) Then ageFromComments = FindTheAgeinComments(descriptions(i))

        'Add age to comments
        If ageFromComments = TimeSpan.Zero And Not ageFromTime = TimeSpan.Zero Then
            Dim newDescription As String
            If descriptions(i) Is Nothing Then
                newDescription = "Trail: " & ageFromTime.TotalHours.ToString("F1") & " hod"

                ' Najde řetězec "Trail:" a nahradí ho řetězcem "Trail:" & AgeFromTime
            ElseIf descriptions(i).Contains("Trail:") Then
                newDescription = descriptions(i).Replace($"Trail:", "Trail: " & ageFromTime.TotalHours.ToString("F1") & " hod")
                ' když tam Trai není vytvoří ho a doplní do desc
            Else
                newDescription = "Trail: " & ageFromTime.TotalHours.ToString("F1") & " hod" & descriptions(i)
            End If

            If Not String.IsNullOrWhiteSpace(newDescription) Then SetDescription(i, xmlDoc, newDescription)
            descriptions(i) = newDescription
        Else

        End If




        If Not ageFromTime = TimeSpan.Zero Then
            Return ageFromTime
        ElseIf Not ageFromComments = TimeSpan.Zero Then
            Return ageFromComments
        Else Return TimeSpan.Zero
        End If
        Return TimeSpan.Zero
    End Function

    Private Function CalculateSpeed(i As Integer) As Double 'km/h

        If Not dogStart(i) = DateTime.MinValue AndAlso Not dogFinish(i) = DateTime.MinValue Then
            Return distances(i) / (dogFinish(i) - dogStart(i)).TotalHours
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



    Sub AddTimeNodeToFirstTrkpt(gpxFilePath As String, timeValue As String)
        Dim xmlDoc As New XmlDocument()
        xmlDoc.Load(gpxFilePath)

        ' Vytvoření Namespace Manageru pro správnou práci s jmenným prostorem GPX
        Dim namespaceManager As New XmlNamespaceManager(xmlDoc.NameTable)
        namespaceManager.AddNamespace("gpx", "http://www.topografix.com/GPX/1/1")

        ' Vyhledání prvního uzlu <trkpt>
        Dim firstTrkptNode As XmlNode = xmlDoc.SelectSingleNode("//gpx:trkpt", namespaceManager)

        If firstTrkptNode IsNot Nothing Then
            ' Vytvoření nového uzlu <time>
            Dim timeNode As XmlElement = xmlDoc.CreateElement("time", "http://www.topografix.com/GPX/1/1")
            timeNode.InnerText = timeValue

            ' Přidání uzlu <time> do prvního <trkpt>
            firstTrkptNode.AppendChild(timeNode)

            ' Uložení změn zpět do souboru
            xmlDoc.Save(gpxFilePath)
            Debug.WriteLine("Časový uzel byl úspěšně přidán.")
        Else
            Debug.WriteLine("Uzel <trkpt> nebyl nalezen.")
        End If
    End Sub
    ' Function to read the <link> description from the first <trk> node in the GPX file
    Private Function Getlink(xmlDoc As XmlDocument) As String

        Dim namespaceManager As New XmlNamespaceManager(xmlDoc.NameTable)
        namespaceManager.AddNamespace("gpx", "http://www.topografix.com/GPX/1/1") ' GPX namespace URI

        ' Find the first <trk> node and its <desc> subnode

        Dim linkNodes As XmlNodeList = xmlDoc.SelectNodes("//gpx:link", namespaceManager)

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

    ' Function to read the <desc> description from the first <trk> node in the GPX file
    Private Function GetDescription(i As Integer, xmlDoc As XmlDocument) As String

        Dim namespaceManager As New XmlNamespaceManager(xmlDoc.NameTable)
        namespaceManager.AddNamespace("gpx", "http://www.topografix.com/GPX/1/1") ' GPX namespace URI

        ' Find the first <trk> node and its <desc> subnode
        ' Vyhledání uzlu <trk> v rámci hlavního namespace
        Dim trkNode As XmlNode = xmlDoc.SelectSingleNode("//gpx:trk", namespaceManager)
        Dim descNode As XmlNode = trkNode?.SelectSingleNode("gpx:desc", namespaceManager)

        'Dim descNode As XmlNode = xmlDoc.SelectSingleNode("/gpx:gpx/gpx:trk[1]/gpx:desc", namespaceManager)

        If descNode IsNot Nothing Then
            Return descNode.InnerText
        Else
            Return Nothing '"The <desc> node was not found."
        End If
    End Function

    ' Function to set the <desc> description from the first <trk> node in the GPX file
    Private Function SetDescription(i As Integer, ByRef xmlDoc As XmlDocument, newDescription As String) As Boolean

        Dim namespaceManager As New XmlNamespaceManager(xmlDoc.NameTable)
        namespaceManager.AddNamespace("gpx", "http://www.topografix.com/GPX/1/1") ' GPX namespace URI

        ' Find the first <trk> node and its <desc> subnode
        'Dim descNode As XmlNode = xmlDoc.SelectSingleNode("/gpx:gpx/gpx:trk[1]/gpx:desc", namespaceManager)
        Dim trkNode As XmlNode = xmlDoc.SelectSingleNode("//gpx:trk", namespaceManager)
        Dim descNode As XmlNode = trkNode?.SelectSingleNode("gpx:desc", namespaceManager)
        ' Pokud uzel <desc> neexistuje, vytvoříme jej a přidáme do <trk>
        If descNode Is Nothing Then
            ' Najdeme první uzel <trk>
            'Dim trkNode As XmlNode = xmlDoc.SelectSingleNode("/gpx:gpx/gpx:trk[1]", namespaceManager)
            descNode = xmlDoc.CreateElement("desc", "http://www.topografix.com/GPX/1/1")
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
        Return True


    End Function

    ' Function to read and calculate the length of only the first segment from the GPX file
    Private Function CalculateFirstSegmentDistance(i As Integer, xmlDoc As XmlDocument) As Double
        Dim totalLengthOfFirst_trkseg As Double = 0.0
        Dim lat1, lon1, lat2, lon2 As Double
        Dim firstPoint As Boolean = True

        ' Load the GPX file


        ' Create an XML Namespace Manager with the GPX namespace definition
        Dim namespaceManager As New XmlNamespaceManager(xmlDoc.NameTable)
        namespaceManager.AddNamespace("gpx", "http://www.topografix.com/GPX/1/1") ' GPX namespace URI

        ' Select the first track segment (<trkseg>) using the namespace
        Dim firstSegment As XmlNode = xmlDoc.SelectSingleNode("//gpx:trkseg", namespaceManager)

        ' If the segment exists, calculate its length
        If firstSegment IsNot Nothing Then
            ' Select all track points in the first segment (<trkpt>)
            Dim trackPoints As XmlNodeList = firstSegment.SelectNodes("gpx:trkpt", namespaceManager)

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
                            totalLengthOfFirst_trkseg += HaversineDistance(lat1, lon1, lat2, lon2)

                            ' Move the current point into lat1, lon1 for the next iteration
                            lat1 = lat2
                            lon1 = lon2
                        End If
                    End If
                Catch ex As Exception
                    ' Adding a more detailed exception message
                    Debug.WriteLine("Error: " & ex.Message)
                    ' TODO: Replace direct access to Form1 with a better method for separating logic
                    Form1.txtWarnings.AppendText("Error processing point: " & ex.Message & Environment.NewLine)
                End Try
            Next
        Else
            ' TODO: Replace direct access to Form1 with a better method for separating logic
            Form1.txtWarnings.AppendText("No segment found in GPX file: " & gpxFiles(i) & Environment.NewLine)
        End If

        Return totalLengthOfFirst_trkseg ' Result in kilometers
    End Function

    Public Sub Calculate(directorypath As String, startDate As DateTime, endDate As DateTime, PrependDatetoFileName As Boolean)
        Me.DirectoryPath = directorypath
        dateFrom = startDate
        dateTo = endDate

        gpxFiles.Clear()
        layerStart.Clear()
        dogStart.Clear()
        dogFinish.Clear()
        distances.Clear()
        totalDistances.Clear()
        age.Clear()
        speed.Clear()
        descriptions.Clear()

        gpxFiles = GetGpxFiles(Me.DirectoryPath)

        Try
            For i = 0 To gpxFiles.Count - 1
                Dim gpxfilePath As String = gpxFiles(i)

                Dim xmlDoc As New XmlDocument()
                Try
                    xmlDoc.Load(gpxFiles(i))
                Catch ex As Exception
                    ' Adding a more detailed exception message
                    Debug.WriteLine("Error: " & ex.Message)
                    ' TODO: Replace direct access to Form1 with a better method for separating logic
                    Form1.txtWarnings.AppendText($"File {gpxFiles(i)} could not be read: " & ex.Message & Environment.NewLine)
                End Try

                ' Start calculation using the values
                RenamewptNodes(i, xmlDoc, "předmět")
                layerStart.Add(GetLayerStart(gpxFiles(i)))
                SplitTrackIntoTwo(i, xmlDoc) 'in gpx files, splits a track with two segments into two separate tracks
                descriptions.Add(GetDescription(i, xmlDoc)) 'musí být první - slouží k výpočtu age
                distances.Add(CalculateFirstSegmentDistance(i, xmlDoc))
                If i = 0 Then totalDistances.Add(distances(i)) Else totalDistances.Add(totalDistances(i - 1) + distances(i))
                dogStart.Add(GetDogStart(i, xmlDoc))
                dogFinish.Add(GetDogFinish(i, xmlDoc))
                age.Add(CalculateAge(i, xmlDoc))
                speed.Add(CalculateSpeed(i))

                link.Add(Getlink(xmlDoc))
                If Not link(i) Is Nothing Then link(i) = $"=HYPERTEXTOVÝ.ODKAZ(""{link(i)}"")"

                xmlDoc.Save(gpxFiles(i)) 'hlavně kvůli desc
                'a nakonec
                SetCreatedModifiedDate(i)

                ' Display results
                Form1.txtOutput.AppendText(Path.GetFileNameWithoutExtension(gpxFiles(i)) & " Date: " & layerStart(i).Date.ToString("yyyy-MM-dd") & "  Distance: " & distances(i).ToString("F2") & " km" & Environment.NewLine)
            Next i


            totalDistance = totalDistances(gpxFiles.Count - 1)

            Form1.txtOutput.AppendText(vbCrLf & "Processed period: from " & startDate.ToString("dd.MM.yy") & " to " & endDate.ToString("dd.MM.yy") &
                vbCrLf & "All gpx files from directory: " & directorypath & vbCrLf &
                vbCrLf & "Total number of processed GPX files, thus trails: " & distances.Count &
                vbCrLf &
                vbCrLf & vbCrLf & "Total Route Distance: " & totalDistance.ToString("F2") & " km" & vbCrLf)

        Catch ex As Exception
            MessageBox.Show("An error occurred while processing data: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try







    End Sub

    Private Function GetDogStart(i As Integer, xmldoc As XmlDocument) As Date
        Dim namespaceManager As New XmlNamespaceManager(xmldoc.NameTable)
        namespaceManager.AddNamespace("gpx", "http://www.topografix.com/GPX/1/1") ' GPX namespace URI
        Dim trksegNodes As XmlNodeList = xmldoc.SelectNodes("//gpx:trkseg", namespaceManager)
        Dim dogStart As DateTime



        If trksegNodes.Count > 1 Then

            Dim dogtimeNodes As XmlNodeList = trksegNodes(1).SelectNodes("gpx:trkpt/gpx:time", namespaceManager)

            Dim DogStartTimeNode As XmlNode = dogtimeNodes(0)
            DateTime.TryParse(DogStartTimeNode.InnerText, dogStart)

        End If
        Return dogStart

    End Function
    Private Function GetDogFinish(i As Integer, xmldoc As XmlDocument) As Date
        Dim namespaceManager As New XmlNamespaceManager(xmldoc.NameTable)
        namespaceManager.AddNamespace("gpx", "http://www.topografix.com/GPX/1/1") ' GPX namespace URI
        Dim trksegNodes As XmlNodeList = xmldoc.SelectNodes("//gpx:trkseg", namespaceManager)
        Dim dogFinish As DateTime



        If trksegNodes.Count > 1 Then

            Dim dogtimeNodes As XmlNodeList = trksegNodes(1).SelectNodes("gpx:trkpt/gpx:time", namespaceManager)

            Dim DogFinishTimeNode As XmlNode = dogtimeNodes(dogtimeNodes.Count - 1)
            DateTime.TryParse(DogFinishTimeNode.InnerText, dogFinish)
        End If
        Return dogFinish

    End Function

    ' Get a list of all GPX files in the specified directory
    Public Function GetGpxFiles(directorypath As String) As List(Of String)
        Try

            ' Načteme všechny GPX soubory
            Dim _gpxFiles As List(Of String) = Directory.GetFiles(directorypath, "*.gpx").ToList()





            ' Filtrujeme soubory podle podmínky
            For i As Integer = 0 To _gpxFiles.Count - 1
                Dim _layerStart As DateTime = GetLayerStart(_gpxFiles(i))
                If _layerStart >= dateFrom And _layerStart <= dateTo Then
                    gpxFiles.Add(_gpxFiles(i))
                    'layerStart.Add(_layerStart)
                End If
            Next


            For i = 0 To gpxFiles.Count - 1

                ChangeFilename(i)
            Next i


            gpxFiles.Sort()

            Return gpxFiles
        Catch ex As Exception
            ' Adding a more detailed exception message
            Debug.WriteLine("Error: " & ex.Message)
            Return Nothing
        End Try





    End Function



    Public Sub ChangeFilename(i As Integer)


        Dim fileName As String = Path.GetFileNameWithoutExtension(gpxFiles(i))
        Dim fileExtension As String = Path.GetExtension(gpxFiles(i))
        Dim _layerStart As DateTime = GetLayerStart(gpxFiles(i))

        Dim newFileName As String
        Dim newFilePath As String


        Dim dateTimeFromFileName As DateTime
        Try


            If Regex.IsMatch(fileName, "(?:(\d{4})[-/.](\d{2})[-/.](\d{2})|(\d{2})[-/.](\d{2})[-/.](\d{4}))") Then
                ' If Regex.IsMatch(fileName, "^\d{4}-\d{2}-\d{2}") Then
                ' Extrahování data z názvu souboru
                Dim dateMatch As Match = Regex.Match(fileName, , "(?:(\d{4})[-/.](\d{2})[-/.](\d{2})|(\d{2})[-/.](\d{2})[-/.](\d{4}))")
                If dateMatch.Success Then
                    ' Převedení nalezeného řetězce na DateTime
                    dateTimeFromFileName = DateTime.ParseExact(dateMatch.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                End If

                'zkontroluje, zda je datum v názvu správné

                If dateTimeFromFileName <> Date.MinValue AndAlso dateTimeFromFileName.Date.ToShortDateString <> _layerStart.Date.ToShortDateString Then
                    ' Nahrazení staré hodnoty za novou v názvu souboru
                    newFileName = Regex.Replace(fileName, "^(?:(\d{4})[-/.](\d{2})[-/.](\d{2})|(\d{2})[-/.](\d{2})[-/.](\d{4}))", _layerStart.ToString("yyyy-MM-dd"))
                    newFilePath = Path.Combine(DirectoryPath, newFileName & ".gpx")
                    File.Move(gpxFiles(i), newFilePath)
                    Form1.txtWarnings.AppendText($"Renamed file: {Path.GetFileName(gpxFiles(i))} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                    gpxFiles(i) = newFilePath
                End If
            End If

            ' když na začátku jména souboru není datum, pokusí se ho přidat
            If Not Regex.IsMatch(fileName, "^\d{4}-\d{2}-\d{2}") Then
                newFileName = $"{_layerStart.Date.ToString("yyyy-MM-dd")}{fileName}{fileExtension}"
                newFilePath = Path.Combine(DirectoryPath, newFileName)
                If Form1.chbDateToName.Checked Then
                    If File.Exists(newFilePath) Then
                        ' Handle existing files
                        Dim userInput As String = InputBox($"File {newFilePath} already exists. Enter a new name:")
                        If Not String.IsNullOrWhiteSpace(userInput) Then
                            newFilePath = Path.Combine(DirectoryPath, userInput & fileExtension)
                            File.Move(gpxFiles(i), newFilePath)
                            Form1.txtWarnings.AppendText($"Renamed file: {Path.GetFileName(gpxFiles(i))} to {Path.GetFileName(newFilePath)}.{Environment.NewLine}")
                        Else
                            Form1.txtWarnings.AppendText($"New name for {newFilePath} was not provided.{Environment.NewLine}")

                        End If
                    Else
                        File.Move(gpxFiles(i), newFilePath)
                        gpxFiles(i) = newFilePath
                    End If

                End If
            End If
        Catch ex As Exception
            MessageBox.Show(ex.Message)
        End Try
    End Sub


    Sub SetCreatedModifiedDate(i)
        'change of attributes
        ' Setting the file creation date
        File.SetCreationTime(gpxFiles(i), layerStart(i))
        ' Setting the last modified file date
        File.SetLastWriteTime(gpxFiles(i), layerStart(i))
    End Sub

    Public Sub WriteCSVfile(csvFilePath As String)
        Try

            ' Create the CSV file and write headers
            Using writer As New StreamWriter(csvFilePath, False, System.Text.Encoding.UTF8)
                writer.WriteLine("File Name;Date;Age/h;Distance/km;speed;Total Distance;Description;Video")

                For i As Integer = 0 To distances.Count - 1
                    Dim fileName As String = Path.GetFileNameWithoutExtension(gpxFiles(i))

                    Dim _age As String = ""
                    If age(i) > TimeSpan.Zero Then
                        _age = age(i).TotalHours.ToString("F1")
                    End If

                    ' Write each row in the CSV file
                    writer.Write($"{fileName};")
                    writer.Write($"{layerStart(i).ToString("yyyy-MM-dd")};")
                    writer.Write($"{_age};")
                    writer.Write($"{distances(i):F2};")
                    If Not speed(i) = 0 Then writer.Write($"{speed(i):F2};") Else writer.Write(";")
                    writer.Write($"{totalDistances(i):F2};")
                    writer.Write($"{descriptions(i)};")
                    writer.WriteLine($"{link(i)}")

                Next

                ' Write the total distance at the end of the CSV file
                writer.WriteLine($"Total;;; {TotalDistances(distances.Count - 1):F2}")
            End Using


            Form1.txtWarnings.AppendText($"CSV file created: {csvFilePath}.{Environment.NewLine}")
        Catch ex As Exception
            Form1.txtWarnings.AppendText($"Error creating CSV file: {ex.Message}{Environment.NewLine}")
        End Try
    End Sub


    Public Function SaveAsCsvFile(csvFilename As String) As String
        ' Vytvoření instance SaveFileDialog
        ' Nastavení filtrů a výchozí přípony
        Dim saveFileDialog As New SaveFileDialog With {
            .Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
            .DefaultExt = "csv",
            .AddExtension = True,
            .Title = "A file with this name already exists, Save As:",
            .FileName = csvFilename,
            .InitialDirectory = DirectoryPath
        }

        ' Zobrazení dialogového okna pro "Uložit Jako"
        If saveFileDialog.ShowDialog() = DialogResult.OK Then
            ' Získání cesty k novému souboru z dialogu
            Dim newFilePath As String = saveFileDialog.FileName

            Try
                Me.WriteCSVfile(newFilePath)
            Catch ex As Exception
                MessageBox.Show("Chyba při ukládání souboru: " & ex.Message, "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
            Return newFilePath
        Else
            MessageBox.Show("Uložení bylo zrušeno.", "Uložení souboru", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return Nothing
        End If
    End Function


    ' in gpx files, splits a track with two segments into two separate tracks
    Sub SplitTrackIntoTwo(i As Integer, ByRef xmlDoc As XmlDocument)


        Dim namespaceManager As New XmlNamespaceManager(xmlDoc.NameTable)
        namespaceManager.AddNamespace("gpx", "http://www.topografix.com/GPX/1/1") ' GPX namespace URI

        ' Najdi první uzel <trk>
        Dim trkNode As XmlNode = xmlDoc.SelectSingleNode("//gpx:trk", namespaceManager)

        If trkNode IsNot Nothing Then
            ' Najdi všechny <trkseg> uvnitř <trk>
            Dim trkSegNodes As XmlNodeList = trkNode.SelectNodes("gpx:trkseg", namespaceManager)

            If trkSegNodes.Count > 1 Then
                ' Vytvoř nový uzel <trk>
                Dim newTrkNode As XmlNode = xmlDoc.CreateElement("trk", "http://www.topografix.com/GPX/1/1")

                ' Přesuň druhý <trkseg> do nového <trk>
                Dim secondTrkSeg As XmlNode = trkSegNodes(1)
                trkNode.RemoveChild(secondTrkSeg)
                newTrkNode.AppendChild(secondTrkSeg)

                ' Přidej nový <trk> do dokumentu hned po prvním
                trkNode.ParentNode.InsertAfter(newTrkNode, trkNode)
                xmlDoc.Save(gpxFiles(i))
                Form1.txtWarnings.AppendText($"Track in file {gpxFiles(i)} was successfully split.")
            End If
        End If
    End Sub


    Sub RenamewptNodes(i As Integer, ByRef xmlDoc As XmlDocument, newname As String)

        Dim namespaceManager As New XmlNamespaceManager(xmlDoc.NameTable)
        namespaceManager.AddNamespace("gpx", "http://www.topografix.com/GPX/1/1") ' GPX namespace URI


        ' traverses all <wpt> nodes in the GPX file and overwrites the value of <name> nodes to "-předmět":
        ' Find all <wpt> nodes using the namespace
        Dim wptNodes As XmlNodeList = xmlDoc.SelectNodes("//gpx:wpt", namespaceManager)

        ' Go through each <wpt> node
        For Each wptNode As XmlNode In wptNodes
            ' Najdi uzel <name> uvnitř <wpt> s použitím namespace
            Dim nameNode As XmlNode = wptNode.SelectSingleNode("gpx:name", namespaceManager)

            If nameNode IsNot Nothing AndAlso nameNode.InnerText <> newname Then
                ' Přepiš hodnotu <name> na newname
                nameNode.InnerText = "předmět"
            End If
        Next
    End Sub

End Class

