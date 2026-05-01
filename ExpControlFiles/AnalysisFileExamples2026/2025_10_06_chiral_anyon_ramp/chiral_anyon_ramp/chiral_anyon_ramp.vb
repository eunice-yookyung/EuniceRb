
'===================
'=====Variables=====
lattice1_max = 45
lattice2_max = 45
pinning_hold_time = 1000
red_freq = 17.0
round_dimple_voltage = 4.23
anticonfine_dur_vert = 40
anticonfine_dur_horz = 40
loading_walls_volt = 0
loading_grad_ramp_dur = 0
loading_grad_init = 0
loading_grad_final = 0
loading_grad_comp = 0
loading_lattice_ramp_dur = 0
loading_lattice2_low_depth = 45
walls2_volt = 2.7
grad_ramp_dur = 0
quic_grad_init = 0
quic_grad_final = 0
lattice_ramp_dur = 0
lattice2_low_depth = 45
lattice_mod_ramp_dur = 0
latticemod_amp_1 = 0.2
latticemod_amp_2 = 0.2
latticemod_amp_3 = 0.2
mod_freq_Hz_1 = 608
mod_freq_Hz_2 = 795
mod_freq_Hz_3 = 992
phase_ramp_dur = 0
phase_pi_init = 0
phase_pi_final = 0
hold_dur = 0
is_return = 0
full_counting = 0
doublon_splitting = 0
stop_time = 0
free_var = 0
'=====Variables=====
'===================

'-------------------------------------------------------------------------------------------------------------------------------------------------- Constants

Dim shunt_switch_dur As Double = 10
Dim coil_ramp_dur As Double = 30

'Loading line constants
Dim gravity_offsets_switch_dur As Double = 10
Dim twod_ramp_dur As Double = 5
Dim anticonfine_volt As Double = 2.5
Dim anticonfine_ramp_dur As Double = 1
Dim lattice1_amb_volt As Double = 1.4
Dim lattice2_amb_volt As Double = 1.8

'DMD specific durations
Dim PID_response_dur As Double = 1 'needed to fix spikes in line_DMD
Dim walls_onoff_dur As Double = 5
Dim loadline_DMD_volt_vert As Double = 3.3
Dim loadline_DMD_volt_horz As Double = 3.1 '3.1
Dim DMD_hw_delay As Double = -0.16

'Lattice ramp constants
Dim lattice_freeze_dur As Double = 0.5

' B field ramp constants
Dim grad_onoff_dur As Double = 30
Dim loading_grad_onoff_dur As Double = 30

'Lattice depth for full counting statistics
Dim lattice1_full_counting_depth As Double = 2.5

'Berlin wall for full counting of two columns separately
Dim Berlin_wall_expansion_volt As Double = 3.05

'For doublon splitting
Dim splitting_lattice_dur As Double = 30
Dim splitting_turnon_dur As Double = 30
Dim splitting_ramp_dur As Double = 200
Dim lattice2_doublon_depth As Double = 15
Dim splitting_quic_start As Double = 6.5
Dim splitting_quic_end As Double = 2.3


'----------------------------------------------------------- MOT Creation ---------------------------------------------------------------------------------------

digitaldata2.AddPulse(clock_resynch,1,4)
analogdata.DisableClkDist(0.95, 4.05)
analogdata2.DisableClkDist(0.95, 4.05)

Dim MOT_end_time As Double
MOT_end_time = Me.AddMOTSequenceUpgrade(0, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim transport_start_time As Double = MOT_end_time


'----------------------------------------------------------- Transport ------------------------------------------------------------------------------------------

Dim transport_end_time As Double
transport_end_time = Me.AddTransportSequence(transport_start_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)


'----------------------------------------------------------- Evaporation ----------------------------------------------------------------------------------------

Dim evaporation_end_time As Double
evaporation_end_time = Me.AddEvaporationSequence(transport_end_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)


'----------------------------------------------------------- Make Mott Insulator --------------------------------------------------------------------------------

Dim MI_variables As Double() = Me.AddMottInsulatorSequencev2(evaporation_end_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim twodphysics_start_time As Double = MI_variables(0)
Dim lattice1_max_volt As Double = MI_variables(1)
Dim lattice2_max_volt As Double = MI_variables(2)
Dim end_dipole_voltage As Double = MI_variables(3) 

'--------------------------------------------------------------------- Tracking -----------------------------------------------------------------------------------

Dim delay_tracking As Double = 2000 '1000
Dim tracking_end_time As Double = Me.AddTrackingSequenceUpgrade(delay_tracking, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)


'-------------------------------------------------------------------------------------------------------------------------------------------------- Time Definitions for Cutting

'Time constants for cutting vertical line
Dim line_load_vert_start_time = twodphysics_start_time
Dim cutting1_turnon_start_time As Double = line_load_vert_start_time
Dim cutting1_turnon_end_time As Double = cutting1_turnon_start_time + 5
Dim twod1_rampdown_start_time As Double = cutting1_turnon_end_time
Dim twod1_rampdown_end_time As Double = twod1_rampdown_start_time + twod_ramp_dur
Dim expand_vert_start_time As Double = twod1_rampdown_end_time
Dim expand_vert_end_time As Double = expand_vert_start_time + anticonfine_ramp_dur + anticonfine_dur_vert
Dim twod1_reload_start_time As Double = expand_vert_end_time
Dim twod1_reload_end_time As Double = twod1_reload_start_time + twod_ramp_dur
Dim cutting1_turnoff_start_time As Double = twod1_reload_end_time 'also ramp down anticonfine here, so can ramp back on for the next direction
Dim cutting1_turnoff_end_time As Double = cutting1_turnoff_start_time + 5
Dim line_load_mid_time As Double = cutting1_turnoff_end_time
'skip if zero
If (anticonfine_dur_vert = 0) Then
	line_load_mid_time = line_load_vert_start_time + 20 + PID_response_dur 
End If

'time constants for cutting horizontal line
Dim line_load_horz_start_time As Double = line_load_mid_time + PID_response_dur
Dim cutting2_turnon_start_time As Double = line_load_horz_start_time
Dim cutting2_turnon_end_time As Double = cutting2_turnon_start_time + 5
Dim twod2_rampdown_start_time As Double = cutting2_turnon_end_time
Dim twod2_rampdown_end_time As Double = twod2_rampdown_start_time + twod_ramp_dur
Dim expand_horz_start_time As Double = twod2_rampdown_end_time
Dim expand_horz_end_time As Double = expand_horz_start_time + anticonfine_ramp_dur + anticonfine_dur_horz
Dim twod2_reload_start_time As Double = expand_horz_end_time
Dim twod2_reload_end_time As Double = twod2_reload_start_time + twod_ramp_dur
Dim cutting2_turnoff_start_time As Double = twod2_reload_end_time  'also ramp down anticonfine here, so can ramp back on for the next direction
Dim cutting2_turnoff_end_time As Double = cutting2_turnoff_start_time + 5
Dim walls2_start_time as Double = cutting2_turnoff_end_time + 5
Dim line_load_end_time As Double = cutting2_turnoff_end_time
'skip if zero
If (anticonfine_dur_horz = 0) Then
	line_load_end_time = line_load_mid_time + 20 + PID_response_dur
End If

'Ramp gravity offsets
Dim go_lower_start_time As Double = line_load_end_time
Dim go_lower_end_time As Double = go_lower_start_time + gravity_offsets_switch_dur


'----------------------------------------------------------- Time Definitions for Physics ---------------------------------------------------------------------

' -------- Load initial state (two particles separated by 1 site)

'() Turn on gradient for state loading
Dim loading_grad_turnon_start_time As Double = go_lower_end_time 
Dim loading_grad_turnon_end_time As Double = loading_grad_turnon_start_time + loading_grad_onoff_dur

'() turn on walls (low power)
Dim loading_walls_turnon_start_time As Double = loading_grad_turnon_end_time
Dim loading_walls_turnon_end_time As Double = loading_walls_turnon_start_time + walls_onoff_dur

'() lower lattice depth
Dim loading_lattice_lower_start_time = loading_walls_turnon_end_time
Dim loading_lattice_lower_end_time = loading_lattice_lower_start_time + loading_lattice_ramp_dur

'() Ramp down tilt
Dim loading_grad_lower_start_time As Double = loading_lattice_lower_end_time
Dim loading_grad_lower_end_time As Double = loading_grad_lower_start_time + loading_grad_ramp_dur

'() Raise lattice
Dim loading_lattice_raise_start_time As Double = loading_grad_lower_end_time
Dim loading_lattice_raise_end_time As Double = loading_lattice_raise_start_time + loading_lattice_ramp_dur

' -------- Delocalize in 1D 

'() Turn on gradient
Dim grad_turnon_start_time As Double = loading_lattice_raise_end_time 
Dim grad_turnon_end_time As Double = grad_turnon_start_time + grad_onoff_dur

'() Turn off DMD walls, then switch and turn back on
Dim loading_walls_turnoff_start_time As Double = grad_turnon_end_time
Dim loading_walls_turnoff_end_time As Double = loading_walls_turnoff_start_time + walls_onoff_dur
Dim walls_turnon_start_time As Double = loading_walls_turnoff_end_time + 5 + 2*PID_response_dur
Dim walls_turnon_end_time As Double = walls_turnon_start_time + walls_onoff_dur

' SWAP ORDER ???
'() Lower lattice depth 
Dim lattice_lower_start_time As Double = walls_turnon_end_time
Dim lattice_lower_end_time As Double = lattice_lower_start_time + lattice_ramp_dur

'() Perform 3-component modulation - Ramp part
Dim mod_start_time As Double = lattice_lower_end_time
Dim mod_ramp_end_time As Double = mod_start_time + lattice_mod_ramp_dur
Dim mod_hold_dur As Double = grad_ramp_dur

'() Ramp gradient to final value while doing the 3-tone modulation
Dim grad_lower_start_time As Double = mod_ramp_end_time
Dim grad_lower_end_time As Double = grad_lower_start_time + grad_ramp_dur

'() Ramp statistical phase
Dim phase_ramp_start_time As Double = grad_lower_end_time
Dim phase_ramp_end_time As Double = phase_ramp_start_time + phase_ramp_dur

'() Hold after ramp
Dim hold_start_time As Double = phase_ramp_end_time
Dim hold_end_time As Double = hold_start_time + hold_dur

' -------- Return

'() RETURN: raise gradient
Dim grad_raise_start_time As Double = hold_end_time
Dim grad_raise_end_time As Double = grad_raise_start_time
If (is_return > 0) Then
	grad_raise_end_time = grad_raise_start_time + grad_ramp_dur
End If

'() RETURN: turn off modulation
Dim mod_rampdown_start_time As Double = grad_raise_end_time
Dim mod_rampdown_end_time As Double = mod_rampdown_start_time
If (is_return > 0) Then
	mod_rampdown_end_time = mod_rampdown_start_time + lattice_mod_ramp_dur
End If
Dim mod_end_time As Double = mod_rampdown_end_time

'() RETURN: raise lattice depth
Dim lattice_raise_start_time As Double = mod_rampdown_end_time
Dim lattice_raise_end_time As Double = lattice_raise_start_time + lattice_freeze_dur
If (is_return > 0) Then
	lattice_raise_end_time = lattice_raise_start_time + lattice_ramp_dur
End If

'(11) Turn off DMD walls 
Dim walls_turnoff_start_time As Double = lattice_raise_end_time
Dim walls_turnoff_end_time As Double = walls_turnoff_start_time + walls_onoff_dur


' --------- Full counting methods

'() Doublon splitting
Dim quic_turnon_splitting_start_time As Double = walls_turnoff_end_time
Dim quic_turnon_splitting_end_time As Double = quic_turnon_splitting_start_time
If (doublon_splitting > 0) Then
	quic_turnon_splitting_end_time = quic_turnon_splitting_start_time + splitting_turnon_dur
End If

Dim lower_lattice_splitting_start_time As Double = quic_turnon_splitting_end_time
Dim lower_lattice_splitting_end_time As Double = lower_lattice_splitting_start_time
If (doublon_splitting > 0) Then
	lower_lattice_splitting_end_time = lower_lattice_splitting_start_time + splitting_lattice_dur
End If

Dim quic_ramp_splitting_start_time As Double = lower_lattice_splitting_end_time
Dim quic_ramp_splitting_end_time As Double = quic_ramp_splitting_start_time
If (doublon_splitting > 0) Then
	quic_ramp_splitting_end_time = quic_ramp_splitting_start_time + splitting_ramp_dur
End If

Dim freeze_lattice_splitting_start_time As Double = quic_ramp_splitting_end_time
Dim freeze_lattice_splitting_end_time As Double = freeze_lattice_splitting_start_time
If (doublon_splitting > 0) Then
	freeze_lattice_splitting_end_time = freeze_lattice_splitting_start_time + lattice_freeze_dur
End If

'() turn off gradients
Dim grad_turnoff_start_time As Double = freeze_lattice_splitting_end_time 'turn off coils when is_return = 0
Dim grad_turnoff_end_time As Double = grad_turnoff_start_time + grad_onoff_dur

Dim twodphysics_end_time As Double = grad_turnoff_end_time + stop_time

'() Full counting statistics
Dim full_counting_start_time as Double
Dim full_counting_hold_start_time as Double
Dim full_counting_hold_end_time as Double
Dim full_counting_end_ramp_time as Double
Dim full_counting_end_time As Double
Dim berlin_wall_rampup_start_time As Double
Dim berlin_wall_rampup_end_time As Double
Dim berlin_wall_rampdown_start_time As Double
Dim berlin_wall_rampdown_end_time As Double

Dim full_counting_rampdown_dur As Double = 2
Dim full_counting_hold_dur As Double = 2.3
Dim full_counting_rampup_dur As Double = 1
Dim full_counting_hold_after_dur As Double = 10
If (full_counting > 0) Then
	full_counting_start_time = grad_turnoff_end_time
	full_counting_hold_start_time = full_counting_start_time + full_counting_rampdown_dur
	full_counting_hold_end_time = full_counting_hold_start_time + full_counting_hold_dur
	full_counting_end_ramp_time = full_counting_hold_end_time + full_counting_rampup_dur
	full_counting_end_time = full_counting_end_ramp_time + full_counting_hold_after_dur
	twodphysics_end_time = full_counting_end_time + stop_time	

	berlin_wall_rampup_start_time = full_counting_hold_start_time - 5
	berlin_wall_rampup_end_time = berlin_wall_rampup_start_time + 5
	berlin_wall_rampdown_start_time = full_counting_end_ramp_time
	berlin_wall_rampdown_end_time = berlin_wall_rampdown_start_time + 5
End If

'() freeze lattices, turn off gradients
Dim lattice_freeze_start_time As Double = twodphysics_end_time
Dim lattice_freeze_end_time As Double = lattice_freeze_start_time + lattice_freeze_dur

'() Pinning, image
Dim pinning_start_time As Double = lattice_freeze_end_time 


'------------------------------------------------------------------------- Pinning -------------------------------------------------------------------------

Dim pinning_times As Double() = Me.AddPinningSequence(pinning_start_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim pinning_end_time As Double = pinning_times(0)
Dim pinning_ready_time As Double = pinning_times(1)
Dim molasses_start_time As Double = pinning_times(2)

Dim IT As Double = pinning_end_time + TOF
Dim last_time As Double = IT


'--------------------------------------------------------------------- Invert signals ---------------------------------------------------------------------

digitaldata.AddPulse(mot_low_current, transport_start_time, last_time) 'MOT Current Supply
digitaldata.AddPulse(ta_shutter, transport_start_time, last_time) 'TA Shutter. starts to close early, so the shutter is closed ASAP after the blow away pulse.
digitaldata.AddPulse(repump_shutter, transport_start_time, last_time) 'Repump Shutter
digitaldata2.AddPulse(ixon_flip_mount_ttl, tracking_end_time, last_time)


'------------------------------------------------------------ Lattice Depth Conversion to Volts ------------------------------------------------------------

Dim loading_lattice2_low_volt As Double = lattice2_voltage_offset + lattice2_calib_volt + .5*Log10(loading_lattice2_low_depth/lattice2_calib_depth)
Dim lattice2_low_volt As Double = lattice2_voltage_offset + lattice2_calib_volt + .5*Log10(lattice2_low_depth/lattice2_calib_depth)
Dim lattice2_doublon_volt As Double = lattice2_voltage_offset + lattice2_calib_volt + 0.5 * Log10(lattice2_doublon_depth / lattice2_calib_depth)


'--------------------------------------------------------------------- B field MOSFETS ------------------------------------------------------------------------

' quad MOSFETS
'digitaldata2.AddPulse(ps8_shunt, twodphysics_start_time, pinning_ready_time)

' quic MOSFETS
digitaldata.AddPulse(ps5_enable, twodphysics_start_time, pinning_ready_time)

' quad shim (grad offset)
digitaldata.AddPulse(quad_shim, twodphysics_start_time, go_lower_end_time + gravity_offsets_switch_dur)
digitaldata2.AddPulse(single_quad_shim, twodphysics_start_time, go_lower_end_time + gravity_offsets_switch_dur)

' quic mirror (grad offset)
digitaldata.AddPulse(bias_enable, twodphysics_start_time, pinning_ready_time)
digitaldata2.AddPulse(ioffe_mirror_fet, twodphysics_start_time, pinning_ready_time)
digitaldata.AddPulse(mirror_polarity, go_lower_end_time, pinning_ready_time)


'---------------------------------------------------------------------- Hold Lattices ----------------------------------------------------------------------

digitaldata2.AddPulse(lattice2D765_ttl, twodphysics_start_time, molasses_start_time)
digitaldata2.AddPulse(lattice2D765_ttl2, twodphysics_start_time, molasses_start_time)
digitaldata2.AddPulse(lattice2D765_shutter, twodphysics_start_time, molasses_start_time)
digitaldata2.AddPulse(lattice2D765_shutter2, twodphysics_start_time, molasses_start_time)
digitaldata.AddPulse(ttl_axial_lattice, twodphysics_start_time, molasses_start_time+1)
digitaldata.AddPulse(axial_lattice_shutter, twodphysics_start_time, molasses_start_time)


'--------------------------------------------------------------------- Blue Donut --------------------------------------------------------------------------------

'ramp down Blue Donut for physics 
digitaldata2.AddPulse(anticonfin_ttl, twodphysics_start_time, line_load_vert_start_time)
digitaldata2.AddPulse(anticonfin_shutter, twodphysics_start_time, line_load_vert_start_time)
analogdata.AddSmoothRamp(end_dipole_voltage, 1.44, twodphysics_start_time, line_load_vert_start_time, red_dipole_power)

'--------------------------------------------------------------------- Anticonfine Beam --------------------------------------------------------------------------

'anticonfinement during DMD line loading
digitaldata.AddPulse(blue_dipole_shutter, line_load_vert_start_time - 20, line_load_end_time)

If (anticonfine_dur_vert > 0) Then
	digitaldata2.AddPulse(blue_dipole_ttl, line_load_vert_start_time, line_load_mid_time)
	analogdata.AddSmoothRamp(1, anticonfine_volt, twod1_rampdown_start_time, twod1_rampdown_end_time, red_dipole_power)
	analogdata.AddStep(anticonfine_volt, twod1_rampdown_end_time, twod1_reload_start_time, red_dipole_power)
	analogdata.AddSmoothRamp(anticonfine_volt, 1, twod1_reload_start_time, twod1_reload_end_time, red_dipole_power)
End If

If (anticonfine_dur_horz > 0) Then
	digitaldata2.AddPulse(blue_dipole_ttl, line_load_horz_start_time, line_load_end_time)
	analogdata.AddSmoothRamp(1, anticonfine_volt, twod2_rampdown_start_time, twod2_rampdown_end_time, red_dipole_power)
	analogdata.AddStep(anticonfine_volt, twod2_rampdown_end_time,  twod2_reload_start_time, red_dipole_power)
	analogdata.AddSmoothRamp(anticonfine_volt, 1, twod2_reload_start_time, twod2_reload_end_time, red_dipole_power)
End If


'--------------------------------------------------------------------- Lattice Drop and Expulsion for DMD --------------------------------------------------------

analogdata.AddStep(lattice1_max_volt, twodphysics_start_time, line_load_vert_start_time, lattice2D765_power)
analogdata.AddStep(lattice2_max_volt, twodphysics_start_time, line_load_vert_start_time, lattice2D765_power2)

If (anticonfine_dur_vert > 0) Then
	'2D1
    analogdata.AddStep(lattice1_max_volt, line_load_vert_start_time, twod1_rampdown_start_time, lattice2D765_power)
	analogdata.AddSmoothRamp(lattice1_max_volt, lattice1_amb_volt, twod1_rampdown_start_time,twod1_rampdown_end_time,lattice2D765_power)
    analogdata.AddStep(lattice1_amb_volt, twod1_rampdown_end_time, twod1_reload_start_time, lattice2D765_power)
    analogdata.AddSmoothRamp(lattice1_amb_volt, lattice1_max_volt, twod1_reload_start_time, twod1_reload_end_time,lattice2D765_power)
	analogdata.AddStep(lattice1_max_volt, twod1_reload_end_time, line_load_mid_time, lattice2D765_power)
	'2D2
	analogdata.AddStep(lattice2_max_volt, line_load_vert_start_time, line_load_mid_time, lattice2D765_power2)
Else
	analogdata.AddStep(lattice1_max_volt, line_load_vert_start_time, line_load_mid_time, lattice2D765_power)
	analogdata.AddStep(lattice2_max_volt, line_load_vert_start_time, line_load_mid_time, lattice2D765_power2)
End If

If (anticonfine_dur_horz > 0) Then
	'2D1
	analogdata.AddStep(lattice1_max_volt, line_load_mid_time, line_load_end_time, lattice2D765_power)
	'2D2
	analogdata.AddStep(lattice2_max_volt, line_load_mid_time, twod2_rampdown_start_time, lattice2D765_power2)
	analogdata.AddSmoothRamp(lattice2_max_volt, lattice2_amb_volt, twod2_rampdown_start_time, twod2_rampdown_end_time, lattice2D765_power2)
   	analogdata.AddStep(lattice2_amb_volt, twod2_rampdown_end_time, twod2_reload_start_time, lattice2D765_power2)
    analogdata.AddSmoothRamp(lattice2_amb_volt, lattice2_max_volt, twod2_reload_start_time, twod2_reload_end_time, lattice2D765_power2)
    analogdata.AddStep(lattice2_max_volt, twod2_reload_end_time, line_load_end_time, lattice2D765_power2)
Else
	analogdata.AddStep(lattice1_max_volt, line_load_mid_time, line_load_end_time, lattice2D765_power)
	analogdata.AddStep(lattice2_max_volt, line_load_mid_time, line_load_end_time, lattice2D765_power2)
End If


'--------------------------------------------------------------------- Lattices -------------------------------------------------------------------------------------

'Hold axial lattice
analogdata.AddStep(axial_voltage, twodphysics_start_time, pinning_ready_time, axial_lattice_power)
analogdata.AddSmoothRamp(axial_voltage, 1.76, pinning_ready_time, molasses_start_time, axial_lattice_power)

' Hold 2D1, lower for full counting
If (full_counting > 0) Then
	analogdata.AddStep(lattice1_max_volt, line_load_end_time, full_counting_start_time, lattice2D765_power)
	analogdata.AddSmoothRamp(lattice1_max_volt, lattice1_full_counting_depth, full_counting_start_time, full_counting_hold_start_time, lattice2D765_power)
	analogdata.AddStep(lattice1_full_counting_depth, full_counting_hold_start_time, full_counting_hold_end_time, lattice2D765_power)
	analogdata.AddSmoothRamp(lattice1_full_counting_depth, lattice1_max_volt, full_counting_hold_end_time, full_counting_end_ramp_time, lattice2D765_power)
	analogdata.AddStep(lattice1_max_volt, full_counting_end_ramp_time, lattice_freeze_start_time, lattice2D765_power)
Else
	analogdata.AddStep(lattice1_max_volt, line_load_end_time, lattice_freeze_start_time, lattice2D765_power)
End If

'State Prep:

' Hold 2D2
analogdata.AddStep(lattice2_max_volt, line_load_end_time, loading_lattice_lower_start_time, lattice2D765_power2) '2D2

' Lower 2D2
analogdata.AddSmoothRamp(lattice2_max_volt, loading_lattice2_low_volt, loading_lattice_lower_start_time, loading_lattice_lower_end_time, lattice2D765_power2)
analogdata.AddStep(loading_lattice2_low_volt, loading_lattice_lower_end_time, loading_lattice_raise_start_time, lattice2D765_power2)

' Raise 2D2
analogdata.AddSmoothRamp(loading_lattice2_low_volt, lattice2_max_volt, loading_lattice_raise_start_time, loading_lattice_raise_end_time, lattice2D765_power2)
analogdata.AddStep(lattice2_max_volt, loading_lattice_raise_end_time, lattice_lower_start_time, lattice2D765_power2)

'Main sequence:

' Lower 2D2
analogdata.AddSmoothRamp(lattice2_max_volt, lattice2_low_volt, lattice_lower_start_time, lattice_lower_end_time, lattice2D765_power2)
analogdata.AddStep(lattice2_low_volt, lattice_lower_end_time, mod_start_time, lattice2D765_power2)

' ---- Modulation
If (is_return > 0) Then
	' ramp up and return
	analogdata.AddRampLogSine3PhaseReturn(lattice2_low_depth, latticemod_amp_1, latticemod_amp_2, latticemod_amp_3, mod_freq_Hz_1, mod_freq_Hz_2, mod_freq_Hz_3, 
										  phase_pi_final, phase_pi_final, phase_ramp_dur, mod_start_time, lattice_mod_ramp_dur, mod_hold_dur + hold_dur - phase_ramp_dur, 
										  lattice2_voltage_offset, lattice2_calib_volt, lattice2_calib_depth, lattice2D765_power2)
End If
' ramp up (overwrite first half of return ramp to include phase ramp)
analogdata.AddRampLogSine3PhaseNoReturn(lattice2_low_depth, latticemod_amp_1, latticemod_amp_2, latticemod_amp_3, mod_freq_Hz_1, mod_freq_Hz_2, mod_freq_Hz_3, phase_pi_init, phase_pi_final, phase_ramp_dur, mod_start_time, lattice_mod_ramp_dur, mod_hold_dur, lattice2_voltage_offset, lattice2_calib_volt, lattice2_calib_depth, lattice2D765_power2)
analogdata.AddLogSine3Phase(lattice2_low_depth, latticemod_amp_1, latticemod_amp_2, latticemod_amp_3, mod_freq_Hz_1, mod_freq_Hz_2, mod_freq_Hz_3, phase_pi_final, hold_start_time, hold_end_time, phase_ramp_end_time-mod_start_time, lattice2_voltage_offset, lattice2_calib_volt, lattice2_calib_depth, lattice2D765_power2)

' Raise 2D2
analogdata.AddSmoothRamp(lattice2_low_volt, lattice2_max_volt, lattice_raise_start_time, lattice_raise_end_time, lattice2D765_power2)
analogdata.AddStep(lattice2_max_volt, lattice_raise_end_time, lower_lattice_splitting_start_time, lattice2D765_power2)

'Doublon detection
If (doublon_splitting > 0) Then
	analogdata.AddSmoothRamp(lattice2_max_volt, lattice2_doublon_volt, lower_lattice_splitting_start_time, lower_lattice_splitting_end_time, lattice2D765_power2) 
    analogdata.AddStep(lattice2_doublon_volt, quic_ramp_splitting_start_time, quic_ramp_splitting_end_time, lattice2D765_power2) 
    analogdata.AddSmoothRamp(lattice2_doublon_volt, lattice2_max_volt, freeze_lattice_splitting_start_time, freeze_lattice_splitting_end_time, lattice2D765_power2)
    analogdata.AddStep(lattice2_max_volt, freeze_lattice_splitting_end_time, lattice_freeze_start_time, lattice2D765_power2)
Else
	analogdata.AddStep(lattice2_max_volt, lower_lattice_splitting_start_time, lattice_freeze_start_time, lattice2D765_power2)
End If

'freeze lattices
analogdata.AddSmoothRamp(lattice2_max_volt, lattice2_deepest_volt, lattice_freeze_start_time, lattice_freeze_end_time, lattice2D765_power2)
analogdata.AddStep(lattice2_deepest_volt, lattice_freeze_end_time, pinning_ready_time, lattice2D765_power2)
analogdata.AddSmoothRamp(lattice1_max_volt, lattice1_deepest_volt, lattice_freeze_start_time, lattice_freeze_end_time, lattice2D765_power)
analogdata.AddStep(lattice1_deepest_volt, lattice_freeze_end_time, pinning_ready_time, lattice2D765_power)


'--------------------------------------------------------------------- Magnetic Fields ----------------------------------------------------------------------------------------------------------

' ramp down offsets after cutting
' quad offset
analogdata.AddStep(quad_latt_grad_offset, twodphysics_start_time, go_lower_start_time, ps1_ao)
analogdata.AddSmoothRamp(quad_latt_grad_offset, 0, go_lower_start_time, go_lower_end_time, ps1_ao)
' quic offset
analogdata.AddStep(quic_latt_grad_offset, twodphysics_start_time, go_lower_start_time, ps6_ao)
analogdata.AddSmoothRamp(quic_latt_grad_offset, 0, go_lower_start_time, go_lower_end_time, ps6_ao)

' state prep (loading)
' PS 5 (quic)
analogdata2.AddStep(0, twodphysics_start_time, loading_grad_turnon_start_time, ps5_ao)
analogdata2.AddSmoothRamp(0, loading_grad_init*ps5_scaler, loading_grad_turnon_start_time, loading_grad_turnon_end_time, ps5_ao)
analogdata2.AddStep(loading_grad_init*ps5_scaler, loading_grad_turnon_end_time, loading_grad_lower_start_time, ps5_ao)
analogdata2.AddSmoothRamp(loading_grad_init*ps5_scaler, loading_grad_final*ps5_scaler, loading_grad_lower_start_time, loading_grad_lower_end_time, ps5_ao)
analogdata2.AddStep(loading_grad_final*ps5_scaler, loading_grad_lower_end_time, grad_turnon_start_time, ps5_ao)
' PS 6 (quic mirror)
analogdata.AddSmoothRamp(0, loading_grad_comp, loading_grad_turnon_start_time, loading_grad_turnon_end_time, ps6_ao)
analogdata.AddStep(loading_grad_comp, loading_grad_turnon_end_time, grad_turnon_start_time, ps6_ao)
analogdata.AddSmoothRamp(loading_grad_comp, 0, grad_turnon_start_time, grad_turnon_end_time, ps6_ao)

' 2D delocalization
' PS 5 (quic)
Dim quic_grad_end As Double = loading_grad_final*ps5_scaler
analogdata2.AddSmoothRamp(loading_grad_final*ps5_scaler, quic_grad_init*ps5_scaler, grad_turnon_start_time, grad_turnon_end_time, ps5_ao)
analogdata2.AddStep(quic_grad_init*ps5_scaler, grad_turnon_end_time, grad_lower_start_time, ps5_ao)
analogdata2.AddSmoothRamp(quic_grad_init*ps5_scaler, quic_grad_final*ps5_scaler, grad_lower_start_time, grad_lower_end_time, ps5_ao)
analogdata2.AddStep(quic_grad_final*ps5_scaler, grad_lower_end_time, hold_end_time, ps5_ao)
quic_grad_end = quic_grad_final*ps5_scaler
If (is_return > 0) Then
	analogdata2.AddStep(quic_grad_final*ps5_scaler, hold_end_time, grad_raise_start_time, ps5_ao)
	analogdata2.AddSmoothRamp(quic_grad_final*ps5_scaler, quic_grad_init*ps5_scaler, grad_raise_start_time, grad_raise_end_time, ps5_ao)
	quic_grad_end = quic_grad_init*ps5_scaler
End If
analogdata2.AddStep(quic_grad_end, grad_raise_end_time, quic_turnon_splitting_start_time, ps5_ao)

' Doublon splitting
If (doublon_splitting > 0) Then
	analogdata2.AddSmoothRamp(quic_grad_end, splitting_quic_start*ps5_scaler, quic_turnon_splitting_start_time, quic_turnon_splitting_end_time, ps5_ao)
	analogdata2.AddStep(splitting_quic_start*ps5_scaler, quic_turnon_splitting_end_time, quic_ramp_splitting_start_time, ps5_ao)
	'analogdata2.AddSmoothRamp(splitting_quic_start*ps5_scaler, splitting_quic_end*ps5_scaler, quic_ramp_splitting_start_time, quic_ramp_splitting_end_time, ps5_ao)
	analogdata2.AddSmoothRamp(splitting_quic_start*ps5_scaler, splitting_quic_end*ps5_scaler, quic_ramp_splitting_start_time, quic_ramp_splitting_end_time, ps5_ao)	
	analogdata2.AddStep(splitting_quic_end*ps5_scaler, quic_ramp_splitting_end_time, grad_turnoff_start_time, ps5_ao)
	quic_grad_end = splitting_quic_end*ps5_scaler
Else
	analogdata2.AddStep(quic_grad_end, quic_turnon_splitting_start_time, grad_turnoff_start_time, ps5_ao)
End If

analogdata2.AddSmoothRamp(quic_grad_end, 0, grad_turnoff_start_time, grad_turnoff_end_time, ps5_ao)


'-------------------------------------------------------------------------- DMD Code ------------------------------------------------------------------------

'this line to bring trigger active high, but also serve as the tracking line trigger (on its falling edge, with 6ms delay)
digitaldata2.AddPulse(line_DMD_trigger, evaporation_end_time, molasses_start_time + fluo_image_wait ) 'tracking line
digitaldata2.AddPulse(hor_DMD_trigger, evaporation_end_time, molasses_start_time + fluo_image_wait )'tracking hor

'Now all these are inverted and triggers on the second time stamp (real rising edge)
'pattern switching after a 160us delay, switching itself takes few us or less
digitaldata2.AddPulse(line_DMD_trigger, DMD_hw_delay + line_load_vert_start_time - 0.5, DMD_hw_delay + line_load_vert_start_time) 'cut first direction (line DMD)
digitaldata2.AddPulse(hor_DMD_trigger, DMD_hw_delay + line_load_horz_start_time - 0.5, DMD_hw_delay + line_load_horz_start_time) 'cut second direction (hor DMD)
digitaldata2.AddPulse(hor_DMD_trigger, DMD_hw_delay + loading_walls_turnon_start_time - 0.5, DMD_hw_delay + loading_walls_turnon_start_time) 'Walls (hor DMD)
digitaldata2.AddPulse(hor_DMD_trigger, DMD_hw_delay + walls_turnon_start_time - 0.5, DMD_hw_delay + walls_turnon_start_time) 'Walls (hor DMD)
digitaldata2.AddPulse(line_DMD_trigger, DMD_hw_delay + walls_turnoff_end_time, DMD_hw_delay + walls_turnoff_end_time + 0.5) 'Berlin wall (line DMD)

'Shutter
'digitaldata2.AddPulse(line_DMD_shutter, cutting1_turnon_start_time - 20, molasses_start_time)
digitaldata2.AddPulse(line_DMD_shutter, cutting1_turnon_start_time - 20, twodphysics_start_time)
digitaldata2.AddPulse(line_DMD_shutter, berlin_wall_rampup_start_time - 20, molasses_start_time)
digitaldata2.AddPulse(hor_DMD_shutter, cutting2_turnon_start_time - 20, molasses_start_time)

'Cut one direction with line DMD
If (anticonfine_dur_vert > 0) Then
	digitaldata2.AddPulse(line_DMD_ttl, cutting1_turnon_start_time - PID_response_dur/2, line_load_mid_time + PID_response_dur/2) 
	analogdata2.AddSmoothRamp(line_DMD_start_volt, loadline_DMD_volt_vert, cutting1_turnon_start_time, cutting1_turnon_end_time, line_DMD_power)
	analogdata2.AddStep(loadline_DMD_volt_vert, cutting1_turnon_end_time, cutting1_turnoff_start_time, line_DMD_power)
	analogdata2.AddSmoothRamp(loadline_DMD_volt_vert, line_DMD_start_volt, cutting1_turnoff_start_time, cutting1_turnoff_end_time, line_DMD_power)
End If

' Cut second direction with hor DMD
If (anticonfine_dur_horz > 0) Then
	digitaldata2.AddPulse(hor_DMD_ttl, cutting2_turnon_start_time - PID_response_dur/2, line_load_end_time + PID_response_dur/2)
	analogdata2.AddSmoothRamp(line_DMD_start_volt, loadline_DMD_volt_horz, cutting2_turnon_start_time, cutting2_turnon_end_time, hor_DMD_power)
	analogdata2.AddStep(loadline_DMD_volt_horz, cutting2_turnon_end_time, cutting2_turnoff_start_time, hor_DMD_power)
	analogdata2.AddSmoothRamp(loadline_DMD_volt_horz, line_DMD_start_volt, cutting2_turnoff_start_time, cutting2_turnoff_end_time, hor_DMD_power)
End If

' walls2 state prep (hor DMD)
If (loading_walls_volt > 0) Then
	digitaldata2.AddPulse(hor_DMD_ttl, loading_walls_turnon_start_time - PID_response_dur/2, loading_walls_turnoff_end_time + PID_response_dur/2)
	analogdata2.AddSmoothRamp(line_DMD_start_volt, loading_walls_volt, loading_walls_turnon_start_time, loading_walls_turnon_end_time, hor_DMD_power)
	analogdata2.AddStep(loading_walls_volt, loading_walls_turnon_end_time, loading_walls_turnoff_start_time, hor_DMD_power)
	analogdata2.AddSmoothRamp(loading_walls_volt, line_DMD_start_volt, loading_walls_turnoff_start_time, loading_walls_turnoff_end_time, hor_DMD_power)
End If

' walls2 (hor DMD)
If (walls2_volt > 0) Then
	digitaldata2.AddPulse(hor_DMD_ttl, walls_turnon_start_time - PID_response_dur/2, walls_turnoff_end_time + PID_response_dur/2)
	analogdata2.AddSmoothRamp(line_DMD_start_volt, walls2_volt, walls_turnon_start_time, walls_turnon_end_time, hor_DMD_power)
	analogdata2.AddStep(walls2_volt, walls_turnon_end_time, walls_turnoff_start_time, hor_DMD_power)
	analogdata2.AddSmoothRamp(walls2_volt, line_DMD_start_volt, walls_turnoff_start_time, walls_turnoff_end_time, hor_DMD_power)
End If

' Counting
If (full_counting = 1) Then
	digitaldata2.AddPulse(line_DMD_ttl, berlin_wall_rampup_start_time - PID_response_dur/2, berlin_wall_rampdown_end_time + PID_response_dur/2)
	analogdata2.AddSmoothRamp(line_DMD_start_volt, Berlin_wall_expansion_volt, berlin_wall_rampup_start_time, berlin_wall_rampup_end_time, line_DMD_power)
	analogdata2.AddStep(Berlin_wall_expansion_volt, berlin_wall_rampup_end_time, berlin_wall_rampdown_start_time, line_DMD_power)
	analogdata2.AddSmoothRamp(Berlin_wall_expansion_volt, line_DMD_start_volt, berlin_wall_rampdown_start_time, berlin_wall_rampdown_end_time, line_DMD_power)
End If

'-------------------------------------------------------------------------------------------------------------------------------------------------- Scope trigger

Dim scope_trigger As Double = twodphysics_end_time
digitaldata.AddPulse(64, scope_trigger, scope_trigger + 10)