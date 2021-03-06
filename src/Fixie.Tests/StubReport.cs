﻿namespace Fixie.Tests
{
    using System.Collections.Generic;
    using Fixie.Reports;

    public class StubReport :
        Handler<TestDiscovered>,
        Handler<TestSkipped>,
        Handler<TestPassed>,
        Handler<TestFailed>
    {
        readonly List<string> log = new List<string>();

        public void Handle(TestDiscovered message)
        {
            log.Add($"{message.Test} discovered");
        }

        public void Handle(TestSkipped message)
        {
            log.Add($"{message.Name} skipped: {message.Reason}");
        }

        public void Handle(TestPassed message)
        {
            log.Add($"{message.Name} passed");
        }

        public void Handle(TestFailed message)
        {
            log.Add($"{message.Name} failed: {message.Reason.Message}");
        }

        public IEnumerable<string> Entries => log;
    }
}