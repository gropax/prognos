using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prognos.Core.Winamax
{
    public class OutcomesSnapshot
    {
        public WinamaxEvent Event { get; }
        public DateTime CreatedAt { get; }
        public EventState State { get; }
        //public TimeSpan EventTime { get; }
        public Outcome[] Outcomes { get; }

        public OutcomesSnapshot(WinamaxEvent @event, DateTime createdAt, EventState state, Outcome[] outcomes)
        {
            Event = @event ?? throw new ArgumentNullException(nameof(@event));
            CreatedAt = createdAt;
            State = state;
            Outcomes = outcomes ?? throw new ArgumentNullException(nameof(outcomes));
        }
    }
}
