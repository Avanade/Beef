namespace My.Hr.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="TerminationDetail"/> validator.
    /// </summary>
    public class TerminationDetailValidator : Validator<TerminationDetail>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminationDetailValidator"/> class.
        /// </summary>
        public TerminationDetailValidator()
        {
            Property(x => x.Date).Mandatory();
            Property(x => x.Reason).Mandatory().IsValid();
        }
    }
}