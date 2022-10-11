using Jarvis.API;

namespace Jarvis.Behaviors
{
    public class TestBehavior : IWebUpdate
    {
        public bool Enabled { get; set; } = true;
        public int Priority => 200;

        public void WebUpdate()
        {
            JarvisRequest[] requests = ComSystem.Requests();
            for (int i = 0; i < requests.Length; i++)
            {
                if (Requests.HasKeywords(requests[i], "music", "play"))
                {
                    string[] blades = ComSystem.BladeNames();
                    for (int b = 0; b < blades.Length; b++)
                        ComSystem.QueueBladeCommand(blades[b], "play music");

                    _ = ComSystem.SendJarvisResponse("Playing Music", "Jarvis", requests[i].Id);
                    ComSystem.ConsumeRequest(requests[i].Id);
                }
            }
        }
    }
}
