Imports System.Collections

Public Class clsControlParams
    Dim map As Hashtable

    Public Sub New()
        map = New Hashtable()
    End Sub

    Public Sub Put(ByVal name As String, ByVal value As Double)
        map.Item(name) = value
    End Sub
    Public Function GetItem(ByVal name As String)
        Return map.Item(name)
    End Function
    Public Sub Empty()
        map.Clear()
    End Sub
    Public Function IsDefined(ByVal name As String) As Boolean
        Return map.Contains(name)
    End Function
    Public Function GetAsPacket() As String
        Dim s As String
        s = ""
        Dim en As IDictionaryEnumerator = map.GetEnumerator
        While (en.MoveNext())
            s = s + en.Key + ","
            Dim d As Double = en.Value
            s = s + Convert.ToString(d) + ","
        End While
        s = s.Substring(0, s.Length - 1)
        Return s
    End Function
End Class
