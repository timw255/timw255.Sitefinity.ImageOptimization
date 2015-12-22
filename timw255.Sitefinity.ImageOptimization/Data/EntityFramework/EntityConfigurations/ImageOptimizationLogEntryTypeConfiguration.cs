using System;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using timw255.Sitefinity.ImageOptimization.Models;

namespace timw255.Sitefinity.ImageOptimization.Data.EntityFramework.EntityConfigurations
{
    public class ImageOptimizationLogEntryTypeConfiguration : EntityTypeConfiguration<ImageOptimizationLogEntry>
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageOptimizationLogEntryTypeConfiguration" /> class.
        /// </summary>
        public ImageOptimizationLogEntryTypeConfiguration()
        {
            this.ToTable("ImageOptimization_ImageOptimizationLogEntrys");
            this.HasKey(x => x.Id);
            this.Property(x => x.ImageId).IsRequired();
            this.Property(x => x.Fingerprint).IsRequired();
            this.Property(x => x.OptimizedFileId);
            this.Property(x => x.InitialTotalSize);
            this.Property(x => x.InitialFileExtension).HasMaxLength(255);
            this.Property(x => x.LastModified);
            this.Property(x => x.DateCreated);
            this.Property(x => x.ApplicationName);
        }
        #endregion
    }
}