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

        [ImageProcessingMethod(Title = "CropToAreaImageProcessorMethod", LabelFormat = "CropToAreaSizeFormat", ResourceClassId = "LibrariesResources", DescriptionText = "Generated image will be resized and cropped to desired area", DescriptionImageResourceName = "Telerik.Sitefinity.Modules.Libraries.ImageProcessing.Resources.CropToAreaResize.png", ValidateArgumentsMethodName = "ValidateCropToAreaArguments")]
        public override Image Crop(Image sourceImage, ImageProcessor.CropArguments args)
        {
            string fingerprint = ImageOptimizationHelper.GetImageFingerprint(sourceImage);
            var entry = OptimizationManager.GetImageOptimizationLogEntrys().Where(e => e.Fingerprint == fingerprint).FirstOrDefault();
            int focalPointX = 0;
            int focalPointY = 0;
            int focalPointWidth = 0;
            int focalPointHeight = 0;

            if (entry != null)
            {
                var sfImage = LibManager.GetImage(entry.ImageId);
                focalPointX = sfImage.GetValue<int>("FocalPointX");
                focalPointY = sfImage.GetValue<int>("FocalPointY");
                focalPointWidth = sfImage.GetValue<int>("FocalPointWidth");
                focalPointHeight = sfImage.GetValue<int>("FocalPointHeight");

                if (focalPointX != 0 && focalPointY != 0)
                {
                    return SmartCrop(sourceImage, focalPointX, focalPointY, focalPointWidth, focalPointHeight, args.Width, args.Height);
                }
            }

            return base.Crop(sourceImage, args);
        }

        [ImageProcessingMethod(Title = "ResizeWithFitToAreaImageProcessorMethod", LabelFormat = "ResizeWithFitToAreaSizeFormat", ResourceClassId = "LibrariesResources", DescriptionText = "Generated image will be resized to desired area", DescriptionImageResourceName = "Telerik.Sitefinity.Modules.Libraries.ImageProcessing.Resources.FitToAreaResize.png", ValidateArgumentsMethodName = "ValidateFitToAreaArguments")]
        public override Image Resize(Image sourceImage, ImageProcessor.FitToAreaArguments args)
        {
            return base.Resize(sourceImage, args);
        }

        [ImageProcessingMethod(Title = "ResizeWithFitToSideImageProcessorMethod", LabelFormat = "ResizeWithFitToSideSizeFormat", ResourceClassId = "LibrariesResources", ValidateArgumentsMethodName = "ValidateFitToSideArguments")]
        public override Image Resize(Image sourceImage, FitToSideArguments args)
        {
            return base.Resize(sourceImage, args);
        }

        public Image SmartCrop(Image source, int focalPointX, int focalPointY, int focalPointWidth, int focalPointHeight, int thumbnailWidth, int thumbnailHeight)
        {
            int cropX = focalPointX;
            int cropY = focalPointY;
            int cropWidth = focalPointWidth;
            int cropHeight = focalPointHeight;

            float s;

            if (thumbnailWidth > thumbnailHeight)
            {
                s = thumbnailWidth / thumbnailHeight;
                cropWidth = (int)Math.Round(focalPointWidth * s, 0);
                cropX = focalPointX - ((cropWidth - focalPointWidth) / 2);
            }

            if (thumbnailWidth < thumbnailHeight)
            {
                s = thumbnailHeight / thumbnailWidth;
                cropHeight = (int)Math.Round(focalPointHeight * s, 0);
                cropY = focalPointY - ((cropHeight - focalPointHeight) / 2);
            }

            if (thumbnailWidth == thumbnailHeight)
            {
                int longestSide = Math.Max(focalPointWidth, focalPointHeight);
                cropWidth = longestSide;
                cropHeight = longestSide;

                if (focalPointWidth > focalPointHeight)
                {
                    cropY = focalPointY - ((cropHeight - focalPointHeight) / 2);
                }

                if (focalPointWidth < focalPointHeight)
                {
                    cropX = focalPointX - ((cropWidth - focalPointWidth) / 2);
                }
            }

            if (cropX < 0) { cropX = 0; }
            if (cropY < 0) { cropY = 0; }

            if (cropX + cropWidth > source.Width)
            {
                cropX = cropX - (source.Width - cropWidth);
            }

            if (cropY + cropHeight > source.Height)
            {
                cropY = cropY - (source.Height - cropHeight);
            }

            if (cropX < 0 || cropY < 0)
            {
                // source needs resized up to work :(
            }

            Rectangle crop = new Rectangle(cropX, cropY, cropWidth, cropHeight);

            var bmp = new Bitmap(crop.Width, crop.Height);

            using (var gr = Graphics.FromImage(bmp))
            {
                gr.DrawImage(source, new Rectangle(0, 0, bmp.Width, bmp.Height), crop, GraphicsUnit.Pixel);
            }

            Image thumbnail;
            if (ImagesHelper.TryResizeImage(bmp, thumbnailWidth, thumbnailHeight, out thumbnail, ImageQuality.Medium))
            {
                return thumbnail;
            }
            return bmp;
        } 
    }
}
