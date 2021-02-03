#nullable enable

using Beef.Demo.Common.Entities;
using Beef.Validation;
using System;

namespace Beef.Demo.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="Person"/> validator.
    /// </summary>
    public class PersonValidator : Validator<Person>
    {
        private static readonly Validator<Address> _addressValidator = Validator.Create<Address>()
            .HasProperty(x => x.Street, p => p.Mandatory().Common(CommonValidators.Text))
            .HasProperty(x => x.City, p => p.Mandatory().Common(CommonValidators.Text));

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonValidator"/>.
        /// </summary>
        public PersonValidator()
        {
            Property(x => x.FirstName).Mandatory().Common(CommonValidators.Text);
            Property(x => x.LastName).Mandatory().Common(CommonValidators.Text);
            Property(x => x.Gender).Mandatory().IsValid();
            Property(x => x.EyeColor).IsValid();
            Property(x => x.Birthday).Mandatory().CompareValue(CompareOperator.LessThanEqual, _ => DateTime.Now, _ => "Today");
            Property(x => x.Address).Entity(_addressValidator);
        }
    }
}

#nullable restore