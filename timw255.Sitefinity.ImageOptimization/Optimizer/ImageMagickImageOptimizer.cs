using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Libraries.Model;
using timw255.Sitefinity.ImageOptimization.Configuration;

namespace timw255.Sitefinity.ImageOptimization.Optimizer
{
    public class ImageMagickImageOptimizer : ImageOptimizerBase
    {
        public ImageMagickImageOptimizer()
        {
        }

        public override Stream CompressImageData(Image image, Stream imageData, out string optimizedExtension)
        {
            var settings = _config.Optimizers["ImageMagickImageOptimizer"].Parameters;

            int _imageQuality = Int32.Parse(settings["imageQuality"]);

            MagickReadSettings magickSettings = new MagickReadSettings();

            switch (image.Extension.ToLower())
            {
                case ".png":
                    magickSettings.Format = MagickFormat.Png;
                    break;
                case ".jpg":
                    magickSettings.Format = MagickFormat.Jpg;
                    break;
                case ".jpeg":
                    magickSettings.Format = MagickFormat.Jpeg;
                    break;
                case ".bmp":
                    magickSettings.Format = MagickFormat.Bmp;
                    break;
                default:
                    magickSettings.Format = MagickFormat.Jpg;
                    break;
            }

            using (MagickImage img = new MagickImage(imageData, magickSettings))
            {
                MemoryStream compressed = new MemoryStream();

                img.Quality = _imageQuality;
                img.Write(compressed);

                if (compressed == null)
                {
                    optimizedExtension = "";
                    return null;
                }

                optimizedExtension = Path.GetExtension(image.FilePath);
                return compressed;
            }
        }
    }
}
