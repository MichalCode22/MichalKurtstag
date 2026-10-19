namespace RA_MuniAnnouncements.Models;

public sealed record AuthorityServiceConfig(
    string AuthorityName,
    IReadOnlyDictionary<string, string> StatusMap);

public sealed record ServiceCatalogEntry(
    string FormCode,
    string ServiceName,
    IReadOnlyDictionary<string, AuthorityServiceConfig> Authorities,
    IReadOnlySet<string> ActionRequiredStatuses,
    IReadOnlySet<string> FinalStatuses,
    int SlaDays);