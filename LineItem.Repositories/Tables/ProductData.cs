using System.Collections.Generic;
using LineItem.Models;

namespace LineItem.Repositories.Tables;

/// <summary>
///     Mapping shape for deserializing product data on Product or ProductDraft objects.
/// </summary>
internal record ProductData(
    string Name,
    string Description,
    string ProductCodeFormula,
    List<ProductInput> Inputs,
    List<ProductAdder> Adders,
    ProductPriceDictionary PriceDictionary) : IProductData;