Public Function AddTrackingSequence5Upgrade(ByVal t_start As Double, _
    ByRef cp As clsControlParams, _
    ByRef analogdata As SpectronClient, _
    ByRef analogdata2 As SpectronClient, _
    ByRef digitaldata As digitalDAQdata, _
    ByRef digitaldata2 As digitalDAQdata, _
    ByRef gpib As GPIBControl, _
    ByRef Hermes As KeithleyControl, _
    ByRef dds As AD9959Ev, _
    ByVal tracking_en As Boolean) As Double


	Dim flip_time As Double = t_start
	Dim IT As Double = flip_time + guppy_wait
	Dim last_time = IT + 3 * guppy_interval + 2000
	Dim pulse_start_padding As Double = 200 'shutter_time + 1 '1 ms padding to turn on lattices before rising edge of camera pulse to prevent timing jitter from affecting the images
	Dim t_stop As Double
    
    If tracking_en = True Then
	    '---------------------------------- Invert signals for upgrade ----------------------------------
		'digitaldata2.AddPulse(ixon_flip_mount_ttl, IT + 3 * guppy_interval, last_time)
		'------------------------------------------------------------------------------------------------

	    analogdata.AddStep(DMD_track_765_power, IT - 10 - pulse_start_padding, IT + 3*guppy_interval, lattice2D765_power)
	    analogdata.AddStep(DMD_track_765_power2, IT - 10 - pulse_start_padding, IT + 3*guppy_interval, lattice2D765_power2)
	    analogdata2.AddStep(DMD_track_power_line, IT - 10 - pulse_start_padding, IT + 3*guppy_interval, line_DMD_power)
        analogdata2.AddStep(DMD_track_power_hor, IT - 10 - pulse_start_padding, IT + 3*guppy_interval, hor_DMD_power)
	     
	    digitaldata2.AddPulse(lattice2D765_shutter, IT - 50 - pulse_start_padding, IT + 3*guppy_interval)
	    digitaldata2.AddPulse(lattice2D765_shutter2, IT - 50 - pulse_start_padding, IT + 3*guppy_interval)
	    digitaldata2.AddPulse(line_DMD_shutter, IT - 50 - pulse_start_padding, IT + 3*guppy_interval )
   	    digitaldata2.AddPulse(hor_DMD_shutter, IT - 50 - pulse_start_padding, IT + 3*guppy_interval )

		digitaldata2.AddPulse(lattice2D765_ttl, IT - pulse_start_padding, IT + 2*guppy_interval + guppy_exposure)
		digitaldata2.AddPulse(lattice2D765_ttl2, IT - pulse_start_padding, IT + 2*guppy_interval + guppy_exposure)
		digitaldata2.AddPulse(line_DMD_ttl, IT - pulse_start_padding, IT + 2*guppy_interval + guppy_exposure)
		digitaldata2.AddPulse(hor_DMD_ttl, IT - pulse_start_padding, IT + 2*guppy_interval + guppy_exposure)

	    digitaldata.AddPulse(apogee_camera, IT-1000-shutter_time, IT-1000) 'flush camera
	    digitaldata.AddPulse(apogee_camera, IT-shutter_time, IT) '1
	    digitaldata.AddPulse(apogee_camera, IT-shutter_time + guppy_interval, IT + guppy_interval) '2
	    digitaldata.AddPulse(apogee_camera, IT-shutter_time + 2*guppy_interval, IT + 2*guppy_interval) '3

        t_stop = last_time
    Else
		digitaldata.AddPulse(apogee_camera, IT-1000-shutter_time, IT-1000) 'flush camera
	    digitaldata.AddPulse(apogee_camera, IT-shutter_time, IT) '1
	    digitaldata.AddPulse(apogee_camera, IT-shutter_time + guppy_interval, IT + guppy_interval) '2
	    digitaldata.AddPulse(apogee_camera, IT-shutter_time + 2*guppy_interval, IT + 2*guppy_interval) '3
		t_stop = last_time
        't_stop = t_start
    End If

    Return t_stop
End Function
