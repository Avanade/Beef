// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Data.SqlClient;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Beef.Data.Database
{
    /// <summary>
    /// Adds capabilities (wraps) an <see cref="InvokerBase{TParam}"/> enabling standard functionality to be added to all <see cref="DatabaseBase"/> invocations
    /// specifically exception handling (see <see cref="DatabaseBase.ExceptionHandler"/>).
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public class DatabaseInvoker : InvokerBase<DatabaseBase>
    {
        #region NoResult

        /// <summary>
        /// Invokes an <paramref name="action"/> synchronously.
        /// </summary>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="action">The function to invoke.</param>
        /// <param name="param">The optional parameter passed to the invoke.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        protected override void WrapInvoke(object caller, Action action, DatabaseBase? param = null, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                action();
            }
            catch (SqlException sex)
            {
                if (param != null)
                    param.ExceptionHandler?.Invoke(sex);

                throw;
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex?.InnerException is SqlException sex)
                {
                    if (param != null)
                        param.ExceptionHandler?.Invoke(sex);
                }

                throw;
            }
        }

        /// <summary>
        /// Invokes a <paramref name="func"/> asynchronously.
        /// </summary>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="param">The optional parameter passed to the invoke.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        protected async override Task WrapInvokeAsync(object caller, Func<Task> func, DatabaseBase? param = null, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            try
            {
                await func().ConfigureAwait(false);
            }
            catch (SqlException sex)
            {
                if (param != null)
                    param.ExceptionHandler?.Invoke(sex);

                throw;
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex?.InnerException is SqlException sex)
                {
                    if (param != null)
                        param.ExceptionHandler?.Invoke(sex);
                }

                throw;
            }
        }

        #endregion

        #region WithResult

        /// <summary>
        /// Invokes a <paramref name="func"/> with a <typeparamref name="TResult"/> synchronously.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="param">The optional parameter passed to the invoke.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The result.</returns>
        protected override TResult WrapInvoke<TResult>(object caller, Func<TResult> func, DatabaseBase? param = null, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            try
            {
                return func();
            }
            catch (SqlException sex)
            {
                if (param != null)
                    param.ExceptionHandler?.Invoke(sex);

                throw;
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex?.InnerException is SqlException sex)
                {
                    if (param != null)
                        param.ExceptionHandler?.Invoke(sex);
                }

                throw;
            }
        }

        /// <summary>
        /// Invokes a <paramref name="func"/> with a <typeparamref name="TResult"/> asynchronously.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="param">The optional parameter passed to the invoke.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The result.</returns>
        protected async override Task<TResult> WrapInvokeAsync<TResult>(object caller, Func<Task<TResult>> func, DatabaseBase? param = null, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            try
            {
                return await func().ConfigureAwait(false);
            }
            catch (SqlException sex)
            {
                if (param != null)
                    param.ExceptionHandler?.Invoke(sex);

                throw;
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex?.InnerException is SqlException sex)
                {
                    if (param != null)
                        param.ExceptionHandler?.Invoke(sex);
                }

                throw;
            }
        }

        #endregion
    }
}