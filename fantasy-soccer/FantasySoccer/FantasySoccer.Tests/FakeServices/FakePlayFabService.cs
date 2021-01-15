using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FantasySoccer.Core.Configuration;
using FantasySoccer.Core.Models.Responses;
using FantasySoccer.Core.Services;
using FantasySoccer.Schema.Models;
using FantasySoccer.Schema.Models.PlayFab;
using Moq;
using Newtonsoft.Json;
using PlayFab.ClientModels;

namespace FantasySoccer.Tests.FakeServices
{
    public class FakePlayFabService
    {
        public readonly Mock<IPlayFabService> stub;

        public List<PlayFab.AdminModels.ItemInstance> InventoryUsingAdminAPI { get; set; }

        public List<StatisticValue> Statistics { get; set; } = new List<StatisticValue>();

        public List<PlayFabItem> Store { get; set; } = new List<PlayFabItem>();

        public List<PlayFabItemInventory> Inventory { get; set; } = new List<PlayFabItemInventory>();

        public Dictionary<string, string> TitleData { get; set; } = new Dictionary<string, string>();

        public int Budget { get; set; } = 0;

        public string PlayFabId { get; set; } = "1";

        public string Currency { get; set; } = "FS";

        public FakePlayFabService()
        {
            stub = new Mock<IPlayFabService>();
            stub.Setup(x => x.GetPlayFabId()).Returns(PlayFabId);

            stub.Setup(x => x.GetUserStatisticAsync(It.IsAny<List<string>>()))
                   .ReturnsAsync(() => Statistics);

            stub.Setup(x => x.UpdatePlayerStatistics(It.IsAny<string>(), It.IsAny<Dictionary<string,int>>()))
                   .Returns((string playFabId, Dictionary<string,int> statistic) => UpdateStatistic(statistic));

            stub.Setup(x => x.GetUserInventoryUsingAdminAPI(It.IsAny<string>()))
                   .ReturnsAsync(() => InventoryUsingAdminAPI);

            stub.Setup(x => x.GetStoreItems())
                   .ReturnsAsync(() => Store);

            stub.Setup(x => x.GetStoreItems(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<Func<FutbolPlayer, bool>?>()))
                  .ReturnsAsync((int? size, int? skip, Func<FutbolPlayer, bool> filter) => GetPaginatedStore());
           
            stub.Setup(x => x.GetTitleData(new List<string> { PlayFabConstants.CurrentTournamentId }))
                   .ReturnsAsync(() => TitleData);

            stub.Setup(x => x.GetUserInventory())
                   .ReturnsAsync(() => GetBudgetAndInventory());

            stub.Setup(x => x.UpdateUserInventoryItemCustomData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
                .Returns((string playFabId, string itemInstanceId, Dictionary<string, string> data) => UpdateCustomDataItemInventory(itemInstanceId, data));

            stub.Setup(x => x.PurchaseItem(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((string itemId, int price) => AddInventoryItem(itemId, price));

            stub.Setup(x => x.SetTitleData(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string key, string value) => SetMockedTitle(key, value));

            stub.Setup(x => x.RevokeInventoryItem(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string playFabId, string itemInstance) => DeleteInventoryItem(itemInstance));

            stub.Setup(x => x.AddUserVirtualCurrency(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((string playFabId, int amount) => UpdateBudget(amount));

            stub.Setup(x => x.GetItemInventory(It.IsAny<string>(), It.IsAny<OriginItemEnum>()))
               .ReturnsAsync((string filterValue, OriginItemEnum originItemId) => GetItemInventory(filterValue, originItemId));

            stub.Setup(x => x.SetStoreItems(It.IsAny<List<FutbolPlayer>>()))
               .Returns((List<FutbolPlayer> players) => SetStoreItems(players));

            stub.Setup(x => x.SetCatalogItems(It.IsAny<List<FutbolPlayer>>()))
              .Returns((List<FutbolPlayer> players) => Task.CompletedTask);
        }

        private Task UpdateStatistic(Dictionary<string, int> statistic)
        {
            foreach (var newStatistic in statistic)
            {
                var index = Statistics.FindIndex(item => item.StatisticName == newStatistic.Key);
                if ( index >= 0 )
                {
                    Statistics[index].Value = newStatistic.Value;
                }
                else
                {
                    Statistics.Add(new StatisticValue
                    {
                        StatisticName = newStatistic.Key,
                        Value = newStatistic.Value
                    });
                }
            }
            return Task.CompletedTask;
        }

        private string UpdateBudget(int amount)
        {
            Budget += amount;
            return GetBudgetDescription();
        }

        private Task SetMockedTitle(string key, string value)
        {
            TitleData[key] = value;
            return Task.CompletedTask;
        }

        private PaginatedItem<FutbolPlayer> GetPaginatedStore()
        {
            var players = new List<FutbolPlayer>();
            foreach (var item in Store)
            {
                var futbolPlayer = JsonConvert.DeserializeObject<FutbolPlayer>(item.CustomData);
                futbolPlayer.GeneralStats = new SoccerPlayerGeneralStats { };
                players.Add(futbolPlayer);
            };

            return new PaginatedItem<FutbolPlayer>
            {
                PaginatedItems = players,
                TotalItems = players.Count
            };
        }

        private UserInventoryResponse GetBudgetAndInventory()
        {
            return new UserInventoryResponse
            {
                Inventory = Inventory ?? new List<PlayFabItemInventory>(),
                Budget = Budget
            };
        }

        private Task UpdateCustomDataItemInventory(string itemInstanceId, Dictionary<string, string> data)
        {
            var index = Inventory.FindIndex(i => i.ItemInstanceId == itemInstanceId);
            if (index >= 0 && data.ContainsKey("IsStarter"))
            {
                Inventory[index].CustomDataInventory.IsStarter = data["IsStarter"] == "true";
            }
            return Task.CompletedTask;
        }

        private PurchaseItemResponse AddInventoryItem(string itemId, int price)
        {
            var itemInstanceId = Guid.NewGuid().ToString();
            var itemStore = Store.FirstOrDefault(i => i.ItemId == itemId);

            if (itemStore != null && itemStore.PriceStore == price)
            {
                Inventory.Add(new PlayFabItemInventory {
                    ItemId = itemStore.ItemId,
                    ItemInstanceId = itemInstanceId,
                    DisplayName = itemStore.DisplayName,
                    Description = itemStore.Description,
                    ItemImageUrl = itemStore.ItemImageUrl,
                    Currency = itemStore.Currency,
                    PriceStore = itemStore.PriceStore,
                    CustomDataStore = itemStore.CustomData,
                    CustomDataInventory = new CustomDataInventory {
                        IsStarter = false
                    },
                    PurchaseDate = DateTime.Now,
                });
                SubstractBudget((int)itemStore.PriceStore);
            }

            return new PurchaseItemResponse
            {
                ItemInstanceId = itemInstanceId,
                Budget = GetBudgetDescription()
            };
        }

        private Task DeleteInventoryItem(string itemInstanceId)
        {
            var index = Inventory.FindIndex(i => i.ItemInstanceId == itemInstanceId);
            if (index >= 0)
            {
                Inventory.RemoveAt(index);
            }
            return Task.CompletedTask;
        }

        private PlayFabItemInventory GetItemInventory(string filterValue, OriginItemEnum originItemId)
        {
            return originItemId switch
            {
                OriginItemEnum.Store => Inventory?.FirstOrDefault(item => item.ItemId == filterValue),
                _ => Inventory?.FirstOrDefault(item => item.ItemInstanceId == filterValue),
            };
        }

        private void SubstractBudget(int quantity)
        {
            Budget -= quantity;
        }

        private string GetBudgetDescription()
        {
            return $"{Currency} {Budget}";
        }

        private Task SetStoreItems(List<FutbolPlayer> players)
        {
            var items = new List<PlayFabItem>();
            players.ForEach(p => items.Add(new PlayFabItem
            {
                ItemId = p.ID,
                DisplayName = p.GetFullName(),
                Currency = Currency,
                PriceStore = p.Price,
                CustomData = JsonConvert.SerializeObject(p)
            }));
            Store = items;
            return Task.CompletedTask;
        }
    }
}
