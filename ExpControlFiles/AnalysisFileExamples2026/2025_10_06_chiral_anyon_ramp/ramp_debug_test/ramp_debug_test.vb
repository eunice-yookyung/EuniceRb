
'===================
'=====Variables=====
lattice1_max = 45
lattice2_max = 45
pinning_hold_time = 1000
red_freq = 18.0
round_dimple_voltage = 4.02
anticonfine_dur_vert = 0
anticonfine_dur_horz = 0
loading_walls_volt = 0
loading_grad_ramp_dur = 0
loading_grad_init = 0
loading_grad_final = 0
loading_grad_comp = 0
loading_lattice_ramp_dur = 0
loading_lattice2_low_depth = 45
walls2_volt = 0
grad_ramp_dur = 0
quic_grad_init = 0
quic_grad_final = 0
lattice_ramp_dur = 0
lattice2_low_depth = 45
lattice_mod_ramp_dur = 0
latticemod_amp_1 = 0
latticemod_amp_2 = 0
latticemod_amp_3 = 0
mod_freq_Hz_1 = 608
mod_freq_Hz_2 = 795
mod_freq_Hz_3 = 992
phase_ramp_dur = 0
phase_pi_init = 0
phase_pi_final = 0
hold_dur = 0
is_return = 0
is_full_return = 0
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
Dim splitting_lattice_dur As Double = 10
Dim splitting_turnon_dur As Double = 10
Dim splitting_ramp_dur As Double = 200
Dim lattice2_doublon_depth As Double = 15
Dim splitting_quic_start As Double = 5.5
Dim splitting_quic_end As Double = 3


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

Dim twodphysics_end_time As Double = go_lower_end_time + stop_time

'() freeze lattices, turn off gradients
Dim lattice_freeze_start_time As Double = twodphysics_end_time
Dim lattice_freeze_end_time As Double = lattice_freeze_start_time + lattice_freeze_dur

'() Pinning, image
Dim pinning_start_time As Double = twodphysics_end_time 


'------------------------------------------------------------------------- Pinning -------------------------------------------------------------------------

Dim pinning_times As Double() = Me.AddPinningSequence(pinning_start_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim pinning_end_time As Double = pinning_times(0)
Dim pinning_ready_time As Double = pinning_times(1)
Dim molasses_start_time As Double = pinning_times(2)

Dim IT As Double = pinning_end_time + TOF
Dim last_time As Double = IT


'------------------------------------------------------------ Lattice Depth Conversion to Volts ------------------------------------------------------------

Dim loading_lattice2_low_volt As Double = lattice2_voltage_offset + lattice2_calib_volt + .5*Log10(loading_lattice2_low_depth/lattice2_calib_depth)
Dim lattice2_low_volt As Double = lattice2_voltage_offset + lattice2_calib_volt + .5*Log10(lattice2_low_depth/lattice2_calib_depth)
Dim lattice2_doublon_volt As Double = lattice2_voltage_offset + lattice2_calib_volt + 0.5 * Log10(lattice2_doublon_depth / lattice2_calib_depth)

'--------------------------------------------------------------------- Invert signals ---------------------------------------------------------------------

digitaldata.AddPulse(mot_low_current, transport_start_time, last_time) 'MOT Current Supply
digitaldata.AddPulse(ta_shutter, transport_start_time, last_time) 'TA Shutter. starts to close early, so the shutter is closed ASAP after the blow away pulse.
digitaldata.AddPulse(repump_shutter, transport_start_time, last_time) 'Repump Shutter
digitaldata2.AddPulse(ixon_flip_mount_ttl, tracking_end_time, last_time)

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
analogdata.AddStep(lattice1_max_volt, line_load_end_time, lattice_freeze_start_time, lattice2D765_power)

' Hold 2D2
analogdata.AddStep(lattice2_max_volt, line_load_end_time, lattice_freeze_start_time, lattice2D765_power2) '2D2

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


'-------------------------------------------------------------------------- DMD Code ------------------------------------------------------------------------

'this line to bring trigger active high, but also serve as the tracking line trigger (on its falling edge, with 6ms delay)
digitaldata2.AddPulse(line_DMD_trigger, evaporation_end_time, molasses_start_time + fluo_image_wait ) 'tracking line
digitaldata2.AddPulse(hor_DMD_trigger, evaporation_end_time, molasses_start_time + fluo_image_wait )'tracking hor

'Now all these are inverted and triggers on the second time stamp (real rising edge)
'pattern switching after a 160us delay, switching itself takes few us or less
digitaldata2.AddPulse(line_DMD_trigger, DMD_hw_delay + line_load_vert_start_time - 0.5, DMD_hw_delay + line_load_vert_start_time) 'cut first direction (line DMD)
digitaldata2.AddPulse(hor_DMD_trigger, DMD_hw_delay + line_load_horz_start_time - 0.5, DMD_hw_delay + line_load_horz_start_time) 'cut second direction (hor DMD)
'Shutter
digitaldata2.AddPulse(line_DMD_shutter, cutting1_turnon_start_time - 20, molasses_start_time)
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


'-------------------------------------------------------------------------------------------------------------------------------------------------- Scope trigger

Dim scope_trigger As Double = twodphysics_start_time
digitaldata.AddPulse(64, scope_trigger, scope_trigger + 10)