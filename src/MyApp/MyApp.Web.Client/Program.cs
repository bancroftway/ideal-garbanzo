using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MyApp.Shared.Services;
using MyApp.Web.Client.Services;

namespace MyApp.Web.Client;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        // Add device-specific services used by the MyApp.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();

        await builder.Build().RunAsync();
    }
}
