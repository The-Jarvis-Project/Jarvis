namespace Jarvis.API
{
    /// <summary>
    /// Used for processing requests from an interface.
    /// </summary>
    public class JarvisRequest
    {
        /// <summary>
        /// The request Id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// The content sent in the request.
        /// </summary>
        public string Request { get; set; }
    }
}
