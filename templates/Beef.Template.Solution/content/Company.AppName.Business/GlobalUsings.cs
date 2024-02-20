global using CoreEx;
global using CoreEx.Caching;
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
global using CoreEx.Database.SqlServer.Outbox;
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
global using CoreEx.Http;
global using CoreEx.Http.Extended;
global using CoreEx.Invokers;
global using CoreEx.Json;
global using CoreEx.Mapping;
global using CoreEx.Mapping.Converters;
global using CoreEx.RefData;
global using CoreEx.RefData.Extended;
global using CoreEx.Results;
global using CoreEx.Validation;
global using CoreEx.Validation.Rules;
#if (implement_cosmos)
global using AzCosmos = Microsoft.Azure.Cosmos;
#endif
#if (implement_database || implement_sqlserver)
global using Microsoft.Data.SqlClient;
#endif
#if (implement_entityframework)
global using Microsoft.EntityFrameworkCore;
#endif
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;
#if (implement_mysql)
global using MySql.Data.MySqlClient;
#endif
#if (implement_postgres)
global using Npgsql;
#endif
global using System;
global using System.Collections.Generic;
#if (implement_database || implement_entityframework)
global using System.Data.Common;
#endif
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.Linq;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;
global using System.Net.Http;
global using System.Threading;
global using System.Threading.Tasks;
global using Company.AppName.Business;
global using Company.AppName.Business.Entities;
global using Company.AppName.Business.Data;
global using Company.AppName.Business.DataSvc;
global using Company.AppName.Business.Validation;
global using RefDataNamespace = Company.AppName.Business.Entities;