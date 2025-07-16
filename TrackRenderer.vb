
Imports System.Drawing.Imaging
Imports System.Drawing.Text
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.Taskbar

Public Class PngSequenceCreator
    Private renderer As PngRenderer
    Dim PNGTimes As New List(Of DateTime) ' Časové značky pro PNG obrázky, pokud nejsou, vytvoří se z GPS bodů psa)
    Public frameInterval As Double
    'Public bgPNG As Bitmap

    Public Sub New(renderer As PngRenderer)
        Me.renderer = renderer
    End Sub

    Public Sub CreateFrames(tracks As List(Of TrackAsPointsF), staticBgTransparent As Bitmap, staticbgMap As Bitmap, pngDir As DirectoryInfo, pngTimes As List(Of DateTime), textParts As List(Of (Text As String, Color As Color, FontStyle As FontStyle)))
        Const minFrameInterval As Double = 3.0 'minimální interval mezi snímky v sekundách, aby video nebylo moc velké a rychlé, defaultně 3 sekundy
        Try
            'úklidíme staré PNG obrázky, pokud existují
            IO.Directory.GetFiles(pngDir.FullName).ToList().ForEach(Sub(f) System.IO.File.Delete(f))
        Catch ex As Exception

        End Try
        Dim startTime = pngTimes.First()
        Dim endTime = pngTimes.Last()
        Dim durationSeconds = (endTime - startTime).TotalSeconds
        frameInterval = durationSeconds 'výchozí hodnota, skutečná hodnota bude nalezena v cyklu
        For i = 0 To pngTimes.Count - 2
            'hledám minimální rozdíl kvůli maximální plynulosti
            frameInterval = Math.Max(0.1, frameInterval) 'minimální interval 0.1 sekundy
            frameInterval = Math.Min(frameInterval, (pngTimes(i + 1) - pngTimes(i)).TotalSeconds)
            Debug.WriteLine($"{i} {(pngTimes(i + 1) - pngTimes(i)).TotalSeconds}")
        Next
        frameInterval = Math.Max(minFrameInterval, frameInterval) 'minimální interval 3 sekundy, aby nebyl nulový nebo záporný a video nebylo moc velké

        Dim frameCount = CInt(Math.Ceiling(durationSeconds / frameInterval)) 'počet dynamických snímků

        Dim initialFrames As Integer = 0
        ' Vytvoříme statický obrázek s textem
        If textParts IsNot Nothing Then
            Dim staticTextbmp = renderer.RenderStaticText(textParts, staticbgMap)
            For frameindex As Integer = 0 To 1
                Dim filename = IO.Path.Combine(pngDir.FullName, $"frame_{frameindex:D4}.png")
                staticTextbmp.Save(filename, ImageFormat.Png)
                initialFrames += 1
            Next
        End If

        ' Vytvoříme statický obrázek s mapou 
        If staticbgMap IsNot Nothing Then
            For frameindex As Integer = 2 To 3
                Dim filename = IO.Path.Combine(pngDir.FullName, $"frame_{frameindex:D4}.png")
                staticbgMap.Save(filename, ImageFormat.Png)
                initialFrames += 1
            Next
        End If
        Dim _dogTrail As New List(Of PointF)
        For frameindex As Integer = 0 To frameCount - 1
            Dim frameTime = pngTimes.First().AddSeconds((frameindex) * frameInterval)
            Dim frame = renderer.RenderFrame(tracks, staticBgTransparent, frameTime, _dogTrail)
            Dim frameNumber As String = (frameindex + initialFrames).ToString("D4")
            Dim filename = IO.Path.Combine(pngDir.FullName, $"frame_{frameNumber}.png")
            'frame.Save(filename, Imaging.ImageFormat.Png)
            frame.Save(filename, ImageFormat.Png)
            'uloží pro další použití jako pozadí
            Dim bgPNGFileName = IO.Path.Combine(pngDir.Parent.FullName, "background.png")
            If frameindex = CInt(2 * frameCount / 3) Then frame.Save(bgPNGFileName, ImageFormat.Png)
            frame.Dispose()
        Next
    End Sub

    Public Function GetPngTimes(tracks As List(Of TrackAsPointsF)) As List(Of DateTime)
        For Each Track In tracks
            If Track.TrackPointsF.Count = 0 Then Continue For
            'PNGTimes se tvoří pouze z prvního pohyblivého tracku.
            If Track.IsMoving And Me.PNGTimes.Count = 0 Then
                ' Pokud je track pohyblivý, použijeme jeho body pro časy
                ' Předpokládáme, že body jsou seřazeny podle času
                Return Track.TrackPointsF.Select(Function(tp) tp.Time).ToList()
            End If
        Next

        Return New List(Of DateTime)
    End Function
End Class

Public Class PngRenderer
    Private minVideoSize As Single
    Private windDirection As Double?
    Private windSpeed As Double
    Private trackBounds As RectangleF

    Dim diagonal As Single
    Dim radius As Single  ' poloměr kruhu pro poslední bod, 2.5% šířky obrázku
    Dim penWidth As Single ' šířka pera pro kreslení čar, 1% šířky obrázku
    Dim emSize As Single  '
    Dim font As Font


    Public Sub New(windDirection As Double?, windSpeed As Double?, bgTiles As (bgmap As Bitmap, minTileX As Single, minTileY As Single))
        Me.minVideoSize = minVideoSize
        Me.windDirection = windDirection
        Me.windSpeed = windSpeed
        Me.trackBounds = bgTiles.bgmap.GetBounds(GraphicsUnit.Pixel) 'přepočítá obdélník na souřadnice v pixelech
        diagonal = Math.Sqrt(Me.trackBounds.Width ^ 2 + Me.trackBounds.Height ^ 2)
        Me.radius = 0.02 * diagonal ' poloměr kruhu pro poslední bod, 2.5% šířky obrázku
        Me.penWidth = 0.005 * diagonal ' šířka pera pro kreslení čar, 1% šířky obrázku
        Me.emSize = 0.012 * diagonal '
        Me.font = New Font("Cascadia Code", emSize, FontStyle.Bold)
    End Sub



    Public Function RenderStaticMapBackground(tracksAsPointsF As List(Of TrackAsPointsF), backgroundTiles As (bgmap As Bitmap, minTileX As Single, minTileY As Single)) As Bitmap
        ' Vykresli statické stopy
        ' Vrátí bitmapu s podkladem

        Dim backgroundMap = New Bitmap(backgroundTiles.bgmap)

        Using g As Graphics = Graphics.FromImage(backgroundMap)
            For Each track In tracksAsPointsF
                Dim TrackPoints As List(Of PointF) = track.TrackPointsF.Select(Function(tp) tp.Location).ToList()
                g.DrawLines(New Pen(track.Color, penWidth), TrackPoints.ToArray)
                ' popis, poslední bod atd.
                Dim time As String = track.TrackPointsF.Last.Time.ToString("HH:mm")

                Dim popis As String = track.Label & " " & time
                Dim textSize = g.MeasureString(popis, font)
                Dim contrastColor As Color = GetContrastColor(track.Color)
                Dim p As PointF = TrackPoints.Last  ' poslední bod, posunutý o offset
                g.FillEllipse(New SolidBrush(track.Color), p.X - radius / 2, p.Y - radius / 2, radius, radius)
            Next
        End Using

        Return backgroundMap
    End Function

    Public Function RenderStaticTransparentBackground(tracksAsPointsF As List(Of TrackAsPointsF), backgroundTiles As (bgmap As Bitmap, minTileX As Single, minTileY As Single)) As Bitmap
        ' Vykresli statické stopy, šipku větru, popisky

        Dim arrowlength As Single = 0.06 * diagonal

        Dim staticBmp As New Bitmap(backgroundTiles.bgmap.Width, backgroundTiles.bgmap.Height, PixelFormat.Format32bppArgb)
        Using g As Graphics = Graphics.FromImage(staticBmp)
            g.Clear(Color.Transparent)

            'first the direction of the wind:
            If windDirection IsNot Nothing And windDirection >= 0 And windDirection <= 360 Then

                Dim center As New PointF(backgroundTiles.bgmap.Width / 2, backgroundTiles.bgmap.Height / 2) ' střed růžice
                Dim angle As Double = (windDirection + 90) * Math.PI / 180 '' + 90 kvůli orientaci os, převod úhlu větru na radiány

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
                Dim windText As String = windSpeed.ToString("0.0") & " m/s "
                Dim textSize = g.MeasureString(windText, font)

                ' Chceme text nakreslit kousek za šipku
                Dim popisOffset As Single = 10

                ' Bod, kde bude text - spočítáme ho jako bod za endPoint
                Dim textX As Single = center.X '+ offset * Math.Cos(angle)
                Dim textY As Single = center.Y - popisOffset '* Math.Sin(angle)

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

            For Each track As TrackAsPointsF In tracksAsPointsF
                If track.TrackPointsF.Count = 0 Then Continue For
                If Not track.IsMoving Then
                    Dim TrackPoints As List(Of PointF) = track.TrackPointsF.Select(Function(tp) tp.Location).ToList()
                    g.DrawLines(New Pen(track.Color, penWidth), TrackPoints.ToArray)
                    ' popis, poslední bod atd.
                    Dim time As String = track.TrackPointsF.Last.Time.ToString("HH:mm")

                    Dim popis As String = track.Label & " " & time
                    Dim textSize = g.MeasureString(popis, font)
                    Dim contrastColor As Color = GetContrastColor(track.Color)
                    Dim p As PointF = TrackPoints.Last  ' poslední bod, posunutý o offset
                    g.FillEllipse(New SolidBrush(track.Color), p.X - radius / 2, p.Y - radius / 2, radius, radius)
                    Dim textoffsetX As Single
                    If p.X - textSize.Width - radius < 0 Then
                        ' není místo vlevo, napiš text vpravo od elipsy
                        textoffsetX = radius
                    Else
                        ' je místo, napiš text vlevo
                        textoffsetX = -textSize.Width - radius
                    End If
                    Dim textPos As New PointF(p.X + textoffsetX, p.Y - textSize.Height / 2)
                    DrawTextWithOutline(g, popis, font, track.Color, contrastColor, textPos, 2)

                    'DrawTextWithOutline(g, popis, font, track.Color, contrastColor, New PointF(p.X - textSize.Width - radius, p.Y - textSize.Height / 2), 2)
                End If
            Next
        End Using

        Return staticBmp
    End Function

    Public Function RenderStaticText(textParts As List(Of (Text As String, Color As Color, FontStyle As FontStyle)), bgmap As Bitmap) As Bitmap
        Dim maxWidth As Single = bgmap.Width * 0.9 ' maximální šířka textu, 90% šířky obrázku
        Dim startX As Single = bgmap.Width * 0.05 ' začátek textu, 5% od levého okraje
        Dim startY As Single = bgmap.Height * 0.07 ' začátek textu, 5% od horního okraje
        Dim currentY As Single = startY
        Dim fontSize As Int32 = CInt(bgmap.Height * 0.05) ' výchozí velikost písma


        Dim staticText As New Bitmap(bgmap.Width, bgmap.Height, PixelFormat.Format32bppArgb)
        Using g As Graphics = Graphics.FromImage(staticText)
            g.Clear(Color.White)
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias ' Pro lepší kvalitu textu

            For Each part In textParts
                Using mainFont As New Font("Cascadia Code", fontSize, part.FontStyle)
                    Using textBrush As New SolidBrush(part.Color)
                        ' Oblast, do které se má text vykreslit a zalamovat
                        Dim drawingArea As New RectangleF(startX, startY, maxWidth, 2000) ' Výšku dej dostatečně velkou

                        ' Zavolání naší nové super-funkce!
                        startY = DrawWrappedTextWithEmoji(g, part.Text, mainFont, textBrush, drawingArea)
                    End Using
                End Using

            Next
        End Using
        Return staticText
    End Function



    ''' <summary>
    ''' Vykreslí textový řetězec obsahující českou diakritiku a emoji
    ''' se správným zalamováním slov uvnitř daného obdélníku.
    ''' </summary>
    ''' <param name="g">Grafický kontext (Graphics).</param>
    ''' <param name="text">Text k vykreslení.</param>
    ''' <param name="baseFont">Základní font pro text (např. Segoe UI).</param>
    ''' <param name="brush">Štětec pro barvu textu.</param>
    ''' <param name="layoutRect">Obdélník definující oblast pro kreslení a zalamování.</param>
    Public Function DrawWrappedTextWithEmoji(ByVal g As Graphics, ByVal text As String, ByVal baseFont As Font, ByVal brush As Brush, ByVal layoutRect As RectangleF) As Single
        ' Nastavení pro kvalitnější vykreslování
        g.TextRenderingHint = TextRenderingHint.AntiAlias
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

        ' Font, který použijeme specificky pro emoji
        Using emojiFont As New Font("Segoe UI Emoji", baseFont.Size, baseFont.Style)

            Dim currentPosition As New PointF(layoutRect.X, layoutRect.Y)
            Dim lineHeight As Single = baseFont.GetHeight(g)
            Dim spaceWidth As Single = g.MeasureString(" ", baseFont).Width

            ' Rozdělíme celý text na jednotlivá slova
            Dim words() As String = text.Split(" "c)

            For Each word As String In words
                If String.IsNullOrEmpty(word) Then Continue For

                ' --- Krok 1: Změření šířky aktuálního slova (které může obsahovat text i emoji) ---
                Dim wordWidth As Single = 0
                Dim i As Integer = 0
                While i < word.Length
                    ' Zjednodušená detekce emoji
                    Dim isEmoji As Boolean = (i + 1 < word.Length AndAlso Char.IsSurrogatePair(word, i)) _
                                         OrElse (UnicodeCategory.OtherSymbol = Char.GetUnicodeCategory(word, i))

                    Dim segmentFont As Font = If(isEmoji, emojiFont, baseFont)
                    Dim segmentCharCount As Integer = If(isEmoji AndAlso Char.IsSurrogatePair(word, i), 2, 1)
                    Dim segmentText As String = word.Substring(i, segmentCharCount)

                    wordWidth += g.MeasureString(segmentText, segmentFont).Width
                    i += segmentCharCount
                End While

                ' --- Krok 2: Kontrola zalamování ---
                ' Pokud by slovo přesáhlo šířku a nejsme na začátku řádku, zalomíme.
                If (currentPosition.X + wordWidth > layoutRect.Right) AndAlso (currentPosition.X > layoutRect.X) Then
                    currentPosition.X = layoutRect.X
                    currentPosition.Y += lineHeight
                End If

                ' --- Krok 3: Vykreslení slova po segmentech ---
                i = 0
                While i < word.Length
                    Dim isEmoji As Boolean = (i + 1 < word.Length AndAlso Char.IsSurrogatePair(word, i)) _
                                         OrElse (UnicodeCategory.OtherSymbol = Char.GetUnicodeCategory(word, i))

                    Dim segmentFont As Font = If(isEmoji, emojiFont, baseFont)
                    Dim segmentCharCount As Integer = If(isEmoji AndAlso Char.IsSurrogatePair(word, i), 2, 1)
                    Dim segmentText As String = word.Substring(i, segmentCharCount)

                    g.DrawString(segmentText, segmentFont, brush, currentPosition)

                    ' Posuneme X souřadnici pro další část slova
                    currentPosition.X += g.MeasureString(segmentText, segmentFont).Width * 0.6 ' menší mezera mezi segmenty, aby se to nelepilo
                    i += segmentCharCount
                End While

                ' Po vykreslení slova přidáme mezeru
                currentPosition.X += spaceWidth
            Next
            currentPosition.Y += lineHeight * 2 'odskočí práznou řádku, aby se další text nevykreslil přímo pod posledním slovem
            Return currentPosition.Y
        End Using
    End Function

    Function GetContrastColor(bgColor As Color) As Color
        Dim luminance As Double = 0.299 * bgColor.R + 0.587 * bgColor.G + 0.114 * bgColor.B
        If luminance < 128 Then
            Return Color.White
        Else
            Return Color.Black
        End If
    End Function

    Public Function RenderFrame(tracks As List(Of TrackAsPointsF), staticBackground As Bitmap, frameTime As DateTime, ByRef _dogTrail As List(Of PointF)) As Bitmap
        ' Vykresli aktuální snímek s pohybujícím se psem

        Dim bmp As New Bitmap(staticBackground.Width, staticBackground.Height, PixelFormat.Format32bppArgb)
        Using g As Graphics = Graphics.FromImage(bmp)
            g.Clear(Color.Transparent)

            ' Přidej předpřipravený statický podklad
            g.DrawImage(staticBackground, Point.Empty)

            For Each track As TrackAsPointsF In tracks
                If track.TrackPointsF.Count = 0 Then Continue For
                If track.IsMoving Then

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
                    DrawTextWithOutline(g, popis, font, track.Color, contrastColor, textPos, 2)
                End If
            Next
        End Using

        Return bmp


    End Function

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

End Class

