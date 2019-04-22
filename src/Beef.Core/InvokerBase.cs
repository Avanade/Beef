// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Beef
{
    /// <summary>
    /// Wraps an <see cref="Invoke(object, Action, TParam, string, string, int)"/> and <see cref="InvokeAsync(object, Func{Task}, TParam, string, string, int)"/>
    /// enabling standard functionality to be added to all invocations. 
    /// </summary>
    /// <typeparam name="TInvoker">The <see cref="InvokerBase{TInvoker, TParam, TResult}"/>; being itself for singleton purposes.</typeparam>
    /// <typeparam name="TParam">The optional parameter <see cref="Type"/> (for an <b>Invoke</b>).</typeparam>
    /// <remarks>Provided to enable the likes of logging and/or exception management, etc. in an optional and consistent manner where required
    /// (see <see cref="SetWrapper(Action{InvokerArgsSync{TParam}}, Func{InvokerArgsAsync{TParam}, Task})"/> to enable).</remarks>
    [DebuggerStepThrough()]
    public class InvokerBase<TInvoker, TParam> where TInvoker : InvokerBase<TInvoker, TParam>, new()
    {
        private InvokerSync<TParam> _invokerSync = new InvokerSync<TParam>();
        private InvokerAsync<TParam> _invokerAsync = new InvokerAsync<TParam>();

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static TInvoker Default { get; } = new TInvoker();

        /// <summary>
        /// Sets the primary (base) wrapper(s).
        /// </summary>
        /// <param name="baseSyncWrapper">The primary (base) synchronous invocatiom wrapper.</param>
        /// <param name="baseAsyncWrapper">The primary (base) asynchronous invocatiom wrapper.</param>
        protected void SetBaseWrapper(Action<InvokerArgsSync<TParam>> baseSyncWrapper = null, Func<InvokerArgsAsync<TParam>, Task> baseAsyncWrapper = null)
        {
            _invokerSync.SetBaseWrapper(baseSyncWrapper);
            _invokerAsync.SetBaseWrapper(baseAsyncWrapper);
        }

        /// <summary>
        /// Sets the secondary wrapper(s).
        /// </summary>
        /// <param name="syncWrapper">The secondary synchronous invocatiom wrapper.</param>
        /// <param name="asyncWrapper">The secondary asynchronous invocatiom wrapper.</param>
        public void SetWrapper(Action<InvokerArgsSync<TParam>> syncWrapper = null, Func<InvokerArgsAsync<TParam>, Task> asyncWrapper = null)
        {
            _invokerSync.SetWrapper(syncWrapper);
            _invokerAsync.SetWrapper(asyncWrapper);
        }

        /// <summary>
        /// Invokes an <paramref name="action"/> synchronously.
        /// </summary>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="action">The function to invoke.</param>
        /// <param name="param">The optional parameter passed to the invoke.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        public void Invoke(object caller, Action action, TParam param = default(TParam), [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            _invokerSync.Invoke(caller, action, param, memberName, filePath, lineNumber);
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
        public Task InvokeAsync(object caller, Func<Task> func, TParam param = default(TParam), [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return _invokerAsync.InvokeAsync(caller, func, param, memberName, filePath, lineNumber);
        }
    }

    /// <summary>
    /// Wraps an <see cref="Invoke(object, Func{TResult}, TParam, string, string, int)"/> and <see cref="InvokeAsync(object, Func{Task{TResult}}, TParam, string, string, int)"/>
    /// enabling standard functionality to be added to all invocations with an expected result. 
    /// </summary>
    /// <typeparam name="TInvoker">The <see cref="InvokerBase{TInvoker, TParam, TResult}"/>; being itself for singleton purposes.</typeparam>
    /// <typeparam name="TParam">The optional parameter <see cref="Type"/> (for an <b>Invoke</b>).</typeparam>
    /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
    /// <remarks>Provided to enable the likes of logging and/or exception management, etc. in an optional and consistent manner where required
    /// (see <see cref="SetWrapper(Func{InvokerArgsSync{TParam, TResult}, TResult}, Func{InvokerArgsAsync{TParam, TResult}, Task{TResult}})"/> to enable).</remarks>
    [DebuggerStepThrough()]
    public class InvokerBase<TInvoker, TParam, TResult> where TInvoker : InvokerBase<TInvoker, TParam, TResult>, new()
    {
        private InvokerSync<TParam, TResult> _invokerSyncResult = new InvokerSync<TParam, TResult>();
        private InvokerAsync<TParam, TResult> _invokerAsyncResult = new InvokerAsync<TParam, TResult>();

        /// <summary>
        /// Gets the default (singleton) instance.
        /// </summary>
        public static TInvoker Default { get; } = new TInvoker();

        /// <summary>
        /// Sets the primary (base) wrapper(s).
        /// </summary>
        /// <param name="baseSyncResultWrapper">The primary (base) synchronous with a result invocation wrapper.</param>
        /// <param name="baseAsyncResultWrapper">The primary (base) asynchronous with a result invocation wrapper.</param>
        protected void SetBaseWrapper(Func<InvokerArgsSync<TParam, TResult>, TResult> baseSyncResultWrapper = null, Func<InvokerArgsAsync<TParam, TResult>, Task<TResult>> baseAsyncResultWrapper = null)
        {
            _invokerSyncResult.SetBaseWrapper(baseSyncResultWrapper);
            _invokerAsyncResult.SetBaseWrapper(baseAsyncResultWrapper);
        }

        /// <summary>
        /// Sets the secondary wrapper(s).
        /// </summary>
        /// <param name="syncResultWrapper">The secondary synchronous with a result invocation wrapper.</param>
        /// <param name="asyncResultWrapper">The secondary asynchronous with a result invocation wrapper.</param>
        public void SetWrapper(Func<InvokerArgsSync<TParam, TResult>, TResult> syncResultWrapper = null, Func<InvokerArgsAsync<TParam, TResult>, Task<TResult>> asyncResultWrapper = null)
        {
            _invokerSyncResult.SetWrapper(syncResultWrapper);
            _invokerAsyncResult.SetWrapper(asyncResultWrapper);
        }

        /// <summary>
        /// Invokes a <paramref name="func"/> synchronously.
        /// </summary>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="param">The optional parameter passed to the invoke.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        public TResult Invoke(object caller, Func<TResult> func, TParam param = default(TParam), [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return _invokerSyncResult.Invoke(caller, func, param, memberName, filePath, lineNumber);
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
        public Task<TResult> InvokeAsync(object caller, Func<Task<TResult>> func, TParam param = default(TParam), [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return _invokerAsyncResult.InvokeAsync(caller, func, param, memberName, filePath, lineNumber);
        }
    }

    #region InvokerSync/Async

    /// <summary>
    /// Wraps an <see cref="Invoke"/> enabling standard functionality to be added to all invocations. 
    /// </summary>
    /// <typeparam name="TParam">The optional parameter <see cref="Type"/> (for an <b>Invoke</b>).</typeparam>
    [DebuggerStepThrough()]
    public class InvokerSync<TParam>
    {
        private Action<InvokerArgsSync<TParam>> _baseWrapper = (args) => args.WorkCallbackSync();
        private Action<InvokerArgsSync<TParam>> _wrapper = (args) => args.WorkCallbackSync();
        private bool _baseWrapperIsSet = false;
        private bool _wrapperIsSet = false;

        /// <summary>
        /// Sets the primary (base) wrapper for the <see cref="InvokerArgsSync{TParam}.WorkCallbackSync"/>.
        /// </summary>
        /// <param name="baseWrapper">The primary (base) wrapper for the <see cref="InvokerArgsSync{TParam}.WorkCallbackSync"/>.</param>
        public void SetBaseWrapper(Action<InvokerArgsSync<TParam>> baseWrapper)
        {
            Check.IsFalse(_baseWrapperIsSet, "SetBaseWrapper can only be set once.");
            _baseWrapperIsSet = true;
            if (baseWrapper != null)
                _baseWrapper = baseWrapper;
        }

        /// <summary>
        /// Sets the secondary wrapper for the <see cref="InvokerArgsSync{TParam}.WorkCallbackSync"/>.
        /// </summary>
        /// <param name="wrapper">The secondary wrapper for the <see cref="InvokerArgsSync{TParam}.WorkCallbackSync"/>.</param>
        public void SetWrapper(Action<InvokerArgsSync<TParam>> wrapper)
        {
            Check.IsFalse(_wrapperIsSet, "SetWrapper can only be set once.");
            _wrapperIsSet = true;
            if (wrapper != null)
                _wrapper = wrapper;
        }

        /// <summary>
        /// Invokes an <paramref name="action"/> synchronously.
        /// </summary>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="action">The function to invoke.</param>
        /// <param name="param">The optional parameter passed to the invoke.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        public void Invoke(object caller, Action action, TParam param = default(TParam), [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            _baseWrapper(new InvokerArgsSync<TParam>
            {
                Caller = caller,
                Param = param,
                MemberName = memberName,
                FilePath = filePath,
                LineNumber = lineNumber,
                WorkCallbackSync = () =>
                {
                    _wrapper(new InvokerArgsSync<TParam>
                    {
                        Caller = caller,
                        Param = param,
                        MemberName = memberName,
                        FilePath = filePath,
                        LineNumber = lineNumber,
                        WorkCallbackSync = () => action()
                    });
                },
            });
        }
    }

    /// <summary>
    /// Wraps an <see cref="Invoke"/> with a result enabling standard functionality to be added to all invocations. 
    /// </summary>
    /// <typeparam name="TParam">The optional parameter <see cref="Type"/> (for an <b>Invoke</b>).</typeparam>
    /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
    [DebuggerStepThrough()]
    public class InvokerSync<TParam, TResult>
    {
        private Func<InvokerArgsSync<TParam, TResult>, TResult> _baseWrapper = (args) => args.WorkCallbackSync();
        private Func<InvokerArgsSync<TParam, TResult>, TResult> _wrapper = (args) => args.WorkCallbackSync();
        private bool _baseWrapperIsSet = false;
        private bool _wrapperIsSet = false;

        /// <summary>
        /// Sets the primary (base) wrapper for the <see cref="InvokerArgsSync{TParam, TResult}.WorkCallbackSync"/>.
        /// </summary>
        /// <param name="baseWrapper">The primary (base) wrapper for the <see cref="InvokerArgsSync{TParam, TResult}.WorkCallbackSync"/>.</param>
        public void SetBaseWrapper(Func<InvokerArgsSync<TParam, TResult>, TResult> baseWrapper)
        {
            Check.IsFalse(_baseWrapperIsSet, "SetBaseWrapper can only be set once.");
            _baseWrapperIsSet = true;
            if (baseWrapper != null)
                _baseWrapper = baseWrapper;
        }

        /// <summary>
        /// Sets the secondary wrapper for the <see cref="InvokerArgsSync{TParam, TResult}.WorkCallbackSync"/>.
        /// </summary>
        /// <param name="wrapper">The secondary wrapper for the <see cref="InvokerArgsSync{TParam, TResult}.WorkCallbackSync"/>.</param>
        public void SetWrapper(Func<InvokerArgsSync<TParam, TResult>, TResult> wrapper)
        {
            Check.IsFalse(_wrapperIsSet, "SetWrapper can only be set once.");
            _wrapperIsSet = true;
            if (wrapper != null)
                _wrapper = wrapper;
        }

        /// <summary>
        /// Invokes a <paramref name="func"/> synchronously.
        /// </summary>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="param">The optional parameter passed to the invoke.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        public TResult Invoke(object caller, Func<TResult> func, TParam param = default(TParam), [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return _baseWrapper(new InvokerArgsSync<TParam, TResult>
            {
                Caller = caller,
                Param = param,
                MemberName = memberName,
                FilePath = filePath,
                LineNumber = lineNumber,
                WorkCallbackSync = () =>
                {
                    return _wrapper(new InvokerArgsSync<TParam, TResult>
                    {
                        Caller = caller,
                        Param = param,
                        MemberName = memberName,
                        FilePath = filePath,
                        LineNumber = lineNumber,
                        WorkCallbackSync = () => func()
                    });
                },
            });
        }
    }

    /// <summary>
    /// Wraps an <see cref="InvokeAsync"/> enabling standard functionality to be added to all invocations. 
    /// </summary>
    /// <typeparam name="TParam">The optional parameter <see cref="Type"/> (for an <b>Invoke</b>).</typeparam>
    [DebuggerStepThrough()]
    public class InvokerAsync<TParam>
    {
        private Func<InvokerArgsAsync<TParam>, Task> _baseWrapper = (args) => args.WorkCallbackAsync();
        private Func<InvokerArgsAsync<TParam>, Task> _wrapper = (args) => args.WorkCallbackAsync();
        private bool _baseWrapperIsSet = false;
        private bool _wrapperIsSet = false;

        /// <summary>
        /// Sets the primary (base) wrapper for the <see cref="InvokerArgsAsync{TParam}.WorkCallbackAsync"/>.
        /// </summary>
        /// <param name="baseWrapper">The primary (base) wrapper for the <see cref="InvokerArgsAsync{TParam}.WorkCallbackAsync"/>.</param>
        public void SetBaseWrapper(Func<InvokerArgsAsync<TParam>, Task> baseWrapper = null)
        {
            Check.IsFalse(_baseWrapperIsSet, "SetBaseWrapper can only be set once.");
            _baseWrapperIsSet = true;
            if (baseWrapper != null)
                _baseWrapper = Check.NotNull(baseWrapper, nameof(baseWrapper));
        }

        /// <summary>
        /// Sets the secondary wrapper for the <see cref="InvokerArgsAsync{TParam}.WorkCallbackAsync"/>.
        /// </summary>
        /// <param name="wrapper">The secondary wrapper for the <see cref="InvokerArgsAsync{TParam}.WorkCallbackAsync"/>.</param>
        public void SetWrapper(Func<InvokerArgsAsync<TParam>, Task> wrapper)
        {
            Check.IsFalse(_wrapperIsSet, "SetWrapper can only be set once.");
            _wrapperIsSet = true;
            if (wrapper != null)
                _wrapper = wrapper;
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
        public Task InvokeAsync(object caller, Func<Task> func, TParam param = default(TParam), [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return _baseWrapper(new InvokerArgsAsync<TParam>
            {
                Caller = caller,
                Param = param,
                MemberName = memberName,
                FilePath = filePath,
                LineNumber = lineNumber,
                WorkCallbackAsync = () =>
                {
                    return _wrapper(new InvokerArgsAsync<TParam>
                    {
                        Caller = caller,
                        Param = param,
                        MemberName = memberName,
                        FilePath = filePath,
                        LineNumber = lineNumber,
                        WorkCallbackAsync = () => func()
                    });
                },
            });
        }
    }

    /// <summary>
    /// Wraps an <see cref="InvokeAsync"/> with a result enabling standard functionality to be added to all invocations. 
    /// </summary>
    /// <typeparam name="TParam">The optional parameter <see cref="Type"/> (for an <b>Invoke</b>).</typeparam>
    /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
    [DebuggerStepThrough()]
    public class InvokerAsync<TParam, TResult>
    {
        private Func<InvokerArgsAsync<TParam, TResult>, Task<TResult>> _baseWrapper = (args) => args.WorkCallbackAsync();
        private Func<InvokerArgsAsync<TParam, TResult>, Task<TResult>> _wrapper = (args) => args.WorkCallbackAsync();
        private bool _baseWrapperIsSet = false;
        private bool _wrapperIsSet = false;

        /// <summary>
        /// Sets the primary (base) wrapper for the <see cref="InvokerArgsAsync{TParam}.WorkCallbackAsync"/>.
        /// </summary>
        /// <param name="baseWrapper">The primary (base) wrapper for the <see cref="InvokerArgsAsync{TParam}.WorkCallbackAsync"/>.</param>
        public void SetBaseWrapper(Func<InvokerArgsAsync<TParam, TResult>, Task<TResult>> baseWrapper = null)
        {
            Check.IsFalse(_baseWrapperIsSet, "SetBaseWrapper can only be set once.");
            _baseWrapperIsSet = true;
            if (baseWrapper != null)
                _baseWrapper = Check.NotNull(baseWrapper, nameof(baseWrapper));
        }

        /// <summary>
        /// Sets the secondary wrapper for the <see cref="InvokerArgsAsync{TParam}.WorkCallbackAsync"/>.
        /// </summary>
        /// <param name="wrapper">The secondary wrapper for the <see cref="InvokerArgsAsync{TParam}.WorkCallbackAsync"/>.</param>
        public void SetWrapper(Func<InvokerArgsAsync<TParam, TResult>, Task<TResult>> wrapper)
        {
            Check.IsFalse(_wrapperIsSet, "SetWrapper can only be set once.");
            _wrapperIsSet = true;
            if (wrapper != null)
                _wrapper = wrapper;
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
        public Task<TResult> InvokeAsync(object caller, Func<Task<TResult>> func, TParam param = default(TParam), [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return _baseWrapper(new InvokerArgsAsync<TParam, TResult>
            {
                Caller = caller,
                Param = param,
                MemberName = memberName,
                FilePath = filePath,
                LineNumber = lineNumber,
                WorkCallbackAsync = () =>
                {
                    return _wrapper(new InvokerArgsAsync<TParam, TResult>
                    {
                        Caller = caller,
                        Param = param,
                        MemberName = memberName,
                        FilePath = filePath,
                        LineNumber = lineNumber,
                        WorkCallbackAsync = () => func()
                    });
                },
            });
        }
    }

    #endregion
}