Public Class clsAgilent36xxA
    ' Parser for setting parameters of the Agilent 36xxA series powers supplies
    Implements GPIBParser
    Public Function GenerateCmdString(ByVal command As String, ByVal arg As Double) As String Implements GPIBParser.GenerateCmdString
        Dim commandString As String = ""
        If command.Equals("setcurrent") Then
            commandString = String.Format("CURR ##.####", arg)
        End If
        Return commandString
    End Function

End Class

