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
        ToolTip1 = New ToolTip(components)
        StatusStrip1 = New StatusStrip()
        StatusLabel1 = New ToolStripStatusLabel()
        btnCreateVideos = New Button()
        MenuStrip1 = New MenuStrip()
        mnuFile = New ToolStripMenuItem()
        mnuSelect_directory_gpx_files = New ToolStripMenuItem()
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
        ToolStripMenuItem = New ToolStripMenuItem()
        mnucbActiveDog = New ToolStripComboBox()
        mnuRenameCurrentDog = New ToolStripMenuItem()
        mnuAddDog = New ToolStripMenuItem()
        mnuDeleteCurrentDog = New ToolStripMenuItem()
        mnuPointsForFind = New ToolStripTextBox()
        mnuPointsForSpeed = New ToolStripTextBox
        mnuPointsForAccuracy = New ToolStripTextBox
        mnuPointsForHandler = New ToolStripTextBox
        mnuSelectADirectoryToSaveVideo = New ToolStripMenuItem()
        mnuSetFFmpegPath = New ToolStripMenuItem()
        mnuFactoryReset = New ToolStripMenuItem()
        HelpToolStripMenuItem = New ToolStripMenuItem()
        mnuAbout = New ToolStripMenuItem()
        mnuCheckForUpdates1 = New ToolStripMenuItem()
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
        TabTrial = New TabPage()
        TabVideoExport = New TabPage()
        lvGpxFiles = New ListView()
        clmFileName = New ColumnHeader()
        clmDate = New ColumnHeader()
        clmLength = New ColumnHeader()
        clmAge = New ColumnHeader()
        clmTrkCount = New ColumnHeader()
        PictureBox3 = New PictureBox()
        ' Vytvoření instancí pro DataGridView a jeho sloupce
        ' Vytvoření instancí pro DataGridView a VŠECHNY jeho sloupce
        Me.dgvTrial = New DataGridView()
        Dim panelForDgv As New Panel()

        StatusStrip1.SuspendLayout()
        MenuStrip1.SuspendLayout()
        TabControl1.SuspendLayout()
        TabStats.SuspendLayout()
        gbPeriod.SuspendLayout()
        CType(PictureBox2, ISupportInitialize).BeginInit()
        TabTrial.SuspendLayout()
        TabVideoExport.SuspendLayout()
        CType(PictureBox3, ISupportInitialize).BeginInit()
        CType(Me.dgvTrial, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()

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
        MenuStrip1.Items.AddRange(New ToolStripItem() {mnuFile, mnuSettings, mnuLanguage, ToolStripMenuItem, HelpToolStripMenuItem})
        MenuStrip1.Name = "MenuStrip1"
        ' 
        ' mnuFile
        ' 
        mnuFile.DropDownItems.AddRange(New ToolStripItem() {mnuExportAs, mnuExit})
        resources.ApplyResources(mnuFile, "mnuFile")
        mnuFile.Name = "mnuFile"
        ' 
        ' mnuSelect_directory_gpx_files
        ' 
        mnuSelect_directory_gpx_files.Name = "mnuSelect_directory_gpx_files"
        resources.ApplyResources(mnuSelect_directory_gpx_files, "mnuSelect_directory_gpx_files")
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
        mnuPrependDateToFileName.Checked = True
        mnuPrependDateToFileName.CheckOnClick = True
        mnuPrependDateToFileName.CheckState = CheckState.Checked
        mnuPrependDateToFileName.Name = "mnuPrependDateToFileName"
        mnuPrependDateToFileName.Visible = False
        resources.ApplyResources(mnuPrependDateToFileName, "mnuPrependDateToFileName")
        ' 
        ' mnuTrimGPSNoise
        ' 
        mnuTrimGPSNoise.Checked = False
        mnuTrimGPSNoise.CheckOnClick = True
        mnuTrimGPSNoise.CheckState = CheckState.Checked
        mnuTrimGPSNoise.Name = "mnuTrimGPSNoise"
        mnuTrimGPSNoise.Visible = False
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
        ' ... nahraď původní lblPointsForFind a mnuPointsForFind v AddRange tímto:
        ToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {
                New ToolStripLabel With {.Text = "The category:", .AutoSize = True, .Font = New Font("Cascadia Code Semibold", 14, FontStyle.Bold)},
                mnucbActiveDog,
                mnuSelect_directory_gpx_files,
                mnuRenameCurrentDog,
                mnuAddDog,
                mnuDeleteCurrentDog,
                New ToolStripSeparator(),
                New ToolStripLabel With {.Text = "Adjust the points in this trial category:", .AutoSize = True, .Font = New Font("Cascadia Code Semibold", 14, FontStyle.Bold)},
                New ToolStripLabel With {.Text = "Points for finding the dealer:", .AutoSize = True},
                mnuPointsForFind,
                New ToolStripLabel With {.Text = "Points for speed:", .AutoSize = True},
                mnuPointsForSpeed,
                New ToolStripLabel With {.Text = "Points for accuracy:", .AutoSize = True},
                mnuPointsForAccuracy,
                New ToolStripLabel With {.Text = "Points for handler's work:", .AutoSize = True},
                mnuPointsForHandler,
                New ToolStripSeparator(),
                New ToolStripSeparator(),
                mnuSelectADirectoryToSaveVideo,
                mnuSetFFmpegPath,
                New ToolStripSeparator(),
                New ToolStripSeparator(),
                mnuFactoryReset
            })

        ' Přidání nového ToolStripTextBox pro zadávání čísel do ToolStripMenuItem

        Me.mnuPointsForFind.Name = "mnuPointsForFind"
        Me.mnuPointsForFind.ToolTipText = "Zadejte počet bodů za nalezení"
        Me.mnuPointsForFind.Width = 50
        Me.mnuPointsForFind.Text = "0" ' výchozí hodnota
        Me.mnuPointsForFind.TextBoxTextAlign = HorizontalAlignment.Right
        Me.mnuPointsForFind.MaxLength = 3 ' maximální délka vstupu

        Me.mnuPointsForSpeed.Name = "mnuPointsForSpeed"
        Me.mnuPointsForSpeed.ToolTipText = "Zadejte počet bodů za rychlost"
        Me.mnuPointsForSpeed.Width = 50
        Me.mnuPointsForSpeed.Text = "0" ' výchozí hodnota
        Me.mnuPointsForSpeed.TextBoxTextAlign = HorizontalAlignment.Right
        Me.mnuPointsForSpeed.MaxLength = 3 ' maximální délka vstupu

        Me.mnuPointsForAccuracy.Name = "mnuPointsForAccuracy"
        Me.mnuPointsForAccuracy.ToolTipText = "Zadejte počet bodů za přesnost"
        Me.mnuPointsForAccuracy.Width = 50
        Me.mnuPointsForAccuracy.Text = "0" ' výchozí hodnota
        Me.mnuPointsForAccuracy.TextBoxTextAlign = HorizontalAlignment.Right
        Me.mnuPointsForAccuracy.MaxLength = 3 ' maximální délka vstupu


        Me.mnuPointsForHandler.Name = "mnuPointsForHandler"
        Me.mnuPointsForHandler.ToolTipText = "Zadejte počet bodů za práci psovoda"
        Me.mnuPointsForHandler.Width = 50
        Me.mnuPointsForHandler.Text = "0" ' výchozí hodnota
        Me.mnuPointsForHandler.TextBoxTextAlign = HorizontalAlignment.Right
        Me.mnuPointsForHandler.MaxLength = 3 ' maximální délka vstupu
        '

        resources.ApplyResources(ToolStripMenuItem, "SToolStripMenuItem")
        ToolStripMenuItem.Name = "SToolStripMenuItem"
        ' 
        ' mnucbActiveDog
        ' 
        resources.ApplyResources(mnucbActiveDog, "mnucbActiveDog")
        mnucbActiveDog.Name = "mnucbActiveDog"
        ' 
        ' mnuRenameCurrentDog
        ' 
        mnuRenameCurrentDog.Name = "mnuRenameCurrentDog"
        resources.ApplyResources(mnuRenameCurrentDog, "mnuRenameCurrentDog")
        ' 
        ' mnuAddDog
        ' 
        mnuAddDog.Name = "mnuAddDog"
        resources.ApplyResources(mnuAddDog, "mnuAddDog")
        ' 
        ' mnuDeleteCurrentDog
        ' 
        mnuDeleteCurrentDog.Name = "mnuDeleteCurrentDog"
        resources.ApplyResources(mnuDeleteCurrentDog, "mnuDeleteCurrentDog")
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
        ' HelpToolStripMenuItem
        ' 
        HelpToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {mnuAbout, mnuCheckForUpdates1})
        HelpToolStripMenuItem.Name = "HelpToolStripMenuItem"
        resources.ApplyResources(HelpToolStripMenuItem, "HelpToolStripMenuItem")
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
        ' rtbWarnings
        ' 
        rtbWarnings.BackColor = Color.Beige
        resources.ApplyResources(rtbWarnings, "rtbWarnings")
        rtbWarnings.Name = "rtbWarnings"
        '
        ' TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabStats)
        Me.TabControl1.Controls.Add(Me.TabVideoExport)
        Me.TabControl1.Controls.Add(Me.TabTrial)
        resources.ApplyResources(Me.TabControl1, "TabControl1")
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        TabControl1.DrawMode = TabDrawMode.OwnerDrawFixed


        ' 
        ' TabStats
        ' 
        TabStats.BackColor = Color.Beige
        TabStats.Controls.Add(rtbOutput)
        TabStats.Controls.Add(gbPeriod)
        TabStats.Controls.Add(btnCharts)
        TabStats.Controls.Add(btnReadGpxFiles)
        TabStats.Controls.Add(PictureBox2)
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
        btnCharts.BackColor = Color.Goldenrod
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
        TabVideoExport.BackColor = Color.Beige
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
        lvGpxFiles.BackColor = Color.Beige
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
        ' dgvTracks (Hlavní nastavení DataGridView)
        '
        Me.dgvTrial.AllowUserToAddRows = False
        Me.dgvTrial.AllowUserToDeleteRows = False
        Me.dgvTrial.AutoGenerateColumns = True

        ' DŮLEŽITÉ: Toto nastavení zajistí, že se zobrazí horizontální posuvník
        Me.dgvTrial.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
        Me.dgvTrial.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvTrial.Anchor = (CType((((AnchorStyles.Top Or AnchorStyles.Bottom) _
    Or AnchorStyles.Left) _
    Or AnchorStyles.Right), AnchorStyles))
        Me.dgvTrial.Name = "dgvTracks"
        Me.dgvTrial.Size = New System.Drawing.Size(3000, 2000) 'musí být veliký aby se vytvořily posuvníky v Panelu
        Me.dgvTrial.ScrollBars = ScrollBars.None
        Me.dgvTrial.TabIndex = 0
        Me.dgvTrial.BackColor = Color.Beige
        Me.dgvTrial.DefaultCellStyle.BackColor = Color.Beige
        Me.dgvTrial.DefaultCellStyle.ForeColor = Color.Maroon
        Me.dgvTrial.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        'columnHeaders
        Me.dgvTrial.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True
        Me.dgvTrial.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
        Me.dgvTrial.EnableHeadersVisualStyles = False ' Důležité! Jinak se použije styl systému Windows
        Me.dgvTrial.ColumnHeadersDefaultCellStyle.BackColor = Color.Salmon ' Zvolte požadovanou barvu
        Me.dgvTrial.ColumnHeadersDefaultCellStyle.ForeColor = Color.Maroon ' (volitelné) barva textu
        Me.dgvTrial.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
        Me.dgvTrial.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvTrial.Location = New Point(0, 0)
        Me.dgvTrial.Anchor = AnchorStyles.Top Or AnchorStyles.Left

        ' =======================================================================
        ' Kód pro opravu posuvníku DataGridView (začátek)
        ' =======================================================================


        panelForDgv.Dock = DockStyle.Fill      ' Roztáhne se přes celou záložku.
        panelForDgv.AutoScroll = True         ' Zapne automatické posuvníky.
        panelForDgv.BackColor = Color.Transparent ' Aby nepřekryl barvu záložky


        ' 3. Nyní dgvTracks PŘIDÁME do našeho nového panelu.
        panelForDgv.Controls.Add(Me.dgvTrial)

        ' 4. A nakonec PŘIDÁME panel (který už v sobě obsahuje dgvTracks) na záložku.
        Me.TabTrial.Controls.Add(panelForDgv)

        ' 
        ' TabTrial
        ' 
        TabTrial.BackColor = Color.Beige
        TabTrial.Controls.Add(Me.panelProDgv)
        resources.ApplyResources(TabTrial, "TabTrial")
        TabTrial.Text = "Mantrailing Trial"
        TabTrial.Name = "TabTrial"
        TabTrial.UseVisualStyleBackColor = True


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
        CType(PictureBox2, ISupportInitialize).EndInit()
        CType(Me.dgvTrial, System.ComponentModel.ISupportInitialize).EndInit()
        TabVideoExport.ResumeLayout(False)
        TabVideoExport.PerformLayout()
        TabTrial.ResumeLayout(False)
        TabTrial.PerformLayout()
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
        'If My.Settings.WindowSize <> New Drawing.Size(0, 0) Then
        '    Me.Size = My.Settings.WindowSize
        'End If
    End Sub


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
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

        LoadDogs()
        LoadConfig()
        PopulateDogsToolStrip()

        WriteRTBWarning("Logs:", Color.Maroon)


        mnuPrependDateToFileName.Checked = True ' My.Settings.PrependDateToName
        'mnuTrimGPSNoise.Checked = My.Settings.TrimGPSnoise
        'mnucbActiveDog.SelectedItem = My.Settings.ActiveDog 'todo: načítat z json!


        CreateGpxFileManager()
        Me.cmbTimeInterval.SelectedIndex = 2 'last 365 days

        Me.StatusLabel1.Text = $"GPX files downloaded from: {ZkratCestu(ActiveDogInfo.RemoteDirectory, 130)}" & vbCrLf & $"Video exported to: {ZkratCestu(My.Settings.VideoDirectory, 130)}"
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

        mnuPointsForFind.Text = Me.ActiveDogInfo.PointsForFindMax
        mnuPointsForSpeed.Text = Me.ActiveDogInfo.PointsPer5KmhGrossSpeed
        mnuPointsForAccuracy.Text = Me.ActiveDogInfo.PointsForAccuracyMax
        mnuPointsForHandler.Text = Me.ActiveDogInfo.PointsForHandlerMax


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


    Private Sub mnuPointsForFind_LostFocus(sender As Object, e As Object) Handles mnuPointsForFind.LostFocus
        If Integer.TryParse(mnuPointsForFind.Text, Nothing) Then
            Me.ActiveDogInfo.PointsForFindMax = mnuPointsForFind.Text
        End If
        If Integer.TryParse(mnuPointsForSpeed.Text, Nothing) Then
            Me.ActiveDogInfo.PointsPer5KmhGrossSpeed = mnuPointsForSpeed.Text
        End If
        If Integer.TryParse(mnuPointsForAccuracy.Text, Nothing) Then
            Me.ActiveDogInfo.PointsForAccuracyMax = mnuPointsForAccuracy.Text
        End If
        If Integer.TryParse(mnuPointsForHandler.Text, Nothing) Then
            Me.ActiveDogInfo.PointsForHandlerMax = mnuPointsForHandler.Text
        End If

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
    Friend WithEvents mnuSettings As ToolStripMenuItem
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
    Friend WithEvents ToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents mnuFactoryReset As ToolStripMenuItem
    Friend WithEvents rtbWarnings As RichTextBox
    Friend WithEvents mnuAddDog As ToolStripMenuItem
    Friend WithEvents mnuProcessProcessed As ToolStripMenuItem
    Friend WithEvents mnuSelectADirectoryToSaveVideo As ToolStripMenuItem
    Friend WithEvents mnuSetFFmpegPath As ToolStripMenuItem
    Friend WithEvents mnuExit As ToolStripMenuItem
    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents TabStats As TabPage
    Friend WithEvents TabTrial As TabPage
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
    Friend WithEvents mnuCheckUpdates As ToolStripMenuItem
    Friend WithEvents HelpToolStripMenuItem As ToolStripMenuItem
    Friend WithEvents mnuAbout As ToolStripMenuItem
    Friend WithEvents mnuCheckForUpdates1 As ToolStripMenuItem
    Friend WithEvents mnucbActiveDog As ToolStripComboBox
    Friend WithEvents mnuDeleteCurrentDog As ToolStripMenuItem
    Friend WithEvents mnuRenameCurrentDog As ToolStripMenuItem
    Friend WithEvents dgvTrial As DataGridView
    ' 1. Vytvoříme nový Panel, který bude sloužit jako scroll-kontejner.
    Friend WithEvents panelProDgv As Panel
    Friend WithEvents mnuPointsForFind As ToolStripTextBox
    Friend WithEvents mnuPointsForSpeed As ToolStripTextBox
    Friend WithEvents mnuPointsForAccuracy As ToolStripTextBox
    Friend WithEvents mnuPointsForHandler As ToolStripTextBox

End Class

