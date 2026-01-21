using System.Text.Json;
using ikigai_api.Application.DTOs;
using ikigai_api.Application.Interfaces;
using ikigai_api.Common.Extensions;
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
        if (request.PlayerName.Length > 20)
        {
            throw new ArgumentException("Player name must be less than 20 characters.");
        }

        // 1. สร้าง User ใหม่
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            PlayerName = request.PlayerName,
            CreatedAt = DateTime.UtcNow.ToThaiTime(),
            UpdatedAt = DateTime.UtcNow.ToThaiTime()
        };

        // 2. สร้าง Prologue Data
        var prologue = new PrologueData
        {
            Id = Guid.NewGuid(),
            UserId = newUser.Id,
            // แปลงเป็น JSON String
            SelectedReasons = request.SelectedReasons.ToJsonThai(),
            CreatedAt = DateTime.UtcNow.ToThaiTime(),
            UpdatedAt = DateTime.UtcNow.ToThaiTime()
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
            SelectedHobbies = request.SelectedHobbies.ToJsonThai(),
            CustomHobbies = request.CustomHobbies.ToJsonThai(),
            TopThreeHobbies = request.TopThreeHobbies.ToJsonThai(),
            DreamAnswer = request.DreamAnswer,
            CreatedAt = DateTime.UtcNow.ToThaiTime(),
            UpdatedAt = DateTime.UtcNow.ToThaiTime()
        };

        await _loveRepo.AddAsync(loveData);
        await _loveRepo.SaveChangesAsync();
    }

    public async Task SaveSkillSessionAsync(SaveSkillSessionRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task SaveWorldSessionAsync(SaveWorldSessionRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task SavePaidSessionAsync(SavePaidSessionRequest request)
    {
        throw new NotImplementedException();
    }
}