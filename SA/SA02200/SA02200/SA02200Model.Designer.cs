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
namespace SA02200
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class SA02200Entities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new SA02200Entities object using the connection string found in the 'SA02200Entities' section of the application configuration file.
        /// </summary>
        public SA02200Entities() : base("name=SA02200Entities", "SA02200Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new SA02200Entities object.
        /// </summary>
        public SA02200Entities(string connectionString) : base(connectionString, "SA02200Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new SA02200Entities object.
        /// </summary>
        public SA02200Entities(EntityConnection connection) : base(connection, "SA02200Entities")
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
        public ObjectSet<SYS_Favourite> SYS_Favourite
        {
            get
            {
                if ((_SYS_Favourite == null))
                {
                    _SYS_Favourite = base.CreateObjectSet<SYS_Favourite>("SYS_Favourite");
                }
                return _SYS_Favourite;
            }
        }
        private ObjectSet<SYS_Favourite> _SYS_Favourite;

        #endregion

        #region AddTo Methods
    
        /// <summary>
        /// Deprecated Method for adding a new object to the SYS_Favourite EntitySet. Consider using the .Add method of the associated ObjectSet&lt;T&gt; property instead.
        /// </summary>
        public void AddToSYS_Favourite(SYS_Favourite sYS_Favourite)
        {
            base.AddObject("SYS_Favourite", sYS_Favourite);
        }

        #endregion

        #region Function Imports
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name="userName">No Metadata Documentation available.</param>
        public ObjectResult<SA02200_pgSYS_Favourite_Result> SA02200_pgSYS_Favourite(global::System.String userName)
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
    
            return base.ExecuteFunction<SA02200_pgSYS_Favourite_Result>("SA02200_pgSYS_Favourite", userNameParameter);
        }

        #endregion

    }

    #endregion

    #region Entities
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmEntityTypeAttribute(NamespaceName="SA02200Model", Name="SYS_Favourite")]
    [Serializable()]
    [DataContractAttribute(IsReference=true)]
    public partial class SYS_Favourite : EntityObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new SYS_Favourite object.
        /// </summary>
        /// <param name="userName">Initial value of the UserName property.</param>
        /// <param name="screenNumber">Initial value of the ScreenNumber property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static SYS_Favourite CreateSYS_Favourite(global::System.String userName, global::System.String screenNumber, global::System.Byte[] tstamp)
        {
            SYS_Favourite sYS_Favourite = new SYS_Favourite();
            sYS_Favourite.UserName = userName;
            sYS_Favourite.ScreenNumber = screenNumber;
            sYS_Favourite.tstamp = tstamp;
            return sYS_Favourite;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String UserName
        {
            get
            {
                return _UserName;
            }
            set
            {
                if (_UserName != value)
                {
                    OnUserNameChanging(value);
                    ReportPropertyChanging("UserName");
                    _UserName = StructuralObject.SetValidValue(value, false);
                    ReportPropertyChanged("UserName");
                    OnUserNameChanged();
                }
            }
        }
        private global::System.String _UserName;
        partial void OnUserNameChanging(global::System.String value);
        partial void OnUserNameChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=true, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String ScreenNumber
        {
            get
            {
                return _ScreenNumber;
            }
            set
            {
                if (_ScreenNumber != value)
                {
                    OnScreenNumberChanging(value);
                    ReportPropertyChanging("ScreenNumber");
                    _ScreenNumber = StructuralObject.SetValidValue(value, false);
                    ReportPropertyChanged("ScreenNumber");
                    OnScreenNumberChanged();
                }
            }
        }
        private global::System.String _ScreenNumber;
        partial void OnScreenNumberChanging(global::System.String value);
        partial void OnScreenNumberChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.DateTime> Crtd_DateTime
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
        private Nullable<global::System.DateTime> _Crtd_DateTime;
        partial void OnCrtd_DateTimeChanging(Nullable<global::System.DateTime> value);
        partial void OnCrtd_DateTimeChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
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
                _Crtd_Prog = StructuralObject.SetValidValue(value, true);
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
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
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
                _Crtd_User = StructuralObject.SetValidValue(value, true);
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
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public Nullable<global::System.DateTime> LUpd_DateTime
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
        private Nullable<global::System.DateTime> _LUpd_DateTime;
        partial void OnLUpd_DateTimeChanging(Nullable<global::System.DateTime> value);
        partial void OnLUpd_DateTimeChanged();
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
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
                _LUpd_Prog = StructuralObject.SetValidValue(value, true);
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
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
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
                _LUpd_User = StructuralObject.SetValidValue(value, true);
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
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String CodeGroup
        {
            get
            {
                return _CodeGroup;
            }
            set
            {
                OnCodeGroupChanging(value);
                ReportPropertyChanging("CodeGroup");
                _CodeGroup = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("CodeGroup");
                OnCodeGroupChanged();
            }
        }
        private global::System.String _CodeGroup;
        partial void OnCodeGroupChanging(global::System.String value);
        partial void OnCodeGroupChanged();

        #endregion

    
    }

    #endregion

    #region ComplexTypes
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmComplexTypeAttribute(NamespaceName="SA02200Model", Name="SA02200_pgSYS_Favourite_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class SA02200_pgSYS_Favourite_Result : ComplexObject
    {
        #region Factory Method
    
        /// <summary>
        /// Create a new SA02200_pgSYS_Favourite_Result object.
        /// </summary>
        /// <param name="screenNumber">Initial value of the ScreenNumber property.</param>
        /// <param name="descr">Initial value of the Descr property.</param>
        /// <param name="tstamp">Initial value of the tstamp property.</param>
        public static SA02200_pgSYS_Favourite_Result CreateSA02200_pgSYS_Favourite_Result(global::System.String screenNumber, global::System.String descr, global::System.Byte[] tstamp)
        {
            SA02200_pgSYS_Favourite_Result sA02200_pgSYS_Favourite_Result = new SA02200_pgSYS_Favourite_Result();
            sA02200_pgSYS_Favourite_Result.ScreenNumber = screenNumber;
            sA02200_pgSYS_Favourite_Result.Descr = descr;
            sA02200_pgSYS_Favourite_Result.tstamp = tstamp;
            return sA02200_pgSYS_Favourite_Result;
        }

        #endregion

        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=false)]
        [DataMemberAttribute()]
        public global::System.String ScreenNumber
        {
            get
            {
                return _ScreenNumber;
            }
            set
            {
                OnScreenNumberChanging(value);
                ReportPropertyChanging("ScreenNumber");
                _ScreenNumber = StructuralObject.SetValidValue(value, false);
                ReportPropertyChanged("ScreenNumber");
                OnScreenNumberChanged();
            }
        }
        private global::System.String _ScreenNumber;
        partial void OnScreenNumberChanging(global::System.String value);
        partial void OnScreenNumberChanged();
    
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
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String CodeGroup
        {
            get
            {
                return _CodeGroup;
            }
            set
            {
                OnCodeGroupChanging(value);
                ReportPropertyChanging("CodeGroup");
                _CodeGroup = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("CodeGroup");
                OnCodeGroupChanged();
            }
        }
        private global::System.String _CodeGroup;
        partial void OnCodeGroupChanging(global::System.String value);
        partial void OnCodeGroupChanged();

        #endregion

    }

    #endregion

    
}
