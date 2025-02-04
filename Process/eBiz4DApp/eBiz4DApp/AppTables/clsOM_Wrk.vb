'-- ------------------------------------------------------------
'-- Class name    :  clsOM_Wrk
'-- Created date  :  5/13/2015
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework.Common
Imports HQFramework.DAL
Public Class clsOM_Wrk
#Region "Constants"
	Private Const PP_OM_Wrk As String = "PP_OM_Wrk"
#End Region 

#Region "Member Variables"
	Private mvarInternetAddress As System.String

	Private mvarUserID As System.String

	Private mvarBranchID As System.String

	Private mvarOrderNbr As System.String

	Private mvarProcID As System.String

	Private mvarARRefNbr As System.String

	Private mvarARDocType As System.String

	Private mvarINDocType As System.String

	Private mvarShipperID As System.String

	Private mvarSalesType As System.String

	Private mvarQtyDecPl As System.Int16

	Private mvarDocDesc As System.String

	Private mvarTranAmtDecPl As System.Int16

	Private mvarCuryTranAmtDecPl As System.Int16

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
	Public Property InternetAddress() As System.String
		Get
			Return mvarInternetAddress
		End Get
		Set(ByVal Value As System.String)
			mvarInternetAddress = Value
		End Set
	End Property

	Public Property UserID() As System.String
		Get
			Return mvarUserID
		End Get
		Set(ByVal Value As System.String)
			mvarUserID = Value
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

	Public Property OrderNbr() As System.String
		Get
			Return mvarOrderNbr
		End Get
		Set(ByVal Value As System.String)
			mvarOrderNbr = Value
		End Set
	End Property

	Public Property ProcID() As System.String
		Get
			Return mvarProcID
		End Get
		Set(ByVal Value As System.String)
			mvarProcID = Value
		End Set
	End Property

	Public Property ARRefNbr() As System.String
		Get
			Return mvarARRefNbr
		End Get
		Set(ByVal Value As System.String)
			mvarARRefNbr = Value
		End Set
	End Property

	Public Property ARDocType() As System.String
		Get
			Return mvarARDocType
		End Get
		Set(ByVal Value As System.String)
			mvarARDocType = Value
		End Set
	End Property

	Public Property INDocType() As System.String
		Get
			Return mvarINDocType
		End Get
		Set(ByVal Value As System.String)
			mvarINDocType = Value
		End Set
	End Property

	Public Property ShipperID() As System.String
		Get
			Return mvarShipperID
		End Get
		Set(ByVal Value As System.String)
			mvarShipperID = Value
		End Set
	End Property

	Public Property SalesType() As System.String
		Get
			Return mvarSalesType
		End Get
		Set(ByVal Value As System.String)
			mvarSalesType = Value
		End Set
	End Property

	Public Property QtyDecPl() As System.Int16
		Get
			Return mvarQtyDecPl
		End Get
		Set(ByVal Value As System.Int16)
			mvarQtyDecPl = Value
		End Set
	End Property

	Public Property DocDesc() As System.String
		Get
			Return mvarDocDesc
		End Get
		Set(ByVal Value As System.String)
			mvarDocDesc = Value
		End Set
	End Property

	Public Property TranAmtDecPl() As System.Int16
		Get
			Return mvarTranAmtDecPl
		End Get
		Set(ByVal Value As System.Int16)
			mvarTranAmtDecPl = Value
		End Set
	End Property

	Public Property CuryTranAmtDecPl() As System.Int16
		Get
			Return mvarCuryTranAmtDecPl
		End Get
		Set(ByVal Value As System.Int16)
			mvarCuryTranAmtDecPl = Value
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
			pc.Add(New ParamStruct("@InternetAddress", DbType.String,clsCommon.GetValueDBNull(Me.mvarInternetAddress), ParameterDirection.Input,21 ))
			pc.Add(New ParamStruct("@UserID", DbType.String,clsCommon.GetValueDBNull(Me.mvarUserID), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String,clsCommon.GetValueDBNull(Me.mvarBranchID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@OrderNbr", DbType.String,clsCommon.GetValueDBNull(Me.mvarOrderNbr), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@ProcID", DbType.String,clsCommon.GetValueDBNull(Me.mvarProcID), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@ARRefNbr", DbType.String,clsCommon.GetValueDBNull(Me.mvarARRefNbr), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@ARDocType", DbType.String,clsCommon.GetValueDBNull(Me.mvarARDocType), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@INDocType", DbType.String,clsCommon.GetValueDBNull(Me.mvarINDocType), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@ShipperID", DbType.String,clsCommon.GetValueDBNull(Me.mvarShipperID), ParameterDirection.Input,15 ))
			pc.Add(New ParamStruct("@SalesType", DbType.String,clsCommon.GetValueDBNull(Me.mvarSalesType), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@QtyDecPl", DbType.int16,clsCommon.GetValueDBNull(Me.mvarQtyDecPl), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@DocDesc", DbType.String,clsCommon.GetValueDBNull(Me.mvarDocDesc), ParameterDirection.Input,300 ))
			pc.Add(New ParamStruct("@TranAmtDecPl", DbType.int16,clsCommon.GetValueDBNull(Me.mvarTranAmtDecPl), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@CuryTranAmtDecPl", DbType.int16,clsCommon.GetValueDBNull(Me.mvarCuryTranAmtDecPl), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@tstamp", DbType.String,clsCommon.GetValueDBNull(Me.mvartstamp), ParameterDirection.Input,18 ))
			DAL.ExecPreparedSQL(PP_OM_Wrk, CommandType.StoredProcedure, pc,"")
		Me.mvarInternetAddress = clsCommon.GetValue(pc.Item("@InternetAddress").Value, mvarInternetAddress.GetType().FullName)
		Return (Me.mvarInternetAddress <> String.Empty )
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Update() as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Update", ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@InternetAddress",DbType.String, clsCommon.GetValueDBNull(me.mvarInternetAddress), ParameterDirection.Input,21 ))
			 pc.Add(New ParamStruct("@UserID",DbType.String, clsCommon.GetValueDBNull(me.mvarUserID), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@BranchID",DbType.String, clsCommon.GetValueDBNull(me.mvarBranchID), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@OrderNbr",DbType.String, clsCommon.GetValueDBNull(me.mvarOrderNbr), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@ProcID",DbType.String, clsCommon.GetValueDBNull(me.mvarProcID), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@ARRefNbr",DbType.String, clsCommon.GetValueDBNull(me.mvarARRefNbr), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@ARDocType",DbType.String, clsCommon.GetValueDBNull(me.mvarARDocType), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@INDocType",DbType.String, clsCommon.GetValueDBNull(me.mvarINDocType), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@ShipperID",DbType.String, clsCommon.GetValueDBNull(me.mvarShipperID), ParameterDirection.Input,15 ))
			 pc.Add(New ParamStruct("@SalesType",DbType.String, clsCommon.GetValueDBNull(me.mvarSalesType), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@QtyDecPl",DbType.int16, clsCommon.GetValueDBNull(me.mvarQtyDecPl), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@DocDesc",DbType.String, clsCommon.GetValueDBNull(me.mvarDocDesc), ParameterDirection.Input,300 ))
			 pc.Add(New ParamStruct("@TranAmtDecPl",DbType.int16, clsCommon.GetValueDBNull(me.mvarTranAmtDecPl), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@CuryTranAmtDecPl",DbType.int16, clsCommon.GetValueDBNull(me.mvarCuryTranAmtDecPl), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@tstamp",DbType.String, clsCommon.GetValueDBNull(me.mvartstamp), ParameterDirection.Input,18 ))
			Return (DAL.ExecNonQuery(PP_OM_Wrk, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Delete(ByVal InternetAddress As System.String, ByVal UserID As System.String, ByVal BranchID As System.String, ByVal OrderNbr As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@InternetAddress",DbType.String, clsCommon.GetValueDBNull(InternetAddress), ParameterDirection.Input,21 ))
			pc.Add(New ParamStruct("@UserID",DbType.String, clsCommon.GetValueDBNull(UserID), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@BranchID",DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@OrderNbr",DbType.String, clsCommon.GetValueDBNull(OrderNbr), ParameterDirection.Input,10 ))
			Return (DAL.ExecNonQuery(PP_OM_Wrk, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function GetAll(ByVal InternetAddress As System.String, ByVal UserID As System.String, ByVal BranchID As System.String, ByVal OrderNbr As System.String) as DataTable 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			Dim ds As New DataSet
			pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@InternetAddress", DbType.String, clsCommon.GetValueDBNull(InternetAddress), ParameterDirection.Input, 21 ))
			pc.Add(New ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(UserID), ParameterDirection.Input, 10 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 30 ))
			pc.Add(New ParamStruct("@OrderNbr", DbType.String, clsCommon.GetValueDBNull(OrderNbr), ParameterDirection.Input, 10 ))
			ds = DAL.ExecDataSet(PP_OM_Wrk, CommandType.StoredProcedure, pc,"")
			Dim keys(3) As DataColumn
			Dim column As DataColumn
			column = ds.Tables(0).Columns("InternetAddress")
			Keys(0) = column
			column = ds.Tables(0).Columns("UserID")
			Keys(1) = column
			column = ds.Tables(0).Columns("BranchID")
			Keys(2) = column
			column = ds.Tables(0).Columns("OrderNbr")
			Keys(3) = column
			ds.Tables(0).PrimaryKey = Keys
			Return ds.Tables(0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Sub Reset()
		mvarInternetAddress = String.Empty
		mvarUserID = String.Empty
		mvarBranchID = String.Empty
		mvarOrderNbr = String.Empty
		mvarProcID = String.Empty
		mvarARRefNbr = String.Empty
		mvarARDocType = String.Empty
		mvarINDocType = String.Empty
		mvarShipperID = String.Empty
		mvarSalesType = String.Empty
		mvarQtyDecPl = 0
		mvarDocDesc = String.Empty
		mvarTranAmtDecPl = 0
		mvarCuryTranAmtDecPl = 0
		mvartstamp = String.Empty
	End Sub
	Public Function GetByKey(ByVal InternetAddress As System.String, ByVal UserID As System.String, ByVal BranchID As System.String, ByVal OrderNbr As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Dim ds As New DataSet 
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@InternetAddress", DbType.String, clsCommon.GetValueDBNull(InternetAddress), ParameterDirection.InputOutput, 21 ))
			pc.Add(New ParamStruct("@UserID", DbType.String, clsCommon.GetValueDBNull(UserID), ParameterDirection.InputOutput, 10 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.InputOutput, 30 ))
			pc.Add(New ParamStruct("@OrderNbr", DbType.String, clsCommon.GetValueDBNull(OrderNbr), ParameterDirection.InputOutput, 10 ))
			ds = DAL.ExecDataSet(PP_OM_Wrk, CommandType.StoredProcedure, pc,"")
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
		mvarInternetAddress =  clsCommon.GetValue(row("InternetAddress"), mvarInternetAddress.GetType().FullName)
		mvarUserID =  clsCommon.GetValue(row("UserID"), mvarUserID.GetType().FullName)
		mvarBranchID =  clsCommon.GetValue(row("BranchID"), mvarBranchID.GetType().FullName)
		mvarOrderNbr =  clsCommon.GetValue(row("OrderNbr"), mvarOrderNbr.GetType().FullName)
		mvarProcID =  clsCommon.GetValue(row("ProcID"), mvarProcID.GetType().FullName)
		mvarARRefNbr =  clsCommon.GetValue(row("ARRefNbr"), mvarARRefNbr.GetType().FullName)
		mvarARDocType =  clsCommon.GetValue(row("ARDocType"), mvarARDocType.GetType().FullName)
		mvarINDocType =  clsCommon.GetValue(row("INDocType"), mvarINDocType.GetType().FullName)
		mvarShipperID =  clsCommon.GetValue(row("ShipperID"), mvarShipperID.GetType().FullName)
		mvarSalesType =  clsCommon.GetValue(row("SalesType"), mvarSalesType.GetType().FullName)
		mvarQtyDecPl =  clsCommon.GetValue(row("QtyDecPl"), mvarQtyDecPl.GetType().FullName)
		mvarDocDesc =  clsCommon.GetValue(row("DocDesc"), mvarDocDesc.GetType().FullName)
		mvarTranAmtDecPl =  clsCommon.GetValue(row("TranAmtDecPl"), mvarTranAmtDecPl.GetType().FullName)
		mvarCuryTranAmtDecPl =  clsCommon.GetValue(row("CuryTranAmtDecPl"), mvarCuryTranAmtDecPl.GetType().FullName)
		mvartstamp =  clsCommon.GetValue(row("tstamp"), mvartstamp.GetType().FullName)
	End Sub
#End Region 

#Region "Private Methods"
#End Region 

End Class
