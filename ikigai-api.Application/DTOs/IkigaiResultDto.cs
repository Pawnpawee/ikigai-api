
using ikigai_api.Domain.Entities;

namespace ikigai_api.Application.DTOs
{
    public class IkigaiResultDto
    {
        public Guid Id { get; set; }
        public string? Status { get; set; }
        public List<IkigaiSummaryDto>? Summaries { get; set; }
    }

    public class IkigaiSummaryDto
    {
        public string? ComponentType { get; set; }
        public string? OverallSummary { get; set; }
        public string? ShortSummary { get; set; }
        public List<string>? Strengths { get; set; }
        public List<string>? DevelopmentPoints { get; set; }
    }

    public class IkigaiStartResult
    {
        public Guid ProcessId { get; set; }
        public ProcessStatus Status { get; set; }
        public bool IsExisting { get; set; } // ไว้เช็คว่าเป็นการเริ่มใหม่หรือของเก่า
    }
}

