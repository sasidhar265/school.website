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

builder.Configuration.SetBasePath(AppContext.BaseDirectory);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddCommandLine(args);
builder.WebHost.UseKestrel();
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
