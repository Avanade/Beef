// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System.IO;

namespace OnRamp.Utility
{
    /// <summary>
    /// Defines the supported <see cref="Stream"/> content types.
    /// </summary>
    public enum StreamContentType
    {
        /// <summary>
        /// Specifies that the content type is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// Specifies that the content type is YAML.
        /// </summary>
        Yaml,

        /// <summary>
        /// Specifies that the content type is JSON.
        /// </summary>
        Json
    }
}