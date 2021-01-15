using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FantasySoccer.Models.Configuration;
using FantasySoccer.Schema.Models;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace FantasySoccer.Core.Services
{
    public class CosmosDBService: ICosmosDBService
    {
        private readonly CosmosClient client;
        private readonly CosmosDBConfig configuration;

        public CosmosDBService(CosmosDBConfig config, bool allowBulkInsert = false)
        {
            if (config==null)
            {
                throw new ArgumentException("Argument can not be null", "config");
            }

            if (string.IsNullOrWhiteSpace(config.EndpointUri))
            {
                throw new ArgumentException("Argument can not be null", "config.EndpointUri");
            }

            if (string.IsNullOrWhiteSpace(config.PrimaryKey))
            {
                throw new ArgumentException("Argument can not be null", "config.PrimaryKey");
            }

            if (string.IsNullOrWhiteSpace(config.DatabaseName))
            {
                throw new ArgumentException("Argument can not be null", "config.DatabaseName");
            }

            configuration = config;

            client = new CosmosClient(config.EndpointUri, config.PrimaryKey, 
                new CosmosClientOptions 
                { 
                    AllowBulkExecution = allowBulkInsert,
                    MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(5),
                    MaxRetryAttemptsOnRateLimitedRequests = 5
                });
        }

        public async Task CreateContainerAsync(string containerId, string partitionKeyPath)
        {
            await client.GetDatabase(configuration.DatabaseName).CreateContainerIfNotExistsAsync(containerId, partitionKeyPath);
        }

        public async Task DeleteContainerAsync(Container container)
        {
            await container.DeleteContainerAsync();
        }

        public Container GetContainer(string containerId)
        {
            return client.GetContainer(configuration.DatabaseName, containerId);
        }

        public async Task AddItemsAsync<T>(Container container, T item, string partitionKey, ItemRequestOptions itemRequestOptions = null)
        {
            var partitionKeyStruct = new PartitionKey(partitionKey);
            await container.CreateItemAsync<T>(item, partitionKeyStruct, itemRequestOptions);
        }

        public async Task DeleteItemsAsync<T>(Container container, string id, string partitionKey, ItemRequestOptions itemRequestOptions = null)
        {
            var partitionKeyStruct = new PartitionKey(partitionKey);
            await container.DeleteItemAsync<T>(id, partitionKeyStruct, itemRequestOptions);
        }

        public async Task UpdateItemAsync(Container container, GeneralModel item, ItemRequestOptions itemRequestOptions = null)
        {
            await container.UpsertItemAsync(item, new PartitionKey(item.ID), itemRequestOptions);
        }

        public async Task<T> GetItemById<T>(Container container, string id)
        {
            var query = $"SELECT * FROM c WHERE c.id = '{id}'";
            var results = await GetItemsAsync<T>(container, query);
            return results.FirstOrDefault();
        }

        public async Task AddBulkItemsAsync<T>(Container container, List<T> items) where T : GeneralModel
        {
            var itemsToInsert = new Dictionary<string, Stream>(items.Count);

            foreach (var item in items)
            {
                var itemString = JsonConvert.SerializeObject(item);
                var stream = new MemoryStream(Encoding.ASCII.GetBytes(itemString));
                itemsToInsert.Add(item.ID, stream);
            }

            Task task(Container container, Stream value, string key)
            {
                return AddItemStreamAsync(container, value, key)
                    .ContinueWith((Task<ResponseMessage> task) =>
                    {
                        using (var response = task.Result)
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"Task sucessfully executed");
                            }
                            else
                            {
                                Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                            }
                        }
                    });
            }

            await GenerateTasks(container, itemsToInsert, task);
        }

        public async Task<List<T>> GetItemsAsync<T>(Container container, string customQuery = null)
        {
            var sqlQueryText = customQuery ?? "SELECT * FROM c";
            var queryDefinition = new QueryDefinition(sqlQueryText);
            var queryResultSetIterator = container.GetItemQueryIterator<T>(queryDefinition);
            var result = new List<T>();

            while (queryResultSetIterator.HasMoreResults)
            {
                var currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (var document in currentResultSet)
                {
                    result.Add(document);
                }
            }

            return result;
        }

        public async Task UpdateItemsAsync<T>(Container container, List<T> items) where T : GeneralModel
        {
            var itemsToInsert = new Dictionary<string, Stream>(items.Count);

            foreach (var item in items)
            {
                var itemString = JsonConvert.SerializeObject(item);
                var stream = new MemoryStream(Encoding.ASCII.GetBytes(itemString));
                itemsToInsert.Add(item.ID, stream);
            }

            Task task(Container container, Stream value, string key)
            {
                return UpdateItemStreamAsync(container, value, key)
                    .ContinueWith((Task<ResponseMessage> task) =>
                    {
                        using (var response = task.Result)
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"Task sucessfully executed");
                            }
                            else
                            {
                                Console.WriteLine($"Received {response.StatusCode} ({response.ErrorMessage}).");
                            }
                        }
                    });
            }

            await GenerateTasks(container, itemsToInsert, task);
        }

        private async Task<ResponseMessage> UpdateItemStreamAsync(Container container, Stream item, string partitionKey, ItemRequestOptions itemRequestOptions = null)
        {
            var partitionKeyStruct = new PartitionKey(partitionKey);
            return await container.UpsertItemStreamAsync(item, partitionKeyStruct, itemRequestOptions);
        }

        private async Task<ResponseMessage> AddItemStreamAsync(Container container, Stream item, string partitionKey, ItemRequestOptions itemRequestOptions = null)
        {
            var partitionKeyStruct = new PartitionKey(partitionKey);
            return await container.CreateItemStreamAsync(item, partitionKeyStruct, itemRequestOptions);
        }

        private async Task GenerateTasks(Container container, Dictionary<string, Stream> itemsToInsert, Func<Container, Stream, string, Task> task)
        {
            var tasks = new List<Task>(itemsToInsert.Count);

            foreach (var item in itemsToInsert)
            {
                tasks.Add(task(container, item.Value, item.Key));
            }

            await Task.WhenAll(tasks);
        }
    }
}
