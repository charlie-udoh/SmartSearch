using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Microsoft.Extensions.Configuration;
using Nest;
using SmartSearch.Core.Entities;
using System;

namespace SmartSearch.Core.Services
{
    public class ElasticSearchClient : ISearchClient
    {
        private readonly IConfiguration _configuration;
        private readonly string _url;
        private IElasticClient _client;
        private readonly string _awsRegion;

        public ElasticSearchClient(IConfiguration configuration)
        {
            _configuration = configuration;
            _url = _configuration["ElasticSearch:Url"];
            _awsRegion = configuration["AWS:Region"];
            Initialize();
        }

        public void Initialize()
        {
            var httpConnection = new AwsHttpConnection(_awsRegion);
            var pool = new SingleNodeConnectionPool(new Uri(_url));
            var settings = new ConnectionSettings(pool, httpConnection);
            //var settings = new ConnectionSettings(new Uri(_url));
            AddDefaultMappings(settings);
            _client = new ElasticClient(settings);
            AddIndexMappings(_client);
        }

        private void AddDefaultMappings(ConnectionSettings settings)
        {
            settings
                .DefaultMappingFor<Management>(m => m
                    .IndexName("management")
                )
                .DefaultMappingFor<Property>(m => m
                    .IndexName("property")
                );
        }

        private void AddIndexMappings(IElasticClient client)
        {
            if (!client.Indices.Exists("property").Exists)
            {
                var createPropResponse = client.Indices.Create("property", i => i
                    .Map<Property>(m => m
                        .AutoMap()
                        .Properties(ps => ps
                            .Number(n => n
                                .Name(e => e.propertyID)
                                .Type(NumberType.Integer)
                            )
                            .Text(s => s
                                .Name(np => np.name)
                                .Analyzer("standard")
                            )
                            .Text(s => s
                                .Name(np => np.formerName)
                                .Analyzer("standard")
                            )
                            .Text(s => s
                                .Name(np => np.streetAddress)
                                .Analyzer("standard")
                            )
                            .Text(s => s
                                .Name(np => np.city)
                                .Analyzer("standard")
                            )
                            .Text(s => s
                                .Name(np => np.state)
                                .Analyzer("standard")
                            )
                            .Text(c => c
                                .Name(np => np.market)
                                .Fields(ff => ff
                                    .Text(tt => tt
                                        .Name(np => np.market)
                                        .Analyzer("standard")
                                    )
                                    .Keyword(k => k
                                        .Name("keyword")
                                        .IgnoreAbove(256)
                                     )
                                )
                            )
                            .Number(n => n
                                .Name(e => e.lat)
                                .Type(NumberType.Double)
                            )
                            .Number(n => n
                                .Name(e => e.lng)
                                .Type(NumberType.Double)
                            )
                        )
                    )
                );
            }

            if (!client.Indices.Exists("management").Exists)
            {
                var createMgmtResponse = client.Indices.Create("management", i => i
                    .Map<Management>(map => map
                        .AutoMap()
                        .Properties(ps => ps
                            .Number(n => n
                                .Name(e => e.mgmtID)
                                .Type(NumberType.Integer)
                            )
                            .Text(s => s
                                .Name(np => np.name)
                                .Analyzer("standard")
                            )
                            .Text(s => s
                                .Name(np => np.state)
                                .Analyzer("standard")
                            )
                            .Text(c => c
                                .Name(np => np.market)
                                .Fields(ff => ff
                                    .Text(tt => tt
                                        .Name(np => np.market)
                                        .Analyzer("standard")
                                    )
                                    .Keyword(k => k
                                        .Name("keyword")
                                        .IgnoreAbove(256)
                                     )
                                )
                            )
                        )
                    )
                );
            }
        }

        public IElasticClient Client
        {
            get
            {
                if (_client == null)
                    Initialize();
                return _client;
            }
        }
    }
}
