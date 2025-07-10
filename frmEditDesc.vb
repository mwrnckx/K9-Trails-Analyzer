Imports System.Windows.Forms.VisualStyles.VisualStyleElement

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

    Private mouseDownPos As Point
    Private Sub txtBoxMouseDown(sender As Object, e As MouseEventArgs) Handles txtDog.MouseDown, txtGoal.MouseDown, txtTrail.MouseDown
        mouseDownPos = e.Location
    End Sub

    Private Sub TextBoxMouseMove(sender As Object, e As MouseEventArgs) Handles txtDog.MouseMove, txtGoal.MouseMove, txtTrail.MouseMove
        If e.Button = MouseButtons.Left Then
            ' Když myš ujde pár pixelů, spustíme drag
            Dim dx = Math.Abs(e.X - mouseDownPos.X)
            Dim dy = Math.Abs(e.Y - mouseDownPos.Y)
            If dx >= SystemInformation.DragSize.Width OrElse dy >= SystemInformation.DragSize.Height Then
                If sender.SelectedText.Length > 0 Then
                    sender.DoDragDrop(sender.SelectedText, DragDropEffects.Copy)
                End If
            End If
        End If
    End Sub

    Private Sub TextBox2_DragEnter(sender As Object, e As DragEventArgs) Handles txtDog.DragEnter, txtGoal.DragEnter, txtTrail.DragEnter
        If e.Data.GetDataPresent(DataFormats.Text) Then
            e.Effect = DragDropEffects.Copy
        Else
            e.Effect = DragDropEffects.None
        End If
    End Sub

    Private Sub TextBox_DragDrop(sender As Object, e As DragEventArgs) Handles txtDog.DragDrop, txtGoal.DragDrop, txtTrail.DragDrop
        Dim text As String = CStr(e.Data.GetData(DataFormats.Text))
        ' Vložíme na aktuální pozici kurzoru
        Dim tb As System.Windows.Forms.TextBox = CType(sender, System.Windows.Forms.TextBox)
        Dim pos As Integer = tb.SelectionStart
        tb.Text = tb.Text.Insert(pos, text)
    End Sub

End Class