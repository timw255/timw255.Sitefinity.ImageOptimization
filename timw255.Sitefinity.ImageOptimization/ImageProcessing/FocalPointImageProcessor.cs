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

            if (entry != null)
            {
                var sfImage = LibManager.GetImage(entry.ImageId);
                int focalPointX = sfImage.GetValue<int>("FocalPointX");
                int focalPointY = sfImage.GetValue<int>("FocalPointY");

                if (focalPointX != 0 && focalPointY != 0)
                {
                    return CropImageFromFocalPoint(sourceImage, focalPointX, focalPointY, args.Width, args.Height);
                }
            }

            return base.Crop(sourceImage, args);
        }

        [ImageProcessingMethod(Title = "ResizeWithFitToAreaImageProcessorMethod", LabelFormat = "ResizeWithFitToAreaSizeFormat", ResourceClassId = "LibrariesResources", DescriptionText = "Generated image will be resized to desired area", DescriptionImageResourceName = "Telerik.Sitefinity.Modules.Libraries.ImageProcessing.Resources.FitToAreaResize.png", ValidateArgumentsMethodName = "ValidateFitToAreaArguments")]
        public override Image Resize(Image sourceImage, ImageProcessor.FitToAreaArguments args)
        {
            string fingerprint = ImageOptimizationHelper.GetImageFingerprint(sourceImage);

            var entry = OptimizationManager.GetImageOptimizationLogEntrys().Where(e => e.Fingerprint == fingerprint).FirstOrDefault();

            if (entry != null)
            {
                var sfImage = LibManager.GetImage(entry.ImageId);
                int focalPointX = sfImage.GetValue<int>("FocalPointX");
                int focalPointY = sfImage.GetValue<int>("FocalPointY");

                if (focalPointX != 0 && focalPointY != 0)
                {
                    return CropImageFromFocalPoint(sourceImage, focalPointX, focalPointY, args.MaxWidth, args.MaxHeight);
                }
            }

            return base.Resize(sourceImage, args);
        }

        [ImageProcessingMethod(Title = "ResizeWithFitToSideImageProcessorMethod", LabelFormat = "ResizeWithFitToSideSizeFormat", ResourceClassId = "LibrariesResources", ValidateArgumentsMethodName = "ValidateFitToSideArguments")]
        public override Image Resize(Image sourceImage, FitToSideArguments args)
        {
            string fingerprint = ImageOptimizationHelper.GetImageFingerprint(sourceImage);

            var entry = OptimizationManager.GetImageOptimizationLogEntrys().Where(e => e.Fingerprint == fingerprint).FirstOrDefault();

            if (entry != null)
            {
                var sfImage = LibManager.GetImage(entry.ImageId);
                int focalPointX = sfImage.GetValue<int>("FocalPointX");
                int focalPointY = sfImage.GetValue<int>("FocalPointY");

                if (focalPointX != 0 && focalPointY != 0)
                {
                    return CropImageFromFocalPoint(sourceImage, focalPointX, focalPointY, args.Size, args.Size);
                }
            }

            return base.Resize(sourceImage, args);
        }

        public Bitmap CropImageFromFocalPoint(Image source, int focalPointX, int focalPointY, int width, int height)
        {
            // set the starting point for the crop so that the focal point is in the center of the resulting image
            int x = focalPointX - (width / 2);
            int y = focalPointY - (height / 2);

            Rectangle crop = new Rectangle(x, y, width, height);

            var bmp = new Bitmap(crop.Width, crop.Height);
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.DrawImage(source, new Rectangle(0, 0, bmp.Width, bmp.Height), crop, GraphicsUnit.Pixel);
            }
            return bmp;
        } 
    }
}
