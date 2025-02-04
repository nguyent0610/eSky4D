'-- ------------------------------------------------------------
'-- Class name    :  clsAR_DisplayMethod
'-- Created date  :  5/13/2015
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework.Common
Imports HQFramework.DAL
Public Class clsAR_DisplayMethod
#Region "Constants"
	Private Const PP_AR_DisplayMethod As String = "PP_AR_DisplayMethod"
#End Region 

#Region "Member Variables"
	Private mvarDispMethod As System.String

	Private mvarDescr As System.String

	Private mvarActive As System.Boolean

	Private mvarType As System.String

	Private mvarLevel As System.Double

	Private mvarStyle As System.String

	Private mvarShelf As System.String

	Private mvarSeq As System.Int16

	Private mvarTarget As System.Double

	Private mvarLUpd_Datetime As System.DateTime

	Private mvarLUpd_Prog As System.String

	Private mvarLUpd_User As System.String

	Private mvarCrtd_Datetime As System.DateTime

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
	Public Property DispMethod() As System.String
		Get
			Return mvarDispMethod
		End Get
		Set(ByVal Value As System.String)
			mvarDispMethod = Value
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

	Public Property Active() As System.Boolean
		Get
			Return mvarActive
		End Get
		Set(ByVal Value As System.Boolean)
			mvarActive = Value
		End Set
	End Property

	Public Property Type() As System.String
		Get
			Return mvarType
		End Get
		Set(ByVal Value As System.String)
			mvarType = Value
		End Set
	End Property

	Public Property Level() As System.Double
		Get
			Return mvarLevel
		End Get
		Set(ByVal Value As System.Double)
			mvarLevel = Value
		End Set
	End Property

	Public Property Style() As System.String
		Get
			Return mvarStyle
		End Get
		Set(ByVal Value As System.String)
			mvarStyle = Value
		End Set
	End Property

	Public Property Shelf() As System.String
		Get
			Return mvarShelf
		End Get
		Set(ByVal Value As System.String)
			mvarShelf = Value
		End Set
	End Property

	Public Property Seq() As System.Int16
		Get
			Return mvarSeq
		End Get
		Set(ByVal Value As System.Int16)
			mvarSeq = Value
		End Set
	End Property

	Public Property Target() As System.Double
		Get
			Return mvarTarget
		End Get
		Set(ByVal Value As System.Double)
			mvarTarget = Value
		End Set
	End Property

	Public Property LUpd_Datetime() As System.DateTime
		Get
			Return mvarLUpd_Datetime
		End Get
		Set(ByVal Value As System.DateTime)
			mvarLUpd_Datetime = Value
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

	Public Property Crtd_Datetime() As System.DateTime
		Get
			Return mvarCrtd_Datetime
		End Get
		Set(ByVal Value As System.DateTime)
			mvarCrtd_Datetime = Value
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
			pc.Add(New ParamStruct("@DispMethod", DbType.String,clsCommon.GetValueDBNull(Me.mvarDispMethod), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@Descr", DbType.String,clsCommon.GetValueDBNull(Me.mvarDescr), ParameterDirection.Input,200 ))
			pc.Add(New ParamStruct("@Active", DbType.Boolean,clsCommon.GetValueDBNull(Me.mvarActive), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@Type", DbType.String,clsCommon.GetValueDBNull(Me.mvarType), ParameterDirection.Input,1 ))
			pc.Add(New ParamStruct("@Level", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarLevel), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@Style", DbType.String,clsCommon.GetValueDBNull(Me.mvarStyle), ParameterDirection.Input,1 ))
			pc.Add(New ParamStruct("@Shelf", DbType.String,clsCommon.GetValueDBNull(Me.mvarShelf), ParameterDirection.Input,1 ))
			pc.Add(New ParamStruct("@Seq", DbType.int16,clsCommon.GetValueDBNull(Me.mvarSeq), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@Target", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarTarget), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@LUpd_Datetime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarLUpd_Datetime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@LUpd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@LUpd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@Crtd_Datetime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarCrtd_Datetime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@Crtd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@Crtd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@tstamp", DbType.String,clsCommon.GetValueDBNull(Me.mvartstamp), ParameterDirection.Input,18 ))
			DAL.ExecPreparedSQL(PP_AR_DisplayMethod, CommandType.StoredProcedure, pc,"")
		Me.mvarDispMethod = clsCommon.GetValue(pc.Item("@DispMethod").Value, mvarDispMethod.GetType().FullName)
		Return (Me.mvarDispMethod <> String.Empty )
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Update() as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Update", ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@DispMethod",DbType.String, clsCommon.GetValueDBNull(me.mvarDispMethod), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@Descr",DbType.String, clsCommon.GetValueDBNull(me.mvarDescr), ParameterDirection.Input,200 ))
			 pc.Add(New ParamStruct("@Active",DbType.Boolean, clsCommon.GetValueDBNull(me.mvarActive), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@Type",DbType.String, clsCommon.GetValueDBNull(me.mvarType), ParameterDirection.Input,1 ))
			 pc.Add(New ParamStruct("@Level",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarLevel), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@Style",DbType.String, clsCommon.GetValueDBNull(me.mvarStyle), ParameterDirection.Input,1 ))
			 pc.Add(New ParamStruct("@Shelf",DbType.String, clsCommon.GetValueDBNull(me.mvarShelf), ParameterDirection.Input,1 ))
			 pc.Add(New ParamStruct("@Seq",DbType.int16, clsCommon.GetValueDBNull(me.mvarSeq), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@Target",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarTarget), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@LUpd_Datetime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarLUpd_Datetime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@LUpd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@LUpd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@Crtd_Datetime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarCrtd_Datetime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@Crtd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@Crtd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@tstamp",DbType.String, clsCommon.GetValueDBNull(me.mvartstamp), ParameterDirection.Input,18 ))
			Return (DAL.ExecNonQuery(PP_AR_DisplayMethod, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Delete(ByVal DispMethod As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@DispMethod",DbType.String, clsCommon.GetValueDBNull(DispMethod), ParameterDirection.Input,10 ))
			Return (DAL.ExecNonQuery(PP_AR_DisplayMethod, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function GetAll(ByVal DispMethod As System.String) as DataTable 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			Dim ds As New DataSet
			pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@DispMethod", DbType.String, clsCommon.GetValueDBNull(DispMethod), ParameterDirection.Input, 10 ))
			ds = DAL.ExecDataSet(PP_AR_DisplayMethod, CommandType.StoredProcedure, pc,"")
			Dim keys(0) As DataColumn
			Dim column As DataColumn
			column = ds.Tables(0).Columns("DispMethod")
			Keys(0) = column
			ds.Tables(0).PrimaryKey = Keys
			Return ds.Tables(0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Sub Reset()
		mvarDispMethod = String.Empty
		mvarDescr = String.Empty
		mvarActive = False
		mvarType = String.Empty
		mvarLevel = 0
		mvarStyle = String.Empty
		mvarShelf = String.Empty
		mvarSeq = 0
		mvarTarget = 0
		mvarLUpd_Datetime = Today
		mvarLUpd_Prog = String.Empty
		mvarLUpd_User = String.Empty
		mvarCrtd_Datetime = Today
		mvarCrtd_Prog = String.Empty
		mvarCrtd_User = String.Empty
		mvartstamp = String.Empty
	End Sub
	Public Function GetByKey(ByVal DispMethod As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Dim ds As New DataSet 
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@DispMethod", DbType.String, clsCommon.GetValueDBNull(DispMethod), ParameterDirection.InputOutput, 10 ))
			ds = DAL.ExecDataSet(PP_AR_DisplayMethod, CommandType.StoredProcedure, pc,"")
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
		mvarDispMethod =  clsCommon.GetValue(row("DispMethod"), mvarDispMethod.GetType().FullName)
		mvarDescr =  clsCommon.GetValue(row("Descr"), mvarDescr.GetType().FullName)
		mvarActive =  clsCommon.GetValue(row("Active"), mvarActive.GetType().FullName)
		mvarType =  clsCommon.GetValue(row("Type"), mvarType.GetType().FullName)
		mvarLevel =  clsCommon.GetValue(row("Level"), mvarLevel.GetType().FullName)
		mvarStyle =  clsCommon.GetValue(row("Style"), mvarStyle.GetType().FullName)
		mvarShelf =  clsCommon.GetValue(row("Shelf"), mvarShelf.GetType().FullName)
		mvarSeq =  clsCommon.GetValue(row("Seq"), mvarSeq.GetType().FullName)
		mvarTarget =  clsCommon.GetValue(row("Target"), mvarTarget.GetType().FullName)
		mvarLUpd_Datetime =  clsCommon.GetValue(row("LUpd_Datetime"), mvarLUpd_Datetime.GetType().FullName)
		mvarLUpd_Prog =  clsCommon.GetValue(row("LUpd_Prog"), mvarLUpd_Prog.GetType().FullName)
		mvarLUpd_User =  clsCommon.GetValue(row("LUpd_User"), mvarLUpd_User.GetType().FullName)
		mvarCrtd_Datetime =  clsCommon.GetValue(row("Crtd_Datetime"), mvarCrtd_Datetime.GetType().FullName)
		mvarCrtd_Prog =  clsCommon.GetValue(row("Crtd_Prog"), mvarCrtd_Prog.GetType().FullName)
		mvarCrtd_User =  clsCommon.GetValue(row("Crtd_User"), mvarCrtd_User.GetType().FullName)
		mvartstamp =  clsCommon.GetValue(row("tstamp"), mvartstamp.GetType().FullName)
	End Sub
#End Region 

#Region "Private Methods"
#End Region 

End Class
