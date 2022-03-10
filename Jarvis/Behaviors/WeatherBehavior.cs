using Jarvis.API;

namespace Jarvis.Behaviors
{
    public class WeatherBehavior : IWebUpdate
    {
        public bool Enabled { get; set; } = false;
        public int Priority => 150;

        public void WebUpdate()
        {
            JarvisRequest[] requests = ComSystem.Requests();
            for (int i = 0; i < requests.Length; i++)
            {
                if (Requests.IsQuestion(requests[i]) &&
                    Requests.HasKeywords(requests[i], "weather"))
                {
                    string msg = "Receieved Weather Request";
                    _ = ComSystem.SendResponse(msg, ResponseType.Text, requests[i].Id);
                }
            }
        }
    }
}
