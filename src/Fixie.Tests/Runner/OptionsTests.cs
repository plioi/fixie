namespace Fixie.Tests.Runner
{
    using System;
    using Fixie.Cli;
    using Fixie.Runner;

    public class OptionsTests
    {
        public void DemandsAssemblyPath()
        {
            var options = new Options(null);

            Action validate = options.Validate;

            validate.ShouldThrow<CommandLineException>(
                "Missing required test assembly path.");
        }

        public void ParsesExistingAssemblyPath()
        {
            var assemblyPath = typeof(OptionsTests).Assembly().Location;

            var options = new Options(assemblyPath);

            options.Validate();
        }

        public void DemandsAssemblyPathExists()
        {
            var options = new Options("foo.dll");

            Action validate = options.Validate;
            
            validate.ShouldThrow<CommandLineException>(
                "Specified test assembly does not exist: foo.dll");
        }
    }
}