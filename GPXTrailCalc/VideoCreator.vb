Imports System.Drawing.Imaging
Imports System.Xml
Imports System.Diagnostics
Imports System.Windows.Media.TextFormatting

Friend Class VideoCreator
    Private _gpxRecord As GPXRecord
    Private dogNodes As XmlNodeList
    Private layerNodes As XmlNodeList
    Private crossTracks As List(Of XmlNodeList)
    Private directory As String
    Private bgPNG As Bitmap
    Public Event WarningOccurred(message As String, _color As Color)

    Public Sub New(record As GPXRecord,
                   dogNodes As XmlNodeList,
                   layerNodes As XmlNodeList,
                   crossTracks As List(Of XmlNodeList))
        _gpxRecord = record
        Me.dogNodes = dogNodes
        Me.layerNodes = layerNodes
        Me.crossTracks = crossTracks
    End Sub

    Public Sub Main()

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
        For Each crossTrackNodes In crossTracks
            If crossTrackNodes IsNot Nothing AndAlso crossTrackNodes.Count > 0 Then
                Dim crossTrackPoints = GetTimesAndPoints(crossTrackNodes).Points
                Me.crossTrackPointsList.Add(crossTrackPoints)
            End If
        Next

        ' Vytvoř obrázky pro každý časový záznam
        ' Vygeneruj obrázky
        Dim startTime = dogTimes.First()
        Dim endTime = dogTimes.Last()
        Dim durationSeconds = (endTime - startTime).TotalSeconds
        Dim frameInterval = durationSeconds '4
        'frameInterval napevno na 4 sekundy kvůli snazšímu zpracování v Shotcutu: '
        'rychlost se v Shotcutu dá nastavit na 0.25,  
        'takže délka videa bude přesně odpovídat reálné době pohybu psa
        For i = 0 To dogTimes.Count - 2
            'hledám minimální rozdíl kvůli maximální plynulosti
            frameInterval = Math.Max(0.1, frameInterval) 'minimální interval 0.1 sekundy
            frameInterval = Math.Min(frameInterval, (dogTimes(i + 1) - dogTimes(i)).TotalSeconds)
            Debug.WriteLine($"{i} {(dogTimes(i + 1) - dogTimes(i)).TotalSeconds}")
        Next
        frameInterval = Math.Max(3, frameInterval) 'minimální interval 3 sekundy, aby nebyl nulový nebo záporný a video nebylo moc velké

        Dim frameCount = CInt(Math.Ceiling(durationSeconds / frameInterval))
        Dim fps As Double = 1 / frameInterval 'video framerate

        CreatePNGs(pngDirectory, frameCount, frameInterval)

        Debug.WriteLine("Hotovo! Vygenerováno " & frameCount & " snímků.")
        Dim videoFilename = System.IO.Path.Combine(Me.directory, "overlay")

        CreateVideoWithFfmpeg(videoFilename, pngDirectory, fps)

    End Sub

    Private Sub CreatePNGs(pngDirectory As String, framecount As Integer, frameinterval As Double)
        ' Vytvoř obrázky pro každý časový záznam

        Dim frameIndex As Integer = 0

        ' Kolik snímků: tolik, kolik máme GPS bodů psa
        Dim _dogTrail As New List(Of PointF) From {dogPoints(0)}
        For i As Integer = 0 To framecount - 1

            Using bmp As New Bitmap(imgWidth, imgHeight, PixelFormat.Format32bppArgb)
                Using g As Graphics = Graphics.FromImage(bmp)
                    g.Clear(Color.Transparent)

                    ' Nakresli trasu kladeče
                    If layerPoints.Count > 1 Then
                        g.DrawLines(New Pen(Color.Blue, 5), layerPoints.ToArray())
                    End If

                    ' Nakresli trasy křížení
                    For Each crossTrackPoints In crossTrackPointsList
                        If crossTrackPoints.Count > 1 Then
                            g.DrawLines(New Pen(Color.Green, 5), crossTrackPoints.ToArray())
                        End If
                    Next


                    'nakresli polohu kladeče:
                    Dim font As New Font("Cascadia Code", 12, FontStyle.Bold)
                    Dim popis As String = TrackTypes.TrailLayer ' "TrailLayer" 'tady bude popis kladeče, zatím "Layer"
                    Dim textSize = g.MeasureString(popis, font)
                    Dim radius As Single = 15 'in pixels
                    Dim p As PointF = layerPoints.Last
                    Dim _color As Color = Color.Blue 'tady bude barva kladeče, zatím modrá
                    Dim contrastColor As Color = GetContrastColor(_color)
                    g.FillEllipse(Brushes.Blue, p.X - radius / 2, p.Y - radius / 2, radius, radius)
                    'g.DrawString(popis, font, Brushes.White, p.X - textSize.Width - radius, p.Y - textSize.Height / 2)

                    DrawTextWithOutline(g, popis, font, _color, contrastColor, New PointF(p.X - textSize.Width - radius, p.Y - textSize.Height / 2), 2)

                    'dog
                    Dim frameTime = dogTimes.First().AddSeconds(frameIndex * frameinterval)
                    ' Nakresli trasu psa od startu do aktuálního bodu
                    p = InterpolatedDogPosition(frameTime)
                    _dogTrail.Add(p)
                    If i >= 1 Then
                        g.DrawLines(New Pen(Color.Red, 4), _dogTrail.ToArray)
                    End If

                    ' Nakresli aktuální bod psa (červený)

                    radius = 15
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

    ''' <summary>
    ''' Nakreslí text s obrysem (stínem) okolo.
    ''' </summary>
    ''' <param name="g">Graphics objekt, na který se kreslí.</param>
    ''' <param name="text">Text k vykreslení.</param>
    ''' <param name="font">Font textu.</param>
    ''' <param name="mainColor">Hlavní barva textu.</param>
    ''' <param name="outlineColor">Barva obrysu.</param>
    ''' <param name="pos">Pozice (levý horní roh) textu.</param>
    ''' <param name="outlineSize">Velikost obrysu v pixelech.</param>
    Public Sub DrawTextWithOutline(g As Graphics, text As String, font As Font, mainColor As Color, outlineColor As Color, pos As PointF, outlineSize As Integer)
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


            Dim x = CSng((lon - minLon) * lonDistancePerDegree * scale)
            Dim y = CSng((maxLat - lat) * latDistancePerDegree * scale) ' Y osa obrácená
            points.Add(New PointF(x, y))
        Next
        Return (times, points)
    End Function


    ' Nastav velikost obrázku
    Dim imgWidth As Integer = 600
    Dim imgHeight As Integer = 600
    Dim scale As Double '
    Dim dogTimes As New List(Of DateTime) ' Časové značky pro psy   
    Dim dogPoints As New List(Of PointF) ' Body pro psy
    Dim layerPoints As New List(Of PointF) ' Body pro kladeče
    Dim crossTrackPointsList As New List(Of List(Of PointF)) ' Body pro křížení 
    ReadOnly latDistancePerDegree As Double = 111_320.0 ' průměrně ~111,3 km na jeden stupeň latitude
    Dim lonDistancePerDegree As Double

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
        For Each crossTrackNodes In crossTracks
            ' Pokud jsou křížové body, přepočítej je na body
            If crossTrackNodes IsNot Nothing AndAlso crossTrackNodes.Count > 0 Then
                For Each node As XmlNode In crossTrackNodes
                    Dim lat = Double.Parse(node.Attributes("lat").Value, Globalization.CultureInfo.InvariantCulture)
                    Dim lon = Double.Parse(node.Attributes("lon").Value, Globalization.CultureInfo.InvariantCulture)
                    minLat = Math.Min(minLat, lat)
                    maxLat = Math.Max(maxLat, lat)
                    minLon = Math.Min(minLon, lon)
                    maxLon = Math.Max(maxLon, lon)
                Next
            End If
        Next

        'na každou stranu přidáme 5 % jako margins:
        maxLon += (maxLon - minLon) * 0.05
        minLon -= (maxLon - minLon) * 0.05
        maxLat += (maxLat - minLat) * 0.05
        minLat -= (maxLat - minLat) * 0.05


        ' Vypočítej šířku a výšku obrázku v metrech
        Dim centerLat As Double = (minLat + maxLat) / 2
        lonDistancePerDegree = Math.Cos(centerLat * Math.PI / 180) * latDistancePerDegree
        Dim widthInMeters As Double = (maxLon - minLon) * lonDistancePerDegree
        Dim heightInMeters As Double = (maxLat - minLat) * latDistancePerDegree
        ' Nastav velikost obrázku na základě rozsahu souřadnic
        ' Vypočítej šířku a výšku obrázku v pixelech, tak aby se vešly do zadané velikosti 600x600
        Dim maxImgWidth As Integer = 600
        Dim maxImgHeight As Integer = 600

        Dim scaleX As Double = maxImgWidth / widthInMeters
        Dim scaleY As Double = maxImgHeight / heightInMeters
        scale = Math.Min(scaleX, scaleY) 'in pixels per metre
        imgWidth = CInt(widthInMeters * scale) 'in pixels
        imgHeight = CInt(heightInMeters * scale)
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
            Debug.WriteLine("Hotovo! Video vygenerováno.")
            RaiseEvent WarningOccurred($"Overlayvideo has been created and saved to {outputFile}.mov", Color.Green)
            Dim form As New frmVideoDone(outputFile & ".mov", Me.bgPNG)
            form.ShowDialog()

        End If
    End Sub

    Public Function GetContrastColor(bgColor As Color) As Color
        Dim luminance As Double = 0.299 * bgColor.R + 0.587 * bgColor.G + 0.114 * bgColor.B
        If luminance < 128 Then
            Return Color.White
        Else
            Return Color.Black
        End If
    End Function


End Class


Public Class TrackData
    Public Property Label As String
    Public Property Points As List(Of PointF)
    Public Property Times As New List(Of DateTime) ' Časové značky pro psy   
    Public Property Color As Color
End Class

