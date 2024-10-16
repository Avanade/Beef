/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace My.Hr.Common.Entities;

/// <summary>
/// Represents the <c>Employee</c> base entity.
/// </summary>
public partial class EmployeeBase : IIdentifier<Guid>
{
    /// <summary>
    /// Gets or sets the <c>Employee</c> identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the Unique <c>Employee</c> Email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the First Name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the Last Name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Gets the corresponding <c>Gender</c> text (read-only where selected).
    /// </summary>
    public string? GenderText { get; set; }

    /// <summary>
    /// Gets or sets the Gender code.
    /// </summary>
    public string? Gender { get; set; }

    /// <summary>
    /// Gets or sets the Birthday.
    /// </summary>
    public DateTime Birthday { get; set; }

    /// <summary>
    /// Gets or sets the Start Date.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the Termination.
    /// </summary>
    public TerminationDetail? Termination { get; set; }

    /// <summary>
    /// Gets or sets the Phone No.
    /// </summary>
    public string? PhoneNo { get; set; }
}

/// <summary>
/// Represents the <c>EmployeeBase</c> collection.
/// </summary>
public partial class EmployeeBaseCollection : List<EmployeeBase> { }

/// <summary>
/// Represents the <c>EmployeeBase</c> collection result.
/// </summary>
public class EmployeeBaseCollectionResult : CollectionResult<EmployeeBaseCollection, EmployeeBase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeBaseCollectionResult"/> class.
    /// </summary>
    public EmployeeBaseCollectionResult() { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeBaseCollectionResult"/> class with <paramref name="paging"/>.
    /// </summary>
    /// <param name="paging">The <see cref="PagingArgs"/>.</param>
    public EmployeeBaseCollectionResult(PagingArgs? paging) : base(paging) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeBaseCollectionResult"/> class with <paramref name="items"/> to add.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <param name="paging">The <see cref="PagingArgs"/>.</param>
    public EmployeeBaseCollectionResult(IEnumerable<EmployeeBase> items, PagingArgs? paging = null) : base(paging) => Items.AddRange(items);
}
