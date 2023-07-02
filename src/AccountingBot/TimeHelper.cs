namespace AccountingBot
{
    public static class TimeHelper
    {
        public static long GetTimestampOfToday(int offset = 8, int forwardDays = 0)
        {
            var currentDate = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(offset)).Date;
            currentDate = DateTime.SpecifyKind(currentDate, DateTimeKind.Utc);
            var timestamp = new DateTimeOffset(currentDate).AddHours(-offset).AddDays(-forwardDays).ToUnixTimeMilliseconds();
            return timestamp;
        }

        /// <summary>
        /// 结合当前时间偏移量，将时间字符串转换成时间戳
        /// </summary>
        /// <param name="timeText">时间字符串，如yyyy-MM-dd HH:mm:ss</param>
        /// <param name="offset">时间偏移量</param>
        /// <param name="appendOffsetIfZero">当转换的时间字符串时间偏移量为0时，为其添加当前时间偏移量信息</param>
        /// <returns></returns>
        /// <remarks>支持带时区信息的字符串，例如一个东6区时间：2000-01-01 00:00:00 +06:00</remarks>
        public static long GetLocalTimeFromTimeString(string timeText, int offset = 8, bool appendOffsetIfZero = true)
        {
            var success = DateTimeOffset.TryParse(timeText, null, System.Globalization.DateTimeStyles.AssumeUniversal, out var time);

            if (success)
            {
                if (time.Offset.Hours == 0 && appendOffsetIfZero)
                {
                    time = time.AddHours(-offset);
                }
            }

            return time.ToUnixTimeMilliseconds();
        }
    }
}
