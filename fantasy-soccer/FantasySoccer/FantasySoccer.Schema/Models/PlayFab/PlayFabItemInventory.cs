using System;

namespace FantasySoccer.Schema.Models.PlayFab
{
    public class PlayFabItemInventory
    {
        public string ItemId { get; set; }
        public string ItemInstanceId { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string ItemImageUrl { get; set; }
        public string Currency { get; set; }
        public uint PriceStore { get; set; }
        public string CustomDataStore { get; set; }
        public CustomDataInventory CustomDataInventory { get; set; }
        public DateTime? PurchaseDate { get; set; }
    }
}
