using System.ServiceProcess;

namespace Jarvis
{
    /// <summary>
    /// Wrapper for starting the Jarvis service
    /// </summary>
    public static class Startup
    {
        /// <summary>
        /// Starts the Jarvis service.
        /// </summary>
        public static void Main() => ServiceBase.Run(new Jarvis());
    }
}
