// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Text.RegularExpressions;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides <see cref="string"/> validation including <see cref="MinLength"/>, <see cref="MaxLength"/>, and <see cref="Regex"/>.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="System.Type"/>.</typeparam>
    public class StringRule<TEntity> : ValueRuleBase<TEntity, string> where TEntity : class
    {
        /// <summary>
        /// Gets or sets the minimum length;
        /// </summary>
        public int MinLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum length.
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the regex.
        /// </summary>
        public Regex Regex { get; set; }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override void Validate(PropertyContext<TEntity, string> context)
        {
            Beef.Check.NotNull(context, nameof(context));

            if (string.IsNullOrEmpty(context.Value))
                return;

            if (context.Value.Length < MinLength)
            {
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.MinLengthFormat, MinLength);
                return;
            }

            if (MaxLength.HasValue && context.Value.Length > MaxLength.Value)
            {
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.MaxLengthFormat, MaxLength);
                return;
            }

            if (Regex != null && !Regex.IsMatch(context.Value))
            {
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.RegexFormat);
                return;
            }
        }
    }
}
