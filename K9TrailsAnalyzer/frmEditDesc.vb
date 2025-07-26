Imports System.Windows.Forms.VisualStyles.VisualStyleElement

Public Class frmEditComments
    Public Property GoalPart As String
    Public Property TrailPart As String
    Public Property DogPart As String
    Public Property GoalPartEng As String
    Public Property TrailPartEng As String
    Public Property DogPartEng As String
    Dim goalLabel As String = My.Resources.Resource1.txtGoalLabel 'cíl
    Dim trailLabel As String = My.Resources.Resource1.txtTrailLabel '"Trail:"
    Dim dogLabel As String = My.Resources.Resource1.txtDogLabel '"Pes:"
    Public Property GpxFileName As String
    Private ddHelper1 As RichTextBoxDragDropHelper
    Private ddHelper2 As RichTextBoxDragDropHelper
    Private ddHelper3 As RichTextBoxDragDropHelper

    Public Sub New()

        ' Toto volání je vyžadované návrhářem.
        InitializeComponent()

        ' Přidejte libovolnou inicializaci po volání InitializeComponent().

    End Sub

    Private Sub frmEditComments_Load(sender As Object, e As EventArgs) Handles Me.Load
        rtbGoal.Text = GoalPart
        rtbTrail.Text = TrailPart
        rtbDog.Text = DogPart
        rtbGoalEng.Text = GoalPart
        rtbTrailEng.Text = TrailPart
        rtbDogEng.Text = DogPart
        lblInfo.MaximumSize = New Size(Me.Width * 0.8, Me.Height * 0.8) 'nastaví maximální šířku popisku
        lblInfo.Text = $"{Form1.mnuFile.Text} {GpxFileName}" &
            vbCrLf & My.Resources.Resource1.txtEditDescLabel

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

    Private Sub SaveFormData()
        ' Tato subrutina obsahuje veškerou logiku pro uložení dat z RichTextBoxů
        GoalPart = rtbGoal.Text
        TrailPart = rtbTrail.Text
        DogPart = rtbDog.Text
        GoalPartEng = rtbGoalEng.Text
        TrailPartEng = rtbTrailEng.Text
        DogPartEng = rtbDogEng.Text
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        ' Po kliknutí na OK tlačítko se data uloží a formulář se zavře s výsledkem OK
        SaveFormData()
        Me.DialogResult = DialogResult.OK
        Me.Close()
    End Sub

    Private Sub MyForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        ' Tato událost se spustí, když se formulář zavírá (včetně křížku)
        ' e.CloseReason vám řekne, proč se formulář zavírá.
        ' Pokud se zavírá křížkem (UserClosing) A ještě nebylo nastaveno DialogResult na OK (např. přes tlačítko OK),
        ' pak to ošetříme jako OK.

        If e.CloseReason = CloseReason.UserClosing AndAlso Me.DialogResult <> DialogResult.OK Then
            ' Uživatel zavřel křížkem a NEKLIKL na OK.
            ' Protože chceme stejný efekt jako OK, uložíme data a nastavíme DialogResult.
            SaveFormData()
            Me.DialogResult = DialogResult.OK
            ' Všimněte si, že zde již NEnutíme formulář k zavření pomocí Me.Close().
            ' Událost FormClosing se postará o jeho zavření po dokončení.
        ElseIf Me.DialogResult = DialogResult.OK Then
            ' Pokud je DialogResult již OK (nastaveno tlačítkem OK),
            ' znamená to, že data jsou již uložena (nebo se budou ukládat jinde),
            ' a nemusíme zde nic dalšího dělat, kromě toho, že umožníme zavření.
        End If
        ' Pokud byste chtěli, aby křížek fungoval jako Cancel, udělali byste:
        ' If e.CloseReason = CloseReason.UserClosing AndAlso Me.DialogResult <> DialogResult.OK Then
        '     Me.DialogResult = DialogResult.Cancel
        ' End If

    End Sub



    Private Sub RichTextBox_DragEnter(sender As Object, e As DragEventArgs) _
    Handles rtbDog.DragEnter, rtbGoal.DragEnter, rtbTrail.DragEnter
        Debug.WriteLine("Data formats:")
        For Each fmt In e.Data.GetFormats
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
        Dim rtb = CType(sender, RichTextBox)
        Dim text = CStr(e.Data.GetData(DataFormats.Text))
        Dim pos = rtb.SelectionStart
        rtb.SelectedText = text
    End Sub


End Class