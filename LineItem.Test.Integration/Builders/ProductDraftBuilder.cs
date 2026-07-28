using System.Collections.Generic;
using System.Linq;
using LineItem.Models;

namespace LineItem.Test.Integration.Builders;

/// <summary>
///     Helper class for building test ProductDrafts. Default provides a simple baseline,
///     and allows customization by chaining methods.
/// </summary>
public class ProductDraftBuilder
{
    private readonly List<ProductAdder> _adders = [];
    private readonly List<ProductInput> _inputs = [];
    private string _description = "A test product description.";
    private string _name = "Test Product";
    private long _productCategoryId;
    private string _productCodeFormula = "=W{Width}-H{Height}";

    public static ProductDraftBuilder Default()
    {
        return new ProductDraftBuilder()
            .WithInput("Width", ["24 inches|24", "36 inches|36", "48 inches|48", "60 inches|60", "72 inches|72"])
            .WithInput("Height", ["48 inches|48", "60 inches|60", "72 inches|72", "84 inches|84", "96 inches|96"])
            .WithInput("Color", ["White|white", "Bronze|bronze", "Black|black", "Tan|tan"])
            .WithAdder("Screen", [("None", 0), ("Standard", 50), ("Solar", 120)])
            .WithAdder("Hardware Upgrade", [("Standard", 0), ("Premium", 85), ("Designer", 175)])
            .WithAdder("Tempered Glass Upgrade", [("None", 0), ("Tempered", 150)]);
    }

    public ProductDraftBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public ProductDraftBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ProductDraftBuilder WithCategoryId(long categoryId)
    {
        _productCategoryId = categoryId;
        return this;
    }

    public ProductDraftBuilder WithProductCodeFormula(string formula)
    {
        _productCodeFormula = formula;
        return this;
    }

    public ProductDraftBuilder WithInput(string name, string[] options, bool allowCustom = false, int defaultIndex = 0)
    {
        _inputs.Add(new ProductInput
        {
            Name = name,
            AllowCustomOption = allowCustom,
            DefaultOptionIndex = defaultIndex,
            Options = options.Select(o =>
            {
                var parts = o.Split('|');
                return new ProductInputOption
                {
                    DisplayText = parts[0],
                    Value = parts.Length > 1 ? parts[1] : parts[0]
                };
            }).ToArray()
        });
        return this;
    }

    public ProductDraftBuilder WithAdder(
        string name,
        (string displayText, int price)[] options,
        bool allowCustom = false,
        int defaultIndex = 0
    )
    {
        _adders.Add(new ProductAdder
        {
            Name = name,
            AllowCustomOption = allowCustom,
            DefaultOptionIndex = defaultIndex,
            Options = options.Select(o => new ProductAdderOption
            {
                DisplayText = o.displayText,
                Price = o.price
            }).ToArray()
        });
        return this;
    }

    public ProductDraftBuilder WithoutInputs()
    {
        _inputs.Clear();
        return this;
    }

    public ProductDraftBuilder WithoutAdders()
    {
        _adders.Clear();
        return this;
    }

    public CreateProductDraftRequest BuildCreateRequest()
    {
        return new CreateProductDraftRequest(
            _productCategoryId,
            _name,
            _description,
            _productCodeFormula,
            _inputs,
            _adders,
            new ProductPriceDictionary()
        );
    }

    public UpdateProductDraftRequest BuildUpdateRequest()
    {
        return new UpdateProductDraftRequest(
            _productCategoryId,
            _name,
            _description,
            _productCodeFormula,
            _inputs,
            _adders,
            new ProductPriceDictionary()
        );
    }
}