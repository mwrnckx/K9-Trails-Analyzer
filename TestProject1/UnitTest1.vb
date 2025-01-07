Imports GPXTrailAnalyzer
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.IO ' Pro práci se soubory
' Pøidat Imports pro testovaný projekt
' Nahraïte TvujProjekt skuteèným názvem projektu


Namespace TestProject1
    <TestClass>
    Public Class UnitTest1
        <TestMethod>
        Public Sub GpxReader_LoadFile_ValidFile_LoadsSuccessfully()
            ' Vytvoøení testovacího GPX souboru (nebo použití existujícího)
            Dim testFilePath As String = "test.gpx"
            File.WriteAllText(testFilePath, "<gpx><trk><trkseg><trkpt lat=""50"" lon=""15""></trkpt></trkseg></trk></gpx>")

            ' Vytvoøení instance GpxReader
            Dim reader As New GpxReader(testFilePath)

            ' Ovìøení, že se soubor naèetl bez výjimky
            Assert.IsNotNull(reader)

            'Smazání testovacího souboru
            File.Delete(testFilePath)

        End Sub

        Private testFilePath As String ' Promìnná pro uložení cesty k souboru

        Private Sub TestMethodThatShouldThrowException() ' Bez argumentù!
            Dim test As New GpxReader(testFilePath) ' Použije promìnnou tøídy
        End Sub

        <TestMethod>
        Public Sub GpxReader_LoadFile_InValidFile_ThrowsException()
            ' Vytvoøení testovacího GPX souboru s chybou v XML
            testFilePath = "test.gpx" ' Nastavení promìnné tøídy
            File.WriteAllText(testFilePath, "<gpx><trk><trkseg><trkpt lat=""50"" lon=""15""></trkpt></trkseg></trk></gpx") ' Chyba v XML

            ' Správné použití Assert.ThrowsException s AddressOf
            Assert.ThrowsException(Of Xml.XmlException)(AddressOf TestMethodThatShouldThrowException)

            'Smazání testovacího souboru
            File.Delete(testFilePath)
            testFilePath = "" ' Vyèištìní promìnné
        End Sub

    End Class
End Namespace

