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
        '''  Vyhledá lokalizovaný řetězec podobný No gpx file was found.
        '''</summary>
        Friend Shared ReadOnly Property mBoxNo_gpx_file_was_found() As String
            Get
                Return ResourceManager.GetString("mBoxNo gpx file was found", resourceCulture)
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
        '''  Vyhledá lokalizovaný řetězec podobný Average age of trails: .
        '''</summary>
        Friend Shared ReadOnly Property outAverageAge() As String
            Get
                Return ResourceManager.GetString("outAverageAge", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Average length of trails: .
        '''</summary>
        Friend Shared ReadOnly Property outAverageDistance() As String
            Get
                Return ResourceManager.GetString("outAverageDistance", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Average speed of dog: .
        '''</summary>
        Friend Shared ReadOnly Property outAverageSpeed() As String
            Get
                Return ResourceManager.GetString("outAverageSpeed", resourceCulture)
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
        '''  Vyhledá lokalizovaný řetězec podobný Length: .
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
        '''  Vyhledá lokalizovaný řetězec podobný Dog&apos; s Speed.
        '''</summary>
        Friend Shared ReadOnly Property outSpeed() As String
            Get
                Return ResourceManager.GetString("outSpeed", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Total Length of all trails:   .
        '''</summary>
        Friend Shared ReadOnly Property outTotalLength() As String
            Get
                Return ResourceManager.GetString("outTotalLength", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Total number of processed GPX files, i.e. trails: .
        '''</summary>
        Friend Shared ReadOnly Property outTotalNumberOfGPXFiles() As String
            Get
                Return ResourceManager.GetString("outTotalNumberOfGPXFiles", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný Directory for backing up gpx files before processing.
        '''</summary>
        Friend Shared ReadOnly Property Tooltip_txtBackupDirectory() As String
            Get
                Return ResourceManager.GetString("Tooltip_txtBackupDirectory", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný The directory where all gpx files are stored.
        '''</summary>
        Friend Shared ReadOnly Property Tooltip_txtDirectory() As String
            Get
                Return ResourceManager.GetString("Tooltip_txtDirectory", resourceCulture)
            End Get
        End Property
        
        '''<summary>
        '''  Vyhledá lokalizovaný řetězec podobný   Date  .
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
