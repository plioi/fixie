namespace Fixie.Execution.Behaviors
{
    using System;
    using System.Collections.Generic;

    class ExecuteLifecycle
    {
        readonly Lifecycle lifecycle;

        public ExecuteLifecycle(Lifecycle lifecycle)
        {
            this.lifecycle = lifecycle;
        }

        public void Execute(Type testClass, IReadOnlyList<Case> cases)
        {
            var timeClassExecution = new TimeClassExecution();

            var @class = new Class(testClass, cases);

            timeClassExecution.Execute(@class, () =>
            {
                try
                {
                    lifecycle.Execute(testClass, caseLifecycle =>
                    {
                            var fixture = new Fixture(@class, null, cases);
                            new ExecuteCases().Execute(fixture, caseLifecycle);
                    });
                }
                catch (Exception exception)
                {
                    foreach (var @case in cases)
                        @case.Fail(exception);
                }
            });
        }
    }
}