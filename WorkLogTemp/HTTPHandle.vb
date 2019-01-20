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
Imports MySql.Data.MySqlClient
Imports System.Reflection
Imports System.IO.Compression

Public Class HTTPHandle
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

    ''设置web.config  System.Web.Configuration.WebConfigurationManager.AppSettings.Set("name", "name2");

    Public Function Handle_Test(ByVal context As HttpContext) As NormalResponse '测试
        Return New NormalResponse(True, "WorkLogTemp网络测试成功！这是返回处理信息", "这里返回错误信息", "这里返回数据")
    End Function
    Public Function Handle_SetConfig(ByVal context As HttpContext) As NormalResponse '设置配置信息
        Dim configName As String = context.Request.QueryString("configName")
        Dim value As String = context.Request.QueryString("value")
        System.Web.Configuration.WebConfigurationManager.AppSettings.Set(configName, value)
        Return New NormalResponse(True, "设置成功")
    End Function
    Public Function Handle_GetConfig(ByVal context As HttpContext) As NormalResponse '获取配置信息
        Dim configName As String = context.Request.QueryString("configName")
        Dim sql As String = System.Web.Configuration.WebConfigurationManager.AppSettings(configName)
        Return New NormalResponse(True, "", "", sql)
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

    Public Function Handle_UploadWeekLog(context As HttpContext, data As Object) As NormalResponse '保存WeekLog
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

    Public Function Handle_ProjectAdd(ByVal context As HttpContext, data As Object) As NormalResponse '增加工程

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

    Public Function Handle_UpdateWorkLog(ByVal context As HttpContext) As NormalResponse '更新工作日志
        Dim Stepp As Single = 0
        Try
            'CREATE TABLE Worklog(day varchar(50) not null,name varchar(50) not null, workContent varchar(2000) default '',issue varchar(200) default '',modifiedBy varchar(50) default '',ModifiedDate varchar(50) default '')
            Dim name As String = context.Request.QueryString("name")
            Dim day As String = context.Request.QueryString("day")
            Dim corp As String = context.Request.QueryString("corp")
            Dim workContent As String = context.Request.QueryString("workContent")
            Dim issue As String = context.Request.QueryString("issue")
            Dim project As String = context.Request.QueryString("project")
            Dim city As String = context.Request.QueryString("city")

            'Stepp = 1
            name = Trim(name) : day = Trim(day)
            If name = "" Then Return New NormalResponse(False, "必须输入名字")
            If day = "" Then Return New NormalResponse(False, "必须输入日期")
            'If carrier <> "中国移动" And carrier <> "中国联通" And carrier <> "中国电信" Then Return New NormalResponse(False, "运营商错误")

            Dim Cond As String = "", sql As String


            If corp <> "" Then Cond = " corp='" & corp & "'"
            If workContent <> "" Then Cond = IIf(Cond.Length = 0, "", Cond & " , ") & " workContent ='" & workContent & "'"
            If issue <> "" Then Cond = IIf(Cond.Length = 0, "", Cond & " , ") & " issue ='" & issue & "'"
            If project <> "" Then Cond = IIf(Cond.Length = 0, "", Cond & " , ") & " project ='" & project & "'"
            If city <> "" Then Cond = IIf(Cond.Length = 0, "", Cond & " , ") & " city ='" & city & "'"
            'If city <> "" Then Cond = IIf(Cond = 0, "", " and ") & " city ='" & city & "'"

            If Cond.Length = 0 Then
                Return New NormalResponse(False, "WorkLog没有任何更新", name, "")
            End If
            sql = "update Worklog set " & Cond & " where name='" & name & "' and day='" & day & "'"

            Stepp = 3
            Dim result As String = ORALocalhost.SqlCMD(sql)

            If result = "success" Then
                Return New NormalResponse(True, "更新 " & Cond & " 成功！")
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
            Return New NormalResponse(False, "UpdateWorkLog Err:" & ex.Message & ",Step=" & Stepp)
        End Try
    End Function

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

    Public Function Handle_UploadWorkLog(context As HttpContext, data As Object) As NormalResponse '保存WorkLog
        Dim Stepp As Integer = -1
        Try
            Dim str As String = JsonConvert.SerializeObject(data)
            Stepp = 0
            Try
                Dim DtList As List(Of workLogInfo) = JsonConvert.DeserializeObject(str, GetType(List(Of workLogInfo)))
                If IsNothing(DtList) Then
                    Return New NormalResponse(False, "workLogList is null")
                End If
                If DtList.Count = 0 Then
                    Return New NormalResponse(False, "workLogList count =0")
                End If
                Stepp = 1
                Dim colList() As String = GetOraTableColumns("workLog")
                Dim dt As New DataTable
                For Each col In colList
                    If col <> "ID" Then
                        dt.Columns.Add(col)
                    End If
                Next
                Stepp = 2
                For Each itm In DtList
                    Dim row As DataRow = dt.NewRow
                    row("day".ToUpper) = itm.day
                    row("name".ToUpper) = itm.name
                    row("account".ToUpper) = itm.account
                    row("project".ToUpper) = itm.project
                    row("workContent".ToUpper) = itm.WorkContent
                    row("city".ToUpper) = itm.city
                    row("issue".ToUpper) = itm.issue
                    row("modifiedBy".ToUpper) = itm.modifiedBy
                    row("modifiedDate".ToUpper) = itm.modifiedDate
                    row("corp".ToUpper) = itm.corp

                    row("memo".ToUpper) = itm.day & itm.name

                    dt.Rows.Add(row)
                Next
                Stepp = 20
                Dim result As String = ORALocalhost.SqlCMDListQuickByPara("workLog", dt)
                If result = "success" Then 'true
                    Dim np As New NormalResponse(True, "workLog success,Row=" & dt.Rows.Count, "", "")
                    Return np

                Else
                    Dim np As New NormalResponse(False, result & " step=" & Stepp, "", "")
                    Return np

                End If
            Catch ex As Exception
                Return New NormalResponse(False, "workLog json格式非法,Step=" & Stepp & " err=" & ex.ToString)
            End Try
        Catch ex As Exception
            Return New NormalResponse(False, "Upload workLog Err Step=" & Stepp & " " & ex.ToString)
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
    Structure RunSQLInfo
        Dim connStr As String
        Dim sqllist As List(Of String)
    End Structure
    Private Function Decompress(ByVal data() As Byte) As Byte()
        Dim stream As MemoryStream = New MemoryStream
        Dim gZip As New GZipStream(New MemoryStream(data), CompressionMode.Decompress)
        Dim n As Integer = 0
        While True
            Dim by(409600) As Byte
            n = gZip.Read(by, 0, by.Length)
            If n = 0 Then Exit While
            stream.Write(by, 0, n)
        End While
        gZip.Close()
        Return stream.ToArray
    End Function
    Public Function Handle_RunSQL(ByVal context As HttpContext, ByVal data As Object) As NormalResponse '按Sql来查询
        Try
            Dim str As String = data.ToString
            Dim by() As Byte = Convert.FromBase64String(str)
            Dim realBy() As Byte = Decompress(by)
            str = Encoding.Default.GetString(realBy)
            Dim runSqlInfo As RunSQLInfo = JsonConvert.DeserializeObject(str, GetType(RunSQLInfo))
            If IsNothing(runSqlInfo) Then
                Return New NormalResponse(False, "RunSQLInfo格式非法")
            End If
            If runSqlInfo.sqllist.Count = 0 Then
                Return New NormalResponse(False, "RunSQLInfo.SqlList.count=0")
            End If
            Dim conn As String = runSqlInfo.connStr
            Dim failcount As Integer = 0
            Dim successcount As Integer = 0
            Dim startTime As Date = Now
            Dim result As String = SQLInsertSQLList(conn, runSqlInfo.sqllist)
            'Dim th As New Thread(Sub()
            '                         Dim conn As String = runSqlInfo.connStr
            '                         Dim failCount As Integer = 0
            '                         Dim successCount As Integer = 0
            '                         For Each itm In runSqlInfo.sqllist
            '                             If SQLCmdWithConn(conn, itm) Then
            '                                 successCount = successCount + 1
            '                             Else
            '                                 failCount = failCount + 1
            '                             End If
            '                         Next
            '                     End Sub)
            'th.Start()
            Dim np As NormalResponse
            If result = "success" Then
                np = New NormalResponse(True, result, "", "提交SQL总行数:" & runSqlInfo.sqllist.Count & ",耗时:" & GetTimeSpam(startTime))
            Else
                np = New NormalResponse(False, result, "", "提交SQL总行数:" & runSqlInfo.sqllist.Count & ",耗时:" & GetTimeSpam(startTime))
            End If
            Return np
            'If failCount = 0 Then
            '    Return New NormalResponse(True, "success")
            'Else
            '    Return New NormalResponse(True, "成功:" & successCount & ",失败:" & failCount)
            'End If
        Catch ex As Exception
            Return New NormalResponse(False, ex.Message)
        End Try
    End Function
    Structure GZfileInfo
        Dim cellInfo As cellInfo
        Dim eNodeBId As String
        Dim base64 As String
        Sub New(cellinfo As cellInfo, eNodeBId As String, base64 As String)
            Me.cellInfo = cellinfo
            Me.eNodeBId = eNodeBId
            Me.base64 = base64
        End Sub
    End Structure
    Structure cellInfo
        Dim lon As Double
        Dim lat As Double
        Dim district As String
        Dim province As String
        Dim city As String
        Dim grid As String
        Dim siteType As Integer
    End Structure


    Private Function GetGridBySQL(lon As Double, lat As Double) As Integer
        Dim sql As String = "select id from grid_Table where startlon<=" & lon & " and stoplon>=" & lon & " and startlat<=" & lat & " and stoplat>=" & lat
        Dim dt As DataTable = ORALocalhost.SqlGetDT(sql)
        If IsNothing(dt) Then Return 0
        If dt.Rows.Count = 0 Then Return 0
        Dim row As DataRow = dt.Rows(0)
        Dim str As String = row(0)
        If IsNothing(str) Then Return 0
        If str = "" Then Return 0
        If IsNumeric(str) = False Then Return 0
        Return str
    End Function
    Private Function GetAoARand() As Integer
        Return Int(Rnd() * 121) - 60
    End Function
    Private Function HandleMRDt(dt As DataTable) As DataTable
        If IsNothing(dt) Then Return dt
        'Dim sqlList As New List(Of String)
        dt.Columns.Add("UElon")
        dt.Columns.Add("UElat")
        dt.Columns.Add("UEdis")
        dt.Columns.Add("RSRP")
        dt.Columns.Add("SINR")
        dt.Columns.Add("BDlon")
        dt.Columns.Add("BDlat")
        dt.Columns.Add("UEBDlon")
        dt.Columns.Add("UEBDlat")
        dt.Columns.Add("GDlon")
        dt.Columns.Add("GDlat")
        dt.Columns.Add("UEGDlon")
        dt.Columns.Add("UEGDlat")
        'dt.Columns.Add("Grid")

        For i = dt.Rows.Count - 1 To 0 Step -1
            Dim row As DataRow = dt.Rows(i)
            If IsNothing(row("LteScTadv")) Then Continue For
            If IsNothing(row("LteScAOA")) Then Continue For
            If IsNothing(row("LteScTadv")) Then Continue For
            If IsNothing(row("LteScRSRP")) Then Continue For
            If IsNothing(row("LteScSinrUL")) Then Continue For
            If IsNothing(row("lon")) Then Continue For
            If IsNothing(row("lat")) Then Continue For
            If row("lon") = "NIL" Then Continue For
            If row("lat") = "NIL" Then Continue For
            If row("LteScTadv") = "NIL" Then Continue For
            If row("LteScAOA") = "NIL" Then Continue For
            If row("LteScTadv") = "NIL" Then Continue For
            If row("LteScRSRP") = "NIL" Then Continue For
            If row("LteScSinrUL") = "NIL" Then Continue For

            Dim eNodebId As String = row("eNodebId")
            Dim adv As Double = Val(row("LteScTadv"))
            Dim aoa As Double = Val(row("LteScAOA"))
            adv = adv * 78.12
            aoa = 360 - (aoa / 2)
            Dim oldAoa As Double = aoa
            aoa = aoa + GetAoARand()
            If aoa < 0 Or aoa > 360 Then aoa = oldAoa
            Dim lon As Double = Val(row("lon"))
            Dim lat As Double = Val(row("lat"))
            Dim UElon As Double = lon + (adv * Sin(aoa)) / (111000 * Cos(lat * PI / 180))
            Dim UElat As Double = lat + (adv * Cos(aoa * PI / 180)) / 111000
            Dim dis As Double = GetDistance(lat, lon, UElat, UElon)
            Dim RSRP As Double = row("LteScRSRP")
            Dim SINR As Double = row("LteScSinrUL")
            RSRP = RSRP - 140
            Dim grid As Integer = GetGridBySQL(UElon, UElat)
            If grid = 0 Then
                dt.Rows(i).Delete()
                Continue For
            End If
            row("UElon") = UElon
            row("UElat") = UElat
            row("UEdis") = dis
            row("RSRP") = RSRP
            row("SINR") = SINR

            row("Grid") = grid
            Dim BDlon, BDlat, BDUElon, BDUElat, GDlon, GDlat, GDUElon, GDUElat As Double
            Dim BDxt1 As CoordInfo = GPS2BDS(lon, lat)
            Dim BDxt2 As CoordInfo = GPS2BDS(UElon, UElat)
            If IsNothing(BDxt1) = False And IsNothing(BDxt2) = False Then
                BDlon = BDxt1.x
                BDlat = BDxt1.y
                BDUElon = BDxt2.x
                BDUElat = BDxt2.y
                row("BDlon") = BDlon
                row("BDlat") = BDlat
                row("UEBDlon") = BDUElon
                row("UEBDlat") = BDUElat
            End If
            Dim GDxt1 As CoordInfo = GPS2GDS(lon, lat)
            Dim GDxt2 As CoordInfo = GPS2GDS(UElon, UElat)
            If IsNothing(GDxt1) = False And IsNothing(GDxt2) = False Then
                GDlon = GDxt1.x
                GDlat = GDxt1.y
                GDUElon = GDxt2.x
                GDUElat = GDxt2.y
                row("GDlon") = GDlon
                row("GDlat") = GDlat
                row("UEGDlon") = GDUElon
                row("UEGDlat") = GDUElat
            End If
        Next
        Return dt
    End Function
    Private Function rad(ByVal d As Double) As Double
        rad = d * PI / 180
    End Function
    Private Function GetDistance(ByVal lat1 As Double, ByVal lng1 As Double, ByVal lat2 As Double, ByVal lng2 As Double) As Double
        Dim radlat1 As Double, radlat2 As Double
        Dim a As Double, b As Double, s As Double, Temp As Double
        radlat1 = rad(lat1)
        radlat2 = rad(lat2)
        a = radlat1 - radlat2
        b = rad(lng1) - rad(lng2)
        Temp = Sqrt(Sin(a / 2) ^ 2 + Cos(radlat1) * Cos(radlat2) * Sin(b / 2) ^ 2)
        s = 2 * Atan(Temp / Sqrt(-Temp * Temp + 1))
        s = s * 6378.137
        Return Math.Round(s * 1000, 2)
    End Function
    Private Function GetTimeSpam(ByVal t As Date) As String
        Dim endTime As Date = Now
        Dim ts As TimeSpan = endTime - t
        Dim str As String = ts.Hours.ToString("00") & ":" & ts.Minutes.ToString("00") & ":" & ts.Seconds.ToString("00") & "." & ts.Milliseconds.ToString("000")
        Return str
    End Function
    Private Function SQLCmdWithConn(ByVal conn As String, ByVal CmdString As String) As Boolean
        Try
            Dim SQL As New MySqlConnection(conn)
            SQL.Open()
            Dim SQLCommand As MySqlCommand = New MySqlCommand(CmdString, SQL)
            Dim ResultRowInt As Integer = SQLCommand.ExecuteNonQuery()
            SQL.Close()
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function


    Public Function Handle_GetAccount(ByVal context As HttpContext) As NormalResponse '获得用户账号 
        Dim Stepp As Single = 0
        Try
            Dim account As String = context.Request.QueryString("account") ' Dim city As String = context.Request.QueryString("city")
            Dim passWord As String = context.Request.QueryString("passWord")
            Dim state As String = context.Request.QueryString("state") '状态
            Stepp = 1

            If account = "" Then Return New NormalResponse(False, "用户名为空错误")
            If passWord = "" Then Return New NormalResponse(False, "密码为空错误")

            Dim sql As String = "select password from LogAccount where account='" & account & "' and state<>0" ' And " passWord ='" & passWord & "'"

            Stepp = 3
            Dim Str1 As String = ORALocalhost.SQLGetFirstRowCell(sql)
            If passWord = Str1 Then
                Return New NormalResponse(True, "用户名密码正确", ""， "")
            Else
                Return New NormalResponse(False, "用户名密码错误", "", "")
            End If

            'Stepp = 5

        Catch ex As Exception
            Return New NormalResponse(False, "GetAccount Err:" & ex.Message & ",Step=" & Stepp)
        End Try
    End Function

    Public Function Handle_AccountAdd(ByVal context As HttpContext, data As Object) As NormalResponse '增加用户

        Try
            If IsNothing(data) Then Return New NormalResponse(False, "post data is null")
            Dim str As String = JsonConvert.SerializeObject(data)
            Dim mi As AccountInfo = JsonConvert.DeserializeObject(str, GetType(AccountInfo))
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
    Public Function Handle_GetQOEVideoSource(context As HttpContext) As NormalResponse
        Try
            Dim sql As String = "select * from QOE_VIDEO_SOURCE where isuse=1"
            Dim dt As DataTable = ORALocalhost.SqlGetDT(sql)
            If IsNothing(dt) Then Return New NormalResponse(False, "dt is null")
            If dt.Rows.Count = 0 Then Return New NormalResponse(False, "dt.rows.count=0")
            Return New NormalResponse(True, "", "", dt)
        Catch ex As Exception
            Return New NormalResponse(False, ex.ToString)
        End Try
    End Function
End Class
