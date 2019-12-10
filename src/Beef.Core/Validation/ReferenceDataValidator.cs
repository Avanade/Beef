// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;

namespace Beef.Validation
{
    /// <summary>
    /// Represents the standard <see cref="ReferenceDataBase"/> validation configuration settings.
    /// </summary>
    public static class ReferenceDataValidation
    {
        /// <summary>
        /// Gets or sets the maximum length for the <see cref="ReferenceDataBase.Code"/>.
        /// </summary>
        public static int MaxCodeLength { get; set; } = 30;

        /// <summary>
        /// Gets or sets the maximum length for the <see cref="ReferenceDataBase.Text"/>.
        /// </summary>
        public static int MaxTextLength { get; set; } = 256;

        /// <summary>
        /// Gets or sets the maximum length for the <see cref="ReferenceDataBase.Description"/>.
        /// </summary>
        public static int MaxDescriptionLength { get; set; } = 1000;
    }
}
