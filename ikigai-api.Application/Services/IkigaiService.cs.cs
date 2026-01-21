using System.Text.Json;
using ikigai_api.Application.DTOs;
using ikigai_api.Application.Interfaces;
using ikigai_api.Domain.Entities;
using ikigai_api.Domain.Interfaces;


public class IkigaiService : IIkigaiService
{
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<PrologueData> _prologueRepo;
    private readonly IGenericRepository<LoveSessionData> _loveRepo;

    public IkigaiService(
        IGenericRepository<User> userRepo,
        IGenericRepository<PrologueData> prologueRepo,
        IGenericRepository<LoveSessionData> loveRepo)
    {
        _userRepo = userRepo;
        _prologueRepo = prologueRepo;
        _loveRepo = loveRepo;
    }

    public async Task<Guid> SavePrologueAsync(SavePrologueRequest request)
    {
        // 1. สร้าง User ใหม่
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            PlayerName = request.PlayerName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 2. สร้าง Prologue Data
        var prologue = new PrologueData
        {
            Id = Guid.NewGuid(),
            UserId = newUser.Id,
            // แปลง List<int> เป็น JSON String "[1,4,5]"
            SelectedReasons = JsonSerializer.Serialize(request.SelectedReasons),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 3. บันทึกลง DB
        await _userRepo.AddAsync(newUser);
        await _prologueRepo.AddAsync(prologue);

        await _userRepo.SaveChangesAsync();

        return newUser.Id; // ส่ง UserId กลับไปให้ Frontend เก็บไว้ใช้ต่อ
    }

    public async Task SaveLoveSessionAsync(SaveLoveSessionRequest request)
    {
        // Validation: ตรวจสอบว่ามี 3 อันจริงไหม
        if (request.TopThreeHobbies.Count != 3)
        {
            throw new ArgumentException("Top three hobbies must contain exactly 3 items.");
        }

        var loveData = new LoveSessionData
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            SelectedHobbies = JsonSerializer.Serialize(request.SelectedHobbies),
            CustomHobbies = JsonSerializer.Serialize(request.CustomHobbies),
            TopThreeHobbies = JsonSerializer.Serialize(request.TopThreeHobbies),
            DreamAnswer = request.DreamAnswer,
            //? หมายเหตุ: ถ้าใน Entity ไม่มีที่เก็บ Score อาจต้องเพิ่ม Column "ScoresJson" หรือปล่อยผ่าน
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _loveRepo.AddAsync(loveData);
        await _loveRepo.SaveChangesAsync();
    }

    Task<Guid> IIkigaiService.SaveLoveSessionAsync(SaveLoveSessionRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> SaveSkillSessionAsync(SaveSkillSessionRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> SaveWorldSessionAsync(SaveWorldSessionRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> SavePaidSessionAsync(SavePaidSessionRequest request)
    {
        throw new NotImplementedException();
    }
}