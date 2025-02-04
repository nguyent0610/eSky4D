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
namespace SA40000
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class SA40000Entities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new SA40000Entities object using the connection string found in the 'SA40000Entities' section of the application configuration file.
        /// </summary>
        public SA40000Entities() : base("name=SA40000Entities", "SA40000Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new SA40000Entities object.
        /// </summary>
        public SA40000Entities(string connectionString) : base(connectionString, "SA40000Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new SA40000Entities object.
        /// </summary>
        public SA40000Entities(EntityConnection connection) : base(connection, "SA40000Entities")
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
        public ObjectSet<SYS_CloseDateSetUp> SYS_CloseDateSetUp
        {
            get
            {
                if ((_SYS_CloseDateSetUp == null))
                {
                    _SYS_CloseDateSetUp = base.CreateObjectSet<SYS_CloseDateSetUp>("SYS_CloseDateSetUp");
                }
                return _SYS_CloseDateSetUp;
            }
        }
        private ObjectSet<SYS_CloseDateSetUp> _SYS_CloseDateSetUp;

        #endregion

        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the SYS_CloseDateSetUp EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToSYS_CloseDateSetUp(SYS_CloseDateSetUp sYS_CloseDateSetUp)
        {
            base.AddObject("SYS_CloseDateSetUp", sYS_CloseDateSetUp);
        }

        #endregion

        #region Function Imports
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectResult<SA40000_pgSYS_CloseDateSetUp_Result> SA40000_pgSYS_CloseDateSetUp()
        {
            return base.ExecuteFunction<SA40000_pgSYS_CloseDateSetUp_Result>("SA40000_pgSYS_CloseDateSetUp");
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name="userID">No Metadata Documentation available.</param>
        public ObjectResult<SA40000_pdCompany_Result> SA40000_pdCompany(global::System.String userID)
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
    
            return base.ExecuteFunction<SA40000_pdCompany_Result>("SA40000_pdCompany", userIDParameter);
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name="userID">No Metadata Documentation available.</param>
        public ObjectResult<SA40000_pdTerritory_Result> SA40000_pdTerritory(global::System.String userID)
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
    
            return base.ExecuteFunction<SA40000_pdTerritory_Result>("SA40000_pdTerritory", userIDParameter);
        }

        #endregion

    }

    #endregion

    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="SA40000Model", Name="SYS_CloseDateSetUp")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class SYS_CloseDateSetUp : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new SYS_CloseDateSetUp object.
        /// </summary>
        /// <param name="branchID">Initial value of the BranchID property.</param>
        /// <param name="wrkDateChk">Initial value of the WrkDateChk property.</param>
        /// <param name="wrkAdjDate">Initial value of the WrkAdjDate property.</param>
        /// <param name="wrkOpenDate">Initial value of the WrkOpenDate property.</param>
        /// <param name="wrkLowerDays">Initial value of the WrkLowerDays property.</param>
        /// <param name="wrkUpperDays">Initial value of the WrkUpperDays property.</param>
        /// <param name="crtd_DateTime">Initial value of the Crtd_DateTime property.</param>
        /// <param name="crtd_Prog">Initial value of the Crtd_Prog property.</param>
        /// <param name="crtd_User">Initial value of the Crtd_User property.</param>
        /// <param name="lUpd_DateTime">Initial value of the LUpd_DateTime property.</param>
        /// <param name="lUpd_Prog">Initial value of the LUpd_Prog property.</param>
        /// <param name="lUpd_User">Initial value of the LUpd_User property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static SYS_CloseDateSetUp CreateSYS_CloseDateSetUp(global::System.String branchID, global::System.Boolean wrkDateChk, global::System.DateTime wrkAdjDate, global::System.DateTime wrkOpenDate, global::System.Int32 wrkLowerDays, global::System.Int32 wrkUpperDays, global::System.DateTime crtd_DateTime, global::System.String crtd_Prog, global::System.String crtd_User, global::System.DateTime lUpd_DateTime, global::System.String lUpd_Prog, global::System.String lUpd_User, global::System.Byte[] tstamp)
        {
            SYS_CloseDateSetUp sYS_CloseDateSetUp = new SYS_CloseDateSetUp();
            sYS_CloseDateSetUp.BranchID = branchID;
            sYS_CloseDateSetUp.WrkDateChk = wrkDateChk;
            sYS_CloseDateSetUp.WrkAdjDate = wrkAdjDate;
            sYS_CloseDateSetUp.WrkOpenDate = wrkOpenDate;
            sYS_CloseDateSetUp.WrkLowerDays = wrkLowerDays;
            sYS_CloseDateSetUp.WrkUpperDays = wrkUpperDays;
            sYS_CloseDateSetUp.Crtd_DateTime = crtd_DateTime;
            sYS_CloseDateSetUp.Crtd_Prog = crtd_Prog;
            sYS_CloseDateSetUp.Crtd_User = crtd_User;
            sYS_CloseDateSetUp.LUpd_DateTime = lUpd_DateTime;
            sYS_CloseDateSetUp.LUpd_Prog = lUpd_Prog;
            sYS_CloseDateSetUp.LUpd_User = lUpd_User;
            sYS_CloseDateSetUp.tstamp = tstamp;
            return sYS_CloseDateSetUp;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String BranchID
        {
            get
            {
                return _BranchID;
            }
            set
            {
                if (_BranchID != value)
                {
                    OnBranchIDChanging(value);
                    ReportPropertyChanging("BranchID");
                    _BranchID = StructuralObject.SetValidValue(value, false);
                    ReportPropertyChanged("BranchID");
                    OnBranchIDChanged();
                }
            }
        }
        private global::System.String _BranchID;
        partial void OnBranchIDChanging(global::System.String value);
        partial void OnBranchIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Boolean WrkDateChk
        {
            get
            {
                return _WrkDateChk;
            }
            set
            {
                OnWrkDateChkChanging(value);
                ReportPropertyChanging("WrkDateChk");
                _WrkDateChk = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("WrkDateChk");
                OnWrkDateChkChanged();
            }
        }
        private global::System.Boolean _WrkDateChk;
        partial void OnWrkDateChkChanging(global::System.Boolean value);
        partial void OnWrkDateChkChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime WrkAdjDate
        {
            get
            {
                return _WrkAdjDate;
            }
            set
            {
                OnWrkAdjDateChanging(value);
                ReportPropertyChanging("WrkAdjDate");
                _WrkAdjDate = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("WrkAdjDate");
                OnWrkAdjDateChanged();
            }
        }
        private global::System.DateTime _WrkAdjDate;
        partial void OnWrkAdjDateChanging(global::System.DateTime value);
        partial void OnWrkAdjDateChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime WrkOpenDate
        {
            get
            {
                return _WrkOpenDate;
            }
            set
            {
                OnWrkOpenDateChanging(value);
                ReportPropertyChanging("WrkOpenDate");
                _WrkOpenDate = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("WrkOpenDate");
                OnWrkOpenDateChanged();
            }
        }
        private global::System.DateTime _WrkOpenDate;
        partial void OnWrkOpenDateChanging(global::System.DateTime value);
        partial void OnWrkOpenDateChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 WrkLowerDays
        {
            get
            {
                return _WrkLowerDays;
            }
            set
            {
                OnWrkLowerDaysChanging(value);
                ReportPropertyChanging("WrkLowerDays");
                _WrkLowerDays = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("WrkLowerDays");
                OnWrkLowerDaysChanged();
            }
        }
        private global::System.Int32 _WrkLowerDays;
        partial void OnWrkLowerDaysChanging(global::System.Int32 value);
        partial void OnWrkLowerDaysChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 WrkUpperDays
        {
            get
            {
                return _WrkUpperDays;
            }
            set
            {
                OnWrkUpperDaysChanging(value);
                ReportPropertyChanging("WrkUpperDays");
                _WrkUpperDays = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("WrkUpperDays");
                OnWrkUpperDaysChanged();
            }
        }
        private global::System.Int32 _WrkUpperDays;
        partial void OnWrkUpperDaysChanging(global::System.Int32 value);
        partial void OnWrkUpperDaysChanged();
    
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
    [EdmComplexTypeAttribute(NamespaceName="SA40000Model", Name="SA40000_pdCompany_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class SA40000_pdCompany_Result : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new SA40000_pdCompany_Result object.
        /// </summary>
        /// <param name="cpnyID">Initial value of the CpnyID property.</param>
        public static SA40000_pdCompany_Result CreateSA40000_pdCompany_Result(global::System.String cpnyID)
        {
            SA40000_pdCompany_Result sA40000_pdCompany_Result = new SA40000_pdCompany_Result();
            sA40000_pdCompany_Result.CpnyID = cpnyID;
            return sA40000_pdCompany_Result;
        }

        #endregion

        #region Primitive Properties
    
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
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String CpnyName
        {
            get
            {
                return _CpnyName;
            }
            set
            {
                OnCpnyNameChanging(value);
                ReportPropertyChanging("CpnyName");
                _CpnyName = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("CpnyName");
                OnCpnyNameChanged();
            }
        }
        private global::System.String _CpnyName;
        partial void OnCpnyNameChanging(global::System.String value);
        partial void OnCpnyNameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Territory
        {
            get
            {
                return _Territory;
            }
            set
            {
                OnTerritoryChanging(value);
                ReportPropertyChanging("Territory");
                _Territory = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Territory");
                OnTerritoryChanged();
            }
        }
        private global::System.String _Territory;
        partial void OnTerritoryChanging(global::System.String value);
        partial void OnTerritoryChanged();

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmComplexTypeAttribute(NamespaceName="SA40000Model", Name="SA40000_pdTerritory_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class SA40000_pdTerritory_Result : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new SA40000_pdTerritory_Result object.
        /// </summary>
        /// <param name="territory">Initial value of the Territory property.</param>
        public static SA40000_pdTerritory_Result CreateSA40000_pdTerritory_Result(global::System.String territory)
        {
            SA40000_pdTerritory_Result sA40000_pdTerritory_Result = new SA40000_pdTerritory_Result();
            sA40000_pdTerritory_Result.Territory = territory;
            return sA40000_pdTerritory_Result;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Territory
        {
            get
            {
                return _Territory;
            }
            set
            {
                OnTerritoryChanging(value);
                ReportPropertyChanging("Territory");
                _Territory = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("Territory");
                OnTerritoryChanged();
            }
        }
        private global::System.String _Territory;
        partial void OnTerritoryChanging(global::System.String value);
        partial void OnTerritoryChanged();
    
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

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmComplexTypeAttribute(NamespaceName="SA40000Model", Name="SA40000_pgSYS_CloseDateSetUp_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class SA40000_pgSYS_CloseDateSetUp_Result : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new SA40000_pgSYS_CloseDateSetUp_Result object.
        /// </summary>
        /// <param name="branchID">Initial value of the BranchID property.</param>
        /// <param name="wrkDateChk">Initial value of the WrkDateChk property.</param>
        /// <param name="wrkAdjDate">Initial value of the WrkAdjDate property.</param>
        /// <param name="wrkOpenDate">Initial value of the WrkOpenDate property.</param>
        /// <param name="wrkLowerDays">Initial value of the WrkLowerDays property.</param>
        /// <param name="wrkUpperDays">Initial value of the WrkUpperDays property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static SA40000_pgSYS_CloseDateSetUp_Result CreateSA40000_pgSYS_CloseDateSetUp_Result(global::System.String branchID, global::System.Boolean wrkDateChk, global::System.DateTime wrkAdjDate, global::System.DateTime wrkOpenDate, global::System.Int32 wrkLowerDays, global::System.Int32 wrkUpperDays, global::System.Byte[] tstamp)
        {
            SA40000_pgSYS_CloseDateSetUp_Result sA40000_pgSYS_CloseDateSetUp_Result = new SA40000_pgSYS_CloseDateSetUp_Result();
            sA40000_pgSYS_CloseDateSetUp_Result.BranchID = branchID;
            sA40000_pgSYS_CloseDateSetUp_Result.WrkDateChk = wrkDateChk;
            sA40000_pgSYS_CloseDateSetUp_Result.WrkAdjDate = wrkAdjDate;
            sA40000_pgSYS_CloseDateSetUp_Result.WrkOpenDate = wrkOpenDate;
            sA40000_pgSYS_CloseDateSetUp_Result.WrkLowerDays = wrkLowerDays;
            sA40000_pgSYS_CloseDateSetUp_Result.WrkUpperDays = wrkUpperDays;
            sA40000_pgSYS_CloseDateSetUp_Result.tstamp = tstamp;
            return sA40000_pgSYS_CloseDateSetUp_Result;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Address
        {
            get
            {
                return _Address;
            }
            set
            {
                OnAddressChanging(value);
                ReportPropertyChanging("Address");
                _Address = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Address");
                OnAddressChanged();
            }
        }
        private global::System.String _Address;
        partial void OnAddressChanging(global::System.String value);
        partial void OnAddressChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Territory
        {
            get
            {
                return _Territory;
            }
            set
            {
                OnTerritoryChanging(value);
                ReportPropertyChanging("Territory");
                _Territory = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Territory");
                OnTerritoryChanged();
            }
        }
        private global::System.String _Territory;
        partial void OnTerritoryChanging(global::System.String value);
        partial void OnTerritoryChanged();
    
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
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String BranchName
        {
            get
            {
                return _BranchName;
            }
            set
            {
                OnBranchNameChanging(value);
                ReportPropertyChanging("BranchName");
                _BranchName = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("BranchName");
                OnBranchNameChanged();
            }
        }
        private global::System.String _BranchName;
        partial void OnBranchNameChanging(global::System.String value);
        partial void OnBranchNameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Boolean WrkDateChk
        {
            get
            {
                return _WrkDateChk;
            }
            set
            {
                OnWrkDateChkChanging(value);
                ReportPropertyChanging("WrkDateChk");
                _WrkDateChk = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("WrkDateChk");
                OnWrkDateChkChanged();
            }
        }
        private global::System.Boolean _WrkDateChk;
        partial void OnWrkDateChkChanging(global::System.Boolean value);
        partial void OnWrkDateChkChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime WrkAdjDate
        {
            get
            {
                return _WrkAdjDate;
            }
            set
            {
                OnWrkAdjDateChanging(value);
                ReportPropertyChanging("WrkAdjDate");
                _WrkAdjDate = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("WrkAdjDate");
                OnWrkAdjDateChanged();
            }
        }
        private global::System.DateTime _WrkAdjDate;
        partial void OnWrkAdjDateChanging(global::System.DateTime value);
        partial void OnWrkAdjDateChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime WrkOpenDate
        {
            get
            {
                return _WrkOpenDate;
            }
            set
            {
                OnWrkOpenDateChanging(value);
                ReportPropertyChanging("WrkOpenDate");
                _WrkOpenDate = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("WrkOpenDate");
                OnWrkOpenDateChanged();
            }
        }
        private global::System.DateTime _WrkOpenDate;
        partial void OnWrkOpenDateChanging(global::System.DateTime value);
        partial void OnWrkOpenDateChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 WrkLowerDays
        {
            get
            {
                return _WrkLowerDays;
            }
            set
            {
                OnWrkLowerDaysChanging(value);
                ReportPropertyChanging("WrkLowerDays");
                _WrkLowerDays = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("WrkLowerDays");
                OnWrkLowerDaysChanged();
            }
        }
        private global::System.Int32 _WrkLowerDays;
        partial void OnWrkLowerDaysChanging(global::System.Int32 value);
        partial void OnWrkLowerDaysChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.Int32 WrkUpperDays
        {
            get
            {
                return _WrkUpperDays;
            }
            set
            {
                OnWrkUpperDaysChanging(value);
                ReportPropertyChanging("WrkUpperDays");
                _WrkUpperDays = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("WrkUpperDays");
                OnWrkUpperDaysChanged();
            }
        }
        private global::System.Int32 _WrkUpperDays;
        partial void OnWrkUpperDaysChanging(global::System.Int32 value);
        partial void OnWrkUpperDaysChanged();
    
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
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Boolean> isChange
        {
            get
            {
                return _isChange;
            }
            set
            {
                OnisChangeChanging(value);
                ReportPropertyChanging("isChange");
                _isChange = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("isChange");
                OnisChangeChanged();
            }
        }
        private Nullable<global::System.Boolean> _isChange;
        partial void OnisChangeChanging(Nullable<global::System.Boolean> value);
        partial void OnisChangeChanged();

        #endregion

    }

    #endregion

    
}
