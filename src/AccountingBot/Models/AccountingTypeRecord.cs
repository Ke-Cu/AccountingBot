namespace AccountingBot.Models
{
    public class AccountingTypeRecord
    {
        /// <summary>
        /// 类别ID
        /// </summary>
        public long TypeId { get; set; }

        /// <summary>
        /// 类别名称
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 是否默认类型
        /// </summary>
        public bool Default { get; set; }

        /// <summary>
        /// 消息ID
        /// </summary>
        public long MsgId { get; set; }
    }
}
