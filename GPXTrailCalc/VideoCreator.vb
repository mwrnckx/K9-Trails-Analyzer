Imports System.Drawing.Imaging
Imports System.Xml
Imports System.Diagnostics

Friend Class VideoCreator
    Private _gpxRecord As GPXRecord
    Private dogNodes As XmlNodeList
    Private layerNodes As XmlNodeList
    Private crossTrackNodes As XmlNodeList
    Private directory As String
    Private bgPNG As Bitmap
    Public Event WarningOccurred(message As String, _color As Color)

    Public Sub Main(record As GPXRecord, dogNodes As XmlNodeList, layerNodes As XmlNodeList, crossTrackNodes As XmlNodeList)
        Me._gpxRecord = record
        Me.dogNodes = dogNodes
        Me.layerNodes = layerNodes
        Me.crossTrackNodes = crossTrackNodes
        ' Zjisti cestu k adresáři, kde je GPX soubor
        Dim gpxDir = System.IO.Path.GetDirectoryName(_gpxRecord.Reader.FilePath)
        ' Zjisti název souboru bez přípony
        Dim gpxName = System.IO.Path.GetFileNameWithoutExtension(_gpxRecord.Reader.FilePath)
        ' Sestav cestu k novému adresáři
        Me.directory = System.IO.Path.Combine(gpxDir, gpxName)
        ' Pokud adresář neexistuje, vytvoř ho
        If Not System.IO.Directory.Exists(Me.directory) Then
            System.IO.Directory.CreateDirectory(Me.directory)
        End If
        Dim pngDirectory = System.IO.Path.Combine(Me.directory, "png")
        ' Pokud adresář neexistuje, vytvoř ho
        If Not System.IO.Directory.Exists(pngDirectory) Then
            System.IO.Directory.CreateDirectory(pngDirectory)
        End If


        ' Kód, co generuje obrázky
        FindCoordinateAndImgRange()

        Dim result = GetTimesAndPoints(dogNodes)
        Me.dogTimes = result.Times
        Me.dogPoints = result.Points

        ' Přepočítej kladeče na body
        Me.layerPoints = GetTimesAndPoints(layerNodes).Points

        ' Pokud jsou křížové body, přepočítej je na body
        If crossTrackNodes IsNot Nothing Then
            Dim crossTrackResult = GetTimesAndPoints(crossTrackNodes)
            Me.layerPoints.AddRange(crossTrackResult.Points)
        End If
        ' Vytvoř obrázky pro každý časový záznam
        ' Vygeneruj obrázky
        Dim startTime = dogTimes.First()
        Dim endTime = dogTimes.Last()
        Dim durationSeconds = (endTime - startTime).TotalSeconds
        Dim frameInterval = 4 ' durationSeconds 
        'frameInterval napevno na 4 sekundy kvůli snazšímu zpracování v Shotcutu: '
        'rychlost se v Shotcutu dá nastavit na 0.25,  
        'takže délka videa bude přesně odpovídat reálné době pohybu psa
        'For i = 0 To dogTimes.Count - 2
        '    'hledá minimální rozdíl kvůli maximální plynulosti
        '    frameInterval = Math.Min(frameInterval, (dogTimes(i + 1) - dogTimes(i)).TotalSeconds)
        '    Debug.WriteLine($"{i} {(dogTimes(i + 1) - dogTimes(i)).TotalSeconds}")
        'Next
        Dim frameCount = CInt(Math.Ceiling(durationSeconds / frameInterval))
        Dim fps As Double = 1 / frameInterval 'video framerate
        'Dim frameIndex As Integer = 0

        '' Framerate 0.3 fps = 1 snímek za 3 vteřiny
        '' Kolik snímků: tolik, kolik máme GPS bodů psa, nebo podle délky videa
        'Dim _dogTrail As New List(Of PointF) From {dogPoints(0)}
        'For i As Integer = 0 To frameCount - 1

        '    Using bmp As New Bitmap(imgWidth, imgHeight, PixelFormat.Format32bppArgb)
        '        Using g As Graphics = Graphics.FromImage(bmp)
        '            g.Clear(Color.Transparent)

        '            ' Nakresli trasu kladeče
        '            If layerPoints.Count > 1 Then
        '                g.DrawLines(New Pen(Color.Blue, 5), layerPoints.ToArray())
        '            End If
        '            'dog

        '            Dim frameTime = startTime.AddSeconds(frameIndex * frameInterval)
        '            ' Nakresli trasu psa od startu do aktuálního bodu
        '            Dim p As PointF = InterpolatedDogPosition(frameTime)
        '            _dogTrail.Add(p)
        '            If i >= 1 Then
        '                g.DrawLines(New Pen(Color.Red, 4), _dogTrail.ToArray)
        '            End If

        '            ' Nakresli aktuální bod psa (červený)

        '            Dim radius As Single = 15
        '            g.FillEllipse(Brushes.Red, p.X - radius / 2, p.Y - radius / 2, radius, radius)
        '        End Using

        '        Dim filename = System.IO.Path.Combine(pngDirectory, $"frame_{frameIndex:D4}.png")
        '        bmp.Save(filename, ImageFormat.Png)

        '        frameIndex += 1
        '    End Using
        'Next
        CreatePNGs(pngDirectory, frameCount, frameInterval)

        Console.WriteLine("Hotovo! Vygenerováno " & frameCount & " snímků.")
        Dim videoFilename = System.IO.Path.Combine(Me.directory, "overlay")
        CreateVideoWithFfmpeg(videoFilename, pngDirectory, fps)

    End Sub

    Private Sub CreatePNGs(pngDirectory As String, framecount As Integer, frameinterval As Double)
        ' Vytvoř obrázky pro každý časový záznam
        ' Vygeneruj obrázky



        Dim frameIndex As Integer = 0

        ' Framerate 0.3 fps = 1 snímek za 3 vteřiny
        ' Kolik snímků: tolik, kolik máme GPS bodů psa, nebo podle délky videa
        Dim _dogTrail As New List(Of PointF) From {dogPoints(0)}
        For i As Integer = 0 To framecount - 1

            Using bmp As New Bitmap(imgWidth, imgHeight, PixelFormat.Format32bppArgb)
                Using g As Graphics = Graphics.FromImage(bmp)
                    g.Clear(Color.Transparent)

                    ' Nakresli trasu kladeče
                    If layerPoints.Count > 1 Then
                        g.DrawLines(New Pen(Color.Blue, 5), layerPoints.ToArray())
                    End If
                    'dog

                    Dim frameTime = dogTimes.First().AddSeconds(frameIndex * frameinterval)
                    ' Nakresli trasu psa od startu do aktuálního bodu
                    Dim p As PointF = InterpolatedDogPosition(frameTime)
                    _dogTrail.Add(p)
                    If i >= 1 Then
                        g.DrawLines(New Pen(Color.Red, 4), _dogTrail.ToArray)
                    End If

                    ' Nakresli aktuální bod psa (červený)

                    Dim radius As Single = 15
                    g.FillEllipse(Brushes.Red, p.X - radius / 2, p.Y - radius / 2, radius, radius)
                End Using

                Dim filename = System.IO.Path.Combine(pngDirectory, $"frame_{frameIndex:D4}.png")
                bmp.Save(filename, ImageFormat.Png)
                If frameIndex = CInt(framecount / 2) Then Me.bgPNG = New Bitmap(bmp) ' vytvoří novou instanci bitmapy
                frameIndex += 1
            End Using
        Next

    End Sub

    ''' <summary>
    ''' Vrátí interpolovanou pozici psa pro zadaný čas.
    ''' </summary>
    ''' <param name="frameTime">Čas, pro který hledáš polohu</param>
    ''' <returns>Interpolovaná pozice psa</returns>
    Private Function InterpolatedDogPosition(frameTime As DateTime) As PointF
        ' Pokud je frameTime před prvním časem, vrať první bod
        If frameTime <= dogTimes.First() Then
            Return dogPoints.First()
        End If

        ' Pokud je frameTime po posledním čase, vrať poslední bod
        If frameTime >= dogTimes.Last() Then
            Return dogPoints.Last()
        End If

        ' Najdi dva sousední body mezi kterými frameTime leží
        For i As Integer = 0 To dogTimes.Count - 2
            Dim t1 = dogTimes(i)
            Dim t2 = dogTimes(i + 1)

            If frameTime >= t1 AndAlso frameTime <= t2 Then
                Dim p1 = dogPoints(i)
                Dim p2 = dogPoints(i + 1)

                Dim totalSeconds = (t2 - t1).TotalSeconds
                Dim elapsedSeconds = (frameTime - t1).TotalSeconds
                Dim ratio As Double = elapsedSeconds / totalSeconds

                ' Lineární interpolace
                Dim x As Single = CSng(p1.X + (p2.X - p1.X) * ratio)
                Dim y As Single = CSng(p1.Y + (p2.Y - p1.Y) * ratio)

                Return New PointF(x, y)
            End If
        Next

        ' Teoreticky by to sem nemělo dojít
        Return dogPoints.Last()
    End Function


    Private Function GetTimesAndPoints(trkptnodes As XmlNodeList) As (Times As List(Of DateTime), Points As List(Of PointF))

        ' Získá časy pro psy z dogtrkpts
        Dim times As New List(Of DateTime)
        Dim points As New List(Of PointF)
        For Each trkpt As XmlNode In trkptnodes
            Dim lat = Double.Parse(trkpt.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
            Dim lon = Double.Parse(trkpt.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)
            Dim timetrkpt = _gpxRecord.Reader.SelectSingleChildNode("time", trkpt)
            Dim time As DateTime = DateTime.Parse(timetrkpt.InnerText, Nothing, Globalization.DateTimeStyles.AssumeUniversal)
            times.Add(time)


            Dim x = CSng((lon - minLon) * scale)
            Dim y = CSng((maxLat - lat) * scale) ' Y osa obrácená
            points.Add(New PointF(x, y))
        Next
        Return (times, points)
    End Function


    ' Nastav velikost obrázku
    Dim imgWidth As Integer = 800
    Dim imgHeight As Integer = 600
    Dim scale As Double = Me.imgWidth / (maxLon - minLon) ' Výchozí měřítko, bude přepočítáno v FindCoordinateAndImgRange
    Dim dogTimes As New List(Of DateTime) ' Časové značky pro psy   
    Dim dogPoints As New List(Of PointF) ' Body pro psy
    Dim layerPoints As New List(Of PointF) ' Body pro kladeče

    Dim minLat As Double = Double.MaxValue, maxLat As Double = Double.MinValue
    Dim minLon As Double = Double.MaxValue, maxLon As Double = Double.MinValue
    Private Sub FindCoordinateAndImgRange()
        ' Najdi rozsah souřadnic


        For Each node As XmlNode In layerNodes
            Dim lat = Double.Parse(node.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
            Dim lon = Double.Parse(node.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)
            minLat = Math.Min(minLat, lat)
            maxLat = Math.Max(maxLat, lat)
            minLon = Math.Min(minLon, lon)
            maxLon = Math.Max(maxLon, lon)
        Next

        For Each node As XmlNode In dogNodes
            Dim lat = Double.Parse(node.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
            Dim lon = Double.Parse(node.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)
            minLat = Math.Min(minLat, lat)
            maxLat = Math.Max(maxLat, lat)
            minLon = Math.Min(minLon, lon)
            maxLon = Math.Max(maxLon, lon)
        Next
        If crossTrackNodes IsNot Nothing Then
            For Each node As XmlNode In crossTrackNodes
                Dim lat = Double.Parse(node.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
                Dim lon = Double.Parse(node.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)
                minLat = Math.Min(minLat, lat)
                maxLat = Math.Max(maxLat, lat)
                minLon = Math.Min(minLon, lon)
                maxLon = Math.Max(maxLon, lon)
            Next
        End If
        Dim scaleX As Double = imgWidth / (maxLon - minLon)
        Dim scaleY As Double = imgHeight / (maxLat - minLat)
        scale = Math.Min(scaleX, scaleY)
        imgWidth = CInt((maxLon - minLon) * scale)
        imgHeight = CInt((maxLat - minLat) * scale)
    End Sub



    Private Sub CreateVideoWithFfmpeg(outputFile As String, pngDir As String, framerate As Double)
        Dim psi As New ProcessStartInfo()
        psi.FileName = FindFfmpegPath()

        Dim inputPattern = System.IO.Path.Combine(pngDir, "frame_%04d.png")

        'psi.Arguments = $"-y -framerate {framerate.ToString(System.Globalization.CultureInfo.InvariantCulture)} -i ""{inputPattern}"" -c:v libvpx -auto-alt-ref 0 ""{outputFile}"""
        'vytvoříme video z obrázků s framerate = 1, to je asi 3x rychlejší než reálný čas, takže video bude 3x zrychlené a kratší
        'to proto, že shotcut neumí zpracovat video s nízkým framerate, takže vytvoříme nejdřív rychlé video a pak ho zpomalíme
        'psi.Arguments = $"-y -framerate 1 -i ""{inputPattern}"" -c:v libvpx -auto-alt-ref 0 ""{outputFile}.webm"""
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
            'úklid:
            IO.Directory.GetFiles(pngDir).ToList().ForEach(Sub(f) System.IO.File.Delete(f))
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
            'úklid:
            System.IO.File.Delete((outputFile & "_fast.mov"))
            Console.WriteLine("Hotovo! Video vygenerováno.")
            RaiseEvent WarningOccurred($"Overlayvideo has been created and saved to {outputFile}.mov", Color.Green)
            Dim form As New frmVideoDone(outputFile & ".mov", Me.bgPNG)
            form.ShowDialog()

        End If
    End Sub


    Public Sub New()

    End Sub
End Class
