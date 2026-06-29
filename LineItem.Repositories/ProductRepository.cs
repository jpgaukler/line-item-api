using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LineItem.Models;
using LineItem.Repositories.Interfaces;
using LineItem.Repositories.Options;
using Microsoft.Extensions.Options;

namespace LineItem.Repositories;

public class ProductRepository : DatabaseRepository, IProductRepository
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public ProductRepository(IOptions<DatabaseOptions> options) : base(options)
    {
    }

    // --- Product Draft workflow ---

    public async Task<ProductDraft> CreateDraftAsync(
        Product product,
        long? productId,
        int? baseVersion,
        long createdBy,
        CancellationToken cancellationToken
    )
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.product_draft_insert(@base_product_id, @base_version, @product_data, @created_by);",
            new
            {
                base_product_id = productId,
                base_version = baseVersion,
                product_data = Serialize(product),
                created_by = createdBy
            },
            cancellationToken: cancellationToken);

        var result = await connection.QuerySingleAsync<ProductDraftRow>(command);
        return MapDraft(result);
    }

    public async Task UpdateDraftAsync(
        long draftId,
        Product product,
        long updatedBy,
        CancellationToken cancellationToken
    )
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT lineitem.product_draft_update(@id, @product_data, @updated_by);",
            new
            {
                id = draftId,
                product_data = Serialize(product),
                updated_by = updatedBy
            },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task<ProductDraft?> RetrieveDraftByIdAsync(long draftId, CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.product_draft_retrieve_by_id(@id);",
            new { id = draftId },
            cancellationToken: cancellationToken);

        var result = await connection.QuerySingleOrDefaultAsync<ProductDraftRow>(command);
        return result is null ? null : MapDraft(result);
    }

    public async Task<Product> PublishDraftAsync(
        ProductDraft draft,
        long createdBy,
        CancellationToken cancellationToken
    )
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var baseProductId = draft.BaseProductId;

            // if no existing version, create new product row
            if (baseProductId is null)
            {
                var insertProductCommand = new CommandDefinition(
                    "SELECT * FROM lineitem.product_insert(@product_category_id, @name, @description, @created_by);",
                    new
                    {
                        product_category_id = draft.Product.ProductCategoryId,
                        name = draft.Product.Name,
                        description = draft.Product.Description,
                        created_by = createdBy
                    },
                    transaction,
                    cancellationToken: cancellationToken);
                var productRow = await connection.QuerySingleAsync<ProductRow>(insertProductCommand);

                baseProductId = productRow.Id;
            }

            // insert product version row
            var insertVersionCommand = new CommandDefinition(
                "SELECT * FROM lineitem.product_version_insert(@product_id, @product_data, @created_by);",
                new
                {
                    product_id = baseProductId,
                    product_data = Serialize(draft.Product),
                    created_by = createdBy
                },
                transaction,
                cancellationToken: cancellationToken);
            var productVersionRow = await connection.QuerySingleAsync<ProductVersionRow>(insertVersionCommand);

            // update active product version
            var updateActiveVersionCommand = new CommandDefinition(
                "SELECT lineitem.product_update_active_version(@product_id, @active_version, @updated_by);",
                new
                {
                    product_id = baseProductId,
                    active_version = productVersionRow.Version,
                    updated_by = createdBy
                },
                transaction,
                cancellationToken: cancellationToken);
            await connection.ExecuteAsync(updateActiveVersionCommand);

            // delete the draft row
            var deleteDraftCommand = new CommandDefinition(
                "SELECT lineitem.product_draft_delete(@id);",
                new { id = draft.Id },
                transaction,
                cancellationToken: cancellationToken);
            await connection.ExecuteAsync(deleteDraftCommand);

            await transaction.CommitAsync(cancellationToken);
            return Deserialize(productVersionRow.ProductData);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteDraftAsync(long draftId, CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT lineitem.product_draft_delete(@id);",
            new { id = draftId },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    // --- Product queries ---

    public async Task<Product?> RetrieveByIdAsync(long productId, CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.product_retrieve_active_version_by_id(@id);",
            new { id = productId },
            cancellationToken: cancellationToken);

        var result = await connection.QuerySingleOrDefaultAsync<ProductVersionRow>(command);
        return result is null ? null : MapProduct(result);
    }

    public async Task<Product?> RetrieveVersionByIdAsync(
        long productId,
        int version,
        CancellationToken cancellationToken
    )
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.product_retrieve_specific_version_by_id(@product_id, @version);",
            new
            {
                product_id = productId,
                version
            },
            cancellationToken: cancellationToken);

        var result = await connection.QuerySingleOrDefaultAsync<ProductVersionRow>(command);
        return result is null ? null : MapProduct(result);
    }

    public async Task<IEnumerable<Product>> RetrieveByCategoryIdAsync(
        long categoryId,
        CancellationToken cancellationToken
    )
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.product_retrieve_by_category_id(@category_id);",
            new { category_id = categoryId },
            cancellationToken: cancellationToken);

        var results = await connection.QueryAsync<ProductVersionRow>(command);
        return results.Select(MapProduct);
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.product_search(@search_term);",
            new { search_term = searchTerm },
            cancellationToken: cancellationToken);

        var results = await connection.QueryAsync<ProductVersionRow>(command);
        return results.Select(MapProduct);
    }


    // --- Mapping ---

    private static Product MapProduct(ProductVersionRow row)
    {
        return Deserialize(row.ProductData);
    }

    private static ProductDraft MapDraft(ProductDraftRow row)
    {
        return new ProductDraft
        {
            Id = row.Id,
            BaseProductId = row.BaseProductId,
            BaseVersion = row.BaseVersion,
            Product = Deserialize(row.ProductData),
            CreatedAt = row.CreatedAt,
            CreatedBy = row.CreatedBy,
            UpdatedAt = row.UpdatedAt,
            UpdatedBy = row.UpdatedBy
        };
    }

    private static Product Deserialize(string json)
    {
        return JsonSerializer.Deserialize<Product>(json, _jsonOptions)!;
    }

    private static string Serialize(Product product)
    {
        return JsonSerializer.Serialize(product, _jsonOptions);
    }

    /// <summary>
    ///     Mapping class for the product table.
    /// </summary>
    private class ProductRow
    {
        public long Id { get; set; }
        public long ProductCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? ActiveVersion { get; set; }
        public DateTime CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? UpdatedBy { get; set; }
    }

    /// <summary>
    ///     Mapping class for the product_version table.
    /// </summary>
    private class ProductVersionRow
    {
        public long ProductId { get; set; }
        public int Version { get; set; }
        public string ProductData { get; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public long CreatedBy { get; set; }
    }

    /// <summary>
    ///     Mapping class for the product_draft table.
    /// </summary>
    private class ProductDraftRow
    {
        public long Id { get; set; }
        public long? BaseProductId { get; set; }
        public int? BaseVersion { get; set; }
        public string ProductData { get; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public long CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public long? UpdatedBy { get; set; }
    }
}