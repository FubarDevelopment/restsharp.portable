﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// This authenticator can handle 401 responses and modify the Authentication behavior/result.
    /// </summary>
    public interface IRoundTripAuthenticator : IAuthenticator
    {
        /// <summary>
        /// Will be called when the authentication failed
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        /// <param name="response">Response of the failed request</param>
        void AuthenticationFailed(IRestClient client, IRestRequest request, IRestResponse response);
    }
}
