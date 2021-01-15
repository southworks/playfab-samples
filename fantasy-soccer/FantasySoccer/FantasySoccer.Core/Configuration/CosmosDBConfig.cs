namespace FantasySoccer.Models.Configuration
{
    public class CosmosDBConfig
    {
        public const string CosmosDB = "CosmosDB";
        public string EndpointUri { get; set; }
        public string PrimaryKey { get; set; }
        public string DatabaseName { get; set; }
    }
}
