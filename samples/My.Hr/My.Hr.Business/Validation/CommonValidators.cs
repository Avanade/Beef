using Beef.Validation;
using System.ComponentModel.DataAnnotations;

namespace My.Hr.Business.Validation
{
    /// <summary>
    /// Provides common validator capabilities.
    /// </summary>
    public static class CommonValidators
    {
        private static readonly EmailAddressAttribute _emailValidator = new EmailAddressAttribute();

        /// <summary>
        /// Provides a common person's name validator, ensure max length is 100.
        /// </summary>
        public static CommonValidator<string?> PersonName = CommonValidator.Create<string?>(cv => cv.String(100));

        /// <summary>
        /// Provides a common address's street validator, ensure max length is 100.
        /// </summary>
        public static CommonValidator<string?> Street = CommonValidator.Create<string?>(cv => cv.String(100));

        /// <summary>
        /// Provides a common email validator, ensure max length is 250, is all lowercase, and use validator.
        /// </summary>
        public static CommonValidator<string?> Email = CommonValidator.Create<string?>(cv => cv.String(250).Override(v => v.Value!.ToLowerInvariant()).Must(v => _emailValidator.IsValid(v.Value)));

        /// <summary>
        /// Provides a common phone number validator, just length, but could be regex or other.
        /// </summary>
        public static CommonValidator<string?> PhoneNo = CommonValidator.Create<string?>(cv => cv.String(50));
    }
}