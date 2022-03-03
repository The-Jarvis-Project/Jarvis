using System.ServiceProcess;

namespace Jarvis
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main() => ServiceBase.Run(new Jarvis());
    }
}
