Public Class frmVideoDone
    Private outputFile As String = "outputfile" ' Default value, will be set in constructor
    Public Sub New(_outputfile As String, Optional bgImagepath As String = "", Optional bgPNG As Bitmap = Nothing)

        ' Toto volání je vyžadované návrhářem
        InitializeComponent()
        outputFile = _outputfile
        ' Přidejte libovolnou inicializaci po volání InitializeComponent().
        Me.lblInfo.Text = $"Video showing the dog's movement on the track has been created and saved to:" &
            $"{outputFile}." & vbCrLf & "It can be used as an overlay video in picture-in-picture."
        Try
            Dim img As Image
            Using fs As New IO.FileStream(bgImagepath, IO.FileMode.Open, IO.FileAccess.Read)
                img = Image.FromStream(fs)
            End Using
            Me.BackgroundImage = img
        Catch ex As Exception
            Try
                Me.BackgroundImage = bgPNG
            Catch ex2 As Exception

            End Try
        End Try
        lblInfo.MaximumSize = New Size(Me.Width * 0.8, Me.Height * 0.8) 'nastaví maximální šířku popisku
    End Sub

    Private Sub btnOpenFolder_Click(sender As Object, e As EventArgs) Handles btnOpenFolder.Click
        Dim folderPath As String = System.IO.Path.GetDirectoryName(outputFile)
        'Process.Start("explorer.exe", folderPath)
        Process.Start("explorer.exe", folderPath)


    End Sub

    Private Sub btnPlayVideo_Click(sender As Object, e As EventArgs) Handles btnPlayVideo.Click
        Dim psi As New ProcessStartInfo(outputFile) With {
    .UseShellExecute = True
        }
        Process.Start(psi)
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class