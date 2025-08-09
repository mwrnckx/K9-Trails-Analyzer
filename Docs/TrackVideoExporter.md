# 🎬  TrackVideoExporter – GPX record to Video Rendering Library

**TrackVideoExporter** is a .NET library designed to create annotated overlay video from GPX tracks.
It overlays routes on map tiles and/or on a transparent background, adds timestamps and other data overlays, and produces videos, suitable for canine trailing, tracking, and general GPS recorded tracks visualization.

---


## 🎯 Features

- 🗺️Map overlay from standard tile sources (e.g. OSM, ESRI)
- 🎥 GPX track visualization on transparent background
- 🕒 Timestamp overlays and data annotation
- 🔧 Modular architecture (GPX parser, tile cache, renderer, FFmpeg-based encoder)

---

##  💡 Typical Use Case

Want to show how your dog followed a trail as a picture-in-picture?
Do you have video footage from an action camera and want to display the footage from your gps app in the foreground to see exactly how the dog worked?
This library lets you render the GPS trails (of the runner and the dog) and in the video editor you can add it to your dog's work footage.

🎬 [Example video – trail visualization in action](https://youtu.be/oKWoB5jyZcc)

---

## 🧱 Dependencies
.NET 6+

---

## 🛠️ Getting Started (**vb.net**)

```vb.net
Imports TrackVideoExporter
Imports TrackVideoExporter.TrackVideoExporter

 Public Async Function CreateVideoFromTracks() As Task(Of Boolean)
     Dim allTracks As New List(Of TrackAsTrkPts)
     For Each trkNode As XmlNode In Me.Reader.SelectNodes("trk")
         Dim TrackAsTrkptsList As XmlNodeList = Me.Reader.SelectAllChildNodes("trkpt", trkNode)
         Dim isMoving As Boolean = False 'default for other routes
         Dim trackColor As Color = Color.Green ' Default color for other tracks
         Dim label As String = Me.GetTrkType(trkNode).label
         Dim trkType As String = Me.GetTrkType(trkNode).typ
         If trkType.Trim().ToLower() = TrackTypes.Dog.Trim().ToLower() Then
             isMoving = True
             trackColor = Color.Red
         ElseIf trkType.Trim().ToLower() = TrackTypes.Runner.Trim().ToLower() Then
             trackColor = Color.Blue
         End If
         TrackAsTrkptsList = Me.Reader.SelectAllChildNodes("trkpt", trkNode)
         allTracks.Add(New TrackAsTrkPts With {
             .Label = label,
             .Color = trackColor,
             .IsMoving = isMoving,
             .TrackPoints = TrackAsTrkptsList
                         })
     Next trkNode

     ' Create a video from the tracks and save it in the video directory
     ' Get file name without extension
     Dim gpxName = System.IO.Path.GetFileNameWithoutExtension(Me.Reader.FilePath)
     ' Build a path to a new directory
     If My.Settings.VideoDirectory = "" Then My.Settings.VideoDirectory = My.Settings.Directory
     Dim directory As New IO.DirectoryInfo(System.IO.Path.Combine(My.Settings.VideoDirectory, gpxName))
     ' If the directory does not exist, create it
     If Not directory.Exists Then directory.Create()

     Dim videoCreator As New VideoExportManager(directory, WeatherData._windDirectionWeatherData._windSpeed)
     AddHandler videoCreator.WarningOccurred, AddressOf WriteRTBWarning

     Dim waitForm As New frmPleaseWait()
     waitForm.Show()

     ' Run in the background so the AI doesn't freeze
     Await Task.Run(Async Function()
                 Dim success = Await videoCreator.CreateVideoFromTrkPts(allTracks, DescriptionParts, DescriptionPartsEng)

                                    waitForm.Invoke(Sub()
                                    waitForm.Close()

                                            If success Then
                                                Dim videopath As String = IO.Path.Combine(directory.FullName, "overlay.mov")
                                                Dim bgPNGPath As String = IO.Path.Combine(directory.FullName, "background.png")
                                                Dim form As New frmVideoDone(videopath, bgPNGPath)
                                                form.ShowDialog()
                                                form.Dispose()
                                            Else
                                                MessageBox.Show("Video creation failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                                            End If
                                        End Sub)
                    End Function)

     Return False
 End Function
