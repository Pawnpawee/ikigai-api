namespace ikigai_api.Domain.Entities
{
    public class LoveSessionData
    {
        public Guid Id { get; set; } // LoveSessionDataId
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }

        //* กลุ่ม JSON Data
        public string SelectedHobbies { get; set; } = "[]";
        public string CustomHobbies { get; set; } = "[]";
        public string TopThreeHobbies { get; set; } = "[]";
        public string DreamAnswer { get; set; } = string.Empty; // yes, no, not_sure
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}