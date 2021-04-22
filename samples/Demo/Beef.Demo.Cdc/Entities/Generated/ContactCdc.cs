/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using Beef.Data.Database.Cdc;
using Beef.Entities;
using Beef.Mapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beef.Demo.Cdc.Entities
{
    /// <summary>
    /// Represents the CDC model for the root (parent) database table 'Legacy.Contact'.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class ContactCdc : ITableKey, IETag, IGlobalIdentifier, ICdcLinkIdentifierMapping
    {
        /// <summary>
        /// Gets or sets the <see cref="IGlobalIdentifier.GlobalId"/>.
        /// </summary>
        [JsonProperty("globalId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? GlobalId { get; set; }

        /// <summary>
        /// Gets or sets the 'CID' (Legacy.Contact.ContactId) column value.
        /// </summary>
        public int CID { get; set; }

        /// <summary>
        /// Gets or sets the 'Name' (Legacy.Contact.Name) column value.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the 'Phone' (Legacy.Contact.Phone) column value.
        /// </summary>
        [JsonProperty("phone", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Phone { get; set; }

        /// <summary>
        /// Gets or sets the 'Email' (Legacy.Contact.Email) column value.
        /// </summary>
        [JsonProperty("email", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the 'Active' (Legacy.Contact.Active) column value.
        /// </summary>
        [JsonProperty("active", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? Active { get; set; }

        /// <summary>
        /// Gets or sets the 'Dont Call List' (Legacy.Contact.DontCallList) column value.
        /// </summary>
        [JsonProperty("dontCallList", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? DontCallList { get; set; }

        /// <summary>
        /// Gets or sets the 'Address Id' (Legacy.Contact.AddressId) column value.
        /// </summary>
        [JsonProperty("addressId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int? AddressId { get; set; }

        /// <summary>
        /// Gets or sets the 'Alternate Contact Id' (Legacy.Contact.AlternateContactId) column value.
        /// </summary>
        public int? AlternateContactId { get; set; }

        /// <summary>
        /// Gets or sets the 'Global Alternate Contact Id' (Legacy.Contact.AlternateContactId) mapped identifier value.
        /// </summary>
        [JsonProperty("globalAlternateContactId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? GlobalAlternateContactId { get; set; }

        /// <summary>
        /// Gets or sets the 'Legacy System Code' (Legacy.Contact.legacy_system_code) column value.
        /// </summary>
        [JsonProperty("legacySystemCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? LegacySystemCode { get; set; }

        /// <summary>
        /// Gets or sets the 'UniqueId' column value (join table 'Legacy.ContactMapping').
        /// </summary>
        [JsonProperty("uniqueId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid UniqueId { get; set; }

        /// <summary>
        /// Gets or sets the related (one-to-one) <see cref="ContactCdc.Address"/> (database table 'Legacy.Address').
        /// </summary>
        [JsonProperty("address", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [MapperIgnore()]
        public ContactCdc.AddressCdc? Address { get; set; }

        /// <summary>
        /// Gets or sets the entity tag (calculated as a JSON serialized hash value).
        /// </summary>
        [JsonProperty("etag", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [MapperIgnore()]
        public string? ETag { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [MapperIgnore()]
        public UniqueKey UniqueKey => new UniqueKey(CID);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [MapperIgnore()]
        public string[] UniqueKeyProperties => new string[] { nameof(CID) };

        /// <summary>
        /// Gets or sets the 'CID' <i>primary key</i> (Legacy.Contact.ContactId) column value (from the actual database table primary key; not from the change-data-capture source).
        /// </summary>
        /// <remarks>Will have a <c>default</c> value when the record no longer exists within the database (i.e. has been physically deleted).</remarks>
        public int TableKey_CID { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public UniqueKey TableKey => new UniqueKey(TableKey_CID);

        /// <summary>
        /// Link any new global identifiers.
        /// </summary>
        /// <param name="coll">The <see cref="CdcValueIdentifierMappingCollection"/>.</param>
        /// <param name="idGen">The <see cref="IStringIdentifierGenerator"/>.</param>
        public async Task LinkIdentifierMappingsAsync(CdcValueIdentifierMappingCollection coll, IStringIdentifierGenerator idGen)
        {
            coll.AddAsync(GlobalId == default, async () => new CdcValueIdentifierMapping { Value = this, Property = nameof(GlobalId), Schema = "Legacy", Table = "Contact", Key = this.CreateIdentifierMappingKey(), GlobalId = await idGen.GenerateIdentifierAsync<ContactCdc>().ConfigureAwait(false) });
            coll.AddAsync(GlobalAlternateContactId == default && AlternateContactId != default, async () => new CdcValueIdentifierMapping { Value = this, Property = nameof(GlobalAlternateContactId), Schema = "Legacy", Table = "Contact", Key = AlternateContactId.ToString(), GlobalId = await idGen.GenerateIdentifierAsync<ContactCdc>().ConfigureAwait(false) });
            await (Address?.LinkIdentifierMappingsAsync(coll, idGen) ?? Task.CompletedTask).ConfigureAwait(false);
        }

        /// <summary>
        /// Re-link the new global identifiers.
        /// </summary>
        /// <param name="coll">The <see cref="CdcValueIdentifierMappingCollection"/>.</param>
        public void RelinkIdentifierMappings(CdcValueIdentifierMappingCollection coll)
        {
            coll.Invoke(GlobalId == default, () => GlobalId = coll.GetGlobalId(this, nameof(GlobalId)));
            coll.Invoke(GlobalAlternateContactId == default && AlternateContactId != default, () => GlobalAlternateContactId = coll.GetGlobalId(this, nameof(GlobalAlternateContactId)));
            Address?.RelinkIdentifierMappings(coll);
        }

        #region AddressCdc

        /// <summary>
        /// Represents the CDC model for the related (child) database table 'Legacy.Address' (known uniquely as 'Address').
        /// </summary>
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public partial class AddressCdc : IUniqueKey, IGlobalIdentifier
        {
            /// <summary>
            /// Gets or sets the <see cref="IGlobalIdentifier.GlobalId"/>.
            /// </summary>
            [JsonProperty("globalId", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string? GlobalId { get; set; }

            /// <summary>
            /// Gets or sets the 'AID' (Legacy.Address.Id) column value.
            /// </summary>
            public int AID { get; set; }

            /// <summary>
            /// Gets or sets the 'Street1' (Legacy.Address.Street1) column value.
            /// </summary>
            [JsonProperty("street1", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string? Street1 { get; set; }

            /// <summary>
            /// Gets or sets the 'Street2' (Legacy.Address.Street2) column value.
            /// </summary>
            [JsonProperty("street2", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string? Street2 { get; set; }

            /// <summary>
            /// Gets or sets the 'City' (Legacy.Address.City) column value.
            /// </summary>
            [JsonProperty("city", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string? City { get; set; }

            /// <summary>
            /// Gets or sets the 'State' (Legacy.Address.State) column value.
            /// </summary>
            [JsonProperty("state", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string? State { get; set; }

            /// <summary>
            /// Gets or sets the 'Postal Zip Code' (Legacy.Address.PostalZipCode) column value.
            /// </summary>
            [JsonProperty("postalZipCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string? PostalZipCode { get; set; }

            /// <summary>
            /// Gets or sets the 'Alternate Address Id' (Legacy.Address.AlternateAddressId) column value.
            /// </summary>
            public int? AlternateAddressId { get; set; }

            /// <summary>
            /// Gets or sets the 'Global Alternate Address Id' (Legacy.Address.AlternateAddressId) mapped identifier value.
            /// </summary>
            [JsonProperty("globalAlternateAddressId", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string? GlobalAlternateAddressId { get; set; }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            [MapperIgnore()]
            public UniqueKey UniqueKey => new UniqueKey(AID);

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            [MapperIgnore()]
            public string[] UniqueKeyProperties => new string[] { nameof(AID) };

            /// <summary>
            /// Link any new global identifiers.
            /// </summary>
            /// <param name="coll">The <see cref="CdcValueIdentifierMappingCollection"/>.</param>
            /// <param name="idGen">The <see cref="IStringIdentifierGenerator"/>.</param>
            public async Task LinkIdentifierMappingsAsync(CdcValueIdentifierMappingCollection coll, IStringIdentifierGenerator idGen)
            {
                coll.AddAsync(GlobalId == default, async () => new CdcValueIdentifierMapping { Value = this, Property = nameof(GlobalId), Schema = "Legacy", Table = "Address", Key = this.CreateIdentifierMappingKey(), GlobalId = await idGen.GenerateIdentifierAsync<AddressCdc>().ConfigureAwait(false) });
                coll.AddAsync(GlobalAlternateAddressId == default && AlternateAddressId != default, async () => new CdcValueIdentifierMapping { Value = this, Property = nameof(GlobalAlternateAddressId), Schema = "Legacy", Table = "Address", Key = AlternateAddressId.ToString(), GlobalId = await idGen.GenerateIdentifierAsync<AddressCdc>().ConfigureAwait(false) });
            }

            /// <summary>
            /// Re-link the new global identifiers.
            /// </summary>
            /// <param name="coll">The <see cref="CdcValueIdentifierMappingCollection"/>.</param>
            public void RelinkIdentifierMappings(CdcValueIdentifierMappingCollection coll)
            {
                coll.Invoke(GlobalId == default, () => GlobalId = coll.GetGlobalId(this, nameof(GlobalId)));
                coll.Invoke(GlobalAlternateAddressId == default && AlternateAddressId != default, () => GlobalAlternateAddressId = coll.GetGlobalId(this, nameof(GlobalAlternateAddressId)));
            }
        }

        /// <summary>
        /// Represents the CDC model for the related (child) database table collection 'Legacy.Address'.
        /// </summary>
        public partial class AddressCdcCollection : List<AddressCdc> { }

        #endregion
    }
}

#pragma warning restore
#nullable restore