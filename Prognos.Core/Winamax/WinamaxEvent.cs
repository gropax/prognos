using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prognos.Core.Winamax
{
    public class WinamaxEvent
    {
        public Platform Platform => Platform.Winamax;
        public long Id { get; }
        public Sport Sport { get; }
        public string FirstTeam { get; }
        public string SecondTeam { get; }
        public DateTime StartDate { get; }

        public WinamaxEvent(
            long id,
            Sport sport,
            string firstTeam,
            string secondTeam,
            DateTime startDate)
        {
            Id = id;
            Sport = sport;
            FirstTeam = firstTeam;
            SecondTeam = secondTeam;
            StartDate = startDate;
        }
    }
}
