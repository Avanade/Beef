// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;

namespace Beef.Data.EntityFrameworkCore
{
    /// <summary>
    /// Wraps a <see cref="DbContext"/> <b>invoke</b> enabling standard functionality to be added to all invocations. 
    /// </summary>
    /// <remarks>Any <see cref="SqlException"/> will be transformed using the <see cref="T:DbBase.ExceptionHandler"/>.</remarks>
    public class EfDbInvoker<TDbContext> : InvokerBase<EfDbInvoker<TDbContext>, EfDbBase<TDbContext>> where TDbContext : DbContext, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EfDbInvoker{TDbContext}"/> class.
        /// </summary>
        public EfDbInvoker()
        {
            SetBaseWrapper((s) => WrappedInvoke(s), (a) => WrappedInvokeAsync(a));
        }

        /// <summary>
        /// Wrap the invoke synchronously.
        /// </summary>
        private void WrappedInvoke(InvokerArgsSync<EfDbBase<TDbContext>> args)
        {
            try
            {
                args.WorkCallbackSync();
            }
            catch (SqlException sex)
            {
                if (args.Param != null)
                    args.Param.ExceptionHandler?.Invoke(sex);

                throw;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException deux)
            {
                if (deux.InnerException != null && deux.InnerException is SqlException sex)
                {
                    if (args.Param != null)
                        args.Param.ExceptionHandler?.Invoke(sex);
                }

                throw;
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex?.InnerException is SqlException sex)
                {
                    if (args.Param != null)
                        args.Param.ExceptionHandler?.Invoke(sex);
                }

                throw;
            }
        }

        /// <summary>
        /// Wrap the invoke asynchronously.
        /// </summary>
        private async Task WrappedInvokeAsync(InvokerArgsAsync<EfDbBase<TDbContext>> args)
        {
            try
            {
                await args.WorkCallbackAsync();
            }
            catch (SqlException sex)
            {
                if (args.Param != null)
                    args.Param.ExceptionHandler?.Invoke(sex);

                throw;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException deux)
            {
                if (deux.InnerException != null && deux.InnerException is SqlException sex)
                {
                    if (args.Param != null)
                        args.Param.ExceptionHandler?.Invoke(sex);
                }

                throw;
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex?.InnerException is SqlException sex)
                {
                    if (args.Param != null)
                        args.Param.ExceptionHandler?.Invoke(sex);
                }

                throw;
            }
        }
    }

    /// <summary>
    /// Wraps a <see cref="DbContext"/> <b>invoke</b> enabling standard functionality to be added to all invocations. 
    /// </summary>
    /// <remarks>Any <see cref="SqlException"/> will be transformed using the <see cref="T:DbBase.ExceptionHandler"/>.</remarks>
    public class EfDbInvoker<TDbContext, TResult> : InvokerBase<EfDbInvoker<TDbContext, TResult>, EfDbBase<TDbContext>, TResult> where TDbContext : DbContext, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EfDbInvoker{TDbContext, TResult}"/> class.
        /// </summary>
        public EfDbInvoker()
        {
            SetBaseWrapper((s) => WrappedInvoke(s), (a) => WrappedInvokeAsync(a));
        }

        /// <summary>
        /// Wrap the invoke synchronously with result.
        /// </summary>
        private TResult WrappedInvoke(InvokerArgsSync<EfDbBase<TDbContext>, TResult> args)
        {
            try
            {
                return args.WorkCallbackSync();
            }
            catch (SqlException sex)
            {
                if (args.Param != null)
                    args.Param.ExceptionHandler?.Invoke(sex);

                throw;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException deux)
            {
                if (deux.InnerException != null && deux.InnerException is SqlException sex)
                {
                    if (args.Param != null)
                        args.Param.ExceptionHandler?.Invoke(sex);
                }

                throw;
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex?.InnerException is SqlException sex)
                {
                    if (args.Param != null)
                        args.Param.ExceptionHandler?.Invoke(sex);
                }

                throw;
            }
        }

        /// <summary>
        /// Wrap the invoke asynchronously with result.
        /// </summary>
        private async Task<TResult> WrappedInvokeAsync(InvokerArgsAsync<EfDbBase<TDbContext>, TResult> args)
        {
            try
            {
                return await args.WorkCallbackAsync();
            }
            catch (SqlException sex)
            {
                if (args.Param != null)
                    args.Param.ExceptionHandler?.Invoke(sex);

                throw;
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException deux)
            {
                if (deux.InnerException != null && deux.InnerException is SqlException sex)
                {
                    if (args.Param != null)
                        args.Param.ExceptionHandler?.Invoke(sex);
                }

                throw;
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex?.InnerException is SqlException sex)
                {
                    if (args.Param != null)
                        args.Param.ExceptionHandler?.Invoke(sex);
                }

                throw;
            }
        }
    }
}
