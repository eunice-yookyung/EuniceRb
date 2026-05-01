Imports controldll
Imports System.Text.RegularExpressions

Public Class clsInteractive
    Dim channelmap As Hashtable
    Public rootpath As String = "C:/Users/greinerlab/Documents/GitHub/RbRepository/software/exp_control/ExpControl/dynacode/"
    'instance of Keithley waveform generator controller
    Dim Hermes As KeithleyControl

    Public Sub New()
        Dim ii, jj As Integer
        For ii = 0 To 3
            digitalstate(ii) = 0
        Next
        For jj = 0 To 7
            analogstate(jj) = 0
        Next
        For ii = 0 To 3
            digitalstate2(ii) = 0
        Next
        For jj = 0 To 7
            analogstate2(jj) = 0
        Next
        'Load channel names
        Dim channels As String
        channels = My.Computer.FileSystem.ReadAllText(modCodeGenerator.rootpath + "variables/ChannelsWithCard.txt")
        Dim s, t As String()
        s = Regex.Split(channels, "\n")

        channelmap = New Hashtable()
        'Cols: ChannelName ChannelNum,CardName

        For i As Integer = 0 To (s.Length() - 1)
            t = Regex.Split(s(i), "=")
            channelmap.Add(t(0), t(1))
            'Integer.Parse(u(1))
        Next

        'interactiveAnalog = New Dev1_PCI6733()
        'interactiveAnalog2 = New Dev2_PCI6733()
        MsgBox("Got here!")
        interactiveDigital = New DIO64(0, DSmpClkRate_Hz, 1)
        MsgBox("Done interactiveDigital")
        'interactiveDigital2 = New DIO64(1, DSmpClkRate_Hz, 0)
    End Sub

    Public Sub digitalWrite(ByVal channel As Integer, ByVal state As Integer)
        digitalstate = interactiveDigital.InteractiveWrite(digitalstate, channel, state)
        Dim x As Short = 1
        'digitalstate2 = interactiveDigital2.InteractiveWrite(digitalstate2, 1, (digitalstate2(0) And x))
    End Sub
    Public Sub analogWrite(ByVal channel As Integer, ByVal val As Double)
        'analogstate = interactiveAnalog.InteractiveWrite(analogstate, channel, val)
    End Sub
    Public Sub digitalWrite2(ByVal channel As Integer, ByVal state As Integer)
        'digitalstate2 = interactiveDigital2.InteractiveWrite(digitalstate2, channel, state)
        Dim x As Short = 1
        digitalstate = interactiveDigital.InteractiveWrite(digitalstate, 1, (digitalstate(0) And x))
    End Sub
    Public Sub analogWrite2(ByVal channel As Integer, ByVal val As Double)
        'analogstate2 = interactiveAnalog2.InteractiveWrite(analogstate2, channel, val)
    End Sub

    '2 April 2009, changes to allow interactive control of Keithley generator
    Public Sub KeithleyInteractiveWrite(ByVal command As String)
        'this process the interactive command string and execute the correct routines to write waveform to a Keithley generator
        Dim commandString As String = Split(command, "/")(1)
        'different commands are separated by ;
        Dim InputSignals() As String = Split(commandString, ";")
        If InputSignals(UBound(InputSignals)) = "" Then
            InputSignals(UBound(InputSignals)) = Nothing
        End If
        'the channel to write to, is the same for all commands
        Dim channel(UBound(InputSignals)) As Integer
        For signals As Integer = 0 To InputSignals.Length - 1
            'retrieve method to execute
            Dim SubToCall As String = Split(InputSignals(signals), "(")(0)
            'retrieve arguments to method
            Dim InputToSub As String = Split(InputSignals(signals), "(")(1)
            InputToSub = Split(InputToSub, ")")(0)
            Dim Stringparameters() As String = Split(InputToSub, ",")
            Dim parameters(Stringparameters.Length - 1) As Object
            'handles all methods except AddFromFile
            If SubToCall <> "AddFromFile" Then
                'extract numerical arguments and convert them to numbers
                For j As Integer = 0 To Stringparameters.Length - 1
                    parameters(j) = Val(Stringparameters(j))
                Next j
            ElseIf SubToCall = "AddFromFile" Then
                'first one is a string
                parameters(0) = Stringparameters(0).ToString
                'convert the rest to numbers
                For j As Integer = 1 To Stringparameters.Length - 1
                    parameters(j) = Val(Stringparameters(j))
                Next j
            End If
            channel(signals) = CInt(parameters(UBound(parameters)))
            If signals = 0 Then
                'initiate Keithley controller on processing the first signal
                Hermes = New KeithleyControl(channel(0))
            End If
            'execute signal addition
            Try
                CallByName(Hermes, SubToCall, CallType.Method, parameters)
            Catch ex As MissingMemberException
                MsgBox("Function call is incorrect")
                Exit Sub
            End Try
        Next signals
        'sends error message if not all channels in signal are the same
        For signals As Integer = 0 To channel.Length - 2
            If channel(signals) <> channel(signals + 1) Then
                MsgBox("All commands must target the same channel")
            End If
        Next signals
        'when all signals has been added, write to device
        Hermes.RunSingleWriter(channel(0))
    End Sub

    Public Sub parse(ByVal commandString As String)

        '2 April 2009, begin changes to allow interactive control of Keithley waveform generator
        'valid commands must begin with Hermes.
        If commandString.StartsWith("Hermes") Then
            'run subroutine to allow interactive control of Keithley generator
            KeithleyInteractiveWrite(commandString)
            Exit Sub
        End If
        'end of changes 



        'Handle macros
        If commandString.StartsWith("_") Then

            Dim macroname As String = commandString
            macroname = macroname.Substring(1)
            Dim macrotext As String
            Try
                macrotext = My.Computer.FileSystem.ReadAllText(rootpath + "macros/" + macroname + ".txt")
            Catch ex As Exception
                MsgBox("Could not find a macro with this name")
                Return
            End Try
            Dim p As String()
            p = Regex.Split(macrotext, "\n")
            Dim k As Integer
            For k = 0 To p.GetLength(0) - 1
                p(k) = p(k).Trim()
                parse(p(k))
            Next
            Return
        End If

        'Handle clear command
        If commandString.StartsWith("clear") Then
            Dim ii, jj As Integer
            For ii = 0 To 3
                digitalstate(ii) = 0
            Next
            For jj = 0 To 7
                analogstate(jj) = 0
            Next
            For ii = 0 To 3
                digitalstate2(ii) = 0
            Next
            For jj = 0 To 7
                analogstate2(jj) = 0
            Next
            digitalWrite(1, 0)
            analogWrite(0, 0)
            digitalWrite2(1, 0)
            analogWrite2(0, 0)
            Dim numberHermes As Integer = KeithleyControl.TotalDevice
            Dim cc As Integer
            For cc = 0 To numberHermes - 1
                KeithleyInteractiveWrite(String.Concat("Hermes/clear(", cc.ToString(), ")"))
            Next
            Return
        End If

        'Assume it is a channel name
        Dim s As String()
        s = Regex.Split(commandString, " ")
        If s.GetLength(0) <> 2 Then
            MsgBox("Enter a channel name followed by either on/off or a number")
            Return
        End If
        Dim ch() As String
        Dim channel As String
        Dim channelNum As Integer
        Dim card As String
        Dim state As Integer
        Dim val As Double
        channel = s(0)
        If (Not (channelmap.Contains(channel))) Then
            MsgBox("Channel name not identified")
            Return
        End If

        ch = Regex.Split(channelmap(channel), ",")
        channelNum = Integer.Parse(ch(0))
        card = Integer.Parse(ch(1))

        If s(1).Equals("on") Then
            state = 1
            If (card = 1) Then
                digitalWrite(channelNum, state)
            End If
            If (card = 2) Then
                digitalWrite2(channelNum, state)
            End If
            Return
        End If
        If s(1).Equals("off") Then
            state = 0
            If (card = 1) Then
                digitalWrite(channelNum, state)
            End If
            If (card = 2) Then
                digitalWrite2(channelNum, state)
            End If
            Return
        End If
        Try
            val = Double.Parse(s(1))
        Catch ex As Exception
            MsgBox("Enter a channel name followed by either on/off or a number")
            Return
        End Try

        If (card = 3) Then
            analogWrite(channelNum, val)
        End If
        If (card = 4) Then
            analogWrite2(channelNum, val)
        End If
        If (card = 5) Then
            parse(String.Concat("Hermes/AddStep(", val, ",0,100,", channelNum, ")"))
        End If
        Return
    End Sub

End Class
