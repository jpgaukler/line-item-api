using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;

namespace LineItem.Services.Interfaces;

public interface IProductService
{
    // --- Draft workflow ---

    /// <summary>
    ///     Creates a new draft for a new product.
    /// </summary>
    /// <returns>The created draft.</returns>
    Task<ProductDraft> CreateDraftAsync(Product product, long createdBy, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a draft branched from the active version of an existing published product.
    /// </summary>
    /// <returns>The created draft.</returns>
    Task<ProductDraft> CreateDraftFromProductAsync(long productId, long createdBy, CancellationToken cancellationToken);

    /// <summary>
    ///     Updates an existing draft.
    /// </summary>
    Task UpdateDraftAsync(long draftId, Product product, long updatedBy, CancellationToken cancellationToken);

    /// <summary>
    ///     Retrieves a draft by id.
    /// </summary>
    /// <returns>The draft or null if not found.</returns>
    Task<ProductDraft?> RetrieveDraftByIdAsync(long draftId, CancellationToken cancellationToken);

    /// <summary>
    ///     Publishes a draft. Creates a new product if no base product exists,
    ///     otherwise adds a new version to the existing product.
    /// </summary>
    /// <returns>The published product.</returns>
    Task<Product> PublishDraftAsync(long draftId, long createdBy, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes a draft by id.
    /// </summary>
    Task DeleteDraftAsync(long draftId, CancellationToken cancellationToken);


    // --- Product queries ---

    /// <summary>
    ///     Returns the active version of a product.
    /// </summary>
    /// <returns>The product or null if not found.</returns>
    Task<Product?> RetrieveByIdAsync(long productId, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns a specific historical version of a product.
    /// </summary>
    /// <returns>The product or null if not found.</returns>
    Task<Product?> RetrieveVersionByIdAsync(long productId, int version, CancellationToken cancellationToken);

    /// <summary>
    ///     Returns all published products in a category.
    /// </summary>
    /// <returns>Products in the category or empty list if none found.</returns>
    Task<IEnumerable<Product>> RetrieveByCategoryIdAsync(long categoryId, CancellationToken cancellationToken);

    /// <summary>
    ///     Text search for products across name and description.
    /// </summary>
    /// <returns>Matching products or empty list if none found.</returns>
    Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken);
}