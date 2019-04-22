// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;

namespace Beef.Data.Database
{
    /// <summary>
    /// Wraps a <see cref="DatabaseCommand"/> <b>invoke</b> enabling standard functionality to be added to all invocations. 
    /// </summary>
    /// <remarks>Any <see cref="SqlException"/> will be transformed using <see cref="DatabaseBase.ExceptionHandler"/>.</remarks>
    public class DatabaseInvoker : InvokerBase<DatabaseInvoker, DatabaseBase>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseInvoker"/> class.
        /// </summary>
        public DatabaseInvoker()
        {
            SetBaseWrapper((s) => WrappedInvoke(s), (a) => WrappedInvokeAsync(a));
        }

        /// <summary>
        /// Wrap the invoke synchronously.
        /// </summary>
        private void WrappedInvoke(InvokerArgsSync<DatabaseBase> args)
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
        private async Task WrappedInvokeAsync(InvokerArgsAsync<DatabaseBase> args)
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
    /// Wraps a <see cref="DatabaseCommand"/> <b>invoke</b> enabling standard functionality to be added to all invocations. 
    /// </summary>
    /// <remarks>Any <see cref="SqlException"/> will be transformed using <see cref="DatabaseBase.ExceptionHandler"/>.</remarks>
    public class DatabaseInvoker<TResult> : InvokerBase<DatabaseInvoker<TResult>, DatabaseBase, TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseInvoker{TResult}"/> class.
        /// </summary>
        public DatabaseInvoker()
        {
            SetBaseWrapper((s) => WrappedInvoke(s), (a) => WrappedInvokeAsync(a));
        }

        /// <summary>
        /// Wrap the invoke synchronously with result.
        /// </summary>
        private TResult WrappedInvoke(InvokerArgsSync<DatabaseBase, TResult> args)
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
        private async Task<TResult> WrappedInvokeAsync(InvokerArgsAsync<DatabaseBase, TResult> args)
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
