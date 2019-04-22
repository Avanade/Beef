// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System.Linq;

namespace Beef.Validation.Rules
{
    /// <summary>
    /// Provides validation for a <see cref="ReferenceDataSidListBase"/> including <see cref="MinCount"/>, <see cref="MaxCount"/>, per item <see cref="ReferenceDataBase.IsValid"/>,
    /// and whether to <see cref="AllowDuplicates"/>.
    /// </summary>
    public class ReferenceDataSidListRule<TEntity, TProperty> : ValueRuleBase<TEntity, TProperty>
        where TEntity : class
        where TProperty : ReferenceDataSidListBase
    {
        /// <summary>
        /// Gets or sets the minimum count;
        /// </summary>
        public int MinCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum count.
        /// </summary>
        public int? MaxCount { get; set; }

        /// <summary>
        /// Indicates whether duplicate values are allowed.
        /// </summary>
        public bool AllowDuplicates { get; set; } = false;

        /// <summary>
        /// Validate the property value.
        /// </summary>
        /// <param name="context">The <see cref="PropertyContext{TEntity, TProperty}"/>.</param>
        public override void Validate(PropertyContext<TEntity, TProperty> context)
        {
            if (context.Value == null)
                return;

            if (context.Value.ContainsInvalidItems())
            {
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.InvalidFormat);
                return;
            }

            // Check Min and Max counts.
            if (context.Value.Count < MinCount)
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.MinCountFormat, MinCount);
            else if (MaxCount.HasValue && context.Value.Count > MaxCount.Value)
                context.CreateErrorMessage(ErrorText ?? ValidatorStrings.MaxCountFormat, MaxCount);

            // Check duplicates.
            var dict = new KeyOnlyDictionary<string>();
            foreach (var item in context.Value.ToRefDataList().Where(x => x.IsValid))
            {
                if (dict.ContainsKey(item.Code))
                {
                    context.CreateErrorMessage(ErrorText ?? ValidatorStrings.DuplicateValueFormat, context.Text, item.ToString());
                    return;
                }

                dict.Add(item.Code);
            }
        }
    }
}
