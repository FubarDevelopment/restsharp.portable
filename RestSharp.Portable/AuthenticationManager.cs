using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RestSharp.Portable
{
    /// <summary>
    /// Manager for authentication modules
    /// </summary>
    public class AuthenticationManager
    {
        private readonly IDictionary<string, IRestAuthenticationModule> _authenticationModules = new Dictionary<string, IRestAuthenticationModule>(StringComparer.OrdinalIgnoreCase);

        private readonly AuthHeader _authHeader;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationManager" /> class.
        /// </summary>
        public AuthenticationManager()
            : this(AuthHeader.Www)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationManager" /> class.
        /// </summary>
        /// <param name="authHeader">The authentication/authorization header to use</param>
        public AuthenticationManager(AuthHeader authHeader)
        {
            _authHeader = authHeader;
        }

        /// <summary>
        /// Registers an authentication module with the authentication manager.
        /// </summary>
        /// <param name="authenticationModule">synchronous authentication module</param>
        public void Register(IRestAuthenticationModuleSync authenticationModule)
        {
            _authenticationModules[authenticationModule.AuthenticationType] = authenticationModule;
        }

        /// <summary>
        /// Registers an authentication module with the authentication manager.
        /// </summary>
        /// <param name="authenticationModule">asynchronous authentication module</param>
        public void Register(IRestAuthenticationModuleAsync authenticationModule)
        {
            _authenticationModules[authenticationModule.AuthenticationType] = authenticationModule;
        }

        /// <summary>
        /// Removes authentication modules with the specified authentication scheme from the list of registered modules.
        /// </summary>
        /// <param name="authenticationScheme">The authentication scheme of the module to remove. </param>
        public void Unregister(string authenticationScheme)
        {
            _authenticationModules.Remove(authenticationScheme);
        }

        /// <summary>
        /// Removes the specified authentication module from the list of registered modules.
        /// </summary>
        /// <param name="authenticationModule">authentication module</param>
        public void Unregister(IRestAuthenticationModule authenticationModule)
        {
            Unregister(authenticationModule.AuthenticationType);
        }

        /// <summary>
        /// Test if the authentication with the given scheme is registered
        /// </summary>
        /// <param name="authenticationScheme">The authentication scheme of the module to search for</param>
        /// <returns>true when a module is registered for the given scheme</returns>
        public bool Contains(string authenticationScheme)
        {
            return _authenticationModules.ContainsKey(authenticationScheme);
        }

        /// <summary>
        /// Try to get the authentication module by scheme name.
        /// </summary>
        /// <param name="authenticationScheme">The authentication scheme of the module to search for</param>
        /// <param name="authenticationModule">The found authentication module</param>
        /// <returns>true when a module is found for the given scheme</returns>
        public bool TryGetAuthenticationModule(string authenticationScheme, out IRestAuthenticationModule authenticationModule)
        {
            return _authenticationModules.TryGetValue(authenticationScheme, out authenticationModule);
        }

        /// <summary>
        /// Is the authentication module an asynchronous one?
        /// </summary>
        /// <param name="authenticationScheme">The authentication scheme of the module to search for</param>
        /// <returns>true when the module is asynchronous, false when synchronous and null when it wasn't found</returns>
        public bool? IsAsync(string authenticationScheme)
        {
            IRestAuthenticationModule authModule;
            if (!TryGetAuthenticationModule(authenticationScheme, out authModule))
                return null;
            return authModule is IRestAuthenticationModuleAsync;
        }

        /// <summary>
        /// Pre-authenticates a request (synchronous).
        /// </summary>
        /// <param name="client">The REST client</param>
        /// <param name="request">The REST request to pre-authenticate</param>
        /// <param name="credentials">The credentials to use for the authentication</param>
        public void PreAuthenticate(IRestClient client, IRestRequest request, ICredentials credentials)
        {
            if (credentials == null)
                return;
            var requestUri = client.BuildUri(request);
            foreach (var authenticationModule in _authenticationModules.Values.Where(x => x.CanPreAuthenticate))
            {
                var credential = credentials.GetCredential(requestUri, authenticationModule.AuthenticationType);
                if (credential != null)
                {
                    var authenticationModuleSync = authenticationModule as IRestAuthenticationModuleSync;
                    if (authenticationModuleSync != null)
                    {
                        authenticationModuleSync.PreAuthenticate(client, request, _authHeader, credential);
                    }
                    else
                    {
                        var authenticationModuleAsync = (IRestAuthenticationModuleAsync)authenticationModule;
                        authenticationModuleAsync.PreAuthenticate(client, request, _authHeader, credential).Wait();
                    }
                }
            }
        }

        /// <summary>
        /// Pre-authenticates a request (asynchronous).
        /// </summary>
        /// <param name="client">The REST client</param>
        /// <param name="request">The REST request to pre-authenticate</param>
        /// <param name="credentials">The credentials to use for the authentication</param>
        /// <returns>The task where the authentication gets executed</returns>
        public async Task PreAuthenticateAsync(IRestClient client, IRestRequest request, ICredentials credentials)
        {
            if (credentials == null)
                return;
            var requestUri = client.BuildUri(request);
            foreach (var authenticationModule in _authenticationModules.Values.Where(x => x.CanPreAuthenticate))
            {
                var credential = credentials.GetCredential(requestUri, authenticationModule.AuthenticationType);
                if (credential != null)
                {
                    var authenticationModuleAsync = authenticationModule as IRestAuthenticationModuleAsync;
                    if (authenticationModuleAsync != null)
                    {
                        await authenticationModuleAsync.PreAuthenticate(client, request, _authHeader, credential);
                    }
                    else
                    {
                        var authenticationModuleSync = (IRestAuthenticationModuleSync)authenticationModule;
                        authenticationModuleSync.PreAuthenticate(client, request, _authHeader, credential);
                    }
                }
            }
        }

        /// <summary>
        /// Calls each registered authentication module to find the first module that can respond to the authentication request.
        /// </summary>
        /// <param name="client">The REST client</param>
        /// <param name="request">The REST request to authenticate</param>
        /// <param name="response">The REST response to read the challenge from</param>
        /// <param name="credentials">The credentials to use for the authentication</param>
        /// <returns>true if the challenge could be processed by an authentication module</returns>
        public bool Authenticate(IRestClient client, IRestRequest request, HttpResponseMessage response, ICredentials credentials)
        {
            if (credentials == null)
                return false;
            var requestUri = client.BuildUri(request);
            var challenges = response.GetAuthenticationHeaderInfo(_authHeader);
            foreach (var challenge in challenges)
            {
                var credential = credentials.GetCredential(requestUri, challenge.Name);
                if (credential == null)
                    continue;

                IRestAuthenticationModule authenticationModule;
                if (!TryGetAuthenticationModule(challenge.Name, out authenticationModule))
                    continue;

                var authenticationModuleSync = authenticationModule as IRestAuthenticationModuleSync;
                if (authenticationModuleSync != null)
                {
                    if (authenticationModuleSync.Authenticate(client, request, response, _authHeader, credential))
                        return true;
                }

                var authenticationModuleAsync = (IRestAuthenticationModuleAsync)authenticationModule;
                if (authenticationModuleAsync.Authenticate(client, request, response, _authHeader, credential).Result)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Calls each registered authentication module to find the first module that can respond to the authentication request.
        /// </summary>
        /// <param name="client">The REST client</param>
        /// <param name="request">The REST request to authenticate</param>
        /// <param name="response">The REST response to read the challenge from</param>
        /// <param name="credentials">The credentials to use for the authentication</param>
        /// <returns>true if the challenge could be processed by an authentication module</returns>
        public async Task<bool> AuthenticateAsync(IRestClient client, IRestRequest request, HttpResponseMessage response, ICredentials credentials)
        {
            if (credentials == null)
                return false;
            var requestUri = client.BuildUri(request);
            var challenges = response.GetAuthenticationHeaderInfo(_authHeader);
            foreach (var challenge in challenges)
            {
                var credential = credentials.GetCredential(requestUri, challenge.Name);
                if (credential == null)
                    continue;

                IRestAuthenticationModule authenticationModule;
                if (!TryGetAuthenticationModule(challenge.Name, out authenticationModule))
                    continue;

                var authenticationModuleAsync = authenticationModule as IRestAuthenticationModuleAsync;
                if (authenticationModuleAsync != null)
                {
                    if (await authenticationModuleAsync.Authenticate(client, request, response, _authHeader, credential))
                        return true;
                }

                var authenticationModuleSync = (IRestAuthenticationModuleSync)authenticationModule;
                if (authenticationModuleSync.Authenticate(client, request, response, _authHeader, credential))
                    return true;
            }

            return false;
        }
    }
}
