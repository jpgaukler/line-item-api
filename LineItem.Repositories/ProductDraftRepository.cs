using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LineItem.Models;
using LineItem.Repositories.Helpers;
using LineItem.Repositories.Interfaces;
using LineItem.Repositories.Options;
using Microsoft.Extensions.Options;

namespace LineItem.Repositories;

public class ProductDraftRepository : RepositoryBase, IProductDraftRepository
{
    public ProductDraftRepository(IOptions<DatabaseOptions> options) : base(options)
    {
    }

    public async Task<ProductDraft> CreateAsync(
        ProductDraft draft,
        long createdBy,
        CancellationToken cancellationToken
    )
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.product_draft_insert(@product_category_id, @product_data_json::jsonb, @created_by);",
            new
            {
                product_category_id = draft.ProductCategoryId,
                product_data_json = MapProductData(draft).ToJson(),
                created_by = createdBy
            },
            cancellationToken: cancellationToken);

        var result = await connection.QuerySingleAsync<ProductDraftRow>(command);
        return MapProductDraftRow(result);
    }

    public async Task<ProductDraft> CreateFromProductAsync(
        long productId,
        long createdBy,
        CancellationToken cancellationToken
    )
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.product_draft_insert_from_product(@base_product_id, @created_by);",
            new
            {
                base_product_id = productId,
                created_by = createdBy
            },
            cancellationToken: cancellationToken);

        var result = await connection.QuerySingleAsync<ProductDraftRow>(command);
        return MapProductDraftRow(result);
    }

    public async Task UpdateAsync(
        long draftId,
        ProductDraft draft,
        long updatedBy,
        CancellationToken cancellationToken
    )
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT lineitem.product_draft_update(@id, @product_category_id, @product_data_json::jsonb, @updated_by);",
            new
            {
                id = draftId,
                product_category_id = draft.ProductCategoryId,
                product_data_json = MapProductData(draft).ToJson(),
                updated_by = updatedBy
            },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task<ProductDraft?> RetrieveByIdAsync(long draftId, CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.product_draft_retrieve_by_id(@id);",
            new { id = draftId },
            cancellationToken: cancellationToken);

        var result = await connection.QuerySingleOrDefaultAsync<ProductDraftRow>(command);
        return result is null ? null : MapProductDraftRow(result);
    }

    public async Task<Product> PublishAsync(
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
                        product_category_id = draft.ProductCategoryId,
                        name = draft.Name,
                        description = draft.Description,
                        created_by = createdBy
                    },
                    transaction,
                    cancellationToken: cancellationToken);
                var productRow = await connection.QuerySingleAsync<ProductRow>(insertProductCommand);

                baseProductId = productRow.Id;
            }

            // insert product version row
            var insertVersionCommand = new CommandDefinition(
                "SELECT * FROM lineitem.product_version_insert(@product_id, @product_data_json::jsonb, @created_by);",
                new
                {
                    product_id = baseProductId,
                    product_data_json = MapProductData(draft).ToJson(),
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

            // commit transaction
            await transaction.CommitAsync(cancellationToken);

            // return published product
            var command = new CommandDefinition(
                "SELECT * FROM lineitem.product_retrieve_active_version_by_id(@id);",
                new { id = baseProductId },
                cancellationToken: cancellationToken);

            var result = await connection.QuerySingleOrDefaultAsync<ProductRepository.ProductVersionDetailRow>(command);

            return ProductRepository.MapProductVersionDetailRow(result!);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteAsync(long draftId, CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT lineitem.product_draft_delete(@id);",
            new { id = draftId },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    private static ProductDraft MapProductDraftRow(ProductDraftRow row)
    {
        var productData = row.ProductData.ToProductData();

        return new ProductDraft
        {
            Id = row.Id,
            BaseProductId = row.BaseProductId,
            BaseVersion = row.BaseVersion,
            ProductCategoryId = row.ProductCategoryId,
            CreatedAt = row.CreatedAt,
            CreatedBy = row.CreatedBy,
            UpdatedAt = row.UpdatedAt,
            UpdatedBy = row.UpdatedBy,
            Name = productData.Name,
            Description = productData.Description,
            ProductCodeFormula = productData.ProductCodeFormula,
            Inputs = productData.Inputs,
            Adders = productData.Adders,
            PriceDictionary = productData.PriceDictionary
        };
    }

    private static ProductData MapProductData(ProductDraft productDraft)
    {
        return new ProductData
        (
            productDraft.Name,
            productDraft.Description,
            productDraft.ProductCodeFormula,
            productDraft.Inputs,
            productDraft.Adders,
            productDraft.PriceDictionary
        );
    }

    /// <summary>
    ///     Mapping shape for the product_draft table.
    /// </summary>
    private record ProductDraftRow(
        long Id,
        long? BaseProductId,
        int? BaseVersion,
        long ProductCategoryId,
        string ProductData,
        DateTime CreatedAt,
        long CreatedBy,
        DateTime? UpdatedAt,
        long? UpdatedBy);

    /// <summary>
    ///     Mapping shape for the product table.
    /// </summary>
    private record ProductRow(
        long Id,
        long ProductCategoryId,
        string Name,
        string Description,
        int? ActiveVersion,
        DateTime CreatedAt,
        long CreatedBy,
        DateTime? UpdatedAt,
        long? UpdatedBy);

    /// <summary>
    ///     Mapping class for the product_version table.
    /// </summary>
    private record ProductVersionRow(
        long ProductId,
        int Version,
        string ProductData,
        DateTime CreatedAt,
        long CreatedBy);
}