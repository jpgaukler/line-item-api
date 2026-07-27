using System;

namespace LineItem.Repositories.Tables;

/// <summary>
///     Mapping class for the product_draft table.
/// </summary>
internal record ProductDraftRow(
    long Id,
    long? BaseProductId,
    int? BaseVersion,
    long ProductCategoryId,
    string ProductData,
    DateTime CreatedAt,
    long CreatedBy,
    DateTime? UpdatedAt,
    long? UpdatedBy);