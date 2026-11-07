Public Class PageSetupLink

    Private Sub PageSetupLink_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded

        '重复加载部分
        PanBack.ScrollToHome()

        '非重复加载部分
        Static IsLoaded As Boolean = False
        If IsLoaded Then Return
        IsLoaded = True

        AniControlEnabled += 1
        Reload()
        AniControlEnabled -= 1

    End Sub
    Public Sub Reload()
        SettingService.RefreshSettings(Me)
    End Sub

    '初始化
    Public Sub Reset()
        Try
            SettingService.ResetSettings(Me)
            Logger.Info("已初始化联机页设置")
            Hint(I18n.GetString("SetupLink.MsgInit"), HintType.Green, False)
        Catch ex As Exception
            Logger.Error(ex, I18n.GetString("SetupLink.ErrInit"), LogBehavior.Alert)
        End Try
        Reload()
    End Sub

End Class
