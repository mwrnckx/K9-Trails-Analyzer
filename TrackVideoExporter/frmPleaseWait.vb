﻿Imports System.Drawing
Imports System.Windows
Imports System.Windows.Forms

Public Class frmPleaseWait
    Inherits Form
    Private WithEvents lblMessage As Label
    Private WithEvents progressBar As ProgressBar
    Public Sub New()
        Me.Text = "Please Wait"
        Me.BackColor = Color.FromArgb(CByte(237), CByte(240), CByte(213))
        Me.ControlBox = False
        Me.Size = New Size(400, 150)
        Me.StartPosition = FormStartPosition.CenterScreen
        lblMessage = New Label() With {
            .Text = "I'm making an overlay video, please stand by...",
            .AutoSize = True,
            .Location = New Point(20, 20)
        }
        Me.Controls.Add(lblMessage)
        progressBar = New ProgressBar() With {
            .Location = New Point(20, 60),
            .Size = New Size(360, 20),
            .Style = ProgressBarStyle.Marquee
        }
        Me.Controls.Add(progressBar)
    End Sub
    Public Sub UpdateMessage(message As String)
        lblMessage.Text = message
    End Sub

End Class