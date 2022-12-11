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
        /// <returns></returns>
        /// <remarks>支持带时区信息的字符串，例如一个东6区时间：2000-01-01 00:00:00 +06:00</remarks>
        public static long GetLocalTimeFromTimeString(string timeText, int offset = 8)
        {
            _ = DateTimeOffset.TryParse(timeText, out var time);
            time += time.Offset + (TimeSpan.FromHours(offset) - time.Offset);
            return time.AddHours(-offset).ToUnixTimeMilliseconds();
        }
    }
}
