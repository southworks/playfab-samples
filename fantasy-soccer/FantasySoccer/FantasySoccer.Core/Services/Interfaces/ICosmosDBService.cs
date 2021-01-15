using System.Collections.Generic;
using System.Threading.Tasks;
using FantasySoccer.Schema.Models;
using Microsoft.Azure.Cosmos;

namespace FantasySoccer.Core.Services
{
    public interface ICosmosDBService
    {
        Task CreateContainerAsync(string containerId, string partitionKeyPath);

        Task DeleteContainerAsync(Container container);

        Container GetContainer(string containerId);
        
        Task AddItemsAsync<T>(Container container, T item, string partitionKey, ItemRequestOptions itemRequestOptions = null);
        
        Task DeleteItemsAsync<T>(Container container, string id, string partitionKey, ItemRequestOptions itemRequestOptions = null);
        
        Task UpdateItemAsync(Container container, GeneralModel item, ItemRequestOptions itemRequestOptions = null);
        
        Task UpdateItemsAsync<T>(Container container, List<T> items) where T : GeneralModel;

        Task<T> GetItemById<T>(Container container, string id);
        
        Task AddBulkItemsAsync<T>(Container container, List<T> items) where T : GeneralModel;
        
        Task<List<T>> GetItemsAsync<T>(Container container, string customQuery = null);
    }
}
