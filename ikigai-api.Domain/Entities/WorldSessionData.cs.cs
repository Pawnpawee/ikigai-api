namespace ikigai_api.Domain.Entities
{
    public class WorldSessionData
    {
        public Guid Id { get; set; } // WorldSessionDataId
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }

        //* กลุ่ม JSON Data
        public string CalledUponAnswer { get; set; } = string.Empty; // yes, no
        public string SelectedGifts { get; set; } = "[]";
        public string NoManualChoice { get; set; } = string.Empty; // do_myself, ask_first
        public string MismatchChoice { get; set; } = string.Empty; // adapt_self, adapt_role, both
        public string FutureValueAnswer { get; set; } = string.Empty; // yes, no

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}