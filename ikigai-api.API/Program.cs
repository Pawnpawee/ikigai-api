using Microsoft.EntityFrameworkCore;
using ikigai_api.Infrastructure.Persistence;
using ikigai_api.Application.Interfaces;
using ikigai_api.Domain.Interfaces;
using ikigai_api.Infrastructure.Repositories;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

