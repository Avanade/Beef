// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AutoMapper;
using System;

namespace Beef.Mapper.Converters
{
    /// <summary>
    /// Represents a <see cref="bool"/> to 'Yes' and 'No' literal converter (representing <c>true</c> and <c>false</c> respectively).
    /// </summary>
    public class BooleanToYesNoConverter : PropertyMapperConverterBase<BooleanToYesNoConverter, bool, string>, IValueConverter<bool, string>, IValueConverter<string, bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanToYesNoConverter"/> class.
        /// </summary>
        public BooleanToYesNoConverter() : base((data) => new Tuple<bool, string>[] { new Tuple<bool, string>(true, "Yes"), new Tuple<bool, string>(false, "No") }) { }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="sourceMember"><inheritdoc/></param>
        /// <param name="context"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        string IValueConverter<bool, string>.Convert(bool sourceMember, ResolutionContext context) => ConvertToDest(sourceMember);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="sourceMember"><inheritdoc/></param>
        /// <param name="context"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        bool IValueConverter<string, bool>.Convert(string sourceMember, ResolutionContext context) => ConvertToSrce(sourceMember);
    }
}