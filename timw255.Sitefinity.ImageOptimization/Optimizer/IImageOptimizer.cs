using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Modules.Libraries;

namespace timw255.Sitefinity.ImageOptimization
{
    public interface IImageOptimizer
    {
        Stream OptimizeImage(Image image, Stream imageData, out string optimizedExtension);
    }
}
