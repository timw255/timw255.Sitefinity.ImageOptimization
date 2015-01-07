using System;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using Telerik.Microsoft.Practices.Unity;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Data.Decorators;
using timw255.Sitefinity.ImageOptimization.Data.EntityFramework.Decorators;
using timw255.Sitefinity.ImageOptimization.Models;


namespace timw255.Sitefinity.ImageOptimization.Data.EntityFramework
{
    public class ImageOptimizationEFDataProvider : ImageOptimizationDataProvider, IImageOptimizationEFDataProvider
    {
        #region ImageOptimizationDataProvider
        protected override void Initialize(string providerName, NameValueCollection config, Type managerType, bool initializeDecorator)
        {
            if (!ObjectFactory.IsTypeRegistered(typeof(ImageOptimizationEFDataProviderDecorator)))
                ObjectFactory.Container.RegisterType<IDataProviderDecorator, ImageOptimizationEFDataProviderDecorator>(typeof(ImageOptimizationEFDataProviderDecorator).FullName);

            base.Initialize(providerName, config, managerType, initializeDecorator);
        }

        public override IQueryable<ImageOptimizationLogEntry> GetImageOptimizationLogEntrys()
        {
            return this.Context.ImageOptimizationLogEntrys.Where(p => p.ApplicationName == this.ApplicationName);
        }

        public override ImageOptimizationLogEntry GetImageOptimizationLogEntry(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id cannot be Empty Guid");

            return this.Context.ImageOptimizationLogEntrys.Find(id);
        }

        public override ImageOptimizationLogEntry CreateImageOptimizationLogEntry()
        {
            Guid id = Guid.NewGuid();
            var item = new ImageOptimizationLogEntry(id, this.ApplicationName);

            return this.Context.ImageOptimizationLogEntrys.Add(item);
        }

        public override void UpdateImageOptimizationLogEntry(ImageOptimizationLogEntry entity)
        {
            var context = this.Context;

            if (context.Entry(entity).State == EntityState.Detached)
                context.ImageOptimizationLogEntrys.Attach(entity);

            context.Entry(entity).State = EntityState.Modified;
        }

        public override void DeleteImageOptimizationLogEntry(ImageOptimizationLogEntry entity)
        {
            var context = this.Context;

            if (context.Entry(entity).State == EntityState.Detached)
                context.ImageOptimizationLogEntrys.Attach(entity);

            context.ImageOptimizationLogEntrys.Remove(entity);
        }
        #endregion

        #region IImageOptimizationEFDataProvider
        public ImageOptimizationEFDataProviderContext ProviderContext { get; set; }

        public ImageOptimizationEFDbContext Context
        {
            get
            {
                return (ImageOptimizationEFDbContext)this.GetTransaction();
            }
        }
        #endregion
    }
}