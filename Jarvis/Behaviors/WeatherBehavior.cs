using Jarvis.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jarvis.Behaviors
{
    public class WeatherBehavior : IWebUpdate
    {
        public bool Enabled { get; set; } = false;
        public int Priority => 150;

        public void WebUpdate()
        {
            JarvisRequest[] requests = ComSystem.Requests();
            if (requests.Length > 0)
            {
                
            }
        }
    }
}
