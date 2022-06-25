namespace AccountingBot.Models
{
    /// <summary>
    /// 一天内的账单记录
    /// </summary>
    public class MoneyRecordOneDay
    {
        /// <summary>
        /// 年
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 月
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// 日
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// 当日总花销
        /// </summary>
        public decimal DayTotal { get; set; }

        /// <summary>
        /// 从周一到周日分别为1到7
        /// </summary>
        public int Week { get; set; }

        /// <summary>
        /// 账单列表
        /// </summary>
        public List<MoneyRecord> Records { get; set; }
    }
}
