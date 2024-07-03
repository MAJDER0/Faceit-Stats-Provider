using Faceit_Stats_Provider.Interfaces;

namespace Faceit_Stats_Provider.Services
{

    public class RetryPolicyService : IRetryPolicy
    {
        public async Task<T> RetryPolicyAsync<T>(Func<Task<T>> action, int maxRetries = 3, int delayMilliseconds = 2000)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex)
                {
                    if (i == maxRetries - 1)
                    {
                        throw; 
                    }
                    await Task.Delay(delayMilliseconds);
                }
            }
            return default; 
        }
    }

}
