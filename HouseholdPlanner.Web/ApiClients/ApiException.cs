using System.Net;

namespace HouseholdPlanner.Web.ApiClients;

/// <summary>
/// A failed API call. <see cref="Exception.Message"/> is safe to show to the user:
/// it is the API's own error text, validation messages, or a generic connectivity message.
/// </summary>
public class ApiException(string message, HttpStatusCode? statusCode = null, Exception? inner = null)
    : Exception(message, inner)
{
    public HttpStatusCode? StatusCode { get; } = statusCode;
}
