using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;

namespace LineItem.Repositories.Interfaces;

public interface IProductRepository
{
    /// <summary>
    ///     Returns the active version of a product.
    /// </summary>
    /// <returns>The product or null if no product is found.</returns>
    public Task<Product?> RetrieveByIdAsync(long productId, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns a specific historical version of a product.
    /// </summary>
    /// <returns>The product or null if no product is found.</returns>
    public Task<Product?> RetrieveVersionByIdAsync(long productId, int version, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns all published products (active version) in a category.
    /// </summary>
    /// <returns>The products in the category or empty list if no products are found.</returns>
    public Task<IEnumerable<Product>> RetrieveByCategoryIdAsync(long categoryId, CancellationToken cancellationToken);

    /// <summary>
    ///     Text search for products across name and description.
    /// </summary>
    /// <returns>Matching products or empty list if none found.</returns>
    public Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken);
}