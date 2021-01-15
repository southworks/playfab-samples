namespace FantasySoccerDataSeeder.Models
{
    public interface IDataManagementConfig
    {
        string EndpointUri { get; set; }
        string PrimaryKey { get; set; }
    }
}
