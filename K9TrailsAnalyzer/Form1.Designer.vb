Imports System.ComponentModel
Imports System.IO
Imports System.Reflection
Imports System.Text.Json
Imports System.Threading
Imports TrackVideoExporter

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits Form
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
        Dim DataGridViewCellStyle1 As DataGridViewCellStyle = New DataGridViewCellStyle()
        Dim DataGridViewCellStyle2 As DataGridViewCellStyle = New DataGridViewCellStyle()
        ToolTip1 = New ToolTip(components)
        StatusStrip1 = New StatusStrip()
        StatusLabel1 = New ToolStripStatusLabel()
        btnCreateVideos = New Button()
        MenuStrip1 = New MenuStrip()
        mnuFile = New ToolStripMenuItem()
        mnuSelectADirectoryToSaveVideo = New ToolStripMenuItem()
        mnuExportAs = New ToolStripMenuItem()
        mnuExit = New ToolStripMenuItem()
        mnuGPXprocessing = New ToolStripMenuItem()
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
        mnuCategory = New ToolStripMenuItem()
        mnucbActiveCategory = New ToolStripComboBox()
        mnuSelect_directory_gpx_files = New ToolStripMenuItem()
        mnuRenameCurrentCategory = New ToolStripMenuItem()
        mnuAddNewCategory = New ToolStripMenuItem()
        mnuDeleteCurrentCategory = New ToolStripMenuItem()
        mnuPointInCompetition = New ToolStripMenuItem()
        mnuPointForFindText = New ToolStripMenuItem()
        mnuPointsForFind = New ToolStripTextBox()
        mnuPointsForSpeedText = New ToolStripMenuItem()
        mnuPointsForSpeed = New ToolStripTextBox()
        mnuPointsForAccuracyText = New ToolStripMenuItem()
        mnuPointsForAccuracy = New ToolStripTextBox()
        mnuDogReadingPointsText = New ToolStripMenuItem()
        mnuDogReadingPoints = New ToolStripTextBox()
        mnuHelp = New ToolStripMenuItem()
        mnuAbout = New ToolStripMenuItem()
        mnuCheckForUpdates1 = New ToolStripMenuItem()
        ToolStripSeparator1 = New ToolStripSeparator()
        mnuSetFFmpegPath = New ToolStripMenuItem()
        ToolStripSeparator2 = New ToolStripSeparator()
        mnuFactoryReset = New ToolStripMenuItem()
        mnuCheckUpdates = New ToolStripMenuItem()
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
        TabVideoExport = New TabPage()
        lvGpxFiles = New ListView()
        clmFileName = New ColumnHeader()
        clmDate = New ColumnHeader()
        clmLength = New ColumnHeader()
        clmAge = New ColumnHeader()
        clmTrkCount = New ColumnHeader()
        TabCompetition = New TabPage()
        panelForDgv = New Panel()
        dgvCompetition = New DataGridView()
        bsCompetitions = New BindingSource(components)
        StatusStrip1.SuspendLayout()
        MenuStrip1.SuspendLayout()
        TabControl1.SuspendLayout()
        TabStats.SuspendLayout()
        gbPeriod.SuspendLayout()
        TabVideoExport.SuspendLayout()
        TabCompetition.SuspendLayout()
        panelForDgv.SuspendLayout()
        CType(dgvCompetition, ISupportInitialize).BeginInit()
        CType(bsCompetitions, ISupportInitialize).BeginInit()
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
        MenuStrip1.BackColor = Color.DarkSeaGreen
        resources.ApplyResources(MenuStrip1, "MenuStrip1")
        MenuStrip1.ImageScalingSize = New Size(24, 24)
        MenuStrip1.Items.AddRange(New ToolStripItem() {mnuFile, mnuGPXprocessing, mnuLanguage, mnuCategory, mnuHelp})
        MenuStrip1.Name = "MenuStrip1"
        ' 
        ' mnuFile
        ' 
        mnuFile.DropDownItems.AddRange(New ToolStripItem() {mnuSelectADirectoryToSaveVideo, mnuExportAs, mnuExit})
        resources.ApplyResources(mnuFile, "mnuFile")
        mnuFile.Name = "mnuFile"
        ' 
        ' mnuSelectADirectoryToSaveVideo
        ' 
        mnuSelectADirectoryToSaveVideo.Name = "mnuSelectADirectoryToSaveVideo"
        resources.ApplyResources(mnuSelectADirectoryToSaveVideo, "mnuSelectADirectoryToSaveVideo")
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
        ' mnuGPXprocessing
        ' 
        mnuGPXprocessing.DropDownItems.AddRange(New ToolStripItem() {mnuPrependDateToFileName, mnuTrimGPSNoise, mnuMergingTracks, mnuProcessProcessed})
        resources.ApplyResources(mnuGPXprocessing, "mnuGPXprocessing")
        mnuGPXprocessing.Name = "mnuGPXprocessing"
        ' 
        ' mnuPrependDateToFileName
        ' 
        mnuPrependDateToFileName.Checked = True
        mnuPrependDateToFileName.CheckOnClick = True
        mnuPrependDateToFileName.CheckState = CheckState.Checked
        mnuPrependDateToFileName.Name = "mnuPrependDateToFileName"
        resources.ApplyResources(mnuPrependDateToFileName, "mnuPrependDateToFileName")
        ' 
        ' mnuTrimGPSNoise
        ' 
        mnuTrimGPSNoise.Checked = True
        mnuTrimGPSNoise.CheckOnClick = True
        mnuTrimGPSNoise.CheckState = CheckState.Checked
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
        mnuEnglish.Name = "mnuEnglish"
        resources.ApplyResources(mnuEnglish, "mnuEnglish")
        mnuEnglish.Tag = "en"
        ' 
        ' mnuGerman
        ' 
        mnuGerman.Name = "mnuGerman"
        resources.ApplyResources(mnuGerman, "mnuGerman")
        mnuGerman.Tag = "de"
        ' 
        ' mnuCzech
        ' 
        mnuCzech.Name = "mnuCzech"
        resources.ApplyResources(mnuCzech, "mnuCzech")
        mnuCzech.Tag = "cs"
        ' 
        ' mnuUkrainian
        ' 
        mnuUkrainian.Name = "mnuUkrainian"
        resources.ApplyResources(mnuUkrainian, "mnuUkrainian")
        mnuUkrainian.Tag = "uk"
        ' 
        ' mnuPolish
        ' 
        mnuPolish.Name = "mnuPolish"
        resources.ApplyResources(mnuPolish, "mnuPolish")
        mnuPolish.Tag = "pl"
        ' 
        ' mnuRussian
        ' 
        mnuRussian.Name = "mnuRussian"
        resources.ApplyResources(mnuRussian, "mnuRussian")
        mnuRussian.Tag = "ru"
        ' 
        ' mnuCategory
        ' 
        mnuCategory.DropDownItems.AddRange(New ToolStripItem() {mnucbActiveCategory, mnuSelect_directory_gpx_files, mnuRenameCurrentCategory, mnuAddNewCategory, mnuDeleteCurrentCategory, mnuPointInCompetition})
        resources.ApplyResources(mnuCategory, "mnuCategory")
        mnuCategory.Name = "mnuCategory"
        ' 
        ' mnucbActiveCategory
        ' 
        resources.ApplyResources(mnucbActiveCategory, "mnucbActiveCategory")
        mnucbActiveCategory.Name = "mnucbActiveCategory"
        ' 
        ' mnuSelect_directory_gpx_files
        ' 
        mnuSelect_directory_gpx_files.Name = "mnuSelect_directory_gpx_files"
        resources.ApplyResources(mnuSelect_directory_gpx_files, "mnuSelect_directory_gpx_files")
        ' 
        ' mnuRenameCurrentCategory
        ' 
        mnuRenameCurrentCategory.Name = "mnuRenameCurrentCategory"
        resources.ApplyResources(mnuRenameCurrentCategory, "mnuRenameCurrentCategory")
        ' 
        ' mnuAddNewCategory
        ' 
        mnuAddNewCategory.Name = "mnuAddNewCategory"
        resources.ApplyResources(mnuAddNewCategory, "mnuAddNewCategory")
        ' 
        ' mnuDeleteCurrentCategory
        ' 
        mnuDeleteCurrentCategory.Name = "mnuDeleteCurrentCategory"
        resources.ApplyResources(mnuDeleteCurrentCategory, "mnuDeleteCurrentCategory")
        ' 
        ' mnuPointInCompetition
        ' 
        mnuPointInCompetition.DropDownItems.AddRange(New ToolStripItem() {mnuPointForFindText, mnuPointsForSpeedText, mnuPointsForAccuracyText, mnuDogReadingPointsText})
        mnuPointInCompetition.Name = "mnuPointInCompetition"
        resources.ApplyResources(mnuPointInCompetition, "mnuPointInCompetition")
        ' 
        ' mnuPointForFindText
        ' 
        mnuPointForFindText.DropDownItems.AddRange(New ToolStripItem() {mnuPointsForFind})
        mnuPointForFindText.Name = "mnuPointForFindText"
        resources.ApplyResources(mnuPointForFindText, "mnuPointForFindText")
        ' 
        ' mnuPointsForFind
        ' 
        resources.ApplyResources(mnuPointsForFind, "mnuPointsForFind")
        mnuPointsForFind.Name = "mnuPointsForFind"
        ' 
        ' mnuPointsForSpeedText
        ' 
        mnuPointsForSpeedText.DropDownItems.AddRange(New ToolStripItem() {mnuPointsForSpeed})
        mnuPointsForSpeedText.Name = "mnuPointsForSpeedText"
        resources.ApplyResources(mnuPointsForSpeedText, "mnuPointsForSpeedText")
        ' 
        ' mnuPointsForSpeed
        ' 
        resources.ApplyResources(mnuPointsForSpeed, "mnuPointsForSpeed")
        mnuPointsForSpeed.Name = "mnuPointsForSpeed"
        ' 
        ' mnuPointsForAccuracyText
        ' 
        mnuPointsForAccuracyText.DropDownItems.AddRange(New ToolStripItem() {mnuPointsForAccuracy})
        mnuPointsForAccuracyText.Name = "mnuPointsForAccuracyText"
        resources.ApplyResources(mnuPointsForAccuracyText, "mnuPointsForAccuracyText")
        ' 
        ' mnuPointsForAccuracy
        ' 
        resources.ApplyResources(mnuPointsForAccuracy, "mnuPointsForAccuracy")
        mnuPointsForAccuracy.Name = "mnuPointsForAccuracy"
        ' 
        ' mnuDogReadingPointsText
        ' 
        mnuDogReadingPointsText.DropDownItems.AddRange(New ToolStripItem() {mnuDogReadingPoints})
        mnuDogReadingPointsText.Name = "mnuDogReadingPointsText"
        resources.ApplyResources(mnuDogReadingPointsText, "mnuDogReadingPointsText")
        ' 
        ' mnuDogReadingPoints
        ' 
        resources.ApplyResources(mnuDogReadingPoints, "mnuDogReadingPoints")
        mnuDogReadingPoints.Name = "mnuDogReadingPoints"
        ' 
        ' mnuHelp
        ' 
        mnuHelp.DropDownItems.AddRange(New ToolStripItem() {mnuAbout, mnuCheckForUpdates1, ToolStripSeparator1, mnuSetFFmpegPath, ToolStripSeparator2, mnuFactoryReset})
        mnuHelp.Name = "mnuHelp"
        resources.ApplyResources(mnuHelp, "mnuHelp")
        ' 
        ' mnuAbout
        ' 
        mnuAbout.Name = "mnuAbout"
        resources.ApplyResources(mnuAbout, "mnuAbout")
        ' 
        ' mnuCheckForUpdates1
        ' 
        mnuCheckForUpdates1.Name = "mnuCheckForUpdates1"
        resources.ApplyResources(mnuCheckForUpdates1, "mnuCheckForUpdates1")
        ' 
        ' ToolStripSeparator1
        ' 
        ToolStripSeparator1.Name = "ToolStripSeparator1"
        resources.ApplyResources(ToolStripSeparator1, "ToolStripSeparator1")
        ' 
        ' mnuSetFFmpegPath
        ' 
        mnuSetFFmpegPath.Name = "mnuSetFFmpegPath"
        resources.ApplyResources(mnuSetFFmpegPath, "mnuSetFFmpegPath")
        ' 
        ' ToolStripSeparator2
        ' 
        ToolStripSeparator2.Name = "ToolStripSeparator2"
        resources.ApplyResources(ToolStripSeparator2, "ToolStripSeparator2")
        ' 
        ' mnuFactoryReset
        ' 
        mnuFactoryReset.Name = "mnuFactoryReset"
        resources.ApplyResources(mnuFactoryReset, "mnuFactoryReset")
        ' 
        ' mnuCheckUpdates
        ' 
        mnuCheckUpdates.Name = "mnuCheckUpdates"
        resources.ApplyResources(mnuCheckUpdates, "mnuCheckUpdates")
        ' 
        ' rtbWarnings
        ' 
        rtbWarnings.BackColor = Color.Beige
        resources.ApplyResources(rtbWarnings, "rtbWarnings")
        rtbWarnings.Name = "rtbWarnings"
        ' 
        ' TabControl1
        ' 
        TabControl1.Controls.Add(TabStats)
        TabControl1.Controls.Add(TabVideoExport)
        TabControl1.Controls.Add(TabCompetition)
        resources.ApplyResources(TabControl1, "TabControl1")
        TabControl1.DrawMode = TabDrawMode.OwnerDrawFixed
        TabControl1.Name = "TabControl1"
        TabControl1.SelectedIndex = 0
        ' 
        ' TabStats
        ' 
        TabStats.BackColor = Color.Goldenrod
        TabStats.Controls.Add(rtbOutput)
        TabStats.Controls.Add(gbPeriod)
        TabStats.Controls.Add(btnCharts)
        TabStats.Controls.Add(btnReadGpxFiles)
        resources.ApplyResources(TabStats, "TabStats")
        TabStats.Name = "TabStats"
        ' 
        ' rtbOutput
        ' 
        resources.ApplyResources(rtbOutput, "rtbOutput")
        rtbOutput.BackColor = Color.Beige
        rtbOutput.Name = "rtbOutput"
        ' 
        ' gbPeriod
        ' 
        gbPeriod.BackColor = Color.Beige
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
        cmbTimeInterval.BackColor = Color.Beige
        cmbTimeInterval.FormattingEnabled = True
        cmbTimeInterval.Items.AddRange(New Object() {resources.GetString("cmbTimeInterval.Items"), resources.GetString("cmbTimeInterval.Items1"), resources.GetString("cmbTimeInterval.Items2"), resources.GetString("cmbTimeInterval.Items3"), resources.GetString("cmbTimeInterval.Items4"), resources.GetString("cmbTimeInterval.Items5")})
        resources.ApplyResources(cmbTimeInterval, "cmbTimeInterval")
        cmbTimeInterval.Name = "cmbTimeInterval"
        ' 
        ' dtpEndDate
        ' 
        resources.ApplyResources(dtpEndDate, "dtpEndDate")
        dtpEndDate.CalendarMonthBackground = Color.Beige
        dtpEndDate.Format = DateTimePickerFormat.Custom
        dtpEndDate.Name = "dtpEndDate"
        ' 
        ' dtpStartDate
        ' 
        resources.ApplyResources(dtpStartDate, "dtpStartDate")
        dtpStartDate.CalendarMonthBackground = Color.Beige
        dtpStartDate.CalendarTitleBackColor = SystemColors.ActiveCaptionText
        dtpStartDate.Format = DateTimePickerFormat.Custom
        dtpStartDate.Name = "dtpStartDate"
        ' 
        ' btnCharts
        ' 
        btnCharts.BackColor = Color.DarkSeaGreen
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
        ' TabVideoExport
        ' 
        TabVideoExport.BackColor = Color.Beige
        TabVideoExport.Controls.Add(btnCreateVideos)
        TabVideoExport.Controls.Add(lvGpxFiles)
        resources.ApplyResources(TabVideoExport, "TabVideoExport")
        TabVideoExport.Name = "TabVideoExport"
        TabVideoExport.UseVisualStyleBackColor = True
        ' 
        ' lvGpxFiles
        ' 
        resources.ApplyResources(lvGpxFiles, "lvGpxFiles")
        lvGpxFiles.BackColor = Color.DarkSeaGreen
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
        ' TabCompetition
        ' 
        TabCompetition.BackColor = Color.Salmon
        TabCompetition.Controls.Add(panelForDgv)
        resources.ApplyResources(TabCompetition, "TabCompetition")
        TabCompetition.Name = "TabCompetition"
        ' 
        ' panelForDgv
        ' 
        resources.ApplyResources(panelForDgv, "panelForDgv")
        panelForDgv.BackColor = Color.Transparent
        panelForDgv.Controls.Add(dgvCompetition)
        panelForDgv.Name = "panelForDgv"
        ' 
        ' dgvCompetition
        ' 
        dgvCompetition.AllowUserToAddRows = False
        dgvCompetition.AllowUserToDeleteRows = False
        dgvCompetition.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
        dgvCompetition.BackgroundColor = Color.Salmon
        DataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleCenter
        DataGridViewCellStyle1.BackColor = Color.Salmon
        DataGridViewCellStyle1.Font = New Font("Cascadia Code", 12F)
        DataGridViewCellStyle1.ForeColor = Color.Maroon
        DataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle1.WrapMode = DataGridViewTriState.True
        dgvCompetition.ColumnHeadersDefaultCellStyle = DataGridViewCellStyle1
        dgvCompetition.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        DataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleRight
        DataGridViewCellStyle2.BackColor = Color.Beige
        DataGridViewCellStyle2.Font = New Font("Cascadia Code", 12F)
        DataGridViewCellStyle2.ForeColor = Color.Maroon
        DataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight
        DataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText
        DataGridViewCellStyle2.WrapMode = DataGridViewTriState.False
        dgvCompetition.DefaultCellStyle = DataGridViewCellStyle2
        dgvCompetition.EnableHeadersVisualStyles = False
        resources.ApplyResources(dgvCompetition, "dgvCompetition")
        dgvCompetition.Name = "dgvCompetition"
        ' 
        ' Form1
        ' 
        resources.ApplyResources(Me, "$this")
        AutoScaleMode = AutoScaleMode.Dpi
        Controls.Add(TabControl1)
        Controls.Add(rtbWarnings)
        Controls.Add(StatusStrip1)
        Controls.Add(MenuStrip1)
        MainMenuStrip = MenuStrip1
        Name = "Form1"
        StatusStrip1.ResumeLayout(False)
        StatusStrip1.PerformLayout()
        MenuStrip1.ResumeLayout(False)
        MenuStrip1.PerformLayout()
        TabControl1.ResumeLayout(False)
        TabStats.ResumeLayout(False)
        gbPeriod.ResumeLayout(False)
        TabVideoExport.ResumeLayout(False)
        TabVideoExport.PerformLayout()
        TabCompetition.ResumeLayout(False)
        panelForDgv.ResumeLayout(False)
        CType(dgvCompetition, ISupportInitialize).EndInit()
        CType(bsCompetitions, ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()

    End Sub


    Public Sub New()
        ' Toto volání je vyžadované návrhářem.
        InitializeComponent()
        ' Přidejte libovolnou inicializaci po volání InitializeComponent().

        'nastavuje logiku pro zobrazení názvu trasy v seznamu
        'todo: používá se tohle vůbec??:
        TrackTypeResolvers.LabelResolver = AddressOf ResolveLabel

    End Sub


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ' Nastavení obrázků na ToolStripMenuItem
        Dim imgBasePath As String = Path.Combine(Application.StartupPath, "Resources", "Images")
        Me.mnuCzech.Image = Image.FromFile(Path.Combine(imgBasePath, "cs-flag.png"))
        Me.mnuEnglish.Image = Image.FromFile(Path.Combine(imgBasePath, "en-flag.png"))
        Me.mnuGerman.Image = Image.FromFile(Path.Combine(imgBasePath, "de-flag.png"))
        Me.mnuPolish.Image = Image.FromFile(Path.Combine(imgBasePath, "pl-flag.png"))
        Me.mnuRussian.Image = Image.FromFile(Path.Combine(imgBasePath, "ru-flag.png"))
        Me.mnuUkrainian.Image = Image.FromFile(Path.Combine(imgBasePath, "uk-flag.png"))

        For Each item As ToolStripMenuItem In {mnuCzech, mnuEnglish, mnuGerman, mnuPolish, mnuRussian, mnuUkrainian}
            If item.Image IsNot Nothing Then
                item.ImageScaling = ToolStripItemImageScaling.None
            End If
        Next
        Me.mnuLanguage.ImageScaling = ToolStripItemImageScaling.None

        Me.Icon = New Icon((Path.Combine(imgBasePath, "icon.ico")))
        ' načteme data + config + naplníme combobox
        'při prvním spuštění se načtou data z defaults:
        Dim defaultDir As String = Path.Combine(Application.StartupPath, "defaults")
        Dim appdataDir = Path.Combine(Application.StartupPath, "AppData")
        Directory.CreateDirectory(appdataDir)
        For Each file In Directory.GetFiles(defaultDir)
            Dim appdataPath = Path.Combine(Application.StartupPath, "AppData", Path.GetFileName(file))
            If IO.Path.Exists(appdataPath) Then
                'pókud už existuje uživatelské nastavení, tak se default hodnotami nepřepisuje!
                Continue For
            End If
            IO.File.Move(file, appdataPath) 'pokud tam už něco je nepřepisuje se!
        Next
        LoadUnifiedConfig()
        'LoadCategories()
        'LoadConfig()
        PopulateCategoriesToolStrip()

        WriteRTBWarning("Logs:", Color.Maroon)


        mnuPrependDateToFileName.Checked = True ' My.Settings.PrependDateToName
        'mnuTrimGPSNoise.Checked = My.Settings.TrimGPSnoise

        CreateGpxFileManager()
        Me.cmbTimeInterval.SelectedIndex = 2 'last 365 days

        Me.StatusLabel1.Text = $"GPX files downloaded from: {ZkratCestu(ActiveCategoryInfo.RemoteDirectory, 130)}" & vbCrLf & $"Video exported to: {ZkratCestu(My.Settings.VideoDirectory, 130)}"
        Dim resources = New ComponentResourceManager(Me.GetType())
        LocalizeMenuItems(MenuStrip1.Items, resources)
        SetTooltips()


        mnuPointsForFind.Text = Me.ActiveCategoryInfo.PointsForFindMax
        mnuPointsForSpeed.Text = Me.ActiveCategoryInfo.PointsPerKmhGrossSpeed
        mnuPointsForAccuracy.Text = Me.ActiveCategoryInfo.PointsForAccuracyMax
        mnuDogReadingPoints.Text = Me.ActiveCategoryInfo.PointsForDogReadingMax


        ReadHelp()

        ' Nastavení fontu a barvy textu
        Me.rtbOutput.SelectionStart = Me.rtbOutput.Text.Length ' Pozice na konec textu

        Dim thisAssem As Assembly = GetType(Form1).Assembly
        Dim thisAssemName As AssemblyName = thisAssem.GetName()
        'verze se nastaví v AssamblyInfo.vb nebo v My Project -> Aplikace -> Informace o sestavení!!!!!!!!!!!!!!!!
        Me.Text = thisAssemName.Name & "   " & thisAssemName.Version.ToString

        Me.AcceptButton = Me.btnReadGpxFiles
        'Application.DoEvents()
        Me.AutoScrollPosition = New Point(0, 0)

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


    Private Sub mnuPointsForFind_LostFocus(sender As Object, e As Object) Handles mnuPointsForFind.LostFocus, mnuDogReadingPoints.LostFocus, mnuPointsForSpeed.LostFocus, mnuPointsForAccuracy.LostFocus
        If Integer.TryParse(mnuPointsForFind.Text, Nothing) Then
            Me.ActiveCategoryInfo.PointsForFindMax = mnuPointsForFind.Text
        End If
        If Integer.TryParse(mnuPointsForSpeed.Text, Nothing) Then
            Me.ActiveCategoryInfo.PointsPerKmhGrossSpeed = mnuPointsForSpeed.Text
        End If
        If Integer.TryParse(mnuPointsForAccuracy.Text, Nothing) Then
            Me.ActiveCategoryInfo.PointsForAccuracyMax = mnuPointsForAccuracy.Text
        End If
        If Integer.TryParse(mnuDogReadingPoints.Text, Nothing) Then
            Me.ActiveCategoryInfo.PointsForDogReadingMax = mnuDogReadingPoints.Text
        End If
        'aktualizace názvu menu
        'lokalizovaný text bez hodnot v závorce:
        Dim baseText As String = CStr(Me.mnuPointInCompetition.Tag)

        Me.mnuPointInCompetition.Text = String.Format("{0} ({1}, {2}, {3}, {4})",
    baseText,
    Me.ActiveCategoryInfo.PointsForFindMax,
    Me.ActiveCategoryInfo.PointsPerKmhGrossSpeed,
    Me.ActiveCategoryInfo.PointsForAccuracyMax,
    Me.ActiveCategoryInfo.PointsForDogReadingMax)

    End Sub



    Private Sub mnuPointsForFind_KeyPress(sender As Object, e As KeyPressEventArgs) Handles mnuPointsForFind.KeyPress
        ' Pokud chceš, můžeš nastavit validaci vstupu pouze na čísla:

        If Not Char.IsControl(e.KeyChar) AndAlso Not Char.IsDigit(e.KeyChar) Then
            e.Handled = True

        End If
    End Sub



    Friend WithEvents ToolTip1 As ToolTip
    Friend WithEvents MenuStrip1 As MenuStrip
    Friend WithEvents mnuFile As ToolStripMenuItem
    Friend WithEvents mnuSelect_directory_gpx_files As ToolStripMenuItem
    Friend WithEvents StatusStrip1 As StatusStrip
    Friend WithEvents StatusLabel1 As ToolStripStatusLabel
    Friend WithEvents mnuGPXprocessing As ToolStripMenuItem
    Friend WithEvents mnuPrependDateToFileName As ToolStripMenuItem
    Friend WithEvents mnuLanguage As ToolStripMenuItem
    Friend WithEvents mnuCzech As ToolStripMenuItem
    Friend WithEvents mnuUkrainian As ToolStripMenuItem
    Friend WithEvents mnuEnglish As ToolStripMenuItem
    Friend WithEvents mnuGerman As ToolStripMenuItem
    Friend WithEvents mnuRussian As ToolStripMenuItem
    Friend WithEvents mnuPolish As ToolStripMenuItem
    Friend WithEvents mnuTrimGPSNoise As ToolStripMenuItem
    Friend WithEvents mnuExportAs As ToolStripMenuItem
    Friend WithEvents mnuMergingTracks As ToolStripMenuItem
    Friend WithEvents mnuCategory As ToolStripMenuItem
    Friend WithEvents mnuFactoryReset As ToolStripMenuItem
    Friend WithEvents rtbWarnings As RichTextBox
    Friend WithEvents mnuAddNewCategory As ToolStripMenuItem
    Friend WithEvents mnuProcessProcessed As ToolStripMenuItem
    Friend WithEvents mnuSelectADirectoryToSaveVideo As ToolStripMenuItem
    Friend WithEvents mnuSetFFmpegPath As ToolStripMenuItem
    Friend WithEvents mnuExit As ToolStripMenuItem
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents TabStats As TabPage
    Friend WithEvents TabCompetition As TabPage
    Friend WithEvents TabVideoExport As TabPage
    Friend WithEvents rtbOutput As RichTextBox
    Friend WithEvents gbPeriod As GroupBox
    Friend WithEvents cmbTimeInterval As ComboBox
    Friend WithEvents dtpEndDate As DateTimePicker
    Friend WithEvents dtpStartDate As DateTimePicker
    Friend WithEvents btnCharts As Button
    Friend WithEvents btnReadGpxFiles As Button
    Friend WithEvents lvGpxFiles As ListView
    Friend WithEvents clmFileName As ColumnHeader
    Friend WithEvents clmDate As ColumnHeader
    Friend WithEvents clmLength As ColumnHeader
    Friend WithEvents clmTrkCount As ColumnHeader
    Private WithEvents btnCreateVideos As Button
    Friend WithEvents clmAge As ColumnHeader
    Friend WithEvents mnuCheckUpdates As ToolStripMenuItem
    Friend WithEvents mnuHelp As ToolStripMenuItem
    Friend WithEvents mnuAbout As ToolStripMenuItem
    Friend WithEvents mnuCheckForUpdates1 As ToolStripMenuItem
    Friend WithEvents mnucbActiveCategory As ToolStripComboBox
    Friend WithEvents mnuDeleteCurrentCategory As ToolStripMenuItem
    Friend WithEvents mnuRenameCurrentCategory As ToolStripMenuItem
    Friend WithEvents dgvCompetition As DataGridView
    Private WithEvents bsCompetitions As BindingSource
    ' 1. Vytvoříme nový Panel, který bude sloužit jako scroll-kontejner.
    Friend WithEvents panelForDgv As Panel
    Friend WithEvents mnuPointsForFind As ToolStripTextBox
    Friend WithEvents mnuPointsForSpeed As ToolStripTextBox
    Friend WithEvents mnuPointsForAccuracy As ToolStripTextBox
    Friend WithEvents mnuDogReadingPoints As ToolStripTextBox
    Friend WithEvents mnuPointInCompetition As ToolStripMenuItem
    Friend WithEvents mnuPointForFindText As ToolStripMenuItem
    Friend WithEvents ToolStripSeparator1 As ToolStripSeparator
    Friend WithEvents ToolStripSeparator2 As ToolStripSeparator
    Friend WithEvents mnuPointsForSpeedText As ToolStripMenuItem
    Friend WithEvents mnuPointsForAccuracyText As ToolStripMenuItem
    Friend WithEvents mnuDogReadingPointsText As ToolStripMenuItem


End Class

