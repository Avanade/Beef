// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace Beef.Data.OData
{
    /// <summary>
    /// Wraps a <see cref="ODataBase"/> request enabling standard functionality to be added to all invocations. 
    /// </summary>
    public class ODataInvoker : InvokerBase<ODataInvoker, ODataBase>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataInvoker"/> class.
        /// </summary>
        public ODataInvoker()
        {
            SetBaseWrapper((s) => WrappedInvoke(s), (a) => WrappedInvokeAsync(a));
        }

        /// <summary>
        /// Wrap the invoke synchronously.
        /// </summary>
        private void WrappedInvoke(InvokerArgsSync<ODataBase> args)
        {
            try
            {
                args.WorkCallbackSync();
            }
            catch (HttpRequestException hrex)
            {
                if (args.Param != null)
                    args.Param.ExceptionHandler?.Invoke(hrex);

                throw;
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex?.InnerException is HttpRequestException hrex)
                {
                    if (args.Param != null)
                        args.Param.ExceptionHandler?.Invoke(hrex);
                }

                throw;
            }
        }

        /// <summary>
        /// Wrap the invoke asynchronously.
        /// </summary>
        private async Task WrappedInvokeAsync(InvokerArgsAsync<ODataBase> args)
        {
            try
            {
                await args.WorkCallbackAsync();
            }
            catch (HttpRequestException hrex)
            {
                if (args.Param != null)
                    args.Param.ExceptionHandler?.Invoke(hrex);

                throw;
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex?.InnerException is HttpRequestException hrex)
                {
                    if (args.Param != null)
                        args.Param.ExceptionHandler?.Invoke(hrex);
                }

                throw;
            }
        }
    }

    /// <summary>
    /// Wraps a <see cref="ODataBase"/> request enabling standard functionality to be added to all invocations. 
    /// </summary>
    public class ODataInvoker<TResult> : InvokerBase<ODataInvoker<TResult>, ODataBase, TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataInvoker{TResult}"/> class.
        /// </summary>
        public ODataInvoker()
        {
            SetBaseWrapper((s) => WrappedInvoke(s), (a) => WrappedInvokeAsync(a));
        }

        /// <summary>
        /// Wrap the invoke synchronously with result.
        /// </summary>
        private TResult WrappedInvoke(InvokerArgsSync<ODataBase, TResult> args)
        {
            try
            {
                return args.WorkCallbackSync();
            }
            catch (HttpRequestException hrex)
            {
                if (args.Param != null)
                    args.Param.ExceptionHandler?.Invoke(hrex);

                throw;
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex?.InnerException is HttpRequestException hrex)
                {
                    if (args.Param != null)
                        args.Param.ExceptionHandler?.Invoke(hrex);
                }

                throw;
            }
        }

        /// <summary>
        /// Wrap the invoke asynchronously with result.
        /// </summary>
        private async Task<TResult> WrappedInvokeAsync(InvokerArgsAsync<ODataBase, TResult> args)
        {
            try
            {
                return await args.WorkCallbackAsync();
            }
            catch (HttpRequestException hrex)
            {
                if (args.Param != null)
                    args.Param.ExceptionHandler?.Invoke(hrex);

                throw;
            }
            catch (TargetInvocationException tiex)
            {
                if (tiex?.InnerException is HttpRequestException hrex)
                {
                    if (args.Param != null)
                        args.Param.ExceptionHandler?.Invoke(hrex);
                }

                throw;
            }
        }
    }
}
