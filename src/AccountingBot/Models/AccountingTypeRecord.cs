using Swashbuckle.AspNetCore.Annotations;

namespace AccountingBot.Models
{
    /// <summary>
    /// 记账类型信息
    /// </summary>
    public class AccountingTypeRecord
    {
        /// <summary>
        /// 类别ID
        /// </summary>
        [SwaggerSchema(ReadOnly = true)]
        public long TypeId { get; set; }

        /// <summary>
        /// 类别名称
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 是否默认类型
        /// </summary>
        [SwaggerSchema(ReadOnly = true)]
        public bool Default { get; set; }

        /// <summary>
        /// 消息ID
        /// </summary>
        [SwaggerSchema(ReadOnly = true)]
        public long MsgId { get; set; }
    }
}
