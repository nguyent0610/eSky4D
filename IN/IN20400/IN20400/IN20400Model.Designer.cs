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
namespace IN20400
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class IN20400Entities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new IN20400Entities object using the connection string found in the 'IN20400Entities' section of the application configuration file.
        /// </summary>
        public IN20400Entities() : base("name=IN20400Entities", "IN20400Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new IN20400Entities object.
        /// </summary>
        public IN20400Entities(string connectionString) : base(connectionString, "IN20400Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new IN20400Entities object.
        /// </summary>
        public IN20400Entities(EntityConnection connection) : base(connection, "IN20400Entities")
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
        public ObjectSet<IN_SiteLocation> IN_SiteLocation
        {
            get
            {
                if ((_IN_SiteLocation == null))
                {
                    _IN_SiteLocation = base.CreateObjectSet<IN_SiteLocation>("IN_SiteLocation");
                }
                return _IN_SiteLocation;
            }
        }
        private ObjectSet<IN_SiteLocation> _IN_SiteLocation;

        #endregion

        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the IN_SiteLocation EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToIN_SiteLocation(IN_SiteLocation iN_SiteLocation)
        {
            base.AddObject("IN_SiteLocation", iN_SiteLocation);
        }

        #endregion

        #region Function Imports
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name="cpnyID">No Metadata Documentation available.</param>
        /// <param name="userName">No Metadata Documentation available.</param>
        /// <param name="langID">No Metadata Documentation available.</param>
        /// <param name="siteID">No Metadata Documentation available.</param>
        public ObjectResult<IN20400_pdCheckSiteID_Result> IN20400_pdCheckSiteID(global::System.String cpnyID, global::System.String userName, Nullable<global::System.Int16> langID, global::System.String siteID)
        {
            ObjectParameter cpnyIDParameter;
            if (cpnyID != null)
            {
                cpnyIDParameter = new ObjectParameter("CpnyID", cpnyID);
            }
            else
            {
                cpnyIDParameter = new ObjectParameter("CpnyID", typeof(global::System.String));
            }
    
            ObjectParameter userNameParameter;
            if (userName != null)
            {
                userNameParameter = new ObjectParameter("UserName", userName);
            }
            else
            {
                userNameParameter = new ObjectParameter("UserName", typeof(global::System.String));
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
    
            ObjectParameter siteIDParameter;
            if (siteID != null)
            {
                siteIDParameter = new ObjectParameter("SiteID", siteID);
            }
            else
            {
                siteIDParameter = new ObjectParameter("SiteID", typeof(global::System.String));
            }
    
            return base.ExecuteFunction<IN20400_pdCheckSiteID_Result>("IN20400_pdCheckSiteID", cpnyIDParameter, userNameParameter, langIDParameter, siteIDParameter);
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name="cpnyID">No Metadata Documentation available.</param>
        /// <param name="userName">No Metadata Documentation available.</param>
        /// <param name="langID">No Metadata Documentation available.</param>
        /// <param name="siteID">No Metadata Documentation available.</param>
        public ObjectResult<IN20400_pgLoadSiteLocation_Result> IN20400_pgLoadSiteLocation(global::System.String cpnyID, global::System.String userName, Nullable<global::System.Int16> langID, global::System.String siteID)
        {
            ObjectParameter cpnyIDParameter;
            if (cpnyID != null)
            {
                cpnyIDParameter = new ObjectParameter("CpnyID", cpnyID);
            }
            else
            {
                cpnyIDParameter = new ObjectParameter("CpnyID", typeof(global::System.String));
            }
    
            ObjectParameter userNameParameter;
            if (userName != null)
            {
                userNameParameter = new ObjectParameter("UserName", userName);
            }
            else
            {
                userNameParameter = new ObjectParameter("UserName", typeof(global::System.String));
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
    
            ObjectParameter siteIDParameter;
            if (siteID != null)
            {
                siteIDParameter = new ObjectParameter("SiteID", siteID);
            }
            else
            {
                siteIDParameter = new ObjectParameter("SiteID", typeof(global::System.String));
            }
    
            return base.ExecuteFunction<IN20400_pgLoadSiteLocation_Result>("IN20400_pgLoadSiteLocation", cpnyIDParameter, userNameParameter, langIDParameter, siteIDParameter);
        }

        #endregion

    }

    #endregion

    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="IN20400Model", Name="IN_SiteLocation")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class IN_SiteLocation : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new IN_SiteLocation object.
        /// </summary>
        /// <param name="siteID">Initial value of the SiteID property.</param>
        /// <param name="whseLoc">Initial value of the WhseLoc property.</param>
        /// <param name="descr">Initial value of the Descr property.</param>
        /// <param name="salesAllowed">Initial value of the SalesAllowed property.</param>
        /// <param name="issueAllowed">Initial value of the IssueAllowed property.</param>
        /// <param name="crtd_DateTime">Initial value of the Crtd_DateTime property.</param>
        /// <param name="crtd_Prog">Initial value of the Crtd_Prog property.</param>
        /// <param name="crtd_User">Initial value of the Crtd_User property.</param>
        /// <param name="lUpd_DateTime">Initial value of the LUpd_DateTime property.</param>
        /// <param name="lUpd_Prog">Initial value of the LUpd_Prog property.</param>
        /// <param name="lUpd_User">Initial value of the LUpd_User property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static IN_SiteLocation CreateIN_SiteLocation(global::System.String siteID, global::System.String whseLoc, global::System.String descr, global::System.Boolean salesAllowed, global::System.Boolean issueAllowed, global::System.DateTime crtd_DateTime, global::System.String crtd_Prog, global::System.String crtd_User, global::System.DateTime lUpd_DateTime, global::System.String lUpd_Prog, global::System.String lUpd_User, global::System.Byte[] tstamp)
        {
            IN_SiteLocation iN_SiteLocation = new IN_SiteLocation();
            iN_SiteLocation.SiteID = siteID;
            iN_SiteLocation.WhseLoc = whseLoc;
            iN_SiteLocation.Descr = descr;
            iN_SiteLocation.SalesAllowed = salesAllowed;
            iN_SiteLocation.IssueAllowed = issueAllowed;
            iN_SiteLocation.Crtd_DateTime = crtd_DateTime;
            iN_SiteLocation.Crtd_Prog = crtd_Prog;
            iN_SiteLocation.Crtd_User = crtd_User;
            iN_SiteLocation.LUpd_DateTime = lUpd_DateTime;
            iN_SiteLocation.LUpd_Prog = lUpd_Prog;
            iN_SiteLocation.LUpd_User = lUpd_User;
            iN_SiteLocation.tstamp = tstamp;
            return iN_SiteLocation;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String SiteID
        {
            get
            {
                return _SiteID;
            }
            set
            {
                if (_SiteID != value)
                {
                    OnSiteIDChanging(value);
                    ReportPropertyChanging("SiteID");
                    _SiteID = StructuralObject.SetValidValue(value, false);
                    ReportPropertyChanged("SiteID");
                    OnSiteIDChanged();
                }
            }
        }
        private global::System.String _SiteID;
        partial void OnSiteIDChanging(global::System.String value);
        partial void OnSiteIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String WhseLoc
        {
            get
            {
                return _WhseLoc;
            }
            set
            {
                if (_WhseLoc != value)
                {
                    OnWhseLocChanging(value);
                    ReportPropertyChanging("WhseLoc");
                    _WhseLoc = StructuralObject.SetValidValue(value, false);
                    ReportPropertyChanged("WhseLoc");
                    OnWhseLocChanged();
                }
            }
        }
        private global::System.String _WhseLoc;
        partial void OnWhseLocChanging(global::System.String value);
        partial void OnWhseLocChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
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
                _Descr = StructuralObject.SetValidValue(value, false);
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
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Boolean SalesAllowed
        {
            get
            {
                return _SalesAllowed;
            }
            set
            {
                OnSalesAllowedChanging(value);
                ReportPropertyChanging("SalesAllowed");
                _SalesAllowed = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("SalesAllowed");
                OnSalesAllowedChanged();
            }
        }
        private global::System.Boolean _SalesAllowed;
        partial void OnSalesAllowedChanging(global::System.Boolean value);
        partial void OnSalesAllowedChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Boolean IssueAllowed
        {
            get
            {
                return _IssueAllowed;
            }
            set
            {
                OnIssueAllowedChanging(value);
                ReportPropertyChanging("IssueAllowed");
                _IssueAllowed = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("IssueAllowed");
                OnIssueAllowedChanged();
            }
        }
        private global::System.Boolean _IssueAllowed;
        partial void OnIssueAllowedChanging(global::System.Boolean value);
        partial void OnIssueAllowedChanged();
    
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
    [EdmComplexTypeAttribute(NamespaceName="IN20400Model", Name="IN20400_pdCheckSiteID_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class IN20400_pdCheckSiteID_Result : ComplexObject
    {
        #region Primitive Properties
    
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
        public global::System.String WhseLoc
        {
            get
            {
                return _WhseLoc;
            }
            set
            {
                OnWhseLocChanging(value);
                ReportPropertyChanging("WhseLoc");
                _WhseLoc = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("WhseLoc");
                OnWhseLocChanged();
            }
        }
        private global::System.String _WhseLoc;
        partial void OnWhseLocChanging(global::System.String value);
        partial void OnWhseLocChanged();

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmComplexTypeAttribute(NamespaceName="IN20400Model", Name="IN20400_pgLoadSiteLocation_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class IN20400_pgLoadSiteLocation_Result : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new IN20400_pgLoadSiteLocation_Result object.
        /// </summary>
        /// <param name="siteID">Initial value of the SiteID property.</param>
        /// <param name="whseLoc">Initial value of the WhseLoc property.</param>
        /// <param name="descr">Initial value of the Descr property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static IN20400_pgLoadSiteLocation_Result CreateIN20400_pgLoadSiteLocation_Result(global::System.String siteID, global::System.String whseLoc, global::System.String descr, global::System.Byte[] tstamp)
        {
            IN20400_pgLoadSiteLocation_Result iN20400_pgLoadSiteLocation_Result = new IN20400_pgLoadSiteLocation_Result();
            iN20400_pgLoadSiteLocation_Result.SiteID = siteID;
            iN20400_pgLoadSiteLocation_Result.WhseLoc = whseLoc;
            iN20400_pgLoadSiteLocation_Result.Descr = descr;
            iN20400_pgLoadSiteLocation_Result.tstamp = tstamp;
            return iN20400_pgLoadSiteLocation_Result;
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
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String WhseLoc
        {
            get
            {
                return _WhseLoc;
            }
            set
            {
                OnWhseLocChanging(value);
                ReportPropertyChanging("WhseLoc");
                _WhseLoc = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("WhseLoc");
                OnWhseLocChanged();
            }
        }
        private global::System.String _WhseLoc;
        partial void OnWhseLocChanging(global::System.String value);
        partial void OnWhseLocChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
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
                _Descr = StructuralObject.SetValidValue(value, false);
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
