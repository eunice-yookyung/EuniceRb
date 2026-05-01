Public Class Agilent33250A
    Implements GPIBParser
    Public Function GenerateCmdString(ByVal command As String, ByVal arg As Double) As String Implements GPIBParser.GenerateCmdString
        Dim commandString As String = ""
        If command.Equals("setsweepstart") Then
            commandString = String.Format(":FREQ:STAR {0:##.####} Mhz", arg)
        End If
        If command.Equals("setsweepstop") Then
            commandString = String.Format(":FREQ:STOP {0:##.####} Mhz", arg)
        End If
        If command.Equals("setfreq") Then
            commandString = String.Format(":FREQ {0:##.####} Mhz", arg)
        End If
        Return commandString
    End Function
End Class
