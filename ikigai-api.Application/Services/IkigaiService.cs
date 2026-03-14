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
    private readonly ISseManager _sseManager;

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
        IIkigaiScoreService scoreService,
        ISseManager sseManager)
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
        _sseManager = sseManager;

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
        // Validation
        if (request.TopThreeHobbies.Count < 1)
        {
            throw new ArgumentException("Hobbies must contain atleast 1 items.");
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
    public async Task<Guid> GenerateIkigaiAsync(Guid userId)
    {
        // -------------------------------------------------------------------------
        // Check Existing Completed Result 
        // -------------------------------------------------------------------------

        var existingResult = await _resultRepo.FindAsync(x =>
        x.UserId == userId &&
        (x.Status == ProcessStatus.Completed || x.Status == ProcessStatus.Processing));

        if (existingResult != null)
        {
            return existingResult.Id;
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

        var processId = Guid.NewGuid();

        //เตรียม Payload ส่ง n8n
        var payload = new N8nProcessRequest
        {
            ProcessId = processId,
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
            Id = processId,
            UserId = userId,
            Status = ProcessStatus.Pending,
            LovePercentage = calculatedScores.LoveScore.Percentage,
            GoodAtPercentage = calculatedScores.GoodAtScore.Percentage,
            WorldNeedsPercentage = calculatedScores.WorldNeedsScore.Percentage,
            PaidForPercentage = calculatedScores.PaidForScore.Percentage,
            MaxSessionPercentage = GetMaxSessionType(calculatedScores),
            GeneratedAt = DateTime.UtcNow.ToThaiTime(),
            IkigaiSummaries = new List<IkigaiSummary>()
        };


        await _resultRepo.AddAsync(resultEntity);
        await _resultRepo.SaveChangesAsync();


        // Fire and Forget
        // เรียกหลังจาก Save ลง DB สำเร็จ
        _ = Task.Run(async () =>
        {
            try
            {
                await ProcessInBackgroundAsync(resultEntity.Id, userId, payload);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Background processing failed: {ex.Message}");
            }
        });

        return processId;
    }

    private string GetMaxSessionType(IkigaiScoreResultDto scores)
    {
        var dict = new Dictionary<string, double>
        {
            { "LovePercentage", scores.LoveScore.Percentage },
            { "GoodAtPercentage", scores.GoodAtScore.Percentage },
            { "WorldNeedsPercentage", scores.WorldNeedsScore.Percentage },
            { "PaidForPercentage", scores.PaidForScore.Percentage },
        };
 
        double maxValue = dict.Values.Max();

        var topCategories = dict.Where(kv => kv.Value == maxValue).ToList();

        if (topCategories.Count > 1)
        {
            return "SamePercentage";
        }

        return topCategories.First().Key;
    }

    private async Task ProcessInBackgroundAsync(Guid resultId, Guid userId, N8nProcessRequest? payload = null)
    {
        // STEP 1: Update Status "Processing"
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

        // STEP 2: Call n8n API (Fire and Forget)
        try
        {
            using (var httpClient = _httpClientFactory.CreateClient("n8nClient"))
            {
                // ยิงไปหา Webhook n8n ตัวแม่
                var response = await httpClient.PostAsJsonAsync(_n8nWebhookUrl, payload);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"n8n API failed to start with status: {response.StatusCode}");
                }

            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Failed to trigger n8n: {ex.Message}";
            Console.Error.WriteLine(errorMessage);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IIkigaiResultRepository>();
                var entity = await repo.GetByIdAsync(resultId);

                if (entity != null)
                {
                    entity.Status = ProcessStatus.Failed;
                    entity.ErrorMessage = errorMessage; // เอาไปเซฟลง DB
                    await repo.SaveChangesAsync();
                }
            }

            await _sseManager.SendUpdateAsync(resultId.ToString(), new
            {
                status = "Error",
                progress = -1,
                error = errorMessage // แนบข้อความไปโชว์หน้า UI
            }, -1);
        }
    }

    private List<string> DeserializeToList(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString)) return new List<string>();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(jsonString) ?? new List<string>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to deserialize JSON to List<string>: {ex.Message}");
        }
        return new List<string>();
    }

    private async Task<double> GetPercentageOfAllPlayersAsync(string maxSession)
    {
        if (string.IsNullOrEmpty(maxSession)) return 0.0;

        var countAllPlayer = await _resultRepo.CountAsync(r => r.Status == ProcessStatus.Completed);

        var countInSession = await _resultRepo.CountAsync(r =>
            r.MaxSessionPercentage == maxSession &&
            r.Status == ProcessStatus.Completed);

        if (countAllPlayer == 0) return 0.0;

        double finalPct = ((double)countInSession / countAllPlayer) * 100;

        return Math.Round(finalPct, 2);
    }

    public async Task<IkigaiResultDto?> SaveFinalResultAsync(Guid processId, object resultData)
    {
        // ดึงข้อมูล Entity หลักขึ้นมาก่อน
        var result = await _resultRepo.GetByIdWithSummariesAsync(processId);
        if (result == null) return null;

        try
        {
            // แปลง object
            var jsonString = JsonSerializer.Serialize(resultData);
            var analysis = JsonSerializer.Deserialize<IkigaiAnalysisDto>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (analysis != null)
            {
                // เตรียมข้อมูลลงตาราง IkigaiSummary
                var summariesToAdd = new List<IkigaiSummary>();

                // Helper function (ลอกมาจากโค้ดเดิมของคุณเลยครับ ✅)
                void PrepareData(string type, ComponentResultDto? dto)
                {
                    if (dto == null) return;
                    summariesToAdd.Add(new IkigaiSummary
                    {
                        Id = Guid.NewGuid(),
                        ResultId = result.Id,
                        ComponentType = type,
                        OverallSummary = dto.OverallSummary ?? "",
                        ShortSummary = dto.ShortSummary,
                        StrengthsJson = dto.Strengths.ToJsonThai(),
                        DevelopmentPointsJson = dto.DevelopmentPoints.ToJsonThai()
                    });
                }

                // แมปข้อมูลเข้าตาราง
                PrepareData("What You Love", analysis.WhatYouLove);
                PrepareData("What You Good At", analysis.WhatYouGoodAt);
                PrepareData("What The World Needs", analysis.WhatTheWorldNeed);
                PrepareData("What You Can Be Paid For", analysis.WhatYouCanBePaidFor);
                PrepareData("Passion", analysis.Passion);
                PrepareData("Mission", analysis.Mission);
                PrepareData("Profession", analysis.Profession);
                PrepareData("Vocation", analysis.Vocation);

                // อัปเดตข้อมูลใน Entity หลัก
                result.Status = ProcessStatus.Completed;
                result.GeneratedAt = DateTime.UtcNow.ToThaiTime();

                // บันทึกลง Database
                await _ikigaiSummaryRepo.AddRangeAsync(summariesToAdd);
                _resultRepo.Update(result);
                await _resultRepo.SaveChangesAsync();

                var playersInSessionPct = await GetPercentageOfAllPlayersAsync(result.MaxSessionPercentage ?? string.Empty);

                return new IkigaiResultDto
                {
                    Id = result.Id,
                    Status = result.Status.ToString(),
                    Summaries = result.IkigaiSummaries.Select(s => new IkigaiSummaryDto
                    {
                        ComponentType = s.ComponentType,
                        OverallSummary = s.OverallSummary,
                        ShortSummary = s.ShortSummary,
                        Strengths = s.StrengthsJson.FromJsonThai<List<string>>(),
                        DevelopmentPoints = s.DevelopmentPointsJson.FromJsonThai<List<string>>()
                    }).ToList(),
                    Scores = new IkigaiScoreResultDto
                    {
                        LoveScore = new ScoreDetail { Percentage = result.LovePercentage },
                        GoodAtScore = new ScoreDetail { Percentage = result.GoodAtPercentage },
                        WorldNeedsScore = new ScoreDetail { Percentage = result.WorldNeedsPercentage },
                        PaidForScore = new ScoreDetail { Percentage = result.PaidForPercentage },
                    },
                    MaxSessionPercentage = result.MaxSessionPercentage,
                    PlayersInSessionPct = playersInSessionPct
                };
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Failed to parse and save final result: {ex.Message}";
            Console.WriteLine($"[Error] {errorMessage}");

            result.Status = ProcessStatus.Failed;
            result.ErrorMessage = errorMessage; // เซฟลง DB
            _resultRepo.Update(result);
            await _resultRepo.SaveChangesAsync();

            await _sseManager.SendUpdateAsync(processId.ToString(), new
            {
                status = "Error",
                progress = -1,
                error = errorMessage
            }, -1);
        }
        return null;
    }

    public async Task<IkigaiResultDto?> GetIkigaiResultAsync(Guid processId)
    {
        // 1. ดึงข้อมูลจาก Database พร้อมลูกๆ (Summaries)
        var result = await _resultRepo.GetByIdWithSummariesAsync(processId);

        // ถ้าไม่เจอข้อมูล หรือยังประมวลผลไม่เสร็จ (Status != Completed) คืนค่า null 
        if (result == null || result.Status != ProcessStatus.Completed)
            return null;

        // 2. คำนวณสถิติผู้เล่นคนอื่น 
        var playersInSessionPct = await GetPercentageOfAllPlayersAsync(result.MaxSessionPercentage ?? string.Empty);

        // 3. ประกอบร่าง DTO ตามโครงสร้างที่คุณต้องการเป๊ะๆ 
        return new IkigaiResultDto
        {
            Id = result.Id,
            Status = result.Status.ToString(),
            Summaries = result.IkigaiSummaries.Select(s => new IkigaiSummaryDto
            {
                ComponentType = s.ComponentType,
                OverallSummary = s.OverallSummary,
                ShortSummary = s.ShortSummary,
                Strengths = s.StrengthsJson.FromJsonThai<List<string>>(), // ใช้ Extension ที่คุณมี
                DevelopmentPoints = s.DevelopmentPointsJson.FromJsonThai<List<string>>()
            }).ToList(),
            Scores = new IkigaiScoreResultDto
            {
                LoveScore = new ScoreDetail { Percentage = result.LovePercentage },
                GoodAtScore = new ScoreDetail { Percentage = result.GoodAtPercentage },
                WorldNeedsScore = new ScoreDetail { Percentage = result.WorldNeedsPercentage },
                PaidForScore = new ScoreDetail { Percentage = result.PaidForPercentage },
            },
            MaxSessionPercentage = result.MaxSessionPercentage,
            PlayersInSessionPct = playersInSessionPct
        };
    }

    public async Task<ProcessStatus?> GetStatusOnlyAsync(Guid processId)
    {
        // ดึงเฉพาะ Entity มาดู Status (ไม่ต้อง Include ตารางอื่น)
        var result = await _resultRepo.GetByIdAsync(processId);

        // คืนค่า Status กลับไป (ถ้าไม่เจอเลยจะคืน null)
        return result?.Status;
    }
}