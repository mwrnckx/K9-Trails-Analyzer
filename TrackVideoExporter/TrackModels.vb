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
                Case TrackType.Artickle : Return Color.Orange
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
    ''' <remarks>Each node corresponds to a <trkpt> element in the GPX file.</remarks>
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
    ''' XmlNode representing the entire <trk> element in the GPX file.
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

    Public ReadOnly Property TrackDistance As Double
        Get
            Dim conv As New TrackConverter()
            Return conv.CalculateTrailDistance(Me.TrkNode)
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
    Artickle 'scent artickle
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

    Public Property DogName As StyledText
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
    ''' Weather conditions during the training session.
    ''' </summary>
    ''' <remarks>Contains temperature, wind speed, wind direction, precipitation, relative humidity, and cloud cover.</remarks>

    Public Property weather As StyledText


    ''' <summary>
    ''' Initializes a new instance of the TrailDescription class with destination, trail and performance descriptions.
    ''' </summary>
    ''' <param name="dogName">styledText description of the dog name.</param>
    ''' <remarks>
    ''' The weather conditions are initialized with a placeholder text.
    ''' </remarks>
    ''' <param name="goal">styledText description of the search goal.</param>
    ''' <param name="trail">styledText description of the trail course and parameters.</param>
    ''' <param name="performance">styledText evaluation of team performance (dog + driver).</param>
    Public Sub New(dogName As String, goal As String, trail As String, performance As String, Optional weather As String = " ")
        Me.DogName = New StyledText(dogName, Color.DarkBlue, New Font("Cascadia Code", 12, FontStyle.Bold), doglabel)
        Me.Goal = New StyledText(goal, Color.DarkGreen, New Font("Cascadia Code", 12, FontStyle.Bold), goalLabel)
        Me.Trail = New StyledText(trail, Color.Blue, New Font("Cascadia Code", 12, FontStyle.Bold), trailLabel)
        Me.Performance = New StyledText(performance, Color.Red, New Font("Cascadia Code", 12, FontStyle.Bold), performanceLabel)
        Me.weather = New StyledText(weather, Color.Maroon, New Font("Cascadia Code", 12, FontStyle.Bold), "")
    End Sub
    Public Sub New()
        ' Default constructor for serialization or other purposes
        Me.DogName = New StyledText("Dog Name", Color.DarkBlue, New Font("Cascadia Code", 12, FontStyle.Bold), " ")
        Me.Goal = New StyledText("Goal of the training session", Color.DarkGreen, New Font("Cascadia Code", 12, FontStyle.Bold), " ")
        Me.Trail = New StyledText("Course of the track", Color.Blue, New Font("Cascadia Code", 12, FontStyle.Bold), " ")
        Me.Performance = New StyledText("Evaluation of the team's performance", Color.Red, New Font("Cascadia Code", 12, FontStyle.Bold), " ")
        Me.weather = New StyledText("Weather conditions will be added later.", Color.Maroon, New Font("Cascadia Code", 12, FontStyle.Bold), " ")
    End Sub
    Public Function toList() As List(Of StyledText)
        Dim result As New List(Of StyledText) From {
            Me.DogName,
            Me.Goal,
            Me.Trail,
            Me.Performance,
            Me.weather
        }
        Return result

    End Function
End Class








