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
            string fileTxt = FileSystem.GetFileText(this, "TextJarvisFile.txt");
            Log.Info(fileTxt);
        }
    }
}
