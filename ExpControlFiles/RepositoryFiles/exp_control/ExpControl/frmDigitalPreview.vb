Imports System
Public Class frmDigitalPreview
    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents XAxis1 As NationalInstruments.UI.XAxis
    Friend WithEvents YAxis1 As NationalInstruments.UI.YAxis
    Friend WithEvents WaveformPlot1 As NationalInstruments.UI.WaveformPlot
    Friend WithEvents grPreviewPlot As NationalInstruments.UI.WindowsForms.WaveformGraph
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents wordA As System.Windows.Forms.RadioButton
    Friend WithEvents cmdRegraph As System.Windows.Forms.Button
    Friend WithEvents WordD As System.Windows.Forms.RadioButton
    Friend WithEvents WordC As System.Windows.Forms.RadioButton
    Friend WithEvents WordB As System.Windows.Forms.RadioButton
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.grPreviewPlot = New NationalInstruments.UI.WindowsForms.WaveformGraph
        Me.WaveformPlot1 = New NationalInstruments.UI.WaveformPlot
        Me.XAxis1 = New NationalInstruments.UI.XAxis
        Me.YAxis1 = New NationalInstruments.UI.YAxis
        Me.GroupBox1 = New System.Windows.Forms.GroupBox
        Me.WordD = New System.Windows.Forms.RadioButton
        Me.WordC = New System.Windows.Forms.RadioButton
        Me.WordB = New System.Windows.Forms.RadioButton
        Me.wordA = New System.Windows.Forms.RadioButton
        Me.cmdRegraph = New System.Windows.Forms.Button
        CType(Me.grPreviewPlot, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'grPreviewPlot
        '
        Me.grPreviewPlot.Location = New System.Drawing.Point(24, 8)
        Me.grPreviewPlot.Name = "grPreviewPlot"
        Me.grPreviewPlot.Plots.AddRange(New NationalInstruments.UI.WaveformPlot() {Me.WaveformPlot1})
        Me.grPreviewPlot.Size = New System.Drawing.Size(680, 496)
        Me.grPreviewPlot.TabIndex = 0
        Me.grPreviewPlot.XAxes.AddRange(New NationalInstruments.UI.XAxis() {Me.XAxis1})
        Me.grPreviewPlot.YAxes.AddRange(New NationalInstruments.UI.YAxis() {Me.YAxis1})
        '
        'WaveformPlot1
        '
        Me.WaveformPlot1.XAxis = Me.XAxis1
        Me.WaveformPlot1.YAxis = Me.YAxis1
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.WordD)
        Me.GroupBox1.Controls.Add(Me.WordC)
        Me.GroupBox1.Controls.Add(Me.WordB)
        Me.GroupBox1.Controls.Add(Me.wordA)
        Me.GroupBox1.Location = New System.Drawing.Point(712, 16)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(88, 152)
        Me.GroupBox1.TabIndex = 1
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Display"
        '
        'WordD
        '
        Me.WordD.Location = New System.Drawing.Point(8, 120)
        Me.WordD.Name = "WordD"
        Me.WordD.Size = New System.Drawing.Size(72, 16)
        Me.WordD.TabIndex = 3
        Me.WordD.Text = "Word D"
        '
        'WordC
        '
        Me.WordC.Location = New System.Drawing.Point(8, 88)
        Me.WordC.Name = "WordC"
        Me.WordC.Size = New System.Drawing.Size(72, 16)
        Me.WordC.TabIndex = 2
        Me.WordC.Text = "Word C "
        '
        'WordB
        '
        Me.WordB.Location = New System.Drawing.Point(8, 56)
        Me.WordB.Name = "WordB"
        Me.WordB.Size = New System.Drawing.Size(72, 16)
        Me.WordB.TabIndex = 1
        Me.WordB.Text = "Word B "
        '
        'wordA
        '
        Me.wordA.Checked = True
        Me.wordA.Location = New System.Drawing.Point(8, 24)
        Me.wordA.Name = "wordA"
        Me.wordA.Size = New System.Drawing.Size(72, 16)
        Me.wordA.TabIndex = 0
        Me.wordA.TabStop = True
        Me.wordA.Text = "Word A "
        '
        'cmdRegraph
        '
        Me.cmdRegraph.Location = New System.Drawing.Point(712, 176)
        Me.cmdRegraph.Name = "cmdRegraph"
        Me.cmdRegraph.Size = New System.Drawing.Size(88, 24)
        Me.cmdRegraph.TabIndex = 2
        Me.cmdRegraph.Text = "Regraph"
        '
        'frmDigitalPreview
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(824, 518)
        Me.Controls.Add(Me.cmdRegraph)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.grPreviewPlot)
        Me.Name = "frmDigitalPreview"
        Me.Text = "Form2"
        CType(Me.grPreviewPlot, System.ComponentModel.ISupportInitialize).EndInit()
        Me.GroupBox1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region
    Public Sub makewaveform(ByRef waveform(,) As Double, ByVal channel As Integer, ByVal division As Integer)
        Dim datalength, recordlength, wfmpoints As Integer
        datalength = data.GetLength(0) - 1
        recordlength = fixsign(data(datalength, 0)) + data(datalength, 1) * 65536
        wfmpoints = Int(recordlength / division)
        ReDim waveform(15, wfmpoints)

        Dim scancount As Integer, currentvalue As Integer
        Dim mask() As Short = {1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, -32768}
        Dim ii, jj As Integer
        For ii = 0 To 15
            scancount = 0
            For jj = 0 To wfmpoints
                Do While (fixsign(data(scancount, 0)) + data(scancount, 1) * 65536 < (jj * division))
                    scancount = scancount + 1
                Loop
                If scancount = 0 Then
                    currentvalue = 0
                ElseIf (data(scancount - 1, 2 + channel) And mask(ii)) Then
                    currentvalue = 1
                Else
                    currentvalue = 0
                End If
                waveform(ii, jj) = 2 * (15 - ii) + currentvalue
            Next
        Next
    End Sub
    Public Sub displaygraph()
        Dim waveform(,) As Double
        Dim word As Integer
        Dim datalength, recordlength As Integer
        datalength = data.GetLength(0) - 1
        recordlength = fixsign(data(datalength, 0)) + data(datalength, 1) * 65536
        If wordA.Checked = True Then
            word = 0
        ElseIf WordB.Checked = True Then
            word = 1
        ElseIf WordC.Checked = True Then
            word = 2
        ElseIf WordD.Checked = True Then
            word = 3
        End If
        makewaveform(waveform, word, Math.Max(recordlength / numPoints, 1))
        Me.grPreviewPlot.PlotYMultiple(waveform)
        Me.YAxis1.Visible = False
    End Sub

    Public numPoints As Integer
    Public period_msec As Integer
    Public data(,) As Short

    Private Sub cmdRegraph_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdRegraph.Click
        displaygraph()
    End Sub
End Class
