/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Provides the <see cref="Contact"/> data access.
    /// </summary>
    public partial class ContactData : IContactData
    {
        private readonly IEfDb _ef;
        private readonly IEventPublisher _events;
        private Func<IQueryable<EfModel.Contact>, IQueryable<EfModel.Contact>>? _getAllOnQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactData"/> class.
        /// </summary>
        /// <param name="ef">The <see cref="IEfDb"/>.</param>
        /// <param name="events">The <see cref="IEventPublisher"/>.</param>
        public ContactData(IEfDb ef, IEventPublisher events)
            { _ef = ef.ThrowIfNull(); _events = events.ThrowIfNull(); ContactDataCtor(); }

        partial void ContactDataCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the <see cref="ContactCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <returns>The <see cref="ContactCollectionResult"/>.</returns>
        public Task<ContactCollectionResult> GetAllAsync()
        {
            return _ef.Query<Contact, EfModel.Contact>(q => _getAllOnQuery?.Invoke(q) ?? q).SelectResultAsync<ContactCollectionResult, ContactCollection>();
        }

        /// <summary>
        /// Gets the specified <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        /// <returns>The selected <see cref="Contact"/> where found.</returns>
        public Task<Contact?> GetAsync(Guid id)
        {
            return _ef.GetAsync<Contact, EfModel.Contact>(id);
        }

        /// <summary>
        /// Creates a new <see cref="Contact"/>.
        /// </summary>
        /// <param name="value">The <see cref="Contact"/>.</param>
        /// <returns>The created <see cref="Contact"/>.</returns>
        public Task<Contact> CreateAsync(Contact value) => DataInvoker.Current.InvokeAsync(this, async _ => 
        {
            var r = await _ef.CreateAsync<Contact, EfModel.Contact>(value).ConfigureAwait(false);
            _events.PublishValueEvent(r, new Uri($"/contact/{r.Id}", UriKind.Relative), $"Demo.Contact", "Create");
            return r;
        }, new InvokerArgs { EventPublisher = _events });

        /// <summary>
        /// Updates an existing <see cref="Contact"/>.
        /// </summary>
        /// <param name="value">The <see cref="Contact"/>.</param>
        /// <returns>The updated <see cref="Contact"/>.</returns>
        public Task<Contact> UpdateAsync(Contact value) => DataInvoker.Current.InvokeAsync(this, async _ => 
        {
            var r = await _ef.UpdateAsync<Contact, EfModel.Contact>(value).ConfigureAwait(false);
            _events.PublishValueEvent(r, new Uri($"/contact/{r.Id}", UriKind.Relative), $"Demo.Contact", "Update");
            return r;
        }, new InvokerArgs { EventPublisher = _events });

        /// <summary>
        /// Deletes the specified <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        public Task DeleteAsync(Guid id) => DataInvoker.Current.InvokeAsync(this, async _ => 
        {
            await _ef.DeleteAsync<Contact, EfModel.Contact>(id).ConfigureAwait(false);
            _events.PublishValueEvent(new Contact { Id = id }, new Uri($"/contact/{id}", UriKind.Relative), $"Demo.Contact", "Delete");
        }, new InvokerArgs { EventPublisher = _events });

        /// <summary>
        /// Raise Event.
        /// </summary>
        /// <param name="throwError">Indicates whether throw a DivideByZero exception.</param>
        public Task RaiseEventAsync(bool throwError) => DataInvoker.Current.InvokeAsync(this, async _ => 
        {
            await RaiseEventOnImplementationAsync(throwError);
        }, new InvokerArgs { EventPublisher = _events });

        /// <summary>
        /// Provides the <see cref="Contact"/> to Entity Framework <see cref="EfModel.Contact"/> mapping.
        /// </summary>
        public partial class EntityToModelEfMapper : Mapper<Contact, EfModel.Contact>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EntityToModelEfMapper"/> class.
            /// </summary>
            public EntityToModelEfMapper()
            {
                Map((s, d) => d.ContactId = s.Id, OperationTypes.Any, s => s.Id == default, d => d.ContactId = default);
                Map((s, d) => d.FirstName = s.FirstName, OperationTypes.Any, s => s.FirstName == default, d => d.FirstName = default);
                Map((s, d) => d.LastName = s.LastName, OperationTypes.Any, s => s.LastName == default, d => d.LastName = default);
                Map((s, d) => d.StatusCode = s.StatusSid, OperationTypes.Any, s => s.StatusSid == default, d => d.StatusCode = default);
                Map((s, d) => d.Comms = ObjectToJsonConverter<ContactCommCollection>.Default.ToDestination.Convert(s.Communications), OperationTypes.Any, s => s.Communications == default, d => d.Comms = default);
                EntityToModelEfMapperCtor();
            }

            partial void EntityToModelEfMapperCtor(); // Enables the constructor to be extended.
        }

        /// <summary>
        /// Provides the Entity Framework <see cref="EfModel.Contact"/> to <see cref="Contact"/> mapping.
        /// </summary>
        public partial class ModelToEntityEfMapper : Mapper<EfModel.Contact, Contact>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ModelToEntityEfMapper"/> class.
            /// </summary>
            public ModelToEntityEfMapper()
            {
                Map((s, d) => d.Id = (Guid)s.ContactId, OperationTypes.Any, s => s.ContactId == default, d => d.Id = default);
                Map((s, d) => d.FirstName = (string?)s.FirstName, OperationTypes.Any, s => s.FirstName == default, d => d.FirstName = default);
                Map((s, d) => d.LastName = (string?)s.LastName, OperationTypes.Any, s => s.LastName == default, d => d.LastName = default);
                Map((s, d) => d.StatusSid = (string?)s.StatusCode, OperationTypes.Any, s => s.StatusCode == default, d => d.StatusSid = default);
                Map((s, d) => d.Communications = (ContactCommCollection?)ObjectToJsonConverter<ContactCommCollection>.Default.ToSource.Convert(s.Comms), OperationTypes.Any, s => s.Comms == default, d => d.Communications = default);
                ModelToEntityEfMapperCtor();
            }

            partial void ModelToEntityEfMapperCtor(); // Enables the constructor to be extended.
        }
    }
}

#pragma warning restore
#nullable restore