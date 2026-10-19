using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using RA_MuniAnnouncements.Clients;
using RA_MuniAnnouncements.Models;
using RA_MuniAnnouncements.Services;

namespace MichalKurtstag.Tests;

public sealed class MyRequestsServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 15, 0, 0, 0, TimeSpan.Zero);
    private static readonly IReadOnlyDictionary<string, ServiceCatalogEntry> Catalog =
        GetCatalog();

    [Fact]
    public void Normalize_TranslatesServiceAuthorityAndStatus()
    {
        var result = MyRequestsService.Normalize(Request("1"), Catalog, Now, NullLogger<MyRequestsService>.Instance);

        Assert.NotNull(result);
        Assert.Equal("הנחה בארנונה", result.ServiceName);
        Assert.Equal("עיריית חדרה", result.AuthorityName);
        Assert.Equal("התקבלה", result.Status);
    }

    [Fact]
    public void Normalize_MarksActionRequiredStatus()
    {
        var result = MyRequestsService.Normalize(Request("3"), Catalog, Now, NullLogger<MyRequestsService>.Instance);

        Assert.NotNull(result);
        Assert.True(result.ActionRequired);
    }

    [Fact]
    public async Task MockCatalog_MarksDocumentsRequiredArnonaRequestAsActionRequired()
    {
        var catalog = await new MockServiceCatalogRepository().GetAllAsync(CancellationToken.None);
        var result = MyRequestsService.Normalize(
            Request("3"), catalog, Now, NullLogger<MyRequestsService>.Instance);

        Assert.NotNull(result);
        Assert.True(result.ActionRequired);
    }

    [Fact]
    public void Normalize_UnknownStatusUsesInProgressFallback()
    {
        var result = MyRequestsService.Normalize(Request("unknown"), Catalog, Now, NullLogger<MyRequestsService>.Instance);

        Assert.NotNull(result);
        Assert.Equal("בטיפול", result.Status);
    }

    [Fact]
    public void Normalize_MarksOpenRequestOverdueAfterSla()
    {
        var result = MyRequestsService.Normalize(Request("2"), Catalog, Now, NullLogger<MyRequestsService>.Instance);

        Assert.NotNull(result);
        Assert.True(result.IsOverdue);
    }

    [Fact]
    public void Normalize_DoesNotCountPartialDayPastSlaAsOverdue()
    {
        var request = Request("2") with
        {
            SubmittedAt = new DateTimeOffset(2026, 10, 2, 0, 0, 0, TimeSpan.Zero)
        };
        var catalog = new Dictionary<string, ServiceCatalogEntry>(Catalog)
        {
            ["ARNONA_DISCOUNT"] = Catalog["ARNONA_DISCOUNT"] with { SlaDays = 13 }
        };

        var result = MyRequestsService.Normalize(
            request, catalog, Now.AddHours(23).AddMinutes(59), NullLogger<MyRequestsService>.Instance);

        Assert.NotNull(result);
        Assert.False(result.IsOverdue);
    }

    [Fact]
    public void Normalize_DoesNotMarkFinalRequestOverdue()
    {
        var result = MyRequestsService.Normalize(Request("4"), Catalog, Now, NullLogger<MyRequestsService>.Instance);

        Assert.NotNull(result);
        Assert.False(result.IsOverdue);
    }

    [Fact]
    public void Normalize_DropsRequestWithoutCatalogEntry()
    {
        var result = MyRequestsService.Normalize(
            Request("1") with { FormCode = "MISSING" }, Catalog, Now, NullLogger<MyRequestsService>.Instance);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetRequests_SortsActionRequiredFirstThenByMostRecentUpdate()
    {
        var requests = new[]
        {
            Request("1") with { RequestId = "normal-new", LastUpdate = Now.AddDays(-1) },
            Request("3") with { RequestId = "action-old", LastUpdate = Now.AddDays(-4) },
            Request("3") with { RequestId = "action-new", LastUpdate = Now.AddDays(-2) },
            Request("2") with { RequestId = "normal-old", LastUpdate = Now.AddDays(-5) }
        };
        var service = new MyRequestsService(
            new StubFormsClient(requests),
            new StubCatalogRepository(Catalog),
            TimeProvider.System,
            NullLogger<MyRequestsService>.Instance);

        var response = await service.GetRequestsAsync("012345678", CancellationToken.None);

        Assert.Equal(
            ["action-new", "action-old", "normal-new", "normal-old"],
            response.Requests.Select(request => request.RequestId));
    }

    private sealed class StubFormsClient(IReadOnlyList<FormRequest> requests) : IFormsClient
    {
        public Task<IReadOnlyList<FormRequest>> GetRequestsAsync(string idNumber, CancellationToken cancellationToken) =>
            Task.FromResult(requests);
    }

    private sealed class StubCatalogRepository(
        IReadOnlyDictionary<string, ServiceCatalogEntry> catalog) : IServiceCatalogRepository
    {
        public Task<IReadOnlyDictionary<string, ServiceCatalogEntry>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult(catalog);
    }

    private static FormRequest Request(string statusCode) => new(
        "F-1", "ARNONA_DISCOUNT", "6500", "012345678", statusCode,
        Now.AddDays(-31), Now.AddDays(-2));

    private static IReadOnlyDictionary<string, ServiceCatalogEntry> GetCatalog() =>
        new Dictionary<string, ServiceCatalogEntry>
        {
            ["ARNONA_DISCOUNT"] = new(
                "ARNONA_DISCOUNT", "הנחה בארנונה",
                new Dictionary<string, AuthorityServiceConfig>
                {
                    ["6500"] = new("עיריית חדרה", new Dictionary<string, string>
                    {
                        ["1"] = "התקבלה", ["2"] = "בטיפול", ["3"] = "נדרשים מסמכים",
                        ["4"] = "אושרה", ["5"] = "נדחתה"
                    })
                },
                new HashSet<string> { "3" },
                new HashSet<string> { "4", "5" },
                30)
        };
}