using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.AspNetCore.Mvc;

namespace Search.Api;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private const string ProductIndex = "products_index";

    private readonly ElasticsearchClient _elasticsearchClient;

    public ProductsController(ElasticsearchClient elasticsearchClient)
    {
        _elasticsearchClient = elasticsearchClient;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] Product request)
    {
        await SearchHelper.CreateIndexAsync(_elasticsearchClient, ProductIndex);
        var indexRequest = new IndexRequest<Product>(request);
        var response = await _elasticsearchClient.IndexAsync(request, idx => idx.Index(ProductIndex));
        if (!response.IsValidResponse)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        var response = await _elasticsearchClient.GetAsync<Product>(ProductIndex, id);
        if (!response.IsValidResponse)
        {
            return BadRequest(response);
        }

        return Ok(response.Source);
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchAsync([FromBody] ProductSearchRequest request)
    {
        var terms = new List<Query>
        {
            new TermQuery("name")
            {
                Value = request.Query.ToLower(),
            }
        };

        if (request.IsFreeForm)
        {
            terms.Add(new TermQuery("isFreeForm") { Value = request.IsFreeForm });
        }

        var bq = new BoolQuery() { Must = terms };        
        var searchRequest = new SearchRequest(ProductIndex)
        {
            From = 0,
            Size = 10,
            Query = bq,
        };

        var response = await _elasticsearchClient.SearchAsync<Product>(searchRequest);
        if (!response.IsValidResponse)
        {
            return BadRequest(response);
        }

        return Ok(response.Documents);
    }
}

public class ProductSearchRequest
{
   public string Query { get; set; }
   public bool IsFreeForm { get; set; }
}