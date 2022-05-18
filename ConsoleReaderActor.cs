using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;
using OpenTelemetry.Trace;
using Phobos.Actor;

namespace WinTail
{
    /// <summary>
    /// Actor responsible for reading FROM the console. 
    /// Also responsible for calling <see cref="ActorSystem.Terminate"/>.
    /// </summary>
    class ConsoleReaderActor : UntypedActor
    {
        private static readonly ActivitySource ActivitySource = new ActivitySource(nameof(ConsoleReaderActor));
        public const string ExitCommand = "exit";
        private readonly IActorRef _consoleWriterActor;

        public ConsoleReaderActor(IActorRef consoleWriterActor)
        {
            _consoleWriterActor = consoleWriterActor;
        }

        protected override void OnReceive(object message)
        {
            var read = Console.ReadLine();
            if (!string.IsNullOrEmpty(read) && String.Equals(read, ExitCommand, StringComparison.OrdinalIgnoreCase))
            {
                // shut down the system (acquire handle to system via
                // this actors context)
                Context.System.Terminate();
                return;
            }

            using var mySpan = Context.GetInstrumentation().Tracer.StartActiveSpan("ReadText", SpanKind.Consumer, Context.GetInstrumentation().UsableContext ?? default);
            mySpan.SetAttribute("word", read);

            // send input to the console writer to process and print
            _consoleWriterActor.Tell(read);
            // continue reading messages from the console
            Self.Tell("continue");
        }
    }
}