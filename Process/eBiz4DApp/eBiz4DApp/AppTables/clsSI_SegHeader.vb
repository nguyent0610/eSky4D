'-- ------------------------------------------------------------
'-- Class name    :  clsSI_SegHeader
'-- Created date  :  5/13/2015
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework.Common
Imports HQFramework.DAL
Public Class clsSI_SegHeader
#Region "Constants"
	Private Const PP_SI_SegHeader As String = "PP_SI_SegHeader"
#End Region 

#Region "Member Variables"
	Private mvarSegTypeID As System.Int16

	Private mvarSeg As System.Int16

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
	Public Property SegTypeID() As System.Int16
		Get
			Return mvarSegTypeID
		End Get
		Set(ByVal Value As System.Int16)
			mvarSegTypeID = Value
		End Set
	End Property

	Public Property Seg() As System.Int16
		Get
			Return mvarSeg
		End Get
		Set(ByVal Value As System.Int16)
			mvarSeg = Value
		End Set
	End Property

#End Region 

#Region "Public Methods"
	Public Function Add() as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "AddNew", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@SegTypeID", DbType.int16,clsCommon.GetValueDBNull(Me.mvarSegTypeID), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@Seg", DbType.int16,clsCommon.GetValueDBNull(Me.mvarSeg), ParameterDirection.Input,2 ))
			DAL.ExecPreparedSQL(PP_SI_SegHeader, CommandType.StoredProcedure, pc,"")
		Me.mvarSegTypeID = clsCommon.GetValue(pc.Item("@SegTypeID").Value, mvarSegTypeID.GetType().FullName)
		Return True
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Update() as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Update", ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@SegTypeID",DbType.int16, clsCommon.GetValueDBNull(me.mvarSegTypeID), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@Seg",DbType.int16, clsCommon.GetValueDBNull(me.mvarSeg), ParameterDirection.Input,2 ))
			Return (DAL.ExecNonQuery(PP_SI_SegHeader, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Delete(ByVal SegTypeID As System.Int16, ByVal Seg As System.Int16) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@SegTypeID",DbType.int16, clsCommon.GetValueDBNull(SegTypeID), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@Seg",DbType.int16, clsCommon.GetValueDBNull(Seg), ParameterDirection.Input,2 ))
			Return (DAL.ExecNonQuery(PP_SI_SegHeader, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function GetAll(ByVal SegTypeID As System.Int16, ByVal Seg As System.Int16) as DataTable 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			Dim ds As New DataSet
			pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@SegTypeID", DbType.int16, clsCommon.GetValueDBNull(SegTypeID), ParameterDirection.Input, 2 ))
			pc.Add(New ParamStruct("@Seg", DbType.int16, clsCommon.GetValueDBNull(Seg), ParameterDirection.Input, 2 ))
			ds = DAL.ExecDataSet(PP_SI_SegHeader, CommandType.StoredProcedure, pc,"")
			Dim keys(1) As DataColumn
			Dim column As DataColumn
			column = ds.Tables(0).Columns("SegTypeID")
			Keys(0) = column
			column = ds.Tables(0).Columns("Seg")
			Keys(1) = column
			ds.Tables(0).PrimaryKey = Keys
			Return ds.Tables(0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Sub Reset()
		mvarSegTypeID = 0
		mvarSeg = 0
	End Sub
	Public Function GetByKey(ByVal SegTypeID As System.Int16, ByVal Seg As System.Int16) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Dim ds As New DataSet 
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@SegTypeID", DbType.int16, clsCommon.GetValueDBNull(SegTypeID), ParameterDirection.InputOutput, 2 ))
			pc.Add(New ParamStruct("@Seg", DbType.int16, clsCommon.GetValueDBNull(Seg), ParameterDirection.InputOutput, 2 ))
			ds = DAL.ExecDataSet(PP_SI_SegHeader, CommandType.StoredProcedure, pc,"")
			me.Reset()
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
	Public Sub FillData(row as DataRow)
		mvarSegTypeID =  clsCommon.GetValue(row("SegTypeID"), mvarSegTypeID.GetType().FullName)
		mvarSeg =  clsCommon.GetValue(row("Seg"), mvarSeg.GetType().FullName)
	End Sub
#End Region 

#Region "Private Methods"
#End Region 

End Class
