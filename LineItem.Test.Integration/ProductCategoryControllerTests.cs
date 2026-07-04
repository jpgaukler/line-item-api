using System;
using System.Collections.Generic;
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

public class ProductCategoryControllerTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JSON_OPTIONS = new() { WriteIndented = true };
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private int _outputCounter;
    private long _testUserId;

    public ProductCategoryControllerTests(ITestOutputHelper output)
    {
        _output = output;
        _client = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:7165")
        };
    }

    public async Task InitializeAsync()
    {
        var testUser = new UserModel
        {
            ExternalId = $"auth0|test-{Guid.NewGuid()}",
            DisplayName = "Product Category Test User"
        };

        var response = await _client.PostAsJsonAsync("v1/users", testUser);
        var user = await response.Content.ReadFromJsonAsync<UserModel>();

        _testUserId = user!.Id;
        LogResponse("CREATE USER (setup)", response.StatusCode, $"UserId={_testUserId}");
    }

    public async Task DisposeAsync()
    {
        var response = await _client.DeleteAsync($"v1/users/{_testUserId}");
        LogResponse("DELETE USER (cleanup)", response.StatusCode, $"UserId={_testUserId}");
    }

    // ---- Helpers ----

    private void LogJson(object value)
    {
        _output.WriteLine(JsonSerializer.Serialize(value, JSON_OPTIONS));
    }

    private void LogResponse(string action, HttpStatusCode status, string? detail = null)
    {
        _outputCounter++;
        var count = $"{_outputCounter}.".PadRight(4);
        var actionCol = action.PadRight(30);
        var statusCol = status.ToString().PadRight(10);
        var detailCol = detail ?? string.Empty;
        _output.WriteLine($"{count} | {actionCol} | {statusCol} | {detailCol}");
    }

    private async Task<ProductCategory> CreateCategoryAsync(string name = "Test Category")
    {
        var category = new ProductCategory { Name = name };
        var response = await _client.PostAsJsonAsync($"v1/product-categories?createdBy={_testUserId}", category);
        var created = await response.Content.ReadFromJsonAsync<ProductCategory>();
        LogResponse("CREATE CATEGORY (setup)", response.StatusCode, $"CategoryId={created!.Id}");
        return created;
    }

    private async Task CleanupCategoryAsync(long categoryId)
    {
        var response = await _client.DeleteAsync($"v1/product-categories/{categoryId}");
        LogResponse("DELETE CATEGORY (cleanup)", response.StatusCode, $"CategoryId={categoryId}");
    }

    // ---- Tests ----

    [Fact]
    public async Task ProductCategoryCRUD_IsSuccessful()
    {
        ProductCategory? category = null;

        try
        {
            // CREATE
            var newCategory = new ProductCategory { Name = "Test Category" };
            var response = await _client.PostAsJsonAsync($"v1/product-categories?createdBy={_testUserId}", newCategory);
            category = await response.Content.ReadFromJsonAsync<ProductCategory>();
            LogResponse("CREATE", response.StatusCode, $"CategoryId={category!.Id}");
            // LogJson(category);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location.ToString().Should().Contain("v1/product-categories/");
            category.Should().NotBeNull();
            category.Id.Should().BeGreaterThan(0);
            category.Name.Should().Be(newCategory.Name);
            category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            category.CreatedBy.Should().Be(_testUserId);
            category.UpdatedAt.Should().BeNull();
            category.UpdatedBy.Should().BeNull();

            // RETRIEVE
            var createdAt = category.CreatedAt;
            response = await _client.GetAsync(response.Headers.Location);
            category = await response.Content.ReadFromJsonAsync<ProductCategory>();
            LogResponse("RETRIEVE", response.StatusCode);
            // LogJson(category);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            category.Should().NotBeNull();
            category.Id.Should().BeGreaterThan(0);
            category.Name.Should().Be(newCategory.Name);
            category.CreatedAt.Should().Be(createdAt);
            category.CreatedBy.Should().Be(_testUserId);
            category.UpdatedAt.Should().BeNull();
            category.UpdatedBy.Should().BeNull();

            // RETRIEVE ALL
            response = await _client.GetAsync("v1/product-categories");
            var categories = await response.Content.ReadFromJsonAsync<List<ProductCategory>>();
            LogResponse("RETRIEVE ALL", response.StatusCode, $"Count={categories?.Count}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            categories.Should().NotBeNull();
            categories.Should().Contain(c => c.Id == category!.Id);

            // UPDATE
            var categoryId = category.Id;
            var updatedCategory = new ProductCategory { Name = "Updated Test Category" };
            response = await _client.PutAsJsonAsync(
                $"v1/product-categories/{categoryId}?updatedBy={_testUserId}",
                updatedCategory
            );
            category = await response.Content.ReadFromJsonAsync<ProductCategory>();
            LogResponse("UPDATE", response.StatusCode, $"CategoryId={categoryId}");
            // LogJson(category);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            category.Should().NotBeNull();
            category.Id.Should().Be(categoryId);
            category.Name.Should().Be(updatedCategory.Name);
            category.CreatedAt.Should().Be(createdAt);
            category.CreatedBy.Should().Be(_testUserId);
            category.UpdatedAt.Should().NotBeNull();
            category.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            category.UpdatedBy.Should().Be(_testUserId);

            // DELETE
            response = await _client.DeleteAsync($"v1/product-categories/{categoryId}");
            LogResponse("DELETE", response.StatusCode, $"CategoryId={categoryId}");
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            category = null;

            // RETRIEVE (verify deletion)
            response = await _client.GetAsync($"v1/product-categories/{categoryId}");
            LogResponse("RETRIEVE (verify deletion)", response.StatusCode);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            if (category is not null)
                await CleanupCategoryAsync(category.Id);
        }
    }

    [Fact]
    public async Task CreateCategory_WithEmptyName_ReturnsBadRequest()
    {
        var invalidCategory = new ProductCategory { Name = string.Empty };
        var response = await _client.PostAsJsonAsync($"v1/product-categories?createdBy={_testUserId}", invalidCategory);
        LogResponse("CREATE WITH EMPTY NAME", response.StatusCode);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCategory_WithNameExceedingMaxLength_ReturnsBadRequest()
    {
        var invalidCategory = new ProductCategory { Name = new string('A', 101) };
        var response = await _client.PostAsJsonAsync($"v1/product-categories?createdBy={_testUserId}", invalidCategory);
        LogResponse("CREATE WITH NAME TOO LONG", response.StatusCode);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCategory_WithEmptyName_ReturnsBadRequest()
    {
        ProductCategory? category = null;

        try
        {
            // SETUP
            category = await CreateCategoryAsync();

            // UPDATE
            var invalidCategory = new ProductCategory { Name = string.Empty };
            var response = await _client.PutAsJsonAsync(
                $"v1/product-categories/{category.Id}?updatedBy={_testUserId}",
                invalidCategory
            );
            LogResponse("UPDATE WITH EMPTY NAME", response.StatusCode, $"CategoryId={category.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        finally
        {
            if (category is not null)
                await CleanupCategoryAsync(category.Id);
        }
    }

    [Fact]
    public async Task UpdateCategory_WithNameExceedingMaxLength_ReturnsBadRequest()
    {
        ProductCategory? category = null;

        try
        {
            // SETUP
            category = await CreateCategoryAsync();

            // UPDATE
            var invalidCategory = new ProductCategory { Name = new string('A', 101) };
            var response = await _client.PutAsJsonAsync(
                $"v1/product-categories/{category.Id}?updatedBy={_testUserId}",
                invalidCategory
            );
            LogResponse("UPDATE WITH NAME TOO LONG", response.StatusCode, $"CategoryId={category.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        finally
        {
            if (category is not null)
                await CleanupCategoryAsync(category.Id);
        }
    }

    [Fact]
    public async Task GetCategory_WithInvalidId_ReturnsNotFound()
    {
        const long invalidCategoryId = 0;
        var response = await _client.GetAsync($"v1/product-categories/{invalidCategoryId}");
        LogResponse("GET WITH INVALID ID", response.StatusCode);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCategory_WithInvalidId_ReturnsNotFound()
    {
        const long invalidCategoryId = 0;
        var updatedCategory = new ProductCategory { Name = "Updated Category" };
        var response = await _client.PutAsJsonAsync(
            $"v1/product-categories/{invalidCategoryId}?updatedBy={_testUserId}",
            updatedCategory
        );
        LogResponse("UPDATE WITH INVALID ID", response.StatusCode);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}