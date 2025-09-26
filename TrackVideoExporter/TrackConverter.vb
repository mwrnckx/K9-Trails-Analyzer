Imports System.Drawing
Imports System.Reflection
Imports System.Resources.ResXFileRef
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
        If _tracksAsTrkPts Is Nothing Then Return Nothing
        Dim tracksAsGeoPoints As New List(Of TrackAsGeoPoints)
        For Each track In _tracksAsTrkPts

            Dim _TrackAsGeoPoints = ConvertTrackTrkPtsToGeoPoints(track)

            tracksAsGeoPoints.Add(_TrackAsGeoPoints)
        Next
        Return tracksAsGeoPoints
    End Function

    Public Function ConvertTrackTrkPtsToGeoPoints(track As TrackAsTrkPts) As TrackAsGeoPoints
        If track Is Nothing Then Return Nothing
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



    ' Enum pro reprezentaci stavu (pohyb / stání).
    Private Enum MovementState
        Moving
        Stopped
    End Enum
    ' Struktura pro vrácení výsledků výpočtu.
    Public Structure TrackStats
        Public Property DogDistanceKm As Double ' Vzdálenost skutečně uražená psem (měřeno z trasy psa)
        Public Property RunnerDistanceKm As Double ' Vzdálenost skutečně uražená kladečem (měřeno z trasy kladeče)
        Public Property WeightedDistanceAlongTrailKm As Double ' Vzdálenost uražená psem měřeno po trase kladeče s váhou podle odchylky
        Public Property TotalTime As TimeSpan ' celkový čas trasy psa
        Public Property MovingTime As TimeSpan ' čistý čas, kdy se pes pohyboval
        Public Property StoppedTime As TimeSpan ' čas, kdy psovod stál pes také stál nebo prováděl perimetr (hledal stopu)
        Public Property DogNetSpeedKmh As Double ' čistá rychlost (pouze doba pohybu), vypočetno z délky trasy psa
        Public Property DogGrossSpeedKmh As Double ' hrubá rychlost (včetně zastávek), vypočteno z celkové doby psa a délky trasy kladeče
        Public Property Deviation As Double ' průměrná odchylka trasy psa od trasy kladeče v metrech (vážená časem pohybu)
        Public Property CheckPointsEval As List(Of (distanceFromStartKm As Double, distanceFromTrailm As Double))
        Public Property PoitsInMTcompetition As Integer ' počet bodů v MT soutěži podle pravidel
    End Structure
    Public Function CalculateTrackStats(dogtrkNode As XmlNode, Optional runnerTrkNode As XmlNode = Nothing, Optional wayPoints As TrackAsTrkPts = Nothing) As TrackStats

        ' --- Nastavení prahových hodnot podle GPXSee ---
        Const MOVING_SPEED_THRESHOLD_MS As Double = 0.277  ' m/s (1.0 km/h)
        Const STOPPED_SPEED_THRESHOLD_MS As Double = 0.1 ' m/s (0.36 km/h)

        Dim totalMovingTime As TimeSpan = TimeSpan.Zero
        Dim totalStoppedTime As TimeSpan = TimeSpan.Zero
        Dim totalDogDistanceKm As Double = -1.0F
        Dim totalRunnerDistanceKm As Double = -1.0F
        Dim currentState As MovementState
        Dim SpeedKmh As Double = -1.0F


        Dim dogTrkAsGeoPoints As TrackAsGeoPoints = ConvertTrackTrkPtsToGeoPoints(ConvertTrackAsTrkNodeToTrkPts(New TrackAsTrkNode(dogtrkNode, trackType:=TrackType.Unknown)))
        Dim dogGeoPoints As List(Of TrackGeoPoint) = dogTrkAsGeoPoints.TrackGeoPoints 'trasa psa/psovoda
        Dim runnerTrkAsGeoPoints As TrackAsGeoPoints
        Dim runnerGeoPoints As List(Of TrackGeoPoint)
        ' Převedeme runnera do XY
        Dim runnerXY As New List(Of (X As Double, Y As Double))
        Dim dogXY As New List(Of (X As Double, Y As Double))
        Dim lat0 As Double
        Dim lon0 As Double

        If runnerTrkNode IsNot Nothing Then
            runnerTrkAsGeoPoints = ConvertTrackTrkPtsToGeoPoints(ConvertTrackAsTrkNodeToTrkPts(New TrackAsTrkNode(runnerTrkNode, trackType:=TrackType.Unknown)))
            runnerGeoPoints = runnerTrkAsGeoPoints.TrackGeoPoints
            If runnerGeoPoints.Count > 0 Then
                ' Převedeme runnera do XY
                lat0 = runnerGeoPoints(0).Location.Lat
                lon0 = runnerGeoPoints(0).Location.Lon
                For i As Integer = 0 To runnerGeoPoints.Count - 2
                    Dim point1 As TrackGeoPoint = runnerGeoPoints(i) 'první bod segmentu trasy psa/psovoda
                    Dim point2 As TrackGeoPoint = runnerGeoPoints(i + 1) 'druhý bod segmentu trasy psa/psovoda

                    Dim lat As Double = point1.Location.Lat
                    Dim lon As Double = point1.Location.Lon
                    Dim x, y As Double
                    LatLonToXY(lat, lon, lat0, lon0, x, y)
                    runnerXY.Add((x, y))
                    Dim distanceMeters As Double = HaversineDistance(point1.Location.Lat, point1.Location.Lon, point2.Location.Lat, point2.Location.Lon, "m")
                    If distanceMeters > 0 Then
                        If totalRunnerDistanceKm < 0 Then totalRunnerDistanceKm = 0 'odstranění počáteční hodnoty -1
                    End If
                    totalrunnerDistanceKm += distanceMeters / 1000.0F 'tohle je situace, když čas chybí, nutno započítat!
                Next
            End If
        End If
        Dim totalDeviation As Double = 0F
        Dim weightedDistanceAlongTrailm As Double = 0.0 '  vzdálenost ušlá psem po trase kladeče vážená vzdáleností od trailu
        Dim lastCreditedRunnerSegmentIndex As Integer = -1 ' Index posledního započítaného segmentu kladeče


        Dim segIndex As Integer = 0

        ' --- Hlavní smyčka pro procházení bodů trasy psovoda/psa ---
        Dim distanceFromRunner As Double = Double.MaxValue
        For i As Integer = 0 To dogGeoPoints.Count - 2
            Try
                Dim point1 As TrackGeoPoint = dogGeoPoints(i) 'první bod segmentu trasy psa/psovoda
                Dim point2 As TrackGeoPoint = dogGeoPoints(i + 1) 'druhý bod segmentu trasy psa/psovoda
                Dim dogx, dogy As Double
                LatLonToXY(point1.Location.Lat, point1.Location.Lon, lat0, lon0, dogx, dogy)
                Dim dog As (X As Double, Y As Double) = (dogx, dogy)
                dogXY.Add(dog)

                Dim timeDiff As TimeSpan = point2.Time - point1.Time
                Dim distanceMeters As Double = HaversineDistance(point1.Location.Lat, point1.Location.Lon, point2.Location.Lat, point2.Location.Lon, "m")
                If distanceMeters > 0 Then
                    If totalDogDistanceKm < 0 Then totalDogDistanceKm = 0 'odstranění počáteční hodnoty -1
                End If

                If timeDiff.TotalSeconds <= 0 Then
                    totalDogDistanceKm += distanceMeters / 1000.0F 'tohle je situace, když čas chybí, nutno započítat!
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
                    totalDogDistanceKm += distanceMeters / 1000.0F 'počítá se jen pohyb


                    ' Výpočet odchylky trasy psovoda/psa od druhé trasy (kladeče):
                    If runnerTrkNode IsNot Nothing AndAlso runnerXY.Count > 1 Then
                        ' Najdeme nejbližší bod na aktuálním + sousedních segmentech trasy kladeče
                        Dim windowSize As Int16 = runnerXY.Count / 2 'nezkoumáme celou trasu, ale jen polovinu
                        Dim minDist As Double = Double.MaxValue
                        For j = Math.Max(0, segIndex - windowSize) To Math.Min(runnerXY.Count - 2, segIndex + windowSize)
                            Dim runner1 = runnerXY(j)
                            Dim runner2 = runnerXY(j + 1)
                            Dim dx As Double = runner2.X - runner1.X
                            Dim dy As Double = runner2.Y - runner1.Y
                            Dim t As Double = ((dogx - runner1.X) * dx + (dogy - runner1.Y) * dy) / (dx * dx + dy * dy)
                            t = Math.Max(0, Math.Min(1, t))
                            Dim projX As Double = runner1.X + t * dx
                            Dim projY As Double = runner1.Y + t * dy
                            Dim dist As Double = Math.Sqrt((dogx - projX) ^ 2 + (dogy - projY) ^ 2)
                            If dist < minDist Then
                                minDist = dist
                                segIndex = j ' posuneme okno dopředu
                            End If
                        Next j
                        totalDeviation += minDist * timeDiff.TotalSeconds ' vážená odchylka v metrech * sekundách

                        ' Výpočet váhy podle vzdálenosti (minDist)
                        Dim weight As Double = 0.0
                        If minDist < 10 Then
                            weight = 1.0 ' Plná váha do 10 metrů
                        ElseIf minDist <= 50 Then
                            ' Lineární pokles váhy z 1.0 (při 10m) na 0.0 (při 50m)
                            weight = (50.0 - minDist) / 40.0
                        End If
                        ' Pokud je minDist > 50, váha zůstane 0.0

                        ' Definuje, o kolik segmentů na trase kladeče může pes "poskočit",
                        ' aby to bylo stále považováno za souvislé sledování stopy.
                        Const MAX_SEGMENT_JUMP As Integer = 5

                        ' Pokud je pes dostatečně blízko A posunul se na nový segment kladeče
                        If weight > 0 AndAlso segIndex > lastCreditedRunnerSegmentIndex Then

                            Dim segmentJump As Integer = segIndex - lastCreditedRunnerSegmentIndex

                            ' PŘÍPAD 1: Pes sleduje stopu souvisle (nebo si jen mírně zkracuje)
                            If segmentJump <= MAX_SEGMENT_JUMP Then
                                ' Sečteme délky všech nově "odemčených" segmentů kladeče
                                For k As Integer = lastCreditedRunnerSegmentIndex + 1 To segIndex
                                    Dim p1 = runnerXY(k)
                                    Dim p2 = runnerXY(k + 1)
                                    Dim runnerSegmentLengthMeters As Double = Math.Sqrt((p1.X - p2.X) ^ 2 + (p1.Y - p2.Y) ^ 2)

                                    ' Přičteme délku segmentu kladeče vynásobenou aktuální váhou
                                    weightedDistanceAlongTrailm += runnerSegmentLengthMeters * weight
                                Next k

                                ' PŘÍPAD 2: Pes ztratil stopu a našel ji o velký kus dál.
                                ' Nic se nepřičte, pouze se posune "základna" pro další výpočty.
                                ' Tím ho penalizujeme za přeskočený úsek.
                                ' Můžeme volitelně přičíst jen ten jeden segment, kde stopu znovu našel.
                            Else
                                Dim p1 = runnerXY(segIndex)
                                Dim p2 = runnerXY(segIndex + 1)
                                Dim runnerSegmentLengthMeters As Double = Math.Sqrt((p1.X - p2.X) ^ 2 + (p1.Y - p2.Y) ^ 2)
                                ' Přičteme jen délku aktuálního segmentu, kde se pes "chytil".
                                ' Toto je kompromis - nepřičteme nic z bloudění, ale dáme mu body za znovu nalezení.
                                weightedDistanceAlongTrailm += runnerSegmentLengthMeters * weight
                            End If

                            ' Aktualizujeme index posledního započítaného segmentu v každém případě
                            lastCreditedRunnerSegmentIndex = segIndex
                        End If
                        'teď vypočteme jak nejblíže je trasa psa ke kladeči pokud to bude méně než 10 metrů tak je zřejmé že psovod kladeče našel
                        Dim runnerPosition = runnerXY.Last
                        Dim dist2 As Double = Double.Sqrt((dogx - runnerPosition.X) ^ 2 + (dogy - runnerPosition.Y) ^ 2)
                        If dist2 < distanceFromRunner Then
                            distanceFromRunner = dist2
                        End If

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

        'waypointy 
        'pro každý waypoint najít nejbližší bod na trase kladeče a pro tento bod spočítat vzdálenost od startu po trase kladeče a vzdálenost waypointu od tohoto bodu
        Dim _checkpointsEval = New List(Of (distanceFromStartKm As Double, distanceFromTrailm As Double))
        Dim converter As New TrackConverter
        Dim wayPointsAsGeoPoints As TrackAsGeoPoints = converter.ConvertTrackTrkPtsToGeoPoints(wayPoints)

        'poslední bod tracku psa přidáme jako waypoint - v případě nálezu kladeče bude blízko posledního bodu trasy kladeče
        If wayPointsAsGeoPoints Is Nothing Then
            wayPointsAsGeoPoints = New TrackAsGeoPoints(TrackType.Artickle, New List(Of TrackGeoPoint))
        End If
        wayPointsAsGeoPoints.TrackGeoPoints.Add(dogGeoPoints.Last)

        If wayPointsAsGeoPoints IsNot Nothing AndAlso runnerTrkNode IsNot Nothing AndAlso runnerXY.Count > 1 Then
            For i = 0 To wayPointsAsGeoPoints.TrackGeoPoints.Count - 1
                Dim wp As TrackGeoPoint = wayPointsAsGeoPoints.TrackGeoPoints(i)
                Dim qx, qy As Double
                LatLonToXY(wp.Location.Lat, wp.Location.Lon, lat0, lon0, qx, qy)
                ' Najdeme nejbližší bod 
                Dim minDist As Double = Double.MaxValue
                Dim closestSegIndex As Integer = -1
                For j = 0 To runnerXY.Count - 2
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
                        closestSegIndex = j ' posuneme okno dopředu
                    End If
                Next j
                ' Spočítáme vzdálenost od startu kladeče k tomuto bodu:
                Dim distanceFromStartkm As Double = 0.0
                For k As Integer = 0 To closestSegIndex - 1
                    Dim runnerPoint1 = runnerGeoPoints(k)
                    Dim runnerPoint2 = runnerGeoPoints(k + 1)
                    Dim runnerSegmentLengthMeters As Double = HaversineDistance(runnerPoint1.Location.Lat, runnerPoint1.Location.Lon, runnerPoint2.Location.Lat, runnerPoint2.Location.Lon, "m")
                    distanceFromStartkm += runnerSegmentLengthMeters
                Next k
                _checkpointsEval.Add((distanceFromStartkm, minDist)) 'uložíme vzdálenost od startu v km a vzdálenost od trasy v metrech
            Next i
        End If

        ' TODO: Výpočet bodů v MT soutěži podle pravidel
        Dim pointsInMTcompetition As Integer = 0
        If runnerTrkNode IsNot Nothing AndAlso totalMovingTime.TotalMinutes > 0 Then
            ' Základní body za nalezení kladeče
            If distanceFromRunner < 15.0 Then 'tolerance na nepřesnost gps
                pointsInMTcompetition = 1000 ' 
            End If
            ' Bonusové body za rychlost (hrubá rychlost psa) TODO: tohle dopočítat správně z posledního waypointu???
            Dim grossSpeedKmh As Double = If((totalMovingTime + totalStoppedTime).TotalHours > 0, weightedDistanceAlongTrailm / 1000.0 / (totalMovingTime + totalStoppedTime).TotalHours, 0.0)
            If grossSpeedKmh >= 4.0 Then
                pointsInMTcompetition += 5 ' 5 bodů za průměrnou hrubou rychlost 4 km/h a více
            ElseIf grossSpeedKmh >= 3.0 Then
                pointsInMTcompetition += 3 ' 3 body za průměrnou hrubou rychlost 3-3.99 km/h
            ElseIf grossSpeedKmh >= 2.0 Then
                pointsInMTcompetition += 1 ' 1 bod za průměrnou hrubou rychlost 2-2.99 km/h
            End If
            ' Penalizace za odchylku od trasy kladeče
            Dim averageDeviation As Double = If(totalMovingTime.TotalSeconds > 0, totalDeviation / (totalMovingTime.TotalSeconds), 0.0) ' průměrná odchylka v metrech
            If averageDeviation > 20.0 Then
                pointsInMTcompetition -= 5 ' -5 bodů za průměrnou odchylku větší než 20 m
            ElseIf averageDeviation > 10.0 Then
                pointsInMTcompetition -= 3 ' -3 body za průměrnou odchylku mezi 10-20 m
            ElseIf averageDeviation > 5.0 Then
                pointsInMTcompetition -= 1 ' -1 bod za průměrnou odchylku mezi 5-10 m
            End If
            ' Penalizace za ztrátu stopy (vzdálenost psa od kladeče)


        End If

        Return New TrackStats With {
            .DogDistanceKm = totalDogDistanceKm,
            .RunnerDistanceKm = totalRunnerDistanceKm,
            .WeightedDistanceAlongTrailKm = weightedDistanceAlongTrailm / 1000.0, ' 
            .MovingTime = totalMovingTime,
            .StoppedTime = totalStoppedTime,
           .TotalTime = totalMovingTime + totalStoppedTime,
           .Deviation = If(runnerTrkNode IsNot Nothing AndAlso totalMovingTime.TotalSeconds > 0, totalDeviation / (totalMovingTime.TotalSeconds), -1.0F), ' průměrná odchylka v metrech
           .DogNetSpeedKmh = If(totalMovingTime.TotalHours > 0, totalDogDistanceKm / totalMovingTime.TotalHours, -1.0F),
           .DogGrossSpeedKmh = If((totalMovingTime + totalStoppedTime).TotalHours > 0, totalRunnerDistanceKm / (totalMovingTime + totalStoppedTime).TotalHours, -1.0F),
           .CheckPointsEval = _checkpointsEval
            }

    End Function


    ''' <summary>
    ''' Přibližný převod lat/lon na lokální souřadnice v metrech
    ''' </summary>
    ''' <param name="lat"></param>
    ''' <param name="lon"></param>
    ''' <param name="lat0"></param>
    ''' <param name="lon0"></param>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
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
