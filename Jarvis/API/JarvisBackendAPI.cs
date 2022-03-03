using Microsoft.ML;
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

    /// <summary>
    /// Data type for JarvisResponse.
    /// </summary>
    public enum ResponseType
    {
        Text, Int, Float
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
        /// <param name="type">Response message value type, ex. floating point</param>
        /// <param name="requestId">Id property of the JarvisRequest</param>
        public static async void SendResponse(string msg, ResponseType type, long requestId)
        {
            string typeCode;
            switch (type)
            {
                case ResponseType.Int:
                    typeCode = "int";
                    break;
                case ResponseType.Float:
                    typeCode = "float";
                    break;
                default:
                    typeCode = "text";
                    break;
            }
            bool result = await Jarvis.Service.TrySendResponse(msg, typeCode, requestId);
            if (!result) Log.Warning("Failed to send response.\nRequestId: " + requestId + "\nType: " + type.ToString());
        }

        /// <summary>
        /// Gets all the currently unfilled requests from JarvisLinker.
        /// </summary>
        /// <returns>Unfilled requests</returns>
        public static JarvisRequest[] Requests() => Jarvis.Service.GetUnfilledRequests().ToArray();
    }

    /// <summary>
    /// Convenience functions for machine learning.
    /// </summary>
    public static class MLSystem
    {
        public static IEstimator<ITransformer> Featurize(string outputColumn, string inputColumn) =>
            Jarvis.mlContext.Transforms.Text.FeaturizeText(outputColumn, inputColumn);

        public static IEstimator<ITransformer> NormalizeFeaturize(string outputColumn, string inputColumn) =>
            Jarvis.mlContext.Transforms.Text.NormalizeText(outputColumn + "2", inputColumn)
            .Append(Jarvis.mlContext.Transforms.Text.FeaturizeText(outputColumn, outputColumn + "2"));

        public static IDataView LoadTxt<T>(string dataPath, char separator = '\t',
            bool header = false, bool quotes = false, bool trimSpace = false) =>
            Jarvis.mlContext.Data.LoadFromTextFile<T>(dataPath, separator, header, quotes, trimSpace);

        public static IDataView LoadCsv<T>(string dataPath,
            bool quotes = false, bool trimSpace = true, bool header = true) =>
            Jarvis.mlContext.Data.LoadFromTextFile<T>(dataPath, ',', header, quotes, trimSpace);
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
        /// Tests if a request is most likely a question.
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

        public static bool HasKeywords(JarvisRequest request)
        {
            
        }

        private static string Raw(JarvisRequest request) => request.Request.ToLower().Trim();
    }
}
