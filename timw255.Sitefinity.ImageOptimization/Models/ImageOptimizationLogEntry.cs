using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.OpenAccess;
using Telerik.Sitefinity.Model;

namespace timw255.Sitefinity.ImageOptimization.Models
{
    public class ImageOptimizationLogEntry : IDataItem
    {
        #region Construction
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageOptimizationLogEntry" /> class.
        /// </summary>
        public ImageOptimizationLogEntry()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageOptimizationLogEntry" /> class.
        /// </summary>
        /// <param name="id">The ImageOptimizationLogEntry ID.</param>
        /// <param name="applicationName">Name of the application.</param>
        public ImageOptimizationLogEntry(Guid id, string applicationName)
        {
            this.Id = id;
            this.ApplicationName = applicationName;
            this.DateCreated = DateTime.UtcNow;
            this.LastModified = DateTime.UtcNow;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the name of the application to which this data item belongs to.
        /// </summary>
        /// <value>The name of the application.</value>
        public string ApplicationName
        {
            get
            {
                return this.applicationName;
            }
            set
            {
                this.applicationName = value;
            }
        }

        /// <summary>
        /// The data provider this item was instantiated(retrieved or created) with.
        /// </summary>
        public object Provider
        {
            get
            {
                return this.provider;
            }
            set
            {
                this.provider = value;
            }
        }

        /// <summary>
        /// The transaction scope this item belongs to or null. This property is set by the specific forums provider implementation
        /// </summary>
        public object Transaction
        {
            get
            {
                return this.transaction;
            }
            set
            {
                this.transaction = value;
            }
        }

        /// <summary>
        /// Gets or sets the date created.
        /// </summary>
        /// <value>The date created.</value>
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets the time this item was last modified.
        /// </summary>
        /// <value>The last modified time.</value>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Gets the unique identity of the ImageOptimizationLogEntry.
        /// </summary>
        /// <value>The id.</value>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the ImageId.
        /// </summary>
        public Guid ImageId { get; set; }

        /// <summary>
        /// Gets or sets the Fingerprint.
        /// </summary>
        public string Fingerprint { get; set; }

        /// <summary>
        /// Gets or sets the OptimizedFileId.
        /// </summary>
        public Guid OptimizedFileId { get; set; }

        /// <summary>
        /// Gets or sets the InitialTotalSize.
        /// </summary>
        public long InitialTotalSize { get; set; }

        /// <summary>
        /// Gets or sets the InitialFileExtension.
        /// </summary>
        public string InitialFileExtension { get; set; }

        #endregion

        #region Private fields and constants
        private string applicationName;
        private object provider;
        private object transaction;
        #endregion
    }
}