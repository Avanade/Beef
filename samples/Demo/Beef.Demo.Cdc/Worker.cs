using Beef.Data.Database.Cdc;
using Beef.Demo.Cdc.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Beef.Demo.Cdc
{
    public class PostsCdcBackgroundService : CdcBackgroundService<PostsCdcData>
    {
        private readonly IConfiguration _config;

        public PostsCdcBackgroundService(PostsCdcData data, IConfiguration config, IServiceProvider serviceProvider, ILogger<PostsCdcBackgroundService> logger) : base(data, serviceProvider, logger) => _config = config;

        public override string ServiceName => nameof(PostsCdcBackgroundService);

        public override int IntervalSeconds => _config.GetValue<int>("PostsCdcIntervalSeconds");
    }
}

#nullable restore