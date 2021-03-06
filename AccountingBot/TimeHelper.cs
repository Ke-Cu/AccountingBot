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
    }
}
