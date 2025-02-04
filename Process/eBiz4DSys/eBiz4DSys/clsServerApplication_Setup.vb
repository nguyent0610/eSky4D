'-- ------------------------------------------------------------
'-- Class name    :  clsServerApplication_Setup
'-- Created date  :  12/11/2012
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework
Imports HQFramework.DAL
Imports HQFramework.Common

Public Class clsServerApplication_Setup
#Region "Constants"
    Private Const PP_ServerApplication_Setup As String = "PP_ServerApplication_Setup"
#End Region

#Region "Member Variables"
    Private mvarSetupID As System.String

    Private mvarIsSSL As System.Int32

    Private mvarPwdExpiredDays As System.Int32

    Private mvarDfltPassword As System.String

    Private mvarRunTime As System.Int32

    Private mvarMaxTask As System.Int32

    Private mvarRefreshTime As System.Int32

    Private mvarFolderImportLocal As System.String

    Private mvarFolderInbox As System.String

    Private mvarFolderInboxBackup As System.String

    Private mvarFolderOutbox As System.String

    Private mvarFolderOutboxBackup As System.String

    Private mvarFolderUpload As System.String

    Private mvarFolderExport As System.String

    Private mvarResetTime As System.Int32

#End Region

    Private m_Dal As DataAccess
#Region "Constructors"
    Public Sub New()
        m_Dal = New DataAccess
        Reset()
    End Sub
    Public Sub New(ByVal dal As DataAccess)
        m_Dal = dal
        Reset()
    End Sub
#End Region

#Region "Public Properties"
    Public Property SetupID() As System.String
        Get
            Return mvarSetupID
        End Get
        Set(ByVal Value As System.String)
            mvarSetupID = Value
        End Set
    End Property

    Public Property IsSSL() As System.Int32
        Get
            Return mvarIsSSL
        End Get
        Set(ByVal Value As System.Int32)
            mvarIsSSL = Value
        End Set
    End Property

    Public Property PwdExpiredDays() As System.Int32
        Get
            Return mvarPwdExpiredDays
        End Get
        Set(ByVal Value As System.Int32)
            mvarPwdExpiredDays = Value
        End Set
    End Property

    Public Property DfltPassword() As System.String
        Get
            Return mvarDfltPassword
        End Get
        Set(ByVal Value As System.String)
            mvarDfltPassword = Value
        End Set
    End Property

    Public Property RunTime() As System.Int32
        Get
            Return mvarRunTime
        End Get
        Set(ByVal Value As System.Int32)
            mvarRunTime = Value
        End Set
    End Property

    Public Property MaxTask() As System.Int32
        Get
            Return mvarMaxTask
        End Get
        Set(ByVal Value As System.Int32)
            mvarMaxTask = Value
        End Set
    End Property

    Public Property RefreshTime() As System.Int32
        Get
            Return mvarRefreshTime
        End Get
        Set(ByVal Value As System.Int32)
            mvarRefreshTime = Value
        End Set
    End Property

    Public Property FolderImportLocal() As System.String
        Get
            Return mvarFolderImportLocal
        End Get
        Set(ByVal Value As System.String)
            mvarFolderImportLocal = Value
        End Set
    End Property

    Public Property FolderInbox() As System.String
        Get
            Return mvarFolderInbox
        End Get
        Set(ByVal Value As System.String)
            mvarFolderInbox = Value
        End Set
    End Property

    Public Property FolderInboxBackup() As System.String
        Get
            Return mvarFolderInboxBackup
        End Get
        Set(ByVal Value As System.String)
            mvarFolderInboxBackup = Value
        End Set
    End Property

    Public Property FolderOutbox() As System.String
        Get
            Return mvarFolderOutbox
        End Get
        Set(ByVal Value As System.String)
            mvarFolderOutbox = Value
        End Set
    End Property

    Public Property FolderOutboxBackup() As System.String
        Get
            Return mvarFolderOutboxBackup
        End Get
        Set(ByVal Value As System.String)
            mvarFolderOutboxBackup = Value
        End Set
    End Property

    Public Property FolderUpload() As System.String
        Get
            Return mvarFolderUpload
        End Get
        Set(ByVal Value As System.String)
            mvarFolderUpload = Value
        End Set
    End Property

    Public Property FolderExport() As System.String
        Get
            Return mvarFolderExport
        End Get
        Set(ByVal Value As System.String)
            mvarFolderExport = Value
        End Set
    End Property

    Public Property ResetTime() As System.Int32
        Get
            Return mvarResetTime
        End Get
        Set(ByVal Value As System.Int32)
            mvarResetTime = Value
        End Set
    End Property

#End Region

#Region "Public Methods"
    Public Function Add() As Boolean
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "AddNew", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@SetupID", DbType.String, clsCommon.GetValueDBNull(Me.mvarSetupID), ParameterDirection.Input, 10))
            pc.Add(New ParamStruct("@IsSSL", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarIsSSL), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@PwdExpiredDays", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarPwdExpiredDays), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@DfltPassword", DbType.String, clsCommon.GetValueDBNull(Me.mvarDfltPassword), ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@RunTime", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarRunTime), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@MaxTask", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarMaxTask), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@RefreshTime", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarRefreshTime), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@FolderImportLocal", DbType.String, clsCommon.GetValueDBNull(Me.mvarFolderImportLocal), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@FolderInbox", DbType.String, clsCommon.GetValueDBNull(Me.mvarFolderInbox), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@FolderInboxBackup", DbType.String, clsCommon.GetValueDBNull(Me.mvarFolderInboxBackup), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@FolderOutbox", DbType.String, clsCommon.GetValueDBNull(Me.mvarFolderOutbox), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@FolderOutboxBackup", DbType.String, clsCommon.GetValueDBNull(Me.mvarFolderOutboxBackup), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@FolderUpload", DbType.String, clsCommon.GetValueDBNull(Me.mvarFolderUpload), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@FolderExport", DbType.String, clsCommon.GetValueDBNull(Me.mvarFolderExport), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@ResetTime", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarResetTime), ParameterDirection.Input, 4))
            DAL.ExecPreparedSQL(PP_ServerApplication_Setup, CommandType.StoredProcedure, pc, "")
            Me.mvarSetupID = clsCommon.GetValue(pc.Item("@SetupID").Value, mvarSetupID.GetType().FullName)
            Return (Me.mvarSetupID <> String.Empty)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function Update() As Boolean
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "Update", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@SetupID", DbType.String, clsCommon.GetValueDBNull(Me.mvarSetupID), ParameterDirection.Input, 10))
            pc.Add(New ParamStruct("@IsSSL", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarIsSSL), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@PwdExpiredDays", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarPwdExpiredDays), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@DfltPassword", DbType.String, clsCommon.GetValueDBNull(Me.mvarDfltPassword), ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@RunTime", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarRunTime), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@MaxTask", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarMaxTask), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@RefreshTime", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarRefreshTime), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@FolderImportLocal", DbType.String, clsCommon.GetValueDBNull(Me.mvarFolderImportLocal), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@FolderInbox", DbType.String, clsCommon.GetValueDBNull(Me.mvarFolderInbox), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@FolderInboxBackup", DbType.String, clsCommon.GetValueDBNull(Me.mvarFolderInboxBackup), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@FolderOutbox", DbType.String, clsCommon.GetValueDBNull(Me.mvarFolderOutbox), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@FolderOutboxBackup", DbType.String, clsCommon.GetValueDBNull(Me.mvarFolderOutboxBackup), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@FolderUpload", DbType.String, clsCommon.GetValueDBNull(Me.mvarFolderUpload), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@FolderExport", DbType.String, clsCommon.GetValueDBNull(Me.mvarFolderExport), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@ResetTime", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarResetTime), ParameterDirection.Input, 4))
            Return (DAL.ExecNonQuery(PP_ServerApplication_Setup, CommandType.StoredProcedure, pc, "") > 0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function Delete(ByVal SetupID As System.String) As Boolean
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@SetupID", DbType.String, clsCommon.GetValueDBNull(SetupID), ParameterDirection.Input, 10))
            Return (DAL.ExecNonQuery(PP_ServerApplication_Setup, CommandType.StoredProcedure, pc, "") > 0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function GetAll(ByVal SetupID As System.String) As DataTable
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            Dim ds As New DataSet
            pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@SetupID", DbType.String, clsCommon.GetValueDBNull(SetupID), ParameterDirection.Input, 10))
            ds = DAL.ExecDataSet(PP_ServerApplication_Setup, CommandType.StoredProcedure, pc, "")
            Dim keys(0) As DataColumn
            Dim column As DataColumn
            column = ds.Tables(0).Columns("SetupID")
            keys(0) = column
            ds.Tables(0).PrimaryKey = keys
            Return ds.Tables(0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Sub Reset()
        mvarSetupID = String.Empty
        mvarIsSSL = 0
        mvarPwdExpiredDays = 0
        mvarDfltPassword = String.Empty
        mvarRunTime = 0
        mvarMaxTask = 0
        mvarRefreshTime = 0
        mvarFolderImportLocal = String.Empty
        mvarFolderInbox = String.Empty
        mvarFolderInboxBackup = String.Empty
        mvarFolderOutbox = String.Empty
        mvarFolderOutboxBackup = String.Empty
        mvarFolderUpload = String.Empty
        mvarFolderExport = String.Empty
        mvarResetTime = 0
    End Sub
    Public Function GetByKey(ByVal SetupID As System.String) As Boolean
        Dim DAL As DataAccess = m_Dal
        Dim ds As New DataSet
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@SetupID", DbType.String, clsCommon.GetValueDBNull(SetupID), ParameterDirection.InputOutput, 10))
            ds = DAL.ExecDataSet(PP_ServerApplication_Setup, CommandType.StoredProcedure, pc, "")
            Me.Reset()
            If ds Is Nothing Then
                Return False
            End If
            If ds.Tables(0).Rows.Count > 0 Then
                FillData(ds.Tables(0).Rows(0))
                Return True
            End If
        Catch ex As Exception
            Throw ex
        End Try
        Return False
    End Function
    Public Sub FillData(row As DataRow)
        mvarSetupID = clsCommon.GetValue(row("SetupID"), mvarSetupID.GetType().FullName)
        mvarIsSSL = clsCommon.GetValue(row("IsSSL"), mvarIsSSL.GetType().FullName)
        mvarPwdExpiredDays = clsCommon.GetValue(row("PwdExpiredDays"), mvarPwdExpiredDays.GetType().FullName)
        mvarDfltPassword = clsCommon.GetValue(row("DfltPassword"), mvarDfltPassword.GetType().FullName)
        mvarRunTime = clsCommon.GetValue(row("RunTime"), mvarRunTime.GetType().FullName)
        mvarMaxTask = clsCommon.GetValue(row("MaxTask"), mvarMaxTask.GetType().FullName)
        mvarRefreshTime = clsCommon.GetValue(row("RefreshTime"), mvarRefreshTime.GetType().FullName)
        mvarFolderImportLocal = clsCommon.GetValue(row("FolderImportLocal"), mvarFolderImportLocal.GetType().FullName)
        mvarFolderInbox = clsCommon.GetValue(row("FolderInbox"), mvarFolderInbox.GetType().FullName)
        mvarFolderInboxBackup = clsCommon.GetValue(row("FolderInboxBackup"), mvarFolderInboxBackup.GetType().FullName)
        mvarFolderOutbox = clsCommon.GetValue(row("FolderOutbox"), mvarFolderOutbox.GetType().FullName)
        mvarFolderOutboxBackup = clsCommon.GetValue(row("FolderOutboxBackup"), mvarFolderOutboxBackup.GetType().FullName)
        mvarFolderUpload = clsCommon.GetValue(row("FolderUpload"), mvarFolderUpload.GetType().FullName)
        mvarFolderExport = clsCommon.GetValue(row("FolderExport"), mvarFolderExport.GetType().FullName)
        mvarResetTime = clsCommon.GetValue(row("ResetTime"), mvarResetTime.GetType().FullName)
    End Sub
#End Region

#Region "Private Methods"
#End Region

End Class
