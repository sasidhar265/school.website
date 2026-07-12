using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SchoolConnect.Shared.Configuration;

namespace SchoolConnect.Shared.Services;

public sealed class SchoolContentStore
{
    private const string DefaultContentKey = "default";
    private const string TableName = "school_connect_content";

    private readonly IConfiguration configuration;
    private readonly ILogger<SchoolContentStore> logger;
    private SchoolConnectOptions? cachedOptions;
    private readonly object syncRoot = new();

    public SchoolContentStore(IConfiguration configuration, ILogger<SchoolContentStore> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
    }

    public SchoolConnectOptions Options
    {
        get
        {
            lock (syncRoot)
            {
                return cachedOptions ??= LoadOptions();
            }
        }
    }

    public void Save(SchoolConnectOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var connectionString = GetConnectionString();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            SaveToLocalFile(options);
            lock (syncRoot)
            {
                cachedOptions = options;
            }
            return;
        }

        using var connection = new NpgsqlConnection(connectionString);
        connection.Open();
        EnsureSchema(connection);
        UpsertPayload(connection, options);

        lock (syncRoot)
        {
            cachedOptions = options;
        }
    }

    private SchoolConnectOptions LoadOptions()
    {
        var seed = configuration.GetSection("SchoolConnect").Get<SchoolConnectOptions>() ?? new SchoolConnectOptions();
        var connectionString = GetConnectionString();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return LoadFromLocalFile(seed);
        }

        try
        {
            using var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            EnsureSchema(connection);

            var storedPayload = ReadPayload(connection);
            if (string.IsNullOrWhiteSpace(storedPayload))
            {
                UpsertPayload(connection, seed);
                return seed;
            }

            var loaded = JsonSerializer.Deserialize<SchoolConnectOptions>(storedPayload);
            if (loaded is null)
            {
                return seed;
            }

            // Authentication is configuration-only and must never be sourced from
            // the editable content document stored in PostgreSQL.
            loaded.PortalAuth = seed.PortalAuth;
            return loaded;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falling back to JSON configuration content because PostgreSQL content load failed.");
            return seed;
        }
    }

    private static void EnsureSchema(NpgsqlConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $@"
            create table if not exists {TableName} (
                content_key text primary key,
                payload jsonb not null,
                updated_at timestamptz not null default now()
            );";
        command.ExecuteNonQuery();
    }

    private static string? ReadPayload(NpgsqlConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"select payload::text from {TableName} where content_key = @content_key;";
        command.Parameters.AddWithValue("content_key", DefaultContentKey);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return reader.GetString(0);
    }

    private static void UpsertPayload(NpgsqlConnection connection, SchoolConnectOptions seed)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $@"
            insert into {TableName} (content_key, payload, updated_at)
            values (@content_key, @payload::jsonb, now())
            on conflict (content_key)
            do update set payload = excluded.payload, updated_at = excluded.updated_at;";
        command.Parameters.AddWithValue("content_key", DefaultContentKey);
        command.Parameters.AddWithValue("payload", SerializeContentOnly(seed));
        command.ExecuteNonQuery();
    }

    private static string SerializeContentOnly(SchoolConnectOptions options)
    {
        var clone = JsonSerializer.Deserialize<SchoolConnectOptions>(JsonSerializer.Serialize(options))
            ?? new SchoolConnectOptions();
        clone.PortalAuth = new PortalAuthOptions();
        return JsonSerializer.Serialize(clone);
    }

    private string? GetConnectionString()
    {
        var configured = configuration.GetConnectionString("SchoolConnectDb");
        if (string.IsNullOrWhiteSpace(configured)
            || (!configured.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
                && !configured.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)))
        {
            return configured;
        }

        var uri = new Uri(configured);
        var credentials = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(credentials[0]),
            Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : string.Empty,
            Pooling = true,
            ApplicationName = "SchoolConnect"
        };

        return builder.ConnectionString;
    }

    private SchoolConnectOptions LoadFromLocalFile(SchoolConnectOptions seed)
    {
        var path = GetLocalContentPath();
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return seed;
        }

        try
        {
            var loaded = JsonSerializer.Deserialize<SchoolConnectOptions>(File.ReadAllText(path));
            if (loaded is null)
            {
                return seed;
            }

            loaded.PortalAuth = seed.PortalAuth;
            return loaded;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falling back to configured JSON content because the local editable content file could not be read.");
            return seed;
        }
    }

    private void SaveToLocalFile(SchoolConnectOptions options)
    {
        var path = GetLocalContentPath();
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new InvalidOperationException("Content editing requires either ConnectionStrings:SchoolConnectDb or SchoolConnectContentFile to be configured.");
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var temporaryPath = path + ".tmp";
        File.WriteAllText(temporaryPath, SerializeContentOnly(options));
        File.Move(temporaryPath, path, true);
    }

    private string? GetLocalContentPath()
    {
        var configured = configuration["SchoolConnectContentFile"];
        if (string.IsNullOrWhiteSpace(configured))
        {
            return null;
        }

        return Path.IsPathRooted(configured)
            ? configured
            : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configured));
    }
}
