using AccountingBot;
using AccountingBot.HttpApi;
using AccountingBot.Models;
using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.WithOrigins("*")
        .AllowAnyMethod()
        .AllowAnyHeader());
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(nameof(JwtSettings)));
var bindJwtSettings = new JwtSettings();
builder.Configuration.Bind("JwtSettings", bindJwtSettings);

builder.Services.Configure<BasicAuthConfig>(builder.Configuration.GetSection(nameof(BasicAuthConfig)));

builder.Services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme)
    .AddBasic(options =>
    {
        options.Realm = "Basic Authentication";
        options.Events = new BasicAuthenticationEvents
        {
            OnValidateCredentials = context =>
            {
                var config = context.HttpContext.RequestServices.GetService<IOptions<BasicAuthConfig>>();
                if (context.Username == config.Value.UasrName && context.Password == config.Value.Password)
                {
                    var claims = new[]
                    {
                                    new Claim(ClaimTypes.NameIdentifier, context.Username, ClaimValueTypes.String, context.Options.ClaimsIssuer),
                                    new Claim(ClaimTypes.Name, context.Username, ClaimValueTypes.String, context.Options.ClaimsIssuer)
                                };

                    context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                    context.Success();

                }

                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddRouting(option => option.LowercaseUrls = true);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "可家账单API文档",
        Description = "可家账单API文档",
        Contact = new OpenApiContact
        {
            Name = "可家技术开发团队",
            Url = new Uri("https://github.com/Ke-Cu")
        }
    });
    var filePath = Path.Combine(AppContext.BaseDirectory, "AccountingBot.xml");
    c.IncludeXmlComments(filePath);
});

builder.Services.AddHttpApi<ILoginApi>();
builder.Services.AddHttpApi<IChatApi>();
#if !DEBUG
builder.Services.AddHostedService<BotService>();
#endif

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("CorsPolicy");
app.UseAuthorization();

app.MapControllers();

app.Run();
