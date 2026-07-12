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

### Prerequisites

- .NET 8 SDK
- Docker Desktop for local PostgreSQL
- Git

### 1. Start PostgreSQL

Start Docker Desktop, then run this command from the repository root:

```bash
docker compose up -d postgres
```

Confirm that the database is healthy:

```bash
docker compose ps
docker compose exec postgres pg_isready -U schoolconnect -d schoolconnect
```

The local database connection is defined in `appsettings.Development.json`:

| Setting | Value |
| --- | --- |
| Host | `localhost` |
| Port | `5432` |
| Database | `schoolconnect` |
| Username | `schoolconnect` |
| Password | `schoolconnect_local` |

These credentials are intended only for local development. Do not use them in production.

### 2. Restore and run the website

Application-level settings remain in `appsettings.json`. Editable school content is split by feature under `src/SchoolConnect.Web/SchoolConnect.Web/Data`, including school identity, authentication, notices, academics, transport, gallery, and student records.

```bash
dotnet restore src/SchoolConnect.Web/SchoolConnect.Web/SchoolConnect.Web.csproj --ignore-failed-sources
dotnet run --project src/SchoolConnect.Web/SchoolConnect.Web/SchoolConnect.Web.csproj
```

Open `http://localhost:5080`. The launch profile sets the environment to `Development`, supplies the local administrator credentials, and binds the application to port `5080`.

The web project targets .NET 8 and has `UseAppHost=false` so it can restore on this machine without downloading the native app host package.

### 3. Stop the local services

Stop the website with `Ctrl+C`. Stop PostgreSQL without deleting its data:

```bash
docker compose stop postgres
```

Start the existing database again with `docker compose up -d postgres`.

## PostgreSQL Content Storage

`SchoolContentStore` controls the database lifecycle:

1. The application reads the configured `ConnectionStrings:SchoolConnectDb` value.
2. On first content access, it creates the `school_connect_content` table if it does not exist.
3. If no `default` row exists, it serializes the JSON configuration and inserts it as a PostgreSQL `jsonb` document.
4. Subsequent application starts read the stored document instead of reseeding it.
5. Admin dashboard saves update the same row and its `updated_at` timestamp.
6. If PostgreSQL cannot be reached, the public website falls back to its JSON configuration and logs a warning.

Authentication credentials are never stored in the editable PostgreSQL document. Student, teacher, and administrator credentials remain server configuration.

Inspect the stored content:

```bash
docker compose exec postgres psql -U schoolconnect -d schoolconnect
```

Then run:

```sql
select content_key, updated_at from school_connect_content;
select jsonb_pretty(payload) from school_connect_content where content_key = 'default';
```

### Reset local content

To discard all local PostgreSQL content and reseed from the repository JSON files:

```bash
docker compose down -v
docker compose up -d postgres
```

Warning: `down -v` permanently deletes the local Docker database volume and all admin changes stored in it.

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

The site works with the configuration fallback and does not require a database locally. The Render Blueprint provisions a private, same-region PostgreSQL database and automatically injects its internal connection URL as `ConnectionStrings__SchoolConnectDb`. On first access, the application creates its content table and seeds it from the JSON files. Admin dashboard saves then persist to PostgreSQL and are rendered on the production website after refresh.

The Blueprint uses Render's `basic-256mb` database plan for durable production storage. To experiment at no charge, change the database plan to `free` before creating the Blueprint, but Render's free PostgreSQL databases expire after 30 days and do not include backups, so they are not suitable for production.

## Administrator Dashboard

The protected dashboard is available from **Login > Administration Login** or directly at `/admin`.

Local development credentials are:

```text
Administrator ID: ADMIN
Password: admin@123
```

The dashboard manages:

- school identity, contact information, board and campus labels;
- admissions banner and headline;
- notices, audiences, priorities, and summaries;
- academic events;
- faculty contacts;
- transport routes, drivers, times, and covered areas;
- About Us content, highlights, and principles;
- gallery collections, captions, and image URLs.

Select a section, edit its fields, and choose **Save all changes**. After a successful save, refresh the public website to see the PostgreSQL-backed content. Signing out clears the protected browser session and returns to the homepage.

### Production administrator configuration

Set these Render environment variables before using the dashboard:

```text
SchoolConnect__PortalAuth__Admin__Pin=ADMIN
SchoolConnect__PortalAuth__Admin__Password=<a-long-unique-password>
```

The Blueprint prompts for the administrator password and obtains the database connection directly from the managed `schoolconnect-db` resource. The repository contains no default production administrator password, so admin access remains disabled until the secret is configured. Portal sessions are server-signed, expire after eight hours, and reject edited browser-storage values.

## Public JSON API

SchoolConnect exposes read-only JSON endpoints for website and mobile clients:

| Endpoint | Description |
|---|---|
| `GET /api/health` | Service health and UTC timestamp |
| `GET /api/school` | Public school identity and contact details |
| `GET /api/notices` | Published notices |
| `GET /api/events` | Academic events |
| `GET /api/faculty` | Faculty names, roles, contacts, and photo URLs |
| `GET /api/classes` | Available classes in school order |
| `GET /api/classes/{className}/study-content` | Notes, activities, and assessments for a class |
| `GET /api/classes/{className}/curriculum` | Subjects, topics, and outcomes for a class |

Unknown class resources return a structured JSON `404` response. API errors are isolated from the Blazor status-page pipeline, so API clients never receive an HTML page shell in place of JSON.

## Test and Verification

Run the unit tests:

```bash
dotnet test tests/SchoolConnect.Tests/SchoolConnect.Tests.csproj
```

Run the web smoke tests:

```bash
dotnet test tests/SchoolConnect.Smoke.Tests/SchoolConnect.Smoke.Tests.csproj
```

Build only the web application:

```bash
dotnet build src/SchoolConnect.Web/SchoolConnect.Web/SchoolConnect.Web.csproj --no-restore
```

## Troubleshooting

### Port 5432 is already in use

Stop the existing PostgreSQL service or change the host-side port in `compose.yaml` and update `appsettings.Development.json` to match.

### Docker reports that its API is unavailable

Open Docker Desktop and wait until its engine reports that it is running, then retry `docker compose up -d postgres`.

### Admin save says PostgreSQL is not configured

Confirm the container is healthy with `docker compose ps`, confirm the app is running in the `Development` environment, and restart the web application after changing configuration.

### Website loads but database changes do not appear

Restart the web application to clear its in-memory content cache. Ensure you are connected to the expected database by inspecting `school_connect_content` with `psql`.

### Render database connection fails

Keep the web service and database in the same Render region. The Blueprint uses the private internal connection URL and converts Render's `postgresql://` URL into Npgsql's native connection-string format.

## Project Structure

```text
src/
  SchoolConnect.Shared/       Shared Razor pages, models, services, and CSS
  SchoolConnect.Web/          Blazor Server host and production configuration
  SchoolConnect.Mobile/       .NET MAUI Blazor Hybrid host
tests/
  SchoolConnect.Tests/        Unit tests
  SchoolConnect.Smoke.Tests/  HTTP application smoke tests
compose.yaml                  Local PostgreSQL service
render.yaml                   Render web and PostgreSQL Blueprint
Dockerfile                    Production container build
```

## Next Production Work

- Move student and teacher demo accounts into a full per-user identity database with password reset and account lifecycle management.
- Add a proper API boundary for mobile clients if the app will be installed on student and teacher devices.
- Add push notifications through Firebase Cloud Messaging and Apple Push Notification service.
- Integrate a payment gateway that supports Indian school fee payments.
- Add Telugu copy and localization files if the school wants bilingual screens.
