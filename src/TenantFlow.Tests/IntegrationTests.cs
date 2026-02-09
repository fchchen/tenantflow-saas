using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TenantFlow.Api.Models;
using TenantFlow.Data;
using Xunit;

namespace TenantFlow.Tests;

public class IntegrationTests : IClassFixture<TenantFlowApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;

    public IntegrationTests(TenantFlowApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TenantIsolation_QuotesAreScopedPerTenant()
    {
        var acmeToken = await LoginAsync("admin@acme.dev", SeedData.DemoPassword);
        var globexToken = await LoginAsync("admin@globex.dev", SeedData.DemoPassword);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", acmeToken);
        var createResponse = await _client.PostAsJsonAsync("/api/v1/quotes", new CreateQuoteRequest("Acme Customer", 1234m));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", globexToken);
        var globexQuotesResponse = await _client.GetAsync("/api/v1/quotes");
        Assert.Equal(HttpStatusCode.OK, globexQuotesResponse.StatusCode);
        var globexQuotes = await globexQuotesResponse.Content.ReadFromJsonAsync<List<QuoteResponse>>(JsonOptions);
        Assert.NotNull(globexQuotes);
        Assert.Empty(globexQuotes!);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", acmeToken);
        var acmeQuotesResponse = await _client.GetAsync("/api/v1/quotes");
        var acmeQuotes = await acmeQuotesResponse.Content.ReadFromJsonAsync<List<QuoteResponse>>(JsonOptions);
        Assert.NotNull(acmeQuotes);
        Assert.Single(acmeQuotes!);
    }

    [Fact]
    public async Task PlatformAdmin_CanProvisionTenant()
    {
        var token = await LoginAsync("platform@tenantflow.dev", SeedData.DemoPassword);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var create = await _client.PostAsJsonAsync("/api/v1/admin/tenants", new CreateTenantRequest("Northwind", "northwind"));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var list = await _client.GetFromJsonAsync<List<TenantResponse>>("/api/v1/admin/tenants", JsonOptions);
        Assert.NotNull(list);
        Assert.Contains(list!, t => t.Slug == "northwind");
    }

    [Fact]
    public async Task TenantAdmin_CannotAccessPlatformAdminEndpoint()
    {
        var token = await LoginAsync("admin@acme.dev", SeedData.DemoPassword);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var create = await _client.PostAsJsonAsync("/api/v1/admin/tenants", new CreateTenantRequest("Forbidden", "forbidden"));
        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);
    }

    [Fact]
    public async Task CreateQuote_InvalidPayload_ReturnsBadRequest()
    {
        var token = await LoginAsync("admin@acme.dev", SeedData.DemoPassword);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/v1/quotes", new
        {
            customerName = string.Empty,
            premium = -10
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateQuote_DisabledFeatureFlag_ReturnsForbidden()
    {
        var platformToken = await LoginAsync("platform@tenantflow.dev", SeedData.DemoPassword);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", platformToken);

        var disableResponse = await _client.PutAsJsonAsync($"/api/v1/admin/feature-flags/{DemoIds.AcmeTenantId}", new UpsertFeatureFlagRequest("quote.create", false, 0));
        Assert.Equal(HttpStatusCode.OK, disableResponse.StatusCode);

        var acmeToken = await LoginAsync("admin@acme.dev", SeedData.DemoPassword);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", acmeToken);

        var createResponse = await _client.PostAsJsonAsync("/api/v1/quotes", new CreateQuoteRequest("Blocked", 100m));
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
    }

    private async Task<string> LoginAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", new LoginRequest(email, password));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        Assert.NotNull(payload);
        return payload!.Token;
    }
}
