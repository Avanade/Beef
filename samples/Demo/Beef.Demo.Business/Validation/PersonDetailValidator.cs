#nullable enable

using System;
using Beef.Demo.Common.Entities;
using Beef.Validation;
using Beef.Validation.Rules;

namespace Beef.Demo.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="Person"/> validator.
    /// </summary>
    public class PersonDetailValidator : Validator<PersonDetail>
    {
        private static readonly Validator<WorkHistory> _workHistoryValidator = Validator.Create<WorkHistory>()
            .HasProperty(x => x.Name, p => p.Mandatory().Common(CommonValidators.Text))
            .HasProperty(x => x.StartDate, p => p.Mandatory().CompareValue(CompareOperator.LessThanEqual, DateTime.Now, "today"))
            .HasProperty(x => x.EndDate, p => p.WhenHasValue().DependsOn(o => o.StartDate).CompareProperty(CompareOperator.GreaterThanEqual, o => o.StartDate));

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonValidator"/>.
        /// </summary>
        public PersonDetailValidator()
        {
            IncludeBase(PersonValidator.Default);
            Property(x => x.History).Collection(item: new CollectionRuleItem<WorkHistory>(_workHistoryValidator).UniqueKeyDuplicateCheck());
        }
    }
}

#nullable restore