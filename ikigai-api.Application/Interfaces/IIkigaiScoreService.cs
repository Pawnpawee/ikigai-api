using ikigai_api.Application.DTOs;
using ikigai_api.Domain.Entities;

namespace ikigai_api.Application.Interfaces
{
    public interface IIkigaiScoreService
    {
        IkigaiScoreResultDto CalculateScores(
            LoveSessionData loveData,
            SkillSessionData skillData,
            WorldSessionData worldData,
            PaidSessionData paidData);
    }
}