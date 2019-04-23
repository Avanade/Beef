using Beef.Validation;

namespace Beef.Demo.Business.Validation
{
    /// <summary>
    /// Defines the common/shared validators.
    /// </summary>
    public static class CommonValidators
    {
        /// <summary>
        /// Represents the standard text validator (max length of 50).
        /// </summary>
        public static readonly CommonValidator<string> Text = CommonValidator<string>.Create(v => v.String(50));
    }
}
