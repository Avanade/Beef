/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Business.Data.Model;

/// <summary>
/// Represents the Power Source model.
/// </summary>
public partial class PowerSource : ReferenceDataBase<Guid>
{
    /// <summary>
    /// Gets or sets the Additional Info.
    /// </summary>
    public string? AdditionalInfo { get; set; }
}


/// <summary>
/// Represents the <see cref="PowerSource"/> collection.
/// </summary>
public partial class PowerSourceCollection : List<PowerSource> { }


#pragma warning restore
#nullable restore