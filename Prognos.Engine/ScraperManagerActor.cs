using Akka.Actor;
using Prognos.Core.Winamax;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Prognos.Engine
{
    public class ScraperManagerActor : ReceiveActor
    {
        private IBrowser _browser;
        private readonly Dictionary<long, ScrapingInfo> _childrenByGuid;
        private readonly Dictionary<IActorRef, ScrapingInfo> _childrenByActor;
        private readonly Queue<ScrapingCommand> _eventQueue;

        public ScraperManagerActor()
        {
            _childrenByGuid = new Dictionary<long, ScrapingInfo>();
            _childrenByActor = new Dictionary<IActorRef, ScrapingInfo>();
            _eventQueue = new Queue<ScrapingCommand>();

            Receive<InitializeBrowser>(async e =>
            {
                var context = Context;

                _browser = await InitializeBrowserAsync();
                context.Self.Tell(new TryStartScraping());
            });

            Receive<ScrapingCommand>(c => {
                _eventQueue.Enqueue(c);
                Sender.Tell(new CommandQueued(c));
                Self.Tell(new TryStartScraping());
            });

            Receive<TryStartScraping>(async e =>
            {
                var context = Context;

                if (_browser == null)
                    context.Self.Tell(new InitializeBrowser());
                else if (_eventQueue.Count > 0)
                {
                    var cmd = _eventQueue.Dequeue();
                    var page = await _browser.NewPageAsync();
                    var scraper = CreateScraper(page, cmd);

                    var child = context.ActorOf(ScraperActor.Props(scraper, cmd.Options));
                    context.Watch(child);

                    var scrapingInfo = new ScrapingInfo(cmd, page, child);
                    _childrenByGuid[cmd.PlatformEvent.Id] = scrapingInfo;
                    _childrenByActor[child] = scrapingInfo;

                    context.Self.Tell(new TryStartScraping());
                }
            });

            Receive<Terminated>(e =>
            {
                if (_childrenByActor.TryGetValue(e.ActorRef, out var info))
                {
                    _childrenByGuid.Remove(info.ScrapingCommand.PlatformEvent.Id);
                    _childrenByActor.Remove(info.Actor);
                    info.Page.Dispose();
                }
            });

            Receive<LiveOddsUpdate>(e =>
            {
                Console.WriteLine("Update");
            });

            Receive<OutcomesSnapshot>(e =>
            {
                Console.WriteLine("Snapshot");
            });
        }

        private IPlatformEventScraper CreateScraper(IPage page, ScrapingCommand cmd)
        {
            switch (cmd.PlatformEvent.Platform)
            {
                case Core.Platform.Winamax:
                    return new WinamaxEventScraper(page, cmd.PlatformEvent);
                default:
                    throw new ArgumentException($"Unsupported platform: {cmd.PlatformEvent.Platform}");
            }
        }

        private async Task<IBrowser> InitializeBrowserAsync()
        {
            await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            return await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
        }


        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new ScraperManagerActor());
        }
    }

    class InitializeBrowser { }

    class TryStartScraping { }

    public class ScrapingCommand
    {
        public WinamaxEvent PlatformEvent { get; }
        public ScrapingOptions Options { get; }

        public ScrapingCommand(WinamaxEvent platformEvent, ScrapingOptions scrapingOptions)
        {
            PlatformEvent = platformEvent;
            Options = scrapingOptions;
        }
    }

    public class CommandQueued
    {
        public ScrapingCommand Command { get;  }
        public CommandQueued(ScrapingCommand command)
        {
            Command = command;
        }
    }

    public struct ScrapingInfo
    {
        public ScrapingCommand ScrapingCommand { get; }
        public IPage Page { get; }
        public IActorRef Actor { get; }

        public ScrapingInfo(ScrapingCommand scrapingCommand, IPage page, IActorRef actor)
        {
            ScrapingCommand = scrapingCommand;
            Page = page;
            Actor = actor;
        }
    }

}
