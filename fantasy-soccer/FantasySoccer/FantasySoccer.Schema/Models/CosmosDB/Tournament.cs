using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace FantasySoccer.Schema.Models.CosmosDB
{
    public class Tournament : GeneralModel
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [JsonIgnore]
        public List<FutbolTeam> FutbolTeams { get; set; } = new List<FutbolTeam>();

        public int CurrentRound { get; set; }

        public string[] FutbolTeamIds
        {
            get => FutbolTeams?.Select(futbolTeam => futbolTeam.ID)?.ToArray() ?? Array.Empty<string>();
            set => FutbolTeams = value.Select(id => new FutbolTeam { ID = id }).ToList();
        }
    }
}
