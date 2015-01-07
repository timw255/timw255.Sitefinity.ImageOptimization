using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.Sitefinity.Configuration;
using Telerik.Sitefinity.Data.Configuration;

namespace timw255.Sitefinity.ImageOptimization.Data.EntityFramework
{
    public class ImageOptimizationEFDataConnection
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageOptimizationEFDataConnection" /> class.
        /// </summary>
        /// <param name="connectionName">Name of the connection.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="dataProvider">The data provider.</param>
        private ImageOptimizationEFDataConnection(string connectionName, string connectionString, IImageOptimizationEFDataProvider dataProvider)
        {
            this.connectionName = connectionName;
            this.connectionString = connectionString;
            this.dataProvider = dataProvider;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets the name of the connection.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return this.connectionName;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Initializes the connection.
        /// </summary>
        /// <param name="connectionName">Name of the connection.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <returns></returns>
        public static ImageOptimizationEFDataConnection InitializeConnection(string connectionName, IImageOptimizationEFDataProvider dataProvider)
        {
            IConnectionStringSettings connectionSettings = ImageOptimizationEFDataConnection.GetConnectionStringSettings(connectionName);

            ImageOptimizationEFDataConnection connection;
            if (!ImageOptimizationEFDataConnection.connections.TryGetValue(connectionSettings.Name, out connection))
            {
                lock (ImageOptimizationEFDataConnection.connectionsLock)
                {
                    if (!ImageOptimizationEFDataConnection.connections.TryGetValue(connectionSettings.Name, out connection))
                    {
                        connection = new ImageOptimizationEFDataConnection(connectionSettings.Name, connectionSettings.ConnectionString, dataProvider);
                        ImageOptimizationEFDataConnection.connections.Add(connectionSettings.Name, connection);
                    }
                }
            }
            return connection;
        }

        /// <summary>
        /// Gets the entity framework context.
        /// </summary>
        /// <param name="connectionName">Name of the connection.</param>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        public static ImageOptimizationEFDbContext GetContext(string connectionName, IImageOptimizationEFDataProvider provider)
        {
            ImageOptimizationEFDataConnection connection;

            if (!connections.TryGetValue(connectionName, out connection))
                connection = InitializeConnection(connectionName, provider);

            return new ImageOptimizationEFDbContext(connection.connectionString);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Gets the connection string settings.
        /// </summary>
        /// <param name="connectionStringName">Name of the connection string.</param>
        /// <returns></returns>
        private static IConnectionStringSettings GetConnectionStringSettings(string connectionStringName)
        {
            DataConfig dataConfig = Config.Get<DataConfig>();

            if (!dataConfig.ConnectionStrings.ContainsKey(connectionStringName))
                throw new KeyNotFoundException(connectionStringName);

            return dataConfig.ConnectionStrings[connectionStringName];
        }
        #endregion

        #region Private members
        private static readonly IDictionary<string, ImageOptimizationEFDataConnection> connections = new Dictionary<string, ImageOptimizationEFDataConnection>();
        private static readonly object connectionsLock = new object();
        private readonly IImageOptimizationEFDataProvider dataProvider;
        private readonly string connectionName;
        private string connectionString;
        #endregion
    }
}