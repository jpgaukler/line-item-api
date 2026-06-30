using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Exceptions;
using LineItem.Models;
using LineItem.Repositories.Interfaces;
using LineItem.Services.Interfaces;
using ValidationException = LineItem.Exceptions.ValidationException;

namespace LineItem.Services;

public class ProductService : IProductService
{
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository, IProductCategoryRepository productCategoryRepository)
    {
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
    }

    // --- Draft workflow ---

    public Task<ProductDraft> CreateDraftAsync(Product product, long createdBy, CancellationToken cancellationToken)
    {
        return _productRepository.CreateDraftAsync(product, createdBy, cancellationToken);
    }

    public async Task<ProductDraft> CreateDraftFromProductAsync(
        long productId,
        long createdBy,
        CancellationToken cancellationToken
    )
    {
        if (await _productRepository.RetrieveByIdAsync(productId, cancellationToken) is null)
            throw new NotFoundException($"Product with id {productId} not found.");

        var productDraft = await _productRepository.CreateDraftFromProductAsync(
            productId,
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
        // TODO: ADD VALIDATION FOR THE PRODUCT
        // DO NOT DO THESE YET:
        // product code formula must be valid
        // product price dictionary hash must match the hash of the product inputs
        // product price dictionary must not have duplicate product codes

        var draft = await _productRepository.RetrieveDraftByIdAsync(draftId, cancellationToken)
                    ?? throw new NotFoundException($"Draft with id {draftId} not found.");

        await ValidateProductAsync(draft.Product, cancellationToken);

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


    // --- Validators ---

    private async Task ValidateProductAsync(Product product, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        // Category
        var category = await _productCategoryRepository.RetrieveByIdAsync(product.ProductCategoryId, cancellationToken);
        if (category is null)
            errors.Add($"Product category with id {product.ProductCategoryId} does not exist.");

        // Name
        if (string.IsNullOrWhiteSpace(product.Name))
            errors.Add("Product name is required.");
        else if (product.Name.Length > 100)
            errors.Add("Product name can not exceed 100 characters.");

        // Description
        if (string.IsNullOrWhiteSpace(product.Description))
            errors.Add("Product description is required.");
        else if (product.Description.Length > 500)
            errors.Add("Product description must be 500 characters or less.");

        // Inputs
        var duplicateInputNames = product.Inputs
            .GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateInputNames.Count > 0)
            errors.Add($"Product inputs contain duplicate names: {string.Join(", ", duplicateInputNames)}.");


        foreach (var input in product.Inputs)
        {
            if (input.DefaultOptionIndex < 0 || input.DefaultOptionIndex >= input.Options.Length)
                errors.Add($"Input '{input.Name}' default option index {input.DefaultOptionIndex} is out of range.");

            for (var i = 0; i < input.Options.Length; i++)
            {
                var option = input.Options[i];
                if (string.IsNullOrWhiteSpace(option.DisplayText))
                    errors.Add($"Input '{input.Name}' option {i} requires display text.");
                else if (option.DisplayText.Length > 500)
                    errors.Add($"Input '{input.Name}' option {i} display text must be 500 characters or less.");
            }
        }

        // Adders
        var duplicateAdderNames = product.Adders
            .GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateAdderNames.Count > 0)
            errors.Add($"Product adders contain duplicate names: {string.Join(", ", duplicateAdderNames)}.");

        foreach (var adder in product.Adders)
        {
            if (adder.DefaultOptionIndex < 0 || adder.DefaultOptionIndex >= adder.Options.Length)
                errors.Add($"Adder '{adder.Name}' default option index {adder.DefaultOptionIndex} is out of range.");

            for (var i = 0; i < adder.Options.Length; i++)
            {
                var option = adder.Options[i];

                if (string.IsNullOrWhiteSpace(option.DisplayText))
                    errors.Add($"Adder '{adder.Name}' option {i} requires display text.");
                else if (option.DisplayText.Length > 500)
                    errors.Add($"Adder '{adder.Name}' option {i} display text must be 500 characters or less.");
            }
        }

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}