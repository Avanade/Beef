#nullable enable

using Beef.Demo.Common.Entities;
using Beef.Validation;
using Beef.Validation.Rules;
using System;

namespace Beef.Demo.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="Person"/> validator.
    /// </summary>
    public class PersonValidator : Validator<Person>
    {
        public static readonly Validator<Address> _addressValidator = Validator.Create<Address>()
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
            Property(x => x.Metadata).Dictionary(item: DictionaryRuleItem.Create<string?, string?>(
                key: Validator.CreateCommon<string?>(r => r.Text("Gender").Mandatory().RefDataCode().As<Gender>()),
                value: Validator.CreateCommon<string?>(r => r.Text("Description").Mandatory().String(10))));
        }
    }
}

#nullable restore