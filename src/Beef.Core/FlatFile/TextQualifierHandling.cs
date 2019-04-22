// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.FlatFile
{
    /// <summary>
    /// Represents the text qualifier handling.
    /// </summary>
    public enum TextQualifierHandling
    {
        /// <summary>
        /// Text qualifier is strictly validated and any encountered errant instances will result in an error.
        /// </summary>
        Strict,

        /// <summary>
        /// Text qualifier is loosely validated and any encountered errant instances are allowed/accepted (with a warning).
        /// </summary>
        LooseAllow,

        /// <summary>
        /// Text qualifier is loosely validated and any encountered errant instances are skipped/ignored (with a warning).
        /// </summary>
        LooseSkip
    }
}
