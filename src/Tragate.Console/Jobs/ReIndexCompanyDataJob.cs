using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Hangfire;
using Microsoft.Extensions.Options;
using Nest;
using Serilog;
using Tragate.Console.Dto;
using Tragate.Console.Infrastructure;
using Tragate.Console.Repository;

namespace Tragate.Console.Jobs
{
    public class ReIndexCompanyDataJob : IRecurringJob
    {
        private readonly ICompanyDataRepository _companyDataRepository;
        private readonly ConfigSettings _settings;

        public ReIndexCompanyDataJob(ICompanyDataRepository companyDataRepository, IOptions<ConfigSettings> settings){
            _companyDataRepository = companyDataRepository;
            _settings = settings.Value;
        }

        public void Work(IJobCancellationToken cancellationToken){
            var timer = new Stopwatch();
            timer.Start();
            Log.Information("Tragate.Worker.ReIndexCompanyDataJob \n ReIndexJob has been started ...");
            try{
                const string indexName = "companydata";
                var connectionSettings = new ConnectionSettings(new Uri(_settings.ElasticsearchUrl));
                var _elasticClient = new ElasticClient(connectionSettings);
                _elasticClient.CreateIndex(indexName, c => c
                    .Settings(s => s
                        .NumberOfReplicas(0)
                        .NumberOfShards(1)
                        .Analysis(a => a
                            .Analyzers(ad => ad
                                .Custom("turkish_analyzer", ca => ca
                                    .Tokenizer("standard")
                                    .Filters("lowercase", "my_ascii_folding"))
                            ).TokenFilters(t => t.AsciiFolding("my_ascii_folding",
                                to => to.PreserveOriginal()))
                        )
                    )
                    .Mappings(m => m
                        .Map<CompanyDataDto>(mp => mp
                            .Properties(p => p.Text(t => t.Name(n => n.Membership)
                                .Analyzer("keyword")))
                            .Properties(p => p.Text(t => t.Name(n => n.Title)
                                .Analyzer("turkish_analyzer")))
                        )
                    )
                );

                var counter = 0;
                List<CompanyDataDto> companyDataList;
                do{
                    var waitHandle = new CountdownEvent(1);
                    companyDataList = _companyDataRepository.GetCompanyData(counter, 5000);
                    var bulkAll = _elasticClient.BulkAll(companyDataList,
                        b => b.Index(indexName)
                            .BackOffRetries(2)
                            .BackOffTime("30s")
                            .RefreshOnCompleted()
                            .Size(1000)
                    );

                    bulkAll.Subscribe(new BulkAllObserver(
                        onNext: (b) =>
                        {
                            Log.Information("Tragate.Worker.ReIndexCompanyDataJob all documents has been imported");
                        },
                        onError: (e) => throw e,
                        onCompleted: () => waitHandle.Signal()
                    ));
                    waitHandle.Wait();

                    counter++;
                } while (companyDataList.Any());
            }
            catch (Exception e){
                Log.Error("Tragate.Worker.ReIndexCompanyDataJob.Error \n " + e);
            }

            timer.Stop();
            Log.Information(
                $"Tragate.Worker.ReIndexCompanyDataJob \n ReIndexCompanyDataJob has been ended ... \n Duration Time {timer.Elapsed}");
        }
    }
}