Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.Win32
Imports System.Windows.Threading

Class MainWindow
    Implements INotifyPropertyChanged

    Private filesList As New List(Of String)
    Public Event PropertyChanged As PropertyChangedEventHandler _
        Implements INotifyPropertyChanged.PropertyChanged

    ' This method is called by the Set accessor of each property.
    ' The CallerMemberName attribute that is applied to the optional propertyName
    ' parameter causes the property name of the caller to be substituted as an argument.
    Private Sub NotifyPropertyChanged(<CallerMemberName()> Optional ByVal propertyName As String = Nothing)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Private myStatus As String
    Public Property SetStatus() As String
        Get
            Return myStatus
        End Get
        Set(ByVal value As String)
            If Not (value = myStatus) Then
                myStatus = value
                Dispatcher.Invoke(Sub()

                                      lblStatus.Content = value

                                  End Sub, DispatcherPriority.Send)
                NotifyPropertyChanged()
                lblStatus.Refresh

            End If
        End Set
    End Property


    Public Sub New()
        Me.Left = My.Settings.FormLeft
        Me.Top = My.Settings.FormTop
        Me.DataContext = Me
        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    Private Sub SetFileStatus()
        Dim objPropertySets As RevisionManager.PropertySets = Nothing
        Dim objProperties As RevisionManager.Properties = Nothing
        Dim objProperty As RevisionManager.Property = Nothing

        Dim application As RevisionManager.Application = Nothing
        Dim document As RevisionManager.Document = Nothing

        Try
            Dim logFolder As String = IO.Path.Combine(My.Computer.FileSystem.SpecialDirectories.CurrentUserApplicationData, "StatusChangerForSE")
            If Not IO.Directory.Exists(logFolder) Then
                IO.Directory.CreateDirectory(logFolder)
            End If

            If Not String.IsNullOrEmpty(My.Settings.LogFolder) AndAlso IO.Directory.Exists(My.Settings.LogFolder) Then
                logFolder = My.Settings.LogFolder
            End If
            Dim flname As String = Environment.UserName & String.Format("_{0:yyyy_MM_dd_hh_mm}.txt", DateTime.Now)
            Dim logFile = IO.Path.Combine(logFolder, flname)

            Me.Cursor = Cursors.Wait
            SetStatus = "Starting Revision Manager..."

            application = New RevisionManager.Application()
            objPropertySets = CType(application.PropertySets, RevisionManager.PropertySets)

            For Each fl In filesList
                objPropertySets.Open(fl, False)
                objProperties = objPropertySets.Item("ExtendedSummaryInformation")
                objProperty = objProperties.Item("Status")
                If rdoAvailable.IsChecked Then
                    If objProperty.Value <> RevisionManager.DocumentStatus.igStatusAvailable Then
                        objProperty.Value = RevisionManager.DocumentStatus.igStatusAvailable
                        objPropertySets.Save()
                        My.Computer.FileSystem.WriteAllText(logFile, fl & "," & objProperty.Value, True)
                    End If
                ElseIf rdoInReview.IsChecked Then
                    If objProperty.Value <> RevisionManager.DocumentStatus.igStatusInReview Then
                        objProperty.Value = RevisionManager.DocumentStatus.igStatusInReview
                        objPropertySets.Save()
                        My.Computer.FileSystem.WriteAllText(logFile, fl & "," & objProperty.Value, True)
                    End If
                ElseIf rdoInWork.IsChecked Then
                    If objProperty.Value <> RevisionManager.DocumentStatus.igStatusInWork Then
                        objProperty.Value = RevisionManager.DocumentStatus.igStatusInWork
                        objPropertySets.Save()
                        My.Computer.FileSystem.WriteAllText(logFile, fl & "," & objProperty.Value, True)
                    End If
                ElseIf rdoObsolete.IsChecked Then
                    If objProperty.Value <> RevisionManager.DocumentStatus.igStatusObsolete Then
                        objProperty.Value = RevisionManager.DocumentStatus.igStatusObsolete
                        objPropertySets.Save()
                        My.Computer.FileSystem.WriteAllText(logFile, fl & "," & objProperty.Value, True)
                    End If
                ElseIf rdoReleased.IsChecked Then
                    If objProperty.Value <> RevisionManager.DocumentStatus.igStatusReleased Then
                        objProperty.Value = RevisionManager.DocumentStatus.igStatusReleased
                        objPropertySets.Save()
                        My.Computer.FileSystem.WriteAllText(logFile, fl & "," & objProperty.Value, True)
                    End If
                End If

                objPropertySets.Close()
                SetStatus = fl
            Next

            application.Quit()
            SetStatus = "Done"
        Catch ex As System.Exception
            SetStatus = ex.Message
        Finally
            Me.Cursor = Cursors.Arrow
            ' Release COM Objects.
            If Not (objProperties Is Nothing) Then
                Marshal.ReleaseComObject(objProperties)
                objProperties = Nothing
            End If
            If Not (objPropertySets Is Nothing) Then
                Marshal.ReleaseComObject(objPropertySets)
                objPropertySets = Nothing
            End If
            If Not (application Is Nothing) Then
                Marshal.ReleaseComObject(application)
                application = Nothing
            End If
        End Try

    End Sub

    Private Sub btnBrowse_Click(sender As Object, e As RoutedEventArgs)
        Dim fb As New OpenFileDialog
        With fb
            .Filter = "par|*.par|asm|*.asm|dft|*.dft|psm|*.psm|pwd|*.pwd"
            .Multiselect = True
            .CheckPathExists = True
            If IO.Directory.Exists(My.Settings.LastPath) Then
                .InitialDirectory = My.Settings.LastPath
            End If
            If .ShowDialog() Then
                txtFilesList.Clear()
                filesList.Clear()
                For Each f In .FileNames
                    filesList.Add(f)
                    txtFilesList.AppendText(f & Environment.NewLine)
                Next
                btnProcess.IsEnabled = .FileNames.Length > 0
            End If
        End With
    End Sub

    Private Sub btnProcess_Click(sender As Object, e As RoutedEventArgs)
        If filesList.Count > 0 Then
            SetFileStatus()
        End If
    End Sub

    Private Sub MainWindow_Closing(sender As Object, e As CancelEventArgs) Handles Me.Closing
        My.Settings.FormTop = Me.Top
        My.Settings.FormLeft = Me.Left
        My.Settings.Save()
    End Sub

    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        SetStatus = "Select files"

    End Sub

    Private Sub OnCheck(sender As Object, e As RoutedEventArgs) Handles rdoAvailable.Checked, rdoInWork.Checked, rdoInReview.Checked, rdoObsolete.Checked, rdoReleased.Checked
        SetStatus = sender.Tooltip
    End Sub
End Class
