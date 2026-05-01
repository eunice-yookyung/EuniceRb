Imports System.Threading
Public Class KeithleyControl

    'class parameters
    Private WritingThreads() As Thread
    Private Keithley() As KeithleyGenerator

    'number of devices
    'ip address starts with Hermes 0
    Private Shared IPAddress() As String = {"TCPIP::192.168.1.3::INSTR", _
                                            "TCPIP::192.168.1.2::INSTR", _
                                            "TCPIP::192.168.1.4::INSTR", _
                                            "TCPIP::192.168.1.11::INSTR", _
                                            "TCPIP::192.168.1.82::INSTR", _
                                            "TCPIP::192.168.1.83::INSTR", _
                                            "TCPIP::192.168.1.17::INSTR"} ', _
                                            '"TCPIP::192.168.1.143::INSTR"}

    Public Shared TotalDevice As Integer = IPAddress.Length()

    Private AllSampleFrequency As Double
    Public Delegate Sub DoneDelegate()
    Public WriteDelegate() As DoneDelegate
    Private DoneNess As Integer
    'parameters for triggering device
    Private TriggerTimes() As Double
    Private TriggerChannelUsed() As Integer
    Private TriggerChannelAll() As Integer
    Private TriggerOffSet As Integer
    'array to keep index of channel used
    Private ChannelUsed() As Integer


    Public ReadOnly Property GetDoneNess()
        Get
            Return DoneNess
        End Get
    End Property

    Public ReadOnly Property GetIPAddress()
        Get
            Return IPAddress
        End Get
    End Property


    Public ReadOnly Property GetTriggerTimes()
        'return trigger times of all used channel
        Get
            Return TriggerTimes
        End Get
    End Property

    Public ReadOnly Property GetTriggerChannel()
        'return trigger channel of all used device channel
        Get
            Return TriggerChannelUsed
        End Get
    End Property

    Public Sub New()
        'use this constructor for run code operation

        'set global sampling frequency in Hz
        AllSampleFrequency = 20000
        'initiate instances of device and attach correct threads and delegates to notify when done writing
        ReDim Keithley(TotalDevice - 1)
        ReDim WritingThreads(TotalDevice - 1)
        ReDim WriteDelegate(TotalDevice - 1)
        For i As Integer = 0 To TotalDevice - 1
            WriteDelegate(i) = New DoneDelegate(AddressOf SignalWhenDone)
            Keithley(i) = New KeithleyGenerator(AllSampleFrequency, IPAddress(i), WriteDelegate(i))
            WritingThreads(i) = New Thread(AddressOf Keithley(i).WaveFormWrite)
        Next i
        'this number is incremented as each device finish writing
        DoneNess = 0
        'initialise array to store trigger channels
        ReDim TriggerChannelAll(TotalDevice - 1)
        'starting data point of all device signal
        TriggerOffSet = 5
    End Sub

    Public Sub New(ByVal channel As Integer)
        'set global sampling frequency in Hz
        AllSampleFrequency = 20000
        'initiate instances of device and attach correct threads and delegates to notify when done writing
        ReDim Keithley(TotalDevice - 1)
        ReDim WritingThreads(TotalDevice - 1)
        ReDim WriteDelegate(TotalDevice - 1)
        WriteDelegate(channel) = New DoneDelegate(AddressOf SignalWhenDone)
        Keithley(channel) = New KeithleyGenerator(AllSampleFrequency, IPAddress(channel), WriteDelegate(channel))
        WritingThreads(channel) = New Thread(AddressOf Keithley(channel).WaveFormWrite)
        'this number is incremented as each device finish writing
        DoneNess = -5
    End Sub

    'define corresponding add waveform functions
    Public Sub clear(ByVal channel As Integer)
        Keithley(channel).ClearDevice()
    End Sub
    Public Sub AddStep(ByVal step_volts As Double, ByVal tstart_msec As Double, _
                       ByVal tstop_msec As Double, ByVal channel As Integer)
        Keithley(channel).AddStep(step_volts, tstart_msec, tstop_msec)
    End Sub
    Public Sub AddExpAndRamp(ByVal base_volts As Double, ByVal offset_volts As Double, _
                             ByVal tstart_msec As Double, ByVal tstop_msec As Double, _
                             ByVal tramp_msec As Double, ByVal timeconst_msec As Double, _
                             ByVal start_volts As Double, ByVal stop_volts As Double, ByVal channel As Integer)
        Keithley(channel).AddExpAndRamp(base_volts, offset_volts, tstart_msec, tstop_msec, tramp_msec, timeconst_msec, start_volts, stop_volts)
    End Sub

    Public Sub AddRamp(ByVal start_volts As Double, ByVal stop_volts As Double, ByVal tstart_msec As Double, _
                         ByVal tstop_msec As Double, ByVal channel As Integer)
        Keithley(channel).AddRamp(start_volts, stop_volts, tstart_msec, tstop_msec)
    End Sub

    Public Sub AddSmoothRamp(ByVal start_volts As Double, ByVal stop_volts As Double, ByVal tstart_msec As Double, _
                             ByVal tstop_msec As Double, ByVal channel As Integer)
        Keithley(channel).AddSmoothRamp(start_volts, stop_volts, tstart_msec, tstop_msec)
    End Sub

    Public Sub AddExp(ByVal base_volts As Double, ByVal offset_volts As Double, ByVal tstart_msec As Double, _
                        ByVal tstop_msec As Double, ByVal timeconst_msec As Double, ByVal channel As Integer)
        Keithley(channel).AddExp(base_volts, offset_volts, tstart_msec, tstop_msec, timeconst_msec)
    End Sub
    Public Sub AddSine(ByVal offset_volts As Double, ByVal amp_volts As Double, ByVal freq_Hz As Double, _
                        ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer)
        Keithley(channel).AddSine(offset_volts, amp_volts, freq_Hz, tstart_msec, tstop_msec)
    End Sub
    Public Sub AddFromFile(ByVal filename As String, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer)
        Keithley(channel).AddFromFile(filename, tstart_msec, tstop_msec)
    End Sub

    Public Sub AddTrigger(ByVal TriggerChannel As Integer, ByVal DeviceChannel As Integer)
        'set the trigger channel of corresponding device channel
        TriggerChannelAll(DeviceChannel) = TriggerChannel
    End Sub

    Private Function GetTotalTime() As Double
        Dim maxTime As Double = 0
        For i As Integer = 0 To TotalDevice - 1
            maxTime = Math.Max(maxTime, Keithley(i).GetTotalTime())
        Next i
        Return maxTime
    End Function

    Public Sub SetTriggerProperties()
        'this sets the trigger properties of all used channels and populates array containing used channels
        Dim NumberOfUsedChannel As Integer = 0
        Dim startTime As Double = 0
        For channel As Integer = 0 To TotalDevice - 1
            'run waveform settings to set write settings
            Keithley(channel).WaveFormSettings()
            'determine if channel is used
            If Keithley(channel).GetAmp() <> 0 Then
                'channel is used
                ReDim Preserve TriggerTimes(NumberOfUsedChannel)
                ReDim Preserve TriggerChannelUsed(NumberOfUsedChannel)
                ReDim Preserve ChannelUsed(NumberOfUsedChannel)
                'set trigger times
                startTime = Keithley(channel).GetStartTime()
                If startTime > TriggerOffSet / AllSampleFrequency * 1000 Then
                    TriggerTimes(UBound(TriggerTimes)) = startTime - TriggerOffSet / AllSampleFrequency
                Else
                    TriggerTimes(UBound(TriggerTimes)) = startTime
                End If

                'set corresponding trigger channel
                'throw exception of trigger channel is not defined
                If TriggerChannelAll(channel) = 0 Then
                    Throw New System.Exception("Must define a non zero trigger channel")
                Else
                    TriggerChannelUsed(UBound(TriggerChannelUsed)) = TriggerChannelAll(channel)
                End If
                'record this channel as used
                ChannelUsed(UBound(ChannelUsed)) = channel
                'move starting point to beginning of signal
                Keithley(channel).TrimBegin(TriggerOffSet)
                'Get rid of redundant points after signal has passed
                Keithley(channel).TrimEnd(Int(Keithley(channel).GetTotalTime() * AllSampleFrequency / 1000))
                'increment counter for number of used channels
                NumberOfUsedChannel = NumberOfUsedChannel + 1
            End If
        Next channel
        'pre-process data before writing
        'move starting point of signal
        If ChannelUsed Is Nothing Then
            Exit Sub
        End If
    End Sub

    Public Sub RunAllWriters()
        'SetTriggerProperties must be run before this
        'exit immediately if no channels are used
        If ChannelUsed Is Nothing Then
            Exit Sub
        End If
        'execute write on separate threads
        For i As Integer = 0 To ChannelUsed.Length - 1
            WritingThreads(ChannelUsed(i)).Start()
        Next i
    End Sub

    Public Sub RunSingleWriter(ByVal channel As Integer)
        'use this for interactive control, writes to a single device only
        Dim TrimToPoints As Integer = Int(Keithley(channel).GetTotalTime() * AllSampleFrequency / 1000)
        If TrimToPoints < 262144 Then
            Keithley(channel).TrimEnd(TrimToPoints + 10)
        End If
        WritingThreads(channel).Start()
    End Sub

    Public Sub SignalWhenDone()
        'handles event when all writing is done
        DoneNess = DoneNess + 1
    End Sub

    'added oct 2011 for softscope
    Public ReadOnly Property SampleFrequency()
        Get
            Return AllSampleFrequency
        End Get
    End Property

    Public Function KeithleyData(ByVal channel As Integer) As Double()
        'channel: Hermes channel number, start from 0
        Return (Keithley(channel).GetData())
    End Function

End Class
