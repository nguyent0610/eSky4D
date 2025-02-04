'-- ------------------------------------------------------------
'-- Class name    :  clsOM_DiscCust
'-- Created date  :  5/13/2015
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework.Common
Imports HQFramework.DAL
Public Class clsOM_DiscCust
#Region "Constants"
	Private Const PP_OM_DiscCust As String = "PP_OM_DiscCust"
#End Region 

#Region "Member Variables"
	Private mvarDiscID As System.String

	Private mvarDiscSeq As System.String

	Private mvarCustID As System.String

	Private mvarCrtd_DateTime As System.DateTime

	Private mvarCrtd_Prog As System.String

	Private mvarCrtd_User As System.String

	Private mvarLUpd_DateTime As System.DateTime

	Private mvarLUpd_Prog As System.String

	Private mvarLUpd_User As System.String

	Private mvartstamp As System.String

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
	Public Property DiscID() As System.String
		Get
			Return mvarDiscID
		End Get
		Set(ByVal Value As System.String)
			mvarDiscID = Value
		End Set
	End Property

	Public Property DiscSeq() As System.String
		Get
			Return mvarDiscSeq
		End Get
		Set(ByVal Value As System.String)
			mvarDiscSeq = Value
		End Set
	End Property

	Public Property CustID() As System.String
		Get
			Return mvarCustID
		End Get
		Set(ByVal Value As System.String)
			mvarCustID = Value
		End Set
	End Property

	Public Property Crtd_DateTime() As System.DateTime
		Get
			Return mvarCrtd_DateTime
		End Get
		Set(ByVal Value As System.DateTime)
			mvarCrtd_DateTime = Value
		End Set
	End Property

	Public Property Crtd_Prog() As System.String
		Get
			Return mvarCrtd_Prog
		End Get
		Set(ByVal Value As System.String)
			mvarCrtd_Prog = Value
		End Set
	End Property

	Public Property Crtd_User() As System.String
		Get
			Return mvarCrtd_User
		End Get
		Set(ByVal Value As System.String)
			mvarCrtd_User = Value
		End Set
	End Property

	Public Property LUpd_DateTime() As System.DateTime
		Get
			Return mvarLUpd_DateTime
		End Get
		Set(ByVal Value As System.DateTime)
			mvarLUpd_DateTime = Value
		End Set
	End Property

	Public Property LUpd_Prog() As System.String
		Get
			Return mvarLUpd_Prog
		End Get
		Set(ByVal Value As System.String)
			mvarLUpd_Prog = Value
		End Set
	End Property

	Public Property LUpd_User() As System.String
		Get
			Return mvarLUpd_User
		End Get
		Set(ByVal Value As System.String)
			mvarLUpd_User = Value
		End Set
	End Property

	Public Property tstamp() As System.String
		Get
			Return mvartstamp
		End Get
		Set(ByVal Value As System.String)
			mvartstamp = Value
		End Set
	End Property

#End Region 

#Region "Public Methods"
	Public Function Add() as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "AddNew", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@DiscID", DbType.String,clsCommon.GetValueDBNull(Me.mvarDiscID), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@DiscSeq", DbType.String,clsCommon.GetValueDBNull(Me.mvarDiscSeq), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@CustID", DbType.String,clsCommon.GetValueDBNull(Me.mvarCustID), ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@Crtd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@Crtd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@Crtd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@LUpd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@LUpd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@LUpd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@tstamp", DbType.String,clsCommon.GetValueDBNull(Me.mvartstamp), ParameterDirection.Input,18 ))
			DAL.ExecPreparedSQL(PP_OM_DiscCust, CommandType.StoredProcedure, pc,"")
		Me.mvarDiscID = clsCommon.GetValue(pc.Item("@DiscID").Value, mvarDiscID.GetType().FullName)
		Return (Me.mvarDiscID <> String.Empty )
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Update() as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Update", ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@DiscID",DbType.String, clsCommon.GetValueDBNull(me.mvarDiscID), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@DiscSeq",DbType.String, clsCommon.GetValueDBNull(me.mvarDiscSeq), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@CustID",DbType.String, clsCommon.GetValueDBNull(me.mvarCustID), ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@Crtd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@Crtd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@Crtd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@LUpd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@LUpd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@LUpd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@tstamp",DbType.String, clsCommon.GetValueDBNull(me.mvartstamp), ParameterDirection.Input,18 ))
			Return (DAL.ExecNonQuery(PP_OM_DiscCust, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Delete(ByVal DiscID As System.String, ByVal DiscSeq As System.String, ByVal CustID As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@DiscID",DbType.String, clsCommon.GetValueDBNull(DiscID), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@DiscSeq",DbType.String, clsCommon.GetValueDBNull(DiscSeq), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@CustID",DbType.String, clsCommon.GetValueDBNull(CustID), ParameterDirection.Input,50 ))
			Return (DAL.ExecNonQuery(PP_OM_DiscCust, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function GetAll(ByVal DiscID As System.String, ByVal DiscSeq As System.String, ByVal CustID As System.String) as DataTable 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			Dim ds As New DataSet
			pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@DiscID", DbType.String, clsCommon.GetValueDBNull(DiscID), ParameterDirection.Input, 10 ))
			pc.Add(New ParamStruct("@DiscSeq", DbType.String, clsCommon.GetValueDBNull(DiscSeq), ParameterDirection.Input, 10 ))
			pc.Add(New ParamStruct("@CustID", DbType.String, clsCommon.GetValueDBNull(CustID), ParameterDirection.Input, 50 ))
			ds = DAL.ExecDataSet(PP_OM_DiscCust, CommandType.StoredProcedure, pc,"")
			Dim keys(2) As DataColumn
			Dim column As DataColumn
			column = ds.Tables(0).Columns("DiscID")
			Keys(0) = column
			column = ds.Tables(0).Columns("DiscSeq")
			Keys(1) = column
			column = ds.Tables(0).Columns("CustID")
			Keys(2) = column
			ds.Tables(0).PrimaryKey = Keys
			Return ds.Tables(0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Sub Reset()
		mvarDiscID = String.Empty
		mvarDiscSeq = String.Empty
		mvarCustID = String.Empty
		mvarCrtd_DateTime = Today
		mvarCrtd_Prog = String.Empty
		mvarCrtd_User = String.Empty
		mvarLUpd_DateTime = Today
		mvarLUpd_Prog = String.Empty
		mvarLUpd_User = String.Empty
		mvartstamp = String.Empty
	End Sub
	Public Function GetByKey(ByVal DiscID As System.String, ByVal DiscSeq As System.String, ByVal CustID As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Dim ds As New DataSet 
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@DiscID", DbType.String, clsCommon.GetValueDBNull(DiscID), ParameterDirection.InputOutput, 10 ))
			pc.Add(New ParamStruct("@DiscSeq", DbType.String, clsCommon.GetValueDBNull(DiscSeq), ParameterDirection.InputOutput, 10 ))
			pc.Add(New ParamStruct("@CustID", DbType.String, clsCommon.GetValueDBNull(CustID), ParameterDirection.InputOutput, 50 ))
			ds = DAL.ExecDataSet(PP_OM_DiscCust, CommandType.StoredProcedure, pc,"")
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
		mvarDiscID =  clsCommon.GetValue(row("DiscID"), mvarDiscID.GetType().FullName)
		mvarDiscSeq =  clsCommon.GetValue(row("DiscSeq"), mvarDiscSeq.GetType().FullName)
		mvarCustID =  clsCommon.GetValue(row("CustID"), mvarCustID.GetType().FullName)
		mvarCrtd_DateTime =  clsCommon.GetValue(row("Crtd_DateTime"), mvarCrtd_DateTime.GetType().FullName)
		mvarCrtd_Prog =  clsCommon.GetValue(row("Crtd_Prog"), mvarCrtd_Prog.GetType().FullName)
		mvarCrtd_User =  clsCommon.GetValue(row("Crtd_User"), mvarCrtd_User.GetType().FullName)
		mvarLUpd_DateTime =  clsCommon.GetValue(row("LUpd_DateTime"), mvarLUpd_DateTime.GetType().FullName)
		mvarLUpd_Prog =  clsCommon.GetValue(row("LUpd_Prog"), mvarLUpd_Prog.GetType().FullName)
		mvarLUpd_User =  clsCommon.GetValue(row("LUpd_User"), mvarLUpd_User.GetType().FullName)
		mvartstamp =  clsCommon.GetValue(row("tstamp"), mvartstamp.GetType().FullName)
	End Sub
#End Region 

#Region "Private Methods"
#End Region 

End Class
