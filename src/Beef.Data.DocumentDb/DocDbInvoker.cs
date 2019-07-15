// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Azure.Documents;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Beef.Data.DocumentDb
{
    /// <summary>
    /// Adds capabilities (wraps) an <see cref="InvokerBase{TInvoker, TParam}"/> enabling standard functionality to be added to all <see cref="DocDbBase"/> invocations
    /// specifically exception handling (see <see cref="DocDbBase.ExceptionHandler"/>).
    /// </summary>
    public class DocDbInvoker : InvokerBase<DocDbInvoker, DocDbBase>
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
        protected override void WrapInvoke(object caller, Action action, DocDbBase param = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                action();
            }
            catch (DocumentClientException dcex)
            {
                if (param != null)
                    param.ExceptionHandler?.Invoke(dcex);

                throw;
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex?.InnerException is DocumentClientException dcex)
                {
                    if (param != null)
                        param.ExceptionHandler?.Invoke(dcex);
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
        protected async override Task WrapInvokeAsync(object caller, Func<Task> func, DocDbBase param = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                await func();
            }
            catch (DocumentClientException dcex)
            {
                if (param != null)
                    param.ExceptionHandler?.Invoke(dcex);

                throw;
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex?.InnerException is DocumentClientException dcex)
                {
                    if (param != null)
                        param.ExceptionHandler?.Invoke(dcex);
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
        protected override TResult WrapInvoke<TResult>(object caller, Func<TResult> func, DocDbBase param = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                return func();
            }
            catch (DocumentClientException dcex)
            {
                if (param != null)
                    param.ExceptionHandler?.Invoke(dcex);

                throw;
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex?.InnerException is DocumentClientException dcex)
                {
                    if (param != null)
                        param.ExceptionHandler?.Invoke(dcex);
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
        protected async override Task<TResult> WrapInvokeAsync<TResult>(object caller, Func<Task<TResult>> func, DocDbBase param = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            try
            {
                return await func();
            }
            catch (DocumentClientException dcex)
            {
                if (param != null)
                    param.ExceptionHandler?.Invoke(dcex);

                throw;
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex?.InnerException is DocumentClientException dcex)
                {
                    if (param != null)
                        param.ExceptionHandler?.Invoke(dcex);
                }

                throw;
            }
        }

        #endregion
    }
}