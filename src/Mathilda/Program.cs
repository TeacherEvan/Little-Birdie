using Mathilda;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<Mathilda.Services.PlacesService>();
builder.Services.AddScoped<Mathilda.Services.WeatherService>();
builder.Services.AddScoped<Mathilda.Services.LocalStore>();
builder.Services.AddScoped<Mathilda.Services.LocationService>();
builder.Services.AddScoped<Mathilda.Services.AppSettingsService>();
builder.Services.AddScoped<Mathilda.Services.PrivacyConsentService>();
builder.Services.AddScoped<Mathilda.Services.InstallPromptService>();

// Convex is only wired up when the user supplies a Custom Convex URL; otherwise
// PlacesService / WeatherService receive a null ConvexClient and use mock data.
builder.Services.AddScoped<Mathilda.Services.ConvexClient>(sp =>
{
    var settings = sp.GetRequiredService<Mathilda.Services.AppSettingsService>();
    var http = sp.GetRequiredService<HttpClient>();
    var url = settings.Current.CustomConvexUrl;
    return string.IsNullOrWhiteSpace(url)
        ? null!
        : new Mathilda.Services.ConvexClient(http, url);
});

await builder.Build().RunAsync();
