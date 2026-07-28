using System;
using System.Collections.Generic;

namespace LineItem.Models;

/// <summary>
///     A draft of a product. This could be a new product or a product that is being modified.
///     The draft is mutable until it is published, at which point it becomes a <see cref="Product" /> which is immutable.
/// </summary>
public class ProductDraft : IProductData
{
    /// <summary>
    ///     Database Id.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     The base product that this draft is based on, if this draft is based on another product.
    /// </summary>
    public long? BaseProductId { get; set; }

    /// <summary>
    ///     The version of the base product that this draft is based on, if this draft is based on another product.
    /// </summary>
    public int? BaseVersion { get; set; }

    /// <summary>
    ///     Id of the product category that the product belongs to.
    /// </summary>
    public long ProductCategoryId { get; set; }

    /// <summary>
    ///     Timestamp of when the database record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Id of the user who created the database record.
    /// </summary>
    public long CreatedBy { get; set; }

    /// <summary>
    ///     Timestamp of when the database record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    ///     Id of the user who last updated the database record.
    /// </summary>
    public long? UpdatedBy { get; set; }

    /// <inheritdoc />
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc />
    public string Description { get; set; } = string.Empty;

    /// <inheritdoc />
    public string ProductCodeFormula { get; set; } = string.Empty;

    /// <inheritdoc />
    public List<ProductInput> Inputs { get; set; } = [];

    /// <inheritdoc />
    public List<ProductAdder> Adders { get; set; } = [];

    /// <inheritdoc />
    public ProductPriceDictionary PriceDictionary { get; set; } = new();
}

/// <summary>
///     Represents a request to create a ProductDraft for a new product (not a new version of an existing product).
/// </summary>
public record CreateProductDraftRequest(
    long ProductCategoryId,
    string Name,
    string Description,
    string ProductCodeFormula,
    List<ProductInput> Inputs,
    List<ProductAdder> Adders,
    ProductPriceDictionary PriceDictionary
) : IProductData;

/// <summary>
///     Represents a request to update a ProductDraft.
/// </summary>
public record UpdateProductDraftRequest(
    long ProductCategoryId,
    string Name,
    string Description,
    string ProductCodeFormula,
    List<ProductInput> Inputs,
    List<ProductAdder> Adders,
    ProductPriceDictionary PriceDictionary
) : IProductData;