'-- ------------------------------------------------------------
'-- Class name    :  clsSYS_CloseDateBranchAuto
'-- Created date  :  12/11/2012
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework
Imports HQFramework.DAL
Imports HQFramework.Common

Public Class clsSYS_CloseDateBranchAuto
#Region "Constants"
    Private Const PP_SYS_CloseDateBranchAuto As String = "PP_SYS_CloseDateBranchAuto"
#End Region

#Region "Member Variables"
    Private mvarID As System.Int32

    Private mvarBranchID As System.String

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
    Public Property ID() As System.Int32
        Get
            Return mvarID
        End Get
        Set(ByVal Value As System.Int32)
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

#End Region

#Region "Public Methods"
    Public Function Add() As Boolean
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "AddNew", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@ID", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarID), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(Me.mvarBranchID), ParameterDirection.Input, 20))
            DAL.ExecPreparedSQL(PP_SYS_CloseDateBranchAuto, CommandType.StoredProcedure, pc, "")
            Me.mvarID = clsCommon.GetValue(pc.Item("@ID").Value, mvarID.GetType().FullName)
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
            pc.Add(New ParamStruct("@ID", DbType.Int32, clsCommon.GetValueDBNull(Me.mvarID), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(Me.mvarBranchID), ParameterDirection.Input, 20))
            Return (DAL.ExecNonQuery(PP_SYS_CloseDateBranchAuto, CommandType.StoredProcedure, pc, "") > 0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function Delete(ByVal ID As System.Int32, ByVal BranchID As System.String) As Boolean
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@ID", DbType.Int32, clsCommon.GetValueDBNull(ID), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 20))
            Return (DAL.ExecNonQuery(PP_SYS_CloseDateBranchAuto, CommandType.StoredProcedure, pc, "") > 0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function GetAll(ByVal ID As System.Int32, ByVal BranchID As System.String) As DataTable
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            Dim ds As New DataSet
            pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@ID", DbType.Int32, clsCommon.GetValueDBNull(ID), ParameterDirection.Input, 4))
            pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 20))
            ds = DAL.ExecDataSet(PP_SYS_CloseDateBranchAuto, CommandType.StoredProcedure, pc, "")
            Dim keys(1) As DataColumn
            Dim column As DataColumn
            column = ds.Tables(0).Columns("ID")
            keys(0) = column
            column = ds.Tables(0).Columns("BranchID")
            keys(1) = column
            ds.Tables(0).PrimaryKey = keys
            Return ds.Tables(0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Sub Reset()
        mvarID = 0
        mvarBranchID = String.Empty
    End Sub
    Public Function GetByKey(ByVal ID As System.Int32, ByVal BranchID As System.String) As Boolean
        Dim DAL As DataAccess = m_Dal
        Dim ds As New DataSet
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@ID", DbType.Int32, clsCommon.GetValueDBNull(ID), ParameterDirection.InputOutput, 4))
            pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.InputOutput, 20))
            ds = DAL.ExecDataSet(PP_SYS_CloseDateBranchAuto, CommandType.StoredProcedure, pc, "")
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
        mvarBranchID = clsCommon.GetValue(row("BranchID"), mvarBranchID.GetType().FullName)
    End Sub
#End Region

#Region "Private Methods"
#End Region

End Class
