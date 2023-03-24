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
                Map((s, d) => d.TerminationDate = s.Date, OperationTypes.Any, s => s.Date == default, d => d.TerminationDate = default);
                Map((s, d) => d.TerminationReasonCode = s.ReasonSid, OperationTypes.Any, s => s.ReasonSid == default, d => d.TerminationReasonCode = default);
                EntityToModelEfMapperCtor();
            }

            partial void EntityToModelEfMapperCtor(); // Enables the constructor to be extended.
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
                Map((s, d) => d.Date = (DateTime)s.TerminationDate, OperationTypes.Any, s => s.TerminationDate == default, d => d.Date = default);
                Map((s, d) => d.ReasonSid = (string?)s.TerminationReasonCode, OperationTypes.Any, s => s.TerminationReasonCode == default, d => d.ReasonSid = default);
                ModelToEntityEfMapperCtor();
            }

            partial void ModelToEntityEfMapperCtor(); // Enables the constructor to be extended.
        }
    }
}

#pragma warning restore
#nullable restore