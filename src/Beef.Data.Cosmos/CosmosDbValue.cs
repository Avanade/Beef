// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Provides the base <b>CosmosDb/DocumentDb</b> <see cref="Type"/> and <b>Value</b> capabilities.
    /// </summary>
    public abstract class CosmosDbTypeValue : IStringIdentifier, IETag
    {
        /// <summary>
        /// Gets or sets the <see cref="IStringIdentifier"/> (automatically set/synchronized before sending to Cosmos).
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Type"/> name (automatically set/synchronized before sending to Cosmos).
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IETag"/> (automatically set/synchronized before sending to Cosmos).
        /// </summary>
        [JsonProperty("_etag")]
        public string ETag { get; set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <returns>The value.</returns>
        public abstract object GetValue();

        /// <summary>
        /// Prepares the object before sending to Cosmos.
        /// </summary>
        internal protected abstract void PrepareBefore();

        /// <summary>
        /// Prepares the object after getting from Cosmos.
        /// </summary>
        internal protected abstract void PrepareAfter();
    }

    /// <summary>
    /// Represents a <b>CosmosDb/DocumentDb</b> <see cref="Value"/> that is wrapped, including <see cref="Type"/> name, for persistence.
    /// </summary>
    /// <remarks>The <see cref="CosmosDbTypeValue.Id"/>, <see cref="CosmosDbTypeValue.Type"/> and <see cref="CosmosDbTypeValue.ETag"/> are updated when the <see cref="Value"/> is set, and before
    /// sending to <b>CosmosDB/DocumentDb</b>.</remarks>
    public class CosmosDbTypeValue<T> : CosmosDbTypeValue where T : class, IIdentifier
    {
        private T _value;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [JsonProperty("value")]
        public T Value { get => _value; set => _value = Check.NotNull(value, nameof(Value)); }

        /// <summary>
        /// Gets the <see cref="Value"/>.
        /// </summary>
        /// <returns>The <see cref="Value"/>.</returns>
        public override object GetValue() => Value;

        /// <summary>
        /// Prepares the object before sending to Cosmos.
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
                        throw new InvalidOperationException("An Identifier cannot be inferred for this Type.");
                }

                if (Value is IETag etag)
                    ETag = etag.ETag;
            }

            Type = typeof(T).Name;
        }

        /// <summary>
        /// Prepares the object after getting from Cosmos.
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