using Mathilda;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<Mathilda.Services.PlacesService>();
builder.Services.AddScoped<Mathilda.Services.WeatherService>();
builder.Services.AddScoped<Mathilda.Services.AppSettingsService>();
builder.Services.AddScoped<Mathilda.Services.PrivacyConsentService>();
builder.Services.AddScoped<Mathilda.Services.InstallPromptService>();

await builder.Build().RunAsync();
