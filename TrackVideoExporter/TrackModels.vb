Imports System.Drawing
Imports System.Windows.Forms
Imports System.Xml

Public Class TrackPointF
    Public Property Location As PointF
    Public Property Time As DateTime
End Class

Public MustInherit Class TrackAs 'track as vzor pro všechny třídy reprezentující trasy
    Public Sub New(trackType As TrackType)
        Me.TrackType = trackType
    End Sub

    Public Property TrackType As TrackType


    ' Volitelný override
    Public Property LabelOverride As String = Nothing
    Public Property ColorOverride As Color? = Nothing
    Public Property IsMovingOverride As Boolean? = Nothing
    ''' <summary>
    ''' Returns a label for the track based on its type or an override if provided.
    ''' </summary>
    ''' <returns>Label string for the track.</returns>
    ''' <remarks>Uses TrackTypeResolvers.LabelResolver to get the default label.</remarks>
    Public ReadOnly Property Label As String
        Get
            If LabelOverride IsNot Nothing Then Return LabelOverride
            Return TrackTypeResolvers.LabelResolver(Me.TrackType)
        End Get
    End Property
    ''' <summary>
    ''' Returns the color for the track based on its type or an override if provided.
    ''' </summary>
    ''' <returns>Color for the track.</returns>
    ''' <remarks>Uses default colors based on TrackType if no override is provided.</remarks>
    Public ReadOnly Property Color As Color
        Get
            If ColorOverride.HasValue Then Return ColorOverride.Value
            Select Case Me.TrackType
                Case TrackType.RunnerTrail : Return Color.Blue
                Case TrackType.DogTrack : Return Color.Red
                Case TrackType.CrossTrail : Return Color.Green
                Case TrackType.article : Return Color.Orange
                Case Else : Return Color.Black
            End Select
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether the track is dynamic based on its type or an override if provided.
    ''' </summary>
    ''' <returns>True if the track is moving, otherwise False.</returns>
    ''' <remarks>Defaults to True for DogTrack, False for others unless overridden.</remarks>
    Public ReadOnly Property IsMoving As Boolean
        Get
            If IsMovingOverride IsNot Nothing Then Return IsMovingOverride.Value
            Select Case Me.TrackType
                Case TrackType.DogTrack : Return True
                Case Else : Return False
            End Select
        End Get
    End Property
End Class

Public Class TrackGeoPoint
    ''' <summary>
    ''' The geographic coordinates (latitude and longitude).
    ''' </summary>
    Public Property Location As Coordinates
    ''' <summary>
    ''' The timestamp corresponding to this geo point.
    ''' </summary>
    Public Property Time As DateTime
End Class

Public Class Coordinates
    Public Property Lat As Double
    Public Property Lon As Double
End Class
''' <summary>
''' TrackAsGeoPoints class represents a track with geographic points (latitude, longitude, timestamp).
''' </summary>
Public Class TrackAsGeoPoints
    Inherits TrackAs


    ''' <param name="trackType">The type of the track.</param>
    ''' <param name="trackGeoPoints">A list of geo points (latitude, longitude, timestamp) for the track.</param>
    Public Sub New(trackType As TrackType, trackGeoPoints As List(Of TrackGeoPoint))
        MyBase.New(trackType)
        Me.TrackGeoPoints = trackGeoPoints
    End Sub
    ''' <summary>
    ''' List of geo points (latitude, longitude, timestamp) for the track.
    ''' </summary>
    Public Property TrackGeoPoints As List(Of TrackGeoPoint)
End Class

''' <summary>
''' TrackAsPointsF class represents a track with 2D points (pixel coordinates and timestamps).
''' </summary>
Public Class TrackAsPointsF
    Inherits TrackAs
    ''' <summary>
    ''' Initializes a new instance of the TrackAsPointsF class.
    ''' </summary>
    ''' <param name="trackType">The type of the track.</param>
    ''' <param name="trackPointsF">A list of 2D points (pixel coordinates and timestamps) for the track.</param>
    Public Sub New(trackType As TrackType, trackPointsF As List(Of TrackPointF))
        MyBase.New(trackType)
        Me.TrackPointsF = trackPointsF
    End Sub
    ''' <summary>
    ''' List of 2D points (pixel coordinates and timestamps) for the track.
    ''' </summary>
    Public Property TrackPointsF As List(Of TrackPointF)
End Class
''' <summary>
''' 
''' </summary>
Public Class TrackAsTrkPts 'track as trackPoints
    Inherits TrackAs
    ''' <summary>
    ''' Initializes a new instance of the TrackAsTrkPts class.
    ''' </summary>
    ''' <param name="trackType">The type of the track.</param>
    ''' <param name="trackPoints">A list of XML nodes representing track points in the GPX file.</param>
    Public Sub New(trackType As TrackType, trackPoints As XmlNodeList)
        MyBase.New(trackType)
        Me.TrackPoints = trackPoints
    End Sub
    ''' <summary>
    ''' List of XML nodes representing track points in the GPX file.
    ''' </summary>
    ''' <remarks>Each node corresponds to a trkpt element in the GPX file.</remarks>
    ''' <returns>XmlNodeList containing track points.</returns>
    Public Property TrackPoints As XmlNodeList
End Class

''' <summary>
''' 
''' </summary>
Public Class TrackAsTrkNode 'track as trkNode
    Inherits TrackAs
    ''' <summary>
    ''' Initializes a new instance of the TrackAsTrkNode class.
    ''' </summary>
    ''' <param name="trkNode"></param>
    ''' <param name="trackType"></param>
    Public Sub New(trkNode As XmlNode, trackType As TrackType)
        MyBase.New(trackType)
        Me.TrkNode = trkNode
    End Sub
    ''' <summary>
    ''' XmlNode representing the entire trk element in the GPX file.
    ''' </summary>
    Public Property TrkNode As XmlNode

    Public ReadOnly Property StartTrackGeoPoint As TrackGeoPoint
        Get
            Dim conv As New TrackConverter
            Dim trkSeg As XmlNode = conv.SelectSingleChildNode("trkseg", TrkNode)
            If trkSeg Is Nothing Then Return Nothing
            Dim firstTrkPt As XmlNode = conv.SelectSingleChildNode("trkpt", trkSeg)
            If firstTrkPt Is Nothing Then Return Nothing
            Dim lat, lon As Double
            Try
                lat = Double.Parse(firstTrkPt.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
                lon = Double.Parse(firstTrkPt.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)
            Catch ex As Exception
                Return Nothing
            End Try
            Dim timenode = conv.SelectSingleChildNode("time", firstTrkPt)
            Dim time As DateTime = DateTime.MinValue
            If timenode IsNot Nothing Then
                time = DateTime.Parse(timenode.InnerText, Nothing, Globalization.DateTimeStyles.AssumeUniversal)
            Else
                Dim trkname As String = conv.SelectSingleChildNode("name", TrkNode).InnerText.Trim()
                Dim userInput = conv.PromptForStartTime(trkname, "start")
                If userInput.HasValue Then
                    Dim localtime = userInput.Value
                    time = localtime.ToUniversalTime()
                    conv.CreateAndAddElement(firstTrkPt, "time", time.ToString("yyyy-MM-ddTHH:mm:ssZ"), False)
                Else
                    ' Nezadáno nebo zrušeno
                    MessageBox.Show("The time has not been completed. The track will be skipped..", "Upozornění", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End If

            Dim geopoint As New TrackGeoPoint With {
                    .Location = New Coordinates With {.Lat = lat, .Lon = lon},
                    .Time = time
                }
            Return geopoint
        End Get
    End Property

    Public ReadOnly Property EndTrackGeoPoint As TrackGeoPoint
        Get
            Dim conv As New TrackConverter
            Dim trkPts As XmlNodeList = conv.SelectTrkptNodes(TrkNode)
            If trkPts Is Nothing OrElse trkPts.Count = 0 Then Return Nothing
            Dim lastTrkPt As XmlNode = trkPts(trkPts.Count - 1)
            If lastTrkPt Is Nothing Then Return Nothing

            Dim lat, lon As Double
            Try
                lat = Double.Parse(lastTrkPt.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
                lon = Double.Parse(lastTrkPt.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)
            Catch ex As Exception
                Return Nothing
            End Try

            Dim timenode = conv.SelectSingleChildNode("time", lastTrkPt)
            Dim time As DateTime = DateTime.MinValue
            If timenode IsNot Nothing Then
                time = DateTime.Parse(timenode.InnerText, Nothing, Globalization.DateTimeStyles.AssumeUniversal)
            Else
                Dim trkname As String = conv.SelectSingleChildNode("name", TrkNode).InnerText.Trim()
                Dim userInput = conv.PromptForStartTime(trkname, "end")
                If userInput.HasValue Then
                    Dim localtime = userInput.Value
                    time = localtime.ToUniversalTime()
                    conv.CreateAndAddElement(lastTrkPt, "time", time.ToString("yyyy-MM-ddTHH:mm:ssZ"), False)
                Else
                    ' Nezadáno nebo zrušeno
                    MessageBox.Show("The time has not been completed. The track will be skipped..", "Upozornění", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End If

            Dim geopoint As New TrackGeoPoint With {
                    .Location = New Coordinates With {.Lat = lat, .Lon = lon},
                    .Time = time
                }
            Return geopoint
        End Get
    End Property



    ''' <summary>
    ''' Converts the current TrackAsTrkNode to a TrackAsTrkPts object.
    ''' </summary>
    ''' <returns>A TrackAsTrkPts object containing the track points.</returns>
    ''' <remarks>This method uses the TrackConverter to perform the conversion.</remarks>
    Public Function toTrackAsTrkPts() As TrackAsTrkPts
        Dim converter As New TrackConverter()
        Dim _trackAsTrkPts As TrackAsTrkPts = converter.ConvertTrackAsTrkNodeToTrkPts(Me)
        Return _trackAsTrkPts
    End Function

End Class

''' <summary>
''' TrackType enumeration defines the types of tracks that can be represented.
''' </summary>
''' <remarks>
''' This enumeration is used to categorize different types of tracks, such as Runner trails, dog tracks, and cross-trails.
''' </remarks>>
Public Enum TrackType
    Unknown = 0
    RunnerTrail
    DogTrack
    CrossTrail
    article 'scent article or checkPoint
End Enum

''' <summary>
''' TrackTypeResolvers module provides default resolvers for track type labels.
''' </summary>
''' <remarks>
''' This module allows customization of how track types are displayed in the UI.
''' The default resolver simply returns the name of the enum value as a string.
''' </remarks>
Public Module TrackTypeResolvers ' This module provides default resolvers for track type labels
    Public Property LabelResolver As Func(Of TrackType, String) = Function(tt) tt.ToString()
End Module


''' <summary>
''' Represents a text block with a specific font and color.
''' </summary>
''' <example>StyledText("Hello World", Color.Red, FontStyle.Bold)</example>

Public Class StyledText

    ''' <summary>
    ''' Text content of the styled text.
    ''' </summary>
    ''' <remarks>Can be used for displaying styled text in UI components.</remarks>
    Public Property Text As String
    Public Property Font As Font
    Public Property Color As Color
    Public Property Label As String
    ''' <summary>
    ''' Initializes a new instance of the StyledText class with specified text, color, and font.
    ''' </summary>
    ''' <param name="text">The text to display.</param>
    ''' <param name="color">The color of the text.</param>
    ''' <param name="font">The font of the text.</param>
    ''' <param name="label">Optional label for the styled text.</param>

    Public Sub New(text As String, color As Color, font As Font, label As String)
        Me.Text = text
        Me.Font = font
        Me.Color = color
        Me.Label = label
    End Sub
    Public Sub New()
        ' Default constructor for serialization or other purposes
        Me.Text = "Default Text"
    End Sub

End Class



''' <summary>
''' Contains a language-dependent description of the three main parts of the mantrailing treble:
'''Goal''', ''Trail'', and ''Performance''.
''' </summary>
Public Class TrailReport
    ' 🔧 Lokálně nastav labely 
    Public Const dogLabel As String = "🐕"
    Public Const goalLabel As String = "📍"
    Public Const trailLabel As String = "👣"
    Public Const performanceLabel As String = "🏅"

    Public Property Title As StyledText

    Public Property Category As StyledText
    ''' <summary>
    ''' Description of the goal of the training session.
    ''' </summary>
    Public Property Goal As StyledText



    ''' <summary>
    ''' Description of the course of the track - where and how it was led, terrain, length, age, etc.
    ''' </summary>
    Public Property Trail As StyledText

    ''' <summary>
    ''' Evaluation of the team's performance - how the dog and handler did on the given track.
    ''' </summary>
    Public Property Performance As StyledText

    ''' <summary>
    ''' Evaluation of the team's performance - how the dog and handler did on the given track.
    ''' </summary>
    Public Property PerformancePoints As StyledText

    ''' <summary>
    ''' Weather conditions during the training session.
    ''' </summary>
    ''' <remarks>Contains temperature, wind speed, wind direction, precipitation, relative humidity, and cloud cover.</remarks>
    Public Property weather As StyledText

    Public Property WeatherData As (_temperature As Double?, _windSpeed As Double?, _windDirection As Double?, _precipitation As Double?, _relHumidity As Double?, _cloudCover As Double?)

    'Public Property ScoringData As ScoringData



    ''' <param name="goal">styledText description of the search goal.</param>
    ''' <param name="trail">styledText description of the trail course and parameters.</param>
    ''' <param name="performance">styledText evaluation of team performance (dog + driver).</param>
    Public Sub New(title As String, category As String, goal As String, trail As String, performance As String, points As String, _weatherdata As (_temperature As Double?, _windSpeed As Double?, _windDirection As Double?, _precipitation As Double?, _relHumidity As Double?, _cloudCover As Double?), Optional weather As String = " ")
        Dim mainFont As New Font("Segoe UI Semibold", 12, FontStyle.Bold)
        Me.Title = New StyledText(title, Color.Firebrick, mainFont, "")
        Me.Category = New StyledText(category, Color.Maroon, mainFont, dogLabel)
        Me.Goal = New StyledText(goal, Color.DarkGreen, mainFont, goalLabel)
        Me.Trail = New StyledText(trail, Color.DarkGreen, mainFont, trailLabel)
        Me.Performance = New StyledText(performance, Color.DarkGreen, mainFont, performanceLabel)
        Me.PerformancePoints = New StyledText(points, Color.Maroon, mainFont, "")
        Me.weather = New StyledText(weather, Color.Maroon, mainFont, "")
        Me.WeatherData = _weatherdata
        'Me.ScoringData = _scoringData
    End Sub
    Public Sub New()
        ' Default constructor for serialization or other purposes
        Me.Category = New StyledText("Dog Name", Color.DarkBlue, New Font("Cascadia Code", 12, FontStyle.Bold), " ")
        Me.Goal = New StyledText("Goal of the training session", Color.DarkGreen, New Font("Cascadia Code", 12, FontStyle.Bold), " ")
        Me.Trail = New StyledText("Course of the track", Color.Blue, New Font("Cascadia Code", 12, FontStyle.Bold), " ")
        Me.Performance = New StyledText("Evaluation of the team's performance", Color.Red, New Font("Cascadia Code", 12, FontStyle.Bold), " ")
        Me.weather = New StyledText("Weather conditions will be added later.", Color.Maroon, New Font("Cascadia Code", 12, FontStyle.Bold), " ")
    End Sub
    ''' <summary>
    ''' Converts the TrailReport to a list of StyledText objects.   Basic parts only.
    ''' </summary>
    ''' <returns></returns>
    Public Function ToBasicList(Optional title As String = "Trail description") As List(Of StyledText)
        Me.Title.Text = title
        Dim result As New List(Of StyledText) From {
              Me.Title,
        Me.Category,
            Me.Goal,
            Me.Trail,
            Me.Performance,
            Me.weather
        }
        Return result

    End Function
    ''' <summary>
    ''' Converts the TrailReport to a list of StyledText objects.   Competition parts only.
    ''' </summary>
    ''' <returns></returns>
    Public Function ToCompetitionList(Optional title As String = "Scoring") As List(Of StyledText)
        Me.Title.Text = title
        Dim result As New List(Of StyledText) From {
          Me.Title,
          Me.PerformancePoints
        }
        Return result

    End Function
End Class

''' <summary>
''' structure for returning calculation results.
''' </summary>
Public Class TrailStats
    Public Property DogDistance As Double ' Distance actually traveled by the dog (measured from the dog's route)
    Public Property RunnerDistance As Double ' Distance actually traveled by the runner (measured from the runner's route)
    Public Property WeightedDistanceAlongTrail As Double ' Distance traveled by the dog as measured from the runners's route with weighting by deviation
    Public Property WeightedDistanceAlongTrailPerCent As Double ' Distance traveled by the dog as measured from the runners's route with weighting by deviation
    Public Property WeightedTimePerCent As Double ' Total time of the dog with weighting by deviation divided by total time
    Public Property TrailAge As TimeSpan ' age of the trail 
    Public Property TotalTime As TimeSpan ' total time of the dog's route
    Public Property MovingTime As TimeSpan ' net time the dog moved
    Public Property StoppedTime As TimeSpan ' the time the handler stood the dog also stood or performed a perimeter (looking for a trail)
    Public Property DogNetSpeed As Double ' net speed (moving time only), calculated from the length of the dog's route
    Public Property DogGrossSpeed As Double 'gross speed calculated from the last checkpoint or the dog's last point if the dog is close to the track
    Public Property Deviation As Double ' average deviation of the entire dog's route from the runner's track
    Public Property PointsInMTCompetition As ScoringData '(RunnerFoundPoints As Integer, DogSpeedPoints As Integer, DogAccuracyPoints As Integer, DogReadingPoints As Integer, dogName As String, handlerName As String) ' number of points in MT Competition according to the rules
    Public Property CheckpointsEval As List(Of CheckpointData) '(distanceAlongTrail As Double, deviationFromTrail As Double, dogGrossSpeed As Double)) ' evaluation of checkpoints: distance from start along the runner's route and distance from the route in meters
    Public Property MaxTeamDistance As Double ' maximum distance in metres reached by the team along the runners track (the whole track distance in case of found, the last waypoint near the track if not found)
    Public Property RunnerFound As Boolean ' whether dog found the runner or not
End Class

' Struktura pro data checkpointu
Public Structure CheckpointData
    Public distanceAlongTrail As Double
    Public deviationFromTrail As Double
    Public dogGrossSpeedkmh As Double
End Structure

Public Class ScoringData
    Public Property RunnerFoundPoints As Integer
    Public Property DogSpeedPoints As Integer
    Public Property DogAccuracyPoints As Integer
    Public Property DogReadingPoints As Integer
    Public Property dogName As String
    Public Property handlerName As String
End Class







