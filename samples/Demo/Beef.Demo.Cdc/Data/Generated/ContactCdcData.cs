/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using Beef;
using Beef.Data.Database;
using Beef.Data.Database.Cdc;
using Beef.Entities;
using Beef.Events;
using Beef.Mapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beef.Demo.Cdc.Entities;

namespace Beef.Demo.Cdc.Data
{
    /// <summary>
    /// Enables the CDC data access for database object 'Legacy.Contact'.
    /// </summary>
    public partial interface IContactCdcData : ICdcDataOrchestrator { }

    /// <summary>
    /// Provides the CDC data access for database object 'Legacy.Contact'.
    /// </summary>
    public partial class ContactCdcData : CdcDataOrchestrator<ContactCdc, ContactCdcData.ContactCdcWrapperCollection, ContactCdcData.ContactCdcWrapper, CdcTrackingDbMapper>, IContactCdcData
    {
        private static readonly DatabaseMapper<ContactCdcWrapper> _contactCdcWrapperMapper = DatabaseMapper.CreateAuto<ContactCdcWrapper>();
        private static readonly DatabaseMapper<ContactCdc.AddressCdc> _addressCdcMapper = DatabaseMapper.CreateAuto<ContactCdc.AddressCdc>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactCdcData"/> class.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="evtPub">The <see cref="IEventPublisher"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="idGen">The <see cref="IStringIdentifierGenerator"/>.</param>
        public ContactCdcData(IDatabase db, IEventPublisher evtPub, ILogger<ContactCdcData> logger, IStringIdentifierGenerator idGen) :
            base(db, "[DemoCdc].[spExecuteContactCdcOutbox]", "[DemoCdc].[spCompleteContactCdcOutbox]", evtPub, logger, "[DemoCdc].[spCreateCdcIdentifierMapping]", idGen, new CdcIdentifierMappingDbMapper()) => ContactCdcDataCtor();

        partial void ContactCdcDataCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the outbox entity data from the database.
        /// </summary>
        /// <returns>The corresponding result.</returns>
        protected override async Task<CdcDataOrchestratorResult<ContactCdcWrapperCollection, ContactCdcWrapper>> GetOutboxEntityDataAsync()
        {
            var cColl = new ContactCdcWrapperCollection();

            var result = await SelectQueryMultiSetAsync(
                new MultiSetCollArgs<ContactCdcWrapperCollection, ContactCdcWrapper>(_contactCdcWrapperMapper, r => cColl = r, stopOnNull: true), // Root table: Legacy.Contact
                new MultiSetCollArgs<ContactCdc.AddressCdcCollection, ContactCdc.AddressCdc>(_addressCdcMapper, r =>
                {
                    foreach (var a in r.GroupBy(x => new { x.AID }).Select(g => new { g.Key.AID, Coll = g.ToCollection<ContactCdc.AddressCdcCollection, ContactCdc.AddressCdc>() })) // Join table: Address (Legacy.Address)
                    {
                        cColl.Where(x => x.AddressId == a.AID).ForEach(x => x.Address = a.Coll.SingleOrDefault());
                    }
                }) // Related table: Address (Legacy.Address)
                ).ConfigureAwait(false);

            result.Result.AddRange(cColl);
            return result;
        }

        /// <summary>
        /// Gets the <see cref="Beef.Events.EventData.Subject"/> format.
        /// </summary>
        protected override EventSubjectFormat EventSubjectFormat => EventSubjectFormat.NameAndKey;

        /// <summary>
        /// Gets the <see cref="EventData.Subject"/> (to be further formatted as per <see cref="EventSubjectFormat"/>).
        /// </summary>
        protected override string EventSubject => "Legacy.Contact";

        /// <summary>
        /// Gets the <see cref="Events.EventActionFormat"/>.
        /// </summary>
        protected override EventActionFormat EventActionFormat => EventActionFormat.PastTense;

        /// <summary>
        /// Represents a <see cref="ContactCdc"/> wrapper to append the required (additional) database properties.
        /// </summary>
        public class ContactCdcWrapper : ContactCdc, ICdcWrapper
        {
            /// <summary>
            /// Gets or sets the database CDC <see cref="OperationType"/>.
            /// </summary>
            [MapperProperty("_OperationType", ConverterType = typeof(CdcOperationTypeConverter))]
            public OperationType DatabaseOperationType { get; set; }

            /// <summary>
            /// Gets or sets the database tracking hash code.
            /// </summary>
            [MapperProperty("_TrackingHash")]
            public string? DatabaseTrackingHash { get; set; }

            /// <summary>
            /// Gets or sets the database log sequence number (LSN).
            /// </summary>
            [MapperProperty("_Lsn")]
            public byte[] DatabaseLsn { get; set; }
        }

        /// <summary>
        /// Represents a <see cref="ContactCdcWrapper"/> collection.
        /// </summary>
        public class ContactCdcWrapperCollection : List<ContactCdcWrapper> { }
    }
}

#pragma warning restore
#nullable restore