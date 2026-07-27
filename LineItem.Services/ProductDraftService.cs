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

public class ProductDraftService : IProductDraftService
{
    private readonly IProductCategoryRepository _productCategoryRepository;
    private readonly IProductDraftRepository _productDraftRepository;
    private readonly IProductRepository _productRepository;

    public ProductDraftService(
        IProductDraftRepository productDraftRepository,
        IProductRepository productRepository,
        IProductCategoryRepository productCategoryRepository
    )
    {
        _productDraftRepository = productDraftRepository;
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
    }

    public Task<ProductDraft> CreateAsync(
        ProductDraft draft,
        long createdBy,
        CancellationToken cancellationToken
    )
    {
        return _productDraftRepository.CreateAsync(draft, createdBy, cancellationToken);
    }

    public async Task<ProductDraft> CreateFromProductAsync(
        long productId,
        long createdBy,
        CancellationToken cancellationToken
    )
    {
        if (await _productRepository.RetrieveByIdAsync(productId, cancellationToken) is null)
            throw new NotFoundException($"Product with id {productId} not found.");

        var productDraft = await _productDraftRepository.CreateFromProductAsync(
            productId,
            createdBy,
            cancellationToken);

        return productDraft;
    }

    public async Task UpdateAsync(
        long draftId,
        ProductDraft draft,
        long updatedBy,
        CancellationToken cancellationToken
    )
    {
        if (await _productDraftRepository.RetrieveByIdAsync(draftId, cancellationToken) is null)
            throw new NotFoundException($"Draft with id {draftId} not found.");

        await _productDraftRepository.UpdateAsync(draftId, draft, updatedBy, cancellationToken);
    }

    public Task<ProductDraft?> RetrieveByIdAsync(long draftId, CancellationToken cancellationToken)
    {
        return _productDraftRepository.RetrieveByIdAsync(draftId, cancellationToken);
    }

    public async Task<Product> PublishAsync(long draftId, long createdBy, CancellationToken cancellationToken)
    {
        // TODO: ADD VALIDATION FOR THE PRODUCT
        // DO NOT DO THESE YET:
        // product code formula must be valid
        // product price dictionary hash must match the hash of the product inputs
        // product price dictionary must not have duplicate product codes

        var draft = await _productDraftRepository.RetrieveByIdAsync(draftId, cancellationToken)
                    ?? throw new NotFoundException($"Draft with id {draftId} not found.");

        await ValidateProductDraftAsync(draft, cancellationToken);

        var product = await _productDraftRepository.PublishAsync(draft, createdBy, cancellationToken);

        return product;
    }

    public Task DeleteAsync(long draftId, CancellationToken cancellationToken)
    {
        return _productDraftRepository.DeleteAsync(draftId, cancellationToken);
    }


    private async Task ValidateProductDraftAsync(ProductDraft draft, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        // Category
        var category = await _productCategoryRepository.RetrieveByIdAsync(draft.ProductCategoryId, cancellationToken);
        if (category is null)
            errors.Add($"Product category with id {draft.ProductCategoryId} does not exist.");

        // Name
        if (string.IsNullOrWhiteSpace(draft.Name))
            errors.Add("Product name is required.");
        else if (draft.Name.Length > 100)
            errors.Add("Product name can not exceed 100 characters.");

        // Description
        if (string.IsNullOrWhiteSpace(draft.Description))
            errors.Add("Product description is required.");
        else if (draft.Description.Length > 500)
            errors.Add("Product description must be 500 characters or less.");

        // Inputs
        var duplicateInputNames = draft.Inputs
            .GroupBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateInputNames.Count > 0)
            errors.Add($"Product inputs contain duplicate names: {string.Join(", ", duplicateInputNames)}.");

        foreach (var input in draft.Inputs)
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

                if (string.IsNullOrWhiteSpace(option.Value))
                    errors.Add($"Input '{input.Name}' option {i} requires a value.");
                else if (option.Value.Length > 20)
                    errors.Add($"Input '{input.Name}' option {i} value must be 20 characters or less.");
            }
        }

        // Adders
        var duplicateAdderNames = draft.Adders
            .GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateAdderNames.Count > 0)
            errors.Add($"Product adders contain duplicate names: {string.Join(", ", duplicateAdderNames)}.");

        foreach (var adder in draft.Adders)
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

                if (option.Price < 0)
                    errors.Add($"Adder '{adder.Name}' price {i} cannot be negative.");
            }
        }

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}