// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.Executors
{
    /// <summary>
    /// Represents the base class for an <b>executor</b> that takes arguments.
    /// </summary>
    /// <typeparam name="TArgs">The arguments <see cref="Type"/>.</typeparam>
    public abstract class ExecutorBase<TArgs> : Executor
    {
        /// <summary>
        /// Runs the work.
        /// </summary>
        /// <param name="args">The <see cref="ExecutorRunArgs{TArgs}"/>.</param>
        internal async Task RunAsync(ExecutorRunArgs<TArgs> args)
        {
            try
            {
                await OnRunAsync(args);
                OnPerRunType(args);
            }
            catch (Exception ex)
            {
                if (!args.HasException)
                {
                    args.SetException(ex);
                    OnPerRunType(args);
                }
                throw;
            }
        }

        /// <summary>
        /// Runs the unit of work.
        /// </summary>
        /// <param name="args">The <see cref="ExecutorRunArgs{TArgs}"/>.</param>
        protected abstract Task OnRunAsync(ExecutorRunArgs<TArgs> args);
    }

    /// <summary>
    /// Represents an <b>executor</b> that executes a function directly that takes arguments.
    /// </summary>
    /// <typeparam name="TArgs">The arguments <see cref="Type"/>.</typeparam>
    public sealed class ExecutorFunc<TArgs> : ExecutorBase<TArgs>
    {
        private readonly Func<ExecutorRunArgs<TArgs>, Task> _func;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutorFunc"/> class.
        /// </summary>
        /// <param name="func"></param>
        public ExecutorFunc(Func<ExecutorRunArgs<TArgs>, Task> func)
        {
            _func = Check.NotNull(func, nameof(func));
        }

        /// <summary>
        /// Runs the unit of work.
        /// </summary>
        /// <param name="args">The <see cref="ExecutorRunArgs"/>.</param>
        protected override Task OnRunAsync(ExecutorRunArgs<TArgs> args)
        {
            return _func(args);
        }
    }
}
