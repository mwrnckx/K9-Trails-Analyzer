Imports System.IO



Public Class FfmpegVideoEncoder

    Public Event WarningOccurred(message As String, _color As Color)

    Public Function EncodeFromPngs(pngDir As DirectoryInfo, outputFile As String, frameinterval As Double) As Task(Of Boolean)
        Dim psi As New ProcessStartInfo()
        psi.FileName = FindFfmpegPath()

        Dim inputPattern = System.IO.Path.Combine(pngDir.FullName, "frame_%04d.png")

        'vytvoříme video z obrázků s framerate = 1, to je asi 3x rychlejší než reálný čas, takže video bude 3x zrychlené a kratší
        'to proto, že shotcut neumí zpracovat video s nízkým framerate, takže vytvoříme nejdřív rychlé video a pak ho zpomalíme
        psi.Arguments = $"-y -framerate 1 -i ""{inputPattern}"" -c:v prores_ks -pix_fmt yuva444p10le ""{outputFile}_fast.mov"""

        psi.UseShellExecute = False
        psi.RedirectStandardOutput = False
        psi.RedirectStandardError = False
        psi.CreateNoWindow = True

        Using proc As Process = Process.Start(psi)
            proc.WaitForExit()
        End Using
        If Not System.IO.File.Exists(outputFile & "_fast.mov") Then
            RaiseEvent WarningOccurred("Failed to create video from images.", Color.Red)
            Return Task.FromResult(False)
        Else
            ''úklid:
            'IO.Directory.GetFiles(pngDir).ToList().ForEach(Sub(f) System.IO.File.Delete(f))
            'IO.Directory.Delete(pngDir, True) 'smazat adresář s PNG obrázky
        End If
        ' Vytvoř pomalou verzi videa (aby čas odpovídal reálné délce práce psa):
        Try
            IO.Directory.GetFiles(pngDir.FullName).ToList().ForEach(Sub(f) System.IO.File.Delete(f))
        Catch ex As Exception

        End Try


        Dim strFrameInterval As String = (frameinterval).ToString(System.Globalization.CultureInfo.InvariantCulture)


        Dim psi2 As New ProcessStartInfo

        psi2.FileName = psi.FileName
        psi2.RedirectStandardError = False
        psi2.UseShellExecute = False
        'psi2.Arguments = $"-i ""{outputFile}"" -c:v libvpx -filter:v ""setpts={slowRate}*PTS"" -an  ""{slowFile}"""
        'psi2.Arguments = $"-i ""{outputFile}"" -c:v libvpx -pix_fmt yuva420p -filter:v ""setpts={slowRate}*PTS"" -an  ""{slowFile}"""

        psi2.Arguments = $"-y -i ""{outputFile}_fast.mov"" -c:v prores_ks -pix_fmt yuva444p10le -filter:v ""setpts={strFrameInterval}*PTS"" -r 1 ""{outputFile}.mov"""


        psi2.RedirectStandardOutput = False
        psi2.CreateNoWindow = True

        Using proc2 As Process = Process.Start(psi2)
            proc2.WaitForExit()
        End Using
        If Not System.IO.File.Exists(outputFile & ".mov") Then
            RaiseEvent WarningOccurred($"Failed to create video {outputFile}.mov.", Color.Red)
            Return Task.FromResult(False)
        Else

            Debug.WriteLine("Hotovo! Video vygenerováno.")
            RaiseEvent WarningOccurred($"Overlayvideo has been created and saved to {outputFile}.mov", Color.Green)

            'úklid:
            'IO.Directory.GetFiles(pngDir).ToList().ForEach(Sub(f) System.IO.File.Delete(f))
            Try
                pngDir.Delete() 'smazat adresář s PNG obrázky
            Catch ex As Exception
                RaiseEvent WarningOccurred("Failed to delete PNG directory: " & ex.Message, Color.Red)
                'když selže smazání adresáře, např. když je otevřený v jiném programu, jede se dál
            End Try
            Try
                IO.File.Delete((outputFile & "_fast.mov"))
            Catch ex As Exception
                RaiseEvent WarningOccurred("Failed to delete temporaly video: " & ex.Message, Color.Red)
                'když selže smazání adresáře, např. když je otevřený v jiném programu, jede se dál

            End Try
            Return Task.FromResult(True)
        End If
    End Function

End Class


