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
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Scheduling;
using Telerik.Sitefinity.Versioning;
using Telerik.Sitefinity.Workflow;
using timw255.Sitefinity.ImageOptimization.Configuration;
using timw255.Sitefinity.ImageOptimization.Optimizer;

namespace timw255.Sitefinity.ImageOptimization.Tasks
{
    public class AlbumOptimizationTask : ScheduledTask
    {
        private int _itemsCount;
        private int _currentIndex;

        public Guid AlbumId { get; set; }

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

            ImageOptimizerBase imageOptimizer = (ImageOptimizerBase)Activator.CreateInstance(optimizerSettings.OptimizerType.Assembly.FullName, optimizerSettings.OptimizerType.FullName).Unwrap();

            _itemsCount = imageOptimizer.GetItemsCount(this.AlbumId);

            imageOptimizer.OnImageOptimized += new ImageOptimizerBase.ImageOptimizedHandler(Image_Optimized);

            imageOptimizer.OptimizeAlbum(this.AlbumId);
        }

        private void Image_Optimized(object sender, EventArgs e)
        {
            UpdateProgress();
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
        public Guid AlbumId { get; set; }

        public AlbumOptimizationTaskState()
        {
        }

        public AlbumOptimizationTaskState(AlbumOptimizationTask albumOptimizationTask)
        {
            this.AlbumId = albumOptimizationTask.AlbumId;
        }
    }
}
