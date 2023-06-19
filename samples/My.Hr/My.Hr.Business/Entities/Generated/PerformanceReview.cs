/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace My.Hr.Business.Entities;

/// <summary>
/// Represents the Performance Review entity.
/// </summary>
public partial class PerformanceReview : EntityBase, IIdentifier<Guid>, IETag, IChangeLogEx
{
    private Guid _id;
    private Guid _employeeId;
    private DateTime _date;
    private string? _outcomeSid;
    private string? _reviewer;
    private string? _notes;
    private string? _etag;
    private ChangeLogEx? _changeLog;

    /// <summary>
    /// Gets or sets the <see cref="Employee"/> identifier.
    /// </summary>
    public Guid Id { get => _id; set => SetValue(ref _id, value); }

    /// <summary>
    /// Gets or sets the <see cref="Employee.Id"/> (value is immutable).
    /// </summary>
    public Guid EmployeeId { get => _employeeId; set => SetValue(ref _employeeId, value); }

    /// <summary>
    /// Gets or sets the Date.
    /// </summary>
    public DateTime Date { get => _date; set => SetValue(ref _date, value); }

    /// <summary>
    /// Gets or sets the <see cref="Outcome"/> using the underlying Serialization Identifier (SID).
    /// </summary>
    [JsonPropertyName("outcome")]
    public string? OutcomeSid { get => _outcomeSid; set => SetValue(ref _outcomeSid, value, propertyName: nameof(Outcome)); }

    /// <summary>
    /// Gets the corresponding <see cref="Outcome"/> text (read-only where selected).
    /// </summary>
    public string? OutcomeText => RefDataNamespace.PerformanceOutcome.GetRefDataText(_outcomeSid);

    /// <summary>
    /// Gets or sets the Outcome (see <see cref="RefDataNamespace.PerformanceOutcome"/>).
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    [JsonIgnore]
    public RefDataNamespace.PerformanceOutcome? Outcome { get => _outcomeSid; set => SetValue(ref _outcomeSid, value); }

    /// <summary>
    /// Gets or sets the Reviewer.
    /// </summary>
    public string? Reviewer { get => _reviewer; set => SetValue(ref _reviewer, value); }

    /// <summary>
    /// Gets or sets the Notes.
    /// </summary>
    public string? Notes { get => _notes; set => SetValue(ref _notes, value); }

    /// <summary>
    /// Gets or sets the ETag.
    /// </summary>
    [JsonPropertyName("etag")]
    public string? ETag { get => _etag; set => SetValue(ref _etag, value); }

    /// <summary>
    /// Gets or sets the Change Log (see <see cref="CoreEx.Entities.ChangeLog"/>).
    /// </summary>
    public ChangeLogEx? ChangeLog { get => _changeLog; set => SetValue(ref _changeLog, value); }

    /// <inheritdoc/>
    protected override IEnumerable<IPropertyValue> GetPropertyValues()
    {
        yield return CreateProperty(nameof(Id), Id, v => Id = v);
        yield return CreateProperty(nameof(EmployeeId), EmployeeId, v => EmployeeId = v);
        yield return CreateProperty(nameof(Date), Date, v => Date = v);
        yield return CreateProperty(nameof(OutcomeSid), OutcomeSid, v => OutcomeSid = v);
        yield return CreateProperty(nameof(Reviewer), Reviewer, v => Reviewer = v);
        yield return CreateProperty(nameof(Notes), Notes, v => Notes = v);
        yield return CreateProperty(nameof(ETag), ETag, v => ETag = v);
        yield return CreateProperty(nameof(ChangeLog), ChangeLog, v => ChangeLog = v);
    }
}

/// <summary>
/// Represents the <see cref="PerformanceReview"/> collection.
/// </summary>
public partial class PerformanceReviewCollection : EntityBaseCollection<PerformanceReview, PerformanceReviewCollection>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceReviewCollection"/> class.
    /// </summary>
    public PerformanceReviewCollection() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceReviewCollection"/> class with <paramref name="items"/> to add.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public PerformanceReviewCollection(IEnumerable<PerformanceReview> items) => AddRange(items);
}

/// <summary>
/// Represents the <see cref="PerformanceReview"/> collection result.
/// </summary>
public class PerformanceReviewCollectionResult : EntityCollectionResult<PerformanceReviewCollection, PerformanceReview, PerformanceReviewCollectionResult>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceReviewCollectionResult"/> class.
    /// </summary>
    public PerformanceReviewCollectionResult() { }
        
    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceReviewCollectionResult"/> class with <paramref name="paging"/>.
    /// </summary>
    /// <param name="paging">The <see cref="PagingArgs"/>.</param>
    public PerformanceReviewCollectionResult(PagingArgs? paging) : base(paging) { }
        
    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceReviewCollectionResult"/> class with <paramref name="items"/> to add.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <param name="paging">The optional <see cref="PagingArgs"/>.</param>
    public PerformanceReviewCollectionResult(IEnumerable<PerformanceReview> items, PagingArgs? paging = null) : base(paging) => Items.AddRange(items);
}

#pragma warning restore
#nullable restore