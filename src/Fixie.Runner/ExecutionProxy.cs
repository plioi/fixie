namespace Fixie.Runner
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Cli;

    public class ExecutionProxy
#if NET45
        : LongLivedMarshalByRefObject
#endif
    {
        public int Run(string assemblyFullPath, IReadOnlyList<string> runnerArguments, IReadOnlyList<string> conventionArguments)
        {
            var options = CommandLine.Parse<Options>(runnerArguments);

            var assembly = Assembly.Load(GetAssemblyName(assemblyFullPath));

            return Runner(options).Run(assemblyFullPath, assembly, options, conventionArguments);
        }

        static AssemblyName GetAssemblyName(string assemblyFullPath)
        {
#if NET45
            return AssemblyName.GetAssemblyName(assemblyFullPath);
#else
            return new AssemblyName { Name = Path.GetFileNameWithoutExtension(assemblyFullPath) };
#endif
        }

        static RunnerBase Runner(Options options)
        {
            if (options.DesignTime)
                return new DesignTimeRunner();

            return new ConsoleRunner();
        }
    }
}