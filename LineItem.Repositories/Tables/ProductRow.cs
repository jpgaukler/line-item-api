using System;

namespace LineItem.Repositories.Tables;

/// <summary>
///     Mapping class for the product table.
/// </summary>
internal class ProductRow
{
    public long Id { get; set; }
    public long ProductCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? ActiveVersion { get; set; }
    public DateTime CreatedAt { get; set; }
    public long CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
}