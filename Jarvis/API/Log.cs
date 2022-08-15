using System.Diagnostics;

namespace Jarvis.API
{
    /// <summary>
    /// For logging messages to the Jarvis event log.
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// Logs an info message to the event log.
        /// </summary>
        /// <param name="msg">Info message</param>
        public static void Info(string msg) => Jarvis.Service.Log(msg, EventLogEntryType.Information, 8);

        /// <summary>
        /// Logs a warning message to the event log.
        /// </summary>
        /// <param name="msg">Warning message</param>
        public static void Warning(string msg) => Jarvis.Service.Log(msg, EventLogEntryType.Warning, 9);

        /// <summary>
        /// Logs an error message to the event log.
        /// </summary>
        /// <param name="msg">Error message</param>
        public static void Error(string msg) => Jarvis.Service.Log(msg, EventLogEntryType.Error, 10);
    }
}
