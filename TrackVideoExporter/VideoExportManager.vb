
Imports System.Drawing
Imports System.IO

Namespace TrackVideoExporter


    ''' <summary>
    ''' Class responsible for creating overlay videos from GPS tracks.
    ''' </summary>
    Public Class VideoExportManager

        Private converter As TrackConverter
        Private encoder As FfmpegVideoEncoder

        Private outputDir As DirectoryInfo
        Private windDirection As Double?
        Private windSpeed As Double

        Private backgroundTiles As (bgmap As Bitmap, minTileX As Single, minTileY As Single) = (Nothing, 0, 0)
        Private textParts As New List(Of (Text As String, Color As Color, FontStyle As FontStyle))
        Private textPartsEng As New List(Of (Text As String, Color As Color, FontStyle As FontStyle))

        ''' <summary>
        ''' Raised when a non-critical warning occurs during processing.
        ''' </summary>
        Public Event WarningOccurred(message As String, _color As Color)

        ''' <summary>
        ''' Initializes a new instance of the <see cref="VideoExportManager"/> class.
        ''' </summary>
        ''' <param name="outputDir">Output directory for generated images and video.</param>
        ''' <param name="windDir">Optional wind direction in degrees.</param>
        ''' <param name="windSpeed">Optional wind speed.</param>
        ''' <param name="textParts">Optional list of styled text parts to display.</param>
        Public Sub New(outputDir As DirectoryInfo,
                       Optional windDir As Double? = Nothing,
                       Optional windSpeed As Double = 0,
                       Optional textParts As List(Of (Text As String, Color As Color, FontStyle As FontStyle)) = Nothing)

            Me.outputDir = outputDir
            Me.windDirection = windDir
            Me.windSpeed = windSpeed
            Me.textParts = textParts
            converter = New TrackConverter()

        End Sub

        ''' <summary>
        ''' Converts TRK nodes to geo points and generates an overlay video.
        ''' </summary>
        ''' <param name="_tracksAsTrkNode">List of tracks in TRK node format.</param>
        ''' <param name="textParts">Styled text parts to render in video.</param>
        ''' <returns>True if video was successfully created.</returns>
        Public Async Function CreateVideoFromTrkNode(
            _tracksAsTrkNode As List(Of TrackAsTrkNode),
            textParts As List(Of (Text As String, Color As Color, FontStyle As FontStyle))) As Task(Of Boolean)

            Dim tracksAsTrkPts = converter.ConvertTracksAsTrkNodesToTrackAsTrkPts(_tracksAsTrkNode)
            Me.textParts = textParts
            Return Await CreateVideoFromTrkPts(tracksAsTrkPts, Me.textParts, Me.textPartsEng)

        End Function

        ''' <summary>
        ''' Converts TRK points to geo points and creates a video.
        ''' </summary>
        ''' <param name="_tracksAsTrkPts">List of tracks in TRK point format.</param>
        ''' <param name="textParts">Styled text parts to display (e.g., Czech).</param>
        ''' <param name="textPartsEng">Styled text parts to display (e.g., English).</param>
        ''' <returns>True if video was successfully created.</returns>
        Public Async Function CreateVideoFromTrkPts(
            _tracksAsTrkPts As List(Of TrackAsTrkPts),
            textParts As List(Of (Text As String, Color As Color, FontStyle As FontStyle)),
            textPartsEng As List(Of (Text As String, Color As Color, FontStyle As FontStyle))) As Task(Of Boolean)

            Dim tracksAsGeoPoints As List(Of TrackAsGeoPoints) = converter.ConvertTracksTrkPtsToGeoPoints(_tracksAsTrkPts)
            Me.textParts = textParts
            Me.textPartsEng = textPartsEng
            Return Await CreateVideoFromGeoPoints(tracksAsGeoPoints)

        End Function

        ''' <summary>
        ''' Creates a video from tracks represented as geo points (latitude/longitude).
        ''' </summary>
        ''' <param name="_tracksAsGeoPoints">List of tracks with geographic coordinates.</param>
        ''' <returns>True if video was successfully created.</returns>
        Public Async Function CreateVideoFromGeoPoints(
            _tracksAsGeoPoints As List(Of TrackAsGeoPoints)) As Task(Of Boolean)


            Dim zoom As Integer = 18

            converter.SetCoordinatesBounds(_tracksAsGeoPoints)

            Dim downloader As New OsmTileDownloader()
            backgroundTiles = Await downloader.GetMapBitmap(
                converter.minLat, converter.maxLat,
                converter.minLon, converter.maxLon, zoom)

            Dim _TracksAsPointsF As List(Of TrackAsPointsF) =
                converter.ConvertTracksGeoPointsToPointsF(
                    _tracksAsGeoPoints, backgroundTiles.minTileX, backgroundTiles.minTileY, zoom)

            Return Await CreateVideoFromPointsF(_TracksAsPointsF)

        End Function

        ''' <summary>
        ''' Creates a video from 2D screen points (with timestamps).
        ''' </summary>
        ''' <param name="_tracksAsPointsF">List of 2D track points with timing information.</param>
        ''' <returns>True if video was successfully created.</returns>
        Public Async Function CreateVideoFromPointsF(
            _tracksAsPointsF As List(Of TrackAsPointsF)) As Task(Of Boolean)

            Dim pngDir As DirectoryInfo = Nothing
            Dim pngCreator As PngSequenceCreator = Nothing

            Await Task.Run(Sub()

                               Dim renderer As New PngRenderer(windDirection, windSpeed, Me.backgroundTiles)

                               Dim staticBgTransparent = renderer.RenderStaticTransparentBackground(_tracksAsPointsF, backgroundTiles)
                               Dim staticBgMap = renderer.RenderStaticMapBackground(_tracksAsPointsF, backgroundTiles)

                               pngDir = outputDir.CreateSubdirectory("png")
                               pngCreator = New PngSequenceCreator(renderer)

                               Dim pngTimes = pngCreator.GetPngTimes(_tracksAsPointsF)

                               pngCreator.CreateFrames(_tracksAsPointsF,
                                        staticBgTransparent, staticBgMap,
                                        pngDir, pngTimes, textParts, textPartsEng)

                           End Sub)

            Dim outputFile = IO.Path.Combine(outputDir.FullName, "overlay")
            encoder = New FfmpegVideoEncoder()
            Return Await encoder.EncodeFromPngs(pngDir, outputFile, pngCreator.frameInterval)

        End Function

    End Class

End Namespace

