// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using System;
using System.Collections.Generic;

namespace Beef.Executors
{
    /// <summary>
    /// Provides the base <see cref="Beef.Executors.Executor"/> arguments.
    /// </summary>
    public interface IExecutorBaseArgs
    {
        /// <summary>
        /// Gets the owning <see cref="Executor"/>.
        /// </summary>
        Executor Executor { get; }

        /// <summary>
        /// Gets the <see cref="Executor"/> <see cref="Executor.ExecutorArgs"/> value.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Gets the <see cref="ExecutorRunType"/>.
        /// </summary>
        ExecutorRunType RunType { get; }

        /// <summary>
        /// Gets the <see cref="System.Exception"/> raised when invoking the <see cref="RunType"/>.
        /// </summary>
        Exception Exception { get; }

        /// <summary>
        /// Indicates whether an <see cref="Exception"/> was raised when invoking the <see cref="RunType"/>.
        /// </summary>
        bool HasException { get; }

        /// <summary>
        /// Gets the properties <see cref="Dictionary{TKey, TValue}"/> for passing/storing additional data.
        /// </summary>
        Dictionary<string, object> Properties { get; }
    }

    /// <summary>
    /// Provides the <see cref="Beef.Executors.Executor"/> arguments.
    /// </summary>
    public interface IExecutorArgs : IExecutorBaseArgs
    {
        /// <summary>
        /// Gets the item index (only applicable when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.ItemRun"/>).
        /// </summary>
        long? ItemIndex { get; }

        /// <summary>
        /// Gets the item data (only applicable when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.ItemRun"/>).
        /// </summary>
        object ItemData { get; }

        /// <summary>
        /// Indicates when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.CompletionRun"/> that all the items were completed (i.e. not stopped).
        /// </summary>
        bool? IsCollCompleted { get; }
    }

    /// <summary>
    /// Represents the base <see cref="Executor"/> arguments.
    /// </summary>
    public abstract class ExecutorArgsBase : IExecutorBaseArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutorArgsBase"/> class.
        /// </summary>
        /// <param name="executor">The <see cref="Executor"/>.</param>
        /// <param name="runType">The <see cref="ExecutorRunType"/>.</param>
        internal ExecutorArgsBase(Executor executor, ExecutorRunType runType)
        {
            Executor = Check.NotNull(executor, nameof(executor));
            RunType = runType;
        }

        /// <summary>
        /// Sets the <see cref="IExecutorBaseArgs.Exception"/>.
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/>.</param>
        internal void SetException(Exception ex)
        {
            Exception = ex;
        }

        /// <summary>
        /// Gets the owning <see cref="Executor"/>.
        /// </summary>
        public Executor Executor { get; }

        /// <summary>
        /// Gets the <see cref="Executor"/> <see cref="Executor.ExecutorArgs"/> value.
        /// </summary>
        public object Value => Executor.ExecutorArgs;

        /// <summary>
        /// Gets the <see cref="ExecutorRunType"/>.
        /// </summary>
        public ExecutorRunType RunType { get; }

        /// <summary>
        /// Gets the <see cref="System.Exception"/> raised when invoking the <see cref="IExecutorBaseArgs.RunType"/>.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Indicates whether an <see cref="IExecutorBaseArgs.Exception"/> was raised when invoking the <see cref="IExecutorBaseArgs.RunType"/>.
        /// </summary>
        public bool HasException => Exception != null;

        /// <summary>
        /// Gets a <see cref="DataArgsLogger"/> using <see cref="IExecutorArgs"/> as the data arguments.
        /// </summary>
        public DataArgsLogger Logger => new DataArgsLogger(this);

        /// <summary>
        /// Gets the properties <see cref="Dictionary{TKey, TValue}"/> for passing/storing additional data.
        /// </summary>
        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();
    }

    #region ExecutorCollectionIterateArgs

    /// <summary>
    /// Represents the <see cref="Executor"/> <b>Collection</b> interation arguments.
    /// </summary>
    public sealed class ExecutorCollectionIterateArgs : ExecutorArgsBase, IExecutorArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutorArgsBase"/> class.
        /// </summary>
        /// <param name="executor">The <see cref="Executor"/>.</param>
        internal ExecutorCollectionIterateArgs(Executor executor) : base(executor, ExecutorRunType.CollectionIterate) { }

        /// <summary>
        /// Gets the item index (only applicable when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.ItemRun"/>).
        /// </summary>
        long? IExecutorArgs.ItemIndex => null;

        /// <summary>
        /// Gets the item data (only applicable when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.ItemRun"/>).
        /// </summary>
        object IExecutorArgs.ItemData => null;

        /// <summary>
        /// Indicates when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.CompletionRun"/> that all the items were completed (i.e. not stopped).
        /// </summary>
        bool? IExecutorArgs.IsCollCompleted => null;
    }

    #endregion

    #region ExecutorRunArgs

    /// <summary>
    /// Represents the <see cref="Executor"/> <see cref="ExecutorRunType.Run"/> arguments.
    /// </summary>
    public sealed class ExecutorRunArgs : ExecutorArgsBase, IExecutorArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutorArgsBase"/> class.
        /// </summary>
        /// <param name="executor">The <see cref="Executor"/>.</param>
        internal ExecutorRunArgs(Executor executor) : base(executor, ExecutorRunType.Run) { }

        /// <summary>
        /// Gets the item index (only applicable when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.ItemRun"/>).
        /// </summary>
        long? IExecutorArgs.ItemIndex => null;

        /// <summary>
        /// Gets the item data (only applicable when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.ItemRun"/>).
        /// </summary>
        object IExecutorArgs.ItemData => null;

        /// <summary>
        /// Indicates when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.CompletionRun"/> that all the items were completed (i.e. not stopped).
        /// </summary>
        bool? IExecutorArgs.IsCollCompleted => null;
    }

    /// <summary>
    /// Represents the <see cref="Executor"/> <see cref="ExecutorRunType.Run"/> arguments with corresponding arguments (see <see cref="Value"/>).
    /// </summary>
    /// <typeparam name="TArgs">The arguments <see cref="Type"/>.</typeparam>
    public sealed class ExecutorRunArgs<TArgs> : ExecutorArgsBase, IExecutorArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutorArgsBase"/> class.
        /// </summary>
        /// <param name="executor">The <see cref="Executor"/>.</param>
        internal ExecutorRunArgs(Executor executor) : base(executor, ExecutorRunType.Run) { }

        /// <summary>
        /// Gets the <see cref="Executor"/> arguments value.
        /// </summary>
        public new TArgs Value => (TArgs)((IExecutorArgs)this).Value;

        /// <summary>
        /// Gets the item index (only applicable when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.ItemRun"/>).
        /// </summary>
        long? IExecutorArgs.ItemIndex => null;

        /// <summary>
        /// Gets the item data (only applicable when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.ItemRun"/>).
        /// </summary>
        object IExecutorArgs.ItemData => null;

        /// <summary>
        /// Indicates when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.CompletionRun"/> that all the items were completed (i.e. not stopped).
        /// </summary>
        bool? IExecutorArgs.IsCollCompleted => null;
    }

    #endregion

    #region ExecutorCollectionRunArgs

    /// <summary>
    /// Represents the <see cref="Executor"/> <see cref="ExecutorRunType.CollectionRun"/> arguments.
    /// </summary>
    public sealed class ExecutorCollectionRunArgs : ExecutorArgsBase, IExecutorArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutorArgsBase"/> class.
        /// </summary>
        /// <param name="executor">The <see cref="Executor"/>.</param>
        internal ExecutorCollectionRunArgs(Executor executor) : base(executor, ExecutorRunType.CollectionRun) { }

        /// <summary>
        /// Gets the item index (only applicable when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.ItemRun"/>).
        /// </summary>
        long? IExecutorArgs.ItemIndex => null;

        /// <summary>
        /// Gets the item data (only applicable when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.ItemRun"/>).
        /// </summary>
        object IExecutorArgs.ItemData => null;

        /// <summary>
        /// Indicates when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.CompletionRun"/> that all the items were completed (i.e. not stopped).
        /// </summary>
        bool? IExecutorArgs.IsCollCompleted => null;
    }

    /// <summary>
    /// Represents the <see cref="Executor"/> <see cref="ExecutorRunType.CollectionRun"/> arguments with corresponding arguments (see <see cref="Value"/>).
    /// </summary>
    /// <typeparam name="TArgs">The arguments <see cref="Type"/>.</typeparam>
    public sealed class ExecutorCollectionRunArgs<TArgs> : ExecutorArgsBase, IExecutorArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutorArgsBase"/> class.
        /// </summary>
        /// <param name="executor">The <see cref="Executor"/>.</param>
        internal ExecutorCollectionRunArgs(Executor executor) : base(executor, ExecutorRunType.CollectionRun) { }

        /// <summary>
        /// Gets the <see cref="Executor"/> arguments value.
        /// </summary>
        public new TArgs Value => (TArgs)((IExecutorArgs)this).Value;

        /// <summary>
        /// Gets the item index (only applicable when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.ItemRun"/>).
        /// </summary>
        long? IExecutorArgs.ItemIndex => null;

        /// <summary>
        /// Gets the item data (only applicable when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.ItemRun"/>).
        /// </summary>
        object IExecutorArgs.ItemData => null;

        /// <summary>
        /// Indicates when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.CompletionRun"/> that all the items were completed (i.e. not stopped).
        /// </summary>
        bool? IExecutorArgs.IsCollCompleted => null;
    }

    #endregion

    #region ExecutorItemRunArgs

    /// <summary>
    /// Represents the <see cref="Executor"/> <see cref="ExecutorRunType.ItemRun"/> arguments.
    /// </summary>
    /// <typeparam name="TItem">The arguments <see cref="Type"/>.</typeparam>
    public sealed class ExecutorItemRunArgs<TItem> : ExecutorArgsBase, IExecutorArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutorArgsBase"/> class.
        /// </summary>
        /// <param name="executor">The <see cref="Executor"/>.</param>
        /// <param name="index">The item index.</param>
        /// <param name="item">The item data.</param>
        internal ExecutorItemRunArgs(Executor executor, long index, TItem item) : base(executor, ExecutorRunType.ItemRun)
        {
            Index = index;
            Item = item;
        }

        /// <summary>
        /// Gets the item index.
        /// </summary>
        public long Index { get; private set; }

        /// <summary>
        /// Gets the item.
        /// </summary>
        public TItem Item { get; private set; }

        /// <summary>
        /// Gets the item index (only applicable when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.ItemRun"/>).
        /// </summary>
        long? IExecutorArgs.ItemIndex => Index;

        /// <summary>
        /// Gets the item data (only applicable when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.ItemRun"/>).
        /// </summary>
        object IExecutorArgs.ItemData => Item;

        /// <summary>
        /// Indicates when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.CompletionRun"/> that all the items were completed (i.e. not stopped).
        /// </summary>
        bool? IExecutorArgs.IsCollCompleted => null;
    }

    #endregion

    #region ExecutorCompletionRunArgs

    /// <summary>
    /// Represents the <see cref="Executor"/> <see cref="ExecutorRunType.CompletionRun"/> arguments.
    /// </summary>
    public sealed class ExecutorCompletionRunArgs : ExecutorArgsBase, IExecutorArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutorArgsBase"/> class.
        /// </summary>
        /// <param name="executor">The <see cref="Executor"/>.</param>
        /// <param name="isCompleted">Indicates whether all the items were completed (i.e. not stopped).</param>
        internal ExecutorCompletionRunArgs(Executor executor, bool isCompleted) : base(executor, ExecutorRunType.CompletionRun)
        {
            IsCompleted = isCompleted;
        }

        /// <summary>
        /// Indicates when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.CompletionRun"/> that all the items were completed (i.e. not stopped).
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// Gets the item index (only applicable when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.ItemRun"/>).
        /// </summary>
        long? IExecutorArgs.ItemIndex => null;

        /// <summary>
        /// Gets the item data (only applicable when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.ItemRun"/>).
        /// </summary>
        object IExecutorArgs.ItemData => null;

        /// <summary>
        /// Indicates when the <see cref="ExecutorRunType"/> is <see cref="ExecutorRunType.CompletionRun"/> that all the items were completed (i.e. not stopped).
        /// </summary>
        bool? IExecutorArgs.IsCollCompleted => IsCompleted;
    }

    #endregion
}
