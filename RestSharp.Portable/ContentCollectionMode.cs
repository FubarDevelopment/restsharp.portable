namespace RestSharp.Portable
{
    /// <summary>
    /// Controls if basic content or multi part content is used
    /// </summary>
    public enum ContentCollectionMode
    {
        /// <summary>
        /// Basic content only (ignores file parameters)
        /// </summary>
        BasicContent,
        /// <summary>
        /// MultiPart only if file parameters are used
        /// </summary>
        MultiPartForFileParameters,
        /// <summary>
        /// Always use multi part content
        /// </summary>
        MultiPart
    }
}
