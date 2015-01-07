using System;
using System.Linq;

namespace timw255.Sitefinity.ImageOptimization.Data.EntityFramework
{
    public class ImageOptimizationEFDataProviderContext
    {
        #region Properties
        /// <summary>
        /// Gets or sets the provider key.
        /// </summary>
        /// <value>The provider key.</value>
        public string ProviderKey { get; set; }

        /// <summary>
        /// Gets or sets the connection id.
        /// </summary>
        /// <value>The connection id.</value>
        public string ConnectionId { get; set; }
        #endregion
    }
}
