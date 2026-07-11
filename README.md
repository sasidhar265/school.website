# SchoolConnect

C#/.NET solution for an Andhra Pradesh school portal with:

- `SchoolConnect.Shared`: shared Razor UI, models, and C# services.
- `SchoolConnect.Web`: Blazor web app for browser access.
- `SchoolConnect.Mobile`: .NET MAUI Blazor Hybrid host for Android and iOS.

The current scaffold uses school content for "Sri Venkateswara Convent" at Bayyavaram, Krosur Mandal, Palnadu District, Andhra Pradesh, India. The live content model is now stored in PostgreSQL when a connection string is configured, with a configuration fallback for local development.

## Why This Stack

.NET MAUI Blazor Hybrid is the best C# fit for this requirement because the same Razor components and C# services can run in:

- a web browser through Blazor Server;
- Android as a native MAUI app;
- iOS as a native MAUI app.

That keeps UI, validation, and school workflows in one shared codebase.

## Run Web

Set `ConnectionStrings:SchoolConnectDb` in `src/SchoolConnect.Web/SchoolConnect.Web/appsettings.json` or through an environment variable before production deployment.

Application-level settings remain in `appsettings.json`. Editable school content is split by feature under `src/SchoolConnect.Web/SchoolConnect.Web/Data`, including school identity, authentication, notices, academics, transport, gallery, and student records.

```bash
dotnet restore src/SchoolConnect.Web/SchoolConnect.Web/SchoolConnect.Web.csproj --ignore-failed-sources
dotnet run --project src/SchoolConnect.Web/SchoolConnect.Web/SchoolConnect.Web.csproj
```

The web project targets .NET 8 and has `UseAppHost=false` so it can restore on this machine without downloading the native app host package.

## Run Mobile

Install the MAUI workloads first:

```bash
dotnet workload restore src/SchoolConnect.Mobile/SchoolConnect.Mobile.csproj
```

Then build or run a target:

```bash
dotnet build src/SchoolConnect.Mobile/SchoolConnect.Mobile.csproj -f net8.0-android
dotnet build src/SchoolConnect.Mobile/SchoolConnect.Mobile.csproj -f net8.0-ios
```

iOS builds require Xcode and Apple signing setup on macOS. Android builds require the Android SDK/emulator setup from the MAUI workload.

## Deploy as a Website on Render

The repository includes a production `Dockerfile` and `render.yaml` Blueprint.

1. Sign in to Render and choose **New > Blueprint**.
2. Connect the `sasidhar265/school.website` GitHub repository.
3. Keep `render.yaml` as the Blueprint path and apply the Blueprint.
4. Open the generated `schoolconnect-web.onrender.com` address after the first deployment completes.

Render supplies the `PORT` environment variable automatically. The application binds to that port on `0.0.0.0`. Each push to `master` triggers a new deployment.

The site works with the configuration fallback and does not require a database. To persist edited content in PostgreSQL, add a Render PostgreSQL database and set the web service secret named `ConnectionStrings__SchoolConnectDb` to its connection string.

## Administrator Dashboard

The protected dashboard is available at `/admin`. It currently manages school contact details, the admissions headline, and public notices. Content changes are stored in PostgreSQL, so `ConnectionStrings__SchoolConnectDb` must be configured before saving.

Set these Render environment variables before using the dashboard:

```text
SchoolConnect__PortalAuth__Admin__Pin=ADMIN
SchoolConnect__PortalAuth__Admin__Password=<a-long-unique-password>
ConnectionStrings__SchoolConnectDb=<Render PostgreSQL connection string>
```

The Blueprint declares the password and database connection as secret values and prompts for them during setup. The repository contains no default administrator password, so admin access remains disabled until the secret is configured. Portal sessions are server-signed, expire after eight hours, and reject edited browser-storage values.

## Next Production Work

- Move student and teacher demo accounts into a full per-user identity database with password reset and account lifecycle management.
- Add a proper API boundary for mobile clients if the app will be installed on student and teacher devices.
- Add push notifications through Firebase Cloud Messaging and Apple Push Notification service.
- Integrate a payment gateway that supports Indian school fee payments.
- Add Telugu copy and localization files if the school wants bilingual screens.
