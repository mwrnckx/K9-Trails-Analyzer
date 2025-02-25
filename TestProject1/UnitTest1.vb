Imports GPXTrailAnalyzer
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.IO ' Pro pr�ci se soubory
' P�idat Imports pro testovan� projekt
' Nahra�te TvujProjekt skute�n�m n�zvem projektu


Namespace TestProject1
    <TestClass>
    Public Class UnitTest1
        <TestMethod>
        Public Sub GpxReader_LoadFile_ValidFile_LoadsSuccessfully()
            ' Vytvo�en� testovac�ho GPX souboru (nebo pou�it� existuj�c�ho)
            Dim testFilePath As String = "test.gpx"
            File.WriteAllText(testFilePath, "<gpx><trk><trkseg><trkpt lat=""50"" lon=""15""></trkpt></trkseg></trk></gpx>")

            ' Vytvo�en� instance GpxReader
            Dim reader As New GpxReader(testFilePath)

            ' Ov��en�, �e se soubor na�etl bez v�jimky
            Assert.IsNotNull(reader)

            'Smaz�n� testovac�ho souboru
            File.Delete(testFilePath)

        End Sub

        Private testFilePath As String ' Prom�nn� pro ulo�en� cesty k souboru

        Private Sub TestMethodThatShouldThrowException() ' Bez argument�!
            Dim test As New GpxReader(testFilePath) ' Pou�ije prom�nnou t��dy
        End Sub

        <TestMethod>
        Public Sub GpxReader_LoadFile_InValidFile_ThrowsException()
            ' Vytvo�en� testovac�ho GPX souboru s chybou v XML
            testFilePath = "test.gpx" ' Nastaven� prom�nn� t��dy
            File.WriteAllText(testFilePath, "<gpx><trk><trkseg><trkpt lat=""50"" lon=""15""></trkpt></trkseg></trk></gpx") ' Chyba v XML

            ' Spr�vn� pou�it� Assert.ThrowsException s AddressOf
            Assert.ThrowsException(Of Xml.XmlException)(AddressOf TestMethodThatShouldThrowException)

            'Smaz�n� testovac�ho souboru
            File.Delete(testFilePath)
            testFilePath = "" ' Vy�i�t�n� prom�nn�
        End Sub

    End Class
End Namespace

