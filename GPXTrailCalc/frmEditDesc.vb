Public Class frmEditComments
    Public Property GoalPart As String
    Public Property TrailPart As String
    Public Property DogPart As String
    Dim goalLabel As String = My.Resources.Resource1.txtGoalLabel 'cíl
    Dim trailLabel As String = My.Resources.Resource1.txtTrailLabel '"Trail:"
    Dim dogLabel As String = My.Resources.Resource1.txtDogLabel '"Pes:"
    Public Property GpxFileName As String

    Private Sub frmEditComments_Load(sender As Object, e As EventArgs) Handles Me.Load
        txtGoal.Text = GoalPart
        txtTrail.Text = TrailPart
        txtDog.Text = DogPart
        lblInfo.Text = $"File: {GpxFileName}{vbCrLf}Edit the comments for the goal, trail, and dog parts of the GPX track. " & vbCrLf &
                        "These comments will be saved in the GPX file and displayed in the applications."
        lblGoal.Text = goalLabel
        lblTrail.Text = trailLabel
        lblDog.Text = dogLabel
        Me.btnOK.Focus() 'aby šlo jen odkliknout
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        GoalPart = txtGoal.Text
        TrailPart = txtTrail.Text
        DogPart = txtDog.Text
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub
End Class