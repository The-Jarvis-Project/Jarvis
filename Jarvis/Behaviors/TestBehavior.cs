using Jarvis.API;

namespace Jarvis.Behaviors
{
    public class TestBehavior : IStart, IWebUpdate
    {
        public bool Enabled { get; set; } = true;
        public int Priority => 200;

        public void Start()
        {
            FileSystem.WriteFileText(this, "TestJarvisFile.txt", "Test text");
        }

        public void WebUpdate()
        {
            //string fileTxt = FileSystem.GetFileText(this, "TextJarvisFile.txt");
            //Log.Info("Test File Text: " + fileTxt);
            JarvisRequest[] requests = ComSystem.Requests();
            for (int i = 0; i < requests.Length; i++)
            {
                if (Requests.HasKeywords(requests[i], "music", "play"))
                {
                    _ = ComSystem.SendJarvisResponse("Playing Music", "Jarvis", requests[i].Id);
                }
            }
        }
    }
}
