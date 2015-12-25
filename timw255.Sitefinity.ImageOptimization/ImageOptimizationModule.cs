using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Routing;
using System.Web.UI;
using Telerik.Microsoft.Practices.Unity;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Abstractions.VirtualPath;
using Telerik.Sitefinity.Abstractions.VirtualPath.Configuration;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Data.Events;
using Telerik.Sitefinity.Data.Metadata;
using Telerik.Sitefinity.Fluent.Modules;
using Telerik.Sitefinity.Fluent.Modules.Toolboxes;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Metadata.Model;
using Telerik.Sitefinity.Model;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Modules.Libraries.Configuration;
using Telerik.Sitefinity.Modules.Libraries.ImageProcessing;
using Telerik.Sitefinity.Modules.Pages.Configuration;
using Telerik.Sitefinity.Security.Claims;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Web.UI;
using Telerik.Sitefinity.Web.UI.Backend.Elements.Config;
using Telerik.Sitefinity.Web.UI.Backend.Elements.Enums;
using Telerik.Sitefinity.Web.UI.Backend.Elements.Widgets;
using Telerik.Sitefinity.Web.UI.ContentUI.Config;
using Telerik.Sitefinity.Web.UI.ContentUI.Views.Backend.Master.Config;
using timw255.Sitefinity.ImageOptimization.Configuration;
using timw255.Sitefinity.ImageOptimization.ImageProcessing;

namespace timw255.Sitefinity.ImageOptimization
{
    /// <summary>
    /// Custom Sitefinity module 
    /// </summary>
    public class ImageOptimizationModule : ModuleBase
    {
        #region Properties
        /// <summary>
        /// Gets the landing page id for the module.
        /// </summary>
        /// <value>The landing page id.</value>
        public override Guid LandingPageId
        {
            get
            {
                return SiteInitializer.DashboardPageNodeId;
            }
        }

        /// <summary>
        /// Gets the CLR types of all data managers provided by this module.
        /// </summary>
        /// <value>An array of <see cref="T:System.Type" /> objects.</value>
        public override Type[] Managers
        {
            get
            {
                return new Type[0];
            }
        }
        #endregion

        #region Module Initialization
        /// <summary>
        /// Initializes the service with specified settings.
        /// This method is called every time the module is initializing (on application startup by default)
        /// </summary>
        /// <param name="settings">The settings.</param>
        public override void Initialize(ModuleSettings settings)
        {
            base.Initialize(settings);

            App.WorkWith()
                .Module(settings.Name)
                    .Initialize()
                    .Localization<ImageOptimizationResources>();

            Config.RegisterSection<ImageOptimizationConfig>();

            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "ImageOptimization",
                routeTemplate: "ImageOptimization/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            this.RegisterEventHandlers();

            ObjectFactory.Container.RegisterType<IImageProcessor, FocalPointImageProcessor>(new ContainerControlledLifetimeManager());
        }

        private void RegisterEventHandlers()
        {
            EventHub.Subscribe<IDataEvent>(ImageOptimizationEventHandlers.ContentActionEventHandler);
            EventHub.Subscribe<IRecyclableDataEvent>(ImageOptimizationEventHandlers.RecycleBinEventHandler);
        }

        /// <summary>
        /// Installs this module in Sitefinity system for the first time.
        /// </summary>
        /// <param name="initializer">The Site Initializer. A helper class for installing Sitefinity modules.</param>
        public override void Install(SiteInitializer initializer)
        {
            this.InstallVirtualPaths(initializer);
            this.InstallFocalPointFields(initializer);
            this.InstallBackendScripts(initializer);
        }

        /// <summary>
        /// Upgrades this module from the specified version.
        /// This method is called instead of the Install method when the module is already installed with a previous version.
        /// </summary>
        /// <param name="initializer">The Site Initializer. A helper class for installing Sitefinity modules.</param>
        /// <param name="upgradeFrom">The version this module us upgrading from.</param>
        public override void Upgrade(SiteInitializer initializer, Version upgradeFrom)
        {
        }

        /// <summary>
        /// Uninstalls the specified initializer.
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        public override void Uninstall(SiteInitializer initializer)
        {
            base.Uninstall(initializer);

            this.UninstallBackendScripts(initializer);
        }

        private void UninstallBackendScripts(SiteInitializer initializer)
        {
            string scriptLocation = "timw255.Sitefinity.ImageOptimization.Resources.AlbumExtensions.js, timw255.Sitefinity.ImageOptimization";

            var manager = ConfigManager.GetManager();
            var librariesConfig = manager.GetSection<LibrariesConfig>();

            var albumsBackendList = librariesConfig.ContentViewControls["AlbumsBackend"].ViewsConfig.Values.Where(v => v.ViewName == "AlbumsBackendList").First();

            var albumsScripts = albumsBackendList.Scripts;

            ClientScriptElement albumScriptElement;
            if (albumsScripts.TryGetValue(scriptLocation, out albumScriptElement))
            {
                albumsScripts.Remove(albumScriptElement);

                manager.SaveSection(librariesConfig);
            }

            string imageScriptLocation = "timw255.Sitefinity.ImageOptimization.Resources.ImageExtensions.js, timw255.Sitefinity.ImageOptimization";

            var imagesBackendList = librariesConfig.ContentViewControls["ImagesBackend"].ViewsConfig.Values.Where(v => v.ViewName == "ImagesBackendList").First();

            var imagesScripts = imagesBackendList.Scripts;

            ClientScriptElement imageScriptElement;
            if (imagesScripts.TryGetValue(imageScriptLocation, out imageScriptElement))
            {
                imagesScripts.Remove(imageScriptElement);

                manager.SaveSection(librariesConfig);
            }

            string focalPointScriptLocation = "timw255.Sitefinity.ImageOptimization.Resources.FocalPoints.js, timw255.Sitefinity.ImageOptimization";

            var imagesBackendEdit = librariesConfig.ContentViewControls["ImagesBackend"].ViewsConfig.Values.Where(v => v.ViewName == "ImagesBackendEdit").First();

            var imagesEditScripts = imagesBackendEdit.Scripts;

            ClientScriptElement focalPointScriptElement;
            if (imagesScripts.TryGetValue(focalPointScriptLocation, out focalPointScriptElement))
            {
                imagesScripts.Remove(focalPointScriptElement);

                manager.SaveSection(librariesConfig);
            }
        }

        public override void Unload()
        {
            this.UnloadActionMenuItems();
            base.Unload();
        }

        private void UnloadActionMenuItems()
        {
            bool saveConfig = false;
            string commandPattern = @"(?:\s+)?<li class='sfSeparator'>(?:\s+)?</li>(?:\s+)?<li>(?:\s+)?<a sys:href='javascript:void\(0\);' class='sf_binderCommand_optimize'>Optimize<\/a>(?:\s+)?<\/li>";

            var manager = ConfigManager.GetManager();
            var librariesConfig = manager.GetSection<LibrariesConfig>();

            var albumsBackendList = (MasterGridViewElement)librariesConfig.ContentViewControls["AlbumsBackend"].ViewsConfig.Values.Where(v => v.ViewName == "AlbumsBackendList").First();
            var gridMode = (GridViewModeElement)albumsBackendList.ViewModesConfig.ToList<ViewModeElement>().Where(m => m.Name == "Grid").First();
            var column = (DataColumnElement)gridMode.ColumnsConfig.Values.Where(c => c.Name == "Actions").First();

            string currentClientTemplate = column.ClientTemplate;

            if (Regex.IsMatch(currentClientTemplate, commandPattern))
            {
                string newClientTemplate = Regex.Replace(currentClientTemplate, commandPattern, "");

                column.ClientTemplate = newClientTemplate;

                saveConfig = true;
            }

            var imagesBackendList = (MasterGridViewElement)librariesConfig.ContentViewControls["ImagesBackend"].ViewsConfig.Values.Where(v => v.ViewName == "ImagesBackendList").First();
            var imagesGridMode = (GridViewModeElement)imagesBackendList.ViewModesConfig.ToList<ViewModeElement>().Where(m => m.Name == "Grid").First();
            var imagesActionColumn = (ActionMenuColumnElement)imagesGridMode.ColumnsConfig.Values.Where(c => c.Name == "Actions").First();
            var imagesActionMenuItems = imagesActionColumn.MenuItems;

            if (imagesActionMenuItems.Contains("Optimize"))
            {
                var item = imagesActionMenuItems.AsEnumerable().Where(i => i.Name == "Optimize").FirstOrDefault();

                imagesActionMenuItems.Remove(item);

                saveConfig = true;
            }

            if (saveConfig)
            {
                manager.SaveSection(librariesConfig);
                SystemManager.RestartApplication(true);
            }
        }

        public override void Load()
        {
            base.Load();
            this.LoadActionMenuItems();
        }

        private void LoadActionMenuItems()
        {
            string commandHTML = "<li class='sfSeparator'></li><li><a sys:href='javascript:void(0);' class='sf_binderCommand_optimize'>Optimize</a></li>";
            string commandPattern = @"<li>(?:\s+)?<a sys:href='javascript:void\(0\);' class='sf_binderCommand_optimize'>Optimize<\/a>(?:\s+)?<\/li>";
            string insertPointPattern = @"(?:\s+)?<li class='sfSeparator'>(?:\s+)?<\/li>(?:\s+)?<li>(?:\s+)?<a sys:href='javascript:void\(0\);' class='sf_binderCommand_relocateLibrary'>{\$LibrariesResources, RelocateLibrary\$}<\/a>(?:\s+)?<\/li>";

            var manager = ConfigManager.GetManager();
            var librariesConfig = manager.GetSection<LibrariesConfig>();

            var albumsBackendList = (MasterGridViewElement)librariesConfig.ContentViewControls["AlbumsBackend"].ViewsConfig.Values.Where(v => v.ViewName == "AlbumsBackendList").First();
            var gridMode = (GridViewModeElement)albumsBackendList.ViewModesConfig.ToList<ViewModeElement>().Where(m => m.Name == "Grid").First();
            var column = (DataColumnElement)gridMode.ColumnsConfig.Values.Where(c => c.Name == "Actions").First();

            string currentClientTemplate = column.ClientTemplate;

            if (!Regex.IsMatch(currentClientTemplate, commandPattern))
            {
                var match = Regex.Match(currentClientTemplate, insertPointPattern);
                string newClientTemplate = currentClientTemplate.Insert(match.Index, commandHTML);

                column.ClientTemplate = newClientTemplate;

                manager.SaveSection(librariesConfig);
            }

            var imagesBackendList = (MasterGridViewElement)librariesConfig.ContentViewControls["ImagesBackend"].ViewsConfig.Values.Where(v => v.ViewName == "ImagesBackendList").First();
            var imagesGridMode = (GridViewModeElement)imagesBackendList.ViewModesConfig.ToList<ViewModeElement>().Where(m => m.Name == "Grid").First();
            var imagesActionColumn = (ActionMenuColumnElement)imagesGridMode.ColumnsConfig.Values.Where(c => c.Name == "Actions").First();
            var imagesActionMenuItems = imagesActionColumn.MenuItems;

            var commandWidget = new CommandWidgetElement(imagesActionMenuItems);

            commandWidget.ButtonType = CommandButtonType.Standard;
            commandWidget.Name = "Optimize";
            commandWidget.Text = "Optimize";
            commandWidget.CommandName = "optimize";
            commandWidget.WrapperTagKey = HtmlTextWriterTag.Li;
            commandWidget.WidgetType = typeof(CommandWidgetElement);

            imagesActionColumn.MenuItems.Add(commandWidget);

            manager.SaveSection(librariesConfig);
        }

        #endregion

        #region Public and overriden methods
        /// <summary>
        /// Gets the module configuration.
        /// </summary>
        protected override ConfigSection GetModuleConfig()
        {
            return Config.Get<ImageOptimizationConfig>();
        }
        #endregion

        #region Virtual paths
        /// <summary>
        /// Installs module virtual paths.
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        private void InstallVirtualPaths(SiteInitializer initializer)
        {
            var virtualPaths = initializer.Context.GetConfig<VirtualPathSettingsConfig>().VirtualPaths;
            var moduleVirtualPath = ImageOptimizationModule.ModuleVirtualPath + "*";
            if (!virtualPaths.ContainsKey(moduleVirtualPath))
            {
                virtualPaths.Add(new VirtualPathElement(virtualPaths)
                {
                    VirtualPath = moduleVirtualPath,
                    ResolverName = "EmbeddedResourceResolver",
                    ResourceLocation = typeof(ImageOptimizationModule).Assembly.GetName().Name
                });
            }
        }
        #endregion

        #region Widgets

        private void InstallBackendScripts(SiteInitializer initializer)
        {
            string albumLoadMethodName = "albumsListLoaded";
            string albumScriptLocation = "timw255.Sitefinity.ImageOptimization.Resources.AlbumExtensions.js, timw255.Sitefinity.ImageOptimization";

            var manager = ConfigManager.GetManager();
            var librariesConfig = manager.GetSection<LibrariesConfig>();

            var albumsBackendList = librariesConfig.ContentViewControls["AlbumsBackend"].ViewsConfig.Values.Where(v => v.ViewName == "AlbumsBackendList").First();

            var albumsScripts = albumsBackendList.Scripts;

            ClientScriptElement albumScriptElement;
            if (!albumsScripts.TryGetValue(albumScriptLocation, out albumScriptElement))
            {
                var newClientScript = new ClientScriptElement(albumsScripts);

                newClientScript.ScriptLocation = albumScriptLocation;
                newClientScript.LoadMethodName = albumLoadMethodName;

                albumsScripts.Add(albumScriptLocation, newClientScript);

                manager.SaveSection(librariesConfig);
            }

            string imageLoadMethodName = "imagesListLoaded";
            string imageScriptLocation = "timw255.Sitefinity.ImageOptimization.Resources.ImageExtensions.js, timw255.Sitefinity.ImageOptimization";

            var imagesBackendList = librariesConfig.ContentViewControls["ImagesBackend"].ViewsConfig.Values.Where(v => v.ViewName == "ImagesBackendList").First();

            var imagesScripts = imagesBackendList.Scripts;

            ClientScriptElement imageScriptElement;
            if (!imagesScripts.TryGetValue(imageScriptLocation, out imageScriptElement))
            {
                var newClientScript = new ClientScriptElement(imagesScripts);

                newClientScript.ScriptLocation = imageScriptLocation;
                newClientScript.LoadMethodName = imageLoadMethodName;

                imagesScripts.Add(imageScriptLocation, newClientScript);

                manager.SaveSection(librariesConfig);
            }

            string focalPointLoadMethodName = "focalPointInit";
            string focalPointScriptLocation = "timw255.Sitefinity.ImageOptimization.Resources.FocalPoints.js, timw255.Sitefinity.ImageOptimization";

            var imagesBackendEdit = librariesConfig.ContentViewControls["ImagesBackend"].ViewsConfig.Values.Where(v => v.ViewName == "ImagesBackendEdit").First();

            var imagesEditScripts = imagesBackendEdit.Scripts;

            ClientScriptElement focalPointScriptElement;
            if (!imagesEditScripts.TryGetValue(focalPointScriptLocation, out focalPointScriptElement))
            {
                var newClientScript = new ClientScriptElement(imagesEditScripts);

                newClientScript.ScriptLocation = focalPointScriptLocation;
                newClientScript.LoadMethodName = focalPointLoadMethodName;

                imagesEditScripts.Add(focalPointScriptLocation, newClientScript);

                manager.SaveSection(librariesConfig);
            }
        }
         
        /// <summary>
        /// Installs the focal point fields.
        /// </summary>
        private void InstallFocalPointFields(SiteInitializer initializer)
        {
            MetadataManager managerInTransaction = initializer.GetManagerInTransaction<MetadataManager>();
            MetaType metaType = managerInTransaction.GetMetaType(typeof(Image));
            if (metaType != null)
            {
                MetaField focalPointX = (
                    from f in metaType.Fields
                    where f.FieldName == "FocalPointX"
                    select f).FirstOrDefault<MetaField>();

                if (focalPointX == null)
                {
                    focalPointX = managerInTransaction.CreateMetafield("FocalPointX");
                    focalPointX.Title = "FocalPointX";
                    focalPointX.ClrType = typeof(int).FullName;
                    focalPointX.ColumnName = "focal_point_x";
                    focalPointX.Required = false;
                    focalPointX.Hidden = true;
                    focalPointX.SetMinValue(0);
                    IList<MetaFieldAttribute> metaAttributes = focalPointX.MetaAttributes;
                    MetaFieldAttribute metaFieldAttribute = new MetaFieldAttribute()
                    {
                        Name = "UserFriendlyDataType",
                        Value = UserFriendlyDataType.Integer.ToString()
                    };
                    metaAttributes.Add(metaFieldAttribute);
                    IList<MetaFieldAttribute> metaFieldAttributes = focalPointX.MetaAttributes;
                    MetaFieldAttribute metaFieldAttribute1 = new MetaFieldAttribute()
                    {
                        Name = "IsCommonProperty",
                        Value = "true"
                    };
                    metaFieldAttributes.Add(metaFieldAttribute1);
                    metaType.Fields.Add(focalPointX);
                }

                MetaField focalPointY = (
                    from f in metaType.Fields
                    where f.FieldName == "FocalPointY"
                    select f).FirstOrDefault<MetaField>();

                if (focalPointY == null)
                {
                    focalPointY = managerInTransaction.CreateMetafield("FocalPointY");
                    focalPointY.Title = "FocalPointY";
                    focalPointY.ClrType = typeof(int).FullName;
                    focalPointY.ColumnName = "focal_point_y";
                    focalPointY.Required = false;
                    focalPointY.Hidden = true;
                    focalPointY.SetMinValue(0);
                    IList<MetaFieldAttribute> metaAttributes = focalPointY.MetaAttributes;
                    MetaFieldAttribute metaFieldAttribute = new MetaFieldAttribute()
                    {
                        Name = "UserFriendlyDataType",
                        Value = UserFriendlyDataType.Integer.ToString()
                    };
                    metaAttributes.Add(metaFieldAttribute);
                    IList<MetaFieldAttribute> metaFieldAttributes = focalPointY.MetaAttributes;
                    MetaFieldAttribute metaFieldAttribute1 = new MetaFieldAttribute()
                    {
                        Name = "IsCommonProperty",
                        Value = "true"
                    };
                    metaFieldAttributes.Add(metaFieldAttribute1);
                    metaType.Fields.Add(focalPointY);
                }

                MetaField focalPointWidth = (
                    from f in metaType.Fields
                    where f.FieldName == "FocalPointWidth"
                    select f).FirstOrDefault<MetaField>();

                if (focalPointWidth == null)
                {
                    focalPointWidth = managerInTransaction.CreateMetafield("FocalPointWidth");
                    focalPointWidth.Title = "FocalPointWidth";
                    focalPointWidth.ClrType = typeof(int).FullName;
                    focalPointWidth.ColumnName = "focal_point_width";
                    focalPointWidth.Required = false;
                    focalPointWidth.Hidden = true;
                    focalPointWidth.SetMinValue(0);
                    IList<MetaFieldAttribute> metaAttributes = focalPointWidth.MetaAttributes;
                    MetaFieldAttribute metaFieldAttribute = new MetaFieldAttribute()
                    {
                        Name = "UserFriendlyDataType",
                        Value = UserFriendlyDataType.Integer.ToString()
                    };
                    metaAttributes.Add(metaFieldAttribute);
                    IList<MetaFieldAttribute> metaFieldAttributes = focalPointWidth.MetaAttributes;
                    MetaFieldAttribute metaFieldAttribute1 = new MetaFieldAttribute()
                    {
                        Name = "IsCommonProperty",
                        Value = "true"
                    };
                    metaFieldAttributes.Add(metaFieldAttribute1);
                    metaType.Fields.Add(focalPointWidth);
                }

                MetaField focalPointHeight = (
                    from f in metaType.Fields
                    where f.FieldName == "FocalPointHeight"
                    select f).FirstOrDefault<MetaField>();

                if (focalPointHeight == null)
                {
                    focalPointHeight = managerInTransaction.CreateMetafield("FocalPointHeight");
                    focalPointHeight.Title = "FocalPointHeight";
                    focalPointHeight.ClrType = typeof(int).FullName;
                    focalPointHeight.ColumnName = "focal_point_height";
                    focalPointHeight.Required = false;
                    focalPointHeight.Hidden = true;
                    focalPointHeight.SetMinValue(0);
                    IList<MetaFieldAttribute> metaAttributes = focalPointHeight.MetaAttributes;
                    MetaFieldAttribute metaFieldAttribute = new MetaFieldAttribute()
                    {
                        Name = "UserFriendlyDataType",
                        Value = UserFriendlyDataType.Integer.ToString()
                    };
                    metaAttributes.Add(metaFieldAttribute);
                    IList<MetaFieldAttribute> metaFieldAttributes = focalPointHeight.MetaAttributes;
                    MetaFieldAttribute metaFieldAttribute1 = new MetaFieldAttribute()
                    {
                        Name = "IsCommonProperty",
                        Value = "true"
                    };
                    metaFieldAttributes.Add(metaFieldAttribute1);
                    metaType.Fields.Add(focalPointHeight);
                }

                MetaField focalPointAnchor = (
                    from f in metaType.Fields
                    where f.FieldName == "FocalPointAnchor"
                    select f).FirstOrDefault<MetaField>();

                if (focalPointAnchor == null)
                {
                    focalPointAnchor = managerInTransaction.CreateMetafield("FocalPointAnchor");
                    focalPointAnchor.Title = "FocalPointAnchor";
                    focalPointAnchor.ClrType = typeof(int).FullName;
                    focalPointAnchor.ColumnName = "focal_point_anchor";
                    focalPointAnchor.Required = false;
                    focalPointAnchor.Hidden = true;
                    focalPointAnchor.SetMinValue(0);
                    IList<MetaFieldAttribute> metaAttributes = focalPointAnchor.MetaAttributes;
                    MetaFieldAttribute metaFieldAttribute = new MetaFieldAttribute()
                    {
                        Name = "UserFriendlyDataType",
                        Value = UserFriendlyDataType.Integer.ToString()
                    };
                    metaAttributes.Add(metaFieldAttribute);
                    IList<MetaFieldAttribute> metaFieldAttributes = focalPointAnchor.MetaAttributes;
                    MetaFieldAttribute metaFieldAttribute1 = new MetaFieldAttribute()
                    {
                        Name = "IsCommonProperty",
                        Value = "true"
                    };
                    metaFieldAttributes.Add(metaFieldAttribute1);
                    metaType.Fields.Add(focalPointAnchor);
                }
            }
        }
        #endregion

        #region Upgrade methods
        #endregion

        #region Private members & constants
        public const string ModuleName = "ImageOptimization";
        internal const string ModuleTitle = "Image Optimization";
        internal const string ModuleDescription = "Optimize images in Sitefinity albums";
        internal const string ModuleVirtualPath = "~/ImageOptimization/";
        #endregion
    }
}
