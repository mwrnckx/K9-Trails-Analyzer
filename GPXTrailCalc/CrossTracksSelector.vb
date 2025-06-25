Public Class CrossTrailSelector
    Inherits System.Windows.Forms.Form

    Public Property CrossTrailIndices As List(Of Integer)

    Public Sub New(trackDescriptions As List(Of String), filename As String)
        InitializeComponent()

        Me.Text = "Select Cross - Tracks "
        Me.lblFileName.Text = filename

        For i = 0 To trackDescriptions.Count - 1
            ListBox1.Items.Add($"{trackDescriptions(i)}")
        Next


    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        Try
            'vybrané indexy zmenší o 1 - aby byly zero based
            CrossTrailIndices = txtIndexes.Text.Split(","c).Select(Function(s) Integer.Parse(s.Trim() - 1)).ToList()
            Me.DialogResult = DialogResult.OK
            Me.Close()
        Catch
            MessageBox.Show("Neplatný vstup – použij jen čísla oddělená čárkami.")
        End Try
    End Sub
End Class



