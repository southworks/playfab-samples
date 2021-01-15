using System.Collections.Generic;

namespace FantasySoccer.Models.ViewModels
{
    public class MarketplaceViewModel
    {
        public List<MarketPlaceItems> FutbolPlayers { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
