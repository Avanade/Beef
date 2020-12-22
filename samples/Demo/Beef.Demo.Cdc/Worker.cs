using Beef.Data.Database.Cdc;
using Beef.Demo.Cdc.Data;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Demo.Cdc
{
    public class Worker : BackgroundService
    {
        private readonly PostsCdcData _data;
        private readonly IServiceProvider _sp;

        public Worker(PostsCdcData data, IServiceProvider sp)
        {
            _data = data;
            _sp = sp;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            ExecutionContext.Reset();
            ExecutionContext.SetCurrent(new ExecutionContext { ServiceProvider = _sp });

            CdcExecutorResult cer;

            do
            {
                cer = await _data.ExecuteNextAsync(100, cancellationToken).ConfigureAwait(false);
            } while (cer.CanContinue && !cancellationToken.IsCancellationRequested);
        }
    }
}