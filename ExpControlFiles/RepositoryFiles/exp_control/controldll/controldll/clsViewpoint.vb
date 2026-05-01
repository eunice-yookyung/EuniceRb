Imports System.Runtime.InteropServices
Imports System.Threading

Module modViewpoint

    Const DIO64_ATTR_INPUTMODE As Short = 0
    Const DIO64_ATTR_OUTPUTMODE As Short = 1
    Const DIO64_ATTR_INPUTBUFFERSIZE As Short = 2
    Const DIO64_ATTR_OUTPUTBUFFERSIZE As Short = 3
    Const DIO64_ATTR_MAJORCLOCKSOURCE As Short = 4
    Const DIO64_ATTR_INPUTTHRESHOLD As Short = 5
    Const DIO64_ATTR_OUTPUTTHRESHOLD As Short = 6
    Const DIO64_ATTR_INPUTTIMEOUT As Short = 7
    Const DIO64_ATTR_RTSIGLOBALENABLE As Short = 8
    Const DIO64_ATTR_RTSICLKSOURCE As Short = 9
    Const DIO64_ATTR_RTSICLKTRIG7ENABLE As Short = 10
    Const DIO64_ATTR_EXTERNALCLKENABLE As Short = 11
    Const DIO64_ATTR_PXICLKENABLE As Short = 12
    Const DIO64_ATTR_RTSISCANCLKTRIG0ENABLE As Short = 13
    Const DIO64_ATTR_RTSISTARTTRIG2ENABLE As Short = 14
    Const DIO64_ATTR_RTSISTOPTRIG3ENABLE As Short = 15
    Const DIO64_ATTR_RTSIMODSCANCLKTRIG4ENABLE As Short = 16
    Const DIO64_ATTR_PXISTARENABLE As Short = 17
    Const DIO64_ATTR_PORTROUTING As Short = 18
    Const DIO64_ATTR_STATICOUTPUTMASK As Short = 19
    Const DIO64_ATTR_SERIALNUMBER As Short = 20
    Const DIO64_ATTR_ARMREENABLE As Short = 21
    Const DIO64_ATTR_SCLKENABLE As Short = 22
    Const DIO64_ATTR_FPGAINFO As Short = 23

    Const DIO64_ERR_ILLEGALBOARD As Short = -8
    Const DIO64_ERR_BOARDNOTOPENED As Short = -9
    Const DIO64_ERR_STATUSOVERRUNUNDERRUN As Short = -10
    Const DIO64_ERR_INVALIDPARAMETER As Short = -12
    Const DIO64_ERR_NODRIVERINTERFACE As Short = -13
    Const DIO64_ERR_OCXOOPTIONNA As Short = -14
    Const DIO64_ERR_PXIONLYSIGNALS As Short = -15
    Const DIO64_ERR_STOPTRIGSRCINVALID As Short = -16
    Const DIO64_ERR_PORTNUMBERCONFLICTS As Short = -17
    Const DIO64_ERR_MISSINGDIO64CATFILE As Short = -18
    Const DIO64_ERR_NOTENOUGHRESOURCES As Short = -19
    Const DIO64_ERR_INVALIDSIGNITUREDIO64CAT As Short = -20
    Const DIO64_ERR_REQUIREDIMAGENOTFOUND As Short = -21
    Const DIO64_ERR_ERRORPROGFPGA As Short = -22
    Const DIO64_ERR_FILENOTFOUND As Short = -23
    Const DIO64_ERR_BOARDERROR As Short = -24
    Const DIO64_ERR_FUNCTIONCALLINVALID As Short = -25
    Const DIO64_ERR_NOTENOUGHTRANS As Short = -26

    Structure DIO64STAT
        Public pktsize As Short
        Public portCount As Short
        Public writePtr As Short
        Public readPtr As Short
        Public time As Integer
        Public fifoSize As Integer
        Public fifo0 As Short
        Public ticks As Integer
        Public flags As Short
        Public clkControl As Short
        Public startControl As Short
        Public stopControl As Short
        Public AIControl As Integer
        Public AICurrent As Short
        Public startTime As Integer
        Public stopTime As Integer
        Public user0 As Short
        Public user1 As Short
        Public user2 As Short
        Public user3 As Short
    End Structure


    <DllImport("C:\DIO64Visa\DIO64_Visa32.dll", _
                    EntryPoint:="DIO64_OpenResource", _
                    CallingConvention:=CallingConvention.StdCall, _
                    CharSet:=CharSet.Auto)> _
    Public Function DIO64_OpenResource(ByVal resourceName As String, ByVal board As UShort, ByVal baseio As UShort) As Long
    End Function

    '<DllImport("C:\DIO64Visa\DIO64_Visa32.dll", _
    '            EntryPoint:="DIO64_Open", _
    '            CallingConvention:=CallingConvention.Cdecl, _
    '            ThrowOnUnmappableChar:=True, _
    '            CharSet:=CharSet.Auto)> _
    'Public Function DIO64_Open(<MarshalAs(UnmanagedType.LPStr)> resourceName As String, ByVal board As UShort, ByVal baseio As UShort) As Long
    'End Function

    <DllImport("C:\DIO64Visa\DIO64_Visa32.dll", CallingConvention:=CallingConvention.Cdecl, CharSet:=CharSet.Ansi)> _
    Public Function DIO64_GetAttr(ByVal board As UShort, ByRef attrID As UShort, ByRef value As UShort) As UShort
    End Function

    'Public Declare Auto Sub DIO64_OpenResource Lib "C:\DIO64Visa\DIO64_Visa32.dll" _
    '    (<MarshalAs(UnmanagedType.LPStr)> resourceName As String, ByVal board As UShort, ByVal baseio As UShort)

    'Declare Function DIO64_OpenResource Lib "C:\DIO64Visa\DIO64_Visa32.dll" (ByRef resourceName As String, ByVal board As UShort, ByVal baseio As UShort) As Long
    'Declare Function DIO64_Open Lib "DIO64_Visa32.dll" (ByVal board As Short, ByVal baseio As Short) As Short
    Declare Function DIO64_Load Lib "C:\DIO64Visa\DIO64_Visa32.dll" (ByVal board As Short, ByVal bnmFile As String, ByVal InputHint As Short, ByVal OutputHint As Short) As Short
    Declare Function DIO64_Close Lib "DIO64_Visa32.dll" (ByVal board As Short) As Short
    Declare Function DIO64_In_Start Lib "DIO64_Visa32.dll" (ByVal board As Short, ByVal ticks As Integer, ByRef mask As Short, ByVal maskLength As Short, ByVal flags As Short, ByVal clkControl As Short, ByVal starttype As Short, ByVal startsource As Short, ByVal stopType As Short, ByVal stopsource As Short, ByVal AIControl As Integer, ByRef scanrate As Double) As Short
    Declare Function DIO64_In_Status Lib "DIO64_Visa32.dll" (ByVal board As Short, ByRef scansavail As Integer, ByRef stat As DIO64STAT) As Short
    Declare Function DIO64_In_Read Lib "DIO64_Visa32.dll" (ByVal board As Short, ByRef buffer As Short, ByVal scansToRead As Integer, ByRef stat As DIO64STAT) As Short
    Declare Function DIO64_In_Stop Lib "DIO64_Visa32.dll" (ByVal board As Short) As Short
    Declare Function DIO64_Out_ForceOutput Lib "C:\DIO64Visa\DIO64_Visa32.dll" (ByVal board As Short, ByRef buffer As Short, ByVal mask As Integer) As Short
    Declare Function DIO64_Out_GetInput Lib "DIO64_Visa32.dll" (ByVal board As Short, ByRef buffer As Short) As Short
    Declare Function DIO64_Out_Config Lib "DIO64_Visa32.dll" (ByVal board As Short, ByVal ticks As Integer, ByRef mask As Short, ByVal maskLength As Short, ByVal flags As Short, ByVal clkControl As Short, ByVal starttype As Short, ByVal startsource As Short, ByVal stopType As Short, ByVal stopsource As Short, ByVal AIControl As Integer, ByVal reps As Short, ByVal ntrans As Short, ByRef scanrate As Double) As Short
    Declare Function DIO64_Out_Start Lib "C:\DIO64Visa\DIO64_Visa32.dll" (ByVal board As Short) As Short
    Declare Function DIO64_Out_Status Lib "DIO64_Visa32.dll" (ByVal board As Short, ByRef scansavail As Integer, ByRef stat As DIO64STAT) As Short
    Declare Function DIO64_Out_Write Lib "C:\DIO64Visa\DIO64_Visa32.dll" (ByVal board As Short, ByRef buffer As Short, ByVal bufsize As Integer, ByRef stat As DIO64STAT) As Short
    Declare Function DIO64_Out_Stop Lib "DIO64_Visa32.dll" (ByVal board As Short) As Short

    Declare Function DIO64_SetAttr Lib "DIO64_Visa32.dll" (ByVal board As Short, ByVal attrID As Short, ByVal value As Short) As Short
    'Declare Function DIO64_GetAttr Lib "DIO64_Visa32.dll" (ByVal board As Short, ByRef attrID As Short, ByRef value As Short) As Short

End Module
Public Class DIO64
    Public Sub New(ByVal board As Short, ByVal SmpClkRate_Hz As Double, ByVal ismaster As Integer)
        _status = New DIO64STAT
        _board = board
        _SmpClkRate_Hz = SmpClkRate_Hz
        master = ismaster
        initialize()
    End Sub
    'Public Sub DIO64_Open()
    '    Dim resourceName As String = "PXI4::0::INSTR"
    '    Dim retVal As Integer
    '    'retVal = DIO64_OpenResource(resourceName, 0, 384)
    '    Console.WriteLine(retVal)
    'End Sub
    Public Sub initialize()
        Dim errorout As Long
        Dim scanrate As Double
        Dim mask() As Short = {-1, -1, -1, -1}
        Dim resourceName As String = "PXI4::0::INSTR"
        'Dim hModule As IntPtr, procAddr As String
        'Dim hModule As Long, procAddr As String
        MsgBox("Super new Initialize DIO64 Routine")
        errorout = DIO64_OpenResource(resourceName, 0, 384)
        'hModule = LoadLibrary("C:\DIO64Visa\DIO64_Visa32.dll")
        'procAddr = GetProcAddress(hModule, "DIO64_OpenResource")
        ''hModule = LoadLibrary("user32.dll")
        ''procAddr = GetProcAddress(hModule, "MessageBox")
        'Dim DIO64Thread As Thread
        'DIO64Thread = New Thread(AddressOf DIO64_Open)
        'Console.WriteLine("Hi")
        'errorout = DIO64_Open(resourceName, 0, 384)
        MsgBox("Done initializing DIO64")
        If (errorout <> 0) Then MsgBox("Board Open failed")
        errorout = DIO64_Load(_board, "", 0, 4)
        If (errorout <> 0) Then MsgBox("Board Load failed")
        If master = 1 Then
            errorout = DIO64_SetAttr(_board, 13, 1) 'drives scan clock out RTSI0 
            If (errorout <> 0) Then MsgBox("Board Status failed")
            errorout = DIO64_SetAttr(_board, 14, 1) 'drives start trigger on RTSI2 
            If (errorout <> 0) Then MsgBox("Board start trigger setting failed")
            errorout = DIO64_Out_Config(_board, 1, mask(0), 4, 0, 1, 1, 0, 0, 0, 0, 1, 0, scanrate)   'April 29, 2009 replace internal clock by external 20 MHz source
            If (errorout <> 0) Then MsgBox("Board Config failed")
        Else
            errorout = DIO64_SetAttr(_board, 13, 0) 'does not drive scan clock out RTSI0 
            If (errorout <> 0) Then MsgBox("Board Status failed")
            errorout = DIO64_SetAttr(_board, 14, 1) 'does not drive start trigger on RTSI2 
            If (errorout <> 0) Then MsgBox("Board start trigger setting failed")
            errorout = DIO64_Out_Config(_board, 1, mask(0), 4, 0, 2, 1, 2, 0, 0, 0, 1, 0, 1)
            If (errorout <> 0) Then MsgBox(errorout)
        End If
    End Sub
    Public Sub Start(ByRef data(,) As Short)
        Dim errorout As Short


        errorout = DIO64_Out_Status(_board, _scansavail, _status)
        If (errorout <> 0) Then MsgBox("Board Status failed")

        errorout = DIO64_Out_Write(_board, data(0, 0), data.GetLength(0), _status)
        If (errorout <> 0) Then Throw New System.Exception("Board Write failed")
        errorout = DIO64_Out_Start(_board)
        If (errorout <> 0) Then Throw New System.Exception("Board Start failed")
    End Sub
    Public Function InteractiveWrite(ByRef currentState() As Short, ByVal channel As Integer, ByVal state As Integer) As Short()
        Dim errorout As Short
        Dim portNo, portChannel, j As Integer
        portNo = Math.Floor(channel / 16.0)
        portChannel = channel - portNo * 16 - 1
        Dim s As Short = 0
        For j = 0 To 15
            If j <> portChannel Then
                s = s + (currentState(portNo) And mask(j))
            Else
                s = s + state * mask(j)
            End If
        Next
        currentState(portNo) = s
        errorout = DIO64_Out_ForceOutput(_board, currentState(0), 15)
        If (errorout <> 0) Then Throw New System.Exception("Interactive write failed")
        Return currentState
    End Function
    Public Sub Finish()
        Dim errorout As Short

        errorout = DIO64_Out_Stop(_board)
        If (errorout <> 0) Then Throw New System.Exception("Board Stop failed")
        errorout = DIO64_Close(_board)
        If (errorout <> 0) Then Throw New System.Exception("Board close failed")
    End Sub
    Public Sub Close()
        Dim errorout As Short

        errorout = DIO64_Close(_board)
        If (errorout <> 0) Then Throw New System.Exception("Board close failed")
    End Sub
    Private _status As DIO64STAT
    Private _board As Short
    Private _SmpClkRate_Hz As Double
    Private _scansavail As Integer
    Private mask() As Short = {1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, -32768}        'Used for converting integers to bytes.
    Private master As Integer

    'Private Declare Function LoadLibrary Lib "kernel32" Alias "LoadLibraryA" (ByVal lpLibFileName As String) As Long
    'Private Declare Function FreeLibrary Lib "kernel32" (ByVal hLibModule As Long) As Long
    'Private Declare Function GetProcAddress Lib "kernel32" (ByVal ModuleHandle As Long, ByVal ProcName As String) As Long


    'Private Declare Function LoadLibrary Lib "kernel32" Alias "LoadLibraryA" (ByVal _
    '    lpLibFileName As String) As Long
    'Private Declare Function GetProcAddress Lib "kernel32" (ByVal hModule As Long, _
    '    ByVal lpProcName As String) As Long
    'Private Declare Function FreeLibrary Lib "kernel32" (ByVal hLibModule As Long) _
    '    As Long


    Public Function LoadLibrary(<[In](), MarshalAs(UnmanagedType.LPTStr)> ByVal lpFileName As String) As IntPtr
    End Function

    Public Function GetProcAddress(<[In]()> ByVal hModule As IntPtr, <[In](), MarshalAs(UnmanagedType.LPStr)> ByVal lpProcName As String) As IntPtr
    End Function

    'Private Declare Function FreeLibrary Lib "kernel32" (ByVal hLibModule As Long) As Long
    'Private Declare Function LoadLibrary Lib "kernel32" Alias "LoadLibraryA" (ByVal lpLibFileName As String) As Long
    'Private Declare Function GetProcAddress Lib "kernel32" (ByVal hModule As Long, ByVal lpProcName As String) As Long


End Class
