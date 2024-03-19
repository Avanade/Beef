// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen
{
    /// <summary>
    /// Provides the ability to capture and report endpoint statistics.
    /// </summary>
    internal class EndPointStatistics()
    {
        private readonly List<EndPointData> _endPoints = [];

        /// <summary>
        /// Gets the endpoint count.
        /// </summary>
        public int Count => _endPoints.Count;

        /// <summary>
        /// Adds the <see cref="CommandType.Entity"/> endpoints.
        /// </summary>
        public void AddEntityEndPoints(Config.Entity.CodeGenConfig root)
        {
            var entities = root.Entities!.Where(x => (!x.ExcludeWebApi.HasValue || !x.ExcludeWebApi.Value) && x.Operations!.Count > 0).AsEnumerable();
            foreach (var e in entities)
            {
                foreach (var o in e.Operations!.Where(x => !x.ExcludeWebApi.HasValue || !x.ExcludeWebApi.Value))
                {
                    Add(new EndPointData
                    {
                        Route = o.AgentWebApiRoute,
                        Method = o.WebApiMethod![4..],
                        Status = o.WebApiStatus!,
                        Auth = o.WebApiAuthorize ?? e.WebApiAuthorize,
                        Tags = string.Join(", ", o.WebApiTags!.Count > 0 ? o.WebApiTags : e.WebApiTags!),
                        Name = $"{e.Name}.{o.Name}"
                    });
                }
            }
        }

        /// <summary>
        /// Adds the <see cref="CommandType.RefData"/> endpoints.
        /// </summary>
        public void AddRefDataEndPoints(Config.Entity.CodeGenConfig root)
        {
            // Add the 'GetNamed' global endpoint.
            Add(new EndPointData
            {
                Route = root.RefDataWebApiRoute,
                Method = "GET",
                Status = "OK",
                Auth = root.WebApiAuthorize,
                Tags = null,
                Name = "ReferenceData.GetNamed"
            });

            // Add the configured refdata entities.
            var entities = root.RefDataEntities!;
            foreach (var e in entities)
            {
                Add(new EndPointData
                {
                    Route = e.WebApiRoutePrefix!,
                    Method = "GET",
                    Status = "OK",
                    Auth = e.WebApiAuthorize,
                    Tags = string.Join(", ", e.WebApiTags!),
                    Name = $"ReferenceData.{e.Name}GetAll"
                });
            }
        }

        /// <summary>
        /// Add the <paramref name="data"/> to the collection.
        /// </summary>
        /// <param name="data">The <see cref="EndPointData"/>.</param>
        public void Add(EndPointData data) => _endPoints.Add(data);

        /// <summary>
        /// Write as tabulated data to the <paramref name="logger"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public void WriteTabulated(ILogger logger)
        {
            if (_endPoints.Count == 0)
                return;

            WriteTabulated(logger, false);

            logger.LogInformation("{Content}", string.Empty);
        }

        /// <summary>
        /// Writes the tabulated data.
        /// </summary>
        private void WriteTabulated(ILogger logger, bool indent)
        { 
            // Write table header.
            var routeLength = Math.Max(5, _endPoints.Max(x => x.Route?.Length ?? 0));
            var methodLength = Math.Max(6, _endPoints.Max(x => x.Method?.Length ?? 0));
            var statusLength = Math.Max(6, _endPoints.Max(x => x.Status?.Length ?? 0));
            var authLength = Math.Max(4, _endPoints.Max(x => x.Auth?.Length ?? 0));
            var tagsLength = Math.Max(4, _endPoints.Max(x => x.Tags?.Length ?? 0));

            var hdrRoute = string.Format("{0, -" + routeLength + "}", "Route");
            var hdrMethod = string.Format("{0, -" + methodLength + "}", "Method");
            var hdrStatus = string.Format("{0, -" + statusLength + "}", "Status");
            var hdrAuth = string.Format("{0, -" + authLength + "}", "Auth");
            var hdrTags = string.Format("{0, -" + tagsLength + "}", "Tags");

            if (_endPoints.Count == 0)
                return;

            var prefix = indent ? new string(' ', 6) : string.Empty;

            logger.LogInformation("{Content}", $"{prefix}{hdrRoute} | {hdrMethod} | {hdrStatus} | {hdrAuth} | {hdrTags} | Name");
            logger.LogInformation("{Content}", $"{prefix}{new string('-', routeLength + methodLength + statusLength + authLength + tagsLength + 15 + Math.Max(4, _endPoints.Max(x => x.Name?.Length ?? 0)))}");

            // Write the data.
            foreach (var ep in _endPoints.OrderBy(x => x.Route).ThenBy(x => x.Method).ThenBy(x => x.Name))
            {
                var route = string.Format("{0, -" + routeLength + "}", ep.Route);
                var method = string.Format("{0, -" + methodLength + "}", ep.Method);
                var status = string.Format("{0, -" + statusLength + "}", ep.Status);
                var auth = string.Format("{0, -" + authLength + "}", ep.Auth);
                var tags = string.Format("{0, -" + tagsLength + "}", ep.Tags);

                logger.LogInformation("{Content}", $"{prefix}{route} | {method.ToUpperInvariant()} | {status} | {auth} | {tags} | {ep.Name}");
            }
        }

        /// <summary>
        /// Writes as inline tabulated to the <paramref name="logger"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public void WriteInline(ILogger logger)
        {
            logger.LogInformation("{Content}", $"    Endpoints:{(_endPoints.Count == 0 ? " none" : $" ({_endPoints.Count})")}");
            if (_endPoints.Count > 0)
                WriteTabulated(logger, true);
        }
    }

    /// <summary>
    /// Represents the endpoint data for reporting.
    /// </summary>
    internal class EndPointData
    {
        public string? Route { get; set; }
        public string? Method { get; set; }
        public string? Status { get; set; }
        public string? Auth { get; set; }
        public string? Tags { get; set; }
        public string? Name { get; set; }
    }
}
