using System;

namespace LineItem.Models;

/// <summary>
///     A draft of a product. This could be a new product or a product that is being modified.
///     The draft is mutable until it is published, at which point it becomes a <see cref="Product" /> which is immutable.
/// </summary>
public class ProductDraft
{
    /// <summary>
    ///     Database Id.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     The base product that this draft is based on, if this draft is based on another product.
    /// </summary>
    public long? BaseProductId { get; set; }

    /// <summary>
    ///     The version of the base product that this draft is based on, if this draft is based on another product.
    /// </summary>
    public int? BaseVersion { get; set; }

    /// <summary>
    ///     Full product definition.
    /// </summary>
    public Product Product { get; set; } = null!;

    /// <summary>
    ///     Timestamp of when the database record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Timestamp of when the database record was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    ///     Id of the user who created the database record.
    /// </summary>
    public long CreatedBy { get; set; }

    /// <summary>
    ///     Id of the user who last updated the database record.
    /// </summary>
    public long? UpdatedBy { get; set; }
}