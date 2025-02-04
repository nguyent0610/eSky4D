'-- ------------------------------------------------------------
'-- Class name    :  clsPO_Setup
'-- Created date  :  5/13/2015
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework.Common
Imports HQFramework.DAL
Public Class clsPO_Setup
#Region "Constants"
	Private Const PP_PO_Setup As String = "PP_PO_Setup"
#End Region 

#Region "Member Variables"
	Private mvarBranchID As System.String

	Private mvarSetupID As System.String

	Private mvarAutoRef As System.Int16

	Private mvarAutoReleaseAP As System.Int16

	Private mvarBillAddr1 As System.String

	Private mvarBillAddr2 As System.String

	Private mvarBillAttn As System.String

	Private mvarBillCity As System.String

	Private mvarBillCountry As System.String

	Private mvarBillEmail As System.String

	Private mvarBillFax As System.String

	Private mvarBillName As System.String

	Private mvarBillPhone As System.String

	Private mvarBillState As System.String

	Private mvarBillZip As System.String

	Private mvarDfltLstUnitCost As System.String

	Private mvarDfltRcptFrom As System.String

	Private mvarDfltRcptUnitFromIN As System.Int16

	Private mvarLastBatNbr As System.String

	Private mvarLastPONbr As System.String

	Private mvarLastRcptNbr As System.String

	Private mvarPreFixBat As System.String

	Private mvarShipAddr1 As System.String

	Private mvarShipAddr2 As System.String

	Private mvarShipAttn As System.String

	Private mvarShipCity As System.String

	Private mvarShipCountry As System.String

	Private mvarShipEmail As System.String

	Private mvarShipFax As System.String

	Private mvarShipName As System.String

	Private mvarShipPhone As System.String

	Private mvarShipState As System.String

	Private mvarShipZip As System.String

	Private mvarUseBarCode As System.Int16

	Private mvarCrtd_DateTime As System.DateTime

	Private mvarCrtd_Prog As System.String

	Private mvarCrtd_User As System.String

	Private mvarLUpd_DateTime As System.DateTime

	Private mvarLUpd_Prog As System.String

	Private mvarLUpd_User As System.String

	Private mvartstamp As System.String

	Private mvarEditablePOPrice As System.Boolean

	Private mvarUseIN As System.Boolean

	Private mvarUseAP As System.Boolean

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

	Public Property SetupID() As System.String
		Get
			Return mvarSetupID
		End Get
		Set(ByVal Value As System.String)
			mvarSetupID = Value
		End Set
	End Property

	Public Property AutoRef() As System.Int16
		Get
			Return mvarAutoRef
		End Get
		Set(ByVal Value As System.Int16)
			mvarAutoRef = Value
		End Set
	End Property

	Public Property AutoReleaseAP() As System.Int16
		Get
			Return mvarAutoReleaseAP
		End Get
		Set(ByVal Value As System.Int16)
			mvarAutoReleaseAP = Value
		End Set
	End Property

	Public Property BillAddr1() As System.String
		Get
			Return mvarBillAddr1
		End Get
		Set(ByVal Value As System.String)
			mvarBillAddr1 = Value
		End Set
	End Property

	Public Property BillAddr2() As System.String
		Get
			Return mvarBillAddr2
		End Get
		Set(ByVal Value As System.String)
			mvarBillAddr2 = Value
		End Set
	End Property

	Public Property BillAttn() As System.String
		Get
			Return mvarBillAttn
		End Get
		Set(ByVal Value As System.String)
			mvarBillAttn = Value
		End Set
	End Property

	Public Property BillCity() As System.String
		Get
			Return mvarBillCity
		End Get
		Set(ByVal Value As System.String)
			mvarBillCity = Value
		End Set
	End Property

	Public Property BillCountry() As System.String
		Get
			Return mvarBillCountry
		End Get
		Set(ByVal Value As System.String)
			mvarBillCountry = Value
		End Set
	End Property

	Public Property BillEmail() As System.String
		Get
			Return mvarBillEmail
		End Get
		Set(ByVal Value As System.String)
			mvarBillEmail = Value
		End Set
	End Property

	Public Property BillFax() As System.String
		Get
			Return mvarBillFax
		End Get
		Set(ByVal Value As System.String)
			mvarBillFax = Value
		End Set
	End Property

	Public Property BillName() As System.String
		Get
			Return mvarBillName
		End Get
		Set(ByVal Value As System.String)
			mvarBillName = Value
		End Set
	End Property

	Public Property BillPhone() As System.String
		Get
			Return mvarBillPhone
		End Get
		Set(ByVal Value As System.String)
			mvarBillPhone = Value
		End Set
	End Property

	Public Property BillState() As System.String
		Get
			Return mvarBillState
		End Get
		Set(ByVal Value As System.String)
			mvarBillState = Value
		End Set
	End Property

	Public Property BillZip() As System.String
		Get
			Return mvarBillZip
		End Get
		Set(ByVal Value As System.String)
			mvarBillZip = Value
		End Set
	End Property

	Public Property DfltLstUnitCost() As System.String
		Get
			Return mvarDfltLstUnitCost
		End Get
		Set(ByVal Value As System.String)
			mvarDfltLstUnitCost = Value
		End Set
	End Property

	Public Property DfltRcptFrom() As System.String
		Get
			Return mvarDfltRcptFrom
		End Get
		Set(ByVal Value As System.String)
			mvarDfltRcptFrom = Value
		End Set
	End Property

	Public Property DfltRcptUnitFromIN() As System.Int16
		Get
			Return mvarDfltRcptUnitFromIN
		End Get
		Set(ByVal Value As System.Int16)
			mvarDfltRcptUnitFromIN = Value
		End Set
	End Property

	Public Property LastBatNbr() As System.String
		Get
			Return mvarLastBatNbr
		End Get
		Set(ByVal Value As System.String)
			mvarLastBatNbr = Value
		End Set
	End Property

	Public Property LastPONbr() As System.String
		Get
			Return mvarLastPONbr
		End Get
		Set(ByVal Value As System.String)
			mvarLastPONbr = Value
		End Set
	End Property

	Public Property LastRcptNbr() As System.String
		Get
			Return mvarLastRcptNbr
		End Get
		Set(ByVal Value As System.String)
			mvarLastRcptNbr = Value
		End Set
	End Property

	Public Property PreFixBat() As System.String
		Get
			Return mvarPreFixBat
		End Get
		Set(ByVal Value As System.String)
			mvarPreFixBat = Value
		End Set
	End Property

	Public Property ShipAddr1() As System.String
		Get
			Return mvarShipAddr1
		End Get
		Set(ByVal Value As System.String)
			mvarShipAddr1 = Value
		End Set
	End Property

	Public Property ShipAddr2() As System.String
		Get
			Return mvarShipAddr2
		End Get
		Set(ByVal Value As System.String)
			mvarShipAddr2 = Value
		End Set
	End Property

	Public Property ShipAttn() As System.String
		Get
			Return mvarShipAttn
		End Get
		Set(ByVal Value As System.String)
			mvarShipAttn = Value
		End Set
	End Property

	Public Property ShipCity() As System.String
		Get
			Return mvarShipCity
		End Get
		Set(ByVal Value As System.String)
			mvarShipCity = Value
		End Set
	End Property

	Public Property ShipCountry() As System.String
		Get
			Return mvarShipCountry
		End Get
		Set(ByVal Value As System.String)
			mvarShipCountry = Value
		End Set
	End Property

	Public Property ShipEmail() As System.String
		Get
			Return mvarShipEmail
		End Get
		Set(ByVal Value As System.String)
			mvarShipEmail = Value
		End Set
	End Property

	Public Property ShipFax() As System.String
		Get
			Return mvarShipFax
		End Get
		Set(ByVal Value As System.String)
			mvarShipFax = Value
		End Set
	End Property

	Public Property ShipName() As System.String
		Get
			Return mvarShipName
		End Get
		Set(ByVal Value As System.String)
			mvarShipName = Value
		End Set
	End Property

	Public Property ShipPhone() As System.String
		Get
			Return mvarShipPhone
		End Get
		Set(ByVal Value As System.String)
			mvarShipPhone = Value
		End Set
	End Property

	Public Property ShipState() As System.String
		Get
			Return mvarShipState
		End Get
		Set(ByVal Value As System.String)
			mvarShipState = Value
		End Set
	End Property

	Public Property ShipZip() As System.String
		Get
			Return mvarShipZip
		End Get
		Set(ByVal Value As System.String)
			mvarShipZip = Value
		End Set
	End Property

	Public Property UseBarCode() As System.Int16
		Get
			Return mvarUseBarCode
		End Get
		Set(ByVal Value As System.Int16)
			mvarUseBarCode = Value
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

	Public Property EditablePOPrice() As System.Boolean
		Get
			Return mvarEditablePOPrice
		End Get
		Set(ByVal Value As System.Boolean)
			mvarEditablePOPrice = Value
		End Set
	End Property

	Public Property UseIN() As System.Boolean
		Get
			Return mvarUseIN
		End Get
		Set(ByVal Value As System.Boolean)
			mvarUseIN = Value
		End Set
	End Property

	Public Property UseAP() As System.Boolean
		Get
			Return mvarUseAP
		End Get
		Set(ByVal Value As System.Boolean)
			mvarUseAP = Value
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
			pc.Add(New ParamStruct("@SetupID", DbType.String,clsCommon.GetValueDBNull(Me.mvarSetupID), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@AutoRef", DbType.int16,clsCommon.GetValueDBNull(Me.mvarAutoRef), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@AutoReleaseAP", DbType.int16,clsCommon.GetValueDBNull(Me.mvarAutoReleaseAP), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@BillAddr1", DbType.String,clsCommon.GetValueDBNull(Me.mvarBillAddr1), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@BillAddr2", DbType.String,clsCommon.GetValueDBNull(Me.mvarBillAddr2), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@BillAttn", DbType.String,clsCommon.GetValueDBNull(Me.mvarBillAttn), ParameterDirection.Input,200 ))
			pc.Add(New ParamStruct("@BillCity", DbType.String,clsCommon.GetValueDBNull(Me.mvarBillCity), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@BillCountry", DbType.String,clsCommon.GetValueDBNull(Me.mvarBillCountry), ParameterDirection.Input,3 ))
			pc.Add(New ParamStruct("@BillEmail", DbType.String,clsCommon.GetValueDBNull(Me.mvarBillEmail), ParameterDirection.Input,40 ))
			pc.Add(New ParamStruct("@BillFax", DbType.String,clsCommon.GetValueDBNull(Me.mvarBillFax), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@BillName", DbType.String,clsCommon.GetValueDBNull(Me.mvarBillName), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@BillPhone", DbType.String,clsCommon.GetValueDBNull(Me.mvarBillPhone), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@BillState", DbType.String,clsCommon.GetValueDBNull(Me.mvarBillState), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@BillZip", DbType.String,clsCommon.GetValueDBNull(Me.mvarBillZip), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@DfltLstUnitCost", DbType.String,clsCommon.GetValueDBNull(Me.mvarDfltLstUnitCost), ParameterDirection.Input,1 ))
			pc.Add(New ParamStruct("@DfltRcptFrom", DbType.String,clsCommon.GetValueDBNull(Me.mvarDfltRcptFrom), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@DfltRcptUnitFromIN", DbType.int16,clsCommon.GetValueDBNull(Me.mvarDfltRcptUnitFromIN), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@LastBatNbr", DbType.String,clsCommon.GetValueDBNull(Me.mvarLastBatNbr), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@LastPONbr", DbType.String,clsCommon.GetValueDBNull(Me.mvarLastPONbr), ParameterDirection.Input,100 ))
			pc.Add(New ParamStruct("@LastRcptNbr", DbType.String,clsCommon.GetValueDBNull(Me.mvarLastRcptNbr), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@PreFixBat", DbType.String,clsCommon.GetValueDBNull(Me.mvarPreFixBat), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@ShipAddr1", DbType.String,clsCommon.GetValueDBNull(Me.mvarShipAddr1), ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@ShipAddr2", DbType.String,clsCommon.GetValueDBNull(Me.mvarShipAddr2), ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@ShipAttn", DbType.String,clsCommon.GetValueDBNull(Me.mvarShipAttn), ParameterDirection.Input,200 ))
			pc.Add(New ParamStruct("@ShipCity", DbType.String,clsCommon.GetValueDBNull(Me.mvarShipCity), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@ShipCountry", DbType.String,clsCommon.GetValueDBNull(Me.mvarShipCountry), ParameterDirection.Input,3 ))
			pc.Add(New ParamStruct("@ShipEmail", DbType.String,clsCommon.GetValueDBNull(Me.mvarShipEmail), ParameterDirection.Input,40 ))
			pc.Add(New ParamStruct("@ShipFax", DbType.String,clsCommon.GetValueDBNull(Me.mvarShipFax), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@ShipName", DbType.String,clsCommon.GetValueDBNull(Me.mvarShipName), ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@ShipPhone", DbType.String,clsCommon.GetValueDBNull(Me.mvarShipPhone), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@ShipState", DbType.String,clsCommon.GetValueDBNull(Me.mvarShipState), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@ShipZip", DbType.String,clsCommon.GetValueDBNull(Me.mvarShipZip), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@UseBarCode", DbType.int16,clsCommon.GetValueDBNull(Me.mvarUseBarCode), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@Crtd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@Crtd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_Prog), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@Crtd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_User), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@LUpd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@LUpd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_Prog), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@LUpd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_User), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@tstamp", DbType.String,clsCommon.GetValueDBNull(Me.mvartstamp), ParameterDirection.Input,18 ))
			pc.Add(New ParamStruct("@EditablePOPrice", DbType.Boolean,clsCommon.GetValueDBNull(Me.mvarEditablePOPrice), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@UseIN", DbType.Boolean,clsCommon.GetValueDBNull(Me.mvarUseIN), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@UseAP", DbType.Boolean,clsCommon.GetValueDBNull(Me.mvarUseAP), ParameterDirection.Input,2 ))
			DAL.ExecPreparedSQL(PP_PO_Setup, CommandType.StoredProcedure, pc,"")
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
			 pc.Add(New ParamStruct("@SetupID",DbType.String, clsCommon.GetValueDBNull(me.mvarSetupID), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@AutoRef",DbType.int16, clsCommon.GetValueDBNull(me.mvarAutoRef), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@AutoReleaseAP",DbType.int16, clsCommon.GetValueDBNull(me.mvarAutoReleaseAP), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@BillAddr1",DbType.String, clsCommon.GetValueDBNull(me.mvarBillAddr1), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@BillAddr2",DbType.String, clsCommon.GetValueDBNull(me.mvarBillAddr2), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@BillAttn",DbType.String, clsCommon.GetValueDBNull(me.mvarBillAttn), ParameterDirection.Input,200 ))
			 pc.Add(New ParamStruct("@BillCity",DbType.String, clsCommon.GetValueDBNull(me.mvarBillCity), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@BillCountry",DbType.String, clsCommon.GetValueDBNull(me.mvarBillCountry), ParameterDirection.Input,3 ))
			 pc.Add(New ParamStruct("@BillEmail",DbType.String, clsCommon.GetValueDBNull(me.mvarBillEmail), ParameterDirection.Input,40 ))
			 pc.Add(New ParamStruct("@BillFax",DbType.String, clsCommon.GetValueDBNull(me.mvarBillFax), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@BillName",DbType.String, clsCommon.GetValueDBNull(me.mvarBillName), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@BillPhone",DbType.String, clsCommon.GetValueDBNull(me.mvarBillPhone), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@BillState",DbType.String, clsCommon.GetValueDBNull(me.mvarBillState), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@BillZip",DbType.String, clsCommon.GetValueDBNull(me.mvarBillZip), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@DfltLstUnitCost",DbType.String, clsCommon.GetValueDBNull(me.mvarDfltLstUnitCost), ParameterDirection.Input,1 ))
			 pc.Add(New ParamStruct("@DfltRcptFrom",DbType.String, clsCommon.GetValueDBNull(me.mvarDfltRcptFrom), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@DfltRcptUnitFromIN",DbType.int16, clsCommon.GetValueDBNull(me.mvarDfltRcptUnitFromIN), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@LastBatNbr",DbType.String, clsCommon.GetValueDBNull(me.mvarLastBatNbr), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@LastPONbr",DbType.String, clsCommon.GetValueDBNull(me.mvarLastPONbr), ParameterDirection.Input,100 ))
			 pc.Add(New ParamStruct("@LastRcptNbr",DbType.String, clsCommon.GetValueDBNull(me.mvarLastRcptNbr), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@PreFixBat",DbType.String, clsCommon.GetValueDBNull(me.mvarPreFixBat), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@ShipAddr1",DbType.String, clsCommon.GetValueDBNull(me.mvarShipAddr1), ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@ShipAddr2",DbType.String, clsCommon.GetValueDBNull(me.mvarShipAddr2), ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@ShipAttn",DbType.String, clsCommon.GetValueDBNull(me.mvarShipAttn), ParameterDirection.Input,200 ))
			 pc.Add(New ParamStruct("@ShipCity",DbType.String, clsCommon.GetValueDBNull(me.mvarShipCity), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@ShipCountry",DbType.String, clsCommon.GetValueDBNull(me.mvarShipCountry), ParameterDirection.Input,3 ))
			 pc.Add(New ParamStruct("@ShipEmail",DbType.String, clsCommon.GetValueDBNull(me.mvarShipEmail), ParameterDirection.Input,40 ))
			 pc.Add(New ParamStruct("@ShipFax",DbType.String, clsCommon.GetValueDBNull(me.mvarShipFax), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@ShipName",DbType.String, clsCommon.GetValueDBNull(me.mvarShipName), ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@ShipPhone",DbType.String, clsCommon.GetValueDBNull(me.mvarShipPhone), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@ShipState",DbType.String, clsCommon.GetValueDBNull(me.mvarShipState), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@ShipZip",DbType.String, clsCommon.GetValueDBNull(me.mvarShipZip), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@UseBarCode",DbType.int16, clsCommon.GetValueDBNull(me.mvarUseBarCode), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@Crtd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@Crtd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_Prog), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@Crtd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_User), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@LUpd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@LUpd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_Prog), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@LUpd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_User), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@tstamp",DbType.String, clsCommon.GetValueDBNull(me.mvartstamp), ParameterDirection.Input,18 ))
			 pc.Add(New ParamStruct("@EditablePOPrice",DbType.Boolean, clsCommon.GetValueDBNull(me.mvarEditablePOPrice), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@UseIN",DbType.Boolean, clsCommon.GetValueDBNull(me.mvarUseIN), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@UseAP",DbType.Boolean, clsCommon.GetValueDBNull(me.mvarUseAP), ParameterDirection.Input,2 ))
			Return (DAL.ExecNonQuery(PP_PO_Setup, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Delete(ByVal BranchID As System.String, ByVal SetupID As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID",DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@SetupID",DbType.String, clsCommon.GetValueDBNull(SetupID), ParameterDirection.Input,2 ))
			Return (DAL.ExecNonQuery(PP_PO_Setup, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function GetAll(ByVal BranchID As System.String, ByVal SetupID As System.String) as DataTable 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			Dim ds As New DataSet
			pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 30 ))
			pc.Add(New ParamStruct("@SetupID", DbType.String, clsCommon.GetValueDBNull(SetupID), ParameterDirection.Input, 2 ))
			ds = DAL.ExecDataSet(PP_PO_Setup, CommandType.StoredProcedure, pc,"")
			Dim keys(1) As DataColumn
			Dim column As DataColumn
			column = ds.Tables(0).Columns("BranchID")
			Keys(0) = column
			column = ds.Tables(0).Columns("SetupID")
			Keys(1) = column
			ds.Tables(0).PrimaryKey = Keys
			Return ds.Tables(0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Sub Reset()
		mvarBranchID = String.Empty
		mvarSetupID = String.Empty
		mvarAutoRef = 0
		mvarAutoReleaseAP = 0
		mvarBillAddr1 = String.Empty
		mvarBillAddr2 = String.Empty
		mvarBillAttn = String.Empty
		mvarBillCity = String.Empty
		mvarBillCountry = String.Empty
		mvarBillEmail = String.Empty
		mvarBillFax = String.Empty
		mvarBillName = String.Empty
		mvarBillPhone = String.Empty
		mvarBillState = String.Empty
		mvarBillZip = String.Empty
		mvarDfltLstUnitCost = String.Empty
		mvarDfltRcptFrom = String.Empty
		mvarDfltRcptUnitFromIN = 0
		mvarLastBatNbr = String.Empty
		mvarLastPONbr = String.Empty
		mvarLastRcptNbr = String.Empty
		mvarPreFixBat = String.Empty
		mvarShipAddr1 = String.Empty
		mvarShipAddr2 = String.Empty
		mvarShipAttn = String.Empty
		mvarShipCity = String.Empty
		mvarShipCountry = String.Empty
		mvarShipEmail = String.Empty
		mvarShipFax = String.Empty
		mvarShipName = String.Empty
		mvarShipPhone = String.Empty
		mvarShipState = String.Empty
		mvarShipZip = String.Empty
		mvarUseBarCode = 0
		mvarCrtd_DateTime = Today
		mvarCrtd_Prog = String.Empty
		mvarCrtd_User = String.Empty
		mvarLUpd_DateTime = Today
		mvarLUpd_Prog = String.Empty
		mvarLUpd_User = String.Empty
		mvartstamp = String.Empty
		mvarEditablePOPrice = False
		mvarUseIN = False
		mvarUseAP = False
	End Sub
	Public Function GetByKey(ByVal BranchID As System.String, ByVal SetupID As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Dim ds As New DataSet 
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.InputOutput, 30 ))
			pc.Add(New ParamStruct("@SetupID", DbType.String, clsCommon.GetValueDBNull(SetupID), ParameterDirection.InputOutput, 2 ))
			ds = DAL.ExecDataSet(PP_PO_Setup, CommandType.StoredProcedure, pc,"")
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
		mvarSetupID =  clsCommon.GetValue(row("SetupID"), mvarSetupID.GetType().FullName)
		mvarAutoRef =  clsCommon.GetValue(row("AutoRef"), mvarAutoRef.GetType().FullName)
		mvarAutoReleaseAP =  clsCommon.GetValue(row("AutoReleaseAP"), mvarAutoReleaseAP.GetType().FullName)
		mvarBillAddr1 =  clsCommon.GetValue(row("BillAddr1"), mvarBillAddr1.GetType().FullName)
		mvarBillAddr2 =  clsCommon.GetValue(row("BillAddr2"), mvarBillAddr2.GetType().FullName)
		mvarBillAttn =  clsCommon.GetValue(row("BillAttn"), mvarBillAttn.GetType().FullName)
		mvarBillCity =  clsCommon.GetValue(row("BillCity"), mvarBillCity.GetType().FullName)
		mvarBillCountry =  clsCommon.GetValue(row("BillCountry"), mvarBillCountry.GetType().FullName)
		mvarBillEmail =  clsCommon.GetValue(row("BillEmail"), mvarBillEmail.GetType().FullName)
		mvarBillFax =  clsCommon.GetValue(row("BillFax"), mvarBillFax.GetType().FullName)
		mvarBillName =  clsCommon.GetValue(row("BillName"), mvarBillName.GetType().FullName)
		mvarBillPhone =  clsCommon.GetValue(row("BillPhone"), mvarBillPhone.GetType().FullName)
		mvarBillState =  clsCommon.GetValue(row("BillState"), mvarBillState.GetType().FullName)
		mvarBillZip =  clsCommon.GetValue(row("BillZip"), mvarBillZip.GetType().FullName)
		mvarDfltLstUnitCost =  clsCommon.GetValue(row("DfltLstUnitCost"), mvarDfltLstUnitCost.GetType().FullName)
		mvarDfltRcptFrom =  clsCommon.GetValue(row("DfltRcptFrom"), mvarDfltRcptFrom.GetType().FullName)
		mvarDfltRcptUnitFromIN =  clsCommon.GetValue(row("DfltRcptUnitFromIN"), mvarDfltRcptUnitFromIN.GetType().FullName)
		mvarLastBatNbr =  clsCommon.GetValue(row("LastBatNbr"), mvarLastBatNbr.GetType().FullName)
		mvarLastPONbr =  clsCommon.GetValue(row("LastPONbr"), mvarLastPONbr.GetType().FullName)
		mvarLastRcptNbr =  clsCommon.GetValue(row("LastRcptNbr"), mvarLastRcptNbr.GetType().FullName)
		mvarPreFixBat =  clsCommon.GetValue(row("PreFixBat"), mvarPreFixBat.GetType().FullName)
		mvarShipAddr1 =  clsCommon.GetValue(row("ShipAddr1"), mvarShipAddr1.GetType().FullName)
		mvarShipAddr2 =  clsCommon.GetValue(row("ShipAddr2"), mvarShipAddr2.GetType().FullName)
		mvarShipAttn =  clsCommon.GetValue(row("ShipAttn"), mvarShipAttn.GetType().FullName)
		mvarShipCity =  clsCommon.GetValue(row("ShipCity"), mvarShipCity.GetType().FullName)
		mvarShipCountry =  clsCommon.GetValue(row("ShipCountry"), mvarShipCountry.GetType().FullName)
		mvarShipEmail =  clsCommon.GetValue(row("ShipEmail"), mvarShipEmail.GetType().FullName)
		mvarShipFax =  clsCommon.GetValue(row("ShipFax"), mvarShipFax.GetType().FullName)
		mvarShipName =  clsCommon.GetValue(row("ShipName"), mvarShipName.GetType().FullName)
		mvarShipPhone =  clsCommon.GetValue(row("ShipPhone"), mvarShipPhone.GetType().FullName)
		mvarShipState =  clsCommon.GetValue(row("ShipState"), mvarShipState.GetType().FullName)
		mvarShipZip =  clsCommon.GetValue(row("ShipZip"), mvarShipZip.GetType().FullName)
		mvarUseBarCode =  clsCommon.GetValue(row("UseBarCode"), mvarUseBarCode.GetType().FullName)
		mvarCrtd_DateTime =  clsCommon.GetValue(row("Crtd_DateTime"), mvarCrtd_DateTime.GetType().FullName)
		mvarCrtd_Prog =  clsCommon.GetValue(row("Crtd_Prog"), mvarCrtd_Prog.GetType().FullName)
		mvarCrtd_User =  clsCommon.GetValue(row("Crtd_User"), mvarCrtd_User.GetType().FullName)
		mvarLUpd_DateTime =  clsCommon.GetValue(row("LUpd_DateTime"), mvarLUpd_DateTime.GetType().FullName)
		mvarLUpd_Prog =  clsCommon.GetValue(row("LUpd_Prog"), mvarLUpd_Prog.GetType().FullName)
		mvarLUpd_User =  clsCommon.GetValue(row("LUpd_User"), mvarLUpd_User.GetType().FullName)
		mvartstamp =  clsCommon.GetValue(row("tstamp"), mvartstamp.GetType().FullName)
		mvarEditablePOPrice =  clsCommon.GetValue(row("EditablePOPrice"), mvarEditablePOPrice.GetType().FullName)
		mvarUseIN =  clsCommon.GetValue(row("UseIN"), mvarUseIN.GetType().FullName)
		mvarUseAP =  clsCommon.GetValue(row("UseAP"), mvarUseAP.GetType().FullName)
	End Sub
#End Region 

#Region "Private Methods"
#End Region 

End Class
