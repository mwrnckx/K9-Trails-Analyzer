Imports System.Diagnostics
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Reflection.Metadata
Imports System.Runtime.InteropServices.JavaScript.JSType
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar
'Imports System.Windows.Media.Media3D
'Imports System.Windows.Media.TextFormatting
Imports System.Xml

''' <summary>
''' Creates a video from GPS tracks by converting them to images and encoding them into a video file.
''' Supports multiple input formats: GeoPoints, PointsF, trkpt XmlNodes, and full trk XmlNode.
''' Designed especialy for K9 handlers, visualizing dog's dynamic track on background of static trail-layer's track. Cross tracks can be shown too.
''' </summary>
''' <remarks>
''' The generated video shows both static and moving tracks, with customizable colors and labels.
''' Internally uses ffmpeg for video encoding.
''' </remarks>
''' <example>
''' Dim creator As New VideoCreator(videoFolder, 500)
''' creator.CreateVideoFromGeoPoints(listOfTracks)
''' </example>
Friend Class VideoCreator
    'pro všechny vstupy:
    Private directory As IO.DirectoryInfo
    Dim bgPNG As Bitmap 'pozadí pro závěrečný Formulář, bude nastaveno na prostřední snímek
    Private Property minVideoSize As Single = 300 'minimální velikost obrázku v pixelech, pokud je menší, zvětší se na tuto hodnotu
    Const minFrameInterval As Double = 3.0 'minimální interval mezi snímky v sekundách, aby video nebylo moc velké a rychlé, defaultně 3 sekundy

    'přetížení vstupu dat:
    Private Property TracksAsTrkPts As List(Of TrackAsTrkPts)
    Private Property TracksAsPointsF As List(Of TrackAsPointsF)
    Private Property TracksAsGeoPoints As List(Of TrackAsGeoPoints)
    Private Property TracksAsTrkNode As List(Of TrackAsTrkNode)
    Private Property WindDirection As Double?    ' směr větru v stupních, pokud je potřeba, defaultně nothing
    Private Property Windspeed As Double  ' rychlost větru v m/s, pokud je potřeba, defaultně 0 (klid)

    Dim imgWidth As Integer = 600
    Dim imgHeight As Integer = 600
    Dim PNGTimes As New List(Of DateTime) ' Časové značky pro PNG obrázky, pokud nejsou, vytvoří se z GPS bodů psa)
    Dim pngDirectory As DirectoryInfo ' Adresář pro PNG obrázky
    Dim pixelsPerMetre As Double = 1.0 'přepočet z GPS na pixely, defaultně 1 pixel = 1 metr


    ReadOnly latDistancePerDegree As Double = 111_320.0 ' průměrně ~111,3 km na jeden stupeň latitude
    Dim lonDistancePerDegree As Double

    Dim minLat As Double = Double.MaxValue, maxLat As Double = Double.MinValue
    Dim minLon As Double = Double.MaxValue, maxLon As Double = Double.MinValue

    Public Event WarningOccurred(message As String, _color As Color)

    ''' <summary>
    ''' Initializes a new instance of the VideoCreator class.
    ''' </summary>
    ''' <param name="videoPath">Directory where images and the video will be saved.</param>
    ''' <param name="_minVideoSize">Minimum size of the output video in pixels.</param>
    Public Sub New(videoPath As IO.DirectoryInfo, _minVideoSize As Single, Optional _windDir As Double = Nothing, Optional _windSpeed As Double = Nothing)
        ' Nastav cestu k adresáři, kde se budou ukládat obrázky a video
        Me.directory = videoPath
        'pomocný adresář pro PNG obrázky
        ' Pokud adresář neexistuje, vytvoř ho
        pngDirectory = Me.directory.CreateSubdirectory("png")
        minVideoSize = _minVideoSize
        WindDirection = _windDir
        Windspeed = _windSpeed
    End Sub

    ''' <summary>
    ''' Creates a video from tracks represented as geo points (latitude/longitude coordinates).
    ''' </summary>
    ''' <param name="_tracksAsGeoPoints">List of tracks containing geo points.</param>
    Public Sub CreateVideoFromGeoPoints(_tracksAsGeoPoints As List(Of TrackAsGeoPoints))
        TracksAsGeoPoints = _tracksAsGeoPoints
        Dim textSize As Single = 0
        For Each Track In _tracksAsGeoPoints

            For Each geoPoint As TrackGeoPoint In Track.TrackGeoPoints
                minLat = Math.Min(minLat, geoPoint.Location.Lat)
                maxLat = Math.Max(maxLat, geoPoint.Location.Lat)
                minLon = Math.Min(minLon, geoPoint.Location.Lon)
                maxLon = Math.Max(maxLon, geoPoint.Location.Lon)
            Next
        Next

        'add 5% to the edges
        Dim margins = (maxLat - minLat) * 0.05
        minLat -= margins
        maxLat += margins
        margins = (maxLon - minLon) * 0.05
        minLon -= margins
        maxLon += margins


        ' Calculate the width and height of the figure in metres
        Dim centerLat As Double = (minLat + maxLat) / 2
        lonDistancePerDegree = Math.Cos(centerLat * Math.PI / 180) * latDistancePerDegree
        Dim widthInMeters As Double = (maxLon - minLon) * lonDistancePerDegree
        Dim heightInMeters As Double = (maxLat - minLat) * latDistancePerDegree
        pixelsPerMetre = Math.Min(minVideoSize / widthInMeters, minVideoSize / heightInMeters) 'přepočet z GPS na pixely, defaultně 1 pixel = 1 metr
        Me.imgWidth = widthInMeters * pixelsPerMetre
        Me.imgHeight = heightInMeters * pixelsPerMetre

        'converts routes to points and saves to TrackAsPointsF
        Dim _TracksAsPointsF As List(Of TrackAsPointsF) = ConvertTracksGeoPointsToPointsF(_tracksAsGeoPoints) 'přepočítá trasy na body a uloží do TrackAsPointsF

        CreateVideoFromPointsF(_TracksAsPointsF)
    End Sub


    ''' <summary>
    ''' Creates a video from tracks represented as trkNode XML nodes.
    ''' </summary>
    ''' <param name="_tracksAsTrkNode">List of tracks containing trkNode elements.</param>
    Public Sub CreateVideoFromTrkNode(_tracksAsTrkNode As List(Of TrackAsTrkNode))
        TracksAsTrkNode = _tracksAsTrkNode
        Dim tracksAsTrkPts As New List(Of TrackAsTrkPts)
        For Each track In TracksAsTrkNode
            Dim trkptNodes As XmlNodeList = SelectTrkptNodes(track.TrkNode)
            Dim _TrackAsTrkPts As New TrackAsTrkPts With {
                .Label = track.Label,
                .Color = track.Color,
                .IsMoving = track.IsMoving,
                .TrackPoints = trkptNodes
            }
            tracksAsTrkPts.Add(_TrackAsTrkPts)
        Next
        CreateVideoFromTrkPts(tracksAsTrkPts)
    End Sub

    ''' <summary>
    ''' Creates a video from tracks represented as lists of trkpt XML nodes.
    ''' </summary>
    ''' <param name="_tracksAsTrkPts">List of tracks containing trkpt XML nodes.</param>
    Public Sub CreateVideoFromTrkPts(_tracksAsTrkPts As List(Of TrackAsTrkPts))
        TracksAsTrkPts = _tracksAsTrkPts
        Dim _tracksAsGeoPoints As List(Of TrackAsGeoPoints) = ConvertTracksTrkPtsToGeoPoints(_tracksAsTrkPts) 'přepočítá trasy na body a uloží do TrackAsGeoPoints
        CreateVideoFromGeoPoints(_tracksAsGeoPoints)
    End Sub


    ''' <summary>
    ''' Creates a video from tracks converted to 2D points with timestamps.
    ''' </summary>
    ''' <param name="_tracksAsPointsF">List of tracks containing 2D points and times.</param>
    Public Sub CreateVideoFromPointsF(_tracksAsPointsF As List(Of TrackAsPointsF))
        TracksAsPointsF = _tracksAsPointsF
        ' Vytvoř obrázky pro každý časový záznam
        ' Vygeneruj obrázky
        Dim startTime = PNGTimes.First()
        Dim endTime = PNGTimes.Last()
        Dim durationSeconds = (endTime - startTime).TotalSeconds
        Dim frameInterval = durationSeconds 'výchozí hodnota, skutečná hodnota bude nalezena v cyklu
        For i = 0 To PNGTimes.Count - 2
            'hledám minimální rozdíl kvůli maximální plynulosti
            frameInterval = Math.Max(0.1, frameInterval) 'minimální interval 0.1 sekundy
            frameInterval = Math.Min(frameInterval, (PNGTimes(i + 1) - PNGTimes(i)).TotalSeconds)
            Debug.WriteLine($"{i} {(PNGTimes(i + 1) - PNGTimes(i)).TotalSeconds}")
        Next
        frameInterval = Math.Max(minFrameInterval, frameInterval) 'minimální interval 3 sekundy, aby nebyl nulový nebo záporný a video nebylo moc velké

        Dim frameCount = CInt(Math.Ceiling(durationSeconds / frameInterval))
        Dim fps As Double = 1 / frameInterval 'video framerate

        createPNGs(pngDirectory, frameCount, frameInterval)

        Debug.WriteLine("Vygenerováno " & frameCount & " snímků.")
        Dim videoFilename = IO.Path.Combine(Me.directory.FullName, "overlay")

        CreateVideoWithFfmpeg(videoFilename, pngDirectory.FullName, fps)
    End Sub

    ''' <summary>
    ''' Creates PNG images for each frame of the video.
    ''' </summary>
    ''' <param name="pngDirectory">Directory where PNG images will be saved.</param>
    ''' <param name="framecount">Total number of frames to create.</param>
    ''' <param name="frameinterval">Time interval between frames in seconds.</param>
    Private Sub createPNGs(pngDirectory As IO.DirectoryInfo, framecount As Integer, frameinterval As Double)
        Dim diagonal As Single = Math.Sqrt(imgWidth ^ 2 + imgHeight ^ 2)
        Dim radius As Single = 0.02 * diagonal ' poloměr kruhu pro poslední bod, 2.5% šířky obrázku
        Dim penWidth As Single = 0.005 * diagonal ' šířka pera pro kreslení čar, 1% šířky obrázku
        Dim emSize As Single = 0.012 * diagonal '
        Dim arrowlength As Single = 0.06 * diagonal

        ' Předem vytvoř statickou bitmapu
        Dim staticBmp As New Bitmap(imgWidth, imgHeight, PixelFormat.Format32bppArgb)
        Dim font As New Font("Cascadia Code", emSize, FontStyle.Bold)

        Using g As Graphics = Graphics.FromImage(staticBmp)
            g.Clear(Color.Transparent)

            'first the direction of the wind:
            If WindDirection IsNot Nothing And WindDirection >= 0 And WindDirection <= 360 Then

                Dim center As New PointF(imgWidth / 2, imgHeight / 2) ' střed růžice
                Dim angle As Double = (WindDirection + 90) * Math.PI / 180 '' + 90 kvůli orientaci os, převod úhlu větru na radiány

                Dim endX As Single = center.X + arrowlength * Math.Cos(angle)
                Dim endY As Single = center.Y + arrowlength * Math.Sin(angle)

                Dim endPoint As New PointF(endX, endY)
                g.DrawLine(New Pen(Color.Orange, penWidth), center, endPoint)
                ' A pak stejně nakreslíš šipku

                '' Úhel křidélek šipky (např. 30°)
                Dim wingAngle As Double = Math.PI / 6

                ' Body křidélek
                Dim arrowSize As Single = 15 ' délka křidélka
                Dim x1 As Single = endPoint.X - arrowSize * Math.Cos(angle - wingAngle)
                Dim y1 As Single = endPoint.Y - arrowSize * Math.Sin(angle - wingAngle)

                Dim x2 As Single = endPoint.X - arrowSize * Math.Cos(angle + wingAngle)
                Dim y2 As Single = endPoint.Y - arrowSize * Math.Sin(angle + wingAngle)

                ' Nakresli křidélka
                g.DrawLine(New Pen(Color.Orange, penWidth / 2), endPoint, New PointF(x1, y1))
                g.DrawLine(New Pen(Color.Orange, penWidth / 2), endPoint, New PointF(x2, y2))

                'popis šipky
                ' Text k šipce
                Dim windText As String = Windspeed.ToString("0.0") & " m/s "
                Dim textSize = g.MeasureString(windText, Font)

                ' Chceme text nakreslit kousek za šipku
                Dim offset As Single = 10

                ' Bod, kde bude text - spočítáme ho jako bod za endPoint
                Dim textX As Single = center.X '+ offset * Math.Cos(angle)
                Dim textY As Single = center.Y - offset '* Math.Sin(angle)

                ' Uložíme transformaci
                Dim oldState = g.Save()

                ' Přesuneme se do bodu textu
                g.TranslateTransform(textX, textY)

                ' Otočíme souřadnicový systém podle směru šipky
                g.RotateTransform(CSng(angle * 180 / Math.PI))

                ' Text zarovnáme tak, aby byl středem na ose šipky
                Dim textPos As New PointF(0, -textSize.Height)

                ' Nakreslíme text (s outline, pokud chceš)
                Dim contrastColor As Color = GetContrastColor(Color.Black)
                DrawTextWithOutline(g, windText, font, Color.Black, contrastColor, textPos, 1)

                ' Vrátíme původní transformaci
                g.Restore(oldState)
            End If

            For Each track As TrackAsPointsF In Me.TracksAsPointsF
                If track.TrackPointsF.Count = 0 Then Continue For
                If Not track.IsMoving Then
                    Dim TrackPoints As List(Of PointF) = track.TrackPointsF.Select(Function(tp) tp.Location).ToList()
                    g.DrawLines(New Pen(track.Color, penWidth), TrackPoints.ToArray)
                    ' popis, poslední bod atd.
                    Dim time As String = track.TrackPointsF.Last.Time.ToString("HH:mm")

                    Dim popis As String = track.Label & " " & time
                    Dim textSize = g.MeasureString(popis, Font)
                    Dim contrastColor As Color = GetContrastColor(track.Color)
                    Dim p As PointF = TrackPoints.Last
                    g.FillEllipse(New SolidBrush(track.Color), p.X - radius / 2, p.Y - radius / 2, radius, radius)
                    Dim offsetX As Single
                    If p.X - textSize.Width - radius < 0 Then
                        ' není místo vlevo, napiš text vpravo od elipsy
                        offsetX = radius
                    Else
                        ' je místo, napiš text vlevo
                        offsetX = -textSize.Width - radius
                    End If
                    Dim textPos As New PointF(p.X + offsetX, p.Y - textSize.Height / 2)
                    DrawTextWithOutline(g, popis, font, track.Color, contrastColor, textPos, 2)

                    'DrawTextWithOutline(g, popis, font, track.Color, contrastColor, New PointF(p.X - textSize.Width - radius, p.Y - textSize.Height / 2), 2)
                End If
            Next
        End Using

        Dim frameIndex As Integer = 0
        Dim _dogTrail As New List(Of PointF)
        For i As Integer = 0 To framecount - 1
            Using bmp As New Bitmap(imgWidth, imgHeight, PixelFormat.Format32bppArgb)
                Using g As Graphics = Graphics.FromImage(bmp)
                    g.Clear(Color.Transparent)

                    ' Přidej předpřipravený statický podklad
                    g.DrawImage(staticBmp, Point.Empty)

                    For Each track As TrackAsPointsF In Me.TracksAsPointsF
                        If track.TrackPointsF.Count = 0 Then Continue For
                        If track.IsMoving Then
                            Dim frameTime = PNGTimes.First().AddSeconds(frameIndex * frameinterval)

                            Dim p As PointF = InterpolatedDogPosition(track, frameTime)
                            _dogTrail.Add(p)

                            If _dogTrail.Count > 1 Then g.DrawLines(New Pen(track.Color, penWidth), _dogTrail.ToArray)
                            g.FillEllipse(Brushes.Red, p.X - radius / 2, p.Y - radius / 2, radius, radius)
                            ' popis, poslední bod atd.

                            'Dim font As New Font("Cascadia Code", emSize, FontStyle.Bold)
                            Dim popis As String = frameTime.ToString("HH:mm:ss")
                            Dim textSize = g.MeasureString(popis, font)
                            Dim offsetX As Single
                            If p.X - textSize.Width - radius < 0 Then
                                ' není místo vlevo, napiš text vpravo od elipsy
                                offsetX = radius
                            Else
                                ' je místo, napiš text vlevo
                                offsetX = -textSize.Width - radius
                            End If
                            Dim textPos As New PointF(p.X + offsetX, p.Y - textSize.Height / 2)
                            Dim contrastColor As Color = GetContrastColor(track.Color)

                            'Dim textPos As New PointF(imgWidth - textSize.Width, 0)'alternativně vpravo nahoře
                            DrawTextWithOutline(g, popis, Font, track.Color, contrastColor, textPos, 2)
                        End If
                    Next
                End Using

                Dim filename = IO.Path.Combine(pngDirectory.FullName, $"frame_{frameIndex:D4}.png")
                bmp.Save(filename, ImageFormat.Png)
                If frameIndex = CInt(2 * framecount / 3) Then Me.bgPNG = New Bitmap(bmp)
                frameIndex += 1
            End Using
        Next
    End Sub


    Private Function InterpolatedDogPosition(track As TrackAsPointsF, frameTime As DateTime) As PointF

        Dim TrackPoints As List(Of PointF) = track.TrackPointsF.Select(Function(tp) tp.Location).ToList()
        Dim TrackTimes As List(Of DateTime) = track.TrackPointsF.Select(Function(tp) tp.Time).ToList()

        ' Pokud je frameTime před prvním časem, vrať první bod
        If frameTime <= TrackTimes.First() Then
            Return TrackPoints.First()
        End If

        ' Pokud je frameTime po posledním čase, vrať poslední bod
        If frameTime >= TrackTimes.Last() Then
            Return TrackPoints.Last()
        End If

        ' Najdi dva sousední body mezi kterými frameTime leží
        For i As Integer = 0 To TrackTimes.Count - 2
            Dim t1 = TrackTimes(i)
            Dim t2 = TrackTimes(i + 1)

            If frameTime >= t1 AndAlso frameTime <= t2 Then
                Dim p1 = TrackPoints(i)
                Dim p2 = TrackPoints(i + 1)

                Dim totalSeconds = (t2 - t1).TotalSeconds
                Dim elapsedSeconds = (frameTime - t1).TotalSeconds
                Dim ratio As Double = elapsedSeconds / totalSeconds

                ' Lineární interpolace
                Dim x As Single = CSng(p1.X + (p2.X - p1.X) * ratio)
                Dim y As Single = CSng(p1.Y + (p2.Y - p1.Y) * ratio)

                Return New PointF(x, y)
            End If
        Next

        ' Teoreticky by to sem nemělo dojít, ale fallback:
        Return TrackPoints.Last()
    End Function


    Private Sub DrawTextWithOutline(g As Graphics, text As String, font As Font, mainColor As Color, outlineColor As Color, pos As PointF, outlineSize As Integer)
        Using mainBrush As New SolidBrush(mainColor)
            Using outlineBrush As New SolidBrush(outlineColor)
                ' Kresli obrys – cyklem dokola podle outlineSize
                For dx As Integer = -outlineSize To outlineSize
                    For dy As Integer = -outlineSize To outlineSize
                        ' Kreslit jen body okolo, ne střed (jinak by byl outline moc silný)
                        If dx <> 0 OrElse dy <> 0 Then
                            g.DrawString(text, font, outlineBrush, pos.X + dx, pos.Y + dy)
                        End If
                    Next
                Next
                ' Nakonec hlavní text přesně na pozici
                g.DrawString(text, font, mainBrush, pos)
            End Using
        End Using
    End Sub




    Private Function ConvertTracksGeoPointsToPointsF(_tracksAsGeoPoints As List(Of TrackAsGeoPoints))

        Dim _tracksAsPointsF As New List(Of TrackAsPointsF)
        For Each Track In _tracksAsGeoPoints
            Dim _TrackAsPointsF As New TrackAsPointsF With {
                .Label = Track.Label,
                .Color = Track.Color,
                .IsMoving = Track.IsMoving,
                .TrackPointsF = New List(Of TrackPointF)
            }
            Dim createPNGTimes As Boolean = False 'pokud Track.IsMoving a Me.PNGTimes je Nothing, vytvoříme PNGTimes
            'PNGTimes se tvoří pouze z prvního pohyblivého tracku.
            If Track.IsMoving And Me.PNGTimes.Count = 0 Then createPNGTimes = True
            For Each geoPoint As TrackGeoPoint In Track.TrackGeoPoints
                Dim x = CSng(((geoPoint.Location.Lon - minLon)) * lonDistancePerDegree * pixelsPerMetre) 'pozice X osa, přepočítaná na pixely
                Dim y = CSng(((maxLat - geoPoint.Location.Lat)) * latDistancePerDegree * pixelsPerMetre) ' Y osa obrácená

                Dim _trackpointF As New TrackPointF With {
                        .Location = New PointF With {.X = x, .Y = y},
                        .Time = geoPoint.Time
                    }
                If createPNGTimes Then
                    'pokud Track.IsMoving a Me.PNGTimes je Nothing, vytvoříme PNGTimes
                    If Not Me.PNGTimes.Contains(geoPoint.Time) Then
                        Me.PNGTimes.Add(geoPoint.Time)
                    End If
                End If
                _TrackAsPointsF.TrackPointsF.Add(_trackpointF)
            Next
            _tracksAsPointsF.Add(_TrackAsPointsF)
        Next
        Return _tracksAsPointsF

    End Function



    Private Function ConvertTracksTrkPtsToGeoPoints(_tracksAsTrkPts As List(Of TrackAsTrkPts)) As List(Of TrackAsGeoPoints)
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





    Private Sub CreateVideoWithFfmpeg(outputFile As String, pngDir As String, framerate As Double)
        Dim psi As New ProcessStartInfo()
        psi.FileName = FindFfmpegPath()

        Dim inputPattern = System.IO.Path.Combine(pngDir, "frame_%04d.png")

        'vytvoříme video z obrázků s framerate = 1, to je asi 3x rychlejší než reálný čas, takže video bude 3x zrychlené a kratší
        'to proto, že shotcut neumí zpracovat video s nízkým framerate, takže vytvoříme nejdřív rychlé video a pak ho zpomalíme
        psi.Arguments = $"-y -framerate 1 -i ""{inputPattern}"" -c:v prores_ks -pix_fmt yuva444p10le ""{outputFile}_fast.mov"""

        psi.UseShellExecute = False
        psi.RedirectStandardOutput = False
        psi.RedirectStandardError = False
        psi.CreateNoWindow = False

        Using proc As Process = Process.Start(psi)
            proc.WaitForExit()
        End Using
        If Not System.IO.File.Exists(outputFile & "_fast.mov") Then
            RaiseEvent WarningOccurred("Failed to create video from images.", Color.Red)
            Return
        Else
            ''úklid:
            'IO.Directory.GetFiles(pngDir).ToList().ForEach(Sub(f) System.IO.File.Delete(f))
            'IO.Directory.Delete(pngDir, True) 'smazat adresář s PNG obrázky
        End If
        ' Vytvoř pomalou verzi videa (aby čas odpovídal reálné délce práce psa):

        Dim slowRate As String = (1 / framerate).ToString(System.Globalization.CultureInfo.InvariantCulture)


        Dim psi2 As New ProcessStartInfo

        psi2.FileName = psi.FileName
        psi2.RedirectStandardError = False
        psi2.UseShellExecute = False
        'psi2.Arguments = $"-i ""{outputFile}"" -c:v libvpx -filter:v ""setpts={slowRate}*PTS"" -an  ""{slowFile}"""
        'psi2.Arguments = $"-i ""{outputFile}"" -c:v libvpx -pix_fmt yuva420p -filter:v ""setpts={slowRate}*PTS"" -an  ""{slowFile}"""
        psi2.Arguments = $"-y -i ""{outputFile}_fast.mov"" -c:v prores_ks -pix_fmt yuva444p10le -filter:v ""setpts={slowRate}*PTS"" -r 1 ""{outputFile}.mov"""

        psi2.RedirectStandardOutput = False

        Using proc2 As Process = Process.Start(psi2)
            proc2.WaitForExit()
        End Using
        If Not System.IO.File.Exists(outputFile & ".mov") Then
            RaiseEvent WarningOccurred($"Failed to create video {outputFile}.mov.", Color.Red)
            Return
        Else

            Debug.WriteLine("Hotovo! Video vygenerováno.")
            RaiseEvent WarningOccurred($"Overlayvideo has been created and saved to {outputFile}.mov", Color.Green)
            Dim form As New frmVideoDone(outputFile & ".mov", Me.bgPNG)
            form.ShowDialog()
            If Me.bgPNG IsNot Nothing Then
                Me.bgPNG.Dispose()
                Me.bgPNG = Nothing
            End If
            'úklid:
            'IO.Directory.GetFiles(pngDir).ToList().ForEach(Sub(f) System.IO.File.Delete(f))
            Try
                IO.Directory.Delete(pngDir, True) 'smazat adresář s PNG obrázky
            Catch ex As Exception
                RaiseEvent WarningOccurred("Failed to delete PNG directory: " & ex.Message, Color.Red)
                'když selže smazání adresáře, např. když je otevřený v jiném programu, jede se dál
            End Try

            IO.File.Delete((outputFile & "_fast.mov"))
        End If
    End Sub

    Function GetContrastColor(bgColor As Color) As Color
        Dim luminance As Double = 0.299 * bgColor.R + 0.587 * bgColor.G + 0.114 * bgColor.B
        If luminance < 128 Then
            Return Color.White
        Else
            Return Color.Black
        End If
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


    Function SelectTrkptNodes(trkNode As XmlNode) As XmlNodeList
        Dim nsmgr As New XmlNamespaceManager(trkNode.OwnerDocument.NameTable)
        Dim ns As String = trkNode.GetNamespaceOfPrefix("") ' získá default namespace parent uzlu
        nsmgr.AddNamespace("gpx", ns)
        ' V GPX je to: trk > trkseg > trkpt
        Return trkNode.SelectNodes(".//gpx:trkpt", nsmgr)
    End Function

End Class


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






