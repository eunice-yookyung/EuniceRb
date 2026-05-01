Imports NationalInstruments.VisaNS
Imports System.Text
Imports System.Threading

Public Class KeithleyGenerator

    'class parameters listed here
    'sample rate of generator in Hz
    Private SampleClockRate_Hz As Double
    'a list associating the four channels to their corresponding IP address for commnication
    Private IPAddresses As String
    'maximum number of samples to each device
    Private NumberOfSamples As Integer
    'a 2D array that stores data to write, first index controls the device ID
    Private DataArray() As Double
    'a 1D array corresponds to amplitude setting of each channel
    Private Amp As Double
    'a 1D array corresponding to offset setting for each channel
    Private Offset As Double
    'scale parameter for writing waveform
    Private ScaleNumber As Double
    'message based VISA session for communicating to device
    Private NewSession As MessageBasedSession
    Private DoneDelegate As KeithleyControl.DoneDelegate

    Public ReadOnly Property GetAmp() As Double
        Get
            Return Amp
        End Get
    End Property

    'added oct 2011 for softscope
    Public ReadOnly Property GetData() As Double()
        Get
            Return DataArray
        End Get
    End Property


    Public Sub New(ByVal smpClkRate_Hz As Double, ByVal ipaddress As String, ByVal Handler As KeithleyControl.DoneDelegate)
        'class constructor
        'set sample clock rate, this define frequency of the arbitrary waveform for a given sample number
        SampleClockRate_Hz = smpClkRate_Hz
        'set number of samples to maximum allowed per waveform
        NumberOfSamples = 2800000
        'assocating channels with corresponding IP address
        IPAddresses = ipaddress
        ReDim DataArray(NumberOfSamples - 1)
        Amp = 0
        Offset = 0
        DoneDelegate = Handler
    End Sub

    Public Sub AddStep(ByVal step_volts As Double, ByVal tstart_msec As Double, _
                       ByVal tstop_msec As Double)
        'subroutine to add a step to the waveform, overwrite existing signal
        'this calculate the sample positions where step exists
        Dim StartIndex As Integer = Int(tstart_msec * SampleClockRate_Hz / 1000)
        Dim StopIndex As Integer = Int(tstop_msec * SampleClockRate_Hz / 1000 - 0.0001)
        For nn As Integer = StartIndex To StopIndex
            DataArray(nn) = step_volts
        Next nn
    End Sub

    Public Sub AddExpAndRamp(ByVal base_volts As Double, ByVal offset_volts As Double, _
                             ByVal tstart_msec As Double, ByVal tstop_msec As Double, _
                             ByVal tramp_msec As Double, ByVal timeconst_msec As Double, _
                             ByVal start_volts As Double, ByVal stop_volts As Double)
        'subroutine to add an exponential ramp to waveform, overwrite existing signal
        Dim StartIndex As Integer = Int(tstart_msec * SampleClockRate_Hz / 1000)
        Dim StopIndex As Integer = Int(tstop_msec * SampleClockRate_Hz / 1000)
        Dim RampIndex As Integer = Int(tramp_msec * SampleClockRate_Hz / 1000)
        For nn As Integer = StartIndex To StartIndex + RampIndex
            DataArray(nn) = start_volts + (stop_volts - start_volts) * (nn - StartIndex) / RampIndex + _
            offset_volts - base_volts * Math.Exp((tstop_msec - tstart_msec) * (nn - StartIndex) / (StopIndex - StartIndex) / timeconst_msec)
        Next nn
        For nn As Integer = StartIndex + RampIndex + 1 To StopIndex
            DataArray(nn) = stop_volts + offset_volts - base_volts * Math.Exp((tstop_msec - tstart_msec) * _
                                                                                     (nn - StartIndex) / (StopIndex - StartIndex) / timeconst_msec)
        Next nn
    End Sub

    Public Sub AddRamp(ByVal start_volts As Double, ByVal stop_volts As Double, ByVal tstart_msec As Double, _
                       ByVal tstop_msec As Double)
        'subroutine to add ramp to waveform, overwrite existing signal
        Dim StartIndex As Integer = Int(tstart_msec * SampleClockRate_Hz / 1000)
        Dim StopIndex As Integer = Int(tstop_msec * SampleClockRate_Hz / 1000)
        For nn As Integer = StartIndex To StopIndex
            DataArray(nn) = start_volts + (stop_volts - start_volts) * (nn - StartIndex) / (StopIndex - StartIndex)
        Next nn
    End Sub

    Public Sub AddSmoothRamp(ByVal start_volts As Double, ByVal stop_volts As Double, ByVal tstart_msec As Double, _
                             ByVal tstop_msec As Double)
        'subroutine to add a smooth ramp to waveform, overwrite existing signal
        Dim StartIndex As Integer = Int(tstart_msec * SampleClockRate_Hz / 1000)
        Dim StopIndex As Integer = Int(tstop_msec * SampleClockRate_Hz / 1000)
        Dim x, f As Double
        For nn As Integer = StartIndex To StopIndex
            x = (nn - StartIndex) / (StopIndex - StartIndex)
            f = 10 * Math.Pow(x, 3) - 15 * Math.Pow(x, 4) + 6 * Math.Pow(x, 5)
            DataArray(nn) = start_volts + (stop_volts - start_volts) * f
        Next nn
    End Sub

    Public Sub AddExp(ByVal base_volts As Double, ByVal offset_volts As Double, ByVal tstart_msec As Double, _
                      ByVal tstop_msec As Double, ByVal timeconst_msec As Double)
        'subroutine to add exponential to waveform, overwrite existing signal
        Dim StartIndex As Integer = Int(tstart_msec * SampleClockRate_Hz / 1000)
        Dim StopIndex As Integer = Int(tstop_msec * SampleClockRate_Hz / 1000)
        For nn As Integer = StartIndex To StopIndex
            DataArray(nn) = offset_volts - base_volts * Math.Exp((tstop_msec - tstart_msec) * (nn - StartIndex) / (StopIndex - StartIndex) / timeconst_msec)
        Next nn
    End Sub
    Public Sub AddSine(ByVal offset_volts As Double, ByVal amp_volts As Double, ByVal freq_Hz As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double)
        'Adds a sine waveform to a selected channel starting with phase zero
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'offset_volts as Double                     offset for sine waveform, in volts
        'amp_volts as Double                        amplitude for sine waveform, in volts
        'freq_Hz as Double                          frequency in Hz
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        Dim StartIndex As Integer = Int(tstart_msec * SampleClockRate_Hz / 1000)
        Dim StopIndex As Integer = Int(tstop_msec * SampleClockRate_Hz / 1000)
        Dim period As Double = 1.0 / freq_Hz
        period = period * SampleClockRate_Hz
        For nn As Integer = StartIndex To StopIndex
            DataArray(nn) = offset_volts + amp_volts * Math.Sin(2 * Math.PI / period * (nn - StartIndex))
        Next nn
    End Sub
    Public Sub AddFromFile(ByVal filename As String, ByVal tstart_msec As Double, ByVal tstop_msec As Double)
        'subroutine to add sampled waveform from text file, input file contains a column of numbers at same sampling rate
        Dim FileString As String = My.Computer.FileSystem.ReadAllText(filename)
        Dim DataString() As String = Split(FileString, vbNewLine)
        Dim StartIndex As Integer = Int(tstart_msec * SampleClockRate_Hz / 1000)
        Dim StopIndex As Integer = Int(tstop_msec * SampleClockRate_Hz / 1000)
        For nn As Integer = StartIndex To StopIndex
            DataArray(nn) = CDbl(DataString(nn - StartIndex))
        Next nn
    End Sub

    Public Function GetTotalTime() As Double
        'return the maximum time span in ms out of signal
        Dim maxIndex As Integer = 0
        For i As Integer = 0 To NumberOfSamples - 1
            If DataArray(i) <> 0 Then
                maxIndex = i
            End If
        Next i
        Return maxIndex / SampleClockRate_Hz * 1000
    End Function

    Private Function GetStartIndex() As Integer
        'return location in index of the first nonzero signal
        Dim FirstIndex As Integer
        For i As Integer = 0 To DataArray.Length - 1
            If DataArray(i) <> 0 Then
                FirstIndex = i
                Exit For
            End If
        Next i
        Return FirstIndex
    End Function

    Public Function GetStartTime() As Double
        'this function returns the starting time (ms) of the signal must run this before TrimBegin
        'return the starting time in unit of ms
        Return (1000 * GetStartIndex() / SampleClockRate_Hz)
    End Function
    Public Sub TrimEnd(ByVal samples As Integer)
        'get rid of redundant zeros after signals has passed
        ReDim Preserve DataArray(samples)
        NumberOfSamples = DataArray.Length
    End Sub

    Public Sub TrimBegin(ByVal FirstIndex As Integer)
        'move the first nonzero signal to the given starting index
        Dim CurrentIndex As Integer = GetStartIndex()
        Dim TargetIndex As Integer = FirstIndex
        'only trim if beginning index is after TargetIndex
        If CurrentIndex > TargetIndex Then
            'move all points forward
            Do While CurrentIndex <= UBound(DataArray)
                DataArray(TargetIndex) = DataArray(CurrentIndex)
                TargetIndex = TargetIndex + 1
                CurrentIndex = CurrentIndex + 1
            Loop
            'set points after signal to zero
            Do While TargetIndex <= UBound(DataArray)
                DataArray(TargetIndex) = 0
                TargetIndex = TargetIndex + 1
            Loop
        End If
    End Sub

    Public Sub WaveFormSettings()
        'Populate the correct amplitude and offset setting for each channel
        Dim maxValue As Double = 0
        Dim minValue As Double = 0
        For i As Integer = 0 To NumberOfSamples - 1
            If DataArray(i) > maxValue Then
                maxValue = DataArray(i)
            ElseIf DataArray(i) < minValue Then
                minValue = DataArray(i)
            End If
        Next i
        Offset = (maxValue + minValue) / 2
        Amp = (maxValue - minValue) / 2
        ScaleNumber = Math.Max(Math.Abs(maxValue), Math.Abs(minValue))
    End Sub

    Public Sub WaveFormWrite()
        'writes waveform to device. Takes about 30s for maximum length waveform
        'open communication to device
        Try
            NewSession = CType(ResourceManager.GetLocalManager().Open(IPAddresses), MessageBasedSession)
            WaveFormSettings()
            If Amp <> 0 Then
                'write waveform if channel is used
                WriteToDevice(Math.Min(8192, DataArray.Length))
            End If
            DoneDelegate.Invoke()
            CloseSession()
        Catch ex As InvalidCastException
            MsgBox("Resource selected must be a message-based session")
            'Throw New System.Exception("Resource selected must be a message-based session")
        Catch exp As Exception
            MsgBox("error inside writing")
            'Throw New System.Exception("error inside writing")
        End Try
        'clean up after writing
        Thread.CurrentThread.Abort()
    End Sub

    Private Sub WriteToDevice(ByVal ChunkSize As Integer)
        'buffer containing string to write
        Dim buf As New System.Text.StringBuilder
        NewSession.Write("OUTPUT OFF")
        'set trigger source to external
        NewSession.Write("TRIGGER:SOURCE EXTERNAL")
        NewSession.Write("TRIGGER:SLOPE POSITIVE")
        'set burst mode to 1 cycle
        NewSession.Write("BURST:MODE TRIGGERED; NCYCLES 1")
        NewSession.Write("BURST:STATE ON")
        NewSession.Write("FUNC:USER EXP_RISE")
        'delete any previous data
        NewSession.Write("DATA:DEL:All")
        'reset some attributes of session
        NewSession.SendEndEnabled = False
        NewSession.Timeout = 60000 'set time out to 60 s
        NewSession.Write("DATA VOLATILE, ")
        'now write data points to device in chunks
        Dim NumberOfWrites As Integer = Int(NumberOfSamples / ChunkSize)
        Dim i As Integer = 0
        If NumberOfWrites > 1 Then
            For j As Integer = 1 To NumberOfWrites - 1
                Do While i < j * ChunkSize - 1
                    buf.Append(((DataArray(i) - Offset) / Amp).ToString & ", ")
                    i = i + 1
                Loop
                'one chunck has been completed
                NewSession.Write(buf.ToString)
                buf = New System.Text.StringBuilder
            Next j
        End If

        'write the last chunk
        NewSession.SendEndEnabled = True
        Do While i < NumberOfSamples - 1
            buf.Append(((DataArray(i) - Offset) / Amp).ToString & ", ")
            i = i + 1
        Loop
        NewSession.Write(buf.ToString)
        NewSession.Write("OUTPUT OFF")
        NewSession.Write("FUNCTION:USER VOLATILE")
        'Write settings to device one at a time to avoid turning output on
        Dim frequency As Double = SampleClockRate_Hz / DataArray.Length
        NewSession.Write("FREQUENCY " + frequency.ToString() + " HZ")
        NewSession.Write("VOLTAGE " + (2 * Amp).ToString() + " VPP ")
        NewSession.Write("VOLTAGE:OFFSET " + Offset.ToString() + " V")
        NewSession.Write("OUTPUT ON")
        'reset attributes back to original value
        NewSession.Timeout = 2000
        'save written waveform
        'NewSession.Write("DATA:COPY NewWaveForm;*OPC?")
    End Sub
    Public Sub ClearDevice()
        Try
            NewSession = CType(ResourceManager.GetLocalManager().Open(IPAddresses), MessageBasedSession)
            NewSession.Write("OUTPUT OFF")
            CloseSession()
        Catch ex As InvalidCastException
            MsgBox("Resource selected must be a message-based session")
            'Throw New System.Exception("Resource selected must be a message-based session")
        Catch exp As Exception
            MsgBox("error inside writing")
            'Throw New System.Exception("error inside writing")
        End Try
    End Sub

    Private Sub CloseSession()
        'close visa session
        NewSession.Dispose()
    End Sub

End Class
