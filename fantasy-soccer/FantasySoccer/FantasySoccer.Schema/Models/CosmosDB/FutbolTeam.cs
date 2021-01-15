using System.Collections.Generic;
using FantasySoccer.Schema.Models.PlayFab;
using Newtonsoft.Json;

namespace FantasySoccer.Schema.Models.CosmosDB
{

    public class FutbolTeam: Models.GeneralModel
    {
        public string Name { get; set; }

        public int[] FutbolPlayersId { get; set; }

        [JsonIgnore]
        public List<FutbolPlayer> Players { get; set; }
    }
}
