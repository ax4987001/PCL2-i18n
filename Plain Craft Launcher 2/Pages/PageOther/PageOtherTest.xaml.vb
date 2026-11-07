Imports System.IO
Imports System.Runtime.InteropServices

Public Class PageOtherTest

    <DllImport("psapi.dll")>
    Private Shared Function EmptyWorkingSet(hProcess As IntPtr) As Integer
    End Function

    Private Sub RubbishClear_Click(sender As Object, e As MouseButtonEventArgs)
        RunInNewThread(Sub() RubbishClear(), "RubbishClear")
    End Sub

    Private Sub MemoryOptimize_Click(sender As Object, e As MouseButtonEventArgs)
        RunInNewThread(Sub() MemoryOptimize(True), "MemoryOptimize")
    End Sub

    Private Sub DownloadCustom_Click(sender As Object, e As MouseButtonEventArgs)
        Dim url As String = TextUrl.Text.Trim()
        If String.IsNullOrWhiteSpace(url) Then
            Hint(I18n.GetString("PageOtherTest.MsgNoUrl"))
            Return
        End If
        Dim fileName As String = TextFileName.Text.Trim()
        If String.IsNullOrWhiteSpace(fileName) Then
            Hint(I18n.GetString("PageOtherTest.MsgNoFile"))
            Return
        End If
        Dim folder As String = TextFolder.Text.Trim()
        StartCustomDownload(url, fileName, folder)
    End Sub

    Private Sub BrowseFolder_Click(sender As Object, e As MouseButtonEventArgs)
        Dim dialog As New Forms.FolderBrowserDialog()
        dialog.Description = I18n.GetString("PageOtherTest.MsgSelectFolder")
        If Not String.IsNullOrWhiteSpace(TextFolder.Text) AndAlso Directory.Exists(TextFolder.Text) Then
            dialog.SelectedPath = TextFolder.Text
        End If
        If dialog.ShowDialog() = Forms.DialogResult.OK Then
            TextFolder.Text = dialog.SelectedPath
        End If
    End Sub

    Private Sub OpenFolder_Click(sender As Object, e As MouseButtonEventArgs)
        Dim folder As String = TextFolder.Text.Trim()
        If String.IsNullOrWhiteSpace(folder) Then
            folder = Path.Combine(Paths.Base, "PCL", "CustomDownloads")
        End If
        If Directory.Exists(folder) Then
            Process.Start("explorer.exe", folder)
        Else
            Hint(I18n.GetString("PageOtherTest.MsgNoFolder"))
        End If
    End Sub

    Public Shared Sub StartCustomDownload(Url As String, FileName As String, Optional Folder As String = Nothing)
        Try
            If String.IsNullOrWhiteSpace(Folder) Then
                Folder = Path.Combine(Paths.Base, "PCL", "CustomDownloads")
            End If
            If Not Directory.Exists(Folder) Then Directory.CreateDirectory(Folder)
            Dim destPath = Path.Combine(Folder, FileName)
            
            Hint(I18n.GetString("PageOtherTest.MsgStartDownload") & FileName)
            RunInNewThread(
                Sub()
                    Try
                        NetDownloadByLoader(Url, destPath)
                        Hint(I18n.GetString("PageOtherTest.MsgDownloadDone") & FileName)
                    Catch ex As Exception
                        Logger.Error(ex, "自定义下载失败")
                        Hint(I18n.GetString("PageOtherTest.MsgDownloadFail") & ex.Message)
                    End Try
                End Sub, "CustomDownload")
        Catch ex As Exception
            Logger.Error(ex, "自定义下载异常")
            Hint(I18n.GetString("PageOtherTest.MsgDownloadError") & ex.Message)
        End Try
    End Sub

    Public Shared Sub Jrrp()
        Hint(I18n.GetString("PageOtherTest.HintNoTreasureBox"))
    End Sub

    Public Shared Sub RubbishClear()
        Try
            Dim sizeCleared As Long = 0
            ' 清理日志和崩溃报告
            Dim dirsToClean As String() = {"logs", "crash-reports"}
            For Each dirName In dirsToClean
                Dim targetDir As String = Path.Combine(ModMinecraft.McFolderSelected, dirName)
                If Directory.Exists(targetDir) Then
                    For Each file In Directory.GetFiles(targetDir)
                        Try
                            Dim info As New FileInfo(file)
                            sizeCleared += info.Length
                            info.Delete()
                        Catch ex As Exception
                        End Try
                    Next
                End If
            Next
            
            Dim mbCleared As Double = sizeCleared / 1024 / 1024
            Hint(String.Format(I18n.GetString("PageOtherTest.MsgClearDone"), Math.Round(mbCleared, 2)))
        Catch ex As Exception
            Logger.Error(ex, "清理游戏垃圾失败")
            Hint(I18n.GetString("PageOtherTest.MsgClearFail") & ex.Message)
        End Try
    End Sub

    Public Shared Sub MemoryOptimize(ShowHint As Boolean)
        Try
            MemoryOptimizeInternal(ShowHint)
        Catch ex As Exception
            Logger.Error(ex, "内存优化失败")
            If ShowHint Then Hint(I18n.GetString("PageOtherTest.MsgOptFail") & ex.Message)
        End Try
    End Sub

    Public Shared Sub MemoryOptimizeInternal(ShowHint As Boolean)
        Try
            Dim pList = Process.GetProcesses()
            For Each p In pList
                Try
                    EmptyWorkingSet(p.Handle)
                Catch ex As Exception
                End Try
            Next
            If ShowHint Then Hint(I18n.GetString("PageOtherTest.MsgOptDone"))
        Catch ex As Exception
            Logger.Error(ex, "执行内存优化失败")
            If ShowHint Then Hint(I18n.GetString("PageOtherTest.MsgOptError"))
        End Try
    End Sub

    Public Shared Function GetRandomCave() As String
        Return I18n.GetString("PageOtherTest.HintNoTreasureBox")
    End Function

    Public Shared Function GetRandomHint() As String
        Return I18n.GetString("PageOtherTest.HintNoTreasureBox")
    End Function

    Public Shared Function GetRandomPresetHint() As String
        Return I18n.GetString("PageOtherTest.HintNoTreasureBox")
    End Function

End Class
