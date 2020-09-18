using Beef.Validation;
using System;
using Company.AppName.Common.Entities;

namespace Company.AppName.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="Person"/> validator.
    /// </summary>
    public class PersonValidator : Validator<Person, PersonValidator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonValidator"/> class.
        /// </summary>
        public PersonValidator()
        {
            Property(x => x.FirstName).Mandatory().String(100);
            Property(x => x.LastName).Mandatory().String(100);
            Property(x => x.Gender).Mandatory().IsValid();
            Property(x => x.Birthday).Mandatory().CompareValue(CompareOperator.LessThanEqual, _ => DateTime.UtcNow, _ => "Today");
        }
    }
}