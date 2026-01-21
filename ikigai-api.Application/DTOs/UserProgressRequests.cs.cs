namespace ikigai_api.Application.DTOs
{
    // 1. Prologue Request
    public class SavePrologueRequest
    {
        public string PlayerName { get; set; } = string.Empty;
        public List<int> SelectedReasons { get; set; } = new();
    }

    // 2. Love Session Request
    public class SaveLoveSessionRequest
    {
        public Guid UserId { get; set; }
        public List<string> SelectedHobbies { get; set; } = new();
        public List<string> CustomHobbies { get; set; } = new();
        public List<string> TopThreeHobbies { get; set; } = new(); // Required 3 items
        public string DreamAnswer { get; set; } = string.Empty; // yes, no, not_sure
    }

    public class SaveSkillSessionRequest
    {
        public Guid UserId { get; set; }
        public List<string> SelectedHardSkills { get; set; } = new();
        public List<string> CustomHardSkills { get; set; } = new();
        public List<string> SelectedSoftSkills { get; set; } = new();
        public List<string> CustomSoftSkills { get; set; } = new();
        public string SkillsMatchJob { get; set; } = string.Empty; // match, not_match
        public string UseSkillsInNewRole { get; set; } = string.Empty; // yes, no, not_sure
    }

    public class SaveWorldSessionRequest
    {
        public Guid UserId { get; set; }
        public string CalledUponAnswer { get; set; } = string.Empty; // yes, no
        public List<string> SelectedGifts { get; set; } = new();
        public string NoManualChoice { get; set; } = string.Empty; // do_myself, ask_first
        public string MismatchChoice { get; set; } = string.Empty; // adapt_self, adapt_role, both
        public string FutureValueAnswer { get; set; } = string.Empty; // yes, no
    }

    public class SavePaidSessionRequest
    {
        public Guid UserId { get; set; }
        public string EverPaidAnswer { get; set; } = string.Empty; // yes, no
        public List<string> SelectedJobCards { get; set; } = new();
        public List<string> MonetizableExperience { get; set; } = new();
    }
}