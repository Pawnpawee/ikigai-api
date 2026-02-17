using Microsoft.EntityFrameworkCore;
using ikigai_api.Domain.Entities;

namespace ikigai_api.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        //* ประกาศ Table ทั้งหมด
        public DbSet<User> Users { get; set; }
        public DbSet<PrologueData> PrologueDatas { get; set; }
        public DbSet<LoveSessionData> LoveSessionDatas { get; set; }
        public DbSet<SkillSessionData> SkillSessionDatas { get; set; }
        public DbSet<WorldSessionData> WorldSessionDatas { get; set; }
        public DbSet<PaidSessionData> PaidSessionDatas { get; set; }
        public DbSet<IkigaiResult> IkigaiResults { get; set; }
        public DbSet<IkigaiSummary> IkigaiSummaries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //? Config ความสัมพันธ์ (Fluent API) เพื่อความชัวร์

            // User 1 : N PrologueData
            modelBuilder.Entity<User>()
                .HasMany(u => u.PrologueDatas)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade); // ลบ User แล้วข้อมูลหายด้วย

            modelBuilder.Entity<User>()
            .HasMany(u => u.LoveSessionDatas)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
            .HasMany(u => u.SkillSessionDatas)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
            .HasMany(u => u.WorldSessionDatas)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
            .HasMany(u => u.PaidSessionDatas)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            // IkigaiResult 1 : N IkigaiSummary
            modelBuilder.Entity<IkigaiResult>()
                .HasMany(r => r.IkigaiSummaries)
                .WithOne(s => s.Result)
                .HasForeignKey(s => s.ResultId);

            modelBuilder.Entity<IkigaiResult>()
            .Property(e => e.GeneratedAt)
            .HasColumnType("timestamp without time zone"); // หรือ timestamptz

        }
    }
}