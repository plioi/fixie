﻿namespace Fixie.Tests.Reports
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Reports;
    using static Utility;

    public abstract class MessagingTests
    {
        protected MessagingTests()
        {
            TestClass = FullName<SampleTestClass>();
            GenericTestClass = FullName<SampleGenericTestClass>();
        }

        protected string TestClass { get; }
        protected string GenericTestClass { get; }
        protected static Type TestClassType => typeof(SampleTestClass);

        readonly Type[] candidateTypes =
        {
            typeof(SampleTestClass),
            typeof(SampleGenericTestClass),
            typeof(EmptyTestClass)
        };

        protected class Output
        {
            public Output(string[] console)
                => Console = console;

            public string[] Console { get; }
        }

        protected async Task DiscoverAsync(Report report)
        {
            var discovery = new SelfTestDiscovery();

            using var console = new RedirectedConsole();

            await Utility.DiscoverAsync(report, discovery, candidateTypes);

            console.Lines().ShouldBeEmpty();
        }

        protected Task<Output> RunAsync(Report report)
        {
            return RunAsync(_ => report);
        }

        protected Task<Output> RunAsync(Report report, Discovery discovery)
        {
            return RunAsync(_ => report, discovery);
        }

        protected Task<Output> RunAsync(Func<TextWriter, Report> getReport)
        {
            return RunAsync(getReport, new SelfTestDiscovery());
        }

        protected async Task<Output> RunAsync(Func<TextWriter, Report> getReport, Discovery discovery)
        {
            var execution = new MessagingTestsExecution();

            using var console = new RedirectedConsole();

            await Utility.RunAsync(getReport(Console.Out), discovery, execution, candidateTypes);

            return new Output(console.Lines().ToArray());
        }

        class MessagingTestsExecution : Execution
        {
            public async Task RunAsync(TestAssembly testAssembly)
            {
                foreach (var test in testAssembly.Tests)
                {
                    if (test.Has<SkipAttribute>(out var skip))
                    {
                        await test.SkipAsync(skip.Reason);
                        continue;
                    }

                    foreach (var parameters in FromInputAttributes(test))
                        await test.RunAsync(parameters);
                }
            }
        }

        protected class Base
        {
            public void Pass()
            {
                WhereAmI();
            }

            protected static void WhereAmI([CallerMemberName] string member = default!)
                => Console.WriteLine("Standard Out: " + member);
        }

        class SampleTestClass : Base
        {
            public void Fail()
            {
                WhereAmI();
                throw new FailureException();
            }

            public void FailByAssertion()
            {
                WhereAmI();
                1.ShouldBe(2);
            }

            const string alert = "\x26A0";
            [Skip(alert + " Skipped with attribute.")]
            public void Skip()
            {
                throw new ShouldBeUnreachableException();
            }
        }

        protected class SampleGenericTestClass
        {
            [Input("A")]
            [Input("B")]
            [Input(123)]
            public void ShouldBeString<T>(T genericArgument)
            {
                genericArgument.ShouldBe<string>();
            }
        }

        class EmptyTestClass
        {
        }

        protected static string At(string method)
            => At<SampleTestClass>(method);

        protected static string At<T>(string method)
            => Utility.At<T>(method);

        protected static string TestClassPath()
            => PathToThisFile();
    }
}