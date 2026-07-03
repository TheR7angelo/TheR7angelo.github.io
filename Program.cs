using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using TheR7angelo.github.io;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var host = builder.Build();

var jsRuntime = host.Services.GetRequiredService<IJSRuntime>();

try
{
    var browserLang = await jsRuntime.InvokeAsync<string>("window.navigator.language");
    if (!string.IsNullOrEmpty(browserLang))
    {
        var customCulture = new CultureInfo(browserLang);
        CultureInfo.DefaultThreadCurrentCulture = customCulture;
        CultureInfo.DefaultThreadCurrentUICulture = customCulture;
    }
}
catch
{
    // Ignore
}

await host.RunAsync();