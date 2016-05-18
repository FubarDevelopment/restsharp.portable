namespace RestSharp.Portable.OAuth1
{
    /// <summary>
    /// The interface a random number generator must implement
    /// </summary>
    public interface IRandom
    {
        /// <summary>
        /// Gets the next random value with <paramref name="minValue"/> &lt;= n &lt; <paramref name="maxValue"/>
        /// </summary>
        /// <param name="minValue">The minimum value (inclusive)</param>
        /// <param name="maxValue">The maximum value (exclusive)</param>
        /// <returns>the next random value</returns>
        int Next(int minValue, int maxValue);

        /// <summary>
        /// Gets the next <paramref name="count"/> random values with <paramref name="minValue"/> &lt;= n &lt; <paramref name="maxValue"/>
        /// </summary>
        /// <param name="minValue">The minimum value (inclusive)</param>
        /// <param name="maxValue">The maximum value (exclusive)</param>
        /// <param name="count">The number of random values to generate</param>
        /// <returns>the next random values</returns>
        int[] Next(int minValue, int maxValue, int count);
    }
}
