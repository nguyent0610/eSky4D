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
namespace AR20100
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class AR20100Entities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new AR20100Entities object using the connection string found in the 'AR20100Entities' section of the application configuration file.
        /// </summary>
        public AR20100Entities() : base("name=AR20100Entities", "AR20100Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new AR20100Entities object.
        /// </summary>
        public AR20100Entities(string connectionString) : base(connectionString, "AR20100Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new AR20100Entities object.
        /// </summary>
        public AR20100Entities(EntityConnection connection) : base(connection, "AR20100Entities")
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
        public ObjectSet<AR_CustClass> AR_CustClass
        {
            get
            {
                if ((_AR_CustClass == null))
                {
                    _AR_CustClass = base.CreateObjectSet<AR_CustClass>("AR_CustClass");
                }
                return _AR_CustClass;
            }
        }
        private ObjectSet<AR_CustClass> _AR_CustClass;

        #endregion

        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the AR_CustClass EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToAR_CustClass(AR_CustClass aR_CustClass)
        {
            base.AddObject("AR_CustClass", aR_CustClass);
        }

        #endregion

        #region Function Imports
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectResult<AR20100_ppAR_CustClass_Result> AR20100_ppAR_CustClass()
        {
            return base.ExecuteFunction<AR20100_ppAR_CustClass_Result>("AR20100_ppAR_CustClass");
        }

        #endregion

    }

    #endregion

    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="AR20100Model", Name="AR_CustClass")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class AR_CustClass : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new AR_CustClass object.
        /// </summary>
        /// <param name="classId">Initial value of the ClassId property.</param>
        /// <param name="tradeDisc">Initial value of the TradeDisc property.</param>
        /// <param name="lUpd_DateTime">Initial value of the LUpd_DateTime property.</param>
        /// <param name="lUpd_Prog">Initial value of the LUpd_Prog property.</param>
        /// <param name="lUpd_User">Initial value of the LUpd_User property.</param>
        /// <param name="crtd_DateTime">Initial value of the Crtd_DateTime property.</param>
        /// <param name="crtd_Prog">Initial value of the Crtd_Prog property.</param>
        /// <param name="crtd_User">Initial value of the Crtd_User property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static AR_CustClass CreateAR_CustClass(global::System.String classId, global::System.Double tradeDisc, global::System.DateTime lUpd_DateTime, global::System.String lUpd_Prog, global::System.String lUpd_User, global::System.DateTime crtd_DateTime, global::System.String crtd_Prog, global::System.String crtd_User, global::System.Byte[] tstamp)
        {
            AR_CustClass aR_CustClass = new AR_CustClass();
            aR_CustClass.ClassId = classId;
            aR_CustClass.TradeDisc = tradeDisc;
            aR_CustClass.LUpd_DateTime = lUpd_DateTime;
            aR_CustClass.LUpd_Prog = lUpd_Prog;
            aR_CustClass.LUpd_User = lUpd_User;
            aR_CustClass.Crtd_DateTime = crtd_DateTime;
            aR_CustClass.Crtd_Prog = crtd_Prog;
            aR_CustClass.Crtd_User = crtd_User;
            aR_CustClass.tstamp = tstamp;
            return aR_CustClass;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String ClassId
        {
            get
            {
                return _ClassId;
            }
            set
            {
                if (_ClassId != value)
                {
                    OnClassIdChanging(value);
                    ReportPropertyChanging("ClassId");
                    _ClassId = StructuralObject.SetValidValue(value, false);
                    ReportPropertyChanged("ClassId");
                    OnClassIdChanged();
                }
            }
        }
        private global::System.String _ClassId;
        partial void OnClassIdChanging(global::System.String value);
        partial void OnClassIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String City
        {
            get
            {
                return _City;
            }
            set
            {
                OnCityChanging(value);
                ReportPropertyChanging("City");
                _City = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("City");
                OnCityChanged();
            }
        }
        private global::System.String _City;
        partial void OnCityChanging(global::System.String value);
        partial void OnCityChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Country
        {
            get
            {
                return _Country;
            }
            set
            {
                OnCountryChanging(value);
                ReportPropertyChanging("Country");
                _Country = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Country");
                OnCountryChanged();
            }
        }
        private global::System.String _Country;
        partial void OnCountryChanging(global::System.String value);
        partial void OnCountryChanged();
    
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
        public global::System.String District
        {
            get
            {
                return _District;
            }
            set
            {
                OnDistrictChanging(value);
                ReportPropertyChanging("District");
                _District = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("District");
                OnDistrictChanged();
            }
        }
        private global::System.String _District;
        partial void OnDistrictChanging(global::System.String value);
        partial void OnDistrictChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String PriceClass
        {
            get
            {
                return _PriceClass;
            }
            set
            {
                OnPriceClassChanging(value);
                ReportPropertyChanging("PriceClass");
                _PriceClass = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("PriceClass");
                OnPriceClassChanged();
            }
        }
        private global::System.String _PriceClass;
        partial void OnPriceClassChanging(global::System.String value);
        partial void OnPriceClassChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String State
        {
            get
            {
                return _State;
            }
            set
            {
                OnStateChanging(value);
                ReportPropertyChanging("State");
                _State = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("State");
                OnStateChanged();
            }
        }
        private global::System.String _State;
        partial void OnStateChanging(global::System.String value);
        partial void OnStateChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Terms
        {
            get
            {
                return _Terms;
            }
            set
            {
                OnTermsChanging(value);
                ReportPropertyChanging("Terms");
                _Terms = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Terms");
                OnTermsChanged();
            }
        }
        private global::System.String _Terms;
        partial void OnTermsChanging(global::System.String value);
        partial void OnTermsChanged();
    
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
        public global::System.Double TradeDisc
        {
            get
            {
                return _TradeDisc;
            }
            set
            {
                OnTradeDiscChanging(value);
                ReportPropertyChanging("TradeDisc");
                _TradeDisc = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("TradeDisc");
                OnTradeDiscChanged();
            }
        }
        private global::System.Double _TradeDisc;
        partial void OnTradeDiscChanging(global::System.Double value);
        partial void OnTradeDiscChanged();
    
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
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String TaxDflt
        {
            get
            {
                return _TaxDflt;
            }
            set
            {
                OnTaxDfltChanging(value);
                ReportPropertyChanging("TaxDflt");
                _TaxDflt = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("TaxDflt");
                OnTaxDfltChanged();
            }
        }
        private global::System.String _TaxDflt;
        partial void OnTaxDfltChanging(global::System.String value);
        partial void OnTaxDfltChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String TaxID00
        {
            get
            {
                return _TaxID00;
            }
            set
            {
                OnTaxID00Changing(value);
                ReportPropertyChanging("TaxID00");
                _TaxID00 = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("TaxID00");
                OnTaxID00Changed();
            }
        }
        private global::System.String _TaxID00;
        partial void OnTaxID00Changing(global::System.String value);
        partial void OnTaxID00Changed();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String TaxID01
        {
            get
            {
                return _TaxID01;
            }
            set
            {
                OnTaxID01Changing(value);
                ReportPropertyChanging("TaxID01");
                _TaxID01 = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("TaxID01");
                OnTaxID01Changed();
            }
        }
        private global::System.String _TaxID01;
        partial void OnTaxID01Changing(global::System.String value);
        partial void OnTaxID01Changed();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String TaxID02
        {
            get
            {
                return _TaxID02;
            }
            set
            {
                OnTaxID02Changing(value);
                ReportPropertyChanging("TaxID02");
                _TaxID02 = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("TaxID02");
                OnTaxID02Changed();
            }
        }
        private global::System.String _TaxID02;
        partial void OnTaxID02Changing(global::System.String value);
        partial void OnTaxID02Changed();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String TaxID03
        {
            get
            {
                return _TaxID03;
            }
            set
            {
                OnTaxID03Changing(value);
                ReportPropertyChanging("TaxID03");
                _TaxID03 = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("TaxID03");
                OnTaxID03Changed();
            }
        }
        private global::System.String _TaxID03;
        partial void OnTaxID03Changing(global::System.String value);
        partial void OnTaxID03Changed();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String PrefixCustID
        {
            get
            {
                return _PrefixCustID;
            }
            set
            {
                OnPrefixCustIDChanging(value);
                ReportPropertyChanging("PrefixCustID");
                _PrefixCustID = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("PrefixCustID");
                OnPrefixCustIDChanged();
            }
        }
        private global::System.String _PrefixCustID;
        partial void OnPrefixCustIDChanging(global::System.String value);
        partial void OnPrefixCustIDChanged();

        #endregion

    
    }

    #endregion

    #region ComplexTypes
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmComplexTypeAttribute(NamespaceName="AR20100Model", Name="AR20100_ppAR_CustClass_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class AR20100_ppAR_CustClass_Result : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new AR20100_ppAR_CustClass_Result object.
        /// </summary>
        /// <param name="classId">Initial value of the ClassId property.</param>
        /// <param name="tradeDisc">Initial value of the TradeDisc property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static AR20100_ppAR_CustClass_Result CreateAR20100_ppAR_CustClass_Result(global::System.String classId, global::System.Double tradeDisc, global::System.Byte[] tstamp)
        {
            AR20100_ppAR_CustClass_Result aR20100_ppAR_CustClass_Result = new AR20100_ppAR_CustClass_Result();
            aR20100_ppAR_CustClass_Result.ClassId = classId;
            aR20100_ppAR_CustClass_Result.TradeDisc = tradeDisc;
            aR20100_ppAR_CustClass_Result.tstamp = tstamp;
            return aR20100_ppAR_CustClass_Result;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String ClassId
        {
            get
            {
                return _ClassId;
            }
            set
            {
                OnClassIdChanging(value);
                ReportPropertyChanging("ClassId");
                _ClassId = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("ClassId");
                OnClassIdChanged();
            }
        }
        private global::System.String _ClassId;
        partial void OnClassIdChanging(global::System.String value);
        partial void OnClassIdChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String City
        {
            get
            {
                return _City;
            }
            set
            {
                OnCityChanging(value);
                ReportPropertyChanging("City");
                _City = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("City");
                OnCityChanged();
            }
        }
        private global::System.String _City;
        partial void OnCityChanging(global::System.String value);
        partial void OnCityChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Country
        {
            get
            {
                return _Country;
            }
            set
            {
                OnCountryChanging(value);
                ReportPropertyChanging("Country");
                _Country = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Country");
                OnCountryChanged();
            }
        }
        private global::System.String _Country;
        partial void OnCountryChanging(global::System.String value);
        partial void OnCountryChanged();
    
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
        public global::System.String District
        {
            get
            {
                return _District;
            }
            set
            {
                OnDistrictChanging(value);
                ReportPropertyChanging("District");
                _District = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("District");
                OnDistrictChanged();
            }
        }
        private global::System.String _District;
        partial void OnDistrictChanging(global::System.String value);
        partial void OnDistrictChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String PriceClass
        {
            get
            {
                return _PriceClass;
            }
            set
            {
                OnPriceClassChanging(value);
                ReportPropertyChanging("PriceClass");
                _PriceClass = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("PriceClass");
                OnPriceClassChanged();
            }
        }
        private global::System.String _PriceClass;
        partial void OnPriceClassChanging(global::System.String value);
        partial void OnPriceClassChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String State
        {
            get
            {
                return _State;
            }
            set
            {
                OnStateChanging(value);
                ReportPropertyChanging("State");
                _State = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("State");
                OnStateChanged();
            }
        }
        private global::System.String _State;
        partial void OnStateChanging(global::System.String value);
        partial void OnStateChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Terms
        {
            get
            {
                return _Terms;
            }
            set
            {
                OnTermsChanging(value);
                ReportPropertyChanging("Terms");
                _Terms = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Terms");
                OnTermsChanged();
            }
        }
        private global::System.String _Terms;
        partial void OnTermsChanging(global::System.String value);
        partial void OnTermsChanged();
    
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
        public global::System.Double TradeDisc
        {
            get
            {
                return _TradeDisc;
            }
            set
            {
                OnTradeDiscChanging(value);
                ReportPropertyChanging("TradeDisc");
                _TradeDisc = StructuralObject.SetValidValue(value);
                ReportPropertyChanged("TradeDisc");
                OnTradeDiscChanged();
            }
        }
        private global::System.Double _TradeDisc;
        partial void OnTradeDiscChanging(global::System.Double value);
        partial void OnTradeDiscChanged();
    
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
        public global::System.String TaxDflt
        {
            get
            {
                return _TaxDflt;
            }
            set
            {
                OnTaxDfltChanging(value);
                ReportPropertyChanging("TaxDflt");
                _TaxDflt = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("TaxDflt");
                OnTaxDfltChanged();
            }
        }
        private global::System.String _TaxDflt;
        partial void OnTaxDfltChanging(global::System.String value);
        partial void OnTaxDfltChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String TaxID00
        {
            get
            {
                return _TaxID00;
            }
            set
            {
                OnTaxID00Changing(value);
                ReportPropertyChanging("TaxID00");
                _TaxID00 = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("TaxID00");
                OnTaxID00Changed();
            }
        }
        private global::System.String _TaxID00;
        partial void OnTaxID00Changing(global::System.String value);
        partial void OnTaxID00Changed();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String TaxID01
        {
            get
            {
                return _TaxID01;
            }
            set
            {
                OnTaxID01Changing(value);
                ReportPropertyChanging("TaxID01");
                _TaxID01 = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("TaxID01");
                OnTaxID01Changed();
            }
        }
        private global::System.String _TaxID01;
        partial void OnTaxID01Changing(global::System.String value);
        partial void OnTaxID01Changed();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String TaxID02
        {
            get
            {
                return _TaxID02;
            }
            set
            {
                OnTaxID02Changing(value);
                ReportPropertyChanging("TaxID02");
                _TaxID02 = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("TaxID02");
                OnTaxID02Changed();
            }
        }
        private global::System.String _TaxID02;
        partial void OnTaxID02Changing(global::System.String value);
        partial void OnTaxID02Changed();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String TaxID03
        {
            get
            {
                return _TaxID03;
            }
            set
            {
                OnTaxID03Changing(value);
                ReportPropertyChanging("TaxID03");
                _TaxID03 = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("TaxID03");
                OnTaxID03Changed();
            }
        }
        private global::System.String _TaxID03;
        partial void OnTaxID03Changing(global::System.String value);
        partial void OnTaxID03Changed();

        #endregion

    }

    #endregion

    
}
