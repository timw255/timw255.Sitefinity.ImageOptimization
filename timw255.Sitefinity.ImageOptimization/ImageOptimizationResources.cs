using System;
using System.Linq;
using Telerik.Sitefinity.Localization;
using Telerik.Sitefinity.Localization.Data;

namespace timw255.Sitefinity.ImageOptimization
{
    /// <summary>
    /// Localizable strings for the ImageOptimization module
    /// </summary>
    /// <remarks>
    /// You can use Sitefinity Thunder to edit this file.
    /// To do this, open the file's context menu and select Edit with Thunder.
    /// 
    /// If you wish to install this as a part of a custom module,
    /// add this to the module's Initialize method:
    /// App.WorkWith()
    ///     .Module(ModuleName)
    ///     .Initialize()
    ///         .Localization<ImageOptimizationResources>();
    /// </remarks>
    /// <see cref="http://www.sitefinity.com/documentation/documentationarticles/developers-guide/how-to/how-to-import-events-from-facebook/creating-the-resources-class"/>
    [ObjectInfo("ImageOptimizationResources", ResourceClassId = "ImageOptimizationResources", Title = "ImageOptimizationResourcesTitle", TitlePlural = "ImageOptimizationResourcesTitlePlural", Description = "ImageOptimizationResourcesDescription")]
    public class ImageOptimizationResources : Resource
    {
        #region Construction
        /// <summary>
        /// Initializes new instance of <see cref="ImageOptimizationResources"/> class with the default <see cref="ResourceDataProvider"/>.
        /// </summary>
        public ImageOptimizationResources()
        {
        }

        /// <summary>
        /// Initializes new instance of <see cref="ImageOptimizationResources"/> class with the provided <see cref="ResourceDataProvider"/>.
        /// </summary>
        /// <param name="dataProvider"><see cref="ResourceDataProvider"/></param>
        public ImageOptimizationResources(ResourceDataProvider dataProvider)
            : base(dataProvider)
        {
        }
        #endregion

        #region Class Description
        /// <summary>
        /// ImageOptimization Resources
        /// </summary>
        [ResourceEntry("ImageOptimizationResourcesTitle",
            Value = "Image Optimization module labels",
            Description = "The title of this class.",
            LastModified = "2014/12/30")]
        public string ImageOptimizationResourcesTitle
        {
            get
            {
                return this["ImageOptimizationResourcesTitle"];
            }
        }

        /// <summary>
        /// ImageOptimization Resources Title plural
        /// </summary>
        [ResourceEntry("ImageOptimizationResourcesTitlePlural",
            Value = "Image Optimization module labels",
            Description = "The title plural of this class.",
            LastModified = "2014/12/30")]
        public string ImageOptimizationResourcesTitlePlural
        {
            get
            {
                return this["ImageOptimizationResourcesTitlePlural"];
            }
        }

        /// <summary>
        /// Contains localizable resources for ImageOptimization module.
        /// </summary>
        [ResourceEntry("ImageOptimizationResourcesDescription",
            Value = "Contains localizable resources for Image Optimization module.",
            Description = "The description of this class.",
            LastModified = "2014/12/30")]
        public string ImageOptimizationResourcesDescription
        {
            get
            {
                return this["ImageOptimizationResourcesDescription"];
            }
        }

        [ResourceEntry("PreserveFocalArea", Value = "Preserve focal area", Description = "word: Preserve focal area", LastModified = "2015/23/12")]
        public string PreserveFocalArea
        {
            get
            {
                return base["PreserveFocalArea"];
            }
        }

        [ResourceEntry("SmartScaleCropImageProcessorMethod", Value = "Smart scale and crop", Description = "Smart scale and crop", LastModified = "2015/24/12")]
        public string SmartScaleCropImageProcessorMethod
        {
            get
            {
                return base["SmartScaleCropImageProcessorMethod"];
            }
        }

        [ResourceEntry("SmartScaleCropSizeFormat", Value = "Smart scale and crop to {Width}x{Height}", Description = "Smart scale and crop to {Width}x{Height}", LastModified = "2015/24/12")]
        public string SmartScaleCropSizeFormat
        {
            get
            {
                return base["SmartScaleCropSizeFormat"];
            }
        }
        #endregion
    }
}