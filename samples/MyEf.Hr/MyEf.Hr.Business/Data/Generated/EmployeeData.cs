/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace MyEf.Hr.Business.Data;

/// <summary>
/// Provides the <see cref="Employee"/> data access.
/// </summary>
public partial class EmployeeData : IEmployeeData
{
    private readonly IEfDb _ef;
    private Func<IQueryable<EfModel.Employee>, EmployeeArgs?, IQueryable<EfModel.Employee>>? _getByArgsOnQuery;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeData"/> class.
    /// </summary>
    /// <param name="ef">The <see cref="IEfDb"/>.</param>
    public EmployeeData(IEfDb ef)
        { _ef = ef.ThrowIfNull(); EmployeeDataCtor(); }

    partial void EmployeeDataCtor(); // Enables additional functionality to be added to the constructor.

    /// <inheritdoc/>
    public Task<Result<Employee?>> GetAsync(Guid id)
        => _ef.GetWithResultAsync<Employee, EfModel.Employee>(id);

    /// <inheritdoc/>
    public Task<Result<Employee>> CreateAsync(Employee value)
        => _ef.CreateWithResultAsync<Employee, EfModel.Employee>(value);

    /// <inheritdoc/>
    public Task<Result<Employee>> UpdateAsync(Employee value)
        => _ef.UpdateWithResultAsync<Employee, EfModel.Employee>(value);

    /// <inheritdoc/>
    public Task<Result> DeleteAsync(Guid id)
        => _ef.DeleteWithResultAsync<Employee, EfModel.Employee>(id);

    /// <inheritdoc/>
    public Task<Result<EmployeeBaseCollectionResult>> GetByArgsAsync(EmployeeArgs? args, PagingArgs? paging)
        => _ef.Query<EmployeeBase, EfModel.Employee>(q => _getByArgsOnQuery?.Invoke(q, args) ?? q).WithPaging(paging).SelectResultWithResultAsync<EmployeeBaseCollectionResult, EmployeeBaseCollection>();

    /// <inheritdoc/>
    public Task<Result<Employee>> TerminateAsync(TerminationDetail value, Guid id) => TerminateOnImplementationAsync(value, id);

    /// <summary>
    /// Provides the <see cref="Employee"/> to Entity Framework <see cref="EfModel.Employee"/> mapping.
    /// </summary>
    public partial class EntityToModelEfMapper : Mapper<Employee, EfModel.Employee>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityToModelEfMapper"/> class.
        /// </summary>
        public EntityToModelEfMapper()
        {
            Base<EmployeeBaseData.EntityToModelEfMapper>();
            Map((s, d) => d.AddressJson = ObjectToJsonConverter<Address>.Default.ToDestination.Convert(s.Address), OperationTypes.Any, s => s.Address == default, d => d.AddressJson = default);
            Map((o, s, d) => d.EmergencyContacts = o.Map(s.EmergencyContacts, d.EmergencyContacts), OperationTypes.Any, s => s.EmergencyContacts == default, d => d.EmergencyContacts = default);
            Map((s, d) => d.RowVersion = StringToBase64Converter.Default.ToDestination.Convert(s.ETag), OperationTypes.Any, s => s.ETag == default, d => d.RowVersion = default);
            Flatten(s => s.ChangeLog, OperationTypes.Any, s => s.ChangeLog == default);
            EntityToModelEfMapperCtor();
        }

        partial void EntityToModelEfMapperCtor(); // Enables the constructor to be extended.

        /// <inheritdoc/>
        protected override void OnRegister(Mapper<Employee, EfModel.Employee> mapper) => mapper.Owner.Register(new Mapper<ChangeLogEx, EfModel.Employee>()
            .Map((s, d) => d.CreatedBy = s.CreatedBy, OperationTypes.AnyExceptUpdate)
            .Map((s, d) => d.CreatedDate = s.CreatedDate, OperationTypes.AnyExceptUpdate)
            .Map((s, d) => d.UpdatedBy = s.UpdatedBy, OperationTypes.AnyExceptCreate)
            .Map((s, d) => d.UpdatedDate = s.UpdatedDate, OperationTypes.AnyExceptCreate));
    }

    /// <summary>
    /// Provides the Entity Framework <see cref="EfModel.Employee"/> to <see cref="Employee"/> mapping.
    /// </summary>
    public partial class ModelToEntityEfMapper : Mapper<EfModel.Employee, Employee>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelToEntityEfMapper"/> class.
        /// </summary>
        public ModelToEntityEfMapper()
        {
            Base<EmployeeBaseData.ModelToEntityEfMapper>();
            Map((s, d) => d.Address = (Address?)ObjectToJsonConverter<Address>.Default.ToSource.Convert(s.AddressJson!), OperationTypes.Any, s => s.AddressJson == default, d => d.Address = default);
            Map((o, s, d) => d.EmergencyContacts = o.Map(s.EmergencyContacts, d.EmergencyContacts), OperationTypes.Any, s => s.EmergencyContacts == default, d => d.EmergencyContacts = default);
            Map((s, d) => d.ETag = (string?)StringToBase64Converter.Default.ToSource.Convert(s.RowVersion!), OperationTypes.Any, s => s.RowVersion == default, d => d.ETag = default);
            Expand<ChangeLogEx>((d, v) => d.ChangeLog = v, OperationTypes.Any, d => d.ChangeLog = default);
            ModelToEntityEfMapperCtor();
        }

        partial void ModelToEntityEfMapperCtor(); // Enables the constructor to be extended.

        /// <inheritdoc/>
        protected override void OnRegister(Mapper<EfModel.Employee, Employee> mapper) => mapper.Owner.Register(new Mapper<EfModel.Employee, ChangeLogEx>()
            .Map((s, d) => d.CreatedBy = s.CreatedBy, OperationTypes.AnyExceptUpdate)
            .Map((s, d) => d.CreatedDate = s.CreatedDate, OperationTypes.AnyExceptUpdate)
            .Map((s, d) => d.UpdatedBy = s.UpdatedBy, OperationTypes.AnyExceptCreate)
            .Map((s, d) => d.UpdatedDate = s.UpdatedDate, OperationTypes.AnyExceptCreate));
    }
}

#pragma warning restore
#nullable restore