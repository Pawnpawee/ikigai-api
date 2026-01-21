using ikigai_api.Domain.Entities;

namespace ikigai_api.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //? Navigation Properties (ความสัมพันธ์กับตารางอื่น)
        public virtual ICollection<PrologueData> PrologueDatas { get; set; } = new List<PrologueData>();
        public virtual ICollection<LoveSessionData> LoveSessionDatas { get; set; } = new List<LoveSessionData>();
        public virtual ICollection<SkillSessionData> SkillSessionDatas { get; set; } = new List<SkillSessionData>();
        public virtual ICollection<WorldSessionData> WorldSessionDatas { get; set; } = new List<WorldSessionData>();
        public virtual ICollection<PaidSessionData> PaidSessionDatas { get; set; } = new List<PaidSessionData>();

        public virtual ICollection<IkigaiResult> IkigaiResults { get; set; } = new List<IkigaiResult>();
    }
}