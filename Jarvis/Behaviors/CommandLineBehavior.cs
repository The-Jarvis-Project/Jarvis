﻿using Jarvis.API;
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
            none, kill, load, unload, wipe, postblade
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
            BladeMsg[] bladeResponses = ComSystem.BladeResponses();
            for (int i = 0; i < requests.Length; i++)
                if (requests[i].Request.StartsWith("--"))
                    ProcessCmdLine(GetCmdLine(requests[i].Request), requests[i].Id, string.Empty);
            for (int i = 0; i < bladeResponses.Length; i++)
                if (bladeResponses[i].Data.StartsWith("--"))
                    ProcessCmdLine(GetCmdLine(bladeResponses[i].Data), -1, bladeResponses[i].Origin);
        }

        private CommandLine GetCmdLine(string text)
        {
            string[] split = text.Split('?');
            for (int i = 0; i < split.Length; i++) split[i] = split[i].Trim();

            Command cmd;
            string cmdArg = split[0].ToLower().Replace("--", string.Empty);
            cmd = (Command)Enum.Parse(typeof(Command), cmdArg);
            string[] args = new string[split.Length - 1];
            for (int i = 0; i < args.Length; i++) args[i] = split[i + 1];
            return new CommandLine(cmd, args);
        }

        private async void ProcessCmdLine(CommandLine cmd, long requestId, string blade)
        {
            if (cmd.Command == Command.kill)
            {
                await ComSystem.SendJarvisResponse("[kill] Killing Jarvis service and wiping database",
                    "Jarvis", requestId);
                await ComSystem.WipeDatabase();
                Jarvis.Service.ForceStop();
            }
            else if (cmd.Command == Command.load)
            {
                if (cmd.Args.Length == 2 && cmd.Args[0].EndsWith(".cs"))
                {
                    HotBehavior behavior = new HotBehavior(cmd.Args[0], cmd.Args[1]);
                    hotLoadedBehaviors.Add(behavior);
                    await ComSystem.SendJarvisResponse("[load] Loaded " + behavior.Name, "Jarvis", requestId);
                }
                else await ComSystem.SendJarvisResponse("[load] Invalid arguments!", "Jarvis", requestId);
            }
            else if (cmd.Command == Command.unload)
            {
                for (int i = 0; i < hotLoadedBehaviors.Count; i++)
                {
                    if (cmd.Args.Length > 0 && hotLoadedBehaviors[i].Name == cmd.Args[0])
                    {
                        Log.Warning(hotLoadedBehaviors[i].Name + " is being removed");
                        if (hotLoadedBehaviors[i].HasStop)
                        {
                            Jarvis.HotLoading.RemoveFromStop(hotLoadedBehaviors[i].Name);
                            Log.Warning(hotLoadedBehaviors[i].Name + " removed from stop");
                        }
                        if (hotLoadedBehaviors[i].HasUpdate)
                        {
                            Jarvis.HotLoading.RemoveFromUpdate(hotLoadedBehaviors[i].Name);
                            Log.Warning(hotLoadedBehaviors[i].Name + " removed from update");
                        }
                        if (hotLoadedBehaviors[i].HasWebUpdate)
                        {
                            Jarvis.HotLoading.RemoveFromWeb(hotLoadedBehaviors[i].Name);
                            Log.Warning(hotLoadedBehaviors[i].Name + " removed from web update");
                        }
                        hotLoadedBehaviors.RemoveAt(i);
                        Log.Warning(hotLoadedBehaviors[i].Name + " removed");
                        try
                        {
                            await ComSystem.SendJarvisResponse("[unload] Unload " + hotLoadedBehaviors[i].Name,
                                "Jarvis", requestId);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message);
                        }
                        Log.Warning(hotLoadedBehaviors[i].Name + " sent response");
                        i--;
                    }
                }
            }
            else if (cmd.Command == Command.wipe) await ComSystem.WipeDatabase();
            else if (cmd.Command == Command.postblade && !string.IsNullOrEmpty(blade))
            {
                if (cmd.Args.Length == 2)
                {
                    bool successful = Jarvis.Service.TrackBlade(cmd.Args[0], cmd.Args[1]);
                    Log.Info("Blade: " + cmd.Args[0] + "\nPosted: " + successful);
                    await ComSystem.ConsumeBladeMsgs(blade, true, true);
                    ComSystem.QueueBladeCommand(blade, "posted blade: " + successful);
                }
            }
        }
    }
}
