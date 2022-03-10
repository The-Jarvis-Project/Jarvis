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
            kill, loadb, unloadb
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
            string[] split = request.Request.Split(new[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < split.Length; i++) split[i] = split[i].Trim();

            Command cmd;
            string cmdArg = split[0].ToLower();
            cmd = (Command)Enum.Parse(typeof(Command), cmdArg);
            string[] args = new string[split.Length - 1];
            for (int i = 0; i < args.Length; i++) args[i] = split[i + 1];
            return new CommandLine(cmd, args);
        }

        private void ProcessCommandLine(CommandLine cmd, long requestId)
        {
            if (cmd.Command == Command.kill)
            {
                Jarvis.Service.ForceStop();
                ComSystem.SendResponse("[kill] Killing Jarvis service", ResponseType.Text, requestId);
            }
            else if (cmd.Command == Command.loadb)
            {
                HotBehavior behavior = new HotBehavior(cmd.Args[0], cmd.Args[1]);
                hotLoadedBehaviors.Add(behavior);
                ComSystem.SendResponse("[loadb] Loaded " + behavior.Name, ResponseType.Text, requestId);
            }
            else if (cmd.Command == Command.unloadb)
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
                        i--;
                    }
                }
            }
        }
    }
}
