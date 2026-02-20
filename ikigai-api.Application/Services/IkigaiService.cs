using System.Net.Http.Json;
using System.Text.Json;
using ikigai_api.Application.DTOs;
using ikigai_api.Application.Interfaces;
using ikigai_api.Common.Extensions;
using ikigai_api.Domain.Entities;
using ikigai_api.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;



public class IkigaiService : IIkigaiService
{
    private readonly IGenericRepository<User> _userRepo;
    private readonly IGenericRepository<PrologueData> _prologueRepo;
    private readonly IGenericRepository<LoveSessionData> _loveRepo;
    private readonly IGenericRepository<SkillSessionData> _skillRepo;
    private readonly IGenericRepository<WorldSessionData> _worldRepo;
    private readonly IGenericRepository<PaidSessionData> _paidRepo;
    private readonly IGenericRepository<IkigaiSummary> _ikigaiSummaryRepo;
    private readonly IIkigaiResultRepository _resultRepo;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _n8nWebhookUrl;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IIkigaiScoreService _scoreService;

    public IkigaiService(
        IGenericRepository<User> userRepo,
        IGenericRepository<PrologueData> prologueRepo,
        IGenericRepository<LoveSessionData> loveRepo,
        IGenericRepository<SkillSessionData> skillRepo,
        IGenericRepository<WorldSessionData> worldRepo,
        IGenericRepository<PaidSessionData> paidRepo,
        IGenericRepository<IkigaiSummary> ikigaiSummaryRepo,
        IIkigaiResultRepository resultRepo,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory,
        IIkigaiScoreService scoreService)
    {
        _userRepo = userRepo;
        _prologueRepo = prologueRepo;
        _loveRepo = loveRepo;
        _skillRepo = skillRepo;
        _worldRepo = worldRepo;
        _paidRepo = paidRepo;
        _resultRepo = resultRepo;
        _ikigaiSummaryRepo = ikigaiSummaryRepo;
        _scoreService = scoreService;
        _httpClientFactory = httpClientFactory;
        _n8nWebhookUrl = configuration["N8nIntegration:WebhookUrl"]
                                 ?? throw new ArgumentNullException("N8n Webhook URL is not configured in appsettings.json");
        _serviceScopeFactory = serviceScopeFactory;
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
    public async Task<IkigaiStartResult> StartIkigaiProcessingAsync(Guid userId)
    {
        // -------------------------------------------------------------------------
        // Check Existing Completed Result 
        // -------------------------------------------------------------------------

        var existingResult = await _resultRepo.FindAsync(x =>
        x.UserId == userId &&
        (x.Status == ProcessStatus.Completed || x.Status == ProcessStatus.Processing));

        if (existingResult != null)
        {
            return new IkigaiStartResult
            {
                ProcessId = existingResult.Id,
                Status = existingResult.Status,
                IsExisting = true,
            };
        }

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

        // คำนวณคะแนนจาก Service
        var calculatedScores = _scoreService.CalculateScores(love, skill, world, paid);

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

        var resultEntity = new IkigaiResult
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = ProcessStatus.Pending,
            LovePercentage = calculatedScores.LoveScore.Percentage,
            GoodAtPercentage = calculatedScores.GoodAtScore.Percentage,
            WorldNeedsPercentage = calculatedScores.WorldNeedsScore.Percentage,
            PaidForPercentage = calculatedScores.PaidForScore.Percentage,
            GeneratedAt = DateTime.UtcNow.ToThaiTime(),
            IkigaiSummaries = new List<IkigaiSummary>()
        };

        try
        {
            await _resultRepo.AddAsync(resultEntity);
            await _resultRepo.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error saving initial result: {ex.Message}");

            var raceResult = await _resultRepo.FindAsync(x => x.UserId == userId && x.Status == ProcessStatus.Pending);
            if (raceResult != null)
            {
                return new IkigaiStartResult
                {
                    ProcessId = raceResult.Id,
                    Status = ProcessStatus.Pending,
                    IsExisting = true
                };
            }

            throw; // ถ้าไม่ใช่เรื่องข้อมูลซ้ำ ให้ throw error ปกติ
        }

        // Fire and Forget
        // เรียกหลังจาก Save ลง DB สำเร็จ
        try
        {
            await ProcessInBackgroundAsync(resultEntity.Id, userId, payload);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Background Task Trigger Failed: {ex.Message}");
        }

        return new IkigaiStartResult
        {
            ProcessId = resultEntity.Id,
            Status = ProcessStatus.Pending,
            IsExisting = false,
        };
    }

    private async Task ProcessInBackgroundAsync(Guid resultId, Guid userId, N8nProcessRequest? payload = null)
    {
        // =========================================================================
        // STEP 1: Update Status "Processing" 
        // =========================================================================
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var repo = scope.ServiceProvider.GetRequiredService<IIkigaiResultRepository>();
            var entity = await repo.GetByIdAsync(resultId);

            if (entity != null)
            {
                entity.Status = ProcessStatus.Processing;
                await repo.SaveChangesAsync();
            }
        }

        string? errorMessage = null;
        N8nProcessResponse? n8nResult = null;
        bool isSuccess = false;

        // =========================================================================
        // STEP 2: Main Logic (API Call + Business Logic)
        // =========================================================================
        try
        {
            // 2.1 Call External API
            using (var httpClient = _httpClientFactory.CreateClient("n8nClient"))
            {
                var response = await httpClient.PostAsJsonAsync(_n8nWebhookUrl, payload);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"n8n API failed with status: {response.StatusCode}");
                }

                var rawString = await response.Content.ReadAsStringAsync();
                var n8nList = rawString.FromJsonThai<List<N8nProcessResponse>>();
                n8nResult = n8nList?.FirstOrDefault();

                if (n8nResult == null) throw new Exception("n8n returned empty result or invalid JSON");
            }

            // 2.2 Save Success Result
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var resultRepo = scope.ServiceProvider.GetRequiredService<IIkigaiResultRepository>();
                var summaryRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<IkigaiSummary>>();

                var entity = await resultRepo.GetByIdWithDetailsAsync(resultId);

                if (entity != null && n8nResult.IkigaiAnalysis != null)
                {

                    entity.Status = ProcessStatus.Completed;
                    entity.GeneratedAt = DateTime.UtcNow.ToThaiTime();

                    var analysis = n8nResult.IkigaiAnalysis;
                    var summariesToAdd = new List<IkigaiSummary>();

                    void PrepareData(string type, ComponentResultDto? dto)
                    {
                        if (dto == null) return;
                        summariesToAdd.Add(new IkigaiSummary
                        {
                            Id = Guid.NewGuid(),
                            ResultId = entity.Id,
                            ComponentType = type,
                            OverallSummary = dto.OverallSummary ?? "",
                            ShortSummary = dto.ShortSummary,
                            StrengthsJson = dto.Strengths.ToJsonThai(),
                            DevelopmentPointsJson = dto.DevelopmentPoints.ToJsonThai()
                        });
                    }

                    PrepareData("What You Love", analysis?.WhatYouLove);
                    PrepareData("What You Good At", analysis?.WhatYouGoodAt);
                    PrepareData("What The World Needs", analysis?.WhatTheWorldNeed);
                    PrepareData("What You Can Be Paid For", analysis?.WhatYouCanBePaidFor);
                    PrepareData("Passion", analysis?.Passion);
                    PrepareData("Mission", analysis?.Mission);
                    PrepareData("Profession", analysis?.Profession);
                    PrepareData("Vocation", analysis?.Vocation);

                    await summaryRepo.AddRangeAsync(summariesToAdd);
                    await resultRepo.SaveChangesAsync();

                    isSuccess = true;
                }
            }
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }

        // =========================================================================
        // STEP 3: Final Error Handling (The "Safety Net")
        // =========================================================================

        if (!isSuccess)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                try
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IIkigaiResultRepository>();

                    var entity = await repo.GetByIdAsync(resultId);

                    if (entity != null)
                    {
                        entity.Status = ProcessStatus.Failed;
                        entity.ErrorMessage = errorMessage;

                        await repo.SaveChangesAsync();
                    }
                }
                catch (Exception finalEx)
                {
                    Console.WriteLine($"Critical failure updating status: {finalEx.Message}");
                }
            }
        }
    }
    public async Task<IkigaiResult?> GetProcessStatusAsync(Guid id)
    {
        return await _resultRepo.GetByIdWithSummariesAsync(id);
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
        return await _resultRepo.GetByIdWithSummariesAsync(userId);
    }
}