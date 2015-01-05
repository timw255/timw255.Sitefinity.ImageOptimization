using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Workflow;

namespace timw255.Sitefinity.ImageOptimization.Optimizer
{
    public abstract class ImageOptimizerBase
    {
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

        internal int GetItemsCount(Guid albumId)
        {
            return Manager.GetAlbum(albumId).Images().Where(i => i.Status == ContentLifecycleStatus.Master && !i.GetValue<bool>("Optimized")).Count();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="imageData"></param>
        /// <param name="optimizedExtension"></param>
        /// <returns></returns>
        public abstract Stream OptimizeImageData(Image image, Stream imageData, out string optimizedExtension);

        internal void OptimizeImage(Guid parentId, Guid imageId)
        {
            Album album = Manager.GetAlbum(parentId);

            var albumProvider = (LibrariesDataProvider)album.Provider;

            var image = album.Images().Where(i => i.Id == imageId && i.Status == ContentLifecycleStatus.Master && !i.GetValue<bool>("Optimized")).FirstOrDefault();

            if (image != null)
            {
                // Pull the Stream of the image from the provider.
                // This saves us from having to care about BlobStorage
                Stream imageData = albumProvider.Download(image);

                // Can't trust the length of Stream. Copying to a MemoryStream
                using (MemoryStream ms = new MemoryStream())
                {
                    imageData.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    string optimizedExtension;
                    Stream optimizedImage = this.OptimizeImageData(image, ms, out optimizedExtension);

                    // There are different reasons why the optimizer would return null.
                    // 1. An error occured (in which case the optimizer should throw or handle the exception)
                    // 2. Some other mechanism is being used to handle the item updates (callbacks)
                    if (optimizedImage != null)
                    {
                        // Check out the master to get a temp version.
                        Image temp = Manager.Lifecycle.CheckOut(image) as Image;

                        // Make the modifications to the temp version.
                        Manager.Upload(temp, optimizedImage, optimizedExtension);

                        temp.SetValue("Optimized", true);

                        // Check in the temp version.
                        // After the check in the temp version is deleted.
                        Manager.Lifecycle.CheckIn(temp);

                        Manager.SaveChanges();

                        // Check to see if this image is already published.
                        // If it is, we need to publish the "Master" to update "Live"
                        if (image.GetWorkflowState() == "Published")
                        {
                            var bag = new Dictionary<string, string>();
                            bag.Add("ContentType", typeof(Image).FullName);
                            WorkflowManager.MessageWorkflow(image.Id, typeof(Image), albumProvider.Name, "Publish", false, bag);
                        }
                    }

                    OnImageOptimized(this, EventArgs.Empty);
                }
            }
        }

        internal void OptimizeAlbum(Guid albumId)
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
