'===================
'=====Variables=====
free_var = 30
new_shutter_offset_time = 1.6
evap_time = 24400
new_compressed_mot_time = 6
new_molasses_time = 6
new_mot_detuning_molasses_time = 25
new_rp_shutter_offset_time = 8
new_optical_pumping_time = 0.2
new_two2_pumping_time = 0.05
if_blow_away_F2 = 1
new_TOF = 10
'=====Variables=====
Dim final_time_delay As Double = 5000
'===================
TOF = new_TOF
shutter_offset_time = new_shutter_offset_time
molasses_time = new_molasses_time
compressed_mot_time = new_compressed_mot_time
optical_pumping_time = new_optical_pumping_time

Dim two2_pumping_time As Double = new_two2_pumping_time
Dim mot_detuning_molasses_time As Double = new_mot_detuning_molasses_time 
Dim rp_shutter_offset_time  As Double = new_rp_shutter_offset_time
'***************************** MOT Creation ************************************************
digitaldata2.AddPulse(clock_resynch,1,4)
analogdata.DisableClkDist(0.95, 4.05)
analogdata2.DisableClkDist(0.95, 4.05)

Dim t_start As Double = 0
'CHANGE BACK HERE
'MOT_end_time = Me.AddMOTSequenceUpgrade(0, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)

Dim transport_start_time As Double = t_start + mot_load_time + 0.5 + MHT
Dim t_stop As Double = t_start + mot_load_time + 0.5 + MHT
    'Dim blow_away_start_time As Double = 0.8


'=======INVERT SIGNALS FOR UPGRADE=====
digitaldata.AddPulse(mot_low_current, t_start + mot_load_time - molasses_time, transport_start_time) 'MOT Current Supply
digitaldata.AddPulse(ta_shutter, t_start + mot_load_time - shutter_offset_time, transport_start_time) 'TA Shutter. starts to close early, so the shutter is closed ASAP after the blow away pulse.
digitaldata.AddPulse(repump_shutter, t_start + mot_load_time - rp_shutter_offset_time , transport_start_time) 'Repump Shutter starts to close early

'Load MOT and then do compression and molasses
digitaldata2.AddPulse(mot_detuning, _
	t_start + mot_load_time - molasses_time - compressed_mot_time, _
	t_start + mot_load_time - molasses_time - compressed_mot_time + mot_detuning_molasses_time) 'MOT Detuning molasses 2026/04/29 define mot_detuning_molasses_time as a free variable


'Optical Pumping
digitaldata.AddPulse(ttl_80MHz, t_start + mot_load_time, transport_start_time) '-80MHz TTL(Switches light to 0th order AOM)
digitaldata.AddPulse(ttl_97MHz, t_start + mot_load_time, t_start + mot_load_time + 30) '97.5MHz TTL (Switches light to 0th order AOM)
digitaldata.AddPulse(ttl_78MHz, t_start + mot_load_time, t_start + mot_load_time + 60) '78MHz TTL to keep repump off until shutter closes
'2026/05/01 not doing anything, cuz ttl is always on

'Optical pumping 1-1, 2-2
digitaldata.AddPulse(ttl_N133MHz, t_start + mot_load_time, t_start + mot_load_time + two2_pumping_time) ' -133MHz TTL(2->2 AOM)
digitaldata.AddPulse(ttl_N78MHz, t_start + mot_load_time, t_start + mot_load_time + optical_pumping_time - 0.02) ' -78MHz TTL (1->1 AOM)
digitaldata.AddPulse(optical_pumping, t_start + mot_load_time - 0.5, t_start + optical_pumping_time + 0.2) 'Optical Pumping B Field
digitaldata.AddPulse(polarizer_shutter_11, t_start + mot_load_time - 60, t_start + mot_load_time + 30) 'Optical Pumping Shutter


'Magnetic Trapping
digitaldata.AddPulse(ps1_shunt, t_start, t_start + mot_load_time - 100) 'PS1 Shunt
digitaldata.AddPulse(ps2_shunt, t_start, t_stop) 'PS2 shunt
digitaldata.AddPulse(ps3_shunt, t_start, t_stop) 'PS3 shunt
digitaldata.AddPulse(ps4_shunt, t_start, t_stop) 'PS4 shunt

'modify 0.2 to optical_pumping_time if scanning variable
digitaldata.AddPulse(mot_high_current, t_start + mot_load_time + optical_pumping_time, transport_start_time) 'MOT High Current
digitaldata2.AddPulse(cap_discharge, t_start + mot_load_time + optical_pumping_time, transport_start_time) 'Capacitor Discharge
'analogdata.AddStep(0.043, t_start + mot_load_time - 100, t_start + mot_load_time + 0.2, ps1_ao) 'Test on 2022/09/13, Windiup approach?
analogdata.AddExpAndRamp(2, 2, t_start + mot_load_time + optical_pumping_time, transport_start_time, 20, -30, 0, 1.33, ps1_ao)
'analogdata.AddExpAndRamp(2, 2, t_start + mot_load_time + 0.2, transport_start_time, 20, -30, 0, 1.31543, ps1_ao) 'Test on 2022/09/13

'If if_blow_away_F2 = 1 Then 
	'Blow away atoms in f=2 if any
		'digitaldata.AddPulse(ttl_N133MHz, mot_load_time+blow_away_start_time, mot_load_time+blow_away_start_time+10) ' -133MHz TTL(2->2 AOM)
'End If

digitaldata.AddPulse(ttl_N133MHz, mot_load_time+blow_away_start_time, mot_load_time+blow_away_start_time+10) ' -133MHz TTL(2->2 AOM)


'***************************** Transport **************************************************

Dim transport_end_time As Double
transport_end_time = Me.AddTransportSequence(transport_start_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)

'***************************** Evaporation ************************************************
Dim evaporation_end_time As Double
evaporation_end_time = Me.AddEvaporationSequenceWithEvapTimeAsVariable(transport_end_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)

'************************************************************ Definition of time variables ***************************************************************

Dim vertical_move_end_time As Double = evaporation_end_time+cigar_move_time
Dim IT As Double = evaporation_end_time+cigar_move_time+hold_time+TOF
'Dim IT As Double = evaporation_end_time
Dim releaseTime As Double = IT - TOF

'**************************** Imaging ******************************************************
Dim last_time As Double
'last_time = final_time_delay + Me.AddAbsorptionImagingSequence(IT, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim ghetto_fix_start_time As Double = Me.AddAbsorptionImagingSequenceUpgrade(IT, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
'Dim ghetto_fix_start_time As Double = Me.AddAbsorptionImagingSequenceSelect12Repump(IT, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True,if_repump12_imaging)

Dim ghetto_fix_end_time As Double = ghetto_fix_start_time + final_time_delay
last_time = ghetto_fix_end_time

'*** UNCOMMENT TO ALIGN BIG LATTICE ****
'ADDED 2024/07/23
'digitaldata2.AddPulse(big_lattice_shutter, IT - 50, last_time)
'digitaldata2.AddPulse(big_lattice_ttl, IT - shutter_time, IT + imaging_time) 'camera atoms image
'***************************************

'************************************************************ End of Time Variable Definitions **********************************************************
'************************************************************ Coils **************************************************************************

'=======INVERT SIGNALS FOR UPGRADE=====
digitaldata.AddPulse(mot_low_current, t_start + mot_load_time - molasses_time, ghetto_fix_end_time) 'MOT Current Supply
digitaldata.AddPulse(ta_shutter, t_start + mot_load_time - shutter_offset_time, ghetto_fix_end_time) 'TA Shutter. starts to close early, so the shutter is closed ASAP after the blow away pulse.
digitaldata.AddPulse(repump_shutter, t_start + mot_load_time - rp_shutter_offset_time, IT-35) 'Repump Shutter
digitaldata.AddPulse(repump_shutter, IT- rp_shutter_offset_time, last_time) 'Repump Shutter
'======================================

'MOSFET Management for all QUIC trap coils and coils that control final position of atoms.
digitaldata.AddPulse(offset_fet,evaporation_end_time, releaseTime)'Offset FET
digitaldata.AddPulse(bias_enable,evaporation_end_time,releaseTime) 'Enables bias coils
digitaldata.AddPulse(transport_13,evaporation_end_time,releaseTime)'Keeps T13 on through remainder of experiment
'digitaldata.AddPulse(transport_13_toggle,evaporation_end_time,releaseTime)
digitaldata.AddPulse(ps5_enable,evaporation_end_time,releaseTime)
digitaldata.AddPulse(ps5_shunt,releaseTime,releaseTime+100)
digitaldata2.AddPulse(ps7_shunt,releaseTime,releaseTime+100)
digitaldata2.AddPulse(ioffe_mirror_fet,evaporation_end_time,releaseTime)
digitaldata.AddPulse(quad_shim,evaporation_end_time,releaseTime)
digitaldata.AddPulse(ps4_shunt, evaporation_end_time,releaseTime+200)
digitaldata.AddPulse(imaging_coil,evaporation_end_time,releaseTime)

'CHANGE BACK HERE
'analogdata.AddStep(2.342,evaporation_end_time,releaseTime, ps2_ao)'Holds cigar trap on during evaporation
analogdata.AddStep(2.342*.8,evaporation_end_time,releaseTime, ps2_ao)'Holds cigar trap on during evaporation

analogdata2.AddSmoothRamp(0,quic_push*ps5_scaler,evaporation_end_time,evaporation_end_time+cigar_move_time,ps5_ao)
analogdata2.AddStep(quic_push*ps5_scaler,evaporation_end_time+cigar_move_time,releaseTime,ps5_ao)
analogdata.AddSmoothRamp(ps6_scaler*0,ps6_scaler*quic_push*quic_mirror_ratio/16.0+.02,evaporation_end_time,evaporation_end_time+cigar_move_time,ps6_ao)
analogdata.AddStep(ps6_scaler*quic_push*quic_mirror_ratio/16.0+.02,evaporation_end_time+cigar_move_time,releaseTime,ps6_ao)
analogdata.AddSmoothRamp(0.04,quad_axis_position, evaporation_end_time, evaporation_end_time+cigar_move_time, ps1_ao)
analogdata.AddStep(quad_axis_position,evaporation_end_time+cigar_move_time,releaseTime,ps1_ao)
analogdata.AddSmoothRamp(.875,0.03,evaporation_end_time,evaporation_end_time+cigar_move_time,ps3_ao)
analogdata.AddStep(0.03,evaporation_end_time+cigar_move_time,releaseTime,ps3_ao)
analogdata.AddSmoothRamp(0,black_coil_zed*0.5,evaporation_end_time,evaporation_end_time+cigar_move_time,ps7_ao)
analogdata.AddStep(black_coil_zed*0.5,evaporation_end_time+cigar_move_time,releaseTime,ps7_ao)

'*********************************************************** Begin Absorption Imaging ********************************************************************

digitaldata2.AddPulse(lattice2D765_shutter, ghetto_fix_start_time, ghetto_fix_end_time)

'Scope trigger
'Dim scope_trigger As Double = MOT_end_time - 200 'Until 2023/10/02 - 3:46 PM
'Dim scope_trigger As Double =  IT
'Dim scope_trigger As Double = MOT_end_time
'Dim scope_trigger As Double = mot_load_time
Dim scope_trigger As Double = evaporation_end_time
'Dim scope_trigger As Double = 0 + mot_load_time - molasses_time -compressed_mot_time+12 
'Dim scope_trigger As Double = MOT_end_time - 0.5 - MHT - shutter_offset_time
'Dim scope_trigger As Double =  IT - shutter_time

'Dim scope_trigger As Double =  evaporation_end_time+cigar_move_time
'Dim scope_trigger As Double =  last_time - 2000
digitaldata.AddPulse(64, scope_trigger, scope_trigger+10)
