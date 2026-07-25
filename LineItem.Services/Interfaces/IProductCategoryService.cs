using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;

namespace LineItem.Services.Interfaces;

public interface IProductCategoryService
{
    /// <summary>
    ///     Creates a new product category.
    /// </summary>
    public Task<ProductCategory> CreateAsync(
        ProductCategory category,
        long createdBy,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Retrieves a product category by id.
    /// </summary>
    /// <returns>The category or null if not found.</returns>
    public Task<ProductCategory?> RetrieveByIdAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns all product categories.
    /// </summary>
    public Task<IEnumerable<ProductCategory>> RetrieveAllAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Updates a product category.
    /// </summary>
    /// <returns>The updated category or null if not found.</returns>
    public Task<ProductCategory?> UpdateAsync(
        long id,
        ProductCategory category,
        long updatedBy,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Deletes a product category by id.
    /// </summary>
    public Task DeleteAsync(long id, CancellationToken cancellationToken);
}