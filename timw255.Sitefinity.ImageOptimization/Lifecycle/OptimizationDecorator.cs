using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Security.Claims;

namespace timw255.Sitefinity.ImageOptimization.Lifecycle
{
    class OptimizationDecorator : LifecycleDecorator
    {
        public OptimizationDecorator(ILifecycleManager manager, LifecycleItemCopyDelegate copyDelegate, params Type[] itemTypes)
            : base(manager, copyDelegate, itemTypes)
        {
        }

        public OptimizationDecorator(ILifecycleManager manager, Action<Content, Content> copyDelegate, params Type[] itemTypes)
            : base(manager, copyDelegate, itemTypes)
        {
        }

        protected override ILifecycleDataItemGeneric ExecuteOnPublish(ILifecycleDataItemGeneric masterItem, ILifecycleDataItemGeneric liveItem, System.Globalization.CultureInfo culture = null, DateTime? publicationDate = null)
        {
            if (masterItem is Image)
            {
                var masterImage = masterItem as Image;
                var liveImage = liveItem as Image;

                if (liveImage != null && masterImage.FileId != liveImage.FileId)
                {
                    masterImage.SetValue("Optimized", false);
                }

                return base.ExecuteOnPublish(masterItem as ILifecycleDataItemGeneric, liveItem, culture, publicationDate);
            }
            else
            {
                return base.ExecuteOnPublish(masterItem, liveItem, culture, publicationDate);
            }
        }
    }
}
