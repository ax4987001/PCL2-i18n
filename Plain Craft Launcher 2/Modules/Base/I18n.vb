Public Class I18n

    Public Shared CurrentLang As String = "en_US"
    Private Shared LangDict As New Dictionary(Of String, String)
    Public Shared Event LanguageChanged()
    
    ' 可用语言列表（从文件读取）
    Public Shared AvailableLangs As New List(Of String)()
    
    ' 内置默认语言字典（Base64，首次运行时释放）
    Private Const ZH_CN_B64 As String = "ewoJIkFwcGxpY2F0aW9uLkVycm9yIjoi5Y+R55Sf5pyq55+l6ZSZ6K+v5Y+I5peg5rOV5o2V6I63IiwKCSJBcHBsaWNhdGlvbi5Mb2dJbml0RXJyIjoi5Yid5aeL5YyW5pel5b+X5aSx6LSlIiwKCSJGb3JtTWFpbi5Mb2dJbml0UG9vbEVyciI6IuWIt+Wni+WMluS4u+e6v+eoi+axoOetieaMh+S7p+Wksei0pSIsCgkiRm9ybU1haW4uTG9nQ2xlYXJVcGRhdGVFcnIiOiLmuIXnkIboh6rWK+abtOaWsOaWh+S7tuWksei0pSIsCgkiRm9ybU1haW4uVGVsZW1ldHJ5U3RhcnQiOiLlkK/liqgiLAoJIkZvcm1NYWluLkhpbnRPcGVuU291cmNlIjoieyUxJX1QQ0wyIiwKCSJGb3JtTWFpbi5IaW50T3BlblNvdXJjZVRpdGxlIjoi5byA5rqQ54mI5pysIiwKCSJGb3JtTWFpbi5IaW50VW5sb2NrRmFuIjoi5bey6Kej6ZSB57KJ5Lid5Lyg5a6iIiwKCSJGb3JtTWFpbi5IaW50VGl0bGUiOiLmj5DnpLoiLAoJIkZvcm1NYWluLkxvZ1VwZ3JhZGUiOiLmsYflh6rotaznvqTvvJolMSAtPiAlMiIsCgkiRm9ybU1haW4uTG9nSGlnaGVzdFVwZ3JhZGUiOiLmlrDnjqnogIXvvJolMSAtPiAlMiIsCgkiU2VsZWN0UmlnaHQuVGl0bGVFbXB0eSI6IuOAkOaXoOWPr+eUqOeJiOacrOOAkSIsCgkiU2VsZWN0UmlnaHQuVGV4dEVtcHR5Ijoi5b2T5YmN5paH5Lu25aS55Lit5rKh5pyJ5Y+R546w5Y+v5Lul5Z+65pys55qEIE1pbmVjcmFmdCDniYjmnKzvvIEiLAoJIlNlbGVjdFJpZ2h0LkJ0blNldHVwIjoi6K6+572uIiwKCSJNb2RNaW5lY3JhZnQuQ3VycmVudEZvbGRlciI6IuW9k+WJjeaWh+S7tuWkuiIKfQ=="
    Private Const EN_US_B64 As String = "ewoJIkFwcGxpY2F0aW9uLkVycm9yIjoiQW4gdW5rbm93biBlcnJvciBvY2N1cnJlZCBBTkQgY291bGQgbm90IGJlIGNhdWdodC4iLAoJIkFwcGxpY2F0aW9uLkxvZ0luaXRFcnIiOiJGYWlsZWQgdG8gaW5pdGlhbGl6ZSBsb2dnaW5nIiwKCSJGb3JtTWFpbi5Mb2dJbml0UG9vbEVyciI6IkZhaWxlZCB0byBpbml0aWFsaXplIG1haW4gdGhyZWFkIHBvb2wiLAoJIkZvcm1NYWluLkxvZ0NsZWFyVXBkYXRlRXJyIjoiRmFpbGVkIHRvIGNsZWFyIGF1dG8tdXBkYXRlIGZpbGVzIiwKCSJGb3JtTWFpbi5UZWxlbWV0cnlTdGFydCI6IlN0YXJ0ZWQiLAoJIkZvcm1NYWluLkhpbnRPcGVuU291cmNlIjoieyUxJX1QQ0wyIiwKCSJGb3JtTWFpbi5IaW50T3BlblNvdXJjZVRpdGxlIjoiT3BlbiBTb3VyY2UgVmVyc2lvbiIsCgkiRm9ybU1haW4uSGludFVubG9ja0ZhbiI6IlVubG9ja2VkIGZhbiB0cmFuc21pc3Npb24iLAoJIkZvcm1NYWluLkhpbnRUaXRsZSI6IkhpbnQiLAoJIkZvcm1NYWluLkxvZ1VwZ3JhZGUiOiJVcGdyYWRlZCBmcm9tICUxIHRvICUyIiwKCSJGb3JtTWFpbi5Mb2dIaWdoZXN0VXBncmFkZSI6IkhpZ2hlc3QgcmVjb3JkZWQgdmVyc2lvbiB1cGRhdGVkOiAlMSAtPiAlMiIsCgkiU2VsZWN0UmlnaHQuVGl0bGVFbXB0eSI6IltObyBBdmFpbGFibGUgVmVyc2lvbl0iLAoJIlNlbGVjdFJpZ2h0LlRleHRFbXB0eSI6Ik5vIHBsYXlhYmxlIE1pbmVjcmFmdCB2ZXJzaW9uIGZvdW5kIGluIHRoZSBjdXJyZW50IGZvbGRlciEiLAoJIlNlbGVjdFJpZ2h0LkJ0blNldHVwIjoiU2V0dXAifQ=="

    ' 初始化并释放语言文件
    Public Shared Sub InitLangFiles()
        Dim langDir As String = Paths.Base & "PCL\Lang\"
        If Not System.IO.Directory.Exists(langDir) Then
            System.IO.Directory.CreateDirectory(langDir)
        End If
        
        Dim zhFile As String = langDir & "zh_CN.json"
        If Not System.IO.File.Exists(zhFile) Then
            System.IO.File.WriteAllText(zhFile, System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(ZH_CN_B64)))
        End If
        
        Dim enFile As String = langDir & "en_US.json"
        If Not System.IO.File.Exists(enFile) Then
            System.IO.File.WriteAllText(enFile, System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(EN_US_B64)))
        End If
    End Sub

    Public Shared Sub UpdateLanguage()
        Try
            InitLangFiles()
            
            Dim langDir As String = Paths.Base & "PCL\Lang\"
            AvailableLangs.Clear()
            If System.IO.Directory.Exists(langDir) Then
                For Each file In System.IO.Directory.GetFiles(langDir, "*.json")
                    AvailableLangs.Add(System.IO.Path.GetFileNameWithoutExtension(file))
                Next
            End If
            If AvailableLangs.Count = 0 Then
                AvailableLangs.Add("en_US")
                AvailableLangs.Add("zh_CN")
            End If

            Dim langIndex As Integer = Settings.Get(Of Integer)("UiLanguage")
            If langIndex >= 0 AndAlso langIndex < AvailableLangs.Count Then
                CurrentLang = AvailableLangs(langIndex)
            Else
                CurrentLang = "en_US"
                Settings.Set("UiLanguage", AvailableLangs.IndexOf("en_US"))
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
