
using ikigai_api.Domain.Entities;
using ikigai_api.Domain.Interfaces;

public interface IIkigaiResultRepository : IGenericRepository<IkigaiResult>
{
    Task<IkigaiResult?> GetResultWithDetailsAsync(Guid userId);
}