using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Localization;
using Telerik.Sitefinity.Modules.GenericContent.Configuration;
using Telerik.Sitefinity.Web.Configuration;
using timw255.Sitefinity.ImageOptimization.Data.EntityFramework;
using timw255.Sitefinity.ImageOptimization.Optimizer;

namespace timw255.Sitefinity.ImageOptimization.Configuration
{
    [ObjectInfo(Title = "ImageOptimization", Description = "Configuration for the Image Optimization module")]
    public class ImageOptimizationConfig : ModuleConfigBase
    {
        [ObjectInfo(Title = "Default Optimizer", Description = "Default optimizer used for image optimization")]
        [ConfigurationProperty("defaultOptimizer", DefaultValue = "ImageMagickImageOptimizer")]
        public string DefaultOptimizer
        {
            get
            {
                return (string)this["defaultOptimizer"];
            }
            set
            {
                this["defaultOptimizer"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the default data provider. 
        /// </summary>
        [DescriptionResource(typeof(ConfigDescriptions), "DefaultProvider")]
        [ConfigurationProperty("defaultProvider", DefaultValue = "ImageOptimizationEFDataProvider")]
        public override string DefaultProvider
        {
            get
            {
                return (string)this["defaultProvider"];
            }
            set
            {
                this["defaultProvider"] = value;
            }
        }

        [ConfigurationProperty("optimizers")]
        public virtual ConfigElementDictionary<string, ImageOptimizerSettings> Optimizers
        {
            get
            {
                return (ConfigElementDictionary<string, ImageOptimizerSettings>)this["optimizers"];
            }
        }

        protected override void InitializeDefaultProviders(ConfigElementDictionary<string, DataProviderSettings> providers)
        {
            providers.Add(new DataProviderSettings(providers)
            {
                Name = "ImageOptimizationEFDataProvider",
                Title = "ImageOptimizationEFDataProvider",
                Description = "A provider that stores image optimization data in database using Entity Framework.",
                ProviderType = typeof(ImageOptimizationEFDataProvider),
                Parameters = new NameValueCollection() { { "applicationName", "/ImageOptimization" } }
            });
        }

        protected override void OnPropertiesInitialized()
        {
            base.OnPropertiesInitialized();
            if (this.Optimizers.Count == 0)
            {
                this.InstallOptimizers();
            }
        }

        private void InstallOptimizers()
        {
            ImageOptimizerSettings imageOptimizerElement;
            
            imageOptimizerElement = new ImageOptimizerSettings(this.Optimizers)
            {
                Name = "KrakenImageOptimizer",
                Title = "Kraken Image Optimizer",
                Description = "Image optimizer that uses the Kraken.io image service",
                OptimizerType = typeof(KrakenImageOptimizer),
                Enabled = true
            };

            imageOptimizerElement.Parameters.Add("apiKey", "");
            imageOptimizerElement.Parameters.Add("apiSecret", "");
            imageOptimizerElement.Parameters.Add("useCallbacks", "False");
            imageOptimizerElement.Parameters.Add("callbackURL", "http://www.yoursite.com/ImageOptimization/KrakenCallback");
            imageOptimizerElement.Parameters.Add("useLossyOptimization", "False");

            this.Optimizers.Add("KrakenImageOptimizer", imageOptimizerElement);

            imageOptimizerElement = new ImageOptimizerSettings(this.Optimizers)
            {
                Name = "ImageMagickImageOptimizer",
                Title = "Image Magick Image Optimizer",
                Description = "Image optimizer that uses ImageMagick image for image optimization",
                OptimizerType = typeof(ImageMagickImageOptimizer),
                Enabled = true
            };

            imageOptimizerElement.Parameters.Add("imageQuality", "85");

            this.Optimizers.Add("ImageMagickImageOptimizer", imageOptimizerElement);
        }
    }
}
