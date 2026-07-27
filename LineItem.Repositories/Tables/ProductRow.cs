using System;

namespace LineItem.Repositories.Tables;

/// <summary>
///     Mapping class for the product table.
/// </summary>
internal record ProductRow(
    long Id,
    long ProductCategoryId,
    string Name,
    string Description,
    int? ActiveVersion,
    DateTime CreatedAt,
    long CreatedBy,
    DateTime? UpdatedAt,
    long? UpdatedBy);