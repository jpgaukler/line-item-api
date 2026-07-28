using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LineItem.Models;
using LineItem.Test.Integration.Setup;
using Xunit;
using Xunit.Abstractions;

namespace LineItem.Test.Integration;

public class ProductCategoryControllerTests : IntegrationTestBase
{
    public ProductCategoryControllerTests(ApiFixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public async Task ProductCategoryCRUD_IsSuccessful()
    {
        ProductCategory? category = null;

        try
        {
            // CREATE
            var newCategory = new CreateProductCategoryRequest("Test Category");
            var response = await Client.PostAsJsonAsync("v1/product-categories", newCategory);
            category = await response.Content.ReadFromJsonAsync<ProductCategory>();
            LogResponse(response, $"CategoryId={category!.Id}");
            // LogJson(category);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location.ToString().Should().Contain("v1/product-categories/");
            category.Should().NotBeNull();
            category.Id.Should().BeGreaterThan(0);
            category.Name.Should().Be(newCategory.Name);
            category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            category.CreatedBy.Should().Be(TestUserId);
            category.UpdatedAt.Should().BeNull();
            category.UpdatedBy.Should().BeNull();

            // RETRIEVE
            var createdAt = category.CreatedAt;
            response = await Client.GetAsync(response.Headers.Location);
            category = await response.Content.ReadFromJsonAsync<ProductCategory>();
            LogResponse(response);
            // LogJson(category);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            category.Should().NotBeNull();
            category.Id.Should().BeGreaterThan(0);
            category.Name.Should().Be(newCategory.Name);
            category.CreatedAt.Should().Be(createdAt);
            category.CreatedBy.Should().Be(TestUserId);
            category.UpdatedAt.Should().BeNull();
            category.UpdatedBy.Should().BeNull();

            // RETRIEVE ALL
            response = await Client.GetAsync("v1/product-categories");
            var categories = await response.Content.ReadFromJsonAsync<List<ProductCategory>>();
            LogResponse(response, $"Count={categories?.Count}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            categories.Should().NotBeNull();
            categories.Should().Contain(c => c.Id == category!.Id);

            // UPDATE
            var categoryId = category.Id;
            var updatedCategory = new UpdateProductCategoryRequest("Updated Test Category");
            response = await Client.PutAsJsonAsync(
                $"v1/product-categories/{categoryId}",
                updatedCategory
            );
            category = await response.Content.ReadFromJsonAsync<ProductCategory>();
            LogResponse(response);
            // LogJson(category);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            category.Should().NotBeNull();
            category.Id.Should().Be(categoryId);
            category.Name.Should().Be(updatedCategory.Name);
            category.CreatedAt.Should().Be(createdAt);
            category.CreatedBy.Should().Be(TestUserId);
            category.UpdatedAt.Should().NotBeNull();
            category.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            category.UpdatedBy.Should().Be(TestUserId);

            // DELETE
            response = await Client.DeleteAsync($"v1/product-categories/{categoryId}");
            LogResponse(response);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            category = null;

            // RETRIEVE (verify deletion)
            response = await Client.GetAsync($"v1/product-categories/{categoryId}");
            LogResponse(response);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            if (category is not null)
                await CleanupProductCategoryAsync(category.Id);
        }
    }

    [Fact]
    public async Task CreateCategory_WithEmptyName_ReturnsBadRequest()
    {
        var request = new CreateProductCategoryRequest(string.Empty);
        var response = await Client.PostAsJsonAsync("v1/product-categories", request);
        LogResponse(response);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCategory_WithNameExceedingMaxLength_ReturnsBadRequest()
    {
        var request = new CreateProductCategoryRequest(new string('A', 101));
        var response = await Client.PostAsJsonAsync("v1/product-categories", request);
        LogResponse(response);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCategory_WithEmptyName_ReturnsBadRequest()
    {
        ProductCategory? category = null;

        try
        {
            // SETUP
            category = await CreateProductCategoryAsync();

            // UPDATE
            var invalidCategory = new UpdateProductCategoryRequest(string.Empty);
            var response = await Client.PutAsJsonAsync(
                $"v1/product-categories/{category.Id}",
                invalidCategory
            );
            LogResponse(response);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        finally
        {
            if (category is not null)
                await CleanupProductCategoryAsync(category.Id);
        }
    }

    [Fact]
    public async Task UpdateCategory_WithNameExceedingMaxLength_ReturnsBadRequest()
    {
        ProductCategory? category = null;

        try
        {
            // SETUP
            category = await CreateProductCategoryAsync();

            // UPDATE
            var invalidCategory = new UpdateProductCategoryRequest(new string('A', 101));
            var response = await Client.PutAsJsonAsync(
                $"v1/product-categories/{category.Id}",
                invalidCategory
            );
            LogResponse(response);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
        finally
        {
            if (category is not null)
                await CleanupProductCategoryAsync(category.Id);
        }
    }

    [Fact]
    public async Task GetCategory_WithInvalidId_ReturnsNotFound()
    {
        const long invalidCategoryId = 0;
        var response = await Client.GetAsync($"v1/product-categories/{invalidCategoryId}");
        LogResponse(response);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateCategory_WithInvalidId_ReturnsNotFound()
    {
        const long invalidCategoryId = 0;
        var updatedCategory = new UpdateProductCategoryRequest("Updated Category");
        var response = await Client.PutAsJsonAsync(
            $"v1/product-categories/{invalidCategoryId}",
            updatedCategory
        );
        LogResponse(response);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}