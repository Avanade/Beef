// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides <see cref="string"/> <see cref="Wildcard"/> validation.
    /// </summary>
    /// <typeparam name="TEntity">The entity <see cref="System.Type"/>.</typeparam>
    public class WildcardRule<TEntity> : ValueRuleBase<TEntity, string?> where TEntity : class
    {
        /// <summary>
        /// Gets or sets the <see cref="Wildcard"/> configuration (uses <see cref="Wildcard.Default"/> where <c>null</c>).
        /// </summary>
        public Wildcard? Wildcard { get; set; }

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override void Validate(PropertyContext<TEntity, string?> context)
        {
            Beef.Check.NotNull(context, nameof(context));

            if (string.IsNullOrEmpty(context.Value))
                return;

            var wildcard = Wildcard ?? Wildcard.Default ?? Wildcard.MultiAll;
            if (wildcard != null && !wildcard.Validate(context.Value))
            {
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.WildcardFormat);
                return;
            }
        }
    }
}