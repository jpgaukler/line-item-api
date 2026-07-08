using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using LineItem.Models;
using LineItem.Test.Integration.Builders;
using LineItem.Test.Integration.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace LineItem.Test.Integration;

public class ProductDraftControllerTests : IntegrationTestBase
{
    public ProductDraftControllerTests(ApiFixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    protected override async Task OnInitializeAsync()
    {
        await CreateTestUserAsync();
    }

    protected override async Task OnDisposeAsync()
    {
        await CleanupTestUserAsync();
    }

    [Fact]
    public async Task ProductDraftCRUD_IsSuccessful()
    {
        ProductDraft? draft = null;

        try
        {
            // CREATE
            var newProduct = ProductBuilder.Default().Build();
            var response = await Client.PostAsJsonAsync($"v1/product-drafts?createdBy={TestUserId}", newProduct);
            draft = await response.Content.ReadFromJsonAsync<ProductDraft>();
            LogResponse("CREATE", response.StatusCode, $"DraftId={draft!.Id}");
            // LogJson(draft);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location.ToString().Should().Contain("v1/product-drafts/");
            draft.Should().NotBeNull();
            draft.Id.Should().BeGreaterThan(0);
            draft.BaseProductId.Should().BeNull();
            draft.BaseVersion.Should().BeNull();
            draft.Product.Id.Should().Be(0);
            draft.Product.Version.Should().Be(0);
            draft.Product.ProductCategoryId.Should().Be(newProduct.ProductCategoryId);
            draft.Product.Name.Should().Be(newProduct.Name);
            draft.Product.Description.Should().Be(newProduct.Description);
            draft.Product.Inputs.Should().BeEquivalentTo(newProduct.Inputs);
            draft.Product.Adders.Should().BeEquivalentTo(newProduct.Adders);
            draft.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            draft.CreatedBy.Should().Be(TestUserId);
            draft.UpdatedAt.Should().BeNull();
            draft.UpdatedBy.Should().BeNull();

            // RETRIEVE
            var createdAt = draft.CreatedAt;
            response = await Client.GetAsync(response.Headers.Location);
            LogResponse("RETRIEVE", response.StatusCode);
            draft = await response.Content.ReadFromJsonAsync<ProductDraft>();
            // LogJson(draft);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            draft.Should().NotBeNull();
            draft.BaseProductId.Should().BeNull();
            draft.BaseVersion.Should().BeNull();
            draft.Product.Id.Should().Be(0);
            draft.Product.Version.Should().Be(0);
            draft.Product.ProductCategoryId.Should().Be(newProduct.ProductCategoryId);
            draft.Product.Name.Should().Be(newProduct.Name);
            draft.Product.Description.Should().Be(newProduct.Description);
            draft.Product.Inputs.Should().BeEquivalentTo(newProduct.Inputs);
            draft.Product.Adders.Should().BeEquivalentTo(newProduct.Adders);
            draft.CreatedAt.Should().Be(createdAt);
            draft.CreatedBy.Should().Be(TestUserId);
            draft.UpdatedAt.Should().BeNull();
            draft.UpdatedBy.Should().BeNull();

            // UPDATE
            var draftId = draft.Id;
            var updatedProduct = ProductBuilder.Default()
                .WithName("Updated name")
                .WithDescription("Updated description")
                .WithProductCodeFormula("=MAT{Material}")
                .WithInput("Material", ["Carbon steel|CS", "Stainless steel|SS"])
                .WithAdder("Level Sensor", [("High level", 100), ("Low level", 100)])
                .Build();
            response = await Client.PutAsJsonAsync(
                $"v1/product-drafts/{draftId}?updatedBy={TestUserId}",
                updatedProduct
            );
            LogResponse("UPDATE DRAFT", response.StatusCode);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // RETRIEVE (verify update)
            response = await Client.GetAsync($"v1/product-drafts/{draftId}");
            LogResponse("RETRIEVE (verify update)", response.StatusCode);
            draft = await response.Content.ReadFromJsonAsync<ProductDraft>();
            // LogJson(draft);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            draft.Should().NotBeNull();
            draft.Id.Should().BeGreaterThan(0);
            draft.BaseProductId.Should().BeNull();
            draft.BaseVersion.Should().BeNull();
            draft.Product.Id.Should().Be(0);
            draft.Product.Version.Should().Be(0);
            draft.Product.ProductCategoryId.Should().Be(updatedProduct.ProductCategoryId);
            draft.Product.Name.Should().Be(updatedProduct.Name);
            draft.Product.Description.Should().Be(updatedProduct.Description);
            draft.Product.Inputs.Should().BeEquivalentTo(updatedProduct.Inputs);
            draft.Product.Adders.Should().BeEquivalentTo(updatedProduct.Adders);
            draft.CreatedAt.Should().Be(createdAt);
            draft.CreatedBy.Should().Be(TestUserId);
            draft.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            draft.UpdatedBy.Should().Be(TestUserId);

            // DELETE
            response = await Client.DeleteAsync($"v1/product-drafts/{draftId}");
            LogResponse("DELETE", response.StatusCode);
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // RETRIEVE (verify deletion)
            response = await Client.GetAsync($"v1/product-drafts/{draftId}");
            LogResponse("RETRIEVE (verify deletion)", response.StatusCode);
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
            draft = await CreateProductDraftAsync(
                ProductBuilder.Default()
                    .WithCategoryId(productCategory.Id)
                    .Build()
            );

            // PUBLISH
            var response =
                await Client.PostAsync($"v1/product-drafts/{draft.Id}/publish?createdBy={TestUserId}", null);
            LogResponse("PUBLISH", response.StatusCode);
            product = await response.Content.ReadFromJsonAsync<Product>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            product.Should().NotBeNull();
            product.Id.Should().BeGreaterThan(0);
            product.Version.Should().Be(1);
            product.ProductCategoryId.Should().Be(draft.Product.ProductCategoryId);
            product.Name.Should().Be(draft.Product.Name);
            product.Description.Should().Be(draft.Product.Description);
            product.Inputs.Should().BeEquivalentTo(draft.Product.Inputs);
            product.Adders.Should().BeEquivalentTo(draft.Product.Adders);

            // RETRIEVE (verify product exists)
            response = await Client.GetAsync($"v1/products/{product.Id}");
            LogResponse("RETRIEVE PRODUCT", response.StatusCode);
            var retrievedProduct = await response.Content.ReadFromJsonAsync<Product>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            retrievedProduct.Should().NotBeNull();
            retrievedProduct.Id.Should().Be(product.Id);
            retrievedProduct.Version.Should().Be(1);
            retrievedProduct.ProductCategoryId.Should().Be(draft.Product.ProductCategoryId);
            retrievedProduct.Description.Should().Be(draft.Product.Description);
            retrievedProduct.Inputs.Should().BeEquivalentTo(draft.Product.Inputs);
            retrievedProduct.Adders.Should().BeEquivalentTo(draft.Product.Adders);

            // VERIFY DRAFT DELETED
            response = await Client.GetAsync($"v1/product-drafts/{draft.Id}");
            LogResponse("(VERIFY DRAFT DELETED", response.StatusCode);
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
            productDraft1 = await CreateProductDraftAsync(
                ProductBuilder.Default()
                    .WithCategoryId(productCategory.Id)
                    .Build()
            );
            product = await PublishProductDraftAsync(productDraft1.Id);

            // CREATE DRAFT FROM PRODUCT
            var response =
                await Client.PostAsync($"v1/product-drafts/from-product/{product.Id}?createdBy={TestUserId}", null);
            productDraft2 = await response.Content.ReadFromJsonAsync<ProductDraft>();
            LogResponse("CREATE DRAFT FROM PRODUCT", response.StatusCode, $"DraftId={productDraft2!.Id}");
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            productDraft2.Should().NotBeNull();
            productDraft2.Id.Should().BeGreaterThan(0);
            productDraft2.BaseProductId.Should().Be(product.Id);
            productDraft2.BaseVersion.Should().Be(1);
            productDraft2.Product.ProductCategoryId.Should().Be(product.ProductCategoryId);
            productDraft2.Product.Name.Should().Be(product.Name);
            productDraft2.Product.Description.Should().Be(product.Description);
            productDraft2.Product.Inputs.Should().BeEquivalentTo(product.Inputs);
            productDraft2.Product.Adders.Should().BeEquivalentTo(product.Adders);

            // UPDATE DRAFT
            var updatedProduct = productDraft2.Product;
            updatedProduct.Name = "Updated Product Name";
            response = await Client.PutAsJsonAsync(
                $"v1/product-drafts/{productDraft2.Id}?updatedBy={TestUserId}",
                updatedProduct
            );
            LogResponse("UPDATE DRAFT", response.StatusCode);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // PUBLISH UPDATED DRAFT 
            response = await Client.PostAsync($"v1/product-drafts/{productDraft2.Id}/publish?createdBy=1", null);
            LogResponse("PUBLISH UPDATED DRAFT", response.StatusCode);
            var publishedProduct = await response.Content.ReadFromJsonAsync<Product>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            publishedProduct.Should().NotBeNull();
            publishedProduct.Name.Should().Be("Updated Product Name");
            publishedProduct.Version.Should().Be(2);

            // VERIFY VERSION HISTORY
            response = await Client.GetAsync($"v1/products/{product.Id}");
            LogResponse("RETRIEVE ACTIVE VERSION", response.StatusCode);
            var activeVersion = await response.Content.ReadFromJsonAsync<Product>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            activeVersion.Should().NotBeNull();
            activeVersion.Name.Should().Be(updatedProduct.Name);

            response = await Client.GetAsync($"v1/products/{product.Id}/versions/1");
            LogResponse("RETRIEVE VERSION 1", response.StatusCode);
            var version1 = await response.Content.ReadFromJsonAsync<Product>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            version1.Should().NotBeNull();
            version1.Name.Should().Be(product.Name);

            response = await Client.GetAsync($"v1/products/{product.Id}/versions/2");
            LogResponse("RETRIEVE VERSION 2", response.StatusCode);
            var version2 = await response.Content.ReadFromJsonAsync<Product>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            version2.Should().NotBeNull();
            version2.Name.Should().Be(updatedProduct.Name);
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
            var invalidProduct = ProductBuilder.Default()
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
                .Build();

            draft = await CreateProductDraftAsync(invalidProduct);

            // PUBLISH
            var response =
                await Client.PostAsync($"v1/product-drafts/{draft.Id}/publish?createdBy={TestUserId}", null);
            var errors = await response.Content.ReadFromJsonAsync<List<string>>();
            LogResponse("PUBLISH INVALID PRODUCT", response.StatusCode, $"Errors={errors?.Count}");
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
        LogResponse("GET PRODUCT", response.StatusCode);
    }

    [Fact]
    public async Task CreateDraftFromProduct_WithInvalidProductId_ReturnsNotFound()
    {
        const long invalidProductId = 0;
        var response = await Client.PostAsync($"v1/product-drafts/from-product/{invalidProductId}?createdBy=1", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        LogResponse("CREATE DRAFT", response.StatusCode);
    }

    [Fact]
    public async Task PublishDraft_WithInvalidId_ReturnsNotFound()
    {
        const long invalidDraftId = 0;
        var response = await Client.PostAsync($"v1/product-drafts/{invalidDraftId}/publish?createdBy=1", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        LogResponse("PUBLISH PRODUCT", response.StatusCode);
    }
}