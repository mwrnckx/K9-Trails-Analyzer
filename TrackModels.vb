Imports System.Xml

Public Class TrackPointF
    Public Property Location As PointF
    Public Property Time As DateTime
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

Public Class TrackAsGeoPoints
    ''' <summary>
    ''' Label describing the track (e.g., dog, handler).
    ''' </summary>
    Public Property Label As String

    ''' <summary>
    ''' Color used to draw the track.
    ''' </summary>
    Public Property Color As Color

    ''' <summary>
    ''' Indicates if this track represents a moving object.
    ''' </summary>
    Public Property IsMoving As Boolean = False

    ''' <summary>
    ''' List of geo points (latitude, longitude, timestamp) for the track.
    ''' </summary>
    Public Property TrackGeoPoints As List(Of TrackGeoPoint)

End Class

Public Class TrackAsPointsF
    ''' <summary>
    ''' Label describing the track.
    ''' </summary>
    Public Property Label As String

    ''' <summary>
    ''' Color used to draw the track.
    ''' </summary>
    Public Property Color As Color

    ''' <summary>
    ''' Indicates if this track represents a moving object.
    ''' </summary>
    Public Property IsMoving As Boolean = False

    ''' <summary>
    ''' List of 2D points (pixel coordinates and timestamps) for the track.
    ''' </summary>
    Public Property TrackPointsF As List(Of TrackPointF)

End Class

Public Class TrackAsTrkPts 'track as trackPoints
    ''' <summary>
    ''' Label describing the track.
    ''' </summary>
    Public Property Label As String

    ''' <summary>
    ''' Color used to draw the track.
    ''' </summary>
    Public Property Color As Color

    ''' <summary>
    ''' Indicates if this track represents a moving object.
    ''' </summary>
    Public Property IsMoving As Boolean = False

    ''' <summary>
    ''' XmlNodeList containing all trkpt elements in the track.
    ''' </summary>
    Public Property TrackPoints As XmlNodeList

End Class

Public Class TrackAsTrkNode 'track as trkNode
    ''' <summary>
    ''' Label describing the track.
    ''' </summary>
    Public Property Label As String

    ''' <summary>
    ''' Color used to draw the track.
    ''' </summary>
    Public Property Color As Color

    ''' <summary>
    ''' Indicates if this track represents a moving object.
    ''' </summary>
    Public Property IsMoving As Boolean = False

    ''' <summary>
    ''' The XmlNode representing the trk element of the track.
    ''' </summary>
    Public Property TrkNode As XmlNode

End Class


