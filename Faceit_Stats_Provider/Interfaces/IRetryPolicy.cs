namespace Faceit_Stats_Provider.Interfaces
{
    public interface IRetryPolicy
    {
        Task<T> RetryPolicyAsync<T>(Func<Task<T>> action, int maxRetries = 3, int delayMilliseconds = 2000);
    }
}
