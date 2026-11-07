Public Class PclLogger
    Inherits FileLogger

    ''' <inheritdoc/>
    Public Overrides Function Format(Text As String, Level As LogLevel, FilePath As String, Ex As Exception) As String
        Text = MyBase.Format(Text, Level, FilePath, Ex)
        Text = FilterUserName(FilterAccessToken(Text, "*"c), "*"c)
        Return Text
    End Function

    ''' <inheritdoc/>
    Public Overrides Sub HandleBehavior(RawMessage As String, FormattedMessage As String, Behavior As LogBehavior, Ex As Exception)
        If IsProgramEnding Then Return
        MyBase.HandleBehavior(RawMessage, FormattedMessage, Behavior, Ex)
        Dim BriefText = If(Ex Is Nothing, RawMessage, If(RawMessage Is Nothing, "", $"{RawMessage}：") & Ex.GetDisplay(False))
        BriefText = FilterUserName(FilterAccessToken(BriefText, "*"c), "*"c)
        Dim DetailText = If(Ex Is Nothing, RawMessage, If(RawMessage Is Nothing, "", $"{RawMessage}：") & Ex.GetDisplay(True))
        DetailText = FilterUserName(FilterAccessToken(DetailText, "*"c), "*"c)
        Select Case Behavior
            Case LogBehavior.None
                '啥也不干
            Case LogBehavior.ToastIfDebug
                If BuildType = BuildTypes.Debug OrElse ModeDebug Then Hint(I18n.GetString("PclLogger.DebugMode").Replace("%1%", BriefText), HintType.Blue, False)
            Case LogBehavior.Toast
                Hint(BriefText, HintType.Red, False)
            Case LogBehavior.Alert
                MyMsgBox(DetailText, I18n.GetString("ModMain.FeedbackLogTitle"), IsWarn:=True)
            Case LogBehavior.AlertThenFeedback
                If PageSetupSystem.IsLauncherNewest Then
                    If MyMsgBox(DetailText & vbCrLf & vbCrLf & I18n.GetString("PclLogger.SuggestFeedback"), I18n.GetString("ModMain.FeedbackLogTitle"), I18n.GetString("PclLogger.BtnFeedback"), I18n.GetString("ModEvent.BtnCancel"), IsWarn:=True) = 1 Then Feedback(False, True)
                Else
                    MyMsgBox(DetailText & vbCrLf & vbCrLf & I18n.GetString("PclLogger.SuggestUpdate"), I18n.GetString("ModMain.FeedbackLogTitle"), IsWarn:=True)
                End If
            Case LogBehavior.AlertThenCrash
                Static FirstTrigger As Boolean = True
                If FirstTrigger Then
                    '首次触发
                    FirstTrigger = False
                    If PageSetupSystem.IsLauncherNewest Then
                        If MsgBox(DetailText & vbCrLf & vbCrLf & I18n.GetString("PclLogger.SuggestFeedback"), MsgBoxStyle.Critical + MsgBoxStyle.YesNo, I18n.GetString("ModMain.FeedbackLogTitle")) = MsgBoxResult.Yes Then Feedback(False, True)
                    Else
                        MsgBox(DetailText & vbCrLf & vbCrLf & I18n.GetString("PclLogger.SuggestUpdate"), MsgBoxStyle.Critical, I18n.GetString("ModMain.FeedbackLogTitle"))
                    End If
                Else
                    '多次触发，直接使程序崩溃（这通常代表着在其他线程循环触发严重异常）
                    Thread.Sleep(2000)
                End If
                FormMain.EndProgramForce(ProcessReturnValues.Exception)
        End Select
        '遥测
        If Behavior >= LogBehavior.Toast Then Telemetry("错误日志", "Exception", DetailText)
    End Sub

End Class
