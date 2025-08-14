Imports System.ComponentModel
Imports System.Xml
Imports TrackVideoExporter

Public Class frmCrossTrailSelector
    Inherits System.Windows.Forms.Form


    Dim trkList As List(Of TrackAsTrkNode) 'List(Of Tuple(Of XmlNode, DateTime, String))
    Public Sub New(_trkList As List(Of TrackAsTrkNode), filename As String)
        InitializeComponent()
        Me.trkList = _trkList
        Me.Text = "Select types of the tracks "
        Me.txtInfo.Text = String.Format(My.Resources.Resource1.CrossTrail_IntroText, filename, trkList.Count)


    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click

        Me.Close()
    End Sub

    Private Sub frmCrossTrailSelector_Load(sender As Object, e As EventArgs) Handles Me.Load
        ' Sloupec s typem trasy
        ' Připrav seznam KeyValuePair pro ComboBox
        Dim trackTypeItems As New List(Of KeyValuePair(Of TrackType, String))()
        For Each value As TrackType In [Enum].GetValues(GetType(TrackType))
            trackTypeItems.Add(New KeyValuePair(Of TrackType, String)(value, TrackTypeResolvers.LabelResolver(value)))
        Next
        typeColumn.DataSource = trackTypeItems
        typeColumn.DataPropertyName = "TrackType"
        typeColumn.ValueMember = "Key"
        typeColumn.DisplayMember = "Value"

        dgvTracks.Rows.Clear()

        For i = 0 To trkList.Count - 1
            Dim conv As New TrackConverter
            Dim name As String = conv.SelectSingleChildNode("name", trkList(i).TrkNode)?.InnerText
            Dim desc As String = conv.SelectSingleChildNode("desc", trkList(i).TrkNode)?.InnerText
            Dim start As String = $"{trkList(i).StartTrackGeoPoint.Time.ToLocalTime}"
            Dim type As TrackType = trkList(i).TrackType
            ' předvol výchozí typ trasy podle původního typu

            dgvTracks.Rows.Add(name, start, desc, type)
        Next
        Select Case trkList.Count
            Case 1 'only one track
                If dgvTracks.Rows(dgvTracks.Rows.Count - 1).Cells("TypeColumn").Value = TrackType.Unknown Then dgvTracks.Rows(dgvTracks.Rows.Count - 1).Cells("TypeColumn").Value = TrackType.DogTrack
            Case 2
                If dgvTracks.Rows(0).Cells("TypeColumn").Value = TrackType.Unknown Then dgvTracks.Rows(0).Cells("TypeColumn").Value = TrackType.RunnerTrail
                If dgvTracks.Rows(dgvTracks.Rows.Count - 1).Cells("TypeColumn").Value = TrackType.Unknown Then dgvTracks.Rows(dgvTracks.Rows.Count - 1).Cells("TypeColumn").Value = TrackType.DogTrack
                ' Pokud jsou dvě trasy, nastav první jako Runner trail a druhou jako dogtrack
            Case Else ' Poslední řádek je přednastaven jako dogtrack
                If dgvTracks.Rows(dgvTracks.Rows.Count - 1).Cells("TypeColumn").Value = TrackType.Unknown Then dgvTracks.Rows(dgvTracks.Rows.Count - 1).Cells("TypeColumn").Value = TrackType.DogTrack
        End Select

        If dgvTracks.Rows.Count > 0 Then
            dgvTracks.CurrentCell = dgvTracks.Rows(0).Cells("typeColumn") ' nebo Cells(1) pokud chceš podle indexu
            dgvTracks.BeginEdit(True) ' rovnou aktivuje ComboBox
        End If
    End Sub

    Private Sub frmCrossTrailSelector_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        Try
            'Dim RunnerTrailsCount As Integer = 0
            'Dim dogtracksCount As Integer = 0


            'For i = 0 To dgvTracks.Rows.Count - 1
            '    Dim row = dgvTracks.Rows(i)
            '    Dim selectedType = CType(row.Cells("TypeColumn").Value, TrackType)
            '    trkList(i).TrackType = selectedType
            '    If selectedType = TrackType.RunnerTrail Then
            '        RunnerTrailsCount += 1
            '    ElseIf selectedType = TrackType.DogTrack Then
            '        dogtracksCount += 1
            '    ElseIf selectedType = TrackType.Unknown Then
            '        mboxEx("For each track you have to choose its type!")
            '        e.Cancel = True ' Zabrání uzavření formuláře
            '        Return
            '    End If
            'Next


            'If RunnerTrailsCount > 1 Then
            '    mboxEx("There can be only one Runner trail.")
            '    e.Cancel = True ' Zabrání uzavření formuláře
            '    Return ' Zabrání uzavření formuláře
            'ElseIf dogtracksCount > 1 Then
            '    mboxEx("There can be only one dog track.")
            '    e.Cancel = True ' Zabrání uzavření formuláře
            '    Return ' Zabrání uzavření formuláře
            'End If
            If Me.ValidateTrailTypes() Then

                Me.DialogResult = DialogResult.OK
            Else
                e.Cancel = True
            End If


        Catch
            MessageBox.Show("Selection processing error.")
        End Try
    End Sub


    Public Function ValidateTrailTypes() As Boolean

        Dim RunnerTrailsCount As Integer = 0
        Dim dogtracksCount As Integer = 0


        For i = 0 To dgvTracks.Rows.Count - 1
            Dim row = dgvTracks.Rows(i)
            Dim selectedType = CType(row.Cells("TypeColumn").Value, TrackType)
            trkList(i).TrackType = selectedType
            If selectedType = TrackType.RunnerTrail Then
                RunnerTrailsCount += 1
            ElseIf selectedType = TrackType.DogTrack Then
                dogtracksCount += 1
            ElseIf selectedType = TrackType.Unknown Then
                mboxEx("For each track you have to choose its type!")
                Return False
            End If
        Next


        If RunnerTrailsCount > 1 Then
            mboxEx("There can be only one Runner trail.")

            Return False ' Zabrání uzavření formuláře
        ElseIf dogtracksCount > 1 Then
            mboxEx("There can be only one dog track.")

            Return False ' Zabrání uzavření formuláře
        End If

        Return True
    End Function

End Class



