using Microsoft.AspNetCore.Mvc;

namespace AccountingBot.Controllers;

/// <summary>
/// 账单信息
/// </summary>
[ApiController]
[Route("[controller]")]
public class AccountingController : ControllerBase
{
    /// <summary>
    /// 获取今日账单信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("today")]
    public async Task<IActionResult> GetTodayAccountingInfo()
    {
        var details = await DataHelper.GetMoneyRecordsAsync(TimeHelper.GetTimestampOfToday());
        return Ok(details);
    }
}
