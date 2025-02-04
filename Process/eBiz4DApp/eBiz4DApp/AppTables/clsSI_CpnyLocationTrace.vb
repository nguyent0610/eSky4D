'-- ------------------------------------------------------------
'-- Class name    :  clsSI_CpnyLocationTrace
'-- Created date  :  5/13/2015
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework.Common
Imports HQFramework.DAL
Public Class clsSI_CpnyLocationTrace
#Region "Constants"
	Private Const PP_SI_CpnyLocationTrace As String = "PP_SI_CpnyLocationTrace"
#End Region 

#Region "Member Variables"
	Private mvarBranchID As System.String

	Private mvarLat As System.Double

	Private mvarLng As System.Double

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
	Public Property BranchID() As System.String
		Get
			Return mvarBranchID
		End Get
		Set(ByVal Value As System.String)
			mvarBranchID = Value
		End Set
	End Property

	Public Property Lat() As System.Double
		Get
			Return mvarLat
		End Get
		Set(ByVal Value As System.Double)
			mvarLat = Value
		End Set
	End Property

	Public Property Lng() As System.Double
		Get
			Return mvarLng
		End Get
		Set(ByVal Value As System.Double)
			mvarLng = Value
		End Set
	End Property

#End Region 

#Region "Public Methods"
	Public Function Add() as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "AddNew", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String,clsCommon.GetValueDBNull(Me.mvarBranchID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@Lat", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarLat), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@Lng", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarLng), ParameterDirection.Input,8 ))
			DAL.ExecPreparedSQL(PP_SI_CpnyLocationTrace, CommandType.StoredProcedure, pc,"")
		Me.mvarBranchID = clsCommon.GetValue(pc.Item("@BranchID").Value, mvarBranchID.GetType().FullName)
		Return (Me.mvarBranchID <> String.Empty )
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Update() as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Update", ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@BranchID",DbType.String, clsCommon.GetValueDBNull(me.mvarBranchID), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@Lat",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarLat), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@Lng",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarLng), ParameterDirection.Input,8 ))
			Return (DAL.ExecNonQuery(PP_SI_CpnyLocationTrace, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Delete(ByVal BranchID As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID",DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input,30 ))
			Return (DAL.ExecNonQuery(PP_SI_CpnyLocationTrace, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function GetAll(ByVal BranchID As System.String) as DataTable 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			Dim ds As New DataSet
			pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 30 ))
			ds = DAL.ExecDataSet(PP_SI_CpnyLocationTrace, CommandType.StoredProcedure, pc,"")
			Dim keys(0) As DataColumn
			Dim column As DataColumn
			column = ds.Tables(0).Columns("BranchID")
			Keys(0) = column
			ds.Tables(0).PrimaryKey = Keys
			Return ds.Tables(0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Sub Reset()
		mvarBranchID = String.Empty
		mvarLat = 0
		mvarLng = 0
	End Sub
	Public Function GetByKey(ByVal BranchID As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Dim ds As New DataSet 
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.InputOutput, 30 ))
			ds = DAL.ExecDataSet(PP_SI_CpnyLocationTrace, CommandType.StoredProcedure, pc,"")
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
		mvarBranchID =  clsCommon.GetValue(row("BranchID"), mvarBranchID.GetType().FullName)
		mvarLat =  clsCommon.GetValue(row("Lat"), mvarLat.GetType().FullName)
		mvarLng =  clsCommon.GetValue(row("Lng"), mvarLng.GetType().FullName)
	End Sub
#End Region 

#Region "Private Methods"
#End Region 

End Class
