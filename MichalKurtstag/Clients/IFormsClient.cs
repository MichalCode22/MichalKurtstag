using RA_MuniAnnouncements.Models;

namespace RA_MuniAnnouncements.Clients;

public interface IFormsClient
{
    Task<IReadOnlyList<FormRequest>> GetRequestsAsync(string idNumber, CancellationToken cancellationToken);
}