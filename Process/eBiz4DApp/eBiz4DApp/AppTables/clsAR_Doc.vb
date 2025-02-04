'-- ------------------------------------------------------------
'-- Class name    :  clsAR_Doc
'-- Created date  :  2016-09-14
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework
Imports HQFramework.DAL
Imports HQFramework.Common
Public Class clsAR_Doc
#Region "Constants"
	Private Const PP_AR_Doc As String = "PP_AR_Doc"
#End Region 

#Region "Member Variables"
	Private mvarBranchID As System.String

	Private mvarBatNbr As System.String

	Private mvarRefNbr As System.String

	Private mvarCustId As System.String

	Private mvarDiscBal As System.Double

	Private mvarDiscDate As System.DateTime

	Private mvarDocBal As System.Double

	Private mvarDocDate As System.DateTime

	Private mvarDocDesc As System.String

	Private mvarDocType As System.String

	Private mvarDueDate As System.DateTime

	Private mvarInvcNbr As System.String

	Private mvarInvcNote As System.String

	Private mvarNoteId As System.Int32

	Private mvarOrdNbr As System.String

	Private mvarOrigDocAmt As System.Double

	Private mvarRlsed As System.Int16

	Private mvarSlsperId As System.String

	Private mvarTaxId00 As System.String

	Private mvarTaxId01 As System.String

	Private mvarTaxId02 As System.String

	Private mvarTaxId03 As System.String

	Private mvarTaxTot00 As System.Double

	Private mvarTaxTot01 As System.Double

	Private mvarTaxTot02 As System.Double

	Private mvarTaxTot03 As System.Double

	Private mvarTerms As System.String

	Private mvarTxblTot00 As System.Double

	Private mvarTxblTot01 As System.Double

	Private mvarTxblTot02 As System.Double

	Private mvarTxblTot03 As System.Double

	Private mvarLUpd_DateTime As System.DateTime

	Private mvarLUpd_Prog As System.String

	Private mvarLUpd_User As System.String

	Private mvarCrtd_DateTime As System.DateTime

	Private mvarCrtd_Prog As System.String

	Private mvarCrtd_User As System.String

	Private mvartstamp As System.String

	Private mvarDeliveryID As System.String

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

	Public Property CustId() As System.String
		Get
			Return mvarCustId
		End Get
		Set(ByVal Value As System.String)
			mvarCustId = Value
		End Set
	End Property

	Public Property DiscBal() As System.Double
		Get
			Return mvarDiscBal
		End Get
		Set(ByVal Value As System.Double)
			mvarDiscBal = Value
		End Set
	End Property

	Public Property DiscDate() As System.DateTime
		Get
			Return mvarDiscDate
		End Get
		Set(ByVal Value As System.DateTime)
			mvarDiscDate = Value
		End Set
	End Property

	Public Property DocBal() As System.Double
		Get
			Return mvarDocBal
		End Get
		Set(ByVal Value As System.Double)
			mvarDocBal = Value
		End Set
	End Property

	Public Property DocDate() As System.DateTime
		Get
			Return mvarDocDate
		End Get
		Set(ByVal Value As System.DateTime)
			mvarDocDate = Value
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

	Public Property DocType() As System.String
		Get
			Return mvarDocType
		End Get
		Set(ByVal Value As System.String)
			mvarDocType = Value
		End Set
	End Property

	Public Property DueDate() As System.DateTime
		Get
			Return mvarDueDate
		End Get
		Set(ByVal Value As System.DateTime)
			mvarDueDate = Value
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

	Public Property NoteId() As System.Int32
		Get
			Return mvarNoteId
		End Get
		Set(ByVal Value As System.Int32)
			mvarNoteId = Value
		End Set
	End Property

	Public Property OrdNbr() As System.String
		Get
			Return mvarOrdNbr
		End Get
		Set(ByVal Value As System.String)
			mvarOrdNbr = Value
		End Set
	End Property

	Public Property OrigDocAmt() As System.Double
		Get
			Return mvarOrigDocAmt
		End Get
		Set(ByVal Value As System.Double)
			mvarOrigDocAmt = Value
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

	Public Property SlsperId() As System.String
		Get
			Return mvarSlsperId
		End Get
		Set(ByVal Value As System.String)
			mvarSlsperId = Value
		End Set
	End Property

	Public Property TaxId00() As System.String
		Get
			Return mvarTaxId00
		End Get
		Set(ByVal Value As System.String)
			mvarTaxId00 = Value
		End Set
	End Property

	Public Property TaxId01() As System.String
		Get
			Return mvarTaxId01
		End Get
		Set(ByVal Value As System.String)
			mvarTaxId01 = Value
		End Set
	End Property

	Public Property TaxId02() As System.String
		Get
			Return mvarTaxId02
		End Get
		Set(ByVal Value As System.String)
			mvarTaxId02 = Value
		End Set
	End Property

	Public Property TaxId03() As System.String
		Get
			Return mvarTaxId03
		End Get
		Set(ByVal Value As System.String)
			mvarTaxId03 = Value
		End Set
	End Property

	Public Property TaxTot00() As System.Double
		Get
			Return mvarTaxTot00
		End Get
		Set(ByVal Value As System.Double)
			mvarTaxTot00 = Value
		End Set
	End Property

	Public Property TaxTot01() As System.Double
		Get
			Return mvarTaxTot01
		End Get
		Set(ByVal Value As System.Double)
			mvarTaxTot01 = Value
		End Set
	End Property

	Public Property TaxTot02() As System.Double
		Get
			Return mvarTaxTot02
		End Get
		Set(ByVal Value As System.Double)
			mvarTaxTot02 = Value
		End Set
	End Property

	Public Property TaxTot03() As System.Double
		Get
			Return mvarTaxTot03
		End Get
		Set(ByVal Value As System.Double)
			mvarTaxTot03 = Value
		End Set
	End Property

	Public Property Terms() As System.String
		Get
			Return mvarTerms
		End Get
		Set(ByVal Value As System.String)
			mvarTerms = Value
		End Set
	End Property

	Public Property TxblTot00() As System.Double
		Get
			Return mvarTxblTot00
		End Get
		Set(ByVal Value As System.Double)
			mvarTxblTot00 = Value
		End Set
	End Property

	Public Property TxblTot01() As System.Double
		Get
			Return mvarTxblTot01
		End Get
		Set(ByVal Value As System.Double)
			mvarTxblTot01 = Value
		End Set
	End Property

	Public Property TxblTot02() As System.Double
		Get
			Return mvarTxblTot02
		End Get
		Set(ByVal Value As System.Double)
			mvarTxblTot02 = Value
		End Set
	End Property

	Public Property TxblTot03() As System.Double
		Get
			Return mvarTxblTot03
		End Get
		Set(ByVal Value As System.Double)
			mvarTxblTot03 = Value
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

	Public Property DeliveryID() As System.String
		Get
			Return mvarDeliveryID
		End Get
		Set(ByVal Value As System.String)
			mvarDeliveryID = Value
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
			pc.Add(New ParamStruct("@CustId", DbType.String,clsCommon.GetValueDBNull(Me.mvarCustId), ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@DiscBal", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarDiscBal), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@DiscDate", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarDiscDate.Date), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@DocBal", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarDocBal), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@DocDate", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarDocDate.Date), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@DocDesc", DbType.String,clsCommon.GetValueDBNull(Me.mvarDocDesc), ParameterDirection.Input,100 ))
			pc.Add(New ParamStruct("@DocType", DbType.String,clsCommon.GetValueDBNull(Me.mvarDocType), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@DueDate", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarDueDate.Date), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@InvcNbr", DbType.String,clsCommon.GetValueDBNull(Me.mvarInvcNbr), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@InvcNote", DbType.String,clsCommon.GetValueDBNull(Me.mvarInvcNote), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@NoteId", DbType.int32,clsCommon.GetValueDBNull(Me.mvarNoteId), ParameterDirection.Input,4 ))
			pc.Add(New ParamStruct("@OrdNbr", DbType.String,clsCommon.GetValueDBNull(Me.mvarOrdNbr), ParameterDirection.Input,15 ))
			pc.Add(New ParamStruct("@OrigDocAmt", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarOrigDocAmt), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@Rlsed", DbType.int16,clsCommon.GetValueDBNull(Me.mvarRlsed), ParameterDirection.Input,2 ))
			pc.Add(New ParamStruct("@SlsperId", DbType.String,clsCommon.GetValueDBNull(Me.mvarSlsperId), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@TaxId00", DbType.String,clsCommon.GetValueDBNull(Me.mvarTaxId00), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@TaxId01", DbType.String,clsCommon.GetValueDBNull(Me.mvarTaxId01), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@TaxId02", DbType.String,clsCommon.GetValueDBNull(Me.mvarTaxId02), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@TaxId03", DbType.String,clsCommon.GetValueDBNull(Me.mvarTaxId03), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@TaxTot00", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarTaxTot00), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@TaxTot01", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarTaxTot01), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@TaxTot02", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarTaxTot02), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@TaxTot03", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarTaxTot03), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@Terms", DbType.String,clsCommon.GetValueDBNull(Me.mvarTerms), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@TxblTot00", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarTxblTot00), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@TxblTot01", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarTxblTot01), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@TxblTot02", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarTxblTot02), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@TxblTot03", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarTxblTot03), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@LUpd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@LUpd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@LUpd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@Crtd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@Crtd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@Crtd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@tstamp", DbType.String,clsCommon.GetValueDBNull(Me.mvartstamp), ParameterDirection.Input,18 ))
			pc.Add(New ParamStruct("@DeliveryID", DbType.String,clsCommon.GetValueDBNull(Me.mvarDeliveryID), ParameterDirection.Input,30 ))
			DAL.ExecPreparedSQL(PP_AR_Doc, CommandType.StoredProcedure, pc,"")
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
			 pc.Add(New ParamStruct("@CustId",DbType.String, clsCommon.GetValueDBNull(me.mvarCustId), ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@DiscBal",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarDiscBal), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@DiscDate",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarDiscDate.Date), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@DocBal",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarDocBal), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@DocDate",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarDocDate.Date), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@DocDesc",DbType.String, clsCommon.GetValueDBNull(me.mvarDocDesc), ParameterDirection.Input,100 ))
			 pc.Add(New ParamStruct("@DocType",DbType.String, clsCommon.GetValueDBNull(me.mvarDocType), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@DueDate",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarDueDate.Date), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@InvcNbr",DbType.String, clsCommon.GetValueDBNull(me.mvarInvcNbr), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@InvcNote",DbType.String, clsCommon.GetValueDBNull(me.mvarInvcNote), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@NoteId",DbType.int32, clsCommon.GetValueDBNull(me.mvarNoteId), ParameterDirection.Input,4 ))
			 pc.Add(New ParamStruct("@OrdNbr",DbType.String, clsCommon.GetValueDBNull(me.mvarOrdNbr), ParameterDirection.Input,15 ))
			 pc.Add(New ParamStruct("@OrigDocAmt",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarOrigDocAmt), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@Rlsed",DbType.int16, clsCommon.GetValueDBNull(me.mvarRlsed), ParameterDirection.Input,2 ))
			 pc.Add(New ParamStruct("@SlsperId",DbType.String, clsCommon.GetValueDBNull(me.mvarSlsperId), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@TaxId00",DbType.String, clsCommon.GetValueDBNull(me.mvarTaxId00), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@TaxId01",DbType.String, clsCommon.GetValueDBNull(me.mvarTaxId01), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@TaxId02",DbType.String, clsCommon.GetValueDBNull(me.mvarTaxId02), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@TaxId03",DbType.String, clsCommon.GetValueDBNull(me.mvarTaxId03), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@TaxTot00",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarTaxTot00), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@TaxTot01",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarTaxTot01), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@TaxTot02",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarTaxTot02), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@TaxTot03",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarTaxTot03), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@Terms",DbType.String, clsCommon.GetValueDBNull(me.mvarTerms), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@TxblTot00",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarTxblTot00), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@TxblTot01",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarTxblTot01), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@TxblTot02",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarTxblTot02), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@TxblTot03",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarTxblTot03), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@LUpd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@LUpd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@LUpd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@Crtd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@Crtd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@Crtd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@tstamp",DbType.String, clsCommon.GetValueDBNull(me.mvartstamp), ParameterDirection.Input,18 ))
			 pc.Add(New ParamStruct("@DeliveryID",DbType.String, clsCommon.GetValueDBNull(me.mvarDeliveryID), ParameterDirection.Input,30 ))
			Return (DAL.ExecNonQuery(PP_AR_Doc, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Delete(ByVal BranchID As System.String, ByVal BatNbr As System.String, ByVal RefNbr As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID",DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@BatNbr",DbType.String, clsCommon.GetValueDBNull(BatNbr), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@RefNbr",DbType.String, clsCommon.GetValueDBNull(RefNbr), ParameterDirection.Input,10 ))
			Return (DAL.ExecNonQuery(PP_AR_Doc, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function GetAll(ByVal BranchID As System.String, ByVal BatNbr As System.String, ByVal RefNbr As System.String) as DataTable 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			Dim ds As New DataSet
			pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 30 ))
			pc.Add(New ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(BatNbr), ParameterDirection.Input, 10 ))
			pc.Add(New ParamStruct("@RefNbr", DbType.String, clsCommon.GetValueDBNull(RefNbr), ParameterDirection.Input, 10 ))
			ds = DAL.ExecDataSet(PP_AR_Doc, CommandType.StoredProcedure, pc,"")
			Dim keys(2) As DataColumn
			Dim column As DataColumn
			column = ds.Tables(0).Columns("BranchID")
			Keys(0) = column
			column = ds.Tables(0).Columns("BatNbr")
			Keys(1) = column
			column = ds.Tables(0).Columns("RefNbr")
			Keys(2) = column
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
		mvarCustId = String.Empty
		mvarDiscBal = 0
		mvarDiscDate = Today
		mvarDocBal = 0
		mvarDocDate = Today
		mvarDocDesc = String.Empty
		mvarDocType = String.Empty
		mvarDueDate = Today
		mvarInvcNbr = String.Empty
		mvarInvcNote = String.Empty
		mvarNoteId = 0
		mvarOrdNbr = String.Empty
		mvarOrigDocAmt = 0
		mvarRlsed = 0
		mvarSlsperId = String.Empty
		mvarTaxId00 = String.Empty
		mvarTaxId01 = String.Empty
		mvarTaxId02 = String.Empty
		mvarTaxId03 = String.Empty
		mvarTaxTot00 = 0
		mvarTaxTot01 = 0
		mvarTaxTot02 = 0
		mvarTaxTot03 = 0
		mvarTerms = String.Empty
		mvarTxblTot00 = 0
		mvarTxblTot01 = 0
		mvarTxblTot02 = 0
		mvarTxblTot03 = 0
		mvarLUpd_DateTime = Today
		mvarLUpd_Prog = String.Empty
		mvarLUpd_User = String.Empty
		mvarCrtd_DateTime = Today
		mvarCrtd_Prog = String.Empty
		mvarCrtd_User = String.Empty
		mvartstamp = String.Empty
		mvarDeliveryID = String.Empty
	End Sub
	Public Function GetByKey(ByVal BranchID As System.String, ByVal BatNbr As System.String, ByVal RefNbr As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Dim ds As New DataSet 
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.InputOutput, 30 ))
			pc.Add(New ParamStruct("@BatNbr", DbType.String, clsCommon.GetValueDBNull(BatNbr), ParameterDirection.InputOutput, 10 ))
			pc.Add(New ParamStruct("@RefNbr", DbType.String, clsCommon.GetValueDBNull(RefNbr), ParameterDirection.InputOutput, 10 ))
			ds = DAL.ExecDataSet(PP_AR_Doc, CommandType.StoredProcedure, pc,"")
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
		mvarCustId =  clsCommon.GetValue(row("CustId"), mvarCustId.GetType().FullName)
		mvarDiscBal =  clsCommon.GetValue(row("DiscBal"), mvarDiscBal.GetType().FullName)
		mvarDiscDate =  clsCommon.GetValue(row("DiscDate"), mvarDiscDate.GetType().FullName)
		mvarDocBal =  clsCommon.GetValue(row("DocBal"), mvarDocBal.GetType().FullName)
		mvarDocDate =  clsCommon.GetValue(row("DocDate"), mvarDocDate.GetType().FullName)
		mvarDocDesc =  clsCommon.GetValue(row("DocDesc"), mvarDocDesc.GetType().FullName)
		mvarDocType =  clsCommon.GetValue(row("DocType"), mvarDocType.GetType().FullName)
		mvarDueDate =  clsCommon.GetValue(row("DueDate"), mvarDueDate.GetType().FullName)
		mvarInvcNbr =  clsCommon.GetValue(row("InvcNbr"), mvarInvcNbr.GetType().FullName)
		mvarInvcNote =  clsCommon.GetValue(row("InvcNote"), mvarInvcNote.GetType().FullName)
		mvarNoteId =  clsCommon.GetValue(row("NoteId"), mvarNoteId.GetType().FullName)
		mvarOrdNbr =  clsCommon.GetValue(row("OrdNbr"), mvarOrdNbr.GetType().FullName)
		mvarOrigDocAmt =  clsCommon.GetValue(row("OrigDocAmt"), mvarOrigDocAmt.GetType().FullName)
		mvarRlsed =  clsCommon.GetValue(row("Rlsed"), mvarRlsed.GetType().FullName)
		mvarSlsperId =  clsCommon.GetValue(row("SlsperId"), mvarSlsperId.GetType().FullName)
		mvarTaxId00 =  clsCommon.GetValue(row("TaxId00"), mvarTaxId00.GetType().FullName)
		mvarTaxId01 =  clsCommon.GetValue(row("TaxId01"), mvarTaxId01.GetType().FullName)
		mvarTaxId02 =  clsCommon.GetValue(row("TaxId02"), mvarTaxId02.GetType().FullName)
		mvarTaxId03 =  clsCommon.GetValue(row("TaxId03"), mvarTaxId03.GetType().FullName)
		mvarTaxTot00 =  clsCommon.GetValue(row("TaxTot00"), mvarTaxTot00.GetType().FullName)
		mvarTaxTot01 =  clsCommon.GetValue(row("TaxTot01"), mvarTaxTot01.GetType().FullName)
		mvarTaxTot02 =  clsCommon.GetValue(row("TaxTot02"), mvarTaxTot02.GetType().FullName)
		mvarTaxTot03 =  clsCommon.GetValue(row("TaxTot03"), mvarTaxTot03.GetType().FullName)
		mvarTerms =  clsCommon.GetValue(row("Terms"), mvarTerms.GetType().FullName)
		mvarTxblTot00 =  clsCommon.GetValue(row("TxblTot00"), mvarTxblTot00.GetType().FullName)
		mvarTxblTot01 =  clsCommon.GetValue(row("TxblTot01"), mvarTxblTot01.GetType().FullName)
		mvarTxblTot02 =  clsCommon.GetValue(row("TxblTot02"), mvarTxblTot02.GetType().FullName)
		mvarTxblTot03 =  clsCommon.GetValue(row("TxblTot03"), mvarTxblTot03.GetType().FullName)
		mvarLUpd_DateTime =  clsCommon.GetValue(row("LUpd_DateTime"), mvarLUpd_DateTime.GetType().FullName)
		mvarLUpd_Prog =  clsCommon.GetValue(row("LUpd_Prog"), mvarLUpd_Prog.GetType().FullName)
		mvarLUpd_User =  clsCommon.GetValue(row("LUpd_User"), mvarLUpd_User.GetType().FullName)
		mvarCrtd_DateTime =  clsCommon.GetValue(row("Crtd_DateTime"), mvarCrtd_DateTime.GetType().FullName)
		mvarCrtd_Prog =  clsCommon.GetValue(row("Crtd_Prog"), mvarCrtd_Prog.GetType().FullName)
		mvarCrtd_User =  clsCommon.GetValue(row("Crtd_User"), mvarCrtd_User.GetType().FullName)
		mvartstamp =  clsCommon.GetValue(row("tstamp"), mvartstamp.GetType().FullName)
		mvarDeliveryID =  clsCommon.GetValue(row("DeliveryID"), mvarDeliveryID.GetType().FullName)
	End Sub
#End Region 

#Region "Private Methods"
#End Region 

End Class
