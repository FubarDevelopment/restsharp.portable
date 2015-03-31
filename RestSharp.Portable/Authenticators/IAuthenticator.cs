namespace RestSharp.Portable.Authenticators
{
    /// <summary>
    /// The authenticator interface
    /// </summary>
    public interface IAuthenticator
    {
        /// <summary>
        /// Modifies the request to ensure that the authentication requirements are met.
        /// </summary>
        /// <param name="client">Client executing this request</param>
        /// <param name="request">Request to authenticate</param>
        void Authenticate(IRestClient client, IRestRequest request);
    }
}
