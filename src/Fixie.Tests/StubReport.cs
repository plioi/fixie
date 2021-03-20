﻿namespace Fixie.Tests
{
    using System.Collections.Generic;
    using Fixie.Reports;

    public class StubReport :
        Handler<TestDiscovered>,
        Handler<CaseSkipped>,
        Handler<CasePassed>,
        Handler<CaseFailed>
    {
        readonly List<string> log = new List<string>();

        public void Handle(TestDiscovered message)
        {
            log.Add($"{message.Test.FullName} discovered");
        }

        public void Handle(CaseSkipped message)
        {
            var optionalReason = message.Reason == null ? null : ": " + message.Reason;
            log.Add($"{message.Name} skipped{optionalReason}");
        }

        public void Handle(CasePassed message)
        {
            log.Add($"{message.Name} passed");
        }

        public void Handle(CaseFailed message)
        {
            log.Add($"{message.Name} failed: {message.Exception.Message}");
        }

        public IEnumerable<string> Entries => log;
    }
}