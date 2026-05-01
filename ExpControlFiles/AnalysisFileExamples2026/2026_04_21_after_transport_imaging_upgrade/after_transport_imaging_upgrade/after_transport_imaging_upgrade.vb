'===================
'=====Variables=====
compress = 1
new_hold_time = 100
new_TOF = 10
'=====Variables=====

Dim final_time_delay As Double = 5000
TOF = new_TOF
hold_time = new_hold_time

'***************************** MOT Creation ************************************************

digitaldata2.AddPulse(clock_resynch,1,4)
analogdata.DisableClkDist(0.95, 4.05)
analogdata2.DisableClkDist(0.95, 4.05)

Dim MOT_end_time As Double
MOT_end_time = Me.AddMOTSequenceUpgrade(0, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)
Dim transport_start_time As Double = MOT_end_time


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

Dim scope_trigger As Double = MOT_end_time - 200
digitaldata.AddPulse(64, scope_trigger, scope_trigger+10)


'=======INVERT SIGNALS FOR UPGRADE=====
digitaldata.AddPulse(mot_low_current, transport_start_time, last_time) 'MOT Current Supply
digitaldata.AddPulse(ta_shutter, transport_start_time, last_time) 'TA Shutter. starts to close early, so the shutter is closed ASAP after the blow away pulse.
digitaldata.AddPulse(repump_shutter, transport_start_time, IT - 35) 'Repump Shutter
digitaldata.AddPulse(repump_shutter, IT, last_time) 'Repump Shutter
'======================================
