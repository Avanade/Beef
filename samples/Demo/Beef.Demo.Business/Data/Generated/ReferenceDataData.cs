/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Provides the <b>ReferenceData</b> data access.
    /// </summary>
    public partial class ReferenceDataData : IReferenceDataData
    {
        private readonly IDatabase _db;
        private readonly IEfDb _ef;
        private readonly DemoCosmosDb _cosmos;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataData"/> class.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="ef">The <see cref="IEfDb"/>.</param>
        /// <param name="cosmos">The <see cref="DemoCosmosDb"/>.</param>
        /// <param name="mapper">The <see cref="IMapper"/>.</param>
        public ReferenceDataData(IDatabase db, IEfDb ef, DemoCosmosDb cosmos, IMapper mapper)
            { _db = db ?? throw new ArgumentNullException(nameof(db)); _ef = ef ?? throw new ArgumentNullException(nameof(ef)); _cosmos = cosmos ?? throw new ArgumentNullException(nameof(cosmos)); _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper)); ReferenceDataDataCtor(); }

        partial void ReferenceDataDataCtor(); // Enables additional functionality to be added to the constructor.

        /// <inheritdoc/>
        public Task<RefDataNamespace.CountryCollection> CountryGetAllAsync()
            => DataInvoker.Current.InvokeAsync(this, _ => _db.ReferenceData<RefDataNamespace.CountryCollection, RefDataNamespace.Country, Guid>("[Ref].[spCountryGetAll]").LoadAsync("CountryId"), InvokerArgs.TransactionSuppress);

        /// <inheritdoc/>
        public Task<RefDataNamespace.USStateCollection> USStateGetAllAsync()
            => DataInvoker.Current.InvokeAsync(this, _ => _db.ReferenceData<RefDataNamespace.USStateCollection, RefDataNamespace.USState, Guid>("[Ref].[spUSStateGetAll]").LoadAsync("USStateId"), InvokerArgs.TransactionSuppress);

        /// <inheritdoc/>
        public Task<RefDataNamespace.GenderCollection> GenderGetAllAsync()
            => DataInvoker.Current.InvokeAsync(this, _ => 
            {
                return _db.ReferenceData<RefDataNamespace.GenderCollection, RefDataNamespace.Gender, Guid>("[Ref].[spGenderGetAll]").LoadAsync("GenderId", additionalProperties: (dr, item) =>
                {
                    item.AlternateName = dr.GetValue<string>("AlternateName");
                    item.TripCode = dr.GetValue<string>("TripCode");
                    item.Country = ReferenceDataIdConverter<RefDataNamespace.Country, Guid?>.Default.ToSource.Convert(dr.GetValue<Guid?>("CountryId"));
                });
            }, InvokerArgs.TransactionSuppress);

        /// <inheritdoc/>
        public Task<RefDataNamespace.EyeColorCollection> EyeColorGetAllAsync()
            => DataInvoker.Current.InvokeAsync(this, _ =>_ef.Query<RefDataNamespace.EyeColor, EfModel.EyeColor>().SelectQueryAsync<RefDataNamespace.EyeColorCollection>(), InvokerArgs.TransactionSuppress);

        /// <inheritdoc/>
        public Task<RefDataNamespace.PowerSourceCollection> PowerSourceGetAllAsync()
            => DataInvoker.Current.InvokeAsync(this, _ => _cosmos.ValueContainer<RefDataNamespace.PowerSource, Model.PowerSource>("RefData").Query().SelectQueryAsync<RefDataNamespace.PowerSourceCollection>());

        /// <inheritdoc/>
        public Task<RefDataNamespace.CompanyCollection> CompanyGetAllAsync()
            => DataInvoker.Current.InvokeAsync(this, _ => CompanyGetAll_OnImplementationAsync());

        /// <inheritdoc/>
        public Task<RefDataNamespace.StatusCollection> StatusGetAllAsync()
            => DataInvoker.Current.InvokeAsync(this, _ =>_ef.Query<RefDataNamespace.Status, EfModel.Status>().SelectQueryAsync<RefDataNamespace.StatusCollection>(), InvokerArgs.TransactionSuppress);

        /// <inheritdoc/>
        public Task<RefDataNamespace.CommunicationTypeCollection> CommunicationTypeGetAllAsync()
            => DataInvoker.Current.InvokeAsync(this, _ => CommunicationTypeGetAll_OnImplementationAsync());

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.EyeColor"/> to Entity Framework <see cref="EfModel.EyeColor"/> mapping.
        /// </summary>
        public partial class EyeColorToModelEfMapper : Mapper<RefDataNamespace.EyeColor, EfModel.EyeColor>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EyeColorToModelEfMapper"/> class.
            /// </summary>
            public EyeColorToModelEfMapper()
            {
                Map((s, d) => d.EyeColorId = s.Id);
                Map((s, d) => d.Code = s.Code);
                Map((s, d) => d.Text = s.Text);
                Map((s, d) => d.IsActive = s.IsActive);
                Map((s, d) => d.SortOrder = s.SortOrder);
                Map((s, d) => d.RowVersion = StringToBase64Converter.Default.ToDestination.Convert(s.ETag));
                EyeColorToModelEfMapperCtor();
            }

            partial void EyeColorToModelEfMapperCtor(); // Enables the constructor to be extended.

            /// <inheritdoc/>
            public override bool IsSourceInitial(EyeColor s)
                => s.Id == default
                && s.Code == default
                && s.Text == default
                && s.IsActive == default
                && s.SortOrder == default
                && s.ETag == default;
        }

        /// <summary>
        /// Provides the Entity Framework <see cref="EfModel.EyeColor"/> to <see cref="RefDataNamespace.EyeColor"/> mapping.
        /// </summary>
        public partial class ModelToEyeColorEfMapper : Mapper<EfModel.EyeColor, RefDataNamespace.EyeColor>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ModelToEntityEfMapper"/> class.
            /// </summary>
            public ModelToEyeColorEfMapper()
            {
                Map((s, d) => d.Id = (Guid)s.EyeColorId);
                Map((s, d) => d.Code = (string?)s.Code);
                Map((s, d) => d.Text = (string?)s.Text);
                Map((s, d) => d.IsActive = (bool)s.IsActive);
                Map((s, d) => d.SortOrder = (int)s.SortOrder);
                Map((s, d) => d.ETag = (string?)StringToBase64Converter.Default.ToSource.Convert(s.RowVersion));
                ModelToEyeColorEfMapperCtor();
            }

            partial void ModelToEyeColorEfMapperCtor(); // Enables the constructor to be extended.

            /// <inheritdoc/>
            public override bool IsSourceInitial(EfModel.EyeColor s)
                => s.EyeColorId == default
                && s.Code == default
                && s.Text == default
                && s.IsActive == default
                && s.SortOrder == default
                && s.RowVersion == default;
        }

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.PowerSource"/> to Entity Framework <see cref="Model.PowerSource"/> mapping.
        /// </summary>
        public partial class PowerSourceToModelCosmosMapper : Mapper<RefDataNamespace.PowerSource, Model.PowerSource>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PowerSourceToModelCosmosMapper"/> class.
            /// </summary>
            public PowerSourceToModelCosmosMapper()
            {
                Map((s, d) => d.Id = s.Id);
                Map((s, d) => d.Code = s.Code);
                Map((s, d) => d.Text = s.Text);
                Map((s, d) => d.IsActive = s.IsActive);
                Map((s, d) => d.SortOrder = s.SortOrder);
                Map((s, d) => d.ETag = s.ETag);
                Map((s, d) => d.AdditionalInfo = s.AdditionalInfo);
                PowerSourceToModelCosmosMapperCtor();
            }

            partial void PowerSourceToModelCosmosMapperCtor(); // Enables the constructor to be extended.
        }

        /// <summary>
        /// Provides the Entity Framework <see cref="Model.PowerSource"/> to <see cref="RefDataNamespace.PowerSource"/> mapping.
        /// </summary>
        public partial class ModelToPowerSourceCosmosMapper : Mapper<Model.PowerSource, RefDataNamespace.PowerSource>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ModelToEntityCosmosMapper"/> class.
            /// </summary>
            public ModelToPowerSourceCosmosMapper()
            {
                Map((s, d) => d.Id = (Guid)s.Id);
                Map((s, d) => d.Code = (string?)s.Code);
                Map((s, d) => d.Text = (string?)s.Text);
                Map((s, d) => d.IsActive = (bool)s.IsActive);
                Map((s, d) => d.SortOrder = (int)s.SortOrder);
                Map((s, d) => d.ETag = (string?)s.ETag);
                Map((s, d) => d.AdditionalInfo = (string?)s.AdditionalInfo);
                ModelToPowerSourceCosmosMapperCtor();
            }

            partial void ModelToPowerSourceCosmosMapperCtor(); // Enables the constructor to be extended.
        }

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.Status"/> to Entity Framework <see cref="EfModel.Status"/> mapping.
        /// </summary>
        public partial class StatusToModelEfMapper : Mapper<RefDataNamespace.Status, EfModel.Status>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StatusToModelEfMapper"/> class.
            /// </summary>
            public StatusToModelEfMapper()
            {
                Map((s, d) => d.StatusId = s.Id);
                Map((s, d) => d.Code = s.Code);
                Map((s, d) => d.Text = s.Text);
                Map((s, d) => d.IsActive = s.IsActive);
                Map((s, d) => d.SortOrder = s.SortOrder);
                Map((s, d) => d.RowVersion = StringToBase64Converter.Default.ToDestination.Convert(s.ETag));
                StatusToModelEfMapperCtor();
            }

            partial void StatusToModelEfMapperCtor(); // Enables the constructor to be extended.

            /// <inheritdoc/>
            public override bool IsSourceInitial(Status s)
                => s.Id == default
                && s.Code == default
                && s.Text == default
                && s.IsActive == default
                && s.SortOrder == default
                && s.ETag == default;
        }

        /// <summary>
        /// Provides the Entity Framework <see cref="EfModel.Status"/> to <see cref="RefDataNamespace.Status"/> mapping.
        /// </summary>
        public partial class ModelToStatusEfMapper : Mapper<EfModel.Status, RefDataNamespace.Status>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ModelToEntityEfMapper"/> class.
            /// </summary>
            public ModelToStatusEfMapper()
            {
                Map((s, d) => d.Id = (string?)s.StatusId);
                Map((s, d) => d.Code = (string?)s.Code);
                Map((s, d) => d.Text = (string?)s.Text);
                Map((s, d) => d.IsActive = (bool)s.IsActive);
                Map((s, d) => d.SortOrder = (int)s.SortOrder);
                Map((s, d) => d.ETag = (string?)StringToBase64Converter.Default.ToSource.Convert(s.RowVersion));
                ModelToStatusEfMapperCtor();
            }

            partial void ModelToStatusEfMapperCtor(); // Enables the constructor to be extended.

            /// <inheritdoc/>
            public override bool IsSourceInitial(EfModel.Status s)
                => s.StatusId == default
                && s.Code == default
                && s.Text == default
                && s.IsActive == default
                && s.SortOrder == default
                && s.RowVersion == default;
        }
    }
}

#pragma warning restore
#nullable restore