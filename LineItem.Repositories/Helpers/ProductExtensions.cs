using System.Text.Json;
using System.Text.Json.Serialization;
using LineItem.Models;

namespace LineItem.Repositories.Helpers;

/// <summary>
///     Mapping class for the product_version table.
/// </summary>
internal static class ProductExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    extension(string productJson)
    {
        internal Product ToProduct()
        {
            return JsonSerializer.Deserialize<Product>(productJson, _jsonOptions)!;
        }
    }

    extension(Product product)
    {
        internal string ToJson()
        {
            return JsonSerializer.Serialize(product, _jsonOptions);
        }
    }
}