namespace ikigai_api.Domain.Entities
{
    public class PrologueData
    {
        public Guid Id { get; set; } // PrologueDataId
        
        public Guid UserId { get; set; } 
        public virtual User? User { get; set; } 

        //? เก็บเป็น JSON String ตาม Schema (NVARCHAR(MAX))
        public string SelectedReasons { get; set; } = "[]"; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}