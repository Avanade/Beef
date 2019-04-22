// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;

namespace Beef
{
    /// <summary>
    /// Manages the automatic creation and lifetime of the likes of connections across multiple invocations within the context of an executing
    /// <b>Thread</b> (see <see cref="ThreadStaticAttribute"/>). This enables the likes of database connections, contexts, or other expensive objects to be shared.
    /// </summary>
    public class DataContextScope : IDisposable
    {
        private static Dictionary<Type, Delegate> _registered = new Dictionary<Type, Delegate>();

        private DataContextScope _parent;
        private DataContextScopeOption _option;
        private Dictionary<Type, object> _dataContexts = null;

        /// <summary>
        /// Register a <b>context</b> and its creation function.
        /// </summary>
        /// <typeparam name="T">The registering <see cref="Type"/>.</typeparam>
        /// <typeparam name="TDc">The data context <see cref="Type"/>.</typeparam>
        /// <param name="create">The data context creation function.</param>
        public static void RegisterContext<T, TDc>(Func<TDc> create) where TDc : class
        {
            if (create == null)
                throw new ArgumentNullException("create");

            _registered.Add(typeof(T), create);
        }

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
        public static DataContextScope Current
        {
            get { return ExecutionContext.Current.DataContextScope; }
            internal set { ExecutionContext.Current.DataContextScope = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataContextScope"/> class referencing its parent.
        /// </summary>
        private DataContextScope(DataContextScope parent, DataContextScopeOption option)
        {
            _parent = parent;
            _option = option;
            if (IsPrimaryInstance)
                _dataContexts = new Dictionary<Type, object>();
            else
                _dataContexts = parent._dataContexts;
        }

        /// <summary>
        /// Indicates whether the current instance is considered a primary instance.
        /// </summary>
        private bool IsPrimaryInstance
        {
            get { return _parent == null || _option == DataContextScopeOption.RequiresNew; }
        }

        /// <summary>
        /// Gets the context value for the <see cref="Type"/>.
        /// </summary>
        /// <typeparam name="T">The registering <see cref="Type"/>.</typeparam>
        /// <typeparam name="TDc">The data context <see cref="Type"/>.</typeparam>
        /// <returns>The data context value.</returns>
        /// <remarks>Where a context does not already exist a new instance will be created.</remarks>
        public TDc GetContext<T, TDc>()
        {
            return (TDc)GetContext(typeof(T));
        }

        /// <summary>
        /// Gets the context value for the <see cref="Type"/>.
        /// </summary>
        /// <returns>The context value.</returns>
        /// <remarks>Where a context does not already exist a new instance will be created.</remarks>
        public object GetContext(Type type)
        {
            if (_dataContexts.ContainsKey(type))
                return _dataContexts[type];

            if (!_registered.ContainsKey(type))
                throw new InvalidOperationException("Type must be registered (see RegisterContext) to enable creation.");

            var val = _registered[type].DynamicInvoke();
            _dataContexts.Add(type, val);
            return val;
        }

        /// <summary>
        /// Closes and disposes the <see cref="DataContextScope"/>.
        /// </summary>
        public void Dispose()
        {
            DisposeAll();
            Current = _parent;
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
                foreach (KeyValuePair<Type, object> pair in _dataContexts)
                {
                    if (pair.Value != null)
                    {
                        if (pair.Value is IDisposable val)
                            val.Dispose();
                    }
                }
            }

            _dataContexts.Clear();
        }
    }
}