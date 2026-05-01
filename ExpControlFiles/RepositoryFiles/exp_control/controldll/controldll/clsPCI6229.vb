Option Strict Off
Option Explicit On

'This class configures a NI PCI-6733 analog output card set as device "Dev1". 
Imports NationalInstruments.DAQmx
Imports System

Public Class Dev2_PCI6229
    Inherits Task

    Public Sub New()
        MyBase.New()
        Me.Configure()
        Me.Timing.ConfigureSampleClock("", 1000, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, 100)
    End Sub

    Public Sub New(ByVal SmpClkSource As String, ByVal SmpClkRate_Hz As Double, ByVal NumSamples As Integer)
        MyBase.New()
        Me.Configure()
        Me.Timing.ConfigureSampleClock(SmpClkSource, SmpClkRate_Hz, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, NumSamples)
    End Sub

    Public Sub Configure()
        Dim ch As DOChannel = Me.DOChannels.CreateChannel("Dev2/port0", "DigitalOut", ChannelLineGrouping.OneChannelForAllLines)
        ch.InvertLines = False
    End Sub

    Public Overloads Sub Start(ByVal data(,) As Integer)
        writer = New DigitalMultiChannelWriter(Me.Stream)
        writer.WriteMultiSamplePort(True, data)
    End Sub
    Public Sub finish()
        Me.Stop()
        Me.Dispose()
    End Sub
    Private writer As DigitalMultiChannelWriter
End Class


