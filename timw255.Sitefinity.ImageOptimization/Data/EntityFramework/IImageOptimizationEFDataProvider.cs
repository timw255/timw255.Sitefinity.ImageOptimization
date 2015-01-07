using System;
using System.Linq;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Data.Decorators;
using timw255.Sitefinity.ImageOptimization.Data.EntityFramework.Decorators;

namespace timw255.Sitefinity.ImageOptimization.Data.EntityFramework
{
    [DataProviderDecorator(typeof(ImageOptimizationEFDataProviderDecorator))]
    public interface IImageOptimizationEFDataProvider : IDataProviderBase
    {
        #region Methods
        /// <summary>
        /// Gets or sets the provider context.
        /// </summary>
        /// <value>The provider context.</value>
        ImageOptimizationEFDataProviderContext ProviderContext { get; set; }

        /// <summary>
        /// Gets the db context.
        /// </summary>
        /// <value>The db context.</value>
        ImageOptimizationEFDbContext Context { get; }
        #endregion
    }
}
