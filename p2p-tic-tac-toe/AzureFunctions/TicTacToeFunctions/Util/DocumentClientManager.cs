using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using TicTacToeFunctions.Models;

namespace TicTacToeFunctions.Util
{
    public class DocumentClientManager
    {
        public static async Task<List<T>> FilterDocumentClient<T>(DocumentClient client, string databaseName, string collectionName, Expression<Func<T, bool>> filter = null)
        {
            var collectionUri = CreateDocumentCollectionUri(databaseName, collectionName);
            var options = CreateFeedOptions();
            var query = client.CreateDocumentQuery<T>(collectionUri, options)
                    .AsDocumentQuery();

            var results = await GetValuesFromDocumentQueryAsyc(query);

            // TODO: improve where filter. Keep in mind this: 
            // https://stackoverflow.com/questions/33839854/c-sharp-linq-any-not-working-on-documentdb-createdocumentquery
            if (filter != null)
            {
                results = results.Where(filter.Compile()).ToList();
            }

            return results;
        }

        public static async Task DeleteDocumentFromClientAsync(DocumentClient client, Document document, object partitionKey)
        {
            var options = CreateRequestOptions(partitionKey);

            await client.DeleteDocumentAsync(document.SelfLink, options);
        }

        public static async Task ReplaceDocument<T>(DocumentClient client, T data, string databaseName, string collectionName) where T : ICustomDocument
        {
            var uri = UriFactory.CreateDocumentUri(databaseName, collectionName, data.Id);

            await client.ReplaceDocumentAsync(
                uri,
                data,
                new RequestOptions
                {
                    AccessCondition = new AccessCondition
                    {
                        Type = AccessConditionType.IfMatch,
                        Condition = data.ETag
                    }
                });
        }

        private static Uri CreateDocumentCollectionUri(string databaseId, string collectionId)
        {
            if (string.IsNullOrWhiteSpace(databaseId))
            {
                throw new ArgumentNullException("databaseId", "databaseId can not be null or empty");
            }

            if (string.IsNullOrWhiteSpace(collectionId))
            {
                throw new ArgumentNullException("collectionId", "collectionId can not be null or empty");
            }

            return UriFactory.CreateDocumentCollectionUri(databaseId: databaseId, collectionId: collectionId);
        }

        private static FeedOptions CreateFeedOptions(bool enableCrossPartitionQuery = true)
        {
            return new FeedOptions
            {
                EnableCrossPartitionQuery = enableCrossPartitionQuery
            };
        }

        private static async Task<List<T>> GetValuesFromDocumentQueryAsyc<T>(IDocumentQuery<T> query)
        {
            List<T> results = new List<T>();

            while (query.HasMoreResults)
            {
                foreach (var element in await query.ExecuteNextAsync())
                {
                    results.Add((T)element);
                }
            }

            return results;
        }

        private static RequestOptions CreateRequestOptions(object partitionKey)
        {
            return new RequestOptions
            {
                PartitionKey = new PartitionKey(partitionKey)
            };
        }
    }
}
