using Jarvis.API.Lang;
using Jarvis.Behaviors;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

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

    /// <summary>
    /// System for communicating with JarvisLinker.
    /// </summary>
    public static class ComSystem
    {
        /// <summary>
        /// Sends a response back to JarvisLinker from a request.
        /// </summary>
        /// <param name="msg">Response message</param>
        /// <param name="origin">Response origin</param>
        /// <param name="requestId">Id property of the JarvisRequest</param>
        public static async Task SendJarvisResponse(string msg, string origin, long requestId)
        {
            bool result = await Jarvis.Service.TrySendResponse(msg, origin, requestId);
            if (!result) Log.Warning("Failed to send response.\nRequestId: " + requestId + "\nOrigin: " + origin);
        }

        public static async Task SendBladeRequest(string data, string origin) =>
            throw new NotImplementedException();

        /// <summary>
        /// Gets all the currently unfilled requests from JarvisLinker.
        /// </summary>
        /// <returns>Unfilled requests</returns>
        public static JarvisRequest[] Requests() => Jarvis.Service.GetUnfilledRequests().ToArray();

        public static BladeMsg[] BladeResponses() => throw new NotImplementedException();

        /// <summary>
        /// Wipes the JarvisLinker database of all requests and responses.
        /// </summary>
        public static async Task WipeDatabase()
        {
            bool result = await Jarvis.Service.TryWipeDatabase();
            if (!result) Log.Warning("Failed to delete all items from JarvisLinker.");
        }
    }

    /// <summary>
    /// Hard-Coded functions for getting attributes of JarvisRequests.
    /// </summary>
    public static class Requests
    {
        /// <summary>
        /// Creates the request that is used internally when finding request attributes.
        /// </summary>
        /// <param name="request">The original request</param>
        /// <returns>The raw request</returns>
        public static JarvisRequest GetRaw(JarvisRequest request) =>
            new JarvisRequest()
            {
                Id = request.Id,
                Request = Raw(request)
            };

        /// <summary>
        /// Tests if a request is most likely a question (Case Insensitive).
        /// </summary>
        /// <param name="request">The request to check</param>
        /// <returns>Whether or not the request is most likely a question</returns>
        public static bool IsQuestion(JarvisRequest request)
        {
            string raw = Raw(request);
            return raw.Contains("?") ||
                raw.StartsWith("who") ||
                raw.StartsWith("what") ||
                raw.StartsWith("why") ||
                raw.StartsWith("where") ||
                raw.StartsWith("when") ||
                raw.StartsWith("which") ||
                raw.StartsWith("how");
        }

        /// <summary>
        /// Checks if a request has certain keywords, if it contains all of them it will return true (Case Insensitive).
        /// </summary>
        /// <param name="request">The request to check</param>
        /// <param name="keywords">The keywords to check for</param>
        /// <returns>Whether or not the request contains all the keywords</returns>
        public static bool HasKeywords(JarvisRequest request, params string[] keywords)
        {
            string[] words = GetWords(request);
            for (int i = 0; i < keywords.Length; i++)
            {
                bool hasKeyword = false;
                for (int w = 0; w < words.Length; w++)
                {
                    if (keywords[i] == words[w])
                    {
                        hasKeyword = true;
                        break;
                    }
                }
                if (!hasKeyword) return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the raw words in a request.
        /// </summary>
        /// <param name="request">The request to split up</param>
        /// <returns>All of the raw words in the request</returns>
        private static string[] GetWords(JarvisRequest request) =>
            Raw(request).Split(Word.separators, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Lowercases and trims a request.
        /// </summary>
        /// <param name="request">The request to convert</param>
        /// <returns>A raw string of the request's data</returns>
        private static string Raw(JarvisRequest request) => Polish(request).Request.ToLower().Trim();

        /// <summary>
        /// Removes irrelevant characters from the request.
        /// </summary>
        /// <param name="request">The request to fix</param>
        /// <returns>The cleaned up request</returns>
        public static JarvisRequest Polish(JarvisRequest request)
        {
            request.Request = request.Request.Replace(Environment.NewLine, "\n");
            return request;
        }
    }

    /// <summary>
    /// Class for getting a behavior's personal files.
    /// </summary>
    public static class FileSystem
    {
        private static string GetRelDir<T>(T behavior) where T : IBehaviorBase
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory,
                behaviorDir = baseDir + @"\Behaviors\" + behavior.GetType().Name;
            if (!Directory.Exists(behaviorDir)) Directory.CreateDirectory(behaviorDir);
            return behaviorDir;
        }

        private static string GetRelFile<T>(T behavior, string fileName) where T : IBehaviorBase =>
            GetRelDir(behavior) + "\\" + fileName;

        public static string GetFileText<T>(T behavior, string fileName) where T : IBehaviorBase
        {
            string file = GetRelFile(behavior, fileName);
            if (!File.Exists(file)) return null;
            return File.ReadAllText(file);
        }

        public static byte[] GetFileRaw<T>(T behavior, string fileName) where T : IBehaviorBase
        {
            string file = GetRelFile(behavior, fileName);
            if (!File.Exists(file)) return null;
            return File.ReadAllBytes(file);
        }

        public static bool ClearFiles<T>(T behavior) where T : IBehaviorBase
        {
            string behaviorDir = GetRelDir(behavior);
            string[] files = Directory.GetFiles(behaviorDir);
            for (int i = 0; i < files.Length; i++) File.Delete(files[i]);
            return true;
        }

        public static void WriteFileText<T>(T behavior, string fileName, string text) where T : IBehaviorBase =>
            File.WriteAllText(GetRelFile(behavior, fileName), text);

        public static void WriteFileBytes<T>(T behavior, string fileName, byte[] data) where T : IBehaviorBase =>
            File.WriteAllBytes(GetRelFile(behavior, fileName), data);
    }
}
