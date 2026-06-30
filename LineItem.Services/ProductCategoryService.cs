using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Exceptions;
using LineItem.Repositories.Interfaces;
using LineItem.Services.Interfaces;

namespace LineItem.Services;

public class ProductCategoryService : IProductCategoryService
{
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly IProductRepository _productRepository;

    public ProductCategoryService(
        IProductCategoryRepository productCategoryRepository,
        IProductRepository productRepository
    )
    {
        _productCategoryRepository = productCategoryRepository;
        _productRepository = productRepository;
    }

    public async Task<ProductCategory> CreateAsync(
        ProductCategory category,
        long createdBy,
        CancellationToken cancellationToken
    )
    {
        ValidateCategory(category);
        return await _productCategoryRepository.CreateAsync(category, createdBy, cancellationToken);
    }

    public Task<ProductCategory?> RetrieveByIdAsync(long id, CancellationToken cancellationToken)
    {
        return _productCategoryRepository.RetrieveByIdAsync(id, cancellationToken);
    }

    public Task<IEnumerable<ProductCategory>> RetrieveAllAsync(CancellationToken cancellationToken)
    {
        return _productCategoryRepository.RetrieveAllAsync(cancellationToken);
    }

    public Task<ProductCategory?> UpdateAsync(
        long id,
        ProductCategory category,
        long updatedBy,
        CancellationToken cancellationToken
    )
    {
        ValidateCategory(category);
        return _productCategoryRepository.UpdateAsync(id, category, updatedBy, cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        var products = await _productRepository.RetrieveByCategoryIdAsync(id, cancellationToken);
        if (products.Any())
            throw new ValidationException("A category with associated products can not be deleted.");

        await _productCategoryRepository.DeleteAsync(id, cancellationToken);
    }

    private static void ValidateCategory(ProductCategory category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
            throw new ValidationException("Product category name is required.");

        if (category.Name.Length > 100)
            throw new ValidationException("Product category name can not exceed 100 characters.");
    }
}