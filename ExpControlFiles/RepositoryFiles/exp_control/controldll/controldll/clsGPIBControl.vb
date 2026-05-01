Public Class GPIBControl
    Public Sub GPIBdev(ByVal devicenum As Integer, ByVal commandString As String)
        Dim dev As New GPIBdevice(devicenum)
        dev.write(commandString)
    End Sub
    Public Sub E4400B(ByVal devicenum As Integer, ByVal command As String, ByVal arg As Double)
        Dim dev As New GPIBdevice(devicenum)
        Dim E4400Bparser As New E4400B()
        dev.write(E4400Bparser.GenerateCmdString(command, arg))
    End Sub
    Public Sub Agilent33220A(ByVal devicenum As Integer, ByVal command As String, ByVal arg As Double)
        Dim dev As New GPIBdevice(devicenum)
        Dim Agilent33220Aparser As New Agilent33220A()
        dev.write(Agilent33220Aparser.GenerateCmdString(command, arg))
    End Sub
    Public Sub Agilent33250A(ByVal devicenum As Integer, ByVal command As String, ByVal arg As Double)
        Dim dev As New GPIBdevice(devicenum)
        Dim Agilent33250Aparser As New Agilent33250A()
        dev.write(Agilent33250Aparser.GenerateCmdString(command, arg))
    End Sub
    Public Sub clsAgilent36xxA(ByVal devicenum As Integer, ByVal command As String, ByVal arg As Double)
        Dim dev As New GPIBdevice(devicenum)
        Dim clsAgilent36xxAparser As New clsAgilent36xxA()
        dev.write(clsAgilent36xxAparser.GenerateCmdString(command, arg))
    End Sub
End Class
