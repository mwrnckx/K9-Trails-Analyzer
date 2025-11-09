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


    Public Async Function CreateVideoFromGPXRecord(_gpxRecord As GPXRecord) As Task(Of Boolean)

        ' Create a video from the dog &runner tracks and save it in the video directory
        Dim gpxName = System.IO.Path.GetFileNameWithoutExtension(_gpxRecord.FileName)

        Dim directory As New IO.DirectoryInfo(System.IO.Path.Combine(My.Settings.VideoDirectory, gpxName))
        ' If the directory does not exist, create it
        If Not directory.Exists Then directory.Create()
        Dim FFmpegPath As String = FindAnSaveFfmpegPath()
        Dim videoCreator As New VideoExportManager(FFmpegPath, directory, _gpxRecord.WeatherData.windDirection, _gpxRecord.WeatherData.windSpeed)
        AddHandler videoCreator.WarningOccurred, AddressOf WriteRTBWarning

        Dim waitForm As New frmPleaseWait("I'm making an overlay video, please stand by...")
        waitForm.Show()

        ' Run in the background so the AI doesn't freeze
        Await Task.Run(Async Function()

                           Dim success = Await videoCreator.CreateVideoFromTrkNodes(_gpxRecord.Tracks, _gpxRecord.TrailStats.MaxDeviationGeoPoints, _gpxRecord.WptNodes, _gpxRecord.LocalisedReports)

                           ' When finished, return to the UI thread and perform the actions
                           waitForm.Invoke(Sub()
                                               waitForm.Close()

                                               If success Then
                                                   Dim videopath As String = IO.Path.Combine(directory.FullName, "overlay.webm")
                                                   Dim bgPNGPath As String = IO.Path.Combine(directory.FullName, "TracksOnMap.png")
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
