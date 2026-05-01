Imports System.Reflection
Imports System.CodeDom.Compiler
Imports Microsoft.VisualBasic
Imports System.Text.RegularExpressions
Imports controldll
Imports expcontrol.SpectronServiceReference

Module modCodeGenerator
    Dim experiment As Object
    Public rootpath As String = "C:/Users/greinerlab/Documents/GitHub/RbRepository/software/exp_control/ExpControl/dynacode/"
    Function generateCode(ByVal programLocation As String) As Integer
        'Load files for dynamic code creation
        Dim header, footer, program, userprogram As String
        Dim channels, expConstants, expVariables, auxRoutines As String
        Dim variables As String = ""
        header = My.Computer.FileSystem.ReadAllText(rootpath + "backbone/header.txt")
        footer = My.Computer.FileSystem.ReadAllText(rootpath + "backbone/footer.txt")

        'If the variables part is not found
        If InStr(My.Computer.FileSystem.ReadAllText(programLocation), "'=====Variables=====") = 0 Then
            MsgBox("Error: Invalid exp. variables in the program file.")
            Return -1
        End If
        Dim userprogramAllSplit As String()
        userprogramAllSplit = Regex.Split(My.Computer.FileSystem.ReadAllText(programLocation), "'=====Variables=====")

        userprogram = userprogramAllSplit(0) + userprogramAllSplit(2)
        expVariables = userprogramAllSplit(1).Trim()

        program = vbNewLine + "Public Sub build(ByRef cp As clsControlParams, " _
                                            & "ByRef spectron1 As SpectronClient, " _
                                            & "ByRef spectron2 As SpectronClient, " _
                                            & "ByRef digitaldata As digitalDAQdata, " _
                                            & "ByRef digitaldata2 As digitalDAQdata, " _
                                            & "ByRef gpib As GPIBControl, " _
                                            & "ByRef Hermes As KeithleyControl, " _
                                            & "ByRef dds As AD9959Ev)" + vbNewLine
        channels = My.Computer.FileSystem.ReadAllText(rootpath + "variables/ChannelsWithCard.txt")
        expConstants = My.Computer.FileSystem.ReadAllText(rootpath + "variables/ExpConstants.txt")
        auxRoutines = My.Computer.FileSystem.ReadAllText(rootpath + "variables/AuxRoutines.txt")

        'Declare variables and add experiment variables to hashtable
        Dim s, t, u As String()
        s = Regex.Split(channels, "\n")
        For i As Integer = 0 To (s.Length() - 1)
            If (s(i).Contains("=")) Then
                t = Regex.Split(s(i), "=")
                u = Regex.Split(t(1), ",")  'discard card number
                variables = variables + "Dim " + t(0) + " As Integer = " + u(0) + vbNewLine
            End If
        Next
        s = Regex.Split(expConstants, "\n")
        For i As Integer = 0 To (s.Length() - 1)
            t = Regex.Split(s(i), "=")
            variables = variables + "Dim " + t(0) + " As Double = " + t(1) + vbNewLine
        Next
        s = Regex.Split(expVariables, "\n")
        For i As Integer = 0 To (s.Length() - 1)
            t = Regex.Split(s(i), "=")
            variables = variables + "Dim " + t(0) + " As Double = " + t(1) + vbNewLine
            'Chr(34) returns a double quote, e.g. "
            program = program + "If cp.IsDefined(" + Chr(34) + t(0) + Chr(34) + ") Then" _
                        + vbNewLine + t(0) + "= cp.GetItem(" + Chr(34) + t(0) + Chr(34) + ")" _
                        + vbNewLine + "Else" + vbNewLine + "cp.Put(" + Chr(34) + t(0) + Chr(34) _
                        + "," + t(0) + ")" + vbNewLine + "End If" + vbNewLine
        Next
        s = Regex.Split(auxRoutines, "\n")
        For i As Integer = 0 To (s.Length() - 1)
            program = program + s(i) + vbNewLine
        Next
        program = program + userprogram + vbNewLine

        ''test alex
        'Using outfile As New IO.StreamWriter("C:\varTest.txt")
        '    For i As Integer = 0 To Session.Count - 1
        '        outfile.Write("<p>" + Session.Keys(i).ToString() + " - " + Session(i).ToString() + "</p>")
        '    Next
        'End Using

        program = program + "End Sub" + vbNewLine
        'Declare subroutines
        Dim subs As String = ""
        'Compile the code
        Dim loCompiler As CodeDomProvider
        loCompiler = CodeDomProvider.CreateProvider("VisualBasic")
        'Dim loCompiler As ICodeCompiler = New VBCodeProvider().CreateCompiler()
        Dim loParameters As CompilerParameters = New CompilerParameters()
        'perhaps unnecessary since this is included in the header as "Imports controldll"?
        loParameters.ReferencedAssemblies.Add("C:\software\controldll\controldll\bin\Debug\controldll.dll")
        loParameters.GenerateInMemory = False
        Dim code As String = header + variables + program + subs + footer

        'Clean up other return/newline types: replace with vbNewLine
        'Note: "\n" includes vbNewLine
        Dim codeLines As String() = Regex.Split(code, "\n")
        'Remove white space from all the lines
        For i As Integer = 0 To codeLines.Length - 1
            codeLines(i) = codeLines(i).Trim()
        Next
        code = String.Join(vbNewLine, codeLines)
        'in order to make the line number in the errors match the real line numbers
        codeLines = Regex.Split(code, vbNewLine)

        'test alex
        'Using outfile As New IO.StreamWriter("C:\codeTest.txt")
        'outfile.Write(code)
        'End Using


        Dim loCompiled As CompilerResults = loCompiler.CompileAssemblyFromSource(loParameters, code)
        'Show compiler errors if available
        If (loCompiled.Errors.HasErrors) Then

            Dim lcErrorMsg As String = ""
            lcErrorMsg = loCompiled.Errors.Count.ToString() + " Errors:"
            For x As Integer = 0 To Math.Min(loCompiled.Errors.Count - 1, 4)
                lcErrorMsg = lcErrorMsg + vbNewLine + vbNewLine + "Line: " + loCompiled.Errors(x).Line.ToString() + " - " + loCompiled.Errors(x).ErrorText
                If codeLines(loCompiled.Errors(x).Line - 1).Length > 100 Then
                    lcErrorMsg = lcErrorMsg + vbNewLine + "      [ " + Left$(codeLines(loCompiled.Errors(x).Line - 1), 100) + "   ... ]"
                Else
                    lcErrorMsg = lcErrorMsg + vbNewLine + "      [ " + codeLines(loCompiled.Errors(x).Line - 1) + " ]"
                End If
            Next
            MsgBox(lcErrorMsg)
            Return -1

        Else
            'Instantiate Experiment class 
            Dim loAssembly As Assembly = loCompiled.CompiledAssembly
            experiment = loAssembly.CreateInstance("Experiment")

        End If
        Return 0
    End Function

    Public Sub runCode(ByRef cp As clsControlParams, _
                        ByRef spectron1 As SpectronClient, ByRef spectron2 As SpectronClient, _
                        ByRef digitaldata As digitalDAQdata, ByRef digitaldata2 As digitalDAQdata, _
                        ByRef gpib As GPIBControl, ByRef Hermes As KeithleyControl, ByRef dds As AD9959Ev)
        cp.Put("starter", 0.0)
        Dim loCodeParms() As Object = {cp, spectron1, spectron2, digitaldata, digitaldata2, gpib, Hermes, dds}
        Try
            experiment.GetType().InvokeMember("build", BindingFlags.InvokeMethod, Nothing, experiment, loCodeParms)
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub logCode(ByVal programLocation As String, ByVal log_Dir As String, ByVal dt As DataTable)

        Dim userprogramName, logExpParam As String
        'Dim channels, expConstants, expVariables As String
        'Dim variables As String = ""
        'header = My.Computer.FileSystem.ReadAllText(rootpath + "backbone/header.txt")
        'footer = My.Computer.FileSystem.ReadAllText(rootpath + "backbone/footer.txt")

        Dim t As String()
        Dim sep(3) As Char
        sep(0) = "\"
        sep(1) = "."
        t = programLocation.Split(sep)
        userprogramName = t(t.GetLength(0) - 2)

        'Copy the files to data folder, without overwriting.
        My.Computer.FileSystem.CopyFile(programLocation, log_Dir + userprogramName + ".txt")
        My.Computer.FileSystem.CopyFile(rootpath + "variables/ChannelsWithCard.txt", log_Dir + "ChannelsWithCard.txt")
        My.Computer.FileSystem.CopyFile(rootpath + "variables/ExpConstants.txt", log_Dir + "ExpConstants.txt")
        My.Computer.FileSystem.CopyFile(rootpath + "variables/AuxRoutines.txt", log_Dir + "AuxRoutines.txt")
        'My.Computer.FileSystem.CopyFile(rootpath + "variables/ExpVariables.txt", log_Dir + "ExpVariables.txt")

        'Log the time, program
        Dim currentTime As String = System.DateTime.Now.ToString()
        logExpParam = "Experiment Log" + vbNewLine + vbNewLine
        logExpParam = logExpParam + currentTime + vbNewLine + vbNewLine
        logExpParam = logExpParam + "Experiment Program Used:" + vbNewLine + programLocation + vbNewLine + vbNewLine

        'Log batch parameters from gui.dt
        logExpParam = logExpParam + "Batch Length:  " + vbNewLine + dt.Rows.Count().ToString() + vbNewLine + vbNewLine
        logExpParam = logExpParam + "==========================" + vbNewLine

        Dim col As DataColumn
        For Each col In dt.Columns
            logExpParam = logExpParam + col.ColumnName().ToString() + ",  "
        Next col
        logExpParam = logExpParam + vbNewLine + "--------------------------" + vbNewLine

        Dim row As DataRow
        For Each row In dt.Rows
            For Each col In dt.Columns
                logExpParam = logExpParam + row(col).ToString() + ",  "
            Next col
            logExpParam = logExpParam + vbNewLine
        Next row

        Using outfile As New IO.StreamWriter(log_Dir + "logExpParam.txt")
            outfile.Write(logExpParam)
        End Using

    End Sub
End Module
