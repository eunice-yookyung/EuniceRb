Imports controldll
Imports NationalInstruments.DAQmx
Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.ServiceModel
Imports expcontrol.SpectronServiceReference

Module modMain
    Public digitaltask As DIO64
    Public digitaltask2 As DIO64

    Public interactiveDigital As DIO64
    Public interactiveDigital2 As DIO64

    Public analogtask As Dev1_PCI6733
    Public analogtask2 As Dev2_PCI6733

    'Public interactiveAnalog As Dev1_PCI6733
    'Public interactiveAnalog2 As Dev2_PCI6733

    Dim epAddress As New EndpointAddress("http://192.168.1.135:8000/SpectronService/Spectron/SpectronService")
    Dim spectron1 As New SpectronClient(New WSHttpBinding(), epAddress)
    Dim spectron2 As New SpectronClient(New WSHttpBinding(), epAddress)

    Public digitalstate(3) As Short
    Public analogstate(7) As Double
    Public digitalstate2(3) As Short
    Public analogstate2(7) As Double

    Dim SetASmpClkRate_Hz As Integer = 10000 'Declare and initialize Analog Sample Clock Rate(PCI6733)
    Dim SetASmpClkRate2_Hz As Integer = 40000
    Public DSmpClkRate_Hz As Integer = 20000000 'SET IN HARDWARE Declare and initialize Digital Sample Clock Rate(Viewpoint Card) 
    Dim gui As New frmGUI

    Public gui_inter As New frmGUI_interactive(frmGUI)
    'Public gui_autofill As New frmGUI_autofill(frmGUI)

    Dim s As clsControlParamServer
    Dim cp As clsControlParams
    Public programLocation As String

#Region "Declarations"
    Dim DIO_board As Integer = 0

    Dim AClk_Divisor As Integer = (DSmpClkRate_Hz / SetASmpClkRate_Hz)
    Dim ASmpClkRate_Hz As Double = DSmpClkRate_Hz / AClk_Divisor
    Dim AClk_Divisor2 As Integer = (DSmpClkRate_Hz / SetASmpClkRate2_Hz)
    Dim ASmpClkRate2_Hz As Double = DSmpClkRate_Hz / AClk_Divisor2

    'Dim writer As AnalogMultiChannelWriter
    'Dim analogdata As analogDAQdata
    'Dim analogdata2 As analogDAQdata
    Dim gpib As GPIBControl
    Dim Hermes As KeithleyControl
    Dim dds As AD9959Ev

    Dim digitaldata As digitalDAQdata
    Dim digitaldata2 As digitalDAQdata

#End Region

    Sub main()
        Dim cb As New AsyncCallback(AddressOf clientsConnected)
        Dim del As New startParamServerDelegate(AddressOf startParamServer)
        Dim spectrontask1 As IntPtr
        Dim spectrontask2 As IntPtr
        Dim total_cycles As Double = 10000000

        spectron1.Open()
        spectron2.Open()

        spectrontask1 = spectron1.configure_653x("/dev1/line0:31", Nothing, "OnboardClock", total_cycles)
        spectrontask2 = spectron2.configure_653x("/dev2/line0:31", Nothing, "OnboardClock", total_cycles)

        spectron1.initialize_data(total_cycles)
        spectron2.initialize_data(total_cycles)

        del.BeginInvoke(cb, del)
        gui.ShowDialog()
    End Sub
    Sub startParamServer()
        s = New clsControlParamServer(1)
        cp = New clsControlParams()
    End Sub
    Delegate Sub startParamServerDelegate()
    Sub clientsConnected(ByVal ar As IAsyncResult)
        gui.ChooseProgramButton.Enabled = True
        gui.StatusLabel.Text = "Load experiment program..."
        gui.Refresh()
    End Sub
    Sub prepareToRun(ByVal fileName As String)
        programLocation = fileName

        'draw dt, dtloop
        gui.buildDataTables()
        gui.AutoFill.Enabled = True
        gui.AddExpsButton.Enabled = True
        gui.writeSeqButton.Enabled = True
        gui.randomizeButton.Enabled = True

        gui.RunButton.Enabled = True

        gui.RunVirtualButton.Enabled = True

        gui.StatusLabel.Text = "Ready to start next experiment..."
        Dim t As String()
        Dim sep(3) As Char
        sep(0) = "\"
        sep(1) = "."
        t = fileName.Split(sep)
        Dim x As String
        x = "Current experiment: "
        x = String.Concat(x, t(t.GetLength(0) - 2))
        gui.currentExpFileLabel.Text = x
        gui.Refresh()

    End Sub
    Sub experimentCompleted(ByVal ar As IAsyncResult)
        gui.StatusLabel.Text = "Ready to start next experiment..."
        gui.ChooseProgramButton.Enabled = True
        gui.RunButton.Enabled = True

        gui.RunVirtualButton.Enabled = True

        gui.BatchButton.Enabled = False
        gui.Refresh()
    End Sub

    Sub runExperiment()

        If (generateCode(programLocation) = -1) Then 'test compilation of the code to make sure there are no bugs in it
            Return
        End If
        s.SendRunSignal()
        gui.runningState = gui.continuous
        gui.BatchButton.Enabled = True
        gui.RunButton.Enabled = False
        gui.RunVirtualButton.Enabled = False
        gui.StopButton.Enabled = True
        gui.ChooseProgramButton.Enabled = False

        'disable interactive cmds during experiment
        gui.interactiveCmdText.BackColor = Color.Gray
        gui.interactiveCmdText.Enabled = False
        gui.interGUI_Button.BackColor = Color.Gray
        gui.interGUI_Button.Enabled = False


        gui.Refresh()
        cp.Empty()

        interactiveDigital.Close()
        'interactiveDigital2.Close()
        'interactiveAnalog.finish()
        'interactiveAnalog2.finish()
        While True
            If gui.runningState = gui.batch Then
                gui.StatusLabel.Text = "Running batch..."
                If gui.currentExpNo < gui.dt.Rows.Count Then
                    gui.currentExpNo = gui.currentExpNo + 1
                    updateBatchHighlighting()
                    updateControlParamsForBatch()
                Else
                    gui.dgv.Rows(gui.currentExpNo - 1).DefaultCellStyle.BackColor = Color.White
                    gui.currentExpNo = 1
                    updateBatchHighlighting()
                    updateControlParamsForBatch()
                End If
            End If
            If gui.runningState = gui.continuous Then
                gui.StatusLabel.Text = "Running in loop..."
                updateLoopHighlighting()
                updateControlParamsForLoop()
            End If
            generateCode(programLocation)
            gui.Refresh()

            'analogdata = New analogDAQdata(ASmpClkRate_Hz, 3) 'second argument (card #) added 11/05/2011
            'analogdata2 = New analogDAQdata(ASmpClkRate2_Hz, 4)

            digitaldata = New digitalDAQdata(DSmpClkRate_Hz)
            digitaldata2 = New digitalDAQdata(DSmpClkRate_Hz)


            gpib = New GPIBControl()
            'controller for Keithley generators
            Hermes = New KeithleyControl()
            dds = New AD9959Ev()

            runCode(cp, spectron1, spectron2, digitaldata, digitaldata2, gpib, Hermes, dds)


            'get the trigger outputs for Keithley generators
            Hermes.SetTriggerProperties()
            Dim DigitalTimes() As Double = Hermes.GetTriggerTimes()
            Dim DigitalChannels() As Integer = Hermes.GetTriggerChannel()

            'add trigger to digital channels if necessary
            If Not DigitalTimes Is Nothing Then
                For ChannelUsed As Integer = 0 To DigitalTimes.Length - 1
                    digitaldata2.AddPulse(DigitalChannels(ChannelUsed), DigitalTimes(ChannelUsed), DigitalTimes(ChannelUsed) + 50)
                Next ChannelUsed
            End If

            'run writer to write data to Keithley generators, this executes on separate thread. Will return when data processing is complete
            Hermes.RunAllWriters()

            Dim totalTime As Double
            Dim x As Double = digitaldata.GetTotalTime()
            'totalTime = Math.Max(analogdata.GetTotalTime(), digitaldata.GetTotalTime())
            'totalTime = Math.Max(analogdata2.GetTotalTime(), totalTime)
            totalTime = Math.Max(totalTime, digitaldata2.GetTotalTime())
            Dim numsamples As Integer = Int(totalTime * ASmpClkRate_Hz / 1000)
            Dim numsamples2 As Integer = Int(totalTime * ASmpClkRate2_Hz / 1000)
            digitaldata.AddPulse(5, 0, totalTime) 'gets rid of glitch at runtime

            'analogdata.Trim(numsamples)
            'analogdata2.Trim(numsamples2)

            analogtask = New Dev1_PCI6733("RTSI0", ASmpClkRate_Hz, AClk_Divisor, numsamples)
            analogtask2 = New Dev2_PCI6733("RTSI0", ASmpClkRate2_Hz, AClk_Divisor2, numsamples2)
            digitaltask = New DIO64(0, DSmpClkRate_Hz, 1)
            digitaltask2 = New DIO64(1, DSmpClkRate_Hz, 0)

            digitaltask2.Start(digitaldata2.data)
            'analogtask.Start(analogdata.data)
            'analogtask2.Start(analogdata2.data)
            digitaltask.Start(digitaldata.data)
            analogtask.WaitUntilDone(totalTime + 100)

            ''alex test
            'Dim softScope As New clsSoftscope
            'softScope.setSoftscopeData(analogdata, analogdata2, digitaldata, digitaldata2)

            s.SendControlParam(cp)
            If gui.runningState = gui.stopping Then
                s.SendMoreExpAvailable(False)
                analogtask.finish()
                analogtask2.finish()
                'digitaltask.Finish()
                'digitaltask2.Finish()
                Exit While
            Else
                If gui.runningState = gui.batchstopping Then
                    gui.runningState = gui.continuous
                    gui.StopBatchButton.Enabled = False
                    gui.BatchButton.Enabled = True
                    gui.dgv.Rows(gui.currentExpNo - 1).DefaultCellStyle.BackColor = Color.White
                    gui.currentExpNo = 0
                    gui.Refresh()
                End If
                s.SendMoreExpAvailable(True)
                'analogtask.finish()
                'analogtask2.finish()
                'digitaltask.Finish()
                'digitaltask2.Finish()
            End If
        End While
        gui.runningState = gui.stopped
        interactiveDigital.initialize()
        'interactiveDigital2.initialize()
        'interactiveAnalog = New Dev1_PCI6733()
        'interactiveAnalog2 = New Dev2_PCI6733()

        'enable interactive cmds
        gui.interactiveCmdText.Enabled = True
        gui.interactiveCmdText.BackColor = Color.White
        gui.interGUI_Button.Enabled = True
        gui.interGUI_Button.BackColor = Color.White
        gui.Refresh()

    End Sub
    Sub runBatch()
        gui.runningState = gui.batch
        gui.BatchButton.Enabled = False
        gui.StopBatchButton.Enabled = True
        gui.Refresh()
    End Sub

    'added oct 2011 alex: copy from runExperiment, for softscope
    Sub runVirtualExperiment()

        If (generateCode(programLocation) = -1) Then 'test compilation of the code to make sure there are no bugs in it
            Return
        End If

        'save folder name, saved to C:\softscope_output\..\.. 
        Dim saveFolderName As String = (InputBox("Save traces to a folder named:", "Softscope output folder", "default")).Trim()
        If saveFolderName.Length = 0 Then 'cancel pressed or empty input
            Return
        End If

        'just run once, i.e. equiv. to gui.runningState = gui.stopping
        gui.BatchButton.Enabled = False
        gui.RunButton.Enabled = False
        gui.RunVirtualButton.Enabled = False
        gui.RunVirtualButton.Text = "  Running..."
        gui.StopButton.Enabled = False
        gui.ChooseProgramButton.Enabled = False
        gui.StatusLabel.Text = "Running virtual experiment..."

        'disable interactive cmds during experiment
        gui.interactiveCmdText.BackColor = Color.Gray
        gui.interactiveCmdText.Enabled = False
        gui.interGUI_Button.BackColor = Color.Gray
        gui.interGUI_Button.Enabled = False

        interactiveDigital.Close()
        'interactiveDigital2.Close()
        'interactiveAnalog.finish()
        'interactiveAnalog2.finish()

        generateCode(programLocation)
        gui.Refresh()

        'analogdata = New analogDAQdata(ASmpClkRate_Hz, 3) 'second argument (card #) added May 2011
        'analogdata2 = New analogDAQdata(ASmpClkRate2_Hz, 4)
        digitaldata = New digitalDAQdata(DSmpClkRate_Hz)
        digitaldata2 = New digitalDAQdata(DSmpClkRate_Hz)
        gpib = New GPIBControl()
        'controller for Keithley generators
        Hermes = New KeithleyControl()

        dds = New AD9959Ev()

        'runCode(cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds)

        Dim totalTime As Double
        Dim x As Double = digitaldata.GetTotalTime()
        'totalTime = Math.Max(analogdata.GetTotalTime(), digitaldata.GetTotalTime())
        'totalTime = Math.Max(analogdata2.GetTotalTime(), totalTime)
        totalTime = Math.Max(totalTime, digitaldata2.GetTotalTime())
        Dim numsamples As Integer = Int(totalTime * ASmpClkRate_Hz / 1000)
        Dim numsamples2 As Integer = Int(totalTime * ASmpClkRate2_Hz / 1000)
        digitaldata.AddPulse(5, 0, totalTime) 'gets rid of glitch at runtime

        'analogdata.Trim(numsamples)
        'analogdata2.Trim(numsamples2)

        gui.RunVirtualButton.Text = "  Writing..."

        'alex: softscope
        Dim softScope As New clsSoftscope(totalTime, saveFolderName)
        'softScope.writeSoftscopeData(analogdata, analogdata2, digitaldata, digitaldata2, Hermes)

        gui.RunVirtualButton.Text = "  Finishing..."


        gui.runningState = gui.stopped
        interactiveDigital.initialize()
        'interactiveDigital2.initialize()
        'interactiveAnalog = New Dev1_PCI6733()
        'interactiveAnalog2 = New Dev2_PCI6733()

        'enable interactive cmds
        gui.interactiveCmdText.Enabled = True
        gui.interactiveCmdText.BackColor = Color.White
        gui.interGUI_Button.Enabled = True
        gui.interGUI_Button.BackColor = Color.White

        gui.RunVirtualButton.Text = "  Run Virtual"

        gui.Refresh()

    End Sub


    'added dec28 2010 by alex: program logging
    Sub logExperiment(ByVal log_Dir As String)

        log_Dir = log_Dir + "\"
        logCode(programLocation, log_Dir, gui.dt)

    End Sub

    Delegate Sub runExperimentDelegate()
    Sub updateBatchHighlighting()
        gui.dgv.FirstDisplayedScrollingRowIndex = Math.Floor((gui.currentExpNo - 1) / 20.0) * 20
        gui.dgv.Rows(gui.currentExpNo - 1).DefaultCellStyle.BackColor = Color.LightGreen
        If gui.currentExpNo > 1 Then
            gui.dgv.Rows(gui.currentExpNo - 2).DefaultCellStyle.BackColor = Color.White
        End If
        gui.dgv.Refresh()
    End Sub
    Sub updateLoopHighlighting()
        gui.dgvloop.Rows(0).DefaultCellStyle.BackColor = Color.LightGreen
        gui.dgvloop.Refresh()
    End Sub
    Sub updateControlParamsForBatch()
        Dim arrList As ArrayList
        arrList = modUtilities.GetExpVariables()
        Dim var As Object
        Dim counter As Integer = 0
        For Each var In arrList
            cp.Put(var.ToString(), Double.Parse(gui.dt.Rows(gui.currentExpNo - 1).Item(counter).ToString()))
            counter = counter + 1
        Next
    End Sub
    Sub updateControlParamsForLoop()
        Dim arrList As ArrayList
        arrList = modUtilities.GetExpVariables()
        Dim var As Object
        Dim counter As Integer = 0
        For Each var In arrList
            cp.Put(var.ToString(), Double.Parse(gui.dtloop.Rows(0).Item(counter).ToString()))
            counter = counter + 1
        Next
    End Sub
End Module
