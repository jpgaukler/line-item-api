using System;

namespace LineItem.Repositories.Tables;

/// <summary>
///     Mapping class for the product_version table.
/// </summary>
internal record ProductVersionRow(
    long ProductId,
    int Version,
    string ProductData,
    DateTime CreatedAt,
    long CreatedBy);