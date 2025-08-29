
Imports System.DirectoryServices.ActiveDirectory
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Drawing.Text
Imports System.Globalization
Imports System.IO
Imports System.Reflection.Metadata
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar

Public Class PngSequenceCreator
    Private renderer As PngRenderer
    Dim PNGTimes As New List(Of DateTime) ' Časové značky pro PNG obrázky, pokud nejsou, vytvoří se z GPS bodů psa)
    Public frameInterval As Double
    'Public bgPNG As Bitmap

    Public Sub New(renderer As PngRenderer)
        Me.renderer = renderer
    End Sub

    Public Sub CreateFrames(tracks As List(Of TrackAsPointsF), staticBgTransparent As Bitmap, staticbgMap As Bitmap, outputDir As DirectoryInfo, pngTimes As List(Of DateTime), LocalisedReports As Dictionary(Of String, TrailReport))
        'static pngs first:
        ' Vytvoříme statický obrázek s textem
        Dim keys = LocalisedReports.Keys.ToList()
        For Each key In keys
            Dim textParts As New List(Of (Text As String, Color As Color, FontStyle As FontStyle))
            Dim trailReport = LocalisedReports(key)
            Dim staticTextbmp = renderer.RenderStaticText(trailReport)
            Dim filename = IO.Path.Combine(outputDir.FullName, key & "-" & "TrailDescription.png")
            staticTextbmp.Save(filename, ImageFormat.Png)

        Next key


        ' Vytvoříme statický obrázek s mapou 
        If staticbgMap IsNot Nothing Then
            Dim filename = IO.Path.Combine(outputDir.FullName, "TrailsOnMap.png")
            staticbgMap.Save(filename, ImageFormat.Png)
        End If

        Const minFrameInterval As Double = 3.0 'minimální interval mezi snímky v sekundách, aby video nebylo moc velké a rychlé, defaultně 3 sekundy
        Dim pngDir = outputDir.CreateSubdirectory("png")
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
            frameInterval = Math.Max(1, frameInterval) 'minimální interval 1 sekunda
            frameInterval = Math.Min(frameInterval, (pngTimes(i + 1) - pngTimes(i)).TotalSeconds)
            Debug.WriteLine($"{i} {(pngTimes(i + 1) - pngTimes(i)).TotalSeconds}")
        Next
        frameInterval = Math.Max(minFrameInterval, frameInterval) 'minimální interval 3 sekundy, aby nebyl nulový nebo záporný a video nebylo moc velké
        frameInterval = 1 'pro testování, aby bylo video rychlé a krátké
        Dim initialFrames As Integer = 0 'nakonec nepoužito
        Dim _dogTrail As New List(Of PointF)
        Dim frameCount = CInt(Math.Ceiling(durationSeconds / frameInterval)) 'počet dynamických snímků
        For frameindex As Integer = 0 To frameCount - 1
            Dim frameTime = pngTimes.First().AddSeconds((frameindex) * frameInterval)
            Dim frame = renderer.RenderFrame(tracks, staticBgTransparent, frameTime, _dogTrail)
            Dim frameNumber As String = (frameindex + initialFrames).ToString("D4")
            Dim filename = IO.Path.Combine(pngDir.FullName, $"frame_{frameNumber}.png")
            frame.Save(filename, ImageFormat.Png)
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



''' <summary>
''' Provides functionality for rendering PNG images with track data, wind information, and text.
''' </summary>
Public Class PngRenderer
    'Private minVideoSize As Single
    Private windDirection As Double?
    Private windSpeed As Double?
    Private trackBounds As RectangleF

    Dim diagonal As Single
    Dim radius As Single ' poloměr kruhu pro poslední bod, 2.5% šířky obrázku
    Dim penWidth As Single ' šířka pera pro kreslení čar, 1% šířky obrázku
    Dim emSize As Single '
    Dim font As Font

    ''' <summary>
    ''' Initializes a new instance of the <see cref="PngRenderer"/> class.
    ''' </summary>
    ''' <param name="windDirection">The direction of the wind in degrees (0-360), or null if not available.</param>
    ''' <param name="windSpeed">The speed of the wind in m/s, or null if not available.</param>
    ''' <param name="bgTiles">A tuple containing the background bitmap and its minimum tile X and Y coordinates.</param>
    Public Sub New(windDirection As Double?, windSpeed As Double?, bgTiles As (bgmap As Bitmap, minTileX As Single, minTileY As Single))
        'Me.minVideoSize = minVideoSize
        Me.windDirection = windDirection
        Me.windSpeed = windSpeed
        Me.trackBounds = bgTiles.bgmap.GetBounds(GraphicsUnit.Pixel) 'přepočítá obdélník na souřadnice v pixelech
        diagonal = Math.Sqrt(Me.trackBounds.Width ^ 2 + Me.trackBounds.Height ^ 2)
        Me.radius = 0.02 * diagonal ' poloměr kruhu pro poslední bod, 2.5% šířky obrázku
        Me.penWidth = 0.005 * diagonal ' šířka pera pro kreslení čar, 1% šířky obrázku
        Me.emSize = 0.012 * diagonal '
        Me.font = New Font("Cascadia Code", emSize, FontStyle.Bold)
    End Sub


    '   ---
    '  ## Public Functions
    '  ---

    ''' <summary>
    ''' Renders a static map background with tracks.
    ''' </summary>
    ''' <param name="tracksAsPointsF">A list of tracks to be rendered as points.</param>
    ''' <param name="backgroundTiles">A tuple containing the background bitmap and its minimum tile X and Y coordinates.</param>
    ''' <returns>A <see cref="Bitmap"/> containing the rendered static map background.</returns>
    Public Function RenderStaticMapBackground(tracksAsPointsF As List(Of TrackAsPointsF), backgroundTiles As (bgmap As Bitmap, minTileX As Single, minTileY As Single), Optional waypointsAsPointsF As TrackAsPointsF = Nothing) As Bitmap
        ' Vykresli statické stopy
        ' Vrátí bitmapu s podkladem

        Dim backgroundMap = New Bitmap(backgroundTiles.bgmap)

        Using g As Graphics = Graphics.FromImage(backgroundMap)
            'first the direction of the wind:
            If windDirection IsNot Nothing And windDirection >= 0 And windDirection <= 360 Then
                Dim center As New PointF(backgroundTiles.bgmap.Width, 0) ' střed růžice
                Dim arrowlength As Single = 0.07 * diagonal
                DrawWindArrow(g, center, arrowlength, windDirection, windSpeed)
            End If
            For Each track In tracksAsPointsF
                Dim TrackPoints As List(Of PointF) = track.TrackPointsF.Select(Function(tp) tp.Location).ToList()
                g.DrawLines(New Pen(track.Color, penWidth), TrackPoints.ToArray)

                ' popis, poslední bod atd.
                Dim time As String = track.TrackPointsF.Last.Time.ToString("HH:mm")
                Dim p As PointF = TrackPoints.Last  ' poslední bod, posunutý o offset
                g.FillEllipse(New SolidBrush(track.Color), p.X - radius / 2, p.Y - radius / 2, radius, radius)

                Dim popis As String = track.Label
                Dim textSize = g.MeasureString(popis, font)
                Dim textoffsetX As Single
                Dim contrastColor As Color = GetContrastColor(track.Color)
                If p.X - textSize.Width - radius < 0 Then
                    ' není místo vlevo, napiš text vpravo od elipsy
                    textoffsetX = radius
                Else
                    ' je místo, napiš text vlevo
                    textoffsetX = -textSize.Width - radius
                End If
                Dim textPos As New PointF(p.X + textoffsetX, p.Y - textSize.Height / 2)
                DrawTextWithOutline(g, popis, font, track.Color, contrastColor, textPos, 2)

            Next

            If waypointsAsPointsF IsNot Nothing AndAlso waypointsAsPointsF.TrackPointsF.Count > 0 Then
                Dim brush As SolidBrush = New SolidBrush(waypointsAsPointsF.Color) ' plná barva pro statické stopy
                Dim TrackPoints As List(Of PointF) = waypointsAsPointsF.TrackPointsF.Select(Function(tp) tp.Location).ToList()
                For Each wpt In TrackPoints
                    Dim time As String = waypointsAsPointsF.TrackPointsF.Last.Time.ToString("HH:mm")
                    Dim contrastColor As Color = GetContrastColor(waypointsAsPointsF.Color)
                    g.FillEllipse(brush, wpt.X - radius / 2, wpt.Y - radius / 2, radius, radius)
                    Dim popis As String = waypointsAsPointsF.Label
                    Dim textSize = g.MeasureString(popis, font)
                    Dim textoffsetX As Single
                    If wpt.X - textSize.Width - radius < 0 Then
                        ' není místo vlevo, napiš text vpravo od elipsy
                        textoffsetX = radius
                    Else
                        ' je místo, napiš text vlevo
                        textoffsetX = -textSize.Width - radius
                    End If
                    Dim textPos As New PointF(wpt.X + textoffsetX, wpt.Y - textSize.Height / 2)
                    If Not waypointsAsPointsF.IsMoving Then DrawTextWithOutline(g, popis, font, waypointsAsPointsF.Color, contrastColor, textPos, 2)
                Next
            End If
        End Using

        Return backgroundMap
    End Function

    ''' <summary>
    ''' Renders static tracks, wind arrow, and labels onto a transparent background.
    ''' </summary>
    ''' <param name="tracksAsPointsF">A list of tracks to be rendered as points.</param>
    ''' <param name="backgroundTiles">A tuple containing the background bitmap and its minimum tile X and Y coordinates.
    ''' The dimensions of bgmap are used to determine the size of the resulting transparent bitmap.</param>
    ''' <returns>A <see cref="Bitmap"/> with a transparent background, containing the rendered tracks, wind arrow, and labels.</returns>
    Public Function RenderStaticTransparentBackground(tracksAsPointsF As List(Of TrackAsPointsF),
                                                      backgroundTiles As (bgmap As Bitmap, minTileX As Single, minTileY As Single),
                                                      Optional waypointsAsPointsF As TrackAsPointsF = Nothing) As Bitmap
        ' Vykresli statické stopy, šipku větru, popisky

        Dim staticBmp As New Bitmap(backgroundTiles.bgmap.Width, backgroundTiles.bgmap.Height, PixelFormat.Format32bppArgb)
        Using g As Graphics = Graphics.FromImage(staticBmp)
            g.Clear(Color.Transparent)

            'first the direction of the wind:
            If windDirection IsNot Nothing And windDirection >= 0 And windDirection <= 360 Then
                Dim center As New PointF(backgroundTiles.bgmap.Width, 0) ' střed růžice
                Dim arrowlength As Single = 0.07 * diagonal
                DrawWindArrow(g, center, arrowlength, windDirection, windSpeed)
            End If


            For Each track As TrackAsPointsF In tracksAsPointsF
                If track.TrackPointsF.Count = 0 Then Continue For
                Dim brush As SolidBrush
                If track.IsMoving Then
                    Dim semiTransparentColor As Color = Color.FromArgb(100, track.Color) ' 128 = 50% průhlednost
                    brush = New SolidBrush(semiTransparentColor)
                Else
                    brush = New SolidBrush(track.Color) ' plná barva pro statické stopy
                End If

                Dim TrackPoints As List(Of PointF) = track.TrackPointsF.Select(Function(tp) tp.Location).ToList()
                g.DrawLines(New Pen(brush, penWidth), TrackPoints.ToArray)
                ' popis, poslední bod atd.
                Dim time As String = track.TrackPointsF.Last.Time.ToString("HH:mm")
                Dim contrastColor As Color = GetContrastColor(track.Color)
                Dim p As PointF = TrackPoints.Last  ' poslední bod, posunutý o offset
                g.FillEllipse(brush, p.X - radius / 2, p.Y - radius / 2, radius, radius)

                Dim popis As String = track.Label & " " & time
                Dim textSize = g.MeasureString(popis, font)
                Dim textoffsetX As Single
                If p.X - textSize.Width - radius < 0 Then
                    ' není místo vlevo, napiš text vpravo od elipsy
                    textoffsetX = radius
                Else
                    ' je místo, napiš text vlevo
                    textoffsetX = -textSize.Width - radius
                End If
                Dim textPos As New PointF(p.X + textoffsetX, p.Y - textSize.Height / 2)
                If Not track.IsMoving Then DrawTextWithOutline(g, popis, font, track.Color, contrastColor, textPos, 2)

            Next

            If waypointsAsPointsF IsNot Nothing AndAlso waypointsAsPointsF.TrackPointsF.Count > 0 Then
                Dim brush As SolidBrush = New SolidBrush(waypointsAsPointsF.Color) ' plná barva pro statické stopy
                Dim TrackPoints As List(Of PointF) = waypointsAsPointsF.TrackPointsF.Select(Function(tp) tp.Location).ToList()
                For Each wpt In TrackPoints
                    Dim time As String = waypointsAsPointsF.TrackPointsF.Last.Time.ToString("HH:mm")
                    Dim contrastColor As Color = GetContrastColor(waypointsAsPointsF.Color)
                    g.FillEllipse(brush, wpt.X - radius / 2, wpt.Y - radius / 2, radius, radius)
                    Dim popis As String = waypointsAsPointsF.Label
                    Dim textSize = g.MeasureString(popis, font)
                    Dim textoffsetX As Single
                    If wpt.X - textSize.Width - radius < 0 Then
                        ' není místo vlevo, napiš text vpravo od elipsy
                        textoffsetX = radius
                    Else
                        ' je místo, napiš text vlevo
                        textoffsetX = -textSize.Width - radius
                    End If
                    Dim textPos As New PointF(wpt.X + textoffsetX, wpt.Y - textSize.Height / 2)
                    If Not waypointsAsPointsF.IsMoving Then DrawTextWithOutline(g, popis, font, waypointsAsPointsF.Color, contrastColor, textPos, 2)
                Next
            End If
        End Using

        Return staticBmp
    End Function

    ''' <summary>
    ''' Renders static text onto a new bitmap with a white background. The text will wrap and adjust font size to fit within the specified area.
    ''' </summary>
    '' <param name="width">With of the new text bitmap.</param>
    ''' <param name="height">height of the new text bitmap.</param>
    ''' <returns>A <see cref="Bitmap"/> containing the rendered static text.</returns>
    Public Function RenderStaticText(trailReport As TrailReport, Optional width As Integer = 1920, Optional height As Integer = 1440) As Bitmap
        Dim maxWidth As Single = width * 0.9 ' maximální šířka textu, 90% šířky obrázku
        Dim startX As Single = width * 0.05 ' začátek textu, 5% od levého okraje
        Dim startY As Single = 0 ' začátek textu
        Dim currentY As Single = startY
        Dim fontSize As Int32 = CInt(height * 0.05) ' výchozí velikost písma

        Dim staticText As New Bitmap(width, height, PixelFormat.Format32bppArgb)
        Using g As Graphics = Graphics.FromImage(staticText)
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.AntiAlias ' Lepší kvalita textu

            Dim fits As Boolean = False
            Dim i As Integer = 0
            Do Until (fits And i < 100)
                i += 1
                g.Clear(Color.White)
                currentY = startY
                fits = True
                Dim lineHeight As Single

                For Each part In trailReport.toList
                    Using mainFont = New Font(part.Font.FontFamily, fontSize, part.Font.Style)

                        Using textBrush As New SolidBrush(part.Color)
                            Dim drawingArea As New RectangleF(startX, currentY, maxWidth, 2000)
                            currentY = DrawWrappedTextWithEmoji(g, part.Label & "  " & part.Text, mainFont, textBrush, drawingArea)
                        End Using
                        lineHeight = mainFont.GetHeight(g)
                    End Using

                    If currentY + lineHeight > height Then
                        fits = False
                        fontSize = Math.Floor(fontSize * (height / (currentY + lineHeight))) - 1  ' Adaptivní zmenšení písma
                        Exit For ' Není třeba pokračovat, už víme že se nevejde
                    End If
                Next
            Loop
        End Using

        Return staticText
    End Function

    ''' <summary>
    ''' Renders a single frame of the animation, including a static background and a moving track (e.g., a dog's trail).
    ''' </summary>
    ''' <param name="tracks">A list of tracks to be rendered.</param>
    ''' <param name="staticBackground">The pre-rendered static background bitmap to draw upon.</param>
    ''' <param name="frameTime">The current time for which to render the frame.</param>
    ''' <param name="_dogTrail">A list of points representing the dog's trail, which will be updated by this method.</param>
    ''' <returns>A <see cref="Bitmap"/> representing the rendered frame.</returns>
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
                    Dim textSize = g.MeasureString(popis, font) 'todo: přidat doglabel
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



    ''' <summary>
    ''' Draws a simplified wind arrow with speed text and a semi-transparent background.
    ''' </summary>
    ''' <param name="g">The Graphics object to draw on.</param>
    ''' <param name="position">The top-right position of the wind arrow widget.</param>
    ''' <param name="arrowSize">The size of the wind arrow.</param>
    ''' <param name="color">The color of the wind arrow.</param>
    ''' <param name="windDirection">The direction from which the wind is blowing in degrees (0-360).</param>
    ''' <param name="windSpeed">The wind speed in m/s.</param>
    Private Sub DrawWindArrow(g As Graphics, position As PointF, arrowSize As Single, windDirection As Double, windSpeed As Double)
        ' Dynamická barva šipky
        ' Předpokládáme, že funkce GetWindColor je implementována
        Dim Color = GetWindColor(windSpeed)

        Dim text As String = CDbl(windSpeed).ToString("0.0", CultureInfo.InvariantCulture) & " m/s"

        Dim baseFont As New Font("Cascadia Code Semibold", 12, FontStyle.Bold)
        Dim baseSize As SizeF = g.MeasureString(text, baseFont)

        ' Dynamická velikost fontu podle šířky šipky
        Dim scale As Single = arrowSize / baseSize.Width
        Dim fontSize As Single = Math.Max(8, Math.Min(40, baseFont.Size * scale))
        Dim font As New Font("Cascadia Code Semibold", fontSize, FontStyle.Bold)
        Dim textSize As SizeF = g.MeasureString(text, font)

        ' Vypočítáme rozměry a polohu pozadí
        Dim padding As Single = arrowSize * 0.2 ' Zvýšený padding pro větší přesah
        Dim totalWidth As Single = Math.Max(arrowSize, textSize.Width) + padding * 2
        Dim totalHeight As Single = arrowSize + textSize.Height + padding * 2 ' Přidán prostor pro šipku
        Dim backgroundRect As New RectangleF(position.X - totalWidth, position.Y, totalWidth, totalHeight)

        ' Vykreslíme poloprůhledný bílý obdélník s kulatými rohy
        Using bgBrush As New SolidBrush(Color.FromArgb(75, Color.White))
            ' Použijeme GraphicsPath pro zakulacení rohů
            Using path As New System.Drawing.Drawing2D.GraphicsPath()
                Dim radius As Integer = backgroundRect.Width / 4.0F ' Poloměr zakulacení
                path.AddArc(backgroundRect.X, backgroundRect.Y, radius, radius, 180, 90)
                path.AddArc(backgroundRect.X + backgroundRect.Width - radius, backgroundRect.Y, radius, radius, 270, 90)
                path.AddArc(backgroundRect.X + backgroundRect.Width - radius, backgroundRect.Y + backgroundRect.Height - radius, radius, radius, 0, 90)
                path.AddArc(backgroundRect.X, backgroundRect.Y + backgroundRect.Height - radius, radius, radius, 90, 90)
                path.CloseFigure()
                g.FillPath(bgBrush, path)
            End Using
        End Using

        ' Vypočítáme pozice šipky a textu vzhledem k pozadí
        Dim arrowX As Single = backgroundRect.X + totalWidth / 2
        Dim arrowY As Single = backgroundRect.Y + padding + arrowSize / 2
        Dim textX As Single = backgroundRect.X + totalWidth / 2 - textSize.Width / 2
        Dim textY As Single = arrowY + arrowSize / 2 + padding

        ' Kreslení šipky s otočením o 180 stupňů
        Using arrowBrush As New SolidBrush(Color)
            Dim oldState = g.Save()
            g.TranslateTransform(arrowX, arrowY)
            g.RotateTransform(CSng(windDirection + 180)) ' Otočí o 180°

            ' Vykreslení těla šipky (trojúhelník)
            Dim arrowPoints() As PointF = {
            New PointF(0, -arrowSize * 2.0F / 3.0F),
            New PointF(-arrowSize / 3.0F, arrowSize / 3.0F),
            New PointF(0, 0),
            New PointF(arrowSize / 3.0F, arrowSize / 3.0F),
            New PointF(0, -arrowSize * 2.0F / 3.0F)
        }
            g.FillPolygon(arrowBrush, arrowPoints)
            g.DrawLines(New Pen(Color.Black), arrowPoints)
            g.Restore(oldState)
        End Using

        ' Vykreslení textu (rychlost větru)
        Using font
            Using brush As New SolidBrush(Color.Black)
                g.DrawString(text, font, brush, textX, textY)
            End Using
        End Using
    End Sub



    Private Function GetWindColor(speed As Double) As Color
        Select Case speed
            Case < 2
                Return Color.LightGreen
            Case < 3
                Return Color.Green
            Case < 4
                Return Color.Blue
            Case < 5
                Return Color.DarkBlue
            Case < 6
                Return Color.Orange
            Case < 7
                Return Color.DarkOrange
            Case Else
                Return Color.Red
        End Select
    End Function


    ''' <summary>
    ''' Draws a text string containing Czech diacritics and emojis, with proper word wrapping, within a given rectangle.
    ''' </summary>
    ''' <param name="g">The Graphics object to draw on.</param>
    ''' <param name="text">The text string to draw.</param>
    ''' <param name="baseFont">The base font for the text (e.g., Segoe UI).</param>
    ''' <param name="brush">The brush for the text color.</param>
    ''' <param name="layoutRect">The rectangle defining the drawing and wrapping area.</param>
    ''' <returns>The Y-coordinate of the current drawing position after rendering the text.</returns>
    Public Function DrawWrappedTextWithEmoji(ByVal g As Graphics, ByVal text As String, ByVal baseFont As Font, ByVal brush As Brush, ByVal layoutRect As RectangleF) As Single
        ' Nastavení pro kvalitnější vykreslování
        g.TextRenderingHint = TextRenderingHint.AntiAlias
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

        ' Font, který použijeme specificky pro emoji
        Using emojiFont As New Font("Segoe UI Emoji", baseFont.Size, baseFont.Style)
            Dim lineHeight As Single = baseFont.GetHeight(g)
            Dim currentPosition As New PointF(layoutRect.X, layoutRect.Y)
            currentPosition.Y += lineHeight * 1.5 'odskočí práznou řádku
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

            Return currentPosition.Y
        End Using
    End Function

    ''' <summary>
    ''' Determines a contrasting color (black or white) for a given background color.
    ''' </summary>
    ''' <param name="bgColor">The background color.</param>
    ''' <returns>Either <see cref="Color.White"/> or <see cref="Color.Black"/>, depending on which provides better contrast.</returns>
    Function GetContrastColor(bgColor As Color) As Color
        Dim luminance As Double = 0.299 * bgColor.R + 0.587 * bgColor.G + 0.114 * bgColor.B
        If luminance < 128 Then
            Return Color.White
        Else
            Return Color.Black
        End If
    End Function

    ''' <summary>
    ''' Interpolates the position of a track (e.g., a dog's position) at a specific time.
    ''' </summary>
    ''' <param name="track">The track containing location and time data.</param>
    ''' <param name="frameTime">The time for which to interpolate the position.</param>
    ''' <returns>The interpolated <see cref="PointF"/> position of the track at the given time.</returns>
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

    ''' <summary>
    ''' Draws a text string with an outline.
    ''' </summary>
    ''' <param name="g">The Graphics object to draw on.</param>
    ''' <param name="text">The text string to draw.</param>
    ''' <param name="font">The font to use for the text.</param>
    ''' <param name="mainColor">The main color of the text.</param>
    ''' <param name="outlineColor">The color of the text outline.</param>
    ''' <param name="pos">The position to draw the text.</param>
    ''' <param name="outlineSize">The size of the outline in pixels.</param>
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

