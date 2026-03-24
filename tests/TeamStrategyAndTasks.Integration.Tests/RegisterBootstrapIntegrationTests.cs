using System.Net;
using Microsoft.AspNetCore.Hosting;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeamStrategyAndTasks.Infrastructure.Identity;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Abstractions;

namespace TeamStrategyAndTasks.Integration.Tests;

public sealed class RegisterBootstrapIntegrationTests : IAsyncLifetime
{
    private readonly ITestOutputHelper _output;
    private PostgreSqlContainer? _postgres;
    private bool _canRun = true;
    private string _skipReason = string.Empty;

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public RegisterBootstrapIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _postgres = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("tsat_integration")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await _postgres.StartAsync();
        }
        catch (Exception ex) when (ex.Message.Contains("Docker", StringComparison.OrdinalIgnoreCase))
        {
            _canRun = false;
            _skipReason = "Docker is not available. Start Docker Desktop (or configure a Docker endpoint) to run RegisterBootstrapIntegrationTests.";
            _output.WriteLine(_skipReason);
            return;
        }

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((_, configBuilder) =>
                {
                    var cfg = new Dictionary<string, string?>
                    {
                        ["Database:Provider"] = "PostgreSQL",
                        ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
                        ["Jwt:Key"] = "this-is-a-test-only-32-char-minimum-key"
                    };

                    configBuilder.AddInMemoryCollection(cfg);
                });
            });

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    public async Task DisposeAsync()
    {
        if (!_canRun)
            return;

        _client?.Dispose();
        if (_factory is not null)
            await _factory.DisposeAsync();
        if (_postgres is not null)
            await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Register_assigns_administrator_to_first_user_and_contributor_to_second_user()
    {
        if (!_canRun)
        {
            _output.WriteLine(_skipReason);
            return;
        }

        var firstResponse = await RegisterAsync("first@example.com", "First User");
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);

        var secondResponse = await RegisterAsync("second@example.com", "Second User");
        secondResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);

        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var firstUser = await userManager.FindByEmailAsync("first@example.com");
        var secondUser = await userManager.FindByEmailAsync("second@example.com");

        firstUser.Should().NotBeNull();
        secondUser.Should().NotBeNull();

        (await userManager.IsInRoleAsync(firstUser!, "Administrator")).Should().BeTrue();
        (await userManager.IsInRoleAsync(secondUser!, "Contributor")).Should().BeTrue();
    }

    private Task<HttpResponseMessage> RegisterAsync(string email, string displayName)
    {
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["displayName"] = displayName,
            ["email"] = email,
            ["password"] = "Passw0rd!",
            ["confirmPassword"] = "Passw0rd!"
        });

        return _client.PostAsync("/auth/register", form);
    }
}
