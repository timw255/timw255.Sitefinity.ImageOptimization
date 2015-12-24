using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Modules.Libraries;
using System.Drawing;
using System.Drawing.Imaging;

namespace timw255.Sitefinity.ImageOptimization
{
    public static class ImageOptimizationHelper
    {
        private static LibrariesManager _libManager;
        internal static LibrariesManager LibManager
        {
            get
            {
                if (_libManager == null)
                {
                    _libManager = LibrariesManager.GetManager();
                }
                return _libManager;
            }
        }

        public static string GetImageFingerprint(Guid imageId)
        {
            var sourceImage = Bitmap.FromStream(LibManager.Download(imageId));

            return GetImageFingerprint(sourceImage);
        }

        public static string GetImageFingerprint(Image sourceImage)
        {
            string hash;
            using (MemoryStream ms = new MemoryStream())
            {
                sourceImage.Save(ms, ImageFormat.Bmp);

                byte[] data = ReadFully(ms);

                using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
                {
                    hash = Convert.ToBase64String(sha1.ComputeHash(data));
                }
            }

            return hash;
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
