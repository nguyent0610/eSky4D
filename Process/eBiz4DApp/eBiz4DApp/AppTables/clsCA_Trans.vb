'-- ------------------------------------------------------------
'-- Class name    :  clsCA_Trans
'-- Created date  :  2016-09-14
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework
Imports HQFramework.DAL
Imports HQFramework.Common
Public Class clsCA_Trans
#Region "Constants"
	Private Const PP_CA_Trans As String = "PP_CA_Trans"
#End Region 

#Region "Member Variables"
	Private mvarBranchID As System.String

	Private mvarBatNbr As System.String

	Private mvarRefNbr As System.String

	Private mvarLineRef As System.String

	Private mvarBankAcct As System.String

	Private mvarCustID As System.String

	Private mvarEmployeeID As System.String

	Private mvarEntryID As System.String

	Private mvarTranType As System.String

	Private mvarRlsed As System.Int16

	Private mvarTranAmt As System.Double

	Private mvarTranDate As System.DateTime

	Private mvarTranDesc As System.String

	Private mvarVendID As System.String

	Private mvarVendName As System.String

	Private mvarAddr As System.String

	Private mvarInvcDate As System.DateTime

	Private mvarInvcNbr As System.String

	Private mvarInvcNote As System.String

	Private mvarTaxRegNbr As System.String

	Private mvarTaxID As System.String

	Private mvarPayerReceiver As System.String

	Private mvarPayerReceiverAddr As System.String

	Private mvarCrtd_DateTime As System.DateTime

	Private mvarCrtd_Prog As System.String

	Private mvarCrtd_User As System.String

	Private mvarLUpd_DateTime As System.DateTime

	Private mvarLUpd_Prog As System.String

	Private mvarLUpd_User As System.String

	Private mvartstamp As System.String

	Private mvarTransportation As System.String

	Private mvarTrsfToBranchID As System.String

	Private mvarTrsfToBankAcct As System.String

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

	Public Property BatNbr() As System.String
		Get
			Return mvarBatNbr
		End Get
		Set(ByVal Value As System.String)
			mvarBatNbr = Value
		End Set
	End Property

	Public Property RefNbr() As System.String
		Get
			Return mvarRefNbr
		End Get
		Set(ByVal Value As System.String)
			mvarRefNbr = Value
		End Set
	End Property

	Public Property LineRef() As System.String
		Get
			Return mvarLineRef
		End Get
		Set(ByVal Value As System.String)
			mvarLineRef = Value
		End Set
	End Property

	Public Property BankAcct() As System.String
		Get
			Return mvarBankAcct
		End Get
		Set(ByVal Value As System.String)
			mvarBankAcct = Value
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

	Public Property EmployeeID() As System.String
		Get
			Return mvarEmployeeID
		End Get
		Set(ByVal Value As System.String)
			mvarEmployeeID = Value
		End Set
	End Property

	Public Property EntryID() As System.String
		Get
			Return mvarEntryID
		End Get
		Set(ByVal Value As System.String)
			mvarEntryID = Value
		End Set
	End Property

	Public Property TranType() As System.String
		Get
			Return mvarTranType
		End Get
		Set(ByVal Value As System.String)
			mvarTranType = Value
		End Set
	End Property

	Public Property Rlsed() As System.Int16
		Get
			Return mvarRlsed
		End Get
		Set(ByVal Value As System.Int16)
			mvarRlsed = Value
		End Set
	End Property

	Public Property TranAmt() As System.Double
		Get
			Return mvarTranAmt
		End Get
		Set(ByVal Value As System.Double)
			mvarTranAmt = Value
		End Set
	End Property

	Public Property TranDate() As System.DateTime
		Get
			Return mvarTranDate
		End Get
		Set(ByVal Value As System.DateTime)
			mvarTranDate = Value
		End Set
	End Property

	Public Property TranDesc() As System.String
		Get
			Return mvarTranDesc
		End Get
		Set(ByVal Value As System.String)
			mvarTranDesc = Value
		End Set
	End Property

	Public Property VendID() As System.String
		Get
			Return mvarVendID
		End Get
		Set(ByVal Value As System.String)
			mvarVendID = Value
		End Set
	End Property

	Public Property VendName() As System.String
		Get
			Return mvarVendName
		End Get
		Set(ByVal Value As System.String)
			mvarVendName = Value
		End Set
	End Property

	Public Property Addr() As System.String
		Get
			Return mvarAddr
		End Get
		Set(ByVal Value As System.String)
			mvarAddr = Value
		End Set
	End Property

	Public Property InvcDate() As System.DateTime
		Get
			Return mvarInvcDate
		End Get
		Set(ByVal Value As System.DateTime)
			mvarInvcDate = Value
		End Set
	End Property

	Public Property InvcNbr() As System.String
		Get
			Return mvarInvcNbr
		End Get
		Set(ByVal Value As System.String)
			mvarInvcNbr = Value
		End Set
	End Property

	Public Property InvcNote() As System.String
		Get
			Return mvarInvcNote
		End Get
		Set(ByVal Value As System.String)
			mvarInvcNote = Value
		End Set
	End Property

	Public Property TaxRegNbr() As System.String
		Get
			Return mvarTaxRegNbr
		End Get
		Set(ByVal Value As System.String)
			mvarTaxRegNbr = Value
		End Set
	End Property

	Public Property TaxID() As System.String
		Get
			Return mvarTaxID
		End Get
		Set(ByVal Value As System.String)
			mvarTaxID = Value
		End Set
	End Property

	Public Property PayerReceiver() As System.String
		Get
			Return mvarPayerReceiver
		End Get
		Set(ByVal Value As System.String)
			mvarPayerReceiver = Value
		End Set
	End Property

	Public Property PayerReceiverAddr() As System.String
		Get
			Return mvarPayerReceiverAddr
		End Get
		Set(ByVal Value As System.String)
			mvarPayerReceiverAddr = Value
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

	Public Property Transportation() As System.String
		Get
			Return mvarTransportation
		End Get
		Set(ByVal Value As System.String)
			mvarTransportation = Value
		End Set
	End Property

	Public Property TrsfToBranchID() As System.String
		Get
			Return mvarTrsfToBranchID
		End Get
		Set(ByVal Value As System.String)
			mvarTrsfToBranchID = Value
		End Set
	End Property

	Public Property TrsfToBankAcct() As System.String
		Get
			Return mvarTrsfToBankAcct
		End Get
		Set(ByVal Value As System.String)
			mvarTrsfToBankAcct = Value
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
			pc.Add(New ParamStruct("@BatNbr", DbType.String,clsCommon.GetValueDBNull(Me.mvarBatNbr), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@RefNbr", DbType.String,clsCommon.GetValueDBNull(Me.mvarRefNbr), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@LineRef", DbType.String,clsCommon.GetValueDBNull(Me.mvarLineRef), ParameterDirection.Input,5 ))
			pc.Add(New ParamStruct("@BankAcct", DbType.String,clsCommon.GetValueDBNull(Me.mvarBankAcct), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@CustID", DbType.String,clsCommon.GetValueDBNull(Me.mvarCustID), ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@EmployeeID", DbType.String,clsCommon.GetValueDBNull(Me.mvarEmployeeID), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@EntryID", DbType.String,clsCommon.GetValueDBNull(Me.mvarEntryID), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@TranType", DbType.String,clsCommon.GetValueDBNull(Me.mvarTranType), ParameterDirection.Input,3 ))
			pc.Add(New ParamStruct("@Rlsed", DbType.int16,clsCommon.GetValueDBNull(Me.mvarRlsed), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@TranAmt", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarTranAmt), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@TranDate", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarTranDate.Date), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@TranDesc", DbType.String,clsCommon.GetValueDBNull(Me.mvarTranDesc), ParameterDirection.Input,200 ))
			pc.Add(New ParamStruct("@VendID", DbType.String,clsCommon.GetValueDBNull(Me.mvarVendID), ParameterDirection.Input,15 ))
			pc.Add(New ParamStruct("@VendName", DbType.String,clsCommon.GetValueDBNull(Me.mvarVendName), ParameterDirection.Input,200 ))
			pc.Add(New ParamStruct("@Addr", DbType.String,clsCommon.GetValueDBNull(Me.mvarAddr), ParameterDirection.Input,200 ))
			pc.Add(New ParamStruct("@InvcDate", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarInvcDate.Date), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@InvcNbr", DbType.String,clsCommon.GetValueDBNull(Me.mvarInvcNbr), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@InvcNote", DbType.String,clsCommon.GetValueDBNull(Me.mvarInvcNote), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@TaxRegNbr", DbType.String,clsCommon.GetValueDBNull(Me.mvarTaxRegNbr), ParameterDirection.Input,20 ))
			pc.Add(New ParamStruct("@TaxID", DbType.String,clsCommon.GetValueDBNull(Me.mvarTaxID), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@PayerReceiver", DbType.String,clsCommon.GetValueDBNull(Me.mvarPayerReceiver), ParameterDirection.Input,100 ))
			pc.Add(New ParamStruct("@PayerReceiverAddr", DbType.String,clsCommon.GetValueDBNull(Me.mvarPayerReceiverAddr), ParameterDirection.Input,100 ))
			pc.Add(New ParamStruct("@Crtd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@Crtd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@Crtd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@LUpd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@LUpd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@LUpd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@tstamp", DbType.String,clsCommon.GetValueDBNull(Me.mvartstamp), ParameterDirection.Input,18 ))
			pc.Add(New ParamStruct("@Transportation", DbType.String,clsCommon.GetValueDBNull(Me.mvarTransportation), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@TrsfToBranchID", DbType.String,clsCommon.GetValueDBNull(Me.mvarTrsfToBranchID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@TrsfToBankAcct", DbType.String,clsCommon.GetValueDBNull(Me.mvarTrsfToBankAcct), ParameterDirection.Input,10 ))
			DAL.ExecPreparedSQL(PP_CA_Trans, CommandType.StoredProcedure, pc,"")
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
			 pc.Add(New ParamStruct("@BatNbr",DbType.String, clsCommon.GetValueDBNull(me.mvarBatNbr), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@RefNbr",DbType.String, clsCommon.GetValueDBNull(me.mvarRefNbr), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@LineRef",DbType.String, clsCommon.GetValueDBNull(me.mvarLineRef), ParameterDirection.Input,5 ))
			 pc.Add(New ParamStruct("@BankAcct",DbType.String, clsCommon.GetValueDBNull(me.mvarBankAcct), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@CustID",DbType.String, clsCommon.GetValueDBNull(me.mvarCustID), ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@EmployeeID",DbType.String, clsCommon.GetValueDBNull(me.mvarEmployeeID), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@EntryID",DbType.String, clsCommon.GetValueDBNull(me.mvarEntryID), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@TranType",DbType.String, clsCommon.GetValueDBNull(me.mvarTranType), ParameterDirection.Input,3 ))
			 pc.Add(New ParamStruct("@Rlsed",DbType.int16, clsCommon.GetValueDBNull(me.mvarRlsed), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@TranAmt",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarTranAmt), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@TranDate",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarTranDate.Date), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@TranDesc",DbType.String, clsCommon.GetValueDBNull(me.mvarTranDesc), ParameterDirection.Input,200 ))
			 pc.Add(New ParamStruct("@VendID",DbType.String, clsCommon.GetValueDBNull(me.mvarVendID), ParameterDirection.Input,15 ))
			 pc.Add(New ParamStruct("@VendName",DbType.String, clsCommon.GetValueDBNull(me.mvarVendName), ParameterDirection.Input,200 ))
			 pc.Add(New ParamStruct("@Addr",DbType.String, clsCommon.GetValueDBNull(me.mvarAddr), ParameterDirection.Input,200 ))
			 pc.Add(New ParamStruct("@InvcDate",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarInvcDate.Date), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@InvcNbr",DbType.String, clsCommon.GetValueDBNull(me.mvarInvcNbr), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@InvcNote",DbType.String, clsCommon.GetValueDBNull(me.mvarInvcNote), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@TaxRegNbr",DbType.String, clsCommon.GetValueDBNull(me.mvarTaxRegNbr), ParameterDirection.Input,20 ))
			 pc.Add(New ParamStruct("@TaxID",DbType.String, clsCommon.GetValueDBNull(me.mvarTaxID), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@PayerReceiver",DbType.String, clsCommon.GetValueDBNull(me.mvarPayerReceiver), ParameterDirection.Input,100 ))
			 pc.Add(New ParamStruct("@PayerReceiverAddr",DbType.String, clsCommon.GetValueDBNull(me.mvarPayerReceiverAddr), ParameterDirection.Input,100 ))
			 pc.Add(New ParamStruct("@Crtd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@Crtd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@Crtd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@LUpd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@LUpd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@LUpd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@tstamp",DbType.String, clsCommon.GetValueDBNull(me.mvartstamp), ParameterDirection.Input,18 ))
			 pc.Add(New ParamStruct("@Transportation",DbType.String, clsCommon.GetValueDBNull(me.mvarTransportation), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@TrsfToBranchID",DbType.String, clsCommon.GetValueDBNull(me.mvarTrsfToBranchID), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@TrsfToBankAcct",DbType.String, clsCommon.GetValueDBNull(me.mvarTrsfToBankAcct), ParameterDirection.Input,10 ))
			Return (DAL.ExecNonQuery(PP_CA_Trans, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Delete(ByVal BranchID As System.String, ByVal BatNbr As System.String, ByVal RefNbr As System.String, ByVal LineRef As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID",DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@BatNbr",DbType.String, clsCommon.GetValueDBNull(BatNbr), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@RefNbr",DbType.String, clsCommon.GetValueDBNull(RefNbr), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@LineRef",DbType.String, clsCommon.GetValueDBNull(LineRef), ParameterDirection.Input,5 ))
			Return (DAL.ExecNonQuery(PP_CA_Trans, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function GetAll(ByVal BranchID As System.String, ByVal BatNbr As System.String, ByVal RefNbr As System.String, ByVal LineRef As System.String) as DataTable 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			Dim ds As New DataSet
			pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 30 ))
			pc.Add(New ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(BatNbr), ParameterDirection.Input, 10 ))
			pc.Add(New ParamStruct("@RefNbr", DbType.String, clsCommon.GetValueDBNull(RefNbr), ParameterDirection.Input, 10 ))
			pc.Add(New ParamStruct("@LineRef", DbType.String, clsCommon.GetValueDBNull(LineRef), ParameterDirection.Input, 5 ))
			ds = DAL.ExecDataSet(PP_CA_Trans, CommandType.StoredProcedure, pc,"")
			Dim keys(3) As DataColumn
			Dim column As DataColumn
			column = ds.Tables(0).Columns("BranchID")
			Keys(0) = column
			column = ds.Tables(0).Columns("BatNbr")
			Keys(1) = column
			column = ds.Tables(0).Columns("RefNbr")
			Keys(2) = column
			column = ds.Tables(0).Columns("LineRef")
			Keys(3) = column
			ds.Tables(0).PrimaryKey = Keys
			Return ds.Tables(0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Sub Reset()
		mvarBranchID = String.Empty
		mvarBatNbr = String.Empty
		mvarRefNbr = String.Empty
		mvarLineRef = String.Empty
		mvarBankAcct = String.Empty
		mvarCustID = String.Empty
		mvarEmployeeID = String.Empty
		mvarEntryID = String.Empty
		mvarTranType = String.Empty
		mvarRlsed = 0
		mvarTranAmt = 0
		mvarTranDate = Today
		mvarTranDesc = String.Empty
		mvarVendID = String.Empty
		mvarVendName = String.Empty
		mvarAddr = String.Empty
		mvarInvcDate = Today
		mvarInvcNbr = String.Empty
		mvarInvcNote = String.Empty
		mvarTaxRegNbr = String.Empty
		mvarTaxID = String.Empty
		mvarPayerReceiver = String.Empty
		mvarPayerReceiverAddr = String.Empty
		mvarCrtd_DateTime = Today
		mvarCrtd_Prog = String.Empty
		mvarCrtd_User = String.Empty
		mvarLUpd_DateTime = Today
		mvarLUpd_Prog = String.Empty
		mvarLUpd_User = String.Empty
		mvartstamp = String.Empty
		mvarTransportation = String.Empty
		mvarTrsfToBranchID = String.Empty
		mvarTrsfToBankAcct = String.Empty
	End Sub
	Public Function GetByKey(ByVal BranchID As System.String, ByVal BatNbr As System.String, ByVal RefNbr As System.String, ByVal LineRef As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Dim ds As New DataSet 
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.InputOutput, 30 ))
			pc.Add(New ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(BatNbr), ParameterDirection.InputOutput, 10 ))
			pc.Add(New ParamStruct("@RefNbr", DbType.String, clsCommon.GetValueDBNull(RefNbr), ParameterDirection.InputOutput, 10 ))
			pc.Add(New ParamStruct("@LineRef", DbType.String, clsCommon.GetValueDBNull(LineRef), ParameterDirection.InputOutput, 5 ))
			ds = DAL.ExecDataSet(PP_CA_Trans, CommandType.StoredProcedure, pc,"")
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
		mvarBatNbr =  clsCommon.GetValue(row("BatNbr"), mvarBatNbr.GetType().FullName)
		mvarRefNbr =  clsCommon.GetValue(row("RefNbr"), mvarRefNbr.GetType().FullName)
		mvarLineRef =  clsCommon.GetValue(row("LineRef"), mvarLineRef.GetType().FullName)
		mvarBankAcct =  clsCommon.GetValue(row("BankAcct"), mvarBankAcct.GetType().FullName)
		mvarCustID =  clsCommon.GetValue(row("CustID"), mvarCustID.GetType().FullName)
		mvarEmployeeID =  clsCommon.GetValue(row("EmployeeID"), mvarEmployeeID.GetType().FullName)
		mvarEntryID =  clsCommon.GetValue(row("EntryID"), mvarEntryID.GetType().FullName)
		mvarTranType =  clsCommon.GetValue(row("TranType"), mvarTranType.GetType().FullName)
		mvarRlsed =  clsCommon.GetValue(row("Rlsed"), mvarRlsed.GetType().FullName)
		mvarTranAmt =  clsCommon.GetValue(row("TranAmt"), mvarTranAmt.GetType().FullName)
		mvarTranDate =  clsCommon.GetValue(row("TranDate"), mvarTranDate.GetType().FullName)
		mvarTranDesc =  clsCommon.GetValue(row("TranDesc"), mvarTranDesc.GetType().FullName)
		mvarVendID =  clsCommon.GetValue(row("VendID"), mvarVendID.GetType().FullName)
		mvarVendName =  clsCommon.GetValue(row("VendName"), mvarVendName.GetType().FullName)
		mvarAddr =  clsCommon.GetValue(row("Addr"), mvarAddr.GetType().FullName)
		mvarInvcDate =  clsCommon.GetValue(row("InvcDate"), mvarInvcDate.GetType().FullName)
		mvarInvcNbr =  clsCommon.GetValue(row("InvcNbr"), mvarInvcNbr.GetType().FullName)
		mvarInvcNote =  clsCommon.GetValue(row("InvcNote"), mvarInvcNote.GetType().FullName)
		mvarTaxRegNbr =  clsCommon.GetValue(row("TaxRegNbr"), mvarTaxRegNbr.GetType().FullName)
		mvarTaxID =  clsCommon.GetValue(row("TaxID"), mvarTaxID.GetType().FullName)
		mvarPayerReceiver =  clsCommon.GetValue(row("PayerReceiver"), mvarPayerReceiver.GetType().FullName)
		mvarPayerReceiverAddr =  clsCommon.GetValue(row("PayerReceiverAddr"), mvarPayerReceiverAddr.GetType().FullName)
		mvarCrtd_DateTime =  clsCommon.GetValue(row("Crtd_DateTime"), mvarCrtd_DateTime.GetType().FullName)
		mvarCrtd_Prog =  clsCommon.GetValue(row("Crtd_Prog"), mvarCrtd_Prog.GetType().FullName)
		mvarCrtd_User =  clsCommon.GetValue(row("Crtd_User"), mvarCrtd_User.GetType().FullName)
		mvarLUpd_DateTime =  clsCommon.GetValue(row("LUpd_DateTime"), mvarLUpd_DateTime.GetType().FullName)
		mvarLUpd_Prog =  clsCommon.GetValue(row("LUpd_Prog"), mvarLUpd_Prog.GetType().FullName)
		mvarLUpd_User =  clsCommon.GetValue(row("LUpd_User"), mvarLUpd_User.GetType().FullName)
		mvartstamp =  clsCommon.GetValue(row("tstamp"), mvartstamp.GetType().FullName)
		mvarTransportation =  clsCommon.GetValue(row("Transportation"), mvarTransportation.GetType().FullName)
		mvarTrsfToBranchID =  clsCommon.GetValue(row("TrsfToBranchID"), mvarTrsfToBranchID.GetType().FullName)
		mvarTrsfToBankAcct =  clsCommon.GetValue(row("TrsfToBankAcct"), mvarTrsfToBankAcct.GetType().FullName)
	End Sub
#End Region 

#Region "Private Methods"
#End Region 

End Class
