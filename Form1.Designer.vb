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
        components = New Container()
        Dim resources As ComponentResourceManager = New ComponentResourceManager(GetType(Form1))
        dtpStartDate = New DateTimePicker()
        dtpEndDate = New DateTimePicker()
        btnReadGpxFiles = New Button()
        btnCharts = New Button()
        ToolTip1 = New ToolTip(components)
        StatusStrip1 = New StatusStrip()
        StatusLabel1 = New ToolStripStatusLabel()
        MenuStrip1 = New MenuStrip()
        mnuFile = New ToolStripMenuItem()
        mnuSelect_directory_gpx_files = New ToolStripMenuItem()
        mnuSelectBackupDirectory = New ToolStripMenuItem()
        mnuSelectADirectoryToSaveVideo = New ToolStripMenuItem()
        mnuExportAs = New ToolStripMenuItem()
        mnuSettings = New ToolStripMenuItem()
        mnuPrependDateToFileName = New ToolStripMenuItem()
        mnuTrimGPSNoise = New ToolStripMenuItem()
        mnuMergingTracks = New ToolStripMenuItem()
        mnuProcessProcessed = New ToolStripMenuItem()
        mnuAskForVideo = New ToolStripMenuItem()
        mnuLanguage = New ToolStripMenuItem()
        mnuEnglish = New ToolStripMenuItem()
        mnuGerman = New ToolStripMenuItem()
        mnuCzech = New ToolStripMenuItem()
        mnuUkrainian = New ToolStripMenuItem()
        mnuPolish = New ToolStripMenuItem()
        mnuRussian = New ToolStripMenuItem()
        SToolStripMenuItem = New ToolStripMenuItem()
        FactoryResetToolStripMenuItem = New ToolStripMenuItem()
        mnuDogName = New ToolStripMenuItem()
        PictureBox1 = New PictureBox()
        gbPeriod = New GroupBox()
        lblScentArtickle = New Label()
        rtbOutput = New RichTextBox()
        rtbWarnings = New RichTextBox()
        StatusStrip1.SuspendLayout()
        MenuStrip1.SuspendLayout()
        CType(PictureBox1, ISupportInitialize).BeginInit()
        gbPeriod.SuspendLayout()
        SuspendLayout()
        ' 
        ' dtpStartDate
        ' 
        resources.ApplyResources(dtpStartDate, "dtpStartDate")
        dtpStartDate.CalendarMonthBackground = Color.FromArgb(CByte(255), CByte(255), CByte(192))
        dtpStartDate.Format = DateTimePickerFormat.Custom
        dtpStartDate.Name = "dtpStartDate"
        ' 
        ' dtpEndDate
        ' 
        resources.ApplyResources(dtpEndDate, "dtpEndDate")
        dtpEndDate.CalendarMonthBackground = Color.FromArgb(CByte(255), CByte(255), CByte(192))
        dtpEndDate.Format = DateTimePickerFormat.Custom
        dtpEndDate.Name = "dtpEndDate"
        ' 
        ' btnReadGpxFiles
        ' 
        btnReadGpxFiles.BackColor = Color.Salmon
        resources.ApplyResources(btnReadGpxFiles, "btnReadGpxFiles")
        btnReadGpxFiles.Name = "btnReadGpxFiles"
        btnReadGpxFiles.UseVisualStyleBackColor = False
        ' 
        ' btnCharts
        ' 
        btnCharts.BackColor = Color.DarkGoldenrod
        resources.ApplyResources(btnCharts, "btnCharts")
        btnCharts.Name = "btnCharts"
        btnCharts.UseVisualStyleBackColor = False
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
        ' MenuStrip1
        ' 
        MenuStrip1.BackColor = Color.FromArgb(CByte(172), CByte(209), CByte(158))
        MenuStrip1.ImageScalingSize = New Size(24, 24)
        MenuStrip1.Items.AddRange(New ToolStripItem() {mnuFile, mnuSettings, mnuLanguage, SToolStripMenuItem})
        resources.ApplyResources(MenuStrip1, "MenuStrip1")
        MenuStrip1.Name = "MenuStrip1"
        ' 
        ' mnuFile
        ' 
        mnuFile.DropDownItems.AddRange(New ToolStripItem() {mnuSelect_directory_gpx_files, mnuSelectBackupDirectory, mnuSelectADirectoryToSaveVideo, mnuExportAs})
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
        ' mnuSettings
        ' 
        mnuSettings.DropDownItems.AddRange(New ToolStripItem() {mnuPrependDateToFileName, mnuTrimGPSNoise, mnuMergingTracks, mnuProcessProcessed, mnuAskForVideo})
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
        ' mnuAskForVideo
        ' 
        mnuAskForVideo.CheckOnClick = True
        mnuAskForVideo.Name = "mnuAskForVideo"
        resources.ApplyResources(mnuAskForVideo, "mnuAskForVideo")
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
        SToolStripMenuItem.DropDownItems.AddRange(New ToolStripItem() {FactoryResetToolStripMenuItem, mnuDogName})
        resources.ApplyResources(SToolStripMenuItem, "SToolStripMenuItem")
        SToolStripMenuItem.Name = "SToolStripMenuItem"
        ' 
        ' FactoryResetToolStripMenuItem
        ' 
        FactoryResetToolStripMenuItem.Name = "FactoryResetToolStripMenuItem"
        resources.ApplyResources(FactoryResetToolStripMenuItem, "FactoryResetToolStripMenuItem")
        ' 
        ' mnuDogName
        ' 
        mnuDogName.Name = "mnuDogName"
        resources.ApplyResources(mnuDogName, "mnuDogName")
        ' 
        ' PictureBox1
        ' 
        resources.ApplyResources(PictureBox1, "PictureBox1")
        PictureBox1.Name = "PictureBox1"
        PictureBox1.TabStop = False
        ' 
        ' gbPeriod
        ' 
        gbPeriod.BackColor = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        gbPeriod.Controls.Add(dtpEndDate)
        gbPeriod.Controls.Add(dtpStartDate)
        gbPeriod.FlatStyle = FlatStyle.Flat
        resources.ApplyResources(gbPeriod, "gbPeriod")
        gbPeriod.Name = "gbPeriod"
        gbPeriod.TabStop = False
        ' 
        ' lblScentArtickle
        ' 
        resources.ApplyResources(lblScentArtickle, "lblScentArtickle")
        lblScentArtickle.BackColor = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        lblScentArtickle.Name = "lblScentArtickle"
        ' 
        ' rtbOutput
        ' 
        resources.ApplyResources(rtbOutput, "rtbOutput")
        rtbOutput.BackColor = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        rtbOutput.Name = "rtbOutput"
        ' 
        ' rtbWarnings
        ' 
        rtbWarnings.BackColor = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        resources.ApplyResources(rtbWarnings, "rtbWarnings")
        rtbWarnings.Name = "rtbWarnings"
        ' 
        ' Form1
        ' 
        resources.ApplyResources(Me, "$this")
        AutoScaleMode = AutoScaleMode.Font
        Controls.Add(rtbWarnings)
        Controls.Add(rtbOutput)
        Controls.Add(lblScentArtickle)
        Controls.Add(gbPeriod)
        Controls.Add(StatusStrip1)
        Controls.Add(btnCharts)
        Controls.Add(btnReadGpxFiles)
        Controls.Add(MenuStrip1)
        Controls.Add(PictureBox1)
        MainMenuStrip = MenuStrip1
        Name = "Form1"
        StatusStrip1.ResumeLayout(False)
        StatusStrip1.PerformLayout()
        MenuStrip1.ResumeLayout(False)
        MenuStrip1.PerformLayout()
        CType(PictureBox1, ISupportInitialize).EndInit()
        gbPeriod.ResumeLayout(False)
        ResumeLayout(False)
        PerformLayout()

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
    Friend WithEvents mnuSelectADirectoryToSaveVideo As ToolStripMenuItem
End Class

