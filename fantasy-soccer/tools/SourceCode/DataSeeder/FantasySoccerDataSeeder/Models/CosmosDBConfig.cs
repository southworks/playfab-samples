namespace FantasySoccerDataSeeder.Models
{
    public class CosmosDBConfig: IDataManagementConfig
    {
        public string EndpointUri { get; set; }
        public string PrimaryKey { get; set; }
    }
}
