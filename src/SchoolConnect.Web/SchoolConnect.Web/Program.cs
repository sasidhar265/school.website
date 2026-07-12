using SchoolConnect.Web.Components;
using SchoolConnect.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.AspNetCore.Identity;
using SchoolConnect.Shared.Configuration;
using SchoolConnect.Web;
using Microsoft.AspNetCore.HttpOverrides;
using System.Threading.RateLimiting;

var webRootPath = ResolveWebRootPath();

var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
    WebRootPath = webRootPath,
    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
        ?? Environments.Production
});

// CreateEmptyBuilder still supplies default configuration providers. Replace them
// before loading application settings and the separate school-content files so
// nested arrays are not bound twice.
builder.Configuration.Sources.Clear();
builder.Configuration.SetBasePath(AppContext.BaseDirectory);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);

string[] schoolContentFiles =
[
    "school.json",
    "portal-auth.json",
    "notices.json",
    "academic-events.json",
    "quick-actions.json",
    "student-modules.json",
    "teacher-modules.json",
    "bus-routes.json",
    "faculty-contacts.json",
    "about-us.json",
    "student-timetables.json",
    "student-curricula.json",
    "student-contents.json",
    "student-progresses.json",
    "gallery-year-groups.json"
];

foreach (var schoolContentFile in schoolContentFiles)
{
    builder.Configuration.AddJsonFile(
        Path.Combine("Data", schoolContentFile),
        optional: false,
        reloadOnChange: false);
}

builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(args);
builder.WebHost.UseKestrel(options => options.AddServerHeader = false);

// Cloud hosts such as Render provide the public HTTP port through PORT.
// Bind to every container interface so the platform's proxy can reach Kestrel.
var platformPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(platformPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{platformPort}");
}
else if (!string.IsNullOrWhiteSpace(builder.Configuration["ASPNETCORE_URLS"]))
{
    builder.WebHost.UseUrls(builder.Configuration["ASPNETCORE_URLS"]!);
}

builder.WebHost.UseStaticWebAssets();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<SchoolContentStore>();
builder.Services.AddScoped<SchoolContentService>();
builder.Services.AddSingleton<LoginAttemptGuard>();
builder.Services.AddScoped<PortalSessionService>();
builder.Services.AddDataProtection();
builder.Services.AddSingleton<IPasswordHasher<PortalAccountOptions>, PasswordHasher<PortalAccountOptions>>();
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 300,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var app = builder.Build();
app.UseForwardedHeaders();

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "SAMEORIGIN";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), usb=()";
    headers["Content-Security-Policy"] = "default-src 'self'; base-uri 'self'; object-src 'none'; frame-ancestors 'self'; form-action 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self' ws: wss:";
    if (!app.Environment.IsDevelopment())
    {
        headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    }

    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found");
// Status-code pages wrap the remaining pipeline. Disable that feature only for
// API requests so their 404 responses can never be replaced by Blazor markup.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        var statusCodePages = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IStatusCodePagesFeature>();
        if (statusCodePages is not null)
        {
            statusCodePages.Enabled = false;
        }
    }

    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.UseStaticFiles();
app.UseRateLimiter();
app.MapSchoolApi();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(SchoolConnect.Shared.Pages.Dashboard).Assembly);

app.Run();

static string ResolveWebRootPath()
{
    var sourceRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "wwwroot"));
    if (Directory.Exists(sourceRoot))
    {
        return sourceRoot;
    }

    var publishedRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
    if (Directory.Exists(publishedRoot))
    {
        return publishedRoot;
    }

    return sourceRoot;
}
