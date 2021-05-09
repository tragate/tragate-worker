using Hangfire;

namespace Tragate.Console.Infrastructure
{
    public interface IRecurringJob
    {
        void Work(IJobCancellationToken cancellationToken);
    }
}