using ikigai_api.Application.DTOs;

namespace ikigai_api.Application.Interfaces
{
    public interface IIkigaiService
    {
        Task<Guid> SavePrologueAsync(SavePrologueRequest request);
        Task SaveLoveSessionAsync(SaveLoveSessionRequest request);
        Task SaveSkillSessionAsync(SaveSkillSessionRequest request);
        Task SaveWorldSessionAsync(SaveWorldSessionRequest request);
        Task SavePaidSessionAsync(SavePaidSessionRequest request);

    }
}