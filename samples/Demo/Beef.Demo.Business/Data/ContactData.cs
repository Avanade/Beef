using CoreEx.Data.Querying;

namespace Beef.Demo.Business.Data
{
    public partial class ContactData
    {
        private readonly static QueryArgsConfig _config = QueryArgsConfig.Create()
            .WithFilter(filter => filter
                .AddField<string>(nameof(Contact.LastName), c => c.WithOperators(QueryFilterOperator.AllStringOperators).WithUpperCase())
                .AddField<string>(nameof(Contact.FirstName), c => c.WithOperators(QueryFilterOperator.AllStringOperators).WithUpperCase())
                .AddReferenceDataField<Status>(nameof(Contact.Status), nameof(EfModel.Contact.StatusCode)))
            .WithOrderBy(orderBy => orderBy
                .AddField(nameof(Contact.LastName))
                .AddField(nameof(Contact.FirstName))
                .WithDefault($"{nameof(Contact.LastName)}, {nameof(Contact.FirstName)}"));

        partial void ContactDataCtor()
        {
            _getByQueryOnQuery = (q, a) => q.Where(_config, a).OrderBy(_config, a);
        }

        private Task RaiseEventOnImplementationAsync(bool throwError)
        {
            _events.PublishEvent("Contact", "Made");

            // The exception should result in a rollback of the database event enqueue - fingers crossed ;-)
            if (throwError)
                throw new DivideByZeroException();

            return Task.CompletedTask;
        }
    }
}