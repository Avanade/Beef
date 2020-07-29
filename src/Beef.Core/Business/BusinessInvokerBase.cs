// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Transactions;

namespace Beef.Business
{
    /// <summary>
    /// Adds capabilities (wraps) an <see cref="InvokerBase{TParam}"/> enabling standard functionality to be added to all <b> business tier</b> invocations using
    /// a <see cref="BusinessInvokerArgs"/> to configure the <see cref="TransactionScope"/> and <see cref="BusinessInvokerArgs.ExceptionHandler">exception handling</see>. 
    /// </summary>
    public abstract class BusinessInvokerBase : InvokerBase<BusinessInvokerArgs>
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
        protected override void WrapInvoke(object caller, Action action, BusinessInvokerArgs? param = null, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            Check.NotNull(action, nameof(action));

            BusinessInvokerArgs bia = param ?? BusinessInvokerArgs.Default;
            TransactionScope? txn = null;
            OperationType ot = ExecutionContext.Current.OperationType;

            try
            {
                if (bia.IncludeTransactionScope)
                    txn = new TransactionScope(bia.TransactionScopeOption, TransactionScopeAsyncFlowOption.Enabled);

                action();
                txn?.Complete();
            }
            catch (Exception ex)
            {
                bia.ExceptionHandler?.Invoke(ex);
                throw;
            }
            finally
            {
                txn?.Dispose();
                ExecutionContext.Current.OperationType = ot;
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
        protected async override Task WrapInvokeAsync(object caller, Func<Task> func, BusinessInvokerArgs? param = null, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            Check.NotNull(func, nameof(func));

            BusinessInvokerArgs bia = param ?? BusinessInvokerArgs.Default;
            TransactionScope? txn = null;
            OperationType ot = ExecutionContext.Current.OperationType;

            try
            {
                if (bia.IncludeTransactionScope)
                    txn = new TransactionScope(bia.TransactionScopeOption, TransactionScopeAsyncFlowOption.Enabled);

                await func().ConfigureAwait(false);
                txn?.Complete();
            }
            catch (Exception ex)
            {
                bia.ExceptionHandler?.Invoke(ex);
                throw;
            }
            finally
            {
                txn?.Dispose();
                ExecutionContext.Current.OperationType = ot;
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
        protected override TResult WrapInvoke<TResult>(object caller, Func<TResult> func, BusinessInvokerArgs? param = null, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            Check.NotNull(func, nameof(func));

            BusinessInvokerArgs bia = param ?? BusinessInvokerArgs.Default;
            TransactionScope? txn = null;
            OperationType ot = ExecutionContext.Current.OperationType;

            try
            {
                if (bia.IncludeTransactionScope)
                    txn = new TransactionScope(bia.TransactionScopeOption, TransactionScopeAsyncFlowOption.Enabled);

                var result = func();
                txn?.Complete();
                return result;
            }
            catch (Exception ex)
            {
                bia.ExceptionHandler?.Invoke(ex);
                throw;
            }
            finally
            {
                txn?.Dispose();
                ExecutionContext.Current.OperationType = ot;
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
        protected async override Task<TResult> WrapInvokeAsync<TResult>(object caller, Func<Task<TResult>> func, BusinessInvokerArgs? param = null, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            Check.NotNull(func, nameof(func));

            BusinessInvokerArgs bia = Check.NotNull(param ?? BusinessInvokerArgs.Default, nameof(param));
            TransactionScope? txn = null;
            OperationType ot = ExecutionContext.Current.OperationType;

            try
            {
                if (bia.IncludeTransactionScope)
                    txn = new TransactionScope(bia.TransactionScopeOption, TransactionScopeAsyncFlowOption.Enabled);

                var result = await func().ConfigureAwait(false);
                txn?.Complete();
                return result;
            }
            catch (Exception ex)
            {
                bia.ExceptionHandler?.Invoke(ex);
                throw;
            }
            finally
            {
                txn?.Dispose();
                ExecutionContext.Current.OperationType = ot;
            }
        }

        #endregion
    }

    /// <summary>
    /// Provides arguments for the <see cref="BusinessInvokerBase"/>.
    /// </summary>
    public class BusinessInvokerArgs
    {
        /// <summary>
        /// Gets or sets the default <see cref="BusinessInvokerArgs"/> where <see cref="IncludeTransactionScope"/> is <c>false</c>.
        /// </summary>
        public static BusinessInvokerArgs Default { get; set; } = new BusinessInvokerArgs();

        /// <summary>
        /// Gets the default <see cref="BusinessInvokerArgs"/> where <see cref="IncludeTransactionScope"/> is <c>true</c> 
        /// and <see cref="TransactionScopeOption"/> is <see cref="TransactionScopeOption.Suppress"/>.
        /// </summary>
        public static BusinessInvokerArgs TransactionSuppress { get; } = new BusinessInvokerArgs { IncludeTransactionScope = true, TransactionScopeOption = TransactionScopeOption.Suppress };

        /// <summary>
        /// Gets the default <see cref="BusinessInvokerArgs"/> where <see cref="IncludeTransactionScope"/> is <c>true</c> 
        /// and <see cref="TransactionScopeOption"/> is <see cref="TransactionScopeOption.RequiresNew"/>.
        /// </summary>
        public static BusinessInvokerArgs TransactionRequiresNew { get; } = new BusinessInvokerArgs { IncludeTransactionScope = true, TransactionScopeOption = TransactionScopeOption.RequiresNew };

        /// <summary>
        /// Indicates whether to wrap the invocation with a <see cref="TransactionScope"/> (see <see cref="TransactionScopeOption"/>). Defaults to <c>false</c>.
        /// </summary>
        public bool IncludeTransactionScope { get; set; } = false;

        /// <summary>
        /// Gets or sets the <see cref="System.Transactions.TransactionScopeOption"/> (see <see cref="IncludeTransactionScope"/>). Defaults to <see cref="TransactionScopeOption.Required"/>.
        /// </summary>
        public TransactionScopeOption TransactionScopeOption { get; set; } = TransactionScopeOption.Required;

        /// <summary>
        /// Gets or sets the unhandled <see cref="Exception"/> handler.
        /// </summary>
        public Action<Exception>? ExceptionHandler { get; set; }
    }
}