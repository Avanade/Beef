using Beef.Demo.Common.Entities;
using Beef.Validation;

namespace Beef.Demo.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="ProductArgs"/> validator.
    /// </summary>
    public class ProductArgsValidator : Validator<ProductArgs>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonArgsValidator"/>.
        /// </summary>
        public ProductArgsValidator()
        {
            Property(x => x.Name).Common(CommonValidators.Text).Wildcard();
            Property(x => x.Description).Common(CommonValidators.Text).Wildcard();
        }
    }
}
