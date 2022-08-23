using Jarvis.API;

namespace Jarvis.Behaviors
{
    public class RequestConsumeBehavior : IWebUpdate
    {
        public bool Enabled { get; set; } = true;
        public int Priority => int.MaxValue;

        public void WebUpdate()
        {
            JarvisRequest[] requests = ComSystem.Requests();
            for (int i = 0; i < requests.Length; i++)
                ComSystem.ConsumeRequest(requests[i].Id);
        }
    }
}
