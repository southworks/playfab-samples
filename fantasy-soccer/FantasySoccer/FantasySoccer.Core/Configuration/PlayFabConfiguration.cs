namespace FantasySoccer.Core.Configuration
{
    public class PlayFabConfiguration
    {
        public const string PlayFab = "PlayFab";
        public string TitleId { get; set; }
        public string ConnectionId { get; set; }
        public string DeveloperSecretKey { get; set; }
        public string CatalogName { get; set; }
        public string StoreName { get; set; }
        public string Currency { get; set; }
        public string AllUserSegmentId { get; set; }
    }
}
