namespace Fixie.Runner
{
    using System.IO;
    using Cli;

    public class Options
    {
        public Options(string assemblyPath)
        {
            AssemblyPath = assemblyPath;
        }

        public string AssemblyPath { get; }
        public ReportFormat? ReportFormat { get; set; }
        public bool? TeamCity { get; set; }

        // IDE Arguments
        public bool DesignTime { get; set; }
        public int? Port { get; set; }
        public bool List { get; set; }
        public bool WaitCommand { get; set; }

        public void Validate()
        {
            if (AssemblyPath == null)
                throw new CommandLineException("Missing required test assembly path.");

            if (!File.Exists(AssemblyPath))
                throw new CommandLineException("Specified test assembly does not exist: " + AssemblyPath);
        }
    }
}