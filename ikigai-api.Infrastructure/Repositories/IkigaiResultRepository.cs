
using ikigai_api.Domain.Entities;
using ikigai_api.Domain.Interfaces;
using ikigai_api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ikigai_api.Infrastructure.Repositories;

public class IkigaiResultRepository : GenericRepository<IkigaiResult>, IIkigaiResultRepository
{
    private readonly ApplicationDbContext _context;

    public IkigaiResultRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IkigaiResult?> GetByIdWithSummariesAsync(Guid id)
    {
        return await _context.Set<IkigaiResult>()
            .AsNoTracking()
            .Include(x => x.IkigaiSummaries)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IkigaiResult?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Set<IkigaiResult>()
            .Include(x => x.IkigaiSummaries) 
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}