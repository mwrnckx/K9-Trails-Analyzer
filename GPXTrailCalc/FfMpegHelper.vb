﻿Module FfMpegHelper
    Public Function FindFfmpegPath() As String
        ' 1. Zkontroluj, jestli je uložená cesta a soubor tam je
        If Not String.IsNullOrEmpty(My.Settings.FfmpegPath) AndAlso
       System.IO.File.Exists(My.Settings.FfmpegPath) Then
            Return My.Settings.FfmpegPath
        End If

        ' 2. Zkus typické cesty
        Dim commonPaths = New String() {
        "C:\Programy\ffmpeg\bin\ffmpeg.exe",
        "C:\Program Files\ffmpeg\bin\ffmpeg.exe",
        "C:\ffmpeg\bin\ffmpeg.exe",
         "C:\Programs\ffmpeg\bin\ffmpeg.exe"
    }

        For Each path In commonPaths
            If System.IO.File.Exists(path) Then
                My.Settings.FfmpegPath = path
                My.Settings.Save()
                Return path
            End If
        Next

        ' 3. Zkus najít v PATH
        Try
            Dim psi As New ProcessStartInfo("where", "ffmpeg")
            psi.RedirectStandardOutput = True
            psi.UseShellExecute = False
            psi.CreateNoWindow = True
            Using proc As Process = Process.Start(psi)
                proc.WaitForExit()
                Dim output As String = proc.StandardOutput.ReadToEnd()
                Dim lines = output.Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
                If lines.Length > 0 AndAlso System.IO.File.Exists(lines(0)) Then
                    My.Settings.FfmpegPath = lines(0)
                    My.Settings.Save()
                    Return lines(0)
                End If
            End Using
        Catch
            ' ignore
        End Try

        ' 4. Pokud nenajde, zeptej se uživatele
        Dim ofd As New OpenFileDialog()
        ofd.Title = "Najdi ffmpeg.exe"
        ofd.Filter = "ffmpeg.exe|ffmpeg.exe"
        If ofd.ShowDialog() = DialogResult.OK Then
            My.Settings.FfmpegPath = ofd.FileName
            My.Settings.Save()
            Return ofd.FileName
        Else
            Throw New Exception("Nepodařilo se najít ffmpeg.exe")
        End If
    End Function

End Module
