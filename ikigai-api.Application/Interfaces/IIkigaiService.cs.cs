using ikigai_api.Application.DTOs;

namespace ikigai_api.Application.Interfaces
{
    public interface IIkigaiService
    {
        Task<Guid> SavePrologueAsync(SavePrologueRequest request);
        Task<Guid> SaveLoveSessionAsync(SaveLoveSessionRequest request);
        Task<Guid> SaveSkillSessionAsync(SaveSkillSessionRequest request);
        Task<Guid> SaveWorldSessionAsync(SaveWorldSessionRequest request);
        Task<Guid> SavePaidSessionAsync(SavePaidSessionRequest request);

    }
}