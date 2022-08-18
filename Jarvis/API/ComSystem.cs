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
        /// <returns>A task returning whether or not the response was sent</returns>
        public static async Task<bool> SendJarvisResponse(string msg, string origin, long requestId)
        {
            bool result = await Jarvis.Service.TrySendResponse(msg, origin, requestId);
            if (!result) Log.Warning("Failed to send response.\nRequestId: " + requestId + "\nOrigin: " + origin);
            return result;
        }

        /// <summary>
        /// Queues a command to send to a blade via a BladeMsg.
        /// </summary>
        /// <param name="blade">The name of the blade</param>
        /// <param name="data">The message to send</param>
        /// <returns>Whether or not the command could be added</returns>
        public static bool QueueBladeCommand(string blade, string data)
        {
            bool result = Jarvis.Service.TrySendBladeCommand(blade, data);
            if (!result) Log.Warning("Failed to send blade command.\nOrigin: " + blade);
            return result;
        }

        /// <summary>
        /// Gets all the currently unfilled requests from JarvisLinker.
        /// </summary>
        /// <returns>Unfilled requests</returns>
        public static JarvisRequest[] Requests() => Jarvis.Service.GetUnfilledRequests().ToArray();

        /// <summary>
        /// Gets all current blade responses as an array.
        /// </summary>
        /// <returns>All blade responses</returns>
        public static BladeMsg[] BladeResponses() => Jarvis.Service.GetBladeResponses();

        /// <summary>
        /// Wipes the JarvisLinker database of all requests and responses.
        /// </summary>
        /// <returns>A task that returns whether or not the database was wiped</returns>
        public static async Task<bool> WipeDatabase()
        {
            bool result = await Jarvis.Service.TryWipeDatabase();
            if (!result) Log.Warning("Failed to delete all items from JarvisLinker.");
            return result;
        }

        /// <summary>
        /// Deletes blade commands and responses.
        /// </summary>
        /// <param name="blade">The blade to delete from</param>
        /// <param name="cmd">Whether or not to delete the command</param>
        /// <param name="response">Whether or not to delete the response</param>
        /// <returns>A task that specifies whether or not the messages were deleted</returns>
        public static async Task<bool> ConsumeBladeMsgs(string blade, bool cmd = true, bool response = true) =>
            await Jarvis.Service.ConsumeBladeMessages(blade, cmd, response);

        /// <summary>
        /// Marks a JarvisRequest as already utilized.
        /// </summary>
        /// <param name="requestId">The requestId to consume</param>
        /// <returns>Whether or not the request was already consumed</returns>
        public static bool ConsumeRequest(long requestId) => Jarvis.Service.ConsumeRequest(requestId);
    }
}
