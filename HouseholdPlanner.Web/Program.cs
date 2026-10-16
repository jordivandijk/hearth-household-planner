using HouseholdPlanner.Web;
using HouseholdPlanner.Web.ApiClients.Implementations;
using HouseholdPlanner.Web.ApiClients.Interfaces;
using HouseholdPlanner.Web.Auth;
using HouseholdPlanner.Web.Services;
using HouseholdPlanner.Web.Theme;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Empty when the API is on the app's own domain under /api/. Docker fills it from API_URL in .env.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] is { Length: > 0 } url ? url : builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(sp =>
{
    var http = new HttpClient(new SessionExpiredHandler(sp) { InnerHandler = new HttpClientHandler() }) { BaseAddress = new Uri(apiBaseUrl) };
    // The API answers errors in this language.
    http.DefaultRequestHeaders.AcceptLanguage.ParseAdd(Languages.Current);
    return http;
});

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = true;
    config.SnackbarConfiguration.MaxDisplayedSnackbars = 1;
    config.SnackbarConfiguration.ShowCloseIcon = false;
    config.SnackbarConfiguration.HideIcon = true;
    // Solid, so the list behind a toast doesn't show through its text.
    config.SnackbarConfiguration.MaximumOpacity = 100;
    config.SnackbarConfiguration.VisibleStateDuration = 4200;
});

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<HouseholdAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<HouseholdAuthStateProvider>());

builder.Services.AddScoped<IAuthApiClient, AuthApiClient>();
builder.Services.AddScoped<IHouseholdApiClient, HouseholdApiClient>();
builder.Services.AddScoped<ITaskApiClient, TaskApiClient>();
builder.Services.AddScoped<IGroceryApiClient, GroceryApiClient>();
builder.Services.AddScoped<IApiKeyApiClient, ApiKeyApiClient>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<HomeStore>();
builder.Services.AddScoped<ThemeState>();
builder.Services.AddScoped<DateText>();

var host = builder.Build();

// The language has to be set before the app starts: that's when Blazor loads its translations.
var js = host.Services.GetRequiredService<IJSRuntime>();
Languages.Apply(await Languages.ReadAsync(js));
await js.InvokeVoidAsync("document.documentElement.setAttribute", "lang", Languages.Current);

await host.RunAsync();
