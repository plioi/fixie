namespace Fixie.Runner
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Cli;

    public class Program
    {
        public const int FatalError = -1;

        [STAThread]
        static int Main(string[] arguments)
        {
            try
            {
                string[] runnerArguments;
                string[] conventionArguments;
                SplitArguments(arguments, out runnerArguments, out conventionArguments);

                var options = ParseRunnerArguments(runnerArguments);

                using (var environment = new ExecutionEnvironment(options.AssemblyPath))
                    return environment.Run(runnerArguments, conventionArguments);
            }
            catch (CommandLineException exception)
            {
                using (Foreground.Red)
                    Console.WriteLine(exception.Message);

                Console.WriteLine();
                Console.WriteLine(Usage());
                return FatalError;
            }
            catch (Exception exception)
            {
                using (Foreground.Red)
                    Console.WriteLine($"Fatal Error: {exception}");
                return FatalError;
            }
        }

        static void SplitArguments(string[] arguments, out string[] runnerArguments, out string[] conventionArguments)
        {
            var runnerArgumentList = new List<string>();
            var conventionArgumentList = new List<string>();

            bool separatorFound = false;
            foreach (var arg in arguments)
            {
                if (arg == "--")
                {
                    separatorFound = true;
                    continue;
                }

                if (separatorFound)
                    conventionArgumentList.Add(arg);
                else
                    runnerArgumentList.Add(arg);
            }

            runnerArguments = runnerArgumentList.ToArray();
            conventionArguments = conventionArgumentList.ToArray();
        }

        static Options ParseRunnerArguments(string[] runnerArguments)
        {
            string[] unusedArguments;
            var options = CommandLine.Parse<Options>(runnerArguments, out unusedArguments);

            using (Foreground.Yellow)
                foreach (var unusedArgument in unusedArguments)
                    Console.WriteLine($"The argument '{unusedArgument}' was unexpected and will be ignored.");

            options.Validate();
            return options;
        }

        static string Usage()
        {
            //TODO: As of .NET Core 1.0.0 VS 2015 Tooling Preview 2, it appears that you cannot have a -- separator yet
            //      between [dotnet arguments] and Fixie's own arguments. The implementation of https://github.com/dotnet/cli/blob/rel/1.0.0/src/dotnet/CommandLine/CommandLineApplication.cs
            //      on master, though, suggests that the omission has already been fixed. Once that fix is deployed, this usage
            //      will need to change to reflect the corrected behavior.

            return new StringBuilder()
                .AppendLine("Usage: dotnet test [dotnet arguments]")
                .AppendLine("       dotnet test [dotnet arguments] [--report-format <NUnit|xUnit>] [--team-city <on|off>] [--] [convention arguments]...")
                .AppendLine()
                .AppendLine()
                .AppendLine("    dotnet arguments")
                .AppendLine("        Arguments to be used by 'dotnet test'. See 'dotnet test --help'.")
                .AppendLine()
                .AppendLine("    --report-format <NUnit|xUnit>")
                .AppendLine("        Write test results to a file, using NUnit or xUnit XML format.")
                .AppendLine()
                .AppendLine("    --team-city <on|off>")
                .AppendLine("        When this option is *not* specified, the need for TeamCity-")
                .AppendLine("        formatted console output is automatically detected. Use this")
                .AppendLine("        option to force TeamCity-formatted output on or off.")
                .AppendLine()
                .AppendLine("    --")
                .AppendLine("        When present, all of the following arguments will be passed along")
                .AppendLine("        for use from within a convention.")
                .AppendLine()
                .AppendLine("    convention arguments")
                .AppendLine("        Arbitrary arguments made available to conventions at runtime.")
                .ToString();
        }
    }
}