using System.Net.Http.Json;
using System.Text.Json;
using ikigai_api.Application.DTOs;
using ikigai_api.Application.Interfaces;
using ikigai_api.Common.Extensions;
using ikigai_api.Domain.Entities;
using ikigai_api.Domain.Interfaces;
using Microsoft.Extensions.Configuration;


public class IkigaiService : IIkigaiService
{
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<PrologueData> _prologueRepo;
    private readonly IGenericRepository<LoveSessionData> _loveRepo;
    private readonly IGenericRepository<SkillSessionData> _skillRepo;
    private readonly IGenericRepository<WorldSessionData> _worldRepo;
    private readonly IGenericRepository<PaidSessionData> _paidRepo;
    private readonly IIkigaiResultRepository _resultRepo;
    private readonly HttpClient _httpClient;
    private readonly string _n8nWebhookUrl;

    public IkigaiService(
        IGenericRepository<User> userRepo,
        IGenericRepository<PrologueData> prologueRepo,
        IGenericRepository<LoveSessionData> loveRepo,
        IGenericRepository<SkillSessionData> skillRepo,
        IGenericRepository<WorldSessionData> worldRepo,
        IGenericRepository<PaidSessionData> paidRepo,
        IIkigaiResultRepository resultRepo,
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _userRepo = userRepo;
        _prologueRepo = prologueRepo;
        _loveRepo = loveRepo;
        _skillRepo = skillRepo;
        _worldRepo = worldRepo;
        _paidRepo = paidRepo;
        _resultRepo = resultRepo;
        _httpClient = httpClient;
        _n8nWebhookUrl = configuration["N8nIntegration:WebhookUrl"]
                                 ?? throw new ArgumentNullException("N8n Webhook URL is not configured in appsettings.json");
    }

    public async Task<Guid> SavePrologueAsync(SavePrologueRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PlayerName) || request.PlayerName.Length > 20)
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

        if (hardSkills.Count < 2 || softSkills.Count < 3)
        {
            throw new ArgumentException("You must select at least two hard skills and at least three soft skills.");
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
            throw new ArgumentException("User ID is required.");
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
            throw new ArgumentException("User ID is required.");
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

    public async Task<IkigaiResult> ProcessIkigaiAsync(Guid userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        var prologue = await _prologueRepo.FindAsync(x => x.UserId == userId);
        var love = await _loveRepo.FindAsync(x => x.UserId == userId);
        var skill = await _skillRepo.FindAsync(x => x.UserId == userId);
        var world = await _worldRepo.FindAsync(x => x.UserId == userId);
        var paid = await _paidRepo.FindAsync(x => x.UserId == userId);

        // ตรวจสอบข้อมูลครบถ้วนก่อนส่ง
        if (prologue == null || love == null || skill == null || world == null || paid == null)
        {
            throw new InvalidOperationException("Incomplete session data. Please complete all sessions.");
        }

        //เตรียม Payload ส่ง n8n

        var payload = new N8nProcessRequest
        {
            UserId = user.Id,
            PlayerName = user.PlayerName,

            Prologue = new PrologueDto
            {
                SelectedReasons = DeserializeToList(prologue.SelectedReasons)
            },

            LoveSession = new LoveSessionDto
            {
                SelectedHobbies = DeserializeToList(love.SelectedHobbies),
                CustomHobbies = DeserializeToList(love.CustomHobbies),
                TopThreeHobbies = DeserializeToList(love.TopThreeHobbies),
                DreamAnswer = love.DreamAnswer
            },

            SkillSession = new SkillSessionDto
            {
                SelectedHardSkills = DeserializeToList(skill.SelectedHardSkills),
                CustomHardSkills = DeserializeToList(skill.CustomHardSkills),
                SelectedSoftSkills = DeserializeToList(skill.SelectedSoftSkills),
                CustomSoftSkills = DeserializeToList(skill.CustomSoftSkills),
                SkillsMatchJob = skill.SkillsMatchJob,
                UseSkillsInNewRole = skill.UseSkillsInNewRole
            },

            WorldSession = new WorldSessionDto
            {
                CalledUponAnswer = world.CalledUponAnswer,
                SelectedGifts = DeserializeToList(world.SelectedGifts),
                NoManualChoice = world.NoManualChoice,
                MismatchChoice = world.MismatchChoice,
                FutureValueAnswer = world.FutureValueAnswer
            },

            PaidSession = new PaidSessionDto
            {
                EverPaidAnswer = paid.EverPaidAnswer,
                SelectedJobCards = DeserializeToList(paid.SelectedJobCards),
                MonetizableExperience = paid.MonetizableExperience
            }
        };

        // Call n8n Webhook
        var response = await _httpClient.PostAsJsonAsync(_n8nWebhookUrl, payload);
        response.EnsureSuccessStatusCode();

        var n8nResult = await response.Content.ReadFromJsonAsync<N8nProcessResponse>();
        if (n8nResult == null) throw new Exception("Failed to parse n8n response");

        if (n8nResult == null || n8nResult.IkigaiAnalysis == null)
        {
            throw new Exception("Invalid response structure from n8n");
        }

        var analysis = n8nResult.IkigaiAnalysis;

        var resultEntity = new IkigaiResult
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GeneratedAt = DateTime.UtcNow,
            IkigaiSummaries = new List<IkigaiSummary>()
        };

        // --- 4 Circles ---
        AddSummary(resultEntity, "What You Love", analysis.WhatYouLove);
        AddSummary(resultEntity, "What You Good At", analysis.WhatYouGoodAt);
        AddSummary(resultEntity, "What The World Needs", analysis.WhatTheWorldNeed);
        AddSummary(resultEntity, "What You Can Be Paid For", analysis.WhatYouCanBePaidFor);

        // --- 4 Intersections ---
        AddSummary(resultEntity, "Passion", analysis.Passion);
        AddSummary(resultEntity, "Mission", analysis.Mission);
        AddSummary(resultEntity, "Profession", analysis.Profession);
        AddSummary(resultEntity, "Vocation", analysis.Vocation);

        // Save to DB
        await _resultRepo.AddAsync(resultEntity);
        await _resultRepo.SaveChangesAsync();

        return resultEntity;
    }

    private void AddSummary(IkigaiResult parent, string componentName, ComponentResultDto? dto)
    {
        if (dto == null) return;

        var summary = new IkigaiSummary
        {
            Id = Guid.NewGuid(),
            ResultId = parent.Id, 
            ComponentType = componentName, 
            OverallSummary = dto.OverallSummary ?? "ไม่พบข้อมูลสรุป",
            StrengthsJson = JsonSerializer.Serialize(dto.Strengths ?? new List<string>()),
            DevelopmentPointsJson = JsonSerializer.Serialize(dto.DevelopmentPoints ?? new List<string>())
        };

        parent.IkigaiSummaries.Add(summary);
    }

    private List<string> DeserializeToList(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString)) return new List<string>();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(jsonString) ?? new List<string>();
        }
        catch
        {
            return new List<string>(); // กัน Error กรณี JSON พัง
        }
    }

    public async Task<IkigaiResult?> GetIkigaiResultAsync(Guid userId)
    {
        return await _resultRepo.GetResultWithDetailsAsync(userId);
    }
}