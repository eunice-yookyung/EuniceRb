Imports System.Text.RegularExpressions
Imports controldll
Imports controldll.analogDAQdata
Imports controldll.digitalDAQdata


Public Class clsSoftscope

    Dim rootDir As String = "C:\softscope_output\"
    Dim logDir As String

    'Dim A1 As Double(,) 'A(8,xx) - 9 rows, first row (0) time axis, the rest (1-8) 8 channel values
    'Dim A2 As Double(,)

    'totalTime: used for Hermes
    Dim totalTime As Double
    Dim triggerTime_msec As Double

    Public Sub New(ByVal total_time As Double, ByVal folderName As String)
        totalTime = total_time
        logDir = rootDir + folderName
        If (Not System.IO.Directory.Exists(logDir)) Then
            System.IO.Directory.CreateDirectory(logDir)
        End If

    End Sub

    Public Sub writeSoftscopeData(ByVal analog1 As analogDAQdata, ByVal analog2 As analogDAQdata, _
                                ByVal digital1 As digitalDAQdata, ByVal digital2 As digitalDAQdata, _
                                ByVal Hermes As KeithleyControl)

        triggerTime_msec = getSoftscopeTriggerTime(digital1)

        logAnalogData(analog1, analog2)
        logDigitalData(digital1, digital2)
        logHermesData(Hermes)

        'logProgramParam()

    End Sub

    Public Function getSoftscopeTriggerTime(ByVal digital1 As digitalDAQdata) As Double

        ' Scope Trigger is at digitaldata (1) line 64, i.e. channel=3,value= mask(15)= -32768
        ' Look through List_digital1 to find the scopeTriggerTime_msec

        Dim triggerTime_msec As Double
        Dim currentNode As digitalDAQnode = digital1.list.first
        Dim i As Integer
        For i = 1 To (digital1.list.count - 1)
            If currentNode.value(3) = (-32768) Then
                triggerTime_msec = currentNode.timestamp / digital1.SmpClkRate_Hz * 1000
                Exit For
            End If
            currentNode = currentNode.nextnode
        Next
        Return triggerTime_msec

    End Function

    Private Sub logDigitalData(ByVal digital1 As digitalDAQdata, ByVal digital2 As digitalDAQdata)

        Dim D1_text(63) As String
        Dim D2_text(63) As String
        Dim DSmpClkRate1 As Double = digital1.SmpClkRate_Hz
        Dim DSmpClkRate2 As Double = digital2.SmpClkRate_Hz

        Dim i, j As Integer
        For i = 0 To 63
            D1_text(i) = "D1_" + (i + 1).ToString("00")
            D2_text(i) = "D2_" + (i + 1).ToString("00")
        Next

        Dim currentChannelNum As Integer
        Dim currentTime As Double
        Dim currentNode As digitalDAQnode

        'convert the list into edge times for each channel
        'D1 first
        currentNode = digital1.list.first
        For i = 0 To (digital1.list.count - 1)
            If i > 0 Then
                currentNode = currentNode.nextnode
            End If
            For j = 0 To 3
                If currentNode.value(j) <> 0 Then
                    currentChannelNum = Math.Log(Math.Abs(currentNode.value(j) + 0.0), 2) + 16 * j + 1  'plus 0.0 to convert to double
                    currentTime = currentNode.timestamp / DSmpClkRate1 * 1000 - triggerTime_msec
                    D1_text(currentChannelNum - 1) = D1_text(currentChannelNum - 1) + ", " + currentTime.ToString() '"F3")

                End If
            Next
        Next
        'now D2
        currentNode = digital2.list.first
        For i = 0 To (digital2.list.count - 1)
            If i > 0 Then
                currentNode = currentNode.nextnode
            End If
            For j = 0 To 3
                If currentNode.value(j) <> 0 Then
                    currentChannelNum = Math.Log(Math.Abs(currentNode.value(j) + 0.0), 2) + 16 * j + 1  'plus 0.0 to convert to double
                    currentTime = currentNode.timestamp / DSmpClkRate2 * 1000 - triggerTime_msec
                    D2_text(currentChannelNum - 1) = D2_text(currentChannelNum - 1) + ", " + currentTime.ToString() '"F3")

                End If
            Next
        Next

        'logs the digitalData
        Dim txtFile As String = logDir + "\D1_current.txt"
        Dim outFile As IO.StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(txtFile, False, System.Text.Encoding.ASCII)
        For i = 0 To 63
            outFile.WriteLine(D1_text(i))
        Next
        outFile.Close()

        txtFile = logDir + "\D2_current.txt"
        outFile = My.Computer.FileSystem.OpenTextFileWriter(txtFile, False, System.Text.Encoding.ASCII)
        For i = 0 To 63
            outFile.WriteLine(D2_text(i))
        Next
        outFile.Close()

    End Sub



    Private Sub logAnalogData(ByVal analog1 As analogDAQdata, ByVal analog2 As analogDAQdata)

        Dim ASmpClkRate1 As Double = analog1.SmpClkRate_Hz
        Dim ASmpClkRate2 As Double = analog2.SmpClkRate_Hz

        Dim csvFile As String
        Dim outFile As IO.StreamWriter
        Dim i, j As Integer

        'logs the analogData

        'A1 first
        csvFile = logDir + "\A1_current.csv"
        outFile = My.Computer.FileSystem.OpenTextFileWriter(csvFile, False, System.Text.Encoding.ASCII)

        ' time axis in first row
        Dim A1_textRow(analog1.data.GetLength(1) - 1) As String

        'j=0: time
        For i = 0 To analog1.data.GetLength(1) - 1
            A1_textRow(i) = (i / ASmpClkRate1 * 1000 - triggerTime_msec).ToString() '"F3") '3 decimal, 1 us
        Next
        outFile.WriteLine(Join(A1_textRow, ", "))

        For j = 1 To 8
            'If analog1.IsChannelUsed(j - 1) = True Then
            For i = 0 To analog1.data.GetLength(1) - 1
                'If analog1.data(j - 1, i) = 0 Then
                '    A1_textRow(i) = "0"
                'Else
                '    A1_textRow(i) = analog1.data(j - 1, i).ToString() '"F3")
                'End If
                A1_textRow(i) = analog1.data(j - 1, i).ToString()
            Next
            outFile.WriteLine(Join(A1_textRow, ", "))
            'Else
            'outFile.WriteLine(";")
            'End If
        Next
        outFile.Close()


        'then A2
        'A1 first
        csvFile = logDir + "\A2_current.csv"
        outFile = My.Computer.FileSystem.OpenTextFileWriter(csvFile, False, System.Text.Encoding.ASCII)
        ' time axis in first row
        Dim A2_textRow(analog2.data.GetLength(1) - 1) As String

        'j=0: time
        For i = 0 To analog2.data.GetLength(1) - 1
            A2_textRow(i) = (i / ASmpClkRate2 * 1000 - triggerTime_msec).ToString() '"F3") '3 decimal, 1 us
        Next
        outFile.WriteLine(Join(A2_textRow, ", "))

        For j = 1 To 8
            'If analog2.IsChannelUsed(j - 1) = True Then
            For i = 0 To analog2.data.GetLength(1) - 1
                'If analog2.data(j - 1, i) = 0 Then
                '    A2_textRow(i) = "0"
                'Else
                '    A2_textRow(i) = analog2.data(j - 1, i).ToString() '"F3")
                'End If
                A2_textRow(i) = analog2.data(j - 1, i).ToString()
            Next
            outFile.WriteLine(Join(A2_textRow, ", "))
            'Else
            'outFile.WriteLine(";")
            'End If
        Next
        outFile.Close()

    End Sub


    Private Sub logHermesData(ByVal Hermes As KeithleyControl)

        'Number of Keithley channels
        Dim numChn As Integer = Hermes.GetIPAddress.Length()
        'Hermes clock rate
        Dim HSmpClkRate As Double = Hermes.SampleFrequency()
        'Hermes.KeithleyData(1).Length is a fixed constant ~ 140 sec of data
        Dim numSamples As Integer = Int(totalTime * HSmpClkRate / 1000)

        Dim txtFile As String = logDir + "\Hermes_current.csv"
        Dim outFile As IO.StreamWriter = My.Computer.FileSystem.OpenTextFileWriter(txtFile, False, System.Text.Encoding.ASCII)
        Dim i, j As Integer

        Dim H_textRow(numSamples - 1) As String

        'time axis first
        For i = 0 To numSamples - 1
            H_textRow(i) = (i / HSmpClkRate * 1000 - triggerTime_msec).ToString() '"F3") '3 decimal, 1 us
        Next
        outFile.WriteLine(Join(H_textRow, ", ")) 'format as csv

        Dim data As Double()

        For j = 0 To numChn - 1

            data = Hermes.KeithleyData(j)
            For i = 0 To numSamples - 1
                H_textRow(i) = data(i).ToString() '"F3") '3 decimal, 1 us
            Next
            outFile.WriteLine(Join(H_textRow, ", "))
        Next
        outFile.Close()


    End Sub


End Class