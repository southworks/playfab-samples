using Newtonsoft.Json;

namespace FantasySoccer.Schema.Models
{
    public class GeneralModel
    {
        [JsonProperty("id")]
        public string ID { get; set; }
    }
}
