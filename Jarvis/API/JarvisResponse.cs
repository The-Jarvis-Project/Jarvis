namespace Jarvis.API
{
    /// <summary>
    /// Object used for transfering a response back to an interface.
    /// </summary>
    public class JarvisResponse
    {
        /// <summary>
        /// The response Id.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Where this response originated from.
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// The data of the response.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// The JarvisRequest.Id that this response is responding to.
        /// </summary>
        public long RequestId { get; set; }
    }

    /// <summary>
    /// (Object used for data transfer only.)
    /// </summary>
    public class JarvisResponseDTO
    {
        /// <summary>
        /// Where this response originated from.
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// The data of the response.
        /// </summary>
        public string Data { get; set; }

        /// <summary>
        /// The JarvisRequest.Id that this response is responding to.
        /// </summary>
        public long RequestId { get; set; }
    }
}
