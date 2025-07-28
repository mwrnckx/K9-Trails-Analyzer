Imports System.Drawing
Imports System.IO



Public Class FfmpegVideoEncoder

    Public Event WarningOccurred(message As String, _color As Color)

    Public Function EncodeFromPngs(FfmpegPath As String, outputDir As DirectoryInfo, outputFile As String, frameinterval As Double) As Task(Of Boolean)
        Dim psi As New ProcessStartInfo()
        psi.FileName = FfmpegPath
        Dim pngDir = outputDir.CreateSubdirectory("png")
        Dim inputPattern = System.IO.Path.Combine(pngDir.FullName, "frame_%04d.png")

        'vytvoříme video z obrázků s framerate = 1
        'psi.Arguments = $"-y -framerate 1 -i ""{inputPattern}"" -c:v prores_ks -pix_fmt yuva444p10le ""{outputFile}.mov"""
        psi.Arguments = $"-y -framerate 1 -i ""{inputPattern}"" -c:v libvpx -pix_fmt yuva420p -auto-alt-ref 0 -crf 25 -b:v 0 ""{outputFile}.webm"""
        psi.UseShellExecute = False
        psi.RedirectStandardOutput = False
        psi.RedirectStandardError = False
        psi.CreateNoWindow = True

        Using proc As Process = Process.Start(psi)
            proc.WaitForExit()
        End Using
        If Not System.IO.File.Exists(outputFile & ".webm") Then
            RaiseEvent WarningOccurred("Failed to create video from images.", Color.Red)
            Return Task.FromResult(False)
        End If

        Try
            'úklid:
            'IO.Directory.GetFiles(pngDir.FullName).ToList().ForEach(Sub(f) System.IO.File.Delete(f))
            pngDir.Delete(True) 'smazat adresář 
        Catch ex As Exception

        End Try

        'zrušeno!!!!
        'Dim strFrameInterval As String = (frameinterval).ToString(System.Globalization.CultureInfo.InvariantCulture)


        'Dim psi2 As New ProcessStartInfo

        'psi2.FileName = psi.FileName
        'psi2.RedirectStandardError = False
        'psi2.UseShellExecute = False
        'psi2.Arguments = $"-y -i ""{outputFile}_fast.mov"" -c:v prores_ks -pix_fmt yuva444p10le -filter:v ""setpts={strFrameInterval}*PTS"" -r 1 ""{outputFile}.mov"""

        'psi2.RedirectStandardOutput = False
        'psi2.CreateNoWindow = True

        'Using proc2 As Process = Process.Start(psi2)
        '    proc2.WaitForExit()
        'End Using
        If Not System.IO.File.Exists(outputFile & ".webm") Then
            RaiseEvent WarningOccurred($"Failed to create video {outputFile}.webm.", Color.Red)
            Return Task.FromResult(False)
        Else

            Debug.WriteLine("Hotovo! Video vygenerováno.")
            RaiseEvent WarningOccurred($"Overlayvideo has been created and saved to {outputFile}.webm", Color.Green)

            'úklid:
            Try
                pngDir.Delete(True) 'smazat adresář s PNG obrázky
            Catch ex As Exception
                RaiseEvent WarningOccurred("Failed to delete PNG directory: " & ex.Message, Color.Red)
                'když selže smazání adresáře, např. když je otevřený v jiném programu, jede se dál
            End Try
            'Try
            '    IO.File.Delete((outputFile & "_fast.mov"))
            'Catch ex As Exception
            '    RaiseEvent WarningOccurred("Failed to delete temporaly video: " & ex.Message, Color.Red)
            '    'když selže smazání adresáře, např. když je otevřený v jiném programu, jede se dál

            'End Try
            Return Task.FromResult(True)
        End If
    End Function

End Class


