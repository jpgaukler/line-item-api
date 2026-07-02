using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LineItem.Models;
using Xunit;
using Xunit.Abstractions;

namespace LineItem.Test.Integration;

public class ProductDraftControllerTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JSON_OPTIONS = new() { WriteIndented = true };
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private long _testUserId;

    public ProductDraftControllerTests(ITestOutputHelper output)
    {
        _output = output;
        _client = new HttpClient
        {
            // BaseAddress = new Uri("https://dev.api.line-item.app")
            BaseAddress = new Uri("https://localhost:7165")
        };
    }

    public async Task InitializeAsync()
    {
        var testUser = new UserModel
        {
            ExternalId = $"auth0|test-{Guid.NewGuid()}",
            DisplayName = "Product Draft Test User"
        };

        var response = await _client.PostAsJsonAsync("v1/users", testUser);
        var user = await response.Content.ReadFromJsonAsync<UserModel>();

        _testUserId = user!.Id;

        _output.WriteLine($"BEFORE EACH | Test user created | UserId={_testUserId}");
    }

    public async Task DisposeAsync()
    {
        await _client.DeleteAsync($"v1/users/{_testUserId}");

        _output.WriteLine($"AFTER EACH | Test user deleted | UserId={_testUserId}");
    }

    // ---- Helpers ----

    private static Product BuildTestProduct(string name = "Test Product")
    {
        return new Product
        {
            ProductCategoryId = 1,
            Name = name,
            Description = "A test product description",
            ProductCodeFormula = "={width}-{height}",
            Inputs =
            [
                new ProductInput
                {
                    Name = "Width",
                    AllowCustomOption = false,
                    DefaultOptionIndex = 0,
                    Options =
                    [
                        new ProductInputOption { DisplayText = "24 inches", Value = "24" },
                        new ProductInputOption { DisplayText = "36 inches", Value = "36" },
                        new ProductInputOption { DisplayText = "48 inches", Value = "48" }
                    ]
                },
                new ProductInput
                {
                    Name = "Height",
                    AllowCustomOption = false,
                    DefaultOptionIndex = 0,
                    Options =
                    [
                        new ProductInputOption { DisplayText = "48 inches", Value = "48" },
                        new ProductInputOption { DisplayText = "60 inches", Value = "60" },
                        new ProductInputOption { DisplayText = "72 inches", Value = "72" }
                    ]
                }
            ],
            Adders =
            [
                new ProductAdder
                {
                    Name = "Screen",
                    AllowCustomOption = false,
                    DefaultOptionIndex = 0,
                    Options =
                    [
                        new ProductAdderOption { DisplayText = "None", Price = 0 },
                        new ProductAdderOption { DisplayText = "Standard", Price = 50 }
                    ]
                }
            ]
        };
    }

    private string Serialize(object? value)
    {
        return JsonSerializer.Serialize(value, JSON_OPTIONS);
    }

    private async Task<ProductDraft> CreateDraftAsync(Product? product = null)
    {
        var response = await _client.PostAsJsonAsync("v1/product-drafts?createdBy=1", product ?? BuildTestProduct());
        var draft = await response.Content.ReadFromJsonAsync<ProductDraft>();
        return draft!;
    }

    private async Task CleanupDraftAsync(long draftId)
    {
        await _client.DeleteAsync($"v1/product-drafts/{draftId}");
    }

    // ---- Tests ----

    [Fact]
    public async Task DraftCRUD_WithValidProduct_IsSuccessful()
    {
        ProductDraft? draft = null;

        try
        {
            // CREATE
            var newProduct = BuildTestProduct();
            var response = await _client.PostAsJsonAsync($"v1/product-drafts?createdBy={_testUserId}", newProduct);
            draft = await response.Content.ReadFromJsonAsync<ProductDraft>();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location.ToString().Should().Contain("v1/product-drafts/");
            draft.Should().NotBeNull();
            draft.Id.Should().BeGreaterThan(0);
            draft.BaseProductId.Should().BeNull();
            draft.BaseVersion.Should().BeNull();
            draft.Product.Name.Should().Be(newProduct.Name);
            draft.Product.Description.Should().Be(newProduct.Description);
            draft.Product.Inputs.Should().BeEquivalentTo(newProduct.Inputs);
            draft.Product.Adders.Should().BeEquivalentTo(newProduct.Adders);
            draft.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            draft.CreatedBy.Should().Be(_testUserId);
            draft.UpdatedAt.Should().BeNull();
            draft.UpdatedBy.Should().BeNull();

            _output.WriteLine($"CREATE | Response: {response.StatusCode} | DraftId={draft.Id}");
            // _output.WriteLine(Serialize(draft));

            // RETRIEVE
            var createdAt = draft.CreatedAt;
            response = await _client.GetAsync(response.Headers.Location);
            draft = await response.Content.ReadFromJsonAsync<ProductDraft>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            draft.Should().NotBeNull();
            draft.BaseProductId.Should().BeNull();
            draft.BaseVersion.Should().BeNull();
            draft.Product.Name.Should().Be(newProduct.Name);
            draft.Product.Description.Should().Be(newProduct.Description);
            draft.Product.Inputs.Should().BeEquivalentTo(newProduct.Inputs);
            draft.Product.Adders.Should().BeEquivalentTo(newProduct.Adders);
            draft.CreatedAt.Should().Be(createdAt);
            draft.CreatedBy.Should().Be(_testUserId);
            draft.UpdatedAt.Should().BeNull();
            draft.UpdatedBy.Should().BeNull();
            _output.WriteLine($"RETRIEVE | Response: {response.StatusCode}");
            // _output.WriteLine(Serialize(draft)); 

            // UPDATE
            var draftId = draft.Id;
            var updatedProduct = BuildTestProduct("Updated Test Product");
            response = await _client.PutAsJsonAsync(
                $"v1/product-drafts/{draftId}?updatedBy={_testUserId}",
                updatedProduct
            );
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            _output.WriteLine($"UPDATE | Response: {response.StatusCode}.");

            // RETRIEVE (verify update)
            response = await _client.GetAsync($"v1/product-drafts/{draftId}");
            draft = await response.Content.ReadFromJsonAsync<ProductDraft>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            draft.Should().NotBeNull();
            draft.Id.Should().BeGreaterThan(0);
            draft.BaseProductId.Should().BeNull();
            draft.BaseVersion.Should().BeNull();
            draft.Product.Name.Should().Be(updatedProduct.Name);
            draft.Product.Description.Should().Be(updatedProduct.Description);
            draft.Product.Inputs.Should().BeEquivalentTo(newProduct.Inputs);
            draft.Product.Adders.Should().BeEquivalentTo(newProduct.Adders);
            draft.CreatedAt.Should().Be(createdAt);
            draft.CreatedBy.Should().Be(_testUserId);
            draft.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            draft.UpdatedBy.Should().Be(_testUserId);
            _output.WriteLine($"RETRIEVE (verify update) | Response: {response.StatusCode}");
            // _output.WriteLine(Serialize(draft)); 

            // DELETE
            response = await _client.DeleteAsync($"v1/product-drafts/{draftId}");
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            draft = null;
            _output.WriteLine($"DELETE | Response: {response.StatusCode}");

            // RETRIEVE (verify deletion)
            response = await _client.GetAsync($"v1/product-drafts/{draftId}");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            _output.WriteLine($"RETRIEVE (verify deletion) | Response: {response.StatusCode}");
        }
        finally
        {
            if (draft is not null)
                await CleanupDraftAsync(draft.Id);
        }
    }

    // [Fact]
    // public async Task PublishDraft_WithValidProduct_CreatesNewProduct()
    // {
    //     ProductDraft? draft = null;
    //
    //     try
    //     {
    //         // CREATE DRAFT
    //         draft = await CreateDraftAsync();
    //
    //         _output.WriteLine($"CREATE - Draft created: DraftId={draft.Id}");
    //
    //         draft.Should().NotBeNull();
    //         draft.Id.Should().BeGreaterThan(0);
    //
    //         // PUBLISH
    //         var response = await _client.PostAsync($"v1/product-drafts/{draft.Id}/publish?createdBy=1", null);
    //         var product = await response.Content.ReadFromJsonAsync<Product>();
    //
    //         _output.WriteLine($"PUBLISH - Product published:\n{Serialize(product)}");
    //
    //         response.StatusCode.Should().Be(HttpStatusCode.OK);
    //         product.Should().NotBeNull();
    //         product.Id.Should().BeGreaterThan(0);
    //         product.Name.Should().Be(draft.Product.Name);
    //         product.Description.Should().Be(draft.Product.Description);
    //         product.Inputs.Should().BeEquivalentTo(draft.Product.Inputs);
    //         product.Adders.Should().BeEquivalentTo(draft.Product.Adders);
    //
    //         // RETRIEVE (verify product exists)
    //         response = await _client.GetAsync($"v1/products/{product.Id}");
    //         var retrievedProduct = await response.Content.ReadFromJsonAsync<Product>();
    //
    //         _output.WriteLine(
    //             $"RETRIEVE - Product retrieved:\n{Serialize(retrievedProduct)}");
    //
    //         response.StatusCode.Should().Be(HttpStatusCode.OK);
    //         retrievedProduct.Should().NotBeNull();
    //         retrievedProduct!.Id.Should().Be(product.Id);
    //
    //         // VERIFY DRAFT DELETED
    //         response = await _client.GetAsync($"v1/product-drafts/{draft.Id}");
    //
    //         _output.WriteLine($"RETRIEVE (verify draft deleted) - Response: {response.StatusCode}");
    //
    //         response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    //         draft = null;
    //     }
    //     finally
    //     {
    //         if (draft is not null)
    //             await CleanupDraftAsync(draft.Id);
    //     }
    // }
    //
    // [Fact]
    // public async Task CreateDraftFromProduct_WithValidProduct_IsSuccessful()
    // {
    //     ProductDraft? initialDraft = null;
    //     ProductDraft? editDraft = null;
    //
    //     try
    //     {
    //         // CREATE AND PUBLISH INITIAL PRODUCT
    //         initialDraft = await CreateDraftAsync();
    //         var response = await _client.PostAsync($"v1/product-drafts/{initialDraft.Id}/publish?createdBy=1", null);
    //         var product = await response.Content.ReadFromJsonAsync<Product>();
    //
    //         _output.WriteLine($"PUBLISH - Initial product published: ProductId={product!.Id}");
    //
    //         response.StatusCode.Should().Be(HttpStatusCode.OK);
    //         product.Should().NotBeNull();
    //         initialDraft = null;
    //
    //         // CREATE DRAFT FROM PRODUCT
    //         response = await _client.PostAsync($"v1/product-drafts/from-product/{product.Id}?createdBy=1", null);
    //         editDraft = await response.Content.ReadFromJsonAsync<ProductDraft>();
    //
    //         _output.WriteLine(
    //             $"CREATE FROM PRODUCT - Edit draft created:\n{Serialize(editDraft)}");
    //
    //         response.StatusCode.Should().Be(HttpStatusCode.Created);
    //         editDraft.Should().NotBeNull();
    //         editDraft!.Id.Should().BeGreaterThan(0);
    //         editDraft.BaseProductId.Should().Be(product.Id);
    //         editDraft.BaseVersion.Should().Be(1);
    //         editDraft.Product.Name.Should().Be(product.Name);
    //
    //         // PUBLISH EDIT DRAFT
    //         var updatedProduct = editDraft.Product;
    //         updatedProduct.Name = "Updated Product Name";
    //
    //         await _client.PutAsJsonAsync($"v1/product-drafts/{editDraft.Id}?updatedBy=1", updatedProduct);
    //         response = await _client.PostAsync($"v1/product-drafts/{editDraft.Id}/publish?createdBy=1", null);
    //         var publishedProduct = await response.Content.ReadFromJsonAsync<Product>();
    //
    //         _output.WriteLine(
    //             $"PUBLISH - Edit draft published:\n{Serialize(publishedProduct)}");
    //
    //         response.StatusCode.Should().Be(HttpStatusCode.OK);
    //         publishedProduct.Should().NotBeNull();
    //         publishedProduct!.Name.Should().Be("Updated Product Name");
    //         editDraft = null;
    //
    //         // VERIFY VERSION HISTORY
    //         response = await _client.GetAsync($"v1/products/{product.Id}/versions/1");
    //         var version1 = await response.Content.ReadFromJsonAsync<Product>();
    //
    //         _output.WriteLine($"RETRIEVE VERSION 1:\n{Serialize(version1)}");
    //
    //         response.StatusCode.Should().Be(HttpStatusCode.OK);
    //         version1!.Name.Should().Be(product.Name);
    //
    //         response = await _client.GetAsync($"v1/products/{product.Id}/versions/2");
    //         var version2 = await response.Content.ReadFromJsonAsync<Product>();
    //
    //         _output.WriteLine($"RETRIEVE VERSION 2:\n{Serialize(version2)}");
    //
    //         response.StatusCode.Should().Be(HttpStatusCode.OK);
    //         version2!.Name.Should().Be("Updated Product Name");
    //     }
    //     finally
    //     {
    //         if (initialDraft is not null)
    //             await CleanupDraftAsync(initialDraft.Id);
    //         if (editDraft is not null)
    //             await CleanupDraftAsync(editDraft.Id);
    //     }
    // }
    //
    // [Fact]
    // public async Task PublishDraft_WithInvalidProduct_ReturnsBadRequest()
    // {
    //     ProductDraft? draft = null;
    //
    //     try
    //     {
    //         var invalidProduct = BuildTestProduct();
    //         invalidProduct.Name = string.Empty;
    //
    //         draft = await CreateDraftAsync(invalidProduct);
    //
    //         _output.WriteLine($"CREATE - Draft created with invalid product: DraftId={draft.Id}");
    //
    //         var response = await _client.PostAsync($"v1/product-drafts/{draft.Id}/publish?createdBy=1", null);
    //         var errors = await response.Content.ReadFromJsonAsync<List<string>>();
    //
    //         _output.WriteLine($"PUBLISH - Validation errors:\n{Serialize(errors)}");
    //
    //         response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    //         errors.Should().NotBeNull();
    //         errors.Should().NotBeEmpty();
    //         errors.Should().Contain(e => e.Contains("name", StringComparison.OrdinalIgnoreCase));
    //     }
    //     finally
    //     {
    //         if (draft is not null)
    //             await CleanupDraftAsync(draft.Id);
    //     }
    // }
    //
    // [Fact]
    // public async Task PublishDraft_WithDuplicateInputNames_ReturnsBadRequest()
    // {
    //     ProductDraft? draft = null;
    //
    //     try
    //     {
    //         var invalidProduct = BuildTestProduct();
    //         invalidProduct.Inputs[1].Name = invalidProduct.Inputs[0].Name;
    //
    //         draft = await CreateDraftAsync(invalidProduct);
    //
    //         _output.WriteLine($"CREATE - Draft created with duplicate input names: DraftId={draft.Id}");
    //
    //         var response = await _client.PostAsync($"v1/product-drafts/{draft.Id}/publish?createdBy=1", null);
    //         var errors = await response.Content.ReadFromJsonAsync<List<string>>();
    //
    //         _output.WriteLine($"PUBLISH - Validation errors:\n{Serialize(errors)}");
    //
    //         response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    //         errors.Should().NotBeNull();
    //         errors.Should().Contain(e => e.Contains("duplicate", StringComparison.OrdinalIgnoreCase));
    //     }
    //     finally
    //     {
    //         if (draft is not null)
    //             await CleanupDraftAsync(draft.Id);
    //     }
    // }
    //
    [Fact]
    public async Task GetDraft_WithInvalidId_ReturnsNotFound()
    {
        const long invalidDraftId = 0;

        var response = await _client.GetAsync($"v1/product-drafts/{invalidDraftId}");

        _output.WriteLine($"GET - Attempted to retrieve draft with invalid ID: {invalidDraftId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateDraftFromProduct_WithInvalidProductId_ReturnsNotFound()
    {
        const long invalidProductId = 0;

        var response = await _client.PostAsync($"v1/product-drafts/from-product/{invalidProductId}?createdBy=1", null);

        _output.WriteLine($"CREATE FROM PRODUCT - Attempted with invalid ProductId: {invalidProductId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PublishDraft_WithInvalidId_ReturnsNotFound()
    {
        const long invalidDraftId = 0;

        var response = await _client.PostAsync($"v1/product-drafts/{invalidDraftId}/publish?createdBy=1", null);

        _output.WriteLine($"PUBLISH - Attempted to publish draft with invalid ID: {invalidDraftId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}