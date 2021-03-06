using System;
using System.Collections.Generic;
using System.Data;
using NHibernate.Bytecode;
using NHibernate.Connection;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibConfiguration = NHibernate.Cfg.Configuration;

namespace FluentNHibernate.Cfg.Db
{
    public abstract class PersistenceConfiguration<TThisConfiguration> : PersistenceConfiguration<TThisConfiguration, ConnectionStringBuilder>
        where TThisConfiguration : PersistenceConfiguration<TThisConfiguration, ConnectionStringBuilder>
    {}

    public abstract class PersistenceConfiguration<TThisConfiguration, TConnectionString> : IPersistenceConfigurer
        where TThisConfiguration : PersistenceConfiguration<TThisConfiguration, TConnectionString>
        where TConnectionString : ConnectionStringBuilder, new()
    {
        protected const string DialectKey = "dialect"; // Newer one, but not supported by everything
        protected const string AltDialectKey = "hibernate.dialect"; // Some older NHib tools require this
        protected const string DefaultSchemaKey = "default_schema"; 
        protected const string UseOuterJoinKey = "use_outer_join";
        protected const string MaxFetchDepthKey = "max_fetch_depth";
        protected const string UseReflectionOptimizerKey = "use_reflection_optimizer";
        protected const string QuerySubstitutionsKey = "query.substitutions";
        protected const string ShowSqlKey = "show_sql";
        protected const string FormatSqlKey = "format_sql";

		protected const string CollectionTypeFactoryClassKey = NHibernate.Cfg.Environment.CollectionTypeFactoryClass;
        protected const string ConnectionProviderKey = "connection.provider";
        protected const string DefaultConnectionProviderClassName = "NHibernate.Connection.DriverConnectionProvider";
        protected const string DriverClassKey = "connection.driver_class";
        protected const string ConnectionStringKey = "connection.connection_string";
        const string IsolationLevelKey = "connection.isolation";
        protected const string ProxyFactoryFactoryClassKey = "proxyfactory.factory_class";
        protected const string DefaultProxyFactoryFactoryClassName = "NHibernate.ByteCode.Castle.ProxyFactoryFactory, NHibernate.ByteCode.Castle";
        protected const string AdoNetBatchSizeKey = "adonet.batch_size";
        protected const string CurrentSessionContextClassKey = "current_session_context_class";

        private readonly Dictionary<string, string> values = new Dictionary<string, string>();

        private bool nextBoolSettingValue = true;
        private readonly TConnectionString connectionString;
        private readonly CacheSettingsBuilder cache = new CacheSettingsBuilder();

        protected PersistenceConfiguration()
        {
            values[ConnectionProviderKey] = DefaultConnectionProviderClassName;
            values[ProxyFactoryFactoryClassKey] =  DefaultProxyFactoryFactoryClassName;
            connectionString = new TConnectionString();
        }

        protected virtual IDictionary<string, string> CreateProperties()
        {
            if (connectionString.IsDirty)
                Raw(ConnectionStringKey, connectionString.Create());

            if (cache.IsDirty)
            {
                foreach (var pair in cache.Create())
                {
                    Raw(pair.Key, pair.Value);
                }
            }

            return values;
        }

        public NHibConfiguration ConfigureProperties(NHibConfiguration nhibernateConfig)
        {
            var settings = CreateProperties();

            nhibernateConfig.SetProperties(settings);

            return nhibernateConfig;
        }

        public IDictionary<string, string> ToProperties()
        {
            return CreateProperties();
        }

        protected void ToggleBooleanSetting(string settingKey)
        {
            var value = nextBoolSettingValue.ToString().ToLowerInvariant();

            values[settingKey] = value;

            nextBoolSettingValue = true;
        }

        /// <summary>
        /// Negates the next boolean option.
        /// </summary>
        public TThisConfiguration DoNot
        {
            get
            {
                nextBoolSettingValue = false;
                return (TThisConfiguration)this;
            }
        }

        /// <summary>
        /// Sets the database dialect. This shouldn't be necessary
        /// if you've used one of the provided database configurations.
        /// </summary>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration Dialect(string dialect)
        {
            values[DialectKey] = dialect;
            values[AltDialectKey] = dialect;
            return (TThisConfiguration) this;
        }

        /// <summary>
        /// Sets the database dialect. This shouldn't be necessary
        /// if you've used one of the provided database configurations.
        /// </summary>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration Dialect<T>()
            where T : Dialect
        {
            return Dialect(typeof (T).AssemblyQualifiedName);
        }

        /// <summary>
        /// Sets the default database schema
        /// </summary>
        /// <param name="schema">Default schema name</param>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration DefaultSchema(string schema)
        {
            values[DefaultSchemaKey] = schema;
            return (TThisConfiguration) this;
        }

        /// <summary>
        /// Enables the outer-join option.
        /// </summary>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration UseOuterJoin()
        {
            ToggleBooleanSetting(UseOuterJoinKey);
            return (TThisConfiguration) this;
        }

        /// <summary>
        /// Sets the max fetch depth.
        /// </summary>
        /// <param name="maxFetchDepth">Max fetch depth</param>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration MaxFetchDepth(int maxFetchDepth)
        {
            values[MaxFetchDepthKey] = maxFetchDepth.ToString();
            return (TThisConfiguration)this;
        }

        /// <summary>
        /// Enables the reflection optimizer.
        /// </summary>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration UseReflectionOptimizer()
        {
            ToggleBooleanSetting(UseReflectionOptimizerKey);
            return (TThisConfiguration) this;
        }

        /// <summary>
        /// Sets any query stubstitutions that NHibernate should
        /// perform.
        /// </summary>
        /// <param name="substitutions">Substitutions</param>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration QuerySubstitutions(string substitutions)
        {
            values[QuerySubstitutionsKey] = substitutions;
            return (TThisConfiguration)this;
        }

        /// <summary>
        /// Enables the show SQL option.
        /// </summary>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration ShowSql()
        {
            ToggleBooleanSetting(ShowSqlKey);
            return (TThisConfiguration)this;
        }

        /// <summary>
        /// Enables the format SQL option.
        /// </summary>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration FormatSql()
        {
            ToggleBooleanSetting(FormatSqlKey);
            return (TThisConfiguration)this;
        }

        /// <summary>
        /// Sets the database provider. This shouldn't be necessary
        /// if you're using one of the provided database configurations.
        /// </summary>
        /// <param name="provider">Provider type</param>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration Provider(string provider)
        {
            values[ConnectionProviderKey] = provider;
            return (TThisConfiguration)this;
        }

        /// <summary>
        /// Sets the database provider. This shouldn't be necessary
        /// if you're using one of the provided database configurations.
        /// </summary>
        /// <typeparam name="T">Provider type</typeparam>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration Provider<T>()
            where T : IConnectionProvider
        {
            return Provider(typeof(T).AssemblyQualifiedName);
        }

        /// <summary>
        /// Specify the database driver. This isn't necessary
        /// if you're using one of the provided database configurations.
        /// </summary>
        /// <param name="driverClass">Driver type</param>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration Driver(string driverClass)
        {
            values[DriverClassKey] = driverClass;
            return (TThisConfiguration)this;
        }

        /// <summary>
        /// Specify the database driver. This isn't necessary
        /// if you're using one of the provided database configurations.
        /// </summary>
        /// <typeparam name="T">Driver type</typeparam>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration Driver<T>()
            where T : IDriver
        {
            return Driver(typeof(T).AssemblyQualifiedName);
        }

        /// <summary>
        /// Configure the connection string
        /// </summary>
        /// <example>
        ///     ConnectionString(x =>
        ///     {
        ///       x.Server("db_server");
        ///       x.Database("Products");
        ///     });
        /// </example>
        /// <param name="connectionStringExpression">Closure for building the connection string</param>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration ConnectionString(Action<TConnectionString> connectionStringExpression)
        {
            connectionStringExpression(connectionString);
            return (TThisConfiguration)this;
        }

        /// <summary>
        /// Set the connection string.
        /// </summary>
        /// <param name="value">Connection string to use</param>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration ConnectionString(string value)
        {
            connectionString.Is(value);
            return (TThisConfiguration)this;
        }

        /// <summary>
        /// Configure caching.
        /// </summary>
        /// <example>
        ///     Cache(x =>
        ///     {
        ///       x.UseQueryCache();
        ///       x.UseMinimalPuts();
        ///     });
        /// </example>
        /// <param name="cacheExpression">Closure for configuring caching</param>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration Cache(Action<CacheSettingsBuilder> cacheExpression)
        {
            cacheExpression(cache);
            return (TThisConfiguration)this;
        }

        /// <summary>
        /// Sets a raw property on the NHibernate configuration. Use this method
        /// if there isn't a specific option available in the API.
        /// </summary>
        /// <param name="key">Setting key</param>
        /// <param name="value">Setting value</param>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration Raw(string key, string value)
        {
            values[key] = value;
            return (TThisConfiguration) this;
        }

		/// <summary>
		/// Sets the collectiontype.factory_class property.
		/// NOTE: NHibernate 2.1 only
		/// </summary>
		/// <param name="collectionTypeFactoryClass">factory class</param>
		/// <returns>Configuration</returns>
		public TThisConfiguration CollectionTypeFactory(string collectionTypeFactoryClass)
		{
			values[CollectionTypeFactoryClassKey] = collectionTypeFactoryClass;
			return (TThisConfiguration)this;
		}

		/// <summary>
		/// Sets the collectiontype.factory_class property.
		/// NOTE: NHibernate 2.1 only
		/// </summary>
		/// <param name="collectionTypeFactoryClass">factory class</param>
		/// <returns>Configuration</returns>
		public TThisConfiguration CollectionTypeFactory(Type collectionTypeFactoryClass)
		{
			values[CollectionTypeFactoryClassKey] = collectionTypeFactoryClass.AssemblyQualifiedName;
			return (TThisConfiguration)this;
		}

		/// <summary>
		/// Sets the collectiontype.factory_class property.
		/// NOTE: NHibernate 2.1 only
		/// </summary>
		/// <typeparam name="TCollectionTypeFactory">factory class</typeparam>
		/// <returns>Configuration</returns>
		public TThisConfiguration CollectionTypeFactory<TCollectionTypeFactory>() where TCollectionTypeFactory : ICollectionTypeFactory
		{
			return CollectionTypeFactory(typeof(TCollectionTypeFactory));
		}

        /// <summary>
        /// Sets the proxyfactory.factory_class property.
        /// NOTE: NHibernate 2.1 only
        /// </summary>
        /// <param name="proxyFactoryFactoryClass">factory class</param>
        /// <returns>Configuration</returns>
        public TThisConfiguration ProxyFactoryFactory(string proxyFactoryFactoryClass)
        {
            values[ProxyFactoryFactoryClassKey] = proxyFactoryFactoryClass;
            return (TThisConfiguration)this;
        }

        /// <summary>
        /// Sets the proxyfactory.factory_class property.
        /// NOTE: NHibernate 2.1 only
        /// </summary>
        /// <param name="proxyFactoryFactory">factory class</param>
        /// <returns>Configuration</returns>
        public TThisConfiguration ProxyFactoryFactory(Type proxyFactoryFactory)
        {
            values[ProxyFactoryFactoryClassKey] = proxyFactoryFactory.AssemblyQualifiedName;
            return (TThisConfiguration)this;
        }

        /// <summary>
        /// Sets the proxyfactory.factory_class property.
        /// NOTE: NHibernate 2.1 only
        /// </summary>
        /// <typeparam name="TProxyFactoryFactory">factory class</typeparam>
        /// <returns>Configuration</returns>
        public TThisConfiguration ProxyFactoryFactory<TProxyFactoryFactory>() where TProxyFactoryFactory : IProxyFactoryFactory
        {
            return ProxyFactoryFactory(typeof(TProxyFactoryFactory));
        }

        /// <summary>
        /// Sets the adonet.batch_size property.
        /// </summary>
        /// <param name="size">Batch size</param>
        /// <returns>Configuration</returns>
        public TThisConfiguration AdoNetBatchSize(int size)
        {
            values[AdoNetBatchSizeKey] = size.ToString();
            return (TThisConfiguration)this;
        }

        /// <summary>
        /// Sets the current_session_context_class property.
        /// </summary>
        /// <param name="currentSessionContextClass">current session context class</param>
        /// <returns>Configuration</returns>
        public TThisConfiguration CurrentSessionContext(string currentSessionContextClass)
        {
            values[CurrentSessionContextClassKey] = currentSessionContextClass;
            return (TThisConfiguration)this;
        }

        /// <summary>
        /// Sets the current_session_context_class property.
        /// </summary>
        /// <typeparam name="TSessionContext">Implementation of ICurrentSessionContext to use</typeparam>
        /// <returns>Configuration</returns>
        public TThisConfiguration CurrentSessionContext<TSessionContext>() where TSessionContext : NHibernate.Context.ICurrentSessionContext
        {
            return CurrentSessionContext(typeof(TSessionContext).AssemblyQualifiedName);
        }

        /// <summary>
        /// Sets the connection isolation level. NHibernate setting: connection.isolation
        /// </summary>
        /// <param name="connectionIsolation">Isolation level</param>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration IsolationLevel(IsolationLevel connectionIsolation)
        {
            return IsolationLevel(connectionIsolation.ToString());
        }

        /// <summary>
        /// Sets the connection isolation level. NHibernate setting: connection.isolation
        /// </summary>
        /// <param name="connectionIsolation">Isolation level</param>
        /// <returns>Configuration builder</returns>
        public TThisConfiguration IsolationLevel(string connectionIsolation)
        {
            values[IsolationLevelKey] = connectionIsolation;
            return (TThisConfiguration)this;
        }
    }
}