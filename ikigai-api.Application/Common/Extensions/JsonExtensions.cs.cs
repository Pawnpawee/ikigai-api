using System.Text.Json;
using System.Text.Encodings.Web;

namespace ikigai_api.Common.Extensions 
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerOptions _thaiOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false
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