﻿namespace Jarvis.Behaviors
{
    public class TestHotBehaviorBehavior : IStart
    {
        public bool Enabled { get; set; } = false;
        public int Priority => 200;

        public void Start()
        {
            HotBehavior testHotBehavior = new HotBehavior
                (@"C:\Users\rexjw\Documents\TestJarvisBehaviors\WeatherBehavior2.cs",
                "Jarvis.Behaviors.WeatherBehavior2");
        }
    }
}
