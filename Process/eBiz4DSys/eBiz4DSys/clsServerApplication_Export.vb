'-- ------------------------------------------------------------
'-- Class name    :  clsServerApplication_Export
'-- Created date  :  12/11/2012
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework
Imports HQFramework.DAL
Imports HQFramework.Common

Public Class clsServerApplication_Export
#Region "Constants"
    Private Const PP_ServerApplication_Export As String = "PP_ServerApplication_Export"
#End Region

#Region "Member Variables"
    Private mvarID As System.String

    Private mvarIsActive As System.Int32

    Private mvarPriority As System.Int32

    Private mvarPlanSync As System.Boolean

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
    Public Property ID() As System.String
        Get
            Return mvarID
        End Get
        Set(ByVal Value As System.String)
            mvarID = Value
        End Set
    End Property

    Public Property IsActive() As System.Int32
        Get
            Return mvarIsActive
        End Get
        Set(ByVal Value As System.Int32)
            mvarIsActive = Value
        End Set
    End Property

    Public Property Priority() As System.Int32
        Get
            Return mvarPriority
        End Get
        Set(ByVal Value As System.Int32)
            mvarPriority = Value
        End Set
    End Property

    Public Property PlanSync() As System.Boolean
        Get
            Return mvarPlanSync
        End Get
        Set(ByVal Value As System.Boolean)
            mvarPlanSync = Value
        End Set
    End Property

#End Region

#Region "Public Methods"
    Public Function Add() As Boolean
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "AddNew", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@ID", DbType.String, clsCommon.GetValueDBNull(Me.mvarID), ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@IsActive", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarIsActive), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@Priority", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarPriority), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@PlanSync", DbType.Boolean, clsCommon.GetValueDBNull(Me.mvarPlanSync), ParameterDirection.Input, 2))
            DAL.ExecPreparedSQL(PP_ServerApplication_Export, CommandType.StoredProcedure, pc, "")
            Me.mvarID = clsCommon.GetValue(pc.Item("@ID").Value, mvarID.GetType().FullName)
            Return (Me.mvarID <> String.Empty)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function Update() As Boolean
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "Update", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@ID", DbType.String, clsCommon.GetValueDBNull(Me.mvarID), ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@IsActive", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarIsActive), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@Priority", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarPriority), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@PlanSync", DbType.Boolean, clsCommon.GetValueDBNull(Me.mvarPlanSync), ParameterDirection.Input, 2))
            Return (DAL.ExecNonQuery(PP_ServerApplication_Export, CommandType.StoredProcedure, pc, "") > 0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function Delete(ByVal ID As System.String) As Boolean
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@ID", DbType.String, clsCommon.GetValueDBNull(ID), ParameterDirection.Input, 50))
            Return (DAL.ExecNonQuery(PP_ServerApplication_Export, CommandType.StoredProcedure, pc, "") > 0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function GetAll(ByVal ID As System.String) As DataTable
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            Dim ds As New DataSet
            pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@ID", DbType.String, clsCommon.GetValueDBNull(ID), ParameterDirection.Input, 50))
            ds = DAL.ExecDataSet(PP_ServerApplication_Export, CommandType.StoredProcedure, pc, "")
            Dim keys(0) As DataColumn
            Dim column As DataColumn
            column = ds.Tables(0).Columns("ID")
            keys(0) = column
            ds.Tables(0).PrimaryKey = keys
            Return ds.Tables(0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Sub Reset()
        mvarID = String.Empty
        mvarIsActive = 0
        mvarPriority = 0
        mvarPlanSync = False
    End Sub
    Public Function GetByKey(ByVal ID As System.String) As Boolean
        Dim DAL As DataAccess = m_Dal
        Dim ds As New DataSet
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@ID", DbType.String, clsCommon.GetValueDBNull(ID), ParameterDirection.InputOutput, 50))
            ds = DAL.ExecDataSet(PP_ServerApplication_Export, CommandType.StoredProcedure, pc, "")
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
        mvarID = clsCommon.GetValue(row("ID"), mvarID.GetType().FullName)
        mvarIsActive = clsCommon.GetValue(row("IsActive"), mvarIsActive.GetType().FullName)
        mvarPriority = clsCommon.GetValue(row("Priority"), mvarPriority.GetType().FullName)
        mvarPlanSync = clsCommon.GetValue(row("PlanSync"), mvarPlanSync.GetType().FullName)
    End Sub
#End Region

#Region "Private Methods"
#End Region

End Class
