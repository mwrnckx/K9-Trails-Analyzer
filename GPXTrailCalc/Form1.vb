﻿Imports System.ComponentModel
Imports System.Globalization
Imports System.IO
Imports System.Resources
Imports System.Threading
Imports System.Windows.Forms.DataVisualization.Charting
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports GPXTrailAnalyzer.My.Resources

Public Class Form1

    Private gpxCalculator As GPXDistanceCalculator
    Private currentCulture As CultureInfo = Thread.CurrentThread.CurrentCulture

    Private Sub btnReadGpxFiles_Click(sender As Object, e As EventArgs) Handles btnReadGpxFiles.Click
        Try
            'send directoryPath to gpxCalculator
            If gpxCalculator.ReadAndProcessData(dtpStartDate.Value, dtpEndDate.Value) Then
                Me.btnChartDistances.Visible = True
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


    Private Sub btnOpenDataFile_Click(sender As Object, e As EventArgs) Handles mnuSaveAsCsvFile.Click
        If gpxCalculator.distances.Count < 1 Then
            MessageBox.Show(My.Resources.Resource1.mBoxMissingData)
            Return
        End If

        Dim csvFileName As String = "GPX_File_Data_" & Today.ToString("yyyy-MM-dd") 'Path.Combine(directoryPath, "GPX_File_Data_" & Today.ToString("yyyy-MM-dd") & ".csv")

        Using dialog As New SaveFileDialog()
            dialog.Filter = "Soubory csv|*.csv"
            dialog.CheckFileExists = True 'když existuje zeptá se 
            dialog.AddExtension = True
            dialog.InitialDirectory = My.Settings.Directory
            dialog.Title = "Save as CSV"
            dialog.FileName = csvFileName

            If dialog.ShowDialog() = DialogResult.OK Then

                Debug.WriteLine($"Selected file: {dialog.FileName}")
                Dim csvFilePath As String = dialog.FileName
                Try
                    gpxCalculator.WriteCSVfile(csvFilePath)
                Catch ex As Exception
                    MessageBox.Show($"{My.Resources.Resource1.mBoxErrorCreatingCSV}: {csvFilePath} " & ex.Message & vbCrLf, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Using



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





    Public Sub ChangeLanguage(sender As Object, e As EventArgs) Handles mnuCzech.Click, mnuGerman.Click, mnuRussian.Click, mnuUkrainian.Click, mnuPolish.Click, mnuEnglish.Click
        Me.SuspendLayout()
        Dim cultureName As String = sender.Tag
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


        ' Nastavení ToolTip pro jednotlivé ovládací prvky

        mnuSelect_directory_gpx_files.ToolTipText = Resource1.Tooltip_txtDirectory
        mnuSelectBackupDirectory.ToolTipText = Resource1.Tooltip_txtBackupDirectory
        'TODO
        mnuSaveAsCsvFile.ToolTipText = Resource1.Tooltip_txtDirectory

        ' Nastavení ToolTip pro jednotlivé ovládací prvky

        ToolTip1.SetToolTip(btnChartDistances, Resource1.Tooltip_txtDirectory)


        ' Přidej další ovládací prvky, jak je potřeba



    End Sub


    'Private Sub btnCS_Click(sender As Object, e As EventArgs) Handles btnCS.Click
    '    ChangeLanguage("cs") ' Nastaví češtinu
    'End Sub

    'Private Sub btnEng_Click(sender As Object, e As EventArgs) Handles btnEng.Click
    '    ChangeLanguage("en") ' Nastaví angličtinu
    'End Sub

    'Private Sub btnDe_Click(sender As Object, e As EventArgs) Handles btnDe.Click
    '    ChangeLanguage("de") ' Nastaví češtinu
    'End Sub
    'Private Sub btnRu_Click(sender As Object, e As EventArgs) Handles btnRu.Click
    '    ChangeLanguage("ru") ' Nastaví češtinu
    'End Sub
    'Private Sub btnPl_Click(sender As Object, e As EventArgs) Handles btnPl.Click
    '    ChangeLanguage("pl") ' Nastaví češtinu
    'End Sub

    'Private Sub btnUK_Click(sender As Object, e As EventArgs) Handles btnUK.Click
    '    ChangeLanguage("uk")
    'End Sub




    Private Sub chbDateToName_CheckedChanged(sender As Object, e As EventArgs) Handles mnuPrependDateToFileName.CheckedChanged
        My.Settings.PrependDateToName = mnuPrependDateToFileName.Checked
    End Sub



    Private Sub mnuSelect_directory_gpx_files_Click(sender As Object, e As EventArgs) Handles mnuSelect_directory_gpx_files.Click, mnuSelectBackupDirectory.Click
        Dim folderDialog As New FolderBrowserDialog


        If sender Is Me.mnuSelect_directory_gpx_files Then
            folderDialog.SelectedPath = My.Settings.Directory
        ElseIf sender Is Me.mnuSelectBackupDirectory Then
            folderDialog.ShowNewFolderButton = True
            folderDialog.SelectedPath = My.Settings.BackupDirectory
        End If



        If folderDialog.ShowDialog() = DialogResult.OK Then

            If sender Is Me.mnuSelect_directory_gpx_files Then
                My.Settings.Directory = folderDialog.SelectedPath
            ElseIf sender Is Me.mnuSelectBackupDirectory Then
                My.Settings.BackupDirectory = folderDialog.SelectedPath
            End If

        End If

        My.Settings.Save()
        Me.StatusLabel1.Text = $"Directory: {ZkratCestu(My.Settings.Directory, 130)}" & vbCrLf & $"Backup Directory: {ZkratCestu(My.Settings.BackupDirectory, 130)}"

    End Sub


    Private Function ZkratCestu(cesta As String, maxDelka As Integer) As String
        ' Pokud je cesta krátká, není třeba ji upravovat
        If cesta.Length <= maxDelka Or maxDelka < 9 Then
            Return cesta
        End If

        ' Počet znaků, které ponecháme na začátku a na konci
        Dim pocetZnakuNaKazdeStrane As Integer = (maxDelka - 7) \ 2

        ' Vytvoříme zkrácenou cestu
        Dim zacatek As String = cesta.Substring(0, pocetZnakuNaKazdeStrane)
        Dim konec As String = cesta.Substring(cesta.Length - pocetZnakuNaKazdeStrane)

        Return zacatek & "  ...  " & konec
    End Function


End Class

