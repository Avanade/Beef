/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using Beef;
using Beef.Data.Database;
using Beef.Data.Database.Cdc;
using Beef.Events;
using Beef.Mapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beef.Demo.Cdc.Entities;

namespace Beef.Demo.Cdc.Data
{
    /// <summary>
    /// Enables the CDC data access for database object 'Legacy.Posts'.
    /// </summary>
    public partial interface IPostsCdcData : ICdcDataOrchestrator { }

    /// <summary>
    /// Provides the CDC data access for database object 'Legacy.Posts'.
    /// </summary>
    public partial class PostsCdcData : CdcDataOrchestrator<PostsCdc, PostsCdcData.PostsCdcWrapperCollection, PostsCdcData.PostsCdcWrapper, CdcTrackingDbMapper>, IPostsCdcData
    {
        private static readonly DatabaseMapper<PostsCdcWrapper> _postsCdcWrapperMapper = DatabaseMapper.CreateAuto<PostsCdcWrapper>();
        private static readonly DatabaseMapper<PostsCdc.CommentsCdc> _commentsCdcMapper = DatabaseMapper.CreateAuto<PostsCdc.CommentsCdc>();
        private static readonly DatabaseMapper<PostsCdc.CommentsTagsCdc> _commentsTagsCdcMapper = DatabaseMapper.CreateAuto<PostsCdc.CommentsTagsCdc>();
        private static readonly DatabaseMapper<PostsCdc.PostsTagsCdc> _postsTagsCdcMapper = DatabaseMapper.CreateAuto<PostsCdc.PostsTagsCdc>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PostsCdcData"/> class.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="evtPub">The <see cref="IEventPublisher"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public PostsCdcData(IDatabase db, IEventPublisher evtPub, ILogger<PostsCdcData> logger) :
            base(db, "[DemoCdc].[spExecutePostsCdcOutbox]", "[DemoCdc].[spCompletePostsCdcOutbox]", evtPub, logger) => PostsCdcDataCtor();

        partial void PostsCdcDataCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the outbox entity data from the database.
        /// </summary>
        /// <returns>The corresponding result.</returns>
        protected override async Task<CdcDataOrchestratorResult<PostsCdcWrapperCollection, PostsCdcWrapper>> GetOutboxEntityDataAsync()
        {
            var pColl = new PostsCdcWrapperCollection();

            var result = await SelectQueryMultiSetAsync(
                new MultiSetCollArgs<PostsCdcWrapperCollection, PostsCdcWrapper>(_postsCdcWrapperMapper, r => pColl = r, stopOnNull: true), // Root table: Legacy.Posts
                new MultiSetCollArgs<PostsCdc.CommentsCdcCollection, PostsCdc.CommentsCdc>(_commentsCdcMapper, r =>
                {
                    foreach (var c in r.GroupBy(x => new { x.PostsId }).Select(g => new { g.Key.PostsId, Coll = g.ToCollection<PostsCdc.CommentsCdcCollection, PostsCdc.CommentsCdc>() })) // Join table: Comments (Legacy.Comments)
                    {
                        pColl.Single(x => x.PostsId == c.PostsId).Comments = c.Coll;
                    }
                }), // Related table: Comments (Legacy.Comments)
                new MultiSetCollArgs<PostsCdc.CommentsTagsCdcCollection, PostsCdc.CommentsTagsCdc>(_commentsTagsCdcMapper, r =>
                {
                    foreach (var c in r.GroupBy(x => new { x.Posts_PostsId }).Select(g => new { g.Key.Posts_PostsId, Coll = g.ToList() })) // Join table: Comments (Legacy.Comments)
                    {
                        var pItem = pColl.Single(x => x.PostsId == c.Posts_PostsId).Comments;
                        foreach (var ct in c.Coll.GroupBy(x => new { x.CommentsId }).Select(g => new { g.Key.CommentsId, Coll = g.ToCollection<PostsCdc.CommentsTagsCdcCollection, PostsCdc.CommentsTagsCdc>() })) // Join table: CommentsTags (Legacy.Tags)
                        {
                            pItem.Single(x => x.CommentsId == ct.CommentsId).Tags = ct.Coll;
                        }
                    }
                }), // Related table: CommentsTags (Legacy.Tags)
                new MultiSetCollArgs<PostsCdc.PostsTagsCdcCollection, PostsCdc.PostsTagsCdc>(_postsTagsCdcMapper, r =>
                {
                    foreach (var pt in r.GroupBy(x => new { x.PostsId }).Select(g => new { g.Key.PostsId, Coll = g.ToCollection<PostsCdc.PostsTagsCdcCollection, PostsCdc.PostsTagsCdc>() })) // Join table: PostsTags (Legacy.Tags)
                    {
                        pColl.Single(x => x.PostsId == pt.PostsId).Tags = pt.Coll;
                    }
                }) // Related table: PostsTags (Legacy.Tags)
                ).ConfigureAwait(false);

            result.Result.AddRange(pColl);
            return result;
        }

        /// <summary>
        /// Gets the <see cref="EventData.Subject"/> without the appended key value(s).
        /// </summary>
        protected override string EventSubject => "Legacy.Post";

        /// <summary>
        /// Gets the <see cref="Events.EventActionFormat"/>.
        /// </summary>
        protected override EventActionFormat EventActionFormat => EventActionFormat.PastTense;

        /// <summary>
        /// Represents a <see cref="PostsCdc"/> wrapper to append the required (additional) database properties.
        /// </summary>
        public class PostsCdcWrapper : PostsCdc, ICdcWrapper
        {
            /// <summary>
            /// Gets or sets the database CDC <see cref="OperationType"/>.
            /// </summary>
            [MapperProperty("_OperationType", ConverterType = typeof(CdcOperationTypeConverter))]
            public OperationType DatabaseOperationType { get; set; }

            /// <summary>
            /// Gets or sets the database tracking hash code.
            /// </summary>
            [MapperProperty("_TrackingHash")]
            public string? DatabaseTrackingHash { get; set; }

            /// <summary>
            /// Gets or sets the database log sequence number (LSN).
            /// </summary>
            [MapperProperty("_Lsn")]
            public byte[] DatabaseLsn { get; set; }
        }

        /// <summary>
        /// Represents a <see cref="PostsCdcWrapper"/> collection.
        /// </summary>
        public class PostsCdcWrapperCollection : List<PostsCdcWrapper> { }
    }
}

#pragma warning restore
#nullable restore