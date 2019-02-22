Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.IO
Imports System.Math
Imports System.Net.Sockets
Imports System.Net
Imports System.Net.HttpListener
Imports System.Data
Imports System.Threading
Imports System.Threading.Thread
Imports System
Imports System.Int32
Imports System.BitConverter
Imports Newtonsoft
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports System.Xml
Imports System.Web
Imports System.Reflection
Imports System.IO.Compression
Imports OfficeOpenXml


Public Class HTTPHandle
    Structure loginInfo
        Dim usr As String
        Dim name As String
        Dim token As String
        Sub New(usr As String, name As String, token As String)
            Me.usr = usr
            Me.name = name
            Me.token = token
        End Sub
    End Structure
    Structure workLogInfo

        Dim day As String
        Dim name As String
        Dim project As String
        Dim WorkContent As String
        Dim city As String
        Dim issue As String
        Dim modifiedBy As String
        Dim modifiedDate As String
        Dim corp As String
        Dim account As String
        Dim files As List(Of workFileInfo)
    End Structure
    Structure workFileInfo
        Dim fileName As String
        Dim base64 As String
    End Structure
    Structure workFileInfo2
        Dim fileName As String
        Dim url As String
        Sub New(fileName As String, url As String)
            Me.fileName = fileName
            Me.url = url
        End Sub
    End Structure

    Structure accountInfo

        Dim workID As String
        Dim account As String
        Dim password As String
        Dim name As String
        Dim corp As String
        Dim state As String '是否正常

    End Structure
    Structure logProjectInfo
        Dim project As String
        Dim corp As String
        Dim state As String '0则为关闭

    End Structure

    Structure weekLogInfo

        Dim day As String
        Dim name As String
        Dim account As String
        Dim project As String
        Dim worksum As String
        Dim problem As String
        Dim plan As String
        Dim advise As String
        Dim city As String
        Dim issue As String
        Dim corp As String
        Dim memo As String
        Dim case1 As Object
        Dim case2 As Object
        Dim case3 As Object
        Dim modifiedBy As String
        Dim modifiedDate As String

    End Structure
    '用户登陆
    Public Function Handle_login(ByVal context As HttpContext) As NormalResponse '用户登陆
        Try
            Dim account As String = context.Request.QueryString("usr")
            Dim passWord As String = context.Request.QueryString("pwd")
            If account = "" Then Return New NormalResponse(False, "用户名为空")
            If passWord = "" Then Return New NormalResponse(False, "密码为空")
            Dim sql As String = "select password,token,name from LogAccount where account='" & account & "' and state<>0" ' And " passWord ='" & passWord & "'"
            Dim dt As DataTable = ORALocalhost.SqlGetDT(sql)
            If IsNothing(dt) Then Return New NormalResponse(False, "用户名或密码错误")
            If dt.Rows.Count = 0 Then Return New NormalResponse(False, "用户名或密码错误")
            Dim row As DataRow = dt.Rows(0)
            Dim OraPwd As String = row("password".ToUpper).ToString
            Dim OraToken As String = row("token".ToUpper).ToString
            Dim oraName As String = row("name".ToUpper).ToString
            If IsDBNull(OraToken) Then OraToken = ""
            If OraPwd = passWord Then
                If OraToken = "" Then
                    OraToken = GetNewToken(account, True)
                End If
                Dim linfo As New loginInfo(account, oraName, OraToken)
                Return New NormalResponse(True, "success", "", linfo)
            Else
                Return New NormalResponse(False, "用户名或密码错误", "", "")
            End If
        Catch ex As Exception
            Return New NormalResponse(False, ex.ToString)
        End Try
    End Function
    Public Function Handle_Test(ByVal context As HttpContext) As NormalResponse '测试
        Return New NormalResponse(True, "WorkLogTemp网络测试成功！这是返回处理信息", "这里返回错误信息", "这里返回数据")
    End Function

    Public Function Handle_UpdateWeekLog(ByVal context As HttpContext) As NormalResponse '更新工作周报
        Dim Stepp As Single = 0
        Try            '  day name	account	project	worksum	problem	plan	advise	city	issue	corp	memo	case1	case2	case3	modifiedby	modifieddate

            Dim name As String = context.Request.QueryString("name")
            Dim day As String = context.Request.QueryString("day")
            Dim corp As String = context.Request.QueryString("corp")
            Dim worksum As String = context.Request.QueryString("worksum")
            Dim problem As String = context.Request.QueryString("problem")
            Dim plan As String = context.Request.QueryString("plan")
            Dim advise As String = context.Request.QueryString("advise")
            Dim issue As String = context.Request.QueryString("issue")
            Dim project As String = context.Request.QueryString("project")
            Dim city As String = context.Request.QueryString("city")

            Dim case1 As Object = context.Request.QueryString("case1")
            Dim case2 As Object = context.Request.QueryString("case2")
            Dim case3 As Object = context.Request.QueryString("case3")
            'Stepp = 1
            name = Trim(name) : day = Trim(day)
            If name = "" Then Return New NormalResponse(False, "必须输入名字")
            If day = "" Then Return New NormalResponse(False, "必须输入日期")

            Dim Cond As String = "", sql As String


            If corp <> "" Then Cond = " corp='" & corp & "'"
            If worksum <> "" Then Cond = IIf(Cond.Length = 0, "", Cond & " , ") & " worksum ='" & worksum & "'"
            If issue <> "" Then Cond = IIf(Cond.Length = 0, "", Cond & " , ") & " issue ='" & issue & "'"
            If project <> "" Then Cond = IIf(Cond.Length = 0, "", Cond & " , ") & " project ='" & project & "'"
            If city <> "" Then Cond = IIf(Cond.Length = 0, "", Cond & " , ") & " city ='" & city & "'"
            If Not (case1 Is Nothing) Then Cond = IIf(Cond.Length = 0, "", Cond & " , ") & " case1 ='" & case1 & "'"
            'If city <> "" Then Cond = IIf(Cond = 0, "", " and ") & " city ='" & city & "'"

            If Cond.Length = 0 Then
                Return New NormalResponse(False, "WeekLog没有任何更新", name, "")
            End If
            sql = "update Weeklog set " & Cond & " where memo='" & day & name & "'"

            Stepp = 3
            Dim result As String = ORALocalhost.SqlCMD(sql)

            If result = "success" Then
                Return New NormalResponse(True, "更新 " & Cond & " 成功！")
            Else
                Return New NormalResponse(False, result)
            End If

            Return New NormalResponse(True, "", "", "")
        Catch ex As Exception
            Return New NormalResponse(False, "UpdateWeekLog Err:" & ex.Message & ",Step=" & Stepp)
        End Try
    End Function
    '获取周报
    Public Function Handle_GetWeekLog(ByVal context As HttpContext) As NormalResponse '获取工作周报
        Dim Stepp As Single = 0
        Try
            Dim name As String = context.Request.QueryString("name")
            Dim day As String = context.Request.QueryString("day")
            Dim corp As String = context.Request.QueryString("corp")

            'Stepp = 1
            name = Trim(name) : day = Trim(day)
            If name = "" Then Return New NormalResponse(False, "必须输入名字")
            If day = "" Then Return New NormalResponse(False, "必须输入日期")
            'If carrier <> "中国移动" And carrier <> "中国联通" And carrier <> "中国电信" Then Return New NormalResponse(False, "运营商错误")

            Dim sql As String = "select project,workSum,problem,plan,advise,issue,city,corp,case1,case2,case3 from Weeklog  where memo='" & day & name & "'"

            Stepp = 3
            Dim dt As DataTable = ORALocalhost.SqlGetDT(sql)
            If IsNothing(dt) Then
                Return New NormalResponse(False, "WeekLog没有任何数据nothing", sql, "")
            End If
            If dt.Rows.Count = 0 Then
                Return New NormalResponse(False, "WeekLog没有任何数据", sql, "")
            End If
            Stepp = 4
            ' "select province,city,district,netType,GDlon,GDlat,RSRP,time,SINR,eNodeBId,CellId from SDKTABLE"
            'Carrier,province,city,district,netType,GDlon,GDlat,RSRP,time,SINR
            dt.Columns(0).ColumnName = "project"            ' dt.Columns(1).ColumnName = "carrier"
            dt.Columns(1).ColumnName = "weekSum"
            dt.Columns(2).ColumnName = "problem"
            dt.Columns(3).ColumnName = "plan"
            dt.Columns(3).ColumnName = "advise"
            dt.Columns(3).ColumnName = "issue"
            dt.Columns(3).ColumnName = "city"
            dt.Columns(3).ColumnName = "corp"
            dt.Columns(3).ColumnName = "case1"
            dt.Columns(3).ColumnName = "case2"
            dt.Columns(3).ColumnName = "case3"
            'Stepp = 5
            Return New NormalResponse(True, "", "", dt)
        Catch ex As Exception
            Return New NormalResponse(False, "GetWeekLog Err:" & ex.Message & ",Step=" & Stepp)
        End Try
    End Function
    '上传周报
    Public Function Handle_UploadWeekLog(context As HttpContext, data As Object, token As String) As NormalResponse '保存WeekLog
        Dim Stepp As Integer = -1
        Try
            Dim str As String = JsonConvert.SerializeObject(data)
            Stepp = 0
            Try
                Dim DtList As List(Of weekLogInfo) = JsonConvert.DeserializeObject(str, GetType(List(Of weekLogInfo)))
                If IsNothing(DtList) Then
                    Return New NormalResponse(False, "weekLogList is null")
                End If
                If DtList.Count = 0 Then
                    Return New NormalResponse(False, "weekLogList count =0")
                End If
                Stepp = 1
                Dim colList() As String = GetOraTableColumns("weekLog")
                Dim dt As New DataTable
                For Each col In colList
                    If col <> "ID" Then
                        dt.Columns.Add(col)
                    End If
                Next
                Stepp = 2
                'day	name	account	project	worksum	problem	plan	advise	city	issue	corp	memo	case1	case2	case3	modifiedby	modifieddate

                For Each itm In DtList
                    Dim row As DataRow = dt.NewRow
                    row("day".ToUpper) = itm.day
                    row("name".ToUpper) = itm.name
                    row("account".ToUpper) = itm.account
                    row("project".ToUpper) = itm.project
                    row("worksum".ToUpper) = itm.worksum
                    row("problem".ToUpper) = itm.problem
                    row("plan".ToUpper) = itm.plan
                    row("advise".ToUpper) = itm.advise
                    row("city".ToUpper) = itm.city
                    row("issue".ToUpper) = itm.issue

                    row("corp".ToUpper) = itm.corp
                    row("memo".ToUpper) = itm.day & itm.name
                    row("case1".ToUpper) = itm.case1
                    row("case2".ToUpper) = itm.case2
                    row("case3".ToUpper) = itm.case3
                    row("modifiedBy".ToUpper) = itm.modifiedBy
                    row("modifiedDate".ToUpper) = itm.modifiedDate

                    dt.Rows.Add(row)
                Next
                Stepp = 20
                Dim result As String = ORALocalhost.SqlCMDListQuickByPara("weekLog", dt)
                If result = "success" Then 'true
                    Dim np As New NormalResponse(True, "weekLog success,Row=" & dt.Rows.Count, "", "")
                    Return np

                Else
                    Dim np As New NormalResponse(False, result & " step=" & Stepp, "", "")
                    Return np

                End If
            Catch ex As Exception
                Return New NormalResponse(False, "weekLog json格式非法,Step=" & Stepp & " err=" & ex.ToString)
            End Try
        Catch ex As Exception
            Return New NormalResponse(False, "Upload weekLog Err Step=" & Stepp & " " & ex.ToString)
        End Try
    End Function

    Public Function Handle_GetProject(ByVal context As HttpContext) As NormalResponse '获取项目名称
        Dim Stepp As Single = 0
        Try

            Dim sql As String = "select project,corp from logProject where state<>'0'"

            Stepp = 3
            Dim dt As DataTable = ORALocalhost.SqlGetDT(sql)
            If IsNothing(dt) Then
                Return New NormalResponse(False, "project 没有任何数据nothing", sql, "")
            End If
            If dt.Rows.Count = 0 Then
                Return New NormalResponse(False, "project 没有任何数据", sql, "")
            End If
            Stepp = 4
            ' "select province,city,district,netType,GDlon,GDlat,RSRP,time,SINR,eNodeBId,CellId from SDKTABLE"
            'Carrier,province,city,district,netType,GDlon,GDlat,RSRP,time,SINR
            dt.Columns(0).ColumnName = "project"            ' dt.Columns(1).ColumnName = "carrier"
            dt.Columns(1).ColumnName = "corp"

            'Stepp = 5
            Return New NormalResponse(True, "", "", dt)
        Catch ex As Exception
            Return New NormalResponse(False, "GetProject Err:" & ex.Message & ",Step=" & Stepp)
        End Try
    End Function
    Public Function Handle_CloseProject(ByVal context As HttpContext) As NormalResponse 'Close project
        Dim Stepp As Single = 0
        Try
            Dim project As String = context.Request.QueryString("project")
            Dim corp As String = context.Request.QueryString("corp")
            'Stepp = 1
            project = Trim(project) : corp = Trim(corp)
            If project = "" Then Return New NormalResponse(False, "必须输入project") '            If corp = "" Then Return New NormalResponse(False, "必须输入日期")

            Dim sql As String = "update logProject set state='0' where project='" & project & "'"

            Dim result As String = ORALocalhost.SqlCMD(sql)

            If result = "success" Then
                Return New NormalResponse(True, "close " & project & " 成功！")
            Else
                Return New NormalResponse(False, result)
            End If
            Stepp = 4

            Return New NormalResponse(True, "", "", "")
        Catch ex As Exception
            Return New NormalResponse(False, "close project Err:" & ex.Message & ",Step=" & Stepp)
        End Try
    End Function

    Public Function Handle_ProjectAdd(ByVal context As HttpContext, data As Object, token As String) As NormalResponse '增加工程

        Try
            If IsNothing(data) Then Return New NormalResponse(False, "post data is null")
            Dim str As String = JsonConvert.SerializeObject(data)
            Dim mi As logProjectInfo = JsonConvert.DeserializeObject(str, GetType(logProjectInfo))
            If IsNothing(mi) Then Return New NormalResponse(False, "ProjectInfo is null,maybe json is error")

            Dim sql As String = "insert into logProject (PROJECT,CORP,STATE) values ('{0}','{1}','{2}')"
            sql = String.Format(sql, New String() {mi.project, mi.corp, mi.state})
            Dim result As String = ORALocalhost.SqlCMD(sql)
            If result = "success" Then
                Return New NormalResponse(True, result)
            Else
                Return New NormalResponse(False, result, "", sql)
            End If
        Catch ex As Exception
            Return New NormalResponse(False, ex.ToString)
        End Try
    End Function
    Public Function Handle_GetWorker(ByVal context As HttpContext) As NormalResponse '获取工作人员
        Dim Stepp As Single = 0
        Try
            'CREATE TABLE Worklog(day varchar(50) not null,name varchar(50) not null, workContent varchar(2000) default '',issue varchar(200) default '',modifiedBy varchar(50) default '',ModifiedDate varchar(50) default '')
            Dim sql As String = "select account,name from LogAccount where state<>'0'"

            Stepp = 3
            Dim dt As DataTable = ORALocalhost.SqlGetDT(sql)
            If IsNothing(dt) Then
                Return New NormalResponse(False, "Worker没有任何数据nothing", sql, "")
            End If
            If dt.Rows.Count = 0 Then
                Return New NormalResponse(False, "Worker没有任何数据", sql, "")
            End If
            Stepp = 4
            ' "select province,city,district,netType,GDlon,GDlat,RSRP,time,SINR,eNodeBId,CellId from SDKTABLE"
            'Carrier,province,city,district,netType,GDlon,GDlat,RSRP,time,SINR
            dt.Columns(0).ColumnName = "account"            ' dt.Columns(1).ColumnName = "carrier"
            dt.Columns(1).ColumnName = "name"
            'dt.Columns(2).ColumnName = "city"

            'Stepp = 5
            Return New NormalResponse(True, "", "", dt)
        Catch ex As Exception
            Return New NormalResponse(False, "GetWorker Err:" & ex.Message & ",Step=" & Stepp)
        End Try
    End Function
    '更新日报
    Public Function Handle_UpdateWorkLog(ByVal context As HttpContext) As NormalResponse '更新工作日志
        Dim Stepp As Single = 0
        Try
            Dim name As String = context.Request.QueryString("name")
            Dim day As String = context.Request.QueryString("day")
            Dim corp As String = context.Request.QueryString("corp")
            Dim workContent As String = context.Request.QueryString("workContent")
            Dim issue As String = context.Request.QueryString("issue")
            Dim project As String = context.Request.QueryString("project")
            Dim city As String = context.Request.QueryString("city")
            Dim token As String = context.Request.QueryString("token")
            Dim usrInfo As userInfo = GetUsrInfoByToken(token)
            If IsNothing(usrInfo) Then Return New NormalResponse(False, "token无效")
            name = Trim(name) : day = Trim(day)
            If name = "" Then Return New NormalResponse(False, "必须输入名字")
            If day = "" Then Return New NormalResponse(False, "必须输入日期")

            Dim Cond As String = "", sql As String

            If corp <> "" Then Cond = " corp='" & corp & "'"
            If workContent <> "" Then Cond = IIf(Cond.Length = 0, "", Cond & " , ") & " workContent ='" & workContent & "'"
            If issue <> "" Then Cond = IIf(Cond.Length = 0, "", Cond & " , ") & " issue ='" & issue & "'"
            If project <> "" Then Cond = IIf(Cond.Length = 0, "", Cond & " , ") & " project ='" & project & "'"
            If city <> "" Then Cond = IIf(Cond.Length = 0, "", Cond & " , ") & " city ='" & city & "'"

            If Cond.Length = 0 Then
                Return New NormalResponse(False, "WorkLog没有任何更新", name, "")
            End If
            Cond = IIf(Cond.Length = 0, "", Cond & " , ") & " modifiedBy ='" & usrInfo.name & "'"
            Cond = IIf(Cond.Length = 0, "", Cond & " , ") & " modifiedDate ='" & Now.ToString("yyyy-MM-dd HH:mm:ss") & "'"
            sql = "update Worklog set " & Cond & " where name='" & name & "' and day='" & day & "'"

            Dim result As String = ORALocalhost.SqlCMD(sql)

            If result = "success" Then
                Return New NormalResponse(True, "更新 " & Cond & " 成功！")
            Else
                Return New NormalResponse(False, result)
            End If

        Catch ex As Exception
            Return New NormalResponse(False, "UpdateWorkLog Err:" & ex.Message & ",Step=" & Stepp)
        End Try
    End Function
    '获取日报
    Public Function Handle_GetWorkLog(ByVal context As HttpContext) As NormalResponse '获取工作日志
        Dim Stepp As Single = 0
        Try
            'CREATE TABLE Worklog(day varchar(50) not null,name varchar(50) not null, workContent varchar(2000) default '',issue varchar(200) default '',modifiedBy varchar(50) default '',ModifiedDate varchar(50) default '')
            Dim name As String = context.Request.QueryString("name")
            Dim day As String = context.Request.QueryString("day")
            Dim corp As String = context.Request.QueryString("corp")
            'Stepp = 1
            name = Trim(name) : day = Trim(day)
            If name = "" Then Return New NormalResponse(False, "必须输入名字")
            If day = "" Then Return New NormalResponse(False, "必须输入日期")
            'If carrier <> "中国移动" And carrier <> "中国联通" And carrier <> "中国电信" Then Return New NormalResponse(False, "运营商错误")

            Dim sql As String = "select project,workContent,issue,city from Worklog "

            sql = sql & " where name='" & name & "' and day='" & day & "'"
            If corp <> "" Then sql = sql & " "
            'If city <> "" Then sql = sql & " and city='" & city & "'"

            Stepp = 3
            Dim dt As DataTable = ORALocalhost.SqlGetDT(sql)
            If IsNothing(dt) Then
                Return New NormalResponse(False, "WorkLog没有任何数据nothing", sql, "")
            End If
            If dt.Rows.Count = 0 Then
                Return New NormalResponse(False, "WorkLog没有任何数据", sql, "")
            End If
            Stepp = 4
            ' "select province,city,district,netType,GDlon,GDlat,RSRP,time,SINR,eNodeBId,CellId from SDKTABLE"
            'Carrier,province,city,district,netType,GDlon,GDlat,RSRP,time,SINR
            dt.Columns(0).ColumnName = "project"            ' dt.Columns(1).ColumnName = "carrier"
            dt.Columns(1).ColumnName = "workContent"
            dt.Columns(2).ColumnName = "issue"
            dt.Columns(3).ColumnName = "city"

            'Stepp = 5
            Return New NormalResponse(True, "", "", dt)
        Catch ex As Exception
            Return New NormalResponse(False, "GetWorkLog Err:" & ex.Message & ",Step=" & Stepp)
        End Try
    End Function
    '上传日报
    Public Function Handle_UploadWorkLog(context As HttpContext, data As Object, token As String) As NormalResponse '保存WorkLog
        Try
            Dim str As String = JsonConvert.SerializeObject(data)
            Try
                Dim DtList As List(Of workLogInfo) = JsonConvert.DeserializeObject(str, GetType(List(Of workLogInfo)))
                If IsNothing(DtList) Then
                    Return New NormalResponse(False, "workLogList is null")
                End If
                If DtList.Count = 0 Then
                    Return New NormalResponse(False, "workLogList count =0")
                End If
                Dim usrInfo As userInfo = GetUsrInfoByToken(token)
                If IsNothing(usrInfo) Then Return New NormalResponse(False, "token无效")
                Dim colList() As String = GetOraTableColumns("workLog")
                Dim dt As New DataTable
                For Each col In colList
                    If col <> "ID" Then
                        dt.Columns.Add(col)
                    End If
                Next
                Dim errDt As New DataTable
                errDt.Columns.Add("fileName")
                errDt.Columns.Add("error")
                Dim dTmp As String = Now.ToString("yyyy-MM-dd HH:mm:ss")
                For Each itm In DtList
                    itm.modifiedDate = dTmp
                    itm.modifiedBy = usrInfo.name
                    Dim row As DataRow = dt.NewRow
                    row("day".ToUpper) = itm.day
                    row("name".ToUpper) = usrInfo.name
                    row("account".ToUpper) = usrInfo.usr
                    row("project".ToUpper) = itm.project
                    row("workContent".ToUpper) = itm.WorkContent
                    row("city".ToUpper) = itm.city
                    row("issue".ToUpper) = itm.issue
                    row("modifiedBy".ToUpper) = itm.modifiedBy
                    row("modifiedDate".ToUpper) = itm.modifiedDate
                    row("corp".ToUpper) = itm.corp
                    row("memo".ToUpper) = itm.day & itm.name
                    If IsNothing(itm.files) = False Then
                        Dim workFiles As New List(Of workFileInfo2)
                        Dim visualPath As String = "workLogFiles"
                        Dim rootPath As String = System.Web.HttpContext.Current.Server.MapPath("~/" & visualPath & "/")
                        Dim urlPath As String = "http://111.53.74.132:8091/" & visualPath & "/"
                        For Each wf In itm.files
                            Dim fileName As String = wf.fileName
                            Dim base64 As String = wf.base64
                            Dim nFileName As String = fileName
                            Dim filePath As String = rootPath & nFileName
                            Dim fileUrl As String = urlPath & nFileName
                            If File.Exists(filePath) Then
                                nFileName = Now.Ticks & "_" & fileName
                                filePath = rootPath & nFileName
                                fileUrl = urlPath & nFileName
                            End If
                            Try
                                Dim buffer() As Byte = Convert.FromBase64String(base64)
                                File.WriteAllBytes(filePath, buffer)
                                workFiles.Add(New workFileInfo2(fileName, fileUrl))
                            Catch ex As Exception
                                Dim errRow As DataRow = errDt.NewRow
                                errRow("fileName") = fileName
                                errRow("error") = ex.ToString
                                errDt.Rows.Add(errRow)
                            End Try
                        Next
                        Dim fileUrlsJson As String = JsonConvert.SerializeObject(workFiles)
                        row("fileUrls".ToUpper) = fileUrlsJson
                    End If
                    dt.Rows.Add(row)
                Next
                If errDt.Rows.Count > 0 Then
                    Dim np As New NormalResponse(False, "部分文件上传失败！", "", errDt)
                    Return np
                End If
                Dim result As String = ORALocalhost.SqlCMDListQuickByPara("workLog", dt)
                If result = "success" Then 'true
                    Dim np As New NormalResponse(True, "workLog success,Row=" & dt.Rows.Count, "", "")
                    Return np

                Else
                    Dim np As New NormalResponse(False, result)
                    Return np
                End If
            Catch ex As Exception
                Return New NormalResponse(False, "workLog json格式非法," & "err=" & ex.ToString)
            End Try
        Catch ex As Exception
            Return New NormalResponse(False, "Upload workLog Err " & ex.ToString)
        End Try
    End Function

    Structure LocalTestInfo
        Public id As Integer
        Public time As String
        Public json As String
        Public type As String
    End Structure

    Private Function GetOraTableColumns(tableName As String) As String()
        'select COLUMN_NAME from user_tab_columns where table_name ='QOE_REPORT_TABLE'
        Try
            Dim sql As String = "select COLUMN_NAME from user_tab_columns where table_name ='" & tableName.ToUpper & "'"
            Dim dt As DataTable = ORALocalhost.SqlGetDT(sql)
            If IsNothing(dt) Then Return Nothing
            If dt.Rows.Count = 0 Then Return Nothing
            Dim list As New List(Of String)
            For Each row As DataRow In dt.Rows
                If IsNothing(row(0)) = False Then
                    list.Add(row(0).ToString)
                End If
            Next
            Return list.ToArray
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Private Function OracleSelectPage(sql As String, startIndex As Long, count As Long) As String
        Dim sb As New StringBuilder
        sb.Append("select * from ( ")
        sb.Append("select A.*, Rownum RN from ( ")
        sb.Append(sql)
        sb.Append(") A ")
        sb.Append(" where Rownum<=" & startIndex + count & " )")
        sb.Append("where RN>=" & startIndex)
        Return sb.ToString
    End Function

    Private Function GetTimeSpan(ByVal t As Date) As String
        Dim endTime As Date = Now
        Dim ts As TimeSpan = endTime - t
        Dim str As String = ts.Hours.ToString("00") & ":" & ts.Minutes.ToString("00") & ":" & ts.Seconds.ToString("00") & "." & ts.Milliseconds.ToString("000")
        Return str
    End Function



    Public Function Handle_AccountAdd(ByVal context As HttpContext, data As Object, token As String) As NormalResponse '增加用户

        Try
            If IsNothing(data) Then Return New NormalResponse(False, "post data is null")
            Dim str As String = JsonConvert.SerializeObject(data)
            Dim mi As accountInfo = JsonConvert.DeserializeObject(str, GetType(accountInfo))
            If IsNothing(mi) Then Return New NormalResponse(False, "AccountInfo is null,maybe json is error")

            Dim sql As String = "insert into LogAccount (WORKID,ACCOUNT,PASSWORD,NAME,CORP,STATE) values ('{0}','{1}','{2}','{3}','{4}','{5}')"
            sql = String.Format(sql, New String() {mi.workID, mi.account, mi.password, mi.name, mi.corp, mi.state})
            Dim result As String = ORALocalhost.SqlCMD(sql)
            If result = "success" Then
                Return New NormalResponse(True, result)
            Else
                Return New NormalResponse(False, result, "", sql)
            End If
        Catch ex As Exception
            Return New NormalResponse(False, ex.ToString)
        End Try
    End Function
    Public Function Handle_AccountModify(ByVal context As HttpContext) As NormalResponse '修改用户密码
        Dim Stepp As Single = 0
        Try
            'CREATE TABLE Worklog(day varchar(50) not null,name varchar(50) not null, workContent varchar(2000) default '',issue varchar(200) default '',modifiedBy varchar(50) default '',ModifiedDate varchar(50) default '')
            Dim account As String = context.Request.QueryString("account")
            Dim name As String = context.Request.QueryString("name")
            Dim password As String = context.Request.QueryString("password")
            Dim newPassword As String = context.Request.QueryString("newPassword")
            Dim corp As String = context.Request.QueryString("corp")

            'Stepp = 1

            account = Trim(account)
            If account = "" Then Return New NormalResponse(False, "必须输入账号")
            If password = "" Then Return New NormalResponse(False, "必须输入密码")
            If newPassword = "" Then Return New NormalResponse(False, "必须输入新密码")
            'If carrier <> "中国移动" And carrier <> "中国联通" And carrier <> "中国电信" Then Return New NormalResponse(False, "运营商错误")

            Dim sql As String

            sql = "update logAccount set password='" & newPassword & "' where account='" & account & "'" ' and password='" & password & "'"

            Stepp = 3
            Dim result As String = ORALocalhost.SqlCMD(sql)

            If result = "success" Then
                Return New NormalResponse(True, "更新密码成功！")
            Else
                Return New NormalResponse(False, result)
            End If
            Stepp = 4
            ' "select province,city,district,netType,GDlon,GDlat,RSRP,time,SINR,eNodeBId,CellId from SDKTABLE"
            'Carrier,province,city,district,netType,GDlon,GDlat,RSRP,time,SINR
            'dt.Columns(0).ColumnName = "project"            ' dt.Columns(1).ColumnName = "carrier"
            'dt.Columns(1).ColumnName = "workContent"
            'dt.Columns(2).ColumnName = "issue"

            'Stepp = 5
            Return New NormalResponse(True, "", "", "")
        Catch ex As Exception
            Return New NormalResponse(False, "AccountModify Err:" & ex.Message & ",Step=" & Stepp)
        End Try
    End Function
    Public Function Handle_GetIndexPageTable0(context As HttpContext) As NormalResponse '2018-12-23 09:54:00 更新 增加首页拼接表
        Try
            'Dim count As String = context.Request.QueryString("count")
            'If count = "" Then count = 10
            'If IsNumeric(count) = False Then
            '    Return New NormalResponse(False, "count 非数字")
            'End If
            Dim sql As String = "select substr(datetime,1,10),4*count(substr(datetime,1,10)) from Qoe_Video_TABLE GROUP BY substr(datetime,1,10) ORDER BY substr(datetime,1,10) desc"
            sql = OracleSelectPage(sql, 0, 7)
            Dim dt As DataTable = ORALocalhost.SqlGetDT(sql)
            If IsNothing(dt) Then Return New NormalResponse(False, "没有任何数据")
            If dt.Rows.Count = 0 Then Return New NormalResponse(False, "没有任何数据")
            Dim dayList As New List(Of String)
            For Each row In dt.Rows
                If IsNothing(row(0)) = False Then
                    If row(0) <> "" Then
                        dayList.Add(row(0))
                    End If
                End If
            Next
            Dim resultDt As New DataTable
            resultDt.Columns.Add("时间")
            resultDt.Columns.Add("采样点总数(移动)")
            resultDt.Columns.Add("采样点总数(联通)")
            resultDt.Columns.Add("采样点总数(电信)")
            resultDt.Columns.Add("RSRP(移动)")
            resultDt.Columns.Add("RSRP(联通)")
            resultDt.Columns.Add("RSRP(电信)")
            For Each itm In dayList
                Dim row As DataRow = resultDt.NewRow
                row("时间") = itm
                sql = "select CARRIER,4*COUNT(CARRIER) from Qoe_Video_TABLE WHERE substr(datetime,1,10)='" & itm & "' GROUP BY CARRIER"
                dt = Nothing
                dt = ORALocalhost.SqlGetDT(sql)
                If IsNothing(dt) = False Then
                    If dt.Rows.Count > 0 Then
                        For Each rw As DataRow In dt.Rows
                            If IsNothing(rw(0)) Then Continue For
                            If IsDBNull(rw(0)) Then Continue For
                            Dim carrier As String = rw(0)
                            Dim carrierCount As Integer = rw(1)
                            If carrier = "中国移动" Then
                                row("采样点总数(移动)") = carrierCount
                                sql = "select sum(SIGNAL_STRENGTH) from Qoe_Video_TABLE WHERE substr(datetime,1,10)='" & itm & "' and CARRIER='中国移动' and SIGNAL_STRENGTH<0"
                                Dim info As String = ORALocalhost.SQLInfo(sql)
                                If IsNothing(info) = False Then
                                    If IsNumeric(info) Then
                                        Dim rsrp As Integer = Math.Floor(Val(info) / carrierCount * 4)
                                        row("RSRP(移动)") = rsrp
                                    End If
                                End If
                            End If
                            If carrier = "中国联通" Then
                                row("采样点总数(联通)") = carrierCount
                                sql = "select sum(SIGNAL_STRENGTH) from Qoe_Video_TABLE WHERE substr(datetime,1,10)='" & itm & "' and CARRIER='中国联通' and SIGNAL_STRENGTH<0"
                                Dim info As String = ORALocalhost.SQLInfo(sql)
                                File.WriteAllText("d:\hhhhhSQLGetFirstRowCell.txt", info)
                                If IsNothing(info) = False Then
                                    If IsNumeric(info) Then
                                        Dim rsrp As Integer = Math.Floor(Val(info) / carrierCount * 4)
                                        row("RSRP(联通)") = rsrp
                                    End If
                                End If
                            End If

                            If carrier = "中国电信" Then
                                row("采样点总数(电信)") = carrierCount
                                sql = "select sum(SIGNAL_STRENGTH) from Qoe_Video_TABLE WHERE substr(datetime,1,10)='" & itm & "' and CARRIER='中国电信' and SIGNAL_STRENGTH<0"
                                Dim info As String = ORALocalhost.SQLInfo(sql)
                                If IsNothing(info) = False Then
                                    If IsNumeric(info) Then
                                        Dim rsrp As Integer = Math.Floor(Val(info) / carrierCount * 4)
                                        row("RSRP(电信)") = rsrp
                                    End If
                                End If
                            End If
                        Next
                    End If
                End If
                resultDt.Rows.Add(row)
            Next
            Return New NormalResponse(True, "", "", resultDt)
        Catch ex As Exception
            Return New NormalResponse(False, ex.ToString)
        End Try
    End Function
    Structure PersonalWeeklogInfo
        Dim name As String
        Dim weekDay As String
        Dim thisWeekWork As String
        Dim problem As String
        Dim nextWeekWork As String
        Dim advice As String
    End Structure
    '前端发起制作周报命令
    Public Function Handle_DoPersonalWeekLog(context As HttpContext, data As Object, token As String) As NormalResponse
        Try
            Dim str As String = JsonConvert.SerializeObject(data)
            Dim pi As PersonalWeeklogInfo = JsonConvert.DeserializeObject(str, GetType(PersonalWeeklogInfo))
            If IsNothing(pi) Then Return New NormalResponse(False, "PersonalWeeklogInfo格式非法")
            Dim np As NormalResponse = DoPersonalWeekLog(pi)
            Return np
        Catch ex As Exception
            Return New NormalResponse(False, ex.ToString)
        End Try
    End Function
    '制作周报
    Private Function DoPersonalWeekLog(pi As PersonalWeeklogInfo) As NormalResponse
        '  Log("制作周报，用户:" & name)
        Dim name As String = pi.name
        Dim weekday As Date
        If pi.weekDay = "thisWeek" Then
            weekDay = Now
        Else
            weekDay = Date.Parse(pi.weekDay)
        End If
        Dim thisWeekWork As String = pi.thisWeekWork
        Dim problem As String = pi.problem
        Dim nextWeekWork As String = pi.nextWeekWork
        Dim advice As String = pi.advice

        Dim weekStart As Date = GetWeekStart(weekDay)
        Dim weekEnd As Date = weekStart.AddDays(6)
        Dim weekStartStr As String = weekStart.ToString("yyyy-MM-dd")
        Dim weekEndStr As String = weekEnd.ToString("yyyy-MM-dd")
        ' Log("选择日期 " & GetWeekday(weekDay) & ",星期六:" & weekStartStr & ",星期五:" & weekEndStr)
        ' Log("查询数据库...")
        Dim sql As String = "select * from worklog where name='{0}' and day between '{1}' and '{2}' order by day asc"
        sql = String.Format(sql, New String() {name, weekStartStr, weekEndStr})
        Dim dt As DataTable = ORALocalhost.SqlGetDT(sql)
        If IsNothing(dt) Then
            ' Log("没有数据")
            Return New NormalResponse(False, "没有查询到该用户本周数据")
        End If
        If dt.Rows.Count = 0 Then
            Return New NormalResponse(False, "没有查询到该用户本周数据")
        End If
        ' Log("数据行数:" & dt.Rows.Count)
        ' Log("读取模板...")
        Dim excelModuleFilePath As String = ""
        Dim moduleFilePath As String = System.Web.HttpContext.Current.Server.MapPath("~/dev/个人周报模板.xlsx")
        Dim excel As New ExcelPackage(New FileInfo(moduleFilePath))
        Dim sheet As ExcelWorksheet = excel.Workbook.Worksheets(1)
        sheet.Cells(2, 2).Value = name  '写周报的人员名称
        sheet.Cells(2, 5).Value = weekEndStr  '写周报的日期  周五
        Dim workCount As Integer = dt.Rows.Count
        For i = 0 To 6
            sheet.Cells(i + 5, 2).Value = weekStart.AddDays(i).ToString("MM月dd日")
            Dim writeDay As String = weekStart.AddDays(i).ToString("yyyy-MM-dd")
            For Each row As DataRow In dt.Rows
                Dim thisDay As String = row("DAY")
                If thisDay = writeDay Then
                    Dim project As String = row("project".ToUpper).ToString
                    Dim workContent As String = row("workContent".ToUpper).ToString
                    Dim city As String = row("city".ToUpper).ToString
                    sheet.Cells(i + 5, 3).Value = city
                    sheet.Cells(i + 5, 4).Value = project
                    sheet.Cells(i + 5, 6).Value = workContent
                    Exit For
                End If
            Next
        Next
        sheet.Cells(12, 2).Value = thisWeekWork
        sheet.Cells(13, 2).Value = problem
        sheet.Cells(14, 2).Value = nextWeekWork
        sheet.Cells(15, 2).Value = advice
        Dim saveFileName As String = "周报-{0}({1}-{2}).xlsx"
        saveFileName = String.Format(saveFileName, New String() {name, weekStart.ToString("yyyyMMdd"), weekEnd.ToString("MMdd")})
        'Log("保存Excel到文件 " & saveFilePath)
        Dim visualPath As String = "worklogfiles"
        Dim rootPath As String = System.Web.HttpContext.Current.Server.MapPath("~/" & visualPath & "/") & "personalWeekLogExcel/"
        Dim saveFilePath As String = rootPath & saveFileName
        If File.Exists(saveFilePath) Then File.Delete(saveFilePath)
        excel.SaveAs(New FileInfo(saveFilePath))
        Dim url As String = "http://111.53.74.132:8091/" & visualPath & "/personalWeekLogExcel/" & saveFileName
        Return New NormalResponse(True, "", "", url)
        'Log("Done!")
    End Function
    Private Function GetWeekday(d As Date) As String
        Dim week As String() = {"星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"}
        Dim weekInt As Integer = Convert.ToInt32(d.DayOfWeek)
        Return week(weekInt)
    End Function
    Private Function GetWeekStart(d As Date) As Date
        Dim week As String() = {"星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六"}
        Dim weekInt As Integer = Convert.ToInt32(d.DayOfWeek)
        Return d.AddDays(-1 * weekInt - 1)
    End Function
End Class
