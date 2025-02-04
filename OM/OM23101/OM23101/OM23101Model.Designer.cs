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
namespace OM23101
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class OM23101Entities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new OM23101Entities object using the connection string found in the 'OM23101Entities' section of the application configuration file.
        /// </summary>
        public OM23101Entities() : base("name=OM23101Entities", "OM23101Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new OM23101Entities object.
        /// </summary>
        public OM23101Entities(string connectionString) : base(connectionString, "OM23101Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new OM23101Entities object.
        /// </summary>
        public OM23101Entities(EntityConnection connection) : base(connection, "OM23101Entities")
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
        public ObjectSet<OM_FCSBranch> OM_FCSBranch
        {
            get
            {
                if ((_OM_FCSBranch == null))
                {
                    _OM_FCSBranch = base.CreateObjectSet<OM_FCSBranch>("OM_FCSBranch");
                }
                return _OM_FCSBranch;
            }
        }
        private ObjectSet<OM_FCSBranch> _OM_FCSBranch;

        #endregion

        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the OM_FCSBranch EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToOM_FCSBranch(OM_FCSBranch oM_FCSBranch)
        {
            base.AddObject("OM_FCSBranch", oM_FCSBranch);
        }

        #endregion

        #region Function Imports
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectResult<OM23101_getIN_ProductClass_Result> OM23101_getIN_ProductClass()
        {
            return base.ExecuteFunction<OM23101_getIN_ProductClass_Result>("OM23101_getIN_ProductClass");
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name="userID">No Metadata Documentation available.</param>
        /// <param name="territory">No Metadata Documentation available.</param>
        /// <param name="state">No Metadata Documentation available.</param>
        /// <param name="fCSDate">No Metadata Documentation available.</param>
        public int OM23101_pgLoadGrid(global::System.String userID, global::System.String territory, global::System.String state, Nullable<global::System.DateTime> fCSDate)
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
    
            ObjectParameter territoryParameter;
            if (territory != null)
            {
                territoryParameter = new ObjectParameter("Territory", territory);
            }
            else
            {
                territoryParameter = new ObjectParameter("Territory", typeof(global::System.String));
            }
    
            ObjectParameter stateParameter;
            if (state != null)
            {
                stateParameter = new ObjectParameter("State", state);
            }
            else
            {
                stateParameter = new ObjectParameter("State", typeof(global::System.String));
            }
    
            ObjectParameter fCSDateParameter;
            if (fCSDate.HasValue)
            {
                fCSDateParameter = new ObjectParameter("FCSDate", fCSDate);
            }
            else
            {
                fCSDateParameter = new ObjectParameter("FCSDate", typeof(global::System.DateTime));
            }
    
            return base.ExecuteFunction("OM23101_pgLoadGrid", userIDParameter, territoryParameter, stateParameter, fCSDateParameter);
        }

        #endregion

    }

    #endregion

    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="OM23101Model", Name="OM_FCSBranch")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class OM_FCSBranch : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new OM_FCSBranch object.
        /// </summary>
        /// <param name="branchID">Initial value of the BranchID property.</param>
        /// <param name="fCSDate">Initial value of the FCSDate property.</param>
        /// <param name="classID">Initial value of the ClassID property.</param>
        /// <param name="crtd_DateTime">Initial value of the Crtd_DateTime property.</param>
        /// <param name="crtd_Prog">Initial value of the Crtd_Prog property.</param>
        /// <param name="crtd_User">Initial value of the Crtd_User property.</param>
        /// <param name="lUpd_DateTime">Initial value of the LUpd_DateTime property.</param>
        /// <param name="lUpd_Prog">Initial value of the LUpd_Prog property.</param>
        /// <param name="lUpd_User">Initial value of the LUpd_User property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static OM_FCSBranch CreateOM_FCSBranch(global::System.String branchID, global::System.DateTime fCSDate, global::System.String classID, global::System.DateTime crtd_DateTime, global::System.String crtd_Prog, global::System.String crtd_User, global::System.DateTime lUpd_DateTime, global::System.String lUpd_Prog, global::System.String lUpd_User, global::System.Byte[] tstamp)
        {
            OM_FCSBranch oM_FCSBranch = new OM_FCSBranch();
            oM_FCSBranch.BranchID = branchID;
            oM_FCSBranch.FCSDate = fCSDate;
            oM_FCSBranch.ClassID = classID;
            oM_FCSBranch.Crtd_DateTime = crtd_DateTime;
            oM_FCSBranch.Crtd_Prog = crtd_Prog;
            oM_FCSBranch.Crtd_User = crtd_User;
            oM_FCSBranch.LUpd_DateTime = lUpd_DateTime;
            oM_FCSBranch.LUpd_Prog = lUpd_Prog;
            oM_FCSBranch.LUpd_User = lUpd_User;
            oM_FCSBranch.tstamp = tstamp;
            return oM_FCSBranch;
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
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.DateTime FCSDate
        {
            get
            {
                return _FCSDate;
            }
            set
            {
                if (_FCSDate != value)
                {
                    OnFCSDateChanging(value);
                    ReportPropertyChanging("FCSDate");
                    _FCSDate = StructuralObject.SetValidValue(value);
                    ReportPropertyChanged("FCSDate");
                    OnFCSDateChanged();
                }
            }
        }
        private global::System.DateTime _FCSDate;
        partial void OnFCSDateChanging(global::System.DateTime value);
        partial void OnFCSDateChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String ClassID
        {
            get
            {
                return _ClassID;
            }
            set
            {
                if (_ClassID != value)
                {
                    OnClassIDChanging(value);
                    ReportPropertyChanging("ClassID");
                    _ClassID = StructuralObject.SetValidValue(value, false);
                    ReportPropertyChanged("ClassID");
                    OnClassIDChanged();
                }
            }
        }
        private global::System.String _ClassID;
        partial void OnClassIDChanging(global::System.String value);
        partial void OnClassIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.Double> SellInKPI
        {
            get
            {
                return _SellInKPI;
            }
            set
            {
                OnSellInKPIChanging(value);
                ReportPropertyChanging("SellInKPI");
                _SellInKPI = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("SellInKPI");
                OnSellInKPIChanged();
            }
        }
        private Nullable<global::System.Double> _SellInKPI;
        partial void OnSellInKPIChanging(Nullable<global::System.Double> value);
        partial void OnSellInKPIChanged();
    
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
    [EdmComplexTypeAttribute(NamespaceName="OM23101Model", Name="OM23101_getIN_ProductClass_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class OM23101_getIN_ProductClass_Result : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new OM23101_getIN_ProductClass_Result object.
        /// </summary>
        /// <param name="classID">Initial value of the ClassID property.</param>
        public static OM23101_getIN_ProductClass_Result CreateOM23101_getIN_ProductClass_Result(global::System.String classID)
        {
            OM23101_getIN_ProductClass_Result oM23101_getIN_ProductClass_Result = new OM23101_getIN_ProductClass_Result();
            oM23101_getIN_ProductClass_Result.ClassID = classID;
            return oM23101_getIN_ProductClass_Result;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String ClassID
        {
            get
            {
                return _ClassID;
            }
            set
            {
                OnClassIDChanging(value);
                ReportPropertyChanging("ClassID");
                _ClassID = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("ClassID");
                OnClassIDChanged();
            }
        }
        private global::System.String _ClassID;
        partial void OnClassIDChanging(global::System.String value);
        partial void OnClassIDChanged();
    
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

    #endregion

    
}
