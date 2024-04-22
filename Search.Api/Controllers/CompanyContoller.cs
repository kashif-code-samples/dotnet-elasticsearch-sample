using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.TermVectors;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.AspNetCore.Mvc;

namespace Search.Api;

[ApiController]
[Route("[controller]")]
public class CompanyController : ControllerBase
{
    private const string CompanyIndex = "company_index";

    private readonly ElasticsearchClient _elasticsearchClient;

    public CompanyController(ElasticsearchClient elasticsearchClient)
    {
        _elasticsearchClient = elasticsearchClient;
    }

        [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] Company request)
    {
        await SearchHelper.CreateIndexAsync(_elasticsearchClient, CompanyIndex);
        var indexRequest = new IndexRequest<Company>(request);
        var response = await _elasticsearchClient.IndexAsync(request, idx => idx.Index(CompanyIndex));
        if (!response.IsValidResponse)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync(int id)
    {
        var response = await _elasticsearchClient.GetAsync<Company>(CompanyIndex, id);
        if (!response.IsValidResponse)
        {
            return BadRequest(response);
        }

        return Ok(response.Source);
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchAsync([FromBody] CompanySearchRequest request)
    {
        var termQuery = new TermQuery("name")
        {
            Value = request.Query.ToLower(),
        };

        var terms = new List<Query>
        {
            termQuery
        };

        if (request.SirenOnly)
        {
            var q = new BoolQuery()
            {
                Should = new List<Query>
                {
                    new BoolQuery
                    {
                        Must = new List<Query>
                        {
                            new TermQuery("country")
                            {
                                Value = "fr"
                            },
                            new BoolQuery
                            {
                                MustNot = new List<Query>
                                {
                                    new ExistsQuery
                                    {
                                        Field = "isHeadOffice"
                                    }
                                }
                            }
                        },
                    },
                    new BoolQuery
                    {
                        MustNot = new List<Query>
                        {
                            new TermQuery("country")
                            {
                                Value = "fr"
                            },
                        }
                    }
                }
            };

            terms.Add(q);
        }

        var boolQuery = new BoolQuery() { Must = terms };

        var searchRequest = new SearchRequest(CompanyIndex)
        {
            From = 0,
            Size = 10,
            Query = boolQuery,
        };

        var response = await _elasticsearchClient.SearchAsync<Company>(searchRequest);
        if (!response.IsValidResponse)
        {
            return BadRequest(response);
        }

        return Ok(response.Documents);
    }
}

public class CompanySearchRequest
{
    public string Query { get; set; }
    public string CountryCode { get; set; }
    public bool SirenOnly { get; set; }
}