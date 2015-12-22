using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Versioning;
using Telerik.Sitefinity.Workflow;
using timw255.Sitefinity.ImageOptimization.Configuration;

namespace timw255.Sitefinity.ImageOptimization.Optimizer
{
    /// <summary>
    /// Base class for ImageOptimizers
    /// </summary>
    public abstract class ImageOptimizerBase
    {
        protected ImageOptimizationConfig _config;

        public delegate void ImageOptimizedHandler(object o, EventArgs e);
        public event ImageOptimizedHandler OnImageOptimized;

        private LibrariesManager _libManager;
        internal LibrariesManager LibManager
        {
            get
            {
                if (_libManager == null)
                {
                    _libManager = LibrariesManager.GetManager();
                }
                return _libManager;
            }
        }

        private ImageOptimizationManager _optimizationManager;
        internal ImageOptimizationManager OptimizationManager
        {
            get
            {
                if (_optimizationManager == null)
                {
                    _optimizationManager = ImageOptimizationManager.GetManager();
                }
                return _optimizationManager;
            }
        }

        public ImageOptimizerBase()
        {
            _config = Config.Get<ImageOptimizationConfig>();
        }

        /// <summary>
        /// Returns the total number of unoptimized images in the Sitefinity album with the specified Id
        /// </summary>
        /// <param name="albumId">Id of the album</param>
        /// <returns></returns>
        public virtual int GetItemsCount(Guid albumId)
        {
            var optimizedImageIds = new HashSet<Guid>(OptimizationManager.GetImageOptimizationLogEntrys().Where(e => e.OptimizedFileId != Guid.Empty).Select(e => e.ImageId));
            var images = LibManager.GetAlbum(albumId).Images()
                .Where(i => i.Status == ContentLifecycleStatus.Master && !optimizedImageIds.Contains(i.Id));

            return images.Count();
        }

        /// <summary>
        /// Compress the image data for a Sitefinity image
        /// </summary>
        /// <param name="image">Sitefinity image being optimized</param>
        /// <param name="imageData">Uncompressed image data</param>
        /// <param name="optimizedExtension">File extension of the resulting image stream being returned</param>
        /// <returns></returns>
        public abstract Stream CompressImageData(Image image, Stream imageData, out string optimizedExtension);

        /// <summary>
        /// Optimize a single Sitefinity image
        /// </summary>
        /// <param name="parentId">Id of the album that contains the image</param>
        /// <param name="imageId">Id of the master version of the image</param>
        public virtual void OptimizeImage(Guid albumId, Guid masterImageId)
        {
            Album album = LibManager.GetAlbum(albumId);

            // This saves us from having to care about BlobStorage later in the method
            var albumProvider = (LibrariesDataProvider)album.Provider;

            // This should exist!
            var image = album.Images().Where(i => i.Id == masterImageId && i.Status == ContentLifecycleStatus.Master).Single();

            // Pull the Stream of the image from the provider.
            Stream imageData = albumProvider.Download(image);

            using (MemoryStream ms = new MemoryStream())
            {
                // Can't trust the length of Stream. Copying to a MemoryStream
                imageData.CopyTo(ms);
                // Be kind...rewind
                ms.Seek(0, SeekOrigin.Begin);

                // Optimization methods may return the image in a different format than what was provided.
                // If that happens, we need to know about it.
                string optimizedExtension;
                Stream optimizedImage = this.CompressImageData(image, ms, out optimizedExtension);

                // There are different reasons why the optimizer would return null.
                // 1. An error occured (in which case the optimizer should throw or handle the exception)
                // 2. Some other mechanism is being used to handle the item updates (callbacks)
                if (optimizedImage != null)
                {
                    var oLogEntry = OptimizationManager.GetImageOptimizationLogEntrys().Where(e => e.ImageId == image.Id).FirstOrDefault();// .CreateImageOptimizationLogEntry();

                    //oLogEntry.ImageId = image.Id;
                    //oLogEntry.InitialFileExtension = image.Extension;
                    //oLogEntry.InitialTotalSize = image.TotalSize;
                    
                    // Check out the master to get a temp version.
                    Image temp = LibManager.Lifecycle.CheckOut(image) as Image;

                    // Make the modifications to the temp version.
                    LibManager.Upload(temp, optimizedImage, optimizedExtension);

                    // Check in the temp version.
                    // After the check in the temp version is deleted.
                    LibManager.Lifecycle.CheckIn(temp);

                    oLogEntry.OptimizedFileId = image.FileId;

                    OptimizationManager.SaveChanges();

                    LibManager.SaveChanges();

                    // Check to see if this image is already published.
                    // If it is, we need to publish the "Master" to update "Live"
                    if (image.GetWorkflowState() == "Published")
                    {
                        var bag = new Dictionary<string, string>();
                        bag.Add("ContentType", typeof(Image).FullName);
                        WorkflowManager.MessageWorkflow(image.Id, typeof(Image), albumProvider.Name, "Publish", false, bag);
                    }
                }

                // Let concerned parties know that processing has completed for this item
                ImageOptimizedHandler tmp = OnImageOptimized;
                if (tmp != null)
                {
                    OnImageOptimized(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Optimize all images in the Sitefinity album with the specified Id
        /// </summary>
        /// <param name="albumId">Id of the album to be optimized</param>
        public virtual void OptimizeAlbum(Guid albumId)
        {
            // Get all the unoptimized image items
            var optimizedImageIds = new HashSet<Guid>(OptimizationManager.GetImageOptimizationLogEntrys().Where(e => e.OptimizedFileId != Guid.Empty).Select(e => e.ImageId));
            var images = LibManager.GetAlbum(albumId).Images()
                .Where(i => i.Status == ContentLifecycleStatus.Master && !optimizedImageIds.Contains(i.Id));

            foreach (Image image in images)
            {
                this.OptimizeImage(albumId, image.Id);
            }
        }
    }
}
