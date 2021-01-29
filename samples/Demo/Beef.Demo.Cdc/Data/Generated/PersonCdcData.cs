/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using Beef;
using Beef.Data.Database;
using Beef.Data.Database.Cdc;
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
    /// Enables the CDC data access for database object 'Demo.Person'.
    /// </summary>
    public partial interface IPersonCdcData : ICdcDataOrchestrator { }

    /// <summary>
    /// Provides the CDC data access for database object 'Demo.Person'.
    /// </summary>
    public partial class PersonCdcData : CdcDataOrchestrator<PersonCdc, PersonCdcData.PersonCdcWrapperCollection, PersonCdcData.PersonCdcWrapper, CdcTrackingDbMapper>, IPersonCdcData
    {
        private static readonly DatabaseMapper<PersonCdcWrapper> _personCdcWrapperMapper = DatabaseMapper.CreateAuto<PersonCdcWrapper>();

        private readonly Beef.Demo.Business.IPersonManager _personManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonCdcData"/> class.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="evtPub">The <see cref="IEventPublisher"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="personManager"></param>
        public PersonCdcData(IDatabase db, IEventPublisher evtPub, ILogger<PersonCdcData> logger, Beef.Demo.Business.IPersonManager personManager) :
            base(db, "[DemoCdc].[spExecutePersonCdcOutbox]", evtPub, logger)
        {
            _personManager = Check.NotNull(personManager, nameof(personManager));
            PersonCdcDataCtor();
        }

        partial void PersonCdcDataCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the outbox entity data from the database.
        /// </summary>
        /// <param name="maxBatchSize">The recommended maximum batch size.</param>
        /// <param name="incomplete">Indicates whether to return the last <b>incomplete</b> outbox where <c>true</c>; othewise, <c>false</c> for the next new outbox.</param>
        /// <returns>The corresponding result.</returns>
        protected override async Task<CdcDataOrchestratorResult<PersonCdcWrapperCollection, PersonCdcWrapper>> GetOutboxEntityDataAsync(int maxBatchSize, bool incomplete)
        {
            var pColl = new PersonCdcWrapperCollection();

            var result = await SelectQueryMultiSetAsync(maxBatchSize, incomplete,
                new MultiSetCollArgs<PersonCdcWrapperCollection, PersonCdcWrapper>(_personCdcWrapperMapper, r => pColl = r, stopOnNull: true) // Root table: Demo.Person
                ).ConfigureAwait(false);

            result.Result.AddRange(pColl);
            return result;
        }

        /// <summary>
        /// Gets the <see cref="EventData.Subject"/> without the appended key value(s).
        /// </summary>
        protected override string EventSubject => "Demo.Cdc.Person";

        /// <summary>
        /// Gets the <see cref="Events.EventActionFormat"/>.
        /// </summary>
        protected override EventActionFormat EventActionFormat => EventActionFormat.PastTense;

        /// <summary>
        /// Represents a <see cref="PersonCdc"/> wrapper to append the required (additional) database properties.
        /// </summary>
        public class PersonCdcWrapper : PersonCdc, ICdcWrapper
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
        }

        /// <summary>
        /// Represents a <see cref="PersonCdcWrapper"/> collection.
        /// </summary>
        public class PersonCdcWrapperCollection : List<PersonCdcWrapper> { }
    }
}

#pragma warning restore
#nullable restore