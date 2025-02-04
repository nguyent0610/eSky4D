'-- ------------------------------------------------------------
'-- Class name    :  clsServerApp_Sync
'-- Created date  :  12/11/2012
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework
Imports HQFramework.DAL
Imports HQFramework.Common

Public Class clsServerApp_Sync
#Region "Constants"
    Private Const PP_ServerApp_Sync As String = "PP_ServerApp_Sync"
#End Region

#Region "Member Variables"
    Private mvarSyncID As System.Int32

    Private mvarID As System.String

    Private mvarBranchID As System.String

    Private mvarStatus As System.String

    Private mvarExportDate As System.DateTime

    Private mvarImportDate As System.DateTime

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
    Public Property SyncID() As System.Int32
        Get
            Return mvarSyncID
        End Get
        Set(ByVal Value As System.Int32)
            mvarSyncID = Value
        End Set
    End Property

    Public Property ID() As System.String
        Get
            Return mvarID
        End Get
        Set(ByVal Value As System.String)
            mvarID = Value
        End Set
    End Property

    Public Property BranchID() As System.String
        Get
            Return mvarBranchID
        End Get
        Set(ByVal Value As System.String)
            mvarBranchID = Value
        End Set
    End Property

    Public Property Status() As System.String
        Get
            Return mvarStatus
        End Get
        Set(ByVal Value As System.String)
            mvarStatus = Value
        End Set
    End Property

    Public Property ExportDate() As System.DateTime
        Get
            Return mvarExportDate
        End Get
        Set(ByVal Value As System.DateTime)
            mvarExportDate = Value
        End Set
    End Property

    Public Property ImportDate() As System.DateTime
        Get
            Return mvarImportDate
        End Get
        Set(ByVal Value As System.DateTime)
            mvarImportDate = Value
        End Set
    End Property

#End Region

#Region "Public Methods"
    Public Function Add() As Boolean
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "AddNew", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@SyncID", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarSyncID), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@ID", DbType.String, clsCommon.GetValueDBNull(Me.mvarID), ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(Me.mvarBranchID), ParameterDirection.Input, 30))
            pc.Add(New ParamStruct("@Status", DbType.String, clsCommon.GetValueDBNull(Me.mvarStatus), ParameterDirection.Input, 10))
            pc.Add(New ParamStruct("@ExportDate", DbType.DateTime, clsCommon.GetValueDBNull(Me.mvarExportDate), ParameterDirection.Input, 16))
            pc.Add(New ParamStruct("@ImportDate", DbType.DateTime, clsCommon.GetValueDBNull(Me.mvarImportDate), ParameterDirection.Input, 16))
            DAL.ExecPreparedSQL(PP_ServerApp_Sync, CommandType.StoredProcedure, pc, "")
            Me.mvarSyncID = clsCommon.GetValue(pc.Item("@SyncID").Value, mvarSyncID.GetType().FullName)
            Return True
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function Update() As Boolean
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "Update", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@SyncID", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarSyncID), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@ID", DbType.String, clsCommon.GetValueDBNull(Me.mvarID), ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(Me.mvarBranchID), ParameterDirection.Input, 30))
            pc.Add(New ParamStruct("@Status", DbType.String, clsCommon.GetValueDBNull(Me.mvarStatus), ParameterDirection.Input, 10))
            pc.Add(New ParamStruct("@ExportDate", DbType.DateTime, clsCommon.GetValueDBNull(Me.mvarExportDate), ParameterDirection.Input, 16))
            pc.Add(New ParamStruct("@ImportDate", DbType.DateTime, clsCommon.GetValueDBNull(Me.mvarImportDate), ParameterDirection.Input, 16))
            Return (DAL.ExecNonQuery(PP_ServerApp_Sync, CommandType.StoredProcedure, pc, "") > 0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function Delete(ByVal SyncID As System.Int32) As Boolean
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@SyncID", DbType.Int32, clsCommon.GetValueDBNull(SyncID), ParameterDirection.Input, 4))
            Return (DAL.ExecNonQuery(PP_ServerApp_Sync, CommandType.StoredProcedure, pc, "") > 0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function GetAll(ByVal SyncID As System.Int32) As DataTable
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            Dim ds As New DataSet
            pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@SyncID", DbType.Int32, clsCommon.GetValueDBNull(SyncID), ParameterDirection.Input, 4))
            ds = DAL.ExecDataSet(PP_ServerApp_Sync, CommandType.StoredProcedure, pc, "")
            Dim keys(0) As DataColumn
            Dim column As DataColumn
            column = ds.Tables(0).Columns("SyncID")
            keys(0) = column
            ds.Tables(0).PrimaryKey = keys
            Return ds.Tables(0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Sub Reset()
        mvarSyncID = 0
        mvarID = String.Empty
        mvarBranchID = String.Empty
        mvarStatus = String.Empty
        mvarExportDate = Today
        mvarImportDate = Today
    End Sub
    Public Function GetByKey(ByVal SyncID As System.Int32) As Boolean
        Dim DAL As DataAccess = m_Dal
        Dim ds As New DataSet
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@SyncID", DbType.Int32, clsCommon.GetValueDBNull(SyncID), ParameterDirection.InputOutput, 4))
            ds = DAL.ExecDataSet(PP_ServerApp_Sync, CommandType.StoredProcedure, pc, "")
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
        mvarSyncID = clsCommon.GetValue(row("SyncID"), mvarSyncID.GetType().FullName)
        mvarID = clsCommon.GetValue(row("ID"), mvarID.GetType().FullName)
        mvarBranchID = clsCommon.GetValue(row("BranchID"), mvarBranchID.GetType().FullName)
        mvarStatus = clsCommon.GetValue(row("Status"), mvarStatus.GetType().FullName)
        mvarExportDate = clsCommon.GetValue(row("ExportDate"), mvarExportDate.GetType().FullName)
        mvarImportDate = clsCommon.GetValue(row("ImportDate"), mvarImportDate.GetType().FullName)
    End Sub
#End Region

#Region "Private Methods"
#End Region

End Class
