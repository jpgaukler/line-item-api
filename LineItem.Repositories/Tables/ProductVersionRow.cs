using System;

namespace LineItem.Repositories.Tables;

/// <summary>
///     Mapping class for the product_version table.
/// </summary>
internal class ProductVersionRow
{
    public long ProductId { get; set; }
    public int Version { get; set; }
    public string ProductData { get; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
}