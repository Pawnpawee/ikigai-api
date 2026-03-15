using System.Text.Json;
using System.Text.Encodings.Web;

namespace ikigai_api.Common.Extensions
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerOptions _thaiOptions = new JsonSerializerOptions
        {
            // 1. ช่วยให้อ่านภาษาไทยออก (ไม่เป็น \uXXXX)
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,

            // 2. ไม่จัด Format ย่อหน้า (ประหยัดพื้นที่ Log)
            WriteIndented = false,

            // 3.  อ่าน JSON ได้แม้ตัวเล็ก/ใหญ่ไม่ตรงกัน (เช่น JSON ส่ง "what_you_love" แต่ C# เป็น "WhatYouLove" ก็จะ map เข้า)
            PropertyNameCaseInsensitive = true
        };

        public static string ToJsonThai<T>(this T data)
        {
            return JsonSerializer.Serialize(data, _thaiOptions);
        }

        public static T? FromJsonThai<T>(this string json)
        {
            if (string.IsNullOrEmpty(json)) return default;

            return JsonSerializer.Deserialize<T>(json, _thaiOptions);

        }
    }
}