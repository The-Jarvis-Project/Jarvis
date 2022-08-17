using Jarvis.Behaviors;
using Jarvis.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Timers;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Text;
using Microsoft.ML;
using System.Linq;

namespace Jarvis
{
    /// <summary>
    /// State of the Jarvis Windows service.
    /// (Used Internally)
    /// </summary>
    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    /// <summary>
    /// (Used Internally)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };

    /// <summary>
    /// The Jarvis Windows service class.  Contains critical functions.
    /// </summary>
    public partial class Jarvis : ServiceBase
    {
        private const int updateMs = 100, logMs = 90000, webRequestMs = 2000;
        private Timer updateTimer, logTimer, webRequestTimer;
        private ServiceState state;

        private string requestsJson, responsesJson;
        private List<JarvisRequest> requests;
        private List<JarvisResponse> responses;
        private readonly List<JarvisRequest> unfilledRequests;

        private string bladeCmdsJson, bladeResponsesJson;
        private readonly HashSet<string> trackedBlades;
        private readonly Dictionary<string, Queue<BladeMsg>> bladeCmdQueue;
        private readonly Dictionary<string, BladeMsg> bladeCmds, bladeResponses;

        private static Jarvis singleton;
        private static readonly HttpClient client = new HttpClient();
        private static readonly string 
            requestUrl = "https://jarvislinker.azurewebsites.net/api/JarvisRequests",
            responseUrl = "https://jarvislinker.azurewebsites.net/api/JarvisResponses",
            bladeCmdURL = "https://jarvislinker.azurewebsites.net/api/BladeCommands",
            bladeResponseURL = "https://jarvislinker.azurewebsites.net/api/BladeResponses";

        private readonly List<IUpdate> updateBehaviors;
        private readonly List<IWebUpdate> webBehaviors;
        private readonly List<IStart> startBehaviors;
        private readonly List<IStop> stopBehaviors;

        private readonly List<string> updateNames;
        private readonly List<string> webNames;
        private readonly List<string> startNames;
        private readonly List<string> stopNames;

        /// <summary>
        /// The MLContext used for all Jarvis ML.Net machine learning functions.
        /// </summary>
        public static readonly MLContext mlContext = new MLContext(100);

        /// <summary>
        /// Create a new Jarvis service instance.
        /// </summary>
        public Jarvis()
        {
            InitializeComponent();
            singleton = this;
            eventLog = new EventLog();
            requestsJson = string.Empty;
            responsesJson = string.Empty;
            if (!EventLog.SourceExists("JarvisEventSource"))
                EventLog.CreateEventSource("JarvisEventSource", "JarvisLog");
            eventLog.Source = "JarvisEventSource";
            eventLog.Log = "JarvisLog";
            eventLog.Clear();
            eventLog.WriteEntry("Initialized Service", EventLogEntryType.Information, 0);
            state = ServiceState.SERVICE_STOPPED;

            string behaviorsText = string.Empty;
            updateBehaviors = new List<IUpdate>();
            webBehaviors = new List<IWebUpdate>();
            startBehaviors = new List<IStart>();
            stopBehaviors = new List<IStop>();
            updateNames = new List<string>();
            webNames = new List<string>();
            startNames = new List<string>();
            stopNames = new List<string>();
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].IsClass && types[i].GetInterface(nameof(IBehaviorBase)) != null)
                {
                    object inst = Activator.CreateInstance(types[i]);
                    if (types[i].GetInterface(nameof(IUpdate)) != null)
                    {
                        updateBehaviors.Add(inst as IUpdate);
                        behaviorsText += "\n" + types[i].Name + " (Update)";
                    }
                    if (types[i].GetInterface(nameof(IWebUpdate)) != null)
                    {
                        webBehaviors.Add(inst as IWebUpdate);
                        behaviorsText += "\n" + types[i].Name + " (WebUpdate)";
                    }
                    if (types[i].GetInterface(nameof(IStart)) != null)
                    {
                        startBehaviors.Add(inst as IStart);
                        behaviorsText += "\n" + types[i].Name + " (Start)";
                    }
                    if (types[i].GetInterface(nameof(IStop)) != null)
                    {
                        stopBehaviors.Add(inst as IStop);
                        behaviorsText += "\n" + types[i].Name + " (Stop)";
                    }
                }
            }
            updateBehaviors.Sort(CompareBehaviors);
            webBehaviors.Sort(CompareBehaviors);
            startBehaviors.Sort(CompareBehaviors);
            stopBehaviors.Sort(CompareBehaviors);
            eventLog.WriteEntry("Initialized Behaviors: " + behaviorsText, EventLogEntryType.Information, 6);

            requests = new List<JarvisRequest>();
            unfilledRequests = new List<JarvisRequest>();
            responses = new List<JarvisResponse>();

            trackedBlades = new HashSet<string>();
            bladeCmds = new Dictionary<string, BladeMsg>();
            bladeResponses = new Dictionary<string, BladeMsg>();
            bladeCmdQueue = new Dictionary<string, Queue<BladeMsg>>();

            Assembly[] refed = AppDomain.CurrentDomain.GetAssemblies();
            string referencedAssemblies = "Referenced Assemblies:";
            for (int i = 0; i < refed.Length; i++) referencedAssemblies += "\n" + refed[i].GetName().Name;
            eventLog.WriteEntry(referencedAssemblies, EventLogEntryType.Information, 9);
        }

        private int CompareBehaviors(IBehaviorBase x, IBehaviorBase y)
        {
            if (x.Priority < y.Priority) return -1;
            else if (x.Priority > y.Priority) return 1;
            return 0;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        protected override void OnStart(string[] args)
        {
            ServiceStatus serviceStatus = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_START_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(ServiceHandle, ref serviceStatus);
            state = ServiceState.SERVICE_START_PENDING;

            try
            {
                for (int i = 0; i < startBehaviors.Count; i++)
                    if (startBehaviors[i].Enabled)
                        startBehaviors[i].Start();
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry("Error On Start: " + ex.Message + "\n\n" +
                    ex.Source + "\n\n" + ex.Data + "\n\n" + ex.StackTrace, EventLogEntryType.Error, 9);
            }

            updateTimer = new Timer(updateMs);
            logTimer = new Timer(logMs);
            webRequestTimer = new Timer(webRequestMs);
            updateTimer.Start();
            logTimer.Start();
            webRequestTimer.Start();
            updateTimer.Elapsed += UpdateTimer_Elapsed;
            logTimer.Elapsed += LogTimer_Elapsed;
            webRequestTimer.Elapsed += WebRequestTimer_Elapsed;
            eventLog.WriteEntry("Started", EventLogEntryType.Information, 1);

            state = ServiceState.SERVICE_RUNNING;
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            ServiceStatus serviceStatus = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_STOP_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(ServiceHandle, ref serviceStatus);
            state = ServiceState.SERVICE_STOP_PENDING;

            updateTimer.Dispose();
            logTimer.Dispose();
            webRequestTimer.Dispose();

            try
            {
                for (int i = 0; i < stopBehaviors.Count; i++)
                    if (stopBehaviors[i].Enabled)
                        stopBehaviors[i].Stop();

                client.Dispose();
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry("Error On Stop: " + ex.Message + "\n\n" +
                    ex.Source + "\n\n" + ex.Data + "\n\n" + ex.StackTrace, EventLogEntryType.Error, 10);
            }
            eventLog.WriteEntry("Stopped", EventLogEntryType.Information, 2);

            state = ServiceState.SERVICE_STOPPED;
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }

        protected override void OnPause()
        {
            ServiceStatus serviceStatus = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_PAUSE_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(ServiceHandle, ref serviceStatus);
            state = ServiceState.SERVICE_PAUSE_PENDING;

            updateTimer.Stop();
            logTimer.Stop();
            webRequestTimer.Stop();
            eventLog.WriteEntry("Paused", EventLogEntryType.Information, 3);

            state = ServiceState.SERVICE_PAUSED;
            serviceStatus.dwCurrentState = ServiceState.SERVICE_PAUSED;
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }

        protected override void OnContinue()
        {
            ServiceStatus serviceStatus = new ServiceStatus
            {
                dwCurrentState = ServiceState.SERVICE_CONTINUE_PENDING,
                dwWaitHint = 100000
            };
            SetServiceStatus(ServiceHandle, ref serviceStatus);
            state = ServiceState.SERVICE_CONTINUE_PENDING;

            updateTimer.Start();
            logTimer.Start();
            webRequestTimer.Start();
            eventLog.WriteEntry("Unpaused", EventLogEntryType.Information, 4);

            state = ServiceState.SERVICE_RUNNING;
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                for (int i = 0; i < updateBehaviors.Count; i++)
                    if (updateBehaviors[i].Enabled)
                        updateBehaviors[i].Update();
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry("Error On Update: " + ex.Message + "\n\n" +
                    ex.Source + "\n\n" + ex.Data + "\n\n" + ex.StackTrace, EventLogEntryType.Error, 11);
            }
        }

        private async void WebRequestTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            webRequestTimer.Stop();
            HttpResponseMessage requestMsg = await client.GetAsync(requestUrl),
                responseMsg = await client.GetAsync(responseUrl),
                bladeCmdMsg = await client.GetAsync(bladeCmdURL),
                bladeResponseMsg = await client.GetAsync(bladeResponseURL);
            if (requestMsg.IsSuccessStatusCode && responseMsg.IsSuccessStatusCode)
            {
                try
                {
                    requestsJson = await requestMsg.Content.ReadAsStringAsync();
                    responsesJson = await responseMsg.Content.ReadAsStringAsync();
                    bladeCmdsJson = await bladeCmdMsg.Content.ReadAsStringAsync();
                    bladeResponsesJson = await bladeResponseMsg.Content.ReadAsStringAsync();

                    requests = JsonConvert.DeserializeObject<List<JarvisRequest>>(requestsJson);
                    responses = JsonConvert.DeserializeObject<List<JarvisResponse>>(responsesJson);
                    List<BladeMsg> bladeCmdList = JsonConvert.DeserializeObject<List<BladeMsg>>(bladeCmdsJson),
                        bladeResponseList = JsonConvert.DeserializeObject<List<BladeMsg>>(bladeResponsesJson);

                    for (int i = 0; i < bladeCmdList.Count; i++)
                    {
                        if (trackedBlades.Contains(bladeCmdList[i].Origin))
                            bladeCmds[bladeCmdList[i].Origin] = bladeCmdList[i];
                        else
                        {
                            Service.TrackBlade(bladeCmdList[i].Origin);
                            bladeCmds[bladeCmdList[i].Origin] = bladeCmdList[i];
                        }
                    }
                    for (int i = 0; i < bladeResponseList.Count; i++)
                    {
                        if (trackedBlades.Contains(bladeResponseList[i].Origin))
                            bladeResponses[bladeResponseList[i].Origin] = bladeResponseList[i];
                        else
                        {
                            Service.TrackBlade(bladeResponseList[i].Origin);
                            bladeResponses[bladeResponseList[i].Origin] = bladeResponseList[i];
                        }
                    }



                    unfilledRequests.Clear();
                    HashSet<long> filledRequests = new HashSet<long>();
                    for (int i = 0; i < responses.Count; i++)
                        if (!filledRequests.Contains(responses[i].RequestId))
                            filledRequests.Add(responses[i].RequestId);
                    for (int i = 0; i < requests.Count; i++)
                        if (!filledRequests.Contains(requests[i].Id))
                            unfilledRequests.Add(requests[i]);

                    for (int i = 0; i < webBehaviors.Count; i++)
                        if (webBehaviors[i].Enabled)
                            webBehaviors[i].WebUpdate();
                }
                catch (Exception ex)
                {
                    eventLog.WriteEntry("Error On WebUpdate: " + ex.Message + "\n\n" + 
                        ex.Source + "\n\n" + ex.Data + "\n\n" + ex.StackTrace, EventLogEntryType.Error, 12);
                }
            }
            else eventLog.WriteEntry("Web Update Failed\nCode: " + 
                requestMsg.StatusCode.ToString(), EventLogEntryType.Warning, 7);
            webRequestTimer.Start();
        }

        private void LogTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            string log = "- Status Check -\nState: " + state.ToString() +
                "\nBehaviors: " + (updateBehaviors.Count + webBehaviors.Count + 
                startBehaviors.Count + stopBehaviors.Count) +
                "\n" + requests.Count + " Requests:\n" + requestsJson +
                "\n\n" + responses.Count + " Responses:\n" + responsesJson +
                "\n\n(" + unfilledRequests.Count + " Unfilled Requests)";
            eventLog.WriteEntry(log, EventLogEntryType.Information, 5);
        }

        private async Task<bool> SendResponse(string data, string origin, long requestId)
        {
            JarvisResponseDTO dto = new JarvisResponseDTO
            {
                Origin = origin,
                Data = data,
                RequestId = requestId
            };
            string json = JsonConvert.SerializeObject(dto);
            StringContent jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
            return (await client.PostAsync(responseUrl, jsonContent)).IsSuccessStatusCode;
        }

        private async Task<bool> ClearRequests()
        {
            bool isSuccess = true;
            for (int i = 0; i < requests.Count; i++)
            {
                string delUrl = requestUrl + "/" + requests[i].Id;
                bool success = (await client.DeleteAsync(delUrl)).IsSuccessStatusCode;
                if (!success) isSuccess = false;
            }
            return isSuccess;
        }

        private async Task<bool> ClearResponses()
        {
            bool isSuccess = true;
            for (int i = 0; i < responses.Count; i++)
            {
                string delUrl = responseUrl + "/" + responses[i].Id;
                bool success = (await client.DeleteAsync(delUrl)).IsSuccessStatusCode;
                if (!success) isSuccess = false;
            }
            return isSuccess;
        }

        private async Task<bool> SendBladeCommand(string origin, string data)
        {
            BladeMsg dto = new BladeMsg
            {
                Origin = origin,
                Data = data,
            };
            string json = JsonConvert.SerializeObject(dto);
            StringContent jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
            return (await client.PostAsync(bladeCmdURL, jsonContent)).IsSuccessStatusCode;
        }

        private async Task<bool> DeleteBladeCommand(string blade)
        {
            if (trackedBlades.Contains(blade))
            {
                string delUrl = bladeCmdURL + "/" + blade;
                return (await client.DeleteAsync(delUrl)).IsSuccessStatusCode;
            }
            return false;
        }

        private async Task<bool> DeleteBladeResponse(string blade)
        {
            if (trackedBlades.Contains(blade))
            {
                string delUrl = bladeResponseURL + "/" + blade;
                return (await client.DeleteAsync(delUrl)).IsSuccessStatusCode;
            }
            return false;
        }

        /// <summary>
        /// Link to low level Jarvis service functions.
        /// </summary>
        public static class Service
        {
            /// <summary>
            /// Logs an event to the Javis event log.
            /// </summary>
            /// <param name="msg">Log message</param>
            /// <param name="type">Type of log message</param>
            /// <param name="code">Number code of the log message</param>
            public static void Log(string msg, 
                EventLogEntryType type = EventLogEntryType.Information, int code = 8) =>
                singleton.eventLog.WriteEntry(msg, type, code);

            /// <summary>
            /// Forces the Jarvis service to stop.
            /// </summary>
            public static void ForceStop() => singleton.Stop();

            /// <summary>
            /// Gets the JarvisRequest objects from the last API ping that haven't been responded too.
            /// </summary>
            /// <returns>Current JarvisRequest list</returns>
            public static List<JarvisRequest> GetUnfilledRequests() => singleton.unfilledRequests;

            /// <summary>
            /// Trys to send a response for a specified JarvisRequest.
            /// </summary>
            /// <param name="data">The data of the request</param>
            /// <param name="origin">The origin of the response</param>
            /// <param name="requestId">The id of the request</param>
            /// <returns>If the reponse was sent successfully</returns>
            public static async Task<bool> TrySendResponse(string data, string origin, long requestId)
            {
                bool canSend = false;
                int requestIndex = -1;
                for (int i = 0; i < singleton.unfilledRequests.Count; i++)
                {
                    if (singleton.unfilledRequests[i].Id == requestId)
                    {
                        canSend = true;
                        requestIndex = i;
                        break;
                    }
                }

                if (canSend)
                {
                    bool completed = await singleton.SendResponse(data, origin, requestId);
                    if (completed)
                    {
                        singleton.unfilledRequests.RemoveAt(requestIndex);
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Trys to clear all the requests and responses from JarvisLinker.
            /// </summary>
            /// <returns>Whether or not all the requests and responses were deleted</returns>
            public static async Task<bool> TryWipeDatabase()
            {
                bool requestsDel = await singleton.ClearRequests();
                bool responsesDel = await singleton.ClearResponses();
                return requestsDel && responsesDel;
            }

            /// <summary>
            /// Adds a blade to the set of blades this service can communicate with.
            /// </summary>
            /// <param name="blade">The name of the blade to add</param>
            /// <returns>Whether or not the blade could be added</returns>
            public static bool TrackBlade(string blade)
            {
                if (!singleton.trackedBlades.Contains(blade))
                {
                    singleton.trackedBlades.Add(blade);
                    singleton.bladeCmds.Add(blade, new BladeMsg()
                    {
                        Origin = blade,
                        Data = "--postblade",
                    });
                    singleton.bladeResponses.Add(blade, new BladeMsg()
                    {
                        Origin = blade,
                        Data = string.Empty,
                    });
                    singleton.bladeCmdQueue.Add(blade, new Queue<BladeMsg>());
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Removes a blade from the set of tracked blades.
            /// </summary>
            /// <param name="blade">The name of the blade to remove</param>
            /// <returns>Whether or not the blade could be removed</returns>
            public static bool RetractBlade(string blade)
            {
                if (singleton.trackedBlades.Contains(blade))
                {
                    singleton.trackedBlades.Remove(blade);
                    singleton.bladeCmds.Remove(blade);
                    singleton.bladeResponses.Remove(blade);
                    singleton.bladeCmdQueue.Remove(blade);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Gets all current blade responses in the form of an array.
            /// </summary>
            /// <returns>All current blade responses</returns>
            public static BladeMsg[] GetBladeResponses() => singleton.bladeResponses.Values.ToArray();

            /// <summary>
            /// Trys to send a command to a blade.
            /// </summary>
            /// <param name="blade">The name of the blade to send the command to</param>
            /// <param name="data">The message to send</param>
            /// <returns>Whether or not the blade is in the set of tracked blades</returns>
            public static bool TrySendBladeCommand(string blade, string data)
            {
                if (singleton.trackedBlades.Contains(blade))
                {
                    BladeMsg command = new BladeMsg
                    {
                        Origin = blade,
                        Data = data
                    };
                    singleton.bladeCmdQueue[blade].Enqueue(command);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Deletes blade commands and responses.
            /// </summary>
            /// <param name="blade">The blade to delete from</param>
            /// <param name="cmd">Whether or not to delete the command</param>
            /// <param name="response">Whether or not to delete the response</param>
            /// <returns>A task that specifies whether or not the messages were deleted</returns>
            public static async Task<bool> ConsumeBladeMessages(string blade, bool cmd, bool response)
            {
                bool delCmd, delResponse;
                if (cmd) delCmd = await singleton.DeleteBladeCommand(blade);
                else delCmd = true;

                if (response) delResponse = await singleton.DeleteBladeResponse(blade);
                else delResponse = true;
                return delCmd && delResponse;
            }
        }

        /// <summary>
        /// (Used internally for hot loading custom behaviors.)
        /// </summary>
        public static class HotLoading
        {
            /// <summary>
            /// Adds a function to the stop loop.
            /// </summary>
            /// <param name="name">Name of the behavior</param>
            /// <param name="obj">The function to add</param>
            public static void AddToStopBehaviors(string name, IStop obj)
            {
                singleton.stopNames.Add(name);
                singleton.stopBehaviors.Add(obj);
                singleton.stopBehaviors.Sort(singleton.CompareBehaviors);
            }

            /// <summary>
            /// Adds a function to the main update loop.
            /// </summary>
            /// <param name="name">Name of the behavior</param>
            /// <param name="obj">The function to add</param>
            public static void AddToUpdateBehaviors(string name, IUpdate obj)
            {
                singleton.updateNames.Add(name);
                singleton.updateBehaviors.Add(obj);
                singleton.updateBehaviors.Sort(singleton.CompareBehaviors);
            }

            /// <summary>
            /// Adds a function to the web update loop.
            /// </summary>
            /// <param name="name">Name of the behavior</param>
            /// <param name="obj">The function to add</param>
            public static void AddToWebBehaviors(string name, IWebUpdate obj)
            {
                singleton.webNames.Add(name);
                singleton.webBehaviors.Add(obj);
                singleton.webBehaviors.Sort(singleton.CompareBehaviors);
            }

            /*public static void AddToWebBehaviors(string name, 
             * PropertyInfo enabled, MethodInfo method, object obj)
            {
                bool isEnabled = (bool)enabled.GetValue(obj);
                singleton.hotWebBehaviors.Add((name, isEnabled, method, obj));
            }*/

            /// <summary>
            /// Removes a behavior from the stop loop.
            /// </summary>
            /// <param name="name">The name of the behavior to remove</param>
            public static void RemoveFromStop(string name)
            {
                for (int i = 0; i < singleton.stopNames.Count; i++)
                {
                    if (singleton.stopNames[i] == name)
                    {
                        singleton.stopBehaviors.RemoveAt(i);
                        singleton.stopNames.RemoveAt(i);
                        i--;
                    }
                }
            }

            /// <summary>
            /// Removes a behavior from the update loop.
            /// </summary>
            /// <param name="name">The name of the behavior to remove</param>
            public static void RemoveFromUpdate(string name)
            {
                for (int i = 0; i < singleton.updateNames.Count; i++)
                {
                    if (singleton.updateNames[i] == name)
                    {
                        singleton.updateBehaviors.RemoveAt(i);
                        singleton.updateNames.RemoveAt(i);
                        i--;
                    }
                }
            }

            /// <summary>
            /// Removes a behavior from the web update loop.
            /// </summary>
            /// <param name="name">The name of the behavior to remove</param>
            public static void RemoveFromWeb(string name)
            {
                for (int i = 0; i < singleton.webNames.Count; i++)
                {
                    if (singleton.webNames[i] == name)
                    {
                        singleton.webBehaviors.RemoveAt(i);
                        singleton.webNames.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
    }
}
