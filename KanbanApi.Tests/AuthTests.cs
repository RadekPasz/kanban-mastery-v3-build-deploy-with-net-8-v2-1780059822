using System.Net;
using System.Net.Http.Json;
using KanbanApi.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KanbanApi.Tests;

public class AuthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthTests(WebApplicationFactory<Program> factory)
    {
        // Take the real app the factory bootstrapped from Program.cs and override
        // just the database: rip out the SQLite registration and swap in an
        // in-memory one so tests never touch a real database file.
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's DbContextOptions<ApplicationDbContext> (the SQLite one).
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor is not null)
                    services.Remove(descriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });
        });
    }

    // Each test gets its own client. The HttpClient talks to the app in memory —
    // no real network, no real web server.
    private HttpClient CreateClient() => _factory.CreateClient();

    [Fact]
    public async Task Register_WithValidData_ReturnsOk()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/register", new
        {
            email = "alice@example.com",
            password = "Test123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/register", new
        {
            email = "bob@example.com",
            password = "123" // too short / no uppercase / no symbol
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_AfterRegister_ReturnsTokens()
    {
        var client = CreateClient();

        var register = await client.PostAsJsonAsync("/register", new
        {
            email = "carol@example.com",
            password = "Test123!"
        });
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);

        var login = await client.PostAsJsonAsync("/login", new
        {
            email = "carol@example.com",
            password = "Test123!"
        });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        // The Identity login endpoint returns an access token on success.
        var body = await login.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrEmpty(body?.AccessToken));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = CreateClient();

        await client.PostAsJsonAsync("/register", new
        {
            email = "dave@example.com",
            password = "Test123!"
        });

        var login = await client.PostAsJsonAsync("/login", new
        {
            email = "dave@example.com",
            password = "WrongPassword1!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
    }

    private sealed record LoginResponse(string? TokenType, string? AccessToken, int ExpiresIn, string? RefreshToken);
}
