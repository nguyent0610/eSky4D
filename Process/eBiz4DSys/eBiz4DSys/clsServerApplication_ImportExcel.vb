'-- ------------------------------------------------------------
'-- Class name    :  clsServerApplication_ImportExcel
'-- Created date  :  12/11/2012
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework
Imports HQFramework.DAL
Imports HQFramework.Common

Public Class clsServerApplication_ImportExcel
#Region "Constants"
    Private Const PP_ServerApplication_ImportExcel As String = "PP_ServerApplication_ImportExcel"
#End Region

#Region "Member Variables"
    Private mvarID As System.String

    Private mvarTableName As System.String

    Private mvarCell As System.String

    Private mvarFieldName As System.String

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

    Public Property TableName() As System.String
        Get
            Return mvarTableName
        End Get
        Set(ByVal Value As System.String)
            mvarTableName = Value
        End Set
    End Property

    Public Property Cell() As System.String
        Get
            Return mvarCell
        End Get
        Set(ByVal Value As System.String)
            mvarCell = Value
        End Set
    End Property

    Public Property FieldName() As System.String
        Get
            Return mvarFieldName
        End Get
        Set(ByVal Value As System.String)
            mvarFieldName = Value
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
            pc.Add(New ParamStruct("@TableName", DbType.String, clsCommon.GetValueDBNull(Me.mvarTableName), ParameterDirection.Input, 100))
            pc.Add(New ParamStruct("@Cell", DbType.String, clsCommon.GetValueDBNull(Me.mvarCell), ParameterDirection.Input, 20))
            pc.Add(New ParamStruct("@FieldName", DbType.String, clsCommon.GetValueDBNull(Me.mvarFieldName), ParameterDirection.Input, 100))
            DAL.ExecPreparedSQL(PP_ServerApplication_ImportExcel, CommandType.StoredProcedure, pc, "")
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
            pc.Add(New ParamStruct("@TableName", DbType.String, clsCommon.GetValueDBNull(Me.mvarTableName), ParameterDirection.Input, 100))
            pc.Add(New ParamStruct("@Cell", DbType.String, clsCommon.GetValueDBNull(Me.mvarCell), ParameterDirection.Input, 20))
            pc.Add(New ParamStruct("@FieldName", DbType.String, clsCommon.GetValueDBNull(Me.mvarFieldName), ParameterDirection.Input, 100))
            Return (DAL.ExecNonQuery(PP_ServerApplication_ImportExcel, CommandType.StoredProcedure, pc, "") > 0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function Delete(ByVal ID As System.String, ByVal TableName As System.String, ByVal Cell As System.String) As Boolean
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@ID", DbType.String, clsCommon.GetValueDBNull(ID), ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@TableName", DbType.String, clsCommon.GetValueDBNull(TableName), ParameterDirection.Input, 100))
            pc.Add(New ParamStruct("@Cell", DbType.String, clsCommon.GetValueDBNull(Cell), ParameterDirection.Input, 20))
            Return (DAL.ExecNonQuery(PP_ServerApplication_ImportExcel, CommandType.StoredProcedure, pc, "") > 0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function GetAll(ByVal ID As System.String, ByVal TableName As System.String, ByVal Cell As System.String) As DataTable
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            Dim ds As New DataSet
            pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@ID", DbType.String, clsCommon.GetValueDBNull(ID), ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@TableName", DbType.String, clsCommon.GetValueDBNull(TableName), ParameterDirection.Input, 100))
            pc.Add(New ParamStruct("@Cell", DbType.String, clsCommon.GetValueDBNull(Cell), ParameterDirection.Input, 20))
            ds = DAL.ExecDataSet(PP_ServerApplication_ImportExcel, CommandType.StoredProcedure, pc, "")
            Dim keys(2) As DataColumn
            Dim column As DataColumn
            column = ds.Tables(0).Columns("ID")
            keys(0) = column
            column = ds.Tables(0).Columns("TableName")
            keys(1) = column
            column = ds.Tables(0).Columns("Cell")
            keys(2) = column
            ds.Tables(0).PrimaryKey = keys
            Return ds.Tables(0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Sub Reset()
        mvarID = String.Empty
        mvarTableName = String.Empty
        mvarCell = String.Empty
        mvarFieldName = String.Empty
    End Sub
    Public Function GetByKey(ByVal ID As System.String, ByVal TableName As System.String, ByVal Cell As System.String) As Boolean
        Dim DAL As DataAccess = m_Dal
        Dim ds As New DataSet
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@ID", DbType.String, clsCommon.GetValueDBNull(ID), ParameterDirection.InputOutput, 50))
            pc.Add(New ParamStruct("@TableName", DbType.String, clsCommon.GetValueDBNull(TableName), ParameterDirection.InputOutput, 100))
            pc.Add(New ParamStruct("@Cell", DbType.String, clsCommon.GetValueDBNull(Cell), ParameterDirection.InputOutput, 20))
            ds = DAL.ExecDataSet(PP_ServerApplication_ImportExcel, CommandType.StoredProcedure, pc, "")
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
        mvarTableName = clsCommon.GetValue(row("TableName"), mvarTableName.GetType().FullName)
        mvarCell = clsCommon.GetValue(row("Cell"), mvarCell.GetType().FullName)
        mvarFieldName = clsCommon.GetValue(row("FieldName"), mvarFieldName.GetType().FullName)
    End Sub
#End Region

#Region "Private Methods"
#End Region

End Class
