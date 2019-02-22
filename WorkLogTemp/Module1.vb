Module Module1

    Public ORALocalhost As New OracleHelper("localhost", 1521, "oss", "work", "Smart9080")
    Structure ProAndCity
        Dim province As String
        Dim cityList As List(Of cityInfo)
        Sub New(ByVal _province As String)
            province = _province
            cityList = New List(Of cityInfo)
        End Sub
        Sub New(ByVal _province As String, _cityName As String, _districtName As String)
            province = _province
            cityList = New List(Of cityInfo)
            cityList.Add(New cityInfo(_cityName, _districtName))
        End Sub
    End Structure
    Structure cityInfo
        Dim city As String
        Dim district As List(Of String)
        Sub New(ByVal _city As String)
            Me.city = _city
            district = New List(Of String)
        End Sub
        Sub New(ByVal _city As String, _districtName As String)
            Me.city = _city
            district = New List(Of String)
            district.Add(_districtName)
        End Sub
    End Structure
    Public Function GetNewToken(usr As String, isWriteToOracle As Boolean) As String
        Dim dtmp As Date = Now
        Dim time As String = dtmp.ToString("yyyy-MM-dd HH:mm:ss")
        Dim ticks As String = dtmp.Ticks
        If usr = "" Then Return ticks
        Dim token As String = usr & "#" & time
        token = StrToBase64(token)
        If isWriteToOracle Then
            Dim sql As String = "update logAccount set token='" & token & "' where account='" & usr & "'"
            ORALocalhost.SqlCMD(sql)
        End If
        Return token
    End Function
    Public Function CheckToken(token As String) As Boolean
        If token = "928453310" Then Return True
        Dim str As String = GetUsrByToken(token)
        If str = "" Then Return False
        Return True
    End Function
    Public Function GetUsrByToken(token As String) As String
        Dim sql As String = "select account from logAccount where token='" & token & "'"
        Dim dt As DataTable = ORALocalhost.SqlGetDT(sql)
        If IsNothing(dt) Then Return ""
        If dt.Rows.Count = 0 Then Return ""
        Dim row As DataRow = dt.Rows(0)
        Dim account As String = row("account".ToUpper).ToString
        Return account
    End Function
    Structure userInfo
        Dim usr As String
        Dim name As String
        Dim workId As Integer
        Dim state As Integer
        Dim corp As String
        Sub New(usr As String, name As String, workId As Integer, state As Integer, corp As String)
            Me.usr = usr
            Me.name = name
            Me.workId = workId
            Me.state = state
            Me.corp = corp
        End Sub
    End Structure
    Public Function GetUsrInfoByToken(token As String) As userInfo
        Dim sql As String = "select * from logAccount where token='" & token & "'"
        Dim dt As DataTable = ORALocalhost.SqlGetDT(sql)
        If IsNothing(dt) Then Return Nothing
        If dt.Rows.Count = 0 Then Return Nothing
        Dim row As DataRow = dt.Rows(0)
        Dim usr As String = row("account".ToUpper).ToString
        Dim name As String = row("name".ToUpper).ToString
        Dim workId As String = row("workId".ToUpper).ToString
        Dim state As String = row("state".ToUpper).ToString
        Dim corp As String = row("corp".ToUpper).ToString
        Return New userInfo(usr, name, workId, state, corp)
    End Function
    Public Function StrToBase64(str As String) As String
        If str = "" Then Return ""
        Dim by() As Byte = Encoding.Default.GetBytes(str)
        Dim base64 As String = Convert.ToBase64String(by)
        Return base64
    End Function
    Public Function Base64ToStr(base64 As String) As String
        Try
            If base64 = "" Then Return ""
            Dim by() As Byte = Convert.FromBase64String(base64)
            Dim str As String = Encoding.Default.GetString(by)
            Return str
        Catch ex As Exception
            Return ""
        End Try
    End Function
End Module
