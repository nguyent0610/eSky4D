'-- ------------------------------------------------------------
'-- Class name    :  clsIN_POSMCust
'-- Created date  :  5/13/2015
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework.Common
Imports HQFramework.DAL
Public Class clsIN_POSMCust
#Region "Constants"
	Private Const PP_IN_POSMCust As String = "PP_IN_POSMCust"
#End Region 

#Region "Member Variables"
	Private mvarPosmID As System.String

	Private mvarBranchID As System.String

	Private mvarCustID As System.String

	Private mvarPosmCode As System.String

	Private mvarSlsperID As System.String

	Private mvarQty As System.Int32

	Private mvarAppQty As System.Int32

	Private mvarIsAgree As System.Boolean

	Private mvarStatus As System.String

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
	Public Property PosmID() As System.String
		Get
			Return mvarPosmID
		End Get
		Set(ByVal Value As System.String)
			mvarPosmID = Value
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

	Public Property CustID() As System.String
		Get
			Return mvarCustID
		End Get
		Set(ByVal Value As System.String)
			mvarCustID = Value
		End Set
	End Property

	Public Property PosmCode() As System.String
		Get
			Return mvarPosmCode
		End Get
		Set(ByVal Value As System.String)
			mvarPosmCode = Value
		End Set
	End Property

	Public Property SlsperID() As System.String
		Get
			Return mvarSlsperID
		End Get
		Set(ByVal Value As System.String)
			mvarSlsperID = Value
		End Set
	End Property

	Public Property Qty() As System.Int32
		Get
			Return mvarQty
		End Get
		Set(ByVal Value As System.Int32)
			mvarQty = Value
		End Set
	End Property

	Public Property AppQty() As System.Int32
		Get
			Return mvarAppQty
		End Get
		Set(ByVal Value As System.Int32)
			mvarAppQty = Value
		End Set
	End Property

	Public Property IsAgree() As System.Boolean
		Get
			Return mvarIsAgree
		End Get
		Set(ByVal Value As System.Boolean)
			mvarIsAgree = Value
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
			pc.Add(New ParamStruct("@PosmID", DbType.String,clsCommon.GetValueDBNull(Me.mvarPosmID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String,clsCommon.GetValueDBNull(Me.mvarBranchID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@CustID", DbType.String,clsCommon.GetValueDBNull(Me.mvarCustID), ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@PosmCode", DbType.String,clsCommon.GetValueDBNull(Me.mvarPosmCode), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@SlsperID", DbType.String,clsCommon.GetValueDBNull(Me.mvarSlsperID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@Qty", DbType.int32,clsCommon.GetValueDBNull(Me.mvarQty), ParameterDirection.Input,4 ))
			pc.Add(New ParamStruct("@AppQty", DbType.int32,clsCommon.GetValueDBNull(Me.mvarAppQty), ParameterDirection.Input,4 ))
			pc.Add(New ParamStruct("@IsAgree", DbType.Boolean,clsCommon.GetValueDBNull(Me.mvarIsAgree), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@Status", DbType.String,clsCommon.GetValueDBNull(Me.mvarStatus), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@Crtd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@Crtd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@Crtd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@LUpd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@LUpd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@LUpd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@tstamp", DbType.String,clsCommon.GetValueDBNull(Me.mvartstamp), ParameterDirection.Input,18 ))
			DAL.ExecPreparedSQL(PP_IN_POSMCust, CommandType.StoredProcedure, pc,"")
		Me.mvarPosmID = clsCommon.GetValue(pc.Item("@PosmID").Value, mvarPosmID.GetType().FullName)
		Return (Me.mvarPosmID <> String.Empty )
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Update() as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Update", ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@PosmID",DbType.String, clsCommon.GetValueDBNull(me.mvarPosmID), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@BranchID",DbType.String, clsCommon.GetValueDBNull(me.mvarBranchID), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@CustID",DbType.String, clsCommon.GetValueDBNull(me.mvarCustID), ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@PosmCode",DbType.String, clsCommon.GetValueDBNull(me.mvarPosmCode), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@SlsperID",DbType.String, clsCommon.GetValueDBNull(me.mvarSlsperID), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@Qty",DbType.int32, clsCommon.GetValueDBNull(me.mvarQty), ParameterDirection.Input,4 ))
			 pc.Add(New ParamStruct("@AppQty",DbType.int32, clsCommon.GetValueDBNull(me.mvarAppQty), ParameterDirection.Input,4 ))
			 pc.Add(New ParamStruct("@IsAgree",DbType.Boolean, clsCommon.GetValueDBNull(me.mvarIsAgree), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@Status",DbType.String, clsCommon.GetValueDBNull(me.mvarStatus), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@Crtd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@Crtd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@Crtd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@LUpd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@LUpd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@LUpd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@tstamp",DbType.String, clsCommon.GetValueDBNull(me.mvartstamp), ParameterDirection.Input,18 ))
			Return (DAL.ExecNonQuery(PP_IN_POSMCust, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Delete(ByVal PosmID As System.String, ByVal BranchID As System.String, ByVal CustID As System.String, ByVal PosmCode As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@PosmID",DbType.String, clsCommon.GetValueDBNull(PosmID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@BranchID",DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@CustID",DbType.String, clsCommon.GetValueDBNull(CustID), ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@PosmCode",DbType.String, clsCommon.GetValueDBNull(PosmCode), ParameterDirection.Input,30 ))
			Return (DAL.ExecNonQuery(PP_IN_POSMCust, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function GetAll(ByVal PosmID As System.String, ByVal BranchID As System.String, ByVal CustID As System.String, ByVal PosmCode As System.String) as DataTable 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			Dim ds As New DataSet
			pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@PosmID", DbType.String, clsCommon.GetValueDBNull(PosmID), ParameterDirection.Input, 30 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 30 ))
			pc.Add(New ParamStruct("@CustID", DbType.String, clsCommon.GetValueDBNull(CustID), ParameterDirection.Input, 50 ))
			pc.Add(New ParamStruct("@PosmCode", DbType.String, clsCommon.GetValueDBNull(PosmCode), ParameterDirection.Input, 30 ))
			ds = DAL.ExecDataSet(PP_IN_POSMCust, CommandType.StoredProcedure, pc,"")
			Dim keys(3) As DataColumn
			Dim column As DataColumn
			column = ds.Tables(0).Columns("PosmID")
			Keys(0) = column
			column = ds.Tables(0).Columns("BranchID")
			Keys(1) = column
			column = ds.Tables(0).Columns("CustID")
			Keys(2) = column
			column = ds.Tables(0).Columns("PosmCode")
			Keys(3) = column
			ds.Tables(0).PrimaryKey = Keys
			Return ds.Tables(0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Sub Reset()
		mvarPosmID = String.Empty
		mvarBranchID = String.Empty
		mvarCustID = String.Empty
		mvarPosmCode = String.Empty
		mvarSlsperID = String.Empty
		mvarQty = 0
		mvarAppQty = 0
		mvarIsAgree = False
		mvarStatus = String.Empty
		mvarCrtd_DateTime = Today
		mvarCrtd_Prog = String.Empty
		mvarCrtd_User = String.Empty
		mvarLUpd_DateTime = Today
		mvarLUpd_Prog = String.Empty
		mvarLUpd_User = String.Empty
		mvartstamp = String.Empty
	End Sub
	Public Function GetByKey(ByVal PosmID As System.String, ByVal BranchID As System.String, ByVal CustID As System.String, ByVal PosmCode As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Dim ds As New DataSet 
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@PosmID", DbType.String, clsCommon.GetValueDBNull(PosmID), ParameterDirection.InputOutput, 30 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.InputOutput, 30 ))
			pc.Add(New ParamStruct("@CustID", DbType.String, clsCommon.GetValueDBNull(CustID), ParameterDirection.InputOutput, 50 ))
			pc.Add(New ParamStruct("@PosmCode", DbType.String, clsCommon.GetValueDBNull(PosmCode), ParameterDirection.InputOutput, 30 ))
			ds = DAL.ExecDataSet(PP_IN_POSMCust, CommandType.StoredProcedure, pc,"")
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
		mvarPosmID =  clsCommon.GetValue(row("PosmID"), mvarPosmID.GetType().FullName)
		mvarBranchID =  clsCommon.GetValue(row("BranchID"), mvarBranchID.GetType().FullName)
		mvarCustID =  clsCommon.GetValue(row("CustID"), mvarCustID.GetType().FullName)
		mvarPosmCode =  clsCommon.GetValue(row("PosmCode"), mvarPosmCode.GetType().FullName)
		mvarSlsperID =  clsCommon.GetValue(row("SlsperID"), mvarSlsperID.GetType().FullName)
		mvarQty =  clsCommon.GetValue(row("Qty"), mvarQty.GetType().FullName)
		mvarAppQty =  clsCommon.GetValue(row("AppQty"), mvarAppQty.GetType().FullName)
		mvarIsAgree =  clsCommon.GetValue(row("IsAgree"), mvarIsAgree.GetType().FullName)
		mvarStatus =  clsCommon.GetValue(row("Status"), mvarStatus.GetType().FullName)
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
