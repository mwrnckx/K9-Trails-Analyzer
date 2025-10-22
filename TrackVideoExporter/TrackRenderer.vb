
Imports System.DirectoryServices.ActiveDirectory
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Drawing.Text
Imports System.Globalization
Imports System.IO
Imports System.Reflection.Metadata
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
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
            'Dim textParts As New List(Of (Text As String, Color As Color, FontStyle As FontStyle))
            Dim trailReport = LocalisedReports(key)
            Dim staticTextbmp = renderer.RenderStaticText(trailReport.ToBasicList("Trail description"))
            Dim filename = IO.Path.Combine(outputDir.FullName, key & "-" & "TrailDescription.png")
            staticTextbmp.Save(filename, ImageFormat.Png)

            'hodnocení Competition Points
            Dim trailReportPoints As TrailReport = LocalisedReports(key)
            Dim staticTextbmpPoints = renderer.RenderStaticText(trailReportPoints.ToCompetitionList("Points"))
            Dim filenamePoints = IO.Path.Combine(outputDir.FullName, key & "-" & "Points.png")
            staticTextbmpPoints.Save(filenamePoints, ImageFormat.Png)
        Next key
        '



        ' Vytvoříme statický obrázek s mapou 
        If staticbgMap IsNot Nothing Then
            Dim filename = IO.Path.Combine(outputDir.FullName, "TracksOnMap.png")
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
    Private myWindArrow As Bitmap
    'Private emojiFonts As New PrivateFontCollection() 'hezčí emojis
    'Private emojiFont As Font
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
        Me.windDirection = windDirection
        Me.windSpeed = windSpeed
        Me.trackBounds = bgTiles.bgmap.GetBounds(GraphicsUnit.Pixel) 'přepočítá obdélník na souřadnice v pixelech
        diagonal = Math.Sqrt(Me.trackBounds.Width ^ 2 + Me.trackBounds.Height ^ 2)
        Me.radius = 0.02 * diagonal ' poloměr kruhu pro poslední bod, 2.5% šířky obrázku
        Me.penWidth = 0.005 * diagonal ' šířka pera pro kreslení čar, 1% šířky obrázku
        Me.emSize = 0.012 * diagonal '
        Me.font = New Font("Cascadia Code", emSize, FontStyle.Bold)
        '' Načti font z disku nebo z resource streamu
        'hezčí emoji, ale nefunguje to....
        'Dim emojiFontPath As String = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources\fonts\Twemoji.Mozilla.ttf")
        'emojiFonts.AddFontFile(emojiFontPath)

        '' Vytvoř instanci Fontu z privátní kolekce
        'Dim emojiFontFamily = emojiFonts.Families(0)
        'Me.emojiFont = New Font(emojiFontFamily, 14, FontStyle.Regular)
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
                Dim position As New PointF(backgroundTiles.bgmap.Width, 0) ' pravý horní roh růžice
                Dim scale As Single = 0.15
                DrawWindArrow(g, position, scale, myWindArrow)
                'DrawWindArrow(g, center, arrowlength, windDirection, windSpeed)
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
    ''' <param name="width">With of the new text bitmap.</param>
    ''' <param name="height">height of the new text bitmap.</param>
    ''' <returns>A <see cref="Bitmap"/> containing the rendered static text.</returns>
    Public Function RenderStaticText_old(trailReportList As List(Of StyledText), Optional width As Integer = 1920, Optional height As Integer = 1440) As Bitmap
        Dim maxWidth As Single = width * 0.9 ' maximální šířka textu, 90% šířky obrázku
        Dim startX As Single = width * 0.05 ' začátek textu, 5% od levého okraje
        Dim startY As Single = 0 ' začátek textu
        Dim currentY As Single = startY
        Dim fontSize As Int32 = CInt(height * 0.07) ' výchozí velikost písma

        Dim staticText As New Bitmap(width, height, PixelFormat.Format32bppArgb)
        Using g As Graphics = Graphics.FromImage(staticText)
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.SystemDefault ' Lepší kvalita textu

            Dim fits As Boolean = False
            Dim i As Integer = 0
            Do Until (fits And i < 100)
                i += 1
                g.Clear(Color.LightYellow)
                currentY = startY
                fits = True
                Dim lineHeight As Single

                For Each part In trailReportList
                    If part.Text IsNot Nothing AndAlso part.Text <> "" Then
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
    ''' <param name="speed">The wind speed in m/s.</param>
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
    ''' Creates a bitmap with the wind arrow widget drawn on it, including wind speed and direction.
    ''' The bitmap is saved as "windRose.png" in the specified output directory and stored in the myWindArrow field.
    ''' </summary>
    ''' <param name="outputDir">The directory where the wind arrow bitmap will be saved.</param>
    Public Sub CreateWindArrowBitmap(outputDir As DirectoryInfo)
        ' Size of the wind arrow widget
        Dim arrowSize As Single = 100
        ' Dynamic color of the arrow based on wind speed
        Dim Color As System.Drawing.Color = GetWindColor(windSpeed)

        ' Wind speed text
        Dim text As String = CDbl(windSpeed).ToString("0.0", CultureInfo.InvariantCulture) & " m/s"
        ' Base font for measuring text size
        Dim baseFont As New Font("Cascadia Code Semibold", 12, FontStyle.Bold)

        ' Temporary bitmap for measuring text size
        Dim tempBitmap As New Bitmap(1, 1)
        Dim tempGraphics As Graphics = Graphics.FromImage(tempBitmap)

        ' Measure base text size
        Dim baseSize As SizeF = tempGraphics.MeasureString(text, baseFont)

        ' Dynamic font scaling based on arrow size
        Dim scale As Single = arrowSize / baseSize.Width
        Dim fontSize As Single = Math.Max(8, Math.Min(40, baseFont.Size * scale))
        Dim font As New Font("Cascadia Code Semibold", fontSize, FontStyle.Bold)
        Dim textSize As SizeF = tempGraphics.MeasureString(text, font)

        tempGraphics.Dispose()
        tempBitmap.Dispose()

        ' Calculate bitmap dimensions
        Dim padding As Single = arrowSize * 0.2
        Dim totalWidth As Single = Math.Max(arrowSize, textSize.Width) + padding * 2
        Dim totalHeight As Single = arrowSize + textSize.Height + padding * 2

        ' Create the final bitmap
        Dim bmp As New Bitmap(CInt(totalWidth), CInt(totalHeight))
        Using g As Graphics = Graphics.FromImage(bmp)
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias

            ' Draw semi-transparent white rounded rectangle background
            Dim bgRect As New RectangleF(0, 0, totalWidth, totalHeight)
            Using bgBrush As New SolidBrush(System.Drawing.Color.FromArgb(75, System.Drawing.Color.White))
                Using path As New System.Drawing.Drawing2D.GraphicsPath()
                    Dim radius As Single = bgRect.Width / 4.0F
                    path.AddArc(bgRect.X, bgRect.Y, radius, radius, 180, 90)
                    path.AddArc(bgRect.X + bgRect.Width - radius, bgRect.Y, radius, radius, 270, 90)
                    path.AddArc(bgRect.X + bgRect.Width - radius, bgRect.Y + bgRect.Height - radius, radius, radius, 0, 90)
                    path.AddArc(bgRect.X, bgRect.Y + bgRect.Height - radius, radius, radius, 90, 90)
                    path.CloseFigure()
                    g.FillPath(bgBrush, path)
                End Using
            End Using

            ' Calculate arrow and text positions
            Dim arrowX As Single = totalWidth / 2
            Dim arrowY As Single = padding + arrowSize / 2
            Dim textX As Single = totalWidth / 2 - textSize.Width / 2
            Dim textY As Single = arrowY + arrowSize / 2 + padding

            ' Draw the wind arrow, rotated by 180 degrees to indicate wind direction
            Using arrowBrush As New SolidBrush(Color)
                Dim oldState = g.Save()
                g.TranslateTransform(arrowX, arrowY)
                g.RotateTransform(CSng(windDirection + 180))
                Dim arrowPoints() As PointF = {
                    New PointF(0, -arrowSize / 2.0F),
                    New PointF(-arrowSize / 3.0F, arrowSize / 2.0F),
                    New PointF(0, arrowSize / 3.0F),
                    New PointF(arrowSize / 3.0F, arrowSize / 2.0F),
                    New PointF(0, -arrowSize / 2.0F)
                }
                g.FillPolygon(arrowBrush, arrowPoints)
                g.DrawLines(New Pen(System.Drawing.Color.Black), arrowPoints)
                g.Restore(oldState)
            End Using

            ' Draw wind speed text below the arrow
            Using font
                Using brush As New SolidBrush(System.Drawing.Color.Black)
                    g.DrawString(text, font, brush, textX, textY)
                End Using
            End Using
        End Using
        ' Store the bitmap in the field and save to disk
        Me.myWindArrow = bmp
        Dim filename = IO.Path.Combine(outputDir.FullName, "windRose.png")
        Me.myWindArrow.Save(filename, ImageFormat.Png)
        SaveWindArrowOverlay(outputDir)
    End Sub

    ''' <summary>
    ''' Saves a transparent overlay bitmap with the wind arrow in the top-right corner.
    ''' The overlay is sized according to the specified width and height, and the wind arrow is scaled.
    ''' The resulting PNG is saved as "WindArrowOverlay.png" in the output directory.
    ''' </summary>
    ''' <param name="outputDir">The directory where the overlay PNG will be saved.</param>
    ''' <param name="width">The width of the overlay bitmap (default: 1920).</param>
    ''' <param name="height">The height of the overlay bitmap (default: 1440).</param>
    ''' <param name="scale">The scale of the wind arrow relative to the diagonal of the overlay (default: 0.12).</param>
    Public Sub SaveWindArrowOverlay(outputDir As DirectoryInfo, Optional width As Integer = 1920, Optional height As Integer = 1440, Optional scale As Single = 0.12)
        ' Create a transparent bitmap of the requested size
        Dim overlayBmp As New Bitmap(width, height, PixelFormat.Format32bppArgb)
        Using g As Graphics = Graphics.FromImage(overlayBmp)
            g.Clear(Color.Transparent)
            If myWindArrow IsNot Nothing Then
                ' Position: top-right corner
                Dim position As New PointF(width, 0)
                DrawWindArrow(g, position, scale, myWindArrow)
            End If
        End Using
        Dim filename = IO.Path.Combine(outputDir.FullName, "WindArrowOverlay.png")
        overlayBmp.Save(filename, ImageFormat.Png)
        overlayBmp.Dispose()
    End Sub


    ''' <summary>
    ''' Draws the pre-rendered wind arrow bitmap onto the main graphics context with a scaled size.
    ''' </summary>
    ''' <param name="g">The Graphics object to draw on.</param>
    ''' <param name="position">The top-right position of the wind arrow widget.</param>
    ''' <param name="windArrowBitmap">The pre-rendered bitmap of the wind arrow.</param>
    Public Sub DrawWindArrow(ByVal g As Graphics, ByVal position As PointF, scale As Single, ByVal windArrowBitmap As Bitmap)
        ' Získejte šířku grafického kontextu
        Dim graphicsDiagonal As Single = Math.Sqrt(g.VisibleClipBounds.Height ^ 2 + g.VisibleClipBounds.Width ^ 2)

        ' Vypočítejte cílovou diagonálu jako x % diagonály grafického kontextu
        Dim targetDiagonal As Single = graphicsDiagonal * scale


        ' Vypočítejte cílovou výšku s ohledem na zachování poměru stran
        Dim aspectRatio As Single = CSng(windArrowBitmap.Height) / CSng(windArrowBitmap.Width)
        ' Vypočítejte cílovou šířku a výšku z diagonály a aspect ratio
        Dim targetWidth As Single = targetDiagonal / Math.Sqrt(1 + Math.Pow(aspectRatio, 2))
        Dim targetHeight As Single = targetWidth * aspectRatio

        ' Vytvořte cílový obdélník pro vykreslení
        Dim targetRect As New RectangleF(position.X - targetWidth, position.Y, targetWidth, targetHeight)

        ' Vykreslete bitmapu do hlavního grafického kontextu s novou velikostí
        g.DrawImage(windArrowBitmap, targetRect)
    End Sub



    ''' <summary>
    ''' Draws text containing Czech diacritics and emoji (as PNG images), with word wrapping and vbCrLf handling.
    ''' Emoji images must be stored in "Emoji" subfolder (e.g. "1F463.png" for 👣).
    ''' </summary>
    Public Function DrawWrappedTextWithEmoji_old(ByVal g As Graphics, ByVal text As String, ByVal baseFont As Font, ByVal brush As Brush, ByVal layoutRect As RectangleF) As Single
        g.TextRenderingHint = TextRenderingHint.SystemDefault
        g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality

        Dim lineHeight As Single = baseFont.GetHeight(g)
        Dim currentPosition As New PointF(layoutRect.X, layoutRect.Y)
        currentPosition.Y += lineHeight * 0.5F
        Dim spaceWidth As Single = g.MeasureString(" ", baseFont).Width

        ' 🔹 Emoji regex (zachytí i vícesymbolové emoji)
        'Dim emojiRegex As New Regex("[\uD83C-\uDBFF\uDC00-\uDFFF]+")
        'Dim emojiRegex As New Regex("[\u2614\u2600-\u26FF\u2700-\u27BF\u2B00-\u2BFF\u2C60-\u2C7F\uD83C\uDC00-\uDFFF\uD83D\uDC00-\uDFFF\uD83E\uDC00-\uDFFF]")
        'Dim emojiRegex As New Regex("(?:\u2614|[\u2600-\u27BF\u2B00-\u2BFF\u2C60-\u2C7F]|[\uD83C-\uDBFF][\uDC00-\uDFFF])")
        'Dim emojiRegex As New Regex("(?:\u2614|[\u2300-\u23FF\u2600-\u27BF\u2B00-\u2BFF\u2C60-\u2C7F]|[\uD83C-\uDBFF][\uDC00-\uDFFF])")
        Dim emojiRegex As New Regex("(?:\u2614|[\u2300-\u23FF\u2600-\u27BF\u2B00-\u2BFF\u2C60-\u2C7F]|[\uD800-\uDBFF][\uDC00-\uDFFF](?:\u200D?[\uD800-\uDBFF][\uDC00-\uDFFF])*)")
        ' 🔹 Rozdělíme text podle ručních zalomení
        Dim lines() As String = text.Split(New String() {vbCrLf}, StringSplitOptions.None)

        For Each line As String In lines
            Dim words() As String = line.Split(" "c)
            currentPosition.X = layoutRect.X

            For Each word As String In words
                If String.IsNullOrEmpty(word) Then Continue For

                ' --- Měření šířky slova (včetně emoji obrázků) ---
                Dim wordWidth As Single = 0
                Dim matches = emojiRegex.Matches(word)
                Dim lastIndex As Integer = 0
                For Each m As Match In matches
                    ' text před emoji
                    Dim beforeText As String = word.Substring(lastIndex, m.Index - lastIndex)
                    wordWidth += g.MeasureString(beforeText, baseFont).Width

                    ' emoji "šířka" = výška řádku
                    wordWidth += lineHeight * 0.9F
                    lastIndex = m.Index + m.Length
                Next
                ' zbytek po posledním emoji
                If lastIndex < word.Length Then
                    wordWidth += g.MeasureString(word.Substring(lastIndex), baseFont).Width
                End If

                ' --- Zalamování ---
                If (currentPosition.X + wordWidth > layoutRect.Right) AndAlso (currentPosition.X > layoutRect.X) Then
                    currentPosition.X = layoutRect.X
                    currentPosition.Y += lineHeight
                End If

                ' --- Vykreslení ---
                lastIndex = 0
                For Each m As Match In matches
                    ' text před emoji
                    Dim beforeText As String = word.Substring(lastIndex, m.Index - lastIndex)
                    If beforeText.Length > 0 Then
                        g.DrawString(beforeText, baseFont, brush, currentPosition)
                        currentPosition.X += g.MeasureString(beforeText, baseFont).Width
                    End If

                    ' emoji
                    Dim emojiText As String = m.Value
                    Dim codepoint As Integer = Char.ConvertToUtf32(emojiText, 0)
                    Dim codeHex As String = codepoint.ToString("X4")
                    Dim imgPath As String = Path.Combine(Application.StartupPath, "Resources", "emoji", codeHex & ".png")

                    If File.Exists(imgPath) Then
                        Using emojiImg As Image = Image.FromFile(imgPath)
                            g.DrawImage(emojiImg, currentPosition.X, currentPosition.Y - lineHeight * 0.1F, lineHeight, lineHeight)
                        End Using
                    Else
                        ' fallback, pokud obrázek chybí
                        g.DrawString(emojiText, baseFont, Brushes.Gray, currentPosition)
                    End If

                    currentPosition.X += lineHeight * 0.9F
                    lastIndex = m.Index + m.Length
                Next

                ' zbytek textu po emoji
                If lastIndex < word.Length Then
                    Dim remainingText As String = word.Substring(lastIndex)
                    g.DrawString(remainingText, baseFont, brush, currentPosition)
                    currentPosition.X += g.MeasureString(remainingText, baseFont).Width
                End If

                currentPosition.X += spaceWidth
            Next

            ' 🔹 nová řádka
            currentPosition.Y += lineHeight
        Next

        Return currentPosition.Y
    End Function

    ' * Obálka pro měření (volá DrawWrappedTextWithEmoji s drawText=False)
    Private Function MeasureWrappedTextHeightWithEmoji(ByVal g As Graphics, ByVal text As String, ByVal baseFont As Font, ByVal layoutRect As RectangleF) As Single
        ' Volá hlavní funkci s fiktivním štětcem a drawText = False
        Using dummyBrush As New SolidBrush(Color.Transparent)
            Return DrawWrappedTextWithEmoji(g, text, baseFont, dummyBrush, layoutRect, False)
        End Using
    End Function

    ' ***************************************************************
    ' * NOVÁ FUNKCE PRO POUHÉ MĚŘENÍ VÝŠKY BEZ KRESLENÍ              *
    ' ***************************************************************
    Private Function MeasureStaticTextHeight(trailReportList As List(Of StyledText), fontSize As Int32, Optional width As Integer = 1920, Optional height As Integer = 1440) As Single
        Dim maxWidth As Single = width * 0.9 ' Stejná jako v RenderStaticText
        Dim startX As Single = width * 0.05
        Dim startY As Single = 0
        Dim currentY As Single = startY

        ' Vytvoříme dočasný Graphics objekt pouze pro měření
        ' (ideálně by se měla použít jiná metoda, ale toto je rychlá cesta
        ' k získání GDI+ metrik na existující bitmapě)
        Using tempBitmap As New Bitmap(1, 1)
            Using g As Graphics = Graphics.FromImage(tempBitmap)
                g.TextRenderingHint = Drawing.Text.TextRenderingHint.SystemDefault

                For Each part In trailReportList
                    If part.Text IsNot Nothing AndAlso part.Text <> "" Then
                        Using mainFont = New Font(part.Font.FontFamily, fontSize, part.Font.Style)

                            Dim drawingArea As New RectangleF(startX, currentY, maxWidth, 2000)

                            ' Voláme upravenou měřící funkci, která nekreslí (viz Krok 3)
                            currentY = MeasureWrappedTextHeightWithEmoji(g, part.Label & "  " & part.Text, mainFont, drawingArea)
                        End Using
                    End If
                Next
            End Using
        End Using

        Return currentY
    End Function

    Public Function RenderStaticText(trailReportList As List(Of StyledText), Optional width As Integer = 1920, Optional height As Integer = 1440) As Bitmap
        Dim maxWidth As Single = width * 0.9
        Dim startX As Single = width * 0.05
        Dim startY As Single = 0
        Dim currentY As Single = startY
        Dim fits As Boolean = False
        Dim i As Integer = 0
        Dim neededHeight As Single = Single.MaxValue ' Potřebná výška
        Dim fontSize As Int32 = CInt(height * 0.07) ' Výchozí velikost písma (začátek intervalu)

        Dim minFontSize As Int32 = CInt(height * 0.03)
        Dim maxFontSize As Int32 = fontSize ' Horní mez (začínáme s výchozí hodnotou)
        Dim bestFitFontSize As Int32 = minFontSize ' Nejlepší nalezená velikost, která se vešla

        ' * 1. FÁZE: ADAPTIVNÍ MĚŘENÍ A HLEDÁNÍ SPRÁVNÉHO FONTU *

        ' Nejprve rychle najdeme horní mez (maxFontSize), která se nevejde (pokud se už nevejde startovní font)
        Do While MeasureStaticTextHeight(trailReportList, maxFontSize, width, height) <= height
            maxFontSize = maxFontSize * 2 ' Zvětšujeme horní mez dokud se nevejde
            If maxFontSize > 500 Then Exit Do ' Ochrana
        Loop

        ' Nyní provedeme Binární hledání
        i = 0 ' Reset iterací
        Do While (maxFontSize - minFontSize > 1 AndAlso i < 100)
            i += 1
            fontSize = minFontSize + (maxFontSize - minFontSize) \ 2 ' Zkusíme font uprostřed intervalu

            neededHeight = MeasureStaticTextHeight(trailReportList, fontSize, width, height)

            If neededHeight <= height Then
                ' Vejde se: Zaznamenáme jako nejlepší fit a zkusíme větší (posuneme dolní mez)
                bestFitFontSize = fontSize
                minFontSize = fontSize
            Else
                ' Nevejde se: Font je příliš velký (posuneme horní mez)
                maxFontSize = fontSize
            End If
        Loop

        fontSize = bestFitFontSize ' Použijeme největší font, který se vešel (bestFitFontSize)


        ' * 2. FÁZE: VYKRESLENÍ (Pouze jednou se správnou fontSize) *
        Dim staticText As New Bitmap(width, height, PixelFormat.Format32bppArgb)
        Using g As Graphics = Graphics.FromImage(staticText)
            g.TextRenderingHint = Drawing.Text.TextRenderingHint.SystemDefault
            g.Clear(Color.LightYellow) ' Finální vymazání

            currentY = startY
            For Each part In trailReportList
                If part.Text IsNot Nothing AndAlso part.Text <> "" Then
                    Using mainFont = New Font(part.Font.FontFamily, fontSize, part.Font.Style)
                        Using textBrush As New SolidBrush(part.Color)
                            Dim drawingArea As New RectangleF(startX, currentY, maxWidth, 2000)
                            ' Voláme původní funkci, která KRESLÍ (viz Krok 3)
                            Dim label = If(part.Label IsNot Nothing OrElse part.Label <> "", part.Label & " ", "")
                            currentY = DrawWrappedTextWithEmoji(g, label & part.Text, mainFont, textBrush, drawingArea, True)
                        End Using
                    End Using
                End If
            Next
        End Using

        Return staticText
    End Function

    ' Přidáváme přepínač drawText, aby bylo možné jen měřit
    Public Function DrawWrappedTextWithEmoji(ByVal g As Graphics, ByVal text As String, ByVal baseFont As Font, ByVal brush As Brush, ByVal layoutRect As RectangleF, Optional drawText As Boolean = True) As Single
        ' ... (Všechny tvoje deklarace: lineHeight, currentPosition, spaceWidth, emojiRegex) ...
        g.TextRenderingHint = TextRenderingHint.SystemDefault
        g.SmoothingMode = Drawing2D.SmoothingMode.HighQuality

        Dim lineHeight As Single = baseFont.GetHeight(g)
        Dim currentPosition As New PointF(layoutRect.X, layoutRect.Y)
        currentPosition.Y += lineHeight * 0.5F
        Dim spaceWidth As Single = g.MeasureString(" ", baseFont).Width

        ' 🔹 Emoji regex (zachytí i vícesymbolové emoji)
        Dim emojiRegex As New Regex("(?:\u2614|[\u2300-\u23FF\u2600-\u27BF\u2B00-\u2BFF\u2C60-\u2C7F]|[\uD800-\uDBFF][\uDC00-\uDFFF](?:\u200D?[\uD800-\uDBFF][\uDC00-\uDFFF])*)")
        Dim lines() As String = text.Split(New String() {vbCrLf}, StringSplitOptions.None)

        For Each line As String In lines
            Dim words() As String = line.Split(" "c)
            currentPosition.X = layoutRect.X

            For Each word As String In words
                If String.IsNullOrEmpty(word) Then Continue For

                ' --- Měření šířky slova (VŽDY se provádí) ---
                Dim wordWidth As Single = 0
                Dim matches = emojiRegex.Matches(word)
                Dim lastIndex As Integer = 0
                For Each m As Match In matches
                    ' text před emoji
                    Dim beforeText As String = word.Substring(lastIndex, m.Index - lastIndex)
                    wordWidth += g.MeasureString(beforeText, baseFont).Width

                    ' emoji "šířka" = výška řádku
                    wordWidth += lineHeight * 0.9F
                    lastIndex = m.Index + m.Length
                Next
                ' zbytek po posledním emoji
                If lastIndex < word.Length Then
                    wordWidth += g.MeasureString(word.Substring(lastIndex), baseFont).Width
                End If

                ' --- Zalamování (VŽDY se provádí) ---
                If (currentPosition.X + wordWidth > layoutRect.Right) AndAlso (currentPosition.X > layoutRect.X) Then
                    currentPosition.X = layoutRect.X
                    currentPosition.Y += lineHeight
                End If

                ' --- Vykreslení (Pouze pokud drawText = True) ---
                If drawText Then ' ⬅️ Zde je rozdíl!
                    lastIndex = 0
                    For Each m As Match In matches
                        ' text před emoji
                        Dim beforeText As String = word.Substring(lastIndex, m.Index - lastIndex)
                        If beforeText.Length > 0 Then
                            g.DrawString(beforeText, baseFont, brush, currentPosition)
                            currentPosition.X += g.MeasureString(beforeText, baseFont).Width
                        End If

                        ' emoji
                        Dim emojiText As String = m.Value
                        Dim codepoint As Integer = Char.ConvertToUtf32(emojiText, 0)
                        Dim codeHex As String = codepoint.ToString("X4")
                        Dim imgPath As String = Path.Combine(Application.StartupPath, "Resources", "emoji", codeHex & ".png")

                        If File.Exists(imgPath) Then
                            Using emojiImg As Image = Image.FromFile(imgPath)
                                g.DrawImage(emojiImg, currentPosition.X, currentPosition.Y - lineHeight * 0.1F, lineHeight, lineHeight)
                            End Using
                        Else
                            ' fallback, pokud obrázek chybí
                            g.DrawString(emojiText, baseFont, Brushes.Gray, currentPosition)
                        End If

                        currentPosition.X += lineHeight * 0.9F
                        lastIndex = m.Index + m.Length
                    Next

                    ' zbytek textu po emoji
                    If lastIndex < word.Length Then
                        Dim remainingText As String = word.Substring(lastIndex)
                        g.DrawString(remainingText, baseFont, brush, currentPosition)
                        currentPosition.X += g.MeasureString(remainingText, baseFont).Width
                    End If
                Else
                    ' Pokud nekreslíme, jen posouváme X pozici na základě vypočtené wordWidth
                    currentPosition.X += wordWidth
                End If ' ⬅️ Konec If drawText

                currentPosition.X += spaceWidth
            Next

            ' 🔹 nová řádka (VŽDY se provádí)
            currentPosition.Y += lineHeight
        Next

        Return currentPosition.Y
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

