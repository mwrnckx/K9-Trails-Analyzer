Imports System.ComponentModel
Imports System.Reflection
Imports System.Threading
Imports TrackVideoExporter

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
        components = New Container()
        Dim resources As ComponentResourceManager = New ComponentResourceManager(GetType(Form1))
        ToolTip1 = New ToolTip(components)
        StatusStrip1 = New StatusStrip()
        StatusLabel1 = New ToolStripStatusLabel()
        btnCreateVideos = New Button()
        MenuStrip1 = New MenuStrip()
        mnuFile = New ToolStripMenuItem()
        mnuSelect_directory_gpx_files = New ToolStripMenuItem()
        mnuSelectBackupDirectory = New ToolStripMenuItem()
        mnuExportAs = New ToolStripMenuItem()
        mnuExit = New ToolStripMenuItem()
        mnuSettings = New ToolStripMenuItem()
        mnuPrependDateToFileName = New ToolStripMenuItem()
        mnuTrimGPSNoise = New ToolStripMenuItem()
        mnuMergingTracks = New ToolStripMenuItem()
        mnuProcessProcessed = New ToolStripMenuItem()
        mnuLanguage = New ToolStripMenuItem()
        mnuEnglish = New ToolStripMenuItem()
        mnuGerman = New ToolStripMenuItem()
        mnuCzech = New ToolStripMenuItem()
        mnuUkrainian = New ToolStripMenuItem()
        mnuPolish = New ToolStripMenuItem()
        mnuRussian = New ToolStripMenuItem()
        SToolStripMenuItem = New ToolStripMenuItem()
        mnuDogName = New ToolStripMenuItem()
        mnuSelectADirectoryToSaveVideo = New ToolStripMenuItem()
        mnuSetFFmpegPath = New ToolStripMenuItem()
        mnuFactoryReset = New ToolStripMenuItem()
        mnuAbout = New ToolStripMenuItem()
        mnuCheckUpdates = New ToolStripMenuItem()
        PictureBox1 = New PictureBox()
        lblScentArtickle = New Label()
        rtbWarnings = New RichTextBox()
        TabControl1 = New TabControl()
        TabStats = New TabPage()
        rtbOutput = New RichTextBox()
        gbPeriod = New GroupBox()
        cmbTimeInterval = New ComboBox()
        dtpEndDate = New DateTimePicker()
        dtpStartDate = New DateTimePicker()
        btnCharts = New Button()
        btnReadGpxFiles = New Button()
        PictureBox2 = New PictureBox()
        TabVideoExport = New TabPage()
        lvGpxFiles = New ListView()
        clmFileName = New ColumnHeader()
        clmDate = New ColumnHeader()
        clmLength = New ColumnHeader()
        clmAge = New ColumnHeader()
        clmTrkCount = New ColumnHeader()
        PictureBox3 = New PictureBox()
        StatusStrip1.SuspendLayout()
        MenuStrip1.SuspendLayout()
        CType(PictureBox1, ISupportInitialize).BeginInit()
        TabControl1.SuspendLayout()
        TabStats.SuspendLayout()
        gbPeriod.SuspendLayout()
        CType(PictureBox2, ISupportInitialize).BeginInit()
        TabVideoExport.SuspendLayout()
        CType(PictureBox3, ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' ToolTip1
        ' 
        ToolTip1.IsBalloon = True
        ' 
        ' StatusStrip1
        ' 
        StatusStrip1.ImageScalingSize = New Size(24, 24)
        StatusStrip1.Items.AddRange(New ToolStripItem() {StatusLabel1})
        resources.ApplyResources(StatusStrip1, "StatusStrip1")
        StatusStrip1.Name = "StatusStrip1"
        StatusStrip1.SizingGrip = False
        ToolTip1.SetToolTip(StatusStrip1, resources.GetString("StatusStrip1.ToolTip"))
        ' 
        ' StatusLabel1
        ' 
        StatusLabel1.Name = "StatusLabel1"
        resources.ApplyResources(StatusLabel1, "StatusLabel1")
        ' 
        ' btnCreateVideos
        ' 
        resources.ApplyResources(btnCreateVideos, "btnCreateVideos")
        btnCreateVideos.BackColor = Color.Salmon
        btnCreateVideos.Name = "btnCreateVideos"
        ToolTip1.SetToolTip(btnCreateVideos, resources.GetString("btnCreateVideos.ToolTip"))
        btnCreateVideos.UseVisualStyleBackColor = False
        ' 
        ' MenuStrip1
        ' 
        MenuStrip1.BackColor = Color.FromArgb(CByte(172), CByte(209), CByte(158))
        resources.ApplyResources(MenuStrip1, "MenuStrip1")
        MenuStrip1.ImageScalingSize = New Size(24, 24)
        MenuStrip1.Items.AddRange(New ToolStripItem() {mnuFile, mnuSettings, mnuLanguage, SToolStripMenuItem})
        MenuStrip1.Name = "MenuStrip1"
        ' 
        ' mnuFile
        ' 
        mnuFile.DropDownItems.AddRange(New ToolStripItem() {mnuSelect_directory_gpx_files, mnuSelectBackupDirectory, mnuExportAs, mnuExit})
        resources.ApplyResources(mnuFile, "mnuFile")
        mnuFile.Name = "mnuFile"
        ' 
        ' mnuSelect_directory_gpx_files
        ' 
        mnuSelect_directory_gpx_files.Name = "mnuSelect_directory_gpx_files"
        resources.ApplyResources(mnuSelect_directory_gpx_files, "mnuSelect_directory_gpx_files")
        ' 
        ' mnuSelectBackupDirectory
        ' 
        mnuSelectBackupDirectory.Name = "mnuSelectBackupDirectory"
        resources.ApplyResources(mnuSelectBackupDirectory, "mnuSelectBackupDirectory")
        ' 
        ' mnuExportAs
        ' 
        mnuExportAs.Name = "mnuExportAs"
        resources.ApplyResources(mnuExportAs, "mnuExportAs")
        ' 
        ' mnuExit
        ' 
        mnuExit.Name = "mnuExit"
        resources.ApplyResources(mnuExit, "mnuExit")
        ' 
        ' mnuSettings
        ' 
        mnuSettings.DropDownItems.AddRange(New ToolStripItem() {mnuPrependDateToFileName, mnuTrimGPSNoise, mnuMergingTracks, mnuProcessProcessed})
        resources.ApplyResources(mnuSettings, "mnuSettings")
        mnuSettings.Name = "mnuSettings"
        ' 
        ' mnuPrependDateToFileName
        ' 
        mnuPrependDateToFileName.CheckOnClick = True
        mnuPrependDateToFileName.Name = "mnuPrependDateToFileName"
        resources.ApplyResources(mnuPrependDateToFileName, "mnuPrependDateToFileName")
        ' 
        ' mnuTrimGPSNoise
        ' 
        mnuTrimGPSNoise.CheckOnClick = True
        mnuTrimGPSNoise.Name = "mnuTrimGPSNoise"
        resources.ApplyResources(mnuTrimGPSNoise, "mnuTrimGPSNoise")
        ' 
        ' mnuMergingTracks
        ' 
        mnuMergingTracks.Name = "mnuMergingTracks"
        resources.ApplyResources(mnuMergingTracks, "mnuMergingTracks")
        ' 
        ' mnuProcessProcessed
        ' 
        mnuProcessProcessed.CheckOnClick = True
        mnuProcessProcessed.Name = "mnuProcessProcessed"
        resources.ApplyResources(mnuProcessProcessed, "mnuProcessProcessed")
        ' 
        ' mnuLanguage
        ' 
        mnuLanguage.DropDownItems.AddRange(New ToolStripItem() {mnuEnglish, mnuGerman, mnuCzech, mnuUkrainian, mnuPolish, mnuRussian})
        resources.ApplyResources(mnuLanguage, "mnuLanguage")
        mnuLanguage.Name = "mnuLanguage"
        ' 
        ' mnuEnglish
        ' 
        resources.ApplyResources(mnuEnglish, "mnuEnglish")
        mnuEnglish.Name = "mnuEnglish"
        mnuEnglish.Tag = "en"
        ' 
        ' mnuGerman
        ' 
        resources.ApplyResources(mnuGerman, "mnuGerman")
        mnuGerman.Name = "mnuGerman"
        mnuGerman.Tag = "de"
        ' 
        ' mnuCzech
        ' 
        resources.ApplyResources(mnuCzech, "mnuCzech")
        mnuCzech.Name = "mnuCzech"
        mnuCzech.Tag = "cs"
        ' 
        ' mnuUkrainian
        ' 
        resources.ApplyResources(mnuUkrainian, "mnuUkrainian")
        mnuUkrainian.Name = "mnuUkrainian"
        mnuUkrainian.Tag = "uk"
        ' 
        ' mnuPolish
        ' 
        resources.ApplyResources(mnuPolish, "mnuPolish")
        mnuPolish.Name = "mnuPolish"
        mnuPolish.Tag = "pl"
        ' 
        ' mnuRussian
        ' 
        resources.ApplyResources(mnuRussian, "mnuRussian")
        mnuRussian.Name = "mnuRussian"
        mnuRussian.Tag = "ru"
        ' 
        ' SToolStripMenuItem
        ' 
        SToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {mnuDogName, mnuSelectADirectoryToSaveVideo, mnuSetFFmpegPath, mnuFactoryReset, mnuAbout, mnuCheckUpdates})
        resources.ApplyResources(SToolStripMenuItem, "SToolStripMenuItem")
        SToolStripMenuItem.Name = "SToolStripMenuItem"
        ' 
        ' mnuDogName
        ' 
        mnuDogName.Name = "mnuDogName"
        resources.ApplyResources(mnuDogName, "mnuDogName")
        ' 
        ' mnuSelectADirectoryToSaveVideo
        ' 
        mnuSelectADirectoryToSaveVideo.Name = "mnuSelectADirectoryToSaveVideo"
        resources.ApplyResources(mnuSelectADirectoryToSaveVideo, "mnuSelectADirectoryToSaveVideo")
        ' 
        ' mnuSetFFmpegPath
        ' 
        mnuSetFFmpegPath.Name = "mnuSetFFmpegPath"
        resources.ApplyResources(mnuSetFFmpegPath, "mnuSetFFmpegPath")
        ' 
        ' mnuFactoryReset
        ' 
        mnuFactoryReset.Name = "mnuFactoryReset"
        resources.ApplyResources(mnuFactoryReset, "mnuFactoryReset")
        ' 
        ' mnuAbout
        ' 
        mnuAbout.Name = "mnuAbout"
        resources.ApplyResources(mnuAbout, "mnuAbout")
        ' 
        ' mnuCheckUpdates
        ' 
        mnuCheckUpdates.Name = "mnuCheckUpdates"
        resources.ApplyResources(mnuCheckUpdates, "mnuCheckUpdates")
        ' 
        ' PictureBox1
        ' 
        resources.ApplyResources(PictureBox1, "PictureBox1")
        PictureBox1.Name = "PictureBox1"
        PictureBox1.TabStop = False
        ' 
        ' lblScentArtickle
        ' 
        resources.ApplyResources(lblScentArtickle, "lblScentArtickle")
        lblScentArtickle.BackColor = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        lblScentArtickle.Name = "lblScentArtickle"
        ' 
        ' rtbWarnings
        ' 
        rtbWarnings.BackColor = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        resources.ApplyResources(rtbWarnings, "rtbWarnings")
        rtbWarnings.Name = "rtbWarnings"
        ' 
        ' TabControl1
        ' 
        TabControl1.Controls.Add(TabStats)
        TabControl1.Controls.Add(TabVideoExport)
        resources.ApplyResources(TabControl1, "TabControl1")
        TabControl1.Name = "TabControl1"
        TabControl1.SelectedIndex = 0
        ' 
        ' TabStats
        ' 
        TabStats.Controls.Add(rtbOutput)
        TabStats.Controls.Add(gbPeriod)
        TabStats.Controls.Add(btnCharts)
        TabStats.Controls.Add(btnReadGpxFiles)
        TabStats.Controls.Add(PictureBox2)
        resources.ApplyResources(TabStats, "TabStats")
        TabStats.Name = "TabStats"
        TabStats.UseVisualStyleBackColor = True
        ' 
        ' rtbOutput
        ' 
        resources.ApplyResources(rtbOutput, "rtbOutput")
        rtbOutput.BackColor = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        rtbOutput.Name = "rtbOutput"
        ' 
        ' gbPeriod
        ' 
        gbPeriod.BackColor = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        gbPeriod.Controls.Add(cmbTimeInterval)
        gbPeriod.Controls.Add(dtpEndDate)
        gbPeriod.Controls.Add(dtpStartDate)
        gbPeriod.FlatStyle = FlatStyle.Flat
        resources.ApplyResources(gbPeriod, "gbPeriod")
        gbPeriod.Name = "gbPeriod"
        gbPeriod.TabStop = False
        ' 
        ' cmbTimeInterval
        ' 
        cmbTimeInterval.BackColor = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        cmbTimeInterval.FormattingEnabled = True
        cmbTimeInterval.Items.AddRange(New Object() {resources.GetString("cmbTimeInterval.Items"), resources.GetString("cmbTimeInterval.Items1"), resources.GetString("cmbTimeInterval.Items2"), resources.GetString("cmbTimeInterval.Items3"), resources.GetString("cmbTimeInterval.Items4"), resources.GetString("cmbTimeInterval.Items5")})
        resources.ApplyResources(cmbTimeInterval, "cmbTimeInterval")
        cmbTimeInterval.Name = "cmbTimeInterval"
        ' 
        ' dtpEndDate
        ' 
        resources.ApplyResources(dtpEndDate, "dtpEndDate")
        dtpEndDate.CalendarMonthBackground = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        dtpEndDate.Format = DateTimePickerFormat.Custom
        dtpEndDate.Name = "dtpEndDate"
        ' 
        ' dtpStartDate
        ' 
        resources.ApplyResources(dtpStartDate, "dtpStartDate")
        dtpStartDate.CalendarMonthBackground = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        dtpStartDate.CalendarTitleBackColor = SystemColors.ActiveCaptionText
        dtpStartDate.Format = DateTimePickerFormat.Custom
        dtpStartDate.Name = "dtpStartDate"
        ' 
        ' btnCharts
        ' 
        btnCharts.BackColor = Color.DarkGoldenrod
        resources.ApplyResources(btnCharts, "btnCharts")
        btnCharts.Name = "btnCharts"
        btnCharts.UseVisualStyleBackColor = False
        ' 
        ' btnReadGpxFiles
        ' 
        btnReadGpxFiles.BackColor = Color.Salmon
        resources.ApplyResources(btnReadGpxFiles, "btnReadGpxFiles")
        btnReadGpxFiles.Name = "btnReadGpxFiles"
        btnReadGpxFiles.UseVisualStyleBackColor = False
        ' 
        ' PictureBox2
        ' 
        resources.ApplyResources(PictureBox2, "PictureBox2")
        PictureBox2.Name = "PictureBox2"
        PictureBox2.TabStop = False
        ' 
        ' TabVideoExport
        ' 
        TabVideoExport.BackColor = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        TabVideoExport.Controls.Add(btnCreateVideos)
        TabVideoExport.Controls.Add(lvGpxFiles)
        TabVideoExport.Controls.Add(PictureBox3)
        resources.ApplyResources(TabVideoExport, "TabVideoExport")
        TabVideoExport.Name = "TabVideoExport"
        TabVideoExport.UseVisualStyleBackColor = True
        ' 
        ' lvGpxFiles
        ' 
        resources.ApplyResources(lvGpxFiles, "lvGpxFiles")
        lvGpxFiles.BackColor = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        lvGpxFiles.CheckBoxes = True
        lvGpxFiles.Columns.AddRange(New ColumnHeader() {clmFileName, clmDate, clmLength, clmAge, clmTrkCount})
        lvGpxFiles.Name = "lvGpxFiles"
        lvGpxFiles.UseCompatibleStateImageBehavior = False
        lvGpxFiles.View = View.Details
        ' 
        ' clmFileName
        ' 
        resources.ApplyResources(clmFileName, "clmFileName")
        ' 
        ' clmDate
        ' 
        resources.ApplyResources(clmDate, "clmDate")
        ' 
        ' clmLength
        ' 
        resources.ApplyResources(clmLength, "clmLength")
        ' 
        ' clmAge
        ' 
        resources.ApplyResources(clmAge, "clmAge")
        ' 
        ' clmTrkCount
        ' 
        resources.ApplyResources(clmTrkCount, "clmTrkCount")
        ' 
        ' PictureBox3
        ' 
        resources.ApplyResources(PictureBox3, "PictureBox3")
        PictureBox3.Name = "PictureBox3"
        PictureBox3.TabStop = False
        ' 
        ' Form1
        ' 
        resources.ApplyResources(Me, "$this")
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(TabControl1)
        Controls.Add(rtbWarnings)
        Controls.Add(lblScentArtickle)
        Controls.Add(StatusStrip1)
        Controls.Add(MenuStrip1)
        Controls.Add(PictureBox1)
        MainMenuStrip = MenuStrip1
        Name = "Form1"
        StatusStrip1.ResumeLayout(False)
        StatusStrip1.PerformLayout()
        MenuStrip1.ResumeLayout(False)
        MenuStrip1.PerformLayout()
        CType(PictureBox1, ISupportInitialize).EndInit()
        TabControl1.ResumeLayout(False)
        TabStats.ResumeLayout(False)
        gbPeriod.ResumeLayout(False)
        CType(PictureBox2, ISupportInitialize).EndInit()
        TabVideoExport.ResumeLayout(False)
        TabVideoExport.PerformLayout()
        CType(PictureBox3, ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()

    End Sub


    Public Sub New()
        ' Toto volání je vyžadované návrhářem.
        InitializeComponent()
        ' Přidejte libovolnou inicializaci po volání InitializeComponent().

        'nastavuje logiku pro zobrazení názvu trasy v seznamu
        TrackTypeResolvers.LabelResolver = AddressOf ResolveLabel
        If My.Settings.WindowSize <> New Drawing.Size(0, 0) Then
            Me.Size = My.Settings.WindowSize
        End If
    End Sub


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        WriteRTBWarning("Logs:", Color.Maroon)


        mnuPrependDateToFileName.Checked = My.Settings.PrependDateToName
        mnuTrimGPSNoise.Checked = My.Settings.TrimGPSnoise

        'If My.Settings.Directory = "" Then
        '    My.Settings.Directory = IO.Directory.GetParent(Application.StartupPath).ToString
        'End If

        If My.Settings.BackupDirectory = "" Then
            My.Settings.BackupDirectory = System.IO.Path.Combine(My.Settings.Directory, "gpxFilesBackup")
        End If
        My.Settings.Save()
        CreateGpxFileManager()
        Me.cmbTimeInterval.SelectedIndex = 2 'last 365 days
        'Me.dtpEndDate.Value = Now
        'Me.dtpStartDate.Value = Me.dtpEndDate.Value.AddYears(-1)
        'Me.dtpStartDate.Value = Me.dtpStartDate.Value.AddDays(1)


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

        Me.btnReadGpxFiles.Focus()
        Me.btnReadGpxFiles.Select()
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
    Friend WithEvents lblScentArtickle As Label
    Friend WithEvents mnuTrimGPSNoise As ToolStripMenuItem
    Friend WithEvents mnuExportAs As ToolStripMenuItem
    Friend WithEvents mnuMergingTracks As ToolStripMenuItem
    Friend WithEvents SToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents mnuFactoryReset As ToolStripMenuItem
    Friend WithEvents rtbWarnings As RichTextBox
    Friend WithEvents mnuDogName As ToolStripMenuItem
    Friend WithEvents mnuProcessProcessed As ToolStripMenuItem
    Friend WithEvents mnuSelectADirectoryToSaveVideo As ToolStripMenuItem
    Friend WithEvents mnuSetFFmpegPath As ToolStripMenuItem
    Friend WithEvents mnuExit As ToolStripMenuItem
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents TabStats As TabPage
    Friend WithEvents TabVideoExport As TabPage
    Friend WithEvents rtbOutput As RichTextBox
    Friend WithEvents gbPeriod As GroupBox
    Friend WithEvents cmbTimeInterval As ComboBox
    Friend WithEvents dtpEndDate As DateTimePicker
    Friend WithEvents dtpStartDate As DateTimePicker
    Friend WithEvents btnCharts As Button
    Friend WithEvents btnReadGpxFiles As Button
    Friend WithEvents PictureBox2 As PictureBox
    Friend WithEvents lvGpxFiles As ListView
    Friend WithEvents PictureBox3 As PictureBox
    Friend WithEvents clmFileName As ColumnHeader
    Friend WithEvents clmDate As ColumnHeader
    Friend WithEvents clmLength As ColumnHeader
    Friend WithEvents clmTrkCount As ColumnHeader
    Private WithEvents btnCreateVideos As Button
    Friend WithEvents clmAge As ColumnHeader
    Friend WithEvents mnuAbout As ToolStripMenuItem
    Friend WithEvents mnuCheckUpdates As ToolStripMenuItem
End Class

