using System;
using System.Data.Entity;
using System.Linq;

namespace timw255.Sitefinity.ImageOptimization.Data.EntityFramework
{
    public interface IImageOptimizationEFDbContext
    {
        #region Methods
        /// <summary>
        /// Begins the transaction.
        /// </summary>
        /// <returns></returns>
        DbContextTransaction BeginTransaction();

        /// <summary>
        /// Rollbacks the transaction.
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        void CommitTransaction();
        #endregion
    }
}