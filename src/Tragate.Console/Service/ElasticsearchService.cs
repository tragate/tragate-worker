using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hangfire;
using Microsoft.Extensions.Options;
using Nest;
using Serilog;
using Tragate.Console.Dto;
using Tragate.Console.Dto.Base;
using Tragate.Console.Helper;
using Tragate.Console.Infrastructure;
using Tragate.Console.Repository;

namespace Tragate.Console.Service
{
    public class ElasticsearchService : IElasticsearchService
    {
        private readonly ConfigSettings _settings;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProductRepository _productRepository;

        public ElasticsearchService(IOptions<ConfigSettings> settings,
            ICompanyRepository companyRepository,
            IProductRepository productRepository){
            _companyRepository = companyRepository;
            _productRepository = productRepository;
            _settings = settings.Value;
        }

        public void CreateIndex(string indexName, IJobCancellationToken cancellationToken){
            var _elasticClient = GetConnection(indexName);
            _elasticClient.CreateIndex(indexName, c => c
                .Settings(s => s
                    .NumberOfReplicas(0)
                    .NumberOfShards(1)
                    .RefreshInterval("5s")
                    .Analysis(a => a
                        .Analyzers(ad => ad
                            .Custom("path_hierarchy_analyzer", ca => ca
                                .Tokenizer("path_hierarchy_tokenizer")
                            ).Custom("turkish_analyzer", ca => ca
                                .Tokenizer("standard")
                                .Filters("lowercase", "my_ascii_folding"))
                        )
                        .Tokenizers(t => t
                            .Pattern("path_hierarchy_tokenizer", ph => ph
                                .Pattern("/")
                            )
                        ).TokenFilters(t => t.AsciiFolding("my_ascii_folding",
                            to => to.PreserveOriginal()))
                    )
                )
                .Mappings(m => m
                    .Map<Root>(mp => mp
                        .RoutingField(r => r.Required())
                        .AutoMap<CompanyDto>()
                        .AutoMap<ProductDto>()
                        .Properties(props => props
                            .Join(j => j.Name(p => p.JoinField)
                                .Relations(r => r.Join<CompanyDto, ProductDto>())))
                        .Properties(p => p.Text(t => t.Name(n => n.CategoryPath)
                            .Analyzer("path_hierarchy_analyzer")))
                        .Properties(p => p.Text(t => t.Name(n => n.Slug)
                            .Analyzer("keyword")))
                        .Properties(p => p.Text(t => t.Name(n => n.CategoryTags)
                            .Analyzer("keyword")))
                        .Properties(p => p.Text(t => t.Name(n => n.Title)
                            .Analyzer("turkish_analyzer")))
                        .Properties(p => p.Text(t => t.Name(n => n.CategoryText)
                            .Analyzer("turkish_analyzer")))
                    )
                )
            );

            IndexCompany(indexName, cancellationToken);
            IndexProduct(indexName, cancellationToken);
        }

        private void IndexCompany(string indexName, IJobCancellationToken cancellationToken){
            var counter = 0;
            List<CompanyDto> companies;
            do{
                companies = _companyRepository.GetCompanies(counter, 5000);
                cancellationToken.ThrowIfCancellationRequested();
                Bulk<Root>(companies, indexName);
                counter++;
            } while (companies.Any());
        }

        private void IndexProduct(string indexName, IJobCancellationToken cancellationToken){
            var counter = 0;
            List<ProductDto> products;
            do{
                products = _productRepository.GetProducts(counter, 5000);
                cancellationToken.ThrowIfCancellationRequested();
                Bulk(products, indexName);
                counter++;
            } while (products.Any());
        }

        private void Bulk<T>(IEnumerable<T> list, string indexName) where T : class{
            if (!list.Any()) return;
            var waitHandle = new CountdownEvent(1);
            var bulkAll = GetConnection(indexName).BulkAll(list,
                b => b.Index(indexName)
                    .BackOffRetries(2)
                    .BackOffTime("30s")
                    .RefreshOnCompleted()
                    .Size(1000)
            );

            bulkAll.Subscribe(new BulkAllObserver(
                onNext: (b) => { Log.Information($"Tragate.Worker.ReIndexJob all documents has been imported"); },
                onError: (e) => throw e,
                onCompleted: () => waitHandle.Signal()
            ));
            waitHandle.Wait();
        }

        /// <summary>
        /// change alias,remove old indices skip 2 index
        /// </summary>
        /// <param name="indexName"></param>
        public void DoConfigure(string indexName){
            GetConnection(indexName).Alias(a => a.Add(i => i.Index(indexName).Alias(TragateConstants.AliasName)));
            var indices = GetIndexNames(TragateConstants.AliasName);
            foreach (var oldIndexName in indices){
                if (oldIndexName.Trim() != indexName.Trim()){
                    GetConnection(indexName)
                        .Alias(a => a.Remove(i => i.Index(oldIndexName).Alias(TragateConstants.AliasName)));
                }
            }

            RemoveOldIndices(TragateConstants.IndexPrefix, 2);
            GetConnection(indexName).UpdateIndexSettings(Indices.Index(indexName),
                i => i.IndexSettings(c => c.RefreshInterval("5s")));
        }


        private IEnumerable<string> GetIndexNames(string indexName){
            var client = new ElasticClient(new ConnectionSettings(new Uri(_settings.ElasticsearchUrl)));
            var indicesStats = client.IndicesStats(indexName);
            if (indicesStats.Indices == null)
                return new List<string>();

            var keys = indicesStats.Indices
                .Select(s => s.Key);

            return keys.ToList();
        }

        private ElasticClient GetConnection(string indexName){
            var connectionSettings = new ConnectionSettings(new Uri(_settings.ElasticsearchUrl))
                .DefaultMappingFor<Root>(m => m.IndexName(indexName).TypeName(TragateConstants.ROOT_TYPE))
                .DefaultMappingFor<ProductDto>(m => m.IndexName(indexName).TypeName(TragateConstants.ROOT_TYPE)
                    .IdProperty(p => p.UuId))
                .DefaultMappingFor<CompanyDto>(m =>
                    m.IndexName(indexName).TypeName(indexName).RelationName(TragateConstants.PARENT_TYPE));

            return new ElasticClient(connectionSettings);
        }

        private void RemoveOldIndices(string indexPrefix, byte skipCount){
            var olderIndices = GetIndexNames(TragateConstants.IndexPrefixWildCard)
                .Select(s => long.Parse(s.Split(char.Parse(TragateConstants.IndexNameSplitter)).LastOrDefault()))
                .DefaultIfEmpty()
                .OrderByDescending(o => o)
                .Skip(skipCount)
                .Select(slct => indexPrefix + TragateConstants.IndexNameSplitter + slct.ToString())
                .ToArray();
            if (olderIndices.Any()){
                var client = new ElasticClient(new ConnectionSettings(new Uri(_settings.ElasticsearchUrl)));
                client.DeleteIndex(Indices.Index(olderIndices));
            }
        }
    }
}