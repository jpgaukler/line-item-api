using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;

namespace LineItem.Services.Interfaces;

public interface IProductDraftService
{
    // --- Draft workflow ---

    /// <summary>
    ///     Creates a new draft for a new product.
    /// </summary>
    /// <returns>The created draft.</returns>
    public Task<ProductDraft> CreateAsync(Product product, long createdBy, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a draft branched from the active version of an existing published product.
    /// </summary>
    /// <returns>The created draft.</returns>
    public Task<ProductDraft> CreateFromProductAsync(
        long productId,
        long createdBy,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Updates an existing draft.
    /// </summary>
    public Task UpdateAsync(long id, Product product, long updatedBy, CancellationToken cancellationToken);

    /// <summary>
    ///     Retrieves a draft by id.
    /// </summary>
    /// <returns>The draft or null if not found.</returns>
    public Task<ProductDraft?> RetrieveByIdAsync(long id, CancellationToken cancellationToken);

    /// <summary>
    ///     Publishes a draft. Creates a new product if no base product exists,
    ///     otherwise adds a new version to the existing product.
    /// </summary>
    /// <returns>The published product.</returns>
    public Task<Product> PublishAsync(long id, long createdBy, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes a draft by id.
    /// </summary>
    public Task DeleteAsync(long id, CancellationToken cancellationToken);
}