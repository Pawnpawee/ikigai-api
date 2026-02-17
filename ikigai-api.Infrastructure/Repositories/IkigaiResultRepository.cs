
using ikigai_api.Domain.Entities;
using ikigai_api.Infrastructure.Persistence;
using ikigai_api.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

public class IkigaiResultRepository : GenericRepository<IkigaiResult>, IIkigaiResultRepository
{
    private readonly ApplicationDbContext _context;

    public IkigaiResultRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IkigaiResult?> GetResultWithDetailsAsync(Guid processId) 
    {
        return await _context.Set<IkigaiResult>()
            .Include(x => x.IkigaiSummaries)
            .FirstOrDefaultAsync(x => x.Id == processId);
    }

    public async Task<IkigaiResult?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Set<IkigaiResult>()
            .Include(x => x.IkigaiSummaries) 
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}