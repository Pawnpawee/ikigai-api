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
    private readonly IGenericRepository<SkillSessionData> _skillRepo;
    private readonly IGenericRepository<WorldSessionData> _worldRepo;
    private readonly IGenericRepository<PaidSessionData> _paidRepo;

    public IkigaiService(
        IGenericRepository<User> userRepo,
        IGenericRepository<PrologueData> prologueRepo,
        IGenericRepository<LoveSessionData> loveRepo,
        IGenericRepository<SkillSessionData> skillRepo,
        IGenericRepository<WorldSessionData> worldRepo,
        IGenericRepository<PaidSessionData> paidRepo)
    {
        _userRepo = userRepo;
        _prologueRepo = prologueRepo;
        _loveRepo = loveRepo;
        _skillRepo = skillRepo;
        _worldRepo = worldRepo;
        _paidRepo = paidRepo;
    }

    public async Task<Guid> SavePrologueAsync(SavePrologueRequest request)
    {
        if (!request.PlayerName.Any() || request.PlayerName.Length > 20)
        {
            throw new ArgumentException("Player name is required and must be 20 characters or less.");
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
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException("User ID is required.");
        }
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
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException("User ID is required.");
        }

        var hardSkills = request.SelectedHardSkills
            .Concat(request.CustomHardSkills)
            .ToList();

        var softSkills = request.SelectedSoftSkills
            .Concat(request.CustomSoftSkills)
            .ToList();

        if (hardSkills.Count < 2 && softSkills.Count < 3)
        {
            throw new ArgumentException("At least two hard skills and three soft skills must be selected or provided.");
        }

        var skillData = new SkillSessionData
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            SelectedHardSkills = hardSkills.ToJsonThai(),
            CustomHardSkills = request.CustomHardSkills.ToJsonThai(),
            SelectedSoftSkills = softSkills.ToJsonThai(),
            CustomSoftSkills = request.CustomSoftSkills.ToJsonThai(),
            SkillsMatchJob = request.SkillsMatchJob,
            UseSkillsInNewRole = request.UseSkillsInNewRole,
            CreatedAt = DateTime.UtcNow.ToThaiTime(),
            UpdatedAt = DateTime.UtcNow.ToThaiTime()
        };

        await _skillRepo.AddAsync(skillData);
        await _skillRepo.SaveChangesAsync();
    }

    public async Task SaveWorldSessionAsync(SaveWorldSessionRequest request)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException("Invalid User ID.");
        }

        if (!request.SelectedGifts.Any())
        {
            throw new ArgumentException("At least one gift must be selected.");
        }

        var worldData = new WorldSessionData
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CalledUponAnswer = request.CalledUponAnswer.ToJsonThai(),
            SelectedGifts = request.SelectedGifts.ToJsonThai(),
            NoManualChoice = request.NoManualChoice,
            MismatchChoice = request.MismatchChoice,
            FutureValueAnswer = request.FutureValueAnswer,
            CreatedAt = DateTime.UtcNow.ToThaiTime(),
            UpdatedAt = DateTime.UtcNow.ToThaiTime()
        };

        await _worldRepo.AddAsync(worldData);
        await _worldRepo.SaveChangesAsync();
    }

    public async Task SavePaidSessionAsync(SavePaidSessionRequest request)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException("Invalid User ID.");
        }

        if (!request.SelectedJobCards.Any())
        {
            throw new ArgumentException("At least one career must be selected.");
        }

        var paidData = new PaidSessionData
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            EverPaidAnswer = request.EverPaidAnswer,
            SelectedJobCards = request.SelectedJobCards.ToJsonThai(),
            MonetizableExperience = request.MonetizableExperience,
            CreatedAt = DateTime.UtcNow.ToThaiTime(),
            UpdatedAt = DateTime.UtcNow.ToThaiTime()
        };

        await _paidRepo.AddAsync(paidData);
        await _paidRepo.SaveChangesAsync();
    }
}