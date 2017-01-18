namespace Fixie.Runner
{
    using System;
    using Contracts;
    using Execution;

    public class DesignTimeDiscoveryListener : Handler<MethodDiscovered>
    {
        readonly IDesignTimeSink discoveryRecorder;
        readonly SourceLocationProvider sourceLocationProvider;

        public DesignTimeDiscoveryListener(IDesignTimeSink discoveryRecorder, string assemblyPath)
        {
            this.discoveryRecorder = discoveryRecorder;
            sourceLocationProvider = new SourceLocationProvider(assemblyPath);
        }

        public void Handle(MethodDiscovered message)
        {
            var methodGroup = new MethodGroup(message.Class, message.Method);

            var test = new Test
            {
                FullyQualifiedName = methodGroup.FullName,
                DisplayName = methodGroup.FullName
            };

            try
            {
                SourceLocation sourceLocation;
                if (sourceLocationProvider.TryGetSourceLocation(methodGroup, out sourceLocation))
                {
                    test.CodeFilePath = sourceLocation.CodeFilePath;
                    test.LineNumber = sourceLocation.LineNumber;
                }
            }
            catch (Exception exception)
            {
                discoveryRecorder.Log(exception.ToString());
            }

            discoveryRecorder.SendTestFound(test);
        }
    }
}