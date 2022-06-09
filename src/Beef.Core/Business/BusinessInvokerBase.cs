// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Beef.Business
{
    /// <summary>
    /// Adds capabilities (wraps) an <see cref="InvokerBase{TParam}"/> enabling standard functionality to be added to all <b> business tier</b> invocations using
    /// a <see cref="BusinessInvokerArgs"/> to configure the <see cref="TransactionScope"/> and <see cref="BusinessInvokerArgs.ExceptionHandler">exception handling</see>. 
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public abstract class BusinessInvokerBase : CoreEx.Abstractions.InvokerBase<object, BusinessInvokerArgs>
    {
        /// <inheritdoc/>
        protected async override Task<TResult> OnInvokeAsync<TResult>(object owner, Func<CancellationToken, Task<TResult>> func, BusinessInvokerArgs? param, CancellationToken cancellationToken)
        {
            BusinessInvokerArgs bia = param ?? BusinessInvokerArgs.Default;
            TransactionScope? txn = null;
            OperationType ot = ExecutionContext.Current.OperationType;
            if (bia.OperationType.HasValue)
                ExecutionContext.Current.OperationType = bia.OperationType.Value;

            try
            {
                if (bia.IncludeTransactionScope)
                    txn = new TransactionScope(bia.TransactionScopeOption, TransactionScopeAsyncFlowOption.Enabled);

                var result = await func(cancellationToken).ConfigureAwait(false);
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
    }

    /// <summary>
    /// Provides arguments for the <see cref="BusinessInvokerBase"/>.
    /// </summary>
    public class BusinessInvokerArgs
    {
        /// <summary>
        /// Gets or sets the <i>default</i> <see cref="BusinessInvokerArgs"/> where <see cref="IncludeTransactionScope"/> is <c>false</c> and <see cref="OperationType"/> is <c>null</c>.
        /// </summary>
        public static BusinessInvokerArgs Default { get; set; } = new BusinessInvokerArgs();

        /// <summary>
        /// Gets the <see cref="BusinessInvokerArgs"/> where <see cref="OperationType"/> is <see cref="Beef.OperationType.Read"/> and <see cref="IncludeTransactionScope"/> is <c>false</c>.
        /// </summary>
        public static BusinessInvokerArgs Read { get; } = new BusinessInvokerArgs { OperationType = Beef.OperationType.Read };

        /// <summary>
        /// Gets the <see cref="BusinessInvokerArgs"/> where <see cref="OperationType"/> is <see cref="Beef.OperationType.Create"/> and <see cref="IncludeTransactionScope"/> is <c>false</c>.
        /// </summary>
        public static BusinessInvokerArgs Create { get; } = new BusinessInvokerArgs { OperationType = Beef.OperationType.Create };

        /// <summary>
        /// Gets the <see cref="BusinessInvokerArgs"/> where <see cref="OperationType"/> is <see cref="Beef.OperationType.Update"/> and <see cref="IncludeTransactionScope"/> is <c>false</c>.
        /// </summary>
        public static BusinessInvokerArgs Update { get; } = new BusinessInvokerArgs { OperationType = Beef.OperationType.Update };

        /// <summary>
        /// Gets the <see cref="BusinessInvokerArgs"/> where <see cref="OperationType"/> is <see cref="Beef.OperationType.Delete"/> and <see cref="IncludeTransactionScope"/> is <c>false</c>.
        /// </summary>
        public static BusinessInvokerArgs Delete { get; } = new BusinessInvokerArgs { OperationType = Beef.OperationType.Delete };

        /// <summary>
        /// Gets the <see cref="BusinessInvokerArgs"/> where <see cref="OperationType"/> is <see cref="Beef.OperationType.Unspecified"/> and <see cref="IncludeTransactionScope"/> is <c>false</c>.
        /// </summary>
        public static BusinessInvokerArgs Unspecified { get; } = new BusinessInvokerArgs { OperationType = Beef.OperationType.Unspecified };

        /// <summary>
        /// Gets the <see cref="BusinessInvokerArgs"/> where <see cref="IncludeTransactionScope"/> is <c>true</c> 
        /// and <see cref="TransactionScopeOption"/> is <see cref="TransactionScopeOption.Suppress"/>.
        /// </summary>
        public static BusinessInvokerArgs TransactionSuppress { get; } = new BusinessInvokerArgs { IncludeTransactionScope = true, TransactionScopeOption = TransactionScopeOption.Suppress };

        /// <summary>
        /// Gets the <see cref="BusinessInvokerArgs"/> where <see cref="IncludeTransactionScope"/> is <c>true</c> 
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
        /// Gets or sets the <see cref="Beef.OperationType"/> to override the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.OperationType"/>.
        /// </summary>
        public Beef.OperationType? OperationType { get; set; }

        /// <summary>
        /// Gets or sets the unhandled <see cref="Exception"/> handler.
        /// </summary>
        public Action<Exception>? ExceptionHandler { get; set; }
    }
}