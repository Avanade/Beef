using Cdr.Banking.Business.Entities;
using CoreEx;
using CoreEx.Validation;

namespace Cdr.Banking.Business.Validation
{
    /// <summary>
    /// Provides common validators.
    /// </summary>
    public static class Validators
    {
        /// <summary>
        /// Validates an <see cref="Account.Id"/> value to ensure that the executing user is authorized to access.
        /// </summary>
        public static CommonValidator<string?> AccountId => CommonValidator.Create<string?>(v => v.Custom(ctx =>
        {
            if (ctx.Value == null || !ExecutionContext.Current.Accounts.Contains(ctx.Value))
                throw new AuthorizationException();
        }));
    }
}