// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Beef
{
    /// <summary>
    /// Wraps an <b>Invoke</b> enabling standard functionality to be added to all invocations. 
    /// </summary>
    /// <typeparam name="TParam">The optional parameter <see cref="Type"/> (for an <b>Invoke</b>).</typeparam>
    [DebuggerStepThrough()]
    public abstract class InvokerBase<TParam> : Invoker
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
        public void Invoke(object caller, Action action, TParam? param = default, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            => WrapInvoke(caller, action, param, memberName, filePath, lineNumber);

        /// <summary>
        /// Invokes an <paramref name="action"/> synchronously.
        /// </summary>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="action">The function to invoke.</param>
        /// <param name="param">The optional parameter passed to the invoke.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        protected virtual void WrapInvoke(object caller, Action action, TParam? param = default, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            => Check.NotNull(action, nameof(action)).Invoke();

        /// <summary>
        /// Invokes a <paramref name="func"/> asynchronously.
        /// </summary>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="param">The optional parameter passed to the invoke.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        public async Task InvokeAsync(object caller, Func<Task> func, TParam? param = default, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            => await WrapInvokeAsync(caller, func, param, memberName, filePath, lineNumber).ConfigureAwait(false);

        /// <summary>
        /// Invokes a <paramref name="func"/> asynchronously.
        /// </summary>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="param">The optional parameter passed to the invoke.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        protected virtual async Task WrapInvokeAsync(object caller, Func<Task> func, TParam? param = default, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            Check.NotNull(func, nameof(func));
            await func.Invoke().ConfigureAwait(false);
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
        public TResult Invoke<TResult>(object caller, Func<TResult> func, TParam? param = default, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            => WrapInvoke(caller, func, param, memberName, filePath, lineNumber);

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
        protected virtual TResult WrapInvoke<TResult>(object caller, Func<TResult> func, TParam? param = default, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            => Check.NotNull(func, nameof(func)).Invoke();

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
        public Task<TResult> InvokeAsync<TResult>(object caller, Func<Task<TResult>> func, TParam? param = default, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            => WrapInvokeAsync(caller, func, param, memberName, filePath, lineNumber);

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
        protected virtual Task<TResult> WrapInvokeAsync<TResult>(object caller, Func<Task<TResult>> func, TParam? param = default, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            => Check.NotNull(func, nameof(func)).Invoke();

        #endregion
    }

    /// <summary>
    /// Wraps an <b>Invoke</b> enabling standard functionality to be added to all invocations. 
    /// </summary>
    /// <typeparam name="TParam">The optional parameter <see cref="Type"/> (for an <b>Invoke</b>).</typeparam>
    /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
    [DebuggerStepThrough()]
    public abstract class InvokerBase<TParam, TResult> : Invoker
    {
        /// <summary>
        /// Invokes a <paramref name="func"/> with a <typeparamref name="TResult"/> synchronously.
        /// </summary>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="param">The optional parameter passed to the invoke.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The result.</returns>
        public TResult Invoke(object caller, Func<TResult> func, TParam? param = default, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            => WrapInvoke(caller, func, param, memberName, filePath, lineNumber);

        /// <summary>
        /// Invokes a <paramref name="func"/> with a <typeparamref name="TResult"/> synchronously.
        /// </summary>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="param">The optional parameter passed to the invoke.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The result.</returns>
        protected virtual TResult WrapInvoke(object caller, Func<TResult> func, TParam? param = default, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            => Check.NotNull(func, nameof(func)).Invoke();

        /// <summary>
        /// Invokes a <paramref name="func"/> with a <typeparamref name="TResult"/> asynchronously.
        /// </summary>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="param">The optional parameter passed to the invoke.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The result.</returns>
        public Task<TResult> InvokeAsync(object caller, Func<Task<TResult>> func, TParam? param = default, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            => WrapInvokeAsync(caller, func, param, memberName, filePath, lineNumber);

        /// <summary>
        /// Invokes a <paramref name="func"/> with a <typeparamref name="TResult"/> asynchronously.
        /// </summary>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="param">The optional parameter passed to the invoke.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The result.</returns>
        protected virtual Task<TResult> WrapInvokeAsync(object caller, Func<Task<TResult>> func, TParam? param = default, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            => Check.NotNull(func, nameof(func)).Invoke();
    }
}