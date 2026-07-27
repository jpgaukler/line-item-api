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
            "SELECT * FROM lineitem.product_draft_insert(@product_category_id, @product_data::jsonb, @created_by);",
            new
            {
                product_category_id = draft.ProductCategoryId,
                product_data = draft.ToProductData().ToJson(),
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
        ProductDraft draft,
        long updatedBy,
        CancellationToken cancellationToken
    )
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT lineitem.product_draft_update(@id, @product_category_id, @product_data::jsonb, @updated_by);",
            new
            {
                id = draftId,
                product_category_id = draft.ProductCategoryId,
                product_data = draft.ToProductData().ToJson(),
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
                "SELECT * FROM lineitem.product_version_insert(@product_id, @product_data::jsonb, @created_by);",
                new
                {
                    product_id = baseProductId,
                    product_data = draft.ToProductData().ToJson(),
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

            return new Product
            {
                Id = baseProductId.Value,
                Version = productVersionRow.Version,
                ProductCategoryId = draft.ProductCategoryId,
                Name = draft.Name,
                Description = draft.Description,
                ProductCodeFormula = draft.ProductCodeFormula,
                Inputs = draft.Inputs,
                Adders = draft.Adders,
                PriceDictionary = draft.PriceDictionary
            };
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
}