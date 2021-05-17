using Nest;
using Newtonsoft.Json;
using SmartSearch.Core.DTOs;
using SmartSearch.Core.Entities;
using SmartSearch.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SmartSearch.Core.Services
{
    public class DataService : IDataService
    {
        private readonly ISearchClient _searchClient;

        public DataService(ISearchClient searchClient)
        {
            _searchClient = searchClient;
        }

        public async Task<object> SearchData(string query, List<string> market, int limit, int skip)
        {
            var propertyfilters = new List<Func<QueryContainerDescriptor<object>, QueryContainer>>();
            var managementFilters = new List<Func<QueryContainerDescriptor<object>, QueryContainer>>();
            if (market.Any())
            {
                propertyfilters.Add(fq => fq.Terms(t => t.Field("property.market.keyword").Terms(market)));
                managementFilters.Add(fq => fq.Terms(t => t.Field("mgmt.market.keyword").Terms(market)));
            }

            var searchResponse = await _searchClient.Client.SearchAsync<object>(s => s
                .Index(Indices.Index(typeof(Property)).And(typeof(Management)))
                .Query(q => q
                    .MultiMatch(m => m
                        .Fields(f => f
                            .Field(Infer.Field<PropertyIndex>(ff => ff.property.city))
                            .Field(Infer.Field<PropertyIndex>(ff => ff.property.formerName))
                            .Field(Infer.Field<PropertyIndex>(ff => ff.property.market))
                            .Field(Infer.Field<PropertyIndex>(ff => ff.property.name))
                            .Field(Infer.Field<PropertyIndex>(ff => ff.property.state))
                            .Field(Infer.Field<PropertyIndex>(ff => ff.property.streetAddress))
                        )
                        .Operator(Operator.Or)
                        .Query(query)
                    ) && +q.Bool(bq => bq.Filter(propertyfilters))
                    ||
                    q
                    .MultiMatch(m => m
                        .Fields(f => f
                            .Field(Infer.Field<ManagementIndex>(ff => ff.mgmt.market))
                            .Field(Infer.Field<ManagementIndex>(ff => ff.mgmt.name))
                            .Field(Infer.Field<ManagementIndex>(ff => ff.mgmt.state))
                        )
                        .Operator(Operator.Or)
                        .Query(query)
                    ) && +q.Bool(bq => bq.Filter(managementFilters))
                )
                .From((skip - 1) * limit)
                .Size(limit)
            );

            return searchResponse.Documents;
        }

        public async Task<DataServiceResponse> SaveData(string filePath, string documentType)
        {
            var jsonString = File.ReadAllText(filePath);
            switch (documentType)
            {
                case "property":
                    var properties = JsonConvert.DeserializeObject<List<PropertyIndex>>(jsonString);
                    var propResp = await _searchClient.Client.IndexManyAsync(properties, documentType);
                    if (!propResp.IsValid)
                        return new DataServiceResponse { Successful = false, Message = "Unable to index Properties data" };
                    break;
                case "management":
                    var managements = JsonConvert.DeserializeObject<List<ManagementIndex>>(jsonString);
                    var mgmtResp = await _searchClient.Client.IndexManyAsync(managements, documentType);
                    if (!mgmtResp.IsValid)
                        return new DataServiceResponse { Successful = false, Message = "Unable to index Management data" };
                    break;
                default:
                    return new DataServiceResponse { Successful = false, Message = "Document Type is invalid" };
            }
            await _searchClient.Client.Indices.RefreshAsync(documentType);
            return new DataServiceResponse { Successful = true, Message = "Indexed successfully" };
        }

        public string[] GetAllowedDocumentTypes()
        {
            return new string[] { "property", "management" };
        }
    }
}
