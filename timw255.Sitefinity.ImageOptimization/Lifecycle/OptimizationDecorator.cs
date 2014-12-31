using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Security.Claims;

namespace timw255.Sitefinity.ImageOptimization.Lifecycle
{
    class OptimizationDecorator : LifecycleDecorator
    {
        public OptimizationDecorator(ILifecycleManager manager, LifecycleItemCopyDelegate copyDelegate, params Type[] itemTypes)
            : base(manager, copyDelegate, itemTypes)
        {
        }

        protected override ILifecycleDataItemGeneric ExecuteOnPublish(ILifecycleDataItemGeneric masterItem, ILifecycleDataItemGeneric liveItem, System.Globalization.CultureInfo culture = null, DateTime? publicationDate = null)
        {
            var identity = ClaimsManager.GetCurrentIdentity();
            var userName = identity.Name;

            if (masterItem is Image)
            {
                var imageItem = masterItem as Image;
                if (liveItem != null && userName != "system")
                {
                    imageItem.SetValue("Optimized", false);
                }
                return base.ExecuteOnPublish(imageItem as ILifecycleDataItemGeneric, liveItem, culture, publicationDate);
            }
            else
            {
                return base.ExecuteOnPublish(masterItem, liveItem, culture, publicationDate);
            }
        }
    }
}
