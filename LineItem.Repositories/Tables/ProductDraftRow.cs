using System;

namespace LineItem.Repositories.Tables;

/// <summary>
///     Mapping class for the product_draft table.
/// </summary>
internal class ProductDraftRow
{
    public long Id { get; set; }
    public long? BaseProductId { get; set; }
    public int? BaseVersion { get; set; }
    public string ProductData { get; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}