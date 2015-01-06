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

        private LibrariesManager _manager;
        internal LibrariesManager Manager
        {
            get
            {
                if (_manager == null)
                {
                    _manager = LibrariesManager.GetManager();
                }
                return _manager;
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
            return Manager.GetAlbum(albumId).Images().Where(i => i.Status == ContentLifecycleStatus.Master && !i.GetValue<bool>("Optimized")).Count();
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
            Album album = Manager.GetAlbum(albumId);

            // This saves us from having to care about BlobStorage later in the method
            var albumProvider = (LibrariesDataProvider)album.Provider;

            // This should exist!
            var image = album.Images().Where(i => i.Id == masterImageId && i.Status == ContentLifecycleStatus.Master && !i.GetValue<bool>("Optimized")).Single();

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
                    // Check out the master to get a temp version.
                    Image temp = Manager.Lifecycle.CheckOut(image) as Image;

                    // Make the modifications to the temp version.
                    Manager.Upload(temp, optimizedImage, optimizedExtension);

                    // Set the Optimized flag to prevent re-processing images for no reason.
                    temp.SetValue("Optimized", true);

                    // Check in the temp version.
                    // After the check in the temp version is deleted.
                    Manager.Lifecycle.CheckIn(temp);

                    Image liveImage = (Image)Manager.Lifecycle.GetLive(image);

                    // Probably not the best idea ever BUT, it needs to replace the FileId without tripping workflow.
                    liveImage.FileId = image.FileId;
                    liveImage.SetValue("Optimized", true);

                    Manager.SaveChanges();
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
            var images = Manager.GetAlbum(albumId).Images().Where(i => i.Status == ContentLifecycleStatus.Master && !i.GetValue<bool>("Optimized"));

            foreach (Image image in images)
            {
                this.OptimizeImage(albumId, image.Id);
            }
        }
    }
}
