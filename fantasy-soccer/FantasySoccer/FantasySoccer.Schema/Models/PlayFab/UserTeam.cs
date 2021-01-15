using System.Collections.Generic;
using System.Linq;

namespace FantasySoccer.Schema.Models.PlayFab
{
    public class UserTeam: GeneralModel
    {
        public string UserPlayFabID { get; set; }

        public IDictionary<int, int> MatchdayScores { get; set; }

        public List<FutbolPlayer> Players { get; set; }

        public int TournamentScore => MatchdayScores != null && MatchdayScores.Count > 0 ? MatchdayScores.Sum(matchdayScore => matchdayScore.Value) : 0;
    }
}
