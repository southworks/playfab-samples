using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace FantasySoccerDataSeeder.Models
{
    public interface IDataManagementService
    {
        Task CreateContainerAsync(string databaseId, string containerId, string partitionKeyPath);
        Container GetContainer(string databaseId, string containerId);
        Task DeleteContainerAsync(string databaseId, string containerId);
        Task AddItemsAsync<T>(Container container, T item, string partitionKey, ItemRequestOptions itemRequestOptions = null);
    }
}
