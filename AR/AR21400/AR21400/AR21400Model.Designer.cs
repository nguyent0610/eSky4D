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
namespace AR21400
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class AR21400Entities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new AR21400Entities object using the connection string found in the 'AR21400Entities' section of the application configuration file.
        /// </summary>
        public AR21400Entities() : base("name=AR21400Entities", "AR21400Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new AR21400Entities object.
        /// </summary>
        public AR21400Entities(string connectionString) : base(connectionString, "AR21400Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new AR21400Entities object.
        /// </summary>
        public AR21400Entities(EntityConnection connection) : base(connection, "AR21400Entities")
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
        public ObjectSet<AR_SellingProducts> AR_SellingProducts
        {
            get
            {
                if ((_AR_SellingProducts == null))
                {
                    _AR_SellingProducts = base.CreateObjectSet<AR_SellingProducts>("AR_SellingProducts");
                }
                return _AR_SellingProducts;
            }
        }
        private ObjectSet<AR_SellingProducts> _AR_SellingProducts;

        #endregion

        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the AR_SellingProducts EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToAR_SellingProducts(AR_SellingProducts aR_SellingProducts)
        {
            base.AddObject("AR_SellingProducts", aR_SellingProducts);
        }

        #endregion

        #region Function Imports
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectResult<AR21400_pgLoadSellingProducts_Result> AR21400_pgLoadSellingProducts()
        {
            return base.ExecuteFunction<AR21400_pgLoadSellingProducts_Result>("AR21400_pgLoadSellingProducts");
        }

        #endregion

    }

    #endregion

    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="AR21400Model", Name="AR_SellingProducts")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class AR_SellingProducts : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new AR_SellingProducts object.
        /// </summary>
        /// <param name="code">Initial value of the Code property.</param>
        /// <param name="descr">Initial value of the Descr property.</param>
        /// <param name="lUpd_Datetime">Initial value of the LUpd_Datetime property.</param>
        /// <param name="lUpd_Prog">Initial value of the LUpd_Prog property.</param>
        /// <param name="lUpd_User">Initial value of the LUpd_User property.</param>
        /// <param name="crtd_Datetime">Initial value of the Crtd_Datetime property.</param>
        /// <param name="crtd_Prog">Initial value of the Crtd_Prog property.</param>
        /// <param name="crtd_User">Initial value of the Crtd_User property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static AR_SellingProducts CreateAR_SellingProducts(global::System.String code, global::System.String descr, global::System.DateTime lUpd_Datetime, global::System.String lUpd_Prog, global::System.String lUpd_User, global::System.DateTime crtd_Datetime, global::System.String crtd_Prog, global::System.String crtd_User, global::System.Byte[] tstamp)
        {
            AR_SellingProducts aR_SellingProducts = new AR_SellingProducts();
            aR_SellingProducts.Code = code;
            aR_SellingProducts.Descr = descr;
            aR_SellingProducts.LUpd_Datetime = lUpd_Datetime;
            aR_SellingProducts.LUpd_Prog = lUpd_Prog;
            aR_SellingProducts.LUpd_User = lUpd_User;
            aR_SellingProducts.Crtd_Datetime = crtd_Datetime;
            aR_SellingProducts.Crtd_Prog = crtd_Prog;
            aR_SellingProducts.Crtd_User = crtd_User;
            aR_SellingProducts.tstamp = tstamp;
            return aR_SellingProducts;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Code
        {
            get
            {
                return _Code;
            }
            set
            {
                if (_Code != value)
                {
                    OnCodeChanging(value);
                    ReportPropertyChanging("Code");
                    _Code = StructuralObject.SetValidValue(value, false);
                    ReportPropertyChanged("Code");
                    OnCodeChanged();
                }
            }
        }
        private global::System.String _Code;
        partial void OnCodeChanging(global::System.String value);
        partial void OnCodeChanged();
    
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
        public global::System.DateTime LUpd_Datetime
        {
            get
            {
                return _LUpd_Datetime;
            }
            set
            {
                OnLUpd_DatetimeChanging(value);
                ReportPropertyChanging("LUpd_Datetime");
                _LUpd_Datetime = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("LUpd_Datetime");
                OnLUpd_DatetimeChanged();
            }
        }
        private global::System.DateTime _LUpd_Datetime;
        partial void OnLUpd_DatetimeChanging(global::System.DateTime value);
        partial void OnLUpd_DatetimeChanged();
    
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
        public global::System.DateTime Crtd_Datetime
        {
            get
            {
                return _Crtd_Datetime;
            }
            set
            {
                OnCrtd_DatetimeChanging(value);
                ReportPropertyChanging("Crtd_Datetime");
                _Crtd_Datetime = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("Crtd_Datetime");
                OnCrtd_DatetimeChanged();
            }
        }
        private global::System.DateTime _Crtd_Datetime;
        partial void OnCrtd_DatetimeChanging(global::System.DateTime value);
        partial void OnCrtd_DatetimeChanged();
    
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
    [EdmComplexTypeAttribute(NamespaceName="AR21400Model", Name="AR21400_pgLoadSellingProducts_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class AR21400_pgLoadSellingProducts_Result : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new AR21400_pgLoadSellingProducts_Result object.
        /// </summary>
        /// <param name="code">Initial value of the Code property.</param>
        /// <param name="descr">Initial value of the Descr property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static AR21400_pgLoadSellingProducts_Result CreateAR21400_pgLoadSellingProducts_Result(global::System.String code, global::System.String descr, global::System.Byte[] tstamp)
        {
            AR21400_pgLoadSellingProducts_Result aR21400_pgLoadSellingProducts_Result = new AR21400_pgLoadSellingProducts_Result();
            aR21400_pgLoadSellingProducts_Result.Code = code;
            aR21400_pgLoadSellingProducts_Result.Descr = descr;
            aR21400_pgLoadSellingProducts_Result.tstamp = tstamp;
            return aR21400_pgLoadSellingProducts_Result;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String Code
        {
            get
            {
                return _Code;
            }
            set
            {
                OnCodeChanging(value);
                ReportPropertyChanging("Code");
                _Code = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("Code");
                OnCodeChanged();
            }
        }
        private global::System.String _Code;
        partial void OnCodeChanging(global::System.String value);
        partial void OnCodeChanged();
    
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
