Public Class I18n

    Public Shared CurrentLang As String = "zh_CN"
    Private Shared LangDict As New Dictionary(Of String, String)
    Public Shared Event LanguageChanged()

    Public Shared Sub UpdateLanguage()
        Try
            Dim langIndex As Integer = Settings.Get(Of Integer)("UiLanguage")
            If langIndex = 0 Then
                CurrentLang = "zh_CN"
            ElseIf langIndex = 1 Then
                CurrentLang = "en_US"
            Else
                CurrentLang = "zh_CN"
            End If

            LangDict.Clear()
            Dim langFile As String = Paths.Base & "PCL\Lang\" & CurrentLang & ".json"
            If FileUtils.Exists(langFile) Then
                Dim jsonContent As String = FileUtils.ReadAsString(langFile)
                Dim parsed As JObject = jsonContent.DeserializeJson()
                For Each prop In parsed.Properties()
                    LangDict(prop.Name) = prop.Value.ToString()
                    Application.Current.Resources(prop.Name) = prop.Value.ToString()
                Next
            End If

            Logger.Info("Language updated to " & CurrentLang & ", keys: " & LangDict.Count)
            RaiseEvent LanguageChanged()
        Catch ex As Exception
            Logger.Error(ex, "Failed to update language")
        End Try
    End Sub

    Public Shared Function GetString(key As String, Optional defaultStr As String = "") As String
        If LangDict.ContainsKey(key) Then
            Return LangDict(key)
        End If
        If Not String.IsNullOrEmpty(defaultStr) Then
            Return defaultStr
        End If
        Return key
    End Function

End Class
