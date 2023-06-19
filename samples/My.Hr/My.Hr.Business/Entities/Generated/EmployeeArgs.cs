/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace My.Hr.Business.Entities;

/// <summary>
/// Represents the <see cref="Employee"/> search arguments entity.
/// </summary>
public partial class EmployeeArgs : EntityBase
{
    private string? _firstName;
    private string? _lastName;
    private List<string?>? _gendersSids;
    private DateTime? _startFrom;
    private DateTime? _startTo;
    private bool? _isIncludeTerminated;

    /// <summary>
    /// Gets or sets the First Name.
    /// </summary>
    public string? FirstName { get => _firstName; set => SetValue(ref _firstName, value); }

    /// <summary>
    /// Gets or sets the Last Name.
    /// </summary>
    public string? LastName { get => _lastName; set => SetValue(ref _lastName, value); }

    /// <summary>
    /// Gets or sets the <see cref="Genders"/> list using the underlying Serialization Identifier (SID).
    /// </summary>
    [JsonPropertyName("genders")]
    public List<string?>? GendersSids { get => _gendersSids; set => SetValue(ref _gendersSids, value, propertyName: nameof(Genders)); }

    /// <summary>
    /// Gets or sets the Genders (see <see cref="RefDataNamespace.Gender"/>).
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [JsonIgnore]
    public ReferenceDataCodeList<RefDataNamespace.Gender>? Genders { get => new ReferenceDataCodeList<RefDataNamespace.Gender>(ref _gendersSids); set => SetValue(ref _gendersSids, value?.ToCodeList(), propertyName: nameof(Genders)); }

    /// <summary>
    /// Gets or sets the Start From.
    /// </summary>
    public DateTime? StartFrom { get => _startFrom; set => SetValue(ref _startFrom, value, transform: DateTimeTransform.DateOnly); }

    /// <summary>
    /// Gets or sets the Start To.
    /// </summary>
    public DateTime? StartTo { get => _startTo; set => SetValue(ref _startTo, value, transform: DateTimeTransform.DateOnly); }

    /// <summary>
    /// Indicates whether Is Include Terminated.
    /// </summary>
    [JsonPropertyName("includeTerminated")]
    public bool? IsIncludeTerminated { get => _isIncludeTerminated; set => SetValue(ref _isIncludeTerminated, value); }

    /// <inheritdoc/>
    protected override IEnumerable<IPropertyValue> GetPropertyValues()
    {
        yield return CreateProperty(nameof(FirstName), FirstName, v => FirstName = v);
        yield return CreateProperty(nameof(LastName), LastName, v => LastName = v);
        yield return CreateProperty(nameof(GendersSids), GendersSids, v => GendersSids = v);
        yield return CreateProperty(nameof(StartFrom), StartFrom, v => StartFrom = v);
        yield return CreateProperty(nameof(StartTo), StartTo, v => StartTo = v);
        yield return CreateProperty(nameof(IsIncludeTerminated), IsIncludeTerminated, v => IsIncludeTerminated = v);
    }
}

#pragma warning restore
#nullable restore