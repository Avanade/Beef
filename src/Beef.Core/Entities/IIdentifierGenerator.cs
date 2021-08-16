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
    /// Provides the <see cref="GenerateIdentifierAsync"/> for an <see cref="Int32"/>.
    /// </summary>
    public interface IInt32IdentifierGenerator : IIdentifierGenerator
    {
        /// <summary>
        /// Generate a new <see cref="Int32"/> identifier for a specified <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/> that the identifier is required for.</typeparam>
        /// <returns>The newly generated identifier.</returns>
        Task<int> GenerateIdentifierAsync<T>();

        /// <summary>
        /// Generate a new <see cref="Int32"/> identifier for a default <see cref="object"/> <see cref="Type"/>.
        /// </summary>
        /// <returns>The newly generated identifier.</returns>
        Task<int> GenerateIdentifierAsync() => GenerateIdentifierAsync<object>();
    }

    /// <summary>
    /// Provides the <see cref="GenerateIdentifierAsync"/> for an <see cref="Int64"/>.
    /// </summary>
    public interface IInt64IdentifierGenerator : IIdentifierGenerator
    {
        /// <summary>
        /// Generate a new <see cref="Int64"/> identifier for a specified <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/> that the identifier is required for.</typeparam>
        /// <returns>The newly generated identifier.</returns>
        Task<long> GenerateIdentifierAsync<T>();

        /// <summary>
        /// Generate a new <see cref="Int64"/> identifier for a default <see cref="object"/> <see cref="Type"/>.
        /// </summary>
        /// <returns>The newly generated identifier.</returns>
        Task<long> GenerateIdentifierAsync() => GenerateIdentifierAsync<object>();
    }

    /// <summary>
    /// Provides the <see cref="GenerateIdentifierAsync"/> for a <see cref="Guid"/>.
    /// </summary>
    public interface IGuidIdentifierGenerator : IIdentifierGenerator
    {
        /// <summary>
        /// Generate a new <see cref="Guid"/> identifier for a specified <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/> that the identifier is required for.</typeparam>
        /// <returns>The newly generated identifier.</returns>
        Task<Guid> GenerateIdentifierAsync<T>();

        /// <summary>
        /// Generate a new <see cref="Guid"/> identifier for a default <see cref="object"/> <see cref="Type"/>.
        /// </summary>
        /// <returns>The newly generated identifier.</returns>
        Task<Guid> GenerateIdentifierAsync() => GenerateIdentifierAsync<object>();
    }

    /// <summary>
    /// Provides the <see cref="GenerateIdentifierAsync"/> for a <see cref="string"/>.
    /// </summary>
    public interface IStringIdentifierGenerator : IIdentifierGenerator
    {
        /// <summary>
        /// Generate a new <see cref="string"/> identifier for a specified <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/> that the identifier is required for.</typeparam>
        /// <returns>The newly generated identifier.</returns>
        Task<string> GenerateIdentifierAsync<T>();

        /// <summary>
        /// Generate a new <see cref="string"/> identifier for default <see cref="object"/> <see cref="Type"/>.
        /// </summary>
        /// <returns>The newly generated identifier.</returns>
        Task<string> GenerateIdentifierAsync() => GenerateIdentifierAsync<object>();
    }
}