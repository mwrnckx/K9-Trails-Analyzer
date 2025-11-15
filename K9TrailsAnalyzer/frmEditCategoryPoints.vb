Public Class frmEditCategoryPoints
    Private _categoryInfo As CategoryInfo

    Public Sub New(cat As CategoryInfo)
        InitializeComponent()
        _categoryInfo = cat
    End Sub

    Private Sub frmEditCategoryPoints_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        numFind.Value = _categoryInfo.PointsForFindMax
        numSpeed.Value = CDec(_categoryInfo.PointsPerKmhGrossSpeed)
        numAcc.Value = _categoryInfo.PointsForAccuracyMax
        numRead.Value = _categoryInfo.PointsForDogReadingMax
        numPick.Value = _categoryInfo.PointsForEarlyPickUpMax
    End Sub

    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click
        _categoryInfo.PointsForFindMax = CInt(numFind.Value)
        _categoryInfo.PointsPerKmhGrossSpeed = CDbl(numSpeed.Value)
        _categoryInfo.PointsForAccuracyMax = CInt(numAcc.Value)
        _categoryInfo.PointsForDogReadingMax = CInt(numRead.Value)
        _categoryInfo.PointsForEarlyPickUpMax = CInt(numPick.Value)

        Me.DialogResult = DialogResult.OK
    End Sub

    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        Me.DialogResult = DialogResult.Cancel
    End Sub
End Class
