'===========================================================
' BEC movement subroutines - reduced current upgrade
'
' Purpose:
'   Build a full experiment timing sequence for:
'     1. MOT loading
'     2. Transport
'     3. Evaporation
'     4. Final magnetic-field move
'     5. Release and time-of-flight
'     6. Absorption imaging
'
' Notes:
'   - This version uses upgraded MOT and imaging subroutines.
'   - A key modification is the reduced ps2_ao current:
'         2.342 * 0.8
'===========================================================


'===================
' User / scan variables
'===================
free_var = 30
new_shutter_offset_time = 2.5
evap_time = 25000
new_TOF = 10

Dim final_time_delay As Double = 5000

' Apply chosen settings to global timing variables
TOF = new_TOF
shutter_offset_time = new_shutter_offset_time


'===========================================================
' 1) MOT creation
'===========================================================

' Resynchronize timing clock
digitaldata2.AddPulse(clock_resynch, 1, 4)

' Temporarily disable clock distribution on analog boards
analogdata.DisableClkDist(0.95, 4.05)
analogdata2.DisableClkDist(0.95, 4.05)

' Run upgraded MOT sequence starting at t = 0
' Returns the end time of the MOT stage
Dim MOT_end_time As Double
MOT_end_time = Me.AddMOTSequenceUpgrade( _
    0, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)

' Transport begins immediately after MOT sequence ends
Dim transport_start_time As Double = MOT_end_time


'===========================================================
' 2) Transport
'===========================================================

' Move atoms from MOT region toward science/trap region
' Returns the end time of transport
Dim transport_end_time As Double
transport_end_time = Me.AddTransportSequence( _
    transport_start_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)


'===========================================================
' 3) Evaporation
'===========================================================

' Perform evaporation with evap_time as a variable parameter
' Returns the end time of evaporation
Dim evaporation_end_time As Double
evaporation_end_time = Me.AddEvaporationSequenceWithEvapTimeAsVariable( _
    transport_end_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)

' Alternative older version:
' evaporation_end_time = Me.AddEvaporationSequence( _
'     transport_end_time, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)


'===========================================================
' 4) Derived timing definitions
'===========================================================

' End of final cigar/vertical move after evaporation
Dim vertical_move_end_time As Double = evaporation_end_time + cigar_move_time

' Imaging time:
'   evaporation ends
'   -> move atoms
'   -> hold
'   -> release
'   -> expand for TOF
'   -> image at IT
Dim IT As Double = evaporation_end_time + cigar_move_time + hold_time + TOF

' Release occurs TOF before imaging
Dim releaseTime As Double = IT - TOF


'===========================================================
' 5) Absorption imaging sequence
'===========================================================

Dim last_time As Double

' Older imaging call:
' last_time = final_time_delay + Me.AddAbsorptionImagingSequence( _
'     IT, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)

' Upgraded imaging call
Dim ghetto_fix_start_time As Double = Me.AddAbsorptionImagingSequenceUpgrade( _
    IT, cp, analogdata, analogdata2, digitaldata, digitaldata2, gpib, Hermes, dds, True)

' Hold final sequence open for some extra delay after imaging
Dim ghetto_fix_end_time As Double = ghetto_fix_start_time + final_time_delay

' Define experiment end time
last_time = ghetto_fix_end_time


'===========================================================
' Optional lattice alignment block
'===========================================================

'*** UNCOMMENT TO ALIGN BIG LATTICE ****
' ADDED 2024/07/23
' digitaldata2.AddPulse(big_lattice_shutter, IT - 50, last_time)
' digitaldata2.AddPulse(big_lattice_ttl, IT - shutter_time, IT + imaging_time) ' camera atoms image
'***************************************


'===========================================================
' 6) Digital outputs: shutters / inverted upgrade logic
'===========================================================

' Inverted/updated logic for hardware upgrade

' Keep MOT current supply active during post-MOT sequence
digitaldata.AddPulse(mot_low_current, transport_start_time, last_time)

' Close TA shutter early and keep it closed
digitaldata.AddPulse(ta_shutter, transport_start_time, last_time)

' Repump shutter control:
'   open during most of sequence before imaging,
'   then again after imaging
digitaldata.AddPulse(repump_shutter, transport_start_time, IT - 35)
digitaldata.AddPulse(repump_shutter, IT, last_time)


'===========================================================
' 7) Digital outputs: magnetic hardware management
'===========================================================

' These outputs maintain the QUIC/final trap configuration
' from the end of evaporation until atom release

digitaldata.AddPulse(offset_fet, evaporation_end_time, releaseTime)          ' Offset FET
digitaldata.AddPulse(bias_enable, evaporation_end_time, releaseTime)         ' Enable bias coils
digitaldata.AddPulse(transport_13, evaporation_end_time, releaseTime)        ' Keep T13 on
'digitaldata.AddPulse(transport_13_toggle, evaporation_end_time, releaseTime)
digitaldata.AddPulse(ps5_enable, evaporation_end_time, releaseTime)
digitaldata.AddPulse(ps5_shunt, releaseTime, releaseTime + 100)
digitaldata2.AddPulse(ps7_shunt, releaseTime, releaseTime + 100)
digitaldata2.AddPulse(ioffe_mirror_fet, evaporation_end_time, releaseTime)
digitaldata.AddPulse(quad_shim, evaporation_end_time, releaseTime)
digitaldata.AddPulse(ps4_shunt, evaporation_end_time, releaseTime + 200)
digitaldata.AddPulse(imaging_coil, evaporation_end_time, releaseTime)


'===========================================================
' 8) Analog outputs: coil currents during move/hold
'===========================================================

' Hold cigar trap on during evaporation->release interval
' Old value:
' analogdata.AddStep(2.342, evaporation_end_time, releaseTime, ps2_ao)

' Reduced-current upgrade version
analogdata.AddStep(2.342 * 0.8, evaporation_end_time, releaseTime, ps2_ao)

' Ramp fields during final magnetic move, then hold until release

' ps5 supply: push field
analogdata2.AddSmoothRamp( _
    0, quic_push * ps5_scaler, _
    evaporation_end_time, evaporation_end_time + cigar_move_time, ps5_ao)
analogdata2.AddStep( _
    quic_push * ps5_scaler, _
    evaporation_end_time + cigar_move_time, releaseTime, ps5_ao)

' ps6 supply: mirror-related contribution
analogdata.AddSmoothRamp( _
    ps6_scaler * 0, _
    ps6_scaler * quic_push * quic_mirror_ratio / 16.0 + 0.02, _
    evaporation_end_time, evaporation_end_time + cigar_move_time, ps6_ao)
analogdata.AddStep( _
    ps6_scaler * quic_push * quic_mirror_ratio / 16.0 + 0.02, _
    evaporation_end_time + cigar_move_time, releaseTime, ps6_ao)

' ps1 supply: move quad axis position
analogdata.AddSmoothRamp( _
    0.04, quad_axis_position, _
    evaporation_end_time, evaporation_end_time + cigar_move_time, ps1_ao)
analogdata.AddStep( _
    quad_axis_position, _
    evaporation_end_time + cigar_move_time, releaseTime, ps1_ao)

' ps3 supply: ramp down during move
analogdata.AddSmoothRamp( _
    0.875, 0.03, _
    evaporation_end_time, evaporation_end_time + cigar_move_time, ps3_ao)
analogdata.AddStep( _
    0.03, _
    evaporation_end_time + cigar_move_time, releaseTime, ps3_ao)

' ps7 supply: black coil z contribution
analogdata.AddSmoothRamp( _
    0, black_coil_zed * 0.5, _
    evaporation_end_time, evaporation_end_time + cigar_move_time, ps7_ao)
analogdata.AddStep( _
    black_coil_zed * 0.5, _
    evaporation_end_time + cigar_move_time, releaseTime, ps7_ao)


'===========================================================
' 9) Imaging-related digital outputs
'===========================================================

' Keep 765 lattice shutter active during imaging/final delay window
digitaldata2.AddPulse(lattice2D765_shutter, ghetto_fix_start_time, ghetto_fix_end_time)


'===========================================================
' 10) Scope trigger
'===========================================================

' Candidate triggers kept for reference:
' Dim scope_trigger As Double = MOT_end_time - 200
' Dim scope_trigger As Double = IT
' Dim scope_trigger As Double = MOT_end_time
' Dim scope_trigger As Double = mot_load_time
Dim scope_trigger As Double = evaporation_end_time
' Dim scope_trigger As Double = 0 + mot_load_time - molasses_time - compressed_mot_time + 12
' Dim scope_trigger As Double = MOT_end_time - 0.5 - MHT - shutter_offset_time
' Dim scope_trigger As Double = IT - shutter_time
' Dim scope_trigger As Double = evaporation_end_time + cigar_move_time
' Dim scope_trigger As Double = last_time - 2000

' Send 10-ms pulse on digital channel 64 for oscilloscope triggering
digitaldata.AddPulse(64, scope_trigger, scope_trigger + 10)