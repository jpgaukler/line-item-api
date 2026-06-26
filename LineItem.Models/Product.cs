using System;
using System.Collections.Generic;

namespace LineItem.Models;

/// <summary>
///     Class representing a row in the 'products' database table. Includes relational columns for querying, along
///     with JSON snapshot of the entire product definition.
/// </summary>
public class ProductVersionRow
{
    /// <summary>
    ///     Database Id.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     1-indexed version of the product.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    ///     Name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }
    public bool IsArchived { get; set; }

    /// <summary>
    ///     Immutable JSON snapshot of the entire product definition.
    /// </summary>
    public string Snapshot { get; set; } = string.Empty;

    /// <summary>
    ///     Timestamp of when the database record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Timestamp of when the database record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    ///     Id of the user who created the database record.
    /// </summary>
    public long CreatedBy { get; set; }

    /// <summary>
    ///     Id of the user who last updated the database record.
    /// </summary>
    public long? UpdatedBy { get; set; }
}

/// <summary>
///     Represents the definition of a product. This serves as a template for adding products to a quote.
/// </summary>
public class Product
{
    /// <summary>
    ///     Database Id.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Description of the product.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Formula used to generate the product code.
    /// </summary>
    public string ProductCodeFormula { get; set; } = string.Empty;

    /// <summary>
    ///     Inputs that determine form, fit, and functionality of the product.
    /// </summary>
    public List<ProductInput> Inputs { get; set; } = [];

    /// <summary>
    ///     Optional adders that can be added to the product.
    /// </summary>
    public List<ProductAdder> Adders { get; set; } = [];

    /// <summary>
    ///     Dictionary of product codes and their prices.
    /// </summary>
    public ProductPriceDictionary? PriceDictionary { get; set; }
}

/// <summary>
///     An optional feature or accessory that can be added to a product.
/// </summary>
public class ProductAdder
{
    /// <summary>
    ///     Name of the adder.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Whether the adder allows a custom user input.
    /// </summary>
    public bool AllowCustomOption { get; set; }

    /// <summary>
    ///     The index of the default option.
    /// </summary>
    public int DefaultOptionIndex { get; set; }

    /// <summary>
    ///     The options available for the adder.
    /// </summary>
    public ProductAdderOption[] Options { get; set; } = [];
}

/// <summary>
///     An option available for an adder.
/// </summary>
public class ProductAdderOption
{
    /// <summary>
    ///     The value displayed in the UI when configuring the adder.
    /// </summary>
    public string DisplayText { get; set; } = string.Empty;

    /// <summary>
    ///     The value that is added to the base product price.
    /// </summary>
    public int Price { get; set; }
}

/// <summary>
///     An input that determines the form, fit, and functionality of a product.
/// </summary>
public class ProductInput
{
    /// <summary>
    ///     Name of the input.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Whether the input allows a custom user input.
    /// </summary>
    public bool AllowCustomOption { get; set; }

    /// <summary>
    ///     The index of the default option.
    /// </summary>
    public int DefaultOptionIndex { get; set; }

    /// <summary>
    ///     The options available for the input.
    /// </summary>
    public ProductInputOption[] Options { get; set; } = [];
}

/// <summary>
///     An option available for a product input.
/// </summary>
public class ProductInputOption
{
    /// <summary>
    ///     The value displayed in the UI when configuring the input.
    /// </summary>
    private string DisplayText { get; set; } = string.Empty;

    /// <summary>
    ///     The value that is used to generate the product code.
    /// </summary>
    private string Value { get; set; } = string.Empty;
}

/// <summary>
///     A dictionary of product codes and their base prices.
/// </summary>
public class ProductPriceDictionary
{
    /// <summary>
    ///     A hash of the product code formula and possible input values that can be used to generate the product code.
    ///     This hash can be used to determine if the price dictionary is out of date,
    ///     if product inputs have changed since last generating the pricing dictionary.
    /// </summary>
    public string ProductCodeHash { get; set; }

    /// <summary>
    ///     A map of product code to base prices.
    /// </summary>
    public Dictionary<string, int> Prices { get; set; } = [];
}