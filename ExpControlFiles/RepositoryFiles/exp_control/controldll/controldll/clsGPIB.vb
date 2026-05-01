Imports NationalInstruments.NI4882
Public Class GPIBdevice
    Public Sub New(ByVal devicenum As Integer)
        Dim boardnumber As Integer
        Dim pad As Integer
        boardnumber = Math.Floor(devicenum / 100.0)
        pad = devicenum - boardnumber * 100
        Try
            device = New Device(boardnumber, CByte(pad))
        Catch ex As Exception
        End Try
    End Sub
    Public Sub write(ByVal commandString As String)
        Try
            device.Write(ReplaceCommonEscapeSequences(commandString))
        Catch ex As Exception
        End Try
    End Sub
    Public Function read() As String
        Try
            Return insertcommonescapesequences(device.ReadString())
        Catch ex As Exception
            Return ""
        End Try
    End Function
    Private Function ReplaceCommonEscapeSequences(ByVal s As String) As String
        Return s.Replace("\n", ControlChars.Lf).Replace("\r", ControlChars.Cr)
    End Function
    Private Function insertcommonescapesequences(ByVal s As String) As String
        Return s.Replace(ControlChars.Lf, "\n").Replace(ControlChars.Cr, "\r")
    End Function
    Private device As device
End Class
