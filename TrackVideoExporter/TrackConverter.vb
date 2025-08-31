Imports System.Drawing
Imports System.Runtime.CompilerServices.RuntimeHelpers
Imports System.Security.Cryptography
Imports System.Windows.Forms
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar
Imports System.Xml
Imports Microsoft.VisualBasic.Logging

''' <summary>
''' Provides conversion of GPX data between various internal formats used for video rendering.
''' </summary>
Public Class TrackConverter
    ''' <summary>
    ''' Raised when a non-critical warning occurs during processing.
    ''' </summary>
    Public Event WarningOccurred(message As String, _color As Color)
    ''' <summary>
    ''' Minimum latitude found in all tracks.
    ''' </summary>
    Public minLat As Double = Double.MaxValue

    ''' <summary>
    ''' Maximum latitude found in all tracks.
    ''' </summary>
    Public maxLat As Double = Double.MinValue

    ''' <summary>
    ''' Minimum longitude found in all tracks.
    ''' </summary>
    Public minLon As Double = Double.MaxValue

    ''' <summary>
    ''' Maximum longitude found in all tracks.
    ''' </summary>
    Public maxLon As Double = Double.MinValue

    ''' <summary>
    ''' Initializes a new instance of the <see cref="TrackConverter"/> class.
    ''' </summary>
    Public Sub New()
    End Sub

    ''' <summary>
    ''' Converts a list of GPX track nodes to a list of track points with XML nodes.
    ''' </summary>
    ''' <param name="_tracksAsTrkNode">List of track nodes containing raw GPX XML data.</param>
    ''' <returns>List of <see cref="TrackAsTrkPts"/> representing extracted track points.</returns>
    Public Function ConvertTracksAsTrkNodesToTrackAsTrkPts(_tracksAsTrkNode As List(Of TrackAsTrkNode)) As List(Of TrackAsTrkPts)
        Dim tracksAsTrkPts As New List(Of TrackAsTrkPts)
        For Each track In _tracksAsTrkNode
            Dim _TrackAsTrkPts As TrackAsTrkPts = ConvertTrackAsTrkNodeToTrkPts(track)
            tracksAsTrkPts.Add(_TrackAsTrkPts)
        Next
        Return tracksAsTrkPts
    End Function

    Public Function ConvertTrackAsTrkNodeToTrkPts(track As TrackAsTrkNode) As TrackAsTrkPts
        Dim trkptNodes As XmlNodeList = SelectTrkptNodes(track.TrkNode)
        Dim _TrackAsTrkPts As New TrackAsTrkPts(track.TrackType, trkptNodes)
        Return _TrackAsTrkPts
    End Function

    ''' <summary>
    ''' Converts XML track points to geographical points with timestamps.
    ''' </summary>
    ''' <param name="_tracksAsTrkPts">List of tracks containing XML track point nodes.</param>
    ''' <returns>List of <see cref="TrackAsGeoPoints"/> with lat/lon and time.</returns>
    Public Function ConvertTracksTrkPtsToGeoPoints(_tracksAsTrkPts As List(Of TrackAsTrkPts)) As List(Of TrackAsGeoPoints)
        Dim tracksAsGeoPoints As New List(Of TrackAsGeoPoints)
        For Each track In _tracksAsTrkPts

            Dim _TrackAsGeoPoints = ConvertTrackTrkPtsToGeoPoints(track)

            tracksAsGeoPoints.Add(_TrackAsGeoPoints)
        Next
        Return tracksAsGeoPoints
    End Function

    Public Function ConvertTrackTrkPtsToGeoPoints(track As TrackAsTrkPts) As TrackAsGeoPoints
        Dim trackGeoPoints As New List(Of TrackGeoPoint)
        For Each trkptnode As XmlNode In track.TrackPoints
            Dim lat = Double.Parse(trkptnode.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
            Dim lon = Double.Parse(trkptnode.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)
            Dim timenode = SelectSingleChildNode("time", trkptnode)
            Dim time As DateTime
            If timenode IsNot Nothing Then
                time = DateTime.Parse(timenode.InnerText, Nothing, Globalization.DateTimeStyles.AssumeUniversal)
            Else
                Debug.WriteLine("Time node not found in trkpt.")
                time = DateTime.MinValue
            End If

            Dim geopoint As New TrackGeoPoint With {
                .Location = New Coordinates With {.Lat = lat, .Lon = lon},
                .Time = time
            }
            trackGeoPoints.Add(geopoint)
        Next
        Dim _TrackAsGeoPoints As New TrackAsGeoPoints(track.TrackType, trackGeoPoints)
        Return _TrackAsGeoPoints
    End Function


    ''' <summary>
    ''' Calculates bounding box (min/max lat/lon) from a list of geographical tracks.
    ''' </summary>
    ''' <param name="_tracksAsGeoPoints">List of tracks containing geographical points.</param>
    Public Sub SetCoordinatesBounds(_tracksAsGeoPoints As List(Of TrackAsGeoPoints))
        For Each Track In _tracksAsGeoPoints
            For Each geoPoint As TrackGeoPoint In Track.TrackGeoPoints
                minLat = Math.Min(minLat, geoPoint.Location.Lat)
                maxLat = Math.Max(maxLat, geoPoint.Location.Lat)
                minLon = Math.Min(minLon, geoPoint.Location.Lon)
                maxLon = Math.Max(maxLon, geoPoint.Location.Lon)
            Next
        Next
    End Sub

    ''' <summary>
    ''' Converts geographical points to pixel coordinates for drawing on a map image.
    ''' </summary>
    ''' <param name="_tracksAsGeoPoints">List of geographical tracks.</param>
    ''' <param name="minTileX">X index of the top-left map tile.</param>
    ''' <param name="minTileY">Y index of the top-left map tile.</param>
    ''' <param name="zoom">Zoom level of the tile map.</param>
    ''' <returns>List of <see cref="TrackAsPointsF"/> with 2D screen coordinates and timestamps.</returns>
    Public Function ConvertTracksGeoPointsToPointsF(_tracksAsGeoPoints As List(Of TrackAsGeoPoints), minTileX As Single, minTileY As Single, zoom As Integer) As List(Of TrackAsPointsF)
        Dim latDistancePerDegree As Double = 111_320.0
        Dim centerLat As Double = (minLat + maxLat) / 2
        Dim lonDistancePerDegree As Double = Math.Cos(centerLat * Math.PI / 180) * latDistancePerDegree
        Dim widthInMeters As Double = (maxLon - minLon) * lonDistancePerDegree
        Dim heightInMeters As Double = (maxLat - minLat) * latDistancePerDegree

        Dim _tracksAsPointsF As New List(Of TrackAsPointsF)
        For Each Track In _tracksAsGeoPoints
            Dim _TrackAsPointsF = ConvertTrackGeoPointsToPointsF(Track, minTileX, minTileY, zoom)
            _tracksAsPointsF.Add(_TrackAsPointsF)
        Next
        Return _tracksAsPointsF
    End Function

    Public Function ConvertTrackGeoPointsToPointsF(track As TrackAsGeoPoints, minTileX As Single, minTileY As Single, zoom As Integer) As TrackAsPointsF
        Dim _TrackAsPointsF As New TrackAsPointsF(track.TrackType, New List(Of TrackPointF))
        For Each geoPoint As TrackGeoPoint In track.TrackGeoPoints
            Dim pt = LatLonToPixel(geoPoint.Location.Lat, geoPoint.Location.Lon, zoom, minTileX, minTileY)
            Dim _trackpointF As New TrackPointF With {
                .Location = New PointF With {.X = pt.X, .Y = pt.Y},
                .Time = geoPoint.Time
            }
            _TrackAsPointsF.TrackPointsF.Add(_trackpointF)
        Next
        Return _TrackAsPointsF
    End Function

    ''' <summary>
    ''' Converts geographical coordinates (lat, lon) to pixel coordinates within a composite tile image.
    ''' </summary>
    ''' <param name="lat">Latitude in decimal degrees.</param>
    ''' <param name="lon">Longitude in decimal degrees.</param>
    ''' <param name="zoom">Zoom level of the tile map.</param>
    ''' <param name="minTileX">X index of the top-left tile.</param>
    ''' <param name="minTileY">Y index of the top-left tile.</param>
    ''' <returns>PointF with X and Y pixel positions in the tile image.</returns>
    Function LatLonToPixel(lat As Double, lon As Double, zoom As Integer, minTileX As Integer, minTileY As Integer) As PointF
        Dim n = Math.Pow(2, zoom)
        Dim tileX = (lon + 180.0) / 360.0 * n
        Dim tileY = (1 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 1 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2 * n
        Dim pixelX = CSng((tileX - minTileX) * 256)
        Dim pixelY = CSng((tileY - minTileY) * 256)
        Return New PointF(pixelX, pixelY)
    End Function

    ''' <summary>
    ''' Selects all &lt;trkpt&gt; nodes from a GPX track node.
    ''' </summary>
    ''' <param name="trkNode">The GPX &lt;trk&gt; node.</param>
    ''' <returns>XmlNodeList containing all &lt;trkpt&gt; elements.</returns>
    Function SelectTrkptNodes(trkNode As XmlNode) As XmlNodeList
        Dim nsmgr As New XmlNamespaceManager(trkNode.OwnerDocument.NameTable)
        Dim ns As String = trkNode.GetNamespaceOfPrefix("")
        nsmgr.AddNamespace("gpx", ns)
        Return trkNode.SelectNodes(".//gpx:trkpt", nsmgr)
    End Function

    ''' <summary>
    ''' Selects a single child node from a parent node, using the GPX namespace.
    ''' </summary>
    ''' <param name="childName">Name of the child element to select (e.g., "time").</param>
    ''' <param name="parent">The parent XmlNode (e.g., trkpt).</param>
    ''' <returns>The selected XmlNode, or Nothing if not found.</returns>
    Public Function SelectSingleChildNode(childName As String, parent As XmlNode) As XmlNode
        Dim nsmgr As New XmlNamespaceManager(parent.OwnerDocument.NameTable)
        Dim ns As String = parent.GetNamespaceOfPrefix("")
        nsmgr.AddNamespace("gpx", ns)
        Return parent.SelectSingleNode($"gpx:{childName}", nsmgr)
    End Function

    ' Metoda pro výběr poduzlů z uzlu Node

    Public Function SelectChildNodes(childName As String, parent As XmlNode) As XmlNodeList
        Dim nsmgr As New XmlNamespaceManager(parent.OwnerDocument.NameTable)
        Dim ns As String = parent.GetNamespaceOfPrefix("")
        nsmgr.AddNamespace("gpx", ns)
        Return parent.SelectNodes($"gpx:{childName}", nsmgr)
    End Function

    Public Function CreateAndAddElement(parentNode As XmlElement,
                                XpathchildNodeName As String,
                                value As String,
                                insertAfter As Boolean,
                                Optional attName As String = "",
                                Optional attValue As String = ""
                               ) As XmlNode



        Dim childNodes As XmlNodeList = SelectAllChildNodes(XpathchildNodeName, parentNode)

        ' Kontrola duplicity
        For Each node As XmlNode In childNodes
            If (node.Attributes(attName)?.Value = attValue) Then ' zkontroluje zda node s atributem attvalue už neexistuje:
                'node.RemoveAll() ' odstraní všechny podřízené uzly, pokud existují
                node.InnerText = value ' nastaví text na nový
                'If node IsNot Nothing AndAlso node.ParentNode IsNot Nothing Then
                '    node.ParentNode.RemoveChild(node)
                'End If
                Return node ' nalezen existující uzel, končíme
            End If
        Next

        ' Pokud jsme žádný nenalezli, tak ho přidáme
        Dim insertedNode As XmlNode = Nothing
        Dim childNode As XmlElement = CreateElement(XpathchildNodeName, parentNode)
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

    ' Metoda pro rekurentní výběr všech poduzlů z uzlu Node
    Public Function SelectAllChildNodes(XpathChildName As String, node As XmlNode) As XmlNodeList
        Dim nsmgr As New XmlNamespaceManager(node.OwnerDocument.NameTable)
        Dim ns As String = node.GetNamespaceOfPrefix("")
        nsmgr.AddNamespace("gpx", ns)
        Return node.SelectNodes(".//" & XpathChildName, nsmgr)
    End Function

    Public Function CreateElement(nodename As String, parent As XmlNode, Optional _namespaceUri As String = Nothing) As XmlNode
        Dim xmlDoc As XmlDocument = parent.OwnerDocument
        If _namespaceUri IsNot Nothing Then
            ' Pokud je zadán jmenný prostor, použijeme ho

            Return xmlDoc.CreateElement(nodename, _namespaceUri)
        End If
        Return xmlDoc.CreateElement(nodename, xmlDoc.DocumentElement.NamespaceURI)
    End Function


    'Public Function CalculateTrailDistance(trkNode As XmlNode) As Double
    '    Dim totalLengthOfFirst_trkseg As Double = -1.0F
    '    Dim lat1, lon1, lat2, lon2 As Double
    '    Dim firstPoint As Boolean = True

    '    Dim timeNode As XmlNode = Nothing

    '    Dim firstSegment As XmlNode = SelectSingleChildNode("trkseg", trkNode)
    '    ' If the segment exists, calculate its length
    '    If firstSegment IsNot Nothing Then
    '        ' Select all track points in the first segment (<trkpt>)
    '        Dim trackPoints As XmlNodeList = SelectChildNodes("trkpt", firstSegment)

    '        ' Calculate the distance between each point in the first segment
    '        For Each point As XmlNode In trackPoints
    '            Try
    '                ' Check if attributes exist and load them as Double
    '                If point.Attributes("lat") IsNot Nothing AndAlso point.Attributes("lon") IsNot Nothing Then
    '                    Dim lat As Double = Convert.ToDouble(point.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
    '                    Dim lon As Double = Convert.ToDouble(point.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)

    '                    If firstPoint Then
    '                        ' Initialize the first point
    '                        lat1 = lat
    '                        lon1 = lon
    '                        firstPoint = False
    '                    Else
    '                        ' Calculate the distance between the previous and current point
    '                        lat2 = lat
    '                        lon2 = lon
    '                        If totalLengthOfFirst_trkseg < 0 Then totalLengthOfFirst_trkseg = 0 'odstranění počáteční hodnoty -1
    '                        totalLengthOfFirst_trkseg += HaversineDistance(lat1, lon1, lat2, lon2, "km")

    '                        ' Move the current point into lat1, lon1 for the next iteration
    '                        lat1 = lat2
    '                        lon1 = lon2
    '                    End If
    '                End If
    '            Catch ex As Exception
    '                ' Adding a more detailed exception message
    '                Debug.WriteLine("Error: " & ex.Message)
    '                RaiseEvent WarningOccurred("Error processing point: " & ex.Message & Environment.NewLine, Color.Red)
    '            End Try
    '        Next
    '    End If

    '    Return totalLengthOfFirst_trkseg ' Result in kilometers

    'End Function

    ' Enum pro reprezentaci stavu (pohyb / stání).
    Private Enum MovementState
        Moving
        Stopped
    End Enum
    ' Struktura pro vrácení výsledků výpočtu.
    Public Structure TrackStats
        Public Property DistanceKm As Double
        Public Property TotalTime As TimeSpan
        Public Property MovingTime As TimeSpan
        Public Property StoppedTime As TimeSpan
        Public Property SpeedKmh As Double
        Public Property Deviation As Double
    End Structure
    Public Function CalculateTrackStats(trkNode As XmlNode, Optional anotherTrkNode As XmlNode = Nothing) As TrackStats
        Dim trkAsGeoPoints As TrackAsGeoPoints = ConvertTrackTrkPtsToGeoPoints(ConvertTrackAsTrkNodeToTrkPts(New TrackAsTrkNode(trkNode, trackType:=TrackType.Unknown)))

        ' --- Nastavení prahových hodnot podle GPXSee ---
        Const MOVING_SPEED_THRESHOLD_MS As Double = 0.277  ' m/s (1.0 km/h)
        Const STOPPED_SPEED_THRESHOLD_MS As Double = 0.1 ' m/s (0.36 km/h)

        Dim totalMovingTime As TimeSpan = TimeSpan.Zero
        Dim totalStoppedTime As TimeSpan = TimeSpan.Zero
        Dim totalDistanceKm As Double = -1.0F
        Dim currentState As MovementState
        Dim SpeedKmh As Double = -1.0F

        Dim trackGeoPoints As List(Of TrackGeoPoint) = trkAsGeoPoints.TrackGeoPoints
        Dim anotherTrkAsGeoPoints As TrackAsGeoPoints
        Dim runnerGeoPoints As List(Of TrackGeoPoint)
        ' Převedeme runnera do XY
        Dim runnerXY As New List(Of (X As Double, Y As Double))
        Dim lat0 As Double
        Dim lon0 As Double
        If anotherTrkNode IsNot Nothing Then
            anotherTrkAsGeoPoints = ConvertTrackTrkPtsToGeoPoints(ConvertTrackAsTrkNodeToTrkPts(New TrackAsTrkNode(anotherTrkNode, trackType:=TrackType.Unknown)))
            runnerGeoPoints = anotherTrkAsGeoPoints.TrackGeoPoints
            ' Převedeme runnera do XY
            lat0 = runnerGeoPoints(0).Location.Lat
            lon0 = runnerGeoPoints(0).Location.Lon
            For Each rpoint As TrackGeoPoint In runnerGeoPoints
                Dim lat As Double = rpoint.Location.Lat 'Convert.ToDouble(rpoint.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
                Dim lon As Double = rpoint.Location.Lon 'Convert.ToDouble(rpoint.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)
                Dim x, y As Double
                LatLonToXY(lat, lon, lat0, lon0, x, y)
                runnerXY.Add((x, y))
            Next
        End If
        Dim totalDeviation As Double = 0F

        Dim segIndex As Integer = 0

        ' --- Hlavní smyčka pro procházení bodů ---
        For i As Integer = 0 To trackGeoPoints.Count - 2
            Try
                Dim point1 As TrackGeoPoint = trackGeoPoints(i)
                Dim point2 As TrackGeoPoint = trackGeoPoints(i + 1)


                Dim timeDiff As TimeSpan = point2.Time - point1.Time
                Dim distanceMeters As Double = HaversineDistance(point1.Location.Lat, point1.Location.Lon, point2.Location.Lat, point2.Location.Lon, "m")
                If distanceMeters > 0 Then
                    If totalDistanceKm < 0 Then totalDistanceKm = 0 'odstranění počáteční hodnoty -1
                End If
                ' Přeskočíme segmenty s nulovým nebo záporným časem
                If timeDiff.TotalSeconds <= 0 Then
                    totalDistanceKm += distanceMeters / 1000.0F 'tohle je situace, když čas chybí, nutno zaočítat!
                    Continue For
                End If

                Dim speedMs As Double = distanceMeters / timeDiff.TotalSeconds


                ' --- Logika stavového automatu (hystereze) ---
                ' Pro první segment musíme určit počáteční stav
                If i = 0 Then
                    currentState = If(speedMs > MOVING_SPEED_THRESHOLD_MS, MovementState.Moving, MovementState.Stopped)
                Else
                    ' Pro další segmenty měníme stav pouze při překročení opačné hranice
                    If currentState = MovementState.Moving AndAlso speedMs < STOPPED_SPEED_THRESHOLD_MS Then
                        currentState = MovementState.Stopped
                    ElseIf currentState = MovementState.Stopped AndAlso speedMs > MOVING_SPEED_THRESHOLD_MS Then
                        currentState = MovementState.Moving
                    End If
                End If

                ' --- Sčítání časů podle aktuálního stavu ---
                If currentState = MovementState.Moving Then
                    totalMovingTime = totalMovingTime.Add(timeDiff)
                    totalDistanceKm += distanceMeters / 1000.0F 'počítá se jen pohyb


                    ' Výpočet odchylky od druhé trasy:
                    If anotherTrkNode IsNot Nothing Then
                        Dim qx, qy As Double
                        LatLonToXY(point1.Location.Lat, point1.Location.Lon, lat0, lon0, qx, qy)

                        ' Najdeme nejbližší bod na aktuálním + sousedních segmentech
                        Dim windowSize As Int16 = runnerXY.Count / 2
                        Dim minDist As Double = Double.MaxValue
                        For j = Math.Max(0, segIndex - windowSize) To Math.Min(runnerXY.Count - 2, segIndex + windowSize)
                            Dim p1 = runnerXY(j)
                            Dim p2 = runnerXY(j + 1)
                            Dim dx As Double = p2.X - p1.X
                            Dim dy As Double = p2.Y - p1.Y
                            Dim t As Double = ((qx - p1.X) * dx + (qy - p1.Y) * dy) / (dx * dx + dy * dy)
                            t = Math.Max(0, Math.Min(1, t))
                            Dim projX As Double = p1.X + t * dx
                            Dim projY As Double = p1.Y + t * dy
                            Dim dist As Double = Math.Sqrt((qx - projX) ^ 2 + (qy - projY) ^ 2)
                            If dist < minDist Then
                                minDist = dist
                                segIndex = j ' posuneme okno dopředu
                            End If
                        Next j
                        totalDeviation += minDist * timeDiff.TotalSeconds ' vážená odchylka v metrech * sekundách
                    End If
                Else
                        totalStoppedTime = totalStoppedTime.Add(timeDiff)
                End If

            Catch ex As Exception
                ' Adding a more detailed exception message
                Debug.WriteLine("Error: " & ex.Message)
                RaiseEvent WarningOccurred("Error processing point: " & ex.Message & Environment.NewLine, Color.Red)
            End Try
        Next i
        'End If


        Return New TrackStats With {
                .DistanceKm = totalDistanceKm,
                .MovingTime = totalMovingTime,
                .StoppedTime = totalStoppedTime,
                .Deviation = If(anotherTrkNode IsNot Nothing AndAlso totalMovingTime.TotalSeconds > 0, totalDeviation / (totalMovingTime.TotalSeconds), -1.0F), ' průměrná odchylka v metrech
                .TotalTime = totalMovingTime + totalStoppedTime,
                .SpeedKmh = If(totalMovingTime.TotalHours > 0, totalDistanceKm / totalMovingTime.TotalHours, -1.0F)
            }

    End Function



    'Public Function CalculateDeviation(dogTrkNode As XmlNode, runnerTrkNode As XmlNode) As Double?
    '    Dim totalDeviation As Double = 0.0
    '    Dim lat1, lon1, lat2, lon2 As Double

    '    Dim timeNode As XmlNode = Nothing

    '    Dim dogSegment As XmlNode = SelectSingleChildNode("trkseg", dogTrkNode)
    '    Dim runnerSegment As XmlNode = SelectSingleChildNode("trkseg", runnerTrkNode)
    '    ' If the segment exists, calculate its length
    '    If dogSegment IsNot Nothing AndAlso runnerSegment IsNot Nothing Then
    '        ' Select all track points in the first segment (<trkpt>)
    '        Dim dogTrackPoints As XmlNodeList = SelectChildNodes("trkpt", dogSegment)
    '        Dim runnerTrackPoints As XmlNodeList = SelectChildNodes("trkpt", runnerSegment)

    '        ' Calculate the distance between each point in the first segment
    '        For Each dpoint As XmlNode In dogTrackPoints
    '            Try
    '                ' Check if attributes exist and load them as Double
    '                If dpoint.Attributes("lat") IsNot Nothing AndAlso dpoint.Attributes("lon") IsNot Nothing Then
    '                    lat1 = Convert.ToDouble(dpoint.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
    '                    lon1 = Convert.ToDouble(dpoint.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)

    '                    Dim deviationForThisPoint As Double = Double.MaxValue
    '                    For Each rpoint As XmlNode In runnerTrackPoints
    '                        ' Check if attributes exist and load them as Double
    '                        If rpoint.Attributes("lat") IsNot Nothing AndAlso rpoint.Attributes("lon") IsNot Nothing Then
    '                            lat2 = Convert.ToDouble(rpoint.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
    '                            lon2 = Convert.ToDouble(rpoint.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)

    '                            ' Calculate the distance between the previous and current point
    '                            Dim deviationForThisRunnerPoint As Double = HaversineDistance(lat1, lon1, lat2, lon2, "m")
    '                            If deviationForThisRunnerPoint < deviationForThisPoint Then
    '                                deviationForThisPoint = deviationForThisRunnerPoint
    '                            End If
    '                        End If
    '                    Next
    '                    totalDeviation += deviationForThisPoint
    '                End If
    '            Catch ex As Exception
    '                ' Adding a more detailed exception message
    '                Debug.WriteLine("Error: " & ex.Message)
    '                'RaiseEvent WarningOccurred("Error processing point: " & ex.Message & Environment.NewLine, Color.Red)
    '            End Try
    '        Next
    '        Return totalDeviation / dogTrackPoints.Count ' Average deviation in meters
    '    End If
    '    Return Nothing
    'End Function

    Public Function CalculateDeviationProjection(dogTrkNode As XmlNode, runnerTrkNode As XmlNode) As Double
        Dim dogTrkAsGeoPoints As TrackAsGeoPoints = ConvertTrackTrkPtsToGeoPoints(ConvertTrackAsTrkNodeToTrkPts(New TrackAsTrkNode(dogTrkNode, trackType:=TrackType.Unknown)))
        Dim runnerTrkAsGeoPoints As TrackAsGeoPoints = ConvertTrackTrkPtsToGeoPoints(ConvertTrackAsTrkNodeToTrkPts(New TrackAsTrkNode(runnerTrkNode, trackType:=TrackType.Unknown)))


        Dim totalDeviation As Double = 0F


        Dim dogGeoPoints As List(Of TrackGeoPoint) = dogTrkAsGeoPoints.TrackGeoPoints
        Dim runnerGeoPoints As List(Of TrackGeoPoint) = runnerTrkAsGeoPoints.TrackGeoPoints

        ' Převedeme runnera do XY
        Dim runnerXY As New List(Of (X As Double, Y As Double))
        Dim lat0 As Double = runnerGeoPoints(0).Location.Lat
        Dim lon0 As Double = runnerGeoPoints(0).Location.Lon

        For Each rpoint As TrackGeoPoint In runnerGeoPoints
            Dim lat As Double = rpoint.Location.Lat 'Convert.ToDouble(rpoint.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
            Dim lon As Double = rpoint.Location.Lon 'Convert.ToDouble(rpoint.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)
            Dim x, y As Double
            LatLonToXY(lat, lon, lat0, lon0, x, y)
            runnerXY.Add((x, y))
        Next

        Dim segIndex As Integer = 0
        For Each dpoint As TrackGeoPoint In dogGeoPoints
            Dim lat As Double = dpoint.Location.Lat 'Convert.ToDouble(rpoint.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
            Dim lon As Double = dpoint.Location.Lon 'Convert.ToDouble(rpoint.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)
            Dim qx, qy As Double
            LatLonToXY(lat, lon, lat0, lon0, qx, qy)

            ' Najdeme nejbližší bod na aktuálním + sousedních segmentech
            Dim windowSize As Int16 = runnerXY.Count / 2
            Dim minDist As Double = Double.MaxValue
            For i = Math.Max(0, segIndex - windowSize) To Math.Min(runnerXY.Count - 2, segIndex + windowSize)
                Dim p1 = runnerXY(i)
                Dim p2 = runnerXY(i + 1)
                Dim dx As Double = p2.X - p1.X
                Dim dy As Double = p2.Y - p1.Y
                Dim t As Double = ((qx - p1.X) * dx + (qy - p1.Y) * dy) / (dx * dx + dy * dy)
                t = Math.Max(0, Math.Min(1, t))
                Dim projX As Double = p1.X + t * dx
                Dim projY As Double = p1.Y + t * dy
                Dim dist As Double = Math.Sqrt((qx - projX) ^ 2 + (qy - projY) ^ 2)
                If dist < minDist Then
                    minDist = dist
                    segIndex = i ' posuneme okno dopředu
                End If
            Next
            totalDeviation += minDist
        Next

        Return totalDeviation / dogGeoPoints.Count
    End Function

    ' Přibližný převod lat/lon na lokální souřadnice v metrech
    Private Sub LatLonToXY(lat As Double, lon As Double, lat0 As Double, lon0 As Double, ByRef x As Double, ByRef y As Double)
        Dim R As Double = 6371000.0 ' poloměr Země v m
        Dim dLat As Double = (lat - lat0) * Math.PI / 180.0
        Dim dLon As Double = (lon - lon0) * Math.PI / 180.0
        Dim meanLat As Double = (lat + lat0) / 2.0 * Math.PI / 180.0
        x = R * dLon * Math.Cos(meanLat)
        y = R * dLat
    End Sub



    ' Function to calculate the distance in km between two GPS points using the Haversine formula
    Public Function HaversineDistance(lat1 As Double, lon1 As Double, lat2 As Double, lon2 As Double, units As String) As Double
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
    ' Function to convert degrees to radians
    Private Function DegToRad(degrees As Double) As Double
        Const PI As Double = 3.14159265358979
        Return degrees * PI / 180
    End Function

    Public Function PromptForStartTime(trackName As String, start_end As String, Optional maxTries As Integer = 3) As DateTime?
        Dim input As String
        Dim parsedDate As DateTime
        Dim attempt As Integer = 0

        While attempt < maxTries
            input = InputBox($"There is a missing {start_end} time in the {trackName} track." & vbCrLf &
                         "Enter the time in the format: yyyy-MM-ddTHH:mm:ss",
                         "Fill in the time",
                         Now.ToString("yyyy-MM-ddTHH:mm:ss"), MessageBoxIcon.Warning)

            ' Uživatel kliknul "Zrušit" nebo nechal prázdné → přerušit
            If String.IsNullOrWhiteSpace(input) Then Return Nothing

            If DateTime.TryParse(input, parsedDate) Then
                Return parsedDate
            Else
                MessageBox.Show("Invalid date/time format. Try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                attempt += 1
            End If
        End While

        ' Pokud se nepodařilo ani na třetí pokus, návrat Nothing
        Return Nothing
    End Function



End Class
