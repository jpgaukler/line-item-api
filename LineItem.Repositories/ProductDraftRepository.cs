using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LineItem.Models;
using LineItem.Repositories.Helpers;
using LineItem.Repositories.Interfaces;
using LineItem.Repositories.Options;
using LineItem.Repositories.Tables;
using Microsoft.Extensions.Options;

namespace LineItem.Repositories;

public class ProductDraftRepository : DatabaseRepository, IProductDraftRepository
{
    public ProductDraftRepository(IOptions<DatabaseOptions> options) : base(options)
    {
    }

    // --- Product Draft workflow ---

    public async Task<ProductDraft> CreateAsync(
        Product product,
        long createdBy,
        CancellationToken cancellationToken
    )
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.product_draft_insert(@product_data::jsonb, @created_by);",
            new
            {
                product_data = product.ToJson(),
                created_by = createdBy
            },
            cancellationToken: cancellationToken);

        var result = await connection.QuerySingleAsync<ProductDraftRow>(command);
        return MapDraft(result);
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
        return MapDraft(result);
    }

    public async Task UpdateAsync(
        long draftId,
        Product product,
        long updatedBy,
        CancellationToken cancellationToken
    )
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT lineitem.product_draft_update(@id, @product_data::jsonb, @updated_by);",
            new
            {
                id = draftId,
                product_data = product.ToJson(),
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
        return result is null ? null : MapDraft(result);
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
                "SELECT * FROM lineitem.product_version_insert(@product_id, @product_data::jsonb, @created_by);",
                new
                {
                    product_id = baseProductId,
                    product_data = draft.Product.ToJson(),
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
            return productVersionRow.ProductData.ToProduct();
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

    // --- Mapping ---

    private static ProductDraft MapDraft(ProductDraftRow row)
    {
        return new ProductDraft
        {
            Id = row.Id,
            BaseProductId = row.BaseProductId,
            BaseVersion = row.BaseVersion,
            Product = row.ProductData.ToProduct(),
            CreatedAt = row.CreatedAt,
            CreatedBy = row.CreatedBy,
            UpdatedAt = row.UpdatedAt,
            UpdatedBy = row.UpdatedBy
        };
    }
}