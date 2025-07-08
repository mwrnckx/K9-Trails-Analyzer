Public Class frmVideoDone
    Private outputFile As String = "outputfile" ' Default value, will be set in constructor
    Private _bgBitmap As Bitmap
    Public Sub New(_outputfile As String, _bgbmp As Bitmap)

        ' Toto volání je vyžadované návrhářem
        InitializeComponent()
        outputFile = _outputfile
        ' Přidejte libovolnou inicializaci po volání InitializeComponent().
        Me.lblInfo.Text = $"Video showing the dog's movement on the track has been created and saved to:" & vbCrLf & $"{outputFile}." & vbCrLf & "It can be used as an overlay video in picture-in-picture."
        Me._bgBitmap = _bgbmp
        Me.BackgroundImage = _bgBitmap


    End Sub

    Private Sub btnOpenFolder_Click(sender As Object, e As EventArgs) Handles btnOpenFolder.Click
        Dim folderPath As String = System.IO.Path.GetDirectoryName(outputFile)
        'Process.Start("explorer.exe", folderPath)
        Process.Start(folderPath, $"/select,""{outputFile}""")

    End Sub

    Private Sub btnPlayVideo_Click(sender As Object, e As EventArgs) Handles btnPlayVideo.Click
        Dim psi As New ProcessStartInfo(outputFile) With {
    .UseShellExecute = True
}
        Process.Start(psi)

        'Process.Start(outputFile)
    End Sub

    Private Sub btnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Me.Close()
    End Sub
End Class