using System.ComponentModel.DataAnnotations;

namespace AccountingBot.Models
{
    public class BaseOutput
    {
        [Range(0, 0)]
        public int Code { get; set; }

        public string Msg { get; set; }
    }

    public class BaseOutput<T> : BaseOutput
    {
        public T Data { get; set; }
    }
}
