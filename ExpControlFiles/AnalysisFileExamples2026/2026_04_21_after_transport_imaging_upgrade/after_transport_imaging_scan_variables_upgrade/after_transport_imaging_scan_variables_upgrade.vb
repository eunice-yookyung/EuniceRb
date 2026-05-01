'===================
'=====Variables=====
compress = 1
new_shutter_offset_time = 1.7
new_compressed_mot_time = 6
new_molasses_time = 6
new_mot_detuning_molasses_time = 25
new_rp_shutter_offset_time = 8
new_optical_pumping_time = 0.2
if_blow_away_F2 = 1
new_hold_time = 100
new_TOF = 10
'=====Variables=====

Dim final_time_delay As Double = 5000
TOF = new_TOF
hold_time = new_hold_time

shutter_offset_time = new_shutter_offset_time
molasses_time = new_molasses_time
compressed_mot_time = new_compressed_mot_time
optical_pumping_time = new_optical_pumping_time

Dim mot_detuning_molasses_time  = new_mot_detuning_molasses_time 
Dim rp_shutter_offset_time = new_rp_shutter_offset_time

'***************************** MOT Creation ************************************************

digitaldata2.AddPulse(clock_resynch,1,4)
analogdata.DisableClkDist(0.95, 4.05)
analogdata2.DisableClkDist(0.95, 4.05)

'Dim MOT_end_time As Double
'MOT_end_time = Me.AddMOTSequenceUpgrade(0, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
'Dim transport_start_time As Double = MOT_end_time
'Dim MOT_end_time As Double

'CHANGE BACK HERE
'MOT_end_time = Me.AddMOTSequenceUpgrade(0, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim t_start As Double = 0
Dim transport_start_time As Double = t_start + mot_load_time + 0.5 + MHT
Dim t_stop As Double = t_start + mot_load_time + 0.5 + MHT
    'Dim blow_away_start_time As Double = 0.8

'=======INVERT SIGNALS FOR UPGRADE=====
digitaldata.AddPulse(mot_low_current, t_start + mot_load_time - molasses_time, transport_start_time) 'MOT Current Supply
digitaldata.AddPulse(ta_shutter, t_start + mot_load_time - shutter_offset_time, transport_start_time) 'TA Shutter. starts to close early, so the shutter is closed ASAP after the blow away pulse.
digitaldata.AddPulse(repump_shutter, t_start + mot_load_time - rp_shutter_offset_time , transport_start_time) 'Repump Shutter starts to close early
'======================================
'Load MOT and then do molasses
digitaldata2.AddPulse(mot_detuning, _
	t_start + mot_load_time - molasses_time - compressed_mot_time, _
	t_start + mot_load_time - molasses_time - compressed_mot_time + mot_detuning_molasses_time) 'MOT Detuning molasses 2026/04/29 define mot_detuning_molasses_time as a free variable
     
'Optical Pumping
digitaldata.AddPulse(ttl_80MHz, t_start + mot_load_time, t_stop) '-80MHz TTL(Switches light to 0th order AOM)
digitaldata.AddPulse(ttl_97MHz, t_start + mot_load_time, t_start + mot_load_time + 30) '97.5MHz TTL (Switches light to 0th order AOM)
digitaldata.AddPulse(ttl_N133MHz, t_start + mot_load_time, t_start + mot_load_time + optical_pumping_time) ' -133MHz TTL(2->2 AOM)
digitaldata.AddPulse(ttl_N78MHz, t_start + mot_load_time, t_start + mot_load_time + optical_pumping_time - 0.02) ' -78MHz TTL (1->1 AOM)
digitaldata.AddPulse(ttl_78MHz, t_start + mot_load_time, t_start + mot_load_time + 60) '78MHz TTL to keep repump off until shutter closes
digitaldata.AddPulse(optical_pumping, t_start + mot_load_time - 0.5, t_start + mot_load_time + 0.4) 'Optical Pumping B Field
digitaldata.AddPulse(polarizer_shutter_11, t_start + mot_load_time - 60, t_start + mot_load_time + 30) 'Optical Pumping Shutter
     

'Magnetic Trapping
digitaldata.AddPulse(ps1_shunt, t_start, t_start + mot_load_time - 100) 'PS1 Shunt
'digitaldata.AddPulse(ps2_shunt, t_start, t_start + mot_load_time) 'PS2 Shunt
'digitaldata.AddPulse(ps3_shunt, t_start, t_start + mot_load_time) 'PS3 Shunt
'digitaldata.AddPulse(ps4_shunt, t_start, t_start + mot_load_time) 'PS4 Shunt

digitaldata.AddPulse(ps2_shunt, t_start, t_stop) 'PS2 shunt
digitaldata.AddPulse(ps3_shunt, t_start, t_stop) 'PS3 shunt
digitaldata.AddPulse(ps4_shunt, t_start, t_stop) 'PS4 shunt

'modify 0.2 to optical_pumping_time if scanning variable
digitaldata.AddPulse(mot_high_current, t_start + mot_load_time + optical_pumping_time, t_stop) 'MOT High Current
digitaldata2.AddPulse(cap_discharge, t_start + mot_load_time + optical_pumping_time, t_stop) 'Capacitor Discharge
'analogdata.AddStep(0.043, t_start + mot_load_time - 100, t_start + mot_load_time + 0.2, ps1_ao) 'Test on 2022/09/13, Windiup approach?
analogdata.AddExpAndRamp(2, 2, t_start + mot_load_time + optical_pumping_time, t_stop, 20, -30, 0, 1.33, ps1_ao)
'analogdata.AddExpAndRamp(2, 2, t_start + mot_load_time + 0.2, transport_start_time, 20, -30, 0, 1.31543, ps1_ao) 'Test on 2022/09/13

'Blow away atoms in f=2 if any
digitaldata.AddPulse(ttl_N133MHz, mot_load_time+blow_away_start_time, mot_load_time+blow_away_start_time+10) ' -133MHz TTL(2->2 AOM)

'If if_blow_away_F2 = 1 Then 
'	'Blow away atoms in f=2 if any
'	digitaldata.AddPulse(ttl_N133MHz, mot_load_time+blow_away_start_time, mot_load_time+blow_away_start_time+10) ' -133MHz TTL(2->2 AOM)
'End If
	


'***************************** Transport **************************************************

Dim transport_end_time As Double
transport_end_time = Me.AddTransportSequence(transport_start_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)


'***************************** Evaporation ************************************************

Dim evaporation_end_time As Double

If compress = 1 Then

    'If compress: ramp up Quad and T13 to their final value for evaporation sequence
    analogdata.AddSmoothRamp(1.415, 0.8 * 2.342, _
        transport_end_time- 0.4, transport_end_time + quad_rampup_time, ps1_ao) 'Ramps quadrupole from final transport value to quic value(33.96->56.208)
    analogdata.AddStep(0.8 * 2.342, transport_end_time + quad_rampup_time, transport_end_time + quad_rampup_time + hold_time, ps1_ao)

    analogdata.AddSmoothRamp(0.2895, 0.585, _
        transport_end_time- 0.4, transport_end_time + quad_rampup_time, ps3_ao) 'Ramps final transport(T13) to higher value to keep pace with quad rampup (11.58->23.4)
    analogdata.AddStep(0.585, transport_end_time + quad_rampup_time, transport_end_time + quad_rampup_time + hold_time, ps3_ao)

    digitaldata.AddPulse(transport_13, transport_end_time, transport_end_time + quad_rampup_time + hold_time) 'Keeps T13 on through remainder of experiment
    digitaldata.AddPulse(quad_fet, transport_end_time, transport_end_time + quad_rampup_time + hold_time) 'Quadrupole mosfet

    evaporation_end_time = transport_end_time + quad_rampup_time + hold_time

Else

    'If not compress: keep Quad and T13 at their final transport value
    analogdata.AddStep(1.415, transport_end_time, transport_end_time + hold_time, ps1_ao)   
    analogdata.AddStep(0.2895, transport_end_time, transport_end_time + hold_time, ps3_ao)

    digitaldata.AddPulse(transport_13, transport_end_time, transport_end_time + hold_time) 'Keeps T13 on through remainder of experiment
    digitaldata.AddPulse(quad_fet, transport_end_time, transport_end_time + hold_time) 'Quadrupole mosfet
    
    evaporation_end_time = transport_end_time + hold_time

End If


'************************************************************ Definition of time variables ***************************************************************

Dim releaseTime As Double = evaporation_end_time
Dim IT As Double = releaseTime + TOF


'*********************************************************** Begin Absorption Imaging ********************************************************************

Dim last_time As Double
Dim ghetto_fix_start_time As Double = Me.AddAbsorptionImagingSequenceUpgrade(IT, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)

Dim ghetto_fix_end_time As Double = ghetto_fix_start_time + final_time_delay
digitaldata2.AddPulse(lattice2D765_shutter, ghetto_fix_start_time, ghetto_fix_end_time)
last_time = ghetto_fix_end_time

Dim scope_trigger As Double = transport_end_time - 200
digitaldata.AddPulse(64, scope_trigger, scope_trigger+10)


'=======INVERT SIGNALS FOR UPGRADE=====
digitaldata.AddPulse(mot_low_current, transport_start_time, last_time) 'MOT Current Supply
digitaldata.AddPulse(ta_shutter,  transport_start_time-shutter_offset_time, last_time) 'TA Shutter. starts to close early, so the shutter is closed ASAP after the blow away pulse.
digitaldata.AddPulse(repump_shutter, transport_start_time - rp_shutter_offset_time, IT-35) 'Repump Shutter
digitaldata.AddPulse(repump_shutter, IT- rp_shutter_offset_time, last_time) 'Repump Shutter
'======================================
