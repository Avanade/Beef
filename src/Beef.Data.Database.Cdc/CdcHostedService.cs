// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Provides the base (non-generics) capabilities for the Change Data Capture (CDC) <see cref="IHostedService"/> services.
    /// </summary>
    public abstract class CdcHostedService : TimerHostedServiceBase
    {
        private TimeSpan? _interval;
        private int? _maxQuerySize;
        private bool? _continueWithDataLoss;

        /// <summary>
        /// The configuration name for <see cref="Interval"/>.
        /// </summary>
        public const string IntervalName = "Interval";

        /// <summary>
        /// The configuration name for <see cref="MaxQuerySize"/>.
        /// </summary>
        public const string MaxQuerySizeName = "MaxQuerySize";

        /// <summary>
        /// The configuration name for <see cref="ContinueWithDataLoss"/>.
        /// </summary>
        public const string ContinueWithDataLossName = "ContinueWithDataLoss";

        /// <summary>
        /// Gets or sets the default interval seconds used where the specified <see cref="Interval"/> is less than or equal to zero. Defaults to <b>one</b> minute.
        /// </summary>
        public static TimeSpan DefaultInterval { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="CdcHostedService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        public CdcHostedService(IServiceProvider serviceProvider, ILogger logger, IConfiguration? config = null) : base(serviceProvider, logger, config) { }

        /// <summary>
        /// Gets or sets the interval between each execution.
        /// </summary>
        /// <remarks>Will default to configuration, a) <see cref="TimerHostedServiceBase.ServiceName"/> : <see cref="IntervalName"/>, then b) <see cref="IntervalName"/>, where specified; otherwise, <see cref="DefaultInterval"/>.</remarks>
        public override TimeSpan Interval
        { 
            get => _interval ?? Config.GetValue<TimeSpan?>($"{ServiceName}:{IntervalName}") ?? Config.GetValue<TimeSpan?>(IntervalName) ?? DefaultInterval;
            set => _interval = value;
        }

        /// <summary>
        /// Gets or sets the maximum query size to limit the number of CDC (Change Data Capture) rows that are batched in a <see cref="CdcOutbox"/>.
        /// </summary>
        /// <remarks>Where specified overrides the <see cref="ICdcDataOrchestrator.MaxQuerySize"/>.
        /// <para>Will default to configuration, a) <see cref="TimerHostedServiceBase.ServiceName"/> : <see cref="MaxQuerySizeName"/>, then b) <see cref="MaxQuerySizeName"/>, where specified.</para></remarks>
        public virtual int? MaxQuerySize
        {
            get => _maxQuerySize ?? Config.GetValue<int?>($"{ServiceName}:{MaxQuerySizeName}") ?? Config.GetValue<int?>(MaxQuerySizeName);
            set => _maxQuerySize = value;
        }

        /// <summary>
        /// Indicates whether to ignore any data loss and continue using the CDC (Change Data Capture) data that is available.
        /// </summary>
        /// <remarks>For more information as to why data loss may occur see: https://docs.microsoft.com/en-us/sql/relational-databases/track-changes/administer-and-monitor-change-data-capture-sql-server 
        /// <para>Will default to configuration, a) <see cref="TimerHostedServiceBase.ServiceName"/> : <see cref="ContinueWithDataLossName"/>, then b) <see cref="ContinueWithDataLossName"/>, where specified.</para></remarks>
        public virtual bool? ContinueWithDataLoss
        {
            get => _continueWithDataLoss ?? Config.GetValue<bool?>($"{ServiceName}:{ContinueWithDataLossName}") ?? Config.GetValue<bool?>(ContinueWithDataLossName);
            set => _continueWithDataLoss = value;
        }
    }

    /// <summary>
    /// Represents the base class for Change Data Capture (CDC) <see cref="IHostedService"/> services.
    /// </summary>
    public abstract class CdcHostedService<TCdcDataOrchestrator> : CdcHostedService where TCdcDataOrchestrator : ICdcDataOrchestrator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CdcHostedService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        public CdcHostedService(IServiceProvider serviceProvider, ILogger logger, IConfiguration? config = null) : base(serviceProvider, logger, config) { }

        /// <summary>
        /// Executes the data orchestration for the next outbox and/or incomplete outbox.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var cdo = ExecutionContext.GetService<TCdcDataOrchestrator>(throwExceptionOnNull: true)!;

            var mqs = MaxQuerySize;
            if (mqs.HasValue)
                cdo.MaxQuerySize = mqs.Value;

            var cwdl = ContinueWithDataLoss;
            if (cwdl.HasValue)
                cdo.ContinueWithDataLoss = cwdl.Value;

            // Keep executing until unsuccessful or reached end of CDC data.
            while (true)
            {
                var result = await cdo.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                if (cancellationToken.IsCancellationRequested)
                    return;

                // Where successful, then the next outbox should be attempted immediately.
                if (!result.IsSuccessful || result.Outbox == null)
                    return; // Retry later.
            } 
        }
    }
}