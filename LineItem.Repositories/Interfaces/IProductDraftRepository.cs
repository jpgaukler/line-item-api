using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;

namespace LineItem.Repositories.Interfaces;

public interface IProductDraftRepository
{
    /// <summary>
    ///     Creates a new draft for a new product.
    /// </summary>
    /// <returns>The created draft.</returns>
    public Task<ProductDraft> CreateAsync(
        ProductDraft draft,
        long createdBy,
        CancellationToken cancellationToken
    );

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
    public Task UpdateAsync(
        long draftId,
        ProductDraft draft,
        long updatedBy,
        CancellationToken cancellationToken
    );

    /// <summary>
    ///     Retrieves a product draft by id.
    /// </summary>
    /// <returns>The product draft or null if no draft is found.</returns>
    public Task<ProductDraft?> RetrieveByIdAsync(long draftId, CancellationToken cancellationToken);

    /// <summary>
    ///     Publishes a product draft. If the draft has no base_product_id, creates a new product
    ///     and product_version (v1), otherwise adds a new version, and bumps active_version.
    ///     Both paths execute in a single transaction.
    /// </summary>
    public Task PublishAsync(ProductDraft draft, long createdBy, CancellationToken cancellationToken);

    /// <summary>
    ///     Deletes a product draft by id.
    /// </summary>
    public Task DeleteAsync(long draftId, CancellationToken cancellationToken);
}