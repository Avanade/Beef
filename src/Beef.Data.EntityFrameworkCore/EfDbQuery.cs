// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.Data.EntityFrameworkCore
{
    /// <summary>
    /// Encapsulates an Entity Framework query enabling all select-like capabilities.
    /// </summary>
    /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
    /// <typeparam name="TModel">The entity framework model <see cref="Type"/>.</typeparam>
    /// <typeparam name="TDbContext">The <see cref="DbContext"/> <see cref="Type"/>.</typeparam>
    public class EfDbQuery<T, TModel, TDbContext> where T : class, new() where TModel : class, new() where TDbContext : DbContext, new()
    {
        private readonly EfDbBase<TDbContext> _db;
        private readonly Func<IQueryable<TModel>, IQueryable<TModel>>? _query;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfDbQuery{T, TModel, TDbContext}"/> class.
        /// </summary>
        /// <param name="db">The <see cref="DbSet{TModel}"/>.</param>
        /// <param name="queryArgs">The <see cref="EfDbArgs{T, TModel}"/>.</param>
        /// <param name="query">A function to modify the underlying <see cref="IQueryable{TModel}"/>.</param>
        internal EfDbQuery(EfDbBase<TDbContext> db, EfDbArgs<T, TModel> queryArgs, Func<IQueryable<TModel>, IQueryable<TModel>>? query = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            QueryArgs = queryArgs ?? throw new ArgumentNullException(nameof(queryArgs));
            _query = query;
        }

        /// <summary>
        /// Gets the <see cref="EfDbArgs{T, TModel}"/>.
        /// </summary>
        public EfDbArgs<T, TModel> QueryArgs { get; private set; }

        /// <summary>
        /// Manages the DbContext and underlying query construction and lifetime.
        /// </summary>
        private void ExecuteQuery(Action<IQueryable<TModel>> execute)
        {
            EfDbInvoker<TDbContext>.Default.Invoke(this, () =>
            {
                using var db = new EfDbBase<TDbContext>.EfDbContextManager(QueryArgs);
                var dbSet = db.DbContext.Set<TModel>();
                execute((_query == null) ? dbSet : _query(dbSet));
            }, _db);
        }

        /// <summary>
        /// Manages the DbContext and underlying query construction and lifetime.
        /// </summary>
        private TResult ExecuteQuery<TResult>(Func<IQueryable<TModel>, TResult> execute)
        {
            return EfDbInvoker<TDbContext>.Default.Invoke(this, () =>
            {
                using var db = new EfDbBase<TDbContext>.EfDbContextManager(QueryArgs);
                var dbSet = db.DbContext.Set<TModel>();
                return execute((_query == null) ? dbSet : _query(dbSet));
            }, _db);
        }

        /// <summary>
        /// Sets the paging from the <see cref="PagingArgs"/>.
        /// </summary>
        private IQueryable<TModel> SetPaging(IQueryable<TModel> query, PagingArgs? paging)
        {
            if (paging == null)
                return query;

            var q = query;
            if (paging.Skip > 0)
                q = q.Skip((int)paging.Skip);

            return q.Take((int)(paging == null ? PagingArgs.DefaultTake : paging.Take));
        }

        #region SelectSingle/SelectFirst

        /// <summary>
        /// Selects a single item.
        /// </summary>
        /// <returns>The single item.</returns>
        public T SelectSingle()
        {
            return QueryArgs.Mapper.MapToSrce(ExecuteQuery(q => q.Single()), Mapper.OperationTypes.Get)!;
        }

        /// <summary>
        /// Selects a single item or default.
        /// </summary>
        /// <returns>The single item or default.</returns>
        public T SelectSingleOrDefault()
        {
            return QueryArgs.Mapper.MapToSrce(ExecuteQuery(q => q.SingleOrDefault()), Mapper.OperationTypes.Get)!;
        }

        /// <summary>
        /// Selects first item.
        /// </summary>
        /// <returns>The first item.</returns>
        public T SelectFirst()
        {
            return QueryArgs.Mapper.MapToSrce(ExecuteQuery(q => q.First()), Mapper.OperationTypes.Get)!;
        }

        /// <summary>
        /// Selects first item or default.
        /// </summary>
        /// <returns>The single item or default.</returns>
        public T SelectFirstOrDefault()
        {
            return QueryArgs.Mapper.MapToSrce(ExecuteQuery(q => q.FirstOrDefault()), Mapper.OperationTypes.Get)!;
        }

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
                var q = SetPaging(query, QueryArgs.Paging);

                foreach (var item in q)
                {
                    coll.Add(QueryArgs.Mapper.MapToSrce(item, Mapper.OperationTypes.Get) ?? throw new InvalidOperationException("Mapping from the EF entity must not result in a null value."));
                }

                if (QueryArgs.Paging != null && QueryArgs.Paging.IsGetCount)
                    QueryArgs.Paging.TotalCount = query.LongCount();
            });
        }

        #endregion
    }
}