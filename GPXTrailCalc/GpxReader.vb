Imports System.Xml

Public Class GpxReader
    Private xmlDoc As XmlDocument
    Private namespaceManager As XmlNamespaceManager
    Private filePath As String
    Private namespacePrefix As String

    Public Property Nodes As XmlNodeList



    ' Konstruktor načte XML dokument a nastaví XmlNamespaceManager
    Public Sub New(_filePath As String)
        xmlDoc = New XmlDocument()
        xmlDoc.Load(_filePath)
        filePath = _filePath

        ' Zjištění namespace, pokud je definován
        Dim rootNode As XmlNode = xmlDoc.DocumentElement
        Dim namespaceUri As String = rootNode.NamespaceURI

        ' Inicializace XmlNamespaceManager s dynamicky zjištěným namespace
        namespaceManager = New XmlNamespaceManager(xmlDoc.NameTable)
        If Not String.IsNullOrEmpty(namespaceUri) Then
            namespaceManager.AddNamespace("gpx", namespaceUri) ' Použijeme lokální prefix "gpx"
            namespacePrefix = "gpx:"
        Else
            namespaceManager.AddNamespace("", namespaceUri) ' Použijeme lokální prefix "gpx"
            namespacePrefix = ""
        End If
    End Sub

    'Metoda pro získání jednoho uzlu na základě XPath
    Public Function SelectSingleChildNode(childname As String, Node As XmlNode) As XmlNode
        If Node IsNot Nothing Then
            Return Node.SelectSingleNode(namespacePrefix & childname, namespaceManager)
        Else Return Nothing
        End If


    End Function



    ' Metoda pro získání seznamu uzlů na základě XPath
    Public Function SelectNodes(nodeName As String) As XmlNodeList

        Nodes = xmlDoc.SelectNodes("//" & namespacePrefix & nodeName, namespaceManager)

        Return Nodes
    End Function

    ' Metoda pro výběr jednoho uzlu na základě názvu
    Public Function SelectSingleNode(nodename As String) As XmlNode
        Return xmlDoc.SelectSingleNode("//" & namespacePrefix & nodename, namespaceManager)

    End Function

    ' Metoda pro výběr poduzlů z uzlu Node
    Public Function SelectChildNodes(childName As String, node As XmlNode) As XmlNodeList
        Return node.SelectNodes(namespacePrefix & childName, namespaceManager)
    End Function

    ' Metoda pro rekurentní výběr všech poduzlů z uzlu Node
    Public Function SelectAllChildNodes(childName As String, node As XmlNode) As XmlNodeList
        Return node.SelectNodes(".//" & namespacePrefix & childName, namespaceManager)
    End Function

    Public Function CreateElement(nodename As String) As XmlNode
        Return xmlDoc.CreateElement(nodename, "http://www.topografix.com/GPX/1/1")
    End Function

    Public Function CreateElement(parentNode As XmlElement, childNodeName As String, value As String) As XmlElement
        Dim childNode As XmlElement = CreateElement(childNodeName)
        childNode.InnerText = value
        ' Přidání uzlu <childNodeName> do prvního <parentNode>
        parentNode.AppendChild(childNode)

        Return parentNode

    End Function

    Public Function ImportNode(node As XmlNode, deepClone As Boolean)
        Return xmlDoc.ImportNode(node, deepClone)
    End Function

    Public Sub Save(save As Boolean)
        If save Then xmlDoc.Save(filePath)
    End Sub
End Class

