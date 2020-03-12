// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System.Collections.Generic;

namespace Beef.Validation
{
    /// <summary>
    /// Represents the optional extended arguments for an entity validation.
    /// </summary>
    public class ValidationArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationArgs"/> class.
        /// </summary>
        public ValidationArgs() { }

        /// <summary>
        /// Indicates whether to use the JSON name for the <see cref="MessageItem"/> <see cref="MessageItem.Property"/>; by default (<c>false</c>) uses the .NET name.
        /// </summary>
        public static bool DefaultUseJsonNames { get; set; } = false;

        /// <summary>
        /// Gets or sets the optional name of a selected (specific) property to validate for the entity (<c>null</c> indicates to validate all).
        /// </summary>
        /// <remarks>Nested or fully quailified entity names are not supported for this type of validation; only a property of the primary entity can be selected.</remarks>
        public string? SelectedPropertyName { get; set; }

        /// <summary>
        /// Gets or sets the entity prefix used for fully qualified <i>entity.property</i> naming (<c>null</c> represents the root).
        /// </summary>
        public string? FullyQualifiedEntityName { get; set; }

        /// <summary>
        /// Gets or sets the entity prefix used for fully qualified <i>entity.property</i> naming (<c>null</c> represents the root).
        /// </summary>
        public string? FullyQualifiedJsonEntityName { get; set; }

        /// <summary>
        /// Indicates (overrides <see cref="DefaultUseJsonNames"/>) whether to use the JSON name for the <see cref="MessageItem"/> <see cref="MessageItem.Property"/>;
        /// defaults to <c>null</c> (uses the <see cref="DefaultUseJsonNames"/> value).
        /// </summary>
        public bool? UseJsonNames { get; set; }

        /// <summary>
        /// Gets <see cref="UseJsonNames"/> selection.
        /// </summary>
        internal bool UseJsonNamesSelection => UseJsonNames ?? DefaultUseJsonNames;

        /// <summary>
        /// Indicates that a shallow validation is required; i.e. will only validate the top level properties.
        /// </summary>
        /// <remarks>The default deep validation will not only validate the top level properties, but also those children down the object graph;
        /// i.e. sub-objects and collections.</remarks>
        public bool ShallowValidation { get; set; }

        /// <summary>
        /// Gets the configuration parameters.
        /// </summary>
        /// <remarks>Configuration parameters provide a means to pass values down through the validation stack.</remarks>
        public Dictionary<string, object> Config { get; internal set; } = new Dictionary<string, object>();
    }
}
