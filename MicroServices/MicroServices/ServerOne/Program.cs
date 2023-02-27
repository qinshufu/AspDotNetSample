using ServerOne;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddConsulClient(new Uri("http://127.0.0.1:8500"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Map("/health/check", context => context.Response.WriteAsync("ok"));

app.UseConsul(new ConsulRegistrationConfigration()
{
    HealthCheckUri = "http://localhost:5166/health/check",
    ServerAddress = "127.0.0.1",
    ServerPort = 8500,
    ServiceName = Assembly.GetEntryAssembly()?.GetName().Name ?? Guid.NewGuid().ToString()[..5],
});

app.Run();
