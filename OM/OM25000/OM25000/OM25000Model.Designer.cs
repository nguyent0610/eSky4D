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
namespace OM25000
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class OM25000Entities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new OM25000Entities object using the connection string found in the 'OM25000Entities' section of the application configuration file.
        /// </summary>
        public OM25000Entities() : base("name=OM25000Entities", "OM25000Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new OM25000Entities object.
        /// </summary>
        public OM25000Entities(string connectionString) : base(connectionString, "OM25000Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new OM25000Entities object.
        /// </summary>
        public OM25000Entities(EntityConnection connection) : base(connection, "OM25000Entities")
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
        public ObjectSet<OM_KPI> OM_KPI
        {
            get
            {
                if ((_OM_KPI == null))
                {
                    _OM_KPI = base.CreateObjectSet<OM_KPI>("OM_KPI");
                }
                return _OM_KPI;
            }
        }
        private ObjectSet<OM_KPI> _OM_KPI;

        #endregion

        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the OM_KPI EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToOM_KPI(OM_KPI oM_KPI)
        {
            base.AddObject("OM_KPI", oM_KPI);
        }

        #endregion

        #region Function Imports
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name="userID">No Metadata Documentation available.</param>
        /// <param name="cpnyID">No Metadata Documentation available.</param>
        /// <param name="langID">No Metadata Documentation available.</param>
        public ObjectResult<OM25000_pgLoadKPI_Result> OM25000_pgLoadKPI(global::System.String userID, global::System.String cpnyID, Nullable<global::System.Int16> langID)
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
    
            return base.ExecuteFunction<OM25000_pgLoadKPI_Result>("OM25000_pgLoadKPI", userIDParameter, cpnyIDParameter, langIDParameter);
        }

        #endregion

    }

    #endregion

    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="OM25000Model", Name="OM_KPI")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class OM_KPI : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new OM_KPI object.
        /// </summary>
        /// <param name="kPI">Initial value of the KPI property.</param>
        /// <param name="name">Initial value of the Name property.</param>
        /// <param name="applyFor">Initial value of the ApplyFor property.</param>
        /// <param name="applyTo">Initial value of the ApplyTo property.</param>
        /// <param name="type">Initial value of the Type property.</param>
        /// <param name="lUpd_DateTime">Initial value of the LUpd_DateTime property.</param>
        /// <param name="lUpd_Prog">Initial value of the LUpd_Prog property.</param>
        /// <param name="lUpd_User">Initial value of the LUpd_User property.</param>
        /// <param name="crtd_DateTime">Initial value of the Crtd_DateTime property.</param>
        /// <param name="crtd_Prog">Initial value of the Crtd_Prog property.</param>
        /// <param name="crtd_User">Initial value of the Crtd_User property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static OM_KPI CreateOM_KPI(global::System.String kPI, global::System.String name, global::System.String applyFor, global::System.String applyTo, global::System.String type, global::System.DateTime lUpd_DateTime, global::System.String lUpd_Prog, global::System.String lUpd_User, global::System.DateTime crtd_DateTime, global::System.String crtd_Prog, global::System.String crtd_User, global::System.Byte[] tstamp)
        {
            OM_KPI oM_KPI = new OM_KPI();
            oM_KPI.KPI = kPI;
            oM_KPI.Name = name;
            oM_KPI.ApplyFor = applyFor;
            oM_KPI.ApplyTo = applyTo;
            oM_KPI.Type = type;
            oM_KPI.LUpd_DateTime = lUpd_DateTime;
            oM_KPI.LUpd_Prog = lUpd_Prog;
            oM_KPI.LUpd_User = lUpd_User;
            oM_KPI.Crtd_DateTime = crtd_DateTime;
            oM_KPI.Crtd_Prog = crtd_Prog;
            oM_KPI.Crtd_User = crtd_User;
            oM_KPI.tstamp = tstamp;
            return oM_KPI;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String KPI
        {
            get
            {
                return _KPI;
            }
            set
            {
                if (_KPI != value)
                {
                    OnKPIChanging(value);
                    ReportPropertyChanging("KPI");
                    _KPI = StructuralObject.SetValidValue(value, false);
                    ReportPropertyChanged("KPI");
                    OnKPIChanged();
                }
            }
        }
        private global::System.String _KPI;
        partial void OnKPIChanging(global::System.String value);
        partial void OnKPIChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
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
                _Name = StructuralObject.SetValidValue(value, false);
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
        public global::System.String ApplyFor
        {
            get
            {
                return _ApplyFor;
            }
            set
            {
                OnApplyForChanging(value);
                ReportPropertyChanging("ApplyFor");
                _ApplyFor = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("ApplyFor");
                OnApplyForChanged();
            }
        }
        private global::System.String _ApplyFor;
        partial void OnApplyForChanging(global::System.String value);
        partial void OnApplyForChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String ApplyTo
        {
            get
            {
                return _ApplyTo;
            }
            set
            {
                OnApplyToChanging(value);
                ReportPropertyChanging("ApplyTo");
                _ApplyTo = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("ApplyTo");
                OnApplyToChanged();
            }
        }
        private global::System.String _ApplyTo;
        partial void OnApplyToChanging(global::System.String value);
        partial void OnApplyToChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Type
        {
            get
            {
                return _Type;
            }
            set
            {
                OnTypeChanging(value);
                ReportPropertyChanging("Type");
                _Type = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("Type");
                OnTypeChanged();
            }
        }
        private global::System.String _Type;
        partial void OnTypeChanging(global::System.String value);
        partial void OnTypeChanged();
    
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
    [EdmComplexTypeAttribute(NamespaceName="OM25000Model", Name="OM25000_pgLoadKPI_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class OM25000_pgLoadKPI_Result : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new OM25000_pgLoadKPI_Result object.
        /// </summary>
        /// <param name="kPI">Initial value of the KPI property.</param>
        /// <param name="name">Initial value of the Name property.</param>
        /// <param name="applyFor">Initial value of the ApplyFor property.</param>
        /// <param name="applyTo">Initial value of the ApplyTo property.</param>
        /// <param name="type">Initial value of the Type property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static OM25000_pgLoadKPI_Result CreateOM25000_pgLoadKPI_Result(global::System.String kPI, global::System.String name, global::System.String applyFor, global::System.String applyTo, global::System.String type, global::System.Byte[] tstamp)
        {
            OM25000_pgLoadKPI_Result oM25000_pgLoadKPI_Result = new OM25000_pgLoadKPI_Result();
            oM25000_pgLoadKPI_Result.KPI = kPI;
            oM25000_pgLoadKPI_Result.Name = name;
            oM25000_pgLoadKPI_Result.ApplyFor = applyFor;
            oM25000_pgLoadKPI_Result.ApplyTo = applyTo;
            oM25000_pgLoadKPI_Result.Type = type;
            oM25000_pgLoadKPI_Result.tstamp = tstamp;
            return oM25000_pgLoadKPI_Result;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String KPI
        {
            get
            {
                return _KPI;
            }
            set
            {
                OnKPIChanging(value);
                ReportPropertyChanging("KPI");
                _KPI = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("KPI");
                OnKPIChanged();
            }
        }
        private global::System.String _KPI;
        partial void OnKPIChanging(global::System.String value);
        partial void OnKPIChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
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
                _Name = StructuralObject.SetValidValue(value, false);
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
        public global::System.String ApplyFor
        {
            get
            {
                return _ApplyFor;
            }
            set
            {
                OnApplyForChanging(value);
                ReportPropertyChanging("ApplyFor");
                _ApplyFor = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("ApplyFor");
                OnApplyForChanged();
            }
        }
        private global::System.String _ApplyFor;
        partial void OnApplyForChanging(global::System.String value);
        partial void OnApplyForChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String ApplyForDescr
        {
            get
            {
                return _ApplyForDescr;
            }
            set
            {
                OnApplyForDescrChanging(value);
                ReportPropertyChanging("ApplyForDescr");
                _ApplyForDescr = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("ApplyForDescr");
                OnApplyForDescrChanged();
            }
        }
        private global::System.String _ApplyForDescr;
        partial void OnApplyForDescrChanging(global::System.String value);
        partial void OnApplyForDescrChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String ApplyTo
        {
            get
            {
                return _ApplyTo;
            }
            set
            {
                OnApplyToChanging(value);
                ReportPropertyChanging("ApplyTo");
                _ApplyTo = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("ApplyTo");
                OnApplyToChanged();
            }
        }
        private global::System.String _ApplyTo;
        partial void OnApplyToChanging(global::System.String value);
        partial void OnApplyToChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String ApplyToDescr
        {
            get
            {
                return _ApplyToDescr;
            }
            set
            {
                OnApplyToDescrChanging(value);
                ReportPropertyChanging("ApplyToDescr");
                _ApplyToDescr = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("ApplyToDescr");
                OnApplyToDescrChanged();
            }
        }
        private global::System.String _ApplyToDescr;
        partial void OnApplyToDescrChanging(global::System.String value);
        partial void OnApplyToDescrChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Type
        {
            get
            {
                return _Type;
            }
            set
            {
                OnTypeChanging(value);
                ReportPropertyChanging("Type");
                _Type = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("Type");
                OnTypeChanged();
            }
        }
        private global::System.String _Type;
        partial void OnTypeChanging(global::System.String value);
        partial void OnTypeChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String TypeDescr
        {
            get
            {
                return _TypeDescr;
            }
            set
            {
                OnTypeDescrChanging(value);
                ReportPropertyChanging("TypeDescr");
                _TypeDescr = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("TypeDescr");
                OnTypeDescrChanged();
            }
        }
        private global::System.String _TypeDescr;
        partial void OnTypeDescrChanging(global::System.String value);
        partial void OnTypeDescrChanged();
    
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
