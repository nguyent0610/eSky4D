'-- ------------------------------------------------------------
'-- Class name    :  clsOM_DocNumbering
'-- Created date  :  5/13/2015
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework.Common
Imports HQFramework.DAL
Public Class clsOM_DocNumbering
#Region "Constants"
	Private Const PP_OM_DocNumbering As String = "PP_OM_DocNumbering"
#End Region 

#Region "Member Variables"
	Private mvarBranchID As System.String

	Private mvarOrderType As System.String

	Private mvarLastOrderNbr As System.String

	Private mvarLastShipperNbr As System.String

	Private mvarLastARRefNbr As System.String

	Private mvarLastInvcNbr As System.String

	Private mvarLastInvcNote As System.String

	Private mvarPreFixIN As System.String

	Private mvarPreFixShip As System.String

	Private mvarPreFixSO As System.String

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
	Public Property BranchID() As System.String
		Get
			Return mvarBranchID
		End Get
		Set(ByVal Value As System.String)
			mvarBranchID = Value
		End Set
	End Property

	Public Property OrderType() As System.String
		Get
			Return mvarOrderType
		End Get
		Set(ByVal Value As System.String)
			mvarOrderType = Value
		End Set
	End Property

	Public Property LastOrderNbr() As System.String
		Get
			Return mvarLastOrderNbr
		End Get
		Set(ByVal Value As System.String)
			mvarLastOrderNbr = Value
		End Set
	End Property

	Public Property LastShipperNbr() As System.String
		Get
			Return mvarLastShipperNbr
		End Get
		Set(ByVal Value As System.String)
			mvarLastShipperNbr = Value
		End Set
	End Property

	Public Property LastARRefNbr() As System.String
		Get
			Return mvarLastARRefNbr
		End Get
		Set(ByVal Value As System.String)
			mvarLastARRefNbr = Value
		End Set
	End Property

	Public Property LastInvcNbr() As System.String
		Get
			Return mvarLastInvcNbr
		End Get
		Set(ByVal Value As System.String)
			mvarLastInvcNbr = Value
		End Set
	End Property

	Public Property LastInvcNote() As System.String
		Get
			Return mvarLastInvcNote
		End Get
		Set(ByVal Value As System.String)
			mvarLastInvcNote = Value
		End Set
	End Property

	Public Property PreFixIN() As System.String
		Get
			Return mvarPreFixIN
		End Get
		Set(ByVal Value As System.String)
			mvarPreFixIN = Value
		End Set
	End Property

	Public Property PreFixShip() As System.String
		Get
			Return mvarPreFixShip
		End Get
		Set(ByVal Value As System.String)
			mvarPreFixShip = Value
		End Set
	End Property

	Public Property PreFixSO() As System.String
		Get
			Return mvarPreFixSO
		End Get
		Set(ByVal Value As System.String)
			mvarPreFixSO = Value
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
			pc.Add(New ParamStruct("@BranchID", DbType.String,clsCommon.GetValueDBNull(Me.mvarBranchID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@OrderType", DbType.String,clsCommon.GetValueDBNull(Me.mvarOrderType), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@LastOrderNbr", DbType.String,clsCommon.GetValueDBNull(Me.mvarLastOrderNbr), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@LastShipperNbr", DbType.String,clsCommon.GetValueDBNull(Me.mvarLastShipperNbr), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@LastARRefNbr", DbType.String,clsCommon.GetValueDBNull(Me.mvarLastARRefNbr), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@LastInvcNbr", DbType.String,clsCommon.GetValueDBNull(Me.mvarLastInvcNbr), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@LastInvcNote", DbType.String,clsCommon.GetValueDBNull(Me.mvarLastInvcNote), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@PreFixIN", DbType.String,clsCommon.GetValueDBNull(Me.mvarPreFixIN), ParameterDirection.Input,5 ))
			pc.Add(New ParamStruct("@PreFixShip", DbType.String,clsCommon.GetValueDBNull(Me.mvarPreFixShip), ParameterDirection.Input,5 ))
			pc.Add(New ParamStruct("@PreFixSO", DbType.String,clsCommon.GetValueDBNull(Me.mvarPreFixSO), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@Crtd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@Crtd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@Crtd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@LUpd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@LUpd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@LUpd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@tstamp", DbType.String,clsCommon.GetValueDBNull(Me.mvartstamp), ParameterDirection.Input,18 ))
			DAL.ExecPreparedSQL(PP_OM_DocNumbering, CommandType.StoredProcedure, pc,"")
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
			 pc.Add(New ParamStruct("@OrderType",DbType.String, clsCommon.GetValueDBNull(me.mvarOrderType), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@LastOrderNbr",DbType.String, clsCommon.GetValueDBNull(me.mvarLastOrderNbr), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@LastShipperNbr",DbType.String, clsCommon.GetValueDBNull(me.mvarLastShipperNbr), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@LastARRefNbr",DbType.String, clsCommon.GetValueDBNull(me.mvarLastARRefNbr), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@LastInvcNbr",DbType.String, clsCommon.GetValueDBNull(me.mvarLastInvcNbr), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@LastInvcNote",DbType.String, clsCommon.GetValueDBNull(me.mvarLastInvcNote), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@PreFixIN",DbType.String, clsCommon.GetValueDBNull(me.mvarPreFixIN), ParameterDirection.Input,5 ))
			 pc.Add(New ParamStruct("@PreFixShip",DbType.String, clsCommon.GetValueDBNull(me.mvarPreFixShip), ParameterDirection.Input,5 ))
			 pc.Add(New ParamStruct("@PreFixSO",DbType.String, clsCommon.GetValueDBNull(me.mvarPreFixSO), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@Crtd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@Crtd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@Crtd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@LUpd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@LUpd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@LUpd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@tstamp",DbType.String, clsCommon.GetValueDBNull(me.mvartstamp), ParameterDirection.Input,18 ))
			Return (DAL.ExecNonQuery(PP_OM_DocNumbering, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Delete(ByVal BranchID As System.String, ByVal OrderType As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID",DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@OrderType",DbType.String, clsCommon.GetValueDBNull(OrderType), ParameterDirection.Input,2 ))
			Return (DAL.ExecNonQuery(PP_OM_DocNumbering, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function GetAll(ByVal BranchID As System.String, ByVal OrderType As System.String) as DataTable 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			Dim ds As New DataSet
			pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 30 ))
			pc.Add(New ParamStruct("@OrderType", DbType.String, clsCommon.GetValueDBNull(OrderType), ParameterDirection.Input, 2 ))
			ds = DAL.ExecDataSet(PP_OM_DocNumbering, CommandType.StoredProcedure, pc,"")
			Dim keys(1) As DataColumn
			Dim column As DataColumn
			column = ds.Tables(0).Columns("BranchID")
			Keys(0) = column
			column = ds.Tables(0).Columns("OrderType")
			Keys(1) = column
			ds.Tables(0).PrimaryKey = Keys
			Return ds.Tables(0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Sub Reset()
		mvarBranchID = String.Empty
		mvarOrderType = String.Empty
		mvarLastOrderNbr = String.Empty
		mvarLastShipperNbr = String.Empty
		mvarLastARRefNbr = String.Empty
		mvarLastInvcNbr = String.Empty
		mvarLastInvcNote = String.Empty
		mvarPreFixIN = String.Empty
		mvarPreFixShip = String.Empty
		mvarPreFixSO = String.Empty
		mvarCrtd_DateTime = Today
		mvarCrtd_Prog = String.Empty
		mvarCrtd_User = String.Empty
		mvarLUpd_DateTime = Today
		mvarLUpd_Prog = String.Empty
		mvarLUpd_User = String.Empty
		mvartstamp = String.Empty
	End Sub
	Public Function GetByKey(ByVal BranchID As System.String, ByVal OrderType As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Dim ds As New DataSet 
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.InputOutput, 30 ))
			pc.Add(New ParamStruct("@OrderType", DbType.String, clsCommon.GetValueDBNull(OrderType), ParameterDirection.InputOutput, 2 ))
			ds = DAL.ExecDataSet(PP_OM_DocNumbering, CommandType.StoredProcedure, pc,"")
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
		mvarOrderType =  clsCommon.GetValue(row("OrderType"), mvarOrderType.GetType().FullName)
		mvarLastOrderNbr =  clsCommon.GetValue(row("LastOrderNbr"), mvarLastOrderNbr.GetType().FullName)
		mvarLastShipperNbr =  clsCommon.GetValue(row("LastShipperNbr"), mvarLastShipperNbr.GetType().FullName)
		mvarLastARRefNbr =  clsCommon.GetValue(row("LastARRefNbr"), mvarLastARRefNbr.GetType().FullName)
		mvarLastInvcNbr =  clsCommon.GetValue(row("LastInvcNbr"), mvarLastInvcNbr.GetType().FullName)
		mvarLastInvcNote =  clsCommon.GetValue(row("LastInvcNote"), mvarLastInvcNote.GetType().FullName)
		mvarPreFixIN =  clsCommon.GetValue(row("PreFixIN"), mvarPreFixIN.GetType().FullName)
		mvarPreFixShip =  clsCommon.GetValue(row("PreFixShip"), mvarPreFixShip.GetType().FullName)
		mvarPreFixSO =  clsCommon.GetValue(row("PreFixSO"), mvarPreFixSO.GetType().FullName)
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
