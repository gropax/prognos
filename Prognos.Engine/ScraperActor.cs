using Akka.Actor;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prognos.Engine
{
    public class ScraperActor : ReceiveActor
    {
        private readonly IPlatformEventScraper _scraper;
        private readonly ScrapingOptions _options;

        private PlatformEventState _eventState;
        private readonly ICancelable _cancelScraping;

        public ScraperActor(IPlatformEventScraper scraper, ScrapingOptions options)
        {
            _scraper = scraper;
            _options = options;
            _cancelScraping = new Cancelable(Context.System.Scheduler);

            Receive<ScrapPage>(_ => _scraper.ScrapPage().PipeTo(Self));

            Receive<LiveOddsUpdate>(update =>
            {
                Context.Parent.Forward(update);
            });

            Receive<OutcomesSnapshot>(update =>
            {
                var context = Context;

                Context.Parent.Forward(update);

                if (_eventState == PlatformEventState.Open && update.State == PlatformEventState.Live)
                {
                    _cancelScraping.Cancel(false);
                    _scraper.SubscribeToLive(u => {
                        context.Self.Tell(u);
                    });
                    _eventState = update.State;
                }
                else if (update.State == PlatformEventState.Ended)
                {
                    Context.Parent.Tell(new RecyclePage(_scraper.Page));
                    Context.Stop(Self);
                }
            });
        }

        protected override void PreStart()
        {
            Context.System.Scheduler.ScheduleTellRepeatedly(
                initialDelay: default,
                interval: _options.ScrapInterval,
                receiver: Self,
                message: new ScrapPage(),
                sender: ActorRefs.NoSender,
                cancelable: _cancelScraping);
        }

        protected override void PostStop()
        {
            try
            {
                _cancelScraping.Cancel(false);
            }
            catch { }
            finally
            {
                base.PostStop();
            }
        }

        public static Props Props(IPlatformEventScraper scraper, ScrapingOptions options)
        {
            return Akka.Actor.Props.Create(() => new ScraperActor(scraper, options));
        }
    }

    public class SubscribeToLive { }

    public class ScrapPage { }

    public class ScrapingOptions
    {
        public TimeSpan ScrapInterval { get; }
        public ScrapingOptions(TimeSpan scrapInterval)
        {
            ScrapInterval = scrapInterval;
        }
    }
}
