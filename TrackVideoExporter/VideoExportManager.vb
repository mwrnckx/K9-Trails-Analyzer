
Imports System.Drawing
Imports System.IO

Namespace TrackVideoExporter


    ''' <summary>
    ''' Class responsible for creating overlay videos from GPS tracks.
    ''' </summary>
    Public Class VideoExportManager

        Private converter As TrackConverter
        Private encoder As FfmpegVideoEncoder
        Private FFMpegPath As String
        ''' <summary>
        ''' Directory where output images and video will be saved.
        ''' </summary>
        Private outputDir As DirectoryInfo
        Private windDirection As Double?
        Private windSpeed As Double?

        Private backgroundTiles As (bgmap As Bitmap, minTileX As Single, minTileY As Single) = (Nothing, 0, 0)
        Private LocalisedReports As New Dictionary(Of String, TrailReport)
        'Private textParts As New List(Of (Text As String, Color As Color, FontStyle As FontStyle))
        'Private textPartsEng As New List(Of (Text As String, Color As Color, FontStyle As FontStyle))

        ''' <summary>
        ''' Raised when a non-critical warning occurs during processing.
        ''' </summary>
        Public Event WarningOccurred(message As String, _color As Color)

        ''' <summary>
        ''' Initializes a new instance of the <see cref="VideoExportManager"/> class.
        ''' </summary>
        ''' <param name="FFMpegPath">Path to the FFMpeg executable.</param>
        ''' <param name="outputDir">Output directory for generated images and video.</param>
        ''' <param name="windDir">Optional wind direction in degrees.</param>
        ''' <param name="windSpeed">Optional wind speed.</param>
        ''' <param name="LocalisedReports">Optional dictionary of localised trail reports.</param>
        Public Sub New(FFMpegPath As String, outputDir As DirectoryInfo,
                       Optional windDir As Double? = Nothing,
                       Optional windSpeed As Double? = Nothing,
                         Optional LocalisedReports As Dictionary(Of String, TrailReport) = Nothing)
            Me.FFMpegPath = FFMpegPath
            Me.outputDir = outputDir
            Me.windDirection = windDir
            Me.windSpeed = windSpeed
            Me.LocalisedReports = LocalisedReports
            converter = New TrackConverter()

        End Sub

        ''' <summary>
        ''' Converts TRK nodes to geo points and generates an overlay video.
        ''' </summary>
        '''<param name="localisedReports"></param>
        '''<param name="_tracksAsTrkNode"> </param>
        Public Async Function CreateVideoFromTrkNodes(_tracksAsTrkNode As List(Of TrackAsTrkNode), Optional waypoints As TrackAsTrkPts = Nothing, Optional LocalisedReports As Dictionary(Of String, TrailReport) = Nothing) As Task(Of Boolean)
            Dim tracksAsTrkPts = converter.ConvertTracksAsTrkNodesToTrackAsTrkPts(_tracksAsTrkNode)
            Me.LocalisedReports = LocalisedReports
            Return Await CreateVideoFromTrkPts(tracksAsTrkPts, waypoints, Me.LocalisedReports)
        End Function

        ''' <summary>
        ''' Converts TRK points to geo points and creates a video.
        ''' </summary>
        ''' <param name="_tracksAsTrkPts">List of tracks in TRK point format.</param>
        ''' <param name="LocalisedReports">Dictionary of localised trail reports.</param>
        ''' <returns>True if video was successfully created.</returns>
        Public Async Function CreateVideoFromTrkPts(
            _tracksAsTrkPts As List(Of TrackAsTrkPts),
            waypoints As TrackAsTrkPts,
               LocalisedReports As Dictionary(Of String, TrailReport)) As Task(Of Boolean)
            Dim wayPointsAsGeoPoints As TrackAsGeoPoints = converter.ConvertTrackTrkPtsToGeoPoints(waypoints)
            Dim tracksAsGeoPoints As List(Of TrackAsGeoPoints) = converter.ConvertTracksTrkPtsToGeoPoints(_tracksAsTrkPts)
            Me.LocalisedReports = LocalisedReports
            Return Await CreateVideoFromGeoPoints(tracksAsGeoPoints, wayPointsAsGeoPoints)
        End Function

        ''' <summary>
        ''' Creates a video from tracks represented as geo points (latitude/longitude).
        ''' </summary>
        ''' <param name="_tracksAsGeoPoints">List of tracks with geographic coordinates.</param>
        ''' <returns>True if video was successfully created.</returns>
        Public Async Function CreateVideoFromGeoPoints(
            _tracksAsGeoPoints As List(Of TrackAsGeoPoints),
          Optional waypointsAsGeoPoints As TrackAsGeoPoints = Nothing) As Task(Of Boolean)

            Dim zoom As Integer = 18
            converter.SetCoordinatesBounds(_tracksAsGeoPoints)
            Dim downloader As New OsmTileDownloader()
            backgroundTiles = Await downloader.GetMapBitmap(
                converter.minLat, converter.maxLat,
                converter.minLon, converter.maxLon, zoom)

            Dim _TracksAsPointsF As List(Of TrackAsPointsF) =
                converter.ConvertTracksGeoPointsToPointsF(
                    _tracksAsGeoPoints, backgroundTiles.minTileX, backgroundTiles.minTileY, zoom)
            Dim wayPointsAsPointsF As TrackAsPointsF =
                converter.ConvertTrackGeoPointsToPointsF(
                    waypointsAsGeoPoints, backgroundTiles.minTileX, backgroundTiles.minTileY, zoom)

            Return Await CreateVideoFromPointsF(_TracksAsPointsF, wayPointsAsPointsF)

        End Function

        ''' <summary>
        ''' Creates a video from 2D screen points (with timestamps).
        ''' </summary>
        ''' <param name="_tracksAsPointsF">List of 2D track points with timing information.</param>
        ''' <returns>True if video was successfully created.</returns>
        Public Async Function CreateVideoFromPointsF(
            _tracksAsPointsF As List(Of TrackAsPointsF),
            Optional waypointsAsPointsF As TrackAsPointsF = Nothing) As Task(Of Boolean)

            Dim pngDir As DirectoryInfo = Nothing
            Dim pngCreator As PngSequenceCreator = Nothing

            Await Task.Run(Sub()

                               Dim renderer As New PngRenderer(windDirection, windSpeed, Me.backgroundTiles)
                               renderer.CreateWindArrowBitmap(outputDir)
                               Dim staticBgTransparent = renderer.RenderStaticTransparentBackground(_tracksAsPointsF, backgroundTiles, waypointsAsPointsF)
                               Dim staticBgMap = renderer.RenderStaticMapBackground(_tracksAsPointsF, backgroundTiles, waypointsAsPointsF)


                               pngCreator = New PngSequenceCreator(renderer)

                               Dim pngTimes = pngCreator.GetPngTimes(_tracksAsPointsF)

                               pngCreator.CreateFrames(_tracksAsPointsF,
                                        staticBgTransparent, staticBgMap,
                                        outputDir, pngTimes, Me.LocalisedReports)

                           End Sub)

            Dim outputFile = IO.Path.Combine(outputDir.FullName, "overlay")
            encoder = New FfmpegVideoEncoder()
            Return Await encoder.EncodeFromPngs(FFMpegPath, outputDir, outputFile, pngCreator.frameInterval)

        End Function

    End Class

End Namespace

