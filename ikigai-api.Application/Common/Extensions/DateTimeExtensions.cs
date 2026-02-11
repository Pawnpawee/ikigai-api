namespace ikigai_api.Common.Extensions
{
    public static class DateTimeExtensions
    {
        // ประกาศตัวแปรเก็บ TimeZone ไว้ 
        private static readonly TimeZoneInfo _thaiZone;

        static DateTimeExtensions()
        {
            try
            {
                // สำหรับ Linux / Docker / Railway / macOS
                _thaiZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Bangkok");
            }
            catch (TimeZoneNotFoundException)
            {
                // สำหรับ Windows (Localhost)
                _thaiZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch
            {
                // Fallback สุดท้ายถ้าหาไม่เจอจริงๆ (กันตาย)
                _thaiZone = TimeZoneInfo.Local;
            }
        }

        // Extension Method: แปลงเวลาจาก UTC -> Thai Time
        public static DateTime ToThaiTime(this DateTime utcDateTime)
        {
            // ตรวจสอบว่าเป็น UTC หรือไม่ ถ้าไม่ระบุชนิด ให้ถือว่าเป็น UTC
            if (utcDateTime.Kind == DateTimeKind.Unspecified)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, _thaiZone);
        }
    }
}