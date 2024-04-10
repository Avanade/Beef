namespace Beef.Demo.Business.Validation
{
    public class AddressCollectionValidator : CommonValidator<AddressCollection>
    {
        public AddressCollectionValidator() => this.Collection(1, 2, CollectionRuleItem.Create(PersonValidator._addressValidator).DuplicateCheck(x => x.Street));
    }
}