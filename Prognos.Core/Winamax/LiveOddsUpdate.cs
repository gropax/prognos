using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prognos.Core.Winamax
{
    public class LiveOddsUpdate
    {
        public WinamaxEvent Event { get; }
        public DateTime CreatedAt { get; }
        public EventState State { get; }
        public TimeSpan EventTime { get; }
        public IReadOnlyDictionary<string, float> Update { get; }

        public LiveOddsUpdate(WinamaxEvent @event, DateTime createdAt, TimeSpan eventTime, IReadOnlyDictionary<string, float> update)
        {
            Event = @event;
            CreatedAt = createdAt;
            EventTime = eventTime;
            Update = update;
        }
    }
}
