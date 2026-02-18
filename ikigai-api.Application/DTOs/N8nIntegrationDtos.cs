
using System.Text.Json.Serialization;

namespace ikigai_api.Application.DTOs
{
    public class N8nProcessRequest
    {
        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("playerName")]
        public string PlayerName { get; set; } = string.Empty;

        [JsonPropertyName("prologue")]
        public PrologueDto Prologue { get; set; } = new();

        [JsonPropertyName("loveSession")]
        public LoveSessionDto LoveSession { get; set; } = new();

        [JsonPropertyName("skillSession")]
        public SkillSessionDto SkillSession { get; set; } = new();

        [JsonPropertyName("worldSession")]
        public WorldSessionDto WorldSession { get; set; } = new();

        [JsonPropertyName("paidSession")]
        public PaidSessionDto PaidSession { get; set; } = new();
    }

    public class PrologueDto
    {
        [JsonPropertyName("selectedReasons")]
        public List<string> SelectedReasons { get; set; } = new();
    }

    public class LoveSessionDto
    {
        [JsonPropertyName("selectedHobbies")]
        public List<string> SelectedHobbies { get; set; } = new();

        [JsonPropertyName("customHobbies")]
        public List<string> CustomHobbies { get; set; } = new();

        [JsonPropertyName("topThreeHobbies")]
        public List<string> TopThreeHobbies { get; set; } = new();

        [JsonPropertyName("dreamAnswer")]
        public string DreamAnswer { get; set; } = string.Empty;
    }

    public class SkillSessionDto
    {
        [JsonPropertyName("selectedhardSkills")]
        public List<string> SelectedHardSkills { get; set; } = new();

        [JsonPropertyName("customhardSkills")]
        public List<string> CustomHardSkills { get; set; } = new();

        [JsonPropertyName("selectedsoftSkills")]
        public List<string> SelectedSoftSkills { get; set; } = new();

        [JsonPropertyName("customsoftSkills")]
        public List<string> CustomSoftSkills { get; set; } = new();

        [JsonPropertyName("skillsMatchJob")]
        public string SkillsMatchJob { get; set; } = string.Empty;

        [JsonPropertyName("useSkillsInNewRole")]
        public string UseSkillsInNewRole { get; set; } = string.Empty;
    }

    public class WorldSessionDto
    {
        [JsonPropertyName("calledUponAnswer")]
        public string CalledUponAnswer { get; set; } = string.Empty;

        [JsonPropertyName("selectedGifts")]
        public List<string> SelectedGifts { get; set; } = new();

        [JsonPropertyName("noManualChoice")]
        public string NoManualChoice { get; set; } = string.Empty;

        [JsonPropertyName("mismatchChoice")]
        public string MismatchChoice { get; set; } = string.Empty;

        [JsonPropertyName("futureValueAnswer")]
        public string FutureValueAnswer { get; set; } = string.Empty;
    }

    public class PaidSessionDto
    {
        [JsonPropertyName("everPaidAnswer")]
        public string EverPaidAnswer { get; set; } = string.Empty;

        [JsonPropertyName("selectedJobCards")]
        public List<string> SelectedJobCards { get; set; } = new();

        [JsonPropertyName("monetizableExperience")]
        public string MonetizableExperience { get; set; } = string.Empty;
    }

    public class N8nProcessResponse
    {
        [JsonPropertyName("ikigai_analysis")]
        public required IkigaiAnalysisDto IkigaiAnalysis { get; set; }
    }

    public class IkigaiAnalysisDto
    {
        // --- 1. The 4 Circles ---
        [JsonPropertyName("what_you_love")]
        public ComponentResultDto? WhatYouLove { get; set; }

        [JsonPropertyName("what_you_good_at")]
        public ComponentResultDto? WhatYouGoodAt { get; set; }

        [JsonPropertyName("what_the_world_need")]
        public ComponentResultDto? WhatTheWorldNeed { get; set; }

        [JsonPropertyName("what_you_can_be_paid_for")]
        public ComponentResultDto? WhatYouCanBePaidFor { get; set; }

        // --- 2. The 4 Intersections ---
        [JsonPropertyName("passion")]
        public ComponentResultDto? Passion { get; set; }

        [JsonPropertyName("mission")]
        public ComponentResultDto? Mission { get; set; }

        [JsonPropertyName("profession")]
        public ComponentResultDto? Profession { get; set; }

        [JsonPropertyName("vocation")]
        public ComponentResultDto? Vocation { get; set; }
    }

    public class ComponentResultDto
    {
        [JsonPropertyName("overall_summary")]
        public string OverallSummary { get; set; } = string.Empty;

        [JsonPropertyName("short_summary")]
        public string? ShortSummary { get; set; }

        [JsonPropertyName("strengths")]
        public List<string> Strengths { get; set; } = new();

        [JsonPropertyName("development_points")]
        public List<string> DevelopmentPoints { get; set; } = new();
    }
}
