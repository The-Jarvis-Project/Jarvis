namespace Jarvis.API
{
    public class JarvisResponse
    {
        public long Id { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public long RequestId { get; set; }
    }

    public class JarvisResponseDTO
    {
        public string Type { get; set; }
        public string Data { get; set; }
        public long RequestId { get; set; }
    }
}
