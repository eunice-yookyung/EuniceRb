Public Class frmAnalogPreview
    Inherits System.Windows.Forms.Form
    Friend WithEvents XAxis1 As NationalInstruments.UI.XAxis
    Friend WithEvents YAxis1 As NationalInstruments.UI.YAxis
    Friend WithEvents WaveformPlot1 As NationalInstruments.UI.WaveformPlot
    Friend WithEvents grPreviewPlot As NationalInstruments.UI.WindowsForms.WaveformGraph

    Public Sub New()

    End Sub
    Friend WithEvents WaveformPlot2 As NationalInstruments.UI.WaveformPlot
    Friend WithEvents WaveformPlot3 As NationalInstruments.UI.WaveformPlot
    Friend WithEvents WaveformPlot4 As NationalInstruments.UI.WaveformPlot
    Friend WithEvents WaveformPlot5 As NationalInstruments.UI.WaveformPlot
    Friend WithEvents WaveformPlot6 As NationalInstruments.UI.WaveformPlot
    Friend WithEvents WaveformPlot7 As NationalInstruments.UI.WaveformPlot
    Friend WithEvents WaveformPlot8 As NationalInstruments.UI.WaveformPlot
    Friend WithEvents grpView As System.Windows.Forms.GroupBox
    Friend WithEvents chkAO7 As System.Windows.Forms.CheckBox
    Friend WithEvents chkAO6 As System.Windows.Forms.CheckBox
    Friend WithEvents chkAO5 As System.Windows.Forms.CheckBox
    Friend WithEvents chkAO4 As System.Windows.Forms.CheckBox
    Friend WithEvents chkAO3 As System.Windows.Forms.CheckBox
    Friend WithEvents chkAO2 As System.Windows.Forms.CheckBox
    Friend WithEvents chkAO1 As System.Windows.Forms.CheckBox
    Friend WithEvents chkAO0 As System.Windows.Forms.CheckBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents txtXMin As System.Windows.Forms.TextBox
    Friend WithEvents txtXMax As System.Windows.Forms.TextBox



    Public Sub InitializeComponent()
        Me.grPreviewPlot = New NationalInstruments.UI.WindowsForms.WaveformGraph
        Me.WaveformPlot1 = New NationalInstruments.UI.WaveformPlot
        Me.XAxis1 = New NationalInstruments.UI.XAxis
        Me.YAxis1 = New NationalInstruments.UI.YAxis
        Me.WaveformPlot2 = New NationalInstruments.UI.WaveformPlot
        Me.WaveformPlot3 = New NationalInstruments.UI.WaveformPlot
        Me.WaveformPlot4 = New NationalInstruments.UI.WaveformPlot
        Me.WaveformPlot5 = New NationalInstruments.UI.WaveformPlot
        Me.WaveformPlot6 = New NationalInstruments.UI.WaveformPlot
        Me.WaveformPlot7 = New NationalInstruments.UI.WaveformPlot
        Me.WaveformPlot8 = New NationalInstruments.UI.WaveformPlot
        Me.grpView = New System.Windows.Forms.GroupBox
        Me.chkAO7 = New System.Windows.Forms.CheckBox
        Me.chkAO6 = New System.Windows.Forms.CheckBox
        Me.chkAO5 = New System.Windows.Forms.CheckBox
        Me.chkAO4 = New System.Windows.Forms.CheckBox
        Me.chkAO3 = New System.Windows.Forms.CheckBox
        Me.chkAO2 = New System.Windows.Forms.CheckBox
        Me.chkAO1 = New System.Windows.Forms.CheckBox
        Me.chkAO0 = New System.Windows.Forms.CheckBox
        Me.txtXMin = New System.Windows.Forms.TextBox
        Me.txtXMax = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.Button1 = New System.Windows.Forms.Button
        CType(Me.grPreviewPlot, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.grpView.SuspendLayout()
        Me.SuspendLayout()
        '
        'grPreviewPlot
        '
        Me.grPreviewPlot.CanShowFocus = True
        Me.grPreviewPlot.ImeMode = System.Windows.Forms.ImeMode.NoControl
        Me.grPreviewPlot.Location = New System.Drawing.Point(8, 8)
        Me.grPreviewPlot.Name = "grPreviewPlot"
        Me.grPreviewPlot.PlotAreaColor = System.Drawing.Color.White
        Me.grPreviewPlot.Plots.AddRange(New NationalInstruments.UI.WaveformPlot() {Me.WaveformPlot1, Me.WaveformPlot2, Me.WaveformPlot3, Me.WaveformPlot4, Me.WaveformPlot5, Me.WaveformPlot6, Me.WaveformPlot7, Me.WaveformPlot8})
        Me.grPreviewPlot.SelectionColor = System.Drawing.SystemColors.Highlight
        Me.grPreviewPlot.Size = New System.Drawing.Size(552, 408)
        Me.grPreviewPlot.TabIndex = 0
        Me.grPreviewPlot.XAxes.AddRange(New NationalInstruments.UI.XAxis() {Me.XAxis1})
        Me.grPreviewPlot.YAxes.AddRange(New NationalInstruments.UI.YAxis() {Me.YAxis1})
        '
        'WaveformPlot1
        '
        Me.WaveformPlot1.XAxis = Me.XAxis1
        Me.WaveformPlot1.YAxis = Me.YAxis1
        '
        'WaveformPlot2
        '
        Me.WaveformPlot2.LineColor = System.Drawing.Color.Red
        Me.WaveformPlot2.XAxis = Me.XAxis1
        Me.WaveformPlot2.YAxis = Me.YAxis1
        '
        'WaveformPlot3
        '
        Me.WaveformPlot3.LineColor = System.Drawing.Color.Cyan
        Me.WaveformPlot3.XAxis = Me.XAxis1
        Me.WaveformPlot3.YAxis = Me.YAxis1
        '
        'WaveformPlot4
        '
        Me.WaveformPlot4.LineColor = System.Drawing.Color.Yellow
        Me.WaveformPlot4.XAxis = Me.XAxis1
        Me.WaveformPlot4.YAxis = Me.YAxis1
        '
        'WaveformPlot5
        '
        Me.WaveformPlot5.LineColor = System.Drawing.Color.Indigo
        Me.WaveformPlot5.XAxis = Me.XAxis1
        Me.WaveformPlot5.YAxis = Me.YAxis1
        '
        'WaveformPlot6
        '
        Me.WaveformPlot6.LineColor = System.Drawing.Color.Orange
        Me.WaveformPlot6.XAxis = Me.XAxis1
        Me.WaveformPlot6.YAxis = Me.YAxis1
        '
        'WaveformPlot7
        '
        Me.WaveformPlot7.LineColor = System.Drawing.Color.Black
        Me.WaveformPlot7.XAxis = Me.XAxis1
        Me.WaveformPlot7.YAxis = Me.YAxis1
        '
        'WaveformPlot8
        '
        Me.WaveformPlot8.LineColor = System.Drawing.Color.Magenta
        Me.WaveformPlot8.XAxis = Me.XAxis1
        Me.WaveformPlot8.YAxis = Me.YAxis1
        '
        'grpView
        '
        Me.grpView.Controls.Add(Me.chkAO7)
        Me.grpView.Controls.Add(Me.chkAO6)
        Me.grpView.Controls.Add(Me.chkAO5)
        Me.grpView.Controls.Add(Me.chkAO4)
        Me.grpView.Controls.Add(Me.chkAO3)
        Me.grpView.Controls.Add(Me.chkAO2)
        Me.grpView.Controls.Add(Me.chkAO1)
        Me.grpView.Controls.Add(Me.chkAO0)
        Me.grpView.Location = New System.Drawing.Point(576, 8)
        Me.grpView.Name = "grpView"
        Me.grpView.Size = New System.Drawing.Size(72, 280)
        Me.grpView.TabIndex = 10
        Me.grpView.TabStop = False
        Me.grpView.Text = "View"
        '
        'chkAO7
        '
        Me.chkAO7.Checked = True
        Me.chkAO7.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAO7.Location = New System.Drawing.Point(8, 248)
        Me.chkAO7.Name = "chkAO7"
        Me.chkAO7.Size = New System.Drawing.Size(144, 32)
        Me.chkAO7.TabIndex = 7
        Me.chkAO7.Text = "AO7"
        '
        'chkAO6
        '
        Me.chkAO6.Checked = True
        Me.chkAO6.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAO6.Location = New System.Drawing.Point(8, 216)
        Me.chkAO6.Name = "chkAO6"
        Me.chkAO6.Size = New System.Drawing.Size(144, 32)
        Me.chkAO6.TabIndex = 6
        Me.chkAO6.Text = "AO6"
        '
        'chkAO5
        '
        Me.chkAO5.Checked = True
        Me.chkAO5.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAO5.Location = New System.Drawing.Point(8, 184)
        Me.chkAO5.Name = "chkAO5"
        Me.chkAO5.Size = New System.Drawing.Size(144, 32)
        Me.chkAO5.TabIndex = 5
        Me.chkAO5.Text = "AO5"
        '
        'chkAO4
        '
        Me.chkAO4.Checked = True
        Me.chkAO4.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAO4.Location = New System.Drawing.Point(8, 152)
        Me.chkAO4.Name = "chkAO4"
        Me.chkAO4.Size = New System.Drawing.Size(144, 32)
        Me.chkAO4.TabIndex = 4
        Me.chkAO4.Text = "AO4"
        '
        'chkAO3
        '
        Me.chkAO3.Checked = True
        Me.chkAO3.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAO3.Location = New System.Drawing.Point(8, 120)
        Me.chkAO3.Name = "chkAO3"
        Me.chkAO3.Size = New System.Drawing.Size(144, 32)
        Me.chkAO3.TabIndex = 3
        Me.chkAO3.Text = "AO3"
        '
        'chkAO2
        '
        Me.chkAO2.Checked = True
        Me.chkAO2.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAO2.Location = New System.Drawing.Point(8, 88)
        Me.chkAO2.Name = "chkAO2"
        Me.chkAO2.Size = New System.Drawing.Size(144, 32)
        Me.chkAO2.TabIndex = 2
        Me.chkAO2.Text = "AO2"
        '
        'chkAO1
        '
        Me.chkAO1.Checked = True
        Me.chkAO1.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAO1.Location = New System.Drawing.Point(8, 56)
        Me.chkAO1.Name = "chkAO1"
        Me.chkAO1.Size = New System.Drawing.Size(144, 32)
        Me.chkAO1.TabIndex = 1
        Me.chkAO1.Text = "AO1"
        '
        'chkAO0
        '
        Me.chkAO0.Checked = True
        Me.chkAO0.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkAO0.Location = New System.Drawing.Point(8, 24)
        Me.chkAO0.Name = "chkAO0"
        Me.chkAO0.Size = New System.Drawing.Size(144, 32)
        Me.chkAO0.TabIndex = 0
        Me.chkAO0.Text = "AO0"
        '
        'txtXMin
        '
        Me.txtXMin.Location = New System.Drawing.Point(576, 312)
        Me.txtXMin.Name = "txtXMin"
        Me.txtXMin.Size = New System.Drawing.Size(64, 20)
        Me.txtXMin.TabIndex = 11
        Me.txtXMin.Text = "0"
        '
        'txtXMax
        '
        Me.txtXMax.Location = New System.Drawing.Point(576, 360)
        Me.txtXMax.Name = "txtXMax"
        Me.txtXMax.Size = New System.Drawing.Size(64, 20)
        Me.txtXMax.TabIndex = 12
        Me.txtXMax.Text = "0"
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(576, 288)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(64, 16)
        Me.Label1.TabIndex = 13
        Me.Label1.Text = "Xmin"
        '
        'Label2
        '
        Me.Label2.Location = New System.Drawing.Point(576, 336)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(64, 16)
        Me.Label2.TabIndex = 14
        Me.Label2.Text = "Xmax"
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(576, 392)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(64, 24)
        Me.Button1.TabIndex = 15
        Me.Button1.Text = "Regraph"
        '
        'frmAnalogPreview
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(656, 430)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtXMax)
        Me.Controls.Add(Me.txtXMin)
        Me.Controls.Add(Me.grPreviewPlot)
        Me.Controls.Add(Me.grpView)
        Me.Name = "frmAnalogPreview"
        Me.Text = "Analog Preview"
        CType(Me.grPreviewPlot, System.ComponentModel.ISupportInitialize).EndInit()
        Me.grpView.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Public Sub displaygraph()
        Dim numofplots As Integer = 0
        Dim imin, imax As Integer
        Dim isplotted(7) As Boolean
        Dim check As CheckBox
        Dim ctrl As Control
        For Each ctrl In grpView.Controls
            If TypeOf (ctrl) Is CheckBox Then
                check = ctrl
                isplotted(Val(check.Name.Chars(5))) = (check.CheckState = CheckState.Checked)
                If (check.CheckState = CheckState.Checked) Then numofplots = numofplots + 1
            End If
        Next
        imin = Int(CDbl(Val(txtXMin.Text)) / period_msec)
        imax = Int(CDbl(Val(txtXMax.Text)) / period_msec)

        Dim tempdata(numofplots - 1, (imax - imin - 1)) As Double

        Dim ii, jj As Integer
        Dim count As Integer = 0
        For ii = 0 To 7
            If isplotted(ii) Then
                For jj = 0 To (imax - imin - 1)
                    tempdata(count, jj) = data(ii, imin + jj)
                Next
                count = count + 1
            End If

        Next
        Me.grPreviewPlot.ClearData()
        Me.grPreviewPlot.PlotYMultiple(tempdata)
    End Sub

    Public data(,) As Double
    Public period_msec As Double

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        displaygraph()
    End Sub
End Class
