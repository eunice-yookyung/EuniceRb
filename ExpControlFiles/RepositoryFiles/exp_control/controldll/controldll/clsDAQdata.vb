Imports System
Imports System.Math
Public Class analogDAQdata
    Dim MaxOutputDuration_msec As Double = 70000 'maximum output duration in ms (can be changed if required)
    '  Data object for analog data acquisition boards. Implements basic step, ramp, sine, and arbitrary waveform generation. 
    Public Sub New(ByVal SmpClkRate_Hz As Double, ByVal AnalogCardIndex As Integer)
        'Class constructor 
        'Double SmpClkRate_Hz                       Sample clock rate in Hz 
        'Double OutputDuration_msec                 Output duration in milliseconds 
        _SmpClkRate_Hz = SmpClkRate_Hz
        _AnalogCardIndex = AnalogCardIndex
        _NumSamples = (MaxOutputDuration_msec * _SmpClkRate_Hz / 1000)
        ReDim _data(7, _NumSamples - 1)
        ReDim _is_set(7, _NumSamples - 1)
        Dim ii, jj As Integer
        For ii = 0 To 7
            For jj = 0 To _NumSamples - 1
                _is_set(ii, jj) = False
                _data(ii, jj) = 0
            Next jj
        Next ii
    End Sub

    Private Function MapData(ByVal data As Double, ByVal channel As Integer) As Double
        ' Maps setpoint voltage for log.PD to value for e.g. Log+lin.PD.

        ' Added 11.05.2011, modified routines:
        ' AddStep, AddExp, AddExpAndRamp, AndRamp, AndLogRamp, AndSmoothRamp, AddSine (both 2 routines)

        If channel = 2 And _AnalogCardIndex = 4 Then
            'axial_lattice_power (channel=2,card=4)
            'data = 1.15089 * Pow(10, -7) * Pow(10, 2 * data)
            'data = 2.69838 + 0.601551 * data + 0.217147 * Log(26.1332 * data)
            Return data
        Else
            Return data
        End If
    End Function


    Public Sub AddStep(ByVal step_volts As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer)
        'Adds a step waveform to a selected channel. 
        'Throws an exception of any other waveform data is present for the selected channel during the step duration and Override has not been called. 
        '
        'step_volts as Double                       Step amplitude in volts
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the step waveform is added
        Dim tstart, tstop As Integer
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int((tstop_msec * _SmpClkRate_Hz / 1000) - 0.0001)
        For nn = tstart To tstop
            'If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddStep")
            _data(channel, nn) = step_volts
            _data(channel, nn) = MapData(_data(channel, nn), channel) 'added 11.05.2011, inverse NOT taken care of.
            _is_set(channel, nn) = True
        Next nn
    End Sub
    Public Sub AddExp(ByVal base_volts As Double, ByVal offset_volts As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal timeconst_msec As Double, ByVal channel As Integer)
        'Adds a ramp waveform to a selected channel 
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'base_volts as Double                       base value of the exponential at t=0
        'offset_volts as Double                     offset value of the exponential
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'timeconst_msec as Double                   Time constant of the exponential in mseconds.
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        Dim tstart, tstop As Integer
        Dim nn As Integer
        Dim y As Double
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        For nn = tstart To tstop
            y = 0.0
            If _is_set(channel, nn) Then y = _data(channel, nn) 'inverse of MapData NOT taken care of!
            _data(channel, nn) = y + offset_volts - base_volts * (2.718281828) ^ (((tstop_msec - tstart_msec) * (nn - tstart) / (tstop - tstart)) / timeconst_msec)
            _data(channel, nn) = MapData(_data(channel, nn), channel) 'added 11.05.2011, inverse NOT taken care of.
            _is_set(channel, nn) = True
        Next nn
    End Sub
    Public Sub AddExpAndRamp(ByVal base_volts As Double, ByVal offset_volts As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal tramp_msec As Double, ByVal timeconst_msec As Double, ByVal start_volts As Double, ByVal stop_volts As Double, ByVal channel As Integer)
        'Adds a ramp waveform to a selected channel 
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'base_volts as Double                       base value of the exponential at t=0
        'offset_volts as Double                     offset value of the exponential
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'tramp_msec as Double                       Ramp time, in milliseconds
        'timeconst_msec as Double                   Time constant of the exponential in mseconds.
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        Dim tstart, tstop, tramp As Integer
        Dim y As Double
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        tramp = Int(tramp_msec * _SmpClkRate_Hz / 1000)
        For nn = tstart To tstop
            y = 0.0
            If _is_set(channel, nn) Then y = _data(channel, nn)
            If nn < (tramp + tstart) Then
                _data(channel, nn) = y + start_volts + (stop_volts - start_volts) * (nn - tstart) / tramp + offset_volts - base_volts * (2.718281828) ^ (((tstop_msec - tstart_msec) * (nn - tstart) / (tstop - tstart)) / timeconst_msec)
            Else
                _data(channel, nn) = y + stop_volts + offset_volts - base_volts * (2.718281828) ^ (((tstop_msec - tstart_msec) * (nn - tstart) / (tstop - tstart)) / timeconst_msec)
            End If
            _data(channel, nn) = MapData(_data(channel, nn), channel) 'added 11.05.2011, inverse NOT taken care of.

            _is_set(channel, nn) = True
        Next nn
    End Sub
    Public Sub AddRamp(ByVal start_volts As Double, ByVal stop_volts As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer)
        'Adds a ramp waveform to a selected channel 
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'start_volts as Double                      initial amplitude for the ramp, in volts
        'stop_volts as Double                       final amplitude for the ramp, in volts
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        Dim tstart, tstop As Integer
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        For nn = tstart To tstop
            'If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddRamp")
            _data(channel, nn) = start_volts + (stop_volts - start_volts) * (nn - tstart) / (tstop - tstart)
            _data(channel, nn) = MapData(_data(channel, nn), channel) 'added 11.05.2011, inverse NOT taken care of.
            _is_set(channel, nn) = True
        Next nn
    End Sub
    Private Function tunnel_from_depth(ByVal d As Double)
        'Numerical expression for tunnelling rate in recoils as a function of lattice depth from band structure calculation, good between 3 and 40 recoils. 
        'Calulation saved in Z:\Experiment Software Backup\ExpControl\mathematica
        Dim t As Double = 0.000806452 * (80.85 * Math.Exp(-d / 5.822) + 0.63815 * Math.Exp(-d / 15.349) + 248.836 * Math.Exp(-d / 3.0066))
        Return t
    End Function
    Private Function volt_from_tunnel(ByVal t As Double)
        'Numerical expression for voltage needed for desired tunnelling rate. Good between 10 and 30 recoils.
        'Calulation saved in Z:\Experiment Software Backup\ExpControl\mathematica
        Dim v As Double = 0.49064 + 0.3304 * Math.Exp(-t / 1.7646) + 0.77322 * Math.Exp(-t / 60.384) + 0.27196 * Math.Exp(-t / 0.03982)
        Return v
    End Function
    Private Function volt_from_tunnel_shallow(ByVal t As Double)
        'Fit for shallow lattices!
        'Numerical expression for voltage needed for desired tunnelling rate. Good from 3 to 10 recoils.
        'Calulation saved in Z:\Experiment Software Backup\ExpControl\mathematica
        Dim v As Double = 2 * Math.Pow(10, -0.19378 - 0.00584 * t + 7.1696 * Math.Pow(10, -5) * Math.Pow(t, 2) - 7.2492 * Math.Pow(10, -7) * Math.Pow(t, 3) + 3.5622 * Math.Pow(10, -9) * Math.Pow(t, 4) - 7.3566 * Math.Pow(10, -12) * Math.Pow(t, 5))
        Return v
    End Function
    Private Function depth_from_tunnel(ByVal t As Double)
        'Fit for lattices < 30 Er
        'Numerical expression for lattice depth needed for desired tunnelling rate. Good from 2 to 30 recoils. Tunneling and lattice depth in Er.
        Dim d As Double = -0.70255 + 14.1054 * Math.Exp(-t / 0.0000553825) + 10.497 * Math.Exp(-t / 0.108124) + 10.6318 * Math.Exp(-t / 0.00203487) + 9.01271 * Math.Exp(-t / 0.0124361) + 12.449 * Math.Exp(-t / 0.000336428)
        Return d
    End Function
    Public Function axialdepth_from_2d_depth(ByVal V As Double)
        'Numerical expression for axial depth needed (in axial recoils) for a given 2D depth (in 2d recoils) to keep tunneling between planes and within planes the same
        'Calulation saved in Z:\Experiment Software Backup\ExpControl\mathematica
        'Dim D As Double = 5.79 + 1.43 * V

        'Dim D As Double = 10.988 + 1.1507 * V  'may 03 2012
        Dim D As Double = 8.207 + 1.201 * V 'may 04 2012

        Return D
    End Function
    Public Function mod_start_voltage(ByVal offset_depth As Double, ByVal rel_amp As Double, ByVal phase As Double, ByVal calib_volt As Double, ByVal slope As Double)
        Dim w As Double = tunnel_from_depth(offset_depth)
        Dim u As Double = w * (1 + rel_amp * Math.Sin(phase * Math.PI))
        Dim x As Double = depth_from_tunnel(u)
        Dim v As Double = calib_volt + slope * (Math.Log10(x) - Math.Log10(offset_depth))
        Return v
    End Function
    Public Function mod_end_voltage(ByVal offset_depth As Double, ByVal rel_amp As Double, ByVal phase As Double, ByVal freq_Hz As Double, ByVal duration As Double, ByVal calib_volt As Double, ByVal slope As Double)
        Dim w As Double = tunnel_from_depth(offset_depth)
        Dim u As Double = w * (1 + rel_amp * Math.Sin(phase * Math.PI + 2 * Math.PI * freq_Hz * duration / 1000))
        Dim x As Double = depth_from_tunnel(u)
        Dim v As Double = calib_volt + slope * (Math.Log10(x) - Math.Log10(offset_depth))
        Return v
    End Function
    Public Sub AddTunnelSine(ByVal offset_depth As Double, ByVal rel_amp As Double, ByVal freq_Hz As Double, ByVal phase As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal calib_volt As Double, ByVal slope As Double, ByVal channel As Integer)
        'Sinusoidal modulation of the tunnelling about given lattice depth with relative amplitude rel_amp and initial phase zero on a selected channel.


        'offset_depth as Double                     offset for tunnelling, in recoils
        'rel_amp as Double                          relative amplitude of sinusoidal modulation of tunnelling
        'freq_Hz as Double                          frequency in Hz
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'calib_volt as Double                       calibrated voltage for offset_depth (e.g. lattice2_low_volt)
        'slope as Double                            slope (in V per decade) of photdiodes
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        'phase                                      additional phase in units of Pi
        Dim tstart, tstop As Integer
        Dim period As Double
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        period = 1.0 / freq_Hz
        period = period * _SmpClkRate_Hz
        If tstop > tstart Then
            For nn = tstart To tstop
                'If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddRamp")
                Dim w As Double = tunnel_from_depth(offset_depth)
                Dim u As Double = w * (1 + rel_amp * Math.Sin(phase * Math.PI + 2 * Math.PI / period * (nn - tstart)))
                Dim x As Double = depth_from_tunnel(u)
                Dim y As Double = calib_volt + slope * (Math.Log10(x) - Math.Log10(offset_depth))
                _data(channel, nn) = y
                _is_set(channel, nn) = True
            Next nn
        End If
    End Sub
    Public Sub AddTunnelSineRamp(ByVal offset_depth As Double, ByVal rel_amp As Double, ByVal freq_Hz_start As Double, ByVal freq_Hz_stop As Double, ByVal phase_start As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal calib_volt As Double, ByVal slope As Double, ByVal channel As Integer)
        'Sinusoidal modulation of the tunnelling about given lattice depth with linearly increasing frequency and relative amplitude rel_amp. Specify initial phase in units of Pi. 


        'offset_depth as Double                     offset for tunnelling, in recoils
        'rel_amp as Double                          relative amplitude of sinusoidal modulation of tunnelling
        'freq_Hz_start as Double                    initial frequency in Hz
        'freq_Hz_stop as Double                     final frequency in Hz
        'phase_start as Double                      Initial phase in units of Pi
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'calib_volt as Double                       calibrated voltage for offset_depth (e.g. lattice2_low_volt)
        'slope as Double                            slope (in V per decade) of photdiodes
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        Dim tstart, tstop As Integer
        Dim periodI As Double
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        periodI = 1.0 / freq_Hz_start
        periodI = periodI * _SmpClkRate_Hz
        If tstop > tstart Then
            For nn = tstart To tstop
                'If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddRamp")
                Dim FiT As Double = (1.0 / periodI) * (nn - tstart)
                Dim PhiT = Math.PI * phase_start + 2.0 * Math.PI * FiT * (1.0 + FiT * (freq_Hz_stop / freq_Hz_start - 1) / (2.0 * freq_Hz_start * (tstop_msec - tstart_msec) / 1000.0))
                Dim w As Double = tunnel_from_depth(offset_depth)
                Dim u As Double = w * (1 + rel_amp * Math.Sin(PhiT))
                Dim x As Double = depth_from_tunnel(u)
                Dim y As Double = calib_volt + slope * (Math.Log10(x) - Math.Log10(offset_depth))
                _data(channel, nn) = y
                _is_set(channel, nn) = True
            Next nn
        End If
    End Sub
    Public Sub AddTunnelSineRampTriple(ByVal offset_depth As Double, ByVal rel_amp_1 As Double, ByVal freq_Hz_start_1 As Double, ByVal freq_Hz_stop_1 As Double, ByVal phase_start_1 As Double, ByVal rel_amp_2 As Double, ByVal freq_Hz_start_2 As Double, ByVal freq_Hz_stop_2 As Double, ByVal phase_start_2 As Double, ByVal rel_amp_3 As Double, ByVal freq_Hz_start_3 As Double, ByVal freq_Hz_stop_3 As Double, ByVal phase_start_3 As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal calib_volt As Double, ByVal slope As Double, ByVal channel As Integer)
        'Superimpose three frequency sweeps of modulation in tunneling. Specify amplitude, start and stop frequency and phase in units of Pi for each frequency. 
        'Be sure to use the correct fit function to calculate instantaneous voltage from instantaneous tunneling! 'volt_from tunnel_shallow' for 3 to 10 recoils, 'volt_from_tunnel' for 10 to 30 recoils!


        'offset_depth as Double                     offset for tunnelling, in recoils
        'rel_amp_ias Double                         relative amplitude of the ith sinusoidal modulation of tunnelling
        'freq_Hz_start_i as Double                  initial frequency i in Hz
        'freq_Hz_stop_i as Double                   final frequency i in Hz
        'phase_start_i as Double                    Initial phase i in units of Pi
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'calib_volt as Double                       calibrated voltage for offset_depth (e.g. lattice2_low_volt)
        'slope as Double                            slope (in V per decade) of photdiodes
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        Dim tstart, tstop As Integer
        Dim period_1 As Double
        Dim period_2 As Double
        Dim period_3 As Double
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        period_1 = 1.0 / freq_Hz_start_1
        period_2 = 1.0 / freq_Hz_start_2
        period_3 = 1.0 / freq_Hz_start_3
        period_1 = period_1 * _SmpClkRate_Hz
        period_2 = period_2 * _SmpClkRate_Hz
        period_3 = period_3 * _SmpClkRate_Hz
        If tstop > tstart Then
            For nn = tstart To tstop
                'If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddRamp")
                Dim FiT_1 As Double = (1.0 / period_1) * (nn - tstart)
                Dim FiT_2 As Double = (1.0 / period_2) * (nn - tstart)
                Dim FiT_3 As Double = (1.0 / period_3) * (nn - tstart)
                Dim PhiT_1 = Math.PI * phase_start_1 + 2.0 * Math.PI * FiT_1 * (1.0 + FiT_1 * (freq_Hz_stop_1 / freq_Hz_start_1 - 1) / (2.0 * freq_Hz_start_1 * (tstop_msec - tstart_msec) / 1000.0))
                Dim PhiT_2 = Math.PI * phase_start_2 + 2.0 * Math.PI * FiT_2 * (1.0 + FiT_2 * (freq_Hz_stop_2 / freq_Hz_start_2 - 1) / (2.0 * freq_Hz_start_2 * (tstop_msec - tstart_msec) / 1000.0))
                Dim PhiT_3 = Math.PI * phase_start_3 + 2.0 * Math.PI * FiT_3 * (1.0 + FiT_3 * (freq_Hz_stop_3 / freq_Hz_start_3 - 1) / (2.0 * freq_Hz_start_3 * (tstop_msec - tstart_msec) / 1000.0))
                Dim w As Double = tunnel_from_depth(offset_depth)
                Dim u As Double = w * (1 + rel_amp_1 * Math.Sin(PhiT_1) + rel_amp_2 * Math.Sin(PhiT_2) + rel_amp_3 * Math.Sin(PhiT_3))
                Dim x As Double = depth_from_tunnel(u)
                Dim y As Double = calib_volt + slope * (Math.Log10(x) - Math.Log10(offset_depth))
                _data(channel, nn) = y
                _is_set(channel, nn) = True
            Next nn
        End If
    End Sub
    Public Sub AddTunnelSineFlip(ByVal offset_depth As Double, ByVal rel_amp As Double, ByVal freq_Hz As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal fract As Double, ByVal calib_volt As Double, ByVal slope As Double, ByVal channel As Integer)
        'Sinusoidal modulation of the tunnelling about given lattice depth with relative amplitude rel_amp and initial phase zero. Flip phase by 180 deg after specified fraction of modulation time.
        '
        'offset_depth as Double                     offset for tunnelling, in recoils
        'rel_amp as Double                          relative amplitude of sinusoidal modulation of tunnelling
        'freq_Hz as Double                          frequency in Hz
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'fract as Double                            Fraction of modulation time after which phase is flipped by 180 deg.
        'calib_volt as Double                       calibrated voltage for offset_depth (e.g. lattice2_low_volt)
        'slope as Double                            slope (in V per decade) of photdiodes
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        'phase                                      additional phase in units of Pi
        Dim tstart, tstop, tflip As Integer
        Dim period As Double
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        tflip = tstart + fract * (tstop - tstart)
        period = 1.0 / freq_Hz
        period = period * _SmpClkRate_Hz
        If tstop > tstart Then
            For nn = tstart To tflip
                'If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddRamp")
                Dim w As Double = tunnel_from_depth(offset_depth)
                Dim u As Double = w * (1 + rel_amp * Math.Sin(2 * Math.PI / period * (nn - tstart)))
                Dim x As Double = depth_from_tunnel(u)
                Dim y As Double = calib_volt + slope * (Math.Log10(x) - Math.Log10(offset_depth))
                _data(channel, nn) = y
                _is_set(channel, nn) = True
            Next nn
            For nn = tflip To tstop
                'If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddRamp")
                Dim w As Double = tunnel_from_depth(offset_depth)
                Dim u As Double = w * (1 + rel_amp * Math.Sin(2 * Math.PI / period * (nn - tstart) + Math.PI))
                Dim x As Double = depth_from_tunnel(u)
                Dim y As Double = calib_volt + slope * (Math.Log10(x) - Math.Log10(offset_depth))
                _data(channel, nn) = y
                _is_set(channel, nn) = True
            Next nn
        End If
    End Sub
    Public Sub AddAxialExpRamp(ByVal start_2Ddepth As Double, ByVal stop_2Ddepth As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal axial_calib_depth As Double, ByVal axial_calib_volt As Double, ByVal channel As Integer)
        'Ramp axial lattice such that axial and 2d tunneling are the same. Assumes linear ramp in 2D lattice voltage!!! 
        'Calulation saved in Z:\Experiment Software Backup\ExpControl\mathematica
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'start_2Ddepth as Double                    initial amplitude of 2d lattice, in 2d recoils
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'axial_calib_depth as Double                calibration depth of axial lattice, in axial recoils
        'axial_calib_volt                           calibration voltage axial lattice 
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        'time_const                                 (1/time constant) for 2d lattice depth exponential ramp (linear in voltage) 
        Dim tstart, tstop As Integer
        Dim time_const As Double
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        time_const = Math.Log10(stop_2Ddepth / start_2Ddepth) / (tstop - tstart)
        For nn = tstart To tstop
            'If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddRamp")
            _data(channel, nn) = axial_calib_volt + 0.5 * Math.Log10(axialdepth_from_2d_depth(start_2Ddepth * 10 ^ (time_const * (nn - tstart))) / axial_calib_depth)
            _data(channel, nn) = MapData(_data(channel, nn), channel) 'added 11.05.2011, inverse NOT taken care of.
            _is_set(channel, nn) = True
        Next nn
    End Sub
    Public Sub AddSine(ByVal offset_volts As Double, ByVal amp_volts As Double, ByVal freq_Hz As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer)
        'Adds a sine waveform to a selected channel starting with phase zero
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'offset_volts as Double                     offset for sine waveform, in volts
        'amp_volts as Double                        amplitude for sine waveform, in volts
        'freq_Hz as Double                          frequency in Hz
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        Dim tstart, tstop As Integer
        Dim period As Double
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        period = 1.0 / freq_Hz
        period = period * _SmpClkRate_Hz
        If tstop > tstart Then
            For nn = tstart To tstop
                'If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddRamp")
                Dim y As Double = offset_volts + amp_volts * Math.Sin(2 * Math.PI / period * (nn - tstart))
                _data(channel, nn) = y
                _data(channel, nn) = MapData(_data(channel, nn), channel) 'added 11.05.2011, inverse NOT taken care of.
                _is_set(channel, nn) = True
            Next nn
        End If
    End Sub
    Public Sub AddSinePhase(ByVal offset_volts As Double, ByVal amp_volts As Double, ByVal freq_Hz As Double, ByVal phase_pi As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer)
        'Adds a sine waveform to a selected channel starting with initial phase pi*phase_pi
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'offset_volts as Double                     offset for sine waveform, in volts
        'amp_volts as Double                        amplitude for sine waveform, in volts
        'freq_Hz as Double                          frequency in Hz
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        Dim tstart, tstop As Integer
        Dim period As Double
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        period = 1.0 / freq_Hz
        period = period * _SmpClkRate_Hz
        If tstop > tstart Then
            For nn = tstart To tstop
                'If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddRamp")
                Dim y As Double = offset_volts + amp_volts * Math.Sin(2 * Math.PI / period * (nn - tstart) + Math.PI * phase_pi)
                _data(channel, nn) = y
                _is_set(channel, nn) = True
            Next nn
        End If
    End Sub
    Public Sub AddBlackmanPulse(ByVal offset_volts As Double, ByVal amp_volts As Double, ByVal freq_Hz As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer)
        'Adds a sine waveform with a Blackman envelope
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'offset_volts as Double                     offset for sine waveform, in volts
        'amp_volts as Double                        amplitude for sine waveform, in volts
        'freq_Hz as Double                          frequency in Hz
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        Dim tstart, tstop As Integer
        Dim period As Double
        Dim duration As Double
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        duration = tstart - tstop
        period = 1.0 / freq_Hz
        period = period * _SmpClkRate_Hz
        If tstop > tstart Then
            For nn = tstart To tstop
                'If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddRamp")
                Dim y As Double = offset_volts + amp_volts * Math.Sin(2 * Math.PI / period * (nn - tstart)) * (-0.5 * Math.Cos(2 * Math.PI / duration * (nn - tstart)) + 0.08 * Math.Cos(4 * Math.PI / duration * (nn - tstart)) + 0.42)
                _data(channel, nn) = y
                _is_set(channel, nn) = True
            Next nn
        End If
    End Sub
    Public Sub AddSineFlip(ByVal offset_volts As Double, ByVal amp_volts As Double, ByVal freq_Hz As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal fract As Double, ByVal channel As Integer)
        'Adds a sine waveform to a selected channel starting with phase zero and phase flip of 180 deg after fraction of modulation time.
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'offset_volts as Double                     offset for sine waveform, in volts
        'amp_volts as Double                        amplitude for sine waveform, in volts
        'freq_Hz as Double                          frequency in Hz
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'fract as Double                            Fraction of the total duration after which Pi phase flip occurs
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        Dim tstart, tstop, tflip As Integer
        Dim period As Double
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        tflip = tstart + fract * (tstop - tstart)
        period = 1.0 / freq_Hz
        period = period * _SmpClkRate_Hz
        If tstop > tstart Then
            For nn = tstart To tflip
                'If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddRamp")
                Dim y As Double = offset_volts + amp_volts * Math.Sin(2.0 * Math.PI / period * (nn - tstart))
                _data(channel, nn) = y
                _is_set(channel, nn) = True
            Next nn
            For nn = tflip To tstop
                'If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddRamp")
                Dim z As Double = offset_volts + amp_volts * Math.Sin(2.0 * Math.PI / period * (nn - tstart) + Math.PI)
                _data(channel, nn) = z
                _is_set(channel, nn) = True
            Next nn
        End If
    End Sub
    Public Sub AddSineRamp(ByVal offset_volts As Double, ByVal amp_volts As Double, ByVal freq_Hz_start As Double, ByVal freq_Hz_stop As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer)
        'Adds a sine ramp waveform to a selected channel starting with phase zero, with the frequency linearly increasing in time
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'offset_volts as Double                     offset for sine waveform, in volts
        'amp_volts as Double                        amplitude for sine waveform, in volts
        'freq_Hz as Double                          frequency in Hz
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        Dim tstart, tstop As Integer
        Dim periodI As Double
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        periodI = 1.0 / freq_Hz_start
        periodI = periodI * _SmpClkRate_Hz
        If tstop > tstart Then
            For nn = tstart To tstop
                'If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddRamp")
                Dim FiT As Double = (1.0 / periodI) * (nn - tstart)
                Dim PhiT = 2.0 * Math.PI * FiT * (1.0 + FiT * (freq_Hz_stop / freq_Hz_start - 1) / (2.0 * freq_Hz_start * (tstop_msec - tstart_msec) / 1000.0))
                Dim y As Double = offset_volts + amp_volts * Math.Sin(PhiT)
                _data(channel, nn) = y
                _data(channel, nn) = MapData(_data(channel, nn), channel)
                _is_set(channel, nn) = True
            Next nn
        End If
    End Sub
    Public Sub AddLogRamp(ByVal start_volts As Double, ByVal stop_volts As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer)
        'Adds a ramp waveform to a selected channel 
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'start_volts as Double                      initial amplitude for the ramp, in volts
        'stop_volts as Double                       final amplitude for the ramp, in volts
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        Dim tstart, tstop As Integer
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        For nn = tstart To tstop
            'If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddRamp")
            _data(channel, nn) = start_volts + 0.5 * Math.Log10(1 + (nn - tstart) / (tstop - tstart) * (Math.Pow(10, 2 * stop_volts - 2 * start_volts) - 1))
            _data(channel, nn) = MapData(_data(channel, nn), channel) 'added 11.05.2011, inverse NOT taken care of.
            _is_set(channel, nn) = True
        Next nn
    End Sub
    Public Function AddLogRampRedDipole(ByVal red_start_volts As Double, ByVal red_start_freq As Double, ByVal lattice_start_depth As Double, ByVal lattice_stop_depth As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer) As Double
        'Adds a ramp waveform to a selected channel 
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'red_start_volts as Double                  initial amplitude for the ramp, in volts
        'red_start_freq as Double                   initial red dipole frequency in Hz
        'lattice_start_depth as Double              initial lattice depth in recoils
        'lattice_stop_depth as Double               final lattice depth in recoils
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        '
        'Returns the end voltage
        Dim tstart, tstop As Integer
        Dim nn As Integer
        Dim currentDepth As Double = 0
        Dim currentVolt As Double = 0
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        For nn = tstart To tstop
            currentDepth = lattice_start_depth + (lattice_stop_depth - lattice_start_depth) * (nn - tstart) / (tstop - tstart)
            currentVolt = red_start_volts + 0.5 * Math.Log10(InteractionRatio(currentDepth) + Math.Pow(BlueDeconfinement(currentDepth) / red_start_freq, 2))
            _data(channel, nn) = currentVolt
            _is_set(channel, nn) = True
        Next nn
        Return currentVolt
    End Function
    Public Sub AddSmoothRamp(ByVal start_volts As Double, ByVal stop_volts As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer)
        'Adds a smooth ramp waveform to a selected channel 
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'start_volts as Double                      initial amplitude for the ramp, in volts
        'stop_volts as Double                       final amplitude for the ramp, in volts
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        Dim tstart, tstop As Integer
        Dim x, f As Double
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        For nn = tstart To tstop
            x = (nn - tstart) / (tstop - tstart)
            'f = -2 * Math.Pow(x, 3) + 3 * Math.Pow(x, 2) OLD Spline, only first derivative zero at end points
            f = 10 * Math.Pow(x, 3) - 15 * Math.Pow(x, 4) + 6 * Math.Pow(x, 5)
            _data(channel, nn) = start_volts + (stop_volts - start_volts) * f
            _data(channel, nn) = MapData(_data(channel, nn), channel) 'added 11.05.2011, inverse NOT taken care of.
            _is_set(channel, nn) = True
        Next nn
    End Sub
    Public Sub AddS(ByVal background_volts As Double, ByVal final_volts As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer)
        'Adds an s-shaped smooth waveform to a selected channel 
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'start_volts as Double                      initial amplitude for the ramp, in volts, logarithmic photodiode assumed
        'stop_volts as Double                       final asymptotic amplitude for the ramp, in volts, logarithmic photodiode assumed
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        Dim tstart, tstop, tduration As Integer
        Dim f, fmax, g As Double
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        tduration = tstop - tstart
        fmax = 2.0 * (0.5 - 1.0 / (1 + Math.Exp(-Math.Pow(2.5 * (1.02 * tduration) / (1.0 * tduration), 2))))
        For nn = tstart To tstop
            f = 2.0 * (0.5 - 1.0 / (1 + Math.Exp(-Math.Pow(2.5 * (nn - tstart + 0.02 * tduration) / (1.0 * tduration), 2))))
            g = final_volts + 0.5 * Math.Log10(f / fmax)
            g = Math.Max(g, background_volts)
            _data(channel, nn) = g
            _is_set(channel, nn) = True
        Next nn
    End Sub
    Public Function InteractionRatio(ByVal V As Double) As Double
        'See pg 26-27 of lab book 7 for documentation on this function
        'V is the lattice depth in recoils
        Dim f As Double = 1 + 0.24256 * Math.Pow(V, 1) + 0.0128075 * Math.Pow(V, 2) - 0.00108671 * Math.Pow(V, 3) + 0.0000304067 * Math.Pow(V, 4) - 0.000000297633 * Math.Pow(V, 5)
        Return f
    End Function
    Public Function BlueDeconfinement(ByVal V As Double) As Double
        'See pg 26-27 of lab book 7 for documentation on this function
        'V is the lattice depth in recoils
        Dim g As Double = 6.9397 * Math.Pow(V, 0.5) - 1.41963 * Math.Pow(V, 1) + 0.034345 * Math.Pow(V, 2) - 0.000601338 * Math.Pow(V, 3) + 0.00000424209 * Math.Pow(V, 4)
        Return g
    End Function
    Public Function AddRampRedDipole(ByVal red_start_volts As Double, ByVal red_start_freq As Double, ByVal lattice_start_depth As Double, ByVal lattice_stop_depth As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer) As Double
        'Adds a ramp waveform to a selected channel 
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'red_start_volts as Double                  initial amplitude for the ramp, in volts
        'red_start_freq as Double                   initial red dipole frequency in Hz
        'lattice_start_depth as Double              initial lattice depth in recoils
        'lattice_stop_depth as Double               final lattice depth in recoils
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        '
        'Returns the end voltage
        Dim tstart, tstop As Integer
        Dim nn As Integer
        Dim currentDepth As Double = 0
        Dim currentVolt As Double = 0
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        For nn = tstart To tstop
            currentDepth = lattice_start_depth * Math.Pow(lattice_stop_depth / lattice_start_depth, (nn - tstart) / (tstop - tstart))
            currentVolt = red_start_volts + 0.5 * Math.Log10(InteractionRatio(currentDepth) + Math.Pow(BlueDeconfinement(currentDepth) / red_start_freq, 2))
            _data(channel, nn) = currentVolt
            _is_set(channel, nn) = True
        Next nn
        Return currentVolt
    End Function
    Public Function AddRampRedDipoleBkwd(ByVal red_start_volts As Double, ByVal red_start_freq As Double, ByVal lattice_start_depth As Double, ByVal lattice_stop_depth As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer) As Double
        'Adds a ramp waveform to a selected channel 
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'red_start_volts as Double                  initial amplitude for the ramp, in volts
        'red_start_freq as Double                   initial red dipole frequency in Hz
        'lattice_start_depth as Double              initial lattice depth in recoils
        'lattice_stop_depth as Double               final lattice depth in recoils
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        '
        'Returns the end voltage
        Dim tstart, tstop As Integer
        Dim nn As Integer
        Dim currentDepth As Double = 0
        Dim currentVolt As Double = 0
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        For nn = tstart To tstop
            currentDepth = lattice_start_depth * Math.Pow(lattice_stop_depth / lattice_start_depth, (tstop - nn) / (tstop - tstart))
            currentVolt = red_start_volts + 0.5 * Math.Log10(InteractionRatio(currentDepth) + Math.Pow(BlueDeconfinement(currentDepth) / red_start_freq, 2))
            _data(channel, nn) = currentVolt
            _is_set(channel, nn) = True
        Next nn
        Return currentVolt
    End Function
    Public Function AddSRedDipole(ByVal red_start_volts As Double, ByVal red_start_freq As Double, ByVal lattice_startDepth As Double, ByVal lattice_stop_depth As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer) As Double
        'Adds a ramp waveform to a selected channel 
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'red_start_volts as Double                  initial amplitude for the ramp, in volts
        'red_start_freq as Double                   initial red dipole frequency in Hz
        'lattice_start_depth as Double              initial lattice depth in recoils (not needed, just to keep interface the same)
        'lattice_stop_depth as Double               final lattice depth in recoils
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        '
        'Returns the end voltage
        Dim tstart, tstop, tduration As Integer
        Dim f, fmax As Double
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        tduration = tstop - tstart
        fmax = 2.0 * (0.5 - 1.0 / (1 + Math.Exp(-Math.Pow(2.5 * (1.02 * tduration) / (1.0 * tduration), 2))))
        Dim currentDepth As Double = 0
        Dim currentVolt As Double = 0
        For nn = tstart To tstop
            f = 2.0 * (0.5 - 1.0 / (1 + Math.Exp(-Math.Pow(2.5 * (nn - tstart + 0.02 * tduration) / (1.0 * tduration), 2))))
            currentDepth = f / fmax * lattice_stop_depth
            currentVolt = red_start_volts + 0.5 * Math.Log10(InteractionRatio(currentDepth) + Math.Pow(BlueDeconfinement(currentDepth) / red_start_freq, 2))
            _data(channel, nn) = currentVolt
            _is_set(channel, nn) = True
        Next nn
        Return currentVolt
    End Function
    Public Sub AddFancySmoothRamp(ByVal start_volts As Double, ByVal stop_volts As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal alpha As Double, ByVal beta As Double, ByVal channel As Integer)
        'Adds a smooth ramp waveform to a selected channel 
        'The ramp has zero second derivatives at the endpoints but the first derivative is alpha at the first endpoint and beta at the second
        'Throws an exception of any other waveform data is present for the selected channel during the ramp duration and Override has not been called. 
        '
        'start_volts as Double                      initial amplitude for the ramp, in volts
        'stop_volts as Double                       final amplitude for the ramp, in volts
        'tstart_msec as Double                      Leading edge time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Trailing edge time, in milliseconds from the analog output start trigger
        'alpha (volts/ms)
        'beta (volts/ms)
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        Dim tstart, tstop As Integer
        Dim x, f As Double
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        alpha = alpha * 1000.0 / _SmpClkRate_Hz / (stop_volts - start_volts)
        beta = beta * 1000.0 / _SmpClkRate_Hz / (stop_volts - start_volts)
        Dim a, b, c As Double
        a = -3 * (-2 + alpha + beta)
        b = -15 + 8 * alpha + 7 * beta
        c = -2 * (-5 + 3 * alpha + 2 * beta)
        For nn = tstart To tstop
            x = (nn - tstart) / (tstop - tstart)
            f = alpha * x + c * Math.Pow(x, 3) + b * Math.Pow(x, 4) + a * Math.Pow(x, 5)
            _data(channel, nn) = start_volts + (stop_volts - start_volts) * f
            _is_set(channel, nn) = True
        Next nn
    End Sub
    Public Sub AddSine(ByVal amp_volts As Double, ByVal offset_volts As Double, ByVal period_msec As Double, ByVal phase_radians As Double, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer)
        'Adds a sine waveform to a selected channel 
        'Throws an exception of any other waveform data is present for the selected channel during the sine duration and Override has not been called. 
        '
        'amp_volts                                  amplitude for the sine wave (.5 Vpp), in volts
        'offset_volts as Double                     offset for the sine wave, in volts 
        'period_msec as Double                      sine wave period, in milliseconds from the analog output start trigger
        'phase as Double                            Starting phase, in radians
        'tstart_msec as Double                      start time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       stop time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the sine waveform is added

        Const Pi As Double = 3.1415926535897931
        Dim tstart, tstop As Integer
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        For nn = tstart To tstop
            If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddSine")
            _data(channel, nn) = offset_volts + amp_volts * System.Math.Sin(2 * Pi * (nn - tstart) * 1000 / (_SmpClkRate_Hz * period_msec) + phase_radians)
            _data(channel, nn) = MapData(_data(channel, nn), channel) 'added 11.05.2011, inverse NOT taken care of.
            _is_set(channel, nn) = True
        Next nn
    End Sub
    Public Sub Add1DArb(ByRef arbpoints_volts() As Double, ByVal ArbSmpRate_Hz As Double, ByVal tstart_msec As Double, ByVal channel As Integer)
        'Adds an 1D arbitrary waveform to a selected channel 
        'Starting at tstart_msec, each poiint in arbpoints_volts() is output for 1/(ArbSmpRate_Hz) seconds. 
        'Throws an exception of any other waveform data is present for the selected channel during the arbitrary waveform duration and Override has not been called. 
        '
        'arbpoints_volts() as Double                1D array of waveform samples, in volts 
        'ArbSmpRate_Hz as Double                    Arbitrary waveform sample rate, in Hz. 
        'tstart_msec as Double                      Start time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Stop time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added
        Dim tstart, tstop As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = tstart + Int(arbpoints_volts.GetLength(0) * _SmpClkRate_Hz / ArbSmpRate_Hz)
        Dim nn As Integer
        For nn = tstart To tstop
            If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddStep")
            _data(channel, nn) = arbpoints_volts(Int(nn * ArbSmpRate_Hz / _SmpClkRate_Hz))
            _is_set(channel, nn) = True
        Next
    End Sub
    Public Sub Add2DArb(ByRef arbpointsY_volts() As Double, ByRef arbpointsX_msec() As Double, ByVal tstart_msec As Double, ByVal channel As Integer)
        'Adds a 2D (XY) arbitrary waveform to a selected channel 
        'Starting at tstart_msec, an atbitrary waveform is output as follows: arbpointsX_msec constitutes a series of 
        'timestamps, arbpointsY_volts a series of voltages. Each timestamp corresponds to a voltage, the timestamp-voltage
        'pairs are connected by ramp waveforms. 
        'Throws an exception of any other waveform data is present for the selected channel during the arbitrary waveform duration and Override has not been called. 
        '
        'arbpointsY_volts() as Double               List of voltages. Each voltage corresponds to the timestamp with the same index. 
        'arbpointsX_msec as Double                  Ordered list of timestamps. Must be the same length as arbpointsY_volts. 
        'ArbSmpRate_Hz as Double                    Arbitrary waveform sample rate, in Hz. 
        'tstart_msec as Double                      Start time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Stop time, in milliseconds from the analog output start trigger
        'channel as integer                         (0-7) Specifies the channel to which the ramp waveform is added

        Dim tstart, tstop, numpoints, wfmlength As Integer
        numpoints = arbpointsX_msec.GetLength(0) - 1
        wfmlength = Int(arbpointsX_msec(numpoints) * _SmpClkRate_Hz / 1000)
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = tstart + wfmlength
        Dim nn, currentpoint As Integer, currenttime As Double
        currentpoint = 1
        For nn = tstart To tstop
            If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddStep")
            _is_set(channel, nn) = True
            currenttime = nn * 1000 / _SmpClkRate_Hz
            While currenttime > arbpointsX_msec(currentpoint)
                currentpoint = currentpoint + 1
            End While
            _data(channel, nn) = arbpointsY_volts(currentpoint - 1) _
            + (arbpointsY_volts(currentpoint) - arbpointsY_volts(currentpoint - 1)) / _
            (currenttime - arbpointsX_msec(currentpoint - 1)) * (arbpointsX_msec(currentpoint) - arbpointsX_msec(currentpoint - 1))
        Next
    End Sub
    Public Sub AddFromFile(ByVal filename As String, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal maxCurrent As Double, ByVal noPowerSupplies As Integer, ByVal channel As Integer)
        'Adds a 2D (XY) arbitrary waveform to a selected channel 
        'Starting at tstart_msec, an atbitrary waveform is output as follows: arbpointsX_msec constitutes a series of 
        'timestamps, arbpointsY_volts a series of voltages. Each timestamp corresponds to a voltage, the timestamp-voltage
        'pairs are connected by ramp waveforms. 
        'Throws an exception of any other waveform data is present for the selected channel during the arbitrary waveform duration and Override has not been called. 
        '
        'filename as String                         filename from which to read the waveform
        'maxCurrent                                 maximum current each of the power supplies producing the waveform can generate (ie current at 5 volts control signal)
        'tstart_msec as Double                      Start time, in milliseconds from the analog output start trigger
        'tstop_msec as Double                       Stop time, in milliseconds from the analog output start trigger
        'noPowerSupplies as integer                 number of power supplies sourcing the current
        'channel as integer                         (0-7) Specifies the channel to which the waveform is added
        Dim tstart, tstop As Integer
        Dim x As Double
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        nn = tstart
        Using MyReader As New _
        Microsoft.VisualBasic.FileIO.TextFieldParser(filename)
            MyReader.TextFieldType = Microsoft.VisualBasic.FileIO.FieldType.Delimited
            MyReader.SetDelimiters("\n")
            Dim data As String()
            While Not MyReader.EndOfData
                Try
                    data = MyReader.ReadFields()
                    Dim currentField As String
                    For Each currentField In data
                        x = 0
                        'If _is_set(channel, nn) Then Throw New System.Exception("Overwriting data in AddFromFile")

                        x = Convert.ToDouble(currentField)
                        x = x / noPowerSupplies
                        x = x / maxCurrent * 5
                        _data(channel, nn) = x
                        _is_set(channel, nn) = True
                        nn = nn + 1
                    Next
                Catch ex As Microsoft.VisualBasic.FileIO.MalformedLineException
                    MsgBox("Line " & ex.Message & _
                    "is not valid and will be skipped.")
                End Try
            End While
        End Using
    End Sub
    'Returns total time for analog data output in ms

    Public Function GetTotalTime() As Double
        Dim ii, jj, lastSample, currentLastSample As Integer
        lastSample = 0
        For ii = 0 To 7
            For jj = 0 To _NumSamples - 1
                If (_is_set(ii, NumSamples - 1 - jj)) Then
                    currentLastSample = NumSamples - jj
                    If (currentLastSample > lastSample) Then
                        lastSample = currentLastSample
                    End If
                    Exit For
                End If
            Next jj
        Next ii
        Return lastSample * 1000 / _SmpClkRate_Hz
    End Function
    'Trims the data to a specified number of samples
    Public Sub Trim(ByVal samples As Integer)
        ReDim Preserve _data(7, samples - 1)
    End Sub
    Public Sub Override(ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer)
        'Overrides the generation of exceptions for overlapping waveforms 
        '
        'tstart_msec as Double                      start time for exception override
        'tstop_msec as Double                       stop time for exception override
        'channel as Integer                         (0-7) 
        Dim tstart, tstop As Integer
        Dim nn As Integer
        tstart = Int(tstart_msec * _SmpClkRate_Hz / 1000)
        tstop = Int(tstop_msec * _SmpClkRate_Hz / 1000)
        For nn = tstart To tstop
            _is_set(channel, nn) = False
        Next nn
    End Sub
    Public ReadOnly Property SmpClkRate_Hz() As Double
        Get
            Return _SmpClkRate_Hz
        End Get
    End Property
    Public ReadOnly Property NumSamples() As Long
        Get
            Return _NumSamples
        End Get
    End Property
    Public ReadOnly Property data() As Double(,)
        Get
            Return _data
        End Get
    End Property
    Public Function IsChannelUsed(ByVal channel As Integer) As Boolean
        'channel as Integer  (0-7) 
        Dim i As Integer
        Dim isUsed As Boolean = False
        For i = 0 To _NumSamples - 1
            isUsed = isUsed + _is_set(channel, i)
        Next
        Return isUsed
    End Function
    Private _NumSamples As Long                                            'Number of samples 
    Private _SmpClkRate_Hz As Double                                       'Sample clock rate in Hz 
    Private _AnalogCardIndex As Integer
    '= 3 or 4 as in 'channelsWithCard.txt' differentiate two analog cards, specified in constructor

    Private _data As Double(,)                                             '2D array, 8 by _NumSamples, containing data for DAQ output
    Private _is_set As Boolean(,)                                          '2D array, 8 by _NumSamples, determines whether that channel has been set (collision detection)
End Class


Public Class digitalDAQdata
    ' Data object for Viewpoint systems digital data acquisition boards. The Viewpoint board (see PDF manual) takes a 
    ' series of timestamped scans at which the output values change. 
    Public Sub New(ByVal SmpClkRate_Hz As Double)
        'Class constructor
        '
        'SmpClkRate_Hz as Double                        Sample clock rate in Hz
        _SmpClkRate_Hz = SmpClkRate_Hz
        list = New digitalDAQlist
        Dim initialnode As digitalDAQnode
        initialnode = New digitalDAQnode
        initialnode.timestamp = 0
        Dim ii As Integer
        For ii = 0 To 3
            initialnode.value(ii) = 0
            initialnode.is_set(ii) = 0
            initialnode.function_num = -1
        Next
        list.AddNode(initialnode)

    End Sub
    Public Sub AddPulse(ByVal line_num As Integer, ByVal tstart_msec As Double, ByVal tstop_msec As Double)
        AddCompoundPulse(mask((line_num - 1) Mod 16), tstart_msec, tstop_msec, Int((line_num - 1) / 16))
    End Sub
    Public Sub AddToggle(ByVal pulse_value As Short, ByVal t_msec As Double, ByVal channel As Integer)
        Dim t As Integer
        Dim nodestart As digitalDAQnode
        t = Int(t_msec * SmpClkRate_Hz / 1000)

        nodestart = New digitalDAQnode
        nodestart.timestamp = t
        nodestart.is_set(channel) = pulse_value
        nodestart.value(channel) = pulse_value
        nodestart.function_num = -1
        list.AddNode(nodestart)

    End Sub
    Public Sub AddCompoundPulse(ByVal pulse_value As Short, ByVal tstart_msec As Double, ByVal tstop_msec As Double, ByVal channel As Integer)
        Dim tstart, tstop As Integer
        Dim nodestart, nodestop As digitalDAQnode
        tstart = Int((tstart_msec * SmpClkRate_Hz / 1000))
        tstop = Int((tstop_msec * SmpClkRate_Hz / 1000))

        nodestart = New digitalDAQnode
        nodestart.timestamp = tstart
        nodestart.is_set(channel) = pulse_value
        nodestart.value(channel) = pulse_value
        nodestart.function_num = function_count
        list.AddNode(nodestart)
        'If (Not checknode(nodestart)) Then Throw New System.Exception("Overwriting data in AddPulse")

        nodestop = New digitalDAQnode
        nodestop.timestamp = tstop
        nodestop.is_set(channel) = pulse_value
        nodestop.value(channel) = pulse_value
        nodestop.function_num = function_count
        list.AddNode(nodestop)
        '      If Not checknode(nodestop) Then Throw New System.Exception("insertion collision in AddPulse")

        'If Not checkback(nodestop) Then Throw New System.Exception("Overwriting data in AddPulse")

        function_count = function_count + 1

    End Sub
    'Returns total time for digital data output in ms
    Public Function GetTotalTime() As Double
        Dim d As Double = (list.last.timestamp) * 1000.0 / _SmpClkRate_Hz
        Return d
    End Function

    Private Function checkback(ByVal node As digitalDAQnode) As Boolean
        Dim check As Integer = True
        Dim ii, jj, kk As Integer
        Dim currentnum As Integer
        Dim current As digitalDAQnode
        For ii = 0 To 3
            For jj = 0 To 15
                If (node.is_set(ii) And mask(jj)) Then
                    current = node
                    currentnum = node.function_num
                    For kk = 0 To list.count
                        If current.prevnode Is Nothing Then
                            Exit For
                        ElseIf (current.prevnode.is_set(ii) And mask(jj)) Then
                            If current.prevnode.function_num = currentnum Then
                                Exit For
                            Else
                                check = False
                                Exit For
                            End If
                        End If
                        current = current.prevnode
                    Next kk
                End If
            Next jj
        Next ii
        Return check
    End Function
    Private Function checknode(ByVal node As digitalDAQnode) As Boolean
        Dim check As Integer = True
        Dim ii, jj, kk As Integer
        Dim left_num As Integer = -1, right_num As Integer = -2
        Dim current As digitalDAQnode
        For ii = 0 To 3
            For jj = 0 To 15
                If (node.is_set(ii) And mask(jj)) Then

                    current = node
                    For kk = 0 To list.count
                        If current.prevnode Is Nothing Then
                            Exit For
                        ElseIf (current.prevnode.is_set(ii) And mask(jj)) Then
                            left_num = current.prevnode.function_num
                            Exit For
                        End If
                        current = current.prevnode
                    Next kk

                    current = node
                    For kk = 0 To list.count
                        If current.nextnode Is Nothing Then
                            Exit For
                        ElseIf (current.nextnode.is_set(ii) And mask(jj)) Then
                            right_num = current.nextnode.function_num
                            Exit For
                        End If
                        current = current.nextnode
                    Next kk

                    If right_num = left_num Then check = False
                End If
            Next jj
        Next ii
        Return check
    End Function

    Public Sub AddGlobalShift(ByVal tshift_msec As Double)
        Dim nn As Integer, current As digitalDAQnode
        Dim tshift As Integer = Int((tshift_msec * SmpClkRate_Hz / 1000))
        current = list.first
        For nn = 1 To list.count
            current.timestamp = current.timestamp + tshift
            current = current.nextnode
        Next
    End Sub

    Public ReadOnly Property SmpClkRate_Hz() As Double
        Get
            Return _SmpClkRate_Hz
        End Get
    End Property
    Public ReadOnly Property data() As Short(,)
        Get
            Dim numscans As Integer = list.count - 1, nn As Integer
            Dim runningvalue() As Short = {0, 0, 0, 0}
            Dim tempdata(numscans, 5) As Short
            Dim current As digitalDAQnode = list.first
            For nn = 0 To numscans
                Do

                    runningvalue(0) = runningvalue(0) Xor current.value(0)
                    runningvalue(1) = runningvalue(1) Xor current.value(1)
                    runningvalue(2) = runningvalue(2) Xor current.value(2)
                    runningvalue(3) = runningvalue(3) Xor current.value(3)

                    If (current.timestamp And 65535) < 32768 Then
                        tempdata(nn, 0) = (current.timestamp And 32767)
                    Else
                        tempdata(nn, 0) = -32768 + (current.timestamp And 32767)
                    End If

                    tempdata(nn, 1) = CShort((current.timestamp And -65536) / 65536)
                    tempdata(nn, 2) = runningvalue(0)
                    tempdata(nn, 3) = runningvalue(1)
                    tempdata(nn, 4) = runningvalue(2)
                    tempdata(nn, 5) = runningvalue(3)

                    If current.nextnode Is Nothing Then
                        Exit For
                    End If
                    current = current.nextnode
                Loop While current.prevnode.timestamp = current.timestamp

            Next
            Dim returndata(nn, 5) As Short
            Dim ii, jj As Integer
            For ii = 0 To nn
                For jj = 0 To 5
                    returndata(ii, jj) = tempdata(ii, jj)
                Next
            Next
            Return tempdata

        End Get
    End Property
    Private mask() As Short = {1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, -32768}        'Used for converting integers to bytes. 
    Public ReadOnly list As digitalDAQlist
    Private _data As Short(,)
    Private _SmpClkRate_Hz As Double
    Private function_count As Integer = 0
    Public Class digitalDAQlist
        ' This is a doubly linked list of data nodes, ordered by timestamp. The data stored with each node are 
        ' the channels whose outputs values *change* (from high to low, or low to high) at the associated timestamp. 
        Public first As digitalDAQnode
        Public last As digitalDAQnode
        Public count As Integer
        Public Sub AddNode(ByRef node As digitalDAQnode)
            'standard node addition for a doubly linked list. 

            If first Is Nothing Then
                first = node
                last = node

            ElseIf node.timestamp < first.timestamp Then
                node.nextnode = first
                first.prevnode = node
                first = node
            ElseIf node.timestamp >= last.timestamp Then
                node.prevnode = last
                last.nextnode = node
                last = node
            Else
                Dim nn As Integer, current As digitalDAQnode
                current = first
                For nn = 1 To count
                    If node.timestamp < current.timestamp Then
                        node.prevnode = current.prevnode
                        current.prevnode.nextnode = node
                        node.nextnode = current
                        current.prevnode = node
                        Exit For
                    End If
                    current = current.nextnode
                Next
            End If
            count = count + 1
        End Sub
    End Class
    Public Class digitalDAQnode
        Public timestamp As Integer
        Public value(3) As Short
        Public is_set(3) As Short
        Public function_num As Integer
        Public prevnode As digitalDAQnode
        Public nextnode As digitalDAQnode
    End Class
End Class
