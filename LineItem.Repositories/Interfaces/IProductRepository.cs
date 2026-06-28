using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;

namespace LineItem.Repositories.Interfaces;

public interface IProductRepository
{
    // --- Product Draft workflow ---

    /// <summary>
    ///     Creates a new draft for a new product (base_product_id = null).
    /// </summary>
    /// <param name="product"></param>
    /// <param name="productId">Optionally, the id of the base product to use (for product edits).</param>
    /// <param name="baseVersion">Optionally, the version of the base product to use (for product edits).</param>
    /// <param name="createdBy"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The created draft.</returns>
    public Task<ProductDraft> CreateDraftAsync(
        Product product,
        long? productId,
        int? baseVersion,
        long createdBy,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Overwrites the product_data on an existing draft row.
    /// </summary>
    public Task UpdateDraftAsync(long draftId, Product product, long updatedBy, CancellationToken cancellationToken);

    /// <summary>
    ///     Retrieves a product draft by id.
    /// </summary>
    /// <returns>The product draft or null if no draft is found.</returns>
    public Task<ProductDraft?> RetrieveDraftByIdAsync(long draftId, CancellationToken cancellationToken);

    /// <summary>
    ///     Publishes a product draft. If the draft has no base_product_id, creates a new product
    ///     and product_version (v1), otherwise adds a new version, and bumps active_version.
    ///     Both paths execute in a single transaction.
    /// </summary>
    /// <returns>The published product.</returns>
    public Task<Product> PublishDraftAsync(ProductDraft draft, long createdBy, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes a product draft by id.
    /// </summary>
    public Task DeleteDraftAsync(long draftId, CancellationToken cancellationToken);


    // --- Product queries ---

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
    /// <returns>The products matching the search term or empty list if no products are found.</returns>
    public Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken);
}