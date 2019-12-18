// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Provides the core capabilities for the special-purpose <b>CosmosDb/DocumentDb</b> object that houses an underlying model-<see cref="Value"/>.
    /// </summary>
    public interface ICosmosDbValue
    {
        /// <summary>
        /// Gets or sets the <see cref="Type"/> name.
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the model value.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Prepares the object before sending to Cosmos.
        /// </summary>
        void PrepareBefore();

        /// <summary>
        /// Prepares the object after getting from Cosmos.
        /// </summary>
        void PrepareAfter();
    }

    /// <summary>
    /// Represents a special-purpose <b>CosmosDb/DocumentDb</b> object that houses an underlying model-<see cref="Value"/>, including <see cref="Type"/> name, for persistence.
    /// </summary>
    /// <typeparam name="TModel">The model <see cref="Value"/> <see cref="Type"/>.</typeparam>
    /// <remarks>The <see cref="CosmosDbModelBase.Id"/>, <see cref="Type"/> and <see cref="CosmosDbModelBase.ETag"/> are updated internally when interacting directly with <b>CosmosDB/DocumentDb</b>.</remarks>
    public sealed class CosmosDbValue<TModel> : CosmosDbModelBase, ICosmosDbValue where TModel : class, new()
    {
        private TModel _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbValue{TModel}"/> class.
        /// </summary>
        public CosmosDbValue() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbValue{TModel}"/> class with a <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        public CosmosDbValue(TModel value) => Value = value;

        /// <summary>
        /// Gets or sets the <see cref="Type"/> name.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [JsonProperty("value")]
        public TModel Value { get => _value; set => _value = Check.NotNull(value, nameof(Value)); }

        /// <summary>
        /// Gets the value.
        /// </summary>
        object ICosmosDbValue.Value => _value;

        /// <summary>
        /// Prepares the object before sending to Cosmos.
        /// </summary>
        void ICosmosDbValue.PrepareBefore()
        {
            if (Value != default)
            {
                switch (Value)
                {
                    case IStringIdentifier isi:
                        Id = isi.Id;
                        break;

                    case IIntIdentifier iii:
                        Id = iii.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
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

            Type = typeof(TModel).Name;
        }

        /// <summary>
        /// Prepares the object after getting from Cosmos.
        /// </summary>
        void ICosmosDbValue.PrepareAfter()
        {
            if (Value == default)
                return;

            switch (Value)
            {
                case IStringIdentifier isi:
                    isi.Id = Id;
                    break;

                case IIntIdentifier iii:
                    iii.Id = int.Parse(Id, System.Globalization.CultureInfo.InvariantCulture);
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