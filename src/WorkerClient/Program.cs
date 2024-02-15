using IdentityModel.Client;
using Serilog;
using WorkerClient;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext())
        .ConfigureServices((hostContext, services) =>
        {
            // default cache
            services.AddDistributedMemoryCache();

            services.AddClientCredentialsTokenManagement();
            services.AddSingleton(new DiscoveryCache(hostContext.Configuration["Sts:Authority"]));

            // Configure OAuth access Token management
            services.AddClientCredentialsTokenManagement()
                .AddClient("sts", client =>
                {
                    var sp = services.BuildServiceProvider();

                    var _cache = sp.GetService<DiscoveryCache>();

                    client.TokenEndpoint = _cache.GetAsync().GetAwaiter().GetResult().TokenEndpoint;
                    client.ClientId = hostContext.Configuration["Sts:ClientId"];
                    client.ClientSecret = hostContext.Configuration["Sts:ClientSecret"];

                    client.Scope = "calc:double";
                    
                    client.Resource = hostContext.Configuration["Api:Audience"];

                });

            // Configure http client
            services.AddHttpClient("client",
                    client => { client.BaseAddress = new Uri(hostContext.Configuration["Api:BaseAddress"]); })
                .AddClientCredentialsTokenHandler("sts");

            // services.AddTransient<IClientAssertionService, ClientAssertionService>();

            services.AddHostedService<Worker>();
        })
        .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occured during bootstrapping");
}
finally
{
    Log.CloseAndFlush();
}
