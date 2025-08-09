Imports System.Drawing
Imports System.Net.Http

Public Class OsmTileDownloader
    Private Const TileSize As Integer = 256


    ''' <summary>
    ''' Vytvoří bitmapu složenou z OSM dlaždic pro daný bounding box a zoom.
    ''' </summary>
    ''' <param name="minLat">Minimum latitude</param>
    ''' <param name="maxLat">Maximum latitude</param>
    ''' <param name="minLon">Minimum longitude</param>
    ''' <param name="maxLon">Maximum longitude</param>
    ''' <param name="zoom">Zoom level (např. 13-16)</param>
    ''' <returns>Bitmapa složená z dlaždic</returns>
    Public Async Function GetMapBitmap(minLat As Double, maxLat As Double, minLon As Double, maxLon As Double, zoom As Integer) As Task(Of (bgmap As Bitmap, minTileX As Single, minTileY As Single))
        ' Převod souřadnic na indexy dlaždic
        Dim xMin = LonToTileX(minLon, zoom)
        Dim xMax = LonToTileX(maxLon, zoom)
        Dim yMin = LatToTileY(maxLat, zoom) ' Pozor: maxLat => horní hrana
        Dim yMax = LatToTileY(minLat, zoom) ' minLat => dolní hrana

        Dim tilesX = xMax - xMin + 1
        Dim tilesY = yMax - yMin + 1

        Dim bmpWidth = tilesX * TileSize
        Dim bmpHeight = tilesY * TileSize

        Dim result As New Bitmap(bmpWidth, bmpHeight)

        Using g As Graphics = Graphics.FromImage(result)

            Dim client As New HttpClient()
            client.DefaultRequestHeaders.UserAgent.ParseAdd("K9TrailsAnalyzer/1.0 (mwrnckx@seznam.cz)")
            client.DefaultRequestHeaders.Referrer = New Uri("https://github.com/mwrnckx/")




            For y = yMin To yMax
                For x = xMin To xMax
                    Dim url = $"https://tile.openstreetmap.org/{zoom}/{x}/{y}.png"
                    Debug.WriteLine($"Stahuji: {url}")


                    Try
                        'Dim data = wc.DownloadData(url)
                        Dim data = Await client.GetByteArrayAsync(url)
                        'Dim fileName = IO.Path.GetFileName(url)
                        'IO.File.WriteAllBytes(fileName, data)

                        Await Task.Delay(500) ' pauza, aby tě neblokli

                        Using tileImage As New Bitmap(New IO.MemoryStream(data))
                            Dim offsetX = (x - xMin) * TileSize
                            Dim offsetY = (y - yMin) * TileSize
                            g.DrawImage(tileImage, offsetX, offsetY, TileSize, TileSize)
                        End Using
                    Catch ex As Exception
                        ' Když tile neexistuje nebo je problém, přeskočíme
                        Debug.WriteLine($"Error downloading a tile: {ex.Message}")
                    End Try
                Next
            Next
        End Using

        Return (result, xMin, yMin)
    End Function

    ' Převod longitude na index dlaždice X
    Private Function LonToTileX(lon As Double, zoom As Integer) As Integer
        Return CInt(Math.Floor((lon + 180) / 360 * (2 ^ zoom)))
    End Function

    ' Převod latitude na index dlaždice Y
    Private Function LatToTileY(lat As Double, zoom As Integer) As Integer
        Dim latRad = lat * Math.PI / 180
        Return CInt(Math.Floor((1 - Math.Log(Math.Tan(latRad) + 1 / Math.Cos(latRad)) / Math.PI) / 2 * (2 ^ zoom)))
    End Function
End Class

