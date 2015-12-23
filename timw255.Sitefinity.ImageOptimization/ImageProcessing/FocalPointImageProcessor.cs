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
                    return SmartCrop(sourceImage, focalPointX, focalPointY, focalPointWidth, focalPointHeight, args);
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

        public Image SmartCrop(Image source, int focalPointX, int focalPointY, int focalPointWidth, int focalPointHeight, ImageProcessor.CropArguments args)
        {
            int cropX = focalPointX;
            int cropY = focalPointY;

            float s = 1;
            Image scaledSource = source;
            bool scaleDown = (focalPointWidth > args.Width) || (focalPointHeight > args.Height);
            bool scaleUp = ((focalPointWidth < args.Width) || (focalPointHeight < args.Height)) && args.ScaleUp;
            bool shouldScale = scaleDown || scaleUp;

            if (shouldScale)
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

            Rectangle crop = new Rectangle(cropX, cropY, args.Width, args.Height);

            var bmp = new Bitmap(crop.Width, crop.Height);

            using (var gr = Graphics.FromImage(bmp))
            {
                gr.DrawImage(scaledSource, new Rectangle(0, 0, bmp.Width, bmp.Height), crop, GraphicsUnit.Pixel);
            }

            return bmp;
        } 
    }
}
