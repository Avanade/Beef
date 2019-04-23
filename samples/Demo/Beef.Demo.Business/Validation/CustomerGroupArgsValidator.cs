using Beef.Demo.Common.Entities;
using Beef.Validation;

namespace Beef.Demo.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="CustomerGroupArgs"/> validator.
    /// </summary>
    public class CustomerGroupArgsValidator : Validator<CustomerGroupArgs, CustomerGroupArgsValidator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerGroupArgsValidator"/>.
        /// </summary>
        public CustomerGroupArgsValidator()
        {
            Property(x => x.Company).IsValid();
            Property(x => x.Description).String(50).Wildcard(Wildcard.MultiBasic);
        }
    }
}
