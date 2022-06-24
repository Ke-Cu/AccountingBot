using AccountingBot.HttpApi;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.WithOrigins("*")
        .AllowAnyMethod()
        .AllowAnyHeader());
});

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
//builder.Services.AddHostedService<BotService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("CorsPolicy");
app.UseAuthorization();

app.MapControllers();

app.Run();
