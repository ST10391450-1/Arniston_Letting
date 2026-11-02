using System.Globalization;
using Arniston_Letting_Front.Services;
using Arniston_Letting_Front.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
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

// The site runs behind Nginx in production; honour its forwarded headers so
// the original scheme/IP are seen (only loopback proxies are trusted).
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});

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

        // The cookie lasts up to 8 hours but the API JWT stored inside it
        // expires much sooner (Jwt:ExpiresInMinutes, 60 by default). Once the
        // token lapses every API call returns 401, which the API clients turn
        // into empty lists, so the admin saw blank pages while still "logged
        // in". End the session when the token expires so the user is sent
        // back to the login page instead.
        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var accessToken =
                    context.Properties.GetTokenValue("access_token");

                if (JwtExpiry.IsExpired(accessToken))
                {
                    context.RejectPrincipal();

                    await context.HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme);
                }
            }
        };
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

app.UseForwardedHeaders();

// Basic hardening headers for every response.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;

    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

    await next();
});

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