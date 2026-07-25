using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LineItem.Models;
using LineItem.Repositories.Interfaces;
using LineItem.Repositories.Options;
using Microsoft.Extensions.Options;

namespace LineItem.Repositories;

public class ProductCategoryRepository : RepositoryBase, IProductCategoryRepository
{
    public ProductCategoryRepository(IOptions<DatabaseOptions> options) : base(options)
    {
    }

    public async Task<ProductCategory> CreateAsync(
        ProductCategory category,
        long createdBy,
        CancellationToken cancellationToken
    )
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.product_category_insert(@name, @created_by);",
            new
            {
                name = category.Name,
                created_by = createdBy
            },
            cancellationToken: cancellationToken);

        var inserted = await connection.QuerySingleAsync<ProductCategory>(command);
        return inserted;
    }

    public async Task<ProductCategory?> RetrieveByIdAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.product_category_retrieve_by_id(@id);",
            new { id },
            cancellationToken: cancellationToken);

        var category = await connection.QuerySingleOrDefaultAsync<ProductCategory>(command);
        return category;
    }

    public async Task<IEnumerable<ProductCategory>> RetrieveAllAsync(CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.product_category_retrieve_all();",
            cancellationToken: cancellationToken);

        var results = await connection.QueryAsync<ProductCategory>(command);
        return results;
    }

    public async Task<ProductCategory?> UpdateAsync(
        long id,
        ProductCategory category,
        long updatedBy,
        CancellationToken cancellationToken
    )
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.product_category_update(@id, @name, @updated_by);",
            new
            {
                id,
                name = category.Name,
                updated_by = updatedBy
            },
            cancellationToken: cancellationToken);

        var updated = await connection.QuerySingleOrDefaultAsync<ProductCategory?>(command);
        return updated;
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT lineitem.product_category_delete(@id);",
            new { id },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }
}