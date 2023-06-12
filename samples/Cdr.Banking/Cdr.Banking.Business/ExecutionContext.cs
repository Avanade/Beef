namespace Cdr.Banking.Business
{
    /// <summary>
    /// Extended <see cref="Beef.ExecutionContext"/> that stores the list of <see cref="Accounts"/> that a user has access to.
    /// </summary>
    public class ExecutionContext : CoreEx.ExecutionContext
    {
        /// <summary>
        /// Gets the current <see cref="ExecutionContext"/> instance.
        /// </summary>
        public static new ExecutionContext Current => (ExecutionContext)CoreEx.ExecutionContext.Current;

        /// <summary>
        /// Gets the list of account (identifiers) that the user has access/permission to.
        /// </summary>
        public List<string> Accounts { get; } = new List<string>();
    }
}