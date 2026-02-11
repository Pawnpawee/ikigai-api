namespace ikigai_api.Domain.Entities
{
    public class PaidSessionData
    {
        public Guid Id { get; set; } // PaidSessionDataId
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }

        //* กลุ่ม JSON Data
        public string EverPaidAnswer { get; set; } = string.Empty; // yes, no
        public string SelectedJobCards { get; set; } = "[]";
        public string MonetizableExperience { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}