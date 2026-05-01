Option Strict Off
Option Explicit On

'Imports System.Windows.Forms
Imports System.Threading

Module porting
    'Inp and Out declarations for port I/O using inpout32.dll.
    Public Declare Function Inp Lib "inpout32.dll" Alias "Inp32" (ByVal PortAddress As Integer) As Integer
    Public Declare Sub Out Lib "inpout32.dll" Alias "Out32" (ByVal PortAddress As Integer, ByVal Value As Integer)
End Module

Public Class ParallelPortSPIDriver
    Dim IO_Update As Integer = 0                          'CSB
    Dim SDO As Integer = 1                         'Serial Clock
    Dim SCLK As Integer = 2                          'As seen by DDS board (so input of parallel port)
    Dim SDI As Integer = 3                          'As seen by DDS board (so output of parallel port)
    Dim CSEn_bar As Integer = 4                      'I/O_Sync (resets communication cycle)
    Dim IO_Sync As Integer = 5                    'Transfers buffers to registers
    Dim CS1 As Integer = 6
    Dim CS2 As Integer = 7
    Dim DataRegister As Integer = &H378S            'Data Register of LPT1 (parallel port)
    Dim StatusRegister As Integer = &H379S          'Status Register of LPT1 (parallel port)
    Dim ControlRegister As Integer = &H37AS         'Control Register of LPT1 (parallel port)
    Dim SYSCLK As Double = 400000000               '400 MHz SYSCLK = 20 * 20 MHz
    Dim PulseWidth As Integer = 1                   '
    Dim PulseNumber As Integer = 2000               '

    'Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
    '    InitDDS(2, 20, 20)
    '    For Count As Integer = 1 To 10
    '        DelayRoutine(PulseNumber)
    '    Next
    'InitDDS(2, 20, 20)
    'End Sub

    'Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
    'SetFreq(3, 13200000 - 6080)
    'SetFreq(3, 13000000 - 6080)
    'SetFreq(3, 142000000 - 1562270)
    'SetFreq(3, 142100000 - 1562270 - 6120)
    'SetFreq(3, 142200000)
    'SetFreq(3, 142300000)
    'SetFreq(3, 142400000 - 5930)
    'SetFreq(3, 142500000)
    'SetFreq(3, 142410000)
    'SetFreq(3, 142420000)
    'SetFreq(3, 142430000 - 5930)
    'SetFreq(3, 143208000)
    'SetFreq(3, 140208000)

    '   SetFreq(2, 0, 40500000) 'D1 molasses
    '   For Count As Integer = 1 To 10
    '       DelayRoutine(PulseNumber)
    '  Next
    ' SetFreq(2, 1, 80000000)
    'For Count As Integer = 1 To 10
    '    DelayRoutine(PulseNumber)
    ' Next
    ' SetFreq(2, 2, 80000000)
    'For Count As Integer = 1 To 10
    '    DelayRoutine(PulseNumber)
    'Next
    'SetFreq(2, 3, 80000000)
    'SetFreqSweep(2, 3, 80000000, 80400000, 2000)
    'SetFreq(1, 3, 77760000) 'test

    'SetFreq(0, 142000000)
    'SetFreq(1, 25000000)
    'SetFreq(3, 25000000)
    'End Sub

    Public Sub InitDDS(ByVal BoardNum As Integer, ByVal RefClk As Integer, ByVal ClkMlt As Integer)
        ' Sets the VCO gain control bit (high if SYSCLK (=RefClk*ClkMlt) > 255 MHz)
        ' RefClk should be in MHz
        Dim Temp As Integer
        ResetDDS()
        DelayRoutine(PulseNumber)
        SyncIO()                                    'Reset I/O communication cycle
        DelayRoutine(PulseNumber)
        Temp = ExamineBit(BoardNum, 0)
        If Temp = -1 Then
            SetBit(DataRegister, CS1)
            'MessageBox.Show(1)
        Else
            ClearBit(DataRegister, CS1)
        End If
        Temp = ExamineBit(BoardNum, 1)
        If Temp = -1 Then
            SetBit(DataRegister, CS2)
            'MessageBox.Show(1)
        Else
            ClearBit(DataRegister, CS2)
        End If
        ClearBit(DataRegister, CSEn_bar)
        DelayRoutine(PulseNumber)
        'Instruction byte
        SendByte(1)                             'writing to register 0x01 (FR1)
        'Data byte
        If RefClk * ClkMlt > 255 Then
            SendByte(2 ^ 7 + ClkMlt * 2 ^ 2)        'VCO bit is 7 and ClkMlt is 6:2
        Else

            SendByte(ClkMlt * 2 ^ 2)                'VCO bit is <7> (don't set) and ClkMlt is <6:2>
        End If
        SendByte(0)                             'FR1 is 3 bytes wide so send 2 more
        SendByte(0)                             'bytes of nothing
        DelayRoutine(PulseNumber)
        SetBit(DataRegister, CSEn_bar)
        DelayRoutine(PulseNumber)
        UpdateIO()
    End Sub


    Public Sub SetFreq(ByVal BoardNum As Integer, ByVal Channel As Integer, ByVal Freq As Integer)
        Dim chan_sel As Integer
        Dim FTW As Long                          'frequency tuning word
        Dim BTS As Integer
        Dim Temp As Integer

        FTW = Freq * Math.Pow(2, 32) / SYSCLK
        chan_sel = 2 ^ Channel

        SyncIO()                                    'Reset I/O communication cycle
        DelayRoutine(PulseNumber)
        Temp = ExamineBit(BoardNum, 0)
        If Temp = -1 Then
            SetBit(DataRegister, CS1)
        Else
            ClearBit(DataRegister, CS1)
        End If
        Temp = ExamineBit(BoardNum, 1)
        If Temp = -1 Then
            SetBit(DataRegister, CS2)
        Else
            ClearBit(DataRegister, CS2)
        End If
        ClearBit(DataRegister, CSEn_bar)
        DelayRoutine(PulseNumber)
        '''''Select Channel''''
        'Instruction byte
        SendByte(0)
        'Data byte
        SendByte(chan_sel * 2 ^ 4)                  'channel select is <7:4>
        'SendByte(15 * 2 ^ 4)                  'channel select is <7:4>
        '''''Set Frequency'''''
        'Instruction byte
        SendByte(4)                                 'write to 0x04 register (CFTW0)
        'Data bytes
        'MessageBox.Show(FTW)
        For ByteNum As Integer = 3 To 0 Step -1     'MSB first operation (default)
            'MessageBox.Show(999)
            'BTS = (FTW / (2 ^ (ByteNum * 8))) Mod 256
            BTS = FTW >> (ByteNum * 8)
            'MessageBox.Show(BTS)
            SendByte(BTS)
        Next
        DelayRoutine(PulseNumber)
        SetBit(DataRegister, CSEn_bar)
        DelayRoutine(PulseNumber)
        UpdateIO()
    End Sub

    Public Sub SetFreqSweep(ByVal BoardNum As Integer, ByVal Channel As Integer, ByVal StartFreq As Integer, ByVal StopFreq As Integer, ByVal SweepDuration As Integer)
        Dim chan_sel As Integer
        Dim SFTW As Long                          'start frequency tuning word
        Dim EFTW As Long                          'end frequency tuning word
        Dim CFR As Long                           'CFR word
        Dim RSRR As Long                          'LSRR word
        Dim RDW As Long                           'RDWW word
        Dim TimeStep As Long                      'TimeStep
        Dim BTS As Integer
        Dim Temp As Integer

        SFTW = StartFreq * Math.Pow(2, 32) / SYSCLK
        EFTW = StopFreq * Math.Pow(2, 32) / SYSCLK
        CFR = 2 * Math.Pow(2, 22) + 1 * Math.Pow(2, 14) + 0 * Math.Pow(2, 15)
        TimeStep = SweepDuration * SYSCLK / Math.Pow(2, 32) / Math.Abs(StartFreq - StopFreq)
        RDW = 1
        RSRR = SweepDuration / 1000 * SYSCLK / Math.Pow(2, 32) / Math.Abs(StartFreq - StopFreq) * SYSCLK / 4 + 256
        chan_sel = 2 ^ Channel

        SyncIO()                                    'Reset I/O communication cycle
        DelayRoutine(PulseNumber)
        Temp = ExamineBit(BoardNum, 0)
        If Temp = -1 Then
            SetBit(DataRegister, CS1)
        Else
            ClearBit(DataRegister, CS1)
        End If
        Temp = ExamineBit(BoardNum, 1)
        If Temp = -1 Then
            SetBit(DataRegister, CS2)
        Else
            ClearBit(DataRegister, CS2)
        End If
        ClearBit(DataRegister, CSEn_bar)
        DelayRoutine(PulseNumber)
        '''''Select Channel''''
        'Instruction byte
        SendByte(0)
        'Data byte
        SendByte(chan_sel * 2 ^ 4)                  'channel select is <7:4>
        DelayRoutine(PulseNumber)

        '''''Set CFR register'''''
        'Instruction byte
        SendByte(3)                                 'write to 0x03 register (CFR)
        'Data bytes
        'MessageBox.Show(CFR)
        For ByteNum As Integer = 2 To 0 Step -1     'MSB first operation (default)
            BTS = CFR >> (ByteNum * 8)
            SendByte(BTS)
        Next
        DelayRoutine(PulseNumber)

        '''''Set Start Frequency'''''
        'Instruction byte
        SendByte(4)                                 'write to 0x04 register (CFTW0)
        'Data bytes
        'MessageBox.Show(SFTW)
        For ByteNum As Integer = 3 To 0 Step -1     'MSB first operation (default)
            BTS = SFTW >> (ByteNum * 8)
            SendByte(BTS)
        Next
        DelayRoutine(PulseNumber)

        '''''Set RSRR register'''''
        'Instruction byte
        SendByte(7)                                 'write to 0x07 register (LSRR)
        'Data bytes
        'MessageBox.Show(RSRR)
        For ByteNum As Integer = 1 To 0 Step -1     'MSB first operation (default)
            BTS = RSRR >> (ByteNum * 8)
            SendByte(BTS)
        Next
        DelayRoutine(PulseNumber)

        '''''Set RDW register'''''
        'Instruction byte
        SendByte(8)                                 'write to 0x08 register (RDW)
        'Data bytes
        'MessageBox.Show(RDW)
        For ByteNum As Integer = 3 To 0 Step -1     'MSB first operation (default)
            BTS = RDW >> (ByteNum * 8)
            SendByte(BTS)
        Next
        DelayRoutine(PulseNumber)

        '''''Set Stop Frequency'''''
        'Instruction byte
        SendByte(10)                                 'write to 0x0A register (CW1)
        'Data bytes
        'MessageBox.Show(EFTW)
        For ByteNum As Integer = 3 To 0 Step -1     'MSB first operation (default)
            BTS = EFTW >> (ByteNum * 8)
            SendByte(BTS)
        Next
        DelayRoutine(PulseNumber)

        SetBit(DataRegister, CSEn_bar)
        DelayRoutine(PulseNumber)
        UpdateIO()
    End Sub

    Public Sub ResetDDS()
        'Resets IO communication cycle
        SetBit(DataRegister, SDI)
        For Count As Integer = 1 To 10
            DelayRoutine(PulseNumber)
        Next
        ClearBit(DataRegister, SDI)
    End Sub

    Public Sub SyncIO()
        'Resets IO communication cycle
        SetBit(DataRegister, IO_Sync)
        For Count As Integer = 1 To 10
            DelayRoutine(PulseNumber)
        Next
        ClearBit(DataRegister, IO_Sync)
    End Sub

    Public Sub UpdateIO()
        'Resets IO communication cycle
        SetBit(DataRegister, IO_Update)
        For Count As Integer = 1 To 10
            DelayRoutine(PulseNumber)
        Next
        ClearBit(DataRegister, IO_Update)
    End Sub

    Public Sub SendByte(ByVal Value As Integer)
        Dim Temp As Integer
        For BitNumber As Integer = 7 To 0 Step -1   'MSB first operation (default)
            ClearBit(DataRegister, SCLK)
            DelayRoutine(PulseNumber)
            Temp = ExamineBit(Value, BitNumber)
            If Temp = -1 Then
                'MessageBox.Show(1)
                SetBit(DataRegister, SDO)
            Else
                'MessageBox.Show(0)
                ClearBit(DataRegister, SDO)
            End If
            DelayRoutine(PulseNumber)
            SetBit(DataRegister, SCLK)
            DelayRoutine(PulseNumber)
        Next
        ClearBit(DataRegister, SCLK)
    End Sub

    ' The ClearBit Sub clears the nth bit (Bit%) of an integer (Byte%).
    Public Sub ClearBit(ByVal PortAddress As Integer, ByVal BitAddress As Integer)
        ' Create a bitmask with the 2 to the nth power bit set:
        Dim Mask As Integer
        Dim Value As Integer
        Value = porting.Inp(PortAddress)
        Mask = 2 ^ BitAddress
        ' Clear the nth Bit:
        Value = Value And Not Mask
        'MessageBox.Show(Value)
        porting.Out(PortAddress, Value)
    End Sub

    ' The SetBit Sub will set the nth bit (Bit%) of an integer (Byte%).
    Public Sub SetBit(ByVal PortAddress As Integer, ByVal BitAddress As Integer)
        ' Create a bitmask with the 2 to the nth power bit set:
        Dim Mask As Integer
        Dim Value As Integer
        Value = porting.Inp(PortAddress)
        Mask = 2 ^ BitAddress
        ' Set the nth Bit:
        Value = Value Or Mask
        'MessageBox.Show(Value)
        porting.Out(PortAddress, Value)
    End Sub

    ' The ExamineBit function will return True or False depending on
    ' the value of the nth bit (Bit%) of an integer (Byte%).
    Function ExamineBit(ByVal Value As Integer, ByVal Bit As Integer) As Integer
        ' Create a bitmask with the 2 to the nth power bit set:
        Dim Mask As Integer
        Mask = 2 ^ Bit
        ' Return the truth state of the 2 to the nth power bit:
        ExamineBit = ((Value And Mask) > 0)
    End Function

    Public Sub DelayRoutine(ByVal DelayCount As Integer)
        For iCount As Integer = 1 To DelayCount
        Next
    End Sub
End Class