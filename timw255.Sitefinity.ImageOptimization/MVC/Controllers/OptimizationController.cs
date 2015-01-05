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
using Telerik.Sitefinity;
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
using timw255.Sitefinity.ImageOptimization.Tasks;

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

            ImageOptimizerBase imageOptimizer = (ImageOptimizerBase)Activator.CreateInstance(optimizerSettings.OptimizerType.Assembly.FullName, optimizerSettings.OptimizerType.FullName).Unwrap();

            imageOptimizer.OptimizeImage(parentId, imageId);
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
