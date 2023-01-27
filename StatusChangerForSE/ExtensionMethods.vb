Imports System.Runtime.CompilerServices
Imports System.Windows.Threading

Module ExtensionMethods
    Private EmptyDelegate As Action = Function()
                                      End Function

    <Extension()>
    Sub Refresh(ByVal uiElement As UIElement)
        uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate)
    End Sub
End Module
