global using CoreEx;
global using CoreEx.Entities;
global using CoreEx.Events;
global using CoreEx.RefData;
global using CoreEx.Validation;
global using CoreEx.WebApis;
global using Microsoft.AspNetCore.Mvc;
#if (implement_cosmos)
global using AzCosmos = Microsoft.Azure.Cosmos;
#endif
#if (implement_database || implement_entityframework)
global using Microsoft.Data.SqlClient;
#endif
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.OpenApi.Models;
global using System.Net;
global using System.Reflection;
global using Company.AppName.Business;
global using Company.AppName.Business.Data;
global using Company.AppName.Business.Entities;
global using Company.AppName.Business.Validation;
global using RefDataNamespace = Company.AppName.Business.Entities;