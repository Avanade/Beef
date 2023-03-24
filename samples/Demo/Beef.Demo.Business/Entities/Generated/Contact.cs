/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Business.Entities
{
    /// <summary>
    /// Represents the Contact entity.
    /// </summary>
    public partial class Contact : EntityBase, IIdentifier<Guid>
    {
        private Guid _id;
        private string? _firstName;
        private string? _lastName;
        private string? _statusSid;
        private string? _internalCode;
        private ContactCommCollection? _communications;

        /// <summary>
        /// Gets or sets the <see cref="Contact"/> identifier.
        /// </summary>
        public Guid Id { get => _id; set => SetValue(ref _id, value); }

        /// <summary>
        /// Gets or sets the First Name.
        /// </summary>
        public string? FirstName { get => _firstName; set => SetValue(ref _firstName, value); }

        /// <summary>
        /// Gets or sets the Last Name.
        /// </summary>
        public string? LastName { get => _lastName; set => SetValue(ref _lastName, value); }

        /// <summary>
        /// Gets or sets the <see cref="Status"/> using the underlying Serialization Identifier (SID).
        /// </summary>
        [JsonPropertyName("status")]
        public string? StatusSid { get => _statusSid; set => SetValue(ref _statusSid, value, propertyName: nameof(Status)); }

        /// <summary>
        /// Gets the corresponding <see cref="Status"/> text (read-only where selected).
        /// </summary>
        public string? StatusDescription => Status?.Text;

        /// <summary>
        /// Gets or sets the Status (see <see cref="RefDataNamespace.Status"/>).
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [JsonIgnore]
        public RefDataNamespace.Status? Status { get => _statusSid; set => SetValue(ref _statusSid, value); }

        /// <summary>
        /// Gets or sets the Internal Code.
        /// </summary>
        [JsonIgnore]
        public string? InternalCode { get => _internalCode; set => SetValue(ref _internalCode, value); }

        /// <summary>
        /// Gets or sets the Communications.
        /// </summary>
        public ContactCommCollection? Communications { get => _communications; set => SetValue(ref _communications, value); }

        /// <inheritdoc/>
        protected override IEnumerable<IPropertyValue> GetPropertyValues()
        {
            yield return CreateProperty(nameof(Id), Id, v => Id = v);
            yield return CreateProperty(nameof(FirstName), FirstName, v => FirstName = v);
            yield return CreateProperty(nameof(LastName), LastName, v => LastName = v);
            yield return CreateProperty(nameof(StatusSid), StatusSid, v => StatusSid = v);
            yield return CreateProperty(nameof(InternalCode), InternalCode, v => InternalCode = v);
            yield return CreateProperty(nameof(Communications), Communications, v => Communications = v);
        }
    }

    /// <summary>
    /// Represents the <see cref="Contact"/> collection.
    /// </summary>
    public partial class ContactCollection : EntityBaseCollection<Contact, ContactCollection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContactCollection"/> class.
        /// </summary>
        public ContactCollection() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactCollection"/> class with <paramref name="items"/> to add.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public ContactCollection(IEnumerable<Contact> items) => AddRange(items);
    }

    /// <summary>
    /// Represents the <see cref="Contact"/> collection result.
    /// </summary>
    public class ContactCollectionResult : EntityCollectionResult<ContactCollection, Contact, ContactCollectionResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContactCollectionResult"/> class.
        /// </summary>
        public ContactCollectionResult() { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ContactCollectionResult"/> class with <paramref name="paging"/>.
        /// </summary>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        public ContactCollectionResult(PagingArgs? paging) : base(paging) { }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ContactCollectionResult"/> class with <paramref name="items"/> to add.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <param name="paging">The optional <see cref="PagingArgs"/>.</param>
        public ContactCollectionResult(IEnumerable<Contact> items, PagingArgs? paging = null) : base(paging) => Items.AddRange(items);
    }
}

#pragma warning restore
#nullable restore