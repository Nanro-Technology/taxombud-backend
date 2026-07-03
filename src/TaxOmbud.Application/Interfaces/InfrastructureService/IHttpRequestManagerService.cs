namespace TaxOmbud.Application.Interfaces.InfrastructureService;

public interface IHttpRequestManagerService
{
    Task<T> Get<T>(string requestUri, Dictionary<string, string>? httpHeaders, int timeout = 0)
        where T : class, new();
    string GetUTCTimestamp();
    string GetXTokenHeader(string clientID, string password);
    Task<T> Send<T>(
        HttpMethod method,
        string requestUri,
        object? requestData,
        Dictionary<string, string>? httpHeaders,
        int timeout = 0
    )
        where T : class, new();
}

public interface IHttpResponseMessage
{
    HttpResponseMessage ResponseMessage { get; set; }
}
