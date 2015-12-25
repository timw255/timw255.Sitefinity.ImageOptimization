using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.DynamicModules.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.RelatedData;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Modules.Libraries;

namespace timw255.Sitefinity.ImageOptimization
{
    public static class ImageOptimizationExtensions
    {
        public static string GetThumbnailOrDefault(this Image image, string thumbnailName)
        {
            string mediaUrl = string.Empty;

            if (image != null)
            {
                string thumbUrl = image.ResolveThumbnailUrl(thumbnailName, false);
                if (thumbUrl.IsNullOrEmpty())
                {
                    mediaUrl = image.MediaUrl;
                }
                else
                {
                    mediaUrl = thumbUrl;
                }
            }

            return mediaUrl;
        }
    }
}
