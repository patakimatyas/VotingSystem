namespace VotingSystem.Blazor.Infrastructure
{
    public interface IHttpRequestUtility
    {
        Task<T?> ExecuteGetHttpRequestAsync<T>(string url);
        Task<T?> ExecutePostHttpRequestAsync<TIn, T>(string url, TIn data);
        Task ExecutePostHttpRequestAsync(string url);
        Task ExecutePostHttpRequestAsync<TIn>(string url, TIn content);

    }
}
