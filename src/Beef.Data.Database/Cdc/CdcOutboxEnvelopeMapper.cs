// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Provides the <see cref="CdcOutboxEnvelope"/> database mapping capability.
    /// </summary>
    public class CdcOutboxEnvelopeMapper : DatabaseMapper<CdcOutboxEnvelope, CdcOutboxEnvelopeMapper>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CdcOutboxEnvelopeMapper"/> class.
        /// </summary>
        public CdcOutboxEnvelopeMapper()
        {
            Property(s => s.Id, "OutboxEnvelopeId");
            Property(s => s.CreatedDate);
            Property(s => s.FirstLsn, "FirstProcessedLsn");
            Property(s => s.LastLsn, "LastProcessedLsn");
            Property(s => s.HasBeenCompleted);
            Property(s => s.ProcessedDate);
        }
    }
}