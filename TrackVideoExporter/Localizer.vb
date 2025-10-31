Imports System.Globalization
Imports System.Resources
Imports TrackVideoExporter.My.Resources

Public NotInheritable Class Localizer

    Private Shared ReadOnly rm As ResourceManager = Strings.ResourceManager

    Public Shared Function GetString(key As String, lang As String) As String
        Dim ci = CultureInfo.GetCultureInfo(lang)
        Dim text As String = rm.GetString(key, ci)

        ' fallback na angličtinu, pokud chybí
        If text Is Nothing AndAlso lang <> "en" Then
            text = rm.GetString(key, CultureInfo.GetCultureInfo("en"))
        End If

        ' fallback na výchozí resx
        If text Is Nothing Then
            text = rm.GetString(key, CultureInfo.InvariantCulture)
        End If

        Return text
    End Function

End Class
