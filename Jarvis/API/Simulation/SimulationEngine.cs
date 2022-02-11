using System.Collections.Generic;

namespace Jarvis.API.Simulation
{
    public static class SimulationEngine
    {
        private static readonly Dictionary<string, World> worlds;

        private delegate void SimulateFunc();
        private static SimulateFunc simulateCallback;

        static SimulationEngine()
        {
            worlds = new Dictionary<string, World>();
        }

        public static void Simulate() => simulateCallback?.Invoke();

        public static World GetWorld(string name)
        {
            if (worlds.TryGetValue(name, out World world)) return world;
            return null;
        }

        public static void DestroyWorld(string name)
        {
            if (worlds.ContainsKey(name))
            {
                simulateCallback -= worlds[name].Simulate;
                worlds.Remove(name);
            }
        }

        public static bool CreateWorld(string name) => CreateWorld(name, Bounds.Infinite);

        public static bool CreateWorld(string name, Bounds bounds)
        {
            if (!worlds.ContainsKey(name))
            {
                worlds.Add(name, new World(bounds));
                simulateCallback += worlds[name].Simulate;
                return true;
            }
            return false;
        }
    }
}
