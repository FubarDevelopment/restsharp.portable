using System;
using System.Collections.Generic;
using System.Reflection;
using RestSharp.Portable.OAuth2.Configuration;
using RestSharp.Portable.OAuth2.Infrastructure;
using System.Linq;

namespace RestSharp.Portable.OAuth2
{
    /// <summary>
    /// Provides an interface to all supported authentication methods
    /// </summary>
    public class AuthorizationRoot
    {
        private readonly IRequestFactory _requestFactory;
        private readonly IOAuth2Configuration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationRoot" /> class.
        /// </summary>
        /// <param name="configurationManager">The configuration manager.</param>
        /// <param name="configurationSectionName">Name of the configuration section.</param>
        /// <param name="requestFactory">The request factory.</param>
        public AuthorizationRoot(
            IConfigurationManager configurationManager, 
            string configurationSectionName, 
            IRequestFactory requestFactory)
        {
            _requestFactory = requestFactory;
            _configuration = configurationManager
                .GetConfiguration<IOAuth2Configuration>(configurationSectionName);
        }
        
        /// <summary>
        /// Returns collection of clients which were configured 
        /// using application configuration file and are enabled.
        /// </summary>
        public virtual IEnumerable<IClient> Clients
        {
            get
            {
                var types = GetClientTypes().ToList();
                Func<IClientConfiguration, Type> getType = 
                    configuration => types.FirstOrDefault(x => x.Name == configuration.ClientTypeName);

                return
                    _configuration
                        .Where(configuration => configuration.IsEnabled)
                        .Select(configuration => new { configuration, type = getType(configuration) })
                        .Where(o => o.type != null)
                        .Select(o => (IClient)Activator.CreateInstance(o.type, _requestFactory, o.configuration));                
            }
        }

        /// <summary>
        /// Returns collection of client types to consider
        /// </summary>        
        protected virtual IEnumerable<Type> GetClientTypes()
        {
#if NET40ALIKE
            return Assembly.GetExecutingAssembly().GetTypes().Where(typeof(IClient).IsAssignableFrom);
#else
            return GetType().GetTypeInfo().Assembly.DefinedTypes.Where(typeof(IClient).GetTypeInfo().IsAssignableFrom).Select(x => x.AsType());
#endif
        }
    }
}
