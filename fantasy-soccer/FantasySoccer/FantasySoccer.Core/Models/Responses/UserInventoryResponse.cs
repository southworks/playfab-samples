using System.Collections.Generic;
using FantasySoccer.Schema.Models.PlayFab;

namespace FantasySoccer.Core.Models.Responses
{
    public class UserInventoryResponse
    {
        public List<PlayFabItemInventory> Inventory { get; set; }
        public int Budget { get; set; }
    }
}
