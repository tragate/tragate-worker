using System;
using System.Diagnostics;
using Hangfire;
using Serilog;
using Tragate.Console.Helper;
using Tragate.Console.Infrastructure;
using Tragate.Console.Service;

namespace Tragate.Console.Jobs
{
    public class ReIndexJob : IRecurringJob
    {
        private readonly IElasticsearchService _elasticsearchService;

        public ReIndexJob(IElasticsearchService elasticsearchService){
            _elasticsearchService = elasticsearchService;
        }

        public void Work(IJobCancellationToken cancellationToken){
            var timer = new Stopwatch();
            timer.Start();
            Log.Information("Tragate.Worker.ReIndexJob \n ReIndexJob has been started ...");
            try{
                var indexName = TragateConstants.IndexPrefix + TragateConstants.IndexNameSplitter +
                                DateTime.Now.ToString(TragateConstants.IndexNameTimeFormat);
                _elasticsearchService.CreateIndex(indexName, cancellationToken);
                _elasticsearchService.DoConfigure(indexName);
            }
            catch (Exception e){
                Log.Error("Tragate.Worker.ReIndexJob.Error \n " + e);
            }

            timer.Stop();
            Log.Information(
                $"Tragate.Worker.ReIndexJob \n ReIndexJob has been ended ... \n Duration Time {timer.Elapsed}");
        }
    }
}