using System;

namespace LineItem.Repositories.Tables;

/// <summary>
///     Mapping class for the product_active_version view.
/// </summary>
internal record ProductActiveVersionRow(
    long Id,
    long ProductCategoryId,
    string Name,
    string Description,
    int Version,
    string ProductData,
    DateTime CreatedAt,
    long CreatedBy);