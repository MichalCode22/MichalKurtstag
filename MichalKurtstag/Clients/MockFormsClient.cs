using RA_MuniAnnouncements.Models;

namespace RA_MuniAnnouncements.Clients;

public sealed class MockFormsClient : IFormsClient
{
    private static readonly IReadOnlyList<FormRequest> Requests =
    [
        new("F-88213", "ARNONA_DISCOUNT", "6500", "012345678", "3",
            new DateTimeOffset(2026, 9, 2, 10, 15, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 9, 20, 8, 0, 0, TimeSpan.Zero)),
        new("P-10420", "PARKING_PERMIT", "6500", "012345678", "5",
            new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 4, 12, 30, 0, TimeSpan.Zero)),
        new("R-55102", "HAZARD_REPORT", "6500", "012345678", "2",
            new DateTimeOffset(2026, 10, 17, 7, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 2, 10, 0, 0, TimeSpan.Zero)),
        new("X-00001", "UNKNOWN_FORM", "6500", "012345678", "1",
            new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero))
    ];

    public Task<IReadOnlyList<FormRequest>> GetRequestsAsync(
        string idNumber,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<FormRequest>>(
            Requests.Where(request => request.ApplicantId == idNumber).ToArray());
    }
}