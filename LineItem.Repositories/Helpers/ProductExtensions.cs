using System.Text.Json;
using System.Text.Json.Serialization;
using LineItem.Models;
using LineItem.Repositories.Tables;

namespace LineItem.Repositories.Helpers;

internal static class ProductExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    extension(Product product)
    {
        internal ProductData ToProductData()
        {
            return new ProductData
            (
                product.Name,
                product.Description,
                product.ProductCodeFormula,
                product.Inputs,
                product.Adders,
                product.PriceDictionary
            );
        }
    }

    extension(ProductDraft productDraft)
    {
        internal ProductData ToProductData()
        {
            return new ProductData
            (
                productDraft.Name,
                productDraft.Description,
                productDraft.ProductCodeFormula,
                productDraft.Inputs,
                productDraft.Adders,
                productDraft.PriceDictionary
            );
        }
    }

    extension(ProductData productData)
    {
        internal string ToJson()
        {
            return JsonSerializer.Serialize(productData, _jsonOptions);
        }
    }

    extension(string json)
    {
        internal ProductData ToProductData()
        {
            return JsonSerializer.Deserialize<ProductData>(json, _jsonOptions)!;
        }
    }
}