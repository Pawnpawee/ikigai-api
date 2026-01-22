using Microsoft.EntityFrameworkCore;
using ikigai_api.Infrastructure.Persistence;
using ikigai_api.Application.Interfaces;
using ikigai_api.Domain.Interfaces;
using ikigai_api.Infrastructure.Repositories;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(Program).Assembly.GetName().Name)
    ));

// Add services to the container.
builder.Services.AddControllers();

//? --- [จุดที่ 1] เพิ่ม Code ตรงนี้เพื่อลงทะเบียน Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IIkigaiService, IkigaiService>();

var allowedOrigins = builder.Configuration
    .GetSection("CorsSettings:AllowedOrigins")
    .Get<string[]>();

if (allowedOrigins == null || allowedOrigins.Length == 0)
{
    Console.WriteLine("Warning: No CORS origins configured!");
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
    .WithOrigins(allowedOrigins)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
}

app.MapControllers();

app.Run();

