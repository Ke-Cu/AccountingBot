using AccountingBot.Models;
using WebApiClientCore.Attributes;

namespace AccountingBot.HttpApi
{
    [HttpHost("http://127.0.0.1:8080/")]
    public interface IChatApi
    {
        [HttpGet("/fetchMessage")]
        Task<BaseOutput<List<MessageOutput>>> FetchMessageAsync([Header("sessionKey")] string sessionKey, int count);

        [HttpPost("/sendGroupMessage")]
        Task<BaseOutput> SendGroupMessageAsync([Header("sessionKey")] string sessionKey, [RawStringContent("application/json")] string text);
    }
}
