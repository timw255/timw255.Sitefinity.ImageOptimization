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
using timw255.Sitefinity.ImageOptimization.Optimizer;

namespace timw255.Sitefinity.ImageOptimization.Configuration
{
    [ObjectInfo(Title = "ImageOptimization", Description = "Configuration for the Image Optimization module")]
    public class ImageOptimizationConfig : ConfigSection
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

        [ConfigurationProperty("optimizers")]
        public virtual ConfigElementDictionary<string, ImageOptimizerSettings> Optimizers
        {
            get
            {
                return (ConfigElementDictionary<string, ImageOptimizerSettings>)this["optimizers"];
            }
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
                Name = "KrakenIOImageOptimizer",
                Title = "KrakenIO Image Optimizer",
                Description = "Image optimizer that uses the Kraken.io image service",
                OptimizerType = typeof(KrakenIOImageOptimizer),
                Enabled = true
            };

            imageOptimizerElement.Parameters.Add("apiKey", "");
            imageOptimizerElement.Parameters.Add("apiSecret", "");
            imageOptimizerElement.Parameters.Add("useCallbacks", "False");
            imageOptimizerElement.Parameters.Add("callbackURL", "http://www.yoursite.com/api/Optimization");
            imageOptimizerElement.Parameters.Add("useLossyOptimization", "False");

            this.Optimizers.Add("KrakenIOImageOptimizer", imageOptimizerElement);

            imageOptimizerElement = new ImageOptimizerSettings(this.Optimizers)
            {
                Name = "ImageMagickImageOptimizer",
                Title = "Image Magick Image Optimizer",
                Description = "Image optimizer that uses ImageMagick image for image optimization",
                OptimizerType = typeof(ImageMagickImageOptimizer),
                Enabled = true
            };

            imageOptimizerElement.Parameters.Add("imageQuality", "90");

            this.Optimizers.Add("ImageMagickImageOptimizer", imageOptimizerElement);
        }
    }
}
