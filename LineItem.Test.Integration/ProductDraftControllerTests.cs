using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LineItem.Models;
using LineItem.Test.Integration.Builders;
using LineItem.Test.Integration.Setup;
using Xunit;
using Xunit.Abstractions;

namespace LineItem.Test.Integration;

public class ProductDraftControllerTests : IntegrationTestBase
{
    public ProductDraftControllerTests(ApiFixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public async Task ProductDraftCRUD_IsSuccessful()
    {
        ProductDraft? draft = null;

        try
        {
            // CREATE
            var request = ProductDraftBuilder.Default().BuildCreateRequest();
            var response = await Client.PostAsJsonAsync("v1/product-drafts", request);
            draft = await response.Content.ReadFromJsonAsync<ProductDraft>();
            LogResponse(response, $"DraftId={draft!.Id}");
            // LogJson(draft);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location.ToString().Should().Contain("v1/product-drafts/");
            draft.Should().NotBeNull();
            draft.Id.Should().BeGreaterThan(0);
            draft.BaseProductId.Should().BeNull();
            draft.BaseVersion.Should().BeNull();
            draft.ProductCategoryId.Should().Be(request.ProductCategoryId);
            draft.Name.Should().Be(request.Name);
            draft.Description.Should().Be(request.Description);
            draft.Inputs.Should().BeEquivalentTo(request.Inputs);
            draft.Adders.Should().BeEquivalentTo(request.Adders);
            draft.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            draft.CreatedBy.Should().Be(TestUserId);
            draft.UpdatedAt.Should().BeNull();
            draft.UpdatedBy.Should().BeNull();

            // RETRIEVE
            var createdAt = draft.CreatedAt;
            response = await Client.GetAsync(response.Headers.Location);
            LogResponse(response);
            draft = await response.Content.ReadFromJsonAsync<ProductDraft>();
            // LogJson(draft);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            draft.Should().NotBeNull();
            draft.BaseProductId.Should().BeNull();
            draft.BaseVersion.Should().BeNull();
            draft.ProductCategoryId.Should().Be(request.ProductCategoryId);
            draft.Name.Should().Be(request.Name);
            draft.Description.Should().Be(request.Description);
            draft.Inputs.Should().BeEquivalentTo(request.Inputs);
            draft.Adders.Should().BeEquivalentTo(request.Adders);
            draft.CreatedAt.Should().Be(createdAt);
            draft.CreatedBy.Should().Be(TestUserId);
            draft.UpdatedAt.Should().BeNull();
            draft.UpdatedBy.Should().BeNull();

            // UPDATE
            var draftId = draft.Id;
            var updatedProduct = ProductDraftBuilder.Default()
                .WithName("Updated name")
                .WithDescription("Updated description")
                .WithProductCodeFormula("=MAT{Material}")
                .WithInput("Material", ["Carbon steel|CS", "Stainless steel|SS"])
                .WithAdder("Level Sensor", [("High level", 100), ("Low level", 100)])
                .BuildCreateRequest();
            response = await Client.PutAsJsonAsync(
                $"v1/product-drafts/{draftId}",
                updatedProduct
            );
            LogResponse(response);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // RETRIEVE (verify update)
            response = await Client.GetAsync($"v1/product-drafts/{draftId}");
            LogResponse(response);
            draft = await response.Content.ReadFromJsonAsync<ProductDraft>();
            // LogJson(draft);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            draft.Should().NotBeNull();
            draft.Id.Should().BeGreaterThan(0);
            draft.BaseProductId.Should().BeNull();
            draft.BaseVersion.Should().BeNull();
            draft.ProductCategoryId.Should().Be(updatedProduct.ProductCategoryId);
            draft.Name.Should().Be(updatedProduct.Name);
            draft.Description.Should().Be(updatedProduct.Description);
            draft.Inputs.Should().BeEquivalentTo(updatedProduct.Inputs);
            draft.Adders.Should().BeEquivalentTo(updatedProduct.Adders);
            draft.CreatedAt.Should().Be(createdAt);
            draft.CreatedBy.Should().Be(TestUserId);
            draft.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            draft.UpdatedBy.Should().Be(TestUserId);

            // DELETE
            response = await Client.DeleteAsync($"v1/product-drafts/{draftId}");
            LogResponse(response);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // RETRIEVE (verify deletion)
            response = await Client.GetAsync($"v1/product-drafts/{draftId}");
            LogResponse(response);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        finally
        {
            if (draft is not null)
                await CleanupProductDraftAsync(draft.Id);
        }
    }

    [Fact]
    public async Task PublishDraft_WithValidProduct_CreatesNewProduct()
    {
        ProductCategory? productCategory = null;
        ProductDraft? draft = null;
        Product? product = null;

        try
        {
            // SETUP
            productCategory = await CreateProductCategoryAsync();
            var request = ProductDraftBuilder.Default()
                .WithCategoryId(productCategory.Id)
                .BuildCreateRequest();
            draft = await CreateProductDraftAsync(request);

            // PUBLISH
            var response =
                await Client.PostAsync($"v1/product-drafts/{draft.Id}/publish", null);
            LogResponse(response);
            product = await response.Content.ReadFromJsonAsync<Product>();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            product.Should().NotBeNull();
            product.Id.Should().BeGreaterThan(0);
            product.Version.Should().Be(1);
            product.ProductCategoryId.Should().Be(draft.ProductCategoryId);
            product.Name.Should().Be(draft.Name);
            product.Description.Should().Be(draft.Description);
            product.Inputs.Should().BeEquivalentTo(draft.Inputs);
            product.Adders.Should().BeEquivalentTo(draft.Adders);
            product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            product.CreatedBy.Should().Be(TestUserId);

            // RETRIEVE (verify product exists)
            response = await Client.GetAsync(response.Headers.Location);
            LogResponse(response);
            var retrievedProduct = await response.Content.ReadFromJsonAsync<Product>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            retrievedProduct.Should().NotBeNull();
            retrievedProduct.Id.Should().Be(product.Id);
            retrievedProduct.Version.Should().Be(1);
            retrievedProduct.ProductCategoryId.Should().Be(draft.ProductCategoryId);
            retrievedProduct.Description.Should().Be(draft.Description);
            retrievedProduct.Inputs.Should().BeEquivalentTo(draft.Inputs);
            retrievedProduct.Adders.Should().BeEquivalentTo(draft.Adders);
            retrievedProduct.CreatedAt.Should().Be(product.CreatedAt);
            retrievedProduct.CreatedBy.Should().Be(TestUserId);

            // VERIFY DRAFT DELETED
            response = await Client.GetAsync($"v1/product-drafts/{draft.Id}");
            LogResponse(response);
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            draft = null;
        }
        finally
        {
            if (draft is not null)
                await CleanupProductDraftAsync(draft.Id);
            if (product is not null)
                await CleanupProductAsync(product.Id);
            if (productCategory is not null)
                await CleanupProductCategoryAsync(productCategory.Id);
        }
    }

    [Fact]
    public async Task PublishDraftFromProduct_WithValidProduct_CreatesNewProductVersion()
    {
        ProductCategory? productCategory = null;
        ProductDraft? productDraft1 = null;
        ProductDraft? productDraft2 = null;
        Product? product = null;

        try
        {
            // SETUP 
            productCategory = await CreateProductCategoryAsync();
            var createRequest = ProductDraftBuilder.Default()
                .WithCategoryId(productCategory.Id)
                .BuildCreateRequest();
            productDraft1 = await CreateProductDraftAsync(createRequest);
            product = await PublishProductDraftAsync(productDraft1.Id);

            // CREATE DRAFT FROM PRODUCT
            var response =
                await Client.PostAsync($"v1/product-drafts/from-product/{product.Id}", null);
            productDraft2 = await response.Content.ReadFromJsonAsync<ProductDraft>();
            LogResponse(response, $"DraftId={productDraft2!.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            productDraft2.Should().NotBeNull();
            productDraft2.Id.Should().BeGreaterThan(0);
            productDraft2.BaseProductId.Should().Be(product.Id);
            productDraft2.BaseVersion.Should().Be(1);
            productDraft2.ProductCategoryId.Should().Be(product.ProductCategoryId);
            productDraft2.Name.Should().Be(product.Name);
            productDraft2.Description.Should().Be(product.Description);
            productDraft2.Inputs.Should().BeEquivalentTo(product.Inputs);
            productDraft2.Adders.Should().BeEquivalentTo(product.Adders);
            productDraft2.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            productDraft2.CreatedBy.Should().Be(TestUserId);
            productDraft2.UpdatedAt.Should().BeNull();
            productDraft2.UpdatedBy.Should().BeNull();

            // UPDATE DRAFT
            var updateRequest = ProductDraftBuilder.Default()
                .WithName("Updated Product Name")
                .WithCategoryId(productCategory.Id)
                .BuildUpdateRequest();
            response = await Client.PutAsJsonAsync(
                $"v1/product-drafts/{productDraft2.Id}",
                updateRequest
            );
            LogResponse(response);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // PUBLISH UPDATED DRAFT 
            response = await Client.PostAsync($"v1/product-drafts/{productDraft2.Id}/publish", null);
            LogResponse(response);
            var updatedProduct = await response.Content.ReadFromJsonAsync<Product>();
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            updatedProduct.Should().NotBeNull();
            updatedProduct.Name.Should().Be("Updated Product Name");
            updatedProduct.Version.Should().Be(2);
            updatedProduct.CreatedAt.Should().BeAfter(product.CreatedAt);

            // VERIFY VERSION HISTORY
            response = await Client.GetAsync($"v1/products/{product.Id}");
            LogResponse(response);
            var activeVersion = await response.Content.ReadFromJsonAsync<Product>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            activeVersion.Should().NotBeNull();
            activeVersion.Name.Should().Be(updatedProduct.Name);
            activeVersion.Version.Should().Be(2);
            activeVersion.CreatedAt.Should().Be(updatedProduct.CreatedAt);

            response = await Client.GetAsync($"v1/products/{product.Id}/versions/1");
            LogResponse(response);
            var version1 = await response.Content.ReadFromJsonAsync<Product>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            version1.Should().NotBeNull();
            version1.Name.Should().Be(product.Name);
            version1.Version.Should().Be(1);
            version1.CreatedAt.Should().BeBefore(updatedProduct.CreatedAt);

            response = await Client.GetAsync($"v1/products/{product.Id}/versions/2");
            LogResponse(response);
            var version2 = await response.Content.ReadFromJsonAsync<Product>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            version2.Should().NotBeNull();
            version2.Name.Should().Be(updatedProduct.Name);
            version2.Version.Should().Be(2);
            version2.CreatedAt.Should().BeAfter(version1.CreatedAt);
        }
        finally
        {
            if (productDraft1 is not null)
                await CleanupProductDraftAsync(productDraft1.Id);
            if (productDraft2 is not null)
                await CleanupProductDraftAsync(productDraft2.Id);
            if (product is not null)
                await CleanupProductAsync(product.Id);
            if (productCategory is not null)
                await CleanupProductCategoryAsync(productCategory.Id);
        }
    }

    [Fact]
    public async Task PublishDraft_WithInvalidProduct_ReturnsBadRequest()
    {
        ProductDraft? draft = null;

        try
        {
            // SETUP - build a product that violates every validation rule
            var invalidProduct = ProductDraftBuilder.Default()
                .WithName(string.Empty)
                .WithDescription(string.Empty)
                .WithCategoryId(0)
                .WithInput("DuplicateInputName", ["24 inches|24", "48 inches|48"])
                .WithInput("DuplicateInputName", ["24 inches|24", "48 inches|48"])
                .WithInput("InputMissingDisplayText", [$"{string.Empty}|24"])
                .WithInput("InputMissingValue", [$"24 inches|{string.Empty}"])
                .WithInput("DefaultInputIndexOutOfRange", ["24 inches|24"], false, 999)
                .WithAdder("DuplicateAdderName", [("Standard", 50)])
                .WithAdder("DuplicateAdderName", [("Standard", 50)])
                .WithAdder("DefaultAdderIndexOutOfRange", [("Standard", 50)], false, 999)
                .WithAdder("NegativeAdderPrice", [("Standard", -50)])
                .WithAdder("AdderMissingDisplayText", [(string.Empty, 50)], false, 999)
                .BuildCreateRequest();

            draft = await CreateProductDraftAsync(invalidProduct);

            // PUBLISH
            var response =
                await Client.PostAsync($"v1/product-drafts/{draft.Id}/publish", null);
            var errors = await response.Content.ReadFromJsonAsync<List<string>>();
            LogResponse(response, $"Errors={errors?.Count}");
            // LogJson(errors!);
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            errors.Should().NotBeNull();
            errors.Should().NotBeEmpty();

            // name
            errors.Should().Contain(e => e.Contains("name is required"));

            // description
            errors.Should().Contain(e => e.Contains("description is required"));

            //category
            errors.Should().Contain(e => e.Contains("category") && e.Contains("does not exist"));

            // inputs
            errors.Should().Contain(e => e.Contains("inputs contain duplicate names"));
            errors.Should().Contain(e =>
                e.Contains("Input") && e.Contains("default option index") && e.Contains("out of range"));
            errors.Should().Contain(e => e.Contains("Input") && e.Contains("requires display text"));
            errors.Should().Contain(e => e.Contains("Input") && e.Contains("requires a value"));

            // adders
            errors.Should().Contain(e => e.Contains("adders contain duplicate names"));
            errors.Should().Contain(e =>
                e.Contains("Adder") && e.Contains("default option index") && e.Contains("out of range"));
            errors.Should().Contain(e => e.Contains("Adder") && e.Contains("requires display text"));
            errors.Should()
                .Contain(e => e.Contains("Adder") && e.Contains("price") && e.Contains("cannot be negative"));
        }
        finally
        {
            if (draft is not null)
                await CleanupProductDraftAsync(draft.Id);
        }
    }

    [Fact]
    public async Task GetDraft_WithInvalidId_ReturnsNotFound()
    {
        const long invalidDraftId = 0;
        var response = await Client.GetAsync($"v1/product-drafts/{invalidDraftId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        LogResponse(response);
    }

    [Fact]
    public async Task CreateDraftFromProduct_WithInvalidProductId_ReturnsNotFound()
    {
        const long invalidProductId = 0;
        var response = await Client.PostAsync($"v1/product-drafts/from-product/{invalidProductId}", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        LogResponse(response);
    }

    [Fact]
    public async Task PublishDraft_WithInvalidId_ReturnsNotFound()
    {
        const long invalidDraftId = 0;
        var response = await Client.PostAsync($"v1/product-drafts/{invalidDraftId}/publish", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        LogResponse(response);
    }
}