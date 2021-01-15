using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FantasySoccer.Core.Models.Responses;
using FantasySoccer.Schema.Models;
using FantasySoccer.Schema.Models.PlayFab;
using PlayFab.AdminModels;

namespace FantasySoccer.Core.Services
{
    public interface IPlayFabService
    {
        Task<List<PlayFab.ClientModels.StatisticValue>> GetUserStatisticAsync(List<string> listStatistics);
        
        Task SetCatalogItems(List<FutbolPlayer> players);
        
        Task SetStoreItems(List<FutbolPlayer> players);
        
        Task<List<PlayFab.ServerModels.GrantedItemInstance>> GrantItemstoUser(string playFabId, List<FutbolPlayer> players);
        
        Task<List<ItemInstance>> GetUserInventoryUsingAdminAPI(string playFabId);
        
        Task RevokeInventoryItems(string playFabId, List<ItemInstance> inventory);
        
        Task RevokeInventoryItem(string playFabId, string itemInstance);
        
        Task UpdateUserInventoryItemCustomData(string playFabId, string itemInstanceId, Dictionary<string, string> data);
        
        Task UpdatePlayerStatistics(string playFabId, Dictionary<string, int> statistics);
        
        Task<string> AddUserVirtualCurrency(string playFabId, int Amount);

        Task<Dictionary<string, string>> GetTitleData(List<string> keys = null);

        Task SetTitleData(string key, string value);

        Task<List<PlayFabItem>> GetStoreItems();

        Task<PaginatedItem<T>> GetStoreItems<T>(int? size = null, int? skip = null, Func<T, bool> filter = null);

        Task<UserInventoryResponse> GetUserInventory();
        
        Task<PlayFabItemInventory> GetItemInventory(string filterValue, OriginItemEnum originItemId);
        
        Task<List<string>> GetUsersIdAsync();
        
        Task<PurchaseItemResponse> PurchaseItem(string itemId, int price);
        
        string GetPlayFabId();

        Task<UserInventoryResponse> GetUserInventory(string playFabId);
    }
}

