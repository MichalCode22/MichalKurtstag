using RA_MuniAnnouncements.Models;

namespace RA_MuniAnnouncements.Clients;

public sealed class MockServiceCatalogRepository : IServiceCatalogRepository
{
    private static readonly IReadOnlyDictionary<string, ServiceCatalogEntry> Catalog =
        new Dictionary<string, ServiceCatalogEntry>
        {
            ["ARNONA_DISCOUNT"] = Entry("ARNONA_DISCOUNT", "הנחה בארנונה", 30,
                ["3"], ["4", "5"],
                ("6500", "עיריית חדרה", new Dictionary<string, string>
                {
                    ["1"] = "התקבלה", ["2"] = "בטיפול", ["3"] = "נדרשים מסמכים",
                    ["4"] = "נדחתה", ["5"] = "אושרה"
                })),
            ["PARKING_PERMIT"] = Entry("PARKING_PERMIT", "תו חניה", 14,
                ["3"], ["4", "5"],
                ("6500", "עיריית חדרה", new Dictionary<string, string>
                {
                    ["1"] = "התקבלה", ["2"] = "בטיפול", ["3"] = "נדרשים מסמכים",
                    ["4"] = "נדחתה", ["5"] = "אושרה"
                })),
            ["HAZARD_REPORT"] = Entry("HAZARD_REPORT", "דיווח על מפגע", 7,
                [], ["4", "5"],
                ("6500", "עיריית חדרה", new Dictionary<string, string>
                {
                    ["1"] = "התקבלה", ["2"] = "בטיפול", ["3"] = "נדרשים מסמכים",
                    ["4"] = "טופל", ["5"] = "נסגר"
                }))
        };

    public Task<IReadOnlyDictionary<string, ServiceCatalogEntry>> GetAllAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Catalog);
    }

    private static ServiceCatalogEntry Entry(
        string formCode,
        string serviceName,
        int slaDays,
        string[] actionRequired,
        string[] finalStatuses,
        (string Id, string Name, IReadOnlyDictionary<string, string> Statuses) authority) =>
        new(formCode, serviceName,
            new Dictionary<string, AuthorityServiceConfig>
            {
                [authority.Id] = new(authority.Name, authority.Statuses)
            }, actionRequired.ToHashSet(), finalStatuses.ToHashSet(), slaDays);
}