using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LineItem.Models;
using LineItem.Test.Integration.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace LineItem.Test.Integration;

[Collection("Integration")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JSON_OPTIONS = new() { WriteIndented = true };
    private readonly ITestOutputHelper _output;
    protected readonly HttpClient Client;
    private int _outputCounter;
    protected long TestUserId;

    protected IntegrationTestBase(ApiFixture fixture, ITestOutputHelper output)
    {
        _output = output;
        Client = fixture.Client;
    }

    public async Task InitializeAsync()
    {
        await OnInitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await OnDisposeAsync();
    }

    /// <summary>
    ///     Override this method to perform any setup required before running tests.
    /// </summary>
    protected virtual Task OnInitializeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Override this method to perform any cleanup required after running tests.
    /// </summary>
    protected virtual Task OnDisposeAsync()
    {
        return Task.CompletedTask;
    }

    // ---- Helpers ----

    protected void LogJson(object value)
    {
        _output.WriteLine(JsonSerializer.Serialize(value, JSON_OPTIONS));
    }

    protected void LogResponse(string action, HttpStatusCode status, string? detail = null)
    {
        _outputCounter++;
        var count = $"{_outputCounter}.".PadRight(4);
        var actionCol = action.PadRight(30);
        var statusCol = status.ToString().PadRight(10);
        var detailCol = detail ?? string.Empty;
        _output.WriteLine($"{count} | {actionCol} | {statusCol} | {detailCol}");
    }

    protected async Task CreateTestUserAsync()
    {
        var testUser = new UserModel
        {
            ExternalId = $"auth0|test-{Guid.NewGuid()}",
            DisplayName = $"{GetType().Name} Test User"
        };

        var response = await Client.PostAsJsonAsync("v1/users", testUser);
        var user = await response.Content.ReadFromJsonAsync<UserModel>();

        TestUserId = user!.Id;
        LogResponse("CREATE USER (setup)", response.StatusCode, $"UserId={TestUserId}");
    }

    protected async Task CleanupTestUserAsync()
    {
        var response = await Client.DeleteAsync($"v1/users/{TestUserId}");
        LogResponse("DELETE USER (cleanup)", response.StatusCode, $"UserId={TestUserId}");
    }

    protected async Task<ProductCategory> CreateProductCategoryAsync(string name = "Test Category")
    {
        var category = new ProductCategory { Name = name };
        var response = await Client.PostAsJsonAsync($"v1/product-categories?createdBy={TestUserId}", category);
        var created = await response.Content.ReadFromJsonAsync<ProductCategory>();
        LogResponse("CREATE CATEGORY (setup)", response.StatusCode, $"CategoryId={created!.Id}");
        return created;
    }

    protected async Task CleanupProductCategoryAsync(long categoryId)
    {
        var response = await Client.DeleteAsync($"v1/product-categories/{categoryId}");
        LogResponse("DELETE CATEGORY (cleanup)", response.StatusCode, $"CategoryId={categoryId}");
    }

    protected async Task<ProductDraft> CreateProductDraftAsync(Product product)
    {
        var response = await Client.PostAsJsonAsync($"v1/product-drafts?createdBy={TestUserId}", product);
        var draft = await response.Content.ReadFromJsonAsync<ProductDraft>();
        LogResponse("CREATE DRAFT (setup)", response.StatusCode, $"DraftId={draft!.Id}");
        return draft;
    }

    protected async Task<Product> PublishProductDraftAsync(long draftId)
    {
        var response = await Client.PostAsync($"v1/product-drafts/{draftId}/publish?createdBy={TestUserId}", null);
        var product = await response.Content.ReadFromJsonAsync<Product>();
        LogResponse("PUBLISH DRAFT (setup)", response.StatusCode, $"ProductId={product!.Id}");
        return product;
    }

    protected async Task CleanupProductDraftAsync(long draftId)
    {
        var response = await Client.DeleteAsync($"v1/product-drafts/{draftId}");
        LogResponse("DELETE DRAFT (cleanup)", response.StatusCode, $"DraftId={draftId}");
    }

    protected async Task CleanupProductAsync(long productId)
    {
        var response = await Client.DeleteAsync($"v1/products/{productId}");
        LogResponse("DELETE PRODUCT (cleanup)", response.StatusCode, $"ProductId={productId}");
    }
}