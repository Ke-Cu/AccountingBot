namespace AccountingBot.Models
{
    /// <summary>
    /// Bot配置项
    /// </summary>
    public class BotConfig
    {
        public long BotId { get; set; }

        public string VerifyKey { get; set; }

        public long[] Groups { get; set; }
    }
}
