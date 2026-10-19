using RA_MuniAnnouncements.Models;

namespace RA_MuniAnnouncements.Clients;

public interface IServiceCatalogRepository
{
    Task<IReadOnlyDictionary<string, ServiceCatalogEntry>> GetAllAsync(CancellationToken cancellationToken);
}