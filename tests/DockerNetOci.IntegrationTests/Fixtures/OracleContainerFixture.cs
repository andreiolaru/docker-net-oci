using System.Net.Sockets;
using Testcontainers.Oracle;

namespace DockerNetOci.IntegrationTests.Fixtures;

/// <summary>
/// Manages the Oracle database lifecycle for integration tests.
///
/// Three modes (checked in order):
///   1. Explicit: Set INTEGRATION_TEST_ORACLE_CONNECTION env var to any Oracle connection string.
///   2. Local docker-compose: If Oracle is already listening on localhost:1521 (started via
///      docker-compose -f docker/docker-compose.ci.yml up -d), connects automatically.
///   3. Testcontainers: Pulls and starts an Oracle container on the fly.
///
/// For clean-image testing, pass FORCE_TESTCONTAINERS=true to skip auto-detect:
///   dotnet test -- RunConfiguration.EnvironmentVariables.FORCE_TESTCONTAINERS=true
/// </summary>
public class OracleContainerFixture : IAsyncLifetime
{
    private const string ConnectionStringEnvVar = "INTEGRATION_TEST_ORACLE_CONNECTION";
    private const string ImageEnvVar = "TESTCONTAINERS_ORACLE_IMAGE";
    private const string DefaultImage = "container-registry.oracle.com/database/free:23.26.1.0-lite";
    private const string AppUser = "MEMBER_APP";
    private const string AppPassword = "MemberAppPass1";

    private const string DefaultLocalConnectionString =
        $"User Id={AppUser};Password={AppPassword};" +
        "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))" +
        "(CONNECT_DATA=(SERVICE_NAME=freepdb1)))";

    private OracleContainer? _container;
    private string? _connectionString;

    public string ConnectionString => _connectionString
        ?? throw new InvalidOperationException("Fixture not initialized. Call InitializeAsync first.");

    public async Task InitializeAsync()
    {
        // Mode 1: Explicit env var
        var envConnectionString = Environment.GetEnvironmentVariable(ConnectionStringEnvVar);
        if (!string.IsNullOrWhiteSpace(envConnectionString))
        {
            _connectionString = envConnectionString;
            return;
        }

        // Mode 2: Auto-detect local docker-compose Oracle on port 1521 (dev only)
        // Skipped in CI (GitHub Actions, Azure DevOps, GitLab set CI=true)
        // Skipped when FORCE_TESTCONTAINERS=true (for clean image testing)
        var isCi = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
        var forceTestcontainers = string.Equals(
            Environment.GetEnvironmentVariable("FORCE_TESTCONTAINERS"), "true",
            StringComparison.OrdinalIgnoreCase);
        if (!isCi && !forceTestcontainers && IsPortOpen("localhost", 1521))
        {
            _connectionString = DefaultLocalConnectionString;
            return;
        }

        // Mode 3: Testcontainers — fresh Oracle container
        // Image is configurable via TESTCONTAINERS_ORACLE_IMAGE env var (for CI with its own registry)
        var image = Environment.GetEnvironmentVariable(ImageEnvVar) ?? DefaultImage;
        _container = new OracleBuilder()
            .WithImage(image)
            .Build();

        await _container.StartAsync();

        // Create the app user in FREEPDB1 via sqlplus with OS auth (no password needed).
        // We connect as SYSDBA to the CDB, then switch to the PDB.
        var result = await _container.ExecAsync(new[]
        {
            "bash", "-c",
            "echo \"" +
            "ALTER SESSION SET CONTAINER = FREEPDB1;\n" +
            $"CREATE USER {AppUser} IDENTIFIED BY {AppPassword} DEFAULT TABLESPACE SYSAUX TEMPORARY TABLESPACE TEMP;\n" +
            $"GRANT CONNECT, RESOURCE TO {AppUser};\n" +
            $"ALTER USER {AppUser} QUOTA UNLIMITED ON SYSAUX;\n" +
            "EXIT;\n" +
            "\" | sqlplus -s '/ as sysdba'"
        });

        if (result.ExitCode != 0)
            throw new InvalidOperationException(
                $"Failed to create Oracle app user. Exit code: {result.ExitCode}. Stdout: {result.Stdout} Stderr: {result.Stderr}");

        // Build connection string for the app user against FREEPDB1
        var host = _container.Hostname;
        var port = _container.GetMappedPublicPort(1521);
        _connectionString =
            $"User Id={AppUser};Password={AppPassword};" +
            $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={port}))" +
            "(CONNECT_DATA=(SERVICE_NAME=freepdb1)))";
    }

    public async Task DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync().AsTask();
        }
    }

    private static bool IsPortOpen(string host, int port)
    {
        try
        {
            using var client = new TcpClient();
            client.Connect(host, port);
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }
}
