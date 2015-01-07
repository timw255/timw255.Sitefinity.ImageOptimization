using System;
using System.Linq;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
using timw255.Sitefinity.ImageOptimization.Configuration;
using timw255.Sitefinity.ImageOptimization.Models;

namespace timw255.Sitefinity.ImageOptimization
{
    public class ImageOptimizationManager : ManagerBase<ImageOptimizationDataProvider>
    {
        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageOptimizationManager" /> class.
        /// </summary>
        public ImageOptimizationManager()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageOptimizationManager" /> class.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        public ImageOptimizationManager(string providerName)
            : base(providerName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageOptimizationManager" /> class.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="transactionName">Name of the transaction.</param>
        public ImageOptimizationManager(string providerName, string transactionName)
            : base(providerName, transactionName)
        {
        }
        #endregion

        #region Public and overriden methods
        /// <summary>
        /// Gets the default provider delegate.
        /// </summary>
        /// <value>The default provider delegate.</value>
        protected override GetDefaultProvider DefaultProviderDelegate
        {
            get
            {
                return () => Config.Get<ImageOptimizationConfig>().DefaultProvider;
            }
        }

        /// <summary>
        /// Gets the name of the module.
        /// </summary>
        /// <value>The name of the module.</value>
        public override string ModuleName
        {
            get
            {
                return ImageOptimizationModule.ModuleName;
            }
        }

        /// <summary>
        /// Gets the providers settings.
        /// </summary>
        /// <value>The providers settings.</value>
        protected override ConfigElementDictionary<string, DataProviderSettings> ProvidersSettings
        {
            get
            {
                return Config.Get<ImageOptimizationConfig>().Providers;
            }
        }

        /// <summary>
        /// Get an instance of the ImageOptimization manager using the default provider.
        /// </summary>
        /// <returns>Instance of the ImageOptimization manager</returns>
        public static ImageOptimizationManager GetManager()
        {
            return ManagerBase<ImageOptimizationDataProvider>.GetManager<ImageOptimizationManager>();
        }

        /// <summary>
        /// Get an instance of the ImageOptimization manager by explicitly specifying the required provider to use
        /// </summary>
        /// <param name="providerName">Name of the provider to use, or null/empty string to use the default provider.</param>
        /// <returns>Instance of the ImageOptimization manager</returns>
        public static ImageOptimizationManager GetManager(string providerName)
        {
            return ManagerBase<ImageOptimizationDataProvider>.GetManager<ImageOptimizationManager>(providerName);
        }

        /// <summary>
        /// Get an instance of the ImageOptimization manager by explicitly specifying the required provider to use
        /// </summary>
        /// <param name="providerName">Name of the provider to use, or null/empty string to use the default provider.</param>
        /// <param name="transactionName">Name of the transaction.</param>
        /// <returns>Instance of the ImageOptimization manager</returns>
        public static ImageOptimizationManager GetManager(string providerName, string transactionName)
        {
            return ManagerBase<ImageOptimizationDataProvider>.GetManager<ImageOptimizationManager>(providerName, transactionName);
        }

        /// <summary>
        /// Creates a ImageOptimizationLogEntry.
        /// </summary>
        /// <returns>The created ImageOptimizationLogEntry.</returns>
        public ImageOptimizationLogEntry CreateImageOptimizationLogEntry()
        {
            return this.Provider.CreateImageOptimizationLogEntry();
        }

        /// <summary>
        /// Updates the ImageOptimizationLogEntry.
        /// </summary>
        /// <param name="entity">The ImageOptimizationLogEntry entity.</param>
        public void UpdateImageOptimizationLogEntry(ImageOptimizationLogEntry entity)
        {
            this.Provider.UpdateImageOptimizationLogEntry(entity);
        }

        /// <summary>
        /// Deletes the ImageOptimizationLogEntry.
        /// </summary>
        /// <param name="entity">The ImageOptimizationLogEntry entity.</param>
        public void DeleteImageOptimizationLogEntry(ImageOptimizationLogEntry entity)
        {
            this.Provider.DeleteImageOptimizationLogEntry(entity);
        }

        /// <summary>
        /// Gets the ImageOptimizationLogEntry by a specified ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>The ImageOptimizationLogEntry.</returns>
        public ImageOptimizationLogEntry GetImageOptimizationLogEntry(Guid id)
        {
            return this.Provider.GetImageOptimizationLogEntry(id);
        }

        /// <summary>
        /// Gets a query of all the ImageOptimizationLogEntry items.
        /// </summary>
        /// <returns>The ImageOptimizationLogEntry items.</returns>
        public IQueryable<ImageOptimizationLogEntry> GetImageOptimizationLogEntrys()
        {
            return this.Provider.GetImageOptimizationLogEntrys();
        }
        #endregion
    }
}