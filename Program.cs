using Microsoft.EntityFrameworkCore;
using SmartTaskEngine.Application.Services;
using SmartTaskEngine.Infrastructure.Persistence;
using SmartTaskEngine.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. DbContext Baðlantýsý
builder.Services.AddDbContext<SmartTaskDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Akýllý Öncelik Skoru Algoritma Servisi (Dependency Injection)
builder.Services.AddScoped<ISmartPriorityCalculator, SmartPriorityCalculator>();
// Arka Plan Görev Servisi (Background Service)
builder.Services.AddHostedService<DeadlineCheckerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();

app.UseStaticFiles(); // WWWROOT klasöründeki HTML/CSS/JS dosyalarýný dýþarý açar

app.UseAuthorization();

app.MapControllers();

app.Run();