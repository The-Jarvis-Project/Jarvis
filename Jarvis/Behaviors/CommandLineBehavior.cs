using Jarvis.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jarvis.Behaviors
{
    public class CommandLineBehavior : IWebUpdate
    {
        public bool Enabled { get; set; } = true;
        public int Priority => 0;

        private enum Command
        {
            Kill, LoadB
        }

        private struct CommandLine
        {
            public Command Command { get; }
            public string[] Args { get; }

            public CommandLine(Command cmd, params string[] args)
            {
                Command = cmd;
                Args = args;
            }
        }

        public void WebUpdate()
        {
            JarvisRequest[] requests = ComSystem.Requests();
            for (int i = 0; i < requests.Length; i++)
            {
                if(requests[i].Request.StartsWith(""))
            }
        }

        private CommandLine CommandLine(JarvisRequest request) =>
    }
}
