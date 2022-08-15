using Jarvis.API.Lang;
using System;

namespace Jarvis.API
{
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
}
