using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using LineItem.Models;

namespace LineItem.Repositories.Helpers;

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

/// <summary>
///     Extension methods for converting ProductData to and from JSON.
/// </summary>
internal static class ProductDataExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    internal static string ToJson(this ProductData productData)
    {
        return JsonSerializer.Serialize(productData, JsonOptions);
    }

    internal static ProductData ToProductData(this string json)
    {
        return JsonSerializer.Deserialize<ProductData>(json, JsonOptions)!;
    }
}