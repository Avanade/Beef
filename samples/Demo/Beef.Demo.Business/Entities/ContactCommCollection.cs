namespace Beef.Demo.Business.Entities
{
    public partial class ContactCommCollection
    {
        /// <inheritdoc/>
        /// <remarks>Converting to the reference data value and then replacing with code will ensure correct casing etc.</remarks>
        protected override string OnModifyKey(string key) => CommunicationType.ConvertFromCode(key)?.Code!;
    }
}