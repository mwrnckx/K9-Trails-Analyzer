Imports System.ComponentModel
Imports System.Reflection
Imports System.Threading

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form
    Private components As System.ComponentModel.IContainer
    ' The form overrides the Dispose method to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    ' Required by the Windows Form Designer
    ' It can be modified using the Windows Form Designer.
    ' Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.dtpStartDate = New System.Windows.Forms.DateTimePicker()
        Me.dtpEndDate = New System.Windows.Forms.DateTimePicker()
        Me.btnReadGpxFiles = New System.Windows.Forms.Button()
        Me.btnCharts = New System.Windows.Forms.Button()
        Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.StatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.mnuFile = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuSelect_directory_gpx_files = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuSelectBackupDirectory = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuExportAs = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuSettings = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuPrependDateToFileName = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuTrimGPSNoise = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuMergingTracks = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuProcessProcessed = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuAskForVideo = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuLanguage = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuEnglish = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuGerman = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuCzech = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuUkrainian = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuPolish = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuRussian = New System.Windows.Forms.ToolStripMenuItem()
        Me.SToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.FactoryResetToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.mnuDogName = New System.Windows.Forms.ToolStripMenuItem()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.gbPeriod = New System.Windows.Forms.GroupBox()
        Me.lblScentArtickle = New System.Windows.Forms.Label()
        Me.rtbOutput = New System.Windows.Forms.RichTextBox()
        Me.rtbWarnings = New System.Windows.Forms.RichTextBox()
        Me.StatusStrip1.SuspendLayout()
        Me.MenuStrip1.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.gbPeriod.SuspendLayout()
        Me.SuspendLayout()
        '
        'dtpStartDate
        '
        resources.ApplyResources(Me.dtpStartDate, "dtpStartDate")
        Me.dtpStartDate.CalendarMonthBackground = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dtpStartDate.Name = "dtpStartDate"
        '
        'dtpEndDate
        '
        resources.ApplyResources(Me.dtpEndDate, "dtpEndDate")
        Me.dtpEndDate.CalendarMonthBackground = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(255, Byte), Integer), CType(CType(192, Byte), Integer))
        Me.dtpEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom
        Me.dtpEndDate.Name = "dtpEndDate"
        '
        'btnReadGpxFiles
        '
        Me.btnReadGpxFiles.BackColor = System.Drawing.Color.Salmon
        resources.ApplyResources(Me.btnReadGpxFiles, "btnReadGpxFiles")
        Me.btnReadGpxFiles.Name = "btnReadGpxFiles"
        Me.btnReadGpxFiles.UseVisualStyleBackColor = False
        '
        'btnCharts
        '
        Me.btnCharts.BackColor = System.Drawing.Color.DarkGoldenrod
        resources.ApplyResources(Me.btnCharts, "btnCharts")
        Me.btnCharts.Name = "btnCharts"
        Me.btnCharts.UseVisualStyleBackColor = False
        '
        'ToolTip1
        '
        Me.ToolTip1.IsBalloon = True
        '
        'StatusStrip1
        '
        Me.StatusStrip1.ImageScalingSize = New System.Drawing.Size(24, 24)
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.StatusLabel1})
        resources.ApplyResources(Me.StatusStrip1, "StatusStrip1")
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.SizingGrip = False
        Me.ToolTip1.SetToolTip(Me.StatusStrip1, resources.GetString("StatusStrip1.ToolTip"))
        '
        'StatusLabel1
        '
        Me.StatusLabel1.Name = "StatusLabel1"
        resources.ApplyResources(Me.StatusLabel1, "StatusLabel1")
        '
        'MenuStrip1
        '
        Me.MenuStrip1.BackColor = System.Drawing.Color.FromArgb(CType(CType(172, Byte), Integer), CType(CType(209, Byte), Integer), CType(CType(158, Byte), Integer))
        Me.MenuStrip1.GripMargin = New System.Windows.Forms.Padding(2, 2, 0, 2)
        Me.MenuStrip1.ImageScalingSize = New System.Drawing.Size(24, 24)
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuFile, Me.mnuSettings, Me.mnuLanguage, Me.SToolStripMenuItem})
        resources.ApplyResources(Me.MenuStrip1, "MenuStrip1")
        Me.MenuStrip1.Name = "MenuStrip1"
        '
        'mnuFile
        '
        Me.mnuFile.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuSelect_directory_gpx_files, Me.mnuSelectBackupDirectory, Me.mnuExportAs})
        resources.ApplyResources(Me.mnuFile, "mnuFile")
        Me.mnuFile.Name = "mnuFile"
        '
        'mnuSelect_directory_gpx_files
        '
        Me.mnuSelect_directory_gpx_files.Name = "mnuSelect_directory_gpx_files"
        resources.ApplyResources(Me.mnuSelect_directory_gpx_files, "mnuSelect_directory_gpx_files")
        '
        'mnuSelectBackupDirectory
        '
        Me.mnuSelectBackupDirectory.Name = "mnuSelectBackupDirectory"
        resources.ApplyResources(Me.mnuSelectBackupDirectory, "mnuSelectBackupDirectory")
        '
        'mnuExportAs
        '
        Me.mnuExportAs.Name = "mnuExportAs"
        resources.ApplyResources(Me.mnuExportAs, "mnuExportAs")
        '
        'mnuSettings
        '
        Me.mnuSettings.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuPrependDateToFileName, Me.mnuTrimGPSNoise, Me.mnuMergingTracks, Me.mnuProcessProcessed, Me.mnuAskForVideo})
        resources.ApplyResources(Me.mnuSettings, "mnuSettings")
        Me.mnuSettings.Name = "mnuSettings"
        '
        'mnuPrependDateToFileName
        '
        Me.mnuPrependDateToFileName.CheckOnClick = True
        Me.mnuPrependDateToFileName.Name = "mnuPrependDateToFileName"
        resources.ApplyResources(Me.mnuPrependDateToFileName, "mnuPrependDateToFileName")
        '
        'mnuTrimGPSNoise
        '
        Me.mnuTrimGPSNoise.CheckOnClick = True
        Me.mnuTrimGPSNoise.Name = "mnuTrimGPSNoise"
        resources.ApplyResources(Me.mnuTrimGPSNoise, "mnuTrimGPSNoise")
        '
        'mnuMergingTracks
        '
        Me.mnuMergingTracks.Name = "mnuMergingTracks"
        resources.ApplyResources(Me.mnuMergingTracks, "mnuMergingTracks")
        '
        'mnuProcessProcessed
        '
        Me.mnuProcessProcessed.CheckOnClick = True
        Me.mnuProcessProcessed.Name = "mnuProcessProcessed"
        resources.ApplyResources(Me.mnuProcessProcessed, "mnuProcessProcessed")
        '
        'mnuAskForVideo
        '
        Me.mnuAskForVideo.CheckOnClick = True
        Me.mnuAskForVideo.Name = "mnuAskForVideo"
        resources.ApplyResources(Me.mnuAskForVideo, "mnuAskForVideo")
        '
        'mnuLanguage
        '
        Me.mnuLanguage.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuEnglish, Me.mnuGerman, Me.mnuCzech, Me.mnuUkrainian, Me.mnuPolish, Me.mnuRussian})
        resources.ApplyResources(Me.mnuLanguage, "mnuLanguage")
        Me.mnuLanguage.Name = "mnuLanguage"
        '
        'mnuEnglish
        '
        Me.mnuEnglish.Image = Global.GPXTrailAnalyzer.My.Resources.Resources.en_flag
        resources.ApplyResources(Me.mnuEnglish, "mnuEnglish")
        Me.mnuEnglish.Name = "mnuEnglish"
        Me.mnuEnglish.Tag = "en"
        '
        'mnuGerman
        '
        Me.mnuGerman.Image = Global.GPXTrailAnalyzer.My.Resources.Resources.De_Flag
        resources.ApplyResources(Me.mnuGerman, "mnuGerman")
        Me.mnuGerman.Name = "mnuGerman"
        Me.mnuGerman.Tag = "de"
        '
        'mnuCzech
        '
        Me.mnuCzech.Image = Global.GPXTrailAnalyzer.My.Resources.Resources.czech_flag
        resources.ApplyResources(Me.mnuCzech, "mnuCzech")
        Me.mnuCzech.Name = "mnuCzech"
        Me.mnuCzech.Tag = "cs"
        '
        'mnuUkrainian
        '
        Me.mnuUkrainian.Image = Global.GPXTrailAnalyzer.My.Resources.Resources.uk_flag
        resources.ApplyResources(Me.mnuUkrainian, "mnuUkrainian")
        Me.mnuUkrainian.Name = "mnuUkrainian"
        Me.mnuUkrainian.Tag = "uk"
        '
        'mnuPolish
        '
        Me.mnuPolish.Image = Global.GPXTrailAnalyzer.My.Resources.Resources.pl_flag
        resources.ApplyResources(Me.mnuPolish, "mnuPolish")
        Me.mnuPolish.Name = "mnuPolish"
        Me.mnuPolish.Tag = "pl"
        '
        'mnuRussian
        '
        Me.mnuRussian.Image = Global.GPXTrailAnalyzer.My.Resources.Resources.ru_flag
        resources.ApplyResources(Me.mnuRussian, "mnuRussian")
        Me.mnuRussian.Name = "mnuRussian"
        Me.mnuRussian.Tag = "ru"
        '
        'SToolStripMenuItem
        '
        Me.SToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FactoryResetToolStripMenuItem, Me.mnuDogName})
        resources.ApplyResources(Me.SToolStripMenuItem, "SToolStripMenuItem")
        Me.SToolStripMenuItem.Name = "SToolStripMenuItem"
        '
        'FactoryResetToolStripMenuItem
        '
        Me.FactoryResetToolStripMenuItem.Name = "FactoryResetToolStripMenuItem"
        resources.ApplyResources(Me.FactoryResetToolStripMenuItem, "FactoryResetToolStripMenuItem")
        '
        'mnuDogName
        '
        Me.mnuDogName.Name = "mnuDogName"
        resources.ApplyResources(Me.mnuDogName, "mnuDogName")
        '
        'PictureBox1
        '
        resources.ApplyResources(Me.PictureBox1, "PictureBox1")
        Me.PictureBox1.Image = Global.GPXTrailAnalyzer.My.Resources.Resources.bgImage
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.TabStop = False
        '
        'gbPeriod
        '
        Me.gbPeriod.BackColor = System.Drawing.Color.FromArgb(CType(CType(237, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(213, Byte), Integer))
        Me.gbPeriod.Controls.Add(Me.dtpEndDate)
        Me.gbPeriod.Controls.Add(Me.dtpStartDate)
        Me.gbPeriod.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        resources.ApplyResources(Me.gbPeriod, "gbPeriod")
        Me.gbPeriod.Name = "gbPeriod"
        Me.gbPeriod.TabStop = False
        '
        'lblScentArtickle
        '
        resources.ApplyResources(Me.lblScentArtickle, "lblScentArtickle")
        Me.lblScentArtickle.BackColor = System.Drawing.Color.FromArgb(CType(CType(237, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(213, Byte), Integer))
        Me.lblScentArtickle.Name = "lblScentArtickle"
        '
        'rtbOutput
        '
        Me.rtbOutput.BackColor = System.Drawing.Color.FromArgb(CType(CType(237, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(213, Byte), Integer))
        resources.ApplyResources(Me.rtbOutput, "rtbOutput")
        Me.rtbOutput.Name = "rtbOutput"
        '
        'rtbWarnings
        '
        Me.rtbWarnings.BackColor = System.Drawing.Color.FromArgb(CType(CType(237, Byte), Integer), CType(CType(240, Byte), Integer), CType(CType(213, Byte), Integer))
        resources.ApplyResources(Me.rtbWarnings, "rtbWarnings")
        Me.rtbWarnings.Name = "rtbWarnings"
        '
        'Form1
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.rtbWarnings)
        Me.Controls.Add(Me.rtbOutput)
        Me.Controls.Add(Me.lblScentArtickle)
        Me.Controls.Add(Me.gbPeriod)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.btnCharts)
        Me.Controls.Add(Me.btnReadGpxFiles)
        Me.Controls.Add(Me.MenuStrip1)
        Me.Controls.Add(Me.PictureBox1)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "Form1"
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.gbPeriod.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents dtpStartDate As DateTimePicker
    Friend WithEvents dtpEndDate As DateTimePicker
    Friend WithEvents btnReadGpxFiles As Button


    Public Sub New()
        ' Toto volání je vyžadované návrhářem.
        InitializeComponent()
        ' Přidejte libovolnou inicializaci po volání InitializeComponent().
    End Sub


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        WriteRTBWarning("Logs:", Color.Maroon)


        mnuPrependDateToFileName.Checked = My.Settings.PrependDateToName
        mnuTrimGPSNoise.Checked = My.Settings.TrimGPSnoise
        mnuAskForVideo.Checked = My.Settings.AskForVideo

        If My.Settings.Directory = "" Then
            My.Settings.Directory = IO.Directory.GetParent(Application.StartupPath).ToString
        End If

        If My.Settings.BackupDirectory = "" Then
            My.Settings.BackupDirectory = System.IO.Path.Combine(My.Settings.Directory, "gpxFilesBackup")
        End If
        My.Settings.Save()
        CreateGpxFileManager()
        'gpxCalculator = New GPXDistanceCalculator()
        Me.dtpEndDate.Value = Now
        Me.dtpStartDate.Value = Me.dtpEndDate.Value.AddYears(-1)
        Me.dtpStartDate.Value = Me.dtpStartDate.Value.AddDays(1)


        Me.StatusLabel1.Text = $"Directory: {ZkratCestu(My.Settings.Directory, 130)}" & vbCrLf & $"Backup Directory: {ZkratCestu(My.Settings.BackupDirectory, 130)}"
        Dim resources = New ComponentResourceManager(Me.GetType())
        LocalizeMenuItems(MenuStrip1.Items, resources)
        SetTooltips()
        Dim height As Integer = 18
        ' Nastavení obrázku na ToolStripMenuItem
        mnuEnglish.Image = resizeImage(My.Resources.en_flag, Nothing, height)
        mnuGerman.Image = resizeImage(My.Resources.De_Flag, Nothing, height)
        mnuPolish.Image = resizeImage(My.Resources.pl_flag, Nothing, height)
        mnuRussian.Image = resizeImage(My.Resources.ru_flag, Nothing, height)
        mnuUkrainian.Image = resizeImage(My.Resources.uk_flag, Nothing, height)
        mnuCzech.Image = resizeImage(My.Resources.czech_flag, Nothing, height)

        ReadHelp()

        ' Nastavení fontu a barvy textu
        Me.rtbOutput.SelectionStart = Me.rtbOutput.Text.Length ' Pozice na konec textu

        Dim thisAssem As Assembly = GetType(Form1).Assembly
        Dim thisAssemName As AssemblyName = thisAssem.GetName()
        'verze se nastaví v AssamblyInfo.vb nebo v My Project -> Aplikace -> Informace o sestavení!!!!!!!!!!!!!!!!
        Me.Text = thisAssemName.Name & "   " & thisAssemName.Version.ToString


    End Sub

    Private Sub ReadHelp()
        Select Case currentCulture.TwoLetterISOLanguageName
            Case "cs"
                Me.rtbOutput.Rtf = (My.Resources.Readme_cs)
            Case "en"
                Me.rtbOutput.Rtf = (My.Resources.Readme_en)
            Case "de"
                Me.rtbOutput.Rtf = (My.Resources.Readme_de)
            Case "pl"
                Me.rtbOutput.Rtf = (My.Resources.readme_pl)
            Case "ru"
                Me.rtbOutput.Rtf = (My.Resources.readme_ru)
            Case "uk"
                Me.rtbOutput.Rtf = (My.Resources.readme_uk)

        End Select
    End Sub




    Friend WithEvents btnCharts As Button
    Friend WithEvents ToolTip1 As ToolTip
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents mnuFile As ToolStripMenuItem
    Friend WithEvents mnuSelect_directory_gpx_files As ToolStripMenuItem
    Friend WithEvents mnuSelectBackupDirectory As ToolStripMenuItem
    Friend WithEvents PictureBox1 As PictureBox
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents StatusLabel1 As ToolStripStatusLabel
    Friend WithEvents mnuSettings As ToolStripMenuItem
    Friend WithEvents mnuPrependDateToFileName As ToolStripMenuItem
    Friend WithEvents mnuLanguage As ToolStripMenuItem
    Friend WithEvents mnuCzech As ToolStripMenuItem
    Friend WithEvents mnuUkrainian As ToolStripMenuItem
    Friend WithEvents mnuEnglish As ToolStripMenuItem
    Friend WithEvents mnuGerman As ToolStripMenuItem
    Friend WithEvents mnuRussian As ToolStripMenuItem
    Friend WithEvents mnuPolish As ToolStripMenuItem
    Friend WithEvents gbPeriod As GroupBox
    Friend WithEvents lblScentArtickle As Label
    Friend WithEvents mnuTrimGPSNoise As ToolStripMenuItem
    Friend WithEvents rtbOutput As RichTextBox
    Friend WithEvents mnuExportAs As ToolStripMenuItem
    Friend WithEvents mnuMergingTracks As ToolStripMenuItem
    Friend WithEvents SToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents FactoryResetToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents rtbWarnings As RichTextBox
    Friend WithEvents mnuDogName As ToolStripMenuItem
    Friend WithEvents mnuProcessProcessed As ToolStripMenuItem
    Friend WithEvents mnuAskForVideo As ToolStripMenuItem
End Class

