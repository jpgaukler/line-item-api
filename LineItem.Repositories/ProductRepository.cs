using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using LineItem.Models;
using LineItem.Repositories.Helpers;
using LineItem.Repositories.Interfaces;
using LineItem.Repositories.Options;
using Microsoft.Extensions.Options;

namespace LineItem.Repositories;

public class ProductRepository : RepositoryBase, IProductRepository
{
    public ProductRepository(IOptions<DatabaseOptions> options) : base(options)
    {
    }

    public async Task<Product?> RetrieveByIdAsync(long productId, CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.product_retrieve_active_version_by_id(@id);",
            new { id = productId },
            cancellationToken: cancellationToken);

        var result = await connection.QuerySingleOrDefaultAsync<ProductVersionDetailRow>(command);

        return result is null ? null : MapProductVersionDetailRow(result);
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
            "SELECT * FROM lineitem.product_retrieve_version_by_id(@product_id, @version);",
            new
            {
                product_id = productId,
                version
            },
            cancellationToken: cancellationToken);

        var result = await connection.QuerySingleOrDefaultAsync<ProductVersionDetailRow>(command);

        return result is null ? null : MapProductVersionDetailRow(result);
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

        var results = await connection.QueryAsync<ProductVersionDetailRow>(command);
        return results.Select(MapProductVersionDetailRow);
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT * FROM lineitem.product_search(@search_term);",
            new { search_term = searchTerm },
            cancellationToken: cancellationToken);

        var results = await connection.QueryAsync<ProductVersionDetailRow>(command);
        return results.Select(MapProductVersionDetailRow);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync(cancellationToken);

        var command = new CommandDefinition(
            "SELECT lineitem.product_delete(@id);",
            new { id },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    private static Product MapProductVersionDetailRow(ProductVersionDetailRow row)
    {
        var productData = row.ProductData.ToProductData();

        return new Product
        {
            Id = row.Id,
            Version = row.Version,
            ProductCategoryId = row.ProductCategoryId,
            Name = productData.Name,
            Description = productData.Description,
            ProductCodeFormula = productData.ProductCodeFormula,
            Inputs = productData.Inputs,
            Adders = productData.Adders,
            PriceDictionary = productData.PriceDictionary,
            CreatedAt = row.CreatedAt,
            CreatedBy = row.CreatedBy
        };
    }

    /// <summary>
    ///     Mapping shape for the product_version_detail view.
    /// </summary>
    private record ProductVersionDetailRow(
        long Id,
        long ProductCategoryId,
        string Name,
        string Description,
        int ActiveVersion,
        int Version,
        string ProductData,
        DateTime CreatedAt,
        long CreatedBy);
}