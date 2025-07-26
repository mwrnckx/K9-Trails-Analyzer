Imports System.Drawing
Imports System.Xml

''' <summary>
''' Provides conversion of GPX data between various internal formats used for video rendering.
''' </summary>
Public Class TrackConverter

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

    ''' <summary>
    ''' Converts XML track points to geographical points with timestamps.
    ''' </summary>
    ''' <param name="_tracksAsTrkPts">List of tracks containing XML track point nodes.</param>
    ''' <returns>List of <see cref="TrackAsGeoPoints"/> with lat/lon and time.</returns>
    Public Function ConvertTracksTrkPtsToGeoPoints(_tracksAsTrkPts As List(Of TrackAsTrkPts)) As List(Of TrackAsGeoPoints)
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
                    Throw New Exception("Time node not found in trkpt.")
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
            Dim _TrackAsPointsF As New TrackAsPointsF With {
                .Label = Track.Label,
                .Color = Track.Color,
                .IsMoving = Track.IsMoving,
                .TrackPointsF = New List(Of TrackPointF)
            }

            For Each geoPoint As TrackGeoPoint In Track.TrackGeoPoints
                Dim pt = LatLonToPixel(geoPoint.Location.Lat, geoPoint.Location.Lon, zoom, minTileX, minTileY)
                Dim _trackpointF As New TrackPointF With {
                    .Location = New PointF With {.X = pt.X, .Y = pt.Y},
                    .Time = geoPoint.Time
                }
                _TrackAsPointsF.TrackPointsF.Add(_trackpointF)
            Next
            _tracksAsPointsF.Add(_TrackAsPointsF)
        Next
        Return _tracksAsPointsF
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
    Function SelectSingleChildNode(childName As String, parent As XmlNode) As XmlNode
        Dim nsmgr As New XmlNamespaceManager(parent.OwnerDocument.NameTable)
        Dim ns As String = parent.GetNamespaceOfPrefix("")
        nsmgr.AddNamespace("gpx", ns)
        Return parent.SelectSingleNode($"gpx:{childName}", nsmgr)
    End Function

End Class
