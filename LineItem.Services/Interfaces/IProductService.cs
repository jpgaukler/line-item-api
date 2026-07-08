using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;

namespace LineItem.Services.Interfaces;

public interface IProductService
{
    /// <summary>
    ///     Returns the active version of a product.
    /// </summary>
    /// <returns>The product or null if not found.</returns>
    public Task<Product?> RetrieveByIdAsync(long productId, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns a specific historical version of a product.
    /// </summary>
    /// <returns>The product or null if not found.</returns>
    public Task<Product?> RetrieveVersionByIdAsync(long productId, int version, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns all published products in a category.
    /// </summary>
    /// <returns>Products in the category or empty list if none found.</returns>
    public Task<IEnumerable<Product>> RetrieveByCategoryIdAsync(long categoryId, CancellationToken cancellationToken);

    /// <summary>
    ///     Text search for products across name and description.
    /// </summary>
    /// <returns>Matching products or empty list if none found.</returns>
    public Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes a product by id, along with all of its associated product versions.
    /// </summary>
    public Task DeleteAsync(long id, CancellationToken cancellationToken);
}