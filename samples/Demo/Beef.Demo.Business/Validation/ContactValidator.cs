namespace Beef.Demo.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="Contact"/> validator.
    /// </summary>
    public class ContactValidator : Validator<Contact>
    {
        private readonly CommonValidator<string> _commsKey = Validator.CreateFor<string>(r => r.Text(nameof(CommunicationType).ToSentenceCase()).Mandatory().RefDataCode().As<CommunicationType>());
        private readonly Validator<ContactComm> _commsValue = Validator.Create<ContactComm>().HasProperty(p => p.Value, r => r.Mandatory().String(100));

        public ContactValidator()
        {
            Property(x => x.FirstName).Mandatory();
            Property(x => x.LastName).Mandatory();
            Property(x => x.Communications).Dictionary(item: DictionaryRuleItem.Create(_commsKey, _commsValue));
        }

        protected override Task<Result> OnValidateAsync(ValidationContext<Contact> context, CancellationToken cancellationToken)
        {
            if (context.Value.Communications != null && context.Value.Communications.Count(x => x.Value?.IsPreferred == true) > 1)
                context.AddError(x => x.Communications, "Only one of the Communications can be set as Preferred.");

            return Result.SuccessTask;
        }
    }
}