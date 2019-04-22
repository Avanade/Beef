// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Beef.Json
{
    /// <summary>
    /// Represents the result of a merge-patch.
    /// </summary>
    public enum JsonEntityMergeResult
    {
        /// <summary>
        /// An error occured during the processing of the merge-patch.
        /// </summary>
        Error,

        /// <summary>
        /// The merge-patch was successful and resulted in changes to the related entity value.
        /// </summary>
        SuccessWithChanges,

        /// <summary>
        /// The merge-patch was successful and resulted in no changes to the related entity value.
        /// </summary>
        SuccessNoChanges
    }

    /// <summary>
    /// The <see cref="JsonEntityMerge"/> arguments.
    /// </summary>
    public class JsonEntityMergeArgs
    {
        /// <summary>
        /// Gets or sets the log action; bind a log write to an output.
        /// </summary>
        public Action<MessageItem> LogAction { get; set; } = null;

        /// <summary>
        /// Indicates whether to treat warnings as errors.
        /// </summary>
        public bool TreatWarningsAsErrors { get; set; } = false;

        /// <summary>
        /// Indicates whether to check for changes prior to updating entity; will always result in <see cref="JsonEntityMergeResult.SuccessWithChanges"/>.
        /// </summary>
        public bool CheckForChanges { get; set; } = false;

        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="message">The <see cref="MessageItem"/> to log.</param>
        /// <returns>The resulting <see cref="JsonEntityMergeResult"/>.</returns>
        public JsonEntityMergeResult Log(MessageItem message)
        {
            if (TreatWarningsAsErrors && message.Type == MessageType.Warning)
                message.Type = MessageType.Error;

            LogAction?.Invoke(message);
            return message.Type == MessageType.Error ? JsonEntityMergeResult.Error : JsonEntityMergeResult.SuccessNoChanges;
        }
    }

    /// <summary>
    /// Enables a JSON <see cref="Merge{TEntity}(JToken, TEntity, JsonEntityMergeArgs)"/> whereby the contents of a JSON object are merged into an existing entity/oject value.
    /// </summary>
    /// <remarks>This is enabled to largely support: https://tools.ietf.org/html/rfc7386. 
    /// <para>Additional logic has been added to the merge where dealing with JSON arrays into a collection where the underlying entity implements <see cref="IUniqueKey"/>. Where
    /// <see cref="IUniqueKey.HasUniqueKey"/> is set for the <see cref="Type"/> then the item will be matched (finds existing item) and updates, versus full array replacement (normal
    /// behaviour).</para>
    /// </remarks>
    public class JsonEntityMerge
    {
        /// <summary>
        /// Manages the unique key config.
        /// </summary>
        private class UniqueKeyConfig
        {
            private object _lock = new object();
            private IPropertyReflector[] propertyReflectors = null;

            /// <summary>
            /// Indicates whether the collection implements <see cref="IEntityBaseCollection"/>.
            /// </summary>
            public bool IsEntityBaseCollection { get; set; }

            /// <summary>
            /// Gets or sets the property names that make up the unique key
            /// </summary>
            public string[] Properties { get; set; }

            /// <summary>
            /// Gets the <see cref="IPropertyReflector"/> properties that make up the unique key (lazy-loaded and cached for performance).
            /// </summary>
            /// <param name="ier">The parent <see cref="IEntityReflector"/>.</param>
            /// <returns>The <see cref="IPropertyReflector"/> properties.</returns>
            public IPropertyReflector[] GetPropertyReflectors(IEntityReflector ier)
            {
                if (propertyReflectors != null)
                    return propertyReflectors;

                lock (_lock)
                {
                    if (propertyReflectors == null)
                    {
                        var prs = new IPropertyReflector[Properties.Length];
                        for (int i = 0; i < Properties.Length; i++)
                        {
                            prs[i] = ier.GetProperty(Properties[i]);
                            if (prs[i] == null)
                                throw new InvalidOperationException($"Type '{ier.Type.Name}' references a UniqueKey Property '{Properties[i]}' that does not exist.");
                        }

                        propertyReflectors = prs;
                    }
                }

                return propertyReflectors;
            }
        }

        private static readonly EntityReflectorArgs _erArgs = new EntityReflectorArgs()
        {
            AutoPopulateProperties = true,
            NameComparer = StringComparer.OrdinalIgnoreCase,
            EntityBuilder = (er) =>
            {
                var joa = er.Type.GetCustomAttributes<JsonObjectAttribute>(true).FirstOrDefault();
                if (joa == null || joa.MemberSerialization != MemberSerialization.OptIn)
                    throw new InvalidOperationException($"Type '{er.Type.Name}' must declare the JsonObjectAttribute with MemberSerialization.OptIn.");

            },
            PropertyBuilder = (pr) =>
            {
                if (!pr.PropertyExpression.HasJsonPropertyAttribute)
                    return false;

                // Where the underlying type is an EntityBase determine whether it has a unique key and what it is, and whether collection is IEntityBaseCollection.
                if (pr.IsComplexType && pr.ComplexTypeReflector.IsCollection && pr.ComplexTypeReflector.ItemType.IsSubclassOf(typeof(EntityBase)))
                {
                    pr.Tag = new UniqueKeyConfig
                    {
                        IsEntityBaseCollection = pr.PropertyType.IsInstanceOfType(typeof(IEntityBaseCollection)),
                        Properties = ((EntityBase)pr.ComplexTypeReflector.CreateItemValue()).UniqueKeyProperties
                    };
                }

                return true;
            }
        };

        /// <summary>
        /// Merges the JSON content into the <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="json">The <see cref="JToken"/> to merge.</param>
        /// <param name="value">The value to merge into.</param>
        /// <param name="args">The <see cref="JsonEntityMergeArgs"/>.</param>
        /// <returns><c>true</c> indicates that a least one change was made to the value; otherwise, <c>false</c>.</returns>
        public static JsonEntityMergeResult Merge<TEntity>(JToken json, TEntity value, JsonEntityMergeArgs args = null)
        {
            Check.NotNull(json, nameof(json));
            Check.NotNull(value, nameof(value));
            args = args ?? new JsonEntityMergeArgs();

            if (json.Type != JTokenType.Object)
                return args.Log(MessageItem.CreateMessage(json.Path, MessageType.Error, $"The JSON document is malformed and could not be parsed."));

            return MergeApply(args, _erArgs.GetReflector(typeof(TEntity)), json, value);
        }

        /// <summary>
        /// Apply the merge from the json to the entity value.
        /// </summary>
        private static JsonEntityMergeResult MergeApply(JsonEntityMergeArgs args, IEntityReflector er, JToken json, object entity)
        {
            if (!json.HasValues)
                return JsonEntityMergeResult.SuccessNoChanges;

            bool hasError = false;
            bool hasChanged = false;
            foreach (var jp in json.Children<JProperty>())
            {
                // Get the corresponding property from the entity.
                var pr = er.GetJsonProperty(jp.Name);
                if (pr == null)
                {
                    if (args.Log(MessageItem.CreateMessage(jp.Path, MessageType.Warning, $"The JSON path is not valid for the entity.")) == JsonEntityMergeResult.Error)
                        hasError = true;

                    continue;
                }

                // Handle the intrinsic types.
                if (!pr.IsComplexType)
                {
                    if (jp.Value.Type == JTokenType.Array || jp.Value.Type == JTokenType.Object)
                        return args.Log(MessageItem.CreateMessage(jp.Path, MessageType.Error, $"The JSON token is malformed and could not be parsed."));

                    try
                    {
                        if (pr.SetValueFromJToken(entity, jp.Value))
                            hasChanged = true;
                    }
                    catch (FormatException fex)
                    {
                        return args.Log(MessageItem.CreateMessage(jp.Path, MessageType.Error, $"The JSON token is malformed: {fex.Message}"));
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                    continue;
                }

                // Handle complex types (objects, arrays, collections, etc).
                switch (MergeApplyComplex(args, pr, jp, entity))
                {
                    case JsonEntityMergeResult.SuccessWithChanges:
                        hasChanged = true;
                        break;

                    case JsonEntityMergeResult.Error:
                        hasError = true;
                        break;
                }
            }

            return hasError ? JsonEntityMergeResult.Error : hasChanged ? JsonEntityMergeResult.SuccessWithChanges : JsonEntityMergeResult.SuccessNoChanges;
        }

        /// <summary>
        /// Apply the merge from the json to the entity value as a more complex type.
        /// </summary>
        private static JsonEntityMergeResult MergeApplyComplex(JsonEntityMergeArgs args, IPropertyReflector pr, JProperty jp, object entity)
        {
            if (jp.Value.Type == JTokenType.Null)
                return pr.SetValue(entity, null) ? JsonEntityMergeResult.SuccessWithChanges : JsonEntityMergeResult.SuccessNoChanges;

            // Update the sub-entity.
            if (pr.ComplexTypeReflector.ComplexTypeCode == ComplexTypeCode.Object)
            {
                if (jp.Value.Type != JTokenType.Object)
                    return args.Log(MessageItem.CreateMessage(jp.Path, MessageType.Error, $"The JSON token is malformed and could not be parsed."));

                var hasChanged = true;
                var current = pr.PropertyExpression.GetValue(entity);
                if (current == null)
                    current = pr.NewValue(entity).value;
                else
                    hasChanged = false;

                var mr = MergeApply(args, pr.GetEntityReflector(), jp.Value, current);
                return mr == JsonEntityMergeResult.SuccessNoChanges ? (hasChanged ? JsonEntityMergeResult.SuccessWithChanges : JsonEntityMergeResult.SuccessNoChanges) : mr;
            }
            else
            {
                // Ensure we are dealing with an array.
                if (jp.Value.Type != JTokenType.Array)
                    return args.Log(MessageItem.CreateMessage(jp.Path, MessageType.Error, $"The JSON token is malformed and could not be parsed."));

                // Where empty array then update as such.
                if (!jp.Value.HasValues)
                    return UpdateArrayValue(args, pr, entity, (IEnumerable)pr.PropertyExpression.GetValue(entity), (IEnumerable)pr.ComplexTypeReflector.CreateValue());

                // Handle array with primitive types.
                if (!pr.ComplexTypeReflector.IsItemComplexType)
                {
                    var lo = new List<object>();
                    foreach (var iv in jp.Value.Values())
                    {
                        lo.Add(iv.ToObject(pr.ComplexTypeReflector.ItemType));
                    }

                    return UpdateArrayValue(args, pr, entity, (IEnumerable)pr.PropertyExpression.GetValue(entity), (IEnumerable)pr.ComplexTypeReflector.CreateValue(lo));
                }

                // Finally, handle array with complex entity items.
                return (pr.Tag == null) ? MergeApplyComplexItems(args, pr, jp, entity) : MergeApplyUniqueKeyItems(args, pr, jp, entity);
            }
        }

        /// <summary>
        /// Apply the merge as a full collection replacement; there is <b>no</b> way to detect changes or perform partial property update. 
        /// </summary>
        private static JsonEntityMergeResult MergeApplyComplexItems(JsonEntityMergeArgs args, IPropertyReflector pr, JProperty jp, object entity)
        {
            var hasError = false;
            var lo = new List<object>();
            var ier = pr.GetItemEntityReflector();

            foreach (var ji in jp.Values())
            {
                if (ji.Type == JTokenType.Null)
                {
                    lo.Add(null);
                    continue;
                }

                var ival = pr.ComplexTypeReflector.CreateItemValue();
                if (MergeApply(args, ier, ji, ival) == JsonEntityMergeResult.Error)
                    hasError = true;
                else
                    lo.Add(ival);
            }

            if (hasError)
                return JsonEntityMergeResult.Error;

            pr.ComplexTypeReflector.SetValue(entity, lo);
            return JsonEntityMergeResult.SuccessWithChanges;
        }

        /// <summary>
        /// Apply the merge using the UniqueKey to match items between JSON and entity.
        /// </summary>
        private static JsonEntityMergeResult MergeApplyUniqueKeyItems(JsonEntityMergeArgs args, IPropertyReflector pr, JProperty jp, object entity)
        {
            var hasError = false;
            var hasChanges = false;
            var count = 0;
            var ukc = (UniqueKeyConfig)pr.Tag;
            var lo = new List<object>();
            var ier = pr.GetItemEntityReflector();

            // Determine the unique key for a comparison.
            var ukpr = ukc.GetPropertyReflectors(ier);

            // Get the current value to update.
            var current = (IEnumerable)pr.PropertyExpression.GetValue(entity);
            if (current == null)
                hasChanges = true;

            // Merge each item into the new collection.
            foreach (var ji in jp.Values())
            {
                // Check not null.
                if (ji.Type != JTokenType.Object)
                {
                    hasError = true;
                    args.Log(MessageItem.CreateErrorMessage(ji.Path, "The JSON token must be an object where Unique Key value(s) are required."));
                    continue;
                }

                // Generate the unique key from the json properties.
                bool skip = false;
                var uk = new object[ukpr.Length];
                for (int i = 0; i < ukc.Properties.Length; i++)
                {
                    var jk = ji[ukpr[i].JsonName];
                    if (jk == null)
                    {
                        hasError = skip = true;
                        args.Log(MessageItem.CreateErrorMessage(ji.Path, $"The JSON object must specify the '{ukpr[i].JsonName}' token as required for the unique key."));
                        break;
                    }

                    try
                    {
                        uk[i] = ukpr[i].GetJtokenValue(jk);
                    }
                    catch (FormatException fex)
                    {
                        hasError = skip = true;
                        args.Log(MessageItem.CreateMessage(jk.Path, MessageType.Error, $"The JSON token is malformed: {fex.Message}"));
                        break;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                if (skip)
                    continue;

                // Get existing by unique key.
                var uniqueKey = new UniqueKey(uk);
                var item = current == null ? null : ukc.IsEntityBaseCollection 
                    ? ((IEntityBaseCollection)current).GetByUniqueKey(uniqueKey) 
                    : current.OfType<EntityBase>().FirstOrDefault(x => uniqueKey.Equals(x.UniqueKey));

                // Create new if not found.
                if (item == null)
                {
                    hasChanges = true;
                    item = pr.ComplexTypeReflector.CreateItemValue();
                }

                // Update.
                count++;
                var mr = MergeApply(args, ier, ji, item);
                if (mr == JsonEntityMergeResult.Error)
                    hasError = true;
                else
                {
                    if (mr == JsonEntityMergeResult.SuccessWithChanges)
                        hasChanges = true;

                    lo.Add(item);
                }
            }

            if (hasError)
                return JsonEntityMergeResult.Error;

            // Confirm nothing was deleted (only needed where nothing changed so far).
            if (!hasChanges && count == (ukc.IsEntityBaseCollection ? ((IEntityBaseCollection)current).Count : current.OfType<EntityBase>().Count()))
                return JsonEntityMergeResult.SuccessNoChanges;

            pr.ComplexTypeReflector.SetValue(entity, lo);
            return JsonEntityMergeResult.SuccessWithChanges;
        }

        /// <summary>
        /// Updates the array value.
        /// </summary>
        private static JsonEntityMergeResult UpdateArrayValue(JsonEntityMergeArgs args, IPropertyReflector pr, object entity, IEnumerable curVal, IEnumerable newVal)
        {
            if (pr.ComplexTypeReflector.CompareSequence(newVal, curVal))
                return JsonEntityMergeResult.SuccessNoChanges;

            pr.ComplexTypeReflector.SetValue(entity, newVal);
            return JsonEntityMergeResult.SuccessWithChanges;
        }
    }
}