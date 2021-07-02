using Beef.Demo.Common.Entities;
using Beef.Validation;
using Beef.Validation.Rules;

namespace Beef.Demo.Business.Validation
{
    public class AddressCollectionValidator : CollectionValidator<AddressCollection, Address>
    {
        public AddressCollectionValidator()
        {
            MinCount = 1;
            MaxCount = 2;
            Item = CollectionRuleItem.Create(PersonValidator._addressValidator).DuplicateCheck(x => x.Street);
        }
    }
}