// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Beef
{
    /// <summary>
    /// Manages the automatic creation and lifetime of the likes of connections across multiple invocations within the context of an executing
    /// <b>Thread</b> (see <see cref="ExecutionContext"/>). This enables the likes of database connections, contexts, or other expensive objects to be shared.
    /// </summary>
    public sealed class DataContextScope : IDisposable
    {
        private static readonly ConcurrentDictionary<Guid, Delegate> _registered = new ConcurrentDictionary<Guid, Delegate>();

        private readonly DataContextScope? _parent;
        private readonly DataContextScopeOption _option;
        private readonly ConcurrentDictionary<Guid, object> _dataContexts;
        private bool _disposed;

        /// <summary>
        /// Registers a <b>context</b> and its creation function.
        /// </summary>
        /// <typeparam name="TDc">The data context <see cref="Type"/>.</typeparam>
        /// <param name="identifier">The unique identifier.</param>
        /// <param name="create">The data context creation function.</param>
        public static void RegisterContext<TDc>(Guid identifier, Func<TDc> create) where TDc : class
        {
            Check.NotNull(create, nameof(create));
            _registered.TryAdd(identifier, create);
        }

        /// <summary>
        /// Deregisters a preivously registered <b>context</b>.
        /// </summary>
        /// <param name="identifier">The unique identifier.</param>
        public static void DeregisterContext(Guid identifier) => _registered.TryRemove(identifier, out _);

        /// <summary>
        /// Begins a new <see cref="DataContextScope"/>.
        /// </summary>
        /// <param name="option">The <see cref="DataContextScopeOption"/>.</param>
        /// <returns>A <see cref="DataContextScope"/>.</returns>
        /// <remarks>The <paramref name="option"/> determines whether a new connection is created or the existing (default) is leveraged.</remarks>
        public static DataContextScope Begin(DataContextScopeOption option = DataContextScopeOption.UseExisting)
        {
            Current = new DataContextScope(Current, option);
            return Current;
        }

        /// <summary>
        /// Invoke an <paramref name="action"/> within a <see cref="DataContextScope"/>.
        /// </summary>
        /// <param name="action">The <see cref="Action"/> to invoke.</param>
        /// <param name="option">The <see cref="DataContextScopeOption"/>.</param>
        public static void Invoke(Action action, DataContextScopeOption option = DataContextScopeOption.UseExisting)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            using (Begin(option))
            {
                action.Invoke();
            }
        }

        /// <summary>
        /// Invoke a <paramref name="func"/> within a <see cref="DataContextScope"/>.
        /// </summary>
        /// <typeparam name="TResult">The return <see cref="Type"/>.</typeparam>
        /// <param name="func">The <see cref="Func{TResult}"/> to invoke.</param>
        /// <param name="option">The <see cref="DataContextScopeOption"/>.</param>
        /// <returns>The result of the function.</returns>
        public static TResult Invoke<TResult>(Func<TResult> func, DataContextScopeOption option = DataContextScopeOption.UseExisting)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            using (Begin(option))
            {
                return func.Invoke();
            }
        }

        /// <summary>
        /// Gets the _current <see cref="DataContextScope"/>; otherwise, <c>null</c>.
        /// </summary>
        public static DataContextScope? Current
        {
            get { return ExecutionContext.Current.DataContextScope; }
            internal set { ExecutionContext.Current.DataContextScope = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataContextScope"/> class referencing its parent.
        /// </summary>
        private DataContextScope(DataContextScope? parent, DataContextScopeOption option)
        {
            _parent = parent;
            _option = option;
            if (IsPrimaryInstance)
                _dataContexts = new ConcurrentDictionary<Guid, object>();
            else
                _dataContexts = parent!._dataContexts;
        }

        /// <summary>
        /// Indicates whether the current instance is considered a primary instance.
        /// </summary>
        private bool IsPrimaryInstance
        {
            get { return _parent == null || _option == DataContextScopeOption.RequiresNew; }
        }

        /// <summary>
        /// Gets the context value for the unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier.</param>
        /// <returns>The context value.</returns>
        /// <remarks>Where a context does not already exist a new instance will be created.</remarks>
        public object GetContext(Guid identifier)
        {
            if (!_registered.TryGetValue(identifier, out var del))
                throw new InvalidOperationException("Identifier must be registered (see RegisterContext) to enable.");

            return _dataContexts.GetOrAdd(identifier, id =>
            {
                if (!_registered.TryGetValue(identifier, out var del))
                    throw new InvalidOperationException("Identifier must be registered (see RegisterContext) to enable.");

                return del.DynamicInvoke();
            });
        }

        /// <summary>
        /// Closes and disposes the <see cref="DataContextScope"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataContextScope"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                DisposeAll();
                Current = _parent;
            }

            _disposed = true;
        }

        /// <summary>
        /// Disposes all context objects. 
        /// </summary>
        private void DisposeAll()
        {
            if (!IsPrimaryInstance)
                return;

            // Close and dispose of all object contexts.
            if (_dataContexts != null && _dataContexts.Count > 0)
            {
                foreach (KeyValuePair<Guid, object> pair in _dataContexts)
                {
                    if (pair.Value != null)
                    {
                        if (pair.Value is IDisposable val)
                            val.Dispose();
                    }
                }
            }

            _dataContexts?.Clear();
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~DataContextScope()
        {
            Dispose(false);
        }
    }
}