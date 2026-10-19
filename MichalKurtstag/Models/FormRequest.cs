namespace RA_MuniAnnouncements.Models;

public sealed record FormRequest(
    string RequestId,
    string FormCode,
    string AuthorityId,
    string ApplicantId,
    string StatusCode,
    DateTimeOffset SubmittedAt,
    DateTimeOffset LastUpdate);