namespace Beef.Demo.Business
{
    public partial class PersonManager
    {
        partial void PersonManagerCtor()
        {
            _updateOnPreValidateAsync = UpdateOnPreValidateAsync;
        }

        private static Task AddOnImplementationAsync(Person value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return Task.CompletedTask;
        }

        private static Task<Person?> ManagerCustomOnImplementationAsync()
        {
            return Task.FromResult<Person?>(null);
        }

        private async Task UpdateOnPreValidateAsync(Person value, Guid id)
        {
            var curr = await GetAsync(id).ConfigureAwait(false);
            //if (ReferenceEquals(value, curr))
            //    throw new InvalidOperationException("The Get and Update person should not have the same reference!");
        }
    }
}