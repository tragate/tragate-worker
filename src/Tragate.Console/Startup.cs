using System;
using System.Data;
using System.Data.SqlClient;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Tragate.Console.Infrastructure;
using Tragate.Console.Jobs;
using Tragate.Console.Repository;
using Tragate.Console.Service;
using Job = Hangfire.Common.Job;

namespace Tragate.Console
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env){
            Configuration = configuration;
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.Elasticsearch(
                    new ElasticsearchSinkOptions(
                        new Uri(Configuration["ElasticsearchUrl"]))
                    {
                        MinimumLogEventLevel = LogEventLevel.Verbose,
                        AutoRegisterTemplate = true
                    })
                .CreateLogger();
        }

        public void ConfigureServices(IServiceCollection services){
            services.Configure<ConfigSettings>(Configuration);
            services.AddScoped<IElasticsearchService, ElasticsearchService>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<ICompanyDataRepository, CompanyDataRepository>();
            services.AddScoped<IDbConnection>(x =>
                new SqlConnection(Configuration.GetConnectionString("DefaultConnection")));

            services.AddMvc();

            var storage = new SqlServerStorage(
                Configuration["HangfireConnectionStrings"],
                new SqlServerStorageOptions()
                {
                    PrepareSchemaIfNecessary = true
                });
            services.AddHangfire(x => x.UseStorage(storage));
            GlobalConfiguration.Configuration.UseStorage(storage);

            var manager = new RecurringJobManager();
            manager.AddOrUpdate("reindex",
                Job.FromExpression<ReIndexJob>(a => a.Work(JobCancellationToken.Null)),
                Cron.Daily(00, 00), TimeZoneInfo.Local);

            manager.AddOrUpdate("reindexcompanydata",
                Job.FromExpression<ReIndexCompanyDataJob>(a => a.Work(JobCancellationToken.Null)),
                Cron.Daily(00, 00), TimeZoneInfo.Local);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env){
            app.UseHangfireDashboard("");
            app.UseHangfireServer();
            app.UseMvc();
        }
    }
}