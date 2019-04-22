// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef
{
    /// <summary>
    /// Provides the base invocation arguments.
    /// </summary>
    /// <typeparam name="TParam">The <see cref="Param"/> <see cref="Type"/>.</typeparam>
    public abstract class InvokerArgsBase<TParam>
    {
        /// <summary>
        /// Gets the calling (invoking) object.
        /// </summary>
        public object Caller { get; internal set; }

        /// <summary>
        /// Gets the optional parameter for the invoke wrapper.
        /// </summary>
        public TParam Param { get; internal set; }

        /// <summary>
        /// Gets the method or property name of the caller to the method.
        /// </summary>
        public string MemberName { get; internal set; }

        /// <summary>
        /// Gets the full path of the source file that contains the caller.
        /// </summary>
        public string FilePath { get; internal set; }

        /// <summary>
        /// Gets the line number in the source file at which the method is called.
        /// </summary>
        public int LineNumber { get; internal set; }
    }

    /// <summary>
    /// Provides the synchronous invocation arguments.
    /// </summary>
    /// <typeparam name="TParam">The <see cref="InvokerArgsBase{TParam}.Param"/> <see cref="Type"/>.</typeparam>
    public class InvokerArgsSync<TParam> : InvokerArgsBase<TParam>
    {
        /// <summary>
        /// Gets the synchronous callback function to invoke to do the work.
        /// </summary>
        public Action WorkCallbackSync { get; internal set; }
    }

    /// <summary>
    /// Provides the synchronous invocation arguments with a result.
    /// </summary>
    /// <typeparam name="TParam">The <see cref="InvokerArgsBase{TParam}.Param"/> <see cref="Type"/>.</typeparam>
    /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
    public class InvokerArgsSync<TParam, TResult> : InvokerArgsBase<TParam>
    {
        /// <summary>
        /// Gets the synchronous callback function to invoke to do the work.
        /// </summary>
        public Func<TResult> WorkCallbackSync { get; internal set; }
    }

    /// <summary>
    /// Provides the asynchronous invocation arguments.
    /// </summary>
    /// <typeparam name="TParam">The <see cref="InvokerArgsBase{TParam}.Param"/> <see cref="Type"/>.</typeparam>
    public class InvokerArgsAsync<TParam> : InvokerArgsBase<TParam>
    {
        /// <summary>
        /// Gets the asynchronous callback function to invoke to do the work.
        /// </summary>
        public Func<Task> WorkCallbackAsync { get; internal set; }
    }

    /// <summary>
    /// Provides the asynchronous invocation arguments with a result.
    /// </summary>
    /// <typeparam name="TParam">The <see cref="InvokerArgsBase{TParam}.Param"/> <see cref="Type"/>.</typeparam>
    /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
    public class InvokerArgsAsync<TParam, TResult> : InvokerArgsBase<TParam>
    {
        /// <summary>
        /// Gets the asynchronous callback function to invoke to do the work.
        /// </summary>
        public Func<Task<TResult>> WorkCallbackAsync { get; internal set; }
    }
}