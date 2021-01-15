using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FantasySoccer.Schema.Models;
using FantasySoccerDataSeeder.Models;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace FantasySoccerDataSeeder.Services
{
    public class CosmosDBService: IDataManagementService
    {
        private readonly CosmosClient _client;

        public CosmosDBService(IDataManagementConfig config)
        {
            _=config??throw new ArgumentException("Argument can not be null", "config.EndpointUri");
            _=string.IsNullOrWhiteSpace(config.EndpointUri) ? throw new ArgumentException("Argument can not be null", "config.EndpointUri") : "";
            _=string.IsNullOrWhiteSpace(config.PrimaryKey) ? throw new ArgumentException("Argument can not be null", "config.PrimaryKey") : "";

            _client=new CosmosClient(config.EndpointUri, config.PrimaryKey);
        }

        public async Task CreateDatabaseIfNotExistsAsync(string databaseId)
        {
            await _client.CreateDatabaseIfNotExistsAsync(databaseId);
        }

        public async Task CreateContainerAsync(string databaseId, string containerId, string partitionKeyPath)
        {
            await _client.GetDatabase(databaseId).CreateContainerIfNotExistsAsync(containerId, partitionKeyPath);
        }

        public Container GetContainer(string databaseId, string containerId)
        {
            return _client.GetContainer(databaseId, containerId);
        }

        public async Task DeleteContainerAsync(string databaseId, string containerId)
        {
            await GetContainer(databaseId, containerId).DeleteContainerAsync();
        }

        public async Task AddItemsAsync<T>(Container container, T item, string partitionKey, ItemRequestOptions itemRequestOptions = null)
        {
            var partitionKeyStruct = new PartitionKey(partitionKey);
            await container.CreateItemAsync<T>(item, partitionKeyStruct, itemRequestOptions);
        }

        public async Task<ResponseMessage> AddItemStreamAsync(Container container, Stream item, string partitionKey, ItemRequestOptions itemRequestOptions = null)
        {
            var partitionKeyStruct = new PartitionKey(partitionKey);
            return await container.CreateItemStreamAsync(item, partitionKeyStruct, itemRequestOptions);
        }

        public async Task AddBulkItemsAsync<T>(Container container, List<T> items) where T : GeneralModel {
            var itemsToInsert = new Dictionary<string, Stream>(items.Count);
            foreach (var item in items)
            {
                var itemString = JsonConvert.SerializeObject(item);
                var stream = new MemoryStream(Encoding.ASCII.GetBytes(itemString));
                itemsToInsert.Add(item.ID, stream);
            }
            await GenerateTasks(container, itemsToInsert);
        }

        private async Task GenerateTasks(Container container, Dictionary<string, Stream> itemsToInsert){
            var tasks = new List<Task>(itemsToInsert.Count);

            foreach (var item in itemsToInsert)
            {
                tasks.Add(AddItemStreamAsync(container, item.Value, item.Key)
                    .ContinueWith((Task<ResponseMessage> task) =>
                    {
                        using (var response = task.Result)
                        {
                            if (!response.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                            }
                        }
                    }));
            }

            await Task.WhenAll(tasks);
        }

    }
}
