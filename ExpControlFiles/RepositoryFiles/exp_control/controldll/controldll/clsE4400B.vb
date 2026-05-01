Public Class E4400B
    Implements GPIBParser
    Public Function GenerateCmdString(ByVal command As String, ByVal arg As Double) As String Implements GPIBParser.GenerateCmdString
        Dim commandString As String = ""
        'Sets the output frequency in MHz
        If command.Equals("setfreq") Then
            commandString = String.Format(":FREQ {0:#.######} MHz", arg)
        End If
        Return commandstring
    End Function
End Class
