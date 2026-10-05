using System.Globalization;
using Arniston_Letting_Front.Services;
using Arniston_Letting_Front.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Razor Pages - everything under /Admin requires a signed-in user.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin");
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Login/Login");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AllowAnonymousToPage("/Privacy");
});

// Cookie authentication (the API returns the user details on login).
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
    });

builder.Services.AddAuthorization();

// Typed HTTP clients for the Arniston Letting API.
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? "https://arniston.duckdns.org/";

void AddApiClient<TInterface, TImplementation>()
    where TInterface : class
    where TImplementation : class, TInterface
{
    builder.Services.AddHttpClient<TInterface, TImplementation>(client =>
    {
        client.BaseAddress = new Uri(apiBaseUrl);
        client.Timeout = TimeSpan.FromSeconds(30);
    });
}

AddApiClient<IAuthApiService, AuthApiService>();
AddApiClient<IBookingApiService, BookingApiService>();
AddApiClient<IBreakageApiService, BreakageApiService>();
AddApiClient<ICleanerApiService, CleanerApiService>();
AddApiClient<ICleanerTaskApiService, CleanerTaskApiService>();
AddApiClient<IDashboardApiService, DashboardApiService>();
AddApiClient<INotificationApiService, NotificationApiService>();
AddApiClient<IOwnerApiService, OwnerApiService>();
AddApiClient<IPropertyApiService, PropertyApiService>();
AddApiClient<IReportApiService, ReportApiService>();

var app = builder.Build();

// Use a fixed culture so decimals/dates round-trip through HTML inputs
// (type="number" / type="date") regardless of the machine's regional settings.
var culture = new CultureInfo("en-US");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new List<CultureInfo> { culture },
    SupportedUICultures = new List<CultureInfo> { culture },
    RequestCultureProviders = new List<IRequestCultureProvider>()
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
