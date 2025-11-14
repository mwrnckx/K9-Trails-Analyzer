Imports System.ComponentModel
Imports System.Globalization
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports TrackVideoExporter

Public Class frmEditComments
    Public Property Category As String
    Public Property TrailDescription As TrailReport
    Public Property GoalPart As String
    Public Property TrailPart As String
    Public Property DogPart As String
    Public Property Language As String = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLowerInvariant()
    Public Property GpxFileName As String


    'Public ReadOnly Property AllPartsyy As TrailReport
    '    Get
    '        Return New TrailReport("", Me.Category, Me.GoalPart, Me.TrailPart, Me.DogPart, "", (Nothing, Nothing, Nothing, Nothing, Nothing, Nothing))
    '    End Get
    'End Property

    Dim LanguageAbbreviations As New List(Of String) From {
    "cs", ' čeština
    "en", ' angličtina
    "de", ' němčina
    "fr", ' francouzština
    "es", ' španělština
    "it", ' italština
    "pl", ' polština
    "ru", ' ruština
    "uk", ' ukrajinština
    "sk", ' slovenština
    "hu", ' maďarština
    "pt", ' portugalština
    "nl", ' nizozemština
    "sv", ' švédština
    "da", ' dánština
    "fi", ' finština
    "no", ' norština
    "tr", ' turečtina
    "zh", ' čínština
    "ja", ' japonština
    "ko", ' korejština
    "ar", ' arabština
    "he", ' hebrejština
    "el"  ' řečtina
}




    Public Sub New()

        ' Toto volání je vyžadované návrhářem.
        InitializeComponent()

        ' Přidejte libovolnou inicializaci po volání InitializeComponent().
        LanguageAbbreviations.Sort()
        ComboBox1.Items.AddRange(LanguageAbbreviations.ToArray())


    End Sub

    Private Sub frmEditDesc_Load(sender As Object, e As EventArgs) Handles Me.Load
        txtGoal.Text = Me.TrailDescription.Goal.Text
        txtTrail.Text = Me.TrailDescription.Trail.Text
        txtPerformance.Text = Me.TrailDescription.Performance.Text
        If Language IsNot Nothing Then
            ' Pokud je jazyk nastaven, zablokujeme conmbo!
            Me.ComboBox1.Enabled = False

        End If
        Me.ComboBox1.SelectedItem = Language
        'lblInfo.MaximumSize = New Size(Me.Width * 0.8, Me.Height * 0.8) 'nastaví maximální šířku popisku
        lblInfo.Text = $"{Form1.mnuFile.Text} {GpxFileName}" &
            vbCrLf & lblInfo.Text

        txtPerformance.AllowDrop = True
        txtGoal.AllowDrop = True
        txtTrail.AllowDrop = True

        Me.lblGoal.Text = TrailReport.goalLabel & Me.lblGoal.Text
        Me.lblTrail.Text = TrailReport.trailLabel & Me.lblTrail.Text
        Me.lblPerformance.Text = TrailReport.performanceLabel & Me.lblPerformance.Text



        Me.btnOK.Focus() 'aby šlo jen odkliknout
    End Sub

    Private Sub SaveFormData()
        Me.TrailDescription.GoalText = txtGoal.Text
        Me.TrailDescription.TrailText = txtTrail.Text
        Me.TrailDescription.PerformanceText = txtPerformance.Text
        If Me.Language Is Nothing Then
            Me.Language = ComboBox1.SelectedItem?.ToString()?.ToLowerInvariant() ' Uloží vybraný jazyk
        ElseIf Me.Language <> ComboBox1.SelectedItem?.ToString()?.ToLowerInvariant() Then
            ' Pokud se jazyk změnil, aktualizujeme ho
            mboxEx("The language of the existing description cannot be changed!")

        End If
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        ' Po kliknutí na OK tlačítko se data uloží a formulář se zavře s výsledkem OK
        SaveFormData()
        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub MyForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing
        ' Tato událost se spustí, když se formulář zavírá (včetně křížku)
        ' e.CloseReason vám řekne, proč se formulář zavírá.
        ' Pokud se zavírá křížkem (UserClosing) A ještě nebylo nastaveno DialogResult na OK (např. přes tlačítko OK),
        ' pak to ošetříme jako OK.


        If Me.DialogResult = DialogResult.OK Then
            ' Pokud je DialogResult již OK (nastaveno tlačítkem OK),
            ' znamená to, že data jsou již uložena (nebo se budou ukládat jinde),
            ' a nemusíme zde nic dalšího dělat, kromě toho, že umožníme zavření.
        ElseIf Me.DialogResult = DialogResult.Retry Then
            ' Pokud byste chtěli, aby křížek fungoval jako Cancel, udělali byste:
            ' If e.CloseReason = CloseReason.UserClosing AndAlso Me.DialogResult <> DialogResult.OK Then
            '     Me.DialogResult = DialogResult.Cancel
        Else
            SaveFormData()
            Me.DialogResult = DialogResult.OK
        End If

    End Sub

    Private Sub btnAnotherLang_Click(sender As Object, e As EventArgs) Handles btnAnotherLang.Click
        ' Po kliknutí na  tlačítko se data uloží a formulář se zavře s výsledkem retry
        SaveFormData()
        DialogResult = DialogResult.Retry
        Close()
    End Sub

    Private Sub frmEditComments_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        If Me.ComboBox1.SelectedItem = Nothing Then
            Select Case Me.DialogResult
                Case DialogResult.OK, DialogResult.Retry
                    e.Cancel = True
                    MessageBox.Show("Please select a language for the description.", "Language not selected", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Select
        End If
    End Sub
End Class
