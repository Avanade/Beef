namespace Beef.Demo.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="PersonDetail"/> validator.
    /// </summary>
    public class PersonDetailValidator : Validator<PersonDetail>
    {
        private readonly Validator<WorkHistory> _workHistoryValidator = Validator.Create<WorkHistory>()
            .HasProperty(x => x.Name, p => p.Mandatory().Common(CommonValidators.Text))
            .HasProperty(x => x.StartDate, p => p.Mandatory().CompareValue(CompareOperator.LessThanEqual, DateTime.Now, "today"))
            .HasProperty(x => x.EndDate, p => p.WhenHasValue().DependsOn(o => o.StartDate).CompareProperty(CompareOperator.GreaterThanEqual, o => o.StartDate));

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonDetailValidator"/>.
        /// </summary>
        public PersonDetailValidator(IValidatorEx<Person> personValidator)
        {
            IncludeBase(personValidator);
            Property(x => x.History).Collection(item: CollectionRuleItem.Create(_workHistoryValidator).DuplicateCheck(nameof(WorkHistory.Name)));
        }
    }
}