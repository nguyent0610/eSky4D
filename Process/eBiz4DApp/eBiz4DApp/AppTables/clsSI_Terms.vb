'-- ------------------------------------------------------------
'-- Class name    :  clsSI_Terms
'-- Created date  :  5/13/2015
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework.Common
Imports HQFramework.DAL
Public Class clsSI_Terms
#Region "Constants"
	Private Const PP_SI_Terms As String = "PP_SI_Terms"
#End Region 

#Region "Member Variables"
	Private mvarTermsID As System.String

	Private mvarDescr As System.String

	Private mvarApplyTo As System.String

	Private mvarCOD As System.Int16

	Private mvarCreditChk As System.Int16

	Private mvarCycle As System.Int16

	Private mvarDiscIntrv As System.Int16

	Private mvarDiscPct As System.Double

	Private mvarDiscType As System.String

	Private mvarDueIntrv As System.Int16

	Private mvarDueType As System.String

	Private mvarLUpd_DateTime As System.DateTime

	Private mvarLUpd_Prog As System.String

	Private mvarLUpd_User As System.String

	Private mvarCrtd_DateTime As System.DateTime

	Private mvarCrtd_Prog As System.String

	Private mvarCrtd_User As System.String

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
	Public Property TermsID() As System.String
		Get
			Return mvarTermsID
		End Get
		Set(ByVal Value As System.String)
			mvarTermsID = Value
		End Set
	End Property

	Public Property Descr() As System.String
		Get
			Return mvarDescr
		End Get
		Set(ByVal Value As System.String)
			mvarDescr = Value
		End Set
	End Property

	Public Property ApplyTo() As System.String
		Get
			Return mvarApplyTo
		End Get
		Set(ByVal Value As System.String)
			mvarApplyTo = Value
		End Set
	End Property

	Public Property COD() As System.Int16
		Get
			Return mvarCOD
		End Get
		Set(ByVal Value As System.Int16)
			mvarCOD = Value
		End Set
	End Property

	Public Property CreditChk() As System.Int16
		Get
			Return mvarCreditChk
		End Get
		Set(ByVal Value As System.Int16)
			mvarCreditChk = Value
		End Set
	End Property

	Public Property Cycle() As System.Int16
		Get
			Return mvarCycle
		End Get
		Set(ByVal Value As System.Int16)
			mvarCycle = Value
		End Set
	End Property

	Public Property DiscIntrv() As System.Int16
		Get
			Return mvarDiscIntrv
		End Get
		Set(ByVal Value As System.Int16)
			mvarDiscIntrv = Value
		End Set
	End Property

	Public Property DiscPct() As System.Double
		Get
			Return mvarDiscPct
		End Get
		Set(ByVal Value As System.Double)
			mvarDiscPct = Value
		End Set
	End Property

	Public Property DiscType() As System.String
		Get
			Return mvarDiscType
		End Get
		Set(ByVal Value As System.String)
			mvarDiscType = Value
		End Set
	End Property

	Public Property DueIntrv() As System.Int16
		Get
			Return mvarDueIntrv
		End Get
		Set(ByVal Value As System.Int16)
			mvarDueIntrv = Value
		End Set
	End Property

	Public Property DueType() As System.String
		Get
			Return mvarDueType
		End Get
		Set(ByVal Value As System.String)
			mvarDueType = Value
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
			pc.Add(New ParamStruct("@TermsID", DbType.String,clsCommon.GetValueDBNull(Me.mvarTermsID), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@Descr", DbType.String,clsCommon.GetValueDBNull(Me.mvarDescr), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@ApplyTo", DbType.String,clsCommon.GetValueDBNull(Me.mvarApplyTo), ParameterDirection.Input,1 ))
			pc.Add(New ParamStruct("@COD", DbType.int16,clsCommon.GetValueDBNull(Me.mvarCOD), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@CreditChk", DbType.int16,clsCommon.GetValueDBNull(Me.mvarCreditChk), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@Cycle", DbType.int16,clsCommon.GetValueDBNull(Me.mvarCycle), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@DiscIntrv", DbType.int16,clsCommon.GetValueDBNull(Me.mvarDiscIntrv), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@DiscPct", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarDiscPct), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@DiscType", DbType.String,clsCommon.GetValueDBNull(Me.mvarDiscType), ParameterDirection.Input,1 ))
			pc.Add(New ParamStruct("@DueIntrv", DbType.int16,clsCommon.GetValueDBNull(Me.mvarDueIntrv), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@DueType", DbType.String,clsCommon.GetValueDBNull(Me.mvarDueType), ParameterDirection.Input,1 ))
			pc.Add(New ParamStruct("@LUpd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@LUpd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@LUpd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@Crtd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@Crtd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@Crtd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@tstamp", DbType.String,clsCommon.GetValueDBNull(Me.mvartstamp), ParameterDirection.Input,18 ))
			DAL.ExecPreparedSQL(PP_SI_Terms, CommandType.StoredProcedure, pc,"")
		Me.mvarTermsID = clsCommon.GetValue(pc.Item("@TermsID").Value, mvarTermsID.GetType().FullName)
		Return (Me.mvarTermsID <> String.Empty )
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Update() as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Update", ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@TermsID",DbType.String, clsCommon.GetValueDBNull(me.mvarTermsID), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@Descr",DbType.String, clsCommon.GetValueDBNull(me.mvarDescr), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@ApplyTo",DbType.String, clsCommon.GetValueDBNull(me.mvarApplyTo), ParameterDirection.Input,1 ))
			 pc.Add(New ParamStruct("@COD",DbType.int16, clsCommon.GetValueDBNull(me.mvarCOD), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@CreditChk",DbType.int16, clsCommon.GetValueDBNull(me.mvarCreditChk), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@Cycle",DbType.int16, clsCommon.GetValueDBNull(me.mvarCycle), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@DiscIntrv",DbType.int16, clsCommon.GetValueDBNull(me.mvarDiscIntrv), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@DiscPct",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarDiscPct), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@DiscType",DbType.String, clsCommon.GetValueDBNull(me.mvarDiscType), ParameterDirection.Input,1 ))
			 pc.Add(New ParamStruct("@DueIntrv",DbType.int16, clsCommon.GetValueDBNull(me.mvarDueIntrv), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@DueType",DbType.String, clsCommon.GetValueDBNull(me.mvarDueType), ParameterDirection.Input,1 ))
			 pc.Add(New ParamStruct("@LUpd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@LUpd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@LUpd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@Crtd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@Crtd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@Crtd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@tstamp",DbType.String, clsCommon.GetValueDBNull(me.mvartstamp), ParameterDirection.Input,18 ))
			Return (DAL.ExecNonQuery(PP_SI_Terms, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Delete(ByVal TermsID As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@TermsID",DbType.String, clsCommon.GetValueDBNull(TermsID), ParameterDirection.Input,2 ))
			Return (DAL.ExecNonQuery(PP_SI_Terms, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function GetAll(ByVal TermsID As System.String) as DataTable 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			Dim ds As New DataSet
			pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@TermsID", DbType.String, clsCommon.GetValueDBNull(TermsID), ParameterDirection.Input, 2 ))
			ds = DAL.ExecDataSet(PP_SI_Terms, CommandType.StoredProcedure, pc,"")
			Dim keys(0) As DataColumn
			Dim column As DataColumn
			column = ds.Tables(0).Columns("TermsID")
			Keys(0) = column
			ds.Tables(0).PrimaryKey = Keys
			Return ds.Tables(0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Sub Reset()
		mvarTermsID = String.Empty
		mvarDescr = String.Empty
		mvarApplyTo = String.Empty
		mvarCOD = 0
		mvarCreditChk = 0
		mvarCycle = 0
		mvarDiscIntrv = 0
		mvarDiscPct = 0
		mvarDiscType = String.Empty
		mvarDueIntrv = 0
		mvarDueType = String.Empty
		mvarLUpd_DateTime = Today
		mvarLUpd_Prog = String.Empty
		mvarLUpd_User = String.Empty
		mvarCrtd_DateTime = Today
		mvarCrtd_Prog = String.Empty
		mvarCrtd_User = String.Empty
		mvartstamp = String.Empty
	End Sub
	Public Function GetByKey(ByVal TermsID As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Dim ds As New DataSet 
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@TermsID", DbType.String, clsCommon.GetValueDBNull(TermsID), ParameterDirection.InputOutput, 2 ))
			ds = DAL.ExecDataSet(PP_SI_Terms, CommandType.StoredProcedure, pc,"")
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
		mvarTermsID =  clsCommon.GetValue(row("TermsID"), mvarTermsID.GetType().FullName)
		mvarDescr =  clsCommon.GetValue(row("Descr"), mvarDescr.GetType().FullName)
		mvarApplyTo =  clsCommon.GetValue(row("ApplyTo"), mvarApplyTo.GetType().FullName)
		mvarCOD =  clsCommon.GetValue(row("COD"), mvarCOD.GetType().FullName)
		mvarCreditChk =  clsCommon.GetValue(row("CreditChk"), mvarCreditChk.GetType().FullName)
		mvarCycle =  clsCommon.GetValue(row("Cycle"), mvarCycle.GetType().FullName)
		mvarDiscIntrv =  clsCommon.GetValue(row("DiscIntrv"), mvarDiscIntrv.GetType().FullName)
		mvarDiscPct =  clsCommon.GetValue(row("DiscPct"), mvarDiscPct.GetType().FullName)
		mvarDiscType =  clsCommon.GetValue(row("DiscType"), mvarDiscType.GetType().FullName)
		mvarDueIntrv =  clsCommon.GetValue(row("DueIntrv"), mvarDueIntrv.GetType().FullName)
		mvarDueType =  clsCommon.GetValue(row("DueType"), mvarDueType.GetType().FullName)
		mvarLUpd_DateTime =  clsCommon.GetValue(row("LUpd_DateTime"), mvarLUpd_DateTime.GetType().FullName)
		mvarLUpd_Prog =  clsCommon.GetValue(row("LUpd_Prog"), mvarLUpd_Prog.GetType().FullName)
		mvarLUpd_User =  clsCommon.GetValue(row("LUpd_User"), mvarLUpd_User.GetType().FullName)
		mvarCrtd_DateTime =  clsCommon.GetValue(row("Crtd_DateTime"), mvarCrtd_DateTime.GetType().FullName)
		mvarCrtd_Prog =  clsCommon.GetValue(row("Crtd_Prog"), mvarCrtd_Prog.GetType().FullName)
		mvarCrtd_User =  clsCommon.GetValue(row("Crtd_User"), mvarCrtd_User.GetType().FullName)
		mvartstamp =  clsCommon.GetValue(row("tstamp"), mvartstamp.GetType().FullName)
	End Sub
#End Region 

#Region "Private Methods"
#End Region 

End Class
