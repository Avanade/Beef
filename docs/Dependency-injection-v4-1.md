# Upgrading to Beef 4.1.1

The _Beef_ solution went through a significant enhancement to introduce [Dependency Injection (DI)](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) throughout the solution. This required a number of changes to the existing approach where the code-generated classes were instantiated via a `Factory` around a parameterless constructor. Many _Beef_ framework classes had to be refactored as a result to remove the `static` capabilities as these were counter to the DI approach. Because of this, there was a flow on requirement to review and refactor the corresponding testing to support DI, etc.

<br/>

## Breaking changes

As a result of above, there was no simple way to avoid **breaking changes** to the solution. This article will guide a developer, as best it can, through the process of upgrading a _Beef v3.1.x_ solution to _v4.1.1_.

Also, any other framework code that had been preivously marked using the `ObsoluteAttribute` has since been removed.

Where other code/capabilities were deemed obsolete, or had seen limited use, these have also been removed. For example, the `Beef.Diagnostics.Logger` has been largely replaced as a result of the DI changes; and as such should no longer be considered the go to logging enabler.

<br/>

## NuGet Package upgrade

Perform a solution-wide NuGet Package upgrade to the latest 4.1.1 (or greater). Take this opportunity to upgrade all other packages to their respective latest also. 

<br/>

## Company.AppName.CodeGen

The code generation will need to be re-executed to update all of the existing classes (as appropriate) to leverage dependency injection. Some new classes may also be added to enable support.

<br/>

## Company.AppName.Common

Given the dependency injection improvements the need for both the `XxxAgent` and `XxxServiceAgent` is no longer required. All the logic has been moved into the `XxxAgent`; therefore, the `Agents/ServiceAgents` folder and its contents must be deleted.

<br/>

## Company.AppName.Business

Next upgrade the `Company.AppName.Business` project. Specifically making changes to the non-generated data access components.

<br/>

### Database

The `Database.cs` (or alternate name) will need minor changes. This will now inherit from `DatabaseBase` and remove the `Default` static capability:

``` csharp
// -- Before --
public class Database : Beef.Data.Database.Database<Database>
{
    ...
}

// -- After --
public class Database : DatabaseBase
{
    ...
}
```

<br/>

### Entity Framework

The Entity Framework (EF) changes are a little more nuanced. The `EfDb.cs` (or alternate name) will need minor changes. This will now inherit from `EfDbBase`, pass the corresponding `EfDbContext` within the constructor, and remove the `Default` static capability:

``` csharp
// -- Before --
public class EfDb : EfDb<EfDbContext, EfDb>
{
    public EfDb() => OnUpdatePreReadForNotFound = true;
}

// -- After --
public class EfDb : EfDbBase<EfDbContext>
{
    public EfDb(EfDbContext dbContext) : base(dbContext) => OnUpdatePreReadForNotFound = true;
}
```

The `EfDbContext.cs` (or alternate name) will need need a minor change as follows. This will now implement a constructor for both the `DbContextOptions` and `IDatabase`, the latter will be used to source the connection.

``` csharp
// -- Before --
public class EfDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder == null)
            throw new ArgumentNullException(nameof(optionsBuilder));

        base.OnConfiguring(optionsBuilder);

        // Uses the DB connection management from the database class - ensures DB connection pooling and required DB session context setting.
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(Data.Database.Default.CreateConnection());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        ...
}

// -- After --
public class EfDbContext : DbContext
{
    private readonly IDatabase _db;

    public EfDbContext(DbContextOptions<EfDbContext> options, IDatabase db) : base(options) => _db = Check.NotNull(db, nameof(db));

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder == null)
            throw new ArgumentNullException(nameof(optionsBuilder));

        base.OnConfiguring(optionsBuilder);

        // Uses the DB connection management from the database class - ensures DB connection pooling and required DB session context setting.
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(_db.GetConnection());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        ...
}
```

<br/>

### Data/DataSvc/Manager partial classes

Where non-generated Data/DataSvc/Manager layer classes were used these will also need minor changes. Any references to the existing static classes will need to be changed to use the new instance variables passed in the constructor. The parameterless constructor (where used) will need to be refactored to use the new `XxxDataCtor` partial method - as dependency injection can only suport a single public constructor.

``` csharp
// -- Before --
public XxxData()
{
    _getByArgsOnQuery = GetByArgsOnQuery;
}

// -- After --
partial void XxxDataCtor()
{
    _getByArgsOnQuery = GetByArgsOnQuery;
}
```

<br/>

## Company.AppName.Api

This is where the greatest change as a result of dependency injection occurs. There will be significant changes required within the `Startup.cs`. The following is a guide, as there was no specific prescriptive approach in earlier verions in terms of usage, etc. - YMMV.

<br/>

### Startup contructor

The constructor should be greatly simplified as the data source register and alike will be refactored into the `Configure` method. The main requirement is getting an `IConfig` instance for later use.

 ``` csharp
// -- Before --
public class Startup
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfiguration"/>.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
    public Startup(IConfiguration config, ILoggerFactory loggerFactory)
    {
        // Use JSON property names in validation; default the page size and determine whether unhandled exception details are to be included in the response.
        ValidationArgs.DefaultUseJsonNames = true;
        PagingArgs.DefaultTake = config.GetValue<int>("BeefDefaultPageSize");
        WebApiExceptionHandlerMiddleware.IncludeUnhandledExceptionInResponse = config.GetValue<bool>("BeefIncludeExceptionInInternalServerError");

        // Configure the logger.
        _logger = loggerFactory.CreateLogger("Logging");
        Logger.RegisterGlobal((largs) => WebApiStartup.BindLogger(_logger, largs));

        // Configure the cache policies.
        CachePolicyManager.SetFromCachePolicyConfig(config.GetSection("BeefCaching").Get<CachePolicyConfig>());
        CachePolicyManager.StartFlushTimer(CachePolicyManager.TenMinutes, CachePolicyManager.FiveMinutes);

        // Register the database.
        Database.Register(() => new Database(WebApiStartup.GetConnectionString(config, "Database")));

        // Register the ReferenceData provider.
        Beef.RefData.ReferenceDataManager.Register(new ReferenceDataProvider());

        // Setup the event publishing to event hubs.
        var ehcs = config.GetValue<string>("EventHubConnectionString");
        if (!string.IsNullOrEmpty(ehcs))
        {
            Event.PublishSynchronously = true;
            var ehc = EventHubClient.CreateFromConnectionString(ehcs);
            ehc.RetryPolicy = RetryPolicy.Default;
            var ehp = new EventHubPublisher(ehc);
            Event.Register((events) => ehp.Publish(events));
        }
    }

// -- After --
public class Startup
{
    private readonly IConfiguration _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfiguration"/>.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
    public Startup(IConfiguration config)
    {
        _config = config;

        // Use JSON property names in validation and default the page size.
        ValidationArgs.DefaultUseJsonNames = true;
        PagingArgs.DefaultTake = config.GetValue<int>("BeefDefaultPageSize");
    }

```

<br/>

### ConfigureServices method

The `ConfigureServices` will have the greatest change as all of the dependency injection services are added. There are a number of _Beef_ helper methods to assist the developer in getting these right.

``` csharp
// -- Before --
public void ConfigureServices(IServiceCollection services)
{
    // Add services; note Beef requires NewtonsoftJson.
    services.AddApplicationInsightsTelemetry();
    services.AddControllers().AddNewtonsoftJson();
    services.AddHealthChecks();
    services.AddHttpClient();

    services.AddSwaggerGen(c =>
    {
        ...
    });

    services.AddSwaggerGenNewtonsoftSupport();
}

// -- After --
public void ConfigureServices(IServiceCollection services)
{
    // Add the core beef services.
    services.AddBeefExecutionContext()
            .AddBeefRequestCache()
            .AddBeefCachePolicyManager(_config.GetSection("BeefCaching").Get<CachePolicyConfig>())
            .AddBeefWebApiServices()
            .AddBeefBusinessServices();

    // Add the data source services.
    services.AddBeefDatabaseServices(() => new Database(WebApiStartup.GetConnectionString(_config, "Database")))
            .AddBeefEntityFrameworkServices<EfDbContext, EfDb>();

    // Add the generated reference data services.
    services.AddGeneratedReferenceDataManagerServices()
            .AddGeneratedReferenceDataDataSvcServices()
            .AddGeneratedReferenceDataDataServices();

    // Add the generated entity services.
    services.AddGeneratedManagerServices()
            .AddGeneratedDataSvcServices()
            .AddGeneratedDataServices();

    // Add event publishing.
    var ehcs = _config.GetValue<string>("EventHubConnectionString");
    if (!string.IsNullOrEmpty(ehcs))
        services.AddBeefEventHubEventPublisher(ehcs);
    else
        services.AddBeefNullEventPublisher();

    // Add services; note Beef requires NewtonsoftJson.
    services.AddApplicationInsightsTelemetry();
    services.AddControllers().AddNewtonsoftJson();
    services.AddHealthChecks();
    services.AddHttpClient();

    // Add the swagger services.
    services.AddSwaggerGen(c =>
    {
        ...
    });

    services.AddSwaggerGenNewtonsoftSupport();
}
```

<br/>

### Configure method

The `Configure` need some minor changes depending on current usage. The existing `WebApiServiceAgentManager` for registering `HttpClient` etc. has been deprecated and the native dependency injection capabilities should be used instead. The `UseWebApiExceptionHandler` now requires an `ILogger` parameter.

``` csharp
// -- Before --
public void Configure(IApplicationBuilder app, IHttpClientFactory clientFactory)
{
    // Register the ServiceAgent HttpClientCreate (for cross-domain calls) so it uses the factory.
    WebApiServiceAgentManager.RegisterHttpClientCreate((rd) =>
    {
        var hc = clientFactory.CreateClient(rd.BaseAddress.AbsoluteUri);
        hc.BaseAddress = rd.BaseAddress;
        return hc;
    });

    // Add exception handling to the pipeline.
    app.UseWebApiExceptionHandler();

    ...

// -- After --
public void Configure(IApplicationBuilder app, ILogger<WebApiExceptionHandlerMiddleware> logger)
{
    // Override the exception handling.
    app.UseWebApiExceptionHandler(Check.NotNull(logger, nameof(logger)), _config.GetValue<bool>("BeefIncludeExceptionInInternalServerError"));

    ...
}
```