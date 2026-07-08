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
    private readonly Lazy<SchoolConnectOptions> cachedOptions;

    public SchoolContentStore(IConfiguration configuration, ILogger<SchoolContentStore> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
        cachedOptions = new Lazy<SchoolConnectOptions>(LoadOptions, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public SchoolConnectOptions Options => cachedOptions.Value;

    private SchoolConnectOptions LoadOptions()
    {
        var seed = configuration.GetSection("SchoolConnect").Get<SchoolConnectOptions>() ?? new SchoolConnectOptions();
        var connectionString = configuration.GetConnectionString("SchoolConnectDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return seed;
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
            return loaded ?? seed;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falling back to appsettings content because PostgreSQL content load failed.");
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
        command.Parameters.AddWithValue("payload", JsonSerializer.Serialize(seed));
        command.ExecuteNonQuery();
    }
}
