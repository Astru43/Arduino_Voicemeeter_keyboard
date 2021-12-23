using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vm_Serial;

using IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options => {
        options.ServiceName = "VmDebugService";
    })
    .ConfigureServices(services => {
        services.AddHostedService<Worker>();
        services.AddSingleton<SerialService>();
        services.AddSingleton<OutputHandler>();
    })
    .Build();

await host.RunAsync();
