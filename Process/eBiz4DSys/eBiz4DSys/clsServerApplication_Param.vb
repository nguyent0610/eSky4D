'-- ------------------------------------------------------------
'-- Class name    :  clsServerApplication_Param
'-- Created date  :  12/11/2012
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework
Imports HQFramework.DAL
Imports HQFramework.Common

Public Class clsServerApplication_Param
#Region "Constants"
    Private Const PP_ServerApplication_Param As String = "PP_ServerApplication_Param"
#End Region

#Region "Member Variables"
    Private mvarType As System.String

    Private mvarParm01 As System.String

    Private mvarParm02 As System.String

    Private mvarParm03 As System.String

    Private mvarParm04 As System.String

    Private mvarParm05 As System.String

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
    Public Property Type() As System.String
        Get
            Return mvarType
        End Get
        Set(ByVal Value As System.String)
            mvarType = Value
        End Set
    End Property

    Public Property Parm01() As System.String
        Get
            Return mvarParm01
        End Get
        Set(ByVal Value As System.String)
            mvarParm01 = Value
        End Set
    End Property

    Public Property Parm02() As System.String
        Get
            Return mvarParm02
        End Get
        Set(ByVal Value As System.String)
            mvarParm02 = Value
        End Set
    End Property

    Public Property Parm03() As System.String
        Get
            Return mvarParm03
        End Get
        Set(ByVal Value As System.String)
            mvarParm03 = Value
        End Set
    End Property

    Public Property Parm04() As System.String
        Get
            Return mvarParm04
        End Get
        Set(ByVal Value As System.String)
            mvarParm04 = Value
        End Set
    End Property

    Public Property Parm05() As System.String
        Get
            Return mvarParm05
        End Get
        Set(ByVal Value As System.String)
            mvarParm05 = Value
        End Set
    End Property

#End Region

#Region "Public Methods"
    Public Function Add() As Boolean
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "AddNew", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@Type", DbType.String, clsCommon.GetValueDBNull(Me.mvarType), ParameterDirection.Input, 20))
            pc.Add(New ParamStruct("@Parm01", DbType.String, clsCommon.GetValueDBNull(Me.mvarParm01), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@Parm02", DbType.String, clsCommon.GetValueDBNull(Me.mvarParm02), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@Parm03", DbType.String, clsCommon.GetValueDBNull(Me.mvarParm03), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@Parm04", DbType.String, clsCommon.GetValueDBNull(Me.mvarParm04), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@Parm05", DbType.String, clsCommon.GetValueDBNull(Me.mvarParm05), ParameterDirection.Input, 500))
            DAL.ExecPreparedSQL(PP_ServerApplication_Param, CommandType.StoredProcedure, pc, "")
            Me.mvarType = clsCommon.GetValue(pc.Item("@Type").Value, mvarType.GetType().FullName)
            Return (Me.mvarType <> String.Empty)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function Update() As Boolean
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "Update", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@Type", DbType.String, clsCommon.GetValueDBNull(Me.mvarType), ParameterDirection.Input, 20))
            pc.Add(New ParamStruct("@Parm01", DbType.String, clsCommon.GetValueDBNull(Me.mvarParm01), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@Parm02", DbType.String, clsCommon.GetValueDBNull(Me.mvarParm02), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@Parm03", DbType.String, clsCommon.GetValueDBNull(Me.mvarParm03), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@Parm04", DbType.String, clsCommon.GetValueDBNull(Me.mvarParm04), ParameterDirection.Input, 500))
            pc.Add(New ParamStruct("@Parm05", DbType.String, clsCommon.GetValueDBNull(Me.mvarParm05), ParameterDirection.Input, 500))
            Return (DAL.ExecNonQuery(PP_ServerApplication_Param, CommandType.StoredProcedure, pc, "") > 0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function Delete(ByVal Type As System.String) As Boolean
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@Type", DbType.String, clsCommon.GetValueDBNull(Type), ParameterDirection.Input, 20))
            Return (DAL.ExecNonQuery(PP_ServerApplication_Param, CommandType.StoredProcedure, pc, "") > 0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Function GetAll(ByVal Type As System.String) As DataTable
        Dim DAL As DataAccess = m_Dal
        Try
            Dim pc As New ParamCollection
            Dim ds As New DataSet
            pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@Type", DbType.String, clsCommon.GetValueDBNull(Type), ParameterDirection.Input, 20))
            ds = DAL.ExecDataSet(PP_ServerApplication_Param, CommandType.StoredProcedure, pc, "")
            Dim keys(0) As DataColumn
            Dim column As DataColumn
            column = ds.Tables(0).Columns("Type")
            keys(0) = column
            ds.Tables(0).PrimaryKey = keys
            Return ds.Tables(0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function
    Public Sub Reset()
        mvarType = String.Empty
        mvarParm01 = String.Empty
        mvarParm02 = String.Empty
        mvarParm03 = String.Empty
        mvarParm04 = String.Empty
        mvarParm05 = String.Empty
    End Sub
    Public Function GetByKey(ByVal Type As System.String) As Boolean
        Dim DAL As DataAccess = m_Dal
        Dim ds As New DataSet
        Try
            Dim pc As New ParamCollection
            pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input, 50))
            pc.Add(New ParamStruct("@Type", DbType.String, clsCommon.GetValueDBNull(Type), ParameterDirection.InputOutput, 20))
            ds = DAL.ExecDataSet(PP_ServerApplication_Param, CommandType.StoredProcedure, pc, "")
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
        mvarType = clsCommon.GetValue(row("Type"), mvarType.GetType().FullName)
        mvarParm01 = clsCommon.GetValue(row("Parm01"), mvarParm01.GetType().FullName)
        mvarParm02 = clsCommon.GetValue(row("Parm02"), mvarParm02.GetType().FullName)
        mvarParm03 = clsCommon.GetValue(row("Parm03"), mvarParm03.GetType().FullName)
        mvarParm04 = clsCommon.GetValue(row("Parm04"), mvarParm04.GetType().FullName)
        mvarParm05 = clsCommon.GetValue(row("Parm05"), mvarParm05.GetType().FullName)
    End Sub
#End Region

#Region "Private Methods"
#End Region

End Class
