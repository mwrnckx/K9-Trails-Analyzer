﻿Imports System.ComponentModel
Imports System.Globalization
Imports System.IO
Imports System.Resources
Imports System.Threading
Imports System.Windows.Forms.DataVisualization.Charting
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports GPXTrailAnalyzer.My.Resources

Public Class Form1
    Dim directoryPath As String
    Private gpxCalculator As GPXDistanceCalculator
    Private currentCulture As CultureInfo = Thread.CurrentThread.CurrentCulture

    Private Sub btnReadGpxFiles_Click(sender As Object, e As EventArgs) Handles btnReadGpxFiles.Click
        Try
            'send directoryPath to gpxCalculator
            If gpxCalculator.ReadAndProcessData(directoryPath, dtpStartDate.Value, dtpEndDate.Value, chbDateToName.Checked) Then


                Me.btnChartDistances.Visible = True
                Me.btnOpenDataFile.Visible = True
                Me.rbTotDistance.Visible = True
                Me.rbDistances.Visible = True
                Me.rbAge.Visible = True
                Me.rbSpeed.Visible = True
            Else
                MessageBox.Show(My.Resources.Resource1.mBoxDataRetrievalFailed)
            End If
        Catch ex As Exception
            MessageBox.Show(My.Resources.Resource1.mBoxDataRetrievalFailed)
        End Try

    End Sub

    Private Sub btnBrowse_Click(sender As Object, e As EventArgs) Handles btnBrowse.Click
        Dim folderDialog As New FolderBrowserDialog With {
            .SelectedPath = txtDirectory.Text
        }
        If folderDialog.ShowDialog() = DialogResult.OK Then
            txtDirectory.Text = folderDialog.SelectedPath
        End If
    End Sub

    Private Sub txtDirectory_TextChanged(sender As Object, e As EventArgs) Handles txtDirectory.TextChanged
        My.Settings.Directory = Me.txtDirectory.Text
        directoryPath = txtDirectory.Text
    End Sub

    Private Sub btnOpenDataFile_Click(sender As Object, e As EventArgs) Handles btnOpenDataFile.Click
        Dim csvFileName As String = "GPX_File_Data_" & Today.ToString("yyyy-MM-dd") 'Path.Combine(directoryPath, "GPX_File_Data_" & Today.ToString("yyyy-MM-dd") & ".csv")
        Dim csvFilePath As String = Path.Combine(directoryPath, "GPX_File_Data_" & Today.ToString("yyyy-MM-dd") & ".csv")
        Try 'když existuje zeptá se 
            If File.Exists(csvFilePath) Then

                csvFilePath = gpxCalculator.SaveAsCsvFile(csvFileName)
                Process.Start(csvFilePath)

            Else
                gpxCalculator.WriteCSVfile(csvFilePath)
                Process.Start(csvFilePath)
            End If
        Catch ex As Exception
            MessageBox.Show($"{My.Resources.Resource1.mBoxErrorCreatingCSV}: {csvFilePath} " & ex.Message & vbCrLf, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnOpenChart(sender As Object, e As EventArgs) Handles btnChartDistances.Click
        'what to display
        Dim yAxisData() As Double
        Dim yAxisLabel As String
        Dim xAxisData As Date() = gpxCalculator.layerStart.Select(Function(ts) ts).ToArray()
        Dim GrafText As String = rbTotDistance.Text

        If rbTotDistance.Checked Then
            yAxisData = gpxCalculator.totalDistances.Select(Function(ts) ts).ToArray()
            yAxisLabel = My.Resources.Resource1.Y_AxisLabelTotalLength
            GrafText = rbTotDistance.Text
        ElseIf rbDistances.Checked Then
            yAxisData = gpxCalculator.distances.Select(Function(ts) ts).ToArray()
            yAxisLabel = My.Resources.Resource1.Y_AxisLabelLength
            GrafText = rbDistances.Text
        ElseIf rbAge.Checked Then
            ' Filtrování y-hodnot (TotalHours) a x-hodnot (časové značky) pro body, kde TotalHours není nulová
            yAxisData = gpxCalculator.age.
    Where(Function(ts, index) ts.TotalHours <> 0). ' Podmínka pro filtrování TotalHours == 0
    Select(Function(ts) ts.TotalHours).
    ToArray()

            ' Filtrování x-hodnot (časové značky) podle stejných indexů jako yAxisData
            xAxisData = gpxCalculator.layerStart.
    Where(Function(ts, index) gpxCalculator.age(index).TotalHours <> 0).
    Select(Function(ts) ts).
    ToArray()
            yAxisLabel = My.Resources.Resource1.Y_AxisLabelAge
            GrafText = rbAge.Text
        ElseIf rbSpeed.Checked Then
            ' Načtení y-hodnot a filtrování hodnot, kde je y nulové
            yAxisData = gpxCalculator.speed.
    Where(Function(ts, index) gpxCalculator.speed(index) <> 0). ' Podmínka pro filtrování y == 0
    Select(Function(ts) ts).
    ToArray()
            ' Filtrování x-hodnot (časové značky) podle stejného indexu jako yAxisData
            xAxisData = gpxCalculator.layerStart.
    Where(Function(ts, index) gpxCalculator.speed(index) <> 0).
    Select(Function(ts) ts).
    ToArray()

            yAxisLabel = My.Resources.Resource1.Y_AxisLabelSpeed
            GrafText = rbSpeed.Text
        End If



        ' Vytvoření instance DistanceChart s filtrováním bodů, kde je y-hodnota nulová
        If Not gpxCalculator.distances Is Nothing Then
            Dim distanceChart As New DistanceChart(xAxisData, yAxisData, yAxisLabel, Me.dtpStartDate.Value, dtpEndDate.Value, Me.currentCulture)

            ' Zobrazení grafu
            distanceChart.Display(GrafText)
        Else
            MessageBox.Show("First you need to read the data from the gpx files")
        End If


    End Sub





    Public Sub ChangeLanguage(cultureName As String)
        Me.SuspendLayout()

        Thread.CurrentThread.CurrentUICulture = New CultureInfo(cultureName)

        Me.currentCulture = Thread.CurrentThread.CurrentUICulture
        Dim resources = New ComponentResourceManager(Me.GetType())
        resources.ApplyResources(Me, "$this")
        For Each ctrl As Control In Me.Controls
            resources.ApplyResources(ctrl, ctrl.Name)

            If TypeOf ctrl Is DateTimePicker Then
                Dim dtp As DateTimePicker = DirectCast(ctrl, DateTimePicker)

                ' Nastavení formátu podle aktuální kultury
                dtp.Format = DateTimePickerFormat.Custom
                dtp.CustomFormat = Thread.CurrentThread.CurrentUICulture.DateTimeFormat.ShortDatePattern

            End If
        Next

        SetTooltips()

        Me.ResumeLayout()
    End Sub

    Private Sub SetTooltips()

        Dim rm As New ResourceManager("Resource1", GetType(Form1).Assembly)

        ' Nastavení ToolTip pro jednotlivé ovládací prvky
        ToolTip1.SetToolTip(txtDirectory, Resource1.Tooltip_txtDirectory)
        ToolTip1.SetToolTip(txtBackupDirectory, Resource1.Tooltip_txtBackupDirectory)

        ' Přidej další ovládací prvky, jak je potřeba
    End Sub


    Private Sub btnCS_Click(sender As Object, e As EventArgs) Handles btnCS.Click
        ChangeLanguage("cs") ' Nastaví češtinu
    End Sub

    Private Sub btnEng_Click(sender As Object, e As EventArgs) Handles btnEng.Click
        ChangeLanguage("en") ' Nastaví angličtinu
    End Sub

    Private Sub btnDe_Click(sender As Object, e As EventArgs) Handles btnDe.Click
        ChangeLanguage("de") ' Nastaví češtinu
    End Sub
    Private Sub btnRu_Click(sender As Object, e As EventArgs) Handles btnRu.Click
        ChangeLanguage("ru") ' Nastaví češtinu
    End Sub
    Private Sub btnPl_Click(sender As Object, e As EventArgs) Handles btnPl.Click
        ChangeLanguage("pl") ' Nastaví češtinu
    End Sub

    Private Sub btnUK_Click(sender As Object, e As EventArgs) Handles btnUK.Click
        ChangeLanguage("uk")
    End Sub



    Private Sub txtBackupDirectory_textChanged(sender As Object, e As EventArgs) Handles txtBackupDirectory.TextChanged
        My.Settings.BackupDirectory = Me.txtBackupDirectory.Text
    End Sub

    Private Sub chbDateToName_CheckedChanged(sender As Object, e As EventArgs) Handles chbDateToName.CheckedChanged
        My.Settings.PrependDateToName = chbDateToName.Checked
    End Sub

    Private Sub btnBrowseBackup_Click(sender As Object, e As EventArgs) Handles btnBrowseBackup.Click
        Dim folderDialog As New FolderBrowserDialog With {
            .SelectedPath = txtBackupDirectory.Text
        }
        If folderDialog.ShowDialog() = DialogResult.OK Then
            txtBackupDirectory.Text = folderDialog.SelectedPath
        End If
    End Sub


End Class

