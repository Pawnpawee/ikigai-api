using Microsoft.EntityFrameworkCore;
using ikigai_api.Infrastructure.Persistence;
using ikigai_api.Application.Interfaces;
using ikigai_api.Domain.Interfaces;
using ikigai_api.Infrastructure.Repositories;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using ikigai_api.Application.Services;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => 
        {
            // ระบุ Assembly สำหรับ Migration (ของเดิม)
            npgsqlOptions.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);

            // สั่งให้ลองใหม่ถ้ายิงไม่เข้า (สูงสุด 5 ครั้ง, รอห่างกันรอบละไม่เกิน 10 วิ)
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null
            );
            
            // เพิ่มเวลา Timeout ให้รอได้นานขึ้น 
            npgsqlOptions.CommandTimeout(60); 
        }
    ));

// Add services to the container.
builder.Services.AddControllers();

//? --- [จุดที่ 1] เพิ่ม Code ตรงนี้เพื่อลงทะเบียน Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IIkigaiResultRepository, IkigaiResultRepository>();
builder.Services.AddScoped<IIkigaiService, IkigaiService>();
builder.Services.AddScoped<IIkigaiScoreService, IkigaiScoreService>();
builder.Services.AddHttpClient("n8nClient", (serviceProvider, client) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var apiKey = config["N8nIntegration:ApiKey"];

    if (!string.IsNullOrEmpty(apiKey))
    {
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
    }
});

var allowedOrigins = builder.Configuration
    .GetSection("CorsSettings:AllowedOrigins")
    .Get<string[]>();

var sanitizedOrigins = allowedOrigins?.Select(o => o.TrimEnd('/')).ToArray();

if (allowedOrigins == null || allowedOrigins.Length == 0)
{
    allowedOrigins = new string[] { };
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



if (app.Environment.IsDevelopment())
{
    // In development, allow any origin without credentials for easier testing.
    app.UseCors(x => x
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
}
else
{
    // In production, explicitly specify allowed origins and allow credentials only for those.
    app.UseCors(x => x
        .WithOrigins(sanitizedOrigins ?? Array.Empty<string>())
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
}

app.MapControllers();

app.Run();

