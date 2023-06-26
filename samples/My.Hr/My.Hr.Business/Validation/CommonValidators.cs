namespace My.Hr.Business.Validation;

/// <summary>
/// Provides common validator capabilities.
/// </summary>
public static class CommonValidators
{
    /// <summary>
    /// Provides a common person's name validator, ensure max length is 100.
    /// </summary>
    public static CommonValidator<string?> PersonName { get; } = CommonValidator.Create<string?>(cv => cv.String(100));

    /// <summary>
    /// Provides a common address's street validator, ensure max length is 100.
    /// </summary>
    public static CommonValidator<string?> Street { get; } = CommonValidator.Create<string?>(cv => cv.String(100));

    /// <summary>
    /// Provides a common phone number validator, just length, but could be regex or other.
    /// </summary>
    public static CommonValidator<string?> PhoneNo { get; } = CommonValidator.Create<string?>(cv => cv.String(50));
}