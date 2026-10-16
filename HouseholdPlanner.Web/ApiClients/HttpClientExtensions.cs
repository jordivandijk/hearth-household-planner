using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HouseholdPlanner.Web.ApiClients;

/// <summary>
/// The one place that turns HTTP into results or <see cref="ApiException"/>s, so every
/// API client handles failures the same way instead of returning null or an empty list.
/// </summary>
public static class HttpClientExtensions
{
    public static async Task<T> GetJsonAsync<T>(this HttpClient http, string url)
    {
        using var response = await SendAsync(http, new HttpRequestMessage(HttpMethod.Get, url));
        return await ReadJsonAsync<T>(response);
    }

    public static async Task<T> SendJsonAsync<T>(this HttpClient http, HttpMethod method, string url, object? body = null)
    {
        using var response = await SendAsync(http, new HttpRequestMessage(method, url) { Content = ToContent(body) });
        return await ReadJsonAsync<T>(response);
    }

    /// <summary>Like <see cref="SendJsonAsync{T}"/>, but 204 No Content returns null.</summary>
    public static async Task<T?> SendJsonOrDefaultAsync<T>(this HttpClient http, HttpMethod method, string url, object? body = null) where T : class
    {
        using var response = await SendAsync(http, new HttpRequestMessage(method, url) { Content = ToContent(body) });
        return response.StatusCode == HttpStatusCode.NoContent ? null : await ReadJsonAsync<T>(response);
    }

    public static async Task SendJsonAsync(this HttpClient http, HttpMethod method, string url, object? body = null)
    {
        using var response = await SendAsync(http, new HttpRequestMessage(method, url) { Content = ToContent(body) });
    }

    private static HttpContent? ToContent(object? body)
        => body is null ? null : JsonContent.Create(body, body.GetType());

    private static async Task<HttpResponseMessage> SendAsync(HttpClient http, HttpRequestMessage request)
    {
        HttpResponseMessage response;
        try
        {
            response = await http.SendAsync(request);
        }
        // TaskCanceledException is HttpClient's timeout.
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new ApiException(Strings.Text("Error_CantReach"), inner: ex);
        }

        if (response.IsSuccessStatusCode)
            return response;

        var message = await ReadErrorMessageAsync(response);
        response.Dispose();
        throw new ApiException(message, response.StatusCode);
    }

    private static async Task<T> ReadJsonAsync<T>(HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<T>()
                   ?? throw new ApiException(Strings.Text("Error_Empty"), response.StatusCode);
        }
        catch (JsonException ex)
        {
            // Not the API answering, e.g. a Wi-Fi login page.
            throw new ApiException(Strings.Text("Error_Unexpected"), response.StatusCode, ex);
        }
    }

    // The API returns either a plain-text message (from the result pattern) or
    // ValidationProblemDetails (from [ApiController] model validation).
    private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();

        if (response.Content.Headers.ContentType?.MediaType == "application/problem+json")
        {
            try
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("errors", out var errors))
                {
                    var messages = errors.EnumerateObject()
                        .SelectMany(e => e.Value.EnumerateArray().Select(v => v.GetString()))
                        .Where(m => !string.IsNullOrWhiteSpace(m))
                        .Distinct();
                    return string.Join(" ", messages);
                }
                if (doc.RootElement.TryGetProperty("title", out var title))
                    return title.GetString() ?? DefaultMessage(response.StatusCode);
            }
            catch (JsonException)
            {
                // Fall through to the generic message.
            }
        }

        body = body.Trim().Trim('"');
        return string.IsNullOrWhiteSpace(body) || body.Length > 300 ? DefaultMessage(response.StatusCode) : body;
    }

    private static string DefaultMessage(HttpStatusCode status) => status switch
    {
        HttpStatusCode.Unauthorized    => Strings.Text("Error_SessionExpired"),
        HttpStatusCode.Forbidden       => Strings.Text("Error_Forbidden"),
        HttpStatusCode.NotFound        => Strings.Text("Error_NotFound"),
        // The API sends its own text with a 429; this is for a proxy in front of it that limits too.
        HttpStatusCode.TooManyRequests => Strings.Text("Error_TooManyRequests"),
        _                              => Strings.Text("Error_Server")
    };
}
