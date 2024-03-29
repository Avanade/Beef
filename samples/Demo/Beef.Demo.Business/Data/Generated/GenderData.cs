/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Business.Data;

/// <summary>
/// Provides the <see cref="Gender"/> data access.
/// </summary>
public partial class GenderData : IGenderData
{
    private readonly IDatabase _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenderData"/> class.
    /// </summary>
    /// <param name="db">The <see cref="IDatabase"/>.</param>
    public GenderData(IDatabase db)
        { _db = db.ThrowIfNull(); GenderDataCtor(); }

    partial void GenderDataCtor(); // Enables additional functionality to be added to the constructor.

    /// <inheritdoc/>
    public Task<Result<Gender?>> GetAsync(Guid id)
        => _db.StoredProcedure("[Ref].[spGenderGet]").GetWithResultAsync(DbMapper.Default, id);

    /// <inheritdoc/>
    public Task<Result<Gender>> CreateAsync(Gender value)
        => _db.StoredProcedure("[Ref].[spGenderCreate]").CreateWithResultAsync(DbMapper.Default, value);

    /// <inheritdoc/>
    public Task<Result<Gender>> UpdateAsync(Gender value)
        => _db.StoredProcedure("[Ref].[spGenderUpdate]").UpdateWithResultAsync(DbMapper.Default, value);

    /// <summary>
    /// Provides the <see cref="Gender"/> property and database column mapping.
    /// </summary>
    public partial class DbMapper : DatabaseMapperEx<Gender, DbMapper>
    {
        /// <inheritdoc />
        protected override void OnMapToDb(Gender value, DatabaseParameterCollection parameters, OperationTypes operationType)
        {
            parameters.AddParameter("GenderId", value.Id).SetDirectionToOutputOnCreate(operationType);
            parameters.AddParameter("Code", value.Code);
            parameters.AddParameter("Text", value.Text);
            parameters.AddParameter("IsActive", value.IsActive);
            parameters.AddParameter("SortOrder", value.SortOrder);
            parameters.AddParameter("AlternateName", value.AlternateName);
            parameters.AddParameter("TripCode", value.TripCode);
            parameters.AddParameter("CountryId", ReferenceDataIdConverter<RefDataNamespace.Country, Guid?>.Default.ConvertToDestination(value.Country));
            WhenAnyExceptCreate(operationType, () => parameters.AddParameter(parameters.Database.DatabaseColumns.RowVersionName, StringToBase64Converter.Default.ConvertToDestination(value.ETag)));
            OnMapToDbEx(value, parameters, operationType);
        }

        /// <inheritdoc />
        protected override void OnMapFromDb(DatabaseRecord record, Gender value, OperationTypes operationType)
        {
            value.Id = record.GetValue<Guid>("GenderId");
            value.Code = record.GetValue<string?>("Code");
            value.Text = record.GetValue<string?>("Text");
            value.IsActive = record.GetValue<bool>("IsActive");
            value.SortOrder = record.GetValue<int>("SortOrder");
            value.AlternateName = record.GetValue<string?>("AlternateName");
            value.TripCode = record.GetValue<string?>("TripCode");
            value.Country = (RefDataNamespace.Country?)ReferenceDataIdConverter<RefDataNamespace.Country, Guid?>.Default.ConvertToSource(record.GetValue("CountryId"));
            WhenAnyExceptCreate(operationType, () => value.ETag = (string?)StringToBase64Converter.Default.ConvertToSource(record.GetValue(record.Database.DatabaseColumns.RowVersionName)));
            OnMapFromDbEx(record, value, operationType);
        }

        /// <inheritdoc />
        protected override void OnMapKeyToDb(CompositeKey key, DatabaseParameterCollection parameters)
        {
            key.AssertLength(1);
            parameters.AddParameter("GenderId", key.Args[0]);
            OnMapKeyToDbEx(key, parameters);
        }

        partial void OnMapToDbEx(Gender value, DatabaseParameterCollection parameters, OperationTypes operationType); // Enables the DbMapper.OnMapToDb to be extended.
        partial void OnMapFromDbEx(DatabaseRecord record, Gender value, OperationTypes operationType); // Enables the DbMapper.OnMapFromDb to be extended.
        partial void OnMapKeyToDbEx(CompositeKey key, DatabaseParameterCollection parameters); // Enables the DbMapper.OnMapKeyToDb to be extended.
    }
}

#pragma warning restore
#nullable restore