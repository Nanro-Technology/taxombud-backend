using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using TaxOmbud.Application.Interfaces.InfrastructureService;

namespace TaxOmbud.Infrastructure.HttpService;

public class HttpRequestManagerService(ILogger<HttpRequestManagerService> logger)
    : IHttpRequestManagerService
{
    private readonly HttpClient _httpClient = new();

    public async Task<T> Get<T>(
        string? requestUri,
        Dictionary<string, string>? httpHeaders,
        int timeout = 0
    )
        where T : class, new()
    {
        timeout = CheckTimeout(timeout);

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);

        httpRequestMessage.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );

        //Set headers
        if (httpHeaders != null && httpHeaders.Any())
        {
            foreach (var header in httpHeaders)
            {
                if (_httpClient.DefaultRequestHeaders.Contains(header.Key))
                    _httpClient.DefaultRequestHeaders.Remove(header.Key);
                httpRequestMessage.Headers.Add(header.Key, header.Value);
            }
        }

        var cancellationToken = new CancellationTokenSource();
        cancellationToken.CancelAfter(timeout);

        try
        {
            var response = await _httpClient.SendAsync(
                httpRequestMessage,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken.Token
            );
            var content = await response.Content.ReadAsStringAsync();

            logger.LogDebug("Response from {RequestUri} was {Content}", requestUri, content);

            // Only attempt to deserialize if response is successful and content is not empty
            if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
                return SetHttpResponseMessageOnResponseObject(default(T), response)!;

            try
            {
                var responseObject = JsonConvert.DeserializeObject<T>(content);
                responseObject = SetHttpResponseMessageOnResponseObject(responseObject, response);
                return responseObject!;
            }
            catch (JsonReaderException jsonEx)
            {
                logger.LogError(
                    jsonEx,
                    "Failed to deserialize response from {RequestUri}. Content: {Content}",
                    requestUri,
                    content
                );
                return SetHttpResponseMessageOnResponseObject(default(T), response)!;
            }
        }
        catch (Exception ex)
        {
            LogHttpException(ex, HttpMethod.Get, requestUri);
        }
        finally
        {
            cancellationToken.Dispose();
        }
        return default(T)!;
    }

    private T SetHttpResponseMessageOnResponseObject<T>(
        T responseObject,
        HttpResponseMessage responseMessage
    )
    {
        if (responseObject is null)
        {
            responseObject = Activator.CreateInstance<T>();
        }
        try
        {
            var type = typeof(T);
            if (!type.GetInterfaces().Contains(typeof(IHttpResponseMessage)))
            {
                return responseObject;
            }

            var prop = responseObject!
                .GetType()
                .GetProperty(
                    nameof(IHttpResponseMessage.ResponseMessage),
                    BindingFlags.Public | BindingFlags.Instance
                );
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(responseObject, responseMessage);
            }
        }
        catch (Exception ex)
        {
            LogHttpException(
                ex,
                HttpMethod.Get,
                responseMessage.RequestMessage?.RequestUri?.ToString()
            );
        }

        return responseObject;
    }

    public async Task<T> Send<T>(
        HttpMethod method,
        string requestUri,
        object? requestData,
        Dictionary<string, string>? httpHeaders,
        int timeout = 0
    )
        where T : class, new()
    {
        timeout = CheckTimeout(timeout);

        var httpRequestMessage = new HttpRequestMessage(method, requestUri);

        httpRequestMessage.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
        var serializationSetting = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
        };

        if (requestData != null)
        {
            var jsonData = JsonConvert.SerializeObject(requestData, serializationSetting);
            httpRequestMessage.Content = new StringContent(
                jsonData,
                Encoding.UTF8,
                "application/json"
            );
        }

        //Set headers
        if (httpHeaders != null && httpHeaders.Any())
        {
            foreach (var header in httpHeaders)
            {
                if (_httpClient.DefaultRequestHeaders.Contains(header.Key))
                    _httpClient.DefaultRequestHeaders.Remove(header.Key);
                httpRequestMessage.Headers.Add(header.Key, header.Value);
            }
        }

        var cancellationToken = new CancellationTokenSource();
        cancellationToken.CancelAfter(timeout);

        try
        {
            var response = await _httpClient.SendAsync(
                httpRequestMessage,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken.Token
            );
            var content = await response.Content.ReadAsStringAsync();

            logger.LogDebug("Response from {RequestUri} was {Content}", requestUri, content);

            if (string.IsNullOrWhiteSpace(content))
                return SetHttpResponseMessageOnResponseObject(default(T), response)!;

            var responseObject = JsonConvert.DeserializeObject<T>(content);

            responseObject = SetHttpResponseMessageOnResponseObject(responseObject, response);
            return responseObject!;
        }
        catch (Exception ex)
        {
            LogHttpException(ex, method, requestUri);
        }
        finally
        {
            cancellationToken.Dispose();
        }
        return default(T)!;
    }

    private static int CheckTimeout(int timeout)
    {
        if (timeout <= 0)
        {
            timeout = 60000;
        }

        return timeout;
    }

    public string GetXTokenHeader(string clientID, string password)
    {
        var utcdate = DateTime.UtcNow;
        var date = utcdate.ToString("yyyy-MM-ddHHmmss");
        var data = date + clientID + password;
        return SHA512(data);
    }

    public string GetUTCTimestamp()
    {
        var utcdate = DateTime.UtcNow;
        var date = utcdate.ToString("yyyy-MM-ddHHmmss");
        return date;
    }

    private static string SHA512(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        using (var hash = System.Security.Cryptography.SHA512.Create())
        {
            var hashedInputBytes = hash.ComputeHash(bytes);

            // Convert to text
            // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte
            var hashedInputStringBuilder = new StringBuilder(128);
            foreach (var b in hashedInputBytes)
                hashedInputStringBuilder.Append(b.ToString("x2"));
            return hashedInputStringBuilder.ToString();
        }
    }

    private void LogHttpException(Exception ex, HttpMethod method, string? endpoint)
    {
        logger.LogError(
            ex,
            "HTTP request failed. Method: {Method}, Endpoint: {Endpoint}",
            method.ToString(),
            endpoint
        );

        if (ex is HttpRequestException httpEx && httpEx.StatusCode.HasValue)
        {
            logger.LogError("HTTP status code: {StatusCode}", httpEx.StatusCode.Value);
        }

        if (ex.InnerException != null)
        {
            logger.LogError("Inner exception: {InnerExceptionMessage}", ex.InnerException.Message);
        }
    }
}
