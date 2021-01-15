namespace FantasySoccer.Schema.Models.PlayFab
{
    public class PlayFabItem
    {
        public string ItemId { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string ItemImageUrl { get; set; }
        public string Currency { get; set; }
        public uint PriceStore { get; set; }
        public string CustomData { get; set; }
    }
}
