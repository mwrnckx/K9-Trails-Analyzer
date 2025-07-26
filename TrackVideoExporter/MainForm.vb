'Imports System.IO
'Imports System.Windows.Forms
'Imports GPXTrailAnalyzer.TrackVideoExporter

'Public Class MainForm
'    Inherits Form

'    Private WithEvents btnSelectGpx As New Button With {.Text = "Vybrat GPX"}
'    Private WithEvents btnGenerate As New Button With {.Text = "Generovat video"}
'    Private txtGpxPath As New TextBox With {.Width = 300}
'    Private lblStatus As New Label With {.AutoSize = True}

'    Private Sub New()
'        Me.Text = "Video Creator MVP"
'        Me.Size = New Drawing.Size(450, 200)

'        btnSelectGpx.Location = New Drawing.Point(320, 20)
'        txtGpxPath.Location = New Drawing.Point(10, 20)
'        btnGenerate.Location = New Drawing.Point(10, 60)
'        lblStatus.Location = New Drawing.Point(10, 100)

'        Me.Controls.AddRange({txtGpxPath, btnSelectGpx, btnGenerate, lblStatus})
'    End Sub

'    <STAThread>
'    Public Shared Sub MainI()
'        Application.EnableVisualStyles()
'        Application.Run(New MainForm())
'    End Sub

'    Private Sub btnSelectGpx_Click(sender As Object, e As EventArgs) Handles btnSelectGpx.Click
'        Using ofd As New OpenFileDialog()
'            ofd.Filter = "GPX files (*.gpx)|*.gpx"
'            If ofd.ShowDialog() = DialogResult.OK Then
'                txtGpxPath.Text = ofd.FileName
'            End If
'        End Using
'    End Sub

'    Private Async Sub btnGenerate_Click(sender As Object, e As EventArgs) Handles btnGenerate.Click
'        lblStatus.Text = "Pracuji..."
'        Try
'            Dim gpxPath = txtGpxPath.Text
'            If Not File.Exists(gpxPath) Then
'                lblStatus.Text = "Soubor neexistuje."
'                Return
'            End If

'            Dim parser As New GpxReader(" ttt") ' Předpokládám, že GpxReader je třída, kterou jsi vytvořil pro čtení GPX souborů
'            Dim tracks '= parser.Parse(gpxPath) ' Musíš implementovat nebo připojit tvoji logiku GPX > TrackAsTrkNode

'            Dim outputDir = New DirectoryInfo(Path.Combine(Path.GetDirectoryName(gpxPath), "VideoOutput"))
'            If Not outputDir.Exists Then outputDir.Create()

'            Dim creator As New VideoExportManager(outputDir)
'            Dim ok = Await creator.CreateVideoFromTrkNode(tracks, Nothing)

'            lblStatus.Text = If(ok, "Video vygenerováno úspěšně", "Chyba při generování videa")

'        Catch ex As Exception
'            lblStatus.Text = "Chyba: " & ex.Message
'        End Try
'    End Sub
'End Class
