using SchoolConnect.Web.Components;
using SchoolConnect.Shared.Services;
using Microsoft.Extensions.Configuration;
using System.IO;

var webRootPath = ResolveWebRootPath();

var builder = WebApplication.CreateEmptyBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
    WebRootPath = webRootPath
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
builder.WebHost.UseKestrel();

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
builder.Services.AddScoped<PortalSessionService>();

var app = builder.Build();

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

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.UseStaticFiles();
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
