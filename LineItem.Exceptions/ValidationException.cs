namespace LineItem.Exceptions;

/// <summary>
///     Thrown when one or more validation rules fail.
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string error) : this([error])
    {
    }

    public ValidationException(List<string> errors) : base(BuildMessage(errors))
    {
        Errors = errors;
    }

    /// <summary>
    ///     The list of validation error messages.
    /// </summary>
    public List<string> Errors { get; }

    private static string BuildMessage(List<string> errors)
    {
        return
            $"Validation failed with {errors.Count} error(s):{Environment.NewLine}{string.Join(Environment.NewLine, errors)}";
    }
}