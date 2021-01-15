namespace PlayFabLoginWithB2C.Services
{
    public class PlayFabOptions
    {
        public const string PlayFab = "PlayFab";
        public string TitleId { get; set; }
        public string CatalogVersion { get; set; }
        public string StoreId { get; set; }
        public string Currency { get; set; }
        public CacheOptions Cache { get; set; }
        public string ConnectionId { get; set; }
    }
}