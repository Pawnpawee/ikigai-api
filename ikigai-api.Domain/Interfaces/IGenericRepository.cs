using System.Linq.Expressions;

namespace ikigai_api.Domain.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        // ดึงข้อมูลทั้งหมด
        Task<IReadOnlyList<T>> GetAllAsync();
        // ดึงข้อมูลตามเงื่อนไข
        Task<T?> GetByIdAsync(Guid id);
        // เพิ่มข้อมูล
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        // แก้ไข
        void Update(T entity);
        // ลบ
        void Delete(T entity);
        // บันทึกลง Database (Save Changes)
        Task<int> SaveChangesAsync();
        // หาตามเงื่อนไข
        Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate);
        
    }
}