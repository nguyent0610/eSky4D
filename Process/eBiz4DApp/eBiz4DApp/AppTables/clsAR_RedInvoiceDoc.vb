'-- ------------------------------------------------------------
'-- Class name    :  clsAR_RedInvoiceDoc
'-- Created date  :  5/13/2015
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework.Common
Imports HQFramework.DAL
Public Class clsAR_RedInvoiceDoc
#Region "Constants"
	Private Const PP_AR_RedInvoiceDoc As String = "PP_AR_RedInvoiceDoc"
#End Region 

#Region "Member Variables"
	Private mvarBranchID As System.String

	Private mvarInvcNbr As System.String

	Private mvarInvcNote As System.String

	Private mvarCustID As System.String

	Private mvarCuryTaxAmt As System.Double

	Private mvarCuryTxblAmt As System.Double

	Private mvarDocDate As System.DateTime

	Private mvarDocDesc As System.String

	Private mvarPerPost As System.String

	Private mvarStatus As System.String

	Private mvarTaxID As System.String

	Private mvarCrtd_Datetime As System.DateTime

	Private mvarCrtd_Prog As System.String

	Private mvarCrtd_User As System.String

	Private mvarLUpd_Datetime As System.DateTime

	Private mvarLUpd_Prog As System.String

	Private mvarLUpd_User As System.String

	Private mvartstamp As System.String

	Private mvarSlsPerID As System.String

	Private mvarDiscAmt As System.Double

	Private mvarSOFee As System.Double

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

	Public Property CustID() As System.String
		Get
			Return mvarCustID
		End Get
		Set(ByVal Value As System.String)
			mvarCustID = Value
		End Set
	End Property

	Public Property CuryTaxAmt() As System.Double
		Get
			Return mvarCuryTaxAmt
		End Get
		Set(ByVal Value As System.Double)
			mvarCuryTaxAmt = Value
		End Set
	End Property

	Public Property CuryTxblAmt() As System.Double
		Get
			Return mvarCuryTxblAmt
		End Get
		Set(ByVal Value As System.Double)
			mvarCuryTxblAmt = Value
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

	Public Property PerPost() As System.String
		Get
			Return mvarPerPost
		End Get
		Set(ByVal Value As System.String)
			mvarPerPost = Value
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

	Public Property TaxID() As System.String
		Get
			Return mvarTaxID
		End Get
		Set(ByVal Value As System.String)
			mvarTaxID = Value
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

	Public Property tstamp() As System.String
		Get
			Return mvartstamp
		End Get
		Set(ByVal Value As System.String)
			mvartstamp = Value
		End Set
	End Property

	Public Property SlsPerID() As System.String
		Get
			Return mvarSlsPerID
		End Get
		Set(ByVal Value As System.String)
			mvarSlsPerID = Value
		End Set
	End Property

	Public Property DiscAmt() As System.Double
		Get
			Return mvarDiscAmt
		End Get
		Set(ByVal Value As System.Double)
			mvarDiscAmt = Value
		End Set
	End Property

	Public Property SOFee() As System.Double
		Get
			Return mvarSOFee
		End Get
		Set(ByVal Value As System.Double)
			mvarSOFee = Value
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
			pc.Add(New ParamStruct("@InvcNbr", DbType.String,clsCommon.GetValueDBNull(Me.mvarInvcNbr), ParameterDirection.Input,15 ))
			pc.Add(New ParamStruct("@InvcNote", DbType.String,clsCommon.GetValueDBNull(Me.mvarInvcNote), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@CustID", DbType.String,clsCommon.GetValueDBNull(Me.mvarCustID), ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@CuryTaxAmt", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarCuryTaxAmt), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@CuryTxblAmt", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarCuryTxblAmt), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@DocDate", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarDocDate.Date), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@DocDesc", DbType.String,clsCommon.GetValueDBNull(Me.mvarDocDesc), ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@PerPost", DbType.String,clsCommon.GetValueDBNull(Me.mvarPerPost), ParameterDirection.Input,6 ))
			pc.Add(New ParamStruct("@Status", DbType.String,clsCommon.GetValueDBNull(Me.mvarStatus), ParameterDirection.Input,1 ))
			pc.Add(New ParamStruct("@TaxID", DbType.String,clsCommon.GetValueDBNull(Me.mvarTaxID), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@Crtd_Datetime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarCrtd_Datetime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@Crtd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@Crtd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@LUpd_Datetime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarLUpd_Datetime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@LUpd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@LUpd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_User), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@tstamp", DbType.String,clsCommon.GetValueDBNull(Me.mvartstamp), ParameterDirection.Input,18 ))
			pc.Add(New ParamStruct("@SlsPerID", DbType.String,clsCommon.GetValueDBNull(Me.mvarSlsPerID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@DiscAmt", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarDiscAmt), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@SOFee", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarSOFee), ParameterDirection.Input,8 ))
			DAL.ExecPreparedSQL(PP_AR_RedInvoiceDoc, CommandType.StoredProcedure, pc,"")
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
			 pc.Add(New ParamStruct("@InvcNbr",DbType.String, clsCommon.GetValueDBNull(me.mvarInvcNbr), ParameterDirection.Input,15 ))
			 pc.Add(New ParamStruct("@InvcNote",DbType.String, clsCommon.GetValueDBNull(me.mvarInvcNote), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@CustID",DbType.String, clsCommon.GetValueDBNull(me.mvarCustID), ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@CuryTaxAmt",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarCuryTaxAmt), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@CuryTxblAmt",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarCuryTxblAmt), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@DocDate",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarDocDate.Date), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@DocDesc",DbType.String, clsCommon.GetValueDBNull(me.mvarDocDesc), ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@PerPost",DbType.String, clsCommon.GetValueDBNull(me.mvarPerPost), ParameterDirection.Input,6 ))
			 pc.Add(New ParamStruct("@Status",DbType.String, clsCommon.GetValueDBNull(me.mvarStatus), ParameterDirection.Input,1 ))
			 pc.Add(New ParamStruct("@TaxID",DbType.String, clsCommon.GetValueDBNull(me.mvarTaxID), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@Crtd_Datetime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarCrtd_Datetime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@Crtd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@Crtd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@LUpd_Datetime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarLUpd_Datetime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@LUpd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@LUpd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_User), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@tstamp",DbType.String, clsCommon.GetValueDBNull(me.mvartstamp), ParameterDirection.Input,18 ))
			 pc.Add(New ParamStruct("@SlsPerID",DbType.String, clsCommon.GetValueDBNull(me.mvarSlsPerID), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@DiscAmt",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarDiscAmt), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@SOFee",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarSOFee), ParameterDirection.Input,8 ))
			Return (DAL.ExecNonQuery(PP_AR_RedInvoiceDoc, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Delete(ByVal BranchID As System.String, ByVal InvcNbr As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID",DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@InvcNbr",DbType.String, clsCommon.GetValueDBNull(InvcNbr), ParameterDirection.Input,15 ))
			Return (DAL.ExecNonQuery(PP_AR_RedInvoiceDoc, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function GetAll(ByVal BranchID As System.String, ByVal InvcNbr As System.String) as DataTable 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			Dim ds As New DataSet
			pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 30 ))
			pc.Add(New ParamStruct("@InvcNbr", DbType.String, clsCommon.GetValueDBNull(InvcNbr), ParameterDirection.Input, 15 ))
			ds = DAL.ExecDataSet(PP_AR_RedInvoiceDoc, CommandType.StoredProcedure, pc,"")
			Dim keys(1) As DataColumn
			Dim column As DataColumn
			column = ds.Tables(0).Columns("BranchID")
			Keys(0) = column
			column = ds.Tables(0).Columns("InvcNbr")
			Keys(1) = column
			ds.Tables(0).PrimaryKey = Keys
			Return ds.Tables(0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Sub Reset()
		mvarBranchID = String.Empty
		mvarInvcNbr = String.Empty
		mvarInvcNote = String.Empty
		mvarCustID = String.Empty
		mvarCuryTaxAmt = 0
		mvarCuryTxblAmt = 0
		mvarDocDate = Today
		mvarDocDesc = String.Empty
		mvarPerPost = String.Empty
		mvarStatus = String.Empty
		mvarTaxID = String.Empty
		mvarCrtd_Datetime = Today
		mvarCrtd_Prog = String.Empty
		mvarCrtd_User = String.Empty
		mvarLUpd_Datetime = Today
		mvarLUpd_Prog = String.Empty
		mvarLUpd_User = String.Empty
		mvartstamp = String.Empty
		mvarSlsPerID = String.Empty
		mvarDiscAmt = 0
		mvarSOFee = 0
	End Sub
	Public Function GetByKey(ByVal BranchID As System.String, ByVal InvcNbr As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Dim ds As New DataSet 
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.InputOutput, 30 ))
			pc.Add(New ParamStruct("@InvcNbr", DbType.String, clsCommon.GetValueDBNull(InvcNbr), ParameterDirection.InputOutput, 15 ))
			ds = DAL.ExecDataSet(PP_AR_RedInvoiceDoc, CommandType.StoredProcedure, pc,"")
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
		mvarInvcNbr =  clsCommon.GetValue(row("InvcNbr"), mvarInvcNbr.GetType().FullName)
		mvarInvcNote =  clsCommon.GetValue(row("InvcNote"), mvarInvcNote.GetType().FullName)
		mvarCustID =  clsCommon.GetValue(row("CustID"), mvarCustID.GetType().FullName)
		mvarCuryTaxAmt =  clsCommon.GetValue(row("CuryTaxAmt"), mvarCuryTaxAmt.GetType().FullName)
		mvarCuryTxblAmt =  clsCommon.GetValue(row("CuryTxblAmt"), mvarCuryTxblAmt.GetType().FullName)
		mvarDocDate =  clsCommon.GetValue(row("DocDate"), mvarDocDate.GetType().FullName)
		mvarDocDesc =  clsCommon.GetValue(row("DocDesc"), mvarDocDesc.GetType().FullName)
		mvarPerPost =  clsCommon.GetValue(row("PerPost"), mvarPerPost.GetType().FullName)
		mvarStatus =  clsCommon.GetValue(row("Status"), mvarStatus.GetType().FullName)
		mvarTaxID =  clsCommon.GetValue(row("TaxID"), mvarTaxID.GetType().FullName)
		mvarCrtd_Datetime =  clsCommon.GetValue(row("Crtd_Datetime"), mvarCrtd_Datetime.GetType().FullName)
		mvarCrtd_Prog =  clsCommon.GetValue(row("Crtd_Prog"), mvarCrtd_Prog.GetType().FullName)
		mvarCrtd_User =  clsCommon.GetValue(row("Crtd_User"), mvarCrtd_User.GetType().FullName)
		mvarLUpd_Datetime =  clsCommon.GetValue(row("LUpd_Datetime"), mvarLUpd_Datetime.GetType().FullName)
		mvarLUpd_Prog =  clsCommon.GetValue(row("LUpd_Prog"), mvarLUpd_Prog.GetType().FullName)
		mvarLUpd_User =  clsCommon.GetValue(row("LUpd_User"), mvarLUpd_User.GetType().FullName)
		mvartstamp =  clsCommon.GetValue(row("tstamp"), mvartstamp.GetType().FullName)
		mvarSlsPerID =  clsCommon.GetValue(row("SlsPerID"), mvarSlsPerID.GetType().FullName)
		mvarDiscAmt =  clsCommon.GetValue(row("DiscAmt"), mvarDiscAmt.GetType().FullName)
		mvarSOFee =  clsCommon.GetValue(row("SOFee"), mvarSOFee.GetType().FullName)
	End Sub
#End Region 

#Region "Private Methods"
#End Region 

End Class
