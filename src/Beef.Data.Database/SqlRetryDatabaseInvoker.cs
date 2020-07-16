// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Microsoft.Data.SqlClient;
using Polly;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Beef.Data.Database
{
    /// <summary>
    /// A <see cref="DatabaseInvoker"/> for <b>Microsoft SQL Server</b> that uses <b>Polly</b> to perform an exponential retry where a transient error (see <see cref="TransientErrorNumbers"/>) occurs.
    /// </summary>
    /// <remarks>
    /// A retry is attempted using a 250ms backoff strategy (e.g. 0ms, 250ms, 500ms, 750ms, 1000ms) with a 100ms jitter to add a further level of randomness. The 
    /// <see cref="MaxRetries"/> defines how many retries are performed before failing and allowing the error to bubble up and out.
    /// </remarks>
    public class SqlRetryDatabaseInvoker : DatabaseInvoker
    {
        private static readonly Random _jitterer = new Random();

        /// <summary>
        /// Occurs where a retry will occur as a result of a transient (see <see cref="DatabaseBase.SqlTransientErrorNumbers"/>) error.
        /// </summary>
        public static event EventHandler<SqlRetryDatabaseInvokerEventArgs>? ExceptionRetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlRetryDatabaseInvoker"/> class.
        /// </summary>
        /// <param name="maxRetries">The maximum number of retries (defaults to 4).</param>
        /// <param name="transientErrorNumbers">The list of of known <see cref="SqlException.Number"/> values that are considered transient (defaults to <see cref="DatabaseBase.SqlTransientErrorNumbers"/> where not specified).</param>
        public SqlRetryDatabaseInvoker(int maxRetries = 4, IEnumerable<int>? transientErrorNumbers = null)
        {
            if (maxRetries < 0)
                throw new ArgumentException("Maximum retries can not be negative.", nameof(maxRetries));

            MaxRetries = maxRetries;
            TransientErrorNumbers.AddRange(transientErrorNumbers ?? DatabaseBase.SqlTransientErrorNumbers);
        }

        /// <summary>
        /// Gets the maximum number of retries (defaults to 4).
        /// </summary>
        public int MaxRetries { get; private set; }

        /// <summary>
        /// Gets the list of known <see cref="SqlException.Number"/> values that are considered transient; candidates for a retry.
        /// </summary>
        public List<int> TransientErrorNumbers { get; private set; } = new List<int>();

        /// <summary>
        /// Creates a synchronous policy.
        /// </summary>
        private Policy CreateSyncPolicy() => Policy.Handle<SqlException>(sex => TransientErrorNumbers.Contains(sex.Number)).WaitAndRetry(MaxRetries, CalcRetryTimeSpan, LogRetryException);

        /// <summary>
        /// Creates an asynchronous policy.
        /// </summary>
        private AsyncPolicy CreateAsyncPolicy() => Policy.Handle<SqlException>(sex => TransientErrorNumbers.Contains(sex.Number)).WaitAndRetryAsync(MaxRetries, CalcRetryTimeSpan, (ex, ts) => LogRetryException(ex, ts));

        /// <summary>
        /// Calculates the retry timespan.
        /// </summary>
        private static TimeSpan CalcRetryTimeSpan(int count) => TimeSpan.FromMilliseconds((count * 1000) / 4) + TimeSpan.FromMilliseconds(_jitterer.Next(0, 100));

        /// <summary>
        /// Logs the retry exception as a warning.
        /// </summary>
        private void LogRetryException(Exception ex, TimeSpan ts)
        {
            Logger.Default.Warning($"Transient SQL Server Error '{((SqlException)ex).Number}' encountered; will retry in {ts.TotalMilliseconds}ms: {ex.Message}");
            ExceptionRetry?.Invoke(this, new SqlRetryDatabaseInvokerEventArgs((SqlException)ex));
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
        protected override void WrapInvoke(object caller, Action action, DatabaseBase? param = null, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            CreateSyncPolicy().Execute(() => base.WrapInvoke(caller, action, param, memberName, filePath, lineNumber));
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
        protected override Task WrapInvokeAsync(object caller, Func<Task> func, DatabaseBase? param = null, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return CreateAsyncPolicy().ExecuteAsync(() => base.WrapInvokeAsync(caller, func, param, memberName, filePath, lineNumber));
        }

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
        protected override TResult WrapInvoke<TResult>(object caller, Func<TResult> func, DatabaseBase? param = null, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return CreateSyncPolicy().Execute(() => base.WrapInvoke(caller, func, param, memberName, filePath, lineNumber));
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
        protected override Task<TResult> WrapInvokeAsync<TResult>(object caller, Func<Task<TResult>> func, DatabaseBase? param = null, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            return CreateAsyncPolicy().ExecuteAsync(() => base.WrapInvokeAsync(caller, func, param, memberName, filePath, lineNumber));
        }
    }

    /// <summary>
    /// Represents the <see cref="SqlRetryDatabaseInvoker"/> retry <see cref="System.Exception"/> <see cref="EventArgs"/>.
    /// </summary>
    public class SqlRetryDatabaseInvokerEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlRetryDatabaseInvokerEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The corresponding <see cref="SqlException"/>.</param>
        public SqlRetryDatabaseInvokerEventArgs(SqlException exception)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }

        /// <summary>
        /// Gets or sets the <see cref="SqlException"/>.
        /// </summary>
        public SqlException Exception { get; set; }
    }
}