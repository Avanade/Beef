// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Data.Database;
using Beef.Entities;
using Beef.Mapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Data.EntityFrameworkCore
{
    /// <summary>
    /// Represents the core base class for encapsulating the database access layer using entity framework.
    /// </summary>
    public abstract class EfDbBase
    {
        /// <summary>
        /// Transforms and throws the <see cref="IBusinessException"/> equivalent for a <see cref="SqlException"/>.
        /// </summary>
        /// <param name="sex">The <see cref="SqlException"/>.</param>
        public static void ThrowTransformedSqlException(SqlException sex)
        {
            if (sex == null)
                throw new ArgumentNullException(nameof(sex));

            var msg = sex.Message?.TrimEnd();
            switch (sex.Number)
            {
                case 56001: throw new ValidationException(msg, sex);
                case 56002: throw new BusinessException(msg, sex);
                case 56003: throw new AuthorizationException(msg, sex);
                case 56004: throw new ConcurrencyException(msg, sex);
                case 56005: throw new NotFoundException(msg, sex);
                case 56006: throw new ConflictException(msg, sex);
                case 56007: throw new DuplicateException(msg, sex);

                default:
                    if (AlwaysCheckSqlDuplicateErrorNumbers && SqlDuplicateErrorNumbers.Contains(sex.Number))
                        throw new DuplicateException(null, sex);

                    break;
            }
        }

        /// <summary>
        /// Indicates whether to always check the <see cref="SqlDuplicateErrorNumbers"/> when executing the <see cref="ThrowTransformedSqlException(SqlException)"/> method.
        /// </summary>
        public static bool AlwaysCheckSqlDuplicateErrorNumbers { get; set; } = true;

        /// <summary>
        /// Gets or sets the list of known <see cref="SqlException.Number"/> values for the <see cref="ThrowTransformedSqlException(SqlException)"/> method.
        /// </summary>
        public static List<int> SqlDuplicateErrorNumbers { get; } = new List<int>(new int[] { 2601, 2627 });
    }

    /// <summary>
    /// Represents the base class for encapsulating the database access layer using an entity framework <see cref="Microsoft.EntityFrameworkCore.DbContext"/>.
    /// </summary>
    /// <typeparam name="TDbContext">The <see cref="DbContext"/> <see cref="Type"/>.</typeparam>
    public abstract class EfDbBase<TDbContext> : EfDbBase, IEfDb<TDbContext> where TDbContext : DbContext, IEfDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EfDbBase{TDbContext}"/> class.
        /// </summary>
        /// <param name="dbContext">The <see cref="Microsoft.EntityFrameworkCore.DbContext"/>.</param>
        /// <param name="invoker">Enables the <see cref="Invoker"/> to be overridden; defaults to <see cref="EfDbInvoker{TDbContext}"/>.</param>
        public EfDbBase(TDbContext dbContext, EfDbInvoker<TDbContext>? invoker = null)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            Invoker = invoker ?? new EfDbInvoker<TDbContext>();
        }

        /// <summary>
        /// Gets the underlying <typeparamref name="TDbContext"/> instance.
        /// </summary>
        /// <returns>The <typeparamref name="TDbContext"/> instance.</returns>
        public TDbContext DbContext { get; private set; }

        /// <summary>
        /// Gets the <see cref="EfDbInvoker{TDbContext}"/>.
        /// </summary>
        public EfDbInvoker<TDbContext> Invoker { get; private set; }

        /// <summary>
        /// Gets the <see cref="DatabaseEventOutboxInvoker"/> from the <see cref="IEfDbContext.BaseDatabase"/>.
        /// </summary>
        /// <returns>The <see cref="IDatabase.EventOutboxInvoker"/>.</returns>
        public DatabaseEventOutboxInvoker EventOutboxInvoker => DbContext.BaseDatabase.EventOutboxInvoker;

        /// <summary>
        /// Gets or sets the <see cref="DatabaseWildcard"/> to enable wildcard replacement.
        /// </summary>
        public DatabaseWildcard Wildcard { get; set; } = new DatabaseWildcard();

        /// <summary>
        /// Gets or sets the <see cref="SqlException"/> handler (by default set up to execute <see cref="EfDbBase.ThrowTransformedSqlException(SqlException)"/>).
        /// </summary>
        public Action<SqlException> ExceptionHandler { get; set; } = (sex) => ThrowTransformedSqlException(sex);

        /// <summary>
        /// Invokes the <paramref name="action"/> whilst <see cref="DatabaseWildcard.Replace(string)">replacing</see> the <b>wildcard</b> characters when the <paramref name="with"/> is not <c>null</c>.
        /// </summary>
        /// <param name="with">The value with which to verify.</param>
        /// <param name="action">The <see cref="Action"/> to invoke when there is a valid <paramref name="with"/> value; passed the database specific wildcard value.</param>
        public void WithWildcard(string? with, Action<string> action)
        {
            if (with != null)
            {
                with = Wildcard.Replace(with);
                if (with != null)
                    action?.Invoke(with);
            }
        }

        /// <summary>
        /// Invokes the <paramref name="action"/> when the <paramref name="with"/> is not the default value for the <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The with value <see cref="Type"/>.</typeparam>
        /// <param name="with">The value with which to verify.</param>
        /// <param name="action">The <see cref="Action"/> to invoke when there is a valid <paramref name="with"/> value.</param>
        public void With<T>(T with, Action action)
        {
            if (Comparer<T>.Default.Compare(with, default) != 0 && Comparer<T>.Default.Compare(with, default) != 0)
            {
                if (!(with is string) && with is System.Collections.IEnumerable ie && !ie.GetEnumerator().MoveNext())
                    return;

                action?.Invoke();
            }
        }

        /// <summary>
        /// Creates an <see cref="EfDbQuery{T, TModel, TDbContext}"/> to enable select-like capabilities.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The entity framework model <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="IEfDbArgs"/>.</param>
        /// <param name="query">The function to further define the query.</param>
        /// <returns>A <see cref="EfDbQuery{T, TModel, TDbContext}"/>.</returns>
        public IEfDbQuery<T, TModel> Query<T, TModel>(IEfDbArgs args, Func<IQueryable<TModel>, IQueryable<TModel>>? query = null) where T : class, new() where TModel : class, new()
            => new EfDbQuery<T, TModel, TDbContext>(this, args, query);

        /// <summary>
        /// Gets the entity for the specified <paramref name="keys"/> mapping from <typeparamref name="TModel"/> to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The entity framework model <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="IEfDbArgs"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The entity value where found; otherwise, <c>null</c>.</returns>
        public async Task<T?> GetAsync<T, TModel>(IEfDbArgs args, params IComparable[] keys) where T : class, new() where TModel : class, new()
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            object[] efKeys = new object[keys.Length];
            if (args is EfDbArgs<T, TModel> ema)
            {
                CheckKeys(ema, keys);
                for (int i = 0; i < ema.EntityMapper.UniqueKey.Count; i++)
                {
                    efKeys[i] = ema.EntityMapper.UniqueKey[i].ConvertToDestValue(keys[i], Mapper.OperationTypes.Unspecified)!;
                }
            }
            else
                keys.CopyTo(efKeys, 0);

            return await Invoker.InvokeAsync(this, async () =>
            {
                return await FindAsync<T, TModel>(args, efKeys).ConfigureAwait(false);
            }, this).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs a create for the value (reselects and/or automatically saves changes where specified).
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The entity framework model <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="IEfDbArgs"/>.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>The value (refreshed where specified).</returns>
        public async Task<T> CreateAsync<T, TModel>(IEfDbArgs args, T value) where T : class, new() where TModel : class, new()
        {
            CheckSaveArgs<T, TModel>(args);

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value is IChangeLog cl)
            {
                if (cl.ChangeLog == null)
                    cl.ChangeLog = new ChangeLog();

                cl.ChangeLog.CreatedBy = ExecutionContext.HasCurrent ? ExecutionContext.Current.Username : ExecutionContext.EnvironmentUsername;
                cl.ChangeLog.CreatedDate = ExecutionContext.HasCurrent ? ExecutionContext.Current.Timestamp : Cleaner.Clean(DateTime.Now);
            }

            return await Invoker.InvokeAsync(this, async () =>
            {
                TModel model;
                if (args is EfDbArgs<T, TModel> ema)
                    model = ema.EntityMapper.MapToDest(value, Mapper.OperationTypes.Create) ?? throw new InvalidOperationException("Mapping to the EF entity must not result in a null value.");
                else
                    model = (args as EfDbArgs)!.Mapper.Map<T, TModel>(value, Mapper.OperationTypes.Create) ?? throw new InvalidOperationException("Mapping to the EF entity must not result in a null value.");

                // On create the tenant id must have a value specified.
                if (model is IMultiTenant mt)
                    mt.TenantId = ExecutionContext.Current.TenantId;

                DbContext.Add(model);

                if (args.SaveChanges)
                    await DbContext.SaveChangesAsync(true).ConfigureAwait(false);

                if (!args.Refresh)
                    return value;
                else if (args is EfDbArgs<T, TModel> ema2)
                    return ema2.EntityMapper.MapToSrce(model, Mapper.OperationTypes.Get)!;
                else
                    return (args as EfDbArgs)!.Mapper.Map<TModel, T>(model, Mapper.OperationTypes.Get)!;
            }, this).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs an update for the value (reselects and/or automatically saves changes where specified).
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The entity framework model <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="IEfDbArgs"/>.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>The value (refreshed where specified).</returns>
        public async Task<T> UpdateAsync<T, TModel>(IEfDbArgs args, T value) where T : class, new() where TModel : class, new()
        {
            CheckSaveArgs<T, TModel>(args);

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (value is IChangeLog cl)
            {
                if (cl.ChangeLog == null)
                    cl.ChangeLog = new ChangeLog();

                cl.ChangeLog.UpdatedBy = ExecutionContext.HasCurrent ? ExecutionContext.Current.Username : ExecutionContext.EnvironmentUsername;
                cl.ChangeLog.UpdatedDate = ExecutionContext.HasCurrent ? ExecutionContext.Current.Timestamp : Cleaner.Clean(DateTime.Now);
            }

            return await Invoker.InvokeAsync(this, async () =>
            {
                // Check (find) if the entity exists.
                object?[] efKeys;
                if (args is EfDbArgs<T, TModel> ema)
                {
                    efKeys = new object?[ema.EntityMapper.UniqueKey.Count];
                    for (int i = 0; i < ema.EntityMapper.UniqueKey.Count; i++)
                    {
                        var v = ema.EntityMapper.UniqueKey[i].GetSrceValue(value, Mapper.OperationTypes.Unspecified);
                        efKeys[i] = ema.EntityMapper.UniqueKey[i].ConvertToDestValue(v, Mapper.OperationTypes.Unspecified)!;
                    }
                }
                else
                {
                    if (value is IUniqueKey uk)
                        efKeys = uk.UniqueKey.Args;
                    else
                        throw new InvalidOperationException("Value Type must implement `IUniqueKey` to be able to infer underlying key.");
                }

                var model = (TModel)await DbContext.FindAsync(typeof(TModel), efKeys).ConfigureAwait(false);
                if (model == null)
                    throw new NotFoundException();

                // Remove the entity from the tracker before we attempt to update; otherwise, will use existing rowversion and concurrency will not work as expected.
                DbContext.Remove(model);
                DbContext.ChangeTracker.AcceptAllChanges();

                if (args is EfDbArgs<T, TModel> ema2)
                    ema2.EntityMapper.MapToDest(value, model, Mapper.OperationTypes.Update);
                else
                    (args as EfDbArgs)!.Mapper.Map<T, TModel>(value, model, Mapper.OperationTypes.Update);

                DbContext.Update(model);

                if (args.SaveChanges)
                    await DbContext.SaveChangesAsync(true).ConfigureAwait(false);

                if (!args.Refresh)
                    return value;
                else if (args is EfDbArgs<T, TModel> ema3)
                    return ema3.EntityMapper.MapToSrce(model, Mapper.OperationTypes.Get)!;
                else
                    return (args as EfDbArgs)!.Mapper.Map<TModel, T>(model, Mapper.OperationTypes.Get)!;
            }, this).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs a delete for the specified <paramref name="keys"/>.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The entity framework model <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="IEfDbArgs"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <remarks>Where the model implements <see cref="ILogicallyDeleted"/> then this will update the <see cref="ILogicallyDeleted.IsDeleted"/> with <c>true</c> versus perform a physical deletion.</remarks>
        public async Task DeleteAsync<T, TModel>(IEfDbArgs args, params IComparable[] keys) where T : class, new() where TModel : class, new()
        {
            CheckSaveArgs<T, TModel>(args);

            object[] efKeys = new object[keys.Length];
            if (args is EfDbArgs<T, TModel> ema)
            {
                CheckKeys(ema, keys);
                for (int i = 0; i < ema.EntityMapper.UniqueKey.Count; i++)
                {
                    efKeys[i] = ema.EntityMapper.UniqueKey[i].ConvertToDestValue(keys[i], Mapper.OperationTypes.Unspecified)!;
                }
            }
            else
                keys.CopyTo(efKeys, 0);

            await Invoker.InvokeAsync(this, async () =>
            {
                // A pre-read is required to get the row version for concurrency.
                var model = (TModel)await DbContext.FindAsync(typeof(TModel), efKeys).ConfigureAwait(false);
                if (model == null)
                    throw new NotFoundException();

                if (model is ILogicallyDeleted emld)
                {
                    emld.IsDeleted = true;
                    DbContext.Update(model);
                }
                else
                    DbContext.Remove(model);

                if (args.SaveChanges)
                    await DbContext.SaveChangesAsync(true).ConfigureAwait(false);
            }, this).ConfigureAwait(false);
        }

        /// <summary>
        /// Checks keys provided and match against defined.
        /// </summary>
        private static void CheckKeys<T, TModel>(EfDbArgs<T, TModel> args, IComparable[] keys) where T : class, new() where TModel : class, new()
        {
            if (keys == null || keys.Length == 0)
                throw new ArgumentNullException(nameof(keys));

            if (keys.Length != args.EntityMapper.UniqueKey.Count)
                throw new ArgumentException($"The specified keys count '{keys.Length}' does not match the Mapper UniqueKey count '{args.EntityMapper.UniqueKey.Count}'.", nameof(keys));
        }

        /// <summary>
        /// Check the consistency of the save arguments.
        /// </summary>
        private static void CheckSaveArgs<T, TModel>(IEfDbArgs saveArgs) where T : class, new() where TModel : class, new()
        {
            if (saveArgs == null)
                throw new ArgumentNullException(nameof(saveArgs));

            if (saveArgs.Refresh && !saveArgs.SaveChanges)
                throw new ArgumentException("The Refresh property cannot be set to true without the SaveChanges also being set to true (given the save will occur after this method call).", nameof(saveArgs));
        }

        /// <summary>
        /// Performs the EF select single (find).
        /// </summary>
        private async Task<T> FindAsync<T, TModel>(IEfDbArgs args, object[] keys) where T : class, new() where TModel : class, new()
        {
            var model = await DbContext.FindAsync<TModel>(keys).ConfigureAwait(false);
            if (model == default)
                return default!;

            if (args is EfDbArgs<T, TModel> ema)
                return ema.EntityMapper.MapToSrce(model, Mapper.OperationTypes.Get) ?? throw new InvalidOperationException("Mapping from the EF entity must not result in a null value.");
            else
                return (args as EfDbArgs)!.Mapper.Map<T>(model) ?? throw new InvalidOperationException("Mapping from the EF entity must not result in a null value.");
        }
    }
}