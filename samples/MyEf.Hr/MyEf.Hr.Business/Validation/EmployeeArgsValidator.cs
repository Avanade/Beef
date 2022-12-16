namespace MyEf.Hr.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="EmployeeArgs"/> validator.
    /// </summary>
    public class EmployeeArgsValidator : Validator<EmployeeArgs>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeValidator"/> class.
        /// </summary>
        public EmployeeArgsValidator()
        {
            Property(x => x.FirstName).Common(CommonValidators.PersonName).Wildcard();
            Property(x => x.LastName).Common(CommonValidators.PersonName).Wildcard();
            Property(x => x.Genders).AreValid();
            Property(x => x.StartFrom).CompareProperty(CompareOperator.LessThanEqual, x => x.StartTo);
        }
    }
}