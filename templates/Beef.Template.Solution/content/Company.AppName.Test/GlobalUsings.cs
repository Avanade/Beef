#if (implement_database || implement_entityframework)
global using Beef.Database;
#endif
#if (implement_database || implement_sqlserver)
global using Beef.Database.SqlServer;
#endif
#if (implement_mysql)
global using Beef.Database.MySql;
#endif
#if (implement_postgres)
global using Beef.Database.Postgres;
#endif
global using CoreEx;
#if (implement_cosmos)
global using CoreEx.Cosmos;
global using CoreEx.Cosmos.Batch;
#endif
global using CoreEx.Entities;
#if (implement_cosmos)
global using CoreEx.Json.Data;
#endif
global using CoreEx.Http;
global using CoreEx.RefData;
global using CoreEx.Validation;
#if (implement_database || implement_entityframework)
global using DbEx;
#endif
#if (implement_cosmos)
global using AzCosmos = Microsoft.Azure.Cosmos;
#endif
global using Microsoft.Extensions.DependencyInjection;
global using Moq;
global using NUnit.Framework;
global using System;
global using System.Net;
global using System.Reflection;
global using System.Threading;
global using System.Threading.Tasks;
global using UnitTestEx;
global using UnitTestEx.Expectations;
global using Company.AppName.Api;
global using Company.AppName.Business;
global using Company.AppName.Business.Data;
global using Company.AppName.Business.DataSvc;
global using Company.AppName.Business.Validation;
global using Company.AppName.Common.Agents;
global using HttpRequestOptions = CoreEx.Http.HttpRequestOptions;