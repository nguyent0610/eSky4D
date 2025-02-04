﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

[assembly: EdmSchemaAttribute()]
namespace IN20700
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class IN20700Entities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new IN20700Entities object using the connection string found in the 'IN20700Entities' section of the application configuration file.
        /// </summary>
        public IN20700Entities() : base("name=IN20700Entities", "IN20700Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new IN20700Entities object.
        /// </summary>
        public IN20700Entities(string connectionString) : base(connectionString, "IN20700Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new IN20700Entities object.
        /// </summary>
        public IN20700Entities(EntityConnection connection) : base(connection, "IN20700Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        #endregion
    
        #region Partial Methods
    
        partial void OnContextCreated();
    
        #endregion
    
        #region ObjectSet Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectSet<IN_ReasonCode> IN_ReasonCode
        {
            get
            {
                if ((_IN_ReasonCode == null))
                {
                    _IN_ReasonCode = base.CreateObjectSet<IN_ReasonCode>("IN_ReasonCode");
                }
                return _IN_ReasonCode;
            }
        }
        private ObjectSet<IN_ReasonCode> _IN_ReasonCode;

        #endregion

        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the IN_ReasonCode EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToIN_ReasonCode(IN_ReasonCode iN_ReasonCode)
        {
            base.AddObject("IN_ReasonCode", iN_ReasonCode);
        }

        #endregion

        #region Function Imports
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name="userID">No Metadata Documentation available.</param>
        /// <param name="cpnyID">No Metadata Documentation available.</param>
        /// <param name="langID">No Metadata Documentation available.</param>
        public ObjectResult<IN20700_pcLoadEmployeeID_Result> IN20700_pcLoadEmployeeID(global::System.String userID, global::System.String cpnyID, Nullable<global::System.Int16> langID)
        {
            ObjectParameter userIDParameter;
            if (userID != null)
            {
                userIDParameter = new ObjectParameter("UserID", userID);
            }
            else
            {
                userIDParameter = new ObjectParameter("UserID", typeof(global::System.String));
            }
    
            ObjectParameter cpnyIDParameter;
            if (cpnyID != null)
            {
                cpnyIDParameter = new ObjectParameter("CpnyID", cpnyID);
            }
            else
            {
                cpnyIDParameter = new ObjectParameter("CpnyID", typeof(global::System.String));
            }
    
            ObjectParameter langIDParameter;
            if (langID.HasValue)
            {
                langIDParameter = new ObjectParameter("LangID", langID);
            }
            else
            {
                langIDParameter = new ObjectParameter("LangID", typeof(global::System.Int16));
            }
    
            return base.ExecuteFunction<IN20700_pcLoadEmployeeID_Result>("IN20700_pcLoadEmployeeID", userIDParameter, cpnyIDParameter, langIDParameter);
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name="userID">No Metadata Documentation available.</param>
        /// <param name="cpnyID">No Metadata Documentation available.</param>
        /// <param name="langID">No Metadata Documentation available.</param>
        public ObjectResult<IN20700_pcLoadSiteID_Result> IN20700_pcLoadSiteID(global::System.String userID, global::System.String cpnyID, Nullable<global::System.Int16> langID)
        {
            ObjectParameter userIDParameter;
            if (userID != null)
            {
                userIDParameter = new ObjectParameter("UserID", userID);
            }
            else
            {
                userIDParameter = new ObjectParameter("UserID", typeof(global::System.String));
            }
    
            ObjectParameter cpnyIDParameter;
            if (cpnyID != null)
            {
                cpnyIDParameter = new ObjectParameter("CpnyID", cpnyID);
            }
            else
            {
                cpnyIDParameter = new ObjectParameter("CpnyID", typeof(global::System.String));
            }
    
            ObjectParameter langIDParameter;
            if (langID.HasValue)
            {
                langIDParameter = new ObjectParameter("LangID", langID);
            }
            else
            {
                langIDParameter = new ObjectParameter("LangID", typeof(global::System.Int16));
            }
    
            return base.ExecuteFunction<IN20700_pcLoadSiteID_Result>("IN20700_pcLoadSiteID", userIDParameter, cpnyIDParameter, langIDParameter);
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name="userID">No Metadata Documentation available.</param>
        /// <param name="cpnyID">No Metadata Documentation available.</param>
        /// <param name="langID">No Metadata Documentation available.</param>
        public ObjectResult<IN20700_pgLoadReasonCD_Result> IN20700_pgLoadReasonCD(global::System.String userID, global::System.String cpnyID, Nullable<global::System.Int16> langID)
        {
            ObjectParameter userIDParameter;
            if (userID != null)
            {
                userIDParameter = new ObjectParameter("UserID", userID);
            }
            else
            {
                userIDParameter = new ObjectParameter("UserID", typeof(global::System.String));
            }
    
            ObjectParameter cpnyIDParameter;
            if (cpnyID != null)
            {
                cpnyIDParameter = new ObjectParameter("CpnyID", cpnyID);
            }
            else
            {
                cpnyIDParameter = new ObjectParameter("CpnyID", typeof(global::System.String));
            }
    
            ObjectParameter langIDParameter;
            if (langID.HasValue)
            {
                langIDParameter = new ObjectParameter("LangID", langID);
            }
            else
            {
                langIDParameter = new ObjectParameter("LangID", typeof(global::System.Int16));
            }
    
            return base.ExecuteFunction<IN20700_pgLoadReasonCD_Result>("IN20700_pgLoadReasonCD", userIDParameter, cpnyIDParameter, langIDParameter);
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name="reasonCD">No Metadata Documentation available.</param>
        public ObjectResult<global::System.String> IN20700_ppCheckForDeleteReasonCD(global::System.String reasonCD)
        {
            ObjectParameter reasonCDParameter;
            if (reasonCD != null)
            {
                reasonCDParameter = new ObjectParameter("ReasonCD", reasonCD);
            }
            else
            {
                reasonCDParameter = new ObjectParameter("ReasonCD", typeof(global::System.String));
            }
    
            return base.ExecuteFunction<global::System.String>("IN20700_ppCheckForDeleteReasonCD", reasonCDParameter);
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name="userName">No Metadata Documentation available.</param>
        /// <param name="cpnyID">No Metadata Documentation available.</param>
        /// <param name="langID">No Metadata Documentation available.</param>
        public ObjectResult<IN20700_pdConfig_Result> IN20700_pdConfig(global::System.String userName, global::System.String cpnyID, Nullable<global::System.Int16> langID)
        {
            ObjectParameter userNameParameter;
            if (userName != null)
            {
                userNameParameter = new ObjectParameter("UserName", userName);
            }
            else
            {
                userNameParameter = new ObjectParameter("UserName", typeof(global::System.String));
            }
    
            ObjectParameter cpnyIDParameter;
            if (cpnyID != null)
            {
                cpnyIDParameter = new ObjectParameter("CpnyID", cpnyID);
            }
            else
            {
                cpnyIDParameter = new ObjectParameter("CpnyID", typeof(global::System.String));
            }
    
            ObjectParameter langIDParameter;
            if (langID.HasValue)
            {
                langIDParameter = new ObjectParameter("LangID", langID);
            }
            else
            {
                langIDParameter = new ObjectParameter("LangID", typeof(global::System.Int16));
            }
    
            return base.ExecuteFunction<IN20700_pdConfig_Result>("IN20700_pdConfig", userNameParameter, cpnyIDParameter, langIDParameter);
        }

        #endregion

    }

    #endregion

    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="IN20700Model", Name="IN_ReasonCode")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class IN_ReasonCode : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new IN_ReasonCode object.
        /// </summary>
        /// <param name="reasonCD">Initial value of the ReasonCD property.</param>
        /// <param name="crtd_DateTime">Initial value of the Crtd_DateTime property.</param>
        /// <param name="crtd_Prog">Initial value of the Crtd_Prog property.</param>
        /// <param name="crtd_User">Initial value of the Crtd_User property.</param>
        /// <param name="lUpd_DateTime">Initial value of the LUpd_DateTime property.</param>
        /// <param name="lUpd_Prog">Initial value of the LUpd_Prog property.</param>
        /// <param name="lUpd_User">Initial value of the LUpd_User property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static IN_ReasonCode CreateIN_ReasonCode(global::System.String reasonCD, global::System.DateTime crtd_DateTime, global::System.String crtd_Prog, global::System.String crtd_User, global::System.DateTime lUpd_DateTime, global::System.String lUpd_Prog, global::System.String lUpd_User, global::System.Byte[] tstamp)
        {
            IN_ReasonCode iN_ReasonCode = new IN_ReasonCode();
            iN_ReasonCode.ReasonCD = reasonCD;
            iN_ReasonCode.Crtd_DateTime = crtd_DateTime;
            iN_ReasonCode.Crtd_Prog = crtd_Prog;
            iN_ReasonCode.Crtd_User = crtd_User;
            iN_ReasonCode.LUpd_DateTime = lUpd_DateTime;
            iN_ReasonCode.LUpd_Prog = lUpd_Prog;
            iN_ReasonCode.LUpd_User = lUpd_User;
            iN_ReasonCode.tstamp = tstamp;
            return iN_ReasonCode;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String ReasonCD
        {
            get
            {
                return _ReasonCD;
            }
            set
            {
                if (_ReasonCD != value)
                {
                    OnReasonCDChanging(value);
                    ReportPropertyChanging("ReasonCD");
                    _ReasonCD = StructuralObject.SetValidValue(value, false);
                    ReportPropertyChanged("ReasonCD");
                    OnReasonCDChanged();
                }
            }
        }
        private global::System.String _ReasonCD;
        partial void OnReasonCDChanging(global::System.String value);
        partial void OnReasonCDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Descr
        {
            get
            {
                return _Descr;
            }
            set
            {
                OnDescrChanging(value);
                ReportPropertyChanging("Descr");
                _Descr = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Descr");
                OnDescrChanged();
            }
        }
        private global::System.String _Descr;
        partial void OnDescrChanging(global::System.String value);
        partial void OnDescrChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String SiteID
        {
            get
            {
                return _SiteID;
            }
            set
            {
                OnSiteIDChanging(value);
                ReportPropertyChanging("SiteID");
                _SiteID = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("SiteID");
                OnSiteIDChanged();
            }
        }
        private global::System.String _SiteID;
        partial void OnSiteIDChanging(global::System.String value);
        partial void OnSiteIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String SlsperID
        {
            get
            {
                return _SlsperID;
            }
            set
            {
                OnSlsperIDChanging(value);
                ReportPropertyChanging("SlsperID");
                _SlsperID = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("SlsperID");
                OnSlsperIDChanged();
            }
        }
        private global::System.String _SlsperID;
        partial void OnSlsperIDChanging(global::System.String value);
        partial void OnSlsperIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime Crtd_DateTime
        {
            get
            {
                return _Crtd_DateTime;
            }
            set
            {
                OnCrtd_DateTimeChanging(value);
                ReportPropertyChanging("Crtd_DateTime");
                _Crtd_DateTime = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("Crtd_DateTime");
                OnCrtd_DateTimeChanged();
            }
        }
        private global::System.DateTime _Crtd_DateTime;
        partial void OnCrtd_DateTimeChanging(global::System.DateTime value);
        partial void OnCrtd_DateTimeChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Crtd_Prog
        {
            get
            {
                return _Crtd_Prog;
            }
            set
            {
                OnCrtd_ProgChanging(value);
                ReportPropertyChanging("Crtd_Prog");
                _Crtd_Prog = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("Crtd_Prog");
                OnCrtd_ProgChanged();
            }
        }
        private global::System.String _Crtd_Prog;
        partial void OnCrtd_ProgChanging(global::System.String value);
        partial void OnCrtd_ProgChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Crtd_User
        {
            get
            {
                return _Crtd_User;
            }
            set
            {
                OnCrtd_UserChanging(value);
                ReportPropertyChanging("Crtd_User");
                _Crtd_User = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("Crtd_User");
                OnCrtd_UserChanged();
            }
        }
        private global::System.String _Crtd_User;
        partial void OnCrtd_UserChanging(global::System.String value);
        partial void OnCrtd_UserChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime LUpd_DateTime
        {
            get
            {
                return _LUpd_DateTime;
            }
            set
            {
                OnLUpd_DateTimeChanging(value);
                ReportPropertyChanging("LUpd_DateTime");
                _LUpd_DateTime = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("LUpd_DateTime");
                OnLUpd_DateTimeChanged();
            }
        }
        private global::System.DateTime _LUpd_DateTime;
        partial void OnLUpd_DateTimeChanging(global::System.DateTime value);
        partial void OnLUpd_DateTimeChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String LUpd_Prog
        {
            get
            {
                return _LUpd_Prog;
            }
            set
            {
                OnLUpd_ProgChanging(value);
                ReportPropertyChanging("LUpd_Prog");
                _LUpd_Prog = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("LUpd_Prog");
                OnLUpd_ProgChanged();
            }
        }
        private global::System.String _LUpd_Prog;
        partial void OnLUpd_ProgChanging(global::System.String value);
        partial void OnLUpd_ProgChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String LUpd_User
        {
            get
            {
                return _LUpd_User;
            }
            set
            {
                OnLUpd_UserChanging(value);
                ReportPropertyChanging("LUpd_User");
                _LUpd_User = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("LUpd_User");
                OnLUpd_UserChanged();
            }
        }
        private global::System.String _LUpd_User;
        partial void OnLUpd_UserChanging(global::System.String value);
        partial void OnLUpd_UserChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Byte[] tstamp
        {
            get
            {
                return StructuralObject.GetValidValue(_tstamp);
            }
            set
            {
                OntstampChanging(value);
                ReportPropertyChanging("tstamp");
                _tstamp = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("tstamp");
                OntstampChanged();
            }
        }
        private global::System.Byte[] _tstamp;
        partial void OntstampChanging(global::System.Byte[] value);
        partial void OntstampChanged();

        #endregion

    
    }

    #endregion

    #region ComplexTypes
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmComplexTypeAttribute(NamespaceName="IN20700Model", Name="IN20700_pcLoadEmployeeID_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class IN20700_pcLoadEmployeeID_Result : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new IN20700_pcLoadEmployeeID_Result object.
        /// </summary>
        /// <param name="slsperid">Initial value of the Slsperid property.</param>
        /// <param name="branchID">Initial value of the BranchID property.</param>
        public static IN20700_pcLoadEmployeeID_Result CreateIN20700_pcLoadEmployeeID_Result(global::System.String slsperid, global::System.String branchID)
        {
            IN20700_pcLoadEmployeeID_Result iN20700_pcLoadEmployeeID_Result = new IN20700_pcLoadEmployeeID_Result();
            iN20700_pcLoadEmployeeID_Result.Slsperid = slsperid;
            iN20700_pcLoadEmployeeID_Result.BranchID = branchID;
            return iN20700_pcLoadEmployeeID_Result;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Slsperid
        {
            get
            {
                return _Slsperid;
            }
            set
            {
                OnSlsperidChanging(value);
                ReportPropertyChanging("Slsperid");
                _Slsperid = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("Slsperid");
                OnSlsperidChanged();
            }
        }
        private global::System.String _Slsperid;
        partial void OnSlsperidChanging(global::System.String value);
        partial void OnSlsperidChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                OnNameChanging(value);
                ReportPropertyChanging("Name");
                _Name = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Name");
                OnNameChanged();
            }
        }
        private global::System.String _Name;
        partial void OnNameChanging(global::System.String value);
        partial void OnNameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String BranchID
        {
            get
            {
                return _BranchID;
            }
            set
            {
                OnBranchIDChanging(value);
                ReportPropertyChanging("BranchID");
                _BranchID = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("BranchID");
                OnBranchIDChanged();
            }
        }
        private global::System.String _BranchID;
        partial void OnBranchIDChanging(global::System.String value);
        partial void OnBranchIDChanged();

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmComplexTypeAttribute(NamespaceName="IN20700Model", Name="IN20700_pcLoadSiteID_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class IN20700_pcLoadSiteID_Result : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new IN20700_pcLoadSiteID_Result object.
        /// </summary>
        /// <param name="siteID">Initial value of the SiteID property.</param>
        /// <param name="cpnyID">Initial value of the CpnyID property.</param>
        public static IN20700_pcLoadSiteID_Result CreateIN20700_pcLoadSiteID_Result(global::System.String siteID, global::System.String cpnyID)
        {
            IN20700_pcLoadSiteID_Result iN20700_pcLoadSiteID_Result = new IN20700_pcLoadSiteID_Result();
            iN20700_pcLoadSiteID_Result.SiteID = siteID;
            iN20700_pcLoadSiteID_Result.CpnyID = cpnyID;
            return iN20700_pcLoadSiteID_Result;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String SiteID
        {
            get
            {
                return _SiteID;
            }
            set
            {
                OnSiteIDChanging(value);
                ReportPropertyChanging("SiteID");
                _SiteID = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("SiteID");
                OnSiteIDChanged();
            }
        }
        private global::System.String _SiteID;
        partial void OnSiteIDChanging(global::System.String value);
        partial void OnSiteIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                OnNameChanging(value);
                ReportPropertyChanging("Name");
                _Name = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Name");
                OnNameChanged();
            }
        }
        private global::System.String _Name;
        partial void OnNameChanging(global::System.String value);
        partial void OnNameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String CpnyID
        {
            get
            {
                return _CpnyID;
            }
            set
            {
                OnCpnyIDChanging(value);
                ReportPropertyChanging("CpnyID");
                _CpnyID = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("CpnyID");
                OnCpnyIDChanged();
            }
        }
        private global::System.String _CpnyID;
        partial void OnCpnyIDChanging(global::System.String value);
        partial void OnCpnyIDChanged();

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmComplexTypeAttribute(NamespaceName="IN20700Model", Name="IN20700_pdConfig_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class IN20700_pdConfig_Result : ComplexObject
    {
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Boolean> ShowSiteID
        {
            get
            {
                return _ShowSiteID;
            }
            set
            {
                OnShowSiteIDChanging(value);
                ReportPropertyChanging("ShowSiteID");
                _ShowSiteID = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("ShowSiteID");
                OnShowSiteIDChanged();
            }
        }
        private Nullable<global::System.Boolean> _ShowSiteID;
        partial void OnShowSiteIDChanging(Nullable<global::System.Boolean> value);
        partial void OnShowSiteIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Boolean> ShowSlsperID
        {
            get
            {
                return _ShowSlsperID;
            }
            set
            {
                OnShowSlsperIDChanging(value);
                ReportPropertyChanging("ShowSlsperID");
                _ShowSlsperID = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("ShowSlsperID");
                OnShowSlsperIDChanged();
            }
        }
        private Nullable<global::System.Boolean> _ShowSlsperID;
        partial void OnShowSlsperIDChanging(Nullable<global::System.Boolean> value);
        partial void OnShowSlsperIDChanged();

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmComplexTypeAttribute(NamespaceName="IN20700Model", Name="IN20700_pgLoadReasonCD_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class IN20700_pgLoadReasonCD_Result : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new IN20700_pgLoadReasonCD_Result object.
        /// </summary>
        /// <param name="reasonCD">Initial value of the ReasonCD property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static IN20700_pgLoadReasonCD_Result CreateIN20700_pgLoadReasonCD_Result(global::System.String reasonCD, global::System.Byte[] tstamp)
        {
            IN20700_pgLoadReasonCD_Result iN20700_pgLoadReasonCD_Result = new IN20700_pgLoadReasonCD_Result();
            iN20700_pgLoadReasonCD_Result.ReasonCD = reasonCD;
            iN20700_pgLoadReasonCD_Result.tstamp = tstamp;
            return iN20700_pgLoadReasonCD_Result;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String ReasonCD
        {
            get
            {
                return _ReasonCD;
            }
            set
            {
                OnReasonCDChanging(value);
                ReportPropertyChanging("ReasonCD");
                _ReasonCD = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("ReasonCD");
                OnReasonCDChanged();
            }
        }
        private global::System.String _ReasonCD;
        partial void OnReasonCDChanging(global::System.String value);
        partial void OnReasonCDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Descr
        {
            get
            {
                return _Descr;
            }
            set
            {
                OnDescrChanging(value);
                ReportPropertyChanging("Descr");
                _Descr = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Descr");
                OnDescrChanged();
            }
        }
        private global::System.String _Descr;
        partial void OnDescrChanging(global::System.String value);
        partial void OnDescrChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String SiteID
        {
            get
            {
                return _SiteID;
            }
            set
            {
                OnSiteIDChanging(value);
                ReportPropertyChanging("SiteID");
                _SiteID = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("SiteID");
                OnSiteIDChanged();
            }
        }
        private global::System.String _SiteID;
        partial void OnSiteIDChanging(global::System.String value);
        partial void OnSiteIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String SlsperID
        {
            get
            {
                return _SlsperID;
            }
            set
            {
                OnSlsperIDChanging(value);
                ReportPropertyChanging("SlsperID");
                _SlsperID = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("SlsperID");
                OnSlsperIDChanged();
            }
        }
        private global::System.String _SlsperID;
        partial void OnSlsperIDChanging(global::System.String value);
        partial void OnSlsperIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Byte[] tstamp
        {
            get
            {
                return StructuralObject.GetValidValue(_tstamp);
            }
            set
            {
                OntstampChanging(value);
                ReportPropertyChanging("tstamp");
                _tstamp = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("tstamp");
                OntstampChanged();
            }
        }
        private global::System.Byte[] _tstamp;
        partial void OntstampChanging(global::System.Byte[] value);
        partial void OntstampChanged();

        #endregion

    }

    #endregion

    
}
