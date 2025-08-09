Imports System.Windows.Forms
Imports System

Public Module Helper
	'Abych nemusel psat o 5 pismenek vic haha
	Public Function mboxIn(ByVal message_ As String, ByVal defaultResponse As String) As String
		Return InputBox(message_, Application.CompanyName & "  " & Application.ProductName, defaultResponse)
	End Function
	'Abych nemusel psat o 5 pismenek vic haha
	Public Sub mbox(ByVal message_ As String)
		MessageBox.Show(message_, Application.CompanyName & "  " & Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
	End Sub
	Public Sub mbox(owner As System.Windows.Forms.IWin32Window, ByVal message_ As String)
		MessageBox.Show(owner, message_, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1)
	End Sub
	'Abych nemusel psat o 5 pismenek vic haha
	Public Function mboxq(ByVal message_ As String) As System.Windows.Forms.DialogResult
		Return MessageBox.Show(message_, Application.CompanyName & "  " & Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)
	End Function
	Public Function mboxq(ByVal message_ As String, ByVal defButton As MessageBoxDefaultButton) As System.Windows.Forms.DialogResult
		Return MessageBox.Show(message_, Application.CompanyName & "  " & Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Question, defButton)
	End Function
	'Abych nemusel psat o 5 pismenek vic haha
	Public Function mboxqs(ByVal message_ As String) As System.Windows.Forms.DialogResult
		Return MessageBox.Show(message_, Application.CompanyName & "  " & Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1)
	End Function
	Public Function mboxqs(ByVal message_ As String, ByVal defButton As MessageBoxDefaultButton) As System.Windows.Forms.DialogResult
		Return MessageBox.Show(message_, Application.CompanyName & "  " & Application.ProductName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, defButton)
	End Function
	'Abych nemusel psat o 5 pismenek vic haha
	Public Sub mboxEx(ByVal message_ As String)
		MessageBox.Show(message_, Application.CompanyName & "  " & Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
	End Sub
	Public Function mboxQEx(ByVal message_ As String) As System.Windows.Forms.DialogResult
		Return MessageBox.Show(message_, Application.CompanyName & "  " & Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
	End Function
	Public Sub mboxErr(ByVal message_ As String)
		MessageBox.Show(message_, Application.CompanyName & "  " & Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1)
	End Sub
	Public Function mboxWrn(ByVal message_ As String) As DialogResult
		Return MessageBox.Show(message_, Application.CompanyName & "  " & Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1)
	End Function


End Module
