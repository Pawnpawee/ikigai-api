namespace ikigai_api.Domain.Entities
{
    public enum ProcessStatus
    {
        Pending,
        Processing,
        Completed,
        Failed
    }

    public class IkigaiResult
    {
        public Guid Id { get; set; } // ResultId
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }
        public ProcessStatus Status { get; set; } = ProcessStatus.Pending;
        public double LovePercentage { get; set; }
        public double GoodAtPercentage { get; set; }
        public double WorldNeedsPercentage { get; set; }
        public double PaidForPercentage { get; set; }
        public string? ErrorMessage { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<IkigaiSummary> IkigaiSummaries { get; set; } = new List<IkigaiSummary>();
    }

    public class IkigaiSummary
    {
        public Guid Id { get; set; } // IkigaiSummaryId

        public Guid ResultId { get; set; }
        public virtual IkigaiResult? Result { get; set; }

        public string ComponentType { get; set; } = string.Empty; // Passion, Mission, etc.
        public string OverallSummary { get; set; } = string.Empty;
        public string? ShortSummary { get; set; }

        public string StrengthsJson { get; set; } = "[]";
        public string DevelopmentPointsJson { get; set; } = "[]";
    }


}