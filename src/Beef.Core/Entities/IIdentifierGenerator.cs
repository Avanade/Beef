// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.Entities
{
    /// <summary>
    /// Enables the base <c>GenerateIdentifier</c> capability.
    /// </summary>
    public interface IIdentifierGenerator { }

    /// <summary>
    /// Provides the <see cref="GenerateIdentifierAsync"/> for an <see cref="int"/>.
    /// </summary>
    public interface IIntIdentifierGenerator : IIdentifierGenerator
    {
        /// <summary>
        /// Generate a new <see cref="int"/> identifier.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/> that the identifier is required for.</typeparam>
        /// <returns>The newly generated identifier.</returns>
        Task<int> GenerateIdentifierAsync<T>();
    }

    /// <summary>
    /// Provides the <see cref="GenerateIdentifierAsync"/> for a <see cref="Guid"/>.
    /// </summary>
    public interface IGuidIdentifierGenerator : IIdentifierGenerator
    {
        /// <summary>
        /// Generate a new <see cref="Guid"/> identifier.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/> that the identifier is required for.</typeparam>
        /// <returns>The newly generated identifier.</returns>
        Task<Guid> GenerateIdentifierAsync<T>();
    }

    /// <summary>
    /// Provides the <see cref="GenerateIdentifierAsync"/> for a <see cref="string"/>.
    /// </summary>
    public interface IStringIdentifierGenerator : IIdentifierGenerator
    {
        /// <summary>
        /// Generate a new <see cref="int"/> identifier.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/> that the identifier is required for.</typeparam>
        /// <returns>The newly generated identifier.</returns>
        Task<string> GenerateIdentifierAsync<T>();
    }
}