using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Data.Events;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Libraries.Model;

namespace timw255.Sitefinity.ImageOptimization
{
    public static class ImageOptimizationEventHandlers
    {
        public static void ContentActionEventHandler(IDataEvent evt)
        {
            var action = evt.Action;
            var contentType = evt.ItemType;

            if (contentType == typeof(Image) && (action == "Publish" || action == "Updated"))
            {
                var itemId = evt.ItemId;
                var providerName = evt.ProviderName;
                var manager = ManagerBase.GetMappedManager(contentType, providerName);
                var item = manager.GetItemOrDefault(contentType, itemId) as Image;

                if (item.Status == ContentLifecycleStatus.Master)
                {
                    var optimizationManager = ImageOptimizationManager.GetManager();

                    var entry = optimizationManager.GetImageOptimizationLogEntrys().Where(e => e.ImageId == item.Id).FirstOrDefault();

                    if (entry != null && item.FileId != entry.OptimizedFileId)
                    {
                        optimizationManager.DeleteImageOptimizationLogEntry(entry);
                        optimizationManager.SaveChanges();
                    }
                }
            }
        }

        public static void RecycleBinEventHandler(IRecyclableDataEvent evt)
        {
            if (evt.RecycleBinAction == RecycleBinAction.PermanentDelete)
            {
                var itemId = evt.ItemId;

                var optimizationManager = ImageOptimizationManager.GetManager();

                var entry = optimizationManager.GetImageOptimizationLogEntrys().Where(e => e.ImageId == itemId).FirstOrDefault();

                if (entry != null)
                {
                    optimizationManager.DeleteImageOptimizationLogEntry(entry);
                    optimizationManager.SaveChanges();
                }
            }
        }
    }
}
