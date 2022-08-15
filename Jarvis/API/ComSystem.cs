using System;
using System.Threading.Tasks;

namespace Jarvis.API
{
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
}
