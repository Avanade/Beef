global using Azure.Messaging.ServiceBus;
global using Azure.Monitor.OpenTelemetry.Exporter;
global using CoreEx;
global using CoreEx.Azure.ServiceBus;
global using CoreEx.Azure.Storage;
global using CoreEx.Configuration;
#if (implement_cosmos)
global using CoreEx.Cosmos;
#endif
#if (implement_database || implement_entityframework)
global using CoreEx.Database;
#endif
#if (implement_database)
global using CoreEx.Database.Extended;
global using CoreEx.Database.Mapping;
#endif
#if (implement_database || implement_sqlserver)
global using CoreEx.Database.SqlServer;
#endif
#if (implement_mysql)
global using CoreEx.Database.MySql;
#endif
#if (implement_postgres)
global using CoreEx.Database.Postgres;
#endif
global using CoreEx.Entities;
global using CoreEx.Entities.Extended;
#if (implement_entityframework)
global using CoreEx.EntityFrameworkCore;
#endif
global using CoreEx.Events;
global using CoreEx.Events.Subscribing;
global using CoreEx.Http;
global using CoreEx.Hosting;
global using CoreEx.Json;
global using CoreEx.Mapping;
global using CoreEx.Mapping.Converters;
global using CoreEx.RefData;
global using CoreEx.RefData.Extended;
global using CoreEx.Results;
global using CoreEx.Validation;
global using CoreEx.Validation.Rules;
global using Microsoft.Azure.Functions.Worker;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
#if (implement_database || implement_sqlserver)
global using Microsoft.Data.SqlClient;
#endif
#if (implement_mysql)
global using MySql.Data.MySqlClient;
#endif
#if (implement_postgres)
global using Npgsql;
#endif
global using OpenTelemetry.Trace;
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
global using Company.AppName.Business;
global using Company.AppName.Business.Entities;
global using Company.AppName.Business.Data;
global using Company.AppName.Business.DataSvc;
global using Company.AppName.Business.Validation;
global using RefDataNamespace = Company.AppName.Business.Entities;
#if (implement_cosmos)
global using AzCosmos = Microsoft.Azure.Cosmos;
#endif
global using AzServiceBus = Azure.Messaging.ServiceBus;
#if (implement_database || implement_sqlserver)
global using AzBlobs = Azure.Storage.Blobs;
#endif
