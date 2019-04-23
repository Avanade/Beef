using Beef.Demo.Common.Entities;
using Beef.Validation;

namespace Beef.Demo.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="CustomerGroup"/> validator.
    /// </summary>
    public class CustomerGroupValidator : Validator<CustomerGroup, CustomerGroupValidator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerGroupArgsValidator"/>.
        /// </summary>
        public CustomerGroupValidator()
        {
            Property(x => x.Id).Mandatory().String(20);
            Property(x => x.Company).Mandatory().IsValid();
            Property(x => x.Description).Mandatory().String(50);
        }
    }
}
