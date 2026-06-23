using System.Security.Claims;
using KanbanApi.Data;
using KanbanApi.Models;
using KanbanApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IBoardService, BoardService>();

// Required because the pipeline calls app.UseAuthorization() below and the
// /boards endpoints use .RequireAuthorization().
builder.Services.AddAuthorization();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Identity register/login/refresh endpoints.
app.MapIdentityApi<ApplicationUser>();

app.MapGet("/boards", async (ClaimsPrincipal user, IBoardService boards) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId is null)
        return Results.Unauthorized();

    var result = await boards.GetBoardsForUserAsync(userId);
    return Results.Ok(result);
})
.WithName("GetBoards")
.WithOpenApi()
.RequireAuthorization();

app.MapDelete("/boards/{id:int}", async (int id, ClaimsPrincipal user, IBoardService boards) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (userId is null)
        return Results.Unauthorized();

    var deleted = await boards.DeleteBoardAsync(id, userId);

    // DeleteBoardAsync returns false when the board doesn't exist or the
    // user isn't its owner; we don't reveal which, so respond 404 either way.
    return deleted ? Results.NoContent() : Results.NotFound();
})
.WithName("DeleteBoard")
.WithOpenApi()
.RequireAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

// Exposes the implicit Program class (generated from top-level statements) as
// public so the integration test project can use WebApplicationFactory<Program>.
public partial class Program { }
