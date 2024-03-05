using AccountingBot.Models;
using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

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
    public async Task<IActionResult> GetTodayAccountingInfoAsync()
    {
        var details = await DataHelper.GetMoneyRecordsAsync(TimeHelper.GetTimestampOfToday());
        return Ok(new
        {
            TotalAmount = details.Sum(e => e.Amount),
            Details = details
        });
    }

    /// <summary>
    /// 获取最近指定天数内的账单信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("recentdays")]
    public async Task<IActionResult> QueryRecentDaysInfoAsync([Range(1, 365)] int days)
    {
        var details = await DataHelper.GetMoneyRecordsAsync(TimeHelper.GetTimestampOfToday(forwardDays: days - 1), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        var data = details.GroupBy(e =>
         {
             var day = DateTimeOffset.FromUnixTimeMilliseconds(e.CreateTime).ToOffset(TimeSpan.FromHours(8));
             return new DateTimeOffset(day.Year, day.Month, day.Day, 0, 0, 0, TimeSpan.FromHours(8));
         }).ToList();

        var result = new List<MoneyRecordOneDay>();
        decimal totalAmount = 0;

        var previousGroupDate = DateTimeOffset.MinValue;
        for (int i = 0; i < data.Count; i++)
        {
            var group = data[i];

            if (i != 0)
            {
                var diff = group.Key - previousGroupDate;
                if (diff.Days != 1)
                {
                    for (int j = 1; j < diff.Days; j++)
                    {
                        previousGroupDate += TimeSpan.FromDays(j);
                        result.Add(new MoneyRecordOneDay
                        {
                            Year = previousGroupDate.Year,
                            Month = previousGroupDate.Month,
                            Day = previousGroupDate.Day,
                            DayTotal = 0,
                            Records = new List<MoneyRecord>(),
                            Week = previousGroupDate.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)group.Key.DayOfWeek
                        });
                    }
                }
            }

            previousGroupDate = group.Key;
        }

        foreach (var group in data)
        {
            var dataOfDay = new MoneyRecordOneDay
            {
                Year = group.Key.Year,
                Month = group.Key.Month,
                Day = group.Key.Day,
                DayTotal = group.Sum(e => e.Amount),
                Records = group.ToList(),
                Week = group.Key.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)group.Key.DayOfWeek
            };

            result.Add(dataOfDay);
            totalAmount += dataOfDay.DayTotal;
        }

        return Ok(new
        {
            TotalAmount = details.Sum(e => e.Amount),
            Data = result
        });
    }

    /// <summary>
    /// 获取月度统计数据内容
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <returns></returns>
    [HttpGet("monthlystatistics")]
    public async Task<IActionResult> GetMonthlystatisticsAsync(int year, int month)
    {
        var fromTimestamp = TimeHelper.GetLocalTimeFromTimeString($"{year}-{month}");
        var toDate = (new DateTime(year, month, 1)).AddMonths(1);
        var toTimestamp = TimeHelper.GetLocalTimeFromTimeString($"{toDate.Year}-{toDate.Month}");
        var data = await DataHelper.GetMoneyRecordsAsync(fromTimestamp, toTimestamp);
        var result = data.GroupBy(e => e.TypeName).Select(group => new
        {
            Type = group.Key,
            Amount = group.Sum(e => e.Amount),
            Details = group.Select(e => new
            {
                e.Amount,
                e.Item,
                CreateTime = DateTimeOffset.FromUnixTimeMilliseconds(e.CreateTime).ToOffset(TimeSpan.FromHours(8)).ToString("yyyy-MM-dd"),
                e.TypeName
            })
        });

        return Ok(new
        {
            TotalAmount = data.Sum(e => e.Amount),
            Data = result
        });
    }

    /// <summary>
    /// 获取所有记账类型
    /// </summary>
    /// <returns></returns>
    [HttpGet("types")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<AccountingTypeRecord>))]
    public async Task<IActionResult> GetAccountingTypes()
    {
        var typeList = await DataHelper.GetAccountingTypeListAsync();
        return Ok(typeList);
    }

    /// <summary>
    /// 新增类别（需认证）
    /// </summary>
    /// <returns>添加成功的类型ID</returns>
    [HttpPost("type")]
    [Authorize(AuthenticationSchemes = BasicAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> AddAccountingType(AccountingTypeRecord accountingType)
    {
        if (string.IsNullOrEmpty(accountingType.TypeName))
        {
            return BadRequest("类型名称不能为空");
        }


        if (accountingType.TypeName.Length > 30)
        {
            return BadRequest("类型名称长度不能超过30");
        }

        var typeId = await DataHelper.GetAccountingTypeAsync(accountingType.TypeName);
        if (typeId > 0)
        {
            return BadRequest("类型已存在");
        }

        await DataHelper.AddAccountingTypeAsync(accountingType.TypeName, false, -1);
        typeId = await DataHelper.GetAccountingTypeAsync(accountingType.TypeName);

        return Ok(new
        {
            Data = new
            {
                TypeId = typeId
            }
        });
    }

    /// <summary>
    /// 删除类别（需认证）
    /// </summary>
    /// <returns>删除结果</returns>
    [HttpDelete("type")]
    [Authorize(AuthenticationSchemes = BasicAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> RemoveAccountingTypeAsync(long id)
    {
        var errorMsg = await DataHelper.RemoveAccountingTypeByIdAsync(id);
        if (string.IsNullOrEmpty(errorMsg))
        {
            return Ok();
        }
        else
        {
            return BadRequest(errorMsg);
        }
    }

    /// <summary>
    /// 获取指定日期的全部数据
    /// </summary>
    /// <param name="year">年</param>
    /// <param name="month">月</param>
    /// <param name="day">日</param>
    /// <returns></returns>
    [HttpGet("records-on-date")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<MoneyRecord>))]
    public async Task<IActionResult> GetAccountingRecordsByDate(int year, int month, int day)
    {
        var fromTimestamp = TimeHelper.GetLocalTimeFromTimeString($"{year}-{month}-{day}");
        var toTimestamp = fromTimestamp + (long)TimeSpan.FromDays(1).TotalMilliseconds - 1;
        var data = await DataHelper.GetMoneyRecordsAsync(fromTimestamp, toTimestamp);
        return Ok(data);
    }

    /// <summary>
    /// 新增记录（需认证）
    /// </summary>
    /// <returns>新增结果</returns>
    [HttpPost("record")]
    [Authorize(AuthenticationSchemes = BasicAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> AddAccountingRecord(MoneyRecord moneyRecord)
    {
        var typeId = await DataHelper.GetAccountingTypeAsync(moneyRecord.TypeId);
        if (typeId == -1)
        {
            return BadRequest("类型不存在");
        }

        var id = await DataHelper.AddMoneyRecordAsync(-1, moneyRecord.Item, moneyRecord.Amount, -1, typeId, moneyRecord.CreateTime);

        return Ok(new
        {
            Data = new
            {
                Id = id
            }
        });
    }

    /// <summary>
    /// 删除记录（需认证）
    /// </summary>
    /// <returns>删除结果</returns>
    [HttpDelete("record")]
    [Authorize(AuthenticationSchemes = BasicAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> RemoveAccountingRecord(long id)
    {
        var result = await DataHelper.RemoveAccountingRecordByIdAsync(id);
        if (result == true)
        {
            return Ok();
        }
        else
        {
            return BadRequest("记录不存在");
        }
    }

    [HttpPost("remotecall")]
    [Authorize(AuthenticationSchemes = BasicAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IActionResult> RemoteCallAsync([FromBody] MessageOutput message)
    {
        var result = await BotService.GetMessageResponseAsync(message);
        return Ok(result);
    }
}
