using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Data.Decorators;
using Telerik.Sitefinity.Model;

namespace timw255.Sitefinity.ImageOptimization.Data.EntityFramework.Decorators
{
    public class ImageOptimizationEFDataProviderDecorator : IDataProviderDecorator, IExecutionStateDecorator
    {
        #region IExecutionStateDecorator
        public void ClearExecutionStateBag()
        {
            //do nothing here
        }

        public object GetExecutionStateData(string stateBagKey)
        {
            //do nothing here
            return new object();
        }

        public void SetExecutionStateData(string stateBagKey, object data)
        {
            //do nothing here
        }
        #endregion

        #region IDataProviderDecorator
        public DataProviderBase DataProvider { get; set; }

        public void Initialize(string providerName, NameValueCollection config, Type managerType)
        {
            this.connectionName = config["connectionName"];

            if (string.IsNullOrEmpty(this.connectionName))
                this.connectionName = "Sitefinity"; /* the default connectionName */

            var provider = this.DataProvider as IImageOptimizationEFDataProvider;

            if (provider != null)
            {
                ImageOptimizationEFDataConnection connection = ImageOptimizationEFDataConnection.InitializeConnection(this.connectionName, provider);

                provider.ProviderContext = new ImageOptimizationEFDataProviderContext()
                {
                    ProviderKey = Guid.NewGuid().ToString(),
                    ConnectionId = connection.Name
                };

                this.connectionName = connection.Name;
            }
        }

        public object CreateNewTransaction(string transactionName)
        {
            var provider = (IImageOptimizationEFDataProvider)this.DataProvider;

            if (string.IsNullOrEmpty(transactionName))
                return this.GetContext(provider);

            string id = GetScopeId(provider);
            var scope = TransactionManager.GetTransaction<object>(transactionName, id);

            if (scope == null)
            {
                var context = this.GetContext(provider);
                context.BeginTransaction();
                TransactionManager.AddTransaction(transactionName, id, this, context);
                scope = context;
            }

            return scope;
        }

        public void CommitTransaction()
        {
            var dataProvider = (IImageOptimizationEFDataProvider)this.DataProvider;
            var context = dataProvider.Context;
            context.SaveChanges();
            context.CommitTransaction();
        }

        public void FlushTransaction()
        {
            //EF don't support flush by default
        }

        public void RollbackTransaction()
        {
            var dataProvider = (IImageOptimizationEFDataProvider)this.DataProvider;
            dataProvider.Context.RollbackTransaction();
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #region Not Implemented methods
        public T GetOriginalValue<T>(object entity, string propertyName)
        {
            throw new NotImplementedException();
        }

        public void AddPermissionToObject(Telerik.Sitefinity.Security.Model.ISecuredObject securedObject, Telerik.Sitefinity.Data.IManager managerInstance, Telerik.Sitefinity.Security.Model.Permission permission, string transactionName)
        {
            throw new NotImplementedException();
        }

        public void AddPermissionToObject(Telerik.Sitefinity.Security.Model.ISecuredObject securedObject, Telerik.Sitefinity.Security.Model.Permission permission, string transactionName)
        {
            throw new NotImplementedException();
        }

        public string ConvertClrTypeVoaClass(Type clrType)
        {
            throw new NotImplementedException();
        }

        public Type ConvertVoaClassToClrType(string voaClassName)
        {
            throw new NotImplementedException();
        }

        public ManagerInfo CreateManagerInfo(Guid id)
        {
            throw new NotImplementedException();
        }

        public ManagerInfo CreateManagerInfo()
        {
            throw new NotImplementedException();
        }

        public Telerik.Sitefinity.Security.Model.Permission CreatePermission(string permissionSet, Guid objectId, Guid principalId)
        {
            throw new NotImplementedException();
        }

        public void CreatePermissionInheritanceAssociation(Telerik.Sitefinity.Security.Model.ISecuredObject parent, Telerik.Sitefinity.Security.Model.ISecuredObject child)
        {
            throw new NotImplementedException();
        }

        public Telerik.Sitefinity.Security.Model.PermissionsInheritanceMap CreatePermissionsInheritanceMap(Guid objectId, Guid childObjectId, string childObjectTypeName)
        {
            throw new NotImplementedException();
        }

        public T CreateProxy<T>(T dataItem, string listName, string fetchGroup) where T : Telerik.Sitefinity.Model.IDataItem
        {
            throw new NotImplementedException();
        }

        public bool IsFieldDirty(object entity, string fieldName)
        {
            throw new NotImplementedException();
        }

        public T CreateProxy<T>(T dataItem) where T : Telerik.Sitefinity.Model.IDataItem
        {
            throw new NotImplementedException();
        }

        public void DeleteManagerInfo(Telerik.Sitefinity.Model.ManagerInfo info)
        {
            throw new NotImplementedException();
        }

        public void DeletePermission(Telerik.Sitefinity.Security.Model.Permission permission)
        {
            throw new NotImplementedException();
        }

        public void DeletePermissions(object securedObject)
        {
            throw new NotImplementedException();
        }

        public void DeletePermissionsInheritanceAssociation(Telerik.Sitefinity.Security.Model.ISecuredObject parent, Telerik.Sitefinity.Security.Model.ISecuredObject child)
        {
            throw new NotImplementedException();
        }

        public void DeletePermissionsInheritanceAssociation(Telerik.Sitefinity.Security.Model.ISecuredObject parent, Telerik.Sitefinity.Security.Model.ISecuredObject child, bool removeAllChildReferences)
        {
            // This method is required for Sitefinity versions below 5.3.3900.0
            this.DeletePermissionsInheritanceAssociation(parent, child);
        }

        public void DeletePermissionsInheritanceMap(Telerik.Sitefinity.Security.Model.PermissionsInheritanceMap permissionsInheritanceMap)
        {
            throw new NotImplementedException();
        }

        public int GetClassId(Type type)
        {
            throw new NotImplementedException();
        }

        public Telerik.Sitefinity.Security.SecurityConstants.TransactionActionType GetDirtyItemStatus(object itemInTransaction)
        {
            throw new NotImplementedException();
        }

        public System.Collections.IList GetDirtyItems()
        {
            throw new NotImplementedException();
        }

        public IQueryable<Telerik.Sitefinity.Security.Model.PermissionsInheritanceMap> GetInheritanceMaps()
        {
            throw new NotImplementedException();
        }

        public Telerik.Sitefinity.Model.ManagerInfo GetManagerInfo(string managerType, string providerName)
        {
            throw new NotImplementedException();
        }

        public Telerik.Sitefinity.Model.ManagerInfo GetManagerInfo(Guid id)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Telerik.Sitefinity.Model.ManagerInfo> GetManagerInfos()
        {
            throw new NotImplementedException();
        }

        public Guid GetNewGuid()
        {
            throw new NotImplementedException();
        }

        public Telerik.Sitefinity.Security.Model.Permission GetPermission(Guid permissionId)
        {
            throw new NotImplementedException();
        }

        public Telerik.Sitefinity.Security.Model.Permission GetPermission(string permissionSet, Guid objectId, Guid principalId)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Telerik.Sitefinity.Security.Model.PermissionsInheritanceMap> GetPermissionChildren(Guid parentId)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Telerik.Sitefinity.Security.Model.Permission> GetPermissions(Type actualType)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TPermission> GetPermissions<TPermission>() where TPermission : Telerik.Sitefinity.Security.Model.Permission
        {
            throw new NotImplementedException();
        }

        public IQueryable<Telerik.Sitefinity.Security.Model.Permission> GetPermissions()
        {
            throw new NotImplementedException();
        }

        public T GetProxy<T>(Guid id) where T : Telerik.Sitefinity.Model.IDataItem
        {
            throw new NotImplementedException();
        }

        public Telerik.Sitefinity.Security.Model.SecurityRoot GetSecurityRoot(bool create, IDictionary<string, string> permissionsetObjectTitleResKeys, params string[] permissionSets)
        {
            throw new NotImplementedException();
        }

        public Telerik.Sitefinity.Security.Model.SecurityRoot GetSecurityRoot(bool create)
        {
            throw new NotImplementedException();
        }

        public Telerik.Sitefinity.Security.Model.SecurityRoot GetSecurityRoot()
        {
            throw new NotImplementedException();
        }

        public void LockTransactionForRead(object target)
        {
            throw new NotImplementedException();
        }

        public void LockTransactionForWrite(object target)
        {
            throw new NotImplementedException();
        }

        public bool Suspended
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler<Telerik.Sitefinity.Data.ExecutedEventArgs> Executed;

        public event EventHandler<Telerik.Sitefinity.Data.ExecutingEventArgs> Executing;

        public void OnExecuted(Telerik.Sitefinity.Data.ExecutedEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void OnExecuting(Telerik.Sitefinity.Data.ExecutingEventArgs args)
        {
            throw new NotImplementedException();
        }
        #endregion
        #endregion

        #region Private methods
        /// <summary>
        /// Gets the entity framework context.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        private ImageOptimizationEFDbContext GetContext(IImageOptimizationEFDataProvider provider)
        {
            if (provider.ProviderContext == null)
                throw new Exception(String.Format("Provider {0} is not initialized.", provider.Name));

            return ImageOptimizationEFDataConnection.GetContext(this.connectionName, provider);
        }

        /// <summary>
        /// Gets the scope id.
        /// </summary>
        /// <param name="provider">The data provider.</param>
        /// <returns></returns>
        private string GetScopeId(IImageOptimizationEFDataProvider provider)
        {
            return provider.ProviderContext.ProviderKey;
        }
        #endregion

        #region Private members
        private string connectionName;
        #endregion
    }
}