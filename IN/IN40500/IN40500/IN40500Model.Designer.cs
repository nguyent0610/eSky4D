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
namespace IN40500
{
    #region Contexts
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    public partial class IN40500Entities : ObjectContext
    {
        #region Constructors
    
        /// <summary>
        /// Initializes a new IN40500Entities object using the connection string found in the 'IN40500Entities' section of the application configuration file.
        /// </summary>
        public IN40500Entities() : base("name=IN40500Entities", "IN40500Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new IN40500Entities object.
        /// </summary>
        public IN40500Entities(string connectionString) : base(connectionString, "IN40500Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        /// <summary>
        /// Initialize a new IN40500Entities object.
        /// </summary>
        public IN40500Entities(EntityConnection connection) : base(connection, "IN40500Entities")
        {
            this.ContextOptions.LazyLoadingEnabled = true;
            OnContextCreated();
        }
    
        #endregion
    
        #region Partial Methods
    
        partial void OnContextCreated();
    
        #endregion
    
        #region Function Imports
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name="branchID">No Metadata Documentation available.</param>
        /// <param name="siteID">No Metadata Documentation available.</param>
        public ObjectResult<IN40500_ppCheckCreateIN_Tag_Result> IN40500_ppCheckCreateIN_Tag(global::System.String branchID, global::System.String siteID)
        {
            ObjectParameter branchIDParameter;
            if (branchID != null)
            {
                branchIDParameter = new ObjectParameter("BranchID", branchID);
            }
            else
            {
                branchIDParameter = new ObjectParameter("BranchID", typeof(global::System.String));
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
    
            return base.ExecuteFunction<IN40500_ppCheckCreateIN_Tag_Result>("IN40500_ppCheckCreateIN_Tag", branchIDParameter, siteIDParameter);
        }
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        /// <param name="userID">No Metadata Documentation available.</param>
        /// <param name="branchID">No Metadata Documentation available.</param>
        /// <param name="descr">No Metadata Documentation available.</param>
        /// <param name="transDate">No Metadata Documentation available.</param>
        /// <param name="siteID">No Metadata Documentation available.</param>
        /// <param name="classID">No Metadata Documentation available.</param>
        public ObjectResult<IN40500_ppGetInsertIN_TagDetail_Result> IN40500_ppGetInsertIN_TagDetail(global::System.String userID, global::System.String branchID, global::System.String descr, Nullable<global::System.DateTime> transDate, global::System.String siteID, global::System.String classID)
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
    
            ObjectParameter branchIDParameter;
            if (branchID != null)
            {
                branchIDParameter = new ObjectParameter("BranchID", branchID);
            }
            else
            {
                branchIDParameter = new ObjectParameter("BranchID", typeof(global::System.String));
            }
    
            ObjectParameter descrParameter;
            if (descr != null)
            {
                descrParameter = new ObjectParameter("Descr", descr);
            }
            else
            {
                descrParameter = new ObjectParameter("Descr", typeof(global::System.String));
            }
    
            ObjectParameter transDateParameter;
            if (transDate.HasValue)
            {
                transDateParameter = new ObjectParameter("TransDate", transDate);
            }
            else
            {
                transDateParameter = new ObjectParameter("TransDate", typeof(global::System.DateTime));
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
    
            ObjectParameter classIDParameter;
            if (classID != null)
            {
                classIDParameter = new ObjectParameter("ClassID", classID);
            }
            else
            {
                classIDParameter = new ObjectParameter("ClassID", typeof(global::System.String));
            }
    
            return base.ExecuteFunction<IN40500_ppGetInsertIN_TagDetail_Result>("IN40500_ppGetInsertIN_TagDetail", userIDParameter, branchIDParameter, descrParameter, transDateParameter, siteIDParameter, classIDParameter);
        }

        #endregion

    }

    #endregion

    #region ComplexTypes
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmComplexTypeAttribute(NamespaceName="IN40500Model", Name="IN40500_ppCheckCreateIN_Tag_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class IN40500_ppCheckCreateIN_Tag_Result : ComplexObject
    {
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Result
        {
            get
            {
                return _Result;
            }
            set
            {
                OnResultChanging(value);
                ReportPropertyChanging("Result");
                _Result = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Result");
                OnResultChanged();
            }
        }
        private global::System.String _Result;
        partial void OnResultChanging(global::System.String value);
        partial void OnResultChanged();

        #endregion

    }
    
    /// <summary>
    /// No Metadata Documentation available.
    /// </summary>
    [EdmComplexTypeAttribute(NamespaceName="IN40500Model", Name="IN40500_ppGetInsertIN_TagDetail_Result")]
    [DataContractAttribute(IsReference=true)]
    [Serializable()]
    public partial class IN40500_ppGetInsertIN_TagDetail_Result : ComplexObject
    {
        #region Primitive Properties
    
        /// <summary>
        /// No Metadata Documentation available.
        /// </summary>
        [EdmScalarPropertyAttribute(EntityKeyProperty=false, IsNullable=true)]
        [DataMemberAttribute()]
        public global::System.String Result
        {
            get
            {
                return _Result;
            }
            set
            {
                OnResultChanging(value);
                ReportPropertyChanging("Result");
                _Result = StructuralObject.SetValidValue(value, true);
                ReportPropertyChanged("Result");
                OnResultChanged();
            }
        }
        private global::System.String _Result;
        partial void OnResultChanging(global::System.String value);
        partial void OnResultChanged();

        #endregion

    }

    #endregion

    
}
