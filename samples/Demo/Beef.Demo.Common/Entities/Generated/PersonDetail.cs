/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Common.Entities;

/// <summary>
/// Represents the <c>Person</c> detail entity.
/// </summary>
public partial class PersonDetail : Person
{
    /// <summary>
    /// Gets or sets the History.
    /// </summary>
    public WorkHistoryCollection? History { get; set; }
}

/// <summary>
/// Represents the <c>PersonDetail</c> collection.
/// </summary>
public partial class PersonDetailCollection : List<PersonDetail> { }

/// <summary>
/// Represents the <c>PersonDetail</c> collection result.
/// </summary>
public class PersonDetailCollectionResult : CollectionResult<PersonDetailCollection, PersonDetail>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PersonDetailCollectionResult"/> class.
    /// </summary>
    public PersonDetailCollectionResult() { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PersonDetailCollectionResult"/> class with <paramref name="paging"/>.
    /// </summary>
    /// <param name="paging">The <see cref="PagingArgs"/>.</param>
    public PersonDetailCollectionResult(PagingArgs? paging) : base(paging) { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PersonDetailCollectionResult"/> class with <paramref name="items"/> to add.
    /// </summary>
    /// <param name="items">The items to add.</param>
    /// <param name="paging">The <see cref="PagingArgs"/>.</param>
    public PersonDetailCollectionResult(IEnumerable<PersonDetail> items, PagingArgs? paging = null) : base(paging) => Items.AddRange(items);
}


#pragma warning restore
#nullable restore