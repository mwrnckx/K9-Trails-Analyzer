Imports System.Xml
Imports Microsoft.VisualBasic.Logging

Public Class TrackConverter
    Private minVideoSize As Single
    Public minLat As Double = Double.MaxValue
    Public maxLat As Double = Double.MinValue
    Public minLon As Double = Double.MaxValue
    Public maxLon As Double = Double.MinValue


    Public Sub New(minVideoSize As Single)
        Me.minVideoSize = minVideoSize
    End Sub



    Public Function ConvertTracksAsTrkNodesToTrackAsTrkPts(_tracksAsTrkNode As List(Of TrackAsTrkNode)) As List(Of TrackAsTrkPts)

        Dim tracksAsTrkPts As New List(Of TrackAsTrkPts)
        For Each track In _tracksAsTrkNode
            Dim trkptNodes As XmlNodeList = SelectTrkptNodes(track.TrkNode)
            Dim _TrackAsTrkPts As New TrackAsTrkPts With {
                .Label = track.Label,
                .Color = track.Color,
                .IsMoving = track.IsMoving,
                .TrackPoints = trkptNodes
            }
            tracksAsTrkPts.Add(_TrackAsTrkPts)
        Next
        Return tracksAsTrkPts
    End Function

    Public Function ConvertTracksTrkPtsToGeoPoints(_tracksAsTrkPts As List(Of TrackAsTrkPts)) As List(Of TrackAsGeoPoints)
        ' Najdi rozsah souřadnic
        Dim tracksAsGeoPoint As New List(Of TrackAsGeoPoints)
        For Each Track In _tracksAsTrkPts
            Dim _TrackAsGeoPoints As New TrackAsGeoPoints With {
                .Label = Track.Label,
                .Color = Track.Color,
                .IsMoving = Track.IsMoving,
                .TrackGeoPoints = New List(Of TrackGeoPoint)
            }
            For Each trkptnode As XmlNode In Track.TrackPoints
                Dim lat = Double.Parse(trkptnode.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
                Dim lon = Double.Parse(trkptnode.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)
                Dim timenode = SelectSingleChildNode("time", trkptnode)
                Dim time As DateTime
                If timenode IsNot Nothing Then
                    time = DateTime.Parse(timenode.InnerText, Nothing, Globalization.DateTimeStyles.AssumeUniversal)
                Else
                    Throw New Exception("Čas nebyl nalezen v trkpt.")
                End If


                Dim geopoint As New TrackGeoPoint With {
                        .Location = New Coordinates With {.Lat = lat, .Lon = lon},
                        .Time = time
                    }

                _TrackAsGeoPoints.TrackGeoPoints.Add(geopoint)
            Next
            tracksAsGeoPoint.Add(_TrackAsGeoPoints)
        Next
        Return tracksAsGeoPoint

    End Function

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

    Public Function ConvertTracksGeoPointsToPointsF(_tracksAsGeoPoints As List(Of TrackAsGeoPoints), minTileX As Single, minTileY As Single, zoom As Integer) As List(Of TrackAsPointsF)

        Dim textSize As Single = 0



        ' Calculate the width and height of the figure in metres
        Dim latDistancePerDegree As Double = 111_320.0 ' průměrně ~111,3 km na jeden stupeň latitude
        Dim centerLat As Double = (minLat + maxLat) / 2
        Dim lonDistancePerDegree As Double = Math.Cos(centerLat * Math.PI / 180) * latDistancePerDegree
        Dim widthInMeters As Double = (maxLon - minLon) * lonDistancePerDegree
        Dim heightInMeters As Double = (maxLat - minLat) * latDistancePerDegree
        Dim pixelsPerMetre As Double = Math.Min(minVideoSize / widthInMeters, minVideoSize / heightInMeters) 'přepočet z GPS na pixely, defaultně 1 pixel = 1 metr
        'Me.imgWidth = widthInMeters * pixelsPerMetre
        'Me.imgHeight = heightInMeters * pixelsPerMetre
        Dim _tracksAsPointsF As New List(Of TrackAsPointsF)
        For Each Track In _tracksAsGeoPoints
            Dim _TrackAsPointsF As New TrackAsPointsF With {
                .Label = Track.Label,
                .Color = Track.Color,
                .IsMoving = Track.IsMoving,
                .TrackPointsF = New List(Of TrackPointF)
            }

            For Each geoPoint As TrackGeoPoint In Track.TrackGeoPoints
                Dim x = CSng(((geoPoint.Location.Lon - minLon)) * lonDistancePerDegree * pixelsPerMetre) 'pozice X osa, přepočítaná na pixely
                Dim y = CSng(((maxLat - geoPoint.Location.Lat)) * latDistancePerDegree * pixelsPerMetre) ' Y osa obrácená
                Dim pt = LatLonToPixel(geoPoint.Location.Lat, geoPoint.Location.Lon, zoom, minTileX, minTileY)
                x = pt.X
                y = pt.Y
                Dim _trackpointF As New TrackPointF With {
                        .Location = New PointF With {.X = x, .Y = y},
                        .Time = geoPoint.Time
                    }
                _TrackAsPointsF.TrackPointsF.Add(_trackpointF)
            Next
            _tracksAsPointsF.Add(_TrackAsPointsF)
        Next
        Return _tracksAsPointsF

    End Function

    ''' <summary>
    ''' Převede GPS souřadnice (lat, lon) na pixel souřadnice ve složeném obrázku z dlaždic.
    ''' </summary>
    ''' <param name="lat">Zeměpisná šířka</param>
    ''' <param name="lon">Zeměpisná délka</param>
    ''' <param name="zoom">Zoom level (např. 15)</param>
    ''' <param name="minTileX">X souřadnice levé horní dlaždice (celé číslo)</param>
    ''' <param name="minTileY">Y souřadnice levé horní dlaždice (celé číslo)</param>
    ''' <returns>PointF s X a Y v pixelech bitmapy</returns>
    Function LatLonToPixel(lat As Double, lon As Double, zoom As Integer, minTileX As Integer, minTileY As Integer) As PointF
        Dim n = Math.Pow(2, zoom)

        ' Výpočet X a Y v dlaždicích (desetinné číslo)
        Dim tileX = (lon + 180.0) / 360.0 * n
        Dim tileY = (1 - Math.Log(Math.Tan(lat * Math.PI / 180.0) + 1 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2 * n

        ' Převedení na pixel souřadnice v bitmapě
        Dim pixelX = CSng((tileX - minTileX) * 256)
        Dim pixelY = CSng((tileY - minTileY) * 256)

        Return New PointF(pixelX, pixelY)
    End Function




    Function SelectTrkptNodes(trkNode As XmlNode) As XmlNodeList
        Dim nsmgr As New XmlNamespaceManager(trkNode.OwnerDocument.NameTable)
        Dim ns As String = trkNode.GetNamespaceOfPrefix("") ' získá default namespace parent uzlu
        nsmgr.AddNamespace("gpx", ns)
        ' V GPX je to: trk > trkseg > trkpt
        Return trkNode.SelectNodes(".//gpx:trkpt", nsmgr)
    End Function

    ''' <summary>
    ''' Najde poduzel se zadaným názvem, v GPX namespace.
    ''' </summary>
    ''' <param name="childName">např. "time"</param>
    ''' <param name="parent">nadřazený XmlNode (např. trkpt)</param>
    ''' <returns>XmlNode nebo Nothing</returns>
    Function SelectSingleChildNode(childName As String, parent As XmlNode) As XmlNode
        Dim nsmgr As New XmlNamespaceManager(parent.OwnerDocument.NameTable)
        Dim ns As String = parent.GetNamespaceOfPrefix("") ' získá default namespace parent uzlu
        nsmgr.AddNamespace("gpx", ns)
        Return parent.SelectSingleNode($"gpx:{childName}", nsmgr)
    End Function
End Class

