using Elastic.Clients.Elasticsearch;

namespace Search.Api;

public class SearchHelper
{
    public static async Task CreateIndexAsync(ElasticsearchClient elasticsearchClient, string indexName)
    {
        var response = await elasticsearchClient.Indices.ExistsAsync(indexName);
        if (!response.Exists)
        {
            var createResponse = await elasticsearchClient.Indices.CreateAsync(indexName);
        }
    }
}
