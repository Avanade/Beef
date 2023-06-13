/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Provides the <see cref="Robot"/> data access.
    /// </summary>
    public partial class RobotData : IRobotData
    {
        private readonly DemoCosmosDb _cosmos;
        private Func<IQueryable<Model.Robot>, RobotArgs?, IQueryable<Model.Robot>>? _getByArgsOnQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="RobotData"/> class.
        /// </summary>
        /// <param name="cosmos">The <see cref="DemoCosmosDb"/>.</param>
        public RobotData(DemoCosmosDb cosmos)
            { _cosmos = cosmos.ThrowIfNull(); RobotDataCtor(); }

        partial void RobotDataCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the specified <see cref="Robot"/>.
        /// </summary>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        /// <returns>The selected <see cref="Robot"/> where found.</returns>
        public Task<Result<Robot?>> GetAsync(Guid id)
            => _cosmos.Items.GetWithResultAsync(TypeToStringConverter<Guid>.Default.ToDestination.Convert(id));

        /// <summary>
        /// Creates a new <see cref="Robot"/>.
        /// </summary>
        /// <param name="value">The <see cref="Robot"/>.</param>
        /// <returns>The created <see cref="Robot"/>.</returns>
        public Task<Result<Robot>> CreateAsync(Robot value)
            => _cosmos.Items.CreateWithResultAsync(value);

        /// <summary>
        /// Updates an existing <see cref="Robot"/>.
        /// </summary>
        /// <param name="value">The <see cref="Robot"/>.</param>
        /// <returns>The updated <see cref="Robot"/>.</returns>
        public Task<Result<Robot>> UpdateAsync(Robot value)
            => _cosmos.Items.UpdateWithResultAsync(value);

        /// <summary>
        /// Deletes the specified <see cref="Robot"/>.
        /// </summary>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        public Task<Result> DeleteAsync(Guid id)
            => _cosmos.Items.DeleteWithResultAsync(TypeToStringConverter<Guid>.Default.ToDestination.Convert(id));

        /// <summary>
        /// Gets the <see cref="RobotCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Entities.RobotArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="RobotCollectionResult"/>.</returns>
        public Task<Result<RobotCollectionResult>> GetByArgsAsync(RobotArgs? args, PagingArgs? paging)
            => _cosmos.Items.Query(q => _getByArgsOnQuery?.Invoke(q, args) ?? q).WithPaging(paging).SelectResultWithResultAsync<RobotCollectionResult, RobotCollection>();

        /// <summary>
        /// Provides the <see cref="Robot"/> to Entity Framework <see cref="Model.Robot"/> mapping.
        /// </summary>
        public partial class EntityToModelCosmosMapper : Mapper<Robot, Model.Robot>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EntityToModelCosmosMapper"/> class.
            /// </summary>
            public EntityToModelCosmosMapper()
            {
                Map((s, d) => d.Id = TypeToStringConverter<Guid>.Default.ToDestination.Convert(s.Id), OperationTypes.Any, s => s.Id == default, d => d.Id = default);
                Map((s, d) => d.ModelNo = s.ModelNo, OperationTypes.Any, s => s.ModelNo == default, d => d.ModelNo = default);
                Map((s, d) => d.SerialNo = s.SerialNo, OperationTypes.Any, s => s.SerialNo == default, d => d.SerialNo = default);
                Map((s, d) => d.EyeColor = s.EyeColorSid, OperationTypes.Any, s => s.EyeColorSid == default, d => d.EyeColor = default);
                Map((s, d) => d.PowerSource = s.PowerSourceSid, OperationTypes.Any, s => s.PowerSourceSid == default, d => d.PowerSource = default);
                Map((s, d) => d.ETag = s.ETag, OperationTypes.Any, s => s.ETag == default, d => d.ETag = default);
                Map((o, s, d) => d.ChangeLog = o.Map(s.ChangeLog, d.ChangeLog), OperationTypes.Any, s => s.ChangeLog == default, d => d.ChangeLog = default);
                EntityToModelCosmosMapperCtor();
            }

            partial void EntityToModelCosmosMapperCtor(); // Enables the constructor to be extended.
        }

        /// <summary>
        /// Provides the Entity Framework <see cref="Model.Robot"/> to <see cref="Robot"/> mapping.
        /// </summary>
        public partial class ModelToEntityCosmosMapper : Mapper<Model.Robot, Robot>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ModelToEntityCosmosMapper"/> class.
            /// </summary>
            public ModelToEntityCosmosMapper()
            {
                Map((s, d) => d.Id = (Guid)TypeToStringConverter<Guid>.Default.ToSource.Convert(s.Id), OperationTypes.Any, s => s.Id == default, d => d.Id = default);
                Map((s, d) => d.ModelNo = (string?)s.ModelNo, OperationTypes.Any, s => s.ModelNo == default, d => d.ModelNo = default);
                Map((s, d) => d.SerialNo = (string?)s.SerialNo, OperationTypes.Any, s => s.SerialNo == default, d => d.SerialNo = default);
                Map((s, d) => d.EyeColorSid = (string?)s.EyeColor, OperationTypes.Any, s => s.EyeColor == default, d => d.EyeColorSid = default);
                Map((s, d) => d.PowerSourceSid = (string?)s.PowerSource, OperationTypes.Any, s => s.PowerSource == default, d => d.PowerSourceSid = default);
                Map((s, d) => d.ETag = (string?)s.ETag, OperationTypes.Any, s => s.ETag == default, d => d.ETag = default);
                Map((o, s, d) => d.ChangeLog = o.Map(s.ChangeLog, d.ChangeLog), OperationTypes.Any, s => s.ChangeLog == default, d => d.ChangeLog = default);
                ModelToEntityCosmosMapperCtor();
            }

            partial void ModelToEntityCosmosMapperCtor(); // Enables the constructor to be extended.
        }
    }
}

#pragma warning restore
#nullable restore