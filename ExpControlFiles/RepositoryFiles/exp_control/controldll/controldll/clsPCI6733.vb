Option Strict Off
Option Explicit On

'This class configures a NI PCI-6733 analog output card set as device "Dev1". 

Imports NationalInstruments.DAQmx
Imports System

Public Class Dev1_PCI6733
    Inherits Task

    Public Sub New()
        MyBase.New()
        Me.Configure()
        Me.Timing.ConfigureSampleClock("", 1000, SampleClockActiveEdge.Rising, SampleQuantityMode.ContinuousSamples)
        writer = New AnalogMultiChannelWriter(Me.Stream)
    End Sub

    Public Sub New(ByVal SmpClkSource As String, ByVal SmpClkRate_Hz As Double, ByVal NumSamples As Integer)
        MyBase.New()
        Me.Configure()
        Me.Timing.ConfigureSampleClock(SmpClkSource, SmpClkRate_Hz, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, NumSamples)
        writer = New AnalogMultiChannelWriter(Me.Stream)
    End Sub


    Public Sub New(ByVal SmpClkSource As String, ByVal SmpClkRate_Hz As Double, ByVal SmpClkRate_Divisor As Integer, ByVal NumSamples As Integer)
        MyBase.New()
        Me.Configure()

        Me.Timing.SampleClockTimebaseSource = SmpClkSource
        Me.Timing.SampleClockTimebaseDivisor = SmpClkRate_Divisor
        Me.Timing.SampleClockTimebaseActiveEdge = SampleClockTimebaseActiveEdge.Rising
        Me.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger("RTSI2", DigitalEdgeStartTriggerEdge.Rising)
        Me.Timing.ConfigureSampleClock("ao/SampleClockTimebase", SmpClkRate_Hz, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, NumSamples)
        writer = New AnalogMultiChannelWriter(Me.Stream)
    End Sub

    Public Sub Configure()
        Me.AOChannels.CreateVoltageChannel("Dev1/ao0", "VoltageOut0", -10, 10, AOVoltageUnits.Volts)
        Me.AOChannels.CreateVoltageChannel("Dev1/ao1", "VoltageOut1", -10, 10, AOVoltageUnits.Volts)
        Me.AOChannels.CreateVoltageChannel("Dev1/ao2", "VoltageOut2", -10, 10, AOVoltageUnits.Volts)
        Me.AOChannels.CreateVoltageChannel("Dev1/ao3", "VoltageOut3", -10, 10, AOVoltageUnits.Volts)
        Me.AOChannels.CreateVoltageChannel("Dev1/ao4", "VoltageOut4", -10, 10, AOVoltageUnits.Volts)
        Me.AOChannels.CreateVoltageChannel("Dev1/ao5", "VoltageOut5", -10, 10, AOVoltageUnits.Volts)
        Me.AOChannels.CreateVoltageChannel("Dev1/ao6", "VoltageOut6", -10, 10, AOVoltageUnits.Volts)
        Me.AOChannels.CreateVoltageChannel("Dev1/ao7", "VoltageOut7", -10, 10, AOVoltageUnits.Volts)
        'the following sets PS5 to external reference of 2.5V
        'Try
        ' Me.AOChannels.Item("VoltageOut6").DacReferenceSource = AODacReferenceSource.External
        'Me.AOChannels.Item("VoltageOut6").DacReferenceExternalSource = "EXTREF"
        'Me.AOChannels.Item("VoltageOut6").DacReferenceValue = 2.5
        'Catch ex As DaqException
        'MsgBox(ex.Message)
        'End Try

        'Dim ch7 As AOChannel
        'ch7 = Me.AOChannels(7)
        'ch7.DacReferenceSource = AODacReferenceSource.External
    End Sub

    Public Overloads Sub Start(ByVal data(,) As Double)
        writer.WriteMultiSample(True, data)
    End Sub
    Public Function InteractiveWrite(ByVal analogState() As Double, ByVal channel As Integer, ByVal val As Double) As Double()
        analogState(channel) = val
        Dim analogdata(7, 1) As Double
        Dim j As Integer
        For j = 0 To 7
            analogdata(j, 0) = analogState(j)
            analogdata(j, 1) = analogState(j)
        Next
        writer.WriteMultiSample(True, analogdata)
        Return analogState
    End Function
    Public Sub finish()
        Me.Stop()
        Me.Dispose()
    End Sub
    Private writer As AnalogMultiChannelWriter
End Class


