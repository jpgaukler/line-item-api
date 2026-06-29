using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Exceptions;
using LineItem.Models;
using LineItem.Repositories.Interfaces;
using LineItem.Services.Interfaces;

namespace LineItem.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    // --- Draft workflow ---

    public Task<ProductDraft> CreateDraftAsync(Product product, long createdBy, CancellationToken cancellationToken)
    {
        return _productRepository.CreateDraftAsync(product, null, createdBy, cancellationToken);
    }

    public async Task<ProductDraft> CreateDraftFromProductAsync(
        long productId,
        long createdBy,
        CancellationToken cancellationToken
    )
    {
        var product = await _productRepository.RetrieveByIdAsync(productId, cancellationToken)
                      ?? throw new NotFoundException($"Product with id {productId} not found.");

        var productDraft = await _productRepository.CreateDraftAsync(
            product,
            product.Id,
            createdBy,
            cancellationToken);

        return productDraft;
    }

    public async Task UpdateDraftAsync(
        long draftId,
        Product product,
        long updatedBy,
        CancellationToken cancellationToken
    )
    {
        if (await _productRepository.RetrieveDraftByIdAsync(draftId, cancellationToken) is null)
            throw new NotFoundException($"Draft with id {draftId} not found.");

        await _productRepository.UpdateDraftAsync(draftId, product, updatedBy, cancellationToken);
    }

    public Task<ProductDraft?> RetrieveDraftByIdAsync(long draftId, CancellationToken cancellationToken)
    {
        return _productRepository.RetrieveDraftByIdAsync(draftId, cancellationToken);
    }

    public async Task<Product> PublishDraftAsync(long draftId, long createdBy, CancellationToken cancellationToken)
    {
        // TODO: add validation for the product

        var draft = await _productRepository.RetrieveDraftByIdAsync(draftId, cancellationToken)
                    ?? throw new NotFoundException($"Draft with id {draftId} not found.");

        var product = await _productRepository.PublishDraftAsync(draft, createdBy, cancellationToken);
        return product;
    }

    public Task DeleteDraftAsync(long draftId, CancellationToken cancellationToken)
    {
        return _productRepository.DeleteDraftAsync(draftId, cancellationToken);
    }


    // --- Product queries ---

    public Task<Product?> RetrieveByIdAsync(long productId, CancellationToken cancellationToken)
    {
        return _productRepository.RetrieveByIdAsync(productId, cancellationToken);
    }

    public Task<Product?> RetrieveVersionByIdAsync(long productId, int version, CancellationToken cancellationToken)
    {
        return _productRepository.RetrieveVersionByIdAsync(productId, version, cancellationToken);
    }

    public Task<IEnumerable<Product>> RetrieveByCategoryIdAsync(long categoryId, CancellationToken cancellationToken)
    {
        return _productRepository.RetrieveByCategoryIdAsync(categoryId, cancellationToken);
    }

    public Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken)
    {
        return _productRepository.SearchAsync(searchTerm, cancellationToken);
    }
}