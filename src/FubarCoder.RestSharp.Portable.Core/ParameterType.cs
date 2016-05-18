namespace RestSharp.Portable
{
    /// <summary>
    /// Parameter type
    /// </summary>
    public enum ParameterType
    {
        /// <summary>
        /// Parameter will be stored in the URL query for a GET request, or in the body for a POST request
        /// </summary>
        GetOrPost,

        /// <summary>
        /// The parameter is part of the IRestResponse.Resource
        /// </summary>
        UrlSegment,

        /// <summary>
        /// The parameter is part of the resulting URL query
        /// </summary>
        QueryString,

        /// <summary>
        /// The parameter will be sent as HTTP header
        /// </summary>
        HttpHeader,

        /// <summary>
        /// The parameter will be sent in the HTTP POST body
        /// </summary>
        RequestBody
    }
}
