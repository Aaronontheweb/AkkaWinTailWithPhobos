using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.Extensions.Hosting;

namespace WinTail
{
    internal class ConsoleService : BackgroundService
    {
        private readonly IActorRef _consoleReader;
        private readonly IActorRef _consoleWriter;


        public ConsoleService(ActorSystem actorSystem)
        {
            _consoleWriter = actorSystem.ActorOf(Props.Create(() => new ConsoleWriterActor()), "consoleWriter");
            _consoleReader = actorSystem.ActorOf(Props.Create(() => new ConsoleReaderActor(_consoleWriter)), "consoleReader");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _consoleReader.Tell("start");
            await UntilCancelled(stoppingToken);
        }

        static async Task UntilCancelled(CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<bool>();
            using var ctRegistration = ct.Register(() => tcs.SetResult(true));
            await tcs.Task;
        }
    }
}
