using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Data.Events;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Model;

namespace timw255.Sitefinity.ImageOptimization
{
    public static class ImageOptimizationEventHandlers
    {
        private static ImageOptimizationManager _ioManager;
        internal static ImageOptimizationManager IOManager
        {
            get
            {
                if (_ioManager == null)
                {
                    _ioManager = ImageOptimizationManager.GetManager();
                }
                return _ioManager;
            }
        }

        public static void ContentActionEventHandler(IDataEvent evt)
        {
            var action = evt.Action;
            var contentType = evt.ItemType;

            if (contentType == typeof(Image) && (action == "Publish" || action == "Updated"))
            {
                var itemId = evt.ItemId;
                var providerName = evt.ProviderName;
                var manager = ManagerBase.GetMappedManager(contentType, providerName) as LibrariesManager;
                var item = manager.GetItemOrDefault(contentType, itemId) as Image;

                if (item.Status == ContentLifecycleStatus.Temp)
                {
                    var master = manager.Lifecycle.GetMaster(item) as Image;

                    if (master.FileId == item.FileId)
                    {
                        var focalPointChanged = false;

                        focalPointChanged = master.GetValue<int>("FocalPointX") != item.GetValue<int>("FocalPointX");
                        focalPointChanged = focalPointChanged ? true : master.GetValue<int>("FocalPointY") != item.GetValue<int>("FocalPointY");
                        focalPointChanged = focalPointChanged ? true : master.GetValue<int>("FocalPointWidth") != item.GetValue<int>("FocalPointWidth");
                        focalPointChanged = focalPointChanged ? true : master.GetValue<int>("FocalPointHeight") != item.GetValue<int>("FocalPointHeight");

                        if (focalPointChanged)
                        {
                            // need to regenerate thumbnail...somehow
                        }
                    }
                    else
                    {
                        // the image changed, need to set focal point to 0...somehow
                    }
                }

                if (item.Status == ContentLifecycleStatus.Master)
                {
                    var entry = IOManager.GetImageOptimizationLogEntrys().Where(e => e.ImageId == item.Id).FirstOrDefault();

                    if (entry == null)
                    {
                        entry = IOManager.CreateImageOptimizationLogEntry();

                        entry.ImageId = item.Id;
                        entry.Fingerprint = ImageOptimizationHelper.GetImageFingerprint(item.Id);
                        entry.InitialFileExtension = item.Extension;
                        entry.InitialTotalSize = item.TotalSize;

                        IOManager.SaveChanges();
                    }
                    else
                    {
                        if (entry.OptimizedFileId != Guid.Empty && item.FileId != entry.OptimizedFileId)
                        {
                            entry.Fingerprint = ImageOptimizationHelper.GetImageFingerprint(item.Id);
                            entry.OptimizedFileId = Guid.Empty;
                            IOManager.SaveChanges();
                        }
                    }
                }
            }
        }

        public static void RecycleBinEventHandler(IRecyclableDataEvent evt)
        {
            if (evt.RecycleBinAction == RecycleBinAction.PermanentDelete)
            {
                DeleteIOLogEntryForItem(evt.ItemId);
            }
        }

        private static void DeleteIOLogEntryForItem(Guid itemId)
        {
            var entry = IOManager.GetImageOptimizationLogEntrys().Where(e => e.ImageId == itemId).FirstOrDefault();

            if (entry != null)
            {
                IOManager.DeleteImageOptimizationLogEntry(entry);
                IOManager.SaveChanges();
            }
        }
    }
}
