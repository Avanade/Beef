// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Entities
{
    /// <summary>
    /// Represents the trimming of white space characters from a <see cref="System.String"/>.
    /// </summary>
    public enum StringTrim
    {
        /// <summary>Indicates that the <see cref="Cleaner.DefaultStringTrim"/> value should be used.</summary>
        UseDefault,
        /// <summary>The string is left unchanged.</summary>
        None,
        /// <summary>Removes all occurences of white space characters from the beginning and ending of a string.</summary>
        Both,
        /// <summary>Removes all occurences of white space characters from the ending of a string.</summary>
        End,
        /// <summary>Removes all occurences of white space characters from the beginning of a string.</summary>
        Start
    }
}
