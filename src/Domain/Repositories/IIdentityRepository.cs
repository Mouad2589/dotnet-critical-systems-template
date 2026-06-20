using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Repositories;

public interface IIdentityRepository
{
    Task<Identity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Identity?> GetByNationalIdAsync(NationalId nationalId, CancellationToken ct = default);
    Task<bool> ExistsAsync(NationalId nationalId, CancellationToken ct = default);
    Task AddAsync(Identity identity, CancellationToken ct = default);
    Task UpdateAsync(Identity identity, CancellationToken ct = default);
}
