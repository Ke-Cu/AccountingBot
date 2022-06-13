namespace AccountingBot.Models
{
    public class MoneyRecord
    {
        /// <summary>
        /// 创建时间（13位时间戳）
        /// </summary>
        public long CreateTime { get; set; }

        /// <summary>
        /// 创建人ID
        /// </summary>
        public long CreateUser { get; set; }

        /// <summary>
        /// 消息ID
        /// </summary>
        public long MsgId { get; set; }

        /// <summary>
        /// 记账事项
        /// </summary>
        public string Item { get; set; }

        /// <summary>
        /// 记账金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 类别ID
        /// </summary>
        public long TypeId { get; set; }

        /// <summary>
        /// 类别名称
        /// </summary>
        public string TypeName { get; set; }
    }
}
