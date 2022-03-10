using Jarvis.API;
using System;
using System.Collections.Generic;

namespace Jarvis.Behaviors
{
    public class CommandLineBehavior : IStart, IWebUpdate
    {
        public bool Enabled { get; set; } = true;
        public int Priority => 0;

        private enum Command
        {
            none, kill, load, unload, wipe
        }

        private struct CommandLine
        {
            public Command Command { get; }
            public string[] Args { get; }

            public CommandLine(Command cmd, string[] args)
            {
                Command = cmd;
                Args = args;
            }
        }

        private List<HotBehavior> hotLoadedBehaviors;

        public void Start()
        {
            hotLoadedBehaviors = new List<HotBehavior>();
        }

        public void WebUpdate()
        {
            JarvisRequest[] requests = ComSystem.Requests();
            for (int i = 0; i < requests.Length; i++)
                if (requests[i].Request.StartsWith("--"))
                    ProcessCommandLine(GetCommandLine(requests[i]), requests[i].Id);
        }

        private CommandLine GetCommandLine(JarvisRequest request)
        {
            string[] split = request.Request.Split('?');
            for (int i = 0; i < split.Length; i++) split[i] = split[i].Trim();

            Command cmd;
            string cmdArg = split[0].ToLower().Replace("--", string.Empty);
            cmd = (Command)Enum.Parse(typeof(Command), cmdArg);
            string[] args = new string[split.Length - 1];
            for (int i = 0; i < args.Length; i++) args[i] = split[i + 1];
            return new CommandLine(cmd, args);
        }

        private async void ProcessCommandLine(CommandLine cmd, long requestId)
        {
            if (cmd.Command == Command.kill)
            {
                await ComSystem.SendResponse("[kill] Killing Jarvis service and wiping database", ResponseType.Text, requestId);
                await ComSystem.WipeDatabase();
                Jarvis.Service.ForceStop();
            }
            else if (cmd.Command == Command.load)
            {
                if (cmd.Args.Length == 2 && cmd.Args[0].EndsWith(".cs"))
                {
                    HotBehavior behavior = new HotBehavior(cmd.Args[0], cmd.Args[1]);
                    hotLoadedBehaviors.Add(behavior);
                    await ComSystem.SendResponse("[load] Loaded " + behavior.Name, ResponseType.Text, requestId);
                }
                else await ComSystem.SendResponse("[load] Invalid arguments!", ResponseType.Text, requestId);
            }
            else if (cmd.Command == Command.unload)
            {
                for (int i = 0; i < hotLoadedBehaviors.Count; i++)
                {
                    if (hotLoadedBehaviors[i].Name == cmd.Args[0])
                    {
                        if (hotLoadedBehaviors[i].HasStop)
                            Jarvis.Service.HotLoading.RemoveFromStop(hotLoadedBehaviors[i].Name);
                        if (hotLoadedBehaviors[i].HasUpdate)
                            Jarvis.Service.HotLoading.RemoveFromUpdate(hotLoadedBehaviors[i].Name);
                        if (hotLoadedBehaviors[i].HasWebUpdate)
                            Jarvis.Service.HotLoading.RemoveFromWeb(hotLoadedBehaviors[i].Name);
                        hotLoadedBehaviors.RemoveAt(i);
                        await ComSystem.SendResponse("[unload] Unload " + hotLoadedBehaviors[i].Name,
                            ResponseType.Text, requestId);
                        i--;
                    }
                }
            }
            else if (cmd.Command == Command.wipe) await ComSystem.WipeDatabase();
        }
    }
}
