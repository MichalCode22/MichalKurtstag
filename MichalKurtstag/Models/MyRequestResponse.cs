namespace RA_MuniAnnouncements.Models;

public sealed record MyRequestResponse(
    string RequestId,
    string ServiceName,
    string AuthorityName,
    string Status,
    bool ActionRequired,
    bool IsOverdue,
    DateTimeOffset SubmittedAt,
    DateTimeOffset LastUpdate);

public sealed record MyRequestsResponse(IReadOnlyList<MyRequestResponse> Requests);