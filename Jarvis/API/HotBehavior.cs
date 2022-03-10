using Jarvis.API;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;

namespace Jarvis.Behaviors
{
    /// <summary>
    /// Class for creating a behavior on the fly from a file.
    /// </summary>
    public class HotBehavior
    {
        private readonly Assembly compiledAssembly;
        private readonly object behaviorInstance;
        private readonly Type type;
        private readonly bool hasStart = false, hasStop = false,
            hasUpdate = false, hasWebUpdate = false;

        public string Name { get; }

        /// <summary>
        /// Loads a behavior from a file and stores the instance here.
        /// </summary>
        /// <param name="filePath">The file path to load</param>
        /// <param name="behaviorName">The full name of the behavior class</param>
        public HotBehavior(string filePath, string behaviorName)
        {
            Name = behaviorName;
            Dictionary<string, string> providerOptions = new Dictionary<string, string>
            {
                { "CompilerVersion", "v4.0" },
                { "LangVersion", "latest" }
            };
            CSharpCodeProvider codeProvider = new CSharpCodeProvider(providerOptions);

            CompilerParameters compilerParameters = new CompilerParameters
            {
                GenerateInMemory = true,
                GenerateExecutable = false,
                TreatWarningsAsErrors = false
            };
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
                compilerParameters.ReferencedAssemblies.Add(assemblies[i].Location);

            CompilerResults results = codeProvider.CompileAssemblyFromFile(compilerParameters, filePath);
            if (results.Errors.HasErrors || results.Errors.HasWarnings)
            {
                for (int i = 0; i < results.Errors.Count; i++)
                {
                    CompilerError error = results.Errors[i];
                    string errorText = error.ErrorText + "\nLine: " + error.Line + " Column: " + error.Column +
                        "\nFile: " + error.FileName;
                    if (error.IsWarning) Log.Warning(" - Hot Compile Warning - \n\n" + errorText);
                    else Log.Error("[! - Hot Compile Error - !]\n\n" + errorText);
                }
            }

            compiledAssembly = results.CompiledAssembly;
            behaviorInstance = compiledAssembly.CreateInstance(behaviorName);
            type = behaviorInstance.GetType();
            if (type.IsClass && !type.IsAbstract)
            {
                if (type.GetInterface(nameof(IStart)) != null)
                {
                    hasStart = true;
                    Jarvis.Service.HotLoading.UpdateStartBehavior(type as IStart);
                }
                if (type.GetInterface(nameof(IStop)) != null)
                {
                    hasStop = true;
                    Jarvis.Service.HotLoading.AddToStopBehaviors(type as IStop);
                }
                if (type.GetInterface(nameof(IUpdate)) != null)
                {
                    hasUpdate = true;
                    Jarvis.Service.HotLoading.AddToUpdateBehaviors(type as IUpdate);
                }
                if (type.GetInterface(nameof(IWebUpdate)) != null)
                {
                    hasWebUpdate = true;
                    Jarvis.Service.HotLoading.AddToWebBehaviors(type as IWebUpdate);
                }

                string loadedText = "Loaded " + type.Name +
                    " with active behaviors:\nStart: " + hasStart + "\nStop: " + hasStop +
                    "\nUpdate: " + hasUpdate + "\nWebUpdate: " + hasWebUpdate;
                Log.Info(loadedText);
            }
        }
    }
}
