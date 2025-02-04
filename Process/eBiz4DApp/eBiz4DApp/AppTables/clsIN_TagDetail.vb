'-- ------------------------------------------------------------
'-- Class name    :  clsIN_TagDetail
'-- Created date  :  2016-10-24
'-- Created by    :  
'-- Updated by    :  
'-- Generated by    :  Class Maker v1.0
'-- ------------------------------------------------------------

'-- Import Libraries --
Imports HQFramework
Imports HQFramework.DAL
Imports HQFramework.Common
Public Class clsIN_TagDetail
#Region "Constants"
	Private Const PP_IN_TagDetail As String = "PP_IN_TagDetail"
#End Region 

#Region "Member Variables"
	Private mvarTAGID As System.String

	Private mvarInvtID As System.String

	Private mvarInvtName As System.String

	Private mvarCaseUnit As System.String

	Private mvarEAUnit As System.String

	Private mvarCaseCnvFact As System.Double

	Private mvarBookCaseQty As System.Double

	Private mvarBookEAQty As System.Double

	Private mvarActualCaseQty As System.Double

	Private mvarActualEAQty As System.Double

	Private mvarOffetCaseQty As System.Double

	Private mvarOffsetEAQty As System.Double

	Private mvarStkQtyUnder1Month As System.Double

	Private mvarReasonCD As System.String

	Private mvarNotes As System.String

	Private mvarCrtd_DateTime As System.DateTime

	Private mvarCrtd_Prog As System.String

	Private mvarCrtd_User As System.String

	Private mvarLUpd_DateTime As System.DateTime

	Private mvarLUpd_Prog As System.String

	Private mvarLUpd_User As System.String

	Private mvartstamp As System.String

	Private mvarBranchID As System.String

	Private mvarSiteID As System.String

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
	Public Property TAGID() As System.String
		Get
			Return mvarTAGID
		End Get
		Set(ByVal Value As System.String)
			mvarTAGID = Value
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

	Public Property InvtName() As System.String
		Get
			Return mvarInvtName
		End Get
		Set(ByVal Value As System.String)
			mvarInvtName = Value
		End Set
	End Property

	Public Property CaseUnit() As System.String
		Get
			Return mvarCaseUnit
		End Get
		Set(ByVal Value As System.String)
			mvarCaseUnit = Value
		End Set
	End Property

	Public Property EAUnit() As System.String
		Get
			Return mvarEAUnit
		End Get
		Set(ByVal Value As System.String)
			mvarEAUnit = Value
		End Set
	End Property

	Public Property CaseCnvFact() As System.Double
		Get
			Return mvarCaseCnvFact
		End Get
		Set(ByVal Value As System.Double)
			mvarCaseCnvFact = Value
		End Set
	End Property

	Public Property BookCaseQty() As System.Double
		Get
			Return mvarBookCaseQty
		End Get
		Set(ByVal Value As System.Double)
			mvarBookCaseQty = Value
		End Set
	End Property

	Public Property BookEAQty() As System.Double
		Get
			Return mvarBookEAQty
		End Get
		Set(ByVal Value As System.Double)
			mvarBookEAQty = Value
		End Set
	End Property

	Public Property ActualCaseQty() As System.Double
		Get
			Return mvarActualCaseQty
		End Get
		Set(ByVal Value As System.Double)
			mvarActualCaseQty = Value
		End Set
	End Property

	Public Property ActualEAQty() As System.Double
		Get
			Return mvarActualEAQty
		End Get
		Set(ByVal Value As System.Double)
			mvarActualEAQty = Value
		End Set
	End Property

	Public Property OffetCaseQty() As System.Double
		Get
			Return mvarOffetCaseQty
		End Get
		Set(ByVal Value As System.Double)
			mvarOffetCaseQty = Value
		End Set
	End Property

	Public Property OffsetEAQty() As System.Double
		Get
			Return mvarOffsetEAQty
		End Get
		Set(ByVal Value As System.Double)
			mvarOffsetEAQty = Value
		End Set
	End Property

	Public Property StkQtyUnder1Month() As System.Double
		Get
			Return mvarStkQtyUnder1Month
		End Get
		Set(ByVal Value As System.Double)
			mvarStkQtyUnder1Month = Value
		End Set
	End Property

	Public Property ReasonCD() As System.String
		Get
			Return mvarReasonCD
		End Get
		Set(ByVal Value As System.String)
			mvarReasonCD = Value
		End Set
	End Property

	Public Property Notes() As System.String
		Get
			Return mvarNotes
		End Get
		Set(ByVal Value As System.String)
			mvarNotes = Value
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

	Public Property BranchID() As System.String
		Get
			Return mvarBranchID
		End Get
		Set(ByVal Value As System.String)
			mvarBranchID = Value
		End Set
	End Property

	Public Property SiteID() As System.String
		Get
			Return mvarSiteID
		End Get
		Set(ByVal Value As System.String)
			mvarSiteID = Value
		End Set
	End Property

#End Region 

#Region "Public Methods"
	Public Function Add() as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "AddNew", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@TAGID", DbType.String,clsCommon.GetValueDBNull(Me.mvarTAGID), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@InvtID", DbType.String,clsCommon.GetValueDBNull(Me.mvarInvtID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@InvtName", DbType.String,clsCommon.GetValueDBNull(Me.mvarInvtName), ParameterDirection.Input,100 ))
			pc.Add(New ParamStruct("@CaseUnit", DbType.String,clsCommon.GetValueDBNull(Me.mvarCaseUnit), ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@EAUnit", DbType.String,clsCommon.GetValueDBNull(Me.mvarEAUnit), ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@CaseCnvFact", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarCaseCnvFact), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@BookCaseQty", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarBookCaseQty), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@BookEAQty", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarBookEAQty), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@ActualCaseQty", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarActualCaseQty), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@ActualEAQty", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarActualEAQty), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@OffetCaseQty", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarOffetCaseQty), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@OffsetEAQty", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarOffsetEAQty), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@StkQtyUnder1Month", DbType.Decimal,clsCommon.GetValueDBNull(Me.mvarStkQtyUnder1Month), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@ReasonCD", DbType.String,clsCommon.GetValueDBNull(Me.mvarReasonCD), ParameterDirection.Input,6 ))
			pc.Add(New ParamStruct("@Notes", DbType.String,clsCommon.GetValueDBNull(Me.mvarNotes), ParameterDirection.Input,500 ))
			pc.Add(New ParamStruct("@Crtd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@Crtd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@Crtd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarCrtd_User), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@LUpd_DateTime", DbType.DateTime,clsCommon.GetValueDBNull(Me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			pc.Add(New ParamStruct("@LUpd_Prog", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			pc.Add(New ParamStruct("@LUpd_User", DbType.String,clsCommon.GetValueDBNull(Me.mvarLUpd_User), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@tstamp", DbType.String,clsCommon.GetValueDBNull(Me.mvartstamp), ParameterDirection.Input,18 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String,clsCommon.GetValueDBNull(Me.mvarBranchID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@SiteID", DbType.String,clsCommon.GetValueDBNull(Me.mvarSiteID), ParameterDirection.Input,30 ))
			DAL.ExecPreparedSQL(PP_IN_TagDetail, CommandType.StoredProcedure, pc,"")
		Me.mvarTAGID = clsCommon.GetValue(pc.Item("@TAGID").Value, mvarTAGID.GetType().FullName)
		Return (Me.mvarTAGID <> String.Empty )
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Update() as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Update", ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@TAGID",DbType.String, clsCommon.GetValueDBNull(me.mvarTAGID), ParameterDirection.Input,10 ))
			 pc.Add(New ParamStruct("@InvtID",DbType.String, clsCommon.GetValueDBNull(me.mvarInvtID), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@InvtName",DbType.String, clsCommon.GetValueDBNull(me.mvarInvtName), ParameterDirection.Input,100 ))
			 pc.Add(New ParamStruct("@CaseUnit",DbType.String, clsCommon.GetValueDBNull(me.mvarCaseUnit), ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@EAUnit",DbType.String, clsCommon.GetValueDBNull(me.mvarEAUnit), ParameterDirection.Input,50 ))
			 pc.Add(New ParamStruct("@CaseCnvFact",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarCaseCnvFact), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@BookCaseQty",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarBookCaseQty), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@BookEAQty",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarBookEAQty), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@ActualCaseQty",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarActualCaseQty), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@ActualEAQty",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarActualEAQty), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@OffetCaseQty",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarOffetCaseQty), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@OffsetEAQty",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarOffsetEAQty), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@StkQtyUnder1Month",DbType.Decimal, clsCommon.GetValueDBNull(me.mvarStkQtyUnder1Month), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@ReasonCD",DbType.String, clsCommon.GetValueDBNull(me.mvarReasonCD), ParameterDirection.Input,6 ))
			 pc.Add(New ParamStruct("@Notes",DbType.String, clsCommon.GetValueDBNull(me.mvarNotes), ParameterDirection.Input,500 ))
			 pc.Add(New ParamStruct("@Crtd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarCrtd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@Crtd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@Crtd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarCrtd_User), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@LUpd_DateTime",DbType.DateTime, clsCommon.GetValueDBNull(me.mvarLUpd_DateTime), ParameterDirection.Input,16 ))
			 pc.Add(New ParamStruct("@LUpd_Prog",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_Prog), ParameterDirection.Input,8 ))
			 pc.Add(New ParamStruct("@LUpd_User",DbType.String, clsCommon.GetValueDBNull(me.mvarLUpd_User), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@tstamp",DbType.String, clsCommon.GetValueDBNull(me.mvartstamp), ParameterDirection.Input,18 ))
			 pc.Add(New ParamStruct("@BranchID",DbType.String, clsCommon.GetValueDBNull(me.mvarBranchID), ParameterDirection.Input,30 ))
			 pc.Add(New ParamStruct("@SiteID",DbType.String, clsCommon.GetValueDBNull(me.mvarSiteID), ParameterDirection.Input,30 ))
			Return (DAL.ExecNonQuery(PP_IN_TagDetail, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function Delete(ByVal TAGID As System.String, ByVal InvtID As System.String, ByVal BranchID As System.String, ByVal SiteID As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "Delete", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@TAGID",DbType.String, clsCommon.GetValueDBNull(TAGID), ParameterDirection.Input,10 ))
			pc.Add(New ParamStruct("@InvtID",DbType.String, clsCommon.GetValueDBNull(InvtID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@BranchID",DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input,30 ))
			pc.Add(New ParamStruct("@SiteID",DbType.String, clsCommon.GetValueDBNull(SiteID), ParameterDirection.Input,30 ))
			Return (DAL.ExecNonQuery(PP_IN_TagDetail, CommandType.StoredProcedure, pc,"") > 0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Function GetAll(ByVal TAGID As System.String, ByVal InvtID As System.String, ByVal BranchID As System.String, ByVal SiteID As System.String) as DataTable 
		Dim DAL As DataAccess = m_Dal
		Try
			Dim pc As New ParamCollection
			Dim ds As New DataSet
			pc.Add(New ParamStruct("@Action", DbType.String, "GetListData", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@TAGID", DbType.String, clsCommon.GetValueDBNull(TAGID), ParameterDirection.Input, 10 ))
			pc.Add(New ParamStruct("@InvtID", DbType.String, clsCommon.GetValueDBNull(InvtID), ParameterDirection.Input, 30 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.Input, 30 ))
			pc.Add(New ParamStruct("@SiteID", DbType.String, clsCommon.GetValueDBNull(SiteID), ParameterDirection.Input, 30 ))
			ds = DAL.ExecDataSet(PP_IN_TagDetail, CommandType.StoredProcedure, pc,"")
			Dim keys(3) As DataColumn
			Dim column As DataColumn
			column = ds.Tables(0).Columns("TAGID")
			Keys(0) = column
			column = ds.Tables(0).Columns("InvtID")
			Keys(1) = column
			column = ds.Tables(0).Columns("BranchID")
			Keys(2) = column
			column = ds.Tables(0).Columns("SiteID")
			Keys(3) = column
			ds.Tables(0).PrimaryKey = Keys
			Return ds.Tables(0)
		Catch ex As Exception
			Throw ex 
		End Try
	End Function
	Public Sub Reset()
		mvarTAGID = String.Empty
		mvarInvtID = String.Empty
		mvarInvtName = String.Empty
		mvarCaseUnit = String.Empty
		mvarEAUnit = String.Empty
		mvarCaseCnvFact = 0
		mvarBookCaseQty = 0
		mvarBookEAQty = 0
		mvarActualCaseQty = 0
		mvarActualEAQty = 0
		mvarOffetCaseQty = 0
		mvarOffsetEAQty = 0
		mvarStkQtyUnder1Month = 0
		mvarReasonCD = String.Empty
		mvarNotes = String.Empty
		mvarCrtd_DateTime = Today
		mvarCrtd_Prog = String.Empty
		mvarCrtd_User = String.Empty
		mvarLUpd_DateTime = Today
		mvarLUpd_Prog = String.Empty
		mvarLUpd_User = String.Empty
		mvartstamp = String.Empty
		mvarBranchID = String.Empty
		mvarSiteID = String.Empty
	End Sub
	Public Function GetByKey(ByVal TAGID As System.String, ByVal InvtID As System.String, ByVal BranchID As System.String, ByVal SiteID As System.String) as Boolean 
		Dim DAL As DataAccess = m_Dal
		Dim ds As New DataSet 
		Try
			Dim pc As New ParamCollection
			pc.Add(New ParamStruct("@Action", DbType.String, "GetData_ByKey", ParameterDirection.Input,50 ))
			pc.Add(New ParamStruct("@TAGID", DbType.String, clsCommon.GetValueDBNull(TAGID), ParameterDirection.InputOutput, 10 ))
			pc.Add(New ParamStruct("@InvtID", DbType.String, clsCommon.GetValueDBNull(InvtID), ParameterDirection.InputOutput, 30 ))
			pc.Add(New ParamStruct("@BranchID", DbType.String, clsCommon.GetValueDBNull(BranchID), ParameterDirection.InputOutput, 30 ))
			pc.Add(New ParamStruct("@SiteID", DbType.String, clsCommon.GetValueDBNull(SiteID), ParameterDirection.InputOutput, 30 ))
			ds = DAL.ExecDataSet(PP_IN_TagDetail, CommandType.StoredProcedure, pc,"")
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
		mvarTAGID =  clsCommon.GetValue(row("TAGID"), mvarTAGID.GetType().FullName)
		mvarInvtID =  clsCommon.GetValue(row("InvtID"), mvarInvtID.GetType().FullName)
		mvarInvtName =  clsCommon.GetValue(row("InvtName"), mvarInvtName.GetType().FullName)
		mvarCaseUnit =  clsCommon.GetValue(row("CaseUnit"), mvarCaseUnit.GetType().FullName)
		mvarEAUnit =  clsCommon.GetValue(row("EAUnit"), mvarEAUnit.GetType().FullName)
		mvarCaseCnvFact =  clsCommon.GetValue(row("CaseCnvFact"), mvarCaseCnvFact.GetType().FullName)
		mvarBookCaseQty =  clsCommon.GetValue(row("BookCaseQty"), mvarBookCaseQty.GetType().FullName)
		mvarBookEAQty =  clsCommon.GetValue(row("BookEAQty"), mvarBookEAQty.GetType().FullName)
		mvarActualCaseQty =  clsCommon.GetValue(row("ActualCaseQty"), mvarActualCaseQty.GetType().FullName)
		mvarActualEAQty =  clsCommon.GetValue(row("ActualEAQty"), mvarActualEAQty.GetType().FullName)
		mvarOffetCaseQty =  clsCommon.GetValue(row("OffetCaseQty"), mvarOffetCaseQty.GetType().FullName)
		mvarOffsetEAQty =  clsCommon.GetValue(row("OffsetEAQty"), mvarOffsetEAQty.GetType().FullName)
		mvarStkQtyUnder1Month =  clsCommon.GetValue(row("StkQtyUnder1Month"), mvarStkQtyUnder1Month.GetType().FullName)
		mvarReasonCD =  clsCommon.GetValue(row("ReasonCD"), mvarReasonCD.GetType().FullName)
		mvarNotes =  clsCommon.GetValue(row("Notes"), mvarNotes.GetType().FullName)
		mvarCrtd_DateTime =  clsCommon.GetValue(row("Crtd_DateTime"), mvarCrtd_DateTime.GetType().FullName)
		mvarCrtd_Prog =  clsCommon.GetValue(row("Crtd_Prog"), mvarCrtd_Prog.GetType().FullName)
		mvarCrtd_User =  clsCommon.GetValue(row("Crtd_User"), mvarCrtd_User.GetType().FullName)
		mvarLUpd_DateTime =  clsCommon.GetValue(row("LUpd_DateTime"), mvarLUpd_DateTime.GetType().FullName)
		mvarLUpd_Prog =  clsCommon.GetValue(row("LUpd_Prog"), mvarLUpd_Prog.GetType().FullName)
		mvarLUpd_User =  clsCommon.GetValue(row("LUpd_User"), mvarLUpd_User.GetType().FullName)
		mvartstamp =  clsCommon.GetValue(row("tstamp"), mvartstamp.GetType().FullName)
		mvarBranchID =  clsCommon.GetValue(row("BranchID"), mvarBranchID.GetType().FullName)
		mvarSiteID =  clsCommon.GetValue(row("SiteID"), mvarSiteID.GetType().FullName)
	End Sub
#End Region 

#Region "Private Methods"
#End Region 

End Class
