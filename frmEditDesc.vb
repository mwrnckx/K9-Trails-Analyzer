Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Public Class frmEditComments
    Public Property GoalPart As String
    Public Property TrailPart As String
    Public Property DogPart As String
    Dim goalLabel As String = My.Resources.Resource1.txtGoalLabel 'cíl
    Dim trailLabel As String = My.Resources.Resource1.txtTrailLabel '"Trail:"
    Dim dogLabel As String = My.Resources.Resource1.txtDogLabel '"Pes:"
    Public Property GpxFileName As String
    Private ddHelper1 As RichTextBoxDragDropHelper
    Private ddHelper2 As RichTextBoxDragDropHelper
    Private ddHelper3 As RichTextBoxDragDropHelper

    Private Sub frmEditComments_Load(sender As Object, e As EventArgs) Handles Me.Load
        rtbGoal.Text = GoalPart
        rtbTrail.Text = TrailPart
        rtbDog.Text = DogPart
        lblInfo.Text = $"File: {GpxFileName}{vbCrLf}Edit the comments for the goal, trail, and dog parts of the GPX track. " & vbCrLf &
                        "These comments will be saved in the GPX file and displayed in the applications."
        lblGoal.Text = goalLabel
        lblTrail.Text = trailLabel
        lblDog.Text = dogLabel
        ddHelper1 = New RichTextBoxDragDropHelper(rtbDog)
        ddHelper2 = New RichTextBoxDragDropHelper(rtbGoal)
        ddHelper3 = New RichTextBoxDragDropHelper(rtbTrail)
        rtbDog.AllowDrop = True
        rtbGoal.AllowDrop = True
        rtbTrail.AllowDrop = True

        Debug.WriteLine($"rtbDog.AllowDrop (po helperu): {rtbDog.AllowDrop}")
        Debug.WriteLine($"rtbGoal.AllowDrop (po helperu): {rtbGoal.AllowDrop}")
        Debug.WriteLine($"rtbTrail.AllowDrop (po helperu): {rtbTrail.AllowDrop}")


        Me.btnOK.Focus() 'aby šlo jen odkliknout
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        GoalPart = rtbGoal.Text
        TrailPart = rtbTrail.Text
        DogPart = rtbDog.Text
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub



    Private Sub RichTextBox_DragEnter(sender As Object, e As DragEventArgs) _
    Handles rtbDog.DragEnter, rtbGoal.DragEnter, rtbTrail.DragEnter
        Debug.WriteLine("Data formats:")
        For Each fmt In e.Data.GetFormats()
            Debug.WriteLine($" - {fmt}")
        Next


        If e.Data.GetDataPresent(DataFormats.Text) OrElse e.Data.GetDataPresent(DataFormats.UnicodeText) Then
            e.Effect = DragDropEffects.Copy

            e.Effect = DragDropEffects.Copy
        Else
            e.Effect = DragDropEffects.None
        End If
        Application.DoEvents()
        Debug.WriteLine("DragEnter effect set to copy")

    End Sub

    Private Sub RichTextBox_DragDrop(sender As Object, e As DragEventArgs) _
    Handles rtbDog.DragDrop, rtbGoal.DragDrop, rtbTrail.DragDrop
        Dim rtb As RichTextBox = CType(sender, RichTextBox)
        Dim text As String = CStr(e.Data.GetData(DataFormats.Text))
        Dim pos As Integer = rtb.SelectionStart
        rtb.SelectedText = text
    End Sub


End Class