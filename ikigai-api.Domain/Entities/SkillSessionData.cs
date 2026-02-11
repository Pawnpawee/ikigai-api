namespace ikigai_api.Domain.Entities
{
    public class SkillSessionData
    {
        public Guid Id { get; set; } // SkillSessionDataId
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }

        //* กลุ่ม JSON Data
        public string SelectedHardSkills { get; set; } = "[]";
        public string CustomHardSkills { get; set; } = "[]";
        public string SelectedSoftSkills { get; set; } = "[]";
        public string CustomSoftSkills { get; set; } = "[]";
        public string SkillsMatchJob { get; set; } = string.Empty; // match, not_match
        public string UseSkillsInNewRole { get; set; } = string.Empty; // yes, no, not_sure

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}