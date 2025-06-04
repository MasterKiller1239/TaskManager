using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// U¿yj SQL Server (localdb domyœlnie w Visual Studio)
var connectionString =  "Server=localhost;Database=TaskManagerDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=true;";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString).EnableSensitiveDataLogging()
           .EnableDetailedErrors());

builder.Services.AddScoped<ITaskRepository, TaskRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

