// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Mapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.Data.EntityFrameworkCore
{
    /// <summary>
    /// Provides the entity framework query capabilities.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TModel"></typeparam>
    public interface IEfDbQuery<T, TModel> where T : class, new() where TModel : class
    {
        /// <summary>
        /// Selects a single item.
        /// </summary>
        /// <returns>The single item.</returns>
        T SelectSingle();

        /// <summary>
        /// Selects a single item or default.
        /// </summary>
        /// <returns>The single item or default.</returns>
        T? SelectSingleOrDefault();

        /// <summary>
        /// Selects first item.
        /// </summary>
        /// <returns>The first item.</returns>
        T SelectFirst();

        /// <summary>
        /// Selects first item or default.
        /// </summary>
        /// <returns>The single item or default.</returns>
        T? SelectFirstOrDefault();

        /// <summary>
        /// Executes the query command creating a resultant collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <returns>A resultant collection.</returns>
        TColl SelectQuery<TColl>() where TColl : ICollection<T>, new();

        /// <summary>
        /// Executes a query adding to the passed collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <param name="coll">The collection to add items to.</param>
        void SelectQuery<TColl>(TColl coll) where TColl : ICollection<T>;
    }

    /// <summary>
    /// Encapsulates an Entity Framework query enabling all select-like capabilities.
    /// </summary>
    /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
    /// <typeparam name="TModel">The entity framework model <see cref="Type"/>.</typeparam>
    /// <typeparam name="TDbContext">The <see cref="DbContext"/> <see cref="Type"/>.</typeparam>
    public class EfDbQuery<T, TModel, TDbContext> : IEfDbQuery<T, TModel> where T : class, new() where TModel : class, new() where TDbContext : DbContext, IEfDbContext
    {
        private readonly EfDbBase<TDbContext> _db;
        private readonly Func<IQueryable<TModel>, IQueryable<TModel>>? _query;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfDbQuery{T, TModel, TDbContext}"/> class.
        /// </summary>
        /// <param name="db">The <see cref="DbSet{TModel}"/>.</param>
        /// <param name="args">The <see cref="EfDbArgs"/>.</param>
        /// <param name="query">A function to modify the underlying <see cref="IQueryable{TModel}"/>.</param>
        internal EfDbQuery(EfDbBase<TDbContext> db, EfDbArgs args, Func<IQueryable<TModel>, IQueryable<TModel>>? query = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            Args = args ?? throw new ArgumentNullException(nameof(args));
            _query = query;
        }

        /// <summary>
        /// Gets the <see cref="EfDbArgs"/>.
        /// </summary>
        public EfDbArgs Args { get; private set; }

        /// <summary>
        /// Manages the DbContext and underlying query construction and lifetime.
        /// </summary>
        private void ExecuteQuery(Action<IQueryable<TModel>> execute)
        {
            _db.Invoker.Invoke(this, () =>
            {
                var dbSet = _db.DbContext.Set<TModel>();
                execute((_query == null) ? dbSet : _query(dbSet));
            }, _db);
        }

        /// <summary>
        /// Manages the DbContext and underlying query construction and lifetime.
        /// </summary>
        private TResult ExecuteQuery<TResult>(Func<IQueryable<TModel>, TResult> execute)
        {
            return _db.Invoker.Invoke(this, () =>
            {
                var dbSet = _db.DbContext.Set<TModel>();
                return execute((_query == null) ? dbSet : _query(dbSet));
            }, _db);
        }

        /// <summary>
        /// Sets the paging from the <see cref="PagingArgs"/>.
        /// </summary>
        private static IQueryable<TModel> SetPaging(IQueryable<TModel> query, PagingArgs? paging)
        {
            if (paging == null)
                return query;

            var q = query;
            if (paging.Skip > 0)
                q = q.Skip((int)paging.Skip);

            return q.Take((int)(paging == null ? PagingArgs.DefaultTake : paging.Take));
        }

        /// <summary>
        /// Map to source.
        /// </summary>
        private T? MapToSrce(TModel? model)
        {
            if (model == null)
                return null;

            return Args.Mapper.Map<TModel, T>(model, Mapper.OperationTypes.Get)!;
        }

        #region SelectSingle/SelectFirst

        /// <summary>
        /// Selects a single item.
        /// </summary>
        /// <returns>The single item.</returns>
        public T SelectSingle() => MapToSrce(ExecuteQuery(q => q.Single()))!;

        /// <summary>
        /// Selects a single item or default.
        /// </summary>
        /// <returns>The single item or default.</returns>
        public T? SelectSingleOrDefault() => MapToSrce(ExecuteQuery(q => q.SingleOrDefault()));

        /// <summary>
        /// Selects first item.
        /// </summary>
        /// <returns>The first item.</returns>
        public T SelectFirst() => MapToSrce(ExecuteQuery(q => q.First()))!;

        /// <summary>
        /// Selects first item or default.
        /// </summary>
        /// <returns>The single item or default.</returns>
        public T? SelectFirstOrDefault() => MapToSrce(ExecuteQuery(q => q.FirstOrDefault()));

        #endregion

        #region SelectQuery

        /// <summary>
        /// Executes the query command creating a resultant collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <returns>A resultant collection.</returns>
        public TColl SelectQuery<TColl>() where TColl : ICollection<T>, new()
        {
            var coll = new TColl();
            SelectQuery(coll);
            return coll;
        }

        /// <summary>
        /// Executes a query adding to the passed collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <param name="coll">The collection to add items to.</param>
        public void SelectQuery<TColl>(TColl coll) where TColl : ICollection<T>
        {
            ExecuteQuery(query =>
            {
                var q = SetPaging(query, Args.Paging);

                foreach (var item in q)
                {
                    coll.Add(MapToSrce(item) ?? throw new InvalidOperationException("Mapping from the EF entity must not result in a null value."));
                }

                if (Args.Paging != null && Args.Paging.IsGetCount)
                    Args.Paging.TotalCount = query.LongCount();
            });
        }

        #endregion
    }
}