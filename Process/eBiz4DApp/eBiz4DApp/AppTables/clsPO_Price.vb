'-- ------------------------------------------------------------
'-- Class name    :  clsPO_Price
'-- Created date  :  5/13/2015
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework.Common
Imports HQFramework.DAL
Public Class clsPO_Price
#Region "Constants"
	Private Const PP_PO_Price As String = "PP_PO_Price"
#End Region 

#Region "Member Variables"
	Private mvarVendID As System.String

	Private mvarInvtID As System.String

	Private mvarUOM As System.String

	Private mvarPrice As System.Double

	Private mvarCrtd_DateTime As System.DateTime

	Private mvarCrtd_Prog As System.String

	Private mvarCrtd_User As System.String

	Private mvarLUpd_DateTime As System.DateTime

	Private mvarLUpd_Prog As System.String

	Private mvarLUpd_User As System.String

	Private mvartstamp As System.String

	Private mvarPriceID As System.String

	Private mvarDescr As System.String

	Private mvarQtyBreak As System.Double

	Private mvarDisc As System.Double

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
	Public Property VendID() As System.String
		Get
			Return mvarVendID
		End Get
		Set(ByVal Value As System.String)
			mvarVendID = Value
		End Set
	End Property

	Public Property InvtID() As System.String
		Get
			Return mvarInvtID
		End Get
		Set(ByVal Value As System.String)
			mvarInvtID = Value
		End Set
	End Property

	Public Property UOM() As System.String
		Get
			Return mvarUOM
		End Get
		Set(ByVal Value As System.String)
			mvarUOM = Value
		End Set
	End Property

	Public Property Price() As System.Double
		Get
			Return mvarPrice
		End Get
		Set(ByVal Value As System.Double)
			mvarPrice = Value
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

	Public Property PriceID() As System.String
		Get
			Return mvarPriceID
		End Get
		Set(ByVal Value As System.String)
			mvarPriceID = Value
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

	Public Property QtyBreak() As System.Double
		Get
			Return mvarQtyBreak
		End Get
		Set(ByVal Value As System.Double)
			mvarQtyBreak = Value
		End Set
	End Property

	Public Property Disc() As System.Double
		Get
			Return mvarDisc
		End Get
		Set(ByVal Value As System.Double)
			mvarDisc = Value
		End Set
	End Property

#End Region 

#Region "Public Methods"
	Public Function Add() as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "AddNew", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@VendID", DbType.String,clsCommon.GetValueDBNull(Me.mvarVendID), ParameterDirection.Input,15 ))
			pc.Add(New ParamStruct("@InvtID", DbType.String,clsCommon.GetValueDBNull(Me.mvarInvtID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@UOM", DbType.String,clsCommon.GetValueDBNull(Me.mvarUOM), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@Price", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarPrice), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@Crtd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@Crtd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@Crtd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@LUpd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@LUpd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@LUpd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@tstamp", DbType.String,clsCommon.GetValueDBNull(Me.mvartstamp), ParameterDirection.Input,18 ))
			pc.Add(New ParamStruct("@PriceID", DbType.String,clsCommon.GetValueDBNull(Me.mvarPriceID), ParameterDirection.Input,20 ))
			pc.Add(New ParamStruct("@Descr", DbType.String,clsCommon.GetValueDBNull(Me.mvarDescr), ParameterDirection.Input,400 ))
			pc.Add(New ParamStruct("@QtyBreak", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarQtyBreak), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@Disc", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarDisc), ParameterDirection.Input,8 ))
			DAL.ExecPreparedSQL(PP_PO_Price, CommandType.StoredProcedure, pc,"")
		Me.mvarVendID = clsCommon.GetValue(pc.Item("@VendID").Value, mvarVendID.GetType().FullName)
		Return (Me.mvarVendID <> String.Empty )
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Update() as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Update", ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@VendID",DbType.String, clsCommon.GetValueDBNull(me.mvarVendID), ParameterDirection.Input,15 ))
			 pc.Add(New ParamStruct("@InvtID",DbType.String, clsCommon.GetValueDBNull(me.mvarInvtID), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@UOM",DbType.String, clsCommon.GetValueDBNull(me.mvarUOM), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@Price",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarPrice), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@Crtd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@Crtd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@Crtd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@LUpd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@LUpd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@LUpd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@tstamp",DbType.String, clsCommon.GetValueDBNull(me.mvartstamp), ParameterDirection.Input,18 ))
			 pc.Add(New ParamStruct("@PriceID",DbType.String, clsCommon.GetValueDBNull(me.mvarPriceID), ParameterDirection.Input,20 ))
			 pc.Add(New ParamStruct("@Descr",DbType.String, clsCommon.GetValueDBNull(me.mvarDescr), ParameterDirection.Input,400 ))
			 pc.Add(New ParamStruct("@QtyBreak",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarQtyBreak), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@Disc",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarDisc), ParameterDirection.Input,8 ))
			Return (DAL.ExecNonQuery(PP_PO_Price, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Delete(ByVal VendID As System.String, ByVal InvtID As System.String, ByVal UOM As System.String, ByVal PriceID As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@VendID",DbType.String, clsCommon.GetValueDBNull(VendID), ParameterDirection.Input,15 ))
			pc.Add(New ParamStruct("@InvtID",DbType.String, clsCommon.GetValueDBNull(InvtID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@UOM",DbType.String, clsCommon.GetValueDBNull(UOM), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@PriceID",DbType.String, clsCommon.GetValueDBNull(PriceID), ParameterDirection.Input,20 ))
			Return (DAL.ExecNonQuery(PP_PO_Price, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function GetAll(ByVal VendID As System.String, ByVal InvtID As System.String, ByVal UOM As System.String, ByVal PriceID As System.String) as DataTable 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			Dim ds As New DataSet
			pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@VendID", DbType.String, clsCommon.GetValueDBNull(VendID), ParameterDirection.Input, 15 ))
			pc.Add(New ParamStruct("@InvtID", DbType.String, clsCommon.GetValueDBNull(InvtID), ParameterDirection.Input, 30 ))
			pc.Add(New ParamStruct("@UOM", DbType.String, clsCommon.GetValueDBNull(UOM), ParameterDirection.Input, 10 ))
			pc.Add(New ParamStruct("@PriceID", DbType.String, clsCommon.GetValueDBNull(PriceID), ParameterDirection.Input, 20 ))
			ds = DAL.ExecDataSet(PP_PO_Price, CommandType.StoredProcedure, pc,"")
			Dim keys(3) As DataColumn
			Dim column As DataColumn
			column = ds.Tables(0).Columns("VendID")
			Keys(0) = column
			column = ds.Tables(0).Columns("InvtID")
			Keys(1) = column
			column = ds.Tables(0).Columns("UOM")
			Keys(2) = column
			column = ds.Tables(0).Columns("PriceID")
			Keys(3) = column
			ds.Tables(0).PrimaryKey = Keys
			Return ds.Tables(0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Sub Reset()
		mvarVendID = String.Empty
		mvarInvtID = String.Empty
		mvarUOM = String.Empty
		mvarPrice = 0
		mvarCrtd_DateTime = Today
		mvarCrtd_Prog = String.Empty
		mvarCrtd_User = String.Empty
		mvarLUpd_DateTime = Today
		mvarLUpd_Prog = String.Empty
		mvarLUpd_User = String.Empty
		mvartstamp = String.Empty
		mvarPriceID = String.Empty
		mvarDescr = String.Empty
		mvarQtyBreak = 0
		mvarDisc = 0
	End Sub
	Public Function GetByKey(ByVal VendID As System.String, ByVal InvtID As System.String, ByVal UOM As System.String, ByVal PriceID As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Dim ds As New DataSet 
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@VendID", DbType.String, clsCommon.GetValueDBNull(VendID), ParameterDirection.InputOutput, 15 ))
			pc.Add(New ParamStruct("@InvtID", DbType.String, clsCommon.GetValueDBNull(InvtID), ParameterDirection.InputOutput, 30 ))
			pc.Add(New ParamStruct("@UOM", DbType.String, clsCommon.GetValueDBNull(UOM), ParameterDirection.InputOutput, 10 ))
			pc.Add(New ParamStruct("@PriceID", DbType.String, clsCommon.GetValueDBNull(PriceID), ParameterDirection.InputOutput, 20 ))
			ds = DAL.ExecDataSet(PP_PO_Price, CommandType.StoredProcedure, pc,"")
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
		mvarVendID =  clsCommon.GetValue(row("VendID"), mvarVendID.GetType().FullName)
		mvarInvtID =  clsCommon.GetValue(row("InvtID"), mvarInvtID.GetType().FullName)
		mvarUOM =  clsCommon.GetValue(row("UOM"), mvarUOM.GetType().FullName)
		mvarPrice =  clsCommon.GetValue(row("Price"), mvarPrice.GetType().FullName)
		mvarCrtd_DateTime =  clsCommon.GetValue(row("Crtd_DateTime"), mvarCrtd_DateTime.GetType().FullName)
		mvarCrtd_Prog =  clsCommon.GetValue(row("Crtd_Prog"), mvarCrtd_Prog.GetType().FullName)
		mvarCrtd_User =  clsCommon.GetValue(row("Crtd_User"), mvarCrtd_User.GetType().FullName)
		mvarLUpd_DateTime =  clsCommon.GetValue(row("LUpd_DateTime"), mvarLUpd_DateTime.GetType().FullName)
		mvarLUpd_Prog =  clsCommon.GetValue(row("LUpd_Prog"), mvarLUpd_Prog.GetType().FullName)
		mvarLUpd_User =  clsCommon.GetValue(row("LUpd_User"), mvarLUpd_User.GetType().FullName)
		mvartstamp =  clsCommon.GetValue(row("tstamp"), mvartstamp.GetType().FullName)
		mvarPriceID =  clsCommon.GetValue(row("PriceID"), mvarPriceID.GetType().FullName)
		mvarDescr =  clsCommon.GetValue(row("Descr"), mvarDescr.GetType().FullName)
		mvarQtyBreak =  clsCommon.GetValue(row("QtyBreak"), mvarQtyBreak.GetType().FullName)
		mvarDisc =  clsCommon.GetValue(row("Disc"), mvarDisc.GetType().FullName)
	End Sub
#End Region 

#Region "Private Methods"
#End Region 

End Class
