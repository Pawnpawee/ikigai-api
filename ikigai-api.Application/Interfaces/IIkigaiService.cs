
using ikigai_api.Application.DTOs;
using ikigai_api.Domain.Entities;

namespace ikigai_api.Application.Interfaces
{
    public interface IIkigaiService
    {
        Task<Guid> SavePrologueAsync(SavePrologueRequest request);
        Task SaveLoveSessionAsync(SaveLoveSessionRequest request);
        Task SaveSkillSessionAsync(SaveSkillSessionRequest request);
        Task SaveWorldSessionAsync(SaveWorldSessionRequest request);
        Task SavePaidSessionAsync(SavePaidSessionRequest request);
        Task<Guid> GenerateIkigaiAsync(Guid userId);
        Task<IkigaiResultDto?> SaveFinalResultAsync(Guid processId, object resultData);
        Task<IkigaiResultDto?> GetIkigaiResultAsync(Guid processId);
        Task<ProcessStatus?> GetStatusOnlyAsync(Guid processId);



    }
}