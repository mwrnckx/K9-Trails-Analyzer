Imports System.Drawing
Imports System.Windows
Imports System.Windows.Forms

Public Class frmPleaseWait
    Inherits Form
    Private WithEvents lblMessage As Label
    Private WithEvents progressBar As ProgressBar
    Public Sub New(label As String)
        Me.Text = "Please Wait"
        Me.BackColor = Color.Beige
        Me.ControlBox = False
        Me.Size = New Size(400, 150)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Cursor = Cursors.WaitCursor
        lblMessage = New Label() With {
            .Text = label,'"I'm making an overlay video, please stand by...",
        .AutoSize = True,
            .Location = New Point(20, 20)
        }
        Me.Controls.Add(lblMessage)
        progressBar = New ProgressBar() With {
            .Location = New Point(20, 60),
            .Size = New Size(340, 20),
            .Style = ProgressBarStyle.Marquee,
            .Value = 50
        }
        Me.Controls.Add(progressBar)


    End Sub
    Public Sub UpdateMessage(message As String)
        lblMessage.Text = message
    End Sub




End Class