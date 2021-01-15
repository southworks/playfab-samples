using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FantasySoccer.Core.Configuration;
using FantasySoccer.Core.Models.Responses;
using FantasySoccer.Schema.Models;
using FantasySoccer.Schema.Models.PlayFab;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.AdminModels;

namespace FantasySoccer.Core.Services
{
    public class PlayFabService: IPlayFabService
    {
        private readonly IMemoryCache cache;
        private readonly PlayFabConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly MemoryCacheEntryOptions cacheEntryOptions;

        public PlayFabService(
            PlayFabConfiguration configuration,
            IMemoryCache memoryCache,
            IHttpContextAccessor httpContextAccessor = null)
        {
            this.configuration = configuration;

            cache = memoryCache;
            cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetPriority(CacheItemPriority.NeverRemove);
            cache.Set(CacheKeys.StoreVersion, "0", cacheEntryOptions);

            this.httpContextAccessor = httpContextAccessor;

            if (string.IsNullOrWhiteSpace(PlayFabSettings.staticSettings.TitleId))
            {
                PlayFabSettings.staticSettings.TitleId = configuration.TitleId;
            }

            if (string.IsNullOrWhiteSpace(PlayFabSettings.staticSettings.DeveloperSecretKey))
            { 
                PlayFabSettings.staticSettings.DeveloperSecretKey = configuration.DeveloperSecretKey;
            }
        }

        public async Task<List<PlayFab.ClientModels.StatisticValue>> GetUserStatisticAsync(List<string> listStatistics)
        {
            var request = new PlayFab.ClientModels.GetPlayerStatisticsRequest
            {
                AuthenticationContext = new PlayFabAuthenticationContext { ClientSessionTicket = await GetSessionTicketAsync() },
                StatisticNames = listStatistics
            };
            
            var result = await PlayFabClientAPI.GetPlayerStatisticsAsync(request);
            
            return result?.Result?.Statistics;
        }

        public async Task SetCatalogItems(List<FutbolPlayer> players)
        {
            var items = new List<CatalogItem> { };

            foreach (var player in players)
            {
                items.Add(new CatalogItem
                {
                    ItemId = player.ID,
                    DisplayName = $"{player.Name} {player.LastName}",
                    VirtualCurrencyPrices = new Dictionary<string, uint>() { { configuration.Currency, player.Price } },
                    CustomData = JsonConvert.SerializeObject(player)
                });
            }

            var reqUpdateCatalogItems = new UpdateCatalogItemsRequest
            {
                CatalogVersion = configuration.CatalogName,
                Catalog = items
            };

            var result = await PlayFabAdminAPI.SetCatalogItemsAsync(reqUpdateCatalogItems);

            if (result.Error != null)
            {
                Console.WriteLine(result.Error.ErrorMessage);
            }
        }

        public async Task SetStoreItems(List<FutbolPlayer> players)
        {
            var items = new List<StoreItem> { };

            foreach (var player in players)
            {
                items.Add(new StoreItem
                {
                    ItemId = player.ID,
                    VirtualCurrencyPrices = new Dictionary<string, uint>() { { configuration.Currency, player.Price } },
                    CustomData = JsonConvert.SerializeObject(player)
                });
            }

            var reqUpdateStoreItems = new UpdateStoreItemsRequest
            {
                CatalogVersion = configuration.CatalogName,
                Store = items,
                StoreId = configuration.StoreName
            };

            var result = await PlayFabAdminAPI.SetStoreItemsAsync(reqUpdateStoreItems);

            if (result.Error != null)
            {
                Console.WriteLine(result.Error.ErrorMessage);
            }
        }

        public async Task<List<PlayFab.ServerModels.GrantedItemInstance>> GrantItemstoUser(string playFabId, List<FutbolPlayer> players)
        {
            var items = players.Select(players => players.ID).ToList();

            var reqGrantItemsToUser = new PlayFab.ServerModels.GrantItemsToUserRequest
            {
                ItemIds = items,
                PlayFabId = playFabId
            };

            var result = await PlayFabServerAPI.GrantItemsToUserAsync(reqGrantItemsToUser);

            if (result.Error != null)
            {
                Console.WriteLine(result.Error.ErrorMessage);
            }

            return result?.Result?.ItemGrantResults ?? new List<PlayFab.ServerModels.GrantedItemInstance>();
        }

        public async Task<List<ItemInstance>> GetUserInventoryUsingAdminAPI(string playFabId)
        {
            PlayFabResult<GetUserInventoryResult> result; 
            var reqGetInventory = new GetUserInventoryRequest
            {
                PlayFabId = playFabId,
            };

            try
            {
                result = await PlayFabAdminAPI.GetUserInventoryAsync(reqGetInventory);

            }
            catch
            {
                // This is a retry, since we've seen a random issue that fails to return the user inventory
                // and succeeds on the following request
                result = await PlayFabAdminAPI.GetUserInventoryAsync(reqGetInventory);
            }

            return result?.Result?.Inventory ?? new List<ItemInstance>();
        }

        public async Task RevokeInventoryItems(string playFabId, List<ItemInstance> inventory)
        {
            var items = new List<RevokeInventoryItem> { };
            foreach (var item in inventory)
            {
                items.Add(new RevokeInventoryItem
                {
                    PlayFabId = playFabId,
                    ItemInstanceId = item.ItemInstanceId
                });
            }

            var reqGetInventory = new RevokeInventoryItemsRequest
            {
                Items = items
            };

            var result = await PlayFabAdminAPI.RevokeInventoryItemsAsync(reqGetInventory);

            if (result.Error != null)
            {
                Console.WriteLine(result.Error.ErrorMessage);
            }
        }

        public async Task RevokeInventoryItem(string playFabId, string itemInstance)
        {
            var reqRevokeInventoryItem = new RevokeInventoryItemRequest
            {
                PlayFabId = playFabId,
                ItemInstanceId = itemInstance
            };

            var result = await PlayFabAdminAPI.RevokeInventoryItemAsync(reqRevokeInventoryItem);

            if (result.Error != null)
            {
                throw new Exception(result.Error.ErrorMessage);
            }
        }

        public async Task UpdateUserInventoryItemCustomData(string playFabId, string itemInstanceId, Dictionary<string, string> data)
        {
            var reqUpdateUserInventoryItemData = new PlayFab.ServerModels.UpdateUserInventoryItemDataRequest
            {
                PlayFabId = playFabId,
                Data = data,
                ItemInstanceId = itemInstanceId
            };

            var result = await PlayFabServerAPI.UpdateUserInventoryItemCustomDataAsync(reqUpdateUserInventoryItemData);

            if (result.Error != null)
            {
                throw new Exception(result.Error.ErrorMessage);
            }
        }

        public async Task UpdatePlayerStatistics(string playFabId, Dictionary<string, int> statistics)
        {
            var statisticsUser = new List<PlayFab.ServerModels.StatisticUpdate>(statistics.Count);

            foreach (var item in statistics)
            {
                statisticsUser.Add(new PlayFab.ServerModels.StatisticUpdate
                {
                    StatisticName = item.Key,
                    Value = item.Value
                });
            }

            var reqUpdateUserInventoryItemData = new PlayFab.ServerModels.UpdatePlayerStatisticsRequest
            {
                PlayFabId = playFabId,
                Statistics = statisticsUser
            };

            var result = await PlayFabServerAPI.UpdatePlayerStatisticsAsync(reqUpdateUserInventoryItemData);

            if (result.Error != null)
            {
                Console.WriteLine(result.Error.ErrorMessage);
            }
        }

        public async Task<List<string>> GetUsersIdAsync()
        {
            var userIds = new List<string>();
            var reqGetPlayersInSegment = new GetPlayersInSegmentRequest
            {
                SegmentId = configuration.AllUserSegmentId
            };

            var result = await PlayFabAdminAPI.GetPlayersInSegmentAsync(reqGetPlayersInSegment);
            result.Result.PlayerProfiles.ForEach(playerProfile => userIds.Add(playerProfile.PlayerId));
            return userIds;
        }

        public async Task<string> AddUserVirtualCurrency(string playFabId, int Amount)
        {
            var reqAddUserVirtualCurrency = new AddUserVirtualCurrencyRequest
            {
                PlayFabId = playFabId,
                Amount = Amount,
                VirtualCurrency = configuration.Currency
            };

            var result = await PlayFabAdminAPI.AddUserVirtualCurrencyAsync(reqAddUserVirtualCurrency);

            if (result.Error != null)
            {
                throw new Exception(result.Error.ErrorMessage);
            }

            return await UpdateBudget();
        }

        public async Task<PaginatedItem<T>> GetStoreItems<T>(int? size, int? skip, Func<T, bool> filter)
        {
            var items = await GetStoreItems();

            var entities = items.Select(i => JsonConvert.DeserializeObject<T>(i.CustomData)).ToList();


            if(filter != null)
            {
                entities = entities.Where(filter).ToList();
            }

            var paginatedItem = new PaginatedItem<T> { TotalItems = entities.Count };

            if (skip.HasValue)
            {
                entities = entities.Skip(skip.Value).ToList();
            }

            if (size.HasValue)
            {
                entities = entities.Take(size.Value).ToList();
            }

            paginatedItem.PaginatedItems = entities;

            return paginatedItem;
        }

        public async Task<List<PlayFabItem>> GetStoreItems()
        {
            var isCacheUpdated = await IsCacheUpdated();

            if (!cache.TryGetValue(CacheKeys.Store, out List<PlayFabItem> items) || !isCacheUpdated)
            {
                var reqGetCatalog = new GetCatalogItemsRequest
                {
                    CatalogVersion = configuration.CatalogName,
                };

                var reqGetStore = new GetStoreItemsRequest
                {
                    CatalogVersion = configuration.CatalogName,
                    StoreId = configuration.StoreName
                };

                var resultCatalog = await PlayFabAdminAPI.GetCatalogItemsAsync(reqGetCatalog);

                if (resultCatalog.Error != null)
                {
                    throw new Exception(resultCatalog.Error.ErrorMessage);
                }

                var resultStore = await PlayFabAdminAPI.GetStoreItemsAsync(reqGetStore);

                if (resultStore.Error != null)
                {
                    throw new Exception(resultStore.Error.ErrorMessage);
                }

                var catalog = resultCatalog.Result.Catalog;
                var store = resultStore.Result.Store;

                items = MergeAndGetItemList(catalog, store);
                cache.Set(CacheKeys.Store, items, cacheEntryOptions);
                if (!isCacheUpdated) await UpdateStoreVersionCache();
            }

            return items;
        }

        public async Task<UserInventoryResponse> GetUserInventory()
        {
            var reqGetInventory = new PlayFab.ClientModels.GetUserInventoryRequest
            {
                AuthenticationContext = new PlayFabAuthenticationContext { ClientSessionTicket = await GetSessionTicketAsync() },
            };

            var result = await PlayFabClientAPI.GetUserInventoryAsync(reqGetInventory);
            if (result.Error != null)
            {
                throw new Exception(result.Error.ErrorMessage);
            }

            var budget = result?.Result?.VirtualCurrency?.ContainsKey(configuration.Currency) == true ? result.Result.VirtualCurrency[configuration.Currency] : 0;
            var inventory = await CompleteWithDataStore(result.Result.Inventory.ToList<dynamic>());

            return new UserInventoryResponse
            {
                Inventory = inventory,
                Budget = budget
            };
        }

        public async Task<UserInventoryResponse> GetUserInventory(string playfabId)
        {
            var reqGetInventory = new GetUserInventoryRequest
            {
                PlayFabId = playfabId
            };

            var result = await PlayFabAdminAPI.GetUserInventoryAsync(reqGetInventory);
            
            if (result.Error != null)
            {
                throw new Exception(result.Error.ErrorMessage);
            }

            var budget = result?.Result?.VirtualCurrency?.ContainsKey(configuration.Currency) == true ? result.Result.VirtualCurrency[configuration.Currency] : 0;
            var inventory = await CompleteWithDataStore(result.Result.Inventory.ToList<dynamic>());

            return new UserInventoryResponse
            {
                Inventory = inventory,
                Budget = budget
            };
        }

        public async Task<PlayFabItemInventory> GetItemInventory(string filterValue, OriginItemEnum originItemId)
        {
            var userInfo = await GetUserInventory();

            switch (originItemId)
            {
                case OriginItemEnum.Store:
                    return userInfo?.Inventory?.FirstOrDefault(item => item.ItemId == filterValue);
                default:
                    return userInfo?.Inventory?.FirstOrDefault(item => item.ItemInstanceId == filterValue);
            }
        }

        public async Task<PurchaseItemResponse> PurchaseItem(string itemId, int price)
        {
            var reqPurchaseItem = new PlayFab.ClientModels.PurchaseItemRequest
            {
                AuthenticationContext = new PlayFabAuthenticationContext { ClientSessionTicket = await GetSessionTicketAsync() },
                CatalogVersion = configuration.CatalogName,
                StoreId = configuration.StoreName,
                ItemId = itemId,
                VirtualCurrency = configuration.Currency,
                Price = price
            };

            var result = await PlayFabClientAPI.PurchaseItemAsync(reqPurchaseItem);

            if (result.Error != null)
            {
                throw new Exception(result.Error.ErrorMessage);
            }

            return new PurchaseItemResponse
            {
                ItemInstanceId = result?.Result?.Items?.FirstOrDefault()?.ItemInstanceId,
                Budget = await UpdateBudget()
            };
        }

        public async Task<Dictionary<string, string>> GetTitleData(List<string> keys = null)
        {
            var reqGetTitleData = new GetTitleDataRequest 
            {
                Keys = keys
            };

            var result = await PlayFabAdminAPI.GetTitleDataAsync(reqGetTitleData);
            
            if (result.Error != null)
            {
                throw new Exception(result.Error.ErrorMessage);
            }

            return result.Result.Data;
        }

        public async Task SetTitleData(string key, string value)
        {
            var reqSetTitleData = new SetTitleDataRequest 
            { 
                Key = key,
                Value = value
            };
            
            var result = await PlayFabAdminAPI.SetTitleDataAsync(reqSetTitleData);

            if (result.Error != null)
            {
                throw new Exception(result.Error.ErrorMessage);
            }
        }

        private async Task<bool> IsCacheUpdated()
        {
            var titleData = await GetTitleData();
            if (titleData.TryGetValue(CacheKeys.StoreVersion, out var storeVersion))
            {
                var storeVersionCache = cache.Get<string>(CacheKeys.StoreVersion);
                return storeVersion == storeVersionCache;
            }
            return false;
        }

        private async Task UpdateStoreVersionCache()
        {
            var titleData = await GetTitleData();
            if (titleData.TryGetValue(CacheKeys.StoreVersion, out var storeVersion))
            { 
                cache.Set(CacheKeys.StoreVersion, storeVersion, cacheEntryOptions);
            }
        }

        private async Task<string> UpdateBudget()
        {
            var userInfo = await GetUserInventory();
            var budget = $"{configuration.Currency} {userInfo?.Budget}";
            await ModifyClaim("Budget", budget);

            return budget;
        }

        private async Task ModifyClaim(string claimType, string value)
        {
            var claimsIdentity = httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
            var claim = claimsIdentity.FindFirst(claimType);

            if (claim != null)
            {
                claimsIdentity.RemoveClaim(claim);
                claimsIdentity.AddClaim(new Claim(claimType, value));

                var sessionTicket = await httpContextAccessor.HttpContext.GetTokenAsync("SessionTicket");
                var tokenExpiration = await httpContextAccessor.HttpContext.GetTokenAsync("TokenExpiration");

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                };

                authProperties.StoreTokens(new List<AuthenticationToken> {
                    new AuthenticationToken
                    {
                        Name = "SessionTicket",
                        Value = sessionTicket
                    },
                    new AuthenticationToken
                    {
                        Name = "TokenExpiration",
                        Value = tokenExpiration
                    }
                });

                claimsIdentity.AddClaim(new Claim("TokenExpiration", tokenExpiration));

                await httpContextAccessor.HttpContext.SignOutAsync();

                await httpContextAccessor.HttpContext.SignInAsync(
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
            }
        }

        private List<PlayFabItem> MergeAndGetItemList(List<CatalogItem> catalog, List<StoreItem> store)
        {
            var storeWithCurrency = store.Where(item => item.VirtualCurrencyPrices.ContainsKey(configuration.Currency)).ToList();
            var catalogWithCurrency = catalog.Where(item => item.VirtualCurrencyPrices.ContainsKey(configuration.Currency)).ToList();

            var queryStore = catalogWithCurrency
                                .Join(storeWithCurrency,
                                    itemCatalog => itemCatalog.ItemId,
                                    itemStore => itemStore.ItemId,
                                    (itemCatalog, itemStore) => new PlayFabItem
                                    {
                                        ItemId = itemCatalog.ItemId,
                                        DisplayName = itemCatalog.DisplayName,
                                        ItemImageUrl = itemCatalog.ItemImageUrl,
                                        Description = itemCatalog.Description,
                                        Currency = configuration.Currency,
                                        PriceStore = itemStore.VirtualCurrencyPrices.FirstOrDefault(pair => pair.Key == configuration.Currency).Value,
                                        CustomData = itemStore.CustomData.ToString(),
                                    });

            return queryStore.ToList();
        }

        private async Task<List<PlayFabItemInventory>> CompleteWithDataStore(List<dynamic> inventory)
        {
            var store = await GetStoreItems();

            var queryInventory = inventory.GroupBy(i => new { i.ItemId, i.DisplayName })
                            .Select(g =>
                               new
                               {
                                   g.Key.ItemId,
                                   g.Key.DisplayName,
                                   g.FirstOrDefault().ItemInstanceId,
                                   g.FirstOrDefault().CustomData
                               }
                            )
                            .GroupJoin(store, i => i.ItemId, s => s.ItemId,
                                (i, g) =>
                                    new
                                    {
                                        ItemInventory = i,
                                        ItemsStore = g
                                    }
                            )
                            .SelectMany(temp => temp.ItemsStore.DefaultIfEmpty(),
                                (r, s) =>
                                    new PlayFabItemInventory
                                    {
                                        ItemId = r.ItemInventory.ItemId,
                                        ItemInstanceId = r.ItemInventory.ItemInstanceId,
                                        DisplayName = r.ItemInventory.DisplayName,
                                        Description = s?.Description,
                                        ItemImageUrl = s?.ItemImageUrl,
                                        Currency = s.Currency,
                                        PriceStore = s.PriceStore,
                                        CustomDataStore = s?.CustomData?.ToString(),
                                        CustomDataInventory = new CustomDataInventory
                                        {
                                            IsStarter = r.ItemInventory?.CustomData?.ContainsKey("IsStarter") == true ? (r.ItemInventory.CustomData["IsStarter"] == "true" ? true : false) : false
                                        }
                                    }
                            );

            return queryInventory.ToList();
        }

        public string GetPlayFabId() => httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "PlayFabId")?.Value;

        public async Task<string> GetSessionTicketAsync()
        {
            var tokenExpiration = httpContextAccessor.HttpContext.GetTokenAsync("TokenExpiration")?.Result;

            if (string.IsNullOrWhiteSpace(tokenExpiration))
            {
                return null;
            }

            if (DateTime.Compare(DateTime.Now, DateTime.Parse(tokenExpiration)) > 0)
            {
                throw new Exception("The session has expired");
            }

            return await httpContextAccessor.HttpContext.GetTokenAsync("SessionTicket");
        }
    }
}
