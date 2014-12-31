using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Routing;
using Telerik.Microsoft.Practices.Unity;
using Telerik.Sitefinity;
using Telerik.Sitefinity.Abstractions;
using Telerik.Sitefinity.Abstractions.VirtualPath;
using Telerik.Sitefinity.Abstractions.VirtualPath.Configuration;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Data.Metadata;
using Telerik.Sitefinity.Fluent.Modules;
using Telerik.Sitefinity.Fluent.Modules.Toolboxes;
using Telerik.Sitefinity.Modules.Pages.Configuration;
using Telerik.Sitefinity.Services;
using Telerik.Sitefinity.Web.UI;
using Telerik.Sitefinity.GenericContent.Model;
using Telerik.Sitefinity.Libraries.Model;
using Telerik.Sitefinity.Lifecycle;
using Telerik.Sitefinity.Modules.Libraries;
using Telerik.Sitefinity.Modules.Libraries.Configuration;
using Telerik.Sitefinity.Web.UI.ContentUI.Config;
using Telerik.Sitefinity.Web.UI.ContentUI.Views.Backend.Master.Config;
using timw255.Sitefinity.ImageOptimization.Configuration;
using timw255.Sitefinity.ImageOptimization.Lifecycle;

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

            // Add your initialization logic here
            // here we register the module resources
            // but if you have you should register your module configuration or web service here

            App.WorkWith()
                .Module(settings.Name)
                    .Initialize()
                    .Localization<ImageOptimizationResources>();

            Config.RegisterSection<ImageOptimizationConfig>();

            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                name: "timw255ImageOptimization",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Here is also the place to register to some Sitefinity specific events like Bootstrapper.Initialized or subscribe for an event in with the EventHub class            
            // Please refer to the documentation for additional information http://www.sitefinity.com/documentation/documentationarticles/developers-guide/deep-dive/sitefinity-event-system/ieventservice-and-eventhub
        }

        /// <summary>
        /// Installs this module in Sitefinity system for the first time.
        /// </summary>
        /// <param name="initializer">The Site Initializer. A helper class for installing Sitefinity modules.</param>
        public override void Install(SiteInitializer initializer)
        {
            // Here you can install a virtual path to be used for this assembly
            // A virtual path is required to access the embedded resources
            //this.InstallVirtualPaths(initializer);

            // Here you can install you backend pages
            //this.InstallBackendPages(initializer);

            // Here you can also install your page/form/layout widgets
            //this.InstallPageWidgets(initializer);

            this.InstallCustomFields(initializer);
            this.InstallBackendScripts(initializer);
            this.InstallActionMenuItems(initializer);
            this.InstallLifecycleDecorators(initializer);
        }

        /// <summary>
        /// Upgrades this module from the specified version.
        /// This method is called instead of the Install method when the module is already installed with a previous version.
        /// </summary>
        /// <param name="initializer">The Site Initializer. A helper class for installing Sitefinity modules.</param>
        /// <param name="upgradeFrom">The version this module us upgrading from.</param>
        public override void Upgrade(SiteInitializer initializer, Version upgradeFrom)
        {
            // Here you can check which one is your prevous module version and execute some code if you need to
            // See the example bolow
            //
            //if (upgradeFrom < new Version("1.0.1.0"))
            //{
            //    some upgrade code that your new version requires
            //}
        }

        /// <summary>
        /// Uninstalls the specified initializer.
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        public override void Uninstall(SiteInitializer initializer)
        {
            base.Uninstall(initializer);
            // Add your uninstall logic here

            this.UninstallActionMenuItems(initializer);
            this.UninstallBackendScripts(initializer);
        }

        private void UninstallActionMenuItems(SiteInitializer initializer)
        {
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

                manager.SaveSection(librariesConfig);
            }
        }

        private void UninstallBackendScripts(SiteInitializer initializer)
        {
            string scriptLocation = "timw255.Sitefinity.ImageOptimization.Resources.AlbumExtensions.js, timw255.Sitefinity.ImageOptimization";

            var manager = ConfigManager.GetManager();
            var librariesConfig = manager.GetSection<LibrariesConfig>();

            var albumsBackendList = librariesConfig.ContentViewControls["AlbumsBackend"].ViewsConfig.Values.Where(v => v.ViewName == "AlbumsBackendList").First();

            var scripts = albumsBackendList.Scripts;

            ClientScriptElement scriptElement;
            if (scripts.TryGetValue(scriptLocation, out scriptElement))
            {
                scripts.Remove(scriptElement);

                manager.SaveSection(librariesConfig);
            }
        }
        #endregion

        #region Public and overriden methods
        /// <summary>
        /// Gets the module configuration.
        /// </summary>
        protected override ConfigSection GetModuleConfig()
        {
            // If you have a module configuration, you should return it here
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
            // Here you can register your module virtual paths

            //var virtualPaths = initializer.Context.GetConfig<VirtualPathSettingsConfig>().VirtualPaths;
            //var moduleVirtualPath = ImageOptimizationModule.ModuleVirtualPath + "*";
            //if (!virtualPaths.ContainsKey(moduleVirtualPath))
            //{
            //    virtualPaths.Add(new VirtualPathElement(virtualPaths)
            //    {
            //        VirtualPath = moduleVirtualPath,
            //        ResolverName = "EmbeddedResourceResolver",
            //        ResourceLocation = typeof(ImageOptimizationModule).Assembly.GetName().Name
            //    });
            //}
        }
        #endregion

        #region Install backend pages
        /// <summary>
        /// Installs the backend pages.
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        private void InstallBackendPages(SiteInitializer initializer)
        {
            // Using our Fluent Api you can add your module backend pages hierarchy in no time
            // Here is an example using resources to localize the title of the page and adding a dummy control
            // to the module page. 

            //Guid groupPageId = new Guid("cbc96e16-a576-4237-a39d-b0e213810bb2");
            //Guid pageId = new Guid("8dfe187b-f8ad-4ae4-a3a3-5d0eddc40203");

            //initializer.Installer
            //    .CreateModuleGroupPage(groupPageId, "ImageOptimization group page")
            //        .PlaceUnder(SiteInitializer.SitefinityNodeId)
            //        .SetOrdinal(100)
            //        .LocalizeUsing<ImageOptimizationResources>()
            //        .SetTitleLocalized("ImageOptimizationGroupPageTitle")
            //        .SetUrlNameLocalized("ImageOptimizationGroupPageUrlName")
            //        .SetDescriptionLocalized("ImageOptimizationGroupPageDescription")
            //        .ShowInNavigation()
            //        .AddChildPage(pageId, "ImageOptimization page")
            //            .SetOrdinal(1)
            //            .LocalizeUsing<ImageOptimizationResources>()
            //            .SetTitleLocalized("ImageOptimizationPageTitle")
            //            .SetHtmlTitleLocalized("ImageOptimizationPageTitle")
            //            .SetUrlNameLocalized("ImageOptimizationPageUrlName")
            //            .SetDescriptionLocalized("ImageOptimizationPageDescription")
            //            .AddControl(new System.Web.UI.WebControls.Literal()
            //            {
            //                Text = "<h1 class=\"sfBreadCrumb\">ImageOptimization module Installed</h1>",
            //                Mode = LiteralMode.PassThrough
            //            })
            //            .ShowInNavigation()
            //        .Done()
            //    .Done();
        }
        #endregion

        #region Widgets
        /// <summary>
        /// Installs the form widgets.
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        private void InstallFormWidgets(SiteInitializer initializer)
        {
            // Here you can register your custom form widgets in the toolbox.
            // See the example below.

            //string moduleFormWidgetSectionName = "ImageOptimization";
            //string moduleFormWidgetSectionTitle = "ImageOptimization";
            //string moduleFormWidgetSectionDescription = "ImageOptimization";

            //initializer.Installer
            //    .Toolbox(CommonToolbox.FormWidgets)
            //        .LoadOrAddSection(moduleFormWidgetSectionName)
            //            .SetTitle(moduleFormWidgetSectionTitle)
            //            .SetDescription(moduleFormWidgetSectionDescription)
            //            .LoadOrAddWidget<WidgetType>(WidgetNameForDevelopers)
            //                .SetTitle(WidgetTitle)
            //                .SetDescription(WidgetDescription)
            //                .LocalizeUsing<ImageOptimizationResources>()
            //                .SetCssClass(WidgetCssClass) // You can use a css class to add an icon (this is optional)
            //            .Done()
            //        .Done()
            //    .Done();
        }

        /// <summary>
        /// Installs the layout widgets.
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        private void InstallLayoutWidgets(SiteInitializer initializer)
        {
            // Here you can register your custom layout widgets in the toolbox.
            // See the example below.

            //string moduleLayoutWidgetSectionName = "ImageOptimization";
            //string moduleLayoutWidgetSectionTitle = "ImageOptimization";
            //string moduleLayoutWidgetSectionDescription = "ImageOptimization";

            //initializer.Installer
            //    .Toolbox(CommonToolbox.Layouts)
            //        .LoadOrAddSection(moduleLayoutWidgetSectionName)
            //            .SetTitle(moduleLayoutWidgetSectionTitle)
            //            .SetDescription(moduleLayoutWidgetSectionDescription)
            //            .LoadOrAddWidget<LayoutControl>(WidgetNameForDevelopers)
            //                .SetTitle(WidgetTitle)
            //                .SetDescription(WidgetDescription)
            //                .LocalizeUsing<ImageOptimizationResources>()
            //                .SetCssClass(WidgetCssClass) // You can use a css class to add an icon (Optional)
            //                .SetParameters(new NameValueCollection() 
            //                { 
            //                    { "layoutTemplate", FullPathToTheLayoutWidget },
            //                })
            //            .Done()
            //        .Done()
            //    .Done();
        }

        /// <summary>
        /// Installs the page widgets.
        /// </summary>
        /// <param name="initializer">The initializer.</param>
        private void InstallPageWidgets(SiteInitializer initializer)
        {
            // Here you can register your custom page widgets in the toolbox.
            // See the example below.

            //string modulePageWidgetSectionName = "ImageOptimization";
            //string modulePageWidgetSectionTitle = "ImageOptimization";
            //string modulePageWidgetSectionDescription = "ImageOptimization";

            //initializer.Installer
            //    .Toolbox(CommonToolbox.PageWidgets)
            //        .LoadOrAddSection(modulePageWidgetSectionName)
            //            .SetTitle(modulePageWidgetSectionTitle)
            //            .SetDescription(modulePageWidgetSectionDescription)
            //            .LoadOrAddWidget<WidgetType>(WidgetNameForDevelopers)
            //                .SetTitle(WidgetTitle)
            //                .SetDescription(WidgetDescription)
            //                .LocalizeUsing<ImageOptimizationResources>()
            //                .SetCssClass(WidgetCssClass) // You can use a css class to add an icon (Optional)
            //            .Done()
            //        .Done()
            //    .Done();
        }

        private void InstallCustomFields(SiteInitializer initializer)
        {
            MetadataManager metaManager = MetadataManager.GetManager();

            if (metaManager.GetMetaType(typeof(Image)) == null)
            {
                metaManager.CreateMetaType(typeof(Image));
                metaManager.SaveChanges();
            }

            App.WorkWith()
                .DynamicData()
                .Type(typeof(Image))
                .Field()
                .TryCreateNew("Optimized", typeof(bool))
                .SaveChanges(true);
        }

        private void InstallBackendScripts(SiteInitializer initializer)
        {
            string loadMethodName = "albumsListLoaded";
            string scriptLocation = "timw255.Sitefinity.ImageOptimization.Resources.AlbumExtensions.js, timw255.Sitefinity.ImageOptimization";

            var manager = ConfigManager.GetManager();
            var librariesConfig = manager.GetSection<LibrariesConfig>();

            var albumsBackendList = librariesConfig.ContentViewControls["AlbumsBackend"].ViewsConfig.Values.Where(v => v.ViewName == "AlbumsBackendList").First();

            var scripts = albumsBackendList.Scripts;

            ClientScriptElement scriptElement;
            if (!scripts.TryGetValue(scriptLocation, out scriptElement))
            {
                var newClientScript = new ClientScriptElement(scripts);

                newClientScript.ScriptLocation = scriptLocation;
                newClientScript.LoadMethodName = loadMethodName;

                scripts.Add(scriptLocation, newClientScript);

                manager.SaveSection(librariesConfig);
            }
        }

        private void InstallActionMenuItems(SiteInitializer initializer)
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
        }

        private void InstallLifecycleDecorators(SiteInitializer initializer)
        {
            ObjectFactory.Container.RegisterType<ILifecycleDecorator, OptimizationDecorator>(typeof(LibrariesManager).FullName,
                new InjectionConstructor(
                   new InjectionParameter<ILifecycleManager>(null),
                   new InjectionParameter<Action<Content, Content>>(null),
                   new InjectionParameter<Type[]>(null)));
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