using System;
using Newtonsoft.Json;

namespace FantasySoccer.Schema.Models.CosmosDB
{
    public class UserTransaction : Models.GeneralModel
    {
        public string UserPlayFabID { get; set; }
        public string InvolvedFutbolPlayerID { get; set; }
        public DateTime OperationDate { get; set; }
        public OperationTypes OperationType { get; set; }
        [JsonIgnore]
        public string InvolvedFutbolPlayerFullName { get; set; }
    }
}
