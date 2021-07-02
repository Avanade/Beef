using Beef.Validation;
using Company.AppName.Business.Entities;

namespace Company.AppName.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="PersonArgs"/> validator.
    /// </summary>
    public class PersonArgsValidator : Validator<PersonArgs>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonArgsValidator"/> class.
        /// </summary>
        public PersonArgsValidator()
        {
            Property(x => x.FirstName).String(100).Wildcard();
            Property(x => x.LastName).String(100).Wildcard();
            Property(x => x.Genders).AreValid();
        }
    }
}