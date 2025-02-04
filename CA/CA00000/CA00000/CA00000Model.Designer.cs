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
namespace CA00000
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class CA00000Entities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new CA00000Entities object using the connection string found in the 'CA00000Entities' section of the application configuration file.
        /// </summary>
        public CA00000Entities() : base("name=CA00000Entities", "CA00000Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new CA00000Entities object.
        /// </summary>
        public CA00000Entities(string connectionString) : base(connectionString, "CA00000Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new CA00000Entities object.
        /// </summary>
        public CA00000Entities(EntityConnection connection) : base(connection, "CA00000Entities")
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
        public ObjectSet<CA_Setup> CA_Setup
        {
            get
            {
                if ((_CA_Setup == null))
                {
                    _CA_Setup = base.CreateObjectSet<CA_Setup>("CA_Setup");
                }
                return _CA_Setup;
            }
        }
        private ObjectSet<CA_Setup> _CA_Setup;

        #endregion

        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the CA_Setup EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToCA_Setup(CA_Setup cA_Setup)
        {
            base.AddObject("CA_Setup", cA_Setup);
        }

        #endregion

        #region Function Imports
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        public ObjectResult<CA00000_pdLoadSetup_Result> CA00000_pdLoadSetup()
        {
            return base.ExecuteFunction<CA00000_pdLoadSetup_Result>("CA00000_pdLoadSetup");
        }

        #endregion

    }

    #endregion

    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="CA00000Model", Name="CA_Setup")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class CA_Setup : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new CA_Setup object.
        /// </summary>
        /// <param name="branchID">Initial value of the BranchID property.</param>
        /// <param name="setUpID">Initial value of the SetUpID property.</param>
        /// <param name="crtd_DateTime">Initial value of the Crtd_DateTime property.</param>
        /// <param name="crtd_Prog">Initial value of the Crtd_Prog property.</param>
        /// <param name="crtd_User">Initial value of the Crtd_User property.</param>
        /// <param name="lUpd_DateTime">Initial value of the LUpd_DateTime property.</param>
        /// <param name="lUpd_Prog">Initial value of the LUpd_Prog property.</param>
        /// <param name="lUpd_User">Initial value of the LUpd_User property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static CA_Setup CreateCA_Setup(global::System.String branchID, global::System.String setUpID, global::System.DateTime crtd_DateTime, global::System.String crtd_Prog, global::System.String crtd_User, global::System.DateTime lUpd_DateTime, global::System.String lUpd_Prog, global::System.String lUpd_User, global::System.Byte[] tstamp)
        {
            CA_Setup cA_Setup = new CA_Setup();
            cA_Setup.BranchID = branchID;
            cA_Setup.SetUpID = setUpID;
            cA_Setup.Crtd_DateTime = crtd_DateTime;
            cA_Setup.Crtd_Prog = crtd_Prog;
            cA_Setup.Crtd_User = crtd_User;
            cA_Setup.LUpd_DateTime = lUpd_DateTime;
            cA_Setup.LUpd_Prog = lUpd_Prog;
            cA_Setup.LUpd_User = lUpd_User;
            cA_Setup.tstamp = tstamp;
            return cA_Setup;
        }

        #endregion

        #region Simple Properties
    
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
                    _BranchID = StructuralObject.SetValidValue(value, false, "BranchID");
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
        public global::System.String SetUpID
        {
            get
            {
                return _SetUpID;
            }
            set
            {
                if (_SetUpID != value)
                {
                    OnSetUpIDChanging(value);
                    ReportPropertyChanging("SetUpID");
                    _SetUpID = StructuralObject.SetValidValue(value, false, "SetUpID");
                    ReportPropertyChanged("SetUpID");
                    OnSetUpIDChanged();
                }
            }
        }
        private global::System.String _SetUpID;
        partial void OnSetUpIDChanging(global::System.String value);
        partial void OnSetUpIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String LastBatNbr
        {
            get
            {
                return _LastBatNbr;
            }
            set
            {
                OnLastBatNbrChanging(value);
                ReportPropertyChanging("LastBatNbr");
                _LastBatNbr = StructuralObject.SetValidValue(value, true, "LastBatNbr");
                ReportPropertyChanged("LastBatNbr");
                OnLastBatNbrChanged();
            }
        }
        private global::System.String _LastBatNbr;
        partial void OnLastBatNbrChanging(global::System.String value);
        partial void OnLastBatNbrChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String LastPaymentNbr
        {
            get
            {
                return _LastPaymentNbr;
            }
            set
            {
                OnLastPaymentNbrChanging(value);
                ReportPropertyChanging("LastPaymentNbr");
                _LastPaymentNbr = StructuralObject.SetValidValue(value, true, "LastPaymentNbr");
                ReportPropertyChanged("LastPaymentNbr");
                OnLastPaymentNbrChanged();
            }
        }
        private global::System.String _LastPaymentNbr;
        partial void OnLastPaymentNbrChanging(global::System.String value);
        partial void OnLastPaymentNbrChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String LastReceiptNbr
        {
            get
            {
                return _LastReceiptNbr;
            }
            set
            {
                OnLastReceiptNbrChanging(value);
                ReportPropertyChanging("LastReceiptNbr");
                _LastReceiptNbr = StructuralObject.SetValidValue(value, true, "LastReceiptNbr");
                ReportPropertyChanged("LastReceiptNbr");
                OnLastReceiptNbrChanged();
            }
        }
        private global::System.String _LastReceiptNbr;
        partial void OnLastReceiptNbrChanging(global::System.String value);
        partial void OnLastReceiptNbrChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String PreFixBat
        {
            get
            {
                return _PreFixBat;
            }
            set
            {
                OnPreFixBatChanging(value);
                ReportPropertyChanging("PreFixBat");
                _PreFixBat = StructuralObject.SetValidValue(value, true, "PreFixBat");
                ReportPropertyChanged("PreFixBat");
                OnPreFixBatChanged();
            }
        }
        private global::System.String _PreFixBat;
        partial void OnPreFixBatChanging(global::System.String value);
        partial void OnPreFixBatChanged();
    
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
                _Crtd_DateTime = StructuralObject.SetValidValue(value, "Crtd_DateTime");
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
                _Crtd_Prog = StructuralObject.SetValidValue(value, false, "Crtd_Prog");
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
                _Crtd_User = StructuralObject.SetValidValue(value, false, "Crtd_User");
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
                _LUpd_DateTime = StructuralObject.SetValidValue(value, "LUpd_DateTime");
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
                _LUpd_Prog = StructuralObject.SetValidValue(value, false, "LUpd_Prog");
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
                _LUpd_User = StructuralObject.SetValidValue(value, false, "LUpd_User");
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
                _tstamp = StructuralObject.SetValidValue(value, true, "tstamp");
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
    [EdmComplexTypeAttribute(NamespaceName="CA00000Model", Name="CA00000_pdLoadSetup_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class CA00000_pdLoadSetup_Result : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new CA00000_pdLoadSetup_Result object.
        /// </summary>
        /// <param name="branchID">Initial value of the BranchID property.</param>
        /// <param name="setUpID">Initial value of the SetUpID property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static CA00000_pdLoadSetup_Result CreateCA00000_pdLoadSetup_Result(global::System.String branchID, global::System.String setUpID, global::System.Byte[] tstamp)
        {
            CA00000_pdLoadSetup_Result cA00000_pdLoadSetup_Result = new CA00000_pdLoadSetup_Result();
            cA00000_pdLoadSetup_Result.BranchID = branchID;
            cA00000_pdLoadSetup_Result.SetUpID = setUpID;
            cA00000_pdLoadSetup_Result.tstamp = tstamp;
            return cA00000_pdLoadSetup_Result;
        }

        #endregion

        #region Simple Properties
    
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
                _BranchID = StructuralObject.SetValidValue(value, false, "BranchID");
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
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String SetUpID
        {
            get
            {
                return _SetUpID;
            }
            set
            {
                OnSetUpIDChanging(value);
                ReportPropertyChanging("SetUpID");
                _SetUpID = StructuralObject.SetValidValue(value, false, "SetUpID");
                ReportPropertyChanged("SetUpID");
                OnSetUpIDChanged();
            }
        }
        private global::System.String _SetUpID;
        partial void OnSetUpIDChanging(global::System.String value);
        partial void OnSetUpIDChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String LastBatNbr
        {
            get
            {
                return _LastBatNbr;
            }
            set
            {
                OnLastBatNbrChanging(value);
                ReportPropertyChanging("LastBatNbr");
                _LastBatNbr = StructuralObject.SetValidValue(value, true, "LastBatNbr");
                ReportPropertyChanged("LastBatNbr");
                OnLastBatNbrChanged();
            }
        }
        private global::System.String _LastBatNbr;
        partial void OnLastBatNbrChanging(global::System.String value);
        partial void OnLastBatNbrChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String LastPaymentNbr
        {
            get
            {
                return _LastPaymentNbr;
            }
            set
            {
                OnLastPaymentNbrChanging(value);
                ReportPropertyChanging("LastPaymentNbr");
                _LastPaymentNbr = StructuralObject.SetValidValue(value, true, "LastPaymentNbr");
                ReportPropertyChanged("LastPaymentNbr");
                OnLastPaymentNbrChanged();
            }
        }
        private global::System.String _LastPaymentNbr;
        partial void OnLastPaymentNbrChanging(global::System.String value);
        partial void OnLastPaymentNbrChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String LastReceiptNbr
        {
            get
            {
                return _LastReceiptNbr;
            }
            set
            {
                OnLastReceiptNbrChanging(value);
                ReportPropertyChanging("LastReceiptNbr");
                _LastReceiptNbr = StructuralObject.SetValidValue(value, true, "LastReceiptNbr");
                ReportPropertyChanged("LastReceiptNbr");
                OnLastReceiptNbrChanged();
            }
        }
        private global::System.String _LastReceiptNbr;
        partial void OnLastReceiptNbrChanging(global::System.String value);
        partial void OnLastReceiptNbrChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String PreFixBat
        {
            get
            {
                return _PreFixBat;
            }
            set
            {
                OnPreFixBatChanging(value);
                ReportPropertyChanging("PreFixBat");
                _PreFixBat = StructuralObject.SetValidValue(value, true, "PreFixBat");
                ReportPropertyChanged("PreFixBat");
                OnPreFixBatChanged();
            }
        }
        private global::System.String _PreFixBat;
        partial void OnPreFixBatChanging(global::System.String value);
        partial void OnPreFixBatChanged();
    
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
                _tstamp = StructuralObject.SetValidValue(value, false, "tstamp");
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
