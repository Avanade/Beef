﻿using Beef.Demo.Cdc.Data.Model;
using Beef.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Demo.Cdc.Data
{
    public partial class PostsCdcData
    {
        protected override Task<IEnumerable<EventData>> CreateEventDataAsync(List<PostsCdc> coll, CancellationToken? cancellationToken = null)
        {
            return Task.FromResult<IEnumerable<EventData>>(new List<EventData>());
        }
    }
}