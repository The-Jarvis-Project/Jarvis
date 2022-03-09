using Jarvis.API;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jarvis.Behaviors
{
    public class HotBehavior
    {
        private Assembly compiledAssembly;
        private object behaviorInstance;
        private Type type;
        private MethodInfo[] methods;
        private bool hasStart, hasStop, hasUpdate, hasWebUpdate;

        private IStart startContainer;
        private IStop stopContainer;
        private IUpdate updateContainer;
        private IWebUpdate webUpdateContainer;

        public HotBehavior(string filePath, string behaviorName)
        {
            Dictionary<string, string> providerOptions = new Dictionary<string, string>
            {
                { "CompilerVersion", "3.5" }
            };
            CSharpCodeProvider codeProvider = new CSharpCodeProvider(providerOptions);

            CompilerParameters compilerParameters = new CompilerParameters();
            compilerParameters.GenerateInMemory = true;
            compilerParameters.GenerateExecutable = false;
            compilerParameters.TreatWarningsAsErrors = false;

            CompilerResults results = codeProvider.CompileAssemblyFromFile(compilerParameters, filePath);
            if (results.Errors.HasErrors || results.Errors.HasWarnings)
            {
                for (int i = 0; i < results.Errors.Count; i++)
                {
                    if (results.Errors[i].IsWarning)
                        Log.Warning(" - Hot Compile Warning - \n\n" + results.Errors[i].ErrorText);
                    else Log.Error("[! - Hot Compile Error - !]\n\n" + results.Errors[i].ErrorText);
                }
            }
            compiledAssembly = results.CompiledAssembly;
            behaviorInstance = compiledAssembly.CreateInstance(behaviorName);
            type = behaviorInstance.GetType();
            if (type.IsClass) Log.Info("Loaded");
        }
    }
}
