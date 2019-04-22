// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Executors
{
    /// <summary>
    /// Represents the <see cref="Executor"/> <b>Run</b> type.
    /// </summary>
    public enum ExecutorRunType
    {
        /// <summary>
        /// Indicates a <see cref="ExecutorBase.OnRunAsync"/> or <see cref="ExecutorBase{TArgs}.OnRunAsync(ExecutorRunArgs{TArgs})"/> execution type.
        /// </summary>
        Run,

        /// <summary>
        /// Indicates a <see cref="CollectionExecutorBase{TColl, TItem}.OnRunCollectionAsync"/> or <see cref="CollectionExecutorBase{TColl, TItem, TArgs}.OnRunCollectionAsync(ExecutorCollectionRunArgs{TArgs})"/> execution type.
        /// </summary>
        CollectionRun,

        /// <summary>
        /// Indicates the collection iteration processing (see post <see cref="CollectionRun"/>). Only used when an unexpected <see cref="Exception"/> occurs during the iteration processing.
        /// </summary>
        CollectionIterate,

        /// <summary>
        /// Indicates a <see cref="CollectionExecutorCore{TColl, TItem}.OnItemRunAsync(ExecutorItemRunArgs{TItem})"/> execution type (per collection iteration).
        /// </summary>
        ItemRun,

        /// <summary>
        /// Indicates a <see cref="CollectionExecutorCore{TColl, TItem}.OnCompletionRunAsync(ExecutorCompletionRunArgs)"/> execution type.
        /// </summary>
        CompletionRun
    }
}
