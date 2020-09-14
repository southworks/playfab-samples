// Copyright (C) Microsoft Corporation. All rights reserved.

using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TicTacToeFunctions.Models.Helpers;

namespace TicTacToeFunctions.Util
{
    public class DocumentClientManager
    {
        public static async Task<List<T>> GetDocumentsByIdAsync<T>(DocumentClient client, string databaseName, string collectionName, string id, bool enableCrossPartitionQuery = true) where T : IMatchId
        {
            var collectionUri = CreateDocumentCollectionUri(databaseName, collectionName);
            var options = CreateFeedOptions(enableCrossPartitionQuery);

            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(collectionUri, options)
                        .Where(elem => elem.MatchId == id)
                        .AsDocumentQuery();

            return await GetValuesFromDocumentQueryAsyc(query);
        }

        public static async Task<List<T>> FilterDocumentClient<T>(DocumentClient client, string databaseName, string collectionName, Expression<Func<T, bool>> filter = null)
        {
            var collectionUri = CreateDocumentCollectionUri(databaseName, collectionName);
            var options = CreateFeedOptions();

            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(collectionUri, options)
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

        public static async Task<Document> ReplaceDocument(DocumentClient client, string databaseName, string collectionName, Document document)
        {
            ResourceResponse<Document> response = await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, document.Id), document);
            return response.Resource;
        }
    }
}
