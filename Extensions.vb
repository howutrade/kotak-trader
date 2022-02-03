Imports System.Reflection
Imports System.Runtime.CompilerServices

Module Extensions
    <Extension()>
    Sub DoubleBuffered(ByVal Dgv As DataGridView, ByVal Setting As Boolean)
        Dim DgvType As Type = Dgv.[GetType]()
        Dim Pi As PropertyInfo = DgvType.GetProperty("DoubleBuffered", BindingFlags.Instance Or BindingFlags.NonPublic)
        Pi.SetValue(Dgv, Setting, Nothing)
    End Sub

    <Extension()>
    Sub DoubleBuffered(ByVal p As Panel, ByVal Setting As Boolean)
        Dim pType As Type = p.[GetType]()
        Dim Pi As PropertyInfo = pType.GetProperty("DoubleBuffered", BindingFlags.Instance Or BindingFlags.NonPublic)
        Pi.SetValue(p, Setting, Nothing)
    End Sub

    <Extension()>
    Sub ResizeRedraw(ByVal p As Panel, ByVal Setting As Boolean)
        Dim pType As Type = p.[GetType]()
        Dim Pi As PropertyInfo = pType.GetProperty("ResizeRedraw", BindingFlags.Instance Or BindingFlags.NonPublic)
        Pi.SetValue(p, Setting, Nothing)
    End Sub
End Module
