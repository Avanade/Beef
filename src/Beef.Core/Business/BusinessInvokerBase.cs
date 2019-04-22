// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Transactions;

namespace Beef.Business
{
    /// <summary>
    /// Wraps an <see cref="InvokerBase{TInvoker, TParam, TResult}"/> enabling standard functionality to be added to all invocations specifically for the <b>business tier</b> that supports
    /// a <see cref="BusinessInvokerArgs"/> to enable <see cref="DataContextScopeOption"/> and <see cref="TransactionScope"/>. 
    /// </summary>
    [DebuggerStepThrough()]
    public abstract class BusinessInvokerBase<TInvoker> : InvokerBase<TInvoker, BusinessInvokerArgs> where TInvoker : BusinessInvokerBase<TInvoker>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessInvokerBase{T, TResult}"/> class.
        /// </summary>
        /// <remarks>Warning: Inheritors should note that this class uses the 
        /// <see cref="InvokerBase{TInvoker, TParam}.SetBaseWrapper(System.Action{InvokerArgsSync{TParam}}, System.Func{InvokerArgsAsync{TParam}, Task})"/>
        /// to enable this capability and overridding in a sub-class will mean that the underlying functionality will not be executed.</remarks>
        public BusinessInvokerBase()
        {
            SetBaseWrapper((s) => WrappedInvoke(s), (a) => WrappedInvokeAsync(a));
        }

        /// <summary>
        /// Wrap the invoke synchronously.
        /// </summary>
        private void WrappedInvoke(InvokerArgsSync<BusinessInvokerArgs> args)
        {
            BusinessInvokerArgs bia = args.Param ?? BusinessInvokerArgs.Default;
            TransactionScope txn = null;
            DataContextScope ctx = null;
            OperationType ot = ExecutionContext.Current.OperationType;

            try
            {
                if (bia.IncludeTransactionScope)
                    txn = new TransactionScope(bia.TransactionScopeOption);

                ctx = DataContextScope.Begin(bia.DataContextScopeOption);

                args.WorkCallbackSync();

                if (txn != null)
                    txn.Complete();
            }
            catch (Exception ex)
            {
                bia.ExceptionHandler?.Invoke(ex);
                throw;
            }
            finally
            {
                if (ctx != null)
                    ctx.Dispose();

                if (txn != null)
                    txn.Dispose();

                ExecutionContext.Current.OperationType = ot;
            }
        }

        /// <summary>
        /// Wrap the invoke asynchronously.
        /// </summary>
        private async Task WrappedInvokeAsync(InvokerArgsAsync<BusinessInvokerArgs> args)
        {
            BusinessInvokerArgs bia = args.Param ?? BusinessInvokerArgs.Default;
            TransactionScope txn = null;
            DataContextScope ctx = null;
            OperationType ot = ExecutionContext.Current.OperationType;

            try
            {
                if (bia.IncludeTransactionScope)
                    txn = new TransactionScope(bia.TransactionScopeOption);

                ctx = DataContextScope.Begin(bia.DataContextScopeOption);

                await args.WorkCallbackAsync();

                if (txn != null)
                    txn.Complete();
            }
            catch (Exception ex)
            {
                bia.ExceptionHandler?.Invoke(ex);
                throw;
            }
            finally
            {
                if (ctx != null)
                    ctx.Dispose();

                if (txn != null)
                    txn.Dispose();

                ExecutionContext.Current.OperationType = ot;
            }
        }
    }

    /// <summary>
    /// Wraps an <see cref="InvokerBase{TInvoker, TParam, TResult}"/> enabling standard functionality to be added to all invocations specifically for the <b>business tier</b> that supports
    /// a <see cref="BusinessInvokerArgs"/> to enable <see cref="DataContextScopeOption"/> and <see cref="TransactionScope"/>. 
    /// </summary>
    [DebuggerStepThrough()]
    public abstract class BusinessInvokerBase<TInvoker, TResult> : InvokerBase<TInvoker, BusinessInvokerArgs, TResult> where TInvoker : BusinessInvokerBase<TInvoker, TResult>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessInvokerBase{T, TResult}"/> class.
        /// </summary>
        /// <remarks>Warning: Inheritors should note that this class uses the 
        /// <see cref="InvokerBase{TInvoker, TParam, TResult}.SetBaseWrapper(System.Func{InvokerArgsSync{TParam, TResult}, TResult}, System.Func{InvokerArgsAsync{TParam, TResult}, Task{TResult}})"/>
        /// to enable this capability and overridding in a sub-class will mean that the underlying functionality will not be executed.</remarks>
        public BusinessInvokerBase()
        {
            SetBaseWrapper((s) => WrappedInvoke(s), (a) => WrappedInvokeAsync(a));
        }

        #region WrappedInvoke

        /// <summary>
        /// Wrap the invoke synchronously with result.
        /// </summary>
        private TResult WrappedInvoke(InvokerArgsSync<BusinessInvokerArgs, TResult> args)
        {
            BusinessInvokerArgs bia = args.Param ?? BusinessInvokerArgs.Default;
            TransactionScope txn = null;
            DataContextScope ctx = null;
            OperationType ot = ExecutionContext.Current.OperationType;

            try
            {
                if (bia.IncludeTransactionScope)
                    txn = new TransactionScope(bia.TransactionScopeOption);

                ctx = DataContextScope.Begin(bia.DataContextScopeOption);

                var r = args.WorkCallbackSync();

                if (txn != null)
                    txn.Complete();

                return r;
            }
            catch (Exception ex)
            {
                bia.ExceptionHandler?.Invoke(ex);
                throw;
            }
            finally
            {
                if (ctx != null)
                    ctx.Dispose();

                if (txn != null)
                    txn.Dispose();

                ExecutionContext.Current.OperationType = ot;
            }
        }

        /// <summary>
        /// Wrap the invoke asynchronously with result.
        /// </summary>
        private async Task<TResult> WrappedInvokeAsync(InvokerArgsAsync<BusinessInvokerArgs, TResult> args)
        {
            BusinessInvokerArgs bia = args.Param ?? BusinessInvokerArgs.Default;
            TransactionScope txn = null;
            DataContextScope ctx = null;
            OperationType ot = ExecutionContext.Current.OperationType;

            try
            {
                if (bia.IncludeTransactionScope)
                    txn = new TransactionScope(bia.TransactionScopeOption, TransactionScopeAsyncFlowOption.Enabled);

                ctx = DataContextScope.Begin(bia.DataContextScopeOption);

                var r = await args.WorkCallbackAsync();

                if (txn != null)
                    txn.Complete();

                return r;
            }
            catch (Exception ex)
            {
                bia.ExceptionHandler?.Invoke(ex);
                throw;
            }
            finally
            {
                if (ctx != null)
                    ctx.Dispose();

                if (txn != null)
                    txn.Dispose();

                ExecutionContext.Current.OperationType = ot;
            }
        }

        #endregion
    }

    /// <summary>
    /// Provides arguments for the <see cref="BusinessInvokerBase{TInvoker, TResult}"/>.
    /// </summary>
    public class BusinessInvokerArgs
    {
        /// <summary>
        /// Gets the default <see cref="BusinessInvokerArgs"/> where <see cref="DataContextScopeOption"/> is <see cref="DataContextScopeOption.UseExisting"/>, and
        /// <see cref="IncludeTransactionScope"/> is <c>false</c>.
        /// </summary>
        public static BusinessInvokerArgs Default = new BusinessInvokerArgs();

        /// <summary>
        /// Gets the default <see cref="BusinessInvokerArgs"/> where <see cref="DataContextScopeOption"/> is <see cref="DataContextScopeOption.RequiresNew"/>, 
        /// <see cref="IncludeTransactionScope"/> is <c>true</c> and <see cref="TransactionScopeOption"/> is <see cref="TransactionScopeOption.Suppress"/>.
        /// </summary>
        public static BusinessInvokerArgs RequiresNewAndTransactionSuppress = new BusinessInvokerArgs { DataContextScopeOption = DataContextScopeOption.RequiresNew, IncludeTransactionScope = true, TransactionScopeOption = TransactionScopeOption.Suppress };

        /// <summary>
        /// Gets the default <see cref="BusinessInvokerArgs"/> where <see cref="DataContextScopeOption"/> is <see cref="DataContextScopeOption.UseExisting"/>, 
        /// <see cref="IncludeTransactionScope"/> is <c>true</c> and <see cref="TransactionScopeOption"/> is <see cref="TransactionScopeOption.RequiresNew"/>.
        /// </summary>
        public static BusinessInvokerArgs UseExistingAndRequiresNew = new BusinessInvokerArgs { DataContextScopeOption = DataContextScopeOption.UseExisting, IncludeTransactionScope = true, TransactionScopeOption = TransactionScopeOption.RequiresNew };

        /// <summary>
        /// Gets or sets the <see cref="T:DataContextScopeOption"/>. Defaults to <see cref="DataContextScopeOption.UseExisting"/>.
        /// </summary>
        public DataContextScopeOption DataContextScopeOption { get; set; } = DataContextScopeOption.UseExisting;

        /// <summary>
        /// Indicates whether to wrap the invocation with a <see cref="TransactionScope"/> (see <see cref="TransactionScopeOption"/>). Defaults to <c>false</c>.
        /// </summary>
        public bool IncludeTransactionScope { get; set; } = false;

        /// <summary>
        /// Gets or sets the <see cref="T:TransactionScopeOption"/> (see <see cref="IncludeTransactionScope"/>). Defaults to <see cref="TransactionScopeOption.Required"/>.
        /// </summary>
        public TransactionScopeOption TransactionScopeOption { get; set; } = TransactionScopeOption.Required;

        /// <summary>
        /// Gets or sets the unhandled <see cref="Exception"/> handler.
        /// </summary>
        public Action<Exception> ExceptionHandler { get; set; }
    }
}
