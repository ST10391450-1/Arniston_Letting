using System.Globalization;
using Arniston_Letting_Front.Services;
using Arniston_Letting_Front.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin");
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Login/Login");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AllowAnonymousToPage("/Privacy");
});

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];

if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    throw new InvalidOperationException(
        "ApiSettings:BaseUrl is not configured.");
}

builder.Services.AddTransient<JwtAuthorizationHandler>();

void AddApiClient<TInterface, TImplementation>()
    where TInterface : class
    where TImplementation : class, TInterface
{
    builder.Services
        .AddHttpClient<TInterface, TImplementation>(client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddHttpMessageHandler<JwtAuthorizationHandler>();
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

var culture = new CultureInfo("en-US");

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new List<CultureInfo> { culture },
    SupportedUICultures = new List<CultureInfo> { culture },
    RequestCultureProviders = new List<IRequestCultureProvider>()
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
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