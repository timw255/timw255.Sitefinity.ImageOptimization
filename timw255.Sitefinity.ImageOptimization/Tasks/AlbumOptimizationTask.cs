using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Scheduling;
using Telerik.Sitefinity.Workflow;
using Telerik.Sitefinity.Model;
using timw255.Sitefinity.ImageOptimization.Configuration;
using timw255.Sitefinity.ImageOptimization.Optimizer;

namespace timw255.Sitefinity.ImageOptimization.Tasks
{
    public class AlbumOptimizationTask : ScheduledTask
    {
        private int _itemsCount;

        private int _currentIndex;

        public Guid AlbumId
        {
            get;
            set;
        }

        public override string TaskName
        {
            get
            {
                return typeof(AlbumOptimizationTask).FullName;
            }
        }

        public AlbumOptimizationTask()
        {
            Title = "OptimizeAlbum";
            ExecuteTime = DateTime.UtcNow;
            Description = "Optimizing images";
        }

        public override string BuildUniqueKey()
        {
            return this.GetCustomData();
        }

        public override void ExecuteTask()
        {
            ImageOptimizationConfig imageOptimizationConfig = Config.Get<ImageOptimizationConfig>();

            var optimizerSettings = imageOptimizationConfig.Optimizers[imageOptimizationConfig.DefaultOptimizer];

            IImageOptimizer imageOptimizer = (IImageOptimizer)Activator.CreateInstance(optimizerSettings.OptimizerType.Assembly.FullName, optimizerSettings.OptimizerType.FullName).Unwrap();

            LibrariesManager _librariesManager = LibrariesManager.GetManager();
            Album album = _librariesManager.GetAlbum(this.AlbumId);

            var albumProvider = (LibrariesDataProvider)album.Provider;

            var images = album.Images().Where(i => i.Status == ContentLifecycleStatus.Master && !i.GetValue<bool>("Optimized"));

            _itemsCount = images.Count();

            foreach (Image image in images)
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
                    // 2. Some other mechanism is being used to handle the item updates (callbacks, etc.)
                    if (optimizedImage != null)
                    {
                        // Check out the master to get a temp version.
                        Image temp = _librariesManager.Lifecycle.CheckOut(image) as Image;

                        // Make the modifications to the temp version.
                        _librariesManager.Upload(temp, optimizedImage, Path.GetExtension(optimizedExtension));
                        temp.SetValue("Optimized", true);

                        // Check in the temp version.
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

                    UpdateProgress();
                }
            }
        }

        private void UpdateProgress()
        {
            AlbumOptimizationTask albumOptimizationTask = this;
            albumOptimizationTask._currentIndex = albumOptimizationTask._currentIndex + 1;
            TaskProgressEventArgs taskProgressEventArg = new TaskProgressEventArgs()
            {
                Progress = this._currentIndex * 100 / this._itemsCount,
                StatusMessage = ""
            };
            TaskProgressEventArgs taskProgressEventArg1 = taskProgressEventArg;
            this.OnProgressChanged(taskProgressEventArg1);
            if (taskProgressEventArg1.Stopped)
            {
                throw new TaskStoppedException();
            }
        }

        public override string GetCustomData()
        {
            AlbumOptimizationTaskState albumOptimizationTaskState = new AlbumOptimizationTaskState(this);
            return JsonConvert.SerializeObject(albumOptimizationTaskState);
        }

        private void PersistState()
        {
            if (base.Id != Guid.Empty)
            {
                SchedulingManager schedulingManager = new SchedulingManager();
                this.CopyToTaskData(schedulingManager.GetTaskData(base.Id));
                schedulingManager.SaveChanges();
            }
        }

        public override void SetCustomData(string customData)
        {
            AlbumOptimizationTaskState albumOptimizationTaskState = JsonConvert.DeserializeObject<AlbumOptimizationTaskState>(customData);
            this.AlbumId = albumOptimizationTaskState.AlbumId;
        }
    }

    internal class AlbumOptimizationTaskState
    {
        public Guid AlbumId
        {
            get;
            set;
        }

        public AlbumOptimizationTaskState()
        {
        }

        public AlbumOptimizationTaskState(AlbumOptimizationTask albumOptimizationTask)
        {
            this.AlbumId = albumOptimizationTask.AlbumId;
        }
    }
}
