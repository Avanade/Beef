namespace Beef.Demo.Business.Entities
{
    public partial class ContactCommCollection
    {
        protected override void OnInitialization()
        {
            // Converting to the reference data value and then replacing with code will ensure correct casing etc.
            KeyModifier = k => CommunicationType.ConvertFromCode(k)?.Code!;
        }
    }
}