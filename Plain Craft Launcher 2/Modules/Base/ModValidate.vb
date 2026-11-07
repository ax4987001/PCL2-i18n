'提供不同的输入验证方法，名称以 Validate 开头
Public Module ModValidate

    ''' <summary>
    ''' 进行输入验证，并返回错误原因。
    ''' 如果没有错误则返回空字符串。
    ''' </summary>
    Public Function Validate(Text As String, ValidateRules As IEnumerable(Of Validate)) As String
        Dim Result As String = ""
        For Each ValidateRule As Validate In ValidateRules
            Result = ValidateRule.Validate(Text)
            If Result Is Nothing Then Return ""
            If Result <> "" Then Return Result
        Next
        Return Result
    End Function

End Module

''' <summary>
''' 输入验证规则基类。要查看所有的输入验证规则，可在输入 Validate 后查看自动补全。
''' </summary>
Public MustInherit Class Validate
    ''' <summary>
    ''' 验证某字符串是否符合验证要求。若符合，返回空字符串；若不符合，返回错误原因；若需要中断检查并直接通过，返回 Nothing。
    ''' </summary>
    Public MustOverride Function Validate(Str As String) As String
End Class

''' <summary>
''' 若为空则直接通过检查。
''' </summary>
Public Class ValidateNullable
    Inherits Validate
    Public Sub New()
    End Sub
    Public Overrides Function Validate(Str As String) As String
        If IsNothing(Str) OrElse String.IsNullOrEmpty(Str) Then Return Nothing
        Return ""
    End Function
End Class

''' <summary>
''' 不能为 Nothing 或空字符串（不包括全空格检查）。
''' </summary>
Public Class ValidateNullOrEmpty
    Inherits Validate
    Public Sub New()
    End Sub
    Public Overrides Function Validate(Str As String) As String
        If IsNothing(Str) OrElse String.IsNullOrEmpty(Str) Then Return I18n.GetString("ModValidate.ErrorEmpty")
        Return ""
    End Function
End Class

''' <summary>
''' 不能为 Nothing 或空字符串（包括全空格检查）。
''' </summary>
Public Class ValidateNullOrWhiteSpace
    Inherits Validate
    Public Sub New()
    End Sub
    Public Overrides Function Validate(Str As String) As String
        If IsNothing(Str) OrElse String.IsNullOrWhiteSpace(Str) Then Return I18n.GetString("ModValidate.ErrorEmpty")
        Return ""
    End Function
End Class

''' <summary>
''' 自定义的 Validate。
''' </summary>
Public Class ValidateFunc
    Inherits Validate
    Private ReadOnly _func As Func(Of String, String)
    Public Sub New(F As Func(Of String, String))
        _func = F
    End Sub
    Public Overrides Function Validate(Str As String) As String
        Return _func(Str)
    End Function
End Class

''' <summary>
''' 必须满足正则表达式。
''' </summary>
Public Class ValidateRegex
    Inherits Validate
    Public Property Regex As String
    Public Property ErrorDescription As String = I18n.GetString("ModValidate.ErrorRegex")
    Public Sub New()
    End Sub '用于 XAML 初始化
    Public Sub New(Regex As String, Optional ErrorDescription As String = "ModValidate.ErrorRegex")
        Me.Regex = Regex
        If ErrorDescription = "ModValidate.ErrorRegex" Then
            Me.ErrorDescription = I18n.GetString("ModValidate.ErrorRegex")
        Else
            Me.ErrorDescription = ErrorDescription
        End If
    End Sub
    Public Overrides Function Validate(Str As String) As String
        If Not Str.RegexCheck(Regex) Then Return ErrorDescription
        Return ""
    End Function
End Class

''' <summary>
''' 必须是一个完整网址。
''' </summary>
Public Class ValidateHttp
    Inherits Validate
    Public Sub New()
    End Sub '用于 XAML 初始化
    Public Overrides Function Validate(Str As String) As String
        If Str.EndsWithF("/") Then Str = Str.Substring(0, Str.Length - 1)
        If Not Str.RegexCheck("^(http[s]?)\://") Then Return I18n.GetString("ModValidate.ErrorHttp")
        Return ""
    End Function
End Class

''' <summary>
''' 必须为整数。
''' </summary>
Public Class ValidateInteger
    Inherits Validate
    Public Property Min As Integer
    Public Property Max As Integer = Integer.MaxValue
    Public Sub New()
    End Sub '用于 XAML 初始化
    Public Sub New(Min As Integer, Max As Integer)
        Me.Min = Min
        Me.Max = Max
    End Sub
    Public Overrides Function Validate(Str As String) As String
        If Str.Length > 9 Then Return I18n.GetString("ModValidate.ErrorLength")
        Dim Valed As Integer
        If Not Integer.TryParse(Str, Valed) Then Return I18n.GetString("ModValidate.ErrorInteger")
        If Valed > Max Then Return I18n.GetString("ModValidate.ErrorMax").Replace("%1%", Max)
        If Valed < Min Then Return I18n.GetString("ModValidate.ErrorMin").Replace("%1%", Min)
        Return ""
    End Function
End Class

''' <summary>
''' 长度限制。
''' </summary>
Public Class ValidateLength
    Inherits Validate
    Public Property Min As Integer = 0
    Public Property Max As Integer = Integer.MaxValue
    Public Sub New()
    End Sub '用于 XAML 初始化
    Public Sub New(Min As Integer, Optional Max As Integer = Integer.MaxValue)
        Me.Min = Min
        Me.Max = Max
    End Sub
    Public Overrides Function Validate(Str As String) As String
        If Len(Str) <> Max AndAlso Max = Min Then Return I18n.GetString("ModValidate.ErrorLenEq").Replace("%1%", Max)
        If Len(Str) > Max Then Return I18n.GetString("ModValidate.ErrorLenMax").Replace("%1%", Max)
        If Len(Str) < Min Then Return I18n.GetString("ModValidate.ErrorLenMin").Replace("%1%", Min)
        Return ""
    End Function
End Class

''' <summary>
''' 不能包含某些特定字符串。忽略大小写。
''' </summary>
Public Class ValidateExcept
    Inherits Validate
    Public Property Excepts As New ObjectModel.Collection(Of String)
    Public Property ErrorMessage As String
    Public Sub New()
        ErrorMessage = "ModValidate.ErrorInclude"
    End Sub '用于 XAML 初始化
    Public Sub New(Excepts As ObjectModel.Collection(Of String), Optional ErrorMessage As String = "ModValidate.ErrorInclude")
        Me.Excepts = Excepts
        Me.ErrorMessage = ErrorMessage
    End Sub
    Public Sub New(Excepts As IEnumerable(Of String), Optional ErrorMessage As String = "ModValidate.ErrorInclude")
        Me.New(
            New ObjectModel.Collection(Of String)(New List(Of String)(Excepts)),
            ErrorMessage)
    End Sub
    Public Sub New(Excepts As IEnumerable(Of Char), Optional ErrorMessage As String = "ModValidate.ErrorInclude")
        Me.New(
            New ObjectModel.Collection(Of String)(Excepts.Select(Function(c) c.ToString).ToList),
            ErrorMessage)
    End Sub
    Public Overrides Function Validate(Str As String) As String
        For Each Ch As String In Excepts
            If Str.IndexOfF(Ch, True) >= 0 Then
                If IsNothing(ErrorMessage) Then ErrorMessage = ""
                Dim TrueErrorMessage = If(ErrorMessage = "ModValidate.ErrorInclude", I18n.GetString("ModValidate.ErrorInclude"), ErrorMessage)
                Return TrueErrorMessage.Replace("%", Ch)
            End If
        Next
        Return ""
    End Function

End Class

''' <summary>
''' 不能与某些特定字符串相同。
''' </summary>
Public Class ValidateExceptSame
    Inherits Validate
    Public Property Excepts As New ObjectModel.Collection(Of String)
    Public Property ErrorMessage As String
    Public Property IgnoreCase As Boolean = False
    Public Sub New()
    End Sub
    Public Sub New(Excepts As ObjectModel.Collection(Of String), Optional ErrorMessage As String = "ModValidate.ErrorSame", Optional IgnoreCase As Boolean = False)
        Me.Excepts = Excepts
        Me.ErrorMessage = ErrorMessage
        Me.IgnoreCase = IgnoreCase
    End Sub
    Public Sub New(Excepts As IEnumerable(Of String), Optional ErrorMessage As String = "ModValidate.ErrorSame", Optional IgnoreCase As Boolean = False)
        Me.New(
            New ObjectModel.Collection(Of String)(New List(Of String)(Excepts)),
            ErrorMessage, IgnoreCase)
    End Sub
    Public Sub New(Except As String, Optional ErrorMessage As String = "ModValidate.ErrorSame", Optional IgnoreCase As Boolean = False)
        Me.New(
            New ObjectModel.Collection(Of String) From {Except},
            ErrorMessage, IgnoreCase)
    End Sub
    Public Overrides Function Validate(Str As String) As String
        Dim TrueErrorMessage = If(ErrorMessage = "ModValidate.ErrorSame", I18n.GetString("ModValidate.ErrorSame"), ErrorMessage)
        If Str Is Nothing Then Return TrueErrorMessage.Replace("%", "null")
        For Each Ch As String In Excepts
            If IgnoreCase Then
                If Str.Lower = Ch.Lower Then Return TrueErrorMessage.Replace("%", Ch)
            Else
                If Str.Equals(Ch) Then Return TrueErrorMessage.Replace("%", Ch) '使用 = 不确定是否会忽略大小写
            End If
        Next
        Return ""
    End Function

End Class

''' <summary>
''' 必须为特定字符串。
''' </summary>
Public Class ValidateSame
    Inherits Validate
    Public Property Required As String
    Public Property ErrorMessage As String
    Public Property IgnoreCase As Boolean = False
    Public Sub New()
    End Sub
    Public Sub New(Required As String, Optional ErrorMessage As String = "ModValidate.ErrorNotSame", Optional IgnoreCase As Boolean = False)
        Me.Required = Required
        If ErrorMessage = "ModValidate.ErrorNotSame" Then
            Me.ErrorMessage = I18n.GetString("ModValidate.ErrorNotSame")
        Else
            Me.ErrorMessage = ErrorMessage
        End If
        Me.IgnoreCase = IgnoreCase
    End Sub
    Public Overrides Function Validate(Str As String) As String
        Return If(String.Compare(Str, Required, IgnoreCase) = 0, "", ErrorMessage)
    End Function

End Class

''' <summary>
''' 对文件夹名的粗略的特化检测。
''' </summary>
Public Class ValidateFolderName
    Inherits Validate
    Public Property Folder As String
    Public Property UseMinecraftCharCheck As Boolean = True
    Private ReadOnly PathIgnore As IEnumerable(Of String)
    Private ReadOnly IgnoreList As List(Of String)
    Public Sub New()
    End Sub
    Public Sub New(Folder As String, Optional UseMinecraftCharCheck As Boolean = True, Optional IgnoreList As List(Of String) = Nothing)
        Me.Folder = Folder
        Me.UseMinecraftCharCheck = UseMinecraftCharCheck
        On Error Resume Next
        PathIgnore = DirectoryUtils.EnumerateDirectories(Folder)
        Me.IgnoreList = IgnoreList
    End Sub
    Public Overrides Function Validate(Str As String) As String
        Try
            '检查是否为空
            Dim LengthCheck As String = New ValidateNullOrWhiteSpace().Validate(Str)
            If LengthCheck <> "" Then Return LengthCheck
            '检查空格
            If Str.StartsWithF(" ") Then Return I18n.GetString("ModValidate.ErrorFolderNameStart")
            If Str.EndsWithF(" ") Then Return I18n.GetString("ModValidate.ErrorFolderNameEnd")
            '检查长度
            LengthCheck = New ValidateLength(1, 100).Validate(Str)
            If LengthCheck <> "" Then Return LengthCheck
            '检查尾部小数点
            If Str.EndsWithF(".") Then Return I18n.GetString("ModValidate.ErrorFolderNameDot")
            '检查特殊字符
            Dim CharactCheck As String = New ValidateExcept(Path.GetInvalidFileNameChars() & If(UseMinecraftCharCheck, "!;", ""), I18n.GetString("ModValidate.ErrorFolderNameChar")).Validate(Str)
            If CharactCheck <> "" Then Return CharactCheck
            '检查特殊字符串
            Dim InvalidStrCheck As String = New ValidateExceptSame({"CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"}, I18n.GetString("ModValidate.ErrorFolderNameSame"), True).Validate(Str)
            If InvalidStrCheck <> "" Then Return InvalidStrCheck
            '检查 NTFS 8.3 文件名（#4505）
            If Str.RegexCheck(".{2,}~\d") Then Return I18n.GetString("ModValidate.ErrorFolderNameRegex")
            '检查文件夹重名
            Dim Arr As New List(Of String)
            Dim folderName As String
            If PathIgnore IsNot Nothing Then
                For Each FolderPath In PathIgnore
                    folderName = PathUtils.GetLastPart(FolderPath)
                    If IgnoreList Is Nothing OrElse Not IgnoreList.Contains(folderName, StringComparer.OrdinalIgnoreCase) Then Arr.Add(folderName)
                Next
            End If
            Dim SameNameCheck = New ValidateExceptSame(Arr, I18n.GetString("ModValidate.ErrorFolderNameExist"), True).Validate(Str)
            If Not SameNameCheck = "" Then Return SameNameCheck
            Return ""
        Catch ex As Exception
            Logger.Warn(ex, "检查文件夹名出错")
            Return I18n.GetString("ModValidate.ErrorFormat").Replace("%1%", ex.Message)
        End Try
    End Function
End Class

''' <summary>
''' 对文件名的粗略的特化检测。
''' </summary>
Public Class ValidateFileName
    Inherits Validate
    Public Property Name As String
    Public Property UseMinecraftCharCheck As Boolean = True
    Public Property IgnoreCase As Boolean = True
    Public Property ParentFolder As String = Nothing
    Public Property RequireParentFolderExists = True
    Public Sub New()
    End Sub
    Public Sub New(Name As String, Optional UseMinecraftCharCheck As Boolean = True, Optional IgnoreCase As Boolean = True)
        Me.Name = Name
        Me.IgnoreCase = IgnoreCase
        Me.UseMinecraftCharCheck = UseMinecraftCharCheck
    End Sub
    Public Overrides Function Validate(Str As String) As String
        Try
            '检查是否为空
            Dim LengthCheck As String = New ValidateNullOrWhiteSpace().Validate(Str)
            If Not LengthCheck = "" Then Return LengthCheck
            '检查空格
            If Str.StartsWithF(" ") Then Return I18n.GetString("ModValidate.ErrorFileNameStart")
            If Str.EndsWithF(" ") Then Return I18n.GetString("ModValidate.ErrorFileNameEnd")
            '检查长度
            LengthCheck = New ValidateLength(1, 100).Validate(Str)
            If Not LengthCheck = "" Then Return LengthCheck
            '检查尾部小数点
            If Str.EndsWithF(".") Then Return I18n.GetString("ModValidate.ErrorFileNameDot")
            '检查特殊字符
            Dim CharactCheck As String = New ValidateExcept(Path.GetInvalidFileNameChars() & If(UseMinecraftCharCheck, "!;", ""), I18n.GetString("ModValidate.ErrorFileNameChar")).Validate(Str)
            If Not CharactCheck = "" Then Return CharactCheck
            '检查特殊字符串
            Dim InvalidStrCheck As String = New ValidateExceptSame({"CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"}, I18n.GetString("ModValidate.ErrorFileNameSame"), True).Validate(Str)
            If Not InvalidStrCheck = "" Then Return InvalidStrCheck
            '检查 NTFS 8.3 文件名（#4505）
            If Str.RegexCheck(".{2,}~\d") Then Return I18n.GetString("ModValidate.ErrorFileNameRegex")
            '检查文件重名
            If ParentFolder IsNot Nothing Then
                Dim SameNameCheck = New ValidateExceptSame(DirectoryUtils.EnumerateFiles(ParentFolder).Select(Function(f) IO.Path.GetFileName(f)),
                                                           I18n.GetString("ModValidate.ErrorFileNameExist"), IgnoreCase).Validate(Str)
                If Not SameNameCheck = "" Then Return SameNameCheck
            End If
            If RequireParentFolderExists Then Return I18n.GetString("ModValidate.ErrorParentFolder").Replace("%1%", ParentFolder)
            Return ""
        Catch ex As Exception
            Logger.Warn(ex, "检查文件名出错")
            Return I18n.GetString("ModValidate.ErrorFormat").Replace("%1%", ex.Message)
        End Try
    End Function
End Class

''' <summary>
''' 要求输入一个可用的文件夹路径。
''' </summary>
Public Class ValidateFolderPath
    Inherits Validate
    Public Property UseMinecraftCharCheck As Boolean = True
    Public Sub New()
    End Sub
    Public Sub New(UseMinecraftCharCheck As Boolean)
        Me.UseMinecraftCharCheck = UseMinecraftCharCheck
    End Sub
    Public Overrides Function Validate(Str As String) As String
        '去除尾部斜线，统一为 \
        Str = Str.Replace("/", "\")
        If Not PathUtils.RemoveSlashSuffix(Str).EndsWithF(":") Then Str = PathUtils.RemoveSlashSuffix(Str)
        '检查是否为空
        Dim LengthCheck As String = New ValidateNullOrWhiteSpace().Validate(Str)
        If LengthCheck <> "" Then Return LengthCheck
        '检查长度
        LengthCheck = New ValidateLength(1, 254).Validate(Str)
        If LengthCheck <> "" Then Return LengthCheck
        '检查 NTFS 8.3 文件名（#4505）
        Dim Match = Str.RegexSeek(".{2,}~\d")
        If Match IsNot Nothing Then Return I18n.GetString("ModValidate.ErrorFolderNameRegex2").Replace("%1%", Match)
        '检查开头
        If Str.StartsWithF("\\Mac\") Then GoTo Fin
        For Each Drive In Environment.GetLogicalDrives().Where(Function(p) DirectoryUtils.Exists(p))
            Dim DriveName As String = Drive.Upper.First & ":\"
            If Str.Upper = DriveName Then Return ""
            If Str.StartsWithF(DriveName, True) Then GoTo Fin
        Next
        Return I18n.GetString("ModValidate.ErrorFolderPathHead")
Fin:
        '对首层以外的路径检查
        For i = If(Str.StartsWithF("\\Mac\"), 2, 1) To Str.Split("\").Count - 1
            Dim SubStr As String = Str.Split("\")(i)
            '检查是否为空
            Dim SubLengthCheck As String = New ValidateNullOrWhiteSpace().Validate(SubStr)
            If Not SubLengthCheck = "" Then Return I18n.GetString("ModValidate.ErrorFolderPathLen")
            '检查特殊字符
            Dim CharactCheck As String = New ValidateExcept(Path.GetInvalidFileNameChars() & If(UseMinecraftCharCheck, "!;", ""), I18n.GetString("ModValidate.ErrorFolderPathChar2")).Validate(SubStr)
            If Not CharactCheck = "" Then Return CharactCheck
            '检查头部空格
            If SubStr.StartsWithF(" ") Then Return I18n.GetString("ModValidate.ErrorFolderNameStart")
            If SubStr.EndsWithF(" ") Then Return I18n.GetString("ModValidate.ErrorFolderNameEnd")
            '检查尾部小数点
            If SubStr.EndsWithF(".") Then Return I18n.GetString("ModValidate.ErrorFolderNameDot")
            '检查特殊字符串
            Dim InvalidStrCheck As String = New ValidateExceptSame({"CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"}, I18n.GetString("ModValidate.ErrorFolderNameSame")).Validate(SubStr)
            If Not InvalidStrCheck = "" Then Return InvalidStrCheck
        Next
        Return ""
    End Function
End Class
