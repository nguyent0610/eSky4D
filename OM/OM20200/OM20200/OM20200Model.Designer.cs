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
namespace OM20200
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class OM20200Entities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new OM20200Entities object using the connection string found in the 'OM20200Entities' section of the application configuration file.
        /// </summary>
        public OM20200Entities() : base("name=OM20200Entities", "OM20200Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new OM20200Entities object.
        /// </summary>
        public OM20200Entities(string connectionString) : base(connectionString, "OM20200Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new OM20200Entities object.
        /// </summary>
        public OM20200Entities(EntityConnection connection) : base(connection, "OM20200Entities")
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
        public ObjectSet<OM_PriceClass> OM_PriceClass
        {
            get
            {
                if ((_OM_PriceClass == null))
                {
                    _OM_PriceClass = base.CreateObjectSet<OM_PriceClass>("OM_PriceClass");
                }
                return _OM_PriceClass;
            }
        }
        private ObjectSet<OM_PriceClass> _OM_PriceClass;

        #endregion

        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the OM_PriceClass EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToOM_PriceClass(OM_PriceClass oM_PriceClass)
        {
            base.AddObject("OM_PriceClass", oM_PriceClass);
        }

        #endregion

        #region Function Imports
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectResult<OM20200_pgOM_PriceClass_Result> OM20200_pgOM_PriceClass()
        {
            return base.ExecuteFunction<OM20200_pgOM_PriceClass_Result>("OM20200_pgOM_PriceClass");
        }

        #endregion

    }

    #endregion

    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="OM20200Model", Name="OM_PriceClass")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class OM_PriceClass : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new OM_PriceClass object.
        /// </summary>
        /// <param name="priceClassID">Initial value of the PriceClassID property.</param>
        /// <param name="priceClassType">Initial value of the PriceClassType property.</param>
        /// <param name="descr">Initial value of the Descr property.</param>
        /// <param name="crtd_DateTime">Initial value of the Crtd_DateTime property.</param>
        /// <param name="crtd_Prog">Initial value of the Crtd_Prog property.</param>
        /// <param name="crtd_User">Initial value of the Crtd_User property.</param>
        /// <param name="lUpd_DateTime">Initial value of the LUpd_DateTime property.</param>
        /// <param name="lUpd_Prog">Initial value of the LUpd_Prog property.</param>
        /// <param name="lUpd_User">Initial value of the LUpd_User property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static OM_PriceClass CreateOM_PriceClass(global::System.String priceClassID, global::System.String priceClassType, global::System.String descr, global::System.DateTime crtd_DateTime, global::System.String crtd_Prog, global::System.String crtd_User, global::System.DateTime lUpd_DateTime, global::System.String lUpd_Prog, global::System.String lUpd_User, global::System.Byte[] tstamp)
        {
            OM_PriceClass oM_PriceClass = new OM_PriceClass();
            oM_PriceClass.PriceClassID = priceClassID;
            oM_PriceClass.PriceClassType = priceClassType;
            oM_PriceClass.Descr = descr;
            oM_PriceClass.Crtd_DateTime = crtd_DateTime;
            oM_PriceClass.Crtd_Prog = crtd_Prog;
            oM_PriceClass.Crtd_User = crtd_User;
            oM_PriceClass.LUpd_DateTime = lUpd_DateTime;
            oM_PriceClass.LUpd_Prog = lUpd_Prog;
            oM_PriceClass.LUpd_User = lUpd_User;
            oM_PriceClass.tstamp = tstamp;
            return oM_PriceClass;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String PriceClassID
        {
            get
            {
                return _PriceClassID;
            }
            set
            {
                if (_PriceClassID != value)
                {
                    OnPriceClassIDChanging(value);
                    ReportPropertyChanging("PriceClassID");
                    _PriceClassID = StructuralObject.SetValidValue(value, false);
                    ReportPropertyChanged("PriceClassID");
                    OnPriceClassIDChanged();
                }
            }
        }
        private global::System.String _PriceClassID;
        partial void OnPriceClassIDChanging(global::System.String value);
        partial void OnPriceClassIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String PriceClassType
        {
            get
            {
                return _PriceClassType;
            }
            set
            {
                if (_PriceClassType != value)
                {
                    OnPriceClassTypeChanging(value);
                    ReportPropertyChanging("PriceClassType");
                    _PriceClassType = StructuralObject.SetValidValue(value, false);
                    ReportPropertyChanged("PriceClassType");
                    OnPriceClassTypeChanged();
                }
            }
        }
        private global::System.String _PriceClassType;
        partial void OnPriceClassTypeChanging(global::System.String value);
        partial void OnPriceClassTypeChanged();
    
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
    [EdmComplexTypeAttribute(NamespaceName="OM20200Model", Name="OM20200_pgOM_PriceClass_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class OM20200_pgOM_PriceClass_Result : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new OM20200_pgOM_PriceClass_Result object.
        /// </summary>
        /// <param name="priceClassID">Initial value of the PriceClassID property.</param>
        /// <param name="descr">Initial value of the Descr property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static OM20200_pgOM_PriceClass_Result CreateOM20200_pgOM_PriceClass_Result(global::System.String priceClassID, global::System.String descr, global::System.Byte[] tstamp)
        {
            OM20200_pgOM_PriceClass_Result oM20200_pgOM_PriceClass_Result = new OM20200_pgOM_PriceClass_Result();
            oM20200_pgOM_PriceClass_Result.PriceClassID = priceClassID;
            oM20200_pgOM_PriceClass_Result.Descr = descr;
            oM20200_pgOM_PriceClass_Result.tstamp = tstamp;
            return oM20200_pgOM_PriceClass_Result;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String PriceClassID
        {
            get
            {
                return _PriceClassID;
            }
            set
            {
                OnPriceClassIDChanging(value);
                ReportPropertyChanging("PriceClassID");
                _PriceClassID = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("PriceClassID");
                OnPriceClassIDChanged();
            }
        }
        private global::System.String _PriceClassID;
        partial void OnPriceClassIDChanging(global::System.String value);
        partial void OnPriceClassIDChanged();
    
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
