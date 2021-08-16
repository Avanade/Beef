using Beef.Entities;

namespace Beef.Database.Core
{
    /// <summary>
    /// Provides the identifier generators for use when creating data.
    /// </summary>
    public interface IIdentifierGenerators
    {
        /// <summary>
        /// Gets the <see cref="IStringIdentifierGenerator"/>.
        /// </summary>
        IStringIdentifierGenerator? StringGenerator { get => null; }

        /// <summary>
        /// Gets the <see cref="IGuidIdentifierGenerator"/>.
        /// </summary>
        IGuidIdentifierGenerator? GuidGenerator { get => null; }

        /// <summary>
        /// Gets the <see cref="IInt32IdentifierGenerator"/>.
        /// </summary>
        IInt32IdentifierGenerator? IntGenerator { get => null; }
    }

    /// <summary>
    /// Provides the default identifier generators; <see cref="GuidGenerator"/> is set to <see cref="GuidIdentifierGenerator"/> and <see cref="StringGenerator"/> is set to <see cref="StringIdentifierGenerator"/>.
    /// </summary>
    public class DefaultIdentifierGenerators : IIdentifierGenerators
    {
        /// <summary>
        /// Gets the <see cref="IGuidIdentifierGenerator"/>.
        /// </summary>
        public IGuidIdentifierGenerator? GuidGenerator { get => new GuidIdentifierGenerator(); }

        /// <summary>
        /// Gets the <see cref="IStringIdentifierGenerator"/>.
        /// </summary>
        public IStringIdentifierGenerator? StringGenerator { get => new StringIdentifierGenerator(); }
    }
}