using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using MovieTicketsNotifierV3FuncApp.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        builder.AddEnvironmentVariables();
    })
    .ConfigureServices(services =>
    {
        services.AddSingleton<SupabaseService>();
    })
    //.ConfigureFunctionsWorkerDefaults()
    .Build();

host.Run();