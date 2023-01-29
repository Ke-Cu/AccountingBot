using AccountingBot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AccountingBot.Controllers;

/// <summary>
/// 权限相关接口
/// </summary>
[ApiController]
[Route("[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly JwtSettings _jwtSettings;

    public AuthController(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    /// <summary>
    /// 登录
    /// </summary>
    /// <param name="loginInfo">登录信息</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> LoginAsync([FromBody] LoginInfo loginInfo)
    {
        var userInfo = await DataHelper.GetUserAsync(loginInfo.UserName);

        if (userInfo != null)
        {
            var salt = userInfo.Salt;
            var password = PasswordHasher.GetHashedPassword(loginInfo.Password, ref salt);
            if (userInfo.Password == password)
            {
                var token = loginInfo.GenTokenkey(_jwtSettings);
                return Ok(token);
            }
        }

        return new StatusCodeResult(StatusCodes.Status401Unauthorized);
    }

    /// <summary>
    /// Auth verify
    /// </summary>
    [HttpGet]
    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult Verify()
    {
        return Ok();
    }
}

