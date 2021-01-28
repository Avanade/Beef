/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0001, IDE0005, IDE0044, IDE0079, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649

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
    /// Enables the CDC data access for database object 'Demo.Person2'.
    /// </summary>
    public partial interface IPerson2CdcData : ICdcDataOrchestrator { }

    /// <summary>
    /// Provides the CDC data access for database object 'Demo.Person2'.
    /// </summary>
    public partial class Person2CdcData : CdcDataOrchestrator<Person2Cdc, Person2CdcData.Person2CdcWrapperCollection, Person2CdcData.Person2CdcWrapper, CdcTrackingDbMapper>, IPerson2CdcData
    {
        private static readonly DatabaseMapper<Person2CdcWrapper> _person2CdcWrapperMapper = DatabaseMapper.CreateAuto<Person2CdcWrapper>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Person2CdcData"/> class.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="evtPub">The <see cref="IEventPublisher"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public Person2CdcData(IDatabase db, IEventPublisher evtPub, ILogger<Person2CdcData> logger) :
            base(db, "[DemoCdc].[spExecutePerson2CdcOutbox]", evtPub, logger) => Person2CdcDataCtor();

        partial void Person2CdcDataCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the outbox entity data from the database.
        /// </summary>
        /// <param name="maxBatchSize">The recommended maximum batch size.</param>
        /// <param name="incomplete">Indicates whether to return the last <b>incomplete</b> outbox where <c>true</c>; othewise, <c>false</c> for the next new outbox.</param>
        /// <returns>The corresponding result.</returns>
        protected override async Task<CdcDataOrchestratorResult<Person2CdcWrapperCollection, Person2CdcWrapper>> GetOutboxEntityDataAsync(int maxBatchSize, bool incomplete)
        {
            var pColl = new Person2CdcWrapperCollection();

            var result = await SelectQueryMultiSetAsync(maxBatchSize, incomplete,
                new MultiSetCollArgs<Person2CdcWrapperCollection, Person2CdcWrapper>(_person2CdcWrapperMapper, r => pColl = r, stopOnNull: true) // Root table: Demo.Person2
                ).ConfigureAwait(false);

            result.Result.AddRange(pColl);
            return result;
        }

        /// <summary>
        /// Gets the <see cref="EventData.Subject"/> without the appended key value(s).
        /// </summary>
        protected override string EventSubject => "Demo.Cdc.Person2";

        /// <summary>
        /// Gets the <see cref="Events.EventActionFormat"/>.
        /// </summary>
        protected override EventActionFormat EventActionFormat => EventActionFormat.PastTense;

        /// <summary>
        /// Represents a <see cref="Person2Cdc"/> wrapper to append the required (additional) database properties.
        /// </summary>
        public class Person2CdcWrapper : Person2Cdc, ICdcWrapper
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
        /// Represents a <see cref="Person2CdcWrapper"/> collection.
        /// </summary>
        public class Person2CdcWrapperCollection : List<Person2CdcWrapper> { }
    }
}

#pragma warning restore IDE0001, IDE0005, IDE0044, IDE0079, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649
#nullable restore