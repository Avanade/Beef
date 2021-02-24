// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.Entities
{
    /// <summary>
    /// Represents a default <see cref="string"/> identifier generator that uses <see cref="Guid.NewGuid"/> to create.
    /// </summary>
    public class StringIdentifierGenerator : IStringIdentifierGenerator
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <typeparam name="T"><inheritdoc/></typeparam>
        /// <returns><inheritdoc/></returns>
        public Task<string> GenerateIdentifierAsync<T>() => Task.FromResult(Guid.NewGuid().ToString());
    }
}