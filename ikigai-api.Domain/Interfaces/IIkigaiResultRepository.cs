
using ikigai_api.Domain.Entities;

namespace ikigai_api.Domain.Interfaces;

public interface IIkigaiResultRepository : IGenericRepository<IkigaiResult>
{
    Task<IkigaiResult?> GetByIdWithSummariesAsync(Guid id);
}