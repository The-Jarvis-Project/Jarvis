namespace Jarvis.Behaviors
{
    public interface IBehaviorBase
    {
        bool Enabled { get; set; }
        int Priority { get; }
    }

    public interface IUpdate : IBehaviorBase
    {
        void Update();
    }

    public interface IWebUpdate : IBehaviorBase
    {
        void WebUpdate();
    }

    public interface IStart : IBehaviorBase
    {
        void Start();
    }

    public interface IStop : IBehaviorBase
    {
        void Stop();
    }
}
