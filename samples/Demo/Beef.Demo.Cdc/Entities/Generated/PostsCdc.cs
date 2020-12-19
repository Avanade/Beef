/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0079, IDE0005, CA2227, CA1819, CA1056

using Beef.Entities;
using Beef.Data.Database.Cdc;
using Newtonsoft.Json;
using System;

namespace Beef.Demo.Cdc.Entities
{
    /// <summary>
    /// Represents the CDC model for the root (primary) database table 'Legacy.Posts'.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class PostsCdc
    {
        /// <summary>
        /// Gets or sets the 'PostsId' column value.
        /// </summary>
        [JsonProperty("postsId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int PostsId { get; set; }

        /// <summary>
        /// Gets or sets the 'Text' column value.
        /// </summary>
        [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the 'Date' column value.
        /// </summary>
        [JsonProperty("date", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Gets or sets the related (one-to-many) <see cref="PostsCdc.CommentsCollection"/> (database object 'Legacy.Comments').
        /// </summary>
        public PostsCdc.CommentsCollection Commentss { get; set; }

        /// <summary>
        /// Gets or sets the related (one-to-many) <see cref="PostsCdc.TagsCollection"/> (database object 'Legacy.Tags').
        /// </summary>
        public PostsCdc.TagsCollection Tagss { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool HasUniqueKey => true;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public UniqueKey UniqueKey => new UniqueKey(PostsId);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string[] UniqueKeyProperties => new string[] { nameof(PostsId) };

        /// <summary>
        /// Represents the CDC model for the related (child) database table 'Legacy.Comments'.
        /// </summary>
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public partial class Comments
        {
            /// <summary>
            /// Gets or sets the 'CommentsId' column value.
            /// </summary>
            [JsonProperty("commentsId", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int CommentsId { get; set; }

            /// <summary>
            /// Gets or sets the 'PostsId' column value.
            /// </summary>
            [JsonProperty("postsId", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int PostsId { get; set; }

            /// <summary>
            /// Gets or sets the 'Text' column value.
            /// </summary>
            [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string? Text { get; set; }

            /// <summary>
            /// Gets or sets the 'Date' column value.
            /// </summary>
            [JsonProperty("date", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public DateTime? Date { get; set; }

            /// <summary>
            /// Gets or sets the related (one-to-many) <see cref="PostsCdc.TagsCollection"/> (database object 'Legacy.Tags').
            /// </summary>
            public PostsCdc.TagsCollection Tagss { get; set; }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public bool HasUniqueKey => true;

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public UniqueKey UniqueKey => new UniqueKey(CommentsId);

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public string[] UniqueKeyProperties => new string[] { nameof(CommentsId) };
        }

        /// <summary>
        /// Represents the CDC model for the related (child) database table 'Legacy.Tags'.
        /// </summary>
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public partial class Tags
        {
            /// <summary>
            /// Gets or sets the 'TagsId' column value.
            /// </summary>
            [JsonProperty("tagsId", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int TagsId { get; set; }

            /// <summary>
            /// Gets or sets the 'ParentType' column value.
            /// </summary>
            [JsonProperty("parentType", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string? ParentType { get; set; }

            /// <summary>
            /// Gets or sets the 'ParentId' column value.
            /// </summary>
            [JsonProperty("parentId", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int ParentId { get; set; }

            /// <summary>
            /// Gets or sets the 'Text' column value.
            /// </summary>
            [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string? Text { get; set; }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public bool HasUniqueKey => true;

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public UniqueKey UniqueKey => new UniqueKey(TagsId);

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public string[] UniqueKeyProperties => new string[] { nameof(TagsId) };
        }

        /// <summary>
        /// Represents the CDC model for the related (child) database table 'Legacy.Tags'.
        /// </summary>
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public partial class Tags
        {
            /// <summary>
            /// Gets or sets the 'TagsId' column value.
            /// </summary>
            [JsonProperty("tagsId", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int TagsId { get; set; }

            /// <summary>
            /// Gets or sets the 'ParentType' column value.
            /// </summary>
            [JsonProperty("parentType", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string? ParentType { get; set; }

            /// <summary>
            /// Gets or sets the 'ParentId' column value.
            /// </summary>
            [JsonProperty("parentId", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int ParentId { get; set; }

            /// <summary>
            /// Gets or sets the 'Text' column value.
            /// </summary>
            [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string? Text { get; set; }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public bool HasUniqueKey => true;

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public UniqueKey UniqueKey => new UniqueKey(TagsId);

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            public string[] UniqueKeyProperties => new string[] { nameof(TagsId) };
        }
    }
}

#pragma warning restore IDE0079, IDE0005, CA2227, CA1819, CA1056
#nullable restore