// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Provides the base <b>DocumentDb/CosmosDb</b> value capabilities.
    /// </summary>
    public abstract class CosmosDbValue : IStringIdentifier, IETag
    {
        /// <summary>
        /// Gets or sets the <see cref="IStringIdentifier"/>.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Type"/> name.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IETag"/>.
        /// </summary>
        [JsonProperty("_etag")]
        public string ETag { get; set; }

        /// <summary>
        /// Prepares the object before a persistence operation.
        /// </summary>
        internal protected abstract void PrepareBefore();

        /// <summary>
        /// Prepares the object after a persistence operation.
        /// </summary>
        internal protected abstract void PrepareAfter();
    }

    /// <summary>
    /// Represents a <b>DocumentDb/CosmosDb</b> <see cref="Value"/> that is wrapped, including <see cref="Type"/> name, for persistence.
    /// </summary>
    public class CosmosDbValue<T> : CosmosDbValue where T : IIdentifier
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [JsonProperty("value")]
        public T Value { get; set; }

        /// <summary>
        /// Prepares the object before a persistence operation.
        /// </summary>
        internal protected override void PrepareBefore()
        {
            if (Value == default)
                Id = Guid.NewGuid().ToString();
            else
            {
                switch (Value)
                {
                    case IStringIdentifier isi:
                        Id = isi.Id;
                        break;

                    case IIntIdentifier iii:
                        Id = iii.Id.ToString();
                        break;

                    case IGuidIdentifier igi:
                        Id = igi.Id.ToString();
                        break;

                    default:
                        throw new InvalidOperationException("Value is an unknown IIdentifier.");
                }

                if (Value is IETag etag)
                    ETag = etag.ETag;
            }

            Type = typeof(T).Name;
        }

        /// <summary>
        /// Prepares the object after a persistence operation.
        /// </summary>
        internal protected override void PrepareAfter()
        {
            if (Value == default)
                return;

            switch (Value)
            {
                case IStringIdentifier isi:
                    isi.Id = Id;
                    break;

                case IIntIdentifier iii:
                    iii.Id = int.Parse(Id);
                    break;

                case IGuidIdentifier igi:
                    igi.Id = Guid.Parse(Id);
                    break;

                default:
                    throw new InvalidOperationException("Value is an unknown IIdentifier.");
            }

            if (Value is IETag etag)
                etag.ETag = ETag;
        }
    }
}