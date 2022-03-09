using System;
using System.Device.Location;

namespace Jarvis.API
{
    public static class Utilities
    {
        public static (double, double) Location()
        {
            GeoCoordinateWatcher watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default);
            if (watcher.TryStart(false, TimeSpan.FromSeconds(10)))
                return (watcher.Position.Location.Latitude, watcher.Position.Location.Longitude);
            Log.Warning("Could not start location watcher!");
            watcher.Stop();
            watcher.Dispose();
            return (0, 0);
        }
    }
}
