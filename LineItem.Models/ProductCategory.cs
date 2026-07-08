using System;

/// <summary>
///     Represents a category used to organize products.
/// </summary>
public class ProductCategory
{
    /// <summary>
    ///     Database Id.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     Name of the category.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     When the category was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    ///     Id of the user who created the category.
    /// </summary>
    public long CreatedBy { get; set; }

    /// <summary>
    ///     When the category was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    ///     Id of the user who last updated the category.
    /// </summary>
    public long? UpdatedBy { get; set; }
}