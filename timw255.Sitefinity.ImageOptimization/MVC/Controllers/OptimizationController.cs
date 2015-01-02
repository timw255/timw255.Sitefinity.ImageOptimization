using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Scheduling;
using Telerik.Sitefinity.Workflow;
using Telerik.Sitefinity.Model;
using timw255.Sitefinity.ImageOptimization.Tasks;
using timw255.Sitefinity.ImageOptimization.Configuration;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.GenericContent.Model;

namespace timw255.Sitefinity.ImageOptimization.MVC.Controllers
{
    public class OptimizationController : ApiController
    {
        [Authorize]
        [HttpPost]
        public Guid Post(Guid albumId)
        {
            return StartOptimizeAlbumItemsTask(albumId);
        }

        [Authorize]
        [HttpPost]
        public void Post(Guid parentId, Guid imageId)
        {
            ImageOptimizationConfig imageOptimizationConfig = Config.Get<ImageOptimizationConfig>();

            var optimizerSettings = imageOptimizationConfig.Optimizers[imageOptimizationConfig.DefaultOptimizer];

            IImageOptimizer imageOptimizer = (IImageOptimizer)Activator.CreateInstance(optimizerSettings.OptimizerType.Assembly.FullName, optimizerSettings.OptimizerType.FullName).Unwrap();

            LibrariesManager _librariesManager = LibrariesManager.GetManager();
            Album album = _librariesManager.GetAlbum(parentId);

            var albumProvider = (LibrariesDataProvider)album.Provider;

            var image = album.Images().Where(i => i.Id == imageId && i.Status == ContentLifecycleStatus.Master && !i.GetValue<bool>("Optimized")).FirstOrDefault();

            if (image != null)
            {
                // Pull the Stream of the image from the provider.
                // This saves us from having to care about BlobStorage
                Stream imageData = albumProvider.Download(image);

                // Can't trust the length of Stream. Converting to a MemoryStream
                using (MemoryStream ms = new MemoryStream())
                {
                    imageData.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    string optimizedExtension;
                    Stream optimizedImage = imageOptimizer.OptimizeImage(image, ms, out optimizedExtension);

                    // There are different reasons why the optimizer would return null.
                    // 1. An error occured (in which case the optimizer should throw or handle the exception)
                    // 2. Some other mechanism is being used to handle the item updates (callbacks)
                    if (optimizedImage != null)
                    {
                        // Check out the master to get a temp version.
                        Image temp = _librariesManager.Lifecycle.CheckOut(image) as Image;

                        // Make the modifications to the temp version.
                        _librariesManager.Upload(temp, optimizedImage, Path.GetExtension(optimizedExtension));
                        temp.SetValue("Optimized", true);

                        // Checkin the temp and get the updated master version.
                        // After the check in the temp version is deleted.
                        _librariesManager.Lifecycle.CheckIn(temp);

                        _librariesManager.SaveChanges();

                        // Check to see if this image is already published.
                        // If it is, we need to publish the "Master" to update "Live"
                        if (image.GetWorkflowState() == "Published")
                        {
                            var bag = new Dictionary<string, string>();
                            bag.Add("ContentType", typeof(Image).FullName);
                            WorkflowManager.MessageWorkflow(image.Id, typeof(Image), albumProvider.Name, "Publish", false, bag);
                        }
                    }
                }
            }
        }

        private Guid StartOptimizeAlbumItemsTask(Guid albumId)
        {
            SchedulingManager manager = SchedulingManager.GetManager();

            Guid guid = Guid.NewGuid();

            AlbumOptimizationTask albumOptimizationTask = new AlbumOptimizationTask()
            {
                Id = guid,
                AlbumId = albumId
            };

            manager.AddTask(albumOptimizationTask);

            manager.SaveChanges();

            return guid;
        }
    }
}
