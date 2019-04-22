// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a <see cref="Boolean"/> to 'Yes' and 'No' literal converter (representing <c>true</c> and <c>false</c> respectively).
    /// </summary>
    public class BooleanToYesNoConverter : PropertyMapperConverterBase<BooleanToYesNoConverter, bool, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanToYesNoConverter"/> class.
        /// </summary>
        public BooleanToYesNoConverter() : base((data) => new Tuple<bool, string>[] { new Tuple<bool, string>(true, "Yes"), new Tuple<bool, string>(false, "No") }) { }
    }
}
