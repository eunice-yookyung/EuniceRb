Public Class frmGUI
    Public WithEvents dgv As DataGridView
    Public dt As DataTable
    Public WithEvents dgvloop As DataGridView
    Public dtloop As DataTable
    Public inter As clsInteractive
    Dim gui_autofill As New frmGUI_autofill(Me)
    Public runningState As Integer
    Public currentExpNo As Integer = 0
    Public stopping As Integer = 0
    Public stopped As Integer = 1
    Public continuous As Integer = 2
    Public batch As Integer = 3
    Public batchstopping As Integer = 4

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        dgv = New DataGridView()
        dgv.Dock = DockStyle.Fill
        ExpSeqPanel.Controls.Add(dgv)
        dgv.AllowUserToAddRows = True
        dgv.AllowUserToDeleteRows = True

        dgvloop = New DataGridView()
        dgvloop.Dock = DockStyle.Fill
        LoopingPanel.Controls.Add(dgvloop)
        dgvloop.AllowUserToAddRows = False
        dgvloop.AllowUserToDeleteRows = False

        inter = New clsInteractive()

        dt = New DataTable()
        Dim dataset As DataSet
        dataset = New DataSet()
        dataset.Tables.Add(dt)
        dgv.DataSource = dt
        dgv.AutoGenerateColumns = True
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells

        dtloop = New DataTable()
        Dim datasetloop As DataSet
        datasetloop = New DataSet()
        datasetloop.Tables.Add(dtloop)
        dgvloop.DataSource = dtloop
        dgvloop.AutoGenerateColumns = True
        dgvloop.AutoResizeColumns()
        dgvloop.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells

        noToAddField.Text = "1"
        startSeqField.Text = "1"
        noPointsField.Text = "1"
        stopSeqField.Text = "1"
        linearRadio.Select()

        runningState = stopped
    End Sub

    Public Sub buildDataTables()

        'reset dt and dtloop
        dt.Columns.Clear()
        dtloop.Columns.Clear()
        dt.Rows.Clear()
        dtloop.Rows.Clear()

        'generate new dt and dtloop
        Dim arrList As ArrayList
        arrList = modUtilities.GetExpVariables()

        Dim var As Object
        For Each var In arrList
            Dim column As DataColumn
            column = New DataColumn()
            column.DataType = System.Type.GetType("System.Double")
            column.ColumnName = var.ToString()
            dt.Columns.Add(column)
        Next

        For Each var In arrList
            Dim column As DataColumn
            column = New DataColumn()
            column.DataType = System.Type.GetType("System.Double")
            column.ColumnName = var.ToString()
            dtloop.Columns.Add(column)
        Next

        appendNRows(1, dtloop)
        appendNRows(1, dt)

    End Sub

    Private Sub appendNRows(ByVal noRows As Integer, ByVal dtable As DataTable)
        Dim i As Integer
        For i = 1 To noRows Step 1
            Dim row As DataRow
            row = dtable.NewRow()
            Dim arrList As ArrayList
            arrList = modUtilities.GetExpVariablesDefVals()
            Dim var As Object
            Dim j As Integer
            j = 0
            For Each var In arrList
                row(j) = var
                j = j + 1
            Next
            dtable.Rows.Add(row)
        Next i
    End Sub

    'Private Sub dataGridView1_DefaultValuesNeeded(ByVal sender As Object, _
    '    ByVal e As System.Windows.Forms.DataGridViewRowEventArgs)

    '    'With e.Row
    '    '    Dim arrList As ArrayList

    '    '    'empty
    '    '    Dim empty As String = "n/a"
    '    '    arrList = New ArrayList()
    '    '    arrList.Add(empty)
    '    '    'arrList = modUtilities.GetExpVariablesDefVals()

    '    '    Dim var As Object
    '    '    Dim i As Integer
    '    '    i = 0
    '    '    For Each var In arrList
    '    '        .Cells(i).Value = var
    '    '        i = i + 1
    '    '    Next
    '    'End With
    'End Sub

    Private Sub AddExpsButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AddExpsButton.Click
        Dim k As Integer
        k = Integer.Parse(noToAddField.Text)
        appendNRows(k, dt)
    End Sub

    Private Sub writeSeqButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles writeSeqButton.Click
        Dim p As Point
        p = dgv.CurrentCellAddress
        Dim noExtraRows, currentNoRows, noPoints As Integer
        currentNoRows = dt.Rows.Count
        noPoints = Integer.Parse(noPointsField.Text)
        noExtraRows = p.Y + noPoints - currentNoRows
        If (noExtraRows > 0) Then
            appendNRows(noExtraRows, dt)
        End If
        Dim arr As ArrayList
        arr = New ArrayList()
        Dim i As Integer
        Dim startSeq, stopSeq, incSeq As Double
        startSeq = Double.Parse(startSeqField.Text)
        stopSeq = Double.Parse(stopSeqField.Text)
        If linearRadio.Checked() Then
            incSeq = (stopSeq - startSeq) / (noPoints - 1)
            For i = 0 To noPoints - 1
                arr.Add(startSeq + i * incSeq)
            Next
        Else
            incSeq = (Math.Log10(stopSeq) - Math.Log10(startSeq)) / (noPoints - 1)
            For i = 0 To noPoints - 1
                arr.Add(Math.Pow(10, (Math.Log10(startSeq) + i * incSeq)))
            Next
        End If
        For i = 0 To noPoints - 1
            dt.Rows(p.Y + i)(p.X) = arr(i)
        Next
    End Sub


    Private Sub randomizeButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles randomizeButton.Click
        Dim dt2 As DataTable
        dt2 = New DataTable()
        dt2 = dt.Clone()
        Dim noRows, i, rand As Integer
        Dim r As New Random(System.DateTime.Now.Millisecond)
        noRows = dt.Rows.Count
        Dim row As DataRow
        For i = 0 To noRows - 1
            rand = r.Next(0, dt.Rows.Count)
            row = dt.Rows(rand)
            dt2.ImportRow(row)
            dt.Rows.RemoveAt(rand)
        Next
        For i = 0 To noRows - 1
            row = dt2.Rows(i)
            dt.ImportRow(row)
        Next
    End Sub
    Private Sub LoopCellEdited(ByVal sender As Object, ByVal e As DataGridViewCellCancelEventArgs) Handles dgvloop.CellBeginEdit
        dgvloop.Rows(0).DefaultCellStyle.BackColor = Color.Yellow
    End Sub
    Private Sub ChooseProgramButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ChooseProgramButton.Click
        With OpenFileDialog
            If .ShowDialog = System.Windows.Forms.DialogResult.OK Then
                prepareToRun(.FileName)
            End If
        End With
    End Sub
    Private Sub interactiveCmdText_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles interactiveCmdText.KeyPress
        If e.KeyChar = ControlChars.Cr Then
            inter.parse(interactiveCmdText.Text)
            interactiveCmdText.Text = ""
            e.Handled = True
        End If
    End Sub
    Private Sub RunButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RunButton.Click
        Dim cb As New AsyncCallback(AddressOf modMain.experimentCompleted)
        Dim del As New runExperimentDelegate(AddressOf runExperiment)
        del.BeginInvoke(cb, del)
    End Sub

    'added oct 2011, run virtual experiment
    Private Sub RunVirtualButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RunVirtualButton.Click

        Dim cb As New AsyncCallback(AddressOf modMain.experimentCompleted)
        Dim del As New runExperimentDelegate(AddressOf runVirtualExperiment)
        del.BeginInvoke(cb, del)
    End Sub

    Private Sub StopButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles StopButton.Click
        runningState = stopping
        StopButton.Enabled = False
        BatchButton.Enabled = False
        StopBatchButton.Enabled = False
        StatusLabel.Text = "Waiting for current experiment to end..."
        Refresh()
    End Sub

    Private Sub BatchButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles BatchButton.Click
        modMain.runBatch()
    End Sub

    Private Sub StopBatchButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles StopBatchButton.Click
        runningState = batchstopping
        StopBatchButton.Enabled = False
        StatusLabel.Text = "Waiting for current experiment to end..."
        Refresh()
    End Sub

    Private Sub InteractiveGuiButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles interGUI_Button.Click

        If gui_inter.IsLoaded() = False Then
            gui_inter.LoadMacros()
        End If

        'gui_inter.ShowDialog() 'modal
        gui_inter.Show() 'modeless
        gui_inter.Refresh()

    End Sub

    Private Sub AutoFill_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AutoFill.Click

        'Dim gui_autofill As New frmGUI_autofill(Me)
        'gui_autofill.ShowDialog()
        'gui_autofill.Refresh()
        gui_autofill.ShowAutofill(Me.currentExpFileLabel.Text)

    End Sub


    Private Sub logExp_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles logExp_Button.Click


        Dim intResponse As Integer
        intResponse = MsgBox("Only BATCH parameters are logged. Continue?", vbYesNo + vbExclamation, "Attention")

        If intResponse = vbYes Then
            With FolderBrowserDialog
                FolderBrowserDialog.Description = "Select folder to save BATCH experiment logs:"
                'FolderBrowserDialog.RootFolder = System.Environment.SpecialFolder.MyComputer
                If .ShowDialog = System.Windows.Forms.DialogResult.OK Then
                    logExperiment(.SelectedPath)
                End If
            End With

        End If

        'If runningState = batch Or runningState = batchstopping Then
        'End If

    End Sub

End Class