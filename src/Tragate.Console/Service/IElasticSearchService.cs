using System;
using System.Collections.Generic;
using Hangfire;
using Tragate.Console.Dto;

namespace Tragate.Console.Service
{
    public interface IElasticsearchService
    {
        void CreateIndex(string indexName, IJobCancellationToken cancellationToken);
        void DoConfigure(string indexName);
    }
}