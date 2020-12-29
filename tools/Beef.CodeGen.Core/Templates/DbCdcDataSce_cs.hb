﻿{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable

using Microsoft.Extensions.DependencyInjection;

namespace {{Root.Company}}.{{Root.AppName}}.Cdc.Data
{
    /// <summary>
    /// Provides the generated CDC <b>Data</b>-layer services.
    /// </summary>
    public static class ServiceCollectionsExtension
    {
        /// <summary>
        /// Adds the generated CDC <b>Data</b>-layer services.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddGeneratedCdcDataServices(this IServiceCollection services)
        {
{{#each Cdc}}
            {{#if @first}}return services{{else}}               {{/if}}.AddScoped<I{{ModelName}}CdcData, {{ModelName}}CdcData>(){{#if @last}};{{/if}}
{{/each}}
        }
    }
}

#nullable restore