Public Class CrossTrailSelector
    Inherits System.Windows.Forms.Form

    Public Property CrossTrailIndices As List(Of Integer)
    Public Property TrackDescriptions As List(Of String)

    Public Sub New(_trackDescriptions As List(Of String), tracktypesList As List(Of String), filename As String)
        InitializeComponent()
        Me.TrackDescriptions = _trackDescriptions
        Me.Text = "Select Cross-Tracks "
        Me.txtInfo.Text =
    $"GPX record {filename}" & vbCrLf & $"contains {TrackDescriptions.Count} tracks." & vbCrLf &
    "The last track is assumed to be the dog's track." & vbCrLf &
    "Among the remaining tracks, one is probably the layer's track, and the rest may be cross-tracks." & vbCrLf &
    "Please check the tracks that are cross-tracks below."

        ' Přidej popisy do zaškrtávacího seznamu
        For i = 0 To TrackDescriptions.Count - 1
            Dim isChecked As Boolean = tracktypesList(i)?.Contains(TrackTypes.CrossTrack.Trim().ToLower())
            chkListTracks.Items.Add($"{TrackDescriptions(i)}", isChecked)
        Next

    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Try
            ' Vyber indexy zaškrtnutých položek, vracej zero-based
            CrossTrailIndices = chkListTracks.CheckedIndices.Cast(Of Integer).ToList()

            ' Zkontroluj, že zůstaly právě dvě nezaškrtnuté trasy
            If (TrackDescriptions.Count - CrossTrailIndices.Count) <> 2 Then
                Dim unchecked = TrackDescriptions.Count - CrossTrailIndices.Count
                MessageBox.Show($"Exactly two tracks must remain unchecked: one for the layer, and one for the dog, but {unchecked} were found.")
                Return ' Zabrání uzavření formuláře
            End If

            Me.DialogResult = DialogResult.OK
            Me.Close()

        Catch
            MessageBox.Show("Selection processing error.")
        End Try
    End Sub

End Class



