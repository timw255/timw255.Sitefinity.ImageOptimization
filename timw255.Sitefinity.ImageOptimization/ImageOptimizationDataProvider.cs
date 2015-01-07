using System;
using System.Linq;
using Telerik.Sitefinity.Data;
using timw255.Sitefinity.ImageOptimization.Models;

namespace timw255.Sitefinity.ImageOptimization
{
    public abstract class ImageOptimizationDataProvider : DataProviderBase
    {
        #region Public and overriden methods
        /// <summary>
        /// Gets the known types.
        /// </summary>
        public override Type[] GetKnownTypes()
        {
            if (knownTypes == null)
            {
                knownTypes = new Type[]
                {
                    typeof(ImageOptimizationLogEntry)
                };
            }
            return knownTypes;
        }

        /// <summary>
        /// Gets the root key.
        /// </summary>
        /// <value>The root key.</value>
        public override string RootKey
        {
            get
            {
                return "ImageOptimizationDataProvider";
            }
        }
        #endregion

        #region Abstract methods
        /// <summary>
        /// Creates a new ImageOptimizationLogEntry and returns it.
        /// </summary>
        /// <returns>The new ImageOptimizationLogEntry.</returns>
        public abstract ImageOptimizationLogEntry CreateImageOptimizationLogEntry();

        /// <summary>
        /// Gets a ImageOptimizationLogEntry by a specified ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>The ImageOptimizationLogEntry.</returns>
        public abstract ImageOptimizationLogEntry GetImageOptimizationLogEntry(Guid id);

        /// <summary>
        /// Gets a query of all the ImageOptimizationLogEntry items.
        /// </summary>
        /// <returns>The ImageOptimizationLogEntry items.</returns>
        public abstract IQueryable<ImageOptimizationLogEntry> GetImageOptimizationLogEntrys();

        /// <summary>
        /// Updates the ImageOptimizationLogEntry.
        /// </summary>
        /// <param name="entity">The ImageOptimizationLogEntry entity.</param>
        public abstract void UpdateImageOptimizationLogEntry(ImageOptimizationLogEntry entity);

        /// <summary>
        /// Deletes the ImageOptimizationLogEntry.
        /// </summary>
        /// <param name="entity">The ImageOptimizationLogEntry entity.</param>
        public abstract void DeleteImageOptimizationLogEntry(ImageOptimizationLogEntry entity);
        #endregion

        #region Private fields and constants
        private static Type[] knownTypes;
        #endregion
    }
}