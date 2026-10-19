using RA_MuniAnnouncements.Clients;
using RA_MuniAnnouncements.Models;

namespace RA_MuniAnnouncements.Services;

public sealed class MyRequestsService(
    IFormsClient formsClient,
    IServiceCatalogRepository catalogRepository,
    TimeProvider timeProvider,
    ILogger<MyRequestsService> logger)
{
    public async Task<MyRequestsResponse> GetRequestsAsync(string idNumber, CancellationToken cancellationToken)
    {
        var requests = await formsClient.GetRequestsAsync(idNumber, cancellationToken);
        var catalog = await catalogRepository.GetAllAsync(cancellationToken);
        var normalized = requests
            .Select(request => Normalize(request, catalog, timeProvider.GetUtcNow(), logger))
            .Where(request => request is not null)
            .Select(request => request!)
            .OrderByDescending(request => request.ActionRequired)
            .ThenByDescending(request => request.LastUpdate)
            .ToArray();

        return new MyRequestsResponse(normalized);
    }

    public static MyRequestResponse? Normalize(
        FormRequest request,
        IReadOnlyDictionary<string, ServiceCatalogEntry> catalog,
        DateTimeOffset now,
        ILogger logger)
    {
        if (!catalog.TryGetValue(request.FormCode, out var service))
        {
            logger.LogWarning("Skipping request {RequestId}: form code {FormCode} is missing from ServiceCatalog",
                request.RequestId, request.FormCode);
            return null;
        }

        if (!service.Authorities.TryGetValue(request.AuthorityId, out var authority))
        {
            logger.LogWarning("Skipping request {RequestId}: authority {AuthorityId} is missing from ServiceCatalog",
                request.RequestId, request.AuthorityId);
            return null;
        }

        var statusKnown = authority.StatusMap.TryGetValue(request.StatusCode, out var status);
        if (!statusKnown)
        {
            status = "בטיפול";
            logger.LogWarning("Unknown status code {StatusCode} for request {RequestId}; using {FallbackStatus}",
                request.StatusCode, request.RequestId, status);
        }

        var elapsedUtcDays = DateOnly.FromDateTime(now.UtcDateTime).DayNumber
            - DateOnly.FromDateTime(request.SubmittedAt.UtcDateTime).DayNumber;

        return new MyRequestResponse(
            request.RequestId,
            service.ServiceName,
            authority.AuthorityName,
            status!,
            service.ActionRequiredStatuses.Contains(request.StatusCode),
            !service.FinalStatuses.Contains(request.StatusCode)
                && elapsedUtcDays > service.SlaDays,
            request.SubmittedAt,
            request.LastUpdate);
    }
}