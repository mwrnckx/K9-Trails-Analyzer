Public Class RichTextBoxDragDropHelper
    Private ReadOnly rtb As RichTextBox
    Private lastSelectionStart As Integer
    Private lastSelectionLength As Integer
    Private isInSelection As Boolean = False
    Private mouseDownPos As Point

    Public Sub New(richTextBox As RichTextBox)
        rtb = richTextBox
        AddHandler rtb.SelectionChanged, AddressOf RichTextBox_SelectionChanged
        AddHandler rtb.MouseDown, AddressOf RichTextBox_MouseDown
        AddHandler rtb.MouseMove, AddressOf RichTextBox_MouseMove
    End Sub

    Private Sub RichTextBox_SelectionChanged(sender As Object, e As EventArgs)
        lastSelectionStart = rtb.SelectionStart
        lastSelectionLength = rtb.SelectionLength
    End Sub

    Private Sub RichTextBox_MouseDown(sender As Object, e As MouseEventArgs)
        mouseDownPos = e.Location
        Dim charIndex As Integer = rtb.GetCharIndexFromPosition(e.Location)
        Dim selEnd = lastSelectionStart + lastSelectionLength
        isInSelection = (lastSelectionLength > 0 AndAlso charIndex >= lastSelectionStart AndAlso charIndex < selEnd)
    End Sub

    Private Sub RichTextBox_MouseMove(sender As Object, e As MouseEventArgs)
        ' kurzor pro UX
        Dim charIndex As Integer = rtb.GetCharIndexFromPosition(e.Location)
        Dim selEnd = lastSelectionStart + lastSelectionLength
        If lastSelectionLength > 0 AndAlso charIndex >= lastSelectionStart AndAlso charIndex < selEnd Then
            rtb.Cursor = Cursors.Arrow
        Else
            rtb.Cursor = Cursors.IBeam
        End If

        ' Drag začneme jen při pohybu myši a jen když jsme klikli do výběru
        If e.Button = MouseButtons.Left AndAlso isInSelection Then
            Dim dx = Math.Abs(e.X - mouseDownPos.X)
            Dim dy = Math.Abs(e.Y - mouseDownPos.Y)
            If dx >= SystemInformation.DragSize.Width OrElse dy >= SystemInformation.DragSize.Height Then
                Debug.WriteLine($"Drag started: {rtb.Text.Substring(lastSelectionStart, lastSelectionLength)}")
                rtb.DoDragDrop(rtb.SelectedText, DragDropEffects.Copy)
                isInSelection = False ' zabrání dalším dragům
            End If
        End If
    End Sub
End Class
