Imports System.Net.Sockets
Imports System.Net

Public Class clsControlParamServer
    Dim server As TcpListener
    Dim numberOfClients As Integer
    Dim clients As Collection
    'Initializes the server and waits for clients to connect
    Public Sub New(ByVal numClients As Integer)
        clients = New Collection()
        numberOfClients = numClients
        Dim localAddr As IPAddress = IPAddress.Parse("192.168.1.5")
        Dim port As Int32 = 13000
        server = New TcpListener(localAddr, port)
        server.Start()
        For count As Integer = 1 To numberOfClients
            Console.WriteLine("Waiting for a connection...")
            Dim client As TcpClient = server.AcceptTcpClient()
            clients.Add(client)
        Next count
        Console.WriteLine("All clients connected...")
    End Sub

    'Sends experiment control parameters to each of the clients
    Public Sub SendControlParam(ByRef controlParam As clsControlParams)
        For Each client As TcpClient In clients
            Dim clientStream As NetworkStream = client.GetStream()
            Dim readBuff(4096) As Byte
            clientStream.Read(readBuff, 0, readBuff.Length)
            Dim str1 As String = System.Text.Encoding.ASCII.GetString(readBuff, 0, readBuff.Length)
            If (str1.StartsWith("RequestData")) Then
                Dim str2 As String = controlParam.GetAsPacket()
                Dim sendBuff As Byte() = System.Text.Encoding.ASCII.GetBytes(str2)
                clientStream.Write(sendBuff, 0, sendBuff.Length)
            End If
        Next client
    End Sub

    ' Sends a message notifying of the availability of more experimental parameters
    Public Sub SendMoreExpAvailable(ByVal areAvailable As Boolean)
        For Each client As TcpClient In clients
            Dim clientStream As NetworkStream = client.GetStream()
            Dim readBuff(4096) As Byte
            clientStream.Read(readBuff, 0, readBuff.Length)
            Dim str1 As String = System.Text.Encoding.ASCII.GetString(readBuff, 0, readBuff.Length)
            If (str1.StartsWith("AreMoreExpAvailable")) Then
                Dim str2 As String
                If (areAvailable) Then
                    str2 = "1"
                Else
                    str2 = "0"
                End If
                Dim sendBuff As Byte() = System.Text.Encoding.ASCII.GetBytes(str2)
                clientStream.Write(sendBuff, 0, sendBuff.Length)
            End If
        Next client
    End Sub

    ' Sends a message notifying that an experiment is ready to run
    Public Sub SendRunSignal()
        For Each client As TcpClient In clients
            Dim clientStream As NetworkStream = client.GetStream()
            Dim sendBuff As Byte() = System.Text.Encoding.ASCII.GetBytes("RunSignal")
            clientStream.Write(sendBuff, 0, sendBuff.Length)
            Dim readBuff(4096) As Byte
            clientStream.Read(readBuff, 0, readBuff.Length)
        Next
    End Sub
End Class

