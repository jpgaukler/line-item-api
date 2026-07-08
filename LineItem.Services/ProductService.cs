using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

    public Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        // TODO: if product is used on a quote, prevent delete
        return _productRepository.DeleteAsync(id, cancellationToken);
    }
}