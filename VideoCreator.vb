Imports System.IO
'Imports System.Globalization
Imports System.Xml
Imports Windows.Win32.UI.Input

Public Class VideoCreator

    Private converter As TrackConverter
    'Private pngCreator As PngSequenceCreator
    Private encoder As FfmpegVideoEncoder

    Private outputDir As DirectoryInfo
    'Private minVideoSize As Single
    Private windDirection As Double?
    Private windSpeed As Double
    Private imgWidth As Double
    Private imgHeight As Double
    Private backgrounMapPath As String
    Private backgroundTiles As (bgmap As Bitmap, minTileX As Single, minTileY As Single) = (Nothing, 0, 0)
    Dim textParts As New List(Of (Text As String, Color As Color, FontStyle As FontStyle))
    Public Event WarningOccurred(message As String, _color As Color)

    Public Sub New(outputDir As DirectoryInfo, Optional windDir As Double? = Nothing, Optional windSpeed As Double = 0, Optional textParts As List(Of (Text As String, Color As Color, FontStyle As FontStyle)) = Nothing)
        Me.outputDir = outputDir
        'Me.minVideoSize = minVideoSize
        Me.windDirection = windDir
        Me.windSpeed = windSpeed
        Me.textParts = textParts

        converter = New TrackConverter()

    End Sub

    Public Async Function CreateVideoFromTrkNode(_tracksAsTrkNode As List(Of TrackAsTrkNode), textParts As List(Of (Text As String, Color As Color, FontStyle As FontStyle))) As Task(Of Boolean)
        Dim tracksAsTrkPts = converter.ConvertTracksAsTrkNodesToTrackAsTrkPts(_tracksAsTrkNode)
        Me.textParts = textParts
        Return Await CreateVideoFromTrkPts(tracksAsTrkPts, Me.textParts)

    End Function

    Public Async Function CreateVideoFromTrkPts(_tracksAsTrkPts As List(Of TrackAsTrkPts), textParts As List(Of (Text As String, Color As Color, FontStyle As FontStyle))) As Task(Of Boolean)
        Dim tracksAsGeoPoints As List(Of TrackAsGeoPoints) = converter.ConvertTracksTrkPtsToGeoPoints(_tracksAsTrkPts) 'přepočítá trasy na body a uloží do TrackAsGeoPoints
        Me.textParts = textParts
        Return Await CreateVideoFromGeoPoints(tracksAsGeoPoints)
    End Function


    ''' <summary>
    ''' Creates a video from tracks represented as geo points (latitude/longitude coordinates).
    ''' </summary>
    ''' <param name="_tracksAsGeoPoints">List of tracks containing geo points.</param>
    Public Async Function CreateVideoFromGeoPoints(_tracksAsGeoPoints As List(Of TrackAsGeoPoints)) As Task(Of Boolean)
        'converts routes to points and saves to TrackAsPointsF

        backgrounMapPath = IO.Path.Combine(outputDir.FullName, "backgroundMap.png")
        Dim zoom As Integer = 18 ' Zoom level for the background map

        converter.SetCoordinatesBounds(_tracksAsGeoPoints)

        Dim downloader As New OsmTileDownloader()
        backgroundTiles = Await downloader.GetMapBitmap(converter.minLat, converter.maxLat, converter.minLon, converter.maxLon, zoom)

        'backgroundTiles.bgmap.Save(IO.Path.Combine(outputDir.FullName, backgrounMapPath), System.Drawing.Imaging.ImageFormat.Png)


        Dim _TracksAsPointsF As List(Of TrackAsPointsF) = converter.ConvertTracksGeoPointsToPointsF(_tracksAsGeoPoints, backgroundTiles.minTileX, backgroundTiles.minTileY, zoom) 'přepočítá trasy na body a uloží do TrackAsPointsF

        Return Await CreateVideoFromPointsF(_TracksAsPointsF)

    End Function


    ''' <summary>
    ''' Creates a video from tracks converted to 2D points with timestamps.
    ''' </summary>
    ''' <param name="_tracksAsPointsF">List of tracks containing 2D points and times.</param>
    Public Async Function CreateVideoFromPointsF(_tracksAsPointsF As List(Of TrackAsPointsF)) As Task(Of Boolean)
        Dim pngDir As DirectoryInfo = Nothing
        Dim pngCreator As PngSequenceCreator = Nothing

        Await Task.Run(Sub()

                           ' Vykresli statické pozadí
                           Dim renderer As New PngRenderer(windDirection, windSpeed, Me.backgroundTiles)

                           Dim staticBgTransparent = renderer.RenderStaticTransparentBackground(_tracksAsPointsF, backgroundTiles)
                           'staticBgTransparent.Save(IO.Path.Combine(outputDir.FullName, "staticBgTransparent.png"), System.Drawing.Imaging.ImageFormat.Png)

                           Dim staticBgMap = renderer.RenderStaticMapBackground(_tracksAsPointsF, backgroundTiles)
                           'staticBgMap.Save(IO.Path.Combine(outputDir.FullName, "staticBgMap.png"), System.Drawing.Imaging.ImageFormat.Png)

                           ' Generuj PNG snímky podle časů
                           pngDir = outputDir.CreateSubdirectory("png")
                           pngCreator = New PngSequenceCreator(renderer)
                           Dim pngTimes = pngCreator.GetPngTimes(_tracksAsPointsF)
                           pngCreator.CreateFrames(_tracksAsPointsF, staticBgTransparent, staticBgMap, pngDir, pngTimes, textParts)
                       End Sub)
        ' Sestav video
        Dim outputFile = IO.Path.Combine(outputDir.FullName, "overlay")
        encoder = New FfmpegVideoEncoder()
        Return Await encoder.EncodeFromPngs(pngDir, outputFile, pngCreator.frameInterval)


    End Function


    ''' <summary>
    ''' Získá obdélník (bounds) všech bodů ve všech trackách.
    ''' </summary>
    ''' <param name="tracks">List tracků obsahujících 2D body.</param>
    ''' <returns>RectangleF s minX, minY a velikostí (Width, Height).</returns>
    Function GetTrackBounds(tracks As List(Of TrackAsPointsF)) As Rectangle
        ' Sesbíráme všechny body (Location) do jedné kolekce
        Dim allPoints = tracks _
        .Where(Function(t) t.TrackPointsF IsNot Nothing AndAlso t.TrackPointsF.Count > 0) _
        .SelectMany(Function(t) t.TrackPointsF) _
        .Select(Function(tp) tp.Location) _
        .ToList()

        If allPoints.Count = 0 Then
            ' Nemáme žádné body, vrať prázdný RectangleF
            Return Rectangle.Empty
        End If

        Dim minX = allPoints.Min(Function(p) p.X)
        Dim maxX = allPoints.Max(Function(p) p.X)
        Dim minY = allPoints.Min(Function(p) p.Y)
        Dim maxY = allPoints.Max(Function(p) p.Y)

        Dim width = maxX - minX
        Dim height = maxY - minY

        ' Přidáme 5 % okraje na každou stranu
        Dim marginX = width * 0.05
        Dim marginY = height * 0.05

        Dim newMinX = CInt(Math.Floor(minX - marginX))
        Dim newMinY = CInt(Math.Floor(minY - marginY))
        Dim newWidth = CInt(Math.Ceiling(width + 2 * marginX))
        Dim newHeight = CInt(Math.Ceiling(height + 2 * marginY))

        Return New Rectangle(newMinX, newMinY, newWidth, newHeight)

    End Function



End Class

