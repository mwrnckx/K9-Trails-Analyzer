﻿'------------------------------------------------------------------------------
' <auto-generated>
'     Tento kód byl generován nástrojem.
'     Verze modulu runtime:4.0.30319.42000
'
'     Změny tohoto souboru mohou způsobit nesprávné chování a budou ztraceny,
'     dojde-li k novému generování kódu.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    'Tato třída byla automaticky generována třídou StronglyTypedResourceBuilder
    'pomocí nástroje podobného aplikaci ResGen nebo Visual Studio.
    'Chcete-li přidat nebo odebrat člena, upravte souboru .ResX a pak znovu spusťte aplikaci ResGen
    's parametrem /str nebo znovu sestavte projekt aplikace Visual Studio.
    '''<summary>
    '''  Třída prostředků se silnými typy pro vyhledávání lokalizovaných řetězců atp.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute()>  _
    Friend Class Resource1
        
        Private Shared resourceMan As Global.System.Resources.ResourceManager
        
        Private Shared resourceCulture As Global.System.Globalization.CultureInfo
        
        <Global.System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")>  _
        Friend Sub New()
            MyBase.New
        End Sub
        
        '''<summary>
        '''  Vrací instanci ResourceManager uloženou v mezipaměti použitou touto třídou.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Shared ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("GPXTrailAnalyzer.Resource1", GetType(Resource1).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Potlačí vlastnost CurrentUICulture aktuálního vlákna pro všechna
        '''  vyhledání prostředků pomocí třídy prostředků se silnými typy.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Shared Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Article.
        '''</summary>
        Friend Shared ReadOnly Property article() As String
            Get
                Return ResourceManager.GetString("article", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Remember the &apos;No&apos; decision for this couple.
        '''</summary>
        Friend Shared ReadOnly Property chbRemembDecisQ() As String
            Get
                Return ResourceManager.GetString("chbRemembDecisQ", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný From: .
        '''</summary>
        Friend Shared ReadOnly Property lblFrom() As String
            Get
                Return ResourceManager.GetString("lblFrom", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný First file.
        '''</summary>
        Friend Shared ReadOnly Property lblIsThisLayerQ() As String
            Get
                Return ResourceManager.GetString("lblIsThisLayerQ", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Second file.
        '''</summary>
        Friend Shared ReadOnly Property lblIsThisTrackOfTheDog() As String
            Get
                Return ResourceManager.GetString("lblIsThisTrackOfTheDog", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Merge GPX files?.
        '''</summary>
        Friend Shared ReadOnly Property lblMergeGPXtracksQ() As String
            Get
                Return ResourceManager.GetString("lblMergeGPXtracksQ", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Decide whether these two files are related as the track-layer and dog track..
        '''</summary>
        Friend Shared ReadOnly Property lblMergeTwoToOneQ() As String
            Get
                Return ResourceManager.GetString("lblMergeTwoToOneQ", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný By merging them, you will get a single gpx file that contains the tracks of both the trail-layer and the dog..
        '''</summary>
        Friend Shared ReadOnly Property lblMergingYouGet() As String
            Get
                Return ResourceManager.GetString("lblMergingYouGet", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný To: .
        '''</summary>
        Friend Shared ReadOnly Property lblTo() As String
            Get
                Return ResourceManager.GetString("lblTo", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný The gpx files were successfully backed up to the directory:.
        '''</summary>
        Friend Shared ReadOnly Property logBackupOfFiles() As String
            Get
                Return ResourceManager.GetString("logBackupOfFiles", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Data Retrieval Failed.
        '''</summary>
        Friend Shared ReadOnly Property mBoxDataRetrievalFailed() As String
            Get
                Return ResourceManager.GetString("mBoxDataRetrievalFailed", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Error Creating CSV.
        '''</summary>
        Friend Shared ReadOnly Property mBoxErrorCreatingCSV() As String
            Get
                Return ResourceManager.GetString("mBoxErrorCreatingCSV", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Set conditions for merging tracks.
        '''</summary>
        Friend Shared ReadOnly Property mBoxMergingTracksText() As String
            Get
                Return ResourceManager.GetString("mBoxMergingTracksText", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný In order to save data read from gpx files, you must first load the gpx files. Use the button on the form..
        '''</summary>
        Friend Shared ReadOnly Property mBoxMissingData() As String
            Get
                Return ResourceManager.GetString("mBoxMissingData", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný No gpx file was found.
        '''</summary>
        Friend Shared ReadOnly Property mBoxNo_gpx_file_was_found() As String
            Get
                Return ResourceManager.GetString("mBoxNo gpx file was found", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Trim GPS noise at track&apos;s start and end..
        '''</summary>
        Friend Shared ReadOnly Property mnuTrimGPSNoise_Text() As String
            Get
                Return ResourceManager.GetString("mnuTrimGPSNoise.Text", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Age.
        '''</summary>
        Friend Shared ReadOnly Property outAge() As String
            Get
                Return ResourceManager.GetString("outAge", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Processed all gpx files from the directory:.
        '''</summary>
        Friend Shared ReadOnly Property outAll_gpx_files_from_directory() As String
            Get
                Return ResourceManager.GetString("outAll_gpx_files_from_directory", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Average age of trails.
        '''</summary>
        Friend Shared ReadOnly Property outAverageAge() As String
            Get
                Return ResourceManager.GetString("outAverageAge", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Average length of trails.
        '''</summary>
        Friend Shared ReadOnly Property outAverageDistance() As String
            Get
                Return ResourceManager.GetString("outAverageDistance", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Average speed of dog.
        '''</summary>
        Friend Shared ReadOnly Property outAverageSpeed() As String
            Get
                Return ResourceManager.GetString("outAverageSpeed", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Description.
        '''</summary>
        Friend Shared ReadOnly Property outDescription() As String
            Get
                Return ResourceManager.GetString("outDescription", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný   to .
        '''</summary>
        Friend Shared ReadOnly Property outDo() As String
            Get
                Return ResourceManager.GetString("outDo", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný GPX file name.
        '''</summary>
        Friend Shared ReadOnly Property outgpxFileName() As String
            Get
                Return ResourceManager.GetString("outgpxFileName", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Length.
        '''</summary>
        Friend Shared ReadOnly Property outLength() As String
            Get
                Return ResourceManager.GetString("outLength", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Processed period: from  .
        '''</summary>
        Friend Shared ReadOnly Property outProcessed_period_from() As String
            Get
                Return ResourceManager.GetString("outProcessed_period_from", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Dog&apos;s Speed.
        '''</summary>
        Friend Shared ReadOnly Property outSpeed() As String
            Get
                Return ResourceManager.GetString("outSpeed", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Total Length of all trails.
        '''</summary>
        Friend Shared ReadOnly Property outTotalLength() As String
            Get
                Return ResourceManager.GetString("outTotalLength", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Total number of track/trails.
        '''</summary>
        Friend Shared ReadOnly Property outTotalNumberOfGPXFiles() As String
            Get
                Return ResourceManager.GetString("outTotalNumberOfGPXFiles", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Next time ask again.
        '''</summary>
        Friend Shared ReadOnly Property rbAskAgein() As String
            Get
                Return ResourceManager.GetString("rbAskAgein", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný For the other pairs, don&apos;t ask any more  and don&apos;t merge..
        '''</summary>
        Friend Shared ReadOnly Property rbDontAskDontMerge() As String
            Get
                Return ResourceManager.GetString("rbDontAskDontMerge", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný For the other pairs, don&apos;t ask any more  and join straight away (carefully!).
        '''</summary>
        Friend Shared ReadOnly Property rbDontAskMergeQ() As String
            Get
                Return ResourceManager.GetString("rbDontAskMergeQ", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný This is the end of the period within which the trails are to be processed..
        '''</summary>
        Friend Shared ReadOnly Property Tooltip_dtpEnd() As String
            Get
                Return ResourceManager.GetString("Tooltip_dtpEnd", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný This is the beginning of the period within which the trails are to be processed..
        '''</summary>
        Friend Shared ReadOnly Property Tooltip_dtpStart() As String
            Get
                Return ResourceManager.GetString("Tooltip_dtpStart", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Export as .
        '''</summary>
        Friend Shared ReadOnly Property Tooltip_ExportAs() As String
            Get
                Return ResourceManager.GetString("Tooltip_ExportAs", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Directory for backing up gpx files before processing.
        '''</summary>
        Friend Shared ReadOnly Property Tooltip_mnuBackupDirectory() As String
            Get
                Return ResourceManager.GetString("Tooltip_mnuBackupDirectory", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Navigate to the directory containing the GPX files you wish to process..
        '''</summary>
        Friend Shared ReadOnly Property Tooltip_mnuDirectory() As String
            Get
                Return ResourceManager.GetString("Tooltip_mnuDirectory", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Set the maximum time difference (in hours) between two track records that should be considered related and merged into one gpx file as a track of the layer and a track of the dog. The value therefore indicates the maximum age of the trail you are using. A value of 0 disables automatic merging..
        '''</summary>
        Friend Shared ReadOnly Property Tooltip_mnuMergingTracks() As String
            Get
                Return ResourceManager.GetString("Tooltip_mnuMergingTracks", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Prepend date to file names during processing. Useful for sorting files etc..
        '''</summary>
        Friend Shared ReadOnly Property Tooltip_mnuPrependDate() As String
            Get
                Return ResourceManager.GetString("Tooltip_mnuPrependDate", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný At the start of the trail, before you set off, and at the end, before you stop recording, \nthe GPS device often captures inaccurate or erroneous data. \nIf this option is enabled, the program will attempt to automatically remove these inaccuracies..
        '''</summary>
        Friend Shared ReadOnly Property Tooltip_mnuTrim() As String
            Get
                Return ResourceManager.GetString("Tooltip_mnuTrim", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný cross-track.
        '''</summary>
        Friend Shared ReadOnly Property txtCrossTrack() As String
            Get
                Return ResourceManager.GetString("txtCrossTrack", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Dog:.
        '''</summary>
        Friend Shared ReadOnly Property txtDogLabel() As String
            Get
                Return ResourceManager.GetString("txtDogLabel", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Goal:.
        '''</summary>
        Friend Shared ReadOnly Property txtGoalLabel() As String
            Get
                Return ResourceManager.GetString("txtGoalLabel", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Trail:.
        '''</summary>
        Friend Shared ReadOnly Property txtTrailLabel() As String
            Get
                Return ResourceManager.GetString("txtTrailLabel", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný runner.
        '''</summary>
        Friend Shared ReadOnly Property txtTrailLayer() As String
            Get
                Return ResourceManager.GetString("txtTrailLayer", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Date  .
        '''</summary>
        Friend Shared ReadOnly Property X_AxisLabel() As String
            Get
                Return ResourceManager.GetString("X_AxisLabel", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Age of trails (hours).
        '''</summary>
        Friend Shared ReadOnly Property Y_AxisLabelAge() As String
            Get
                Return ResourceManager.GetString("Y-AxisLabelAge", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Length of trails (km).
        '''</summary>
        Friend Shared ReadOnly Property Y_AxisLabelLength() As String
            Get
                Return ResourceManager.GetString("Y-AxisLabelLength", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Sniffed per month (km).
        '''</summary>
        Friend Shared ReadOnly Property Y_AxisLabelMonthly() As String
            Get
                Return ResourceManager.GetString("Y-AxisLabelMonthly", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Speed of the dog on the trails (km/h).
        '''</summary>
        Friend Shared ReadOnly Property Y_AxisLabelSpeed() As String
            Get
                Return ResourceManager.GetString("Y-AxisLabelSpeed", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Sniffed Kilometers .
        '''</summary>
        Friend Shared ReadOnly Property Y_AxisLabelTotalLength() As String
            Get
                Return ResourceManager.GetString("Y-AxisLabelTotalLength", resourceCulture)
            End Get
        End Property
    End Class
End Namespace
