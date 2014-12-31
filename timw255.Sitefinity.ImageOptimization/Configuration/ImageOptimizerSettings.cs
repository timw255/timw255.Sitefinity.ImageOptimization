using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Localization;
using Telerik.Sitefinity.Utilities.TypeConverters;

namespace timw255.Sitefinity.ImageOptimization.Configuration
{
    public class ImageOptimizerSettings : ConfigElement
    {
        [ConfigurationProperty("description", DefaultValue="")]
        public string Description
        {
            get
            {
                return (string)this["description"];
            }
            set
            {
                this["description"] = value;
            }
        }

        [ConfigurationProperty("enabled", DefaultValue=true)]
        public bool Enabled
        {
            get
            {
                return (bool)this["enabled"];
            }
            set
            {
                this["enabled"] = value;
            }
        }

        [ConfigurationProperty("name", IsKey=true, IsRequired=true, DefaultValue="")]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("parameters")]
        public NameValueCollection Parameters
        {
            get
            {
                return (NameValueCollection)this["parameters"];
            }
            set
            {
                this["parameters"] = value;
            }
        }

        [ConfigurationProperty("type", IsRequired=true, IsKey=false)]
        [TypeConverter(typeof(StringTypeConverter))]
        public Type ProviderType
        {
            get
            {
                Type item = (Type)this["type"];
                if (item == null && !string.IsNullOrEmpty(this.ProviderTypeName))
                {
                    item = TypeResolutionService.ResolveType(this.ProviderTypeName);
                    this["type"] = item;
                }
                return item;
            }
            set
            {
                this["type"] = value;
            }
        }

        internal string ProviderTypeName
        {
            get;
            set;
        }

        [ConfigurationProperty("title")]
        public string Title
        {
            get
            {
                return (string)this["title"];
            }
            set
            {
                this["title"] = value;
            }
        }

        public ImageOptimizerSettings(ConfigElement parent)
            : base(parent)
        {
        }
    }
}
