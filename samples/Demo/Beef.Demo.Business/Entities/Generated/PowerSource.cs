/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Business.Entities;

/// <summary>
/// Represents the Power Source entity.
/// </summary>
public partial class PowerSource : ReferenceDataBaseEx<Guid, PowerSource>
{
    private string? _additionalInfo;

    /// <summary>
    /// Gets or sets the Additional Info.
    /// </summary>
    public string? AdditionalInfo { get => _additionalInfo; set => SetValue(ref _additionalInfo, value); }

    /// <inheritdoc/>
    protected override IEnumerable<IPropertyValue> GetPropertyValues()
    {
        foreach (var pv in base.GetPropertyValues())
            yield return pv;

        yield return CreateProperty(nameof(AdditionalInfo), AdditionalInfo, v => AdditionalInfo = v);
    }

    /// <summary>
    /// An implicit cast from an <see cref="IIdentifier.Id"> to <see cref="PowerSource"/>.
    /// </summary>
    /// <param name="id">The <see cref="IIdentifier.Id">.</param>
    /// <returns>The corresponding <see cref="PowerSource"/>.</returns>
    public static implicit operator PowerSource?(Guid id) => ConvertFromId(id);

    /// <summary>
    /// An implicit cast from a <see cref="IReferenceData.Code"> to <see cref="PowerSource"/>.
    /// </summary>
    /// <param name="code">The <see cref="IReferenceData.Code">.</param>
    /// <returns>The corresponding <see cref="PowerSource"/>.</returns>
    public static implicit operator PowerSource?(string? code) => ConvertFromCode(code);
}

/// <summary>
/// Represents the <see cref="PowerSource"/> collection.
/// </summary>
public partial class PowerSourceCollection : ReferenceDataCollectionBase<Guid, PowerSource, PowerSourceCollection>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PowerSourceCollection"/> class.
    /// </summary>
    public PowerSourceCollection() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PowerSourceCollection"/> class with <paramref name="items"/> to add.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public PowerSourceCollection(IEnumerable<PowerSource> items) => AddRange(items);
}

#pragma warning restore
#nullable restore