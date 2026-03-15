using System.Text.Json;
using ikigai_api.Application.DTOs;
using ikigai_api.Application.Interfaces;
using ikigai_api.Domain.Entities;
using ikigai_api.Common.Extensions;

namespace ikigai_api.Application.Services
{
    public class IkigaiScoreService : IIkigaiScoreService
    {
        private const double MAX_SCORE_PER_DIMENSION = 25.0; // ฐานคะแนนดิบเต็มคือ 25 คะแนน

        public IkigaiScoreResultDto CalculateScores(
            LoveSessionData loveData,
            SkillSessionData skillData,
            WorldSessionData worldData,
            PaidSessionData paidData)
        {
            var result = new IkigaiScoreResultDto
            {
                LoveScore = CalculateLoveScore(loveData),
                GoodAtScore = CalculateGoodAtScore(skillData),
                WorldNeedsScore = CalculateWorldNeedsScore(worldData),
                PaidForScore = CalculatePaidForScore(paidData)
            };

            return result;
        }

        private ScoreDetail CalculateLoveScore(LoveSessionData data)
        {
            double score = 0;

            // Scene 6.1: กิจกรรมที่ไม่เบื่อ 1 คะแนนต่อกิจกรรม (Max 10)
            var hobbies = data.SelectedHobbies.FromJsonThai<List<string>>() ?? new List<string>();
            hobbies.AddRange(data.CustomHobbies.FromJsonThai<List<string>>() ?? new List<string>());
            score += Math.Min(hobbies.Count, 10);

            // Scene 6.2: สิ่งที่ทำได้โดยไม่หวังผลตอบแทน ข้อละ 3 คะแนน
            var topThreeHobbies = data.TopThreeHobbies.FromJsonThai<List<string>>() ?? new List<string>();
            score += topThreeHobbies.Count * 3;

            // Scene 6.3: ช่วยให้ตามความฝัน
            if (data.DreamAnswer == "yes") score += 6;
            else if (data.DreamAnswer == "not_sure") score += 3;

            return new ScoreDetail
            {
                RawScore = score,
                Percentage = Math.Round((score / MAX_SCORE_PER_DIMENSION) * 100, 2)
            };
        }

        private ScoreDetail CalculateGoodAtScore(SkillSessionData data)
        {
            double score = 0;

            // Scene 7.1: Hard Skills & Soft Skills ข้อละ 2.5 คะแนน (Max อย่างละ 3 ข้อ = 7.5 คะแนน)
            var hardSkills = data.SelectedHardSkills.FromJsonThai<List<string>>() ?? new List<string>();
            hardSkills.AddRange(data.CustomHardSkills.FromJsonThai<List<string>>() ?? new List<string>());
            score += Math.Min(hardSkills.Count, 3) * 2.5;

            var softSkills = data.SelectedSoftSkills.FromJsonThai<List<string>>() ?? new List<string>();
            softSkills.AddRange(data.CustomSoftSkills.FromJsonThai<List<string>>() ?? new List<string>());
            score += Math.Min(softSkills.Count, 3) * 2.5;

            // Scene 7.2: ตรงกับความต้องการของงาน 
            if (data.SkillsMatchJob == "match") score += 5;

            // Scene 7.3: จะได้ใช้ในอนาคตหรือไม่ 
            if (data.UseSkillsInNewRole == "yes") score += 5;

            return new ScoreDetail
            {
                RawScore = score,
                Percentage = Math.Round((score / MAX_SCORE_PER_DIMENSION) * 100, 2)
            };
        }

        private ScoreDetail CalculateWorldNeedsScore(WorldSessionData data)
        {
            double score = 0;

            // Scene 8.1: เคยมีคนขอให้ช่วย/ขอบคุณ 
            // หมายเหตุ: ใน Entity เก็บเป็น JSON string แต่ค่าหลักคาดว่าเป็น "yes"
            var calledUpon = data.CalledUponAnswer.Replace("\"", ""); // Clean json string quotes just in case
            if (calledUpon == "yes") score += 5;

            // Scene 8.2: สิ่งที่เลือกมอบให้โลก ให้ข้อละ 1 คะแนน (Max 5)
            var gifts = data.SelectedGifts.FromJsonThai<List<string>>() ?? new List<string>();
            score += Math.Min(gifts.Count, 5);

            // Scene 8.3: ทำงานไม่มีคู่มือ 
            if (data.NoManualChoice == "do_myself") score += 5;
            else if (data.NoManualChoice == "ask_first") score += 2;

            // Scene 8.4: งานไม่ตรงใจ 
            if (data.MismatchChoice == "both") score += 5;
            else if (data.MismatchChoice == "adapt_self" || data.MismatchChoice == "adapt_role") score += 3;

            // Scene 8.5: คุณค่าในอีก 10 ปี 
            if (data.FutureValueAnswer == "yes") score += 5;

            return new ScoreDetail
            {
                RawScore = score,
                Percentage = Math.Round((score / MAX_SCORE_PER_DIMENSION) * 100, 2)
            };
        }

        private ScoreDetail CalculatePaidForScore(PaidSessionData data)
        {
            double score = 0;

            // Scene 9.1: เคยได้รับค่าจ้าง 
            if (data.EverPaidAnswer == "yes") score += 5;

            // Scene 9.2: เลือก Job Cards ให้การ์ดละ 5 คะแนน 
            var jobCards = data.SelectedJobCards.FromJsonThai<List<string>>() ?? new List<string>();
            score += jobCards.Count * 5;

            // Scene 9.3: โมเมนต์ Wow คิดเงินได้ (Input length > 0) = 10 คะแนน 
            if (IsValidMonetizableExperience(data.MonetizableExperience))
            {
                score += 10;
            }

            return new ScoreDetail
            {
                RawScore = score,
                Percentage = Math.Round((score / MAX_SCORE_PER_DIMENSION) * 100, 2)
            };
        }


        private bool IsValidMonetizableExperience(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            var cleanInput = input.Trim();

            // 1. ความยาวต้องไม่สั้นเกินไป
            if (cleanInput.Length < 5) return false;

            // 2. ดักการพิมพ์มั่วซ้ำๆ เช่น "aaaaaa", "111111", "asdfasdf"
            var distinctChars = cleanInput.Distinct().Count();
            if (distinctChars < 4) return false;

            return true; // ถ้าผ่านด่านด้านบนมาได้ ถือว่าตั้งใจพิมพ์
        }
    }
}