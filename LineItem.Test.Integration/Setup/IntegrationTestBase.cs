using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using LineItem.Models;
using Xunit;
using Xunit.Abstractions;

namespace LineItem.Test.Integration.Setup;

[Collection("Integration")]
public abstract class IntegrationTestBase
{
    private static readonly JsonSerializerOptions JSON_OPTIONS = new() { WriteIndented = true };
    private readonly ITestOutputHelper _output;

    protected readonly HttpClient Client;
    protected readonly long TestUserId;
    private int _outputCounter;

    protected IntegrationTestBase(ApiFixture fixture, ITestOutputHelper output)
    {
        _output = output;
        Client = fixture.Client;
        TestUserId = fixture.TestUserId;
    }

    // ---- Helpers ----

    protected void LogJson(object value)
    {
        _output.WriteLine(JsonSerializer.Serialize(value, JSON_OPTIONS));
    }

    protected void LogResponse(HttpResponseMessage response, string? detail = null)
    {
        _outputCounter++;
        var count = $"{_outputCounter}.".PadRight(4);
        var method = response.RequestMessage?.Method.Method ?? "UNKNOWN";
        var url = response.RequestMessage?.RequestUri?.PathAndQuery ?? "UNKNOWN";
        var action = $"{method} {url}".PadRight(40);
        var statusCol = response.StatusCode.ToString().PadRight(10);
        var detailCol = detail ?? string.Empty;
        _output.WriteLine($"{count} | {action} | {statusCol} | {detailCol}");
    }

    protected async Task<ProductCategory> CreateProductCategoryAsync(string name = "Test Category")
    {
        var category = new ProductCategory { Name = name };
        var response = await Client.PostAsJsonAsync("v1/product-categories", category);
        var created = await response.Content.ReadFromJsonAsync<ProductCategory>();
        LogResponse(response, $"(setup) CategoryId={created!.Id}");
        return created;
    }

    protected async Task CleanupProductCategoryAsync(long categoryId)
    {
        var response = await Client.DeleteAsync($"v1/product-categories/{categoryId}");
        LogResponse(response, $"(cleanup) CategoryId={categoryId}");
    }

    protected async Task<ProductDraft> CreateProductDraftAsync(Product product)
    {
        var response = await Client.PostAsJsonAsync("v1/product-drafts", product);
        var draft = await response.Content.ReadFromJsonAsync<ProductDraft>();
        LogResponse(response, $"(setup) DraftId={draft!.Id}");
        return draft;
    }

    protected async Task<Product> PublishProductDraftAsync(long draftId)
    {
        var response = await Client.PostAsync($"v1/product-drafts/{draftId}/publish", null);
        var product = await response.Content.ReadFromJsonAsync<Product>();
        LogResponse(response, $"(setup) ProductId={product!.Id}");
        return product;
    }

    protected async Task CleanupProductDraftAsync(long draftId)
    {
        var response = await Client.DeleteAsync($"v1/product-drafts/{draftId}");
        LogResponse(response, $"(cleanup) DraftId={draftId}");
    }

    protected async Task CleanupProductAsync(long productId)
    {
        var response = await Client.DeleteAsync($"v1/products/{productId}");
        LogResponse(response, $"(cleanup) ProductId={productId}");
    }
}