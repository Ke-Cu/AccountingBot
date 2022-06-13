using AccountingBot.Models;
using WebApiClientCore.Attributes;

namespace AccountingBot.HttpApi
{
    [HttpHost("http://127.0.0.1:8080/")]
    public interface ILoginApi
    {
        [HttpPost("/verify")]
        Task<VerifyOutput> VerifyAsync([RawStringContent("application/json")] string text);

        [HttpPost("/bind")]
        Task<BaseOutput> BindAsync([RawStringContent("application/json")] string text);
    }
}
