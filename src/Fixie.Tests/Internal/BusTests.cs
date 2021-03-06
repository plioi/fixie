﻿namespace Fixie.Tests.Internal
{
    using System;
    using System.Threading.Tasks;
    using Assertions;
    using Fixie.Internal;
    using Fixie.Reports;
    using static Utility;

    public class BusTests
    {
        public async Task ShouldPublishEventsToAllReports()
        {
            var reports = new Report[]
            {
                new EventHandler(),
                new AnotherEventHandler(),
                new CombinationEventHandler()
            };

            using var console = new RedirectedConsole();

            var bus = new Bus(Console.Out, reports);
            await bus.PublishAsync(new Event(1));
            await bus.PublishAsync(new AnotherEvent(2));
            await bus.PublishAsync(new Event(3));

            console.Lines()
                .ShouldBe(
                    FullName<EventHandler>() + " handled Event 1",
                    FullName<CombinationEventHandler>() + " handled Event 1",
                    FullName<AnotherEventHandler>() + " handled AnotherEvent 2",
                    FullName<CombinationEventHandler>() + " handled AnotherEvent 2",
                    FullName<EventHandler>() + " handled Event 3",
                    FullName<CombinationEventHandler>() + " handled Event 3");
        }

        public async Task ShouldCatchAndLogExceptionsThrowByProblematicReportsRatherThanInterruptExecution()
        {
            var reports = new Report[]
            {
                new EventHandler(),
                new FailingEventHandler()
            };

            using var console = new RedirectedConsole();

            var bus = new Bus(Console.Out, reports);
            await bus.PublishAsync(new Event(1));
            await bus.PublishAsync(new AnotherEvent(2));
            await bus.PublishAsync(new Event(3));

            console.Lines()
                .ShouldBe(
                    FullName<EventHandler>() + " handled Event 1",
                    FullName<FailingEventHandler>() + $" threw an exception while attempting to handle a message of type {FullName<Event>()}:",
                    "",
                    FullName<StubException>() + ": Could not handle Event 1",
                    "<<Stack Trace>>",
                    "",
                    FullName<EventHandler>() + " handled Event 3",
                    FullName<FailingEventHandler>() + $" threw an exception while attempting to handle a message of type {FullName<Event>()}:",
                    "",
                    FullName<StubException>() + ": Could not handle Event 3",
                    "<<Stack Trace>>");
        }

        class Event : Message
        {
            public Event(int id) { Id = id; }
            public int Id { get; }
        }

        class AnotherEvent : Message
        {
            public AnotherEvent(int id) { Id = id; }
            public int Id { get; }
        }

        class EventHandler : Handler<Event>
        {
            public void Handle(Event message)
                => Log<EventHandler, Event>(message.Id);
        }

        class AnotherEventHandler : Handler<AnotherEvent>
        {
            public void Handle(AnotherEvent message)
                => Log<AnotherEventHandler, AnotherEvent>(message.Id);
        }

        class CombinationEventHandler : Handler<Event>, AsyncHandler<AnotherEvent>
        {
            public void Handle(Event message)
                => Log<CombinationEventHandler, Event>(message.Id);

            public Task HandleAsync(AnotherEvent message)
            {
                Log<CombinationEventHandler, AnotherEvent>(message.Id);
                return Task.CompletedTask;
            }
        }

        class FailingEventHandler : Handler<Event>
        {
            public void Handle(Event message)
                => throw new StubException($"Could not handle {nameof(Event)} {message.Id}");
        }

        static void Log<THandler, TEvent>(int id)
            => Console.WriteLine($"{typeof(THandler).FullName} handled {typeof(TEvent).Name} {id}");

        class StubException : Exception
        {
            public StubException(string message)
                : base(message) { }

            public override string StackTrace
                => "<<Stack Trace>>";
        }
    }
}