using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Modules.Libraries.ImageProcessing;
using Telerik.Sitefinity.Modules.Libraries.Images;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Localization;

namespace timw255.Sitefinity.ImageOptimization.ImageProcessing
{
    public class FocalPointImageProcessor : ImageProcessor
    {
        private LibrariesManager _libManager;
        internal LibrariesManager LibManager
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

        private ImageOptimizationManager _optimizationManager;
        internal ImageOptimizationManager OptimizationManager
        {
            get
            {
                if (_optimizationManager == null)
                {
                    _optimizationManager = ImageOptimizationManager.GetManager();
                }
                return _optimizationManager;
            }
        }

        [ImageProcessingMethod(Title = "SmartScaleCropImageProcessorMethod", LabelFormat = "SmartScaleCropSizeFormat", ResourceClassId = "ImageOptimizationResources", DescriptionText = "Generated image will be scaled and/or cropped to desired size based on focal area", DescriptionImageResourceName = "timw255.Sitefinity.ImageOptimization.Resources.SmartScaleCrop.png", ValidateArgumentsMethodName = "ValidateSmartScaleCropArguments")]
        public Image SmartScaleCrop(Image sourceImage, SmartScaleCropArguments args)
        {
            string fingerprint = ImageOptimizationHelper.GetImageFingerprint(sourceImage);
            var entry = OptimizationManager.GetImageOptimizationLogEntrys().Where(e => e.Fingerprint == fingerprint).FirstOrDefault();
            int focalPointX = 0;
            int focalPointY = 0;
            int focalPointWidth = 0;
            int focalPointHeight = 0;
            int focalPointAnchor = 0;

            if (entry != null)
            {
                var sfImage = LibManager.GetImage(entry.ImageId);
                focalPointX = sfImage.GetValue<int>("FocalPointX");
                focalPointY = sfImage.GetValue<int>("FocalPointY");
                focalPointWidth = sfImage.GetValue<int>("FocalPointWidth");
                focalPointHeight = sfImage.GetValue<int>("FocalPointHeight");
                focalPointAnchor = sfImage.GetValue<int>("FocalPointAnchor");

                if (focalPointX != 0 && focalPointY != 0)
                {
                    return Process(sourceImage, focalPointX, focalPointY, focalPointWidth, focalPointHeight, focalPointAnchor, args);
                }
            }

            return base.Crop(sourceImage, args);
        }

        public Image Process(Image source, int focalPointX, int focalPointY, int focalPointWidth, int focalPointHeight, int focalPointAnchor, SmartScaleCropArguments args)
        {
            int cropX = focalPointX;
            int cropY = focalPointY;

            float s = 1;
            bool scaleDown = (focalPointWidth > args.Width) || (focalPointHeight > args.Height);
            bool scaleUp = ((focalPointWidth < args.Width) || (focalPointHeight < args.Height)) && args.ScaleUp;
            bool shouldScale = scaleDown || scaleUp;

            Image scaledSource = source;
            if (args.PreserveFocalArea && shouldScale)
            {
                if (((float)focalPointWidth / (float)args.Width > (float)focalPointHeight / (float)args.Height))
                {
                    s = (float)args.Width / (float)focalPointWidth;
                }
                else
                {
                    s = (float)args.Height / (float)focalPointHeight;
                }
                ImagesHelper.TryResizeImage(source, (int)Math.Round(source.Width * s, 0), (int)Math.Round(source.Height * s, 0), out scaledSource, args.Quality);
            }

            cropX = Math.Min(Math.Max((int)Math.Round(focalPointX * s + focalPointWidth * s / 2 - args.Width / 2, 0), 0), scaledSource.Width - args.Width);

            if (args.Width > scaledSource.Width)
            {
                cropX = (scaledSource.Width - args.Width) / 2;
            }

            cropY = Math.Min(Math.Max((int)Math.Round(focalPointY * s + focalPointHeight * s / 2 - args.Height / 2, 0), 0), scaledSource.Height - args.Height);

            if (args.Height > scaledSource.Height)
            {
                cropY = (scaledSource.Height - args.Height) / 2;
            }

            if (!args.PreserveFocalArea)
            {
                switch (focalPointAnchor)
                {
                    case 0:
                        break;
                    case 1:
                        cropY = focalPointY;
                        break;
                    case 2:
                        cropX = Math.Max((focalPointX + focalPointWidth) - args.Width, 0);
                        break;
                    case 3:
                        cropY = Math.Max((focalPointY + focalPointHeight) - args.Height, 0);
                        break;
                    case 4:
                        cropX = focalPointX;
                        break;
                }
            }

            Rectangle crop = new Rectangle(cropX, cropY, args.Width, args.Height);

            var bmp = new Bitmap(crop.Width, crop.Height);

            using (var gr = Graphics.FromImage(bmp))
            {
                gr.DrawImage(scaledSource, new Rectangle(0, 0, bmp.Width, bmp.Height), crop, GraphicsUnit.Pixel);
            }

            return bmp;
        }

        protected virtual void ValidateSmartScaleCropArguments(object argument)
        {
            SmartScaleCropArguments cropArgument = argument as SmartScaleCropArguments;
            if (cropArgument == null)
            {
                object[] fullName = new object[] { typeof(SmartScaleCropArguments).FullName };
                throw new InvalidOperationException("Argument of type '{0}' is expected".Arrange(fullName));
            }
            base.ValidateCropToAreaArguments(argument);
        }

        public class SmartScaleCropArguments : CropArguments
        {
            [ImageProcessingProperty(Title = "PreserveFocalArea", ResourceClassId = "ImageOptimizationResources")]
            public bool PreserveFocalArea
            {
                get;
                set;
            }

            public SmartScaleCropArguments()
            {
                this.PreserveFocalArea = false;
            }
        }
    }
}
