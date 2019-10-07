using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prognos.Engine
{
    public enum Platform
    {
        Winamax,
    }

    public enum PlatformEventState
    {
        Open = 0,
        Live,
        Ended,
    }

    public class PlatformEvent
    {
        public Guid Guid { get; }
        public Platform Platform { get; }
        public string PlatformId { get; }

        public PlatformEvent(Platform platform, string platformId)
        {
            Platform = platform;
            PlatformId = platformId ?? throw new ArgumentNullException(nameof(platformId));
            Guid = HashCode.Combine(platform, platformId).ToGuid();
        }

        #region Equality methods
        public override bool Equals(object obj)
        {
            return obj is PlatformEvent @event &&
                   Platform == @event.Platform &&
                   PlatformId == @event.PlatformId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Platform, PlatformId);
        }
        #endregion
    }

    public class LiveOddsUpdate
    {
        public PlatformEvent Event { get; }
        public DateTime CreatedAt { get; }
        public PlatformEventState State { get; }
        public TimeSpan EventTime { get; }
        public IReadOnlyDictionary<string, float> Update { get; }

        public LiveOddsUpdate(PlatformEvent @event, DateTime createdAt, TimeSpan eventTime, IReadOnlyDictionary<string, float> update)
        {
            Event = @event;
            CreatedAt = createdAt;
            EventTime = eventTime;
            Update = update;
        }
    }

    public class OutcomesSnapshot
    {
        public PlatformEvent Event { get; }
        public DateTime CreatedAt { get; }
        public PlatformEventState State { get; }
        //public TimeSpan EventTime { get; }
        public Outcome[] Outcomes { get; }

        public OutcomesSnapshot(PlatformEvent @event, DateTime createdAt, PlatformEventState state, Outcome[] outcomes)
        {
            Event = @event ?? throw new ArgumentNullException(nameof(@event));
            CreatedAt = createdAt;
            State = state;
            Outcomes = outcomes ?? throw new ArgumentNullException(nameof(outcomes));
        }
    }

    public class RequestPage
    {
        public ScrapingCommand ScrapingCommand { get; }
        public RequestPage(ScrapingCommand scrapingCommand)
        {
            ScrapingCommand = scrapingCommand;
        }
    }

    public class PageGranted
    {
        public ScrapingCommand ScrapingCommand { get; }
        public Page Page { get; }
        public PageGranted(ScrapingCommand scrapingCommand, Page page)
        {
            ScrapingCommand = scrapingCommand;
            Page = page;
        }
    }

    public class PageDenied
    {
        public ScrapingCommand ScrapingCommand { get; }
        public PageDenied(ScrapingCommand scrapingCommand)
        {
            ScrapingCommand = scrapingCommand;
        }
    }

    public class RecyclePage
    {
        public Page Page { get; }
        public RecyclePage(Page page)
        {
            Page = page;
        }
    }
}
