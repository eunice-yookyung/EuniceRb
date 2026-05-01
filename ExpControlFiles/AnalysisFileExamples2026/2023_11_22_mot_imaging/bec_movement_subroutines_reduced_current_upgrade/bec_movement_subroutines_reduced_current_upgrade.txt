'===================
'=====Variables=====
free_var = 30
new_shutter_offset_time = 2.5
evap_time = 25000
new_TOF = 10
'=====Variables=====
Dim final_time_delay As Double = 5000
'===================
TOF = new_TOF
shutter_offset_time = new_shutter_offset_time

'***************************** MOT Creation ************************************************
digitaldata2.AddPulse(clock_resynch,1,4)
analogdata.DisableClkDist(0.95, 4.05)
analogdata2.DisableClkDist(0.95, 4.05)

Dim MOT_end_time As Double
'CHANGE BACK HERE
MOT_end_time = Me.AddMOTSequenceUpgrade(0, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim transport_start_time As Double = MOT_end_time

'***************************** Transport **************************************************

Dim transport_end_time As Double
transport_end_time = Me.AddTransportSequence(transport_start_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)

'***************************** Evaporation ************************************************
Dim evaporation_end_time As Double
'Dim evaporation_end_time As Double = transport_end_time
evaporation_end_time = Me.AddEvaporationSequenceWithEvapTimeAsVariable(transport_end_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
'evaporation_end_time = Me.AddEvaporationSequence(transport_end_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)

'************************************************************ Definition of time variables ***************************************************************

Dim vertical_move_end_time As Double = evaporation_end_time+cigar_move_time
Dim IT As Double = evaporation_end_time+cigar_move_time+hold_time+TOF
'Dim IT As Double = evaporation_end_time
Dim releaseTime As Double = IT - TOF

'**************************** Imaging ******************************************************
Dim last_time As Double
'last_time = final_time_delay + Me.AddAbsorptionImagingSequence(IT, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim ghetto_fix_start_time As Double = Me.AddAbsorptionImagingSequenceUpgrade(IT, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
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
digitaldata.AddPulse(mot_low_current, transport_start_time, last_time) 'MOT Current Supply
digitaldata.AddPulse(ta_shutter, transport_start_time, last_time) 'TA Shutter. starts to close early, so the shutter is closed ASAP after the blow away pulse.
digitaldata.AddPulse(repump_shutter, transport_start_time, IT - 35) 'Repump Shutter
digitaldata.AddPulse(repump_shutter, IT, last_time) 'Repump Shutter
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
