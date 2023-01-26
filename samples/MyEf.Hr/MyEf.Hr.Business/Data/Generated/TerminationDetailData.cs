/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace MyEf.Hr.Business.Data
{
    /// <summary>
    /// Provides the <see cref="TerminationDetail"/> data access.
    /// </summary>
    public partial class TerminationDetailData
    {

        /// <summary>
        /// Provides the <see cref="TerminationDetail"/> to Entity Framework <see cref="EfModel.Employee"/> mapping.
        /// </summary>
        public partial class EntityToModelEfMapper : Mapper<TerminationDetail, EfModel.Employee>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EntityToModelEfMapper"/> class.
            /// </summary>
            public EntityToModelEfMapper()
            {
                Map((s, d) => d.TerminationDate = s.Date);
                Map((s, d) => d.TerminationReasonCode = s.ReasonSid);
                EntityToModelEfMapperCtor();
            }

            partial void EntityToModelEfMapperCtor(); // Enables the constructor to be extended.

            /// <inheritdoc/>
            public override bool IsSourceInitial(TerminationDetail s)
                => s.Date == default
                && s.ReasonSid == default;
        }

        /// <summary>
        /// Provides the Entity Framework <see cref="EfModel.Employee"/> to <see cref="TerminationDetail"/> mapping.
        /// </summary>
        public partial class ModelToEntityEfMapper : Mapper<EfModel.Employee, TerminationDetail>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ModelToEntityEfMapper"/> class.
            /// </summary>
            public ModelToEntityEfMapper()
            {
                Map((s, d) => d.Date = (DateTime)s.TerminationDate);
                Map((s, d) => d.ReasonSid = (string?)s.TerminationReasonCode);
                ModelToEntityEfMapperCtor();
            }

            partial void ModelToEntityEfMapperCtor(); // Enables the constructor to be extended.

            /// <inheritdoc/>
            public override bool IsSourceInitial(EfModel.Employee s)
                => s.TerminationDate == default
                && s.TerminationReasonCode == default;
        }
    }
}

#pragma warning restore
#nullable restore