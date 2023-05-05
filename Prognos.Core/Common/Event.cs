using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prognos.Core
{
    public abstract class Event
    {
        public long Id { get; }
        public Sport Sport { get; }

        protected Event(
            long id,
            Sport sport)
        {
            Id = id;
            Sport = sport;
        }
    }


    public class FootballGame : Event
    {
        public string HomeTeam { get; }
        public string AwayTeam { get; }
        public DateTime StartDate { get; }

        public FootballGame(
            long id,
            string homeTeam,
            string awayTeam,
            DateTime startDate)
            : base(id, Sport.Football)
        {
            HomeTeam = homeTeam;
            AwayTeam = awayTeam;
            StartDate = startDate;
        }
    }


    public class TennisGame : Event
    {
        public string FirstPlayer { get; }
        public string SecondPlayer { get; }
        public DateTime StartDate { get; }

        public TennisGame(
            long id,
            string firstPlayer,
            string secondPlayer,
            DateTime startDate)
            : base(id, Sport.Football)
        {
            FirstPlayer = firstPlayer;
            SecondPlayer = secondPlayer;
            StartDate = startDate;
        }
    }

}
