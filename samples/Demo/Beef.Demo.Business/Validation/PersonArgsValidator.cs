﻿#nullable enable

namespace Beef.Demo.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="PersonArgs"/> validator.
    /// </summary>
    public class PersonArgsValidator : Validator<PersonArgs>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonArgsValidator"/>.
        /// </summary>
        public PersonArgsValidator()
        {
            Property(x => x.FirstName).Common(CommonValidators.Text);
            Property(x => x.LastName).Common(CommonValidators.Text);
            Property(x => x.Genders).AreValid();
            Property(x => x.OrderBy).Enum();
        }
    }
}

#nullable restore